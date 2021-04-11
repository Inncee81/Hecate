// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SE.Apollo.Package;
using SE.CommandLine;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Pipeline node to perform dispatching of publishing related build actions
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class PublishController : ProcessorUnit
    {
        private readonly static string[] Tags = new string[]
        {
            "C#",
            "sharp",
            "csharp"
        };

        public override PathDescriptor Target
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return Application.SdkRoot; }
        }
        public override bool Enabled
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return CommandLineOptions.Default.ContainsKey("publish"); }
        }
        public override UInt32 Family
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return (UInt32)ProcessorFamilies.SharpPublish; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public PublishController()
        { }

        private async static Task<int> Process(IEnumerable<BuildModule> modules)
        {
            List<Task> tasks = CollectionPool<List<Task>, Task>.Get();
            HashSet<BuildModule> completed = CollectionPool<HashSet<BuildModule>, BuildModule>.Get();
            try
            {
                foreach (BuildModule module in modules.OrderBy(x => x, SharpModuleComparer.Default))
                {
                    if (!module.IsPackage)
                    {
                        SharpModule sharp;
                        PackageMeta package;

                        module.TryGetProperty<SharpModule>(out sharp);
                        if (!module.TryGetProperty<PackageMeta>(out package))
                        {
                            #region Package
                            package = new PackageMeta();
                            PackageTarget target; if (!PackageTarget.TryParse(module.Name, out target))
                            {
                                target = new PackageTarget
                                (
                                    new PackageId
                                    (
                                        "schroedingerentertainment",
                                        "se",
                                        module.Location.Parent.Name.ToLowerInvariant(),
                                        module.Name.ToLowerInvariant()
                                    ),
                                    PackageVersion.Create(1, 0, 0, false)
                                );
                            }
                            else if (target.Version == 0)
                            {
                                target = new PackageTarget(target.Id, PackageVersion.Create(1, 0, 0, false));
                            }
                            if (string.IsNullOrWhiteSpace(target.Id.Scope))
                            {
                                string scope = null;
                                foreach (Repository repo in PackageManager.Repositories)
                                {
                                    if (repo.Prefixes.TryGetValue(target.Id.Owner, out scope))
                                        break;
                                }
                                if (string.IsNullOrEmpty(scope))
                                {
                                    scope = string.Empty;
                                }
                                target = new PackageTarget(new PackageId(scope, target.Id), target.Version);
                            }
                            package.Id = target.Id;
                            package.Version = target.Version;
                            package.Description = "Hecate Generated";
                            package.License = "UNLICENSED";
                            
                            module.SetProperty(package);
                            #endregion
                        }

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
                                Application.Error(SeverityFlags.None, "'{0}': Missing package.json '{1}'", module.Name, dependency.Name);
                                process = false;
                                break;
                            }
                        }
                        if (process)
                        {
                            Application.Log(SeverityFlags.Full, "Preparing package creation '{0}' (csharp)", module.Name);
                        }
                        else continue;
                        #endregion

                        #region Process
                        Application.Log(SeverityFlags.Full, "Creating {0}", package.FriendlyName(false));

                        tasks.Add(Task.Run(() => CreatePackageMeta(module, sharp, package)).ContinueWith((task) =>
                        {
                            if (task.Status == TaskStatus.RanToCompletion)
                            {
                                lock (completed)
                                {
                                    completed.Add(module);
                                }
                                Application.Log(SeverityFlags.Minimal, "Create package {0} (csharp)", package.FriendlyName(false));
                            }
                            else Application.Error(SeverityFlags.None, "Package creation failed {0} (csharp)", package.FriendlyName(false));

                        }));
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
            DeployCommand deploy = (command as DeployCommand);
            if (deploy != null)
            {
                deploy.Attach(Process(deploy));
                return true;
            }
            else return false;
        }

        private static void CreatePackageMeta(BuildModule module, SharpModule sharp, PackageMeta package)
        {
            foreach (string tag in Tags)
            {
                package.Tags.Add(tag);
            }
            package.Tags.Add(package.Id.Namespace);
            
            package.Dependencies.Clear();
            foreach (BuildModule dependency in sharp.Default.Dependencies)
            {
                PackageMeta pkg; if (dependency.TryGetProperty<PackageMeta>(out pkg))
                {
                    package.Dependencies.Add(pkg.Id, PackageVersion.Create
                    (
                        pkg.Version.Major, 
                        pkg.Version.Minor, 
                        pkg.Version.Revision, 
                        true
                    ));
                }
            }

            using (FileStream fs = FileDescriptor.Create(module.Location, "package.json").Open(FileMode.Create, FileAccess.Write))
            {
                package.Serialize(fs, true);
            }
        }
    }
}
