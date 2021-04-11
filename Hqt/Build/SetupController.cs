// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
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
        public const string GlobalDefine = "SDK_GLOBAL";

        private static SetupController @default;
        /// <summary>
        /// A pointer to the default controller instance
        /// </summary>
        public static SetupController Default
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return @default; }
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

        [MethodImpl(OptimizationExtensions.ForceInline)]
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

        /// <summary>
        /// A utility function to load and initialize a settings object from a config file
        /// </summary>
        /// <param name="name">Name of the config file to locate in /Config</param>
        /// <param name="instance">A class instance to initialize</param>
        /// <returns>True if loading was successful, false otherwise</returns>
        public static bool LoadSettings(string name, object instance, params string[] flags)
        {
            bool result = true;
            if (string.IsNullOrWhiteSpace(Path.GetExtension(name)))
            {
                name = string.Concat(name, ".*");
            }
            Dictionary<string, string> defines = CollectionPool<Dictionary<string, string>, string, string>.Get();
            foreach (string flag in flags)
            {
                defines.Add(flag, "1");
            }
            try
            {
                FileDescriptor configFile; if (Application.ConfigDirectory.FindFile(name, out configFile, PathSeekOptions.RootLevel))
                {
                    IPropertyProvider settings; if (configFile.GetProperties(defines, Application.LogSystem, out settings))
                    {
                        PropertyMapper.Assign(instance, settings, true, true);
                    }
                    else return false;
                }
                else if (Application.ConfigDirectory != Application.SdkConfig && Application.SdkConfig.FindFile(name, out configFile, PathSeekOptions.RootLevel))
                {
                    defines.Add(GlobalDefine, "1");
                    IPropertyProvider settings; if (configFile.GetProperties(defines, Application.LogSystem, out settings))
                    {
                        PropertyMapper.Assign(instance, settings, true, true);
                    }
                    else return false;
                }
                else result = false;
            }
            catch
            {
                CollectionPool<Dictionary<string, string>, string, string>.Return(defines);
            }
            PropertyMapper.Assign(instance, CommandLineOptions.Default, true, true);
            return result;
        }
        /// <summary>
        /// A utility function to load and initialize a settings object from a config file
        /// </summary>
        /// <param name="name">Name of the config file to locate in /Config</param>
        /// <returns>True if loading was successful, false otherwise</returns>
        public static bool LoadSettings<T>(string name, params string[] flags)
        {
            bool result = true;
            if (string.IsNullOrWhiteSpace(Path.GetExtension(name)))
            {
                name = string.Concat(name, ".*");
            }
            Dictionary<string, string> defines = CollectionPool<Dictionary<string, string>, string, string>.Get();
            foreach (string flag in flags)
            {
                defines.Add(flag, "1");
            }
            try
            {
                FileDescriptor configFile; if (Application.ConfigDirectory.FindFile(name, out configFile, PathSeekOptions.RootLevel))
                {
                    IPropertyProvider settings; if (configFile.GetProperties(defines, Application.LogSystem, out settings))
                    {
                        PropertyMapper.Assign<T>(settings, true, true);
                    }
                    else return false;
                }
                else if (Application.ConfigDirectory != Application.SdkConfig && Application.SdkConfig.FindFile(name, out configFile, PathSeekOptions.RootLevel))
                {
                    defines.Add(GlobalDefine, "1");
                    IPropertyProvider settings; if (configFile.GetProperties(defines, Application.LogSystem, out settings))
                    {
                        PropertyMapper.Assign<T>(settings, true, true);
                    }
                    else return false;
                }
                else result = false;
            }
            catch
            {
                CollectionPool<Dictionary<string, string>, string, string>.Return(defines);
            }
            PropertyMapper.Assign<T>(CommandLineOptions.Default, true, true);
            return result;
        }
    }
}
