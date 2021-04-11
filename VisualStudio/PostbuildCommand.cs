// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.CompilerServices;
using SE.Flex;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// A pipeline message used to process a VisualStudio related postbuild task
    /// </summary>
    public class PostbuildCommand : KernelMessage, IEnumerable<VisualStudioProject>
    {
        IEnumerable<VisualStudioProject> projects;

        /// <summary>
        /// Creates a new message instance from the provided set of projects
        /// </summary>
        public PostbuildCommand(TemplateId template, IEnumerable<VisualStudioProject> projects)
            : base(template | (UInt32)ProcessorFamilies.Solution, Application.WorkerPath)
        {
            this.projects = projects;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public IEnumerator<VisualStudioProject> GetEnumerator()
        {
            return projects.GetEnumerator();
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
