// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.Apollo.Package;
using SE.Config;

namespace SE.Hecate.Packages
{
    /// <summary>
    /// An auto converter used on auto-loading package IDs from the command line
    /// </summary>
    public class PackageTargetConverter : ITypeConverter
    {
        /// <summary>
        /// Creates a new instance of the converter
        /// </summary>
        public PackageTargetConverter()
        { }

        public bool TryParseValue(Type targetType, object value, out object result)
        {
            string id = value as string; if (!string.IsNullOrWhiteSpace(id))
            {
                PackageTarget target; if (PackageTarget.TryParse(id, out target))
                {
                    result = target;
                    return true;
                }
            }
            result = null;
            return false;
        }
    }
}
