// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using SE.Config;

namespace SE.Hecate.Build
{
    /// <summary>
    /// A generic set of build action properties
    /// </summary>
    [PropertyInfo(Category = "build command")]
    public class BuildProfile
    {
        string name;
        /// <summary>
        /// The name of this profile
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = string.Concat(platform, "_", target.ToString());
                }
                return name;
            }
        }

        [NamedProperty('p', "platform")]
        [PropertyDescription("Target platform to build for", Type = PropertyType.Optional)]
        string platform;
        /// <summary>
        /// The target platform used by build actions
        /// </summary>
        public string Platform
        {
            get { return platform; }
        }
        
        [NamedProperty('t', "target")]
        [PropertyDescription("Target architecture to build for", Type = PropertyType.Optional)]
        PlatformTarget target;
        /// <summary>
        /// The target architecture used by build actions
        /// </summary>
        public PlatformTarget Target
        {
            get { return target; }
        }

        [NamedProperty("config", TypeConverter = typeof(BuildConfigurationConverter))]
        HashSet<BuildConfiguration> configurations;
        /// <summary>
        /// A set of configuration settings assigned to this profile
        /// </summary>
        public HashSet<BuildConfiguration> Configurations
        {
            get { return configurations; }
        }

        [NamedProperty('c', "default")]
        [PropertyDescription("Name of the build configuation used by single configuration tasks", Type = PropertyType.Optional)]
        string defaultConfig;
        /// <summary>
        /// Name of the build configuation used by single configuration tasks
        /// </summary>
        public BuildConfiguration Default
        {
            get
            {
                BuildConfiguration result = configurations.Where(x => x.Name.Equals(defaultConfig)).FirstOrDefault();
                if (result == null)
                {
                    result = configurations.FirstOrDefault();
                }
                return result;
            }
        }

        /// <summary>
        /// Creates a new instance from current platform and architecture
        /// </summary>
        public BuildProfile()
        {
            this.platform = Application.Platform.ToString();
            this.target = Application.Target;
            this.defaultConfig = "release";

            this.configurations = new HashSet<BuildConfiguration>();
        }
        /// <summary>
        /// Creates a new named instance
        /// </summary>
        /// <param name="name"></param>
        public BuildProfile(string name)
            : this()
        {
            this.name = name;
        }

        /// <summary>
        /// Assigns mandatory default values to this profile
        /// </summary>
        public void AddDefaultValues()
        {
            if (configurations.Count == 0)
                configurations.Add(new BuildConfiguration(defaultConfig));
        }

        public override string ToString()
        {
            return name;
        }
    }
}
