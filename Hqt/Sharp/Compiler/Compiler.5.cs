// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

#if !NET_FRAMEWORK
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime;
using System.Runtime.CompilerServices;
using SE.Json;

namespace SE.Hecate.Sharp
{
    public partial class Compiler : ProcessorUnit
    {
        private static int Process(CompilerCommand compile, PathDescriptor buildCache)
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

            FileDescriptor responseFile = new FileDescriptor(buildCache, "csc.rsp");
            using (FileStream fs = responseFile.Open(FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                compile.Finalize(sw);
            }

            return Run(compile, responseFile);
        }

        static int Run(CompilerCommand compile, FileDescriptor responseFile)
        {
            Process process = new Process();
            ProcessStartInfo info = process.StartInfo;
            info.WorkingDirectory = Application.WorkerPath.GetAbsolutePath();
            info.UseShellExecute = false;

            string command = Environment.ExpandEnvironmentVariables(BuildParameter.Roslyn.Compiler.Trim()).Replace("\\\"", "\"");
            int splitIndex = command.IndexOf(' ');
            if (splitIndex > 0)
            {
                info.FileName = command.Substring(0, splitIndex);
                info.Arguments = command.Substring(splitIndex);
            }
            else info.FileName = command;
            info.Arguments = string.Concat(info.Arguments, " @\"", responseFile.GetRelativePath(Application.WorkerPath), "\"");
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            process.OutputDataReceived += Log;
            process.OutputDataReceived += Error;

            if (process.Start())
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
                int returnCode = process.ExitCode;
                if (returnCode == Application.SuccessReturnCode)
                {
                    CreateRuntimeConfig(compile);
                }
                return returnCode;
            }
            else return Application.FailureReturnCode;
        }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        static void Log(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data) && e.Data.StartsWith("[warning]", StringComparison.InvariantCultureIgnoreCase))
                Application.Warning(SeverityFlags.Minimal, e.Data);
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        static void Error(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                Application.Error(SeverityFlags.None, e.Data);
        }

        private static void CreateRuntimeConfig(CompilerCommand compile)
        {
            JsonDocument config = new JsonDocument();
            JsonNode node = config.AddNode(JsonNodeType.Object);
            {
                node = config.AddNode(node, JsonNodeType.Object);
                node.Name = "runtimeOptions";
                {
                    JsonNode prop = config.AddNode(node, JsonNodeType.String);
                    prop.RawValue = "net5.0";
                    prop.Name = "tfm";

                    if (compile.Frameworks.Count > 1)
                    {
                        node = config.AddNode(node, JsonNodeType.Array);
                        node.Name = "frameworks";
                        foreach(KeyValuePair<string, string> framework in compile.Frameworks)
                        {
                            JsonNode item = config.AddNode(node, JsonNodeType.Object);
                            {
                                prop = config.AddNode(item, JsonNodeType.String);
                                prop.RawValue = framework.Key;
                                prop.Name = "name";

                                prop = config.AddNode(item, JsonNodeType.String);
                                prop.RawValue = framework.Value;
                                prop.Name = "version";
                            }
                        }
                    }
                    else
                    {
                        node = config.AddNode(node, JsonNodeType.Object);
                        node.Name = "framework";
                        {
                            KeyValuePair<string, string> framework = compile.Frameworks.First();

                            prop = config.AddNode(node, JsonNodeType.String);
                            prop.RawValue = framework.Key;
                            prop.Name = "name";

                            prop = config.AddNode(node, JsonNodeType.String);
                            prop.RawValue = framework.Value;
                            prop.Name = "version";
                        }
                    }
                }
            }
            using (FileStream fs = compile.TargetFile.ChangeExtensions("runtimeconfig", "json").Open(FileMode.Create, FileAccess.Write))
            {
                config.Save(fs);
            }
        }
    }
}
#endif