// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

#if NET_FRAMEWORK
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    public partial class CompilerCommand
    {
        CompilerParameters parameters;
        /// <summary>
        /// Additional parameters assigned to this compiler request
        /// </summary>
        public CompilerParameters Parameters
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return parameters; }
        }

        TempFileCollection temporaryFiles;
        /// <summary>
        /// A collection of files that the compiler has created during the compilation process
        /// </summary>
        public TempFileCollection TemporaryFiles
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return temporaryFiles; }
        }

        /// <summary>
        /// The deployment location of the compilation result
        /// </summary>
        public FileDescriptor TargetFile
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get 
            {
                return new FileDescriptor
                (
                    new PathDescriptor(System.IO.Path.GetDirectoryName(parameters.OutputAssembly)), 
                    System.IO.Path.GetFileName(parameters.OutputAssembly)
                );
            }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            set { parameters.OutputAssembly = value.GetAbsolutePath(); }
        }

        /// <summary>
        /// A collection of assembly references assigned to this compiler request
        /// </summary>
        public IList References
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return parameters.ReferencedAssemblies; }
        }

        /// <summary>
        /// Finally processes the provided settings into a compiler related data type
        /// </summary>
        public CompilerParameters Finalize()
        {
            switch (profile.Target)
            {
                case PlatformTarget.Any: parameters.CompilerOptions += " /platform:anycpu"; break;
                case PlatformTarget.Arm: parameters.CompilerOptions += " /platform:ARM"; break;
                case PlatformTarget.x86: parameters.CompilerOptions += " /platform:x86"; break;
                case PlatformTarget.Arm64: parameters.CompilerOptions += " /platform:ARM64"; break;
                case PlatformTarget.x64: 
                default: parameters.CompilerOptions += " /platform:x64"; break;
            }

            parameters.CompilerOptions += " /define:__hqt__";

            #if net40
            parameters.CompilerOptions += ";net40";
            parameters.CompilerOptions += ";NET40";
            parameters.CompilerOptions += ";NET_4_0";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #elif net45
            parameters.CompilerOptions += ";net45";
            parameters.CompilerOptions += ";NET45";
            parameters.CompilerOptions += ";NET_4_5";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #elif net451
            parameters.CompilerOptions += ";net451";
            parameters.CompilerOptions += ";NET451";
            parameters.CompilerOptions += ";NET_4_5_1";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #elif net452
            parameters.CompilerOptions += ";net452";
            parameters.CompilerOptions += ";NET452";
            parameters.CompilerOptions += ";NET_4_5_2";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #elif net46
            parameters.CompilerOptions += ";net46";
            parameters.CompilerOptions += ";NET46";
            parameters.CompilerOptions += ";NET_4_6";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #elif net461
            parameters.CompilerOptions += ";net461";
            parameters.CompilerOptions += ";NET461";
            parameters.CompilerOptions += ";NET_4_6_1";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #elif net462
            parameters.CompilerOptions += ";net462";
            parameters.CompilerOptions += ";NET462";
            parameters.CompilerOptions += ";NET_4_6_2";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #elif net47
            parameters.CompilerOptions += ";net47";
            parameters.CompilerOptions += ";NET47";
            parameters.CompilerOptions += ";NET_4_7";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #elif net471
            parameters.CompilerOptions += ";net471";
            parameters.CompilerOptions += ";NET471";
            parameters.CompilerOptions += ";NET_4_7_1";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #elif net472
            parameters.CompilerOptions += ";net472";
            parameters.CompilerOptions += ";NET472";
            parameters.CompilerOptions += ";NET_4_7_2";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #elif net48
            parameters.CompilerOptions += ";net48";
            parameters.CompilerOptions += ";NET48";
            parameters.CompilerOptions += ";NET_4_8";
            parameters.CompilerOptions += ";NET_FRAMEWORK";
            #endif

            foreach (string define in config.Defines)
            {
                parameters.CompilerOptions += ";";
                parameters.CompilerOptions += define;
            }

            parameters.IncludeDebugInformation = config.DebugSymbols;
            if (config.Optimize)
            {
                parameters.CompilerOptions += " /optimize";
            }
            if (type >= BuildModuleType.Console)
            {
                parameters.GenerateExecutable = true;
                if (type == BuildModuleType.Executable)
                {
                    parameters.CompilerOptions += " /target:winexe";
                }
            }
            return parameters;
        }
    }
}
#endif