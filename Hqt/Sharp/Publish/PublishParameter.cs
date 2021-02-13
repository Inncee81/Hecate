// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.Config;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// A fixed set of publishing related settings provided on the command line
    /// </summary>
    [PropertyInfo(Category = "build command")]
    public sealed class PublishParameter
    {
        [NamedProperty("publish")]
        [PropertyDescription("Creates necessary files to publish code modules as packages", Type = PropertyType.Optional)]
        private static string profile = string.Empty;
        /// <summary>
        /// Enables package creation of provided code modules from an optional build profile
        /// </summary>
        public static string Profile
        {
            get { return profile; }
        }

        private PublishParameter()
        { }
    }
}