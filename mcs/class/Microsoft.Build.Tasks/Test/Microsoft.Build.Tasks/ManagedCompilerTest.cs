//
// ManagedCompilerTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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
using System.IO;
using System.Collections;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.Microsoft.Build.Tasks {

	class MCExtended : ManagedCompiler {
		public MCExtended ()
			: base ()
		{
		}

		public void ARFC (CommandLineBuilderExtension commandLine)
		{
			base.AddResponseFileCommands (commandLine);
		}

		public void ACLC (CommandLineBuilderExtension commandLine)
		{
			base.AddCommandLineCommands (commandLine);
		}

		public new bool CheckAllReferencesExistOnDisk ()
		{
			return base.CheckAllReferencesExistOnDisk ();
		}
			

		protected override string ToolName {
			get { return "something"; }
		}

		protected override string GenerateFullPathToTool ()
		{
			return null;
		}
	}

	[TestFixture]
	public class ManagedCompilerTest {
		
		[Test]
		public void TestAssignment ()
		{
			MCExtended mc = new MCExtended ();

			mc.AdditionalLibPaths = new string [1] { "1" };
			mc.AddModules = new string [1] { "2" };
			mc.CodePage = 3;
			mc.DebugType = "4";
			mc.DefineConstants = "5";
			mc.DelaySign = true;
			mc.EmitDebugInformation = true;
			mc.FileAlignment = 6;
			mc.KeyContainer = "7";
			mc.KeyFile = "8";
			mc.LinkResources = new ITaskItem [1] { new TaskItem ("9") };
			mc.MainEntryPoint = "10";
			mc.NoConfig = true;
			mc.NoLogo = true;
			mc.Optimize = true;
			mc.OutputAssembly = new TaskItem ("11");
			mc.References = new ITaskItem [1] { new TaskItem ("12") };
			mc.Resources = new ITaskItem [1] { new TaskItem ("13") };
			mc.ResponseFiles = new ITaskItem [1] { new TaskItem ("14") };
			mc.Sources = new ITaskItem [1] { new TaskItem ("15") };
			mc.TargetType = "16";
			mc.TreatWarningsAsErrors = true;
			mc.Utf8Output = true;
			mc.Win32Icon = "17";
			mc.Win32Resource = "18";

			Assert.AreEqual ("1", mc.AdditionalLibPaths [0], "A1");
			Assert.AreEqual ("2", mc.AddModules [0], "A2");
			Assert.AreEqual (3, mc.CodePage, "A3");
			Assert.AreEqual ("4", mc.DebugType, "A4");
			Assert.AreEqual ("5", mc.DefineConstants, "A5");
			Assert.AreEqual (true, mc.DelaySign, "A6");
			Assert.AreEqual (true, mc.EmitDebugInformation, "A7");
			Assert.AreEqual (6, mc.FileAlignment, "A8");
			Assert.AreEqual ("7", mc.KeyContainer, "A9");
			Assert.AreEqual ("8", mc.KeyFile, "A10");
			Assert.AreEqual ("9", mc.LinkResources [0].ItemSpec, "A11");
			Assert.AreEqual ("10", mc.MainEntryPoint, "A12");
			Assert.AreEqual (true, mc.NoConfig, "A13");
			Assert.AreEqual (true, mc.NoLogo, "A14");
			Assert.AreEqual (true, mc.Optimize, "A15");
			Assert.AreEqual ("11", mc.OutputAssembly.ItemSpec, "A16");
			Assert.AreEqual ("12", mc.References [0].ItemSpec, "A17");
			Assert.AreEqual ("13", mc.Resources [0].ItemSpec, "A18");
			Assert.AreEqual ("14", mc.ResponseFiles [0].ItemSpec, "A19");
			Assert.AreEqual ("15", mc.Sources [0].ItemSpec, "A20");
			Assert.AreEqual ("16", mc.TargetType, "A21");
			Assert.AreEqual (true, mc.TreatWarningsAsErrors, "A22");
			Assert.AreEqual (true, mc.Utf8Output, "A23");
			Assert.AreEqual ("17", mc.Win32Icon, "A24");
			Assert.AreEqual ("18", mc.Win32Resource, "A25");
		}

		[Test]
		public void TestDefaultValues ()
		{
			MCExtended mc = new MCExtended ();

			Assert.IsNull (mc.AdditionalLibPaths, "A1");
			Assert.IsNull (mc.AddModules, "A2");
			Assert.AreEqual (0, mc.CodePage, "A3");
			Assert.IsNull (mc.DebugType, "A4");
			Assert.IsNull (mc.DefineConstants, "A5");
			Assert.IsFalse (mc.DelaySign, "A6");
			Assert.IsFalse (mc.EmitDebugInformation, "A7");
			Assert.AreEqual (0, mc.FileAlignment, "A8");
			Assert.IsNull (mc.KeyContainer, "A9");
			Assert.IsNull (mc.KeyFile, "A10");
			Assert.IsNull (mc.LinkResources, "A11");
			Assert.IsNull (mc.MainEntryPoint, "A12");
			Assert.IsFalse (mc.NoConfig, "A13");
			Assert.IsFalse (mc.NoLogo, "A14");
			Assert.IsFalse (mc.Optimize, "A15");
			Assert.IsNull (mc.OutputAssembly, "A16");
			Assert.IsNull (mc.References, "A17");
			Assert.IsNull (mc.Resources, "A18");
			Assert.IsNull (mc.ResponseFiles, "A19");
			Assert.IsNull (mc.Sources, "A20");
			Assert.IsNull (mc.TargetType, "A21");
			Assert.IsFalse (mc.TreatWarningsAsErrors, "A22");
			Assert.IsFalse (mc.Utf8Output, "A23");
			Assert.IsNull (mc.Win32Icon, "A24");
			Assert.IsNull (mc.Win32Resource, "A25");
		}

		[Test]
		public void TestAdditionalLibPaths ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.AdditionalLibPaths = new string [2] { "A", "B" };
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestAddModules ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.AddModules = new string [2] { "A", "B" };
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/addmodule:A,B", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestCodePage1 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.CodePage = 1111;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/codepage:1111", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestCodePage2 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}
		
		[Test]
		public void TestDebugType ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.DebugType = "A";
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/debug:A", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestDefineConstants ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.DefineConstants = "A;B";
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestDelaySign1 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.DelaySign = true;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/delaysign+", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestDelaySign2 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.DelaySign = false;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/delaysign-", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestEmitDebugInformation1 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.EmitDebugInformation = true;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/debug:portable", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestEmitDebugInformation2 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.EmitDebugInformation = false;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/debug-", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestFileAlignment1 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.FileAlignment = 100;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/filealign:100", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestFileAlignment2 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestKeyContainer ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.KeyContainer = "A";
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/keycontainer:A", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestKeyFile ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.KeyFile = "A";
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/keyfile:A /publicsign", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestLinkResources ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.LinkResources = new ITaskItem [2] { new TaskItem ("A"), new TaskItem ("B") };
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/linkresource:A /linkresource:B", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestMainEntryPoint ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.MainEntryPoint = "A";
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}
		
		[Test]
		public void TestNoConfig1 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.NoConfig = true;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual ("/noconfig", c2.ToString (), "A2");
		}

		[Test]
		public void TestNoConfig2 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.NoConfig = false;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestNoLogo1 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.NoLogo = true;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/nologo", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestNoLogo2 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.NoLogo = false;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestOptimize1 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.Optimize = true;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/optimize+", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestOptimize2 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.Optimize = false;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/optimize-", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestOutputAssembly ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.OutputAssembly = new TaskItem ("A");
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/out:A", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestReferences ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.References = new ITaskItem [2] { new TaskItem ("A"), new TaskItem ("B") };
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestResources ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.Resources = new ITaskItem [2] { new TaskItem ("A"), new TaskItem ("B") };
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/resource:A /resource:B", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestResponseFiles ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.ResponseFiles = new ITaskItem [2] { new TaskItem ("A"), new TaskItem ("B") };
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestSources ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.Sources = new ITaskItem [2] { new TaskItem ("A"), new TaskItem ("B") };
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/out:A.exe A B", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestTargetType ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.TargetType = "A";
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/target:a", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}
		
		[Test]
		public void TestTreatWarningsAsErrors1 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.TreatWarningsAsErrors = true;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/warnaserror+", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestTreatWarningsAsErrors2 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.TreatWarningsAsErrors = false;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/warnaserror-", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}
		
		[Test]
		[Category ("NotWorking")]
		public void TestUtf8Output1 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.Utf8Output = true;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/utf8output", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestUtf8Output2 ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.Utf8Output = false;
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestWin32Icon ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.Win32Icon = "A";
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual ("/win32icon:A", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestWin32Resource ()
		{
			MCExtended mc = new MCExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			mc.Win32Resource = "A;B";
			mc.ARFC (c1);
			mc.ACLC (c2);
			
			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestCheckAllReferencesExistOnDisk1 ()
		{
			MCExtended mc = new MCExtended ();
			mc.BuildEngine = new TestEngine ();
			
			mc.References = new ITaskItem [0];
			Assert.IsTrue (mc.CheckAllReferencesExistOnDisk (), "A1");

			mc.References = null;
			Assert.IsTrue (mc.CheckAllReferencesExistOnDisk (), "A2");

			string path = TestResourceHelper.GetFullPathOfResource ("Test/resources/test.cs");
			mc.References = new ITaskItem [1] { new TaskItem (path) };
			Assert.IsTrue (mc.CheckAllReferencesExistOnDisk (), "A3");

			mc.References = new ITaskItem [2] { new TaskItem (path), new TaskItem ("X") };
			Assert.IsFalse (mc.CheckAllReferencesExistOnDisk (), "A4");
		}
	}
}
        
