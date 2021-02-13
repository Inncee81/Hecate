// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime;
using SE.CommandLine;
using SE.Config;

namespace SE.App
{
    ///<summary>
    ///Heqet (Egyptian hqt, also hqtyt "Heqtit") is an Egyptian goddess of fertility, identified with Hathor, represented in the form of a frog.
    ///To the Egyptians, the frog was an ancient symbol of fertility, related to the annual flooding of the Nile. Heqet was originally the female
    ///counterpart of Khnum, or the wife of Khnum by whom she became the mother of Her-ur. It has been proposed that her name is the origin of
    ///the name of Hecate, the Greek goddess of witchcraft
    ///</summary>
    ///<remarks>https://en.wikipedia.org/wiki/Heqet</remarks>
    public partial class Program
    {
        private static Activity GetStartupActivity(string[] args)
        {
            try
            {
                if (!Application.CacheDirectory.Parent.Exists())
                {
                    Application.CacheDirectory.Parent.CreateHidden();
                }
            }
            catch { }
            try
            {
                if (!Application.CacheDirectory.Exists())
                {
                    Application.CacheDirectory.Create();
                }
            }
            catch { }
            try
            {
                CommandLineOptions.Default.Flags |= CommandLineFlags.IgnoreCase;
                CommandLineOptions.Default.Load(args);

                using (PropertyMapperResult result = PropertyMapper.Assign<Settings>(CommandLineOptions.Default, true))
                {
                    foreach (Exception er in result.Errors)
                    {
                        Application.Error(er);
                    }
                    Activity activity = new Hecate.LocalActivity();
                    return activity;
                }
            }
            catch (Exception er)
            {
                Application.Error(er);
            }
            return null;
        }
    }
}