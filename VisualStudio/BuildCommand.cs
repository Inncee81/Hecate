// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// A pipeline message used to process a VisualStudio related build task
    /// </summary>
    public class BuildCommand : KernelMessage, IEnumerable<BuildModule>
    {
        IEnumerable<BuildModule> modules;

        /// <summary>
        /// Creates a new message instance from the provided set of modules
        /// </summary>
        public BuildCommand(TemplateId template, ProcessorFamilies project, IEnumerable<BuildModule> modules)
            : base(template | (UInt32)project, Application.ProjectRoot)
        {
            this.modules = modules;
        }

        public IEnumerator<BuildModule> GetEnumerator()
        {
            return modules.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
