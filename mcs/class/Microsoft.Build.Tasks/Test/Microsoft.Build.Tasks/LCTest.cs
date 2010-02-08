//
// LCTest.cs
//
// Author:
//   Ankit Jain (jankit@novell.com)
//
// Copyright 2010 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.IO;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Tasks {

	class LCExtended : LC {
		public LCExtended ()
			: base ()
		{
		}

		public void ACLC (CommandLineBuilderExtension commandLine)
		{
			base.AddCommandLineCommands (commandLine);
		}

		public string TN {
			get { return base.ToolName; }
		}
	}

	[TestFixture]
	public class LCTest {

		[Test]
		public void TestAssignment1 ()
		{
			LC lc = new LC ();

			lc.LicenseTarget = new TaskItem ("bar.exe");
			lc.NoLogo = true;
			lc.OutputDirectory = "abc\\def";
			lc.OutputLicense = new TaskItem ("bar.exe.licenses");
			lc.ReferencedAssemblies = new ITaskItem [] { new TaskItem ("Test.dll") };
			lc.Sources = new ITaskItem [] { new TaskItem ("foo.licx") };

			Assert.AreEqual ("bar.exe", lc.LicenseTarget.ItemSpec, "LicenseTarget");
			Assert.AreEqual (true, lc.NoLogo, "NoLogo");
			Assert.AreEqual ("abc\\def", lc.OutputDirectory, "OutputDirectory");
			Assert.AreEqual ("bar.exe.licenses", lc.OutputLicense.ItemSpec, "OutputLicense");

			Assert.AreEqual (1, lc.ReferencedAssemblies.Length, "Number of ReferenceAssemblies");
			Assert.AreEqual ("Test.dll", lc.ReferencedAssemblies [0].ItemSpec, "ReferencedAssemblies[0]");

			Assert.AreEqual (1, lc.Sources.Length, "Number of Sources");
			Assert.AreEqual ("foo.licx", lc.Sources [0].ItemSpec, "Sources [0]");
		}

		[Test]
		public void TestDefaults ()
		{
			LC lc = new LC ();

			lc.LicenseTarget = new TaskItem ("bar.exe");
			lc.Sources = new ITaskItem [] { new TaskItem ("foo.licx") };

			Assert.AreEqual ("bar.exe", lc.LicenseTarget.ItemSpec, "LicenseTarget");
			Assert.AreEqual (false, lc.NoLogo, "NoLogo");
			Assert.IsNull (lc.OutputDirectory, "OutputDirectory");
			Assert.AreEqual (null, lc.OutputLicense, "OutputLicense");

			Assert.IsNull (lc.ReferencedAssemblies, "ReferencedAssemblies");

			Assert.AreEqual (1, lc.Sources.Length, "Number of Sources");
			Assert.AreEqual ("foo.licx", lc.Sources [0].ItemSpec, "Sources [0]");
		}
	}
}
