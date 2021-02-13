// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;
using SE.Alchemy;
using SE.Apollo.Package;
using SE.CommandLine;
using SE.Config;
using SE.Parsing;

namespace SE.Hecate
{
    /// <summary>
    /// Pipeline node usually to be called before any other task can run
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class SetupController : ProcessorUnit
    {
        public const string SettingsFileName = "Packages.*";
        public const string GlobalDefine = "HQT_GLOBAL";

        private static SetupController @default;
        /// <summary>
        /// A pointer to the default controller instance
        /// </summary>
        public static SetupController Default
        {
            get { return @default; }
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
            get { return (UInt32)ProcessorFamilies.Setup; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public SetupController()
        {
            if (@default == null)
                @default = this;
        }

        public override bool Process(KernelMessage command)
        {
            command.Attach(Taskʾ.Run<int>(() => LoadPackageManager()));
            return true;
        }
        
        /// <summary>
        /// A utility function to load and initialize the internal package manager
        /// </summary>
        public static int LoadPackageManager()
        {
            PropertyMapper.Assign<PackageManager>(CommandLineOptions.Default, true, true);
            FileDescriptor configFile; if (Application.ConfigDirectory.FindFile(SettingsFileName, out configFile, PathSeekOptions.RootLevel))
            {
                IPropertyProvider settings; if (configFile.GetProperties(Application.LogSystem, out settings))
                {
                    PropertyMapper.Assign<PackageManager>(settings, true, true);
                }
                else return Application.FailureReturnCode;
            }
            if (Application.ConfigDirectory != Application.SdkConfig && Application.SdkConfig.FindFile(SettingsFileName, out configFile, PathSeekOptions.RootLevel))
            {
                IPropertyProvider settings; if (configFile.GetProperties(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(GlobalDefine, "1") }, Application.LogSystem, out settings))
                {
                    PropertyMapper.Assign<PackageManager>(settings, true, true);
                }
                else return Application.FailureReturnCode;
            }
            return Application.SuccessReturnCode;
        }
    }
}
