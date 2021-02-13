// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate
{
    /// <summary>
    /// An indicator that this class provides information to the user manual
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class PropertyInfoAttribute : Attribute
    {
        string category;
        /// <summary>
        /// A category information should be provided to
        /// </summary>
        public string Category
        {
            get { return category; }
            set { category = value; }
        }

        /// <summary>
        /// Creates a new attribute instance
        /// </summary>
        public PropertyInfoAttribute()
        {
            this.category = string.Empty;
        }
    }
}
