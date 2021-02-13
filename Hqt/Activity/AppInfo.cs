// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using SE.Config;
using SE.Flex;

namespace SE.Hecate
{
    /// <summary>
    /// Provides basic usage manual information to the user
    /// </summary>
    public class AppInfo : KernelMessage
    {
        Dictionary<string, PropertyPage> pages;

        /// <summary>
        /// Create a new class instance
        /// </summary>
        public AppInfo()
            : base(TemplateId.Create() | (UInt32)ProcessorFamilies.InfoProvider, Application.ProjectRoot)
        {
            this.pages = new Dictionary<string, PropertyPage>();
        }

        /// <summary>
        /// Provides a PropertyPage to fill with parameter/description pairs. If the page doesn't
        /// exist, it will be created
        /// </summary>
        /// <param name="key">A string to provide an identification to the requested page</param>
        /// <returns>A page instance to fill</returns>
        public PropertyPage GetPage(string key)
        {
            PropertyPage page;
            lock (pages)
            {
                if (!pages.TryGetValue(key, out page))
                {
                    page = new PropertyPage(PropertyPageFlags.DashNotation | PropertyPageFlags.HarmonizeFlags);
                    page.Separator = ": ";
                    pages.Add(key, page);
                }
            }
            return page;
        }

        /// <summary>
        /// Prints the entire manual to the connected log channel
        /// </summary>
        public void Print()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Application.Self.Name);
            sb.Append(" ");
            sb.Append(Application.Version);
            if (Application.TargetVersion != VersionFlags.Undefined)
            {
                sb.Append(" .");
                sb.Append
                (
                    Application.TargetVersion.GetVersion()
                                             .ToString()
                                             .Replace("_", string.Empty)
                                             .ToLowerInvariant()
                );
            }
            sb.Append(", Copyright (C) 2017 Schroedinger Entertainment");
            Application.Log(SeverityFlags.None, sb.ToString());
            PropertyPage main = GetPage(string.Empty);
            if (main.Rows.Count > 0)
            {
                PropertyPageKeyValueRow row = new PropertyPageKeyValueRow(string.Empty, PropertyType.Optional);
                row.Keys.Add(string.Concat(main.MakeKey("?"), ", ", main.MakeKey("help")));
                main.Rows.Add(row);
                main.Sort();

                sb.Clear();
                sb.Append(main.GetUsageLine());
                Application.Log(SeverityFlags.None, sb.ToString());

                Application.Log(SeverityFlags.None, string.Empty);
                Application.Log(SeverityFlags.None, main.ToString(Console.BufferWidth));
            }
            IEnumerable<KeyValuePair<string, PropertyPage>> tmp = pages.OrderBy(x => x.Key);
            foreach (KeyValuePair<string, PropertyPage> page in tmp.Where(x => x.Key.EndsWith(" command", StringComparison.InvariantCultureIgnoreCase)))
            {
                PropertyMapper.GetPropertyDescriptions<App.Settings>(page.Value, true, true);
                page.Value.Sort();

                string buffer = page.Value.ToString(Console.BufferWidth);
                if (!string.IsNullOrWhiteSpace(buffer))
                {
                    Application.Log(SeverityFlags.None, string.Empty);
                    sb.Clear();
                    sb.Append("^ ");
                    sb.Append(page.Key.Substring(0, page.Key.Length - 8).ToLower());
                    sb.Append(" ");
                    sb.Append(page.Value.GetUsageLine());
                    Application.Log(SeverityFlags.None, sb.Replace("{", "{{").Replace("}", "}}").ToString());
                    Application.Log(SeverityFlags.None, string.Empty);
                    Application.Log(SeverityFlags.None, buffer);
                }
            }
            foreach (KeyValuePair<string, PropertyPage> page in tmp.Where(x => (!string.IsNullOrEmpty(x.Key) && !x.Key.EndsWith(" command", StringComparison.InvariantCultureIgnoreCase))))
            {
                page.Value.Sort();

                string buffer = page.Value.ToString(Console.BufferWidth);
                if (!string.IsNullOrWhiteSpace(buffer))
                {
                    Application.Log(SeverityFlags.None, string.Empty);
                    sb.Clear();
                    sb.Append("[");
                    sb.Append(page.Key);
                    sb.Append("]");
                    Application.Log(SeverityFlags.None, sb.ToString());
                    Application.Log(SeverityFlags.None, buffer);
                }
            }
        }
    }
}
