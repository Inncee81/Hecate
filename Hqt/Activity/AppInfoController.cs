// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SE.Apollo.Package;
using SE.Config;

namespace SE.Hecate
{
    /// <summary>
    /// Pipeline node to request basic usage manual information
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class AppInfoController : ProcessorUnit
    {
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
            get { return (UInt32)ProcessorFamilies.InfoProvider; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public AppInfoController()
        { }

        private static int Process(AppInfo info, Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes<PropertyInfoAttribute>())
            {
                PropertyInfoAttribute[] attribs = type.GetAttributes<PropertyInfoAttribute>();
                foreach(PropertyInfoAttribute attrib in attribs)
                    PropertyMapper.GetPropertyDescriptions(type, BindingFlags.Instance | BindingFlags.Static, info.GetPage(attrib.Category), true, true);
            }
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            AppInfo info = (command as AppInfo);
            if (info != null)
            {
                PropertyPage page = info.GetPage("install command");
                PropertyMapper.GetPropertyDescriptions<PackageManager>(page, true, true);
                PropertyMapper.GetPropertyDescriptions(new Repository(), page, true, true);

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    command.Attach(Task.Run<int>(() => Process(info, assembly)));
                }
                return true;
            }
            else return false;
        }
    }
}
