// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Threading;
using SE.Actor;
using SE.Reactive;

namespace SE.Hecate
{
    /// <summary>
    /// The primary execution authority which maintains the processing pipeline
    /// </summary>
    public static partial class Kernel
    {
        private readonly static Type ProcessorUnitType = typeof(ProcessorUnit);

        private readonly static MessageStream<KernelMessage>[] commandStreams;
        private readonly static Dictionary<UInt32, ProcessorFamily> families;
        private static ReadWriteLock processorLock;
        private static FiberLocal<bool> exitState;

        static Kernel()
        {
            families = new Dictionary<UInt32, ProcessorFamily>();
            processorLock = new ReadWriteLock();
            commandStreams = new MessageStream<KernelMessage>[]
            {
                new MessageStream<KernelMessage>
                (
                    new BatchDispatcher<KernelMessage>(),
                    new MessageContainer<BatchDispatcher<KernelMessage>>()
                ),
                new MessageStream<KernelMessage>
                (
                    new PriorityDispatcher(),
                    new MessageContainer<PriorityDispatcher>()
                ),
                new MessageStream<KernelMessage>
                (
                    new BatchDispatcher<KernelMessage>(),
                    new MessageContainer<BatchDispatcher<KernelMessage>>()
                )
            };
            exitState = new FiberLocal<bool>();
        }

        /// <summary>
        /// Loads the provided ProcessorUnit instance into the pipeline
        /// </summary>
        /// <param name="core">An instance of a ProcessorUnit attached to the pipeline</param>
        /// <param name="options">Additional configuration settings changing the pipeline related behavior</param>
        public static void Load(ProcessorUnit core, ProcessorUnitAttribute options)
        {
            if (options != null && options.IsExtension)
            {
                IPrioritizedActor actor = (core as IPrioritizedActor);
                if (actor != null)
                {
                    commandStreams[1].Where(core.Family)
                                     .Subscribe(actor);
                }
            }
            else
            {
                ProcessorFamily family;
                processorLock.ReadLock();
                try
                {
                    families.TryGetValue(core.Family, out family);
                }
                finally
                {
                    processorLock.ReadRelease();
                }
                if (family == null)
                {
                    processorLock.WriteLock();
                    try
                    {
                        if (!families.TryGetValue(core.Family, out family))
                        {
                            family = new ProcessorFamily();
                            families.Add(core.Family, family);
                            commandStreams[1].Where(core.Family)
                                             .Subscribe(family);
                        }
                    }
                    finally
                    {
                        processorLock.WriteRelease();
                    }
                }
                family.Add(core);
            }
        }
        /// <summary>
        /// Automatically locates and loads ProcessorUnits available in the assembly
        /// </summary>
        public static void Load()
        {
            AppDomain.CurrentDomain.GetAssemblies()
                     .ForEach(Load);
        }
        private static void Load(Assembly assembly)
        {
            assembly.GetTypes()
                    .ForEach(Load);
        }
        private static void Load(Type type)
        {
            if (!type.IsAbstract && !type.IsInterface)
            {
                if (ProcessorUnitType.IsAssignableFrom(type))
                {
                    try
                    {
                        Load(type.CreateInstance<ProcessorUnit>(), type.GetAttribute<ProcessorUnitAttribute>());
                    }
                    catch (Exception er)
                    {
                        Application.Error(er);
                    }
                }
                else if (type.HasAttribute<InitializeOnLoadAttribute>())
                    type.Initialize();
            }
        }

        /// <summary>
        /// Attaches an input related processing node to the pipeline graph
        /// </summary>
        /// <param name="command">The command ID to which the node should be attached</param>
        /// <param name="node">A processing instance</param>
        /// <returns>A disposable object used to remove the node somewhere in the future</returns>
        public static IDisposable AddPreprocessinggNode(UInt32 command, KernelMessageNode node)
        {
            return commandStreams[0].Where(command)
                                    .Subscribe(node);
        }

        /// <summary>
        /// Attaches an output related processing node to the pipeline graph
        /// </summary>
        /// <param name="command">The command ID to which the node should be attached</param>
        /// <param name="node">A processing instance</param>
        /// <returns>A disposable object used to remove the node somewhere in the future</returns>
        public static IDisposable AddPostprocessingNode(UInt32 command, KernelMessageNode node)
        {
            return commandStreams[2].Where(command)
                                    .Subscribe(node);
        }

        /// <summary>
        /// Dispatches the provided message to all nodes willing to process it
        /// </summary>
        /// <param name="command">The message to be dispatched</param>
        /// <param name="shouldExit">A flag set if a critical failure occurred and processing should be canceled</param>
        /// <returns>True if at least one node accepted the message, false otherwise</returns>
        public static bool Dispatch(KernelMessage command, out bool shouldExit)
        {
            shouldExit = false;
            exitState.Value = false;
            bool result = false; foreach(MessageStream<KernelMessage> commandStream in commandStreams)
            {
                IReactiveStream<KernelMessage, bool> stream; if (!commandStream.NestedStreams.TryGet(ref command, out stream))
                {
                    stream = commandStream;
                }
                if (!(stream as MessageStream<KernelMessage>).Dispatch(command))
                {
                    exitState.TryGet(out shouldExit);
                    if (!shouldExit)
                    {
                        result |= false;
                    }
                    else return false;
                }
                else result |= true;
            }
            return result;
        }

        /// <summary>
        /// Removes the provided ProcessorUnit if it belongs to a processor family
        /// </summary>
        public static void Release(ProcessorUnit core)
        {
            processorLock.WriteLock();
            try
            {
                ProcessorFamily family; if (families.TryGetValue(core.Family, out family))
                {
                    family.Remove(core);
                }
            }
            finally
            {
                processorLock.WriteRelease();
            }
        }

        /// <summary>
        /// Indicates a critical failure in the pipeline which sets it into cancellation state
        /// </summary>
        public static void Exit()
        {
            Application.Error(SeverityFlags.Minimal, "Processing canceled");
            exitState.Value = true;
        }
    }
}