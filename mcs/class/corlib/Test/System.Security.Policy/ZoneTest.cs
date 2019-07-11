//
// ZoneTest.cs - NUnit Test Cases for Zone
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class ZoneTest  {

		[Test]
		public void MyComputer () 
		{
			Zone z = new Zone (SecurityZone.MyComputer);
			Assert.AreEqual (SecurityZone.MyComputer, z.SecurityZone, "MyComputer.SecurityZone");
			Assert.IsTrue ((z.ToString ().IndexOf ("<Zone>MyComputer</Zone>") >= 0), "MyComputer.ToString");
			Zone zc = (Zone) z.Copy ();
			Assert.IsTrue (z.Equals (zc), "MyComputer.Copy.Equals");
			IPermission p = z.CreateIdentityPermission (null);
			Assert.IsNotNull (p, "MyComputer.CreateIdentityPermission");

			Assert.IsTrue (z.Equals (new Zone (SecurityZone.MyComputer)), "MyComputer.MyComputer.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Intranet)), "MyComputer.Intranet.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Trusted)), "MyComputer.Trusted.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Internet)), "MyComputer.Internet.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Untrusted)), "MyComputer.Untrusted.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.NoZone)), "MyComputer.NoZone.Equals");
			Assert.IsTrue (!z.Equals (null), "MyComputer.Null.Equals");
		}

		[Test]
		public void Intranet () 
		{
			Zone z = new Zone (SecurityZone.Intranet);
			Assert.AreEqual (SecurityZone.Intranet, z.SecurityZone, "Intranet.SecurityZone");
			Assert.IsTrue ((z.ToString ().IndexOf ("<Zone>Intranet</Zone>") >= 0), "Intranet.ToString");
			Zone zc = (Zone) z.Copy ();
			Assert.IsTrue (z.Equals (zc), "Intranet.Copy.Equals");
			IPermission p = z.CreateIdentityPermission (null);
			Assert.IsNotNull (p, "Intranet.CreateIdentityPermission");

			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.MyComputer)), "Intranet.MyComputer.Equals");
			Assert.IsTrue (z.Equals (new Zone (SecurityZone.Intranet)), "Intranet.Intranet.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Trusted)), "Intranet.Trusted.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Internet)), "Intranet.Internet.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Untrusted)), "Intranet.Untrusted.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.NoZone)), "Intranet.NoZone.Equals");
			Assert.IsTrue (!z.Equals (null), "Intranet.Null.Equals");
		}

		[Test]
		public void Trusted () 
		{
			Zone z = new Zone (SecurityZone.Trusted);
			Assert.AreEqual (SecurityZone.Trusted, z.SecurityZone, "Trusted.SecurityZone");
			Assert.IsTrue ((z.ToString ().IndexOf ("<Zone>Trusted</Zone>") >= 0), "Trusted.ToString");
			Zone zc = (Zone) z.Copy ();
			Assert.IsTrue (z.Equals (zc), "Trusted.Copy.Equals");
			IPermission p = z.CreateIdentityPermission (null);
			Assert.IsNotNull (p, "Trusted.CreateIdentityPermission");

			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.MyComputer)), "Trusted.MyComputer.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Intranet)), "Trusted.Intranet.Equals");
			Assert.IsTrue (z.Equals (new Zone (SecurityZone.Trusted)), "Trusted.Trusted.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Internet)), "Trusted.Internet.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Untrusted)), "Trusted.Untrusted.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.NoZone)), "Trusted.NoZone.Equals");
			Assert.IsTrue (!z.Equals (null), "Trusted.Null.Equals");
		}

		[Test]
		public void Internet () 
		{
			Zone z = new Zone (SecurityZone.Internet);
			Assert.AreEqual (SecurityZone.Internet, z.SecurityZone, "Internet.SecurityZone");
			Assert.IsTrue ((z.ToString ().IndexOf ("<Zone>Internet</Zone>") >= 0), "Internet.ToString");
			Zone zc = (Zone) z.Copy ();
			Assert.IsTrue (z.Equals (zc), "Internet.Copy.Equals");
			IPermission p = z.CreateIdentityPermission (null);
			Assert.IsNotNull (p, "Internet.CreateIdentityPermission");

			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.MyComputer)), "Internet.MyComputer.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Intranet)), "Internet.Intranet.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Trusted)), "Internet.Trusted.Equals");
			Assert.IsTrue (z.Equals (new Zone (SecurityZone.Internet)), "Internet.Internet.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Untrusted)), "Internet.Untrusted.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.NoZone)), "Internet.NoZone.Equals");
			Assert.IsTrue (!z.Equals (null), "Internet.Null.Equals");
		}

		[Test]
		public void Untrusted () 
		{
			Zone z = new Zone (SecurityZone.Untrusted);
			Assert.AreEqual (SecurityZone.Untrusted, z.SecurityZone, "Untrusted.SecurityZone");
			Assert.IsTrue ((z.ToString ().IndexOf ("<Zone>Untrusted</Zone>") >= 0), "Untrusted.ToString");
			Zone zc = (Zone) z.Copy ();
			Assert.IsTrue (z.Equals (zc), "Untrusted.Copy.Equals");
			IPermission p = z.CreateIdentityPermission (null);
			Assert.IsNotNull (p, "Untrusted.CreateIdentityPermission");

			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.MyComputer)), "Untrusted.MyComputer.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Intranet)), "Untrusted.Intranet.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Trusted)), "Untrusted.Trusted.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Internet)), "Untrusted.Internet.Equals");
			Assert.IsTrue (z.Equals (new Zone (SecurityZone.Untrusted)), "Untrusted.Untrusted.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.NoZone)), "Untrusted.NoZone.Equals");
			Assert.IsTrue (!z.Equals (null), "Untrusted.Null.Equals");
		}

		[Test]
		public void NoZone () 
		{
			Zone z = new Zone (SecurityZone.NoZone);
			Assert.AreEqual (SecurityZone.NoZone, z.SecurityZone, "NoZone.SecurityZone");
			Assert.IsTrue ((z.ToString ().IndexOf ("<Zone>NoZone</Zone>") >= 0), "NoZone.ToString");
			Zone zc = (Zone) z.Copy ();
			Assert.IsTrue (z.Equals (zc), "NoZone.Copy.Equals");
			IPermission p = z.CreateIdentityPermission (null);
			Assert.IsNotNull (p, "NoZone.CreateIdentityPermission");
			// NoZone isn't added to the XML / string of permissions
			Assert.IsTrue (p.ToString ().IndexOf ("NoZone") < 0, "ToString!=NoZone");

			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.MyComputer)), "NoZone.MyComputer.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Intranet)), "NoZone.Intranet.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Trusted)), "NoZone.Trusted.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Internet)), "NoZone.Internet.Equals");
			Assert.IsTrue (!z.Equals (new Zone (SecurityZone.Untrusted)), "NoZone.Untrusted.Equals");
			Assert.IsTrue (z.Equals (new Zone (SecurityZone.NoZone)), "NoZone.NoZone.Equals");
			Assert.IsTrue (!z.Equals (null), "NoZone.Null.Equals");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateFromUrl_Null ()
		{
			Zone.CreateFromUrl (null);
		}

		string[] noZoneUrls = {
			String.Empty,			// not accepted for a Site
		};

		[Test]
		public void CreateFromUrl_NoZone ()
		{
			foreach (string url in noZoneUrls) {
				Zone z = Zone.CreateFromUrl (url);
				Assert.AreEqual (SecurityZone.NoZone, z.SecurityZone, url);
			}
		}

		// files are always rooted (Path.IsPathRooted) and exists (File.Exists)
		string[] myComputerUrls = {
			Path.GetTempFileName (),
#if !MONODROID
			// Assembly.Location doesn't work on Android
			Assembly.GetExecutingAssembly ().Location,
#endif
		};

		[Test]
		public void CreateFromUrl_MyComputer ()
		{
			foreach (string u in myComputerUrls) {
				string url = u;
				Zone z = Zone.CreateFromUrl (url);
				Assert.AreEqual (SecurityZone.MyComputer, z.SecurityZone, url);

				url = "file://" + u;
				z = Zone.CreateFromUrl (url);
				Assert.AreEqual (SecurityZone.MyComputer, z.SecurityZone, url);

				url = "FILE://" + u;
				z = Zone.CreateFromUrl (url);
				Assert.AreEqual (SecurityZone.MyComputer, z.SecurityZone, url);
			}
		}

		string[] intranetUrls = {
			"file://mono/index.html",	// file:// isn't supported as a site
			"FILE://MONO/INDEX.HTML",
		};

		[Test]
		public void CreateFromUrl_Intranet ()
		{
			foreach (string url in intranetUrls) {
				Zone z = Zone.CreateFromUrl (url);
				Assert.AreEqual (SecurityZone.Intranet, z.SecurityZone, url);
			}
		}

		string[] internetUrls = {
			"http://www.example.com",
			"http://64.14.94.188/",
			"HTTP://WWW.EXAMPLE.COM",
			"http://*.example.com",
			"http://www.example.com:8080/index.html",
			"mono://unknown/protocol",
			"/mono/index.html",
		};

		[Test]
		public void CreateFromUrl_Internet ()
		{
			foreach (string url in internetUrls) {
				Zone z = Zone.CreateFromUrl (url);
				Assert.AreEqual (SecurityZone.Internet, z.SecurityZone, url);
			}
		}

		[Test]
		public void ToString_ ()
		{
			Zone z = Zone.CreateFromUrl (String.Empty);
			string ts = z.ToString ();
			Assert.IsTrue (ts.StartsWith ("<System.Security.Policy.Zone"), "Class");
			Assert.IsTrue ((ts.IndexOf (" version=\"1\"") >= 0), "Version");
			Assert.IsTrue ((ts.IndexOf ("<Zone>NoZone</Zone>") >= 0), "Zone");
			Assert.IsTrue ((ts.IndexOf ("</System.Security.Policy.Zone>") >= 0), "End");
		}
	}
}
