// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using SE.Actor;
using SE.Reactive;

namespace SE.Hecate
{
    /// <summary>
    /// Manages conditionally nested message streams associated with a root stream
    /// </summary>
    public class MessageContainer<T> : MessageStreamContainer<KernelMessage> where T : IDispatcher<KernelMessage>, new()
    {
        Dictionary<UInt32, IReactiveStream<KernelMessage, bool>> streams;
        ReadWriteLock streamLock;

        /// <summary>
        /// Creates a new container
        /// </summary>
        public MessageContainer()
        {
            this.streams = new Dictionary<uint, IReactiveStream<KernelMessage, bool>>();
            this.streamLock = new ReadWriteLock();
        }

        public override bool TryGet(object[] parameter, out IReactiveStream<KernelMessage, bool> result)
        {
            if (parameter.Length == 0)
            {
                throw new IndexOutOfRangeException();
            }
            if (!(parameter[0] is UInt32))
            {
                throw new ArgumentOutOfRangeException("parameter");
            }
            UInt32 id = (UInt32)parameter[0];
            streamLock.WriteLock();
            try
            {
                if (!streams.TryGetValue(id, out result))
                {
                    result = new MessageStream<KernelMessage>
                    (
                        new T()
                    );
                    streams.Add(id, result);
                }
                return true;
            }
            finally
            {
                streamLock.WriteRelease();
            }
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override bool TryGet(ref KernelMessage message, out IReactiveStream<KernelMessage, bool> result)
        {
            streamLock.ReadLock();
            try
            {
                return streams.TryGetValue(message.Id, out result);
            }
            finally
            {
                streamLock.ReadRelease();
            }
        }
    }
}