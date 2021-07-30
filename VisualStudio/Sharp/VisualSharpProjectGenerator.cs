// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SE.Hecate.Build;
using SE.Hecate.Sharp;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Pipeline node to generate Microsoft Visual Studio C# Project files
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class VisualSharpProjectGenerator : ProcessorUnit
    {
        const string PackageDirectoryName = "Packages";
        private readonly static string[] DesignerExtensions = new string[]
        {
            "settings",
            "resx",
            "cs"
        };

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
            get { return (UInt32)ProcessorFamilies.VisualSharp; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public VisualSharpProjectGenerator()
        { }

        private static int Process(VisualSharpProject project, BuildProfile profile)
        {
            using (FileStream fs = project.File.Open(FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                #if NET_FRAMEWORK
                sw.WriteLine("<Project ToolsVersion=\"{0}\" DefaultTargets=\"{1}\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">", project.Version.ToolsVersion(), "Build");
                #else
                sw.WriteLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
                #endif
                {
                    CreateHeader(project, profile, sw);
                    sw.WriteLine("  <PropertyGroup>");
                    {
                        #if NET_FRAMEWORK
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.8' \">net48;NET48;NET_4_8;NET_FRAMEWORK</DefineConstants>");
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.7.2' \">net472;NET472;NET_4_7_2;NET_FRAMEWORK</DefineConstants>");
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.7.1' \">net471;NET471;NET_4_7_1;NET_FRAMEWORK</DefineConstants>");
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.7' \">net47;NET47;NET_4_7;NET_FRAMEWORK</DefineConstants>");
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.6.2' \">net462;NET462;NET_4_6_2;NET_FRAMEWORK</DefineConstants>");
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.6.1' \">net461;NET461;NET_4_6_1;NET_FRAMEWORK</DefineConstants>");
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.6' \">net46;NET46;NET_4_6;NET_FRAMEWORK</DefineConstants>");
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.5.2' \">net452;NET452;NET_4_5_2;NET_FRAMEWORK</DefineConstants>");
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.5.1' \">net451;NET451;NET_4_5_1;NET_FRAMEWORK</DefineConstants>");
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.5' \">net45;NET45;NET_4_5;NET_FRAMEWORK</DefineConstants>");
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFrameworkVersion)' == 'v4.0' \">net40;NET40;NET_4_0;NET_FRAMEWORK</DefineConstants>");
                        #else
                        sw.WriteLine("    <DefineConstants Condition=\" '$(TargetFramework)' == 'net5.0' \">net50;NET50;NET_5_0;NET_CORE</DefineConstants>");
                        #endif
                    }
                    sw.WriteLine("  </PropertyGroup>");

                    foreach (VisualStudioProjectTarget target in project.Targets)
                    {
                        CreateConfiguration(project, target, profile, target.Configuration.Name.ToTitleCase(), sw);
                    }
                        
                    CreateReferences(project, project.Targets.SelectMany(x => x.Dependencies).Distinct(), project.Targets.SelectMany(x => x.References).Distinct(), sw);
                    CreateFiles(project, project.Targets.SelectMany(x => x.Files).Distinct(), sw);

                    #if NET_FRAMEWORK
                    sw.WriteLine("  <Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />");
                    #endif
                }
                sw.WriteLine("</Project>");
                sw.Flush();
            }
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            VisualSharpCommand sharp = (command as VisualSharpCommand); 
            if (sharp != null)
            {
                command.Attach(Taskʾ.Run<int>(() => Process(sharp.Project, sharp.Profile)));
                return true;
            }
            else return false;
        }
        
        private static void CreateHeader(VisualSharpProject project, BuildProfile profile, StreamWriter sw)
        {
            sw.WriteLine("  <PropertyGroup>");
            {
                sw.WriteLine("    <Configuration Condition=\" '$(Configuration)' == '' \">{0}</Configuration>", profile.Default.Name.ToTitleCase());
                sw.WriteLine("    <ProjectGuid>{0}</ProjectGuid>", project.ProjectGuid.ToString("B").ToUpperInvariant());

                #if !NET_FRAMEWORK
                sw.WriteLine("    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>");
                sw.WriteLine("    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>");
                sw.WriteLine("    <EnableDefaultEmbeddedResourceItems>false</EnableDefaultEmbeddedResourceItems>");
                sw.WriteLine("    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>");
                sw.WriteLine("    <AppendRuntimeIdentifierToOutputPath>false</AppendRuntimeIdentifierToOutputPath>");
                #endif

                sw.WriteLine("    <RootNamespace>{0}</RootNamespace>", project.DefaultNamespace);
                sw.WriteLine("    <AppDesignerFolder>Properties</AppDesignerFolder>");

                #if net40
                sw.WriteLine("    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>");
                #elif net45
                sw.WriteLine("    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>");
                #elif net451
                sw.WriteLine("    <TargetFrameworkVersion>v4.5.1</TargetFrameworkVersion>");
                #elif net452
                sw.WriteLine("    <TargetFrameworkVersion>v4.5.2</TargetFrameworkVersion>");
                #elif net46
                sw.WriteLine("    <TargetFrameworkVersion>v4.6</TargetFrameworkVersion>");
                #elif net461
                sw.WriteLine("    <TargetFrameworkVersion>v4.6.1</TargetFrameworkVersion>");
                #elif net462
                sw.WriteLine("    <TargetFrameworkVersion>v4.6.2</TargetFrameworkVersion>");
                #elif net47
                sw.WriteLine("    <TargetFrameworkVersion>v4.7</TargetFrameworkVersion>");
                #elif net471
                sw.WriteLine("    <TargetFrameworkVersion>v4.7.1</TargetFrameworkVersion>");
                #elif net472
                sw.WriteLine("    <TargetFrameworkVersion>v4.7.2</TargetFrameworkVersion>");
                #elif net48
                sw.WriteLine("    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>");
                #else
                sw.WriteLine("    <TargetFramework>net5.0</TargetFramework>");
                #endif

                sw.WriteLine("    <WarningLevel>4</WarningLevel>");
            }
            sw.WriteLine("  </PropertyGroup>");
        }
        private static void CreateConfiguration(VisualSharpProject project, VisualStudioProjectTarget target, BuildProfile profile, string name, StreamWriter sw)
        {
            sw.WriteLine("  <PropertyGroup Condition=\" '$(Configuration)' == '{0}' \">", name);
            {
                string assemblyType;
                switch (target.Type)
                {
                    case BuildModuleType.Console:
                        {
                            assemblyType = "Exe";
                        }
                        break;
                    case BuildModuleType.Executable:
                        {
                            assemblyType = "WinExe";
                        }
                        break;
                    default:
                        {
                            assemblyType = "Library";
                        }
                        break;
                }

                sw.WriteLine("    <OutputType>{0}</OutputType>", assemblyType);
                sw.WriteLine("    <AssemblyName>{0}</AssemblyName>", target.AssemblyName);
                sw.WriteLine("    <OutputPath>{0}</OutputPath>", Application.ProjectRoot.Combine(target.Configuration.GetDeploymentPath(project.File.Location, "Sharp")).GetRelativePath(project.File));
                sw.WriteLine("    <Optimize>{0}</Optimize>", target.Configuration.Optimize);
                sw.WriteLine("    <DebugSymbols>{0}</DebugSymbols>", target.Configuration.DebugSymbols);

                if (target.Configuration.Debug)
                    sw.WriteLine("    <DebugType>full</DebugType>");

                sw.Write("    <DefineConstants>$(DefineConstants)");
                {
                    foreach (string define in target.Configuration.Defines.Keys)
                        if (!string.IsNullOrWhiteSpace(define))
                        {
                            sw.Write(";");
                            sw.Write(define);
                        }
                }
                sw.WriteLine("</DefineConstants>");
                sw.WriteLine("    <PlatformTarget>{0}</PlatformTarget>", profile.Target);
            }
            sw.WriteLine("  </PropertyGroup>");
        }
        private static void CreateReferences(VisualSharpProject project, IEnumerable<FileSystemDescriptor> dependencies, IEnumerable<VisualStudioProject> references, StreamWriter sw)
        {
            sw.WriteLine("  <ItemGroup>");
            {
                #if !NET_FRAMEWORK
                PathDescriptor sdkAssemblyPath = Sharp.BuildParameter.Dotnet.Combine("Microsoft.NETCore.App");
                #endif

                foreach (FileDescriptor dependency in dependencies)
                {
                    #if NET_FRAMEWORK
                    if (!Sharp.BuildParameter.ReferenceAssemblies.Contains(dependency.Location))
                    {
                        sw.WriteLine("    <Reference Include=\"{0}\">", dependency.Name);
                        sw.WriteLine("    <HintPath>{0}</HintPath>", dependency.GetRelativePath(project.File));
                        sw.WriteLine("    </Reference>");
                    }
                    else sw.WriteLine("    <Reference Include=\"{0}\" />", dependency.Name);
                    #else
                    if(!sdkAssemblyPath.Contains(dependency.Location))
                    {
                        sw.WriteLine("    <Reference Include=\"{0}\">", dependency.Name);
                        sw.WriteLine("    <HintPath>{0}</HintPath>", dependency.GetRelativePath(project.File));
                        sw.WriteLine("    </Reference>");
                    }
                    #endif
                }
                foreach (VisualStudioProject reference in references)
                {
                    sw.WriteLine("    <ProjectReference Include=\"{0}\">", reference.File.GetRelativePath(project.File));
                    sw.WriteLine("      <Project>{0}</Project>", reference.ProjectGuid.ToString("B").ToUpperInvariant());
                    sw.WriteLine("      <Name>{0}</Name>", reference.Name);
                    sw.WriteLine("    </ProjectReference>");
                }
            }
            sw.WriteLine("  </ItemGroup>");
        }
        private static void CreateFiles(VisualSharpProject project, IEnumerable<FileSystemDescriptor> files, StreamWriter sw)
        {
            sw.WriteLine("  <ItemGroup>");
            {
                foreach (FileDescriptor file in files)
                {
                    switch(file.Extension)
                    {
                        default:
                            {
                                sw.WriteLine("    <None Include=\"{0}\" />", file.GetRelativePath(project.File));
                            }
                            break;
                        case "resx":
                            {
                                sw.WriteLine("    <EmbeddedResource Include=\"{0}\" >", file.GetRelativePath(project.File));
                                sw.WriteLine("      <Generator>ResXFileCodeGenerator</Generator>");
                                FileDescriptor partner = file.ChangeExtensions("Designer", "cs");
                                if (partner.Exists())
                                {
                                    sw.WriteLine("      <LastGenOutput>{0}</LastGenOutput>", partner.FullName);
                                }
                                sw.WriteLine("    </EmbeddedResource>");
                            }
                            break;
                        case "settings":
                            {
                                sw.WriteLine("    <None Include=\"{0}\" >", file.GetRelativePath(project.File));
                                sw.WriteLine("      <Generator>SettingsSingleFileGenerator</Generator>");
                                FileDescriptor partner = file.ChangeExtensions("Designer", "cs");
                                if (partner.Exists())
                                {
                                    sw.WriteLine("      <LastGenOutput>{0}</LastGenOutput>", partner.FullName);
                                }
                                sw.WriteLine("    </None>");
                            }
                            break;
                        case "cs":
                            {
                                if (file.FullName.EndsWith(".Designer.cs"))
                                {
                                    foreach (string extension in DesignerExtensions)
                                    {
                                        FileDescriptor partner = file.ChangeExtensions(extension);
                                        if (partner.Exists())
                                        {
                                            sw.WriteLine("    <Compile Include=\"{0}\" >", file.GetRelativePath(project.File));
                                            sw.WriteLine("      <DependentUpon>{0}</DependentUpon>", partner.FullName);
                                            sw.WriteLine("    </Compile>");
                                            goto Next;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (BuildModule package in project.Packages)
                                    {
                                        if (package.Location.Contains(file.Location))
                                        {
                                            sw.WriteLine("    <Compile Include=\"{0}\">", file.GetRelativePath(project.File));
                                            sw.WriteLine("      <Link>{0}</Link>", Path.Combine(PackageDirectoryName, file.GetRelativePath(package.Location.Parent)));
                                            sw.WriteLine("    </Compile>");
                                            goto Next;
                                        }
                                    }
                                }
                                sw.WriteLine("    <Compile Include=\"{0}\" />", file.GetRelativePath(project.File));
                            Next:;

                            }
                            break;
                    }
                }
            }
            sw.WriteLine("  </ItemGroup>");
        }
    }
}
