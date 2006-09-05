//
// GacIdentityPermissionTest.cs - NUnit Test Cases for GacIdentityPermission
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class GacIdentityPermissionTest {

		[Test]
		public void PermissionStateNone ()
		{
			GacIdentityPermission gip = new GacIdentityPermission (PermissionState.None);

			SecurityElement se = gip.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");

			GacIdentityPermission copy = (GacIdentityPermission)gip.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (gip, copy), "ReferenceEquals");
		}

		[Category ("NotWorking")]
		[Test]
		public void PermissionStateUnrestricted ()
		{
			GacIdentityPermission gip = new GacIdentityPermission (PermissionState.Unrestricted);

			// FX 2.0 now supports Unrestricted for Identity Permissions
			// However the XML doesn't show the Unrestricted status...

			SecurityElement se = gip.ToXml ();
			// only class and version are present
			Assert.AreEqual (2, se.Attributes.Count, "Xml-Attributes");
			Assert.IsNull (se.Children, "Xml-Children");

			GacIdentityPermission copy = (GacIdentityPermission)gip.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (gip, copy), "ReferenceEquals");

			// ... and because it doesn't implement IUnrestrictedPermission
			// there is not way to know if it's unrestricted so...
			Assert.IsTrue (gip.Equals (new GacIdentityPermission (PermissionState.None)), "Unrestricted==None");
			// there is not much difference after all ;-)
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionStateInvalid ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ((PermissionState)2);
		}

		[Test]
		public void GacIdentityPermission_Empty ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();
			Assert.IsNotNull (gip);
		}

		[Test]
		public void Intersect ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();

			GacIdentityPermission intersect = (GacIdentityPermission)gip.Intersect (null);
			Assert.IsNull (intersect, "gip N null");

			GacIdentityPermission empty = new GacIdentityPermission (PermissionState.None);
			intersect = (GacIdentityPermission)gip.Intersect (empty);
			Assert.IsNotNull (intersect, "gip N null");

			intersect = (GacIdentityPermission)gip.Intersect (gip);
			Assert.IsNotNull (intersect, "gip N gip");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Intersect_DifferentPermissions ()
		{
			GacIdentityPermission a = new GacIdentityPermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.Intersect (b);
		}

		[Test]
		public void IsSubsetOf ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();
			Assert.IsFalse (gip.IsSubsetOf (null), "gip.IsSubsetOf (null)");

			GacIdentityPermission empty = new GacIdentityPermission (PermissionState.None);
			Assert.IsFalse (empty.IsSubsetOf (null), "empty.IsSubsetOf (null)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOf_DifferentPermissions ()
		{
			GacIdentityPermission a = new GacIdentityPermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.IsSubsetOf (b);
		}

		[Test]
		public void Union ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();

			GacIdentityPermission union = (GacIdentityPermission)gip.Union (null);
			Assert.IsNotNull (union, "gip U null");

			GacIdentityPermission empty = new GacIdentityPermission (PermissionState.None);
			union = (GacIdentityPermission)gip.Union (empty);
			Assert.IsNotNull (union, "gip U empty");

			union = (GacIdentityPermission)gip.Union (gip);
			Assert.IsNotNull (union, "gip U gip");

			// note: can't be tested with PermissionState.Unrestricted
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_DifferentPermissions ()
		{
			GacIdentityPermission a = new GacIdentityPermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.Union (b);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();
			gip.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();
			SecurityElement se = gip.ToXml ();
			se.Tag = "IMono";
			gip.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();
			SecurityElement se = gip.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			gip.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();
			SecurityElement se = gip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			gip.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();
			SecurityElement se = gip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			gip.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();
			SecurityElement se = gip.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			gip.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			GacIdentityPermission gip = new GacIdentityPermission ();
			SecurityElement se = gip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			gip.FromXml (w);
		}
	}
}

#endif