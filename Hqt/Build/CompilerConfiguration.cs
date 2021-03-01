// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
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
            get { return name; }
        }

        [NamedProperty("compiler")]
        string compiler;
        /// <summary>
        /// The compiler command to execute
        /// </summary>
        public string Compiler
        {
            get { return compiler; }
        }

        [NamedProperty("assembler")]
        string assembler;
        /// <summary>
        /// The assembler command to execute
        /// </summary>
        public string Assembler
        {
            get { return assembler; }
        }

        [NamedProperty("linker")]
        string linker;
        /// <summary>
        /// The linker command to execute
        /// </summary>
        public string Linker
        {
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

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            CompilerConfiguration conf = (obj as CompilerConfiguration);
            if (conf != null)
            {
                return name.Equals(conf.name);
            }
            return false;
        }

        public override string ToString()
        {
            return name.ToString();
        }
    }
}
