// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using SE.Flex;
using SE.Hecate.Build;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// Pipeline node to perform Cpp code file lookups
    /// </summary>
    [ProcessorUnit(IsExtension = true)]
    public class ValidationController : ProcessorUnit, IPrioritizedActor
    {
        /// <summary>
        /// Files that are considered to belong to Cpp code
        /// </summary>
        public readonly static string[] ValidFileExtensions = new string[]
        {
            "*.h",
            "*.hpp",
            "*.c",
            "*.cpp",
            "*.asm",
            "*.S"
        };

        private static Filter fileFilter;
        private static Filter directoryFilter;

        int IPrioritizedActor.Priority
        {
            get { return 0; }
        }
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
            get { return (UInt32)SE.Hecate.ProcessorFamilies.Validation; }
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

        public void Attach(PriorityDispatcher owner)
        { }
        public void Detach(PriorityDispatcher owner)
        { }

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
        public bool OnError(Exception error)
        {
            return true;
        }
        public void OnCompleted()
        { }

        private static async Task<int> Process(IEnumerable<object> modules, BuildModule module, BuildProfile profile, TaskCompletionSource<bool> isValidCppModuleFlag)
        {
            if (!module.IsPackage || await isValidCppModuleFlag.Task)
            {
                List<FileSystemDescriptor> files = CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Get();
                foreach (PathDescriptor subFolder in module.Location.FindDirectories(directoryFilter))
                {
                    subFolder.FindFiles(fileFilter, files);
                }
                module.Location.FindFiles(fileFilter, files, PathSeekOptions.Forward | PathSeekOptions.RootLevel);
                if (files.Where(x => fileFilter.IsMatch(x.FullName)).Any())
                {
                    isValidCppModuleFlag.TrySetResult(true);
                    ValidationCommand command = new ValidationCommand(modules, module, profile, files);
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
                else isValidCppModuleFlag.TrySetResult(false);
            }
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            List<object> modules = CollectionPool<List<object>, object>.Get();
            if (PropertyManager.FindProperties(x => x.Value is BuildModule, modules) > 0)
            {
                List<Task<int>> tasks = CollectionPool<List<Task<int>>, Task<int>>.Get();
                try
                {
                    BuildProfile profile; command.TryGetProperty<BuildProfile>(out profile);
                    TaskCompletionSource<bool> isValidCppModuleFlag = new TaskCompletionSource<bool>();
                    IEnumerator<object> iterator = modules.OrderBy(x => (x as BuildModule), BuildModuleComparer.Default).GetEnumerator();
                    for (bool first = true; iterator.MoveNext(); first = false)
                    {
                        BuildModule module = (iterator.Current as BuildModule);
                        if (first && module.IsPackage)
                        {
                            isValidCppModuleFlag.TrySetResult(false);
                        }
                        tasks.Add(Process(modules, module, profile, isValidCppModuleFlag));
                    }
                    command.Attach(Taskʾ.WhenAll<int>(tasks).ContinueWith<int>((task) =>
                    {
                        if (isValidCppModuleFlag.Task.Result)
                        {
                            SE.Hecate.SetupController.LoadSettings<BuildParameter>("Cpp", profile.Platform.ToUpperInvariant(), profile.Target.ToString().ToUpperInvariant());
                        }
                        CollectionPool<List<object>, object>.Return(modules);
                        return KernelMessage.Validate(task);

                    }));
                }
                finally
                {
                    CollectionPool<List<Task<int>>, Task<int>>.Return(tasks);
                }
                return true;
            }
            else CollectionPool<List<object>, object>.Return(modules);
            return false;
        }
    }
}
