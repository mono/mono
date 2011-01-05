//
// CscTest.cs
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
using System.Collections;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Tasks;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Tasks {

	class CscExtended : Csc {
		public CscExtended ()
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
	}

	[TestFixture]
	public class CscTest {
		[Test]
		public void TestAssignment ()
		{
			Csc csc = new Csc ();

			csc.AllowUnsafeBlocks = true;
			csc.BaseAddress = "1";
			csc.CheckForOverflowUnderflow = true;
			csc.DisabledWarnings = "2";
			csc.DocumentationFile = "3";
			csc.ErrorReport = "4";
			csc.GenerateFullPaths = true;
			csc.LangVersion = "5";
			csc.ModuleAssemblyName = "6";
			csc.NoStandardLib = true;
			csc.PdbFile = "7";
			csc.Platform = "8";
			csc.UseHostCompilerIfAvailable = true;
			csc.WarningLevel = 9;
			csc.WarningsAsErrors = "10";
			csc.WarningsNotAsErrors = "11";

			Assert.AreEqual (true, csc.AllowUnsafeBlocks, "A1");
			Assert.AreEqual ("1", csc.BaseAddress, "A2");
			Assert.AreEqual (true, csc.CheckForOverflowUnderflow, "A3");
			Assert.AreEqual ("2", csc.DisabledWarnings, "A4");
			Assert.AreEqual ("3", csc.DocumentationFile, "A5");
			Assert.AreEqual ("4", csc.ErrorReport, "A6");
			Assert.AreEqual (true, csc.GenerateFullPaths, "A7");
			Assert.AreEqual ("5", csc.LangVersion, "A8");
			Assert.AreEqual ("6", csc.ModuleAssemblyName, "A9");
			Assert.AreEqual ("7", csc.PdbFile, "A10");
			Assert.AreEqual ("8", csc.Platform, "A11");
			Assert.AreEqual (true, csc.UseHostCompilerIfAvailable, "A12");
			Assert.AreEqual (9, csc.WarningLevel, "A13");
			Assert.AreEqual ("10", csc.WarningsAsErrors, "A14");
			Assert.AreEqual ("11", csc.WarningsNotAsErrors, "A15");
		}

		[Test]
		public void TestDefaultValues ()
		{
			Csc csc = new Csc ();

			Assert.IsFalse (csc.AllowUnsafeBlocks, "A1");
			Assert.IsNull (csc.BaseAddress, "A2");
			Assert.IsFalse (csc.CheckForOverflowUnderflow, "A3");
			Assert.IsNull (csc.DisabledWarnings, "A4");
			Assert.IsNull (csc.DocumentationFile, "A5");
			Assert.IsNull (csc.ErrorReport, "A6");
			Assert.IsFalse (csc.GenerateFullPaths, "A7");
			Assert.IsNull (csc.LangVersion, "A8");
			Assert.IsNull (csc.ModuleAssemblyName, "A9");
			Assert.IsNull (csc.PdbFile, "A10");
			Assert.IsNull (csc.Platform, "A11");
			Assert.IsFalse (csc.UseHostCompilerIfAvailable, "A12");
			Assert.AreEqual (4, csc.WarningLevel, "A13");
			Assert.IsNull (csc.WarningsAsErrors, "A14");
			Assert.IsNull (csc.WarningsNotAsErrors, "A15");
		}

	#region CscSpecificVariables

		[Test]
		public void TestAllowUnsafeBlocks1 ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.AllowUnsafeBlocks = true;
			csc.ARFC (clbe);

			Assert.AreEqual ("/unsafe+", clbe.ToString (), "A1");
		}

		[Test]
		public void TestAllowUnsafeBlocks2 ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.AllowUnsafeBlocks = false;
			csc.ARFC (clbe);

			Assert.AreEqual ("/unsafe-", clbe.ToString (), "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestBaseAddress ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.BaseAddress = "A";
			csc.ARFC (clbe);

			Assert.AreEqual ("/baseaddress:A", clbe.ToString (), "A1");
		}

		[Test]
		public void TestCheckForOverflowUnderflow1 ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.CheckForOverflowUnderflow = true;
			csc.ARFC (clbe);

			Assert.AreEqual ("/checked+", clbe.ToString (), "A1");
		}

		[Test]
		public void TestCheckForOverflowUnderflow2 ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.CheckForOverflowUnderflow = false;
			csc.ARFC (clbe);

			Assert.AreEqual ("/checked-", clbe.ToString (), "A1");
		}

		[Test]
		public void TestDisabledWarnings ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.DisabledWarnings = "A";
			csc.ARFC (clbe);

			Assert.AreEqual ("/nowarn:A", clbe.ToString (), "A1");
		}

		[Test]
		public void TestDocumentationFile ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.DocumentationFile = "A";
			csc.ARFC (clbe);

			Assert.AreEqual ("/doc:A", clbe.ToString (), "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestErrorReport ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.ErrorReport = "A";
			csc.ARFC (clbe);

			Assert.AreEqual ("/errorreport:A", clbe.ToString (), "A1");
		}

		[Test]
		public void TestGenerateFullPaths1 ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.GenerateFullPaths = true;
			csc.ARFC (clbe);

			Assert.AreEqual ("/fullpaths", clbe.ToString (), "A1");
		}

		[Test]
		public void TestGenerateFullPaths2 ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.GenerateFullPaths = false;
			csc.ARFC (clbe);

			Assert.AreEqual ("", clbe.ToString (), "A1");
		}

		[Test]
		public void TestLangVersion ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.LangVersion = "A'B";
			csc.ARFC (clbe);

			Assert.AreEqual ("/langversion:\"A'B\"", clbe.ToString (), "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestModuleAssemblyName ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.ModuleAssemblyName = "A'B";
			csc.ARFC (clbe);

			Assert.AreEqual ("/moduleassemblyname:\"A'B\"", clbe.ToString (), "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestPdbFile ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.PdbFile = "A";
			csc.ARFC (clbe);

			Assert.AreEqual ("/pdb:A", clbe.ToString (), "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestPlatform ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.Platform = "A";
			csc.ARFC (clbe);

			Assert.AreEqual ("/platform:A", clbe.ToString (), "A1");
		}

		[Test]
		public void TestWarning ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.WarningLevel = 4;
			csc.ARFC (clbe);

			Assert.AreEqual ("/warn:4", clbe.ToString (), "A1");
		}

		[Test]
		public void TestWarningAsErrors ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.WarningsAsErrors = "A'B";
			csc.ARFC (clbe);

			Assert.AreEqual ("/warnaserror+:\"A'B\"", clbe.ToString (), "A1");
		}
		[Test]
		public void TestWarningNotAsErrors ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			csc.WarningsNotAsErrors = "A'B";
			csc.ARFC (clbe);

			Assert.AreEqual ("/warnaserror-:\"A'B\"", clbe.ToString (), "A1");
		}

	#endregion

		[Test]
		public void TestAdditionalLibPaths ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			csc.AdditionalLibPaths = new string [2] { "A'Foo", "B" };
			csc.ARFC (c1);
			csc.ACLC (c2);

			Assert.AreEqual ("/lib:\"A'Foo\",B", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		// Behavior for this intentionally differs from .net .
		// msbuild doesn't quote the define args, but we do
		// that to make it easier to copy/paste and execute
		// compiler command lines, helps in debugging.
		[Category ("NotDotNet")]
		[Test]
		public void TestDefineConstants ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			csc.DefineConstants = "A;B;;CD;;;Foo  Bar";
			csc.ARFC (c1);
			csc.ACLC (c2);

			Assert.AreEqual ("/define:\"A;B;CD;Foo;Bar\"", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestDefineConstants2 ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			csc.DefineConstants = ";;;";
			csc.ARFC (c1);
			csc.ACLC (c2);

			Assert.AreEqual (String.Empty, c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestMainEntryPoint ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			csc.MainEntryPoint = "A;B";
			csc.ARFC (c1);
			csc.ACLC (c2);

			Assert.AreEqual ("/main:\"A;B\"", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestReferences ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			csc.References = new ITaskItem [2] { new TaskItem ("A;C"), new TaskItem ("B") };
			csc.ARFC (c1);
			csc.ACLC (c2);

			Assert.AreEqual ("/reference:\"A;C\" /reference:B", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestResponseFiles ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			csc.ResponseFiles = new ITaskItem [2] { new TaskItem ("A\'Foo"), new TaskItem ("B") };
			csc.ARFC (c1);
			csc.ACLC (c2);

			Assert.AreEqual ("@\"A'Foo\" @B", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}

		[Test]
		public void TestWin32Resource ()
		{
			CscExtended csc = new CscExtended ();
			CommandLineBuilderExtension c1 = new CommandLineBuilderExtension ();
			CommandLineBuilderExtension c2 = new CommandLineBuilderExtension ();

			csc.Win32Resource = "A;B";
			csc.ARFC (c1);
			csc.ACLC (c2);

			Assert.AreEqual ("/win32res:\"A;B\"", c1.ToString (), "A1");
			Assert.AreEqual (String.Empty, c2.ToString (), "A2");
		}
	}
}
