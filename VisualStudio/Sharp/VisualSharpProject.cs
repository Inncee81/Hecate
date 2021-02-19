// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using SE.Hecate.Build;
using System;
using System.Collections.Generic;
using System.IO;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Microsoft Visual Studio C# .NET Project
    /// </summary>
    public class VisualSharpProject : VisualStudioProject
    {
        public override Guid ProjectTypeGuid
        {
            get { return Guid.ParseExact(SharpGuid, "D"); }
        }

        string defaultNamespace;
        /// <summary>
        /// The default namespace of this project
        /// </summary>
        /// <remarks>The namespace will be used for files created from within VisualStudio</remarks>
        public string DefaultNamespace
        {
            get { return defaultNamespace; }
            set { defaultNamespace = value; }
        }

        readonly HashSet<BuildModule> packages;
        /// <summary>
        /// A collection of packages this project is based on
        /// </summary>
        public HashSet<BuildModule> Packages
        {
            get { return packages; }
        }

        /// <summary>
        /// Creates a new project instance
        /// </summary>
        /// <param name="version">VisualStudio version this project relates to</param>
        /// <param name="projectRoot">The file system ddirectory this project is based on</param>
        /// <param name="name">The name of this project</param>
        public VisualSharpProject(VisualStudioVersion version, PathDescriptor projectRoot, string name)
            : base(version, new FileDescriptor(projectRoot, "{0}.csproj", name), false)
        {
            packages = new HashSet<BuildModule>();
        }
    }
}
