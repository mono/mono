// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.IO;
using Microsoft.Win32;

namespace NUnit.Framework.Internal
{
    [TestFixture]
    public class RuntimeFrameworkTests
    {
        [Test]
        public void CanGetCurrentFramework()
        {
            Version expectedClrVersion = Environment.Version;
            Version expectedFrameworkVersion = new Version(expectedClrVersion.Major, expectedClrVersion.Minor);

#if SILVERLIGHT
            RuntimeType expectedRuntime = RuntimeType.Silverlight;
            expectedClrVersion = new RuntimeFramework(expectedRuntime, expectedFrameworkVersion).ClrVersion;
#else
             RuntimeType expectedRuntime = Type.GetType("Mono.Runtime", false) != null 
                ? RuntimeType.Mono 
                : Environment.OSVersion.Platform == PlatformID.WinCE
                    ? RuntimeType.NetCF
                    : RuntimeType.Net;

            // TODO: Remove duplication of RuntimeFramework code
            switch (expectedRuntime)
            {
                case RuntimeType.Mono:
                    if (expectedFrameworkVersion.Major == 1)
                        expectedFrameworkVersion = new Version(1,0);
                    else if (expectedFrameworkVersion.Major == 2)
                        expectedFrameworkVersion = new Version(3,5);
                    break;

                case RuntimeType.Net:
                case RuntimeType.NetCF:
#if !__MOBILE__
                    if (expectedFrameworkVersion.Major == 2)
                    {
                        RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework");
                        if (key != null)
                        {
                            string installRoot = key.GetValue("InstallRoot") as string;
                            if (installRoot != null)
                            {
                                if (Directory.Exists(Path.Combine(installRoot, "v3.5")))
                                    expectedFrameworkVersion = new Version(3,5);
                                else if (Directory.Exists(Path.Combine(installRoot, "v3.0")))
                                    expectedFrameworkVersion = new Version(3,0);
                            }
                        }
                    }
#endif
                    break;
            }
#endif

            RuntimeFramework framework = RuntimeFramework.CurrentFramework;

            Assert.That(framework.Runtime, Is.EqualTo(expectedRuntime));
            Assert.That(framework.ClrVersion, Is.EqualTo(expectedClrVersion));
            Assert.That(framework.FrameworkVersion, Is.EqualTo(expectedFrameworkVersion));
        }

        [Test]
        public void CurrentFrameworkHasBuildSpecified()
        {
            Assert.That(RuntimeFramework.CurrentFramework.ClrVersion.Build, Is.GreaterThan(0));
        }

#if !NUNITLITE
        [Test, Platform(Exclude="NETCF", Reason="NYI")]
        public void CurrentFrameworkMustBeAvailable()
        {
            Assert.That(RuntimeFramework.CurrentFramework.IsAvailable);
        }

        [Test, Platform(Exclude="NETCF", Reason="NYI")]
        public void CanListAvailableFrameworks()
        {
            RuntimeFramework[] available = RuntimeFramework.AvailableFrameworks;
            Assert.That(available, Has.Length.GreaterThan(0) );
            bool foundCurrent = false;
            foreach (RuntimeFramework framework in available)
            {
                Console.WriteLine("Available: {0}", framework.DisplayName);
                foundCurrent |= RuntimeFramework.CurrentFramework.Supports(framework);
            }
            Assert.That(foundCurrent, "CurrentFramework not listed");
        }
#endif

        [TestCaseSource("frameworkData")]
        public void CanCreateUsingFrameworkVersion(FrameworkData data)
        {
            RuntimeFramework framework = new RuntimeFramework(data.runtime, data.frameworkVersion);
            Assert.AreEqual(data.runtime, framework.Runtime);
            Assert.AreEqual(data.frameworkVersion, framework.FrameworkVersion);
            Assert.AreEqual(data.clrVersion, framework.ClrVersion);
        }

