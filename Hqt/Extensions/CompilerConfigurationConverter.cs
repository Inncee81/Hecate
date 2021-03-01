// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.Config;
using SE.Json;

namespace SE.Hecate.Build
{
    /// <summary>
    /// An auto converter used on auto-loading language compiler sections
    /// </summary>
    public class CompilerConfigurationConverter : ITypeConverter
    {
        /// <summary>
        /// Creates a new instance of the converter
        /// </summary>
        public CompilerConfigurationConverter()
        { }

        public bool TryParseValue(Type targetType, object value, out object result)
        {
            JsonNode node = (value as JsonNode);
            if (node != null && !string.IsNullOrWhiteSpace(node.Name) && node.Type == JsonNodeType.Object)
            {
                List<object> items = CollectionPool<List<object>, object>.Get();
                try
                {
                    JsonDocument settings = new JsonDocument();
                    settings.AddAppend(null, node);

                    result = new CompilerConfiguration(node.Name);
                    PropertyMapper.Assign(result, settings, true, true);
                    return true;
                }
                finally
                {
                    CollectionPool<List<object>, object>.Return(items);
                }
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}
