// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

#if NET_FRAMEWORK
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SE.Hecate.Sharp
{
    public static partial class AssemblyCache
    {
        /// <summary>
        /// Loads CSharp Framework assemblies in an isolated location
        /// </summary>
        public class FrameworkAssemblyLoader : AppDomainʾ.ReferenceObject
        {
            ReadWriteLock assemblyLock;
            readonly Dictionary<string, List<FileDescriptor>> assemblies;
            /// <summary>
            /// A collection of assemblies detected
            /// </summary>
            public Dictionary<string, List<FileDescriptor>> Assemblies
            {
                [MethodImpl(OptimizationExtensions.ForceInline)]
                get { return assemblies; }
            }
            
            /// <summary>
            /// Creates a new worker instance
            /// </summary>
            public FrameworkAssemblyLoader()
            {
                assemblies = new Dictionary<string, List<FileDescriptor>>();
            }

            /// <summary>
            /// Tries to inegrate the provided file into the assembly list
            /// </summary>
            [MethodImpl(OptimizationExtensions.ForceInline)]
            public void LoadReferenceAssembly(FileSystemDescriptor assemblyFile)
            {
                try
                {
                    Assembly.ReflectionOnlyLoadFrom(assemblyFile.GetAbsolutePath());
                }
                catch (FileNotFoundException)
                { }
                catch (FileLoadException)
                { }
                catch (BadImageFormatException)
                { }
            }
            /// <summary>
            /// Tries to integrate the provided full qualified assembly name into the assembly list
            /// </summary>
            public void LoadFrameworkAssembly(string assemblyIdentifier)
            {
                try
                {
                    HashSet<string> namespaces = CollectionPool<HashSet<string>, string>.Get();
                    try
                    {
                        Assembly assembly = Assembly.ReflectionOnlyLoad(assemblyIdentifier);
                        LoadNamespaces(assembly, namespaces);
                        foreach (string namespaceReference in namespaces)
                        {
                            AddAssembly(FileDescriptor.Create(new Uri(assembly.CodeBase).LocalPath), namespaceReference);
                        }
                    }
                    finally
                    {
                        CollectionPool<HashSet<string>, string>.Return(namespaces);
                    }
                }
                catch(FileNotFoundException)
                { }
                catch (FileLoadException)
                { }
                catch (ReflectionTypeLoadException)
                { }
                catch (BadImageFormatException)
                { }
            }
            /// <summary>
            /// Occurs when the resolution of an assembly fails
            /// </summary>
            [MethodImpl(OptimizationExtensions.ForceInline)]
            public Assembly ResolveAssemblies(object sender, ResolveEventArgs args)
            {
                return Assembly.ReflectionOnlyLoad(args.Name);
            }

            void AddAssembly(FileDescriptor assemblyFile, string namespaceReference)
            {
                List<FileDescriptor> sources;
                assemblyLock.ReadLock();
                try
                {
                    assemblies.TryGetValue(namespaceReference, out sources);
                }
                finally
                {
                    assemblyLock.ReadRelease();
                }
                if (sources == null)
                {
                    assemblyLock.WriteLock();
                    try
                    {
                        if (!assemblies.TryGetValue(namespaceReference, out sources))
                        {
                            sources = new List<FileDescriptor>();
                            assemblies.Add(namespaceReference, sources);
                        }
                    }
                    finally
                    {
                        assemblyLock.WriteRelease();
                    }
                }
                lock (sources)
                {
                    if(!sources.Contains(assemblyFile))
                        sources.Add(assemblyFile);
                }
            }
            void LoadNamespaces(Assembly assembly, HashSet<string> namespaces)
            {
                foreach (Type type in assembly.GetExportedTypes())
                    if (type.IsPublic)
                    {
                        string namespaceReference = type.Namespace;
                        if (!string.IsNullOrWhiteSpace(namespaceReference))
                        {
                            namespaces.Add(namespaceReference);
                        }
                    }
            }
        }

        private readonly static Type AssemblyLoaderType = typeof(FrameworkAssemblyLoader);

        private static async Task PopulateAssemblies(HashSet<string> assemblyList, HashSet<FileSystemDescriptor> referenceAssemblies)
        {
            string prefix; if ((Application.Platform & PlatformName.Windows) == PlatformName.Windows)
            {
                prefix = "v";
            }
            else prefix = string.Empty;
            #if net40
            string dirName = string.Concat(prefix, "4.0");
            #elif net45
            string dirName = string.Concat(prefix, "4.5");
            #elif net451
            string dirName = string.Concat(prefix, "4.5.1");
            #elif net452
            string dirName = string.Concat(prefix, "4.5.2");
            #elif net46
            string dirName = string.Concat(prefix, "4.6");
            #elif net461
            string dirName = string.Concat(prefix, "4.6.1");
            #elif net462
            string dirName = string.Concat(prefix, "4.6.2");
            #elif net47
            string dirName = string.Concat(prefix, "4.7");
            #elif net471
            string dirName = string.Concat(prefix, "4.7.1");
            #elif net472
            string dirName = string.Concat(prefix, "4.7.2");
            #elif net48
            string dirName = string.Concat(prefix, "4.8");
            #endif

            AppDomain assemblyDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
            using (FrameworkAssemblyLoader loader = (FrameworkAssemblyLoader)assemblyDomain.CreateInstanceAndUnwrap(AssemblyLoaderType.Assembly.FullName, AssemblyLoaderType.FullName))
            {
                assemblyDomain.ReflectionOnlyAssemblyResolve += loader.ResolveAssemblies;
                PathDescriptor assemblyPath; if (BuildParameter.ReferenceAssemblies.FindDirectory(dirName, out assemblyPath) && assemblyPath.FindFiles("*.dll", referenceAssemblies, PathSeekOptions.RootLevel | PathSeekOptions.Forward) > 0)
                {
                    await referenceAssemblies.ParallelFor(loader.LoadReferenceAssembly);
                }
                await assemblyList.ParallelFor(loader.LoadFrameworkAssembly);
                assemblies = loader.Assemblies;
            }
            AppDomain.Unload(assemblyDomain);
        }
    }
}
#endif