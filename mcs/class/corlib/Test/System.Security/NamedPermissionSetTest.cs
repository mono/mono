//
// NamedPermissionSetTest.cs - NUnit Test Cases for NamedPermissionSet
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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

#if !MOBILE

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security {

	[TestFixture]
	public class NamedPermissionSetTest {

		private static string name = "mono";
		private static string sentinel = "go mono!";

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNameNull () 
		{
			string s = null; // we don't want to confuse the compiler
			NamedPermissionSet nps = new NamedPermissionSet (s);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNameEmpty () 
		{
			NamedPermissionSet nps = new NamedPermissionSet ("");
		}

		[Test]
		public void ConstructorName ()
		{
			NamedPermissionSet nps = new NamedPermissionSet ("name");
			Assert.AreEqual ("name", nps.Name, "Name");
			Assert.IsNull (nps.Description, "Description");
			Assert.IsTrue (nps.IsUnrestricted (), "IsUnrestricted");
			Assert.IsTrue (!nps.IsEmpty (), "IsEmpty");
			Assert.IsTrue (!nps.IsReadOnly, "IsReadOnly");
			Assert.IsTrue (!nps.IsSynchronized, "IsSynchronized");
			Assert.AreEqual (0, nps.Count, "Count");
		}

		[Test]
		public void ConstructorNameReserved ()
		{
			NamedPermissionSet nps = new NamedPermissionSet ("FullTrust");
			Assert.AreEqual ("FullTrust", nps.Name, "Name");
			Assert.IsNull (nps.Description, "Description");
			Assert.IsTrue (nps.IsUnrestricted (), "IsUnrestricted");
			Assert.IsTrue (!nps.IsEmpty (), "IsEmpty");
			Assert.IsTrue (!nps.IsReadOnly, "IsReadOnly");
			Assert.IsTrue (!nps.IsSynchronized, "IsSynchronized");
			Assert.AreEqual (0, nps.Count, "Count");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorNamedPermissionSetNull ()
		{
			NamedPermissionSet nullps = null;
			NamedPermissionSet nps = new NamedPermissionSet (nullps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNameNullPermissionState ()
		{
			new NamedPermissionSet (null, PermissionState.None);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNameEmptyPermissionState ()
		{
			new NamedPermissionSet (String.Empty, PermissionState.None);
		}

		[Test]
		public void ConstructorNamePermissionStateNone ()
		{
			NamedPermissionSet nps = new NamedPermissionSet ("name", PermissionState.None);
			Assert.AreEqual ("name", nps.Name, "Name");
			Assert.IsNull (nps.Description, "Description");
			Assert.IsTrue (!nps.IsUnrestricted (), "IsUnrestricted");
			Assert.IsTrue (nps.IsEmpty (), "IsEmpty");
			Assert.IsTrue (!nps.IsReadOnly, "IsReadOnly");
			Assert.IsTrue (!nps.IsSynchronized, "IsSynchronized");
			Assert.AreEqual (0, nps.Count, "Count");
		}

		[Test]
		public void ConstructorNamePermissionStateUnrestricted ()
		{
			NamedPermissionSet nps = new NamedPermissionSet ("name", PermissionState.Unrestricted);
			Assert.AreEqual ("name", nps.Name, "Name");
			Assert.IsNull (nps.Description, "Description");
			Assert.IsTrue (nps.IsUnrestricted (), "IsUnrestricted");
			Assert.IsTrue (!nps.IsEmpty (), "IsEmpty");
			Assert.IsTrue (!nps.IsReadOnly, "IsReadOnly");
			Assert.IsTrue (!nps.IsSynchronized, "IsSynchronized");
			Assert.AreEqual (0, nps.Count, "Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNameNullPermissionSet ()
		{
			new NamedPermissionSet (null, new PermissionSet (PermissionState.None));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNameEmptyPermissionSet ()
		{
			new NamedPermissionSet (String.Empty, new PermissionSet (PermissionState.None));
		}

		[Test]
		public void ConstructorNamePermissionSetNull ()
		{
			NamedPermissionSet nps = new NamedPermissionSet ("name", null);
			Assert.AreEqual ("name", nps.Name, "Name");
			Assert.IsNull (nps.Description, "Description");
#if NET_2_0
			Assert.IsTrue (!nps.IsUnrestricted (), "IsUnrestricted");
			Assert.IsTrue (nps.IsEmpty (), "IsEmpty");
#else
			Assert.IsTrue (nps.IsUnrestricted (), "IsUnrestricted");
			Assert.IsTrue (!nps.IsEmpty (), "IsEmpty");
#endif
			Assert.IsTrue (!nps.IsReadOnly, "IsReadOnly");
			Assert.IsTrue (!nps.IsSynchronized, "IsSynchronized");
			Assert.AreEqual (0, nps.Count, "Count");
		}

		[Test]
		public void Description () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			// null by default (not empty)
			Assert.IsNull (nps.Description, "Description");
			// is null-able (without exception)
			nps.Description = null;
			Assert.IsNull (nps.Description, "Description(null)");
			nps.Description = sentinel;
			Assert.AreEqual (sentinel, nps.Description, "Description");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NameNull () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			nps.Name = null;
			// strangely this isn't a ArgumentNullException (but so says the doc)
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NameEmpty () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			nps.Name = "";
		}

		[Test]
		public void Name () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			nps.Name = sentinel;
			Assert.AreEqual (sentinel, nps.Name, "Name");
		}

		[Test]
		public void Copy ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			nps.Description = sentinel;
			nps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Assertion));
			NamedPermissionSet copy = (NamedPermissionSet)nps.Copy ();
			Assert.AreEqual (nps.Name, copy.Name, "Name");
			Assert.AreEqual (nps.Description, copy.Description, "Description");
			Assert.AreEqual (nps.Count, copy.Count, "Count");
		}

		[Test]
		public void Copy_Name ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			nps.Description = sentinel;
			nps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Assertion));
			NamedPermissionSet copy = (NamedPermissionSet)nps.Copy ("Copy");
			Assert.AreEqual ("Copy", copy.Name, "Name");
			Assert.AreEqual (nps.Description, copy.Description, "Description");
			Assert.AreEqual (nps.Count, copy.Count, "Count");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Copy_Name_Null ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			NamedPermissionSet copy = (NamedPermissionSet)nps.Copy (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Copy_Name_Empty ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			NamedPermissionSet copy = (NamedPermissionSet)nps.Copy (String.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			nps.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_InvalidPermission () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement ("InvalidPermissionSet", se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", se.Attribute ("version"));
			se2.AddAttribute ("Name", se.Attribute ("Name"));
			nps.FromXml (se2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();
			se.Tag = se.Tag.ToUpper (); // instead of PermissionSet
			nps.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			w.AddAttribute ("Name", se.Attribute ("Name"));
			nps.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			nps.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		// [ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement (se.Tag, se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", "2");
			se2.AddAttribute ("Name", se.Attribute ("Name"));
			nps.FromXml (se2);
			// wow - here we accept a version 2 !!!
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("Name", se.Attribute ("Name"));
			nps.FromXml (w);
		}

		[Test]
		public void FromXml_NoName ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			w.AddAttribute ("version", "1");
			nps.FromXml (w);

			// having a null name can badly influence the rest of the class code
			Assert.IsNull (nps.Name, "Name");
			NamedPermissionSet copy = (NamedPermissionSet) nps.Copy ();
			Assert.IsNull (copy.Name, "Copy.Name");

			copy = nps.Copy ("name");
			Assert.AreEqual ("name", copy.Name, "Copy(Name).Name");

			se = nps.ToXml ();
			Assert.IsNull (se.Attribute ("Name"), "Name attribute");
#if NET_2_0
			Assert.AreEqual (0, nps.GetHashCode (), "GetHashCode");
			Assert.IsTrue (nps.Equals (nps), "Equals-self");
#endif
		}

		[Test]
		public void FromXml () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();
			Assert.IsNotNull (se, "ToXml()");

			NamedPermissionSet nps2 = (NamedPermissionSet) nps.Copy ();
			nps2.FromXml (se);
			Assert.AreEqual (name, nps2.Name, "FromXml-Copy.Name");
			// strangely it's empty when converted from XML (but null when created)
			Assert.AreEqual ("", nps2.Description, "FromXml-Copy.Description");
			Assert.IsTrue (!nps2.IsUnrestricted () , "FromXml-Copy.IsUnrestricted");

			se.AddAttribute ("Description", sentinel);
			nps2.FromXml (se);
			Assert.AreEqual (name, nps2.Name, "FromXml-Add1.Name");
			Assert.AreEqual (sentinel, nps2.Description, "FromXml-Add1.Description");
			Assert.IsTrue (!nps2.IsUnrestricted () , "FromXml-Add1.IsUnrestricted");

			se.AddAttribute ("Unrestricted", "true");
			nps2.FromXml (se);
			Assert.AreEqual (name, nps2.Name, "FromXml-Add2.Name");
			Assert.AreEqual (sentinel, nps2.Description, "FromXml-Add2.Description");
			Assert.IsTrue (nps2.IsUnrestricted () , "FromXml-Add2.IsUnrestricted");
		}

		[Test]
		public void ToXml_None () 
		{
			NamedPermissionSet ps = new NamedPermissionSet (name, PermissionState.None);
			ps.Description = sentinel;
			SecurityElement se = ps.ToXml ();
			Assert.IsTrue (ps.ToString().StartsWith ("<PermissionSet"), "None.ToString().StartsWith");
			Assert.AreEqual ("System.Security.NamedPermissionSet", (se.Attributes ["class"] as string), "None.class");
			Assert.AreEqual ("1", (se.Attributes ["version"] as string), "None.version");
			Assert.AreEqual (name, (se.Attributes ["Name"] as string), "None.Name");
			Assert.AreEqual (sentinel, (se.Attributes ["Description"] as string), "None.Description");
			Assert.IsNull ((se.Attributes ["Unrestricted"] as string), "None.Unrestricted");
		}

		[Test]
		public void ToXml_Unrestricted () 
		{
			NamedPermissionSet ps = new NamedPermissionSet (name, PermissionState.Unrestricted);
			SecurityElement se = ps.ToXml ();
			Assert.IsTrue (ps.ToString().StartsWith ("<PermissionSet"), "Unrestricted.ToString().StartsWith");
			Assert.AreEqual ("System.Security.NamedPermissionSet", (se.Attributes ["class"] as string), "Unrestricted.class");
			Assert.AreEqual ("1", (se.Attributes ["version"] as string), "Unrestricted.version");
			Assert.AreEqual (name, (se.Attributes ["Name"] as string), "Unrestricted.Name");
			Assert.IsNull ((se.Attributes ["Description"] as string), "Unrestricted.Description");
			Assert.AreEqual ("true", (se.Attributes ["Unrestricted"] as string), "Unrestricted.Unrestricted");
		}
#if NET_2_0
		[Test]
		public void Equals () 
		{
			NamedPermissionSet psn = new NamedPermissionSet (name, PermissionState.None);
			NamedPermissionSet psu = new NamedPermissionSet (name, PermissionState.Unrestricted);
			Assert.IsTrue (!psn.Equals (psu), "psn!=psu");
			Assert.IsTrue (!psu.Equals (psn), "psu!=psn");
			NamedPermissionSet cpsn = (NamedPermissionSet) psn.Copy ();
			Assert.IsTrue (cpsn.Equals (psn), "cpsn==psn");
			Assert.IsTrue (psn.Equals (cpsn), "psn==cpsn");
			NamedPermissionSet cpsu = (NamedPermissionSet) psu.Copy ();
			Assert.IsTrue (cpsu.Equals (psu), "cpsu==psu");
			Assert.IsTrue (psu.Equals (cpsu), "psu==cpsu");
			cpsn.Description = sentinel;
			Assert.IsTrue (cpsn.Equals (psn), "cpsn+desc==psn");
			Assert.IsTrue (psn.Equals (cpsn), "psn==cpsn+desc");
			cpsn.Description = sentinel;
			Assert.IsTrue (cpsu.Equals (psu), "cpsu+desc==psu");
			Assert.IsTrue (psu.Equals (cpsu), "psu==cpsu+desc");
		}

		[Test]
		public void GetHashCode_ () 
		{
			NamedPermissionSet psn = new NamedPermissionSet (name, PermissionState.None);
			int nhc = psn.GetHashCode ();
			NamedPermissionSet psu = new NamedPermissionSet (name, PermissionState.Unrestricted);
			int uhc = psu.GetHashCode ();
			Assert.IsTrue (nhc != uhc, "GetHashCode-1");
			psn.Description = sentinel;
			Assert.IsTrue (psn.GetHashCode () == nhc, "GetHashCode-2");
			psu.Description = sentinel;
			Assert.IsTrue (psu.GetHashCode () == uhc, "GetHashCode-3");
		}
#endif
	}
}

#endif