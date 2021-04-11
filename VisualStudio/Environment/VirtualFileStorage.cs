// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Microsoft Visual Studio Project related file system tree
    /// </summary>
    public class VirtualFileStorage : VisualStudioDirectory, IEnumerable<VisualStudioDirectory>
    {
        const string DefaultDirectoryName = "Project";

        Dictionary<string, VisualStudioDirectory> directories;
        /// <summary>
        /// A collection of sub-directories
        /// </summary>
        public Dictionary<string, VisualStudioDirectory> Directories
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return directories; }
        }

        /// <summary>
        /// Creates a new class instance
        /// </summary>
        public VirtualFileStorage()
            : base(DefaultDirectoryName)
        {
            this.directories = new Dictionary<string, VisualStudioDirectory>();
        }

        public IEnumerator<VisualStudioDirectory> GetEnumerator()
        {
            yield return this;
            foreach (VisualStudioDirectory directory in directories.Values)
                yield return directory;
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
