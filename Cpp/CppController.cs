// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;
using SE.Parsing;
using SE.CppLang;
using SE.Hecate.Build;
using System.Runtime.Serialization;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// Pipeline node to perform Cpp code lookup tasks
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class CppController : ProcessorUnit
    {
        struct DependencyResolver
        {
            CppModuleSettings config;
            CacheEntry cache;
            IEnumerable<object> modules;

            public DependencyResolver(CppModuleSettings config, CacheEntry cache, IEnumerable<object> modules)
            {
                this.config = config;
                this.cache = cache;
                this.modules = modules;
            }

            public bool ResolveDependency(FileDescriptor file, bool relative, ref string path, out Stream stream)
            {
                if (relative)
                {
                    if (ResolveDependency(file.Location, ref path, out stream))
                        return true;
                }
                foreach (BuildModule module in modules)
                {
                    if (ResolveDependency(module.Location, ref path, out stream))
                        return true;
                }
                stream = null;
                return false;
            }
            bool ResolveDependency(PathDescriptor baseDir, ref string path, out Stream stream)
            {
                FileDescriptor file = FileDescriptor.Create(baseDir, path);
                if (file.Exists())
                {
                    bool modified; lock (config.IncludeDirectives)
                    {
                        modified = config.IncludeDirectives.Add(file);
                    }
                    if (modified)
                    {
                        cache.IncludeDirectives.Add(file);
                    }
                    stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    path = file.GetAbsolutePath();
                    return true;
                }
                else
                {
                    stream = null;
                    return false;
                }
            }
        }

        private readonly static Type CacheType = typeof(BuildModuleCache<CacheItem>);

        public override PathDescriptor Target
        {
            get { return Application.SdkRoot; }
        }
        public override bool Enabled
        {
            get { return true; }
        }
        public override UInt32 Family
        {
            get { return (UInt32)ProcessorFamilies.CppInitialize; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public CppController()
        { }

        private static int Process(IEnumerable<object> modules, CppModule module, BuildModuleCache<CacheItem> cache, FileDescriptor file)
        {
            Stopwatch timer = new Stopwatch();
            Linter linter = new Linter();
            bool fromCache = true;

            #region Rules
            MainRule mainRule = ParserRulePool<MainRule, CppToken>.Get();
            mainRule.Linter = linter;

            WinMainRule winMainRule = ParserRulePool<WinMainRule, CppToken>.Get();
            winMainRule.Linter = linter;
            #endregion

            try
            {
                linter.AddRule(mainRule);
                linter.AddRule(winMainRule);

                #region Cache
                CacheItem item;
                bool hasCache = false;
                lock (cache)
                {
                    if (!cache.TryGetValue(file, out item) || item.Timestamp != file.Timestamp)
                    {
                        if (item != null)
                        {
                            item.Dispose();
                            item = new CacheItem(file.Timestamp);
                            cache[file] = item;
                        }
                        else
                        {
                            item = new CacheItem(file.Timestamp);
                            cache.Add(file, item);
                        }
                    }
                    else hasCache = !Build.BuildParameter.Rebuild;
                }
                #endregion

                #region Lint
                using (FileStream fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                Lint:

                    timer.Start();
                    foreach (CppModuleSettings config in module.Settings.Values)
                    {
                        CacheEntry entry; if (item.Entries.TryGetValue(config.Name, out entry) && hasCache)
                        {
                            foreach (FileDescriptor dependency in entry.IncludeDirectives)
                            {
                                if (dependency.Timestamp > item.Timestamp)
                                    lock (cache)
                                    {
                                        item.Dispose();
                                        item = new CacheItem(file.Timestamp);
                                        cache[file] = item;

                                        hasCache = false;
                                        goto Lint;
                                    }
                            }
                            lock (config)
                            {
                                if (config.AssemblyType < entry.AssemblyType)
                                    config.AssemblyType = entry.AssemblyType;
                            }
                            foreach (FileDescriptor dependency in entry.IncludeDirectives)
                            {
                                lock (config.IncludeDirectives)
                                    config.IncludeDirectives.Add(dependency);
                            }
                        }
                        else
                        {
                            fromCache = false;
                            if (entry == null)
                            {
                                entry = new CacheEntry();
                                item.Entries.Add(config.Name, entry);

                                lock (cache)
                                {
                                    cache.Modified = true;
                                }
                            }

                            DependencyResolver resolver = new DependencyResolver(config, entry, modules);
                            Linter.DependencyResolveCallback callback = resolver.ResolveDependency;
                            linter.DependencyResolve += callback;
                            try
                            {
                                fs.Position = 0;
                                linter.Defines.Clear();

                                #region Rules
                                mainRule.Settings = config;
                                winMainRule.Settings = config;
                                #endregion

                                foreach (KeyValuePair<string, string> define in config.Defines)
                                {
                                    linter.Define(define.Key, define.Value);
                                }
                                if (!Lint(linter, fs, file.GetAbsolutePath()))
                                    return Application.FailureReturnCode;

                                entry.AssemblyType = config.AssemblyType;
                            }
                            finally
                            {
                                linter.DependencyResolve -= callback;
                            }
                        }
                    }
                    timer.Stop();
                }
                #endregion
            }
            finally
            {
                #region Rules
                ParserRulePool<WinMainRule, CppToken>.Return(winMainRule);
                ParserRulePool<MainRule, CppToken>.Return(mainRule);
                #endregion
            }

            if (!fromCache)
            {
                Application.Log(SeverityFlags.Full, "Linting {0} in {1}ms", file.FullName, timer.ElapsedMilliseconds);
            }
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            ValidationCommand cpp = (command as ValidationCommand);
            if (cpp != null)
            {
                CppModule module = null;
                List<Task<int>> tasks = CollectionPool<List<Task<int>>, Task<int>>.Get();
                try
                {
                    BuildModuleCache<CacheItem> cache = null;
                    FileDescriptor cacheFile = null;

                    foreach (FileDescriptor file in cpp)
                    {
                        switch (file.Extension.ToLowerInvariant())
                        {
                            case "c":
                            case "cpp":
                                {
                                    if (module == null)
                                    {
                                        LoadCache(cpp, out cache, out cacheFile);

                                        module = new CppModule(cpp.Profile, cpp.IsPackage);
                                        cpp.SetProperty(module);
                                    }
                                    tasks.Add(Taskʾ.Run<int>(() => Process(cpp.Modules, module, cache, file)));
                                }
                                goto case "h";
                            case "h":
                            case "hpp":
                            case "asm":
                            case "s":
                                {
                                    if (module == null)
                                    {
                                        LoadCache(cpp, out cache, out cacheFile);

                                        module = new CppModule(cpp.Profile, cpp.IsPackage);
                                        cpp.SetProperty(module);
                                    }
                                    module.Files.Add(file);
                                }
                                break;
                        }
                    }
                    cpp.Attach(Taskʾ.WhenAll(tasks).ContinueWith<int>((task) =>
                    {
                        int returnCode = KernelMessage.Validate(task);
                        if (cacheFile != null && cache.Modified && returnCode == Application.SuccessReturnCode)
                        {
                            try
                            {
                                using (FileStream fs = cacheFile.Open(FileMode.Create, FileAccess.Write))
                                    TypeFormatter.Serialize(fs, cache);
                            }
                            catch (Exception er)
                            {
                                Application.Warning(SeverityFlags.None, "Unable to create '{0}'\n{1}", cacheFile.FullName, er.Message);
                            }
                        }
                        if (cache != null)
                        {
                            foreach (CacheItem item in cache.Values)
                                item.Dispose();
                        }
                        return returnCode;

                    }));
                }
                finally
                {
                    CollectionPool<List<Task<int>>, Task<int>>.Return(tasks);
                }
                return true;
            }
            else return false;
        }

        private static void LoadCache(ValidationCommand cpp, out BuildModuleCache<CacheItem> cache, out FileDescriptor cacheFile)
        {
            cacheFile = new FileDescriptor(Application.CacheDirectory.Combine(cpp.Profile.Name), "{0}.cpp", cpp.Name);
            cache = null;
            try
            {
                if (!cacheFile.Location.Exists())
                    cacheFile.Location.Create();
            }
            catch { }
            if (cacheFile.Exists())
            {
                try
                {
                    using (FileStream fs = cacheFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                        cache = (TypeFormatter.Deserialize(fs, -1, CacheType) as BuildModuleCache<CacheItem>);
                }
                catch (Exception er)
                {
                    Application.Warning(SeverityFlags.None, "Unable to read '{0}'\n{1}", cacheFile.FullName, er.Message);
                }
            }
            if (cache == null)
            {
                cache = new BuildModuleCache<CacheItem>();
            }
        }

        private static bool Lint(Linter linter, FileStream stream, string file)
        {
            if (linter.Parse(stream, false, file))
            {
                foreach (string error in linter.Errors)
                {
                    Application.Warning(SeverityFlags.None, error);
                }
                return true;
            }
            else
            {
                foreach (string error in linter.Errors)
                {
                    Application.Error(SeverityFlags.None, error);
                }
                return false;
            }
        }
    }
}
