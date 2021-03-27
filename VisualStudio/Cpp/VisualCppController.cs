// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using SE.Apollo.Package;
using SE.Flex;
using SE.Hecate.Build;
using SE.Hecate.Cpp;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Pipeline node to perform VisuaStudio C++ project preparations
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class VisualCppController : ProcessorUnit
    {
        const string PackageDirectoryName = "Packages";

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
            get { return (UInt32)ProcessorFamilies.CppProject; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public VisualCppController()
        { }

        private async static Task<int> Process(BuildCommand modules)
        {
            List<Task> tasks = CollectionPool<List<Task>, Task>.Get();
            HashSet<BuildModule> completed = CollectionPool<HashSet<BuildModule>, BuildModule>.Get();
            try
            {
                BuildProfile profile;
                VirtualFileStorage storage;

                modules.TryGetProperty<BuildProfile>(out profile);
                PropertyManager.WriteLock();
                try
                {
                    if (!modules.TryGetProperty<VirtualFileStorage>(out storage))
                    {
                        storage = new VirtualFileStorage();
                        modules.SetProperty(storage);
                    }
                }
                finally
                {
                    PropertyManager.WriteRelease();
                }
                foreach (BuildModule module in modules.OrderBy(x => x, CppModuleComparer.Default))
                {
                    CppModule cpp;
                    PackageMeta package;

                    module.TryGetProperty<CppModule>(out cpp);
                    module.TryGetProperty<PackageMeta>(out package);

                    #region Await
                    bool process = true;
                    foreach (BuildModule dependency in cpp.Default.Dependencies)
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
                        Application.Log(SeverityFlags.Full, "Preparing project creation '{0}' (cpp)", module.Name);
                    }
                    else continue;
                    #endregion

                    VisualCppProject project = new VisualCppProject(BuildParameter.Version, module.Location, module.Name, module.IsPackage);
                    foreach (CppModuleSettings conf in cpp.Settings.Values)
                    {
                        #region Files
                        VisualStudioProjectTarget target = new VisualStudioProjectTarget(conf);
                        target.Type = conf.AssemblyType;

                        foreach (FileDescriptor file in cpp)
                        {
                            target.Files.Add(file);
                        }
                        foreach (FileDescriptor reference in cpp.Default.References)
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
                        target.Type = conf.AssemblyType;
                        #endregion

                        #region Packages
                        HashSet<BuildModule> dependencyModules = CollectionPool<HashSet<BuildModule>, BuildModule>.Get();
                        try
                        {
                            Application.Log(SeverityFlags.Full, "Conflate dependencies '{0}', {1}={2}, {3} (csharp)", module.Name, profile.Platform, profile.Target, conf.Name);

                            ConflatePackages(project, target, module, conf, conf.Dependencies, dependencyModules);
                        }
                        finally
                        {
                            CollectionPool<HashSet<BuildModule>, BuildModule>.Return(dependencyModules);
                        }
                        #endregion

                        project.Targets.Add(target);
                    }

                    #region Create
                    Application.Log(SeverityFlags.Full, "Creating {0}", project.FullName);

                    using (VisualCppCommand command = new VisualCppCommand(project, profile))
                    {
                        command.SetProperty(project);

                        bool exit; if (Kernel.Dispatch(command, out exit))
                        {
                            tasks.Add(command.Task.ContinueWith((task) =>
                            {
                                if (task.Status == TaskStatus.RanToCompletion && task.Result == Application.SuccessReturnCode)
                                {
                                    if (module.IsPackage)
                                    {
                                        VisualStudioDirectory dir; lock (storage.Directories)
                                        {
                                            if (!storage.Directories.TryGetValue(PackageDirectoryName, out dir))
                                            {
                                                dir = new VisualStudioDirectory(PackageDirectoryName);
                                                storage.Directories.Add(PackageDirectoryName, dir);
                                            }
                                        }
                                        lock (dir.Projects)
                                        {
                                            dir.Projects.Add(project);
                                        }
                                    }
                                    module.SetProperty(project);
                                    lock (completed)
                                    {
                                        completed.Add(module);
                                    }
                                    Application.Log(SeverityFlags.Minimal, "Created {0}, {1}={2}", project.FullName, profile.Platform, profile.Target, project.IsPackage ? " (cpp-package)" : string.Empty);
                                }
                                else Application.Error(SeverityFlags.None, "Project creation failed '{0}')", project.File.FullName);

                            }));
                        }
                        else
                        {
                            Application.Error(SeverityFlags.None, "Could not obtain project generator (cpp)");
                            return Application.FailureReturnCode;
                        }
                    }
                    #endregion
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

        private static void ConflatePackages(VisualCppProject project, VisualStudioProjectTarget target, BuildModule current, CppModuleSettings conf, IEnumerable<BuildModule> dependencies, HashSet<BuildModule> dependencyModules)
        {
            foreach (BuildModule dependency in dependencies.OrderBy(x => x, CppModuleComparer.Default))
                if (dependency != current && dependencyModules.Add(dependency))
                {
                    VisualCppProject tmp; if (dependency.TryGetProperty<VisualCppProject>(out tmp))
                    {
                        target.References.Add(tmp);
                    }
                    if (dependency.IsPackage)
                    {
                        CppModule cpp;

                        dependency.TryGetProperty<CppModule>(out cpp);
                        ConflatePackages(project, target, current, conf, cpp.Settings[conf.Name].Dependencies, dependencyModules);
                    }
                }
        }
    }
}
