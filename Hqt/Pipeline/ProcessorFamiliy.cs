// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Threading;

namespace SE.Hecate
{
    /// <summary>
    /// A pipeline processing node container of ProcessorUnits that belong ot the same
    /// family ID
    /// </summary>
    internal class ProcessorFamily : IPrioritizedActor
    {
        List<ProcessorUnit> cores;
        ReadWriteLock coreLock;

        /// <summary>
        /// The amount of ProcessorUnit currently attached to this container
        /// </summary>
        public int Count
        {
            get
            {
                coreLock.ReadLock();
                try
                {
                    if (cores != null)
                    {
                        return cores.Count;
                    }
                    else return 0;
                }
                finally
                {
                    coreLock.ReadRelease();
                }
            }
        }
        public int Priority
        {
            get { return 0; }
        }

        /// <summary>
        /// Creates a new container instance
        /// </summary>
        public ProcessorFamily()
        {
            this.coreLock = new ReadWriteLock();
        }

        /// <summary>
        /// Adds the provided ProcessorUnit instance to this container
        /// </summary>
        public void Add(ProcessorUnit core)
        {
            coreLock.WriteLock();
            try
            {
                if (cores == null)
                {
                    cores = CollectionPool<List<ProcessorUnit>, ProcessorUnit>.Get();
                }
                cores.Add(core);
                cores.Sort((c1, c2) =>
                {
                    return c1.Priority.CompareTo(c2.Priority);
                });
            }
            finally
            {
                coreLock.WriteRelease();
            }
        }

        public bool OnNext(KernelMessage value)
        {
            ProcessorUnit core; if (TryGetCore(value.Path, out core))
            {
                return (value.Id == core.Family && core.Process(value));
            }
            else return false;
        }
        public bool OnError(Exception error)
        {
            Application.Error(error);
            return false;
        }
        public void OnCompleted()
        { }

        /// <summary>
        /// Tries to select the most recent ProcessorUnit that matches to the provided message
        /// location
        /// </summary>
        /// <param name="location">A file system location the message is based on</param>
        /// <param name="core">The computed ProcessorUnit requested to process the message</param>
        /// <returns>True if a matching ProcessorUnit was found, false otherwise</returns>
        public bool TryGetCore(PathDescriptor location, out ProcessorUnit core)
        {
            core = null;
            coreLock.ReadLock();
            try
            {
                if (cores != null && cores.Count > 0)
                {
                    for (int i = cores.Count - 1; i > 0; i--)
                        if (!cores[i].Priority.IsLocalOverride || (cores[i].Target.Order <= location.Order && cores[i].Target.Contains(location)))
                        {
                            if (cores[i].Enabled)
                            {
                                core = cores[i];
                                return true;
                            }
                        }
                    if (!cores[0].Priority.IsLocalOverride && cores[0].Enabled)
                        core = cores[0];
                }
                return (core != null);
            }
            finally
            {
                coreLock.ReadRelease();
            }
        }

        /// <summary>
        /// Adds the provided ProcessorUnit instance from this container
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public bool Remove(ProcessorUnit core)
        {
            coreLock.WriteLock();
            try
            {
                if (cores != null && cores.Remove(core))
                {
                    cores.Sort((c1, c2) =>
                    {
                        return c1.Priority.CompareTo(c2.Priority);
                    });
                    return true;
                }
                else return false;
            }
            finally
            {
                coreLock.WriteRelease();
            }
        }

        public void Attach(PriorityDispatcher owner)
        { }
        public void Detach(PriorityDispatcher owner)
        { }
    }
}
