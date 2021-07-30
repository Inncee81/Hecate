// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SE.Hecate.Build;
using SE.Parsing;
using SE.SharpLang;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Static (Private | Protected | Internal | Public)* 'Main' RoundBracketOpen;
    /// </summary>
    class MainRule : SharpParserRule
    {
        int scope;

        /// <summary>
        /// Creates this rule
        /// </summary>
        public MainRule()
        { }

        protected override ProductionState Process(SharpToken value)
        {
            switch (State)
            {
                
                default:
                case 0: switch (value)
                    {
                        case SharpToken.Class:
                            {
                                OnReset();
                                State = 1;
                            }
                            return ProductionState.Preserve;
                        default: return ProductionState.Revert;
                    }

                case 1: switch (value)
                    {
                        case SharpToken.CurlyBracketOpen:
                            {
                                scope = Linter.Scope;
                            }
                            return ProductionState.Shift;
                        default: return ProductionState.Preserve;
                    }
                    
                #region Static
                case 2: switch (value)
                    {
                        case SharpToken.CurlyBracketClose:
                            {
                                if (Linter.Scope < scope)
                                    return ProductionState.Revert;
                            }
                            return ProductionState.Preserve;
                        case SharpToken.Static:
                            {
                                State = 3;
                            }
                            return ProductionState.Preserve;
                        default: return ProductionState.Preserve;
                    }
                #endregion

                #region (Private | Protected | Internal | Public)
                case 3: switch (value)
                    {
                        case SharpToken.Void:
                        case SharpToken.Int:
                            return ProductionState.Shift;
                        case SharpToken.Private:
                        case SharpToken.Protected:
                        case SharpToken.Internal:
                        case SharpToken.Public:
                            return ProductionState.Preserve;
                    }
                    goto case 2;
                #endregion

                #region 'Main'
                case 4: switch (value)
                    {
                        case SharpToken.Identifier:
                            {
                                if (!Linter.Buffer.Equals("Main", StringComparison.InvariantCulture))
                                    break;
                            }
                            return ProductionState.Shift;
                    }
                    goto case 2;
                #endregion

                #region RoundBracketOpen
                case 5: switch (value)
                    {
                        case SharpToken.RoundBracketOpen:
                            return ProductionState.Success;
                    }
                    goto case 2;
                #endregion
            }
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override void OnReset()
        {
            scope = 0;
        }
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
