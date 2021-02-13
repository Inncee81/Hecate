// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime;
using SE.Config;

namespace SE.App
{
    /// <summary>
    /// Basic settings used on any activity
    /// </summary>
    public sealed class Settings
    {
        [NamedProperty('h', "host")]
        private static string host = null;
        /// <summary>
        /// The provided remote host if any
        /// </summary>
        public static string Host
        {
            get { return host; }
        }

        [NamedProperty('l', "log", DefaultValue = 1)]
        [PropertyDescription("Sets the default log level to any of [0-2]", Type = PropertyType.Optional)]
        /// <summary>
        /// The default log level
        /// </summary>
        public static int LogLevel
        {
            get { return (int)Application.LogSeverity; }
            private set
            {
                if (value < 0)
                {
                    value = 0;
                }
                else if (value > 2)
                {
                    value = 2;
                }
                Application.LogSeverity = (SeverityFlags)value;
            }
        }

        [NamedProperty('?', "help")]
        private static bool displayInfo = false;
        /// <summary>
        /// Help for this program requested
        /// </summary>
        public static bool DisplayInfo
        {
            get { return displayInfo; }
        }

        private Settings()
        { }
    }
}
