// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;
using SE.Flex;

namespace SE.Hecate
{
    /// <summary>
    /// A basic pipeline message used to communicate to the nodes
    /// </summary>
    public class KernelMessage : FlexObject
    {
        /// <summary>
        /// The processor family ID this message relates to
        /// </summary>
        public UInt32 Id
        {
            get { return Template.ComponentId; }
        }

        PathDescriptor path;
        /// <summary>
        /// An absolute file system path this message is based on
        /// </summary>
        public PathDescriptor Path
        {
            get { return path; }
        }

        List<Task<int>> tasks;
        /// <summary>
        /// If successfully dispatched, contains a collection of pending actions 
        /// performed in the background
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Task<int> Task
        {
            get 
            {
                return Taskʾ.WhenAll<int>(tasks)
                            .ContinueWith<int>(Validate);
            }
        }

        /// <summary>
        /// Creates a new pipeline message instance
        /// </summary>
        public KernelMessage(UInt32 id, PathDescriptor path)
            : base(TemplateId.Create() | id)
        {
            this.path = path;
            this.tasks = CollectionPool<List<Task<int>>, Task<int>>.Get();
        }
        /// <summary>
        /// Creates a new pipeline message instance
        /// </summary>
        public KernelMessage(TemplateId template, PathDescriptor path)
            : base(template)
        {
            this.path = path;
            this.tasks = CollectionPool<List<Task<int>>, Task<int>>.Get();
        }
        public override void Dispose()
        {
            Release();
            base.Dispose();
        }

        /// <summary>
        /// Attaches an operation to this message in order to await it's
        /// completion by the original sender
        /// </summary>
        /// <param name="operation">An asynchronous operation</param>
        public void Attach(Task<int> operation)
        {
            tasks.Add(operation);
        }

        /// <summary>
        /// Awaits completion of the provided message synchronously
        /// </summary>
        /// <returns>True if the message was processed properly, false otherwise</returns>
        public int Await()
        {
            int result; if (Task.TryGetResult(out result))
            {
                return result;
            }
            else return Application.FailureReturnCode;
        }

        /// <summary>
        /// Releases allocated resources before disposing
        /// </summary>
        public void Release()
        {
            if (tasks != null)
            {
                CollectionPool<List<Task<int>>, Task<int>>.Return(tasks);
                tasks = null;
            }
        }

        /// <summary>
        /// Shrinks a list of pending actions into a single action
        /// </summary>
        /// <returns>The overall processing result or zero</returns>
        public static int Validate(Task<int[]> result)
        {
            switch (result.Status)
            {
                case TaskStatus.RanToCompletion:
                    {
                        foreach (int processState in result.Result)
                        {
                            if (processState != Application.SuccessReturnCode)
                                return processState;
                        }
                        return Application.SuccessReturnCode;
                    }
                case TaskStatus.Faulted:
                    {
                        foreach (Exception e in result.Exception.InnerExceptions)
                            Application.Error(e);
                    }
                    return Application.FailureReturnCode;
                case TaskStatus.Canceled:
                default: return Application.FailureReturnCode;
            }
        }

        /// <summary>
        /// Creates a new message instance from given parameters
        /// </summary>
        /// <param name="family">The processor family to target</param>
        /// <param name="path">A current worker path to use</param>
        public static KernelMessage Create(ProcessorFamilies family, PathDescriptor path)
        {
            return new KernelMessage((UInt32)family, path);
        }
        /// <summary>
        /// Creates a related message instance from given parameters
        /// </summary>
        /// <param name="root">The root message the instance should base on</param>
        /// <param name="family">The processor family to target</param>
        /// <param name="path">A current worker path to use</param>
        public static KernelMessage Create(KernelMessage root, ProcessorFamilies family, PathDescriptor path)
        {
            return new KernelMessage(root.Template | (UInt32)family, path);
        }
    }
}
