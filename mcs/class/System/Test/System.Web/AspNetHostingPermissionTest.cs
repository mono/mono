//
// AspNetHostingPermissionTest.cs - 
//	NUnit Test Cases for AspNetHostingPermission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

namespace MonoTests.System.Web {

	[TestFixture]
	public class AspNetHostingPermissionTest {

		static AspNetHostingPermissionLevel[] AllLevel = {
			AspNetHostingPermissionLevel.None,
			AspNetHostingPermissionLevel.Minimal,
			AspNetHostingPermissionLevel.Low,
			AspNetHostingPermissionLevel.Medium,
			AspNetHostingPermissionLevel.High,
			AspNetHostingPermissionLevel.Unrestricted,
		};

		static AspNetHostingPermissionLevel[] AllLevelExceptNone = {
			AspNetHostingPermissionLevel.Minimal,
			AspNetHostingPermissionLevel.Low,
			AspNetHostingPermissionLevel.Medium,
			AspNetHostingPermissionLevel.High,
			AspNetHostingPermissionLevel.Unrestricted,
		};

		static AspNetHostingPermissionLevel[] AllLevelExceptUnrestricted = {
			AspNetHostingPermissionLevel.None,
			AspNetHostingPermissionLevel.Minimal,
			AspNetHostingPermissionLevel.Low,
			AspNetHostingPermissionLevel.Medium,
			AspNetHostingPermissionLevel.High,
		};

		[Test]
		public void PermissionState_None ()
		{
			PermissionState ps = PermissionState.None;
			AspNetHostingPermission anhp = new AspNetHostingPermission (ps);
			Assert.AreEqual (AspNetHostingPermissionLevel.None, anhp.Level, "Level");
			Assert.IsFalse (anhp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = anhp.ToXml ();
			// only class and version are present
			Assert.AreEqual ("None", se.Attribute ("Level"), "Xml-Level");
			Assert.IsNull (se.Children, "Xml-Children");

			AspNetHostingPermission copy = (AspNetHostingPermission)anhp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (anhp, copy), "ReferenceEquals");
			Assert.AreEqual (anhp.Level, copy.Level, "Level");
			Assert.AreEqual (anhp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		public void PermissionState_Unrestricted ()
		{
			PermissionState ps = PermissionState.Unrestricted;
			AspNetHostingPermission anhp = new AspNetHostingPermission (ps);
			Assert.AreEqual (AspNetHostingPermissionLevel.Unrestricted, anhp.Level, "Level");
			Assert.IsTrue (anhp.IsUnrestricted (), "IsUnrestricted");

			SecurityElement se = anhp.ToXml ();
#if NET_2_0
			// fixed in 2.0 RC
			Assert.IsNotNull (se.Attribute ("Unrestricted"), "Xml-Unrestricted");
#else
			Assert.IsNull (se.Attribute ("Unrestricted"), "Xml-Unrestricted");
#endif
			Assert.AreEqual ("Unrestricted", se.Attribute ("Level"), "Xml-Level");
			Assert.IsNull (se.Children, "Xml-Children");

			AspNetHostingPermission copy = (AspNetHostingPermission)anhp.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (anhp, copy), "ReferenceEquals");
			Assert.AreEqual (anhp.Level, copy.Level, "Level");
			Assert.AreEqual (anhp.IsUnrestricted (), copy.IsUnrestricted (), "IsUnrestricted ()");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionState_Bad ()
		{
			PermissionState ps = (PermissionState) Int32.MinValue;
			AspNetHostingPermission anhp = new AspNetHostingPermission (ps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AspNetHostingPermissionLevels_Bad ()
		{
			AspNetHostingPermissionLevel ppl = (AspNetHostingPermissionLevel) Int32.MinValue;
			AspNetHostingPermission anhp = new AspNetHostingPermission (ppl);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Level_AspNetHostingPermissionLevels_Bad ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			anhp.Level = (AspNetHostingPermissionLevel) Int32.MinValue;
		}

		[Test]
		public void Copy ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				anhp.Level = ppl;
				AspNetHostingPermission copy = (AspNetHostingPermission)anhp.Copy ();
				Assert.AreEqual (ppl, copy.Level, ppl.ToString ());
			}
		}

		[Test]
		public void Intersect_Null ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			// No intersection with null
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				anhp.Level = ppl;
				IPermission p = anhp.Intersect (null);
#if ! NET_2_0
				if (p != null)
					Assert.Ignore ("Behaviour changed in FX 1.1 SP1");
#endif
				Assert.IsNull (p, ppl.ToString ());
			}
		}

