// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.Config;

namespace SE.Hecate.Build
{
    /// <summary>
    /// A fixed set of build action related settings provided on the command line
    /// </summary>
    [PropertyInfo(Category = "build command")]
    public sealed class BuildParameter
    {
        [NamedProperty('f', "fast")]
        [PropertyDescription("Tells the pipeline to set on processing speed rather than precision", Type = PropertyType.Optional)]
        private static bool fast = false;
        /// <summary>
        /// A flag to process with speed rather than precision
        /// </summary>
        public static bool Fast
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return fast; }
        }

        [NamedProperty('e', "rebuild")]
        [PropertyDescription("Tells the pipeline to performe clean rebuilding ignoring caches", Type = PropertyType.Optional)]
        private static bool rebuild = false;
        /// <summary>
        /// A flag to process with clean data and no caches
        /// </summary>
        public static bool Rebuild
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return rebuild; }
        }

        private BuildParameter()
        { }
    }
}
