// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using SE.Flex;

namespace SE.Hecate.Build
{
    /// <summary>
    /// A pipeline message used to process a build task
    /// </summary>
    public class BuildCommand : KernelMessage, IEnumerable<FileSystemDescriptor>
    {
        IEnumerable<FileSystemDescriptor> paths;

        /// <summary>
        /// Creates a new message instance
        /// </summary>
        /// <param name="paths">A collection of paths to process in this build task</param>
        public BuildCommand(UInt32 id, PathDescriptor path, IEnumerable<FileSystemDescriptor> paths)
            : base(TemplateId.Create() | id, path)
        {
            this.paths = paths;
        }
        /// <summary>
        /// Creates a new message instance
        /// </summary>
        /// <param name="paths">A collection of paths to process in this build task</param>
        public BuildCommand(TemplateId template, PathDescriptor path, IEnumerable<FileSystemDescriptor> paths)
            : base(template, path)
        {
            this.paths = paths;
        }

        /// <summary>
        /// Creates a new message instance from given parameters
        /// </summary>
        /// <param name="family">The processor family to target</param>
        /// <param name="path">A current worker path to use</param>
        /// <param name="paths">A collection of paths included into the build action</param>
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public static BuildCommand Create(ProcessorFamilies family, PathDescriptor path, IEnumerable<FileSystemDescriptor> paths)
        {
            return new BuildCommand((UInt32)family, path, paths);
        }
        /// <summary>
        /// Creates a related message instance from given parameters
        /// </summary>
        /// <param name="root">The root message the instance should base on</param>
        /// <param name="family">The processor family to target</param>
        /// <param name="path">A current worker path to use</param>
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public static BuildCommand Create(BuildCommand root, ProcessorFamilies family, PathDescriptor path)
        {
            return new BuildCommand(root.Template | (UInt32)family, path, root.paths);
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public IEnumerator<FileSystemDescriptor> GetEnumerator()
        {
            return paths.GetEnumerator();
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