		[Test]
		public void Intersect_None ()
		{
			AspNetHostingPermission sp1 = new AspNetHostingPermission (PermissionState.None);
			AspNetHostingPermission sp2 = new AspNetHostingPermission (PermissionState.None);
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				sp2.Level = ppl;
				// 1. Intersect None with ppl
				AspNetHostingPermission result = (AspNetHostingPermission)sp1.Intersect (sp2);
				Assert.AreEqual (AspNetHostingPermissionLevel.None, result.Level, "None N " + ppl.ToString ());
				// 2. Intersect ppl with None
				result = (AspNetHostingPermission)sp2.Intersect (sp1);
				Assert.AreEqual (AspNetHostingPermissionLevel.None, result.Level, ppl.ToString () + "N None");
			}
		}

		[Test]
		public void Intersect_Self ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				anhp.Level = ppl;
				AspNetHostingPermission result = (AspNetHostingPermission)anhp.Intersect (anhp);
				Assert.AreEqual (ppl, result.Level, ppl.ToString ());
			}
		}

		[Test]
		public void Intersect_Unrestricted ()
		{
			// Intersection with unrestricted == Copy
			// a. source (this) is unrestricted
			AspNetHostingPermission sp1 = new AspNetHostingPermission (PermissionState.Unrestricted);
			AspNetHostingPermission sp2 = new AspNetHostingPermission (PermissionState.None);
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				sp2.Level = ppl;
				AspNetHostingPermission result = (AspNetHostingPermission)sp1.Intersect (sp2);
				Assert.AreEqual (sp2.Level, result.Level, "target " + ppl.ToString ());
			}
			// b. destination (target) is unrestricted
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				sp2.Level = ppl;
				AspNetHostingPermission result = (AspNetHostingPermission)sp2.Intersect (sp1);
				Assert.AreEqual (sp2.Level, result.Level, "source " + ppl.ToString ());
			}
		}

		[Test]
		public void IsSubset_Null ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			Assert.IsTrue (anhp.IsSubsetOf (null), "NoLevel");
			foreach (AspNetHostingPermissionLevel ppl in AllLevelExceptNone) {
				anhp.Level = ppl;
				Assert.IsFalse (anhp.IsSubsetOf (null), ppl.ToString ());
			}
		}

		[Test]
		public void IsSubset_None ()
		{
			// IsSubset with none
			// a. source (this) is none -> target is never a subset
			AspNetHostingPermission sp1 = new AspNetHostingPermission (PermissionState.None);
			AspNetHostingPermission sp2 = new AspNetHostingPermission (PermissionState.None);
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				sp2.Level = ppl;
				Assert.IsTrue (sp1.IsSubsetOf (sp2), "target " + ppl.ToString ());
			}
			// b. destination (target) is none -> target is always a subset
			foreach (AspNetHostingPermissionLevel ppl in AllLevelExceptNone) {
				sp2.Level = ppl;
				Assert.IsFalse (sp2.IsSubsetOf (sp1), "source " + ppl.ToString ());
			}
			// exception of NoLevel
			sp2.Level = AspNetHostingPermissionLevel.None;
			Assert.IsTrue (sp2.IsSubsetOf (sp1), "source NoLevel");
		}

		[Test]
		public void IsSubset_Self ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				anhp.Level = ppl;
				AspNetHostingPermission result = (AspNetHostingPermission)anhp.Intersect (anhp);
				Assert.IsTrue (anhp.IsSubsetOf (anhp), ppl.ToString ());
			}
		}

		[Test]
		public void IsSubset_Unrestricted ()
		{
			// IsSubset with unrestricted
			// a. source (this) is unrestricted -> target is never a subset
			AspNetHostingPermission sp1 = new AspNetHostingPermission (PermissionState.Unrestricted);
			AspNetHostingPermission sp2 = new AspNetHostingPermission (PermissionState.None);
			foreach (AspNetHostingPermissionLevel ppl in AllLevelExceptUnrestricted) {
				sp2.Level = ppl;
				Assert.IsFalse (sp1.IsSubsetOf (sp2), "target " + ppl.ToString ());
			}
			// exception of AllLevel
			sp2.Level = AspNetHostingPermissionLevel.Unrestricted;
			Assert.IsTrue (sp1.IsSubsetOf (sp2), "target AllLevel");
			// b. destination (target) is unrestricted -> target is always a subset
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				sp2.Level = ppl;
				Assert.IsTrue (sp2.IsSubsetOf (sp1), "source " + ppl.ToString ());
			}
		}

		[Test]
		public void Union_Null ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			// Union with null is a simple copy
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				anhp.Level = ppl;
				AspNetHostingPermission union = (AspNetHostingPermission)anhp.Union (null);
				Assert.AreEqual (ppl, union.Level, ppl.ToString ());
			}
		}

		[Test]
		public void Union_None ()
		{
			// Union with none is same
			AspNetHostingPermission pp1 = new AspNetHostingPermission (PermissionState.None);
			AspNetHostingPermission pp2 = new AspNetHostingPermission (PermissionState.None);
			AspNetHostingPermission union = null;

			foreach (AspNetHostingPermissionLevel ppl in AllLevelExceptUnrestricted) {
				pp2.Level = ppl;
				
				union = (AspNetHostingPermission)pp1.Union (pp2);
				Assert.IsFalse (union.IsUnrestricted (), "target.Unrestricted " + ppl.ToString ());
				Assert.AreEqual (ppl, union.Level, "target.Level " + ppl.ToString ());

				union = (AspNetHostingPermission)pp2.Union (pp1);
				Assert.IsFalse (union.IsUnrestricted (), "source.Unrestricted " + ppl.ToString ());
				Assert.AreEqual (ppl, union.Level, "source.Level " + ppl.ToString ());
			}

			pp2.Level = AspNetHostingPermissionLevel.Unrestricted;
			union = (AspNetHostingPermission)pp1.Union (pp2);
			Assert.IsTrue (union.IsUnrestricted (), "target.Unrestricted Unrestricted");
			Assert.AreEqual (AspNetHostingPermissionLevel.Unrestricted, union.Level, "target.Level Unrestricted");

			union = (AspNetHostingPermission)pp2.Union (pp1);
			Assert.IsTrue (union.IsUnrestricted (), "source.Unrestricted Unrestricted");
			Assert.AreEqual (AspNetHostingPermissionLevel.Unrestricted, union.Level, "source.Level Unrestricted");
		}

		[Test]
		public void Union_Self ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				anhp.Level = ppl;
				AspNetHostingPermission result = (AspNetHostingPermission)anhp.Union (anhp);
				Assert.AreEqual (ppl, result.Level, ppl.ToString ());
			}
		}

		[Test]
		public void Union_Unrestricted ()
		{
			// Union with unrestricted is unrestricted
			AspNetHostingPermission sp1 = new AspNetHostingPermission (PermissionState.Unrestricted);
			AspNetHostingPermission sp2 = new AspNetHostingPermission (PermissionState.None);
			// a. source (this) is unrestricted
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				sp2.Level = ppl;
				AspNetHostingPermission union = (AspNetHostingPermission)sp1.Union (sp2);
				Assert.IsTrue (union.IsUnrestricted (), "target " + ppl.ToString ());
			}
			// b. destination (target) is unrestricted
			foreach (AspNetHostingPermissionLevel ppl in AllLevel) {
				sp2.Level = ppl;
				AspNetHostingPermission union = (AspNetHostingPermission)sp2.Union (sp1);
				Assert.IsTrue (union.IsUnrestricted (), "source " + ppl.ToString ());
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			anhp.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			SecurityElement se = anhp.ToXml ();
			se.Tag = "IMono";
			anhp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			SecurityElement se = anhp.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			anhp.FromXml (se);
			// note: normally IPermission classes (in corlib) DO care about the
			// IPermission tag
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			SecurityElement se = anhp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			anhp.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_NoClass ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			SecurityElement se = anhp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			anhp.FromXml (w);
			// note: normally IPermission classes (in corlib) DO NOT care about
			// attribute "class" name presence in the XML
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			SecurityElement se = anhp.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			anhp.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_NoVersion ()
		{
			AspNetHostingPermission anhp = new AspNetHostingPermission (PermissionState.None);
			SecurityElement se = anhp.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			anhp.FromXml (w);
		}
	}
}
