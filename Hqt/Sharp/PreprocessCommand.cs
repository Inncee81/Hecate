// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// A pipeline message used to process a CSharp related lookup task
    /// </summary>
    public class PreprocessCommand : KernelMessage, IEnumerable<FileSystemDescriptor>
    {
        IEnumerable<FileSystemDescriptor> files;

        BuildProfile profile;
        /// <summary>
        /// The build profile instance attached to this action
        /// </summary>
        public BuildProfile Profile
        {
            get { return profile; }
        }

        /// <summary>
        /// Creates a new message instance from the provided CSharp files
        /// </summary>
        public PreprocessCommand(BuildModule module, BuildProfile profile, IEnumerable<FileSystemDescriptor> files)
            : base(module.Template | (UInt32)ProcessorFamilies.SharpInitialize, module.Location)
        {
            this.profile = profile;
            this.files = files;
        }

        public IEnumerator<FileSystemDescriptor> GetEnumerator()
        {
            return files.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
