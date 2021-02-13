// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate
{
    /// <summary>
    /// An incomplete list of built-in processor family IDs
    /// </summary>
    public enum ProcessorFamilies
    {
        //Base
        EntryPoint = 0,
        InfoProvider,
        Setup,

        //Build
        Preprocess,
        Conversion,
        Validation,
        Prebuild,
        Build,
        Postbuild,
        Postprocess,
        Deployment,

        //Packages
        Install,
        Remove,

        //Sharp
        SharpInitialize,
        SharpBuild,
        SharpCompile,
        SharpPublish,

        //End-of-Built-In
        Custom
    }
}
