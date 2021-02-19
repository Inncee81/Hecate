// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// A list of extended processor family IDs
    /// </summary>
    public enum ProcessorFamilies
    {
        CppInitialize = (SE.Hecate.ProcessorFamilies.Custom + 16),
        CppBuild,
        CppPublish,
    }
}
