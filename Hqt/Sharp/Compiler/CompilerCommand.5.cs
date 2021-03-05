// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

#if !NET_FRAMEWORK
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    public partial class CompilerCommand
    {
        FileDescriptor outputAssembly;
        /// <summary>
        /// The deployment location of the compilation result
        /// </summary>
        public FileDescriptor TargetFile
        {
            get { return outputAssembly; }
            set { outputAssembly = value; }
        }

        List<string> referencedAssemblies;
        /// <summary>
        /// A collection of assembly references assigned to this compiler request
        /// </summary>
        public List<string> References
        {
            get { return referencedAssemblies; }
        }

        Dictionary<string, string> frameworks;
        /// <summary>
        /// A collection of references to .Net 5 frameworks
        /// </summary>
        public Dictionary<string, string> Frameworks
        {
            get { return frameworks; }
        }

        /// <summary>
        /// Finally processes the provided settings into a compiler related import file
        /// </summary>
        public void Finalize(StreamWriter stream)
        {
            stream.WriteLine("-nologo");
            stream.WriteLine("-parallel");

            switch (type)
            {
                case BuildModuleType.Console: stream.WriteLine("-target:exe"); break;
                case BuildModuleType.Executable: stream.WriteLine("-target:winexe"); break;
                default: stream.WriteLine("-target:library"); break;
            }
            switch (profile.Target)
            {
                case PlatformTarget.Any: stream.WriteLine("-platform:anycpu"); break;
                case PlatformTarget.Arm: stream.WriteLine("-platform:ARM"); break;
                case PlatformTarget.x86: stream.WriteLine("-platform:x86"); break;
                case PlatformTarget.Arm64: stream.WriteLine("-platform:ARM64"); break;
                case PlatformTarget.x64: 
                default: stream.WriteLine("-platform:x64"); break;
            }

            stream.Write("-define:__hqt__");

            #if net50
            stream.Write(";net50");
            stream.Write(";NET50");
            stream.Write(";NET_5_0");
            stream.Write(";NET_CORE");
            #endif

            foreach (string define in config.Defines)
            {
                stream.Write(";");
                stream.Write(define);
            }
            stream.WriteLine();
            if (config.Optimize)
            {
                if (config.DebugSymbols)
                {
                    stream.WriteLine("-debug:pdbonly");
                }
                stream.WriteLine("-optimize");
            }
            else stream.WriteLine("-debug");
            if (WarningAsError)
            {
                stream.WriteLine("-warnaserror");
            }
            stream.WriteLine("-warn:4");
            stream.WriteLine("-langversion:latest");
            stream.WriteLine("-out:\"{0}\"", outputAssembly.GetRelativePath(Application.WorkerPath));

            List<FileSystemDescriptor> coreAssemblies = CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Get();
            try
            {
                if (BuildParameter.Dotnet.FindFiles("Microsoft.NETCore.App/5.*/System.Private.*.dll", coreAssemblies) > 0)
                {
                    foreach (FileSystemDescriptor coreAssembly in coreAssemblies)
                        stream.WriteLine("-reference:\"{0}\"", coreAssembly.GetAbsolutePath());
                }
            }
            finally
            {
                CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Return(coreAssemblies);
            }
            string dotnetPath = BuildParameter.Dotnet.GetAbsolutePath();
            foreach (string assembly in referencedAssemblies)
            {
                if (assembly.StartsWith(dotnetPath, StringComparison.Ordinal))
                {
                    string framework = assembly.Substring(dotnetPath.Length).Trim(System.IO.Path.DirectorySeparatorChar);
                    string version = framework.Substring(framework.IndexOf(System.IO.Path.DirectorySeparatorChar) + 1);

                    framework = framework.Substring(0, framework.IndexOf(System.IO.Path.DirectorySeparatorChar));
                    if (!frameworks.ContainsKey(framework))
                    {
                        version = version.Substring(0, version.IndexOf(System.IO.Path.DirectorySeparatorChar));
                        frameworks.Add(framework, version);
                    }
                }
                stream.WriteLine("-reference:\"{0}\"", assembly);
            }
            foreach (FileDescriptor source in sources)
            {
                stream.WriteLine("\"{0}\"", source.GetRelativePath(Application.WorkerPath));
            }
        }
    }
}
#endif