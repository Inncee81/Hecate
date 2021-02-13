// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using SE.Config;
using SE.Flex;

namespace SE.Hecate
{
    /// <summary>
    /// A pipeline message used to enter processing mode
    /// </summary>
    [PropertyInfo]
    public class LocalEntryPoint : KernelMessage
    {
        [VerbProperty(0)]
        [PropertyDescription("As one of the located commands. Implies 'build' as the default value", Type = PropertyType.Required)]
        string command;
        /// <summary>
        /// The primary command verb passed
        /// </summary>
        public string Command
        {
            get 
            {
                if (!string.IsNullOrWhiteSpace(command))
                {
                    return command.ToLowerInvariant();
                }
                else return string.Empty;
            }
        }

        [VerbProperty(1)]
        [PropertyDescription("Additional arguments assigned to the command", Type = PropertyType.Optional)]
        readonly List<string> args;
        /// <summary>
        /// Additional argument verbs passed
        /// </summary>
        public List<string> Args
        {
            get { return args; }
        }

        /// <summary>
        /// Creates a new message to the provided project path
        /// </summary>
        /// <param name="path">The path to the desired project root</param>
        public LocalEntryPoint(PathDescriptor path)
            : base(TemplateId.Create() | (UInt32)ProcessorFamilies.EntryPoint, path)
        {
            this.args = new List<string>();
        }

        /// <summary>
        /// Moves the primary command verb into the argument verb collection. The primary command
        /// is changed to 'build'
        /// </summary>
        public bool MakeDefault()
        {
            if (Directory.Exists(command) || File.Exists(command))
            {
                args.Insert(0, command);
                command = "build";

                return true;
            }
            else return false;
        }
    }
}
