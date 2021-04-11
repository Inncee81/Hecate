// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
#if NET_FRAMEWORK
using System.CodeDom.Compiler;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// A pipeline message used to process a CSharp related compilation task
    /// </summary>
    public partial class CompilerCommand : KernelMessage, IEnumerable<FileSystemDescriptor>
    {
        BuildProfile profile;
        SharpModuleSettings config;

        BuildModuleType type;
        /// <summary>
        /// The output assembly type of this compiler request
        /// </summary>
        public BuildModuleType Type
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return type; }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            set { type = value; }
        }

        List<FileSystemDescriptor> sources;
        /// <summary>
        /// A collection of source files assigned to this compiler request
        /// </summary>
        public List<FileSystemDescriptor> Sources
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return sources; }
        }

        /// <summary>
        /// A flag indicating if warnings result in a failure
        /// </summary>
        public bool WarningAsError
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return config.WarningAsError; }
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

            #if NET_FRAMEWORK
            this.temporaryFiles = new TempFileCollection();
            this.parameters = new CompilerParameters();
                 parameters.TreatWarningsAsErrors = config.WarningAsError;
                 parameters.TempFiles = temporaryFiles;
                 parameters.GenerateInMemory = false;
                 parameters.WarningLevel = 4;
            #else
            this.referencedAssemblies = CollectionPool<List<string>, string>.Get();
            this.frameworks = CollectionPool<Dictionary<string, string>, string, string>.Get();
            #endif
        }
        public override void Dispose()
        {
            #if !NET_FRAMEWORK
            CollectionPool<Dictionary<string, string>, string, string>.Return(frameworks);
            CollectionPool<List<string>, string>.Return(referencedAssemblies);
            #endif
            CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Return(sources);
            base.Dispose();
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public IEnumerator<FileSystemDescriptor> GetEnumerator()
        {
            return sources.GetEnumerator();
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}