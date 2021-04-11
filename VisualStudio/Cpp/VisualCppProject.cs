// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace SE.Hecate.VisualStudio
{
    /// <summary>
    /// Microsoft Visual Studio C++ Project
    /// </summary>
    public class VisualCppProject : VisualStudioProject
    {
        public override Guid ProjectTypeGuid
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return Guid.ParseExact(CppGuid, "D"); }
        }

        /// <summary>
        /// Creates a new project instance
        /// </summary>
        /// <param name="version">VisualStudio version this project relates to</param>
        /// <param name="projectRoot">The file system ddirectory this project is based on</param>
        /// <param name="name">The name of this project</param>
        public VisualCppProject(VisualStudioVersion version, PathDescriptor projectRoot, string name, bool isPackage)
            : base(version, new FileDescriptor(projectRoot, "{0}.vcxproj", name), isPackage)
        { }
    }
}
