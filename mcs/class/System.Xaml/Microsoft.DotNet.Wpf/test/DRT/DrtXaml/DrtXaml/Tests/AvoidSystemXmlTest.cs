using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DRT;
using System.Xaml;
using DrtXaml.XamlTestFramework;
using System.Xml;
using System.IO;
using System.Security.Permissions;
using System.Reflection;
using Test.Elements;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;

namespace DrtXaml.Tests
{
    [TestClass]
    [Serializable]
    sealed class AvoidSystemXmlTest : XamlTestSuite
    {
        public AvoidSystemXmlTest()
            : base("AvoidSystemXmlTest")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        // =============================================

        [TestMethod]
        public void ConfirmSystemXmlIsNotLoadedByXamlObjectWriter()
        {
            const string AlreadyLoaded = "System.Xml Was already loaded when test began!";
            const string WriterLoaded = "XamlObjectWriter loaded System.Xml when it didn't need to!";

            // Run BamlAvoidXmlTest.exe as a separate process because if it runs under the debugger,
            // tracing code loads System.Xml and fails the test.
            Process process = new Process();
            process.StartInfo = GetProcessStartInfo();
            process.Start();
            process.WaitForExit();
            int ret = process.ExitCode;

            switch(ret)
            {
                case 1:
                    Console.WriteLine("FAIL: {0}", AlreadyLoaded);
                    throw new Exception(AlreadyLoaded);

                case 2:
                    Console.WriteLine("FAIL: {0}", WriterLoaded);
                    throw new Exception(WriterLoaded);

                case 0:
                    // good.
                    break;
            }
        }


        private ProcessStartInfo GetProcessStartInfo()
        {
            bool isNetCore = IsNetCore();

            return new ProcessStartInfo
            {
                FileName = isNetCore ? GetDotNet() : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BamlAvoidXmlTest.exe"),
                Arguments = isNetCore ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BamlAvoidXmlTest.dll") : string.Empty,
                CreateNoWindow = true,
                UseShellExecute = false
            };
        }

        private bool IsNetCore()
        {
            var netCoreFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BamlAvoidXmlTest.dll");
            if (File.Exists(netCoreFileName))
            {
                return true;
            }

            return false;
        }

        private string GetDotNet()
        {
            var mainModule = Process.GetCurrentProcess().MainModule;
            if (string.Equals(Path.GetFileNameWithoutExtension(mainModule.ModuleName), "dotnet", StringComparison.InvariantCultureIgnoreCase))
            {
                return mainModule.FileName;
            }

            return "dotnet.exe";
        }
    }
}
