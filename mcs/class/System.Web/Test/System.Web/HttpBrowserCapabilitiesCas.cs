//
// HttpBrowserCapabilitiesCas.cs 
//	- CAS unit tests for System.Web.HttpBrowserCapabilities
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using NUnit.Framework;

using System;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace MonoCasTests.System.Web {

	// note: this[string] is throwing NullReferenceException everywhere
	// so we "remove" it from action (only for full demands)
	class BoolHttpBrowserCapabilities : HttpBrowserCapabilities {

		public override string this [string key] {
			get { return (key == "platform") ? "Win32" : "true"; }
		}
	}

	class StringHttpBrowserCapabilities : HttpBrowserCapabilities {

		public override string this [string key] {
			get { return String.Empty; }
		}
	}

	class NumericHttpBrowserCapabilities : HttpBrowserCapabilities {

		public override string this [string key] {
			get { return "1"; }
		}
	}

	class VersionHttpBrowserCapabilities : HttpBrowserCapabilities {

		public override string this [string key] {
			get { return "1.2.3.4"; }
		}
	}

	[TestFixture]
	[Category ("CAS")]
	public class HttpBrowserCapabilitiesCas : AspNetHostingMinimal {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void BoolProperties_Deny_Unrestricted ()
		{
			HttpBrowserCapabilities cap = new BoolHttpBrowserCapabilities ();
			Assert.IsTrue (cap.ActiveXControls, "ActiveXControls");
			Assert.IsTrue (cap.AOL, "AOL");
			Assert.IsTrue (cap.BackgroundSounds, "BackgroundSounds");
			Assert.IsTrue (cap.Beta, "Beta");
			Assert.IsTrue (cap.CDF, "CDF");
			Assert.IsTrue (cap.Cookies, "Cookies");
			Assert.IsTrue (cap.Crawler, "Crawler");
			Assert.IsTrue (cap.Frames, "Frames");
			Assert.IsTrue (cap.JavaApplets, "JavaApplets");
			Assert.IsTrue (cap.JavaScript, "JavaScript");
			Assert.IsTrue (cap.Tables, "Tables");
			Assert.IsTrue (cap.VBScript, "VBScript");
			Assert.IsTrue (cap.Win16, "Win16");
			Assert.IsTrue (cap.Win32, "Win32");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StringProperties_Deny_Unrestricted ()
		{
			HttpBrowserCapabilities cap = new StringHttpBrowserCapabilities ();
			Assert.IsNotNull (cap.Browser, "Browser");
#if NET_2_0
			Assert.IsNull (cap.Browsers, "Browsers");
#endif
			Assert.IsNotNull (cap.Platform, "Platform");
			Assert.IsNotNull (cap.Type, "Type");
			Assert.IsNotNull (cap.Version, "Version");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void TypeProperties_Deny_Unrestricted ()
		{
			HttpBrowserCapabilities cap = new StringHttpBrowserCapabilities ();
			Type t = cap.TagWriter;
			// note: right now the value is hardcoded in Mono, i.e. it doesn't come from the ini file
			Assert.IsTrue (((t == null) || (t == typeof (HtmlTextWriter))), "TagWriter");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void NumericProperties_Deny_Unrestricted ()
		{
			HttpBrowserCapabilities cap = new NumericHttpBrowserCapabilities ();
			// int
			Assert.AreEqual (1, cap.MajorVersion, "MajorVersion");
			// double
			Assert.AreEqual (1, cap.MinorVersion, "MinorVersion");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void VersionProperties_Deny_Unrestricted ()
		{
			HttpBrowserCapabilities cap = new VersionHttpBrowserCapabilities ();
			Assert.IsNotNull (cap.ClrVersion, "ClrVersion");
			Assert.IsNotNull (cap.EcmaScriptVersion, "EcmaScriptVersion");
			Assert.IsNotNull (cap.MSDomVersion, "MSDomVersion");
			Assert.IsNotNull (cap.W3CDomVersion, "W3CDomVersion");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void GetClrVersions_Deny_Unrestricted ()
		{
			HttpBrowserCapabilities cap = new StringHttpBrowserCapabilities ();
#if NET_2_0
			Assert.IsNull (cap.GetClrVersions (), "GetClrVersions");
#else
			Version[] versions = cap.GetClrVersions ();
			Assert.AreEqual (1, versions.Length, "GetClrVersions");
			Assert.AreEqual ("0.0", versions [0].ToString (), "Version[0]");
#endif
		}

		// LinkDemand

		public override Type Type {
			get { return typeof (HttpBrowserCapabilities); }
		}
	}
}
