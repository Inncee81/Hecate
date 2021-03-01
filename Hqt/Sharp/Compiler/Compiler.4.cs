// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

#if NET_FRAMEWORK
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime;

namespace SE.Hecate.Sharp
{
    public partial class Compiler : ProcessorUnit
    {
        static Dictionary<string, string> config;

        static Compiler()
        {
            config = new Dictionary<string, string>();
            config.Add("CompilerVersion", "v4.0");
        }

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
    }
}
#endif