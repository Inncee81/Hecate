// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Preforms CSharp code compilation
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class Compiler : ProcessorUnit
    {
        static Dictionary<string, string> config;

        public override PathDescriptor Target
        {
            get { return Application.SdkRoot; }
        }
        public override bool Enabled
        {
            get { return true; }
        }
        public override UInt32 Family
        {
            get { return (UInt32)ProcessorFamilies.SharpCompile; }
        }

        static Compiler()
        {
            config = new Dictionary<string, string>();
            config.Add("CompilerVersion", "v4.0");
        }
        /// <summary>
        /// Creates a new compiler instance
        /// </summary>
        public Compiler()
        { }

        private static int Process(CompilerCommand compile, PathDescriptor buildCache)
        {
            CSharpCodeProvider generator = new CSharpCodeProvider(config);
            try
            {
                CreateAssemblyIdentity(compile, buildCache);
                for(int i = compile.Sources.Count - 1; i >= 0; i--)
                {
                    FileDescriptor file = (compile.Sources[i] as FileDescriptor);
                    switch (file.Extension)
                    {
                        case "resx":
                            {
                                FileDescriptor resourceFile = new FileDescriptor(buildCache, "{0}.resource", file.FullName);
                                using (ResXResourceReader reader = new ResXResourceReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)))
                                using (ResourceWriter writer = new ResourceWriter(resourceFile.GetAbsolutePath()))
                                {
                                    reader.BasePath = file.Location.GetAbsolutePath();
                                    foreach (DictionaryEntry resource in reader)
                                        writer.AddResource(resource.Key as string, resource.Value);
                                }
                                compile.Sources[i] = resourceFile;
                            }
                            break;
                        default: break;
                    }
                }
                if (!compile.TargetFile.Location.Exists())
                try
                {
                    compile.TargetFile.Location.Create();
                }
                catch(Exception)
                { }

                CompilerResults result = generator.CompileAssemblyFromFile(compile.Finalize(), compile.Sources.Select(x => x.GetAbsolutePath()).ToArray());
			    if (result.Errors.Count > 0)
			    {
				    foreach (CompilerError e in result.Errors)
				    {
                        if (e.IsWarning) Application.Warning(SeverityFlags.Minimal, e.ToString().Replace("{", "{{{{"));
                        else Application.Error(SeverityFlags.Minimal, e.ToString().Replace("{", "{{{{"));
				    }
                    if (result.Errors.HasErrors || compile.WarningAsError)
                        return Application.FailureReturnCode;
			    }
                return Application.SuccessReturnCode;
            }
            finally
            {
                compile.TemporaryFiles.Delete();
            }
        }
        public override bool Process(KernelMessage command)
        {
            CompilerCommand compile = (command as CompilerCommand);
            if (compile != null)
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
