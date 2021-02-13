// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using SE.Flex;
using SE.Hecate.Build;
using SE.Hecate.Sharp;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Pipeline node to perform dispatching of VisualStudio related build actions
    /// </summary>
    [ProcessorUnit(IsExtension = true)]
    public class BuildController : ProcessorUnit, IPrioritizedActor
    {
        int IPrioritizedActor.Priority
        {
            get { return 0; }
        }
        public override PathDescriptor Target
        {
            get { return Application.SdkRoot; }
        }
        public override bool Enabled
        {
            get { return true; }
        }
        public override UInt32 Family
        {
            get { return (UInt32)SE.Hecate.ProcessorFamilies.Build; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public BuildController()
        { }

        public void Attach(PriorityDispatcher owner)
        { }
        public void Detach(PriorityDispatcher owner)
        { }

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
        public bool OnError(Exception error)
        {
            return true;
        }
        public void OnCompleted()
        { }

        public override bool Process(KernelMessage command)
        {
            if (BuildParameter.Version != VisualStudioVersion.Invalid)
            {
                List<object> modules = CollectionPool<List<object>, object>.Get();
                if (PropertyManager.FindProperties(x => x.Value is BuildModule, modules) > 0)
                {
                    BuildCommand build = new BuildCommand
                    (
                        command.Template,
                        modules.Select(x => x as BuildModule)
                                .Where(x => !x.IsPackage && (x.HasProperty<SharpModule>() ||
                                                             x.HasProperty<SharpModule>())) //<-- TODO replace with CppModule type
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
            }
            return false;
        }
    }
}
