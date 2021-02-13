// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

#if NET_FRAMEWORK
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Provides access to known CSharp assemblies
    /// </summary>
    public static partial class AssemblyCache
    {
        /// <summary>
        /// Loads CSharp Framework assemblies in an isolated location
        /// </summary>
        public class FrameworkAssemblyLoader : AppDomainʾ.ReferenceObject
        {
            ReadWriteLock assemblyLock;
            readonly Dictionary<string, HashSet<FileDescriptor>> assemblies;
            /// <summary>
            /// A collection of assemblies detected
            /// </summary>
            public Dictionary<string, HashSet<FileDescriptor>> Assemblies
            {
                get { return assemblies; }
            }
            
            /// <summary>
            /// Creates a new worker instance
            /// </summary>
            public FrameworkAssemblyLoader()
            {
                assemblies = new Dictionary<string, HashSet<FileDescriptor>>();
            }

            /// <summary>
            /// Tries to inegrate the provided file into the assembly list
            /// </summary>
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
            public Assembly ResolveAssemblies(object sender, ResolveEventArgs args)
            {
                return Assembly.ReflectionOnlyLoad(args.Name);
            }

            void AddAssembly(FileDescriptor assemblyFile, string namespaceReference)
            {
                HashSet<FileDescriptor> sources;
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
                            sources = new HashSet<FileDescriptor>();
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

            protected override void Dispose(bool disposing)
            {
                RemotingServices.Disconnect(this);
            }
        }

        private readonly static Type AssemblyLoaderType = typeof(FrameworkAssemblyLoader);
        /// <summary>
        /// A collection of assemblies located in current .Net Framework version
        /// </summary>
        public readonly static PathDescriptor ReferenceAssemblies = new PathDescriptor(ReferenceAssemblyPath);
        /// <summary>
        /// A collection of assemblies located in current .Net Framework version
        /// </summary>
        public readonly static PathDescriptor ReferenceAssembliesAlternative = new PathDescriptor(ReferenceAssembliesAlternativePath);

        private static Dictionary<string, HashSet<FileDescriptor>> assemblies;
        private static ReadWriteLock assemblyLock;

        static AssemblyCache()
        {
            assemblyLock = new ReadWriteLock();
            if (!LoadCache())
            {
                Compute();
            }
        }
        private static bool LoadCache()
        {
            //TODO read cache file
            return false;
        }
        private static void Compute()
        {
            HashSet<string> assemblyList = CollectionPool<HashSet<string>, string>.Get();
            HashSet<FileSystemDescriptor> referenceAssemblies = CollectionPool<HashSet<FileSystemDescriptor>, FileSystemDescriptor>.Get();
            try
            {
                PopulateFrameworkAssemblyList(assemblyList);
                AppDomain assemblyDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString());
                using (FrameworkAssemblyLoader loader = (FrameworkAssemblyLoader)assemblyDomain.CreateInstanceAndUnwrap(AssemblyLoaderType.Assembly.FullName, AssemblyLoaderType.FullName))
                {
                    assemblyDomain.ReflectionOnlyAssemblyResolve += loader.ResolveAssemblies;
                    if (ReferenceAssemblies.FindFiles("*.dll", referenceAssemblies, PathSeekOptions.RootLevel | PathSeekOptions.Forward) > 0)
                    {
                        referenceAssemblies.ForEach(loader.LoadReferenceAssembly);
                    }
                    else if (ReferenceAssembliesAlternative.FindFiles("*.dll", referenceAssemblies, PathSeekOptions.RootLevel | PathSeekOptions.Forward) > 0)
                    {
                        referenceAssemblies.ForEach(loader.LoadReferenceAssembly);
                    }
                    assemblyList.ForEach(loader.LoadFrameworkAssembly);
                    assemblies = loader.Assemblies;
                }
                AppDomain.Unload(assemblyDomain);
            }
            finally
            {
                CollectionPool<HashSet<FileSystemDescriptor>, FileSystemDescriptor>.Return(referenceAssemblies);
                CollectionPool<HashSet<string>, string>.Return(assemblyList);
            }
        }

        /// <summary>
        /// Resolves the provided namespace to the collection of assemblies
        /// </summary>
        /// <param name="conf">The set of extended options related to a CSharp code module component assemblies
        /// should be referenced from</param>
        /// <param name="namespaceName">The full qualified namespace</param>
        public static void GetAssemblies(SharpModuleSettings conf, string namespaceName)
        {
            HashSet<FileDescriptor> sources;
            assemblyLock.ReadLock();
            try
            {
                if (!assemblies.TryGetValue(namespaceName, out sources))
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
    }
}
#endif