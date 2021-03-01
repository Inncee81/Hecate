// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using SE.Config;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// A fixed set of Cpp related settings provided on the command line 
    /// </summary>
    public sealed class BuildParameter
    {
        [NamedProperty("version", Cluster = "windowssdk")]
        private static string sdkVersion = string.Empty;
        /// <summary>
        /// The target Windows SDK version to use
        /// </summary>
        public static string SdkVersion
        {
            get { return sdkVersion; }
        }

        [NamedProperty("plugins", TypeConverter = typeof(PathConverter))]
        private static HashSet<PathDescriptor> plugins = new HashSet<PathDescriptor>();
        /// <summary>
        /// A collection of paths that contain Cpp plugin assemblies
        /// </summary>
        public static IEnumerable<PathDescriptor> Plugins
        {
            get { return plugins; }
        }

        private BuildParameter()
        { }
    }
}
