// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using SE.Hecate.Build;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// Cached code configuration metadata
    /// </summary>
    [Serializable]
    [InitializeOnLoad]
    public class CacheEntry : IDisposable
    {
        /// <summary>
        /// The assembly type this configuration was pinned to
        /// </summary>
        [Serialized(0)]
        public BuildModuleType AssemblyType;

        /// <summary>
        /// A collection of include directives referenced in the underlaying code file
        /// </summary>
        [Serialized(1)]
        public List<FileDescriptor> IncludeDirectives;

        static CacheEntry()
        {
            TypeFormatter.Register<CacheEntry>();
        }
        /// <summary>
        /// Creates a new configuration metadata instance
        /// </summary>
        public CacheEntry()
        {
            IncludeDirectives = CollectionPool<List<FileDescriptor>, FileDescriptor>.Get();
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void Dispose()
        {
            CollectionPool<List<FileDescriptor>, FileDescriptor>.Return(IncludeDirectives);
        }
    }
}
