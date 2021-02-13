// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using SE.Hecate.Build;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Microsoft Visual Studio Build Target
    /// </summary>
    public class VisualStudioProjectTarget
    {
        BuildConfiguration configuration;
        /// <summary>
        /// The build configuration this target depends on
        /// </summary>
        public BuildConfiguration Configuration
        {
            get { return configuration; }
        }

        readonly HashSet<FileDescriptor> files;
        /// <summary>
        /// The local collection of files related to this target
        /// </summary>
        public HashSet<FileDescriptor> Files
        {
            get { return files; }
        }

        readonly HashSet<FileDescriptor> dependencies;
        /// <summary>
        /// A collection of dependencies this target relates to
        /// </summary>
        public HashSet<FileDescriptor> Dependencies
        {
            get { return dependencies; }
        }

        readonly HashSet<VisualStudioProject> references;
        /// <summary>
        /// A collection of VisualStudio projects this target relates to
        /// </summary>
        public HashSet<VisualStudioProject> References
        {
            get { return references; }
        }

        BuildModuleType type;
        /// <summary>
        /// The output assembly type of this target
        /// </summary>
        public BuildModuleType Type
        {
            get { return type; }
            set { type = value; }
        }

        string assemblyName;
        /// <summary>
        /// The output assembly name of this target
        /// </summary>
        public string AssemblyName
        {
            get { return assemblyName; }
            set { assemblyName = value; }
        }

        /// <summary>
        /// Creates a new target instance from the provided build configuration
        /// </summary>
        /// <param name="configuration"></param>
        public VisualStudioProjectTarget(BuildConfiguration configuration)
        {
            this.configuration = configuration;
            this.files = new HashSet<FileDescriptor>();
            this.dependencies = new HashSet<FileDescriptor>();
            this.references = new HashSet<VisualStudioProject>();
        }
    }
}
