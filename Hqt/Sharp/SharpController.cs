// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using SE.Hecate.Build;
using SE.Parsing;
using SE.SharpLang;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Pipeline node to perform CSharp code lookup tasks
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class SharpController : ProcessorUnit
    {
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
            get { return (UInt32)ProcessorFamilies.SharpInitialize; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public SharpController()
        { }

        private static int Process(SharpModule module, BuildModuleCache<CacheItem> cache, FileDescriptor file)
        {
            Stopwatch timer = new Stopwatch();
            Linter linter = new Linter();
            bool fromCache = true;

            #region Rules
            NamespaceRule namespaceRule = ParserRulePool<NamespaceRule, SharpToken>.Get();
            namespaceRule.Linter = linter;

            UsingRule usingRule = ParserRulePool<UsingRule, SharpToken>.Get();
            usingRule.Linter = linter;

            MainRule mainRule = ParserRulePool<MainRule, SharpToken>.Get();
            mainRule.Linter = linter;
            #endregion

            try
            {
                linter.AddRule(namespaceRule);
                linter.AddRule(usingRule);
                linter.AddRule(mainRule);

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
                    else hasCache = !BuildParameter.Rebuild;
                }
                #endregion

                #region Lint
                using (FileStream fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    timer.Start();
                    foreach (SharpModuleSettings config in module.Settings.Values)
                    {
                        CacheEntry entry; if(item.Entries.TryGetValue(config.Name, out entry) && hasCache)
                        {
                            lock (config)
                            {
                                if (config.AssemblyType < entry.AssemblyType)
                                    config.AssemblyType = entry.AssemblyType;
                            }
                            foreach (string usingDirective in entry.UsingDirectives)
                            {
                                lock (config.UsingDirectives)
                                    config.UsingDirectives.Add(usingDirective);
                            }
                            foreach (string @namespace in entry.Namespaces)
                            {
                                lock (config.Namespaces)
                                    config.Namespaces.Add(@namespace);
                            }
                        }
                        else
                        {
                            fromCache = false;

                            fs.Position = 0;
                            linter.Defines.Clear();

                            #region Rules
                            if (entry == null)
                            {
                                entry = new CacheEntry();
                                item.Entries.Add(config.Name, entry);

                                lock (cache)
                                {
                                    cache.Modified = true;
                                }
                            }

                            namespaceRule.Settings = config;
                            namespaceRule.Cache = entry;

                            usingRule.Settings = config;
                            usingRule.Cache = entry;

                            mainRule.Settings = config;
                            #endregion

                            foreach (string define in config.Defines)
                            {
                                linter.Define(define);
                            }
                            if (!Lint(linter, fs, file.GetAbsolutePath()))
                                return Application.FailureReturnCode;

                            entry.AssemblyType = config.AssemblyType;
                        }
                    }
                    timer.Stop();
                }
                #endregion
            }
            finally
            {
                #region Rules
                ParserRulePool<MainRule, SharpToken>.Return(mainRule);
                ParserRulePool<UsingRule, SharpToken>.Return(usingRule);
                ParserRulePool<NamespaceRule, SharpToken>.Return(namespaceRule);
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
            ValidationCommand sharp = (command as ValidationCommand);
            if (sharp != null)
            {
                SharpModule module = null;
                List<Task<int>> tasks = CollectionPool<List<Task<int>>, Task<int>>.Get();
                try
                {
                    BuildModuleCache<CacheItem> cache = null;
                    FileDescriptor cacheFile = null;
                    
                    foreach (FileDescriptor file in sharp)
                    {
                        switch (file.Extension.ToLowerInvariant())
                        {
                            case "cs":
                                {
                                    if (module == null)
                                    {
                                        LoadCache(sharp, out cache, out cacheFile);

                                        module = new SharpModule(sharp.Profile);
                                        sharp.SetProperty(module);
                                    }
                                    tasks.Add(Taskʾ.Run<int>(() => Process(module, cache, file)));
                                }
                                goto case "resx";
                            case "resx":
                                {
                                    if (module == null)
                                    {
                                        LoadCache(sharp, out cache, out cacheFile);

                                        module = new SharpModule(sharp.Profile);
                                        sharp.SetProperty(module);
                                    }
                                    module.Files.Add(file);
                                }
                                break;
                        }
                    }
                    sharp.Attach(Taskʾ.WhenAll(tasks).ContinueWith<int>((task) =>
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

        private static void LoadCache(ValidationCommand sharp, out BuildModuleCache<CacheItem> cache, out FileDescriptor cacheFile)
        {
            cacheFile = new FileDescriptor(Application.CacheDirectory.Combine(sharp.Profile.Name), "{0}.sharp", sharp.Name);
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
            #if net40
            linter.Define("net40");
            linter.Define("NET40");
            linter.Define("NET_4_0");
            linter.Define("NET_FRAMEWORK");
            #elif net45
            linter.Define("net45");
            linter.Define("NET45");
            linter.Define("NET_4_5");
            linter.Define("NET_FRAMEWORK");
            #elif net451
            linter.Define("net451");
            linter.Define("NET451");
            linter.Define("NET_4_5_1");
            linter.Define("NET_FRAMEWORK");
            #elif net452
            linter.Define("net452");
            linter.Define("NET452");
            linter.Define("NET_4_5_2");
            linter.Define("NET_FRAMEWORK");
            #elif net46
            linter.Define("net46");
            linter.Define("NET46");
            linter.Define("NET_4_6");
            linter.Define("NET_FRAMEWORK");
            #elif net461
            linter.Define("net461");
            linter.Define("NET461");
            linter.Define("NET_4_6_1");
            linter.Define("NET_FRAMEWORK");
            #elif net462
            linter.Define("net462");
            linter.Define("NET462");
            linter.Define("NET_4_6_2");
            linter.Define("NET_FRAMEWORK");
            #elif net47
            linter.Define("net47");
            linter.Define("NET47");
            linter.Define("NET_4_7");
            linter.Define("NET_FRAMEWORK");
            #elif net471
            linter.Define("net471");
            linter.Define("NET471");
            linter.Define("NET_4_7_1");
            linter.Define("NET_FRAMEWORK");
            #elif net472
            linter.Define("net472");
            linter.Define("NET472");
            linter.Define("NET_4_7_2");
            linter.Define("NET_FRAMEWORK");
            #elif net48
            linter.Define("net48");
            linter.Define("NET48");
            linter.Define("NET_4_8");
            linter.Define("NET_FRAMEWORK");
            #elif net50
            linter.Define("net50");
            linter.Define("NET50");
            linter.Define("NET_5_0");
            linter.Define("NET_CORE");
            #endif

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
