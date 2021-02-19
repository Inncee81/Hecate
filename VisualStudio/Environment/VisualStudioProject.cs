// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Microsoft Visual Studio Project
    /// </summary>
    public abstract class VisualStudioProject
    {
        /// <summary>
        /// Microsoft Visual C# .NET Project GUID
        /// </summary>
        public const string SharpGuid = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";
        /// <summary>
        /// Microsoft Visual C++ Project GUID
        /// </summary>
        public const string CppGuid = "8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942";

        FileDescriptor file;
        /// <summary>
        /// The local project file
        /// </summary>
        public FileDescriptor File
        {
            get { return file; }
        }

        /// <summary>
        /// The name of this project
        /// </summary>
        public string Name
        {
            get { return file.Name; }
        }
        /// <summary>
        /// The full qualified file name of this project
        /// </summary>
        public string FullName
        {
            get { return file.FullName; }
        }

        List<VisualStudioProjectTarget> targets = new List<VisualStudioProjectTarget>();
        /// <summary>
        /// A collection of build targets included in this project
        /// </summary>
        public List<VisualStudioProjectTarget> Targets
        {
            get { return targets; }
        }

        /// <summary>
        /// Implemented in an inherited class, determines the specific project GUID of
        /// this project instance
        /// </summary>
        public abstract Guid ProjectTypeGuid
        {
            get;
        }

        Guid projectGuid;
        /// <summary>
        /// The unique GUID this project relates to
        /// </summary>
        public Guid ProjectGuid
        {
            get { return projectGuid; }
        }

        VisualStudioVersion version;
        /// <summary>
        /// A VisualStudio version flag this project is targeting
        /// </summary>
        public VisualStudioVersion Version
        {
            get { return version; }
        }

        bool isPackage;
        /// <summary>
        /// Determines if this project was build from a package code module
        /// </summary>
        public bool IsPackage
        {
            get { return isPackage; }
        }

        /// <summary>
        /// Creates a new project instance
        /// </summary>
        public VisualStudioProject(VisualStudioVersion version, FileDescriptor file, bool isPackage)
        {
            this.file = file;
            this.version = version;
            this.isPackage = isPackage;

            LoadGuid();
        }

        void LoadGuid()
        {
            if (file.Exists())
            {
                try
                {
                    XmlDocument tmp = new XmlDocument();
                    tmp.Load(file.GetAbsolutePath());

                    XmlNodeList nodes = tmp.GetElementsByTagName("ProjectGuid");
                    if (nodes.Count > 0)
                        projectGuid = Guid.ParseExact(nodes[0].InnerText.Trim('{', '}'), "D");
                }
                catch (Exception)
                { }
            }
            if (projectGuid == Guid.Empty)
                projectGuid = Guid.NewGuid();
        }
    }
}
