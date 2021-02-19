// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SE.Hecate.Build;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// A component added to the coresponding BuildModule when Cpp files have been detected
    /// </summary>
    public class CppModule : IEnumerable<FileDescriptor>
    {
        readonly Dictionary<string, CppModuleSettings> settings;
        /// <summary>
        /// A collection of Cpp related configurations that belong to this component
        /// </summary>
        public Dictionary<string, CppModuleSettings> Settings
        {
            get { return settings; }
        }

        CppModuleSettings @default;
        /// <summary>
        /// The default Cpp related configuration that belongs to this component
        /// </summary>
        public CppModuleSettings Default
        {
            get { return @default; }
        }

        readonly HashSet<FileDescriptor> files;
        /// <summary>
        /// A collection of valid code files detected for this module
        /// </summary>
        public HashSet<FileDescriptor> Files
        {
            get { return files; }
        }

        /// <summary>
        /// Creates a new component instance from the provided profile
        /// </summary>
        public CppModule(BuildProfile profile, bool isPackage)
        {
            this.settings = new Dictionary<string, CppModuleSettings>();
            this.files = new HashSet<FileDescriptor>();
            BuildConfiguration @default = profile.Default;
            if (!BuildParameter.Fast)
            {
                foreach (BuildConfiguration config in profile.Configurations)
                {
                    CppModuleSettings conf = new CppModuleSettings(config, isPackage);
                    if (config == @default)
                    {
                        this.@default = conf;
                    }
                    this.settings.Add(conf.Name, conf);
                }
            }
            else
            {
                CppModuleSettings conf = new CppModuleSettings(@default, isPackage);
                this.@default = conf;
                this.settings.Add(conf.Name, conf);
            }
        }

        public IEnumerator<FileDescriptor> GetEnumerator()
        {
            return files.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
