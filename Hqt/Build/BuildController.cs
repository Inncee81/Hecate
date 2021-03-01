// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using SE.Alchemy;
using SE.Apollo.Package;
using SE.CommandLine;
using SE.Config;
using SE.Parsing;

namespace SE.Hecate.Build
{
    /// <summary>
    /// Pipeline node to perform build tasks
    /// </summary>
    [ProcessorUnit(IsExtension = true)]
    public class BuildController : ProcessorUnit, IPrioritizedActor
    {
        /// <summary>
        /// Files that are ignored when performing the lookup to directory paths containing
        /// or being a module location
        /// </summary>
        public readonly static string[] ReservedFiles = new string[]
        {
            //Data Files
            "*.alc",
            "*.alch",
            "*.arc",
            "*.psd1",
            "*.ps1xml",

            //Shell Files
            "*.cmd",
            "*.bat",
            "*.sh",
            "*.ps1",
            "*.psm1",

            //Packed Files
            "*.tar.gz",
            "*.tar",
            "*.zip",
            "*.rar",
            "*.7z",

            //Other
            "*.dll",
            "*.exe",
            "*.sln",
            "*.pdb"
        };

        Filter fileFilter;
        Filter directoryFilter;

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
        public BuildController()
        {
            fileFilter = new Filter();
            FilterToken root = fileFilter.Add("...");
            FilterToken token;

            foreach (string extension in ReservedFiles)
            {
                token = fileFilter.Add(root, extension);
                token.Type = FilterType.And;
                token.Exclude = true;
            }

            token = fileFilter.Add(root, ".*");
            token.Exclude = true;

            directoryFilter = new Filter();
            token = directoryFilter.Add("...");
            token = directoryFilter.Add(token, ".*");
            token.Exclude = true;
        }

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

        private static async Task<int> Process(List<FileSystemDescriptor> directories)
        {
            using (BuildCommand setup = BuildCommand.Create(ProcessorFamilies.Setup, Application.ProjectRoot, directories))
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

                #region Build Profile
                BuildProfile profile; if(!setup.TryGetProperty(out profile))
                {
                    profile = new BuildProfile();
                    setup.SetProperty(profile);
                }
                Application.Log(SeverityFlags.Full, "Loading profile {0}", profile.Name);
                if (!SetupController.LoadSettings(profile.Name, profile))
                {
                    Application.Warning(SeverityFlags.None, "Build profile '{0}' not found, loading default instead", profile.Name);
                }
                profile.AddDefaultValues();
                #endregion

                #region Build Modules
                Application.Log(SeverityFlags.Full, "Creating build modules");
                {
                    Dictionary<BuildModule, PackageMeta> modules = new Dictionary<BuildModule, PackageMeta>();
                    foreach (PathDescriptor path in directories)
                    {
                        BuildModule module = new BuildModule(path);
                        FileDescriptor packageFile; if (path.FindFile("package.json", out packageFile, PathSeekOptions.RootLevel))
                        {
                            PackageMeta pkg; if (packageFile.GetPackage(Application.LogSystem, out pkg) && pkg.Id.IsValid && pkg.Version.IsValid)
                            {
                                module.SetProperty(pkg);
                                modules.Add(module, pkg);
                            }
                        }
                        setup.SetProperty(path.FullName, module);
                    }
                    foreach(KeyValuePair<BuildModule, PackageMeta> module in modules)
                        if (module.Value.References.Count > 0)
                        {
                            foreach (KeyValuePair<PackageId, PackageVersion> reference in module.Value.References)
                            {
                                PackageMeta dependency = modules.Where(d => d.Value.Id.Match(reference.Key)).Select(d => d.Value).FirstOrDefault();
                                if (dependency != null && dependency.Version.Match(reference.Value))
                                {
                                    module.Key.IsPackage = true;
                                    break;
                                }
                            }
                        }
                }
                LoadPackages(setup, directories);
                #endregion

                bool workDone = false;
                BuildCommand command; for (ProcessorFamilies family = ProcessorFamilies.Preprocess; family <= ProcessorFamilies.Deployment; family++)
                {
                    Application.Log(SeverityFlags.Full, "{0} step", family.ToString().ToLowerInvariant());

                    #region Build
                    command = BuildCommand.Create(setup, family, Application.ProjectRoot);
                    try
                    {
                        if (Kernel.Dispatch(command, out exit))
                        {
                            int code = await command.Task;
                            if (code != Application.SuccessReturnCode)
                            {
                                Application.Error(SeverityFlags.Minimal, "{0} step failed", family.ToString().ToLowerInvariant());
                                return code;
                            }
                            else if (!workDone && family >= ProcessorFamilies.Conversion && family < ProcessorFamilies.Postprocess)
                            {
                                workDone = true;
                            }
                        }
                        else if (exit)
                        {
                            return Application.FailureReturnCode;
                        }
                    }
                    finally
                    {
                        command.Release();
                    }
                    #endregion
                }
                if (!workDone)
                {
                    Application.Warning(SeverityFlags.Minimal, "Pipeline did not process any data");
                }
                else Application.Log(SeverityFlags.Full, "Done!");
            }
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            LocalEntryPoint entryPoint = (command as LocalEntryPoint); 
            if(entryPoint != null && entryPoint.Command.Equals("build", StringComparison.InvariantCultureIgnoreCase))
            {
                List<FileSystemDescriptor> directories = CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Get();
                foreach (PathDescriptor path in entryPoint.Args.Select(arg => command.Path.Combine(arg)))
                {
                    int count;
                    FileDescriptor _ignore; if (path.FindFile(fileFilter, out _ignore))
                    {
                        directories.Add(path);
                        count = 1;
                    }
                    else
                    {
                        count = path.FindDirectories(directoryFilter, directories);
                    }
                    if (count == 0)
                    {
                        Application.Warning(SeverityFlags.Minimal, "No targets found in '{0}'", path.GetAbsolutePath());
                    }
                }
                if (directories.Count > 0)
                {
                    //SDK Folders
                    directories.Remove(Application.SdkRoot);
                    directories.Remove(Application.SdkConfig);

                    //Project Folders
                    directories.Remove(Application.ProjectRoot);
                    directories.Remove(Application.ConfigDirectory);
                }
                if (directories.Count > 0)
                {
                    PropertyMapper.Assign<BuildParameter>(CommandLineOptions.Default, true, true);
                    command.Attach(Process(directories));
                }
                else Application.Warning(SeverityFlags.Minimal, "Nothing to do!");
                return true;
            }
            else return false;
        }

