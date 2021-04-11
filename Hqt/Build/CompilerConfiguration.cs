// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.Config;

namespace SE.Hecate.Build
{
    /// <summary>
    /// Stores a set of options related to a compile command
    /// </summary>
    public class CompilerConfiguration
    {
        readonly string name;
        /// <summary>
        /// A name provided to this configuration
        /// </summary>
        public string Name
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return name; }
        }

        [NamedProperty("compiler")]
        string compiler;
        /// <summary>
        /// The compiler command to execute
        /// </summary>
        public string Compiler
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return compiler; }
        }

        [NamedProperty("assembler")]
        string assembler;
        /// <summary>
        /// The assembler command to execute
        /// </summary>
        public string Assembler
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return assembler; }
        }

        [NamedProperty("linker")]
        string linker;
        /// <summary>
        /// The linker command to execute
        /// </summary>
        public string Linker
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return linker; }
        }

        /// <summary>
        /// Creates a new configuration of the provided name
        /// </summary>
        public CompilerConfiguration(string name)
        {
            this.name = name;
            this.compiler = string.Empty;
            this.assembler = string.Empty;
            this.linker = string.Empty;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override bool Equals(object obj)
        {
            CompilerConfiguration conf = (obj as CompilerConfiguration);
            if (conf != null)
            {
                return name.Equals(conf.name);
            }
            return false;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override string ToString()
        {
            return name.ToString();
        }
    }
}
