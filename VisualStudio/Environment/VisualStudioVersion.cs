// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// The most supported VisualStudio versions
    /// </summary>
    public enum VisualStudioVersion : byte
    {
        Invalid = 0,

        VisualStudio2019 = 160, //16.0
        VisualStudio2017 = 150, //15.0
        VisualStudio2015 = 140, //14.0
        VisualStudio2013 = 120, //12.0
        VisualStudio2012 = 110, //11.0
        VisualStudio2010 = 100 //10.0
    }
}
