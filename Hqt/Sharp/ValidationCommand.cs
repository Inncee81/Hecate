// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// A pipeline message used to process a CSharp related lookup task
    /// </summary>
    public class ValidationCommand : KernelMessage, IEnumerable<FileSystemDescriptor>
    {
        IEnumerable<FileSystemDescriptor> files;
        BuildModule module;

        /// <summary>
        /// The name of the BuildModule instance attached to this action
        /// </summary>
        public string Name
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return module.Name; }
        }

        /// <summary>
        /// Determines if the BuildModule instance attached to this action was load 
        /// as part of the package lookup
        /// </summary>
        public bool IsPackage
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return module.IsPackage; }
        }

        BuildProfile profile;
        /// <summary>
        /// The build profile instance attached to this action
        /// </summary>
        public BuildProfile Profile
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return profile; }
        }

        /// <summary>
        /// Creates a new message instance from the provided CSharp files
        /// </summary>
        public ValidationCommand(BuildModule module, BuildProfile profile, IEnumerable<FileSystemDescriptor> files)
            : base(module.Template | (UInt32)ProcessorFamilies.SharpInitialize, module.Location)
        {
            this.module = module;
            this.profile = profile;
            this.files = files;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public IEnumerator<FileSystemDescriptor> GetEnumerator()
        {
            return files.GetEnumerator();
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
