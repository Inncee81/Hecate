// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace SE.Hecate
{
/// <summary>
/// Keeps a priority indicator which is used to order ProcessorUnits in their family
/// </summary>
    public struct ProcessorPriority : IComparable<ProcessorPriority>
    {
        public static readonly ProcessorPriority Default = new ProcessorPriority(0);

        private readonly byte value;
        /// <summary>
        /// The overall computed value
        /// </summary>
        public int Value
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return (int)value; }
        }

        /// <summary>
        /// Indicates that this ProcessorUnit has the SDK priority
        /// </summary>
        public bool IsSdkRoot
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return ((value & 3) != 0); }
        }
        /// <summary>
        /// Indicates that this ProcessorUnit has the project priority
        /// </summary>
        public bool IsProjectRoot
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return (((value >> 4) & 3) != 0); }
        }
        /// <summary>
        /// Indicates that this ProcessorUnit is an override based on a specific file system location
        /// </summary>
        public bool IsLocalOverride
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return (((value >> 2) & 3) != 0); }
        }

        /// <summary>
        /// Creates a new priority indicator
        /// </summary>
        public ProcessorPriority(byte priority)
        {
            this.value = priority;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public static implicit operator int(ProcessorPriority priority)
        {
            return priority.value;
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public static implicit operator ProcessorPriority(int priority)
        {
            return new ProcessorPriority((byte)(priority & 0xFF));
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public int CompareTo(ProcessorPriority other)
        {
            return value.CompareTo(other.value);
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        /// <summary>
        /// Computes the priority indicator from provided file system location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public static ProcessorPriority Create(PathDescriptor location)
        {
            byte priority = 0;
            if (Application.ProjectRoot.Contains(location))
            {
                priority = SetLocalOverride(priority, Application.ProjectRoot.Order < location.Order);
                priority = SetProjectRoot(priority);
            }
            else
            {
                priority = SetLocalOverride(priority, Application.SdkRoot.Order < location.Order);
                priority = SetSdkRoot(priority);
            }
            return new ProcessorPriority(priority);
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        private static byte SetSdkRoot(byte priority)
        {
            return (byte)(priority | 1);
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        private static byte SetProjectRoot(byte priority)
        {
            return (byte)(priority | (1 << 4));
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        private static byte SetLocalOverride(byte priority, bool value)
        {
            return (byte)(priority | (value ? (1 << 2) : 0));
        }
    }
}
