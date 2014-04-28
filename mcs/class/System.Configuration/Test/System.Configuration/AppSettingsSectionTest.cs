//
// System.Configuration.AppSettingsSectionTest.cs - Unit tests
// for System.Configuration.AppSettingsSection.
//
// Author:
//	Tom Philpot  <tom.philpot@logos.com>
//	Dave Curylo  <curylod@asme.org>
//
// Copyright (C) 2014 Logos Bible Software
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
//

using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;
using NUnit.Framework;

namespace MonoTests.System.Configuration
{
	using Util;

	[TestFixture]
	public class AppSettingsSectionTest
	{
		private string originalCurrentDir;
		private string tempFolder;

		[SetUp]
		public void SetUp ()
		{
			originalCurrentDir = Directory.GetCurrentDirectory ();
			tempFolder = Path.Combine (Path.GetTempPath (), this.GetType ().FullName);
			if (!Directory.Exists (tempFolder))
				Directory.CreateDirectory (tempFolder);
		}

		[TearDown]
		public void TearDown ()
		{
			Directory.SetCurrentDirectory (originalCurrentDir);
			if (Directory.Exists (tempFolder))
				Directory.Delete (tempFolder, true);
			File.Delete ("TestLoadsFromFileAttribute.exe");
			File.Delete ("TestLoadsFromFileAttribute.exe.config");
			File.Delete ("extra.config");
		}
		
		[Test]
		public void TestFile ()
		{
			Directory.SetCurrentDirectory (tempFolder);

			var currentAssembly = Assembly.GetExecutingAssembly ().Location;
			var config = ConfigurationManager.OpenExeConfiguration (currentAssembly);
			Assert.AreEqual ("Test/appSettings.config", config.AppSettings.File, "#A01");
			Assert.AreEqual ("foo", ConfigurationSettings.AppSettings["TestKey1"], "#A02");
			Assert.AreEqual ("bar", ConfigurationSettings.AppSettings["TestKey2"], "#A03");
		}

		[Test]
		public void TestLoadsFileAttributeRelativePath () {
			// Compile a test executable with codedom that returns a string from the file
			// Create an app.config file.
			// Create an extra.config file that is referenced by the app.config file.
			// Process.Start the executable from a different directory (up one).
			// Should not find the setting from extra.config before the fix
			// Should find it after the fix.
			var code = "using System; using System.Configuration; namespace Testing { class LoadsFromFileAttribute { public static void Main(string[] args) { if (ConfigurationManager.AppSettings[\"foo\"] == \"bar\") Environment.Exit (100); }  } }";
			var codeProvider = new CSharpCodeProvider ();
			var icc = codeProvider.CreateCompiler ();
			var parameters = new CompilerParameters ();
			parameters.GenerateExecutable = true;
			parameters.OutputAssembly = "TestLoadsFromFileAttribute.exe";
			parameters.ReferencedAssemblies.Add ("System.Configuration");
			CompilerResults results = icc.CompileAssemblyFromSource (parameters, code);
			if (results.Errors.Count > 0) {
				Assert.Inconclusive (String.Format ("Test assembly failed to build. Errors : {0}", results.Errors [0]));
			}
			File.WriteAllText ("TestLoadsFromFileAttribute.exe.config", "<configuration><appSettings file=\"extra.config\"/></configuration>");
			File.WriteAllText ("extra.config", "<appSettings><add key=\"foo\" value=\"bar\" /></appSettings>");
			var pwd = Environment.CurrentDirectory;
			var p = Process.Start (new ProcessStartInfo {
				WorkingDirectory = "../",
				FileName = Path.Combine (pwd, "TestLoadsFromFileAttribute.exe"),
			});
			p.WaitForExit ();
			Assert.AreEqual (100, p.ExitCode, "Couldn't find setting from extra.config file when launched from different directory.");
		}
	}
}