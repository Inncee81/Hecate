// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return configuration; }
        }

        readonly HashSet<FileDescriptor> files;
        /// <summary>
        /// The local collection of files related to this target
        /// </summary>
        public HashSet<FileDescriptor> Files
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return files; }
        }

        readonly HashSet<FileDescriptor> dependencies;
        /// <summary>
        /// A collection of dependencies this target relates to
        /// </summary>
        public HashSet<FileDescriptor> Dependencies
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return dependencies; }
        }

        readonly HashSet<VisualStudioProject> references;
        /// <summary>
        /// A collection of VisualStudio projects this target relates to
        /// </summary>
        public HashSet<VisualStudioProject> References
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return references; }
        }

        BuildModuleType type;
        /// <summary>
        /// The output assembly type of this target
        /// </summary>
        public BuildModuleType Type
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return type; }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            set { type = value; }
        }

        string assemblyName;
        /// <summary>
        /// The output assembly name of this target
        /// </summary>
        public string AssemblyName
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return assemblyName; }
            [MethodImpl(OptimizationExtensions.ForceInline)]
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
