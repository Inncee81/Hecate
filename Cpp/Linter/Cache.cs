// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// A code module metadata cache object
    /// </summary>
    public class Cache : Dictionary<FileDescriptor, CacheItem>, IDictionary
    {
        bool modified;
        /// <summary>
        /// Determines if the cache was modified
        /// </summary>
        public bool Modified
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return modified; }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            set { modified = value; }
        }

        public new CacheItem this[FileDescriptor key] 
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return base[key]; }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            set
            {
                if (!modified)
                {
                    modified = true;
                }
                base[key] = value;
            }
        }

        /// <summary>
        /// Creates a new object instance
        /// </summary>
        public Cache()
        { }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public new void Add(FileDescriptor key, CacheItem value)
        {
            if (!modified)
            {
                modified = true;
            }
            base.Add(key, value);
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public new void Clear()
        {
            if (!modified)
            {
                modified = true;
            }
            base.Clear();
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public new bool Remove(FileDescriptor key)
        {
            bool result = base.Remove(key);
            if (!modified && result)
            {
                modified = true;
            }
            return result;
        }
    }
}
