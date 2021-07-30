// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using SE.Apollo.Package;
using SE.Hecate.Build;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// Stores a set of extended options related to a Cpp code module component
    /// </summary>
    public class CppModuleSettings
    {
        BuildConfiguration config;
        bool isPackage;

        /// <summary>
        /// A name provided to this configuration
        /// </summary>
        public string Name
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return config.Name; }
        }
        /// <summary>
        /// A collection of macros defined when parsing text
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Defines
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return config.Defines; }
        }
        /// <summary>
        /// Defines if build actions should produce optimized results if possible
        /// </summary>
        public bool Optimize
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return config.Optimize; }
        }
        /// <summary>
        /// Defines if buld actions should add debug information to the result if possible
        /// </summary>
        public bool Debug
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return config.Debug; }
        }
        /// <summary>
        /// Defines if build actions should provide external debug information if possible
        /// </summary>
        public bool DebugSymbols
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return config.DebugSymbols; }
        }
        /// <summary>
        /// Defines if warnings should be treated as errors and stop further processing
        /// </summary>
        public bool WarningAsError
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return config.WarningAsError; }
        }

        HashSet<FileDescriptor> includeDirectives;
        /// <summary>
        /// A collection of include directives referenced in the underlaying code files
        /// </summary>
        public HashSet<FileDescriptor> IncludeDirectives
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return includeDirectives; }
        }

        BuildModuleType assemblyType;
        /// <summary>
        /// The assembly type this configuration results in when an action is performed that
        /// relates to it
        /// </summary>
        public BuildModuleType AssemblyType
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get 
            { 
                if(!isPackage && config.AssemblyType != BuildModuleType.Undefined)
                {
                    return config.AssemblyType;
                }
                else return assemblyType;
            }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            set { assemblyType = value; }
        }

        HashSet<FileDescriptor> references;
        /// <summary>
        /// A collection of assembly references requiered by the underlaying code files
        /// </summary>
        public HashSet<FileDescriptor> References
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return references; }
        }

        HashSet<BuildModule> dependencies;
        /// <summary>
        /// A collection of Cpp code modules required by the underlaying code files
        /// </summary>
        public HashSet<BuildModule> Dependencies
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return dependencies; }
        }

        /// <summary>
        /// Creates a new Cpp configuration from the profided build setting
        /// </summary>
        /// <param name="config"></param>
        public CppModuleSettings(BuildConfiguration config, bool isPackage)
        {
            this.config = config;
            this.isPackage = isPackage;
            this.includeDirectives = new HashSet<FileDescriptor>();
            this.references = new HashSet<FileDescriptor>();
            this.dependencies = new HashSet<BuildModule>();
            this.assemblyType = BuildModuleType.StaticLibrary;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public static implicit operator BuildConfiguration(CppModuleSettings conf)
        {
            return conf.config;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override int GetHashCode()
        {
            return config.GetHashCode();
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override bool Equals(object obj)
        {
            CppModuleSettings conf = (obj as CppModuleSettings);
            if (conf != null)
            {
                return config.Equals(conf.config);
            }
            return false;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
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
        [MethodImpl(OptimizationExtensions.ForceInline)]
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
