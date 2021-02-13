// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Microsoft Visual Studio Project related file system tree
    /// </summary>
    public class VirtualFileStorage : VisualStudioDirectory, IEnumerable<VisualStudioDirectory>
    {
        const string DefaultDirectoryName = "Project";

        List<VisualStudioDirectory> directories;
        /// <summary>
        /// A collection of sub-directories
        /// </summary>
        public List<VisualStudioDirectory> Directories
        {
            get { return directories; }
        }

        /// <summary>
        /// Creates a new class instance
        /// </summary>
        public VirtualFileStorage()
            : base(DefaultDirectoryName)
        {
            this.directories = new List<VisualStudioDirectory>();
        }

        public IEnumerator<VisualStudioDirectory> GetEnumerator()
        {
            yield return this;
            foreach (VisualStudioDirectory directory in directories)
                yield return directory;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
