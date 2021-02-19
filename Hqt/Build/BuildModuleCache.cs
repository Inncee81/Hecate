// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SE.Hecate.Build
{
    /// <summary>
    /// A code module metadata cache object
    /// </summary>
    public class BuildModuleCache<T> : Dictionary<FileDescriptor, T>, IDictionary
    {
        bool modified;
        /// <summary>
        /// Determines if the cache was modified
        /// </summary>
        public bool Modified
        {
            get { return modified; }
            set { modified = value; }
        }

        public new T this[FileDescriptor key] 
        { 
            get { return base[key]; }
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
        public BuildModuleCache()
        { }

        public new void Add(FileDescriptor key, T value)
        {
            if (!modified)
            {
                modified = true;
            }
            base.Add(key, value);
        }
        public new void Clear()
        {
            if (!modified)
            {
                modified = true;
            }
            base.Clear();
        }
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
