// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using SE.Apollo.Package;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Stores a set of extended options related to a CSharp code module component
    /// </summary>
    public class SharpModuleSettings
    {
        BuildConfiguration config;

        /// <summary>
        /// A name provided to this configuration
        /// </summary>
        public string Name
        {
            get { return config.Name; }
        }
        /// <summary>
        /// A collection of macros defined when parsing text
        /// </summary>
        public IEnumerable<string> Defines
        {
            get { return config.Defines.Keys; }
        }
        /// <summary>
        /// Defines if build actions should produce optimized results if possible
        /// </summary>
        public bool Optimize
        {
            get { return config.Optimize; }
        }
        /// <summary>
        /// Defines if buld actions should add debug information to the result if possible
        /// </summary>
        public bool Debug
        {
            get { return config.Debug; }
        }
        /// <summary>
        /// Defines if build actions should provide external debug information if possible
        /// </summary>
        public bool DebugSymbols
        {
            get { return config.DebugSymbols; }
        }
        /// <summary>
        /// Defines if warnings should be treated as errors and stop further processing
        /// </summary>
        public bool WarningAsError
        {
            get { return config.WarningAsError; }
        }

        HashSet<string> usingDirectives;
        /// <summary>
        /// A collection of using directives referenced in the underlaying code files
        /// </summary>
        public HashSet<string> UsingDirectives
        {
            get { return usingDirectives; }
        }

        HashSet<string> namespaces;
        /// <summary>
        /// A collection of namespaces provided in the underlaying code files
        /// </summary>
        public HashSet<string> Namespaces
        {
            get { return namespaces; }
        }

        BuildModuleType assemblyType;
        /// <summary>
        /// The assembly type this configuration results in when an action is performed that
        /// relates to it
        /// </summary>
        public BuildModuleType AssemblyType
        {
            get { return assemblyType; }
            set { assemblyType = value; }
        }

        HashSet<FileDescriptor> references;
        /// <summary>
        /// A collection of assembly references requiered by the underlaying code files
        /// </summary>
        public HashSet<FileDescriptor> References
        {
            get { return references; }
        }

        HashSet<BuildModule> dependencies;
        /// <summary>
        /// A collection of CSharp code modules required by the underlaying code files
        /// </summary>
        public HashSet<BuildModule> Dependencies
        {
            get { return dependencies; }
        }

        /// <summary>
        /// Creates a new CSharp configuration from the profided build setting
        /// </summary>
        /// <param name="config"></param>
        public SharpModuleSettings(BuildConfiguration config)
        {
            this.config = config;
            this.usingDirectives = new HashSet<string>();
            this.namespaces = new HashSet<string>();
            this.references = new HashSet<FileDescriptor>();
            this.dependencies = new HashSet<BuildModule>();
            this.assemblyType = BuildModuleType.DynamicLibrary;
        }

        public static implicit operator BuildConfiguration(SharpModuleSettings conf)
        {
            return conf.config;
        }

        public override int GetHashCode()
        {
            return config.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            SharpModuleSettings conf = (obj as SharpModuleSettings);
            if (conf != null)
            {
                return config.Equals(conf.config);
            }
            return false;
        }

        public override string ToString()
        {
            return config.ToString();
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
            return config.GetDeploymentPath(moduleTarget, family);
        }

        /// <summary>
        /// Tests dependencies against package metadata and removes them if outdated
        /// </summary>
        /// <param name="meta">The package metadata to test against</param>
        /// <returns>True if a more recent package already exists, false otherwise</returns>
        public bool AvaragePackageExists(PackageMeta meta)
        {
            foreach (BuildModule dependency in dependencies)
            {
                PackageMeta pkg; if (dependency.TryGetProperty<PackageMeta>(out pkg) && pkg.Id.Equals(meta.Id))
                {
                    if (pkg.Version < meta.Version)
                    {
                        dependencies.Remove(dependency);
                        break;
                    }
                    else return true;
                }
            }
            return false;
        }
    }
}
