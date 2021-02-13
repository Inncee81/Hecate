// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using SE.CommandLine;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Pipeline node to perform CSharp code compilation preparations
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class CompileController : ProcessorUnit
    {
        public override PathDescriptor Target
        {
            get { return Application.SdkRoot; }
        }
        public override bool Enabled
        {
            get { return CommandLineOptions.Default.ContainsKey("csc"); }
        }
        public override UInt32 Family
        {
            get { return (UInt32)ProcessorFamilies.SharpBuild; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public CompileController()
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

                        module.TryGetProperty<SharpModule>(out sharp);

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
                                Application.Error(SeverityFlags.None, "csc '{0}': Missing dependency '{1}'", module.Name, dependency.Name);
                                process = false;
                                break;
                            }
                        }
                        if (process)
                        {
                            Application.Log(SeverityFlags.Full, "Preparing compilation '{0}' (csharp)", module.Name);
                        }
                        else continue;
                        #endregion

                        #region Files
                        CompilerCommand compile = new CompilerCommand(modules.Template, profile, sharp.Default, module.Location);
                        foreach (FileSystemDescriptor file in sharp)
                        {
                            compile.Sources.Add(file);
                        }
                        foreach (FileDescriptor reference in sharp.Default.References)
                        {
                            compile.References.Add(reference.GetAbsolutePath());
                        }
                        #endregion

                        #region Packages
                        HashSet<BuildModule> dependencyModules = CollectionPool<HashSet<BuildModule>, BuildModule>.Get();
                        try
                        {
                            Application.Log(SeverityFlags.Full, "Conflate dependencies '{0}' (csharp)", module.Name);

                            ConflatePackages(compile, module, sharp.Default.Dependencies, dependencyModules, true);
                        }
                        finally
                        {
                            CollectionPool<HashSet<BuildModule>, BuildModule>.Return(dependencyModules);
                        }
                        #endregion
                        
                        #region Assembly
                        string moduleName = module.Name; 
                        switch (compile.Type)
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
                        compile.TargetFile = FileDescriptor.Create(Application.ProjectRoot.Combine(sharp.Default.GetDeploymentPath(module.Location, "Sharp")), moduleName);
                        #endregion

                        #region Compile
                        Application.Log(SeverityFlags.Full, "Compiling '{0}'", compile.TargetFile);

                        bool exit; if (Kernel.Dispatch(compile, out exit))
                        {
                            tasks.Add(compile.Task.ContinueWith((task) =>
                            {
                                if (task.Status == TaskStatus.RanToCompletion && task.Result == Application.SuccessReturnCode)
                                {
                                    SharpAssembly assembly = new SharpAssembly(profile, sharp.Default, compile.TargetFile);
                                    module.SetProperty(assembly);
                                    lock (completed)
                                    {
                                        completed.Add(module);
                                    }
                                    Application.Log(SeverityFlags.Minimal, "Compiled {0}, {1}={2}, {3} (csharp)", assembly.Location.FullName, assembly.Platform, assembly.Target, sharp.Default.Name);
                                }
                                else Application.Error(SeverityFlags.None, "Compilation failed '{0}' (csharp)", compile.TargetFile.FullName);
                                compile.Release();

                            }));
                        }
                        else
                        {
                            compile.Release();

                            Application.Error(SeverityFlags.None, "Could not obtain compiler (csharp)");
                            return Application.FailureReturnCode;
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
        
        private static void ConflatePackages(CompilerCommand compile, BuildModule current, IEnumerable<BuildModule> dependencies, HashSet<BuildModule> dependencyModules, bool addPackages)
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
                            foreach (FileSystemDescriptor file in sharp)
                            {
                                compile.Sources.Add(file);
                            }
                            foreach (FileSystemDescriptor reference in sharp.Default.References)
                            {
                                string tmp = reference.GetAbsolutePath();
                                if (!compile.References.Contains(tmp))
                                     compile.References.Add(tmp);
                            }
                            ConflatePackages(compile, current, sharp.Default.Dependencies, dependencyModules, true);
                            if (sharp.Default.AssemblyType > compile.Type)
                            {
                                compile.Type = sharp.Default.AssemblyType;
                            }
                        }
                        else ConflatePackages(compile, current, sharp.Default.Dependencies, dependencyModules, false);
                    }
                    else
                    {
                        SharpAssembly assembly; if (dependency.TryGetProperty<SharpAssembly>(out assembly))
                        {
                            string tmp = assembly.Location.GetAbsolutePath();
                            if (!compile.References.Contains(tmp))
                            {
                                compile.References.Add(tmp);
                            }
                            ConflatePackages(compile, current, sharp.Default.Dependencies, dependencyModules, false);
                        }
                    }
                }
        }
    }
}
