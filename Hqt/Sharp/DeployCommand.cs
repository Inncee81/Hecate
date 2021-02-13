// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// A pipeline message used to process a CSharp related deployment task
    /// </summary>
    public class DeployCommand : KernelMessage, IEnumerable<BuildModule>
    {
        IEnumerable<BuildModule> modules;

        /// <summary>
        /// Creates a new message instance from the provided set of modules
        /// </summary>
        public DeployCommand(TemplateId template, IEnumerable<BuildModule> modules)
            : base(template | (UInt32)ProcessorFamilies.SharpPublish, Application.ProjectRoot)
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
