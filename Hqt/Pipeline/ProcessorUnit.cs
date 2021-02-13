// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;

namespace SE.Hecate
{
    /// <summary>
    /// A base processing core node used to build up the pipeline graph
    /// </summary>
    public abstract class ProcessorUnit
    {
        ProcessorPriority priority;
        /// <summary>
        /// This nodes priority in it's family
        /// </summary>
        public ProcessorPriority Priority
        {
            get { return priority; }
        }

        /// <summary>
        /// Determines the file system location this node is bound to
        /// </summary>
        public abstract PathDescriptor Target
        {
            get;
        }
        /// <summary>
        /// Determines if this node is enabled and ready for processing
        /// </summary>
        public abstract bool Enabled
        {
            get;
        }
        /// <summary>
        /// The family ID this node is grouped by
        /// </summary>
        public abstract UInt32 Family
        {
            get;
        }

        /// <summary>
        /// Creates a new PU instance
        /// </summary>
        public ProcessorUnit()
        {
            this.priority = ProcessorPriority.Default;
        }

        /// <summary>
        /// Starts operating on the provided message
        /// </summary>
        /// <returns>True if the message was processed or has pending tasks running 
        /// in the background, false otherwise</returns>
        public abstract bool Process(KernelMessage command);
    }
}