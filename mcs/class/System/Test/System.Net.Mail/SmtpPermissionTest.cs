//
// SmtpPermissionTest.cs - NUnit Test Cases for SmtpPermission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Net.Mail;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Net.Mail {

	[TestFixture]
	public class SmtpPermissionTest {

		[Test]
		public void PermissionState_None ()
		{
			PermissionState ps = PermissionState.None;
			SmtpPermission sp = new SmtpPermission (ps);
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (SmtpAccess.None, sp.Access, "Access");

			SecurityElement se = sp.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes#");
			Assert.IsNull (se.Children, "Xml-Children");

			SmtpPermission copy = (SmtpPermission) sp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (sp, copy), "ReferenceEquals");
			Assert.AreEqual (sp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
			Assert.AreEqual (sp.Access, copy.Access, "copy.Access");
		}

		[Test]
		public void PermissionState_Unrestricted ()
		{
			PermissionState ps = PermissionState.Unrestricted;
			SmtpPermission sp = new SmtpPermission (ps);
			Assert.IsTrue (sp.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, sp.Access, "Access");

			SecurityElement se = sp.ToXml ();
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Xml-Unrestricted");
			Assert.AreEqual (3, se.Attributes.Count, "Xml-Attributes#");
			Assert.IsNull (se.Children, "Xml-Children");

			SmtpPermission copy = (SmtpPermission) sp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (sp, copy), "ReferenceEquals");
			Assert.AreEqual (sp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
			Assert.AreEqual (sp.Access, copy.Access, "copy.Access");
		}

		[Test]
		public void PermissionState_Bad ()
		{
			PermissionState ps = (PermissionState) Int32.MinValue;
			SmtpPermission sp = new SmtpPermission (ps);
			// no ArgumentException here
			Assert.IsFalse (sp.IsUnrestricted ());
			Assert.AreEqual (SmtpAccess.None, sp.Access, "Access");
		}

		[Test]
		public void Ctor_Boolean_True ()
		{
			SmtpPermission sp = new SmtpPermission (true);
			Assert.IsTrue (sp.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, sp.Access, "Access");

			SecurityElement se = sp.ToXml ();
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Xml-Unrestricted");
			Assert.AreEqual (3, se.Attributes.Count, "Xml-Attributes#");
			Assert.IsNull (se.Children, "Xml-Children");
		}

		[Test]
		public void Ctor_Boolean_False ()
		{
			SmtpPermission sp = new SmtpPermission (false);
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (SmtpAccess.None, sp.Access, "Access");

			SecurityElement se = sp.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes#");
			Assert.IsNull (se.Children, "Xml-Children");
		}

		[Test]
		public void Ctor_SmtpAccess_None ()
		{
			SmtpPermission sp = new SmtpPermission (SmtpAccess.None);
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (SmtpAccess.None, sp.Access, "Access");

			SecurityElement se = sp.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes#");
			Assert.IsNull (se.Children, "Xml-Children");
		}

		[Test]
		public void Ctor_SmtpAccess_Connect ()
		{
			SmtpPermission sp = new SmtpPermission (SmtpAccess.Connect);
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (SmtpAccess.Connect, sp.Access, "Access");

			SecurityElement se = sp.ToXml ();
			// only class and version are present
			Assert.AreEqual (3, se.Attributes.Count, "Xml-Attributes#");
			Assert.AreEqual ("Connect", se.Attribute ("Access"), "Xml-Access");
			Assert.IsNull (se.Children, "Xml-Children");
		}

		[Test]
		public void Ctor_SmtpAccess_ConnectToUnrestrictedPort ()
		{
			SmtpPermission sp = new SmtpPermission (SmtpAccess.ConnectToUnrestrictedPort);
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, sp.Access, "Access");

			SecurityElement se = sp.ToXml ();
			// only class and version are present
			Assert.AreEqual (3, se.Attributes.Count, "Xml-Attributes#");
			Assert.AreEqual ("ConnectToUnrestrictedPort", se.Attribute ("Access"), "Xml-Access");
			Assert.IsNull (se.Children, "Xml-Children");
		}

		[Test]
		public void Ctor_SmtpAccess_Invalid ()
		{
			SmtpAccess sa = (SmtpAccess)Int32.MinValue;
			SmtpPermission sp = new SmtpPermission (sa);
			// no exception
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");
			Assert.AreEqual (sa, sp.Access, "Access");

			// invalid access doesn't get serialized to XML
			SecurityElement se = sp.ToXml ();
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes#");
			Assert.IsNull (se.Children, "Xml-Children");

			// but it doesn't roundtrip
			SmtpPermission copy = (SmtpPermission) sp.Copy ();
			Assert.AreEqual (sp.Access, copy.Access, "copy.Access");
		}

		[Test]
		public void AddPermission ()
		{
			SmtpPermission sp = new SmtpPermission (false);
			Assert.AreEqual (SmtpAccess.None, sp.Access, "Access-default");
			sp.AddPermission (SmtpAccess.Connect);
			Assert.AreEqual (SmtpAccess.Connect, sp.Access, "Connect");
			sp.AddPermission (SmtpAccess.ConnectToUnrestrictedPort);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, sp.Access, "ConnectToUnrestrictedPort");
		}

		[Test]
		public void AddPermission_Unrestricted ()
		{
			SmtpPermission sp = new SmtpPermission (true);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, sp.Access, "Access-default");
			sp.AddPermission (SmtpAccess.None);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, sp.Access, "ConnectToUnrestrictedPort");
		}

		[Test]
		public void AddPermission_Invalid ()
		{
			SmtpAccess sa = (SmtpAccess) Int32.MinValue;
			SmtpPermission sp = new SmtpPermission (false);
			sp.AddPermission (sa);
			Assert.AreEqual (SmtpAccess.None, sp.Access, "None");
		}

		[Test]
		public void Intersect ()
		{
			SmtpPermission spn = new SmtpPermission (PermissionState.None);
			Assert.IsNull (spn.Intersect (null), "None N null");
			SmtpPermission inter = (SmtpPermission) spn.Intersect (spn);
			Assert.AreEqual (SmtpAccess.None, inter.Access, "None N None");

			SmtpPermission spu = new SmtpPermission (PermissionState.Unrestricted);
			Assert.IsNull (spu.Intersect (null), "Unrestricted N null");

			SmtpPermission result = (SmtpPermission) spu.Intersect (spu);
			Assert.IsTrue (result.IsUnrestricted (), "Unrestricted N Unrestricted");

			inter = (SmtpPermission) spn.Intersect (spu);
			Assert.AreEqual (SmtpAccess.None, inter.Access, "None N Unrestricted");

			inter = (SmtpPermission) spu.Intersect (spn);
			Assert.AreEqual (SmtpAccess.None, inter.Access, "Unrestricted N None");
		}

		[Test]
		public void Intersect_SmtpAccess ()
		{
			SmtpPermission spn = new SmtpPermission (false);
			SmtpPermission spu = new SmtpPermission (true);
			SmtpPermission spctup = new SmtpPermission (SmtpAccess.ConnectToUnrestrictedPort);
			SmtpPermission spconnect = new SmtpPermission (SmtpAccess.Connect);
			SmtpPermission spnone = new SmtpPermission (SmtpAccess.None);

			SmtpPermission intersect = (SmtpPermission) spn.Intersect (spctup);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "None N ConnectToUnrestrictedPort");
			intersect = (SmtpPermission) spn.Intersect (spconnect);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "None N Connect");
			intersect = (SmtpPermission) spn.Intersect (spnone);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "None N SmtpAccess.None");

			intersect = (SmtpPermission) spu.Intersect (spctup);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, intersect.Access, "Unrestricted N ConnectToUnrestrictedPort");
			intersect = (SmtpPermission) spu.Intersect (spconnect);
			Assert.AreEqual (SmtpAccess.Connect, intersect.Access, "Unrestricted N Connect");
			intersect = (SmtpPermission) spu.Intersect (spnone);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "Unrestricted N SmtpAccess.None");

			intersect = (SmtpPermission) spctup.Intersect (spctup);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, intersect.Access, "ConnectToUnrestrictedPort N ConnectToUnrestrictedPort");
			intersect = (SmtpPermission) spctup.Intersect (spconnect);
			Assert.AreEqual (SmtpAccess.Connect, intersect.Access, "ConnectToUnrestrictedPort N Connect");
			intersect = (SmtpPermission) spctup.Intersect (spnone);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "ConnectToUnrestrictedPort N SmtpAccess.None");
			intersect = (SmtpPermission) spctup.Intersect (spn);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "ConnectToUnrestrictedPort N None");
			intersect = (SmtpPermission) spctup.Intersect (spu);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, intersect.Access, "ConnectToUnrestrictedPort N Unrestricted");

			intersect = (SmtpPermission) spconnect.Intersect (spctup);
			Assert.AreEqual (SmtpAccess.Connect, intersect.Access, "Connect N ConnectToUnrestrictedPort");
			intersect = (SmtpPermission) spconnect.Intersect (spconnect);
			Assert.AreEqual (SmtpAccess.Connect, intersect.Access, "Connect N Connect");
			intersect = (SmtpPermission) spconnect.Intersect (spnone);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "Connect N SmtpAccess.None");
			intersect = (SmtpPermission) spconnect.Intersect (spn);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "Connect N None");
			intersect = (SmtpPermission) spconnect.Intersect (spu);
			Assert.AreEqual (SmtpAccess.Connect, intersect.Access, "Connect N Unrestricted");

			intersect = (SmtpPermission) spnone.Intersect (spctup);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "SmtpAccess.None N ConnectToUnrestrictedPort");
			intersect = (SmtpPermission) spnone.Intersect (spconnect);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "SmtpAccess.None N Connect");
			intersect = (SmtpPermission) spnone.Intersect (spnone);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "SmtpAccess.None N SmtpAccess.None");
			intersect = (SmtpPermission) spnone.Intersect (spn);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "SmtpAccess.None N None");
			intersect = (SmtpPermission) spnone.Intersect (spu);
			Assert.AreEqual (SmtpAccess.None, intersect.Access, "SmtpAccess.None N Unrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_BadPermission ()
		{
			SmtpPermission sp = new SmtpPermission (PermissionState.None);
			sp.Intersect (new SecurityPermission (PermissionState.Unrestricted));
		}

		[Test]
		public void IsSubset ()
		{
			SmtpPermission spn = new SmtpPermission (PermissionState.None);
			SmtpPermission spu = new SmtpPermission (PermissionState.Unrestricted);

			Assert.IsTrue (spn.IsSubsetOf (null), "None IsSubsetOf null");
			Assert.IsFalse (spu.IsSubsetOf (null), "Unrestricted IsSubsetOf null");

			Assert.IsTrue (spn.IsSubsetOf (spn), "None IsSubsetOf None");
			Assert.IsTrue (spu.IsSubsetOf (spu), "Unrestricted IsSubsetOf Unrestricted");

			Assert.IsTrue (spn.IsSubsetOf (spu), "None IsSubsetOf Unrestricted");
			Assert.IsFalse (spu.IsSubsetOf (spn), "Unrestricted IsSubsetOf None");
		}

		[Test]
		public void IsSubset_SmtpAccess ()
		{
			SmtpPermission spn = new SmtpPermission (false);
			SmtpPermission spu = new SmtpPermission (true);
			SmtpPermission spctup = new SmtpPermission (SmtpAccess.ConnectToUnrestrictedPort);
			SmtpPermission spconnect = new SmtpPermission (SmtpAccess.Connect);
			SmtpPermission spnone = new SmtpPermission (SmtpAccess.None);

			Assert.IsTrue (spn.IsSubsetOf (spctup), "None IsSubsetOf ConnectToUnrestrictedPort");
			Assert.IsTrue (spn.IsSubsetOf (spconnect), "None IsSubsetOf Connect");
			Assert.IsTrue (spn.IsSubsetOf (spnone), "None IsSubsetOf SmtpAccess.None");

			Assert.IsFalse (spu.IsSubsetOf (spctup), "Unrestricted IsSubsetOf ConnectToUnrestrictedPort");
			Assert.IsFalse (spu.IsSubsetOf (spconnect), "Unrestricted IsSubsetOf Connect");
			Assert.IsFalse (spu.IsSubsetOf (spnone), "Unrestricted IsSubsetOf SmtpAccess.None");

			Assert.IsTrue (spctup.IsSubsetOf (spctup), "ConnectToUnrestrictedPort IsSubsetOf ConnectToUnrestrictedPort");
			Assert.IsFalse (spctup.IsSubsetOf (spconnect), "ConnectToUnrestrictedPort IsSubsetOf Connect");
			Assert.IsFalse (spctup.IsSubsetOf (spnone), "ConnectToUnrestrictedPort IsSubsetOf SmtpAccess.None");
			Assert.IsFalse (spctup.IsSubsetOf (spn), "ConnectToUnrestrictedPort IsSubsetOf None");
			Assert.IsTrue (spctup.IsSubsetOf (spu), "ConnectToUnrestrictedPort IsSubsetOf Unrestricted");

			Assert.IsTrue (spconnect.IsSubsetOf (spctup), "Connect IsSubsetOf ConnectToUnrestrictedPort");
			Assert.IsTrue (spconnect.IsSubsetOf (spconnect), "Connect IsSubsetOf Connect");
			Assert.IsFalse (spconnect.IsSubsetOf (spnone), "Connect IsSubsetOf SmtpAccess.None");
			Assert.IsFalse (spconnect.IsSubsetOf (spn), "Connect IsSubsetOf None");
			Assert.IsTrue (spconnect.IsSubsetOf (spu), "Connect IsSubsetOf Unrestricted");

			Assert.IsTrue (spnone.IsSubsetOf (spctup), "SmtpAccess.None IsSubsetOf ConnectToUnrestrictedPort");
			Assert.IsTrue (spnone.IsSubsetOf (spconnect), "SmtpAccess.None IsSubsetOf Connect");
			Assert.IsTrue (spnone.IsSubsetOf (spnone), "SmtpAccess.None IsSubsetOf SmtpAccess.None");
			Assert.IsTrue (spnone.IsSubsetOf (spn), "SmtpAccess.None IsSubsetOf None");
			Assert.IsTrue (spnone.IsSubsetOf (spu), "SmtpAccess.None IsSubsetOf Unrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubset_BadPermission ()
		{
			SmtpPermission sp = new SmtpPermission (PermissionState.None);
			sp.IsSubsetOf (new SecurityPermission (PermissionState.Unrestricted));
		}

		[Test]
		public void Union ()
		{
			SmtpPermission spn = new SmtpPermission (PermissionState.None);
			SmtpPermission spu = new SmtpPermission (PermissionState.Unrestricted);

			SmtpPermission result = (SmtpPermission) spn.Union (null);
			Assert.IsFalse (result.IsUnrestricted (), "None U null");

			result = (SmtpPermission) spu.Union (null);
			Assert.IsTrue (result.IsUnrestricted (), "Unrestricted U null");

			result = (SmtpPermission) spn.Union (spn);
			Assert.IsFalse (result.IsUnrestricted (), "None U None");

			result = (SmtpPermission) spu.Union (spu);
			Assert.IsTrue (result.IsUnrestricted (), "Unrestricted U Unrestricted");

			result = (SmtpPermission) spn.Union (spu);
			Assert.IsTrue (result.IsUnrestricted (), "None U Unrestricted");

			result = (SmtpPermission) spu.Union (spn);
			Assert.IsTrue (result.IsUnrestricted (), "Unrestricted U None");
		}

		[Test]
		public void Union_SmtpAccess ()
		{
			SmtpPermission spn = new SmtpPermission (false);
			SmtpPermission spu = new SmtpPermission (true);
			SmtpPermission spctup = new SmtpPermission (SmtpAccess.ConnectToUnrestrictedPort);
			SmtpPermission spconnect = new SmtpPermission (SmtpAccess.Connect);
			SmtpPermission spnone = new SmtpPermission (SmtpAccess.None);

			SmtpPermission union = (SmtpPermission) spn.Union (spctup);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "None U ConnectToUnrestrictedPort");
			union = (SmtpPermission) spn.Union (spconnect);
			Assert.AreEqual (SmtpAccess.Connect, union.Access, "None U Connect");
			union = (SmtpPermission) spn.Union (spnone);
			Assert.AreEqual (SmtpAccess.None, union.Access, "None U SmtpAccess.None");

			union = (SmtpPermission) spu.Union (spctup);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "Unrestricted U ConnectToUnrestrictedPort");
			union = (SmtpPermission) spu.Union (spconnect);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "Unrestricted U Connect");
			union = (SmtpPermission) spu.Union (spnone);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "Unrestricted U SmtpAccess.None");

			union = (SmtpPermission) spctup.Union (spctup);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "ConnectToUnrestrictedPort U ConnectToUnrestrictedPort");
			union = (SmtpPermission) spctup.Union (spconnect);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "ConnectToUnrestrictedPort U Connect");
			union = (SmtpPermission) spctup.Union (spnone);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "ConnectToUnrestrictedPort U SmtpAccess.None");
			union = (SmtpPermission) spctup.Union (spn);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "ConnectToUnrestrictedPort U None");
			union = (SmtpPermission) spctup.Union (spu);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "ConnectToUnrestrictedPort U Unrestricted");

			union = (SmtpPermission) spconnect.Union (spctup);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "Connect U ConnectToUnrestrictedPort");
			union = (SmtpPermission) spconnect.Union (spconnect);
			Assert.AreEqual (SmtpAccess.Connect, union.Access, "Connect U Connect");
			union = (SmtpPermission) spconnect.Union (spnone);
			Assert.AreEqual (SmtpAccess.Connect, union.Access, "Connect U SmtpAccess.None");
			union = (SmtpPermission) spconnect.Union (spn);
			Assert.AreEqual (SmtpAccess.Connect, union.Access, "Connect U None");
			union = (SmtpPermission) spconnect.Union (spu);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "Connect U Unrestricted");

			union = (SmtpPermission) spnone.Union (spctup);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "SmtpAccess.None U ConnectToUnrestrictedPort");
			union = (SmtpPermission) spnone.Union (spconnect);
			Assert.AreEqual (SmtpAccess.Connect, union.Access, "SmtpAccess.None U Connect");
			union = (SmtpPermission) spnone.Union (spnone);
			Assert.AreEqual (SmtpAccess.None, union.Access, "SmtpAccess.None U SmtpAccess.None");
			union = (SmtpPermission) spnone.Union (spn);
			Assert.AreEqual (SmtpAccess.None, union.Access, "SmtpAccess.None U None");
			union = (SmtpPermission) spnone.Union (spu);
			Assert.AreEqual (SmtpAccess.ConnectToUnrestrictedPort, union.Access, "SmtpAccess.None U Unrestricted");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_BadPermission ()
		{
			SmtpPermission sp = new SmtpPermission (PermissionState.None);
			sp.Union (new SecurityPermission (PermissionState.Unrestricted));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			SmtpPermission sp = new SmtpPermission (PermissionState.None);
			sp.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			SmtpPermission sp = new SmtpPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();
			se.Tag = "IMono";
			sp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			SmtpPermission sp = new SmtpPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			sp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			SmtpPermission sp = new SmtpPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			sp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_NoClass ()
		{
			SmtpPermission sp = new SmtpPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			sp.FromXml (w);
			// note: normally IPermission classes (in corlib) DO NOT care about
			// attribute "class" name presence in the XML
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			SmtpPermission sp = new SmtpPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			sp.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			SmtpPermission sp = new SmtpPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			sp.FromXml (w);
		}
	}
}

#endif
