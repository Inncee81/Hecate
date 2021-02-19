// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using SE.Hecate.Build;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Pipeline node to generate Microsoft Visual Studio Solution files
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public partial class SolutionGenerator : ProcessorUnit
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
            get { return (UInt32)ProcessorFamilies.Solution; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public SolutionGenerator()
        { }

        private static int Process(PostbuildCommand solution)
        {
            BuildProfile profile;
            VirtualFileStorage storage;

            solution.TryGetProperty<BuildProfile>(out profile);
            solution.TryGetProperty<VirtualFileStorage>(out storage);

            string name = PathDescriptor.GetCommonParent(solution.Where(x => !x.IsPackage).Select(x => x.File.Location));
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = Path.GetFileName(name);
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Project.sln";
            }
            else name = string.Concat(name, ".sln");
            
            FileDescriptor solutionFile = FileDescriptor.Create(solution.Path, name);
            using (FileStream fs = solutionFile.Open(FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                CreateHeader(sw);
                CreateProjects(solution, solutionFile, sw);

                if (storage != null)
                {
                    CreateDirectories(storage, solutionFile, sw);
                }
                sw.WriteLine("Global");
                {
                    CreateConfigurationList(solution, profile, sw);
                    CreateConfigurations(solution, profile, sw);

                    if (storage != null)
                    {
                        CreateVirtualProjectTree(storage, sw);
                    }
                }
                sw.WriteLine("EndGlobal");
            }
            Application.Log(SeverityFlags.Minimal, "Created {0}", solutionFile.FullName);

            solution.SetProperty(new VisualStudioSolution(profile, solutionFile));
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            PostbuildCommand solution = (command as PostbuildCommand);
            if (solution != null)
            {
                command.Attach(Taskʾ.Run<int>(() => Process(solution)));
                return true;
            }
            else return false;
        }

        private static void CreateHeader(StreamWriter sw)
        {
            switch (BuildParameter.Version)
            {
                case VisualStudioVersion.VisualStudio2019:
                    {
                        sw.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
                        sw.WriteLine("# Visual Studio Version 16");
                        sw.WriteLine("VisualStudioVersion = 16.0.29806.167");
                        sw.WriteLine("MinimumVisualStudioVersion = 10.0.40219.1");
                    }
                    break;
                case VisualStudioVersion.VisualStudio2017:
                    {
                        sw.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
                        sw.WriteLine("# Visual Studio 15");
                        sw.WriteLine("VisualStudioVersion = 15.0.25807.0");
                        sw.WriteLine("MinimumVisualStudioVersion = 10.0.40219.1");
                    }
                    break;
                case VisualStudioVersion.VisualStudio2015:
                    {
                        sw.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
                        sw.WriteLine("# Visual Studio 14");
                        sw.WriteLine("VisualStudioVersion = 14.0.22310.1");
                        sw.WriteLine("MinimumVisualStudioVersion = 10.0.40219.1");
                    }
                    break;
                case VisualStudioVersion.VisualStudio2013:
                    {
                        sw.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
                        sw.WriteLine("# Visual Studio 2013");
                    }
                    break;
                case VisualStudioVersion.VisualStudio2012:
                    {
                        sw.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
                        sw.WriteLine("# Visual Studio 2012");
                    }
                    break;
                case VisualStudioVersion.VisualStudio2010:
                    {
                        sw.WriteLine("Microsoft Visual Studio Solution File, Format Version 11.00");
                        sw.WriteLine("# Visual Studio 2010");
                    }
                    break;
            }
        }

        private static void CreateProjects(IEnumerable<VisualStudioProject> projects, FileDescriptor solutionFile, StreamWriter sw)
        {
            foreach (VisualStudioProject project in projects)
            {
                sw.WriteLine("Project(\"{0}\") = \"{1}\", \"{2}\", \"{3}\"", project.ProjectTypeGuid.ToString("B").ToUpperInvariant(), project.Name, project.File.GetRelativePath(solutionFile.Location), project.ProjectGuid.ToString("B").ToUpperInvariant());
                {
                    IEnumerable<VisualStudioProject> references = project.Targets.SelectMany(x => x.References).OrderBy(x => x.ProjectGuid).Distinct();
                    if (references.Any())
                    {
                        sw.WriteLine("\tProjectSection(ProjectDependencies) = postProject");
                        {
                            foreach (VisualStudioProject dependency in references)
                                sw.WriteLine("\t\t{0} = {0}", dependency.ProjectGuid.ToString("B").ToUpperInvariant());
                        }
                        sw.WriteLine("\tEndProjectSection");
                    }
                }
                sw.WriteLine("EndProject");
            }
        }
        private static void CreateConfigurationList(IEnumerable<VisualStudioProject> projects, BuildProfile profile, StreamWriter sw)
        {
            sw.WriteLine("	GlobalSection(SolutionConfigurationPlatforms) = preSolution");
            {
                bool hasVisualSharpProject = projects.Any(x => x is VisualSharpProject);
                foreach (string name in projects.SelectMany(x => x.Targets).Select(x => x.Configuration.Name).Distinct())
                {
                    // e.g. Debug|x64 = Debug|x64
                    sw.WriteLine("		{0}|{1} = {0}|{1}", name.ToTitleCase(), profile.Target);
                    if (hasVisualSharpProject)
                    {
                        sw.WriteLine("		{0}|{1} = {0}|{1}", name.ToTitleCase(), "Any CPU");
                    }
                }
            }
            sw.WriteLine("	EndGlobalSection");
        }
        private static void CreateConfigurations(IEnumerable<VisualStudioProject> projects, BuildProfile profile, StreamWriter sw)
        {
            sw.WriteLine("	GlobalSection(ProjectConfigurationPlatforms) = postSolution");
            {
                foreach (VisualStudioProject project in projects)
                {
                    bool isVisualSharpProject = (project is VisualSharpProject);
                    foreach (VisualStudioProjectTarget target in project.Targets)
                    {
                        string projectGuid = project.ProjectGuid.ToString("B").ToUpperInvariant();
                        string name = target.Configuration.Name.ToTitleCase();
                        string architecture; if (!isVisualSharpProject)
                        {
                            architecture = profile.Target.ToString();
                        }
                        else architecture = "Any CPU";

                        // e.g. "{4232C52C-680F-4850-8855-DC39419B5E9B}.Debug|x64.ActiveCfg = Debug|x64"
                        sw.WriteLine("		{0}.{1}|{2}.ActiveCfg = {1}|{2}", projectGuid, name, architecture);

                        //could build on this platform
                        sw.WriteLine("		{0}.{1}|{2}.Build.0 = {1}|{2}", projectGuid, name, architecture);

                        //could deploy on this platform
                        sw.WriteLine("		{0}.{1}|{2}.Deploy.0 = {1}|{2}", projectGuid, name, architecture);
                    }
                }
            }
            sw.WriteLine("	EndGlobalSection");
        }

        private static void CreateDirectories(IEnumerable<VisualStudioDirectory> directories, FileDescriptor solutionFile, StreamWriter sw)
        {
            foreach (VisualStudioDirectory directory in directories)
                if (directory.Files.Count > 0 || directory.Projects.Count > 0)
                {
                    sw.WriteLine("Project(\"{0}\") = \"{1}\", \"{1}\", \"{2}\"", Guid.ParseExact(VisualStudioDirectory.DirectoryGuid, "D").ToString("B").ToUpperInvariant(), directory.Name, directory.Guid.ToString("B").ToUpperInvariant());
                    {
                        if (directory.Files.Count > 0)
                        {
                            sw.WriteLine("	ProjectSection(SolutionItems) = preProject");
                            foreach (FileDescriptor file in directory.Files)
                            {
                                sw.WriteLine("		{0} = {0}", file.GetRelativePath(solutionFile.Location));
                            }
                            sw.WriteLine("	EndProjectSection");
                        }
                    }
                    sw.WriteLine("EndProject");
                }
        }
        private static void CreateVirtualProjectTree(IEnumerable<VisualStudioDirectory> directories, StreamWriter sw)
        {
            foreach (VisualStudioDirectory directory in directories)
                if (directory.Projects.Count > 0)
                {
                    sw.WriteLine("	GlobalSection(NestedProjects) = preSolution");
                    {
                        foreach (VisualStudioProject project in directory.Projects)
                        {
                            //	e.g. "{Item-GUID} = {Folder-GUID}"
                            sw.WriteLine("		{0} = {1}", project.ProjectGuid.ToString("B").ToUpperInvariant(), directory.Guid.ToString("B").ToUpperInvariant());
                        }
                    }
                    sw.WriteLine("	EndGlobalSection");
                }
        }
    }
}
