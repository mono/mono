//
// SecurityPermissionTest.cs - NUnit Test Cases for SecurityPermission
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

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

using System.Diagnostics;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	[Category("NotDotNet")]
	public class SecurityPermissionTest {

		static SecurityPermissionFlag [] AllFlags = {
			SecurityPermissionFlag.AllFlags,
			SecurityPermissionFlag.Assertion,
			SecurityPermissionFlag.BindingRedirects,
			SecurityPermissionFlag.ControlAppDomain,
			SecurityPermissionFlag.ControlDomainPolicy,
			SecurityPermissionFlag.ControlEvidence,
			SecurityPermissionFlag.ControlPolicy,
			SecurityPermissionFlag.ControlPrincipal,
			SecurityPermissionFlag.ControlThread,
			SecurityPermissionFlag.Execution,
			SecurityPermissionFlag.Infrastructure,
			SecurityPermissionFlag.NoFlags,
			SecurityPermissionFlag.RemotingConfiguration,
			SecurityPermissionFlag.SerializationFormatter,
			SecurityPermissionFlag.SkipVerification,
			SecurityPermissionFlag.UnmanagedCode };

		static SecurityPermissionFlag [] AllFlagsExceptNoFlags = {
			SecurityPermissionFlag.AllFlags,
			SecurityPermissionFlag.Assertion,
			SecurityPermissionFlag.BindingRedirects,
			SecurityPermissionFlag.ControlAppDomain,
			SecurityPermissionFlag.ControlDomainPolicy,
			SecurityPermissionFlag.ControlEvidence,
			SecurityPermissionFlag.ControlPolicy,
			SecurityPermissionFlag.ControlPrincipal,
			SecurityPermissionFlag.ControlThread,
			SecurityPermissionFlag.Execution,
			SecurityPermissionFlag.Infrastructure,
			SecurityPermissionFlag.RemotingConfiguration,
			SecurityPermissionFlag.SerializationFormatter,
			SecurityPermissionFlag.SkipVerification,
			SecurityPermissionFlag.UnmanagedCode };

		static SecurityPermissionFlag [] AllFlagsExceptAllFlags = {
			SecurityPermissionFlag.Assertion,
			SecurityPermissionFlag.BindingRedirects,
			SecurityPermissionFlag.ControlAppDomain,
			SecurityPermissionFlag.ControlDomainPolicy,
			SecurityPermissionFlag.ControlEvidence,
			SecurityPermissionFlag.ControlPolicy,
			SecurityPermissionFlag.ControlPrincipal,
			SecurityPermissionFlag.ControlThread,
			SecurityPermissionFlag.Execution,
			SecurityPermissionFlag.Infrastructure,
			SecurityPermissionFlag.NoFlags,
			SecurityPermissionFlag.RemotingConfiguration,
			SecurityPermissionFlag.SerializationFormatter,
			SecurityPermissionFlag.SkipVerification,
			SecurityPermissionFlag.UnmanagedCode };

		[Test]
		public void PermissionState_None ()
		{
			PermissionState ps = PermissionState.None;
			SecurityPermission sp = new SecurityPermission (ps);
			Assert.AreEqual (SecurityPermissionFlag.NoFlags, sp.Flags, "Flags");
			Assert.IsFalse (sp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = sp.ToXml ();
			// only class and version are present
			Assert.AreEqual ("NoFlags", se.Attribute ("Flags"), "Xml-Flags");
			Assert.IsNull (se.Children, "Xml-Children");

			SecurityPermission copy = (SecurityPermission)sp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (sp, copy), "ReferenceEquals");
			Assert.AreEqual (sp.Flags, copy.Flags, "Flags");
			Assert.AreEqual (sp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		public void PermissionState_Unrestricted ()
		{
			PermissionState ps = PermissionState.Unrestricted;
			SecurityPermission sp = new SecurityPermission (ps);
			Assert.AreEqual (SecurityPermissionFlag.AllFlags, sp.Flags, "Flags");
			Assert.IsTrue (sp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = sp.ToXml ();
			// only class and version are present
			Assert.AreEqual ("true", se.Attribute ("Unrestricted"), "Xml-Unrestricted");
			Assert.IsNull (se.Children, "Xml-Children");

			SecurityPermission copy = (SecurityPermission)sp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (sp, copy), "ReferenceEquals");
			Assert.AreEqual (sp.Flags, copy.Flags, "Flags");
			Assert.AreEqual (sp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionState_Bad ()
		{
			PermissionState ps = (PermissionState)77;
			SecurityPermission sp = new SecurityPermission (ps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SecurityPermissionFlags_Bad ()
		{
			SecurityPermissionFlag spf = (SecurityPermissionFlag)(SecurityPermissionFlag.AllFlags + 1);
			SecurityPermission sp = new SecurityPermission (spf);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Flags_SecurityPermissionFlags_Bad ()
		{
			SecurityPermissionFlag spf = (SecurityPermissionFlag)(SecurityPermissionFlag.AllFlags + 1);
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			sp.Flags = spf;
		}

		[Test]
		public void Copy ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			foreach (SecurityPermissionFlag spf in AllFlags) {
				sp.Flags = spf;
				SecurityPermission copy = (SecurityPermission) sp.Copy ();
				Assert.AreEqual (spf, copy.Flags, spf.ToString ());
			}
		}

		[Test]
		public void Intersect_Null ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			// No intersection with null
			foreach (SecurityPermissionFlag spf in AllFlags) {
				sp.Flags = spf;
				Assert.IsNull (sp.Intersect (null), spf.ToString ());
			}
		}

		[Test]
		public void Intersect_None ()
		{
			SecurityPermission sp1 = new SecurityPermission (PermissionState.None);
			SecurityPermission sp2 = new SecurityPermission (PermissionState.None);
			foreach (SecurityPermissionFlag spf in AllFlagsExceptNoFlags) {
				sp2.Flags = spf;
				// 1. Intersect None with spf
				SecurityPermission result = (SecurityPermission)sp1.Intersect (sp2);
				Assert.IsNull (result, "None N " + spf.ToString ());
				// 2. Intersect spf with None
				result = (SecurityPermission)sp2.Intersect (sp1);
				Assert.IsNull (result, "None N " + spf.ToString ());
			}
		}

		[Test]
		public void Intersect_Self ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			foreach (SecurityPermissionFlag spf in AllFlagsExceptNoFlags) {
				sp.Flags = spf;
				SecurityPermission result = (SecurityPermission)sp.Intersect (sp);
				Assert.AreEqual (spf, result.Flags, spf.ToString ());
			}
		}

		[Test]
		public void Intersect_Unrestricted ()
		{
			// Intersection with unrestricted == Copy
			// a. source (this) is unrestricted
			SecurityPermission sp1 = new SecurityPermission (PermissionState.Unrestricted);
			SecurityPermission sp2 = new SecurityPermission (PermissionState.None);
			foreach (SecurityPermissionFlag spf in AllFlagsExceptNoFlags) {
				sp2.Flags = spf;
				SecurityPermission result = (SecurityPermission) sp1.Intersect (sp2);
				Assert.AreEqual (sp2.Flags, result.Flags, "target " + spf.ToString ());
			}
			// b. destination (target) is unrestricted
			foreach (SecurityPermissionFlag spf in AllFlagsExceptNoFlags) {
				sp2.Flags = spf;
				SecurityPermission result = (SecurityPermission)sp2.Intersect (sp1);
				Assert.AreEqual (sp2.Flags, result.Flags, "source " + spf.ToString ());
			}
			// exceptions for NoFlags
			sp2.Flags = SecurityPermissionFlag.NoFlags;
			Assert.IsNull (sp1.Intersect (sp2), "target NoFlags");
			Assert.IsNull (sp2.Intersect (sp1), "source NoFlags");
		}

		[Test]
		public void IsSubset_Null () 
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			Assert.IsTrue (sp.IsSubsetOf (null), "NoFlags");
			foreach (SecurityPermissionFlag spf in AllFlagsExceptNoFlags) {
				sp.Flags = spf;
				Assert.IsFalse (sp.IsSubsetOf (null), spf.ToString ());
			}
		}

		[Test]
		public void IsSubset_None ()
		{
			// IsSubset with none
			// a. source (this) is none -> target is never a subset
			SecurityPermission sp1 = new SecurityPermission (PermissionState.None);
			SecurityPermission sp2 = new SecurityPermission (PermissionState.None);
			foreach (SecurityPermissionFlag spf in AllFlags) {
				sp2.Flags = spf;
				Assert.IsTrue (sp1.IsSubsetOf (sp2), "target " + spf.ToString ());
			}
			// b. destination (target) is none -> target is always a subset
			foreach (SecurityPermissionFlag spf in AllFlagsExceptNoFlags) {
				sp2.Flags = spf;
				Assert.IsFalse (sp2.IsSubsetOf (sp1), "source " + spf.ToString ());
			}
			// exception of NoFlags
			sp2.Flags = SecurityPermissionFlag.NoFlags;
			Assert.IsTrue (sp2.IsSubsetOf (sp1), "source NoFlags");
		}

		[Test]
		public void IsSubset_Self ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			foreach (SecurityPermissionFlag spf in AllFlags) {
				sp.Flags = spf;
				SecurityPermission result = (SecurityPermission)sp.Intersect (sp);
				Assert.IsTrue (sp.IsSubsetOf (sp), spf.ToString ());
			}
		}

		[Test]
		public void IsSubset_Unrestricted () 
		{
			// IsSubset with unrestricted
			// a. source (this) is unrestricted -> target is never a subset
			SecurityPermission sp1 = new SecurityPermission (PermissionState.Unrestricted);
			SecurityPermission sp2 = new SecurityPermission (PermissionState.None);
			foreach (SecurityPermissionFlag spf in AllFlagsExceptAllFlags) {
				sp2.Flags = spf;
				Assert.IsFalse (sp1.IsSubsetOf (sp2), "target " + spf.ToString ());
			}
			// exception of AllFlags
			sp2.Flags = SecurityPermissionFlag.AllFlags;
			Assert.IsTrue (sp1.IsSubsetOf (sp2), "target AllFlags");
			// b. destination (target) is unrestricted -> target is always a subset
			foreach (SecurityPermissionFlag spf in AllFlags) {
				sp2.Flags = spf;
				Assert.IsTrue (sp2.IsSubsetOf (sp1), "source " + spf.ToString ());
			}
		}

		[Test]
		public void Union_Null ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			// Union with null is a simple copy
			foreach (SecurityPermissionFlag spf in AllFlags) {
				sp.Flags = spf;
				SecurityPermission union = (SecurityPermission) sp.Union (null);
				Assert.AreEqual (spf, union.Flags, spf.ToString ());
			}
		}

		[Test]
		public void Union_None ()
		{
			// Union with none is same
			SecurityPermission sp1 = new SecurityPermission (PermissionState.None);
			SecurityPermission sp2 = new SecurityPermission (PermissionState.None);
			// a. source (this) is none
			foreach (SecurityPermissionFlag spf in AllFlagsExceptAllFlags) {
				sp2.Flags = spf;
				SecurityPermission union = (SecurityPermission)sp1.Union (sp2);
				Assert.IsFalse (union.IsUnrestricted (), "target " + spf.ToString ());
			}
			// b. destination (target) is none
			foreach (SecurityPermissionFlag spf in AllFlagsExceptAllFlags) {
				sp2.Flags = spf;
				SecurityPermission union = (SecurityPermission)sp2.Union (sp1);
				Assert.IsFalse (union.IsUnrestricted (), "source " + spf.ToString ());
			}
		}

		[Test]
		public void Union_Self ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			foreach (SecurityPermissionFlag spf in AllFlags) {
				sp.Flags = spf;
				SecurityPermission result = (SecurityPermission)sp.Union (sp);
				Assert.AreEqual (spf, result.Flags, spf.ToString ());
			}
		}

		[Test]
		public void Union_Unrestricted ()
		{
			// Union with unrestricted is unrestricted
			SecurityPermission sp1 = new SecurityPermission (PermissionState.Unrestricted);
			SecurityPermission sp2 = new SecurityPermission (PermissionState.None);
			// a. source (this) is unrestricted
			foreach (SecurityPermissionFlag spf in AllFlags) {
				sp2.Flags = spf;
				SecurityPermission union = (SecurityPermission)sp1.Union (sp2);
				Assert.IsTrue (union.IsUnrestricted (), "target " + spf.ToString ());
			}
			// b. destination (target) is unrestricted
			foreach (SecurityPermissionFlag spf in AllFlags) {
				sp2.Flags = spf;
				SecurityPermission union = (SecurityPermission)sp2.Union (sp1);
				Assert.IsTrue (union.IsUnrestricted (), "source " + spf.ToString ());
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			sp.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();
			se.Tag = "IMono";
			sp.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			sp.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			sp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			sp.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			sp.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.None);
			SecurityElement se = sp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			sp.FromXml (w);
		}
	}
}