        [TestCaseSource("frameworkData")]
        public void CanCreateUsingClrVersion(FrameworkData data)
        {
            Assume.That(data.frameworkVersion.Major != 3);

            RuntimeFramework framework = new RuntimeFramework(data.runtime, data.clrVersion);
            Assert.AreEqual(data.runtime, framework.Runtime);
            Assert.AreEqual(data.frameworkVersion, framework.FrameworkVersion);
            Assert.AreEqual(data.clrVersion, framework.ClrVersion);
        }

        [TestCaseSource("frameworkData")]
        public void CanParseRuntimeFramework(FrameworkData data)
        {
            RuntimeFramework framework = RuntimeFramework.Parse(data.representation);
            Assert.AreEqual(data.runtime, framework.Runtime);
            Assert.AreEqual(data.clrVersion, framework.ClrVersion);
        }

        [TestCaseSource("frameworkData")]
        public void CanDisplayFrameworkAsString(FrameworkData data)
        {
            RuntimeFramework framework = new RuntimeFramework(data.runtime, data.frameworkVersion);
            Assert.AreEqual(data.representation, framework.ToString());
            Assert.AreEqual(data.displayName, framework.DisplayName);
        }

        [TestCaseSource("matchData")]
        public bool CanMatchRuntimes(RuntimeFramework f1, RuntimeFramework f2)
        {
            return f1.Supports(f2);
        }

