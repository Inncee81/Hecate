// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SE.Apollo.Package;
using SE.CommandLine;
using SE.Config;
using SE.Json;
using SE.Tar;

namespace SE.Hecate.Packages
{
    /// <summary>
    /// Pipeline node to perform an installation task to current project
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class InstallController : ProcessorUnit
    {
        public const string CachePrefix = "Packages";

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
            get { return (UInt32)ProcessorFamilies.Install; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public InstallController()
        { }

        private static async Task<int> Process(PackageTarget target)
        {
            PathDescriptor _unused; if (InstallParameter.Force || !PackageController.TryGetPackageLocation(target, out _unused))
            {
                HttpClient connection = null;
                try
                {
                    Application.Log(SeverityFlags.Full, "Fetching {0}", target.FriendlyName(false));

                    bool cachePackage = false;
                    Any<PackageMeta> package = await LoadFromCache(target);

                Fetch:
                    if (!package.HasValue || InstallParameter.Force)
                    {
                        #region Repository
                        cachePackage = true;
                        foreach (Repository repo in PackageManager.Repositories)
                        {
                            HttpClient client = repo.GetClient();
                            try
                            {
                                string name; if (repo.Prefixes.TryGetValue(target.Id.Owner, out name))
                                {
                                    name = new PackageId(name, target.Id).ToString();
                                    name = name.Replace("/", "%2F");
                                }
                                else name = target.Id.FriendlyName(false);
                                Any<JsonDocument> metaData = await client.GetPackageMeta(name, Application.LogSystem);
                                if (metaData.HasValue && FindTargetPackage(target.Version, metaData.Value))
                                {
                                    Application.Log(SeverityFlags.Full, "Fetched {0} from '{1}'", target.FriendlyName(false), repo.Address.Host);

                                    PackageMeta pkg = new PackageMeta();
                                    PropertyMapper.Assign(pkg, metaData.Value, true, true);
                                    if (pkg.Id.IsValid && pkg.Version.IsValid)
                                    {
                                        Application.Log(SeverityFlags.Full, "Parsed {0} meta data", pkg.FriendlyName(false));

                                        connection = client;
                                        package = pkg;
                                        client = null;
                                        break;
                                    }
                                }
                            }
                            finally
                            {
                                if (client != null)
                                    client.Dispose();
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region Cached
                        foreach (Repository repo in PackageManager.Repositories)
                            if (package.Value.Content.RemoteLocation.Host == repo.Address.Host)
                            {
                                connection = repo.GetClient();
                                break;
                            }

                        if (connection == null)
                        {
                            package = Any<PackageMeta>.Empty;
                            goto Fetch;
                        }
                        #endregion
                    }
                    if (package.HasValue)
                    {
                        if (!string.IsNullOrWhiteSpace(package.Value.License) && PackageManager.AcceptedLicenses.Contains(package.Value.License))
                        {
                            if (!await InstallFromCache(target, package.Value, cachePackage))
                                await InstallFromRepository(connection, target, package.Value);
                        }
                        else Application.Error(SeverityFlags.None, "Invalid license '{0}' in package {1}", package.Value.License, package.Value.FriendlyName(false));
                    }
                    else Application.Error(SeverityFlags.None, "Unable to locate package {0}", target);
                    return Application.GetReturnCode();
                }   
                finally
                {
                    if (connection != null)
                        connection.Dispose();
                }
            }
            else if(!target.IsDependency || Application.LogSeverity >= SeverityFlags.Full)
            {
                Application.Log(SeverityFlags.Minimal, "{0} is up to date", target.FriendlyName(false));
            }
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            PackageCommand install = (command as PackageCommand);
            if (install != null)
            {
                PropertyMapper.Assign<InstallParameter>(CommandLineOptions.Default, true, true);

                foreach (string package in install)
                {
                    PackageTarget target; if (!PackageTarget.TryParse(package, out target))
                    {
                        Application.Error(SeverityFlags.None, "Invalid package ID '{0}'", target);
                    }
                    else install.Attach(Process(target));
                }
                return true;
            }
            else return false;
        }

        private static bool FindTargetPackage(PackageVersion version, JsonDocument metaData)
        {
            if (version == 0)
            {
                string latest; if (metaData.TryGetValue("dist-tags", "latest", out latest))
                {
                    JsonNode node; if (metaData.TryGetValue<JsonNode>("versions", out node))
                    {
                        metaData.Rebase(node);
                    }
                    if (metaData.TryGetValue<JsonNode>(latest, out node))
                    {
                        metaData.Rebase(node);
                        return true;
                    }
                }
            }
            else if (version.IsCompatibilityVersion)
            {
                JsonNode node; if (metaData.TryGetValue<JsonNode>("versions", out node))
                {
                    metaData.Rebase(node);

                    node = node.Child;
                    while (node != null)
                    {
                        if (!string.IsNullOrWhiteSpace(node.Name))
                        {
                            PackageVersion ver = PackageVersion.Create(node.Name);
                            if (ver.Match(version))
                            {
                                metaData.Rebase(node);
                                return true;
                            }
                        }
                        node = node.Next;
                    }
                }
            }
            else
            {
                JsonNode node; if (metaData.TryGetValue<JsonNode>("versions", out node))
                {
                    metaData.Rebase(node);
                }
                if (metaData.TryGetValue<JsonNode>(version.ToString(), out node))
                {
                    metaData.Rebase(node);
                    return true;
                }
            }
            return false;
        }

        private static async Task<Any<PackageMeta>> LoadFromCache(PackageTarget target)
        {
            Application.Log(SeverityFlags.Full, "Fetching {0} from cache", target.FriendlyName(false));

            FileDescriptor packageFile = FileDescriptor.Create(Application.CacheDirectory.Combine(CachePrefix), string.Concat(target.FriendlyName(false), ".json"));
            using (NamedReadWriteLock packageLock = packageFile.GetLock())
            {
                await packageLock.ReadLockAsync();
                try
                {
                    if (packageFile.Exists())
                    {
                        PackageMeta pkg; if (packageFile.GetPackage(Application.LogSystem, out pkg) && pkg.Id.IsValid && pkg.Version.IsValid && pkg.Content != null)
                        {
                            Application.Log(SeverityFlags.Full, "Parsed {0} meta data", pkg.FriendlyName(false));
                            return pkg;
                        }
                    }
                }
                finally
                {
                    packageLock.ReadRelease();
                }
            }   
            return Any<PackageMeta>.Empty;
        }
        private static async Task<bool> InstallFromCache(PackageTarget target, PackageMeta package, bool cachePackage)
        {
            PathDescriptor cacheDirectory = Application.CacheDirectory.Combine(CachePrefix);
            FileDescriptor packageFile = FileDescriptor.Create(cacheDirectory, string.Concat(package.FriendlyName(false), ".json"));
            if (cachePackage)
            {
                Application.Log(SeverityFlags.Full, "Saving '{0}' to disk", packageFile.FullName);

                using (NamedReadWriteLock packageLock = packageFile.GetLock())
                {
                    await packageLock.WriteLockAsync();
                    try
                    {
                        if (!cacheDirectory.Exists())
                        {
                            try
                            {
                                cacheDirectory.Create();
                            }
                            catch { }
                        }
                        using (FileStream fs = packageFile.Open(FileMode.Create, FileAccess.Write))
                        {
                            package.Serialize(fs);
                        }
                    }
                    finally
                    {
                        packageLock.WriteRelease();
                    }
                }
            }
            packageFile = FileDescriptor.Create(cacheDirectory, string.Concat(package.FriendlyName(false), ".pkg"));
            using (NamedReadWriteLock packageLock = packageFile.GetLock())
            {
                Application.Log(SeverityFlags.Full, "Fetching '{0}' from cache", packageFile.FullName);

                await packageLock.ReadLockAsync();
                try
                {
                    if (packageFile.Exists())
                        return await Install(target, package, packageFile);
                }
                finally
                {
                    packageLock.ReadRelease();
                }
            }
            return false;
        }

        private static async Task<bool> InstallFromRepository(HttpClient connection, PackageTarget target, PackageMeta package)
        {
            FileDescriptor packageFile = FileDescriptor.Create(Application.CacheDirectory.Combine(CachePrefix), string.Concat(package.FriendlyName(false), ".pkg"));
            using (NamedReadWriteLock packageLock = packageFile.GetLock())
            {
                await packageLock.WriteLockAsync();
                try
                {
                    if (!packageFile.Exists() || (Application.StartupTime - packageFile.Timestamp).TotalSeconds > 0)
                    {
                        Application.Log(SeverityFlags.Full, "Fetching '{0}'", packageFile.FullName);

                        using (FileStream fs = packageFile.Open(FileMode.Create, FileAccess.Write))
                        {
                            Any<Stream> data = await connection.GetPackage(package.Content);
                            if (!data.HasValue)
                            {
                                Application.Error(SeverityFlags.None, "Failed to download '{0}'", package.Content.RemoteLocation.Host);
                                return false;
                            }
                            else data.Value.CopyTo(fs);
                        }
                    }
                }
                finally
                {
                    packageLock.WriteRelease();
                }
                await packageLock.ReadLockAsync();
                try
                {
                    if (packageFile.Exists())
                        return await Install(target, package, packageFile);
                }
                finally
                {
                    packageLock.ReadRelease();
                }
            }
            Application.Error(SeverityFlags.None, "{0} does not exist", packageFile.FullName);
            return false;
        }
        private static async Task<bool> Install(PackageTarget target, PackageMeta package, FileDescriptor packageContent)
        {
            PathDescriptor packageLocation = Application.SdkRoot;
            PackageManager.GetLocation(package, ref packageLocation);
            if (Application.ProjectRoot != Application.SdkRoot)
            {
                if (!packageLocation.Exists() || (packageLocation.Exists() && InstallParameter.Force))
                {
                    packageLocation = Application.ProjectRoot;
                    PackageManager.GetLocation(package, ref packageLocation);
                }
            }
            using (NamedSpinlock packageLock = packageLocation.GetExclsuiveLock())
            {
                await packageLock.LockAsync();
                try
                {
                    bool install = (!packageLocation.Exists() || InstallParameter.Force);
                    if (install && target.IsDependency)
                    {
                        FileDescriptor packageFile; if (packageLocation.FindFile("package.json", out packageFile, PathSeekOptions.RootLevel))
                        {
                            #region Package File
                            PackageMeta pkg; if (packageFile.GetPackage(Application.LogSystem, out pkg) && target.Id.Equals(pkg.Id) && (target.Version == 0 || target.Version.Match(pkg.Version)))
                            {
                                package = pkg;
                                install = false;
                            }
                            #endregion
                        }
                    }
                    if (install)
                    {
                        Application.Log(SeverityFlags.Full, "Installing {0}", package.FriendlyName(false));

                        #region Package Integrity
                        if (!string.IsNullOrWhiteSpace(package.Content.Checksum))
                        {
                            using (FileStream fs = packageContent.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                            using (SHA1Managed sha1 = new SHA1Managed())
                            {
                                byte[] hash = sha1.ComputeHash(fs);
                                StringBuilder formatted = new StringBuilder(2 * hash.Length);
                                foreach (byte b in hash)
                                {
                                    formatted.AppendFormat("{0:x2}", b);
                                }
                                if (!formatted.IsEqual(package.Content.Checksum))
                                {
                                    Application.Error(SeverityFlags.None, "{0} invalid checksum", package.FriendlyName(false));
                                    return false;
                                }
                            }
                        }
                        #endregion

                        #region Prepare
                        if (!packageLocation.Exists())
                        {
                            try
                            {
                                packageLocation.Create();
                            }
                            catch (Exception er)
                            {
                                Application.Error(er);
                            }
                        }
                        else
                        {
                            try
                            {
                                packageLocation.Clear();
                            }
                            catch (Exception er)
                            {
                                Application.Error(er);
                                return false;
                            }
                        }
                        #endregion

                        #region Unpack
                        try
                        {
                            using (FileStream content = packageContent.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                            using (GZipStream gzs = new GZipStream(content, CompressionMode.Decompress))
                            using (TarInputStream tar = new TarInputStream(gzs))
                            {
                                foreach (TarEncoding.Entry item in tar.Entries)
                                    if (!string.IsNullOrWhiteSpace(item.Name))
                                    {
                                        string name; if (item.Name.StartsWith("package/"))
                                        {
                                            name = item.Name.Substring(8);
                                        }
                                        else name = item.Name;
                                        if (string.IsNullOrWhiteSpace(name))
                                        {
                                            continue;
                                        }
                                        switch (item.Type)
                                        {
                                            case TarEntryType.Directory:
                                                {
                                                    PathDescriptor t = packageLocation.Combine(name);
                                                    if (!t.Exists())
                                                        t.Create();
                                                }
                                                break;
                                            case TarEntryType.File:
                                                {
                                                    FileDescriptor t = FileDescriptor.Create(packageLocation, name);
                                                    if (!t.Location.Exists())
                                                    {
                                                        t.Location.Create();
                                                    }
                                                    using (FileStream fs = t.Open(FileMode.Create, FileAccess.Write))
                                                        tar.CopyRange(fs, (int)item.Size);
                                                }
                                                break;
                                        }
                                    }
                            }
                        }
                        catch (Exception er)
                        {
                            Application.Error(er);
                            return false;
                        }
                        #endregion

                        Application.Log(SeverityFlags.Minimal, "Installed {0}", package.FriendlyName(false));
                    }
                    else Application.Log(target.IsDependency ? SeverityFlags.Full : SeverityFlags.Minimal, "{0} is up to date", package.FriendlyName(false));

                    #region Dependencies
                    Application.Log(SeverityFlags.Full, "Checking {0} dependencies", package.FriendlyName(false));

                    if (package.Dependencies.Count > 0)
                    {
                        List<Task> tasks = CollectionPool<List<Task>, Task>.Get();
                        try
                        {
                            foreach (PackageTarget dependency in package.Dependencies.ConvertTo(x => new PackageTarget(x.Key, x.Value, true)))
                            {
                                tasks.Add(Process(dependency));
                            }
                            await Taskʾ.WhenAll(tasks);
                        }
                        finally
                        {
                            CollectionPool<List<Task>, Task>.Return(tasks);
                        }
                    }
                    #endregion
                }
                finally
                {
                    packageLock.Release();
                }
            }
            return true;
        }
    }
}
