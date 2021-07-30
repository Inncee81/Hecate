// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using SE.Config;
using SE.CppLang;
using SE.Parsing;

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
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return sdkVersion; }
        }

        [NamedProperty("plugins", TypeConverter = typeof(PathConverter))]
        private static HashSet<PathDescriptor> plugins;
        /// <summary>
        /// A collection of paths that contain Cpp plugin assemblies
        /// </summary>
        public static IEnumerable<PathDescriptor> Plugins
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return plugins; }
        }

        private readonly static List<IParserRulePool<CppToken>> lintingRules;
        /// <summary>
        /// A collection of ParserRule providers used in addition when linting Cpp files
        /// </summary>
        public static List<IParserRulePool<CppToken>> LintingRules
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return lintingRules; }
        }

        static BuildParameter()
        {
            lintingRules = new List<IParserRulePool<CppToken>>();
            {
                lintingRules.Add(ParserRulePool<MainRule, CppToken>.Instance);
                if ((Application.Platform & PlatformName.Windows) == PlatformName.Windows)
                {
                    lintingRules.Add(ParserRulePool<WinMainRule, CppToken>.Instance);
                }
            }
            plugins = new HashSet<PathDescriptor>();
        }
        private BuildParameter()
        { }
    }
}
