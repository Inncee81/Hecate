// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using SE.Parsing;
using SE.SharpLang;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Using (Identifier | (Identifier Dot))+ Semicolon;
    /// </summary>
    public class UsingRule : SharpParserRule
    {
        StringBuilder buffer;

        CacheEntry cache;
        /// <summary>
        /// A CSharp cache entry instance assigned to this rule
        /// </summary>
        public CacheEntry Cache
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return cache; }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            set { cache = value; }
        }

        /// <summary>
        /// Creates this rule
        /// </summary>
        public UsingRule()
        {
            this.buffer = new StringBuilder();
        }

        protected override ProductionState Process(SharpToken value)
        {
            switch (State)
            {
                #region Using
                default:
                case 0: switch (value)
                    {
                        case SharpToken.Using:
                            {
                                OnReset();
                                State = 1;
                            }
                            return ProductionState.Preserve;
                        default:
                            return ProductionState.Revert;
                    }
                #endregion

                #region (Identifier | (Identifier
                case 1: switch (value)
                    {
                        case SharpToken.Identifier:
                            {
                                buffer.Append(Linter.Buffer);
                            }
                            return ProductionState.Shift;
                    }
                    goto case 0;
                #endregion

                #region  Dot))+ Semicolon
                case 2: switch (value)
                    {
                        case SharpToken.Dot:
                            {
                                buffer.Append('.');
                                State = 1;
                            }
                            return ProductionState.Preserve;
                        case SharpToken.Semicolon:
                            return ProductionState.Success;
                    }
                    goto case 0;
                    #endregion
            }
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override void OnReset()
        {
            buffer.Clear();
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override void OnCompleted()
        {
            lock (Settings.UsingDirectives)
            {
                Settings.UsingDirectives.Add(buffer.ToString());
            }
            cache.UsingDirectives.Add(buffer.ToString());
        }
    }
}
