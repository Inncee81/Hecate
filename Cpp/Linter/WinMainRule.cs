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
    /// Int ('WINAPI' | _stdcall) ('WinMain' | 'wWinMain') RoundBracketOpen;
    /// </summary>
    class WinMainRule : CppParserRule
    {
        /// <summary>
        /// Creates this rule
        /// </summary>
        public WinMainRule()
        { }

        protected override ProductionState Process(CppToken value)
        {
            if (Linter.Scope > 0)
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

                    #region ('WINAPI' | '__stdcall')
                    case 1: switch (value)
                        {
                            case CppToken.Identifier: switch(Linter.Buffer)
                                {
                                    case "__stdcall":
                                    case "WINAPI": 
                                        return ProductionState.Shift;
                                    case "WinMain":
                                    case "wWinMain":
                                        {
                                            State = 3;
                                        }
                                        return ProductionState.Preserve;
                                }
                                break;
                        }
                        goto case 0;
                    #endregion

                    #region ('WinMain' | 'wWinMain')
                    case 2: switch (value)
                        {
                            case CppToken.Identifier: switch(Linter.Buffer)
                                {
                                    case "WinMain":
                                    case "wWinMain":
                                        return ProductionState.Shift;
                                }
                                break;
                        }
                        goto case 0;
                    #endregion

                    #region CurlyBraceOpen
                    case 3: switch (value)
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
                if (Settings.AssemblyType < BuildModuleType.Executable)
                    Settings.AssemblyType = BuildModuleType.Executable;
            }
        }
    }
}
