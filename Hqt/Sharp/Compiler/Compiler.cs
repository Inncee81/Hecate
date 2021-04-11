// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Preforms CSharp code compilation
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public partial class Compiler : ProcessorUnit
    {
        public override PathDescriptor Target
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return Application.SdkRoot; }
        }
        public override bool Enabled
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return true; }
        }
        public override UInt32 Family
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return (UInt32)ProcessorFamilies.SharpCompile; }
        }

        /// <summary>
        /// Creates a new compiler instance
        /// </summary>
        public Compiler()
        { }

        public override bool Process(KernelMessage command)
        {
            CompilerCommand compile = (command as CompilerCommand);
            #if NET_FRAMEWORK
            if (compile != null)
            #else
            if(compile != null && BuildParameter.Roslyn != null)
            #endif
            {
                PathDescriptor buildCache = Application.ProjectRoot.Combine(".build");
                try
                {
                    if (!buildCache.Exists())
                        buildCache.CreateHidden();
                }
                catch { }
                buildCache = buildCache.Combine("csc").Combine(compile.TargetFile.Name);
                try
                {
                    if (!buildCache.Exists())
                         buildCache.Create();
                }
                catch { }
                compile.Attach(Taskʾ.Run<int>(() => Process(compile, buildCache)));
                return true;
            }
            return false;
        }

        private static void CreateAssemblyIdentity(CompilerCommand compile, PathDescriptor buildCache)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("using System.Runtime.Versioning;");
            sb.AppendLine();

            #if net40
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.0\")]");
            #elif net45
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.5\")]");
            #elif net451
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.5.1\")]");
            #elif net452
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.5.2\")]");
            #elif net46
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.6\")]");
            #elif net461
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.6.1\")]");
            #elif net462
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.6.2\")]");
            #elif net47
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.7\")]");
            #elif net471
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.7.1\")]");
            #elif net472
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.7.2\")]");
            #elif net48
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETFramework, Version = v4.8\")]");
            #else
            sb.AppendLine("[assembly: TargetFrameworkAttribute(\".NETCoreApp, Version = v5.0\")]");
            #endif
            
            FileDescriptor file = new FileDescriptor(buildCache, "_asmdef.cs");
            using (FileStream fs = file.Open(FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(sb.ToString());
            }
            compile.Sources.Add(file);
        }
    }
}
