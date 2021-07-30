// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.Hecate.Build;
using SE.Parsing;
using SE.CppLang;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// Int 'main' RoundBracketOpen;
    /// </summary>
    class MainRule : CppParserRule
    {
        /// <summary>
        /// Creates this rule
        /// </summary>
        public MainRule()
        { }

        protected override ProductionState Process(CppToken value)
        {
            if (Linter.Scope == 0)
            {
                switch (State)
                {
                    #region Int
                    default:
                    case 0: switch (value)
                        {
                            case CppToken.Int:
                                {
                                    OnReset();
                                    State = 1;
                                }
                                return ProductionState.Preserve;
                            default:
                                return ProductionState.Revert;
                        }
                    #endregion

                    #region 'main'
                    case 1: switch (value)
                        {
                            case CppToken.Identifier:
                                {
                                    if (!Linter.Buffer.Equals("main", StringComparison.InvariantCulture))
                                        break;
                                }
                                return ProductionState.Shift;
                        }
                        goto case 0;
                    #endregion

                    #region CurlyBraceOpen
                    case 2: switch (value)
                        {
                            case CppToken.RoundBracketOpen:
                                return ProductionState.Success;
                        }
                        goto case 0;
                    #endregion
                }
            }
            else return ProductionState.Revert;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override void OnReset()
        { }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override void OnCompleted()
        {
            lock (Settings)
            {
                if (Settings.AssemblyType < BuildModuleType.Console)
                    Settings.AssemblyType = BuildModuleType.Console;
            }
        }
    }
}