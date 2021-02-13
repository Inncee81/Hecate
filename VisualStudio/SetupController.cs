// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
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
            get { return (UInt32)SE.Hecate.ProcessorFamilies.Setup; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public SetupController()
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