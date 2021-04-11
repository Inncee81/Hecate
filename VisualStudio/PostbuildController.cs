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

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Pipeline node to perform dispatching of VisualStudio related postbuild actions
    /// </summary>
    [ProcessorUnit(IsExtension = true)]
    public class PostbuildController : ProcessorUnit, IPrioritizedActor
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
            get { return (UInt32)SE.Hecate.ProcessorFamilies.Postbuild; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public PostbuildController()
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
                PostbuildCommand build = new PostbuildCommand
                (
                    command.Template,
                    modules.Select(ProjectSelector)
                           .Where(x => x != null)
                );
                bool exit; if (build.Any() && Kernel.Dispatch(build, out exit))
                {
                    command.Attach(build.Task.ContinueWith<int>((task) =>
                    {
                        build.Release();
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
                    build.Release();
                    CollectionPool<List<object>, object>.Return(modules);
                    return false;
                }
            }
            else CollectionPool<List<object>, object>.Return(modules);
            return false;
        }

        private static VisualStudioProject ProjectSelector(object buildModule)
        {
            BuildModule module = (buildModule as BuildModule);
            {
                VisualSharpProject project; if (module.TryGetProperty<VisualSharpProject>(out project))
                {
                    return project;
                }
            }
            {
                VisualCppProject project; if (module.TryGetProperty<VisualCppProject>(out project))
                {
                    return project;
                }
            }
            return null;
        }
    }
}