        internal static TestCaseData[] matchData = new TestCaseData[] {
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(3,5)), 
                new RuntimeFramework(RuntimeType.Net, new Version(2,0))) 
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(2,0)), 
                new RuntimeFramework(RuntimeType.Net, new Version(3,5))) 
                .Returns(false),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(3,5)), 
                new RuntimeFramework(RuntimeType.Net, new Version(3,5))) 
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(2,0)), 
                new RuntimeFramework(RuntimeType.Net, new Version(2,0))) 
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(2,0)), 
                new RuntimeFramework(RuntimeType.Net, new Version(2,0,50727))) 
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(2,0,50727)), 
                new RuntimeFramework(RuntimeType.Net, new Version(2,0))) 
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(2,0,50727)), 
                new RuntimeFramework(RuntimeType.Net, new Version(2,0))) 
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(2,0)), 
                new RuntimeFramework(RuntimeType.Mono, new Version(2,0))) 
                .Returns(false),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(2,0)), 
                new RuntimeFramework(RuntimeType.Net, new Version(1,1))) 
                .Returns(false),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(2,0,50727)), 
                new RuntimeFramework(RuntimeType.Net, new Version(2,0,40607))) 
                .Returns(false),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Mono, new Version(1,1)), // non-existent version but it works
                new RuntimeFramework(RuntimeType.Mono, new Version(1,0))) 
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Mono, new Version(2,0)),
                new RuntimeFramework(RuntimeType.Any, new Version(2,0)))
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Any, new Version(2,0)),
                new RuntimeFramework(RuntimeType.Mono, new Version(2,0)))
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Any, new Version(2,0)),
                new RuntimeFramework(RuntimeType.Any, new Version(2,0)))
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Any, new Version(2,0)),
                new RuntimeFramework(RuntimeType.Any, new Version(4,0)))
                .Returns(false),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, RuntimeFramework.DefaultVersion), 
                new RuntimeFramework(RuntimeType.Net, new Version(2,0))) 
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(2,0)),
                new RuntimeFramework(RuntimeType.Net, RuntimeFramework.DefaultVersion)) 
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Any, RuntimeFramework.DefaultVersion), 
                new RuntimeFramework(RuntimeType.Net, new Version(2,0))) 
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(RuntimeType.Net, new Version(2,0)),
                new RuntimeFramework(RuntimeType.Any, RuntimeFramework.DefaultVersion)) 
                .Returns(true)
            };

        public struct FrameworkData
        {
            public RuntimeType runtime;
            public Version frameworkVersion;
            public Version clrVersion;
            public string representation;
            public string displayName;

            public FrameworkData(RuntimeType runtime, Version frameworkVersion, Version clrVersion,
                string representation, string displayName)
            {
                this.runtime = runtime;
                this.frameworkVersion = frameworkVersion;
                this.clrVersion = clrVersion;
                this.representation = representation;
                this.displayName = displayName;
            }

            public override string ToString()
            {
                return string.Format("<{0},{1},{2}>", this.runtime, this.frameworkVersion, this.clrVersion);
            }
        }

        internal FrameworkData[] frameworkData = new FrameworkData[] {
            new FrameworkData(RuntimeType.Net, new Version(1,0), new Version(1,0,3705), "net-1.0", "Net 1.0"),
            //new FrameworkData(RuntimeType.Net, new Version(1,0,3705), new Version(1,0,3705), "net-1.0.3705", "Net 1.0.3705"),
            //new FrameworkData(RuntimeType.Net, new Version(1,0), new Version(1,0,3705), "net-1.0.3705", "Net 1.0.3705"),
            new FrameworkData(RuntimeType.Net, new Version(1,1), new Version(1,1,4322), "net-1.1", "Net 1.1"),
            //new FrameworkData(RuntimeType.Net, new Version(1,1,4322), new Version(1,1,4322), "net-1.1.4322", "Net 1.1.4322"),
            new FrameworkData(RuntimeType.Net, new Version(2,0), new Version(2,0,50727), "net-2.0", "Net 2.0"),
            //new FrameworkData(RuntimeType.Net, new Version(2,0,40607), new Version(2,0,40607), "net-2.0.40607", "Net 2.0.40607"),
            //new FrameworkData(RuntimeType.Net, new Version(2,0,50727), new Version(2,0,50727), "net-2.0.50727", "Net 2.0.50727"),
            new FrameworkData(RuntimeType.Net, new Version(3,0), new Version(2,0,50727), "net-3.0", "Net 3.0"),
            new FrameworkData(RuntimeType.Net, new Version(3,5), new Version(2,0,50727), "net-3.5", "Net 3.5"),
            new FrameworkData(RuntimeType.Net, new Version(4,0), new Version(4,0,30319), "net-4.0", "Net 4.0"),
            new FrameworkData(RuntimeType.Net, RuntimeFramework.DefaultVersion, RuntimeFramework.DefaultVersion, "net", "Net"),
            new FrameworkData(RuntimeType.Mono, new Version(1,0), new Version(1,1,4322), "mono-1.0", "Mono 1.0"),
            new FrameworkData(RuntimeType.Mono, new Version(2,0), new Version(2,0,50727), "mono-2.0", "Mono 2.0"),
            //new FrameworkData(RuntimeType.Mono, new Version(2,0,50727), new Version(2,0,50727), "mono-2.0.50727", "Mono 2.0.50727"),
            new FrameworkData(RuntimeType.Mono, new Version(3,5), new Version(2,0,50727), "mono-3.5", "Mono 3.5"),
            new FrameworkData(RuntimeType.Mono, new Version(4,0), new Version(4,0,30319), "mono-4.0", "Mono 4.0"),
            new FrameworkData(RuntimeType.Mono, RuntimeFramework.DefaultVersion, RuntimeFramework.DefaultVersion, "mono", "Mono"),
            new FrameworkData(RuntimeType.Any, new Version(1,1), new Version(1,1,4322), "v1.1", "v1.1"),
            new FrameworkData(RuntimeType.Any, new Version(2,0), new Version(2,0,50727), "v2.0", "v2.0"),
            //new FrameworkData(RuntimeType.Any, new Version(2,0,50727), new Version(2,0,50727), "v2.0.50727", "v2.0.50727"),
            new FrameworkData(RuntimeType.Any, new Version(3,5), new Version(2,0,50727), "v3.5", "v3.5"),
            new FrameworkData(RuntimeType.Any, new Version(4,0), new Version(4,0,30319), "v4.0", "v4.0"),
            new FrameworkData(RuntimeType.Any, RuntimeFramework.DefaultVersion, RuntimeFramework.DefaultVersion, "any", "Any")
        };
    }
}
