// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using SE.Hecate.Build;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// A pipeline message used to process a Cpp related lookup task
    /// </summary>
    public class ValidationCommand : KernelMessage, IEnumerable<FileSystemDescriptor>
    {
        IEnumerable<FileSystemDescriptor> files;

        IEnumerable<object> modules;
        /// <summary>
        /// The list of BuildModule instances attached to this action
        /// </summary>
        public IEnumerable<object> Modules
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return modules; }
        }

        string name;
        /// <summary>
        /// The name of the BuildModule instance attached to this action
        /// </summary>
        public string Name
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return name; }
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

        bool isPackage;
        /// <summary>
        /// Determines if this action relates to a package module
        /// </summary>
        public bool IsPackage
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return isPackage; }
        }

        /// <summary>
        /// Creates a new message instance from the provided Cpp files
        /// </summary>
        public ValidationCommand(IEnumerable<object> modules, BuildModule module, BuildProfile profile, IEnumerable<FileSystemDescriptor> files)
            : base(module.Template | (UInt32)ProcessorFamilies.CppInitialize, module.Location)
        {
            this.modules = modules;
            this.name = module.Name;
            this.profile = profile;
            this.files = files;
            this.isPackage = module.IsPackage;
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
