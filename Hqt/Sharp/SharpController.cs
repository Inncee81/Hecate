// Copyright (C) 2017 Schroedinger Entertainment
// Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;
using SE.Parsing;
using SE.SharpLang;

namespace SE.Hecate.Sharp
{
    /// <summary>
    /// Pipeline node to perform CSharp code lookup tasks
    /// </summary>
    [ProcessorUnit(IsBuiltIn = true)]
    public class SharpController : ProcessorUnit
    {
        public override PathDescriptor Target
        {
            get { return Application.SdkRoot; }
        }
        public override bool Enabled
        {
            get { return true; }
        }
        public override UInt32 Family
        {
            get { return (UInt32)ProcessorFamilies.SharpInitialize; }
        }

        /// <summary>
        /// Creates a new node instance
        /// </summary>
        public SharpController()
        { }

        private static int Process(SharpModule module, FileDescriptor file)
        {
            Stopwatch timer = new Stopwatch();
            Linter linter = new Linter();

            #region Rules
            NamespaceRule namespaceRule = ParserRulePool<NamespaceRule, SharpToken>.Get();
            namespaceRule.Linter = linter;

            UsingRule usingRule = ParserRulePool<UsingRule, SharpToken>.Get();
            usingRule.Linter = linter;

            MainRule mainRule = ParserRulePool<MainRule, SharpToken>.Get();
            mainRule.Linter = linter;
            #endregion

            try
            {
                linter.AddRule(namespaceRule);
                linter.AddRule(usingRule);
                linter.AddRule(mainRule);

                #region Lint
                using (FileStream fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    //TODO cache lookup

                    timer.Start();
                    foreach (SharpModuleSettings config in module.Settings.Values)
                    {
                        fs.Position = 0;
                        linter.Defines.Clear();

                        #region Rules
                        namespaceRule.Settings = config;
                        usingRule.Settings = config;
                        mainRule.Settings = config;
                        #endregion

                        foreach (string define in config.Defines)
                        {
                            linter.Define(define);
                        }
                        if (!Lint(linter, fs, file.FullName))
                            return Application.FailureReturnCode;
                    }
                    timer.Stop();
                }
                #endregion
            }
            finally
            {
                #region Rules
                ParserRulePool<MainRule, SharpToken>.Return(mainRule);
                ParserRulePool<UsingRule, SharpToken>.Return(usingRule);
                ParserRulePool<NamespaceRule, SharpToken>.Return(namespaceRule);
                #endregion
            }

            Application.Log(SeverityFlags.Full, "Linting {0} in {1}ms", file.FullName, timer.ElapsedMilliseconds);
            return Application.SuccessReturnCode;
        }
        public override bool Process(KernelMessage command)
        {
            PreprocessCommand sharp = (command as PreprocessCommand);
            if (sharp != null)
            {
                SharpModule module = null;
                foreach (FileDescriptor file in sharp)
                {
                    if (file.Extension.Equals("cs", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (module == null)
                        {
                            module = new SharpModule(sharp.Profile);
                            sharp.SetProperty(module);
                        }
                        sharp.Attach(Taskʾ.Run<int>(() => Process(module, file)));
                        module.Files.Add(file);
                    }
                }
                return true;
            }
            else return false;
        }

        private static bool Lint(Linter linter, FileStream stream, string file)
        {
            #if net40
            linter.Define("net40");
            linter.Define("NET40");
            linter.Define("NET_4_0");
            linter.Define("NET_FRAMEWORK");
            #elif net45
            linter.Define("net45");
            linter.Define("NET45");
            linter.Define("NET_4_5");
            linter.Define("NET_FRAMEWORK");
            #elif net451
            linter.Define("net451");
            linter.Define("NET451");
            linter.Define("NET_4_5_1");
            linter.Define("NET_FRAMEWORK");
            #elif net452
            linter.Define("net452");
            linter.Define("NET452");
            linter.Define("NET_4_5_2");
            linter.Define("NET_FRAMEWORK");
            #elif net46
            linter.Define("net46");
            linter.Define("NET46");
            linter.Define("NET_4_6");
            linter.Define("NET_FRAMEWORK");
            #elif net461
            linter.Define("net461");
            linter.Define("NET461");
            linter.Define("NET_4_6_1");
            linter.Define("NET_FRAMEWORK");
            #elif net462
            linter.Define("net462");
            linter.Define("NET462");
            linter.Define("NET_4_6_2");
            linter.Define("NET_FRAMEWORK");
            #elif net47
            linter.Define("net47");
            linter.Define("NET47");
            linter.Define("NET_4_7");
            linter.Define("NET_FRAMEWORK");
            #elif net471
            linter.Define("net471");
            linter.Define("NET471");
            linter.Define("NET_4_7_1");
            linter.Define("NET_FRAMEWORK");
            #elif net472
            linter.Define("net472");
            linter.Define("NET472");
            linter.Define("NET_4_7_2");
            linter.Define("NET_FRAMEWORK");
            #elif net48
            linter.Define("net48");
            linter.Define("NET48");
            linter.Define("NET_4_8");
            linter.Define("NET_FRAMEWORK");
            #elif net50
            linter.Define("net50");
            linter.Define("NET50");
            linter.Define("NET_5_0");
            linter.Define("NET_CORE");
            #endif

            if (linter.Parse(stream, false, file))
            {
                foreach (string error in linter.Errors)
                {
                    Application.Warning(SeverityFlags.None, error);
                }
                return true;
            }
            else
            {
                foreach (string error in linter.Errors)
                {
                    Application.Error(SeverityFlags.None, error);
                }
                return false;
            }
        }
    }
}
