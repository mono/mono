//
// PermissionSetTest.cs - NUnit Test Cases for PermissionSet
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
using System.Collections;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace MonoTests.System.Security {

	[TestFixture]
	public class PermissionSetTest : Assertion {

		[Test]
		public void PermissionStateNone () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert ("PermissionStateNone.IsUnrestricted", !ps.IsUnrestricted ());
			Assert ("PermissionStateNone.IsEmpty", ps.IsEmpty ());
			Assert ("PermissionStateNone.IsReadOnly", !ps.IsReadOnly);
			AssertEquals ("PermissionStateNone.ToXml().ToString()==ToString()", ps.ToXml ().ToString (), ps.ToString ());
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			Assert ("PermissionStateUnrestricted.IsUnrestricted", ps.IsUnrestricted ());
			Assert ("PermissionStateUnrestricted.IsEmpty", !ps.IsEmpty ());
			Assert ("PermissionStateUnrestricted.IsReadOnly", !ps.IsReadOnly);
			AssertEquals ("PermissionStateUnrestricted.ToXml().ToString()==ToString()", ps.ToXml ().ToString (), ps.ToString ());
		}

		[Test]
		public void PermissionSetNull () 
		{
			// no exception is thrown
			PermissionSet ps = new PermissionSet (null);
			Assert ("PermissionStateNull.IsUnrestricted", ps.IsUnrestricted ());
			Assert ("PermissionStateNull.IsEmpty", !ps.IsEmpty ());
			Assert ("PermissionStateNull.IsReadOnly", !ps.IsReadOnly);
			AssertEquals ("PermissionStateNull.ToXml().ToString()==ToString()", ps.ToXml ().ToString (), ps.ToString ());
		}

		[Test]
		public void PermissionSetPermissionSet () 
		{
			FileDialogPermission fdp = new FileDialogPermission (FileDialogPermissionAccess.Open);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (fdp);
			Assert ("ps1.IsEmpty", !ps1.IsEmpty ());

			PermissionSet ps = new PermissionSet (ps1);
			Assert ("PermissionSetPermissionSet.IsUnrestricted", !ps.IsUnrestricted ());
			Assert ("PermissionSetPermissionSet.IsEmpty", !ps.IsEmpty ());
			Assert ("PermissionSetPermissionSet.IsReadOnly", !ps.IsReadOnly);
			AssertEquals ("PermissionSetPermissionSet.ToXml().ToString()==ToString()", ps.ToXml ().ToString (), ps.ToString ());
		}

		[Test]
		public void PermissionSetNamedPermissionSet ()
		{
			NamedPermissionSet nps = new NamedPermissionSet ("Test", PermissionState.Unrestricted);
			PermissionSet ps = new PermissionSet (nps);
			Assert ("IsUnrestricted", ps.IsUnrestricted ());
		}

		[Test]
		public void AddPermission ()
		{
			SecurityPermission sp1 = new SecurityPermission (SecurityPermissionFlag.ControlEvidence);
			SecurityPermission sp2 = new SecurityPermission (SecurityPermissionFlag.ControlPolicy);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityPermission result = (SecurityPermission)ps.AddPermission (sp1);
			AssertEquals ("1-ControlEvidence", 1, ps.Count);
			AssertEquals ("Flags-1", SecurityPermissionFlag.ControlEvidence, result.Flags);

			result = (SecurityPermission)ps.AddPermission (sp2);
			AssertEquals ("1-ControlEvidence+ControlPolicy", 1, ps.Count);
			AssertEquals ("Flags-2", SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence, result.Flags);

			result = (SecurityPermission)ps.AddPermission (sp2);
			AssertEquals ("no change-1", 1, ps.Count);
			AssertEquals ("Flags-3", SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence, result.Flags);

			result = (SecurityPermission)ps.AddPermission (sp1);
			AssertEquals ("no change-2", 1, ps.Count);
			AssertEquals ("Flags-4", SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence, result.Flags);
		}

		[Test]
		public void AddPermission_Null ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			IPermission result = ps.AddPermission (null);
			AssertNull ("Add(null)", result);
			AssertEquals ("0", 0, ps.Count);
		}

		[Test]
		public void AddPermission_SetUnrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.ControlEvidence);
			IPermission result = ps.AddPermission (sp);
			AssertNotNull ("Add(SecurityPermission)", result);
			AssertEquals ("SecurityPermission", SecurityPermissionFlag.AllFlags, (result as SecurityPermission).Flags);
			AssertEquals ("0", 0, ps.Count);
			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			result = ps.AddPermission (zip);
			AssertNotNull ("Add(ZoneIdentityPermission)", result);
			AssertEquals ("ZoneIdentityPermission", zip.SecurityZone, (result as ZoneIdentityPermission).SecurityZone);
			AssertEquals ("1", 1, ps.Count);
		}

		[Test]
		public void AddPermission_PermissionUnrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityPermission sp = new SecurityPermission (PermissionState.Unrestricted);
			IPermission result = ps.AddPermission (sp);
			AssertNotNull ("Add(SecurityPermission)", result);
			AssertEquals ("SecurityPermission", SecurityPermissionFlag.AllFlags, (result as SecurityPermission).Flags);
			AssertEquals ("1", 1, ps.Count);
			Assert ("State", !ps.IsUnrestricted ());
		}

		[Test]
		public void ContainsNonCodeAccessPermissions ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert ("Empty", !ps.ContainsNonCodeAccessPermissions ());

			SecurityPermission sp = new SecurityPermission (PermissionState.Unrestricted);
			ps.AddPermission (sp);
			Assert ("SecurityPermission", !ps.ContainsNonCodeAccessPermissions ());

			PrincipalPermission pp = new PrincipalPermission ("mono", "hacker");
			ps.AddPermission (pp);
			Assert ("PrincipalPermission", ps.ContainsNonCodeAccessPermissions ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertPermissionSet_NullIn ()
		{
			PermissionSet.ConvertPermissionSet (null, new byte [0], "XML");
		}

		[Test]
		public void ConvertPermissionSet_UnknownIn ()
		{
			byte[] result = PermissionSet.ConvertPermissionSet (String.Empty, new byte [0], "XML");
			AssertNull (result);
		}

		[Test]
		public void ConvertPermissionSet_NullData ()
		{
			byte[] result = PermissionSet.ConvertPermissionSet ("BINARY", null, "XML");
			AssertNull (result);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertPermissionSet_NullOut ()
		{
			PermissionSet.ConvertPermissionSet ("BINARY", new byte [0], null);
		}

		[Test]
		[ExpectedException (typeof (SerializationException))]
		public void ConvertPermissionSet_UnknownOut ()
		{
			PermissionSet.ConvertPermissionSet ("BINARY", new byte [0], String.Empty);
		}

		[Test]
#if !NET_2_0
		[Ignore ("Don't know why it doesn't work under Fx 1.1")]
#endif
		public void ConvertPermissionSet_BinaryToBinary ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			byte[] data = Encoding.ASCII.GetBytes (ps.ToString ());
			byte[] result = PermissionSet.ConvertPermissionSet ("XML", data, "BINARY");

			byte[] result2 = PermissionSet.ConvertPermissionSet ("BINARY", result, "BINARY");
			// there's only a little difference - but it doesn't throw an exception
			//Assert ("BINARY!=BINARY", BitConverter.ToString (result) != BitConverter.ToString (result2));
		}

		[Test]
#if !NET_2_0
		[Ignore ("Don't know why it doesn't work under Fx 1.1")]
#endif
		public void ConvertPermissionSet_XmlToBinary ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			byte[] data = Encoding.ASCII.GetBytes (ps.ToString ());
			byte[] result = PermissionSet.ConvertPermissionSet ("XML", data, "BINARY");
			byte[] result2 = PermissionSet.ConvertPermissionSet ("XMLASCII", data, "BINARY");
			AssertEquals ("XML==XMLASCII", BitConverter.ToString (result), BitConverter.ToString (result2));
			byte[] back = PermissionSet.ConvertPermissionSet ("BINARY", result, "XML");
			AssertEquals ("PS-XML", Encoding.ASCII.GetString (back), ps.ToString ());
			back = PermissionSet.ConvertPermissionSet ("BINARY", result2, "XMLASCII");
			AssertEquals ("PS-XMLASCII", Encoding.ASCII.GetString (back), ps.ToString ());
		}

		[Test]
		public void ConvertPermissionSet_XmlToXml ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			byte[] data = Encoding.ASCII.GetBytes (ps.ToString ());
			byte[] result = PermissionSet.ConvertPermissionSet ("XML", data, "XML");
			AssertEquals ("PS-XML", Encoding.ASCII.GetString (result), ps.ToString ());

			result = PermissionSet.ConvertPermissionSet ("XMLASCII", data, "XMLASCII");
			AssertEquals ("PS-XMLASCII", Encoding.ASCII.GetString (result), ps.ToString ());
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (XmlSyntaxException))]
#else
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void ConvertPermissionSet_XmlAsciiToXmlUnicode ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			byte[] data = Encoding.Unicode.GetBytes (ps.ToString ());
			byte[] result = PermissionSet.ConvertPermissionSet ("XMLASCII", data, "XMLUNICODE");
			// the method isn't intended to convert between ASCII and Unicode
		}

		[Test]
		public void Copy_None ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			PermissionSet copy = ps.Copy ();
			Assert ("1.State", !copy.IsUnrestricted ());
			AssertEquals ("1.Count", 0, copy.Count);

			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.ControlEvidence);
			IPermission result = ps.AddPermission (sp);
			AssertNotNull ("1.Add", result);
			copy = ps.Copy ();
			Assert ("2.State", !copy.IsUnrestricted ());
			AssertEquals ("2.Count", 1, copy.Count);

			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			result = ps.AddPermission (zip);
			AssertNotNull ("2.Add", result);
			copy = ps.Copy ();
			Assert ("3.State", !copy.IsUnrestricted ());
			AssertEquals ("3.Count", 2, copy.Count);
		}

		[Test]
		public void Copy_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			PermissionSet copy = ps.Copy ();
			Assert ("1.State", copy.IsUnrestricted ());
			AssertEquals ("1.Count", 0, copy.Count);

			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.ControlEvidence);
			IPermission result = ps.AddPermission (sp);
			AssertNotNull ("1.Add", result);
			copy = ps.Copy ();
			Assert ("2.State", copy.IsUnrestricted ());
			AssertEquals ("2.Count", 0, copy.Count);

			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			result = ps.AddPermission (zip);
			AssertNotNull ("2.Add", result);
			copy = ps.Copy ();
			Assert ("3.State", copy.IsUnrestricted ());
			AssertEquals ("3.Count", 1, copy.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyTo_Null ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.CopyTo (null, 0);
		}

		[Test]
		public void CopyTo_Rank_Empty ()
		{
			IPermission[,] pa = new IPermission [1,1];
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.CopyTo (pa, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyTo_Rank ()
		{
			IPermission [,] pa = new IPermission [1, 1];
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Assertion));
			ps.CopyTo (pa, 0);
		}

		[Test]
		public void CopyTo_NegativeIndex_Empty ()
		{
			IPermission[] pa = new IPermission [1];
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.CopyTo (pa, Int32.MinValue);
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void CopyTo_NegativeIndex ()
		{
			IPermission [] pa = new IPermission [1];
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Assertion));
			ps.CopyTo (pa, Int32.MinValue);
		}

		[Test]
		public void CopyTo_IndexOverLength_Empty ()
		{
			IPermission [] pa = new IPermission [1];
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.CopyTo (pa, pa.Length);
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void CopyTo_IndexOverLength ()
		{
			IPermission [] pa = new IPermission [1];
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new SecurityPermission (SecurityPermissionFlag.Assertion));
			ps.CopyTo (pa, pa.Length);
		}

		[Test]
		public void CopyTo ()
		{
			IPermission [] pa = new IPermission [1];
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (sp);
			ps.CopyTo (pa, 0);
			AssertEquals ("CopyTo", pa [0].ToString (), sp.ToString ());
			Assert ("Reference", Object.ReferenceEquals (pa [0], sp));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXmlNull () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXmlInvalidPermission () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityElement se = ps.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement ("InvalidPermissionSet", se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", se.Attribute ("version"));
			ps.FromXml (se2);
		}

		[Test]
		// [ExpectedException (typeof (ArgumentException))]
		public void FromXmlWrongVersion () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityElement se = ps.ToXml ();
			// can't modify - so we create our own
			SecurityElement se2 = new SecurityElement (se.Tag, se.Text);
			se2.AddAttribute ("class", se.Attribute ("class"));
			se2.AddAttribute ("version", "2");
			ps.FromXml (se2);
			// wow - here we accept a version 2 !!!
		}

		[Test]
		public void FromXmlEmpty () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityElement se = ps.ToXml ();
			AssertNotNull ("Empty.ToXml()", se);
			AssertEquals ("Empty.Count", 0, ps.Count);

			PermissionSet ps2 = (PermissionSet) ps.Copy ();
			ps2.FromXml (se);
			Assert ("FromXml-Copy.IsUnrestricted", !ps2.IsUnrestricted ()); 

			se.AddAttribute ("Unrestricted", "true");
			ps2.FromXml (se);
			Assert ("FromXml-Unrestricted.IsUnrestricted", ps2.IsUnrestricted ());
		}

		[Test]
		public void FromXmlOne () 
		{
			FileDialogPermission fdp = new FileDialogPermission (FileDialogPermissionAccess.Open);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (fdp);
			Assert ("ps1.IsEmpty", !ps1.IsEmpty ());

			PermissionSet ps = new PermissionSet (ps1);
			SecurityElement se = ps.ToXml ();
			AssertNotNull ("One.ToXml()", se);
			AssertEquals ("One.Count", 1, ps.Count);

			PermissionSet ps2 = (PermissionSet) ps.Copy ();
			ps2.FromXml (se);
			Assert ("FromXml-Copy.IsUnrestricted", !ps2.IsUnrestricted ()); 
			AssertEquals ("Copy.Count", 1, ps2.Count);

			se.AddAttribute ("Unrestricted", "true");
			ps2.FromXml (se);
			Assert ("FromXml-Unrestricted.IsUnrestricted", ps2.IsUnrestricted ());
#if NET_2_0
			AssertEquals ("Unrestricted.Count", 0, ps2.Count);
#else
			// IPermission not shown in XML but still present in Count
			AssertEquals ("Unrestricted.Count", 1, ps2.Count);
#endif
		}

		[Test]
