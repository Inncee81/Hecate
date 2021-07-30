// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.CppLang;
using SE.Parsing;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// A generic Cpp related ParserRule
    /// </summary>
    public abstract class CppParserRule : ParserRule<CppToken>
    {
        Linter linter;
        /// <summary>
        /// The Linter instance this rule is assigned to
        /// </summary>
        public Linter Linter
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return linter; }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            set { linter = value; }
        }

        CppModuleSettings settings;
        /// <summary>
        /// A Cpp configuration instance currently in process
        /// </summary>
        public CppModuleSettings Settings
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return settings; }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            set { settings = value; }
        }

        public CppParserRule()
        { }
        public override void Dispose()
        {
            linter = null;
            settings = null;
        }
    }
}
