// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SE.Apollo.Package;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Pipeline node to perform linking actions to already created CSharp code modules
    /// </summary>
    [ProcessorUnit(IsExtension = true)]
    public class PrebuildController : ProcessorUnit, IPrioritizedActor
    {
        int IPrioritizedActor.Priority
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return 0; }
        }
        public override PathDescriptor Target
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return Application.SdkRoot; }
        }
        public override bool Enabled
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return true; }
        }
        public override UInt32 Family
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return (UInt32)ProcessorFamilies.Prebuild; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public PrebuildController()
        { }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void Attach(PriorityDispatcher owner)
        { }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void Detach(PriorityDispatcher owner)
        { }

        [MethodImpl(OptimizationExtensions.ForceInline)]
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
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public bool OnError(Exception error)
        {
            return true;
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void OnCompleted()
        { }

        private static async Task ProcessFirstPass(BuildModule module, SharpModuleSettings sharp, List<object> modules)
        {
            PackageMeta packageInfo; if (module.TryGetProperty<PackageMeta>(out packageInfo))
            {
                #region package.json
                await modules.ParallelFor((x) =>
                {
                    if (x == module)
                        return;

                    BuildModule dependency = (x as BuildModule);
                    SharpModule sharpDep; if (dependency.TryGetProperty<SharpModule>(out sharpDep))
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
                            SharpModuleSettings conf; if (sharpDep.Settings.TryGetValue(sharp.Name, out conf))
                            {
                                foreach (string usingDirective in sharp.UsingDirectives)
                                    foreach (string @namespace in conf.Namespaces)
                                        if(usingDirective.Equals(@namespace))
                                        {
                                            goto AddDependency;
                                        }
                            }
                        }
                        return;

                    AddDependency:
                        lock (sharp.Dependencies)
                        {
                            if(!sharp.AvaragePackageExists(pkg))
                                sharp.Dependencies.Add(dependency);
                        }
                    }
                });
                #endregion
            }
            else
            {
                #region Linking
                await modules.ParallelFor((x) =>
                {
                    if (x == module)
                        return;

                    BuildModule dependency = (x as BuildModule);
                    SharpModule sharpDep; if (dependency.TryGetProperty<SharpModule>(out sharpDep))
                    {
                        SharpModuleSettings conf; if (sharpDep.Settings.TryGetValue(sharp.Name, out conf))
                        {
                            foreach (string usingDirective in sharp.UsingDirectives)
                                foreach (string @namespace in conf.Namespaces)
                                    if (usingDirective.Equals(@namespace))
                                    {
                                        lock (sharp.Dependencies)
                                        {
                                            PackageMeta pkg; if (!dependency.TryGetProperty<PackageMeta>(out pkg) || !sharp.AvaragePackageExists(pkg))
                                            {
                                                sharp.Dependencies.Add(dependency);
                                            }
                                        }
                                        return;
                                    }
                        }
                    }
                });
                #endregion
            }
            List<Task> tasks = CollectionPool<List<Task>, Task>.Get();
            try
            {
                foreach (string namespaceName in sharp.UsingDirectives)
                {
                    tasks.Add(AssemblyCache.GetAssemblies(sharp, namespaceName));
                }
                await Taskʾ.WhenAll(tasks);
            }
            finally
            {
                CollectionPool<List<Task>, Task>.Return(tasks);
            }
        }
        private static void ProcessSecondPass(BuildModule module, SharpModuleSettings sharp, List<object> modules)
        {
            PackageMeta packageInfo; if (module.TryGetProperty<PackageMeta>(out packageInfo))
            {
                if(packageInfo.References.Count > 0)
                modules.ForEach((x) =>
                {
                    if (x == module)
                        return;

                    BuildModule reference = (x as BuildModule);
                    SharpModule sharpDep; if (reference.IsPackage && reference.TryGetProperty<SharpModule>(out sharpDep))
                    {
                        PackageMeta pkg; if (reference.TryGetProperty<PackageMeta>(out pkg))
                        {
                            foreach (PackageVersion version in packageInfo.References.Where(d => d.Key.Match(pkg.Id)).Select(d => d.Value))
                                if (version.Match(pkg.Version))
                                {
                                    SharpModuleSettings conf = sharpDep.Settings[sharp.Name];
                                    BuildModuleType assemblyType = conf.AssemblyType;

                                    #region Packages
                                    HashSet<BuildModule> dependencyModules = CollectionPool<HashSet<BuildModule>, BuildModule>.Get();
                                    try
                                    {
                                        ConflatePackages(ref assemblyType, conf.Dependencies, dependencyModules, sharp.Name);
                                    }
                                    finally
                                    {
                                        CollectionPool<HashSet<BuildModule>, BuildModule>.Return(dependencyModules);
                                    }
                                    #endregion

                                    #region Assembly
                                    string moduleName = module.Name;
                                    switch (assemblyType)
                                    {
                                        case BuildModuleType.DynamicLibrary:
                                            {
                                                moduleName = string.Concat(moduleName, ".dll");
                                            }
                                            break;
                                        case BuildModuleType.Console:
                                        case BuildModuleType.Executable:
                                            {
                                                moduleName = string.Concat(moduleName, ".exe");
                                            }
                                            break;
                                    }
                                    lock (sharp.References)
                                    {
                                        sharp.References.Add(FileDescriptor.Create(Application.ProjectRoot.Combine(conf.GetDeploymentPath(reference.Location, "Sharp")), moduleName));
                                    }
                                    #endregion

                                    lock (sharp.Dependencies)
                                    {
                                        sharp.Dependencies.Remove(reference);
                                    }
                                    break;
                                }
                        }
                        return;
                    }
                });
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
                        SharpModule sharp; if (module.TryGetProperty<SharpModule>(out sharp))
                        {
                            foreach(SharpModuleSettings conf in sharp.Settings.Values)
                                tasks.Add(ProcessFirstPass(module, conf, modules));
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

        private static void ConflatePackages(ref BuildModuleType assemblyType, IEnumerable<BuildModule> dependencies, HashSet<BuildModule> dependencyModules, string configName)
        {
            foreach (BuildModule dependency in dependencies.OrderBy(x => x, SharpModuleComparer.Default))
                if (dependencyModules.Add(dependency))
                {
                    SharpModule sharp;

                    dependency.TryGetProperty<SharpModule>(out sharp);
                    if (dependency.IsPackage)
                    {
                        ConflatePackages(ref assemblyType, sharp.Default.Dependencies, dependencyModules, configName);

                        SharpModuleSettings conf = sharp.Settings[configName];
                        if (conf.AssemblyType > assemblyType)
                        {
                            assemblyType = conf.AssemblyType;
                        }
                    }
                }
        }

        private static int Finalize(Task task, List<object> modules)
        {
            try
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        {
                            #region Plugin
                            modules.ForEach((x) =>
                            {
                                BuildModule module = (x as BuildModule);
                                SharpModule sharp; if (!module.IsPackage && module.TryGetProperty<SharpModule>(out sharp))
                                {
                                    foreach (SharpModuleSettings conf in sharp.Settings.Values)
                                        ProcessSecondPass(module, conf, modules);
                                }
                            });
                            #endregion

                            if (Application.LogSeverity >= SeverityFlags.Minimal)
                            {
                                #region Info
                                HashSet<FileSystemDescriptor> cache = CollectionPool<HashSet<FileSystemDescriptor>, FileSystemDescriptor>.Get();
                                try
                                {
                                    StringBuilder sb = new StringBuilder();
                                    foreach (BuildModule module in modules)
                                    {
                                        SharpModule sharp; if (!module.IsPackage && module.TryGetProperty<SharpModule>(out sharp))
                                        {
                                            sb.Clear();
                                            sb.Append("Loaded module ");
                                            if (Application.LogSeverity >= SeverityFlags.Full)
                                            {
                                                sb.AppendLine(module.Name);
                                                foreach (SharpModuleSettings conf in sharp.Settings.Values)
                                                {
                                                    cache.Clear();
                                                    sb.Append("  |- ");
                                                    sb.Append(conf.Name);
                                                    sb.AppendLine("@csharp");

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
                                                sb.AppendLine(" (csharp)");
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
                SharpModule sharp; if (module.TryGetProperty<SharpModule>(out sharp))
                {
                    SharpModuleSettings conf; if (sharp.Settings.TryGetValue(setting, out conf))
                    {
                        return conf.Dependencies;
                    }
                }
            }
            return ArrayExtension.Empty<BuildModule>();
        }
    }
}
