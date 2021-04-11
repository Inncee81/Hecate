// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Pipeline node to perform CSharp code file lookups
    /// </summary>
    [ProcessorUnit(IsExtension = true)]
    public class ValidationController : ProcessorUnit, IPrioritizedActor
    {
        /// <summary>
        /// Files that are considered to belong to CSharp code
        /// </summary>
        public readonly static string[] ValidFileExtensions = new string[]
        {
            "*.cs",
            "*.resx"
        };

        private static Filter fileFilter;
        private static Filter directoryFilter;

        int IPrioritizedActor.Priority
        {
            [MethodImpl(OptimizationExtensions.ForceInline)]
            get { return 0; }
        }
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
            get { return (UInt32)ProcessorFamilies.Validation; }
        }

        static ValidationController()
        {
            fileFilter = new Filter();
            foreach (string extension in ValidFileExtensions)
            {
                fileFilter.Add(extension);
            }

            directoryFilter = new Filter();
            FilterToken root = directoryFilter.Add("...");
            FilterToken token = directoryFilter.Add(root, ".*");
            token.Type = FilterType.And;
            token.Exclude = true;
            token = directoryFilter.Add(root, "obj");
            token.Type = FilterType.And;
            token.Exclude = true;
            token = directoryFilter.Add(root, "bin");
            token.Type = FilterType.And;
            token.Exclude = true;
        }
        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public ValidationController()
        { }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void Attach(PriorityDispatcher owner)
        { }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void Detach(PriorityDispatcher owner)
        { }

        [MethodImpl(OptimizationExtensions.ForceInline)]
        public bool OnNext(KernelMessage value)
        {
            try
            {
                return Process(value);
            }
            catch (Exception er)
            {
                Application.Error(er);
                return false;
            }
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public bool OnError(Exception error)
        {
            return true;
        }
        [MethodImpl(OptimizationExtensions.ForceInline)]
        public void OnCompleted()
        { }

        private static async Task<int> Process(BuildModule module, BuildProfile profile, TaskCompletionSource<bool> isValidSharpModuleFlag)
        {
            if(!module.IsPackage || await isValidSharpModuleFlag.Task)
            {
                List<FileSystemDescriptor> files = CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Get();
                foreach (PathDescriptor subFolder in module.Location.FindDirectories(directoryFilter))
                {
                    if (subFolder.Name.Equals("resources", StringComparison.InvariantCultureIgnoreCase))
                    {
                        files.AddRange(subFolder.GetFiles());
                    }
                    else subFolder.FindFiles(fileFilter, files);
                }
                module.Location.FindFiles(fileFilter, files, PathSeekOptions.Forward | PathSeekOptions.RootLevel);
                if (files.Where(x => fileFilter.IsMatch(x.FullName)).Any())
                {
                    isValidSharpModuleFlag.TrySetResult(true);
                    ValidationCommand command = new ValidationCommand(module, profile, files);
                    try
                    {
                        bool exit; if (Kernel.Dispatch(command, out exit))
                        {
                            return await command.Task.ContinueWith<int>((task) =>
                            {
                                CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Return(files);
                                return task.Result;

                            });
                        }
                        else if (exit)
                        {
                            return Application.FailureReturnCode;
                        }
                    }
                    finally
                    {
                        command.Release();
                    }
                }
                else isValidSharpModuleFlag.TrySetResult(false);
            }
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            List<object> modules = CollectionPool<List<object>, object>.Get();
            try
            {
                if (PropertyManager.FindProperties(x => x.Value is BuildModule, modules) > 0)
                {
                    BuildProfile profile; command.TryGetProperty<BuildProfile>(out profile);
                    TaskCompletionSource<bool> isValidSharpModuleFlag = new TaskCompletionSource<bool>();
                    IEnumerator<object> iterator = modules.OrderBy(x => (x as BuildModule), BuildModuleComparer.Default).GetEnumerator();
                    for(bool first = true; iterator.MoveNext(); first = false)
                    {
                        BuildModule module = (iterator.Current as BuildModule);
                        if (first && module.IsPackage)
                        {
                            isValidSharpModuleFlag.TrySetResult(false);
                        }
                        command.Attach(Process(module, profile, isValidSharpModuleFlag));
                    }
                    command.Attach(isValidSharpModuleFlag.Task.ContinueWith<int>((task) =>
                    {
                        if (task.Result)
                        {
                            SE.Hecate.SetupController.LoadSettings<BuildParameter>
                            (
                                "Sharp", 
                                profile.Platform.ToUpperInvariant(), 
                                profile.Target.ToString().ToUpperInvariant()

                                #if NET_FRAMEWORK
                                #if net40
                                ,"net40"
                                ,"NET40"
                                ,"NET_4_0"
                                #elif net45
                                ,"net45"
                                ,"NET45"
                                ,"NET_4_5"
                                #elif net451
                                ,"net451"
                                ,"NET451"
                                ,"NET_4_5_1"
                                #elif net452
                                ,"net452"
                                ,"NET452"
                                ,"NET_4_5_2"
                                #elif net46
                                ,"net46"
                                ,"NET46"
                                ,"NET_4_6"
                                #elif net461
                                ,"net461"
                                ,"NET461"
                                ,"NET_4_6_1"
                                #elif net462
                                ,"net462"
                                ,"NET462"
                                ,"NET_4_6_2"
                                #elif net47
                                ,"net47"
                                ,"NET47"
                                ,"NET_4_7"
                                #elif net471
                                ,"net471"
                                ,"NET471"
                                ,"NET_4_7_1"
                                #elif net472
                                ,"net472"
                                ,"NET472"
                                ,"NET_4_7_2"
                                #elif net48
                                ,"net48"
                                ,"NET48"
                                ,"NET_4_8"
                                #endif
                                ,"NET_FRAMEWORK"
                                #else
                                ,"net50"
                                ,"NET50"
                                ,"NET_5_0"
                                ,"NET_CORE"
                                #endif
                            );
                        }
                        return Application.SuccessReturnCode;

                    }));
                    return true;
                }
            }
            finally
            {
                CollectionPool<List<object>, object>.Return(modules);
            }
            return false;
        }
    }
}
