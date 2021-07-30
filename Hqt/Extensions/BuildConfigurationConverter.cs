// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using SE.CommandLine;
using SE.Config;
using SE.Json;

namespace SE.Hecate.Build
{
    /// <summary>
    /// An auto converter used on auto-loading build profile files
    /// </summary>
    public class BuildConfigurationConverter : ITypeConverter
    {
        /// <summary>
        /// Creates a new instance of the converter
        /// </summary>
        public BuildConfigurationConverter()
        { }

        public bool TryParseValue(Type targetType, object value, out object result)
        {
            JsonNode node = (value as JsonNode);
            if (node != null && !string.IsNullOrWhiteSpace(node.Name) && node.Type == JsonNodeType.Object)
            {
                List<object> items = CollectionPool<List<object>, object>.Get();
                try
                {
                    node = node.Child;
                    while (node != null)
                    {
                        if (node.Type == JsonNodeType.Object)
                        {
                            JsonDocument settings = new JsonDocument();
                            settings.AddAppend(null, node);

                            BuildConfiguration config = new BuildConfiguration(node.Name);
                            PropertyMapper.Assign(config, settings, true, true);
                            PropertyMapper.Assign(config, CommandLineOptions.Default, true, true);
                            items.Add(config);
                        }
                        node = node.Next;
                    }
                    result = items.ToArray();
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