        private static void LoadPackages(BuildCommand setup, List<FileSystemDescriptor> directories)
        {
            HashSet<FileSystemDescriptor> packages = CollectionPool<HashSet<FileSystemDescriptor>, FileSystemDescriptor>.Get();
            try
            {
                string t; foreach (string path in PackageManager.PackageLocations.Values)
                {
                    t = path.Replace("[id]", "*.*.*@*")
                            .Replace("[owner]", "*")
                            .Replace("[namespace]", "*")
                            .Replace("[name]", "*");

                    if(!t.StartsWith("..."))
                    {
                        t = string.Concat(".../", t);
                    }
                    Application.ProjectRoot.FindDirectories(t, packages);
                    if (Application.SdkRoot != Application.ProjectRoot)
                    {
                        Application.SdkRoot.FindDirectories(t, packages);
                    }
                }
                t = Path.Combine(PackageManager.DefaultPackageLocation, "*.*.*@*");
                Application.ProjectRoot.FindDirectories(t, packages);
                if (Application.SdkRoot != Application.ProjectRoot)
                {
                    Application.SdkRoot.FindDirectories(t, packages);
                }
                foreach (PathDescriptor path in packages)
                {
                    bool isArgs = false;
                    foreach (PathDescriptor arg in directories)
                    {
                        if (arg.Contains(path) || path.Contains(arg))
                        {
                            isArgs = true;
                            break;
                        }
                    }
                    if (!isArgs)
                    {
                        BuildModule module = new BuildModule(path);
                        module.IsPackage = true;

                        FileDescriptor packageFile; if (path.FindFile("package.json", out packageFile, PathSeekOptions.RootLevel))
                        {
                            PackageMeta pkg; if (packageFile.GetPackage(Application.LogSystem, out pkg) && pkg.Id.IsValid && pkg.Version.IsValid)
                            {
                                module.SetProperty(pkg);
                            }
                        }
                        setup.SetProperty(path.FullName, module);
                    }
                }
            }
            finally
            {
                CollectionPool<HashSet<FileSystemDescriptor>, FileSystemDescriptor>.Return(packages);
            }
        }
    }
}
