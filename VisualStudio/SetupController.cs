// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using SE.CommandLine;
using SE.Config;
using SE.Hecate.Build;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Pipeline node to load VisualStudio related settings and set the build profile if possible
    /// </summary>
    [ProcessorUnit(IsExtension = true)]
    public class SetupController : ProcessorUnit, IPrioritizedActor
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
            get { return (UInt32)SE.Hecate.ProcessorFamilies.Setup; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public SetupController()
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
            PropertyMapper.Assign<BuildParameter>(CommandLineOptions.Default, true, true);
            if (BuildParameter.Version != VisualStudioVersion.Invalid)
            {
                if (!string.IsNullOrWhiteSpace(BuildParameter.Profile))
                {
                    if (command.HasProperty<BuildProfile>())
                    {
                        Application.Error(SeverityFlags.None, "Build profile is ambiguous");
                        Kernel.Exit();
                        return false;
                    }
                    else command.SetProperty(new BuildProfile(BuildParameter.Profile));
                }
            }
            return true;
        }
    }
}