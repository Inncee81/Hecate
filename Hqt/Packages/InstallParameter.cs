// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.Config;

namespace SE.Hecate.Packages
{
    /// <summary>
    /// A fixed set of installation action related settings provided on the command line
    /// </summary>
    [PropertyInfo(Category = "install command")]
    public sealed class InstallParameter
    {
        [NamedProperty('f', "force")]
        [PropertyDescription("Skips further checks and forces packages to be installed", Type = PropertyType.Optional)]
        private static bool force = false;
        /// <summary>
        /// A flag to disable certain existance checks while installing packages
        /// </summary>
        public static bool Force
        {
            get { return force; }
        }

        private InstallParameter()
        { }
    }
}
