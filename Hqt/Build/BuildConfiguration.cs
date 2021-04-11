// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using SE.Apollo.Package;
using SE.Config;

namespace SE.Hecate.Build
{
    /// <summary>
    /// Stores a set of options related to a build profile
    /// </summary>
    public class BuildConfiguration
    {
        public const string DefaultOutputPath = "Deploy";

        readonly string name;
        /// <summary>
        /// A name provided to this configuration
        /// </summary>
        public string Name
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return name; }
        }

        [NamedProperty("define", TypeConverter = typeof(KeyValuePairConverter))]
        Dictionary<string, string> defines;
        /// <summary>
        /// A collection of macros defined when parsing text
        /// </summary>
        public Dictionary<string, string> Defines
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return defines; }
        }

        [NamedProperty("optimize")]
        bool optimize;
        /// <summary>
        /// Defines if build actions should produce optimized results if possible
        /// </summary>
        public bool Optimize
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return optimize || !Debug; }
        }

        [NamedProperty("debug")]
        bool debug;
        /// <summary>
        /// Defines if buld actions should add debug information to the result if possible
        /// </summary>
        public bool Debug
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return debug && !optimize; }
        }

        [NamedProperty("debugsymbols", DefaultValue = true)]
        bool debugSymbols;
        /// <summary>
        /// Defines if build actions should provide external debug information if possible
        /// </summary>
        public bool DebugSymbols
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return debugSymbols; }
        }

        [NamedProperty("warnaserror", DefaultValue = true)]
        bool warningAsError;
        /// <summary>
        /// Defines if warnings should be treated as errors and stop further processing
        /// </summary>
        public bool WarningAsError
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return warningAsError; }
        }

        [NamedProperty("deploymentpaths", TypeConverter = typeof(KeyValuePairConverter))]
        Dictionary<string, string> targetPaths;
        /// <summary>
        /// A collection of locations on which certain build actions will request the absolute deployment
        /// location based on a filter
        /// </summary>
        public Dictionary<string, string> TargetPaths
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return targetPaths; }
        }

        [NamedProperty("parameter", TypeConverter = typeof(KeyValuePairConverter))]
        Dictionary<string, object> properties;
        /// <summary>
        /// A collection of generic properties set
        /// </summary>
        public Dictionary<string, object> Properties
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return properties; }
        }

        /// <summary>
        /// Creates a new configuration of the provided name
        /// </summary>
        public BuildConfiguration(string name)
        {
            this.name = name;
            this.optimize = false;
            this.debug = false;
            this.debugSymbols = true;
            this.warningAsError = true;

            this.defines = new Dictionary<string, string>();
            this.targetPaths = new Dictionary<string, string>();
            this.properties = new Dictionary<string, object>();
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            BuildConfiguration conf = (obj as BuildConfiguration);
            if (conf != null)
            {
                return name.Equals(conf.name);
            }
            return false;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override string ToString()
        {
            return name.ToString();
        }

        /// <summary>
        /// Resolves any provided directory path based on the build action family to an absolute
        /// location under the project root
        /// </summary>
        /// <param name="moduleTarget">The directory path a build module was found at</param>
        /// <param name="family">A build action family to add</param>
        /// <returns>The absolute path the deployment directory</returns>
        public string GetDeploymentPath(PathDescriptor moduleTarget, string family)
        {
            string[] path = moduleTarget.GetAbsolutePath().Split(Path.DirectorySeparatorChar);
            foreach (KeyValuePair<string, string> targetPath in targetPaths)
            {
                Filter filter = new Filter();
                FilterToken last = null;
                string[] tiles = PathDescriptor.Normalize(targetPath.Key).Split('/');
                foreach (string tile in tiles)
                {
                    FilterToken current = null;
                    if (last != null) current = last.GetChild(tile);
                    if (current == null)
                    {
                        if (last != null) current = filter.Add(last, tile);
                        else current = filter.Add(tile);
                    }
                    last = current;
                }
                if (filter.IsMatch(path))
                {
                    return targetPath.Value.Replace("[name]", moduleTarget.Name)
                                           .Replace("[family]", family);
                }
            }
            return DefaultOutputPath;
        }
    }
}
