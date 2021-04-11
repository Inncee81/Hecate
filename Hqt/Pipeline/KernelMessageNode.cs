// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.Reactive;

namespace SE.Hecate
{
    /// <summary>
    /// A base class to be derived by non-ProcessorUnit pipeline nodes
    /// </summary>
    public abstract class KernelMessageNode : IReceiver<KernelMessage, bool>
    {
        public abstract bool OnNext(KernelMessage value);
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public virtual bool OnError(Exception error)
        {
            return true;
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void OnCompleted()
        { }
    }
}