#if !NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void FromXml_PermissionWithoutNamespace ()
		{
			SecurityElement child = new SecurityElement ("IPermission");
			child.AddAttribute ("class", "EnvironmentPermission");
			child.AddAttribute ("version", "1");
			child.AddAttribute ("Read", "USERNAME");

			SecurityElement se = new SecurityElement ("PermissionSet");
			se.AddAttribute ("class", "PermissionSet");
			se.AddAttribute ("version", "1");
			se.AddChild (child);

			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.FromXml (se);
#if NET_2_0
			// not enough information but:
			// a. it doesn't fail
			// b. it does work for policies
			AssertEquals ("Count", 0, ps.Count);
#endif
		}

		[Test]
#if !NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void FromXml_PermissionOutsideCorlib ()
		{
			SecurityElement child = new SecurityElement ("IPermission");
			child.AddAttribute ("class", "PrintingPermission");	// System.Drawing
			child.AddAttribute ("version", "1");
			child.AddAttribute ("Level", "DefaultPrinting");

			SecurityElement se = new SecurityElement ("PermissionSet");
			se.AddAttribute ("class", "PermissionSet");
			se.AddAttribute ("version", "1");
			se.AddChild (child);

			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.FromXml (se);
#if NET_2_0
			// not enough information but:
			// a. it doesn't fail
			// b. it does work for policies
			AssertEquals ("Count", 0, ps.Count);
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WithPermissionWithoutClass ()
		{
			SecurityElement child = new SecurityElement ("IPermission");
			child.AddAttribute ("version", "1");

			SecurityElement se = new SecurityElement ("PermissionSet");
			se.AddAttribute ("class", "PermissionSet");
			se.AddAttribute ("version", "1");
			se.AddChild (child);

			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.FromXml (se);
		}

		[Test]
		public void GetEnumerator ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			ps.AddPermission (sp);
			IEnumerator e = ps.GetEnumerator ();
			AssertNotNull ("GetEnumerator", e);
			int i=0;
			while (e.MoveNext ()) {
				Assert ("SecurityPermission", e.Current is SecurityPermission);
				i++;
			}
			AssertEquals ("Count", 1, i);
		}
