// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.Alchemy;
using SE.Json;

namespace SE.Hecate
{
    /// <summary>
    /// A formatting module used by Alchemy
    /// </summary>
    [InitializeOnLoad]
    public class JsonModule : JsonDocument, IFormatModule
    {
        static JsonModule()
        {
            FormatRegistry.Add<JsonModule>("json");
        }
        /// <summary>
        /// Creates a new instance of this module
        /// </summary>
        public JsonModule()
        { }

        public string Transform(Alchemy.Token token, string input)
        {
            if (token == Alchemy.Token.Identifier)
            {
                return string.Concat("\"", input, "\"");
            }
            else return input;
        }
    }
}
