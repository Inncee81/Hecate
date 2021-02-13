// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Text;
using SE.Parsing;
using SE.SharpLang;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Using (Identifier | (Identifier Dot))+ Semicolon;
    /// </summary>
    public class UsingRule : ParserRule<SharpToken>
    {
        StringBuilder buffer;

        Linter linter;
        /// <summary>
        /// The Linter instance this rule is assigned to
        /// </summary>
        public Linter Linter
        {
            get { return linter; }
            set { linter = value; }
        }
        
        SharpModuleSettings settings;
        /// <summary>
        /// A CSharp configuration instance currently in process
        /// </summary>
        public SharpModuleSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        /// <summary>
        /// Creates this rule
        /// </summary>
        public UsingRule()
        {
            this.buffer = new StringBuilder();
        }
        public override void Dispose()
        {
            linter = null;
            settings = null;
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
                                buffer.Append(linter.Buffer);
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

        public override void OnReset()
        {
            buffer.Clear();
        }
        public override void OnCompleted()
        {
            lock (settings.UsingDirectives)
                settings.UsingDirectives.Add(buffer.ToString());
        }
    }
}
