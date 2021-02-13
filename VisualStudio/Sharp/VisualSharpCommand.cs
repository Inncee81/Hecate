// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// A pipeline message used to process a Visual CSharp related task
    /// </summary>
    public class VisualSharpCommand : KernelMessage
    {
        VisualSharpProject project;
        /// <summary>
        /// The CSharp project instance attached to this action
        /// </summary>
        public VisualSharpProject Project
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

        /// <summary>
        /// Creates a new message instance from the provided CSharp files
        /// </summary>
        public VisualSharpCommand(VisualSharpProject project, BuildProfile profile)
            : base(TemplateId.Create() | (UInt32)ProcessorFamilies.VisualSharp, project.File.Location)
        {
            this.project = project;
            this.profile = profile;
        }
    }
}
