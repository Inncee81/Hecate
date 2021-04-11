// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using SE.Flex;

namespace SE.Hecate.Build
{
    /// <summary>
    /// A container around certain directory path which counts as a module
    /// </summary>
    public class BuildModule : FlexObject
    {
        PathDescriptor location;
        /// <summary>
        /// The directory path of this module
        /// </summary>
        public PathDescriptor Location
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return location; }
        }

        /// <summary>
        /// The name of this module
        /// </summary>
        public string Name
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return location.Name; }
        }

        bool isPackage;
        /// <summary>
        /// Determines if the module was load as part of the package lookup
        /// </summary>
        public bool IsPackage
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return isPackage; }
            [MethodImpl(OptimizationExtensions.ForceInline)]
            internal set { isPackage = value; }
        }

        /// <summary>
        /// Creates a new instance from the provided directory path
        /// </summary>
        public BuildModule(PathDescriptor location)
        {
            this.location = location;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override int GetHashCode()
        {
            return location.GetHashCode();
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override bool Equals(object obj)
        {
            BuildModule tmp = (obj as BuildModule);
            if (tmp != null)
            {
                return location.Equals(tmp.location);
            }
            else return false;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override string ToString()
        {
            return Name;
        }
    }
}
