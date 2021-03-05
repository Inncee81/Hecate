// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using SE.Apollo.Package;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// Pipeline node to perform linking actions to already created Cpp code modules
    /// </summary>
    [ProcessorUnit(IsExtension = true)]
    public class PrebuildController : ProcessorUnit, IPrioritizedActor
    {
        int IPrioritizedActor.Priority
        {
            get { return 0; }
        }
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
            get { return (UInt32)SE.Hecate.ProcessorFamilies.Prebuild; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public PrebuildController()
        { }

        public void Attach(PriorityDispatcher owner)
        { }
        public void Detach(PriorityDispatcher owner)
        { }

        public bool OnNext(KernelMessage value)
        {
            try
            {
                return Process(value);
            }
            catch (Exception er)
            {
                Application.Error(er);
                return false;
            }
        }
        public bool OnError(Exception error)
        {
            return true;
        }
        public void OnCompleted()
        { }

        private static void Process(BuildModule module, CppModuleSettings cpp, List<object> modules)
        {
            PackageMeta packageInfo; if (module.TryGetProperty<PackageMeta>(out packageInfo))
            {
                #region package.json
                modules.ForEach((x) =>
                {
                    if (x == module)
                        return;

                    BuildModule dependency = (x as BuildModule);
                    CppModule cppDep; if (dependency.TryGetProperty<CppModule>(out cppDep))
                    {
                        PackageMeta pkg; if (dependency.TryGetProperty<PackageMeta>(out pkg))
                        {
                            if (packageInfo.Id.Equals(pkg.Id))
                            {
                                return;
                            }
                            foreach (PackageVersion version in pkg.References.Where(d => d.Key.Match(packageInfo.Id)).Select(d => d.Value))
                                if (version.Match(packageInfo.Version))
                                {
                                    goto AddDependency;
                                }
                            foreach (PackageVersion version in packageInfo.Dependencies.Where(d => d.Key.Match(pkg.Id)).Select(d => d.Value))
                                if (version.Match(pkg.Version))
                                {
                                    goto AddDependency;
                                }
                        }
                        if (!Build.BuildParameter.Fast)
                        {
                            foreach (FileDescriptor includeTarget in cpp.IncludeDirectives)
                                if (cppDep.Files.Contains(includeTarget))
                                {
                                    goto AddDependency;
                                }
                        }
                        return;

                    AddDependency:
                        lock (cpp.Dependencies)
                        {
                            if (!cpp.AvaragePackageExists(pkg))
                                 cpp.Dependencies.Add(dependency);
                        }
                    }
                });
                #endregion
            }
            else
            {
                #region Linking
                modules.ForEach((x) =>
                {
                    if (x == module)
                        return;

                    BuildModule dependency = (x as BuildModule);
                    CppModule cppDep; if (dependency.TryGetProperty<CppModule>(out cppDep))
                    {
                        CppModuleSettings conf; if (cppDep.Settings.TryGetValue(cpp.Name, out conf))
                        {
                            foreach (FileDescriptor includeTarget in cpp.IncludeDirectives)
                                if (cppDep.Files.Contains(includeTarget))
                                {
                                    lock (cpp.Dependencies)
                                    {
                                        PackageMeta pkg; if (!dependency.TryGetProperty<PackageMeta>(out pkg) || !cpp.AvaragePackageExists(pkg))
                                        {
                                            cpp.Dependencies.Add(dependency);
                                        }
                                    }
                                    return;
                                }
                        }
                    }
                });
                #endregion
            }
        }
        public override bool Process(KernelMessage command)
        {
            List<object> modules = CollectionPool<List<object>, object>.Get();
            if (PropertyManager.FindProperties(x => x.Value is BuildModule, modules) > 0)
            {
                List<Task> tasks = CollectionPool<List<Task>, Task>.Get();
                try
                {
                    foreach (BuildModule module in modules)
                    {
                        CppModule cpp; if (module.TryGetProperty<CppModule>(out cpp))
                        {
                            foreach(CppModuleSettings conf in cpp.Settings.Values)
                                tasks.Add(Taskʾ.Run(() => Process(module, conf, modules)));
                        }
                    }
                    command.Attach
                    (
                        Taskʾ.WhenAll(tasks)
                             .ContinueWith<int>((task) => Finalize(task, modules))
                    );
                    return true;
                }
                finally
                {
                    CollectionPool<List<Task>, Task>.Return(tasks);
                }
            }
            else CollectionPool<List<object>, object>.Return(modules);
            return false;
        }

        private static int Finalize(Task task, List<object> modules)
        {
            try
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        {
                            if (Application.LogSeverity >= SeverityFlags.Minimal)
                            {
                                #region Info
                                HashSet<FileSystemDescriptor> cache = CollectionPool<HashSet<FileSystemDescriptor>, FileSystemDescriptor>.Get();
                                try
                                {
                                    StringBuilder sb = new StringBuilder();
                                    foreach (BuildModule module in modules)
                                    {
                                        CppModule cpp; if (!module.IsPackage && module.TryGetProperty<CppModule>(out cpp))
                                        {
                                            sb.Clear();
                                            sb.Append("Loaded module ");
                                            if (Application.LogSeverity >= SeverityFlags.Full)
                                            {
                                                sb.AppendLine(module.Name);
                                                foreach (CppModuleSettings conf in cpp.Settings.Values)
                                                {
                                                    cache.Clear();
                                                    sb.Append("  |- ");
                                                    sb.Append(conf.Name);
                                                    sb.AppendLine("@cpp");

                                                    HashSet<BuildModule> visited = CollectionPool<HashSet<BuildModule>, BuildModule>.Get();
                                                    foreach (BuildModule dependency in conf.Dependencies.Traverse(d => Fetch(d, conf.Name, visited)).OrderBy(d => d.Name))
                                                    {
                                                        if (cache.Add(dependency.Location))
                                                        {
                                                            sb.Append("    |- ");
                                                            sb.AppendLine(dependency.Name);
                                                        }
                                                    }
                                                    CollectionPool<HashSet<BuildModule>, BuildModule>.Return(visited);
                                                }
                                            }
                                            else
                                            {
                                                sb.Append(module.Name);
                                                sb.AppendLine(" (cpp)");
                                            }
                                            Application.Log(SeverityFlags.Minimal, sb.ToString().Trim());
                                        }
                                    }
                                }
                                finally
                                {
                                    CollectionPool<HashSet<FileSystemDescriptor>, FileSystemDescriptor>.Return(cache);
                                }
                                #endregion
                            }
                        }
                        return Application.SuccessReturnCode;
                    case TaskStatus.Faulted:
                        {
                            Application.Error(task.Exception.InnerException);
                        }
                        goto default;
                    default: return Application.FailureReturnCode;
                }
            }
            finally
            {
                CollectionPool<List<object>, object>.Return(modules);
            }
        }
        private static IEnumerable<BuildModule> Fetch(BuildModule module, string setting, HashSet<BuildModule> visited)
        {
            if (visited.Add(module))
            {
                CppModule cpp; if (module.TryGetProperty<CppModule>(out cpp))
                {
                    CppModuleSettings conf; if (cpp.Settings.TryGetValue(setting, out conf))
                    {
                        return conf.Dependencies;
                    }
                }
            }
            return ArrayExtension.Empty<BuildModule>();
        }
    }
}