// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.Config;

namespace SE.Hecate.Packages
{
    /// <summary>
    /// A fixed set of remove action related settings provided on the command line
    /// </summary>
    [PropertyInfo(Category = "remove command")]
    public sealed class RemoveParameter
    {
        [NamedProperty('v', "revision")]
        [PropertyDescription("Removes packages up to the provided revision. Implies 'latest' as default revision", Type = PropertyType.Optional)]
        private static bool revision = false;
        /// <summary>
        /// A flag to enable removing packages up to the provided revision
        /// </summary>
        public static bool Revision
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return revision; }
        }

        [NamedProperty('e', "recursive")]
        [PropertyDescription("Also removes packages from the dependency tree if possible", Type = PropertyType.Optional)]
        private static bool recursive = false;
        /// <summary>
        /// A flag to indicate recursive package removal
        /// </summary>
        public static bool Recursive
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return recursive; }
        }

        private RemoveParameter()
        { }
    }
}
