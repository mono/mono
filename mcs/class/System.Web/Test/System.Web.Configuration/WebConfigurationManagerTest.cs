//
// WebConfigurationManagerTest.cs 
//	- unit tests for System.Web.Configuration.WebConfigurationManager
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Configuration;
using _Configuration = System.Configuration.Configuration;
using System.IO;
using System.Web.Configuration;
using System.Web;
using System.Web.Security;
using MonoTests.SystemWeb.Framework;
using System.Web.UI;

namespace MonoTests.System.Web.Configuration {

	[TestFixture]
	public class WebConfigurationManagerTest  {

		[TestFixtureTearDown]
		public void Unload ()
		{
			WebTest.Unload ();
		}

		[SetUp]
		public void TestSetUp ()
		{
			WebTest.CopyResource (GetType (), "CustomSectionEmptyCollection.aspx", "CustomSectionEmptyCollection.aspx");
		}
		
		[Test]
		[Category ("NotWorking")]
		public void OpenMachineConfiguration_1 ()
		{
			_Configuration c1 = WebConfigurationManager.OpenMachineConfiguration ();
			_Configuration c2 = ConfigurationManager.OpenMachineConfiguration ();

			Assert.AreEqual (c1.FilePath, c2.FilePath, "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void OpenMachineConfiguration_2 ()
		{
			_Configuration c1 = WebConfigurationManager.OpenMachineConfiguration ("configTest");
			_Configuration c2 = ConfigurationManager.OpenMachineConfiguration ();

			Assert.AreEqual (c1.FilePath, c2.FilePath, "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void OpenMachineConfiguration_serverNull ()
		{
			_Configuration c1 = WebConfigurationManager.OpenMachineConfiguration ("configTest", null);
			_Configuration c2 = ConfigurationManager.OpenMachineConfiguration ();

			Assert.AreEqual (c1.FilePath, c2.FilePath, "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void OpenWebConfiguration_null ()
		{
			_Configuration web = WebConfigurationManager.OpenWebConfiguration (null);
			_Configuration machine = ConfigurationManager.OpenMachineConfiguration ();

			Assert.AreEqual ("web.config", Path.GetFileName (web.FilePath), "A1");
			Assert.AreEqual (Path.GetDirectoryName (web.FilePath), Path.GetDirectoryName (machine.FilePath), "A2");
		}

		[Test]
		[Category ("NotWorking")]
		public void OpenWebConfiguration_empty ()
		{
			_Configuration web1 = WebConfigurationManager.OpenWebConfiguration (null);
			_Configuration web2 = WebConfigurationManager.OpenWebConfiguration ("");
			_Configuration machine = ConfigurationManager.OpenMachineConfiguration ();

			Assert.AreEqual (web1.FilePath, web2.FilePath, "A1");
			Assert.AreEqual (Path.GetDirectoryName (web2.FilePath), Path.GetDirectoryName (machine.FilePath), "A2");
		}

		[Test]
		[Category ("NotWorking")]
		public void OpenWebConfiguration_siteNull ()
		{
			_Configuration web = WebConfigurationManager.OpenWebConfiguration ("", null);
			_Configuration machine = ConfigurationManager.OpenMachineConfiguration ();

			Assert.AreEqual ("web.config", Path.GetFileName (web.FilePath), "A1");
			Assert.AreEqual (Path.GetDirectoryName (web.FilePath), Path.GetDirectoryName (machine.FilePath), "A2");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void OpenWebConfiguration_siteNull2_absolutePath ()
		{
			WebConfigurationManager.OpenWebConfiguration ("", null, "/clientTest");
		}

		[Test]
		[Category ("NotWorking")]
		public void OpenWebConfiguration_siteNull2 ()
		{
			_Configuration web = WebConfigurationManager.OpenWebConfiguration ("", null, "clientTest");
			_Configuration machine = ConfigurationManager.OpenMachineConfiguration ();

			Assert.AreEqual ("web.config", Path.GetFileName (web.FilePath), "A1");
			Assert.AreEqual (Path.GetDirectoryName (web.FilePath), Path.GetDirectoryName (machine.FilePath), "A2");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetWebApplicationSection_1 ()
		{
			Assert.IsNotNull (WebConfigurationManager.GetWebApplicationSection ("system.web/clientTarget"), "A1");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetSection_1 ()
		{
			object sect1 = WebConfigurationManager.GetSection ("system.web/clientTarget");
			object sect2 = WebConfigurationManager.GetSection ("system.web/clientTarget");
			Assert.AreEqual (sect1, sect2, "A1");

			sect1 = WebConfigurationManager.GetSection ("foo");
			Assert.IsNull (sect1);

			sect1 = WebConfigurationManager.GetSection ("appSettings");
			Assert.IsNotNull (sect1, "A2");

			sect1 = WebConfigurationManager.GetSection ("connectionStrings");
			Assert.IsNotNull (sect1, "A3");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (InvalidOperationException))]
		// InvalidOperationException (WebConfigurationManager.GetSection(sectionName,path) can only be called from within a web application.)
		// thrown from WebConfigurationManager.GetSection
		public void GetSection_2 ()
		{
			object sect1 = WebConfigurationManager.GetSection ("system.web/clientTarget", "/clientTest");
			Assert.IsNull (sect1, "A1");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ClientTarget () {
			new WebTest (PageInvoker.CreateOnLoad (ClientTarget_load)).Run ();
		}

		public static void ClientTarget_load (Page p) {
			ClientTargetSection sec = (ClientTargetSection) WebConfigurationManager.GetSection ("system.web/clientTarget");
			ClientTarget clientTarget = sec.ClientTargets ["downlevel"];
			if (clientTarget == null)
				Assert.Fail ("ClientTarget Section: downlevel");
		}

		[Test]
		[Category ("NotWorking")]
		public void OpenMappedMachineConfiguration ()
		{
			ConfigurationFileMap map = new ConfigurationFileMap ();

			_Configuration c1 = WebConfigurationManager.OpenMappedMachineConfiguration (map, "clientTest");
			_Configuration c2 = ConfigurationManager.OpenMappedMachineConfiguration (map);

			Assert.AreEqual (c1.FilePath, c2.FilePath, "A1");
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		// same stack trace for OpenWebConfiguration_siteNull2_absolutePath.
		public void OpenMappedMachineConfiguration_absolute ()
		{
			ConfigurationFileMap map = new ConfigurationFileMap ();

			WebConfigurationManager.OpenMappedMachineConfiguration (map, "/clientTest");
		}

		[Test]
		public void StaticProps ()
		{
			Assert.IsNotNull (WebConfigurationManager.AppSettings, "A1");
			Assert.IsNotNull (WebConfigurationManager.ConnectionStrings, "A2");
		}

		[Test]
		public void CustomSectionEmptyCollection ()
		{
			WebTest t = new WebTest ("CustomSectionEmptyCollection.aspx");
			t.Run ();
		}
	}
}

#endif
