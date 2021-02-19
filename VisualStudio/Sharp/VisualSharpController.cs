// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using SE.Apollo.Package;
using SE.Hecate.Build;
using SE.Hecate.Sharp;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Pipeline node to perform VisuaStudio C# project preparations
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class VisualSharpController : ProcessorUnit
    {
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
            get { return (UInt32)ProcessorFamilies.SharpProject; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public VisualSharpController()
        { }

        private async static Task<int> Process(BuildCommand modules)
        {
            List<Task> tasks = CollectionPool<List<Task>, Task>.Get();
            HashSet<BuildModule> completed = CollectionPool<HashSet<BuildModule>, BuildModule>.Get();
            try
            {
                BuildProfile profile;

                modules.TryGetProperty<BuildProfile>(out profile);
                foreach (BuildModule module in modules.OrderBy(x => x, SharpModuleComparer.Default))
                {
                    if (!module.IsPackage)
                    {
                        SharpModule sharp;
                        PackageMeta package;

                        module.TryGetProperty<SharpModule>(out sharp);
                        module.TryGetProperty<PackageMeta>(out package);

                        #region Await
                        bool process = true;
                        foreach (BuildModule dependency in sharp.Default.Dependencies)
                        {
                            if (!dependency.IsPackage && !completed.Contains(dependency))
                            {
                                if (tasks.Count > 0)
                                {
                                    await Taskʾ.WhenAll(tasks);
                                    tasks.Clear();

                                    if (completed.Contains(dependency))
                                        continue;
                                }
                                Application.Error(SeverityFlags.None, "'{0}': Missing project '{1}'", module.Name, dependency.Name);
                                process = false;
                                break;
                            }
                        }
                        if (process)
                        {
                            Application.Log(SeverityFlags.Full, "Preparing project creation '{0}' (csharp)", module.Name);
                        }
                        else continue;
                        #endregion

                        VisualSharpProject project = new VisualSharpProject(BuildParameter.Version, module.Location, module.Name);

                        HashSet<string> namespaces = CollectionPool<HashSet<string>, string>.Get();
                        foreach (SharpModuleSettings conf in sharp.Settings.Values)
                        {
                            foreach (string @namespace in conf.Namespaces)
                            {
                                namespaces.Add(@namespace);
                            }

                            #region Files
                            VisualStudioProjectTarget target = new VisualStudioProjectTarget(conf);
                            foreach (FileDescriptor file in sharp)
                            {
                                target.Files.Add(file);
                            }
                            foreach (FileDescriptor reference in sharp.Default.References)
                            {
                                target.Dependencies.Add(reference);
                            }
                            #endregion

                            #region Assembly
                            if (package != null)
                            {
                                target.AssemblyName = package.Id.Name.FromPackageName();
                            }
                            else target.AssemblyName = module.Name;
                            #endregion

                            #region Packages
                            HashSet<BuildModule> dependencyModules = CollectionPool<HashSet<BuildModule>, BuildModule>.Get();
                            try
                            {
                                Application.Log(SeverityFlags.Full, "Conflate dependencies '{0}', {1}={2}, {3} (csharp)", module.Name, profile.Platform, profile.Target, conf.Name);

                                ConflatePackages(project, target, module, conf, conf.Dependencies, dependencyModules, true);
                            }
                            finally
                            {
                                CollectionPool<HashSet<BuildModule>, BuildModule>.Return(dependencyModules);
                            }
                            #endregion

                            project.Targets.Add(target);
                        }
                        project.DefaultNamespace = GetCommonNamespace(namespaces);
                        CollectionPool<HashSet<string>, string>.Return(namespaces);

                        #region Create
                        Application.Log(SeverityFlags.Full, "Creating {0}", project.FullName);

                        using (VisualSharpCommand command = new VisualSharpCommand(project, profile))
                        {
                            command.SetProperty(project);

                            bool exit; if (Kernel.Dispatch(command, out exit))
                            {
                                tasks.Add(command.Task.ContinueWith((task) =>
                                {
                                    if (task.Status == TaskStatus.RanToCompletion && task.Result == Application.SuccessReturnCode)
                                    {
                                        module.SetProperty(project);
                                        lock (completed)
                                        {
                                            completed.Add(module);
                                        }
                                        Application.Log(SeverityFlags.Minimal, "Created {0}, {1}={2}", project.FullName, profile.Platform, profile.Target);
                                    }
                                    else Application.Error(SeverityFlags.None, "Project creation failed '{0}')", project.File.FullName);

                                }));
                            }
                            else
                            {
                                Application.Error(SeverityFlags.None, "Could not obtain project generator (csharp)");
                                return Application.FailureReturnCode;
                            }
                        }
                        #endregion
                    }
                    else break;
                }
                await Taskʾ.WhenAll(tasks);
            }
            finally
            {
                CollectionPool<HashSet<BuildModule>, BuildModule>.Return(completed);
                CollectionPool<List<Task>, Task>.Return(tasks);
            }
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            BuildCommand build = (command as BuildCommand);
            if (build != null)
            {
                build.Attach(Process(build));
                return true;
            }
            else return false;
        }
        
        private static void ConflatePackages(VisualSharpProject project, VisualStudioProjectTarget target, BuildModule current, SharpModuleSettings conf, IEnumerable<BuildModule> dependencies, HashSet<BuildModule> dependencyModules, bool addPackages)
        {
            foreach (BuildModule dependency in dependencies.OrderBy(x => x, SharpModuleComparer.Default))
                if (dependency != current && dependencyModules.Add(dependency))
                {
                    SharpModule sharp;

                    dependency.TryGetProperty<SharpModule>(out sharp);
                    if (dependency.IsPackage)
                    {
                        if (addPackages)
                        {
                            foreach (FileDescriptor file in sharp)
                            {
                                target.Files.Add(file);
                            }
                            foreach (FileDescriptor reference in sharp.Default.References)
                            {
                                if (!target.Dependencies.Contains(reference))
                                    target.Dependencies.Add(reference);
                            }
                            ConflatePackages(project, target, current, conf, sharp.Settings[conf.Name].Dependencies, dependencyModules, true);
                            if (sharp.Settings[conf.Name].AssemblyType > target.Type)
                            {
                                target.Type = sharp.Default.AssemblyType;
                            }
                            project.Packages.Add(dependency);
                        }
                        else ConflatePackages(project, target, current, conf, sharp.Settings[conf.Name].Dependencies, dependencyModules, false);
                    }
                    else
                    {
                        VisualSharpProject tmp; if (dependency.TryGetProperty<VisualSharpProject>(out tmp))
                        {
                            target.References.Add(tmp);
                        }
                        ConflatePackages(project, target, current, conf, sharp.Settings[conf.Name].Dependencies, dependencyModules, false);
                    }
                }
        }
        
        private static string GetCommonNamespace(IEnumerable<string> namespaces)
        {
            HashSet<string> result = new HashSet<string>(namespaces);
            if (result.Count > 1)
            {
                HashSet<string>.Enumerator enumerator = result.GetEnumerator();
                int maxLength = int.MaxValue;
                while (enumerator.MoveNext())
                {
                    int length = enumerator.Current.Length;
                    if (length < maxLength)
                    {
                        enumerator = result.GetEnumerator();
                        maxLength = length;
                    }
                    else if (length > maxLength)
                    {
                        string path = enumerator.Current;
                        result.Remove(path);

                        while (path.Length > maxLength)
                            path = Path.GetDirectoryName(path);

                        result.Add(path);
                        if (length < maxLength)
                            maxLength = length;

                        enumerator = result.GetEnumerator();
                    }
                }
            }
            if (result.Count > 1)
            {
                HashSet<string> cache = new HashSet<string>();
                do
                {
                    foreach (string path in result.Select(x => Path.GetDirectoryName(x)))
                        cache.Add(path);

                    result.Clear();

                    HashSet<string> tmp = cache;
                    cache = result;
                    result = tmp;
                }
                while (result.Count > 1);
            }
            if (string.IsNullOrEmpty(result.FirstOrDefault()))
            {
                return namespaces.FirstOrDefault();
            }
            else return result.First();
        }
    }
}
