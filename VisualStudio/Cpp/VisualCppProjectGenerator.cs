// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using SE.Hecate.Build;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Pipeline node to generate Microsoft Visual Studio C++ Project files
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class VisualCppProjectGenerator : ProcessorUnit
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
            get { return (UInt32)ProcessorFamilies.VisualCpp; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public VisualCppProjectGenerator()
        { }

        private static int Process(VisualCppProject project, BuildProfile profile)
        {
            PathDescriptor buildCache = Application.ProjectRoot.Combine(".build");
            try
            {
                if (!buildCache.Exists())
                    buildCache.CreateHidden();
            }
            catch { }
            using (FileStream fs = project.File.Open(FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sw.WriteLine("<Project ToolsVersion=\"{0}\" DefaultTargets=\"{1}\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">", project.Version.ToolsVersion(), "Build");
                {
                    CreateHeader(project, profile, sw);

                    sw.WriteLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Default.props\" />");
                    sw.WriteLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.props\" />");
                    sw.WriteLine("  <ImportGroup Label=\"ExtensionSettings\">");
                    {
                        sw.WriteLine("    <Import Project=\"$(VCTargetsPath)\\BuildCustomizations\\masm.props\" />");
                    }
                    sw.WriteLine("  </ImportGroup>");
                    
                    foreach (VisualStudioProjectTarget target in project.Targets)
                    {
                        CreateConfiguration(project, target, profile, buildCache, target.Configuration.Name.ToTitleCase(), sw);
                    }
                    CreateFiles(project, project.Targets.SelectMany(x => x.Files).Distinct(), sw);

                    sw.WriteLine("  <Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.targets\" />");
                    sw.WriteLine("  <ImportGroup Label=\"ExtensionTargets\">");
                    {
                        sw.WriteLine("    <Import Project=\"$(VCTargetsPath)\\BuildCustomizations\\masm.targets\" />");
                    }
                    sw.WriteLine("  </ImportGroup>");
                }
                sw.WriteLine("</Project>");
            }
            using (FileStream fs = new FileDescriptor(project.File.Location, "{0}.filters", project.FullName).Open(FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sw.WriteLine("<Project ToolsVersion=\"{0}\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">", project.Version.ToolsVersion());
                {
                    sw.WriteLine("  <ItemGroup>");
                    {
                        SetFilterDirectories(project, project.File.Location, sw);
                    }
                    sw.WriteLine("  </ItemGroup>");
                    
                    SetFilterFiles(project, project.Targets.SelectMany(x => x.Files).Distinct(), sw);
                }
                sw.WriteLine("</Project>");
                sw.Flush();
            }
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            VisualCppCommand cpp = (command as VisualCppCommand); 
            if (cpp != null)
            {
                command.Attach(Taskʾ.Run<int>(() => Process(cpp.Project, cpp.Profile)));
                return true;
            }
            else return false;
        }

        private static void CreateHeader(VisualCppProject project, BuildProfile profile, StreamWriter sw)
        {
            sw.WriteLine("  <PropertyGroup Label=\"Globals\">");
            {
                sw.WriteLine("    <ProjectGuid>{0}</ProjectGuid>", project.ProjectGuid.ToString("B").ToUpperInvariant());
                
                if (Cpp.BuildParameter.SdkVersion != null)
                {
                    sw.WriteLine("    <WindowsTargetPlatformVersion>{0}</WindowsTargetPlatformVersion>", Cpp.BuildParameter.SdkVersion);
                }
            }
            sw.WriteLine("  </PropertyGroup>");
            sw.WriteLine("  <ItemGroup Label=\"ProjectConfigurations\">");

            foreach (VisualStudioProjectTarget target in project.Targets)
            {
                string name = target.Configuration.Name.ToTitleCase();

                sw.WriteLine("    <ProjectConfiguration Include=\"{0}|{1}\">", name, profile.Target);
                sw.WriteLine("      <Configuration>{0}</Configuration>", name);
                sw.WriteLine("      <Platform>{0}</Platform>", profile.Target);
                sw.WriteLine("    </ProjectConfiguration>");
            }
            sw.WriteLine("  </ItemGroup>");
        }
        private static void CreateConfiguration(VisualCppProject project, VisualStudioProjectTarget target, BuildProfile profile, PathDescriptor buildCache, string name, StreamWriter sw)
        {
            string extension;
            sw.WriteLine("  <PropertyGroup Condition=\" '$(Configuration)' == '{0}' \" Label=\"Configuration\">", name);
            {
                string projectType; switch (target.Type)
                {
                    case BuildModuleType.DynamicLibrary:
                        {
                            projectType = "DynamicLibrary";
                            if (profile.Platform.Equals("windows", StringComparison.InvariantCultureIgnoreCase))
                            {
                                extension = ".dll";
                            }
                            else extension = ".so";
                        }
                        break;
                    case BuildModuleType.Executable:
                        {
                            projectType = "Application";
                            extension = ".exe";
                        }
                        break;
                    default:
                        {
                            projectType = "StaticLibrary";
                            if (profile.Platform.Equals("windows", StringComparison.InvariantCultureIgnoreCase))
                            {
                                extension = ".lib";
                            }
                            else extension = ".a";
                        }
                        break;
                }

                sw.WriteLine("    <ConfigurationType>{0}</ConfigurationType>", projectType);
                sw.WriteLine("    <UseDebugLibraries>{0}</UseDebugLibraries>", (target.Configuration.Optimize) ? "false" : "true");
                sw.WriteLine("    <PlatformToolset>{0}</PlatformToolset>", project.Version.ToolsetVersion());
            }
            sw.WriteLine("  </PropertyGroup>");
            sw.WriteLine("  <PropertyGroup Condition=\" '$(Configuration)' == '{0}' \">", name);
            {
                sw.WriteLine("    <OutDir>{0}</OutDir>", Application.ProjectRoot.Combine(target.Configuration.GetDeploymentPath(project.File.Location, "Cpp")).GetRelativePath(project.File));
                sw.WriteLine("    <IntDir>{0}$(ProjectName)\\</IntDir>", buildCache.GetRelativePath(project.File));
                sw.WriteLine("    <TargetName>{0}</TargetName>", project.Name);
                sw.WriteLine("    <TargetExt>{0}</TargetExt>", extension);
            }
            sw.WriteLine("  </PropertyGroup>");
            sw.WriteLine("  <ItemDefinitionGroup Condition=\" '$(Configuration)' == '{0}' \" Label=\"Configuration\">", name);
            {
                sw.WriteLine("    <ClCompile>");
                {
                    sw.Write("      <AdditionalIncludeDirectories>");
                    {
                        sw.Write("$(ProjectDir);");
                        foreach (VisualStudioProject reference in target.References)
                        {
                            sw.Write("$(ProjectDir)");
                            sw.Write(reference.File.Location.GetRelativePath(project.File));
                            sw.Write(";");
                        }
                    }
                    sw.WriteLine("</AdditionalIncludeDirectories>");
                    sw.Write("      <PreprocessorDefinitions>");
                    {
                        foreach (KeyValuePair<string, string> define in target.Configuration.Defines)
                            if (!string.IsNullOrWhiteSpace(define.Key))
                            {
                                sw.Write(define.Key);
                                if (!string.IsNullOrWhiteSpace(define.Value))
                                {
                                    sw.Write('=');
                                    sw.Write(define.Value);
                                }
                                sw.Write(';');
                            }
                    }
                    sw.WriteLine("</PreprocessorDefinitions>");
                    sw.WriteLine("      <WarningLevel>Level4</WarningLevel>");
                    sw.WriteLine("      <TreatWarningAsError>{0}</TreatWarningAsError>", (target.Configuration.WarningAsError) ? "true" : "false");
                    sw.WriteLine("      <ExceptionHandling>false</ExceptionHandling>");

                    if (target.Configuration.Optimize)
                    {
                        sw.WriteLine("      <Optimization>MaxSpeed</Optimization>");
                    }
                }
                sw.WriteLine("    </ClCompile>");
                sw.WriteLine("    <Link>");
                {
                    sw.Write("      <AdditionalLibraryDirectories>");
                    {
                        HashSet<FileSystemDescriptor> directories = CollectionPool<HashSet<FileSystemDescriptor>, FileSystemDescriptor>.Get();
                        try
                        {
                            foreach (VisualStudioProject reference in target.References)
                            {
                                PathDescriptor deploymentPath = Application.ProjectRoot.Combine(target.Configuration.GetDeploymentPath(reference.File.Location, "Cpp"));
                                if (directories.Add(deploymentPath))
                                {
                                    sw.Write("$(ProjectDir)\\");
                                    sw.Write(deploymentPath.GetRelativePath(project.File));
                                    sw.Write(";");
                                }
                            }
                        }
                        finally
                        {
                            CollectionPool<HashSet<FileSystemDescriptor>, FileSystemDescriptor>.Return(directories);
                        }
                    }
                    sw.WriteLine("</AdditionalLibraryDirectories>");
                    sw.Write("      <AdditionalDependencies>");
                    {
                        foreach (VisualStudioProject reference in target.References)
                        {
                            CreateDependencyList(reference, profile, target.Configuration, sw);
                        }
                        foreach (FileDescriptor dependency in target.Dependencies)
                        {
                            if (Application.ProjectRoot.Contains(dependency.Location))
                            {
                                sw.Write(dependency.Location.GetRelativePath(project.File));
                            }
                            else sw.Write(dependency.GetAbsolutePath());
                            sw.Write(";");
                        }
                    }
                    sw.WriteLine("</AdditionalDependencies>");
                    sw.WriteLine("      <GenerateDebugInformation>{0}</GenerateDebugInformation>", target.Configuration.DebugSymbols);

                    if (target.Type == BuildModuleType.Executable && profile.Platform.Equals("windows", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //unifies the entryPoint behavior to all platforms being main instead of WinMain for Windows

                        sw.WriteLine("      <SubSystem>Windows</SubSystem>");
                        sw.WriteLine("      <EntryPointSymbol>mainCRTStartup</EntryPointSymbol>");
                    }
                }
                sw.WriteLine("    </Link>");
            }
            sw.WriteLine("  </ItemDefinitionGroup>");
        }
        private static void CreateDependencyList(VisualStudioProject project, BuildProfile profile, BuildConfiguration conf, StreamWriter sw)
        {
            string extension; switch (project.Targets.Where(x => x.Configuration == conf).First().Type)
            {
                case BuildModuleType.DynamicLibrary:
                    {
                        if (profile.Platform.Equals("windows", StringComparison.InvariantCultureIgnoreCase))
                        {
                            extension = ".dll";
                        }
                        else extension = ".so";
                    }
                    break;
                case BuildModuleType.Executable:
                    {
                        extension = ".exe";
                    }
                    break;
                default:
                    {
                        if (profile.Platform.Equals("windows", StringComparison.InvariantCultureIgnoreCase))
                        {
                            extension = ".lib";
                        }
                        else extension = ".a";
                    }
                    break;
            }
            sw.Write("{0}{1}", project.Name, extension);
            sw.Write(";");
        }
        private static void CreateFiles(VisualCppProject project, IEnumerable<FileDescriptor> files, StreamWriter sw)
        {
            List<FileDescriptor> sourceFiles = CollectionPool<List<FileDescriptor>, FileDescriptor>.Get();
            List<FileDescriptor> assemblyFiles = CollectionPool<List<FileDescriptor>, FileDescriptor>.Get();
            try
            {
                sw.WriteLine("  <ItemGroup>");
                {
                    foreach (FileDescriptor file in files)
                        switch (file.Extension)
                        {
                            case "h":
                            case "hpp":
                                {
                                    sw.WriteLine("    <ClInclude Include=\"{0}\" />", file.GetRelativePath(project.File));
                                }
                                break;
                            case "c":
                            case "cpp":
                                {
                                    sourceFiles.Add(file);
                                }
                                break;
                            case "S":
                            case "asm":
                                {
                                    assemblyFiles.Add(file);
                                }
                                break;
                        }
                }
                sw.WriteLine("  </ItemGroup>");
                sw.WriteLine("  <ItemGroup>");
                {
                    foreach (FileDescriptor file in sourceFiles)
                        sw.WriteLine("    <ClCompile Include=\"{0}\" />", file.GetRelativePath(project.File));
                }
                sw.WriteLine("  </ItemGroup>");
                sw.WriteLine("  <ItemGroup>");
                {
                    foreach (FileDescriptor file in assemblyFiles)
                        sw.WriteLine("    <MASM Include=\"{0}\" />", file.GetRelativePath(project.File));
                }
                sw.WriteLine("  </ItemGroup>");
            }
            finally
            {
                CollectionPool<List<FileDescriptor>, FileDescriptor>.Return(assemblyFiles);
                CollectionPool<List<FileDescriptor>, FileDescriptor>.Return(sourceFiles);
            }
        }

        private static void SetFilterDirectories(VisualCppProject project, PathDescriptor root, StreamWriter sw)
        {
            Filter filter = new Filter();
            FilterToken token = filter.Add(".*");
            token.Exclude = true;

            foreach (PathDescriptor path in root.FindDirectories(filter, PathSeekOptions.RootLevel))
            {
                sw.WriteLine("    <Filter Include=\"{0}\">", path.GetRelativePath(project.File).Trim('\\', '/'));
                sw.WriteLine("      <UniqueIdentifier>" + Guid.NewGuid().ToString("B").ToUpperInvariant() + "</UniqueIdentifier>");
                sw.WriteLine("    </Filter>");

                SetFilterDirectories(project, path, sw);
            }
        }
        private static void SetFilterFiles(VisualCppProject project, IEnumerable<FileDescriptor> files, StreamWriter sw)
        {
            List<FileDescriptor> sourceFiles = CollectionPool<List<FileDescriptor>, FileDescriptor>.Get();
            List<FileDescriptor> assemblyFiles = CollectionPool<List<FileDescriptor>, FileDescriptor>.Get();
            try
            {
                sw.WriteLine("  <ItemGroup>");
                {
                    foreach (FileDescriptor file in files)
                        switch (file.Extension)
                        {
                            case "h":
                            case "hpp":
                                {
                                    if (project.File.Location != file.Location && project.File.Location.Contains(file.Location))
                                    {
                                        sw.WriteLine("    <ClInclude Include=\"{0}\">", file.GetRelativePath(project.File));
                                        sw.WriteLine("      <Filter>{0}</Filter>", file.Location.GetRelativePath(project.File).Trim('\\', '/'));
                                        sw.WriteLine("    </ClInclude>");
                                    }
                                    else sw.WriteLine("    <ClInclude Include=\"{0}\" />", file.GetRelativePath(project.File).Trim('\\', '/'));
                                }
                                break;
                            case "c":
                            case "cpp":
                                {
                                    sourceFiles.Add(file);
                                }
                                break;
                            case "S":
                            case "asm":
                                {
                                    assemblyFiles.Add(file);
                                }
                                break;
                        }
                }
                sw.WriteLine("  </ItemGroup>");
                sw.WriteLine("  <ItemGroup>");
                {
                    foreach (FileDescriptor file in sourceFiles)
                        if (project.File.Location != file.Location && project.File.Location.Contains(file.Location))
                        {
                            sw.WriteLine("    <ClCompile Include=\"{0}\">", file.GetRelativePath(project.File));
                            sw.WriteLine("      <Filter>{0}</Filter>", file.Location.GetRelativePath(project.File).Trim('\\', '/'));
                            sw.WriteLine("    </ClCompile>");
                        }
                        else sw.WriteLine("    <ClCompile Include=\"{0}\" />", file.GetRelativePath(project.File).Trim('\\', '/'));
                }
                sw.WriteLine("  </ItemGroup>");
                sw.WriteLine("  <ItemGroup>");
                {
                    foreach (FileDescriptor file in assemblyFiles)
                        if (project.File.Location != file.Location && project.File.Location.Contains(file.Location))
                        {
                            sw.WriteLine("    <MASM Include=\"{0}\">", file.GetRelativePath(project.File));
                            sw.WriteLine("      <Filter>{0}</Filter>", file.Location.GetRelativePath(project.File).Trim('\\', '/'));
                            sw.WriteLine("    </MASM>");
                        }
                        else sw.WriteLine("    <MASM Include=\"{0}\" />", file.GetRelativePath(project.File).Trim('\\', '/'));
                }
                sw.WriteLine("  </ItemGroup>");
            }
            finally
            {
                CollectionPool<List<FileDescriptor>, FileDescriptor>.Return(assemblyFiles);
                CollectionPool<List<FileDescriptor>, FileDescriptor>.Return(sourceFiles);
            }
        }
    }
}
