// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// A component added to the coresponding BuildModule when compilation has finished successfully
    /// </summary>
    public struct SharpAssembly
    {
        FileDescriptor location;
        /// <summary>
        /// The file system location of the file created by the compiler
        /// </summary>
        public FileDescriptor Location
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return location; }
        }

        string platform;
        /// <summary>
        /// The target platform this assembly was created for
        /// </summary>
        public string Platform
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return platform; }
        }

        PlatformTarget target;
        /// <summary>
        /// The target architecture this assembly was created for
        /// </summary>
        public PlatformTarget Target
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return target; }
        }

        bool isOptimized;
        /// <summary>
        /// Determines if the assembly has been optimized
        /// </summary>
        public bool IsOptimized
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return isOptimized; }
        }

        bool hasDebugSymbols;
        /// <summary>
        /// Determines if the assembly also provides debug information next to it
        /// </summary>
        public bool HasDebugSymbols
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return hasDebugSymbols; }
        }

        /// <summary>
        /// Creates a new component instance
        /// </summary>
        public SharpAssembly(BuildProfile profile, SharpModuleSettings config, FileDescriptor location)
        {
            this.location = location;
            this.platform = profile.Platform;
            this.target = profile.Target;
            this.isOptimized = config.Optimize;
            this.hasDebugSymbols = config.DebugSymbols;
        }
    }
}
