// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;

namespace SE.Hecate.Build
{
    /// <summary>
    /// A comparer to sort code modules
    /// </summary>
    public class BuildModuleComparer : IComparer<BuildModule>
    {
        /// <summary>
        /// A default comparer instance
        /// </summary>
        public readonly static BuildModuleComparer Default = new BuildModuleComparer();

        /// <summary>
        /// Creates a new comparer instance
        /// </summary>
        public BuildModuleComparer()
        { }

        public int Compare(BuildModule x, BuildModule y)
        {
            if (x.Location == y.Location || x.IsPackage == y.IsPackage) return 0;
            else if (x.IsPackage) return 1;
            else if (y.IsPackage) return -1;
            else return 0;
        }
    }
}
