// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.CommandLine;
using SE.Config;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// A fixed set of VisualStudio related settings provided on the command line
    /// </summary>
    [PropertyInfo(Category = "build command")]
    public sealed class BuildParameter
    {
        [NamedProperty("vs2010")]
        [PropertyDescription("Creates a Visual Studio 2010 solution from the given profile name or default", Type = PropertyType.Optional)]
        private static string VisualStudio2010Target
        {
            set 
            {
                profile = value;
                version = VisualStudioVersion.VisualStudio2010;
            }
        }

        [NamedProperty("vs2012")]
        [PropertyDescription("Creates a Visual Studio 2012 solution from the given profile name or default", Type = PropertyType.Optional)]
        private static string VisualStudio2012Target
        {
            set
            {
                profile = value;
                version = VisualStudioVersion.VisualStudio2012;
            }
        }

        [NamedProperty("vs2013")]
        [PropertyDescription("Creates a Visual Studio 2013 solution from the given profile name or default", Type = PropertyType.Optional)]
        private static string VisualStudio2013Target
        {
            set
            {
                profile = value;
                version = VisualStudioVersion.VisualStudio2013;
            }
        }

        [NamedProperty("vs2015")]
        [PropertyDescription("Creates a Visual Studio 2015 solution from the given profile name or default", Type = PropertyType.Optional)]
        private static string VisualStudio2015Target
        {
            set
            {
                profile = value;
                version = VisualStudioVersion.VisualStudio2015;
            }
        }

        [NamedProperty("vs2017")]
        [PropertyDescription("Creates a Visual Studio 2017 solution from the given profile name or default", Type = PropertyType.Optional)]
        private static string VisualStudio2017Target
        {
            set
            {
                profile = value;
                version = VisualStudioVersion.VisualStudio2017;
            }
        }

        [NamedProperty("vs2019")]
        [PropertyDescription("Creates a Visual Studio 2019 solution from the given profile name or default", Type = PropertyType.Optional)]
        private static string VisualStudio2019Target
        {
            set
            {
                profile = value;
                version = VisualStudioVersion.VisualStudio2019;
            }
        }

        private static VisualStudioVersion version;
        /// <summary>
        /// The desired version to use
        /// </summary>
        public static VisualStudioVersion Version
        {
            get { return version; }
        }

        private static string profile = string.Empty;
        /// <summary>
        /// The desired profile to use
        /// </summary>
        public static string Profile
        {
            get { return profile; }
        }

        static BuildParameter()
        {
            if (CommandLineOptions.Default.ContainsKey("vs2010"))
            {
                version = VisualStudioVersion.VisualStudio2010;
            }
            else if (CommandLineOptions.Default.ContainsKey("vs2012"))
            {
                version = VisualStudioVersion.VisualStudio2012;
            }
            else if (CommandLineOptions.Default.ContainsKey("vs2013"))
            {
                version = VisualStudioVersion.VisualStudio2013;
            }
            else if (CommandLineOptions.Default.ContainsKey("vs2015"))
            {
                version = VisualStudioVersion.VisualStudio2015;
            }
            else if (CommandLineOptions.Default.ContainsKey("vs2017"))
            {
                version = VisualStudioVersion.VisualStudio2017;
            }
            else if (CommandLineOptions.Default.ContainsKey("vs2019"))
            {
                version = VisualStudioVersion.VisualStudio2019;
            }
        }
        private BuildParameter()
        { }
    }
}