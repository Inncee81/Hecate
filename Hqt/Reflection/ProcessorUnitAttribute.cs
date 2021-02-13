// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate
{
    /// <summary>
    /// Provides additional configuration settings related to a ProcessorUnit
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ProcessorUnitAttribute : Attribute
    {
        bool isExtension;
        /// <summary>
        /// Determines if this ProcessorUnit is a standalone node rather than being grouped
        /// by a processor family
        /// </summary>
        public bool IsExtension
        {
            get { return isExtension; }
            set { isExtension = value; }
        }

        bool isBuiltIn;
        /// <summary>
        /// Indicates this ProcessorUnit being a built in core with the lowest priority
        /// </summary>
        public bool IsBuiltIn
        {
            get { return isBuiltIn; }
            set { isBuiltIn = value; }
        }

        /// <summary>
        /// Creates a new attribute instance
        /// </summary>
        public ProcessorUnitAttribute()
        { }
    }
}