#if NET_2_0
		[Test]
		public void GetHashCode_ ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			AssertEquals ("Empty", 0, ps.GetHashCode ());
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			ps.AddPermission (sp);
			Assert ("SecurityPermission", ps.GetHashCode () != 0);
			PermissionSet copy = ps.Copy ();
			Assert ("Copy", ps.GetHashCode () != copy.GetHashCode ());
		}
#endif
		[Test]
		public void GetPermission_None ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			AssertNull ("Empty", ps.GetPermission (typeof (SecurityPermission)));
		}

		[Test]
		public void GetPermission_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			AssertNull ("Empty", ps.GetPermission (typeof (SecurityPermission)));
		}

		private void Compare (string msg, PermissionSet ps, bool unrestricted, int count)
		{
			AssertNotNull (msg + "-NullCheck", ps);
			Assert (msg + "-State", (ps.IsUnrestricted () == unrestricted));
			AssertEquals (msg + "-Count", count, ps.Count);
		}

		[Test]
		public void Intersect_Empty ()
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			AssertNull ("None N null", ps1.Intersect (null));
			AssertNull ("None1 N None2", ps1.Intersect (ps2));
			AssertNull ("None2 N None1", ps2.Intersect (ps1));

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			AssertNull ("None1 N Unrestricted", ps1.Intersect (ups1));
			AssertNull ("Unrestricted N None1", ups1.Intersect (ps1));
			AssertNull ("Unrestricted N Null", ups1.Intersect (null));

			PermissionSet ups2 = new PermissionSet (PermissionState.Unrestricted);
			Compare ("ups1 N ups2", ups1.Intersect (ups2), true, 0);
			Compare ("ups2 N ups1", ups2.Intersect (ups1), true, 0);
		}

		[Test]
		public void Intersect_OnePermission ()
		{
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (sp);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			AssertNull ("PS1 N null", ps1.Intersect (null));
			AssertNull ("PS1 N None", ps1.Intersect (ps2));
			AssertNull ("None N PS1", ps2.Intersect (ps1));

			PermissionSet ps3 = ps1.Copy ();
			Compare ("PS1 N PS3", ps1.Intersect (ps3), false, 1);
			Compare ("PS3 N PS1", ps3.Intersect (ps1), false, 1);

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			Compare ("PS1 N Unrestricted", ps1.Intersect (ups1), false, 1);
			Compare ("Unrestricted N PS1", ups1.Intersect (ps1), false, 1);
		}

		[Test]
		public void Intersect_OneNonIUnrestrictedPermission ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (zip);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			AssertNull ("PS1 N null", ps1.Intersect (null));
			AssertNull ("PS1 N None", ps1.Intersect (ps2));
			AssertNull ("None N PS1", ps2.Intersect (ps1));

			PermissionSet ps3 = ps1.Copy ();
			Compare ("PS1 N PS3", ps1.Intersect (ps3), false, 1);
			Compare ("PS3 N PS1", ps3.Intersect (ps1), false, 1);

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			ups1.AddPermission (zip);
			Compare ("PS1 N Unrestricted", ps1.Intersect (ups1), false, 1);
			Compare ("Unrestricted N PS1", ups1.Intersect (ps1), false, 1);

			PermissionSet ups2 = new PermissionSet (PermissionState.Unrestricted);
			Compare ("UPS1 N UPS2", ups1.Intersect (ups2), true, 0);
			Compare ("UPS2 N UPS1", ups2.Intersect (ups1), true, 0);
			ups2.AddPermission (zip);
			Compare ("UPS1 N UPS2+ZIP", ups1.Intersect (ups2), true, 1);
			Compare ("UPS2+ZIP N UPS1", ups2.Intersect (ups1), true, 1);
		}

		[Test]
		public void IsEmpty_None ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert ("Empty.IsEmpty", ps.IsEmpty ());
			ps.AddPermission (new ZoneIdentityPermission (SecurityZone.NoZone));
			AssertEquals ("Count==1", 1, ps.Count);
			Assert ("Zip.IsEmpty", ps.IsEmpty ());	// yes empty!
		}

		[Test]
		public void IsEmpty_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			Assert ("Unrestricted.IsEmpty", !ps.IsEmpty ());
			ps.AddPermission (new ZoneIdentityPermission (SecurityZone.NoZone));
			AssertEquals ("Count==1", 1, ps.Count);
			Assert ("Zip.IsEmpty", !ps.IsEmpty ());	// yes empty!
		}

		[Test]
		public void IsSubset_Empty ()
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Assert ("None.IsSubsetOf(null)", ps1.IsSubsetOf (null));
			Assert ("None1.IsSubsetOf(None2)", ps1.IsSubsetOf (ps2));
			Assert ("None2.IsSubsetOf(None1)", ps2.IsSubsetOf (ps1));

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			Assert ("None1.IsSubsetOf(Unrestricted)", ps1.IsSubsetOf (ups1));
			Assert ("Unrestricted.IsSubsetOf(None1)", !ups1.IsSubsetOf (ps1));
			Assert ("Unrestricted.IsSubsetOf(Null)", !ups1.IsSubsetOf (null));

			PermissionSet ups2 = new PermissionSet (PermissionState.Unrestricted);
			Assert ("ups1IsSubsetOf(ups2)", ups1.IsSubsetOf (ups2));
			Assert ("ups2.IsSubsetOf(ups1)", ups2.IsSubsetOf (ups1));
		}

		[Test]
		public void IsSubset_OnePermission ()
		{
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (sp);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Assert ("PS1.IsSubset(null)", !ps1.IsSubsetOf (null));
			Assert ("PS1.IsSubset(None)", !ps1.IsSubsetOf (ps2));
			Assert ("None.IsSubset(PS1)", ps2.IsSubsetOf (ps1));

			PermissionSet ps3 = ps1.Copy ();
			Assert ("PS1.IsSubset(PS3)", ps1.IsSubsetOf (ps3));
			Assert ("PS3.IsSubset(PS1)", ps3.IsSubsetOf (ps1));

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			Assert ("PS1.IsSubset(Unrestricted)", ps1.IsSubsetOf (ups1));
			Assert ("Unrestricted.IsSubset(PS1)", !ups1.IsSubsetOf (ps1));
		}

		[Test]
		public void IsSubset_OneNonIUnrestrictedPermission ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (zip);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Assert ("PS1.IsSubset(null)", !ps1.IsSubsetOf (null));
			Assert ("PS1.IsSubset(None)", !ps1.IsSubsetOf (ps2));
			Assert ("None.IsSubset(PS1)", ps2.IsSubsetOf (ps1));

			PermissionSet ps3 = ps1.Copy ();
			Assert ("PS1.IsSubset(PS3)", ps1.IsSubsetOf (ps3));
			Assert ("PS3.IsSubset(PS1)", ps3.IsSubsetOf (ps1));

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			ups1.AddPermission (zip);
			Assert ("PS1.IsSubset(Unrestricted)", ps1.IsSubsetOf (ups1));
			Assert ("Unrestricted.IsSubset(PS1)", !ups1.IsSubsetOf (ps1));

			PermissionSet ups2 = new PermissionSet (PermissionState.Unrestricted);
			Assert ("UPS1Z.IsSubset(UPS2)", !ups1.IsSubsetOf (ups2));
			Assert ("UPS2.IsSubset(UPS1Z)", ups2.IsSubsetOf (ups1));
			ups2.AddPermission (zip);
			Assert ("UPS1Z.IsSubset(UPS2Z)", ups1.IsSubsetOf (ups2));
			Assert ("UPS2Z.IsSubset(UPS1Z)", ups2.IsSubsetOf (ups1));
		}

		[Test]
		public void RemovePermission_Null () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			AssertNull (ps.RemovePermission (null));
		}

		[Test]
		public void RemovePermission_None () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			AssertNull ("Empty", ps.RemovePermission (typeof (SecurityPermission)));
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			ps.AddPermission (sp);
			SecurityPermission removed = (SecurityPermission) ps.RemovePermission (typeof (SecurityPermission));
			AssertNotNull ("SecurityPermission", removed);
			AssertEquals ("Flags", sp.Flags, removed.Flags);
			AssertNull ("Empty-Again", ps.RemovePermission (typeof (SecurityPermission)));
		}

		[Test]
		public void RemovePermission_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			AssertNull ("Empty", ps.RemovePermission (typeof (SecurityPermission)));
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			ps.AddPermission (sp);
			AssertNull ("SecurityPermissionn", ps.RemovePermission (typeof (SecurityPermission)));
			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			ps.AddPermission (zip);
			ZoneIdentityPermission removed = (ZoneIdentityPermission)ps.RemovePermission (typeof (ZoneIdentityPermission));
			AssertNotNull ("ZoneIdentityPermission", removed);
		}

		[Test]
		public void SetPermission_Null ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			AssertNull (ps.SetPermission (null));
		}

		[Test]
		public void SetPermission_None ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			AssertEquals ("Empty", 0, ps.Count);
			Assert ("State-None", !ps.IsUnrestricted ());

			SecurityPermission sp = new SecurityPermission (PermissionState.Unrestricted);
			SecurityPermission result = (SecurityPermission)ps.SetPermission (sp);
			AssertEquals ("SecurityPermission", 1, ps.Count);
			AssertEquals ("Flags", SecurityPermissionFlag.AllFlags, result.Flags);
			Assert ("State-None-2", !ps.IsUnrestricted ());

			sp = new SecurityPermission (SecurityPermissionFlag.ControlAppDomain);
			result = (SecurityPermission)ps.SetPermission (sp);
			AssertEquals ("SecurityPermission-2", 1, ps.Count);
			AssertEquals ("Flags", SecurityPermissionFlag.ControlAppDomain, result.Flags);

			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			ZoneIdentityPermission zipr = (ZoneIdentityPermission) ps.SetPermission (zip);
			AssertEquals ("ZoneIdentityPermission", 2, ps.Count);
			AssertEquals ("SecurityZone", SecurityZone.MyComputer, zipr.SecurityZone);

			zip = new ZoneIdentityPermission (SecurityZone.Intranet);
			zipr = (ZoneIdentityPermission)ps.SetPermission (zip);
			AssertEquals ("ZoneIdentityPermission", 2, ps.Count);
			AssertEquals ("SecurityZone", SecurityZone.Intranet, zipr.SecurityZone);
		}

		[Test]
		public void SetPermission_Unrestricted ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.Unrestricted);
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			AssertEquals ("Empty", 0, ps.Count);
			Assert ("State-Unrestricted", ps.IsUnrestricted ());

			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			ZoneIdentityPermission zipr = (ZoneIdentityPermission)ps.SetPermission (zip);
			AssertEquals ("ZoneIdentityPermission", 1, ps.Count);
			AssertEquals ("SecurityZone", SecurityZone.MyComputer, zipr.SecurityZone);
			Assert ("State-Unrestricted-2", ps.IsUnrestricted ());

			zip = new ZoneIdentityPermission (SecurityZone.Intranet);
			zipr = (ZoneIdentityPermission)ps.SetPermission (zip);
			AssertEquals ("ZoneIdentityPermission-2", 1, ps.Count);
			AssertEquals ("SecurityZone-2", SecurityZone.Intranet, zipr.SecurityZone);

			SecurityPermission result = (SecurityPermission)ps.SetPermission (sp);
			AssertEquals ("SecurityPermission", 2, ps.Count);
			AssertEquals ("Flags", SecurityPermissionFlag.AllFlags, result.Flags);
			Assert ("State-None", !ps.IsUnrestricted ());

			sp = new SecurityPermission (SecurityPermissionFlag.ControlAppDomain);
			result = (SecurityPermission)ps.SetPermission (sp);
			AssertEquals ("SecurityPermission-2", 2, ps.Count);
			AssertEquals ("Flags-2", SecurityPermissionFlag.ControlAppDomain, result.Flags);
		}

		[Test]
		public void ToXmlNone () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityElement se = ps.ToXml ();
			Assert ("None.ToString().StartsWith", ps.ToString().StartsWith ("<PermissionSet"));
			AssertEquals ("None.class", "System.Security.PermissionSet", (se.Attributes ["class"] as string));
			AssertEquals ("None.version", "1", (se.Attributes ["version"] as string));
			AssertNull ("None.Unrestricted", (se.Attributes ["Unrestricted"] as string));
		}

		[Test]
		public void ToXmlUnrestricted () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			SecurityElement se = ps.ToXml ();
			Assert ("Unrestricted.ToString().StartsWith", ps.ToString().StartsWith ("<PermissionSet"));
			AssertEquals ("Unrestricted.class", "System.Security.PermissionSet", (se.Attributes ["class"] as string));
			AssertEquals ("Unrestricted.version", "1", (se.Attributes ["version"] as string));
			AssertEquals ("Unrestricted.Unrestricted", "true", (se.Attributes ["Unrestricted"] as string));
		}

		[Test]
		public void Union_Empty ()
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Compare ("None U null", ps1.Union (null), false, 0);
			Compare ("None1 U None2", ps1.Union (ps2), false, 0);
			Compare ("None2 U None1", ps2.Union (ps1), false, 0);

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			Compare ("None1 U Unrestricted", ps1.Union (ups1), true, 0);
			Compare ("Unrestricted U None1", ups1.Union (ps1), true, 0);
			Compare ("Unrestricted U Null", ups1.Union (null), true, 0);

			PermissionSet ups2 = new PermissionSet (PermissionState.Unrestricted);
			Compare ("ups1 U ups2", ups1.Union (ups2), true, 0);
			Compare ("ups2 U ups1", ups2.Union (ups1), true, 0);
		}

		[Test]
		public void Union_OnePermission ()
		{
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (sp);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Compare ("PS1 U null", ps1.Union (null), false, 1);
			Compare ("PS1 U None", ps1.Union (ps2), false, 1);
			Compare ("None U PS1", ps2.Union (ps1), false, 1);

			PermissionSet ps3 = ps1.Copy ();
			Compare ("PS1 U PS3", ps1.Union (ps3), false, 1);
			Compare ("PS3 U PS1", ps3.Union (ps1), false, 1);

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			Compare ("PS1 U Unrestricted", ps1.Union (ups1), true, 0);
			Compare ("Unrestricted U PS1", ups1.Union (ps1), true, 0);
		}

		[Test]
		public void Union_OneNonIUnrestrictedPermission ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (zip);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Compare ("PS1 U null", ps1.Union (null), false, 1);
			Compare ("PS1 U None", ps1.Union (ps2), false, 1);
			Compare ("None U PS1", ps2.Union (ps1), false, 1);

			PermissionSet ps3 = ps1.Copy ();
			Compare ("PS1 U PS3", ps1.Union (ps3), false, 1);
			Compare ("PS3 U PS1", ps3.Union (ps1), false, 1);

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			ups1.AddPermission (zip);
			Compare ("PS1 U Unrestricted", ps1.Union (ups1), true, 1);
			Compare ("Unrestricted U PS1", ups1.Union (ps1), true, 1);

			PermissionSet ups2 = new PermissionSet (PermissionState.Unrestricted);
			Compare ("UPS1 U UPS2", ups1.Union (ups1), true, 1);
			Compare ("UPS2 U UPS1", ups2.Union (ups1), true, 1);
			ups2.AddPermission (zip);
			Compare ("UPS1 U UPS2+ZIP", ups1.Union (ups2), true, 1);
			Compare ("UPS2+ZIP U UPS1", ups2.Union (ups1), true, 1);
		}
	}
}
