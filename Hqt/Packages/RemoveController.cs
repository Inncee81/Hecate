// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using SE.Apollo.Package;
using SE.CommandLine;
using SE.Config;

namespace SE.Hecate.Packages
{
    /// <summary>
    /// Pipeline node to perform a delete task to current project packages
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class RemoveController : ProcessorUnit
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
            get { return (UInt32)ProcessorFamilies.Remove; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public RemoveController()
        { }

        private static async Task<int> Process(PackageTarget target)
        {
            PathDescriptor packageLocation = Application.ProjectRoot;
            PackageManager.GetLocation(target, ref packageLocation);
            if (target.Version.IsCompatibilityVersion || target.Version == 0 || RemoveParameter.Revision)
            {
                List<FileSystemDescriptor> directories = CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Get();
                try
                {
                    if (packageLocation.Parent.FindDirectories(string.Concat(target.Id.FriendlyName(false), "@*"), directories, PathSeekOptions.RootLevel) > 0)
                    {
                        if (target.Version.IsCompatibilityVersion)
                        {
                            Application.Log(SeverityFlags.Full, "Fetching versions {0}", target.Id.FriendlyName(false));

                            #region Matching
                            foreach (ValueTuple<PathDescriptor, PackageTarget> location in directories.ConvertTo(CreateTuple))
                                if (target.Version.Match(location.Item2.Version))
                                {
                                    await RemovePackage(location.Item2, location.Item1);
                                }
                            #endregion
                        }
                        else if (target.Version == 0 && RemoveParameter.Revision)
                        {
                            #region Clear
                            foreach (PathDescriptor location in directories)
                            {
                                PackageTarget result; if (PackageTarget.TryParse(location.Name, out result))
                                {
                                    await RemovePackage(result, location);
                                }
                            }
                            #endregion
                        }
                        else if (RemoveParameter.Revision)
                        {
                            Application.Log(SeverityFlags.Full, "Fetching revisions {0} up to {1}", target.Id.FriendlyName(false), target.Version);

                            #region Revision
                            foreach (ValueTuple<PathDescriptor, PackageTarget> location in directories.ConvertTo(CreateTuple))
                                if (location.Item2.Version <= target.Version)
                                {
                                    await RemovePackage(location.Item2, location.Item1);
                                }
                            #endregion
                        }
                        else
                        {
                            Application.Log(SeverityFlags.Full, "Fetching {0}", target.FriendlyName(false));

                            #region Latest
                            foreach (ValueTuple<PathDescriptor, PackageTarget> location in directories.ConvertTo(CreateTuple))
                            {
                                if (location.Item2.Version > target.Version)
                                {
                                    packageLocation = location.Item1;
                                    target = location.Item2;
                                }
                            }
                            await RemovePackage(target, packageLocation);
                            #endregion
                        }
                    }
                    else
                    {
                        FileDescriptor packageFile; if (packageLocation.FindFile("package.json", out packageFile, PathSeekOptions.RootLevel))
                        {
                            #region Package File
                            PackageMeta pkg; if (packageFile.GetPackage(Application.LogSystem, out pkg) && target.Id.Equals(pkg.Id) && (target.Version == 0 || target.Version.Match(pkg.Version)))
                            {
                                await RemovePackage(new PackageTarget(pkg.Id, pkg.Version), packageLocation);
                            }
                            else Application.Error(SeverityFlags.None, "{0} not found", target.Id.FriendlyName(false));
                            #endregion
                        }
                        else Application.Error(SeverityFlags.None, "{0} not found", target.Id.FriendlyName(false));
                    }
                }
                finally
                {
                    CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Return(directories);
                }
            }
            else await RemovePackage(target, packageLocation);
            return Application.GetReturnCode();
        }
        public override bool Process(KernelMessage command)
        {
            PackageCommand remove = (command as PackageCommand);
            if (remove != null)
            {
                PropertyMapper.Assign<RemoveParameter>(CommandLineOptions.Default, true, true);

                foreach (string package in remove)
                {
                    PackageTarget target; if (!PackageTarget.TryParse(package, out target))
                    {
                        Application.Error(SeverityFlags.None, "Invalid package ID '{0}'", target);
                    }
                    else remove.Attach(Process(target));
                }
                return true;
            }
            else return false;
        }

        private static ValueTuple<PathDescriptor, PackageTarget> CreateTuple(FileSystemDescriptor item)
        {
            PackageTarget result;
            PackageTarget.TryParse(item.Name, out result);

            return new ValueTuple<PathDescriptor, PackageTarget>
            (
                item as PathDescriptor,
                result
            );
        }

        private static async Task<bool> CheckReferences(PackageTarget target)
        {
            if (RemoveParameter.Recursive)
            {
                Application.Log(SeverityFlags.Full, "Checking {0} dependency tree ", target.Id.FriendlyName(false));

                List<FileSystemDescriptor> packages = CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Get();
                List<PackageTarget> dependencies = CollectionPool<List<PackageTarget>, PackageTarget>.Get();
                try
                {
                    if (Application.ProjectRoot.FindFiles("package.json", packages) > 0)
                    {
                        #region References
                        foreach (FileDescriptor packageFile in packages)
                        {
                            using (NamedSpinlock packageLock = packageFile.Location.GetExclsuiveLock())
                            {
                                await packageLock.LockAsync();
                                try
                                {
                                    if (packageFile.Exists())
                                    {
                                        PackageMeta pkg; if (packageFile.GetPackage(Application.LogSystem, out pkg) && pkg.Id.IsValid && pkg.Version.IsValid)
                                        {
                                            PackageVersion version; if (pkg.Dependencies.TryGetValue(target.Id, out version) && version.Match(target.Version))
                                            {
                                                if (!target.IsDependency)
                                                {
                                                    dependencies.Add(new PackageTarget(pkg.Id, pkg.Version, true));
                                                }
                                                else
                                                {
                                                    Application.Log(SeverityFlags.Full, "Found {0} in dependency tree ", target.FriendlyName(false));
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    packageLock.Release();
                                }
                            }
                        }
                        #endregion
                    }

                    #region Dependencies
                    List<Task> tasks = CollectionPool<List<Task>, Task>.Get();
                    try
                    {
                        foreach (PackageTarget dependency in dependencies)
                        {
                            tasks.Add(Process(dependency));
                        }
                        await Taskʾ.WhenAll(tasks);
                    }
                    finally
                    {
                        CollectionPool<List<Task>, Task>.Return(tasks);
                    }
                    #endregion

                    return false;
                }
                finally
                {
                    CollectionPool<List<PackageTarget>, PackageTarget>.Return(dependencies);
                    CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Return(packages);
                }
            }
            else return false;
        }
        private static async Task RemovePackage(PackageTarget target, PathDescriptor location)
        {
            List<PackageTarget> dependencies = CollectionPool<List<PackageTarget>, PackageTarget>.Get();
            try
            {
                if (!await CheckReferences(target))
                {
                    using (NamedSpinlock packageLock = location.GetExclsuiveLock())
                    {
                        await packageLock.LockAsync();
                        try
                        {
                            if (location.Exists())
                            {
                                #region Dependencies
                                Application.Log(SeverityFlags.Full, "Fetching dependencies {0} ", target.FriendlyName(false));

                                if (RemoveParameter.Recursive)
                                {
                                    FileDescriptor packageFile = FileDescriptor.Create(location, "package.json");
                                    if (packageFile.Exists())
                                    {
                                        PackageMeta pkg; if (packageFile.GetPackage(Application.LogSystem, out pkg) && pkg.Id.IsValid && pkg.Version.IsValid && pkg.Dependencies.Count > 0)
                                        {
                                            foreach (PackageTarget dependency in pkg.Dependencies.ConvertTo(x => new PackageTarget(x.Key, x.Value, true)))
                                                dependencies.Add(dependency);
                                        }
                                    }
                                }
                                #endregion

                                #region Remove
                                Application.Log(SeverityFlags.Full, "Removing {0} ", target.FriendlyName(false));

                                location.Delete();
                                List<Task> tasks = CollectionPool<List<Task>, Task>.Get();
                                try
                                {
                                    foreach (PackageTarget dependency in dependencies)
                                    {
                                        tasks.Add(Process(dependency));
                                    }
                                    await Taskʾ.WhenAll(tasks);
                                }
                                finally
                                {
                                    CollectionPool<List<Task>, Task>.Return(tasks);
                                }
                                #endregion
                                
                                Application.Log(SeverityFlags.Minimal, "Removed {0}", target.FriendlyName(false));
                            }
                            else if (!target.IsDependency)
                                Application.Error(SeverityFlags.None, "{0} not found", target.Id.FriendlyName(false));
                        }
                        finally
                        {
                            packageLock.Release();
                        }
                    }
                }
                else Application.Warning(SeverityFlags.Minimal, "Skipped reference {0}", target.FriendlyName(false));
            }
            finally
            {
                CollectionPool<List<PackageTarget>, PackageTarget>.Return(dependencies);
            }
        }
    }
}
