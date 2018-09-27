//
// System.Configuration.AppSettingsSectionTest.cs - Unit tests
// for System.Configuration.AppSettingsSection.
//
// Author:
//	Tom Philpot  <tom.philpot@logos.com>
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
using System.Configuration;
using System.IO;
using System.Reflection;
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
		}
		
		[Test]
		public void TestFile ()
		{
			Directory.SetCurrentDirectory (tempFolder);

			var currentAssembly = TestUtil.ThisApplicationPath;
			var config = ConfigurationManager.OpenExeConfiguration (currentAssembly);
			Assert.AreEqual ("System.Configuration-appSettings.config", config.AppSettings.File, "#A01");
			Assert.AreEqual ("foo", ConfigurationSettings.AppSettings["TestKey1"], "#A02");
			Assert.AreEqual ("bar", ConfigurationSettings.AppSettings["TestKey2"], "#A03");
		}
	}
}