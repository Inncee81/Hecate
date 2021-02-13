// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate.VisualStudio
{
    public static partial class VisualStudioVersionExtension
    {
        /// <summary>
        /// Obtains the toolset version from this VisualStudio version
        /// </summary>
        public static string ToolsetVersion(this VisualStudioVersion flag)
        {
            switch (flag)
            {
                case VisualStudioVersion.VisualStudio2010:
                    return "v100";
                case VisualStudioVersion.VisualStudio2012:
                    return "v110";
                case VisualStudioVersion.VisualStudio2013:
                    return "v120";
                case VisualStudioVersion.VisualStudio2015:
                    return "v140";
                case VisualStudioVersion.VisualStudio2017:
                    return "v141";
                case VisualStudioVersion.VisualStudio2019:
                    return "v142";
            }
            return string.Empty;
        }
    }
}
