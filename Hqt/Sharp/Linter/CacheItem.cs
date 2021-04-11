// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Cached code metadata object
    /// </summary>
    [Serializable]
    [InitializeOnLoad]
    public class CacheItem : IDisposable
    {
        /// <summary>
        /// Last modification timestamp
        /// </summary>
        [Serialized(0)]
        public DateTime Timestamp;

        /// <summary>
        /// A collection of configuration entries
        /// </summary>
        [Serialized(1)]
        public Dictionary<string, CacheEntry> Entries;

        static CacheItem()
        {
            TypeFormatter.Register<CacheItem>();
        }
        /// <summary>
        /// Creates a new metadata object instance
        /// </summary>
        public CacheItem()
        {
            Entries = CollectionPool<Dictionary<string, CacheEntry>, string, CacheEntry>.Get();
        }
        /// <summary>
        /// Creates a new metadata object instance at current date
        /// </summary>
        public CacheItem(DateTime fileDate)
            : this()
        {
            this.Timestamp = fileDate;
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void Dispose()
        {
            foreach (CacheEntry entry in Entries.Values)
            {
                entry.Dispose();
            }
            CollectionPool<Dictionary<string, CacheEntry>, string, CacheEntry>.Return(Entries);
        }
    }
}
