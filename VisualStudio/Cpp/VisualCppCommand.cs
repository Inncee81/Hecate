// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.Flex;
using SE.Hecate.Build;
using SE.Hecate.Cpp;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// A pipeline message used to process a Visual Cpp related task
    /// </summary>
    public class VisualCppCommand : KernelMessage
    {
        private static string windowsSDK = string.Empty;
        /// <summary>
        /// Gets the version string of the latest Windows SDK installed
        /// </summary>
        public static string WindowsSDK
        {
            get { return windowsSDK; }
        }

        VisualCppProject project;
        /// <summary>
        /// The Cpp project instance attached to this action
        /// </summary>
        public VisualCppProject Project
        {
            get { return project; }
        }

        BuildProfile profile;
        /// <summary>
        /// The build profile instance attached to this action
        /// </summary>
        public BuildProfile Profile
        {
            get { return profile; }
        }

        static VisualCppCommand()
        {
            #if _WIN32
            try
            {
                Version ver; if (VisualCppEnvironment.FindLatestWindowsSdk(out ver) != null && ver.Major >= 10)
                {
                    windowsSDK = ver.ToString();
                }
            }
            catch
            { }
            #endif
        }
        /// <summary>
        /// Creates a new message instance from the provided Cpp files
        /// </summary>
        public VisualCppCommand(VisualCppProject project, BuildProfile profile)
            : base(TemplateId.Create() | (UInt32)ProcessorFamilies.VisualCpp, project.File.Location)
        {
            this.project = project;
            this.profile = profile;
        }
    }
}
