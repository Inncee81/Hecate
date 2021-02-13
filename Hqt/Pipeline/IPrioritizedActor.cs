// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.Actor;

namespace SE.Hecate
{
    /// <summary>
    /// An interface to enable certain pipeline nodes to be scheduled based on their priority
    /// </summary>
    public interface IPrioritizedActor : IActor<KernelMessage>
    {
        /// <summary>
        /// An integer indicating this nodes current priority (the lower the better)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// A callback executed when this node is attached to a dispatcher instance
        /// </summary>
        /// <param name="owner"></param>
        void Attach(PriorityDispatcher owner);

        /// <summary>
        /// A callback executed when this node was removed from a dispatcher instance
        /// </summary>
        /// <param name="owner"></param>
        void Detach(PriorityDispatcher owner);
    }
}
