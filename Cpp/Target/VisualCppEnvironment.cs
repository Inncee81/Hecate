// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

#if _WIN32
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SE.Hecate.Cpp
{
    /// <summary>
    /// Utility class for Visual C++ on Microsoft Windows
    /// </summary>
    public static class VisualCppEnvironment
    {
        private readonly static Filter Sdk10Filter;
        private readonly static Filter Sdk8Filter;

        static VisualCppEnvironment()
        {
            Sdk10Filter = new Filter();
            FilterToken root = Sdk10Filter.Add("...");
            root = Sdk10Filter.Add(root, "um");
            Sdk10Filter.Add(root, "windows.h");
            Sdk10Filter.Add(root, "winsdkver.h");

            Sdk8Filter = new Filter();
            Sdk8Filter.Add("windows.h");
            Sdk8Filter.Add("winsdkver.h");
        }

        /// <summary>
        /// Tries to obtain the path to the latest Windows SDK installed
        /// </summary>
        public static PathDescriptor FindLatestWindowsSdk(out Version latest)
        {
            PathDescriptor result = null;
            latest = null;

            foreach (string version in WindowsEnvironment.EnumerateRegistryKeys("Microsoft\\Microsoft SDKs\\Windows"))
            {
                string versionString; if (WindowsEnvironment.TryGetRegistryKey(string.Format(string.Concat("Microsoft\\Microsoft SDKs\\Windows", "\\{0}"), version), "ProductVersion", out versionString))
                {
                    Version versionNumber; if (Version.TryParse(versionString, out versionNumber) && (latest == null || latest < versionNumber))
                    {
                        string path; if (WindowsEnvironment.TryGetRegistryKey(string.Format("Microsoft\\Microsoft SDKs\\Windows\\{0}", version), "InstallationFolder", out path))
                        {
                            result = new PathDescriptor(Path.GetFullPath(Path.Combine(path, "include")));
                            if (versionNumber.Major >= 10)
                            {
                                if (result.FindDirectory(string.Concat(versionNumber.ToString(), "*"), out result))
                                {
                                    FileDescriptor tmp; if (!result.FindFile(Sdk10Filter, out tmp))
                                        goto Failure;

                                    if (Version.TryParse(result.Name, out versionNumber))
                                    {
                                        result = result.Parent.Parent;
                                        latest = versionNumber;
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                FileDescriptor tmp; if (!result.FindFile(Sdk8Filter, out tmp))
                                    goto Failure;

                                result = result.Parent;
                                latest = versionNumber;
                                continue;
                            }

                        Failure:
                            result = null;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Tries to obtain the path to the latest Windows Kit installed
        /// </summary>
        public static PathDescriptor FindLatestWindowsKit(out Version latest)
        {
            latest = null;
            string path; if (WindowsEnvironment.TryGetRegistryKey("Microsoft\\Windows Kits\\Installed Roots", "KitsRoot10", out path))
            {
                foreach (string kit in WindowsEnvironment.EnumerateRegistryKeys("Microsoft\\Windows Kits\\Installed Roots"))
                {
                    Version versionNumber = Version.Parse(kit);
                    if (latest == null || latest < versionNumber)
                        latest = versionNumber;
                }
                return new PathDescriptor(path).Combine("Include", latest.ToString());
            }
            else return null;
        }
    }
}
#endif