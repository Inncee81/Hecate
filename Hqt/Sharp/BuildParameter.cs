// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using SE.Config;
using SE.Parsing;
using SE.SharpLang;
#if !NET_FRAMEWORK
using SE.Hecate.Build;
#endif

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// A fixed set of CSharp related settings provided on the command line 
    /// </summary>
    [PropertyInfo(Category = "build command")]
    public sealed class BuildParameter
    {
        [NamedProperty("csc")]
        [PropertyDescription("Compiles C# code with the given profile name or default", Type = PropertyType.Optional)]
        private static string profile = string.Empty;
        /// <summary>
        /// Enables CSharp code compilation and provides an optional build profile to use
        /// </summary>
        [NamedProperty("publish")]
        [PropertyDescription("Creates necessary files to publish code modules as packages", Type = PropertyType.Optional)]
        public static string Profile
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return profile; }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            private set { profile = value; }
        }

        #if NET_FRAMEWORK
        [NamedProperty("referenceassemblies", TypeConverter = typeof(PathConverter))]
        private static PathDescriptor referenceAssemblies;
        /// <summary>
        /// Framework Assembly Root
        /// </summary>
        public static PathDescriptor ReferenceAssemblies
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return referenceAssemblies; }
        }
        #else
        [NamedProperty("dotnet", TypeConverter = typeof(PathConverter))]
        private static PathDescriptor dotnet;
        /// <summary>
        /// Dotnet SDK path
        /// </summary>
        public static PathDescriptor Dotnet
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return dotnet; }
        }

        [NamedProperty("roslyn", TypeConverter = typeof(CompilerConfigurationConverter))]
        private static CompilerConfiguration roslyn = null;
        /// <summary>
        /// Roslyn compiler settings
        /// </summary>
        public static CompilerConfiguration Roslyn
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return roslyn; }
        }
        #endif

        [NamedProperty("plugins", TypeConverter = typeof(PathConverter))]
        private static HashSet<PathDescriptor> plugins;
        /// <summary>
        /// A collection of paths that contain CSharp plugin assemblies
        /// </summary>
        public static IEnumerable<PathDescriptor> Plugins
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return plugins; }
        }

        private readonly static List<IParserRulePool<SharpToken>> lintingRules;
        /// <summary>
        /// A collection of ParserRule providers used in addition when linting C# files
        /// </summary>
        public static List<IParserRulePool<SharpToken>> LintingRules
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return lintingRules; }
        }

        static BuildParameter()
        {
            #if NET_FRAMEWORK
            if ((Application.Platform & PlatformName.Windows) == PlatformName.Windows)
            {
                referenceAssemblies = new PathDescriptor("%ProgramFiles(x86)%/Reference Assemblies/Microsoft/Framework/.NETFramework");
            }
            else if ((Application.Platform & PlatformName.Linux) == PlatformName.Linux)
            {
                referenceAssemblies = new PathDescriptor("/usr/lib/mono");
            }
            #else
            if ((Application.Platform & PlatformName.Windows) == PlatformName.Windows)
            {
                dotnet = new PathDescriptor("%ProgramFiles%/dotnet/shared");
            }
            #endif
            else Application.Warning(SeverityFlags.None, "Unsupported C# Platform");
            lintingRules = new List<IParserRulePool<SharpToken>>();
            {
                lintingRules.Add(ParserRulePool<NamespaceRule, SharpToken>.Instance);
                lintingRules.Add(ParserRulePool<UsingRule, SharpToken>.Instance);
                lintingRules.Add(ParserRulePool<MainRule, SharpToken>.Instance);
            }
            plugins = new HashSet<PathDescriptor>();
        }
        private BuildParameter()
        { }
    }
}
