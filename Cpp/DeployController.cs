// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// Pipeline node to perform dispatching of Cpp related deployment actions
    /// </summary>
    [ProcessorUnit(IsExtension = true)]
    public class DeployController : ProcessorUnit, IPrioritizedActor
    {
        int IPrioritizedActor.Priority
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return 0; }
        }
        public override PathDescriptor Target
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return Application.SdkRoot; }
        }
        public override bool Enabled
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return true; }
        }
        public override UInt32 Family
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return (UInt32)SE.Hecate.ProcessorFamilies.Deployment; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public DeployController()
        { }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void Attach(PriorityDispatcher owner)
        { }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void Detach(PriorityDispatcher owner)
        { }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public bool OnNext(KernelMessage value)
        {
            try
            {
                return Process(value);
            }
            catch (Exception er)
            {
                Application.Error(er);
                return false;
            }
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public bool OnError(Exception error)
        {
            return true;
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void OnCompleted()
        { }

        public override bool Process(KernelMessage command)
        {
            List<object> modules = CollectionPool<List<object>, object>.Get();
            if (PropertyManager.FindProperties(x => x.Value is BuildModule, modules) > 0)
            {
                DeployCommand deploy = new DeployCommand
                (
                    command.Template,
                    modules.Select(x => x as BuildModule)
                           .Where(x => !x.IsPackage && x.HasProperty<CppModule>())
                );
                bool exit; if (deploy.Any() && Kernel.Dispatch(deploy, out exit))
                {
                    command.Attach(deploy.Task.ContinueWith<int>((task) =>
                    {
                        deploy.Release();
                        CollectionPool<List<object>, object>.Return(modules);

                        switch (task.Status)
                        {
                            case TaskStatus.RanToCompletion: return task.Result;
                            case TaskStatus.Faulted:
                                {
                                    Application.Error(task.Exception.InnerException);
                                }
                                goto default;
                            default: return Application.FailureReturnCode;
                        }

                    }));
                    return true;
                }
                else
                {
                    deploy.Release();
                    CollectionPool<List<object>, object>.Return(modules);
                    return false;
                }
            }
            else CollectionPool<List<object>, object>.Return(modules);
            return false;
        }
    }
}
