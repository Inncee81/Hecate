// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Provides access to known CSharp assemblies
    /// </summary>
    public static partial class AssemblyCache
    {
        private readonly static Type CacheType = typeof(Dictionary<string, List<FileDescriptor>>);
        private readonly static Task CacheInitialization;

        private static Dictionary<string, List<FileDescriptor>> assemblies;
        private static ReadWriteLock assemblyLock;

        static AssemblyCache()
        {
            assemblyLock = new ReadWriteLock();
            Ctor();

            CacheInitialization = Initialize();
        }
        static partial void Ctor();

        private static async Task Initialize()
        {
            FileDescriptor cacheFile; if(!LoadCache(out cacheFile))
            {
                await BuildCache();
                try
                {
                    if (!cacheFile.Location.Exists())
                         cacheFile.Location.Create();
                }
                catch { }
                try
                {
                    using (FileStream fs = cacheFile.Open(FileMode.Create, FileAccess.Write))
                        TypeFormatter.Serialize(fs, assemblies);
                }
                catch (Exception er)
                {
                    Application.Warning(SeverityFlags.None, "Unable to create '{0}'\n{1}", cacheFile.FullName, er.Message);
                }
            }
        }

        private static bool LoadCache(out FileDescriptor cacheFile)
        {
            cacheFile = new FileDescriptor(Application.CacheDirectory, "Sharp.asmref");
            if (!Build.BuildParameter.Rebuild && cacheFile.Exists())
            {
                try
                {
                    using (FileStream fs = cacheFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        assemblies = (TypeFormatter.Deserialize(fs, -1, CacheType) as Dictionary<string, List<FileDescriptor>>);
                        InitializeFromCache();
                    }
                    return true;
                }
                catch (Exception er)
                {
                    Application.Warning(SeverityFlags.None, "Unable to read '{0}'\n{1}", cacheFile.FullName, er.Message);
                }
            }
            return false;
        }
        static partial void InitializeFromCache();
        private static async Task BuildCache()
        {
            HashSet<string> assemblyList = CollectionPool<HashSet<string>, string>.Get();
            HashSet<FileSystemDescriptor> referenceAssemblies = CollectionPool<HashSet<FileSystemDescriptor>, FileSystemDescriptor>.Get();
            try
            {
                PopulateFrameworkAssemblyList(assemblyList);
                await PopulateAssemblies(assemblyList, referenceAssemblies);
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
        public static async Task GetAssemblies(SharpModuleSettings conf, string namespaceName)
        {
            await CacheInitialization;
            AddDefaultAssemblies(conf);

            List<FileDescriptor> sources;
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
        static partial void AddDefaultAssemblies(SharpModuleSettings conf);
    }
}