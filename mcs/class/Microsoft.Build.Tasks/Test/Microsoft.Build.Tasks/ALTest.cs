//
// ALTest.cs
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
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Tasks {

	class ALExtended : AL {
		public ALExtended ()
			: base ()
		{
		}

		public void ARFC (CommandLineBuilderExtension commandLine)
		{
			base.AddResponseFileCommands (commandLine);
		}

		public string TN {
			get { return base.ToolName; }
		}
	}

	[TestFixture]
	public class ALTest {
		
		[Test]
		public void TestAssignment1 ()
		{
			AL al = new AL ();

			al.AlgorithmId = "1";
			al.BaseAddress = "2";
			al.CompanyName = "3";
			al.Configuration = "4";
			al.Copyright = "5";
			al.Culture = "6";
			al.DelaySign = true;
			al.Description = "7";
			al.EmbedResources = new ITaskItem [1] { new TaskItem ("8") };
			al.EvidenceFile = "9";
			al.FileVersion = "10";
			al.Flags = "11";
			al.GenerateFullPaths = true;
			al.KeyContainer = "12";
			al.KeyFile = "13";
			al.LinkResources = new ITaskItem [1] { new TaskItem ("14") };
			al.MainEntryPoint = "15";
			al.OutputAssembly = new TaskItem ("16");
			al.Platform = "17";
			al.ProductName = "18";
			al.ProductVersion = "19";
			al.ResponseFiles = new string [1] { "20" };
			al.SourceModules = new ITaskItem [1] { new TaskItem ("21") };
			al.TargetType = "22";
			al.TemplateFile = "23";
			al.Title = "24";
			al.Trademark = "25";
			al.Version = "26";
			al.Win32Icon = "27";
			al.Win32Resource = "28";

			Assert.AreEqual ("1", al.AlgorithmId, "A1");
			Assert.AreEqual ("2", al.BaseAddress, "A2");
			Assert.AreEqual ("3", al.CompanyName, "A3");
			Assert.AreEqual ("4", al.Configuration, "A4");
			Assert.AreEqual ("5", al.Copyright, "A5");
			Assert.AreEqual ("6", al.Culture, "A6");
			Assert.AreEqual (true, al.DelaySign, "A7");
			Assert.AreEqual ("7", al.Description, "A8");
			Assert.AreEqual ("8", al.EmbedResources [0].ItemSpec, "A9");
			Assert.AreEqual ("9", al.EvidenceFile, "A10");
			Assert.AreEqual ("10", al.FileVersion, "A11");
			Assert.AreEqual ("11", al.Flags, "A12");
			Assert.AreEqual (true, al.GenerateFullPaths, "A13");
			Assert.AreEqual ("12", al.KeyContainer, "A14");
			Assert.AreEqual ("13", al.KeyFile, "A15");
			Assert.AreEqual ("14", al.LinkResources [0].ItemSpec, "A16");
			Assert.AreEqual ("15", al.MainEntryPoint, "A17");
			Assert.AreEqual ("16", al.OutputAssembly.ItemSpec, "A18");
			Assert.AreEqual ("17", al.Platform, "A19");
			Assert.AreEqual ("18", al.ProductName, "A20");
			Assert.AreEqual ("19", al.ProductVersion, "A21");
			Assert.AreEqual ("20", al.ResponseFiles [0], "A22");
			Assert.AreEqual ("21", al.SourceModules [0].ItemSpec, "A23");
			Assert.AreEqual ("22", al.TargetType, "A24");
			Assert.AreEqual ("23", al.TemplateFile, "A25");
			Assert.AreEqual ("24", al.Title, "A26");
			Assert.AreEqual ("25", al.Trademark, "A27");
			Assert.AreEqual ("26", al.Version, "A28");
			Assert.AreEqual ("27", al.Win32Icon, "A29");
			Assert.AreEqual ("28", al.Win32Resource, "A30");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestAssignment2 ()
		{
			ALExtended ale = new ALExtended ();
			Assert.AreEqual ("AL.exe", ale.TN, "A31");
		}

		[Test]
		public void TestDefaultValues ()
		{
			AL al = new AL ();

			Assert.IsNull (al.AlgorithmId, "A1");	
			Assert.IsNull (al.BaseAddress, "A2");
			Assert.IsNull (al.CompanyName, "A3");
			Assert.IsNull (al.Configuration, "A4");
			Assert.IsNull (al.Copyright, "A5");
			Assert.IsNull (al.Culture, "A6");
			Assert.IsFalse (al.DelaySign, "A7");
			Assert.IsNull (al.Description, "A8");
			Assert.IsNull (al.EmbedResources, "A9");
			Assert.IsNull (al.EvidenceFile, "A10");
			Assert.IsNull (al.FileVersion, "A11");
			Assert.IsNull (al.Flags, "A12");
			Assert.IsFalse (al.GenerateFullPaths, "A13");
			Assert.IsNull (al.KeyContainer, "A14");
			Assert.IsNull (al.KeyFile, "A15");
			Assert.IsNull (al.LinkResources, "A16");
			Assert.IsNull (al.MainEntryPoint, "A17");
			Assert.IsNull (al.OutputAssembly, "A18");
			Assert.IsNull (al.Platform, "A19");
			Assert.IsNull (al.ProductName, "A20");
			Assert.IsNull (al.ProductVersion, "A21");
			Assert.IsNull (al.ResponseFiles, "A22");
			Assert.IsNull (al.SourceModules, "A23");
			Assert.IsNull (al.TargetType, "A24");
			Assert.IsNull (al.TemplateFile, "A25");
			Assert.IsNull (al.Title, "A26");
			Assert.IsNull (al.Trademark, "A27");
			Assert.IsNull (al.Version, "A28");
			Assert.IsNull (al.Win32Icon, "A29");
			Assert.IsNull (al.Win32Resource, "A30");
		}

		[Test]
		public void TestAlgorithmId ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.AlgorithmId = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/algid:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestBaseAddress ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.BaseAddress = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/baseaddress:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestCompanyName ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.CompanyName = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/company:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestConfiguration ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Configuration = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/configuration:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestCopyright ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Copyright = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/copyright:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestCulture ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Culture = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/culture:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestDelaySign1 ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.DelaySign = true;
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/delaysign+", clbe.ToString (), "A1");
		}

		[Test]
		public void TestDelaySign2 ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.DelaySign = false;
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/delaysign-", clbe.ToString (), "A1");
		}

		[Test]
		public void TestDescription ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Description = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/description:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestEmbedResources ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.EmbedResources = new ITaskItem [2] { new TaskItem ("a"), new TaskItem ("b") };
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/embed:a /embed:b", clbe.ToString (), "A1");
		}

		[Test]
		public void TestEvidenceFile ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.EvidenceFile = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/evidence:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestFileVersion ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.FileVersion = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/fileversion:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestFlags ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Flags = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/flags:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestGenerateFullPaths1 ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.GenerateFullPaths = true;
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/fullpaths", clbe.ToString (), "A1");
		}

		[Test]
		public void TestGenerateFullPaths2 ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.GenerateFullPaths = false;
			ale.ARFC (clbe);
			
			Assert.AreEqual (String.Empty, clbe.ToString (), "A1");
		}

		[Test]
		public void TestKeyContainer ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.KeyContainer = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/keyname:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestKeyFile ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.KeyFile = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/keyfile:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestLinkResources ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.LinkResources = new ITaskItem [2] { new TaskItem ("a"), new TaskItem ("b") };
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/link:a /link:b", clbe.ToString (), "A1");
		}

		[Test]
		public void TestMainEntryPoint ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.MainEntryPoint = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/main:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestOutputAssembly ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.OutputAssembly = new TaskItem ("a");
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/out:a", clbe.ToString (), "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestPlatform ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Platform = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/platform:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestProductName ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.ProductName = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/product:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestProductVersion ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.ProductVersion = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/productversion:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestResponseFiles ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.ResponseFiles = new string [2] { "a", "b" };
			ale.ARFC (clbe);
			
			Assert.AreEqual ("@a @b", clbe.ToString (), "A1");
		}

		[Test]
		public void TestSourceModules ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.SourceModules = new ITaskItem [2] { new TaskItem ("a"), new TaskItem ("b") };
			ale.ARFC (clbe);
			
			Assert.AreEqual ("a b", clbe.ToString (), "A1");
		}

		[Test]
		public void TestTargetType ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.TargetType = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/target:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestTemplateFile ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.TemplateFile = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/template:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestTitle ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Title = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/title:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestTrademark ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Trademark = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/trademark:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestVersion ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Version = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/version:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestWin32Icon ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Win32Icon = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/win32icon:a", clbe.ToString (), "A1");
		}

		[Test]
		public void TestWin32Resource ()
		{
			ALExtended ale = new ALExtended ();
			CommandLineBuilderExtension clbe = new CommandLineBuilderExtension ();

			ale.Win32Resource = "a";
			ale.ARFC (clbe);
			
			Assert.AreEqual ("/win32res:a", clbe.ToString (), "A1");
		}
	}
}
        
