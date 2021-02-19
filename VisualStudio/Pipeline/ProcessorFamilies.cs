// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// A list of extended processor family IDs
    /// </summary>
    public enum ProcessorFamilies
    {
        SharpProject = (SE.Hecate.ProcessorFamilies.Custom + 32),
        CppProject,
        VisualSharp,
        VisualCpp,
        Solution
    }
}
