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
	public class ZoneTest : Assertion {

		[Test]
		public void MyComputer () 
		{
			Zone z = new Zone (SecurityZone.MyComputer);
			AssertEquals ("MyComputer.SecurityZone", SecurityZone.MyComputer, z.SecurityZone);
			Assert ("MyComputer.ToString", (z.ToString ().IndexOf ("<Zone>MyComputer</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("MyComputer.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("MyComputer.CreateIdentityPermission", p);

			Assert ("MyComputer.MyComputer.Equals", z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("MyComputer.Intranet.Equals", !z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("MyComputer.Trusted.Equals", !z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("MyComputer.Internet.Equals", !z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("MyComputer.Untrusted.Equals", !z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("MyComputer.NoZone.Equals", !z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("MyComputer.Null.Equals", !z.Equals (null));
		}

		[Test]
		public void Intranet () 
		{
			Zone z = new Zone (SecurityZone.Intranet);
			AssertEquals ("Intranet.SecurityZone", SecurityZone.Intranet, z.SecurityZone);
			Assert ("Intranet.ToString", (z.ToString ().IndexOf ("<Zone>Intranet</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("Intranet.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("Intranet.CreateIdentityPermission", p);

			Assert ("Intranet.MyComputer.Equals", !z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("Intranet.Intranet.Equals", z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("Intranet.Trusted.Equals", !z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("Intranet.Internet.Equals", !z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("Intranet.Untrusted.Equals", !z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("Intranet.NoZone.Equals", !z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("Intranet.Null.Equals", !z.Equals (null));
		}

		[Test]
		public void Trusted () 
		{
			Zone z = new Zone (SecurityZone.Trusted);
			AssertEquals ("Trusted.SecurityZone", SecurityZone.Trusted, z.SecurityZone);
			Assert ("Trusted.ToString", (z.ToString ().IndexOf ("<Zone>Trusted</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("Trusted.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("Trusted.CreateIdentityPermission", p);

			Assert ("Trusted.MyComputer.Equals", !z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("Trusted.Intranet.Equals", !z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("Trusted.Trusted.Equals", z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("Trusted.Internet.Equals", !z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("Trusted.Untrusted.Equals", !z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("Trusted.NoZone.Equals", !z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("Trusted.Null.Equals", !z.Equals (null));
		}

		[Test]
		public void Internet () 
		{
			Zone z = new Zone (SecurityZone.Internet);
			AssertEquals ("Internet.SecurityZone", SecurityZone.Internet, z.SecurityZone);
			Assert ("Internet.ToString", (z.ToString ().IndexOf ("<Zone>Internet</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("Internet.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("Internet.CreateIdentityPermission", p);

			Assert ("Internet.MyComputer.Equals", !z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("Internet.Intranet.Equals", !z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("Internet.Trusted.Equals", !z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("Internet.Internet.Equals", z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("Internet.Untrusted.Equals", !z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("Internet.NoZone.Equals", !z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("Internet.Null.Equals", !z.Equals (null));
		}

		[Test]
		public void Untrusted () 
		{
			Zone z = new Zone (SecurityZone.Untrusted);
			AssertEquals ("Untrusted.SecurityZone", SecurityZone.Untrusted, z.SecurityZone);
			Assert ("Untrusted.ToString", (z.ToString ().IndexOf ("<Zone>Untrusted</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("Untrusted.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("Untrusted.CreateIdentityPermission", p);

			Assert ("Untrusted.MyComputer.Equals", !z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("Untrusted.Intranet.Equals", !z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("Untrusted.Trusted.Equals", !z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("Untrusted.Internet.Equals", !z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("Untrusted.Untrusted.Equals", z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("Untrusted.NoZone.Equals", !z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("Untrusted.Null.Equals", !z.Equals (null));
		}

		[Test]
		public void NoZone () 
		{
			Zone z = new Zone (SecurityZone.NoZone);
			AssertEquals ("NoZone.SecurityZone", SecurityZone.NoZone, z.SecurityZone);
			Assert ("NoZone.ToString", (z.ToString ().IndexOf ("<Zone>NoZone</Zone>") >= 0));
			Zone zc = (Zone) z.Copy ();
			Assert ("NoZone.Copy.Equals", z.Equals (zc));
			IPermission p = z.CreateIdentityPermission (null);
			AssertNotNull ("NoZone.CreateIdentityPermission", p);
			// NoZone isn't added to the XML / string of permissions
			Assert ("ToString!=NoZone", p.ToString ().IndexOf ("NoZone") < 0);

			Assert ("NoZone.MyComputer.Equals", !z.Equals (new Zone (SecurityZone.MyComputer)));
			Assert ("NoZone.Intranet.Equals", !z.Equals (new Zone (SecurityZone.Intranet)));
			Assert ("NoZone.Trusted.Equals", !z.Equals (new Zone (SecurityZone.Trusted)));
			Assert ("NoZone.Internet.Equals", !z.Equals (new Zone (SecurityZone.Internet)));
			Assert ("NoZone.Untrusted.Equals", !z.Equals (new Zone (SecurityZone.Untrusted)));
			Assert ("NoZone.NoZone.Equals", z.Equals (new Zone (SecurityZone.NoZone)));
			Assert ("NoZone.Null.Equals", !z.Equals (null));
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
				AssertEquals (url, SecurityZone.NoZone, z.SecurityZone);
			}
		}

		// files are always rooted (Path.IsPathRooted) and exists (File.Exists)
		string[] myComputerUrls = {
			Path.GetTempFileName (),
			Assembly.GetExecutingAssembly ().Location,
		};

		[Test]
		public void CreateFromUrl_MyComputer ()
		{
			foreach (string u in myComputerUrls) {
				string url = u;
				Zone z = Zone.CreateFromUrl (url);
				AssertEquals (url, SecurityZone.MyComputer, z.SecurityZone);

				url = "file://" + u;
				z = Zone.CreateFromUrl (url);
				AssertEquals (url, SecurityZone.MyComputer, z.SecurityZone);

				url = "FILE://" + u;
				z = Zone.CreateFromUrl (url);
				AssertEquals (url, SecurityZone.MyComputer, z.SecurityZone);
			}
		}

		string[] intranetUrls = {
			"file://mono/index.html",	// file:// isn't supported as a site
			"FILE://MONO/INDEX.HTML",
			Path.DirectorySeparatorChar + "mono" + Path.DirectorySeparatorChar + "index.html",
		};

		[Test]
		public void CreateFromUrl_Intranet ()
		{
			foreach (string url in intranetUrls) {
				Zone z = Zone.CreateFromUrl (url);
				AssertEquals (url, SecurityZone.Intranet, z.SecurityZone);
			}
		}

		string[] internetUrls = {
			"http://www.go-mono.com",
			"http://64.14.94.188/",
			"HTTP://WWW.GO-MONO.COM",
			"http://*.go-mono.com",
			"http://www.go-mono.com:8080/index.html",
			"mono://unknown/protocol",
		};

		[Test]
		public void CreateFromUrl_Internet ()
		{
			foreach (string url in internetUrls) {
				Zone z = Zone.CreateFromUrl (url);
				AssertEquals (url, SecurityZone.Internet, z.SecurityZone);
			}
		}

		[Test]
		public void ToString_ ()
		{
			Zone z = Zone.CreateFromUrl (String.Empty);
			string ts = z.ToString ();
			Assert ("Class", ts.StartsWith ("<System.Security.Policy.Zone"));
			Assert ("Version", (ts.IndexOf (" version=\"1\"") >= 0));
			Assert ("Zone", (ts.IndexOf ("<Zone>NoZone</Zone>") >= 0));
			Assert ("End", (ts.IndexOf ("</System.Security.Policy.Zone>") >= 0));
		}
	}
}
