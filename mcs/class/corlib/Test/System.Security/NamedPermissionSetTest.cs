//
// NamedPermissionSetTest.cs - NUnit Test Cases for NamedPermissionSet
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
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security {

	[TestFixture]
	public class NamedPermissionSetTest : Assertion {

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
		public void Description () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			// null by default (not empty)
			AssertNull ("Description", nps.Description);
			// is null-able (without exception)
			nps.Description = null;
			AssertNull ("Description(null)", nps.Description);
			nps.Description = sentinel;
			AssertEquals ("Description", sentinel, nps.Description);
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
			AssertEquals ("Name", sentinel, nps.Name);
		}

		[Test]
		public void Copy ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			nps.Description = sentinel;
			nps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Assertion));
			NamedPermissionSet copy = (NamedPermissionSet)nps.Copy ();
			AssertEquals ("Name", nps.Name, copy.Name);
			AssertEquals ("Description", nps.Description, copy.Description);
			AssertEquals ("Count", nps.Count, copy.Count);
		}

		[Test]
		public void Copy_Name ()
		{
			NamedPermissionSet nps = new NamedPermissionSet (name);
			nps.Description = sentinel;
			nps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Assertion));
			NamedPermissionSet copy = (NamedPermissionSet)nps.Copy ("Copy");
			AssertEquals ("Name", "Copy", copy.Name);
			AssertEquals ("Description", nps.Description, copy.Description);
			AssertEquals ("Count", nps.Count, copy.Count);
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
			AssertNull ("Name", nps.Name);
			NamedPermissionSet copy = (NamedPermissionSet) nps.Copy ();
			AssertNull ("Copy.Name", copy.Name);

			copy = nps.Copy ("name");
			AssertEquals ("Copy(Name).Name", "name", copy.Name);

			se = nps.ToXml ();
			AssertNull ("Name attribute", se.Attribute ("Name"));
#if NET_2_0
			AssertEquals ("GetHashCode", 0, nps.GetHashCode ());
			Assert ("Equals-self", nps.Equals (nps));
#endif
		}

		[Test]
		public void FromXml () 
		{
			NamedPermissionSet nps = new NamedPermissionSet (name, PermissionState.None);
			SecurityElement se = nps.ToXml ();
			AssertNotNull ("ToXml()", se);

			NamedPermissionSet nps2 = (NamedPermissionSet) nps.Copy ();
			nps2.FromXml (se);
			AssertEquals ("FromXml-Copy.Name", name, nps2.Name);
			// strangely it's empty when converted from XML (but null when created)
			AssertEquals ("FromXml-Copy.Description", "", nps2.Description);
			Assert ("FromXml-Copy.IsUnrestricted", !nps2.IsUnrestricted ()); 

			se.AddAttribute ("Description", sentinel);
			nps2.FromXml (se);
			AssertEquals ("FromXml-Add1.Name", name, nps2.Name);
			AssertEquals ("FromXml-Add1.Description", sentinel, nps2.Description);
			Assert ("FromXml-Add1.IsUnrestricted", !nps2.IsUnrestricted ()); 

			se.AddAttribute ("Unrestricted", "true");
			nps2.FromXml (se);
			AssertEquals ("FromXml-Add2.Name", name, nps2.Name);
			AssertEquals ("FromXml-Add2.Description", sentinel, nps2.Description);
			Assert ("FromXml-Add2.IsUnrestricted", nps2.IsUnrestricted ()); 
		}

		[Test]
		public void ToXml_None () 
		{
			NamedPermissionSet ps = new NamedPermissionSet (name, PermissionState.None);
			ps.Description = sentinel;
			SecurityElement se = ps.ToXml ();
			Assert ("None.ToString().StartsWith", ps.ToString().StartsWith ("<PermissionSet"));
			AssertEquals ("None.class", "System.Security.NamedPermissionSet", (se.Attributes ["class"] as string));
			AssertEquals ("None.version", "1", (se.Attributes ["version"] as string));
			AssertEquals ("None.Name", name, (se.Attributes ["Name"] as string));
			AssertEquals ("None.Description", sentinel, (se.Attributes ["Description"] as string));
			AssertNull ("None.Unrestricted", (se.Attributes ["Unrestricted"] as string));
		}

		[Test]
		public void ToXml_Unrestricted () 
		{
			NamedPermissionSet ps = new NamedPermissionSet (name, PermissionState.Unrestricted);
			SecurityElement se = ps.ToXml ();
			Assert ("Unrestricted.ToString().StartsWith", ps.ToString().StartsWith ("<PermissionSet"));
			AssertEquals ("Unrestricted.class", "System.Security.NamedPermissionSet", (se.Attributes ["class"] as string));
			AssertEquals ("Unrestricted.version", "1", (se.Attributes ["version"] as string));
			AssertEquals ("Unrestricted.Name", name, (se.Attributes ["Name"] as string));
			AssertNull ("Unrestricted.Description", (se.Attributes ["Description"] as string));
			AssertEquals ("Unrestricted.Unrestricted", "true", (se.Attributes ["Unrestricted"] as string));
		}
#if NET_2_0
		[Test]
		public void Equals () 
		{
			NamedPermissionSet psn = new NamedPermissionSet (name, PermissionState.None);
			NamedPermissionSet psu = new NamedPermissionSet (name, PermissionState.Unrestricted);
			Assert ("psn!=psu", !psn.Equals (psu));
			Assert ("psu!=psn", !psu.Equals (psn));
			NamedPermissionSet cpsn = (NamedPermissionSet) psn.Copy ();
			Assert ("cpsn==psn", cpsn.Equals (psn));
			Assert ("psn==cpsn", psn.Equals (cpsn));
			NamedPermissionSet cpsu = (NamedPermissionSet) psu.Copy ();
			Assert ("cpsu==psu", cpsu.Equals (psu));
			Assert ("psu==cpsu", psu.Equals (cpsu));
			cpsn.Description = sentinel;
			Assert ("cpsn+desc==psn", cpsn.Equals (psn));
			Assert ("psn==cpsn+desc", psn.Equals (cpsn));
			cpsn.Description = sentinel;
			Assert ("cpsu+desc==psu", cpsu.Equals (psu));
			Assert ("psu==cpsu+desc", psu.Equals (cpsu));
		}

		[Test]
		public void GetHashCode_ () 
		{
			NamedPermissionSet psn = new NamedPermissionSet (name, PermissionState.None);
			int nhc = psn.GetHashCode ();
			NamedPermissionSet psu = new NamedPermissionSet (name, PermissionState.Unrestricted);
			int uhc = psu.GetHashCode ();
			Assert ("GetHashCode-1", nhc != uhc);
			psn.Description = sentinel;
			Assert ("GetHashCode-2", psn.GetHashCode () == nhc);
			psu.Description = sentinel;
			Assert ("GetHashCode-3", psu.GetHashCode () == uhc);
		}
#endif
	}
}
