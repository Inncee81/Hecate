// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SE.Flex;

namespace SE.Hecate.Packages
{
    /// <summary>
    /// A pipeline message used to engage certain package related processing
    /// </summary>
    public class PackageCommand : KernelMessage, IEnumerable<string>
    {
        IEnumerable<string> packages;

        /// <summary>
        /// Creates a new message from provided package IDs
        /// </summary>
        public PackageCommand(UInt32 id, PathDescriptor path, IEnumerable<string> packages)
            : base(TemplateId.Create() | id, path)
        {
            this.packages = packages;
        }
        /// <summary>
        /// Creates a new message from provided package IDs
        /// </summary>
        public PackageCommand(TemplateId template, PathDescriptor path, IEnumerable<string> packages)
            : base(template, path)
        {
            this.packages = packages;
        }

        /// <summary>
        /// Creates a new message instance from given parameters
        /// </summary>
        /// <param name="family">The processor family to target</param>
        /// <param name="path">A current worker path to use</param>
        /// <param name="paths">A collection of paths included into the build action</param>
        public static PackageCommand Create(ProcessorFamilies family, PathDescriptor path, IEnumerable<string> paths)
        {
            return new PackageCommand((UInt32)family, path, paths);
        }
        /// <summary>
        /// Creates a related message instance from given parameters
        /// </summary>
        /// <param name="root">The root message the instance should base on</param>
        /// <param name="family">The processor family to target</param>
        /// <param name="path">A current worker path to use</param>
        public static PackageCommand Create(PackageCommand root, ProcessorFamilies family, PathDescriptor path)
        {
            return new PackageCommand(root.Template | (UInt32)family, path, root.packages);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return packages.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
