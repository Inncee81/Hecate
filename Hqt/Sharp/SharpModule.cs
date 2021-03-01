// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// A component added to the coresponding BuildModule when CSharp files have been detected
    /// </summary>
    public class SharpModule : IEnumerable<FileDescriptor>
    {
        readonly Dictionary<string, SharpModuleSettings> settings;
        /// <summary>
        /// A collection of CSharp related configurations that belong to this component
        /// </summary>
        public Dictionary<string, SharpModuleSettings> Settings
        {
            get { return settings; }
        }

        SharpModuleSettings @default;
        /// <summary>
        /// The default CSharp related configuration that belongs to this component
        /// </summary>
        public SharpModuleSettings Default
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
        public SharpModule(BuildProfile profile)
        {
            this.settings = new Dictionary<string, SharpModuleSettings>();
            this.files = new HashSet<FileDescriptor>();
            BuildConfiguration @default = profile.Default;
            if (!Build.BuildParameter.Fast)
            {
                foreach (BuildConfiguration config in profile.Configurations)
                {
                    SharpModuleSettings conf = new SharpModuleSettings(config);
                    if (config == @default)
                    {
                        this.@default = conf;
                    }
                    this.settings.Add(conf.Name, conf);
                }
            }
            else
            {
                SharpModuleSettings conf = new SharpModuleSettings(@default);
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
