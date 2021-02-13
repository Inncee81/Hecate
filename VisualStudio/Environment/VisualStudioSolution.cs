// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using SE.Hecate.Build;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// A component added to the coresponding command when a solution file was created successfully
    /// </summary>
    public struct VisualStudioSolution
    {
        FileDescriptor location;
        /// <summary>
        /// The file system location of the file created
        /// </summary>
        public FileDescriptor Location
        {
            get { return location; }
        }

        string platform;
        /// <summary>
        /// The target platform this solution was created for
        /// </summary>
        public string Platform
        {
            get { return platform; }
        }

        PlatformTarget target;
        /// <summary>
        /// The target architecture this solution was created for
        /// </summary>
        public PlatformTarget Target
        {
            get { return target; }
        }

        VisualStudioVersion version;
        /// <summary>
        /// The target VisualStudio version this solution was created for
        /// </summary>
        public VisualStudioVersion Version
        {
            get { return version; }
        }

        /// <summary>
        /// Creates a new component instance
        /// </summary>
        public VisualStudioSolution(BuildProfile profile, FileDescriptor location)
        {
            this.location = location;
            this.platform = profile.Platform;
            this.target = profile.Target;
            this.version = BuildParameter.Version;
        }
    }
}