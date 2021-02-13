// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate
{
    /// <summary>
    /// Indicates a class as to be initialized by the Kernel
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class InitializeOnLoadAttribute : Attribute
    {
        /// <summary>
        /// Creates a new attribute instance
        /// </summary>
        public InitializeOnLoadAttribute()
        { }
    }
}
