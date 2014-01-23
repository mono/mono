//
// DnsPermissionTest.cs - NUnit Test Cases for DnsPermission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

#if !MOBILE

using NUnit.Framework;
using System;
using System.Net;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Net {

	[TestFixture]
	public class DnsPermissionTest {

		[Test]
		public void PermissionState_None ()
		{
			PermissionState ps = PermissionState.None;
			DnsPermission dp = new DnsPermission (ps);
			Assert.IsFalse (dp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = dp.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes#");
			Assert.IsNull (se.Children, "Xml-Children");

			DnsPermission copy = (DnsPermission)dp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (dp, copy), "ReferenceEquals");
			Assert.AreEqual (dp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		public void PermissionState_Unrestricted ()
		{
			PermissionState ps = PermissionState.Unrestricted;
			DnsPermission dp = new DnsPermission (ps);
			Assert.IsTrue (dp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = dp.ToXml ();
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Xml-Unrestricted");
			Assert.AreEqual (3, se.Attributes.Count, "Xml-Attributes#");
			Assert.IsNull (se.Children, "Xml-Children");

			DnsPermission copy = (DnsPermission)dp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (dp, copy), "ReferenceEquals");
			Assert.AreEqual (dp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		public void PermissionState_Bad ()
		{
			PermissionState ps = (PermissionState)Int32.MinValue;
			DnsPermission dp = new DnsPermission (ps);
			// no ArgumentException here
			Assert.IsFalse (dp.IsUnrestricted ());
		}

		[Test]
		public void Intersect ()
		{
			DnsPermission dpn = new DnsPermission (PermissionState.None);
			Assert.IsNull (dpn.Intersect (null), "None N null");
			Assert.IsNull (dpn.Intersect (dpn), "None N None");

			DnsPermission dpu = new DnsPermission (PermissionState.Unrestricted);
			Assert.IsNull (dpu.Intersect (null), "Unrestricted N null");

			DnsPermission result = (DnsPermission) dpu.Intersect (dpu);
			Assert.IsTrue (result.IsUnrestricted (), "Unrestricted N Unrestricted");

			Assert.IsNull (dpn.Intersect (dpu), "None N Unrestricted");
			Assert.IsNull (dpu.Intersect (dpn), "Unrestricted N None");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_BadPermission ()
		{
			DnsPermission dp = new DnsPermission (PermissionState.None);
			dp.Intersect (new SecurityPermission (PermissionState.Unrestricted));
		}

		[Test]
		public void IsSubset ()
		{
			DnsPermission dpn = new DnsPermission (PermissionState.None);
			DnsPermission dpu = new DnsPermission (PermissionState.Unrestricted);

			Assert.IsTrue (dpn.IsSubsetOf (null), "None IsSubsetOf null");
			Assert.IsFalse (dpu.IsSubsetOf (null), "Unrestricted IsSubsetOf null");

			Assert.IsTrue (dpn.IsSubsetOf (dpn), "None IsSubsetOf None");
			Assert.IsTrue (dpu.IsSubsetOf (dpu), "Unrestricted IsSubsetOf Unrestricted");

			Assert.IsTrue (dpn.IsSubsetOf (dpu), "None IsSubsetOf Unrestricted");
			Assert.IsFalse (dpu.IsSubsetOf (dpn), "Unrestricted IsSubsetOf None");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubset_BadPermission ()
		{
			DnsPermission dp = new DnsPermission (PermissionState.None);
			dp.IsSubsetOf (new SecurityPermission (PermissionState.Unrestricted));
		}

		[Test]
		public void Union ()
		{
			DnsPermission dpn = new DnsPermission (PermissionState.None);
			DnsPermission dpu = new DnsPermission (PermissionState.Unrestricted);
			
			DnsPermission result = (DnsPermission) dpn.Union (null);
			Assert.IsFalse (result.IsUnrestricted (), "None U null");
			
			result = (DnsPermission) dpu.Union (null);
			Assert.IsTrue (result.IsUnrestricted (), "Unrestricted U null");

			result = (DnsPermission) dpn.Union (dpn);
			Assert.IsFalse (result.IsUnrestricted (), "None U None");

			result = (DnsPermission) dpu.Union (dpu);
			Assert.IsTrue (result.IsUnrestricted (), "Unrestricted U Unrestricted");

			result = (DnsPermission) dpn.Union (dpu);
			Assert.IsTrue (result.IsUnrestricted (), "None U Unrestricted");

			result = (DnsPermission) dpu.Union (dpn);
			Assert.IsTrue (result.IsUnrestricted (), "Unrestricted U None");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_BadPermission ()
		{
			DnsPermission dp = new DnsPermission (PermissionState.None);
			dp.Union (new SecurityPermission (PermissionState.Unrestricted));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			DnsPermission dp = new DnsPermission (PermissionState.None);
			dp.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			DnsPermission dp = new DnsPermission (PermissionState.None);
			SecurityElement se = dp.ToXml ();
			se.Tag = "IMono";
			dp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			DnsPermission dp = new DnsPermission (PermissionState.None);
			SecurityElement se = dp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			dp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			DnsPermission dp = new DnsPermission (PermissionState.None);
			SecurityElement se = dp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			dp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_NoClass ()
		{
			DnsPermission dp = new DnsPermission (PermissionState.None);
			SecurityElement se = dp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			dp.FromXml (w);
			// note: normally IPermission classes (in corlib) DO NOT care about
			// attribute "class" name presence in the XML
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			DnsPermission dp = new DnsPermission (PermissionState.None);
			SecurityElement se = dp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			dp.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			DnsPermission dp = new DnsPermission (PermissionState.None);
			SecurityElement se = dp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			dp.FromXml (w);
		}
	}
}

#endif