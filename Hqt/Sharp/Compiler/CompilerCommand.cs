// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// A pipeline message used to process a CSharp related compilation task
    /// </summary>
    public class CompilerCommand : KernelMessage, IEnumerable<FileSystemDescriptor>
    {
        BuildProfile profile;
        SharpModuleSettings config;

        BuildModuleType type;
        /// <summary>
        /// The output assembly type of this compiler request
        /// </summary>
        public BuildModuleType Type
        {
            get { return type; }
            set { type = value; }
        }

        CompilerParameters parameters;
        /// <summary>
        /// Additional parameters assigned to this compiler request
        /// </summary>
        public CompilerParameters Parameters
        {
            get { return parameters; }
        }

        TempFileCollection temporaryFiles;
        /// <summary>
        /// A collection of files that the compiler has created during the compilation process
        /// </summary>
        public TempFileCollection TemporaryFiles
        {
            get { return temporaryFiles; }
        }

        /// <summary>
        /// The deployment location of the compilation result
        /// </summary>
        public FileDescriptor TargetFile
        {
            get 
            {
                return new FileDescriptor
                (
                    new PathDescriptor(System.IO.Path.GetDirectoryName(parameters.OutputAssembly)), 
                    System.IO.Path.GetFileName(parameters.OutputAssembly)
                );
            }
            set { parameters.OutputAssembly = value.GetAbsolutePath(); }
        }

        /// <summary>
        /// A collection of assembly references assigned to this compiler request
        /// </summary>
        public IList References
        {
            get { return parameters.ReferencedAssemblies; }
        }

        List<FileSystemDescriptor> sources;
        /// <summary>
        /// A collection of source files assigned to this compiler request
        /// </summary>
        public List<FileSystemDescriptor> Sources
        {
            get { return sources; }
        }

        /// <summary>
        /// Additional embedded resources that should be implemented in the result assembly
        /// </summary>
        public IList Resources
        {
            get { return parameters.EmbeddedResources; }
        }

        /// <summary>
        /// A flag indicating if warnings result in a failure
        /// </summary>
        public bool WarningAsError
        {
            get { return parameters.TreatWarningsAsErrors; }
        }

        /// <summary>
        /// Creates a new message instance from the provided settings
        /// </summary>
        public CompilerCommand(TemplateId template, BuildProfile profile, SharpModuleSettings config, PathDescriptor path)
            : base(template | (UInt32)ProcessorFamilies.SharpCompile, path)
        {
            this.profile = profile;
            this.config = config;

            this.type = config.AssemblyType;
            this.sources = CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Get();
            this.temporaryFiles = new TempFileCollection();
            this.parameters = new CompilerParameters();
                 parameters.TreatWarningsAsErrors = config.WarningAsError;
                 parameters.TempFiles = temporaryFiles;
                 parameters.GenerateInMemory = false;
                 parameters.WarningLevel = 4;
        }
        public override void Dispose()
        {
            CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Return(sources);
            base.Dispose();
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
            #elif net50
            parameters.CompilerOptions += ";net50";
            parameters.CompilerOptions += ";NET50";
            parameters.CompilerOptions += ";NET_5_0";
            parameters.CompilerOptions += ";NET_CORE";
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

        public IEnumerator<FileSystemDescriptor> GetEnumerator()
        {
            return sources.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}