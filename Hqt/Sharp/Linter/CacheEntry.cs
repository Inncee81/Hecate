// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
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
        /// A collection of using directives referenced in the underlaying code file
        /// </summary>
        [Serialized(1)]
        public List<string> UsingDirectives;

        /// <summary>
        /// A collection of namespaces declared in the underlaying code file
        /// </summary>
        [Serialized(2)]
        public List<string> Namespaces;

        static CacheEntry()
        {
            TypeFormatter.Register<CacheEntry>();
        }
        /// <summary>
        /// Creates a new configuration metadata instance
        /// </summary>
        public CacheEntry()
        {
            UsingDirectives = CollectionPool<List<string>, string>.Get();
            Namespaces = CollectionPool<List<string>, string>.Get();
        }
        public void Dispose()
        {
            CollectionPool<List<string>, string>.Return(UsingDirectives);
            CollectionPool<List<string>, string>.Return(Namespaces);
        }
    }
}
