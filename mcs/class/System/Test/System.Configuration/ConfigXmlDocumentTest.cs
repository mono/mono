//
// System.Configuration.ConfigXmlDocumentTest.cs - Unit tests for
// System.Configuration.ConfigXmlDocument.
//
// Author:
//	Gert Driesen <drieseng@users.sourceforge.net>
//
// Copyright (C) 2008 Gert Driesen
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
using System.IO;
using System.Configuration;

using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class ConfigXmlDocumentTest {
		private TempDirectory _tempFolder;
		private string tempFolder;

		[SetUp]
		public void SetUp ()
		{
			_tempFolder = new TempDirectory ();
			tempFolder = _tempFolder.Path;
		}

		[TearDown]
		public void TearDown ()
		{
			_tempFolder.Dispose ();
		}

		[Test]
		public void Load ()
		{
			string config_xml = @"
				<configuration>
					<appSettings>
						<add key='anyKey' value='42' />
					</appSettings>
					<system.diagnostics />
				</configuration>";
			string config_file = Path.Combine (tempFolder, "config.xml");
			File.WriteAllText (config_file, config_xml);

			ConfigXmlDocument doc = new ConfigXmlDocument ();
			doc.Load (config_file);
			Assert.AreEqual (1, doc.ChildNodes.Count, "ChildNodes");
			Assert.AreEqual (config_file, doc.Filename, "Filename");
			Assert.AreEqual ("#document", doc.Name, "Name");
			File.Delete (config_file);
		}
	}
}

