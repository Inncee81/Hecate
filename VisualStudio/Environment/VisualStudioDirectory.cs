// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// A virtual directory included in a VisualStudio project
    /// </summary>
    public class VisualStudioDirectory
    {
        /// <summary>
        /// The VisualStudio related directory GUID
        /// </summary>
        public const string DirectoryGuid = "2150E333-8FDC-42A3-9474-1A3956D46DE8";

        Guid guid = Guid.NewGuid();
        /// <summary>
        /// A unique GUID related to this directory instance
        /// </summary>
        public Guid Guid
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return guid; }
        }

        readonly string name;
        /// <summary>
        /// The directory name as displayed in VisualStudio
        /// </summary>
        public string Name
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return name; }
        }

        HashSet<FileSystemDescriptor> files = new HashSet<FileSystemDescriptor>();
        /// <summary>
        /// A collection of files grouped by this directory
        /// </summary>
        public HashSet<FileSystemDescriptor> Files
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return files; }
        }

        HashSet<VisualStudioProject> projects = new HashSet<VisualStudioProject>();
        /// <summary>
        /// A collection of VisualStudio projects grouped by this directory
        /// </summary>
        public HashSet<VisualStudioProject> Projects
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return projects; }
        }

        /// <summary>
        /// Creates a new directory instance of the provided name
        /// </summary>
        public VisualStudioDirectory(string name)
        {
            this.name = name;
        }
    }
}
