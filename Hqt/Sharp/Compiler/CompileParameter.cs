// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.Config;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// A fixed set of CSharp compilation action related settings provided on the command line 
    /// </summary>
    [PropertyInfo(Category = "build command")]
    public sealed class CompileParameter
    {
        [NamedProperty("csc")]
        [PropertyDescription("Compiles C# code with the given profile name or default", Type = PropertyType.Optional)]
        private static string profile = string.Empty;
        /// <summary>
        /// Enables CSharp code compilation and provides an optional build profile to use
        /// </summary>
        public static string Profile
        {
            get { return profile; }
        }

        private CompileParameter()
        { }
    }
}
