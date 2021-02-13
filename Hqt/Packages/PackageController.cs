// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;
using SE.Apollo.Package;

namespace SE.Hecate.Packages
{
    /// <summary>
    /// Pipeline node to engage overall package manager related tasks
    /// </summary>
    [ProcessorUnit(IsExtension = true)]
    public class PackageController : ProcessorUnit, IPrioritizedActor
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
            get { return (UInt32)ProcessorFamilies.EntryPoint; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public PackageController()
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

        private static async Task<int> Process(ProcessorFamilies family, List<string> packageIds)
        {
            using (PackageCommand setup = PackageCommand.Create(ProcessorFamilies.Setup, Application.ProjectRoot, packageIds))
            {
                #region Setup
                Application.Log(SeverityFlags.Full, "Setup environment");
                bool exit; if (Kernel.Dispatch(setup, out exit))
                {
                    int code = await setup.Task;
                    if (code != Application.SuccessReturnCode)
                        return code;
                }
                else if (exit)
                {
                    return Application.FailureReturnCode;
                }
                #endregion

                #region Process
                PackageCommand command = PackageCommand.Create(setup, family, Application.ProjectRoot);
                if (!Kernel.Dispatch(command, out exit))
                {
                    return Application.FailureReturnCode;
                }
                return await command.Task;
                #endregion
            }
        }
        public override bool Process(KernelMessage command)
        {
            LocalEntryPoint entryPoint = (command as LocalEntryPoint); 
            if(entryPoint != null)
            {
                switch (entryPoint.Command.ToLowerInvariant())
                {
                    case "install": command.Attach(Process(ProcessorFamilies.Install, entryPoint.Args)); break;
                    case "remove": command.Attach(Process(ProcessorFamilies.Remove, entryPoint.Args)); break;
                    default: return false;
                }
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Tries to locate the absolute directory path of the provided package
        /// </summary>
        /// <param name="package">The package to locate the installation path for</param>
        /// <param name="location">An absolute directory path computed</param>
        /// <returns>True if the package was located successfully, false otherwise</returns>
        public static bool TryGetPackageLocation(PackageTarget package, out PathDescriptor location)
        {
            List<FileSystemDescriptor> directories = CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Get();
            try
            {
                PathDescriptor packageLocation = Application.SdkRoot;
                PackageManager.GetLocation(package, ref packageLocation);
                if (!TryGetPackageLocation(package, packageLocation, directories, out location))
                {
                    if (Application.ProjectRoot != Application.SdkRoot)
                    {
                        packageLocation = Application.ProjectRoot;
                        PackageManager.GetLocation(package, ref packageLocation);
                        directories.Clear();

                        return TryGetPackageLocation(package, packageLocation, directories, out location);
                    }
                    return false;
                }
                else return true;
            }
            finally
            {
                CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Return(directories);
            }
        }
        /// <summary>
        /// Tries to locate the absolute directory path of the provided package
        /// </summary>
        /// <param name="package">The package to locate the installation path for</param>
        /// <param name="location">An absolute directory path computed</param>
        /// <returns>True if the package was located successfully, false otherwise</returns>
        public static bool TryGetPackageLocation(PackageMeta package, out PathDescriptor location)
        {
            return TryGetPackageLocation(new PackageTarget(package.Id, package.Version), out location);
        }
        private static bool TryGetPackageLocation(PackageTarget package, PathDescriptor packageLocation, List<FileSystemDescriptor> directories, out PathDescriptor location)
        {
            if (packageLocation.Parent.FindDirectories(string.Concat(package.Id.FriendlyName(false), "@*"), directories, PathSeekOptions.RootLevel) > 0)
            {
                foreach (PathDescriptor dir in directories)
                    if (package.Version.Match(PackageVersion.Create(dir.Name.Substring(dir.Name.IndexOf('@') + 1))))
                    {
                        location = dir;
                        return true;
                    }
            }
            else
            {
                FileDescriptor packageFile; if (packageLocation.FindFile("package.json", out packageFile, PathSeekOptions.RootLevel))
                {
                    PackageMeta pkg; if (packageFile.GetPackage(Application.LogSystem, out pkg) && package.Id.Equals(pkg.Id) && package.Version.Match(pkg.Version))
                    {
                        location = packageLocation;
                        return true;
                    }
                }
            }
            location = null;
            return false;
        }
    }
}
