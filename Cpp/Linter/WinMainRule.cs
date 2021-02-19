// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Text;
using SE.Hecate.Build;
using SE.Parsing;
using SE.CppLang;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// Int ('WINAPI' | _stdcall) ('WinMain' | 'wWinMain') RoundBracketOpen;
    /// </summary>
    class WinMainRule : ParserRule<CppToken>
    {
        Linter linter;
        /// <summary>
        /// The Linter instance this rule is assigned to
        /// </summary>
        public Linter Linter
        {
            get { return linter; }
            set { linter = value; }
        }

        CppModuleSettings settings;
        /// <summary>
        /// A Cpp configuration instance currently in process
        /// </summary>
        public CppModuleSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        /// <summary>
        /// Creates this rule
        /// </summary>
        public WinMainRule()
        { }
        public override void Dispose()
        {
            linter = null;
            settings = null;
        }

        protected override ProductionState Process(CppToken value)
        {
            if (linter.Scope > 0)
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
                            case CppToken.Identifier: switch(linter.Buffer)
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
                            case CppToken.Identifier: switch(linter.Buffer)
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

        public override void OnReset()
        { }
        public override void OnCompleted()
        {
            lock (settings)
            {
                if (settings.AssemblyType <= BuildModuleType.Console)
                    settings.AssemblyType = BuildModuleType.Executable;
            }
        }
    }
}
