// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Runtime;
using SE.App;
using SE.CommandLine;
using SE.Config;

namespace SE.Hecate
{
    /// <summary>
    /// An activity to run this program locally
    /// </summary>
    public partial class LocalActivity : Activity
    {
        /// <summary>
        /// Creates a new activity instance
        /// </summary>
        public LocalActivity()
        { }

        /// <summary>
        /// Main (default)
        /// </summary>
        public override int Run(string[] args)
        {
            try
            {
                Kernel.Load();
                if (Settings.DisplayInfo)
                {
                    using (AppInfo info = new AppInfo())
                    {
                        bool exit; if (Kernel.Dispatch(info, out exit))
                        {
                            info.Await();
                            info.Print();
                        }
                        else if (exit)
                        {
                            return Application.FailureReturnCode;
                        }
                    }
                }
                else using (LocalEntryPoint entryPoint = new LocalEntryPoint(Application.WorkerPath))
                {
                    PropertyMapper.Assign(entryPoint, CommandLineOptions.Default, true);
                    bool exit; if (!Kernel.Dispatch(entryPoint, out exit))
                    {
                        if (!exit && entryPoint.MakeDefault())
                        {
                            if (Kernel.Dispatch(entryPoint, out exit))
                                return entryPoint.Await();
                        }
                        Application.Error(SeverityFlags.None, "Failed to process command");
                    }
                    else return entryPoint.Await();
                }
                return Application.GetReturnCode();
            }
            #if !DEBUG
            catch (Exception er)
            {
                Application.Error(er);
                return Application.FailureReturnCode;
            }
            #endif
            finally
            { }
        }
    }
}