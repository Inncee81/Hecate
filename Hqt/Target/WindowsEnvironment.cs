// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

#if _WIN32
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace SE.Hecate
{
    /// <summary>
    /// Utility class for Microsoft Windows
    /// </summary>
    public static class WindowsEnvironment
    {
        /// <summary>
        /// Iterates any value found in the provided registry key
        /// </summary>
        public static string[] EnumerateRegistryKeys(string entry)
        {
            string[] tmp = new string[0];
            HashSet<string> result = CollectionPool<HashSet<string>, string>.Get();
            try
            {
                if (EnumerateRegistryKeys(Registry.CurrentUser, "SOFTWARE\\" + entry, ref tmp))
                {
                    foreach (string item in tmp)
                        result.Add(item);
                }
                if (EnumerateRegistryKeys(Registry.LocalMachine, "SOFTWARE\\" + entry, ref tmp))
                {
                    foreach (string item in tmp)
                        result.Add(item);
                }
                if (EnumerateRegistryKeys(Registry.CurrentUser, "SOFTWARE\\Wow6432Node\\" + entry, ref tmp))
                {
                    foreach (string item in tmp)
                        result.Add(item);
                }
                if (EnumerateRegistryKeys(Registry.LocalMachine, "SOFTWARE\\Wow6432Node\\" + entry, ref tmp))
                {
                    foreach (string item in tmp)
                        result.Add(item);
                }
                return result.ToArray();
            }
            finally
            {
                CollectionPool<HashSet<string>, string>.Return(result);
            }
        }
        private static bool EnumerateRegistryKeys(RegistryKey @base, string entry, ref string[] values)
        {
            RegistryKey key = @base.OpenSubKey(entry, false);
            if (key != null)
            {
                values = key.GetSubKeyNames();
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Tries to find a value in the provided registry key
        /// </summary>
        public static bool TryGetRegistryKey(string entry, string key, out string value)
        {
            if (TryGetRegistryKeyValue("HKEY_CURRENT_USER\\SOFTWARE\\" + entry, key, out value))
                return true;
            else if (TryGetRegistryKeyValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\" + entry, key, out value))
                return true;
            else if (TryGetRegistryKeyValue("HKEY_CURRENT_USER\\SOFTWARE\\Wow6432Node\\" + entry, key, out value))
                return true;
            else if (TryGetRegistryKeyValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\" + entry, key, out value))
                return true;
            else
                return false;
        }
        private static bool TryGetRegistryKeyValue(string entry, string key, out string value)
        {
            value = (Registry.GetValue(entry, key, null) as string);
            if (!string.IsNullOrEmpty(value)) return true;
            else return false;
        }
    }
}
#endif