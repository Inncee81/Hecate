// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate.Build
{
    /// <summary>
    /// Provides output types to nodes processing code
    /// </summary>
    public enum BuildModuleType : byte
    {
        Undefined = 0,
        /// <summary>
        /// C++
        /// </summary>
        StaticLibrary = 1,
        /// <summary>
        /// C++, C#
        /// </summary>
        DynamicLibrary = 2,
        /// <summary>
        /// C++, C#
        /// </summary>
        Console = 3,
        /// <summary>
        /// C++, C#
        /// </summary>
        Executable = 4
    }
}