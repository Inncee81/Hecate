// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

#if !NET_FRAMEWORK
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace SE.Hecate.Sharp
{
    public static partial class AssemblyCache
    {
        class AssemblyReflectContext : AssemblyLoadContext
        {
            AssemblyDependencyResolver resolver;

            public AssemblyReflectContext()
                : base(Guid.NewGuid().ToString(), true)
            {
                this.resolver = new AssemblyDependencyResolver(Assembly.GetExecutingAssembly().Location);
            }

            [MethodImpl(OptimizationExtensions.ForceInline)]
            public void LoadReferenceAssembly(FileSystemDescriptor assemblyFile)
            {
                try
                {
                    LoadFromAssemblyPath(assemblyFile.GetAbsolutePath());
                }
                catch (FileNotFoundException)
                { }
                catch (FileLoadException)
                { }
                catch (BadImageFormatException)
                { }
            }

            [MethodImpl(OptimizationExtensions.ForceInline)]
            public void LoadFrameworkAssembly(string assemblyIdentifier)
            {
                LoadAssembly(assemblyIdentifier, true);
            }

            [MethodImpl(OptimizationExtensions.ForceInline)]
            public void LoadAssembly(string assemblyIdentifier)
            {
                LoadAssembly(assemblyIdentifier, false);
            }
            void LoadAssembly(string assemblyIdentifier, bool asDefault)
            {
                try
                {
                    HashSet<string> namespaces = CollectionPool<HashSet<string>, string>.Get();
                    try
                    {
                        Assembly assembly = LoadFromAssemblyName(new AssemblyName(assemblyIdentifier));
                        if (!asDefault)
                        {
                            LoadNamespaces(assembly, namespaces);
                            foreach (string namespaceReference in namespaces)
                            {
                                AddAssembly(FileDescriptor.Create(new Uri(assembly.Location).LocalPath), namespaceReference);
                            }
                        }
                        else AddAssembly(FileDescriptor.Create(new Uri(assembly.Location).LocalPath), string.Empty);
                    }
                    finally
                    {
                        CollectionPool<HashSet<string>, string>.Return(namespaces);
                    }
                }
                catch (FileNotFoundException)
                { }
                catch (FileLoadException)
                { }
                catch (TypeLoadException)
                { }
                catch (BadImageFormatException)
                { }
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
                    if (!sources.Contains(assemblyFile))
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

            protected override Assembly Load(AssemblyName name)
            {
                foreach (Assembly assembly in AssemblyLoadContext.Default.Assemblies)
                {
                    if (assembly.GetName().Equals(name))
                        return assembly;
                }
                string location = resolver.ResolveAssemblyToPath(name);
                if (!string.IsNullOrWhiteSpace(location))
                {
                    return LoadFromAssemblyPath(location);
                }
                else return null;
            }
        }
        private static int defaultAssemblies;

        [MethodImpl(OptimizationExtensions.ForceInline)]
        static partial void InitializeFromCache()
        {
            List<FileDescriptor> sources; if (assemblies.TryGetValue(string.Empty, out sources))
                defaultAssemblies = sources.Count;
        }

        private static async Task PopulateAssemblies(HashSet<string> assemblyList, HashSet<FileSystemDescriptor> referenceAssemblies)
        {
            assemblies = new Dictionary<string, List<FileDescriptor>>();
            AssemblyReflectContext ctx = new AssemblyReflectContext();
            ctx.EnterContextualReflection();

            PathDescriptor assemblyPath = FindLatest(BuildParameter.Dotnet, "Microsoft.NETCore.App/5.*");
            if(assemblyPath != null)
            {
                if (assemblyPath.FindFiles("*.dll", referenceAssemblies, PathSeekOptions.RootLevel | PathSeekOptions.Forward) > 0)
                {
                    await referenceAssemblies.ParallelFor(ctx.LoadReferenceAssembly);
                }
                await assemblyList.ParallelFor(ctx.LoadFrameworkAssembly);
                List<FileDescriptor> sources; if (assemblies.TryGetValue(string.Empty, out sources))
                    defaultAssemblies = sources.Count;
            }
            assemblyPath = FindLatest(BuildParameter.Dotnet, "Microsoft.WindowsDesktop.App/5.*");
            if (assemblyPath != null)
            {
                referenceAssemblies.Clear();
                assemblyList.Clear();

                PopulateWindowsAssemblyList(assemblyList);
                if (assemblyPath.FindFiles("*.dll", referenceAssemblies, PathSeekOptions.RootLevel | PathSeekOptions.Forward) > 0)
                {
                    await referenceAssemblies.ParallelFor(ctx.LoadReferenceAssembly);
                }
                await assemblyList.ParallelFor(ctx.LoadAssembly);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            ctx.Unload();
        }

        static partial void AddDefaultAssemblies(SharpModuleSettings conf)
        {
            lock (conf.References)
            {
                if (conf.References.Count >= defaultAssemblies)
                    return;
            }
            List<FileDescriptor> sources;
            assemblyLock.ReadLock();
            try
            {
                if (!assemblies.TryGetValue(string.Empty, out sources))
                    return;
            }
            finally
            {
                assemblyLock.ReadRelease();
            }
            lock (sources)
            {
                foreach (FileDescriptor assembly in sources)
                    if (assembly.Exists())
                    {
                        lock(conf.References)
                             conf.References.Add(assembly);
                    }
            }
        }

        static PathDescriptor FindLatest(PathDescriptor root, string filter)
        {
            PathDescriptor latest = null;
            List<FileSystemDescriptor> sdkPaths = CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Get();
            try
            {
                Version latestVersion = new Version();
                if (root.FindDirectories(filter, sdkPaths) != 0)
                    foreach (PathDescriptor path in sdkPaths)
                    {
                        Version ver; if (Version.TryParse(path.Name, out ver) && ver > latestVersion)
                        {
                            latestVersion = ver;
                            latest = path;
                        }
                    }
            }
            finally
            {
                CollectionPool<List<FileSystemDescriptor>, FileSystemDescriptor>.Return(sdkPaths);
            }
            return latest;
        }
    }
}
#endif