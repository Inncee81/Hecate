// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// A pipeline message used to process a Visual Cpp related task
    /// </summary>
    public class VisualCppCommand : KernelMessage
    {
        VisualCppProject project;
        /// <summary>
        /// The Cpp project instance attached to this action
        /// </summary>
        public VisualCppProject Project
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return project; }
        }

        BuildProfile profile;
        /// <summary>
        /// The build profile instance attached to this action
        /// </summary>
        public BuildProfile Profile
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return profile; }
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
