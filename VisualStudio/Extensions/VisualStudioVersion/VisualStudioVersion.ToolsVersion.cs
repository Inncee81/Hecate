// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate.VisualStudio
{
    public static partial class VisualStudioVersionExtension
    {
        /// <summary>
        /// Obtains the tooling version from this VisualStudio version
        /// </summary>
        public static string ToolsVersion(this VisualStudioVersion flag)
        {
            switch (flag)
            {
                case VisualStudioVersion.VisualStudio2010:
                case VisualStudioVersion.VisualStudio2012:
                    return "4.0";
                case VisualStudioVersion.VisualStudio2013:
                    return "12.0";
                case VisualStudioVersion.VisualStudio2015:
                    return "14.0";
                case VisualStudioVersion.VisualStudio2017:
                    return "15.0";
                case VisualStudioVersion.VisualStudio2019:
                    return "16.0";
            }
            return string.Empty;
        }
    }
}
