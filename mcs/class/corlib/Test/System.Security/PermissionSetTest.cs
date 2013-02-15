//
// PermissionSetTest.cs - NUnit Test Cases for PermissionSet
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
using System.Collections;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace MonoTests.System.Security {

	[TestFixture]
	public class PermissionSetTest {

		[Test]
		public void PermissionStateNone () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert.IsTrue (!ps.IsUnrestricted (), "PermissionStateNone.IsUnrestricted");
			Assert.IsTrue (ps.IsEmpty (), "PermissionStateNone.IsEmpty");
			Assert.IsTrue (!ps.IsReadOnly, "PermissionStateNone.IsReadOnly");
			Assert.AreEqual (ps.ToXml ().ToString (), ps.ToString (), "PermissionStateNone.ToXml().ToString()==ToString()");
			Assert.IsTrue (!ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
		}

		[Test]
		public void PermissionStateUnrestricted () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			Assert.IsTrue (ps.IsUnrestricted (), "PermissionStateUnrestricted.IsUnrestricted");
			Assert.IsTrue (!ps.IsEmpty (), "PermissionStateUnrestricted.IsEmpty");
			Assert.IsTrue (!ps.IsReadOnly, "PermissionStateUnrestricted.IsReadOnly");
			Assert.AreEqual (ps.ToXml ().ToString (), ps.ToString (), "PermissionStateUnrestricted.ToXml().ToString()==ToString()");
			Assert.IsTrue (!ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
		}

		[Test]
		public void PermissionSetNull () 
		{
			// no exception is thrown
			PermissionSet ps = new PermissionSet (null);
#if NET_2_0
			Assert.IsTrue (!ps.IsUnrestricted (), "PermissionStateNull.IsUnrestricted");
			Assert.IsTrue (ps.IsEmpty (), "PermissionStateNull.IsEmpty");
#else
			Assert.IsTrue (ps.IsUnrestricted (), "PermissionStateNull.IsUnrestricted");
			Assert.IsTrue (!ps.IsEmpty (), "PermissionStateNull.IsEmpty");
#endif
			Assert.IsTrue (!ps.IsReadOnly, "PermissionStateNull.IsReadOnly");
			Assert.AreEqual (ps.ToXml ().ToString (), ps.ToString (), "PermissionStateNull.ToXml().ToString()==ToString()");
			Assert.IsTrue (!ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
		}

		[Test]
		public void PermissionSetPermissionSet () 
		{
			FileDialogPermission fdp = new FileDialogPermission (FileDialogPermissionAccess.Open);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (fdp);
			Assert.IsTrue (!ps1.IsEmpty (), "ps1.IsEmpty");

			PermissionSet ps = new PermissionSet (ps1);
			Assert.IsTrue (!ps.IsUnrestricted (), "PermissionSetPermissionSet.IsUnrestricted");
			Assert.IsTrue (!ps.IsEmpty (), "PermissionSetPermissionSet.IsEmpty");
			Assert.IsTrue (!ps.IsReadOnly, "PermissionSetPermissionSet.IsReadOnly");
			Assert.AreEqual (ps.ToXml ().ToString (), ps.ToString (), "PermissionSetPermissionSet.ToXml().ToString()==ToString()");
			Assert.IsTrue (!ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
		}

		[Test]
		public void PermissionSetNamedPermissionSet ()
		{
			NamedPermissionSet nps = new NamedPermissionSet ("Test", PermissionState.Unrestricted);
			PermissionSet ps = new PermissionSet (nps);
			Assert.IsTrue (ps.IsUnrestricted (), "IsUnrestricted");
			Assert.IsTrue (!ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
		}

		[Test]
		public void AddPermission ()
		{
			SecurityPermission sp1 = new SecurityPermission (SecurityPermissionFlag.ControlEvidence);
			SecurityPermission sp2 = new SecurityPermission (SecurityPermissionFlag.ControlPolicy);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityPermission result = (SecurityPermission)ps.AddPermission (sp1);
			Assert.AreEqual (1, ps.Count, "1-ControlEvidence");
			Assert.AreEqual (SecurityPermissionFlag.ControlEvidence, result.Flags, "Flags-1");

			result = (SecurityPermission)ps.AddPermission (sp2);
			Assert.AreEqual (1, ps.Count, "1-ControlEvidence+ControlPolicy");
			Assert.AreEqual (SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence, result.Flags, "Flags-2");

			result = (SecurityPermission)ps.AddPermission (sp2);
			Assert.AreEqual (1, ps.Count, "no change-1");
			Assert.AreEqual (SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence, result.Flags, "Flags-3");

			result = (SecurityPermission)ps.AddPermission (sp1);
			Assert.AreEqual (1, ps.Count, "no change-2");
			Assert.AreEqual (SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence, result.Flags, "Flags-4");
		}

		[Test]
		public void AddPermission_Null ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			IPermission result = ps.AddPermission (null);
			Assert.IsNull (result, "Add(null)");
			Assert.AreEqual (0, ps.Count, "0");
		}

		[Test]
		public void AddPermission_SetUnrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.ControlEvidence);
			IPermission result = ps.AddPermission (sp);
			Assert.IsNotNull (result, "Add(SecurityPermission)");
			Assert.AreEqual (SecurityPermissionFlag.AllFlags, (result as SecurityPermission).Flags, "SecurityPermission");
			Assert.AreEqual (0, ps.Count, "0");
			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			result = ps.AddPermission (zip);
			Assert.IsNotNull (result, "Add(ZoneIdentityPermission)");
#if NET_2_0
			// Identity permissions aren't added to unrestricted permission sets in 2.0
			Assert.AreEqual (SecurityZone.NoZone, (result as ZoneIdentityPermission).SecurityZone, "ZoneIdentityPermission");
			Assert.AreEqual (0, ps.Count, "1");
#else
			Assert.AreEqual (zip.SecurityZone, (result as ZoneIdentityPermission).SecurityZone, "ZoneIdentityPermission");
			Assert.AreEqual (1, ps.Count, "1");
#endif
		}

		[Test]
		public void AddPermission_PermissionUnrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityPermission sp = new SecurityPermission (PermissionState.Unrestricted);
			IPermission result = ps.AddPermission (sp);
			Assert.IsNotNull (result, "Add(SecurityPermission)");
			Assert.AreEqual (SecurityPermissionFlag.AllFlags, (result as SecurityPermission).Flags, "SecurityPermission");
			Assert.AreEqual (1, ps.Count, "1");
			Assert.IsTrue (!ps.IsUnrestricted (), "State");
		}

		[Test]
		public void AddPermission_NonCasPermission ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new PrincipalPermission ("name", "role"));
			Assert.AreEqual (1, ps.Count, "Count");
			Assert.IsTrue (!ps.IsEmpty (), "IsEmpty");
		}

		[Test]
		public void AddPermission_NonCasPermissionNone ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new PrincipalPermission (PermissionState.None));
			Assert.AreEqual (1, ps.Count, "Count");
			Assert.IsTrue (ps.IsEmpty (), "IsEmpty");
		}

		[Test]
		public void AddPermission_NonCasPermissionUnrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new PrincipalPermission (PermissionState.Unrestricted));
			Assert.AreEqual (1, ps.Count, "Count");
			Assert.IsTrue (!ps.IsEmpty (), "IsEmpty");
		}

		[Test]
		public void AddPermission_NonCasPermission_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			ps.AddPermission (new PrincipalPermission ("name", "role"));
			Assert.AreEqual (0, ps.Count, "Count");
			Assert.IsTrue (!ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
		}

		[Test]
		public void AddPermission_NonCasPermissionNone_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			ps.AddPermission (new PrincipalPermission (PermissionState.None));
			Assert.AreEqual (0, ps.Count, "Count");
			Assert.IsTrue (!ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
		}

		[Test]
		public void AddPermission_NonCasPermissionUnrestricted_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			ps.AddPermission (new PrincipalPermission (PermissionState.Unrestricted));
			Assert.AreEqual (0, ps.Count, "Count");
			Assert.IsTrue (!ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
		}

		[Test]
		public void AddPermission_NoCopy ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityPermission sp1 = new SecurityPermission (SecurityPermissionFlag.ControlEvidence);
			SecurityPermission result = (SecurityPermission)ps.AddPermission (sp1);
			SecurityPermission entry = (SecurityPermission)ps.GetPermission (typeof (SecurityPermission));

			// are they the same (reference) or different ?
			sp1.Flags = SecurityPermissionFlag.AllFlags;

			result.Flags = SecurityPermissionFlag.Assertion;
		}

		[Test]
		public void ContainsNonCodeAccessPermissions ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert.IsTrue (!ps.ContainsNonCodeAccessPermissions (), "Empty");

			SecurityPermission sp = new SecurityPermission (PermissionState.Unrestricted);
			ps.AddPermission (sp);
			Assert.IsTrue (!ps.ContainsNonCodeAccessPermissions (), "SecurityPermission");

			PrincipalPermission pp = new PrincipalPermission ("mono", "hacker");
			ps.AddPermission (pp);
			Assert.IsTrue (ps.ContainsNonCodeAccessPermissions (), "PrincipalPermission");
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
			Assert.IsNull (result);
		}

		[Test]
		public void ConvertPermissionSet_NullData ()
		{
			byte[] result = PermissionSet.ConvertPermissionSet ("BINARY", null, "XML");
			Assert.IsNull (result);
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
			//Assert.IsTrue (BitConverter.ToString (result) != BitConverter.ToString (result2), "BINARY!=BINARY");
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
			Assert.AreEqual (BitConverter.ToString (result), BitConverter.ToString (result2), "XML==XMLASCII");
			byte[] back = PermissionSet.ConvertPermissionSet ("BINARY", result, "XML");
			Assert.AreEqual (Encoding.ASCII.GetString (back), ps.ToString (), "PS-XML");
			back = PermissionSet.ConvertPermissionSet ("BINARY", result2, "XMLASCII");
			Assert.AreEqual (Encoding.ASCII.GetString (back), ps.ToString (), "PS-XMLASCII");
		}

		[Test]
		public void ConvertPermissionSet_XmlToXml ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			byte[] data = Encoding.ASCII.GetBytes (ps.ToString ());
			byte[] result = PermissionSet.ConvertPermissionSet ("XML", data, "XML");
			Assert.AreEqual (Encoding.ASCII.GetString (result), ps.ToString (), "PS-XML");

			result = PermissionSet.ConvertPermissionSet ("XMLASCII", data, "XMLASCII");
			Assert.AreEqual (Encoding.ASCII.GetString (result), ps.ToString (), "PS-XMLASCII");
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
			Assert.IsTrue (!copy.IsUnrestricted (), "1.State");
			Assert.AreEqual (0, copy.Count, "1.Count");

			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.ControlEvidence);
			IPermission result = ps.AddPermission (sp);
			Assert.IsNotNull (result, "1.Add");
			copy = ps.Copy ();
			Assert.IsTrue (!copy.IsUnrestricted (), "2.State");
			Assert.AreEqual (1, copy.Count, "2.Count");

			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			result = ps.AddPermission (zip);
			Assert.IsNotNull (result, "2.Add");
			copy = ps.Copy ();
			Assert.IsTrue (!copy.IsUnrestricted (), "3.State");
			Assert.AreEqual (2, copy.Count, "3.Count");
		}

		[Test]
		public void Copy_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			PermissionSet copy = ps.Copy ();
			Assert.IsTrue (copy.IsUnrestricted (), "1.State");
			Assert.AreEqual (0, copy.Count, "1.Count");

			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.ControlEvidence);
			IPermission result = ps.AddPermission (sp);
			Assert.IsNotNull (result, "1.Add");
			copy = ps.Copy ();
			Assert.IsTrue (copy.IsUnrestricted (), "2.State");
			Assert.AreEqual (0, copy.Count, "2.Count");

			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			result = ps.AddPermission (zip);
			Assert.IsNotNull (result, "2.Add");
			copy = ps.Copy ();
			Assert.IsTrue (copy.IsUnrestricted (), "3.State");
#if NET_2_0
			// Identity permissions aren't added to unrestricted permission sets in 2.0
			Assert.AreEqual (0, copy.Count, "3.Count");
#else
			Assert.AreEqual (1, copy.Count, "3.Count");
#endif
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
			Assert.AreEqual (pa [0].ToString (), sp.ToString (), "CopyTo");
			Assert.IsTrue (Object.ReferenceEquals (pa [0], sp), "Reference");
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
			Assert.IsNotNull (se, "Empty.ToXml()");
			Assert.AreEqual (0, ps.Count, "Empty.Count");

			PermissionSet ps2 = (PermissionSet) ps.Copy ();
			ps2.FromXml (se);
			Assert.IsTrue (!ps2.IsUnrestricted () , "FromXml-Copy.IsUnrestricted");

			se.AddAttribute ("Unrestricted", "true");
			ps2.FromXml (se);
			Assert.IsTrue (ps2.IsUnrestricted (), "FromXml-Unrestricted.IsUnrestricted");
		}

		[Test]
		public void FromXmlOne () 
		{
			FileDialogPermission fdp = new FileDialogPermission (FileDialogPermissionAccess.Open);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (fdp);
			Assert.IsTrue (!ps1.IsEmpty (), "ps1.IsEmpty");

			PermissionSet ps = new PermissionSet (ps1);
			SecurityElement se = ps.ToXml ();
			Assert.IsNotNull (se, "One.ToXml()");
			Assert.AreEqual (1, ps.Count, "One.Count");

			PermissionSet ps2 = (PermissionSet) ps.Copy ();
			ps2.FromXml (se);
			Assert.IsTrue (!ps2.IsUnrestricted () , "FromXml-Copy.IsUnrestricted");
			Assert.AreEqual (1, ps2.Count, "Copy.Count");

			se.AddAttribute ("Unrestricted", "true");
			ps2.FromXml (se);
			Assert.IsTrue (ps2.IsUnrestricted (), "FromXml-Unrestricted.IsUnrestricted");
#if NET_2_0
			Assert.AreEqual (0, ps2.Count, "Unrestricted.Count");
#else
			// IPermission not shown in XML but still present in Count
			Assert.AreEqual (1, ps2.Count, "Unrestricted.Count");
#endif
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (TypeLoadException))]
#else
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
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (TypeLoadException))]
#else
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
			Assert.IsNotNull (e, "GetEnumerator");
			int i=0;
			while (e.MoveNext ()) {
				Assert.IsTrue (e.Current is SecurityPermission, "SecurityPermission");
				i++;
			}
			Assert.AreEqual (1, i, "Count");
		}
#if NET_2_0
		[Test]
		public void GetHashCode_ ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert.AreEqual (0, ps.GetHashCode (), "Empty");
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			ps.AddPermission (sp);
			Assert.IsTrue (ps.GetHashCode () != 0, "SecurityPermission");
			PermissionSet copy = ps.Copy ();
			Assert.IsTrue (ps.GetHashCode () != copy.GetHashCode (), "Copy");
		}
#endif
		[Test]
		public void GetPermission_Null ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert.IsNull (ps.GetPermission (null), "Empty");
		}

		[Test]
		public void GetPermission_None ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert.IsNull (ps.GetPermission (typeof (SecurityPermission)), "Empty");
		}

		[Test]
		public void GetPermission_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			Assert.IsNull (ps.GetPermission (typeof (SecurityPermission)), "Empty");
		}

		[Test]
		public void GetPermission_Subclass ()
		{
			IsolatedStorageFilePermission isfp = new IsolatedStorageFilePermission (PermissionState.Unrestricted);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (isfp);
			Assert.IsNull (ps.GetPermission (typeof (IsolatedStoragePermission)), "Subclass");
		}

		private void Compare (string msg, PermissionSet ps, bool unrestricted, int count)
		{
			Assert.IsNotNull (ps, msg + "-NullCheck");
			Assert.IsTrue ((ps.IsUnrestricted () == unrestricted), msg + "-State");
			Assert.AreEqual (count, ps.Count, msg + "-Count");
		}

		[Test]
		public void Intersect_Empty ()
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Assert.IsNull (ps1.Intersect (null), "None N null");
			Assert.IsNull (ps1.Intersect (ps2), "None1 N None2");
			Assert.IsNull (ps2.Intersect (ps1), "None2 N None1");

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			Assert.IsNull (ps1.Intersect (ups1), "None1 N Unrestricted");
			Assert.IsNull (ups1.Intersect (ps1), "Unrestricted N None1");
			Assert.IsNull (ups1.Intersect (null), "Unrestricted N Null");

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
			Assert.IsNull (ps1.Intersect (null), "PS1 N null");
			Assert.IsNull (ps1.Intersect (ps2), "PS1 N None");
			Assert.IsNull (ps2.Intersect (ps1), "None N PS1");

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
			Assert.IsNull (ps1.Intersect (null), "PS1 N null");
			Assert.IsNull (ps1.Intersect (ps2), "PS1 N None");
			Assert.IsNull (ps2.Intersect (ps1), "None N PS1");

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
#if NET_2_0
			// Identity permissions aren't added to unrestricted permission sets in 2.0
			Compare ("UPS1 N UPS2+ZIP", ups1.Intersect (ups2), true, 0);
			Compare ("UPS2+ZIP N UPS1", ups2.Intersect (ups1), true, 0);
#else
			Compare ("UPS1 N UPS2+ZIP", ups1.Intersect (ups2), true, 1);
			Compare ("UPS2+ZIP N UPS1", ups2.Intersect (ups1), true, 1);
#endif
		}

		[Test]
		public void IsEmpty_None ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert.IsTrue (ps.IsEmpty (), "Empty.IsEmpty");
			ps.AddPermission (new ZoneIdentityPermission (SecurityZone.NoZone));
			Assert.AreEqual (1, ps.Count, "Count==1");
			Assert.IsTrue (ps.IsEmpty ());	// yes empty!, "Zip.IsEmpty");
		}

		[Test]
		public void IsEmpty_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			Assert.IsTrue (!ps.IsEmpty (), "Unrestricted.IsEmpty");
			ps.AddPermission (new ZoneIdentityPermission (SecurityZone.NoZone));
#if NET_2_0
			// Identity permissions aren't added to unrestricted permission sets in 2.0
			Assert.AreEqual (0, ps.Count, "Count==0");
#else
			Assert.AreEqual (1, ps.Count, "Count==1");
#endif
			Assert.IsTrue (!ps.IsEmpty ());	// yes empty!, "Zip.IsEmpty");
		}

		[Test]
		public void IsSubset_Empty ()
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Assert.IsTrue (ps1.IsSubsetOf (null), "None.IsSubsetOf(null)");
			Assert.IsTrue (ps1.IsSubsetOf (ps2), "None1.IsSubsetOf(None2)");
			Assert.IsTrue (ps2.IsSubsetOf (ps1), "None2.IsSubsetOf(None1)");

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			Assert.IsTrue (ps1.IsSubsetOf (ups1), "None1.IsSubsetOf(Unrestricted)");
			Assert.IsTrue (!ups1.IsSubsetOf (ps1), "Unrestricted.IsSubsetOf(None1)");
			Assert.IsTrue (!ups1.IsSubsetOf (null), "Unrestricted.IsSubsetOf(Null)");

			PermissionSet ups2 = new PermissionSet (PermissionState.Unrestricted);
			Assert.IsTrue (ups1.IsSubsetOf (ups2), "ups1IsSubsetOf(ups2)");
			Assert.IsTrue (ups2.IsSubsetOf (ups1), "ups2.IsSubsetOf(ups1)");
		}

		[Test]
		public void IsSubset_OnePermission ()
		{
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (sp);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Assert.IsTrue (!ps1.IsSubsetOf (null), "PS1.IsSubset(null)");
			Assert.IsTrue (!ps1.IsSubsetOf (ps2), "PS1.IsSubset(None)");
			Assert.IsTrue (ps2.IsSubsetOf (ps1), "None.IsSubset(PS1)");

			PermissionSet ps3 = ps1.Copy ();
			Assert.IsTrue (ps1.IsSubsetOf (ps3), "PS1.IsSubset(PS3)");
			Assert.IsTrue (ps3.IsSubsetOf (ps1), "PS3.IsSubset(PS1)");

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			Assert.IsTrue (ps1.IsSubsetOf (ups1), "PS1.IsSubset(Unrestricted)");
			Assert.IsTrue (!ups1.IsSubsetOf (ps1), "Unrestricted.IsSubset(PS1)");
		}

		[Test]
		public void IsSubset_OneNonIUnrestrictedPermission ()
		{
			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (zip);
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Assert.IsTrue (!ps1.IsSubsetOf (null), "PS1.IsSubset(null)");
			Assert.IsTrue (!ps1.IsSubsetOf (ps2), "PS1.IsSubset(None)");
			Assert.IsTrue (ps2.IsSubsetOf (ps1), "None.IsSubset(PS1)");

			PermissionSet ps3 = ps1.Copy ();
			Assert.IsTrue (ps1.IsSubsetOf (ps3), "PS1.IsSubset(PS3)");
			Assert.IsTrue (ps3.IsSubsetOf (ps1), "PS3.IsSubset(PS1)");

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			ups1.AddPermission (zip);
			Assert.IsTrue (ps1.IsSubsetOf (ups1), "PS1.IsSubset(Unrestricted)");
			Assert.IsTrue (!ups1.IsSubsetOf (ps1), "Unrestricted.IsSubset(PS1)");

			PermissionSet ups2 = new PermissionSet (PermissionState.Unrestricted);
#if NET_2_0
			// as ZoneIdentityPermission isn't added UPS1Z == UPS2
			Assert.IsTrue (ups1.IsSubsetOf (ups2), "UPS1Z.IsSubset(UPS2)");
#else
			Assert.IsTrue (!ups1.IsSubsetOf (ups2), "UPS1Z.IsSubset(UPS2)");
#endif
			Assert.IsTrue (ups2.IsSubsetOf (ups1), "UPS2.IsSubset(UPS1Z)");
			ups2.AddPermission (zip);
			Assert.IsTrue (ups1.IsSubsetOf (ups2), "UPS1Z.IsSubset(UPS2Z)");
			Assert.IsTrue (ups2.IsSubsetOf (ups1), "UPS2Z.IsSubset(UPS1Z)");
		}

		[Test]
		public void IsSubset_NonCasPermission ()
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (new PrincipalPermission ("name", "role"));
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Assert.IsTrue (!ps1.IsSubsetOf (null), "PS1.IsSubset(null)");
			Assert.IsTrue (!ps1.IsSubsetOf (ps2), "PS1.IsSubset(None)");
			Assert.IsTrue (ps2.IsSubsetOf (ps1), "None.IsSubset(PS1)");

			PermissionSet ps3 = ps1.Copy ();
			Assert.IsTrue (ps1.IsSubsetOf (ps3), "PS1.IsSubset(PS3)");
			Assert.IsTrue (ps3.IsSubsetOf (ps1), "PS3.IsSubset(PS1)");

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
#if NET_2_0
			Assert.IsTrue (ps1.IsSubsetOf (ups1), "PS1.IsSubset(Unrestricted)");
#else
			Assert.IsTrue (!ps1.IsSubsetOf (ups1), "PS1.IsSubset(Unrestricted)");
#endif
			Assert.IsTrue (!ups1.IsSubsetOf (ps1), "Unrestricted.IsSubset(PS1)");
		}

		[Test]
		public void IsSubset_NonCasPermission_None ()
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (new PrincipalPermission (PermissionState.None));
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Assert.IsTrue (ps1.IsSubsetOf (null), "PS1.IsSubset(null)");
			Assert.IsTrue (ps1.IsSubsetOf (ps2), "PS1.IsSubset(None)");
			Assert.IsTrue (ps2.IsSubsetOf (ps1), "None.IsSubset(PS1)");

			PermissionSet ps3 = ps1.Copy ();
			Assert.IsTrue (ps1.IsSubsetOf (ps3), "PS1.IsSubset(PS3)");
			Assert.IsTrue (ps3.IsSubsetOf (ps1), "PS3.IsSubset(PS1)");

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
			Assert.IsTrue (ps1.IsSubsetOf (ups1), "PS1.IsSubset(Unrestricted)");
			Assert.IsTrue (!ups1.IsSubsetOf (ps1), "Unrestricted.IsSubset(PS1)");
		}

		[Test]
		public void IsSubset_NonCasPermission_Unrestricted ()
		{
			PermissionSet ps1 = new PermissionSet (PermissionState.None);
			ps1.AddPermission (new PrincipalPermission (PermissionState.Unrestricted));
			PermissionSet ps2 = new PermissionSet (PermissionState.None);
			Assert.IsTrue (!ps1.IsSubsetOf (null), "PS1.IsSubset(null)");
			Assert.IsTrue (!ps1.IsSubsetOf (ps2), "PS1.IsSubset(None)");
			Assert.IsTrue (ps2.IsSubsetOf (ps1), "None.IsSubset(PS1)");

			PermissionSet ps3 = ps1.Copy ();
			Assert.IsTrue (ps1.IsSubsetOf (ps3), "PS1.IsSubset(PS3)");
			Assert.IsTrue (ps3.IsSubsetOf (ps1), "PS3.IsSubset(PS1)");

			PermissionSet ups1 = new PermissionSet (PermissionState.Unrestricted);
#if NET_2_0
			Assert.IsTrue (ps1.IsSubsetOf (ups1), "PS1.IsSubset(Unrestricted)");
#else
			Assert.IsTrue (!ps1.IsSubsetOf (ups1), "PS1.IsSubset(Unrestricted)");
#endif
			Assert.IsTrue (!ups1.IsSubsetOf (ps1), "Unrestricted.IsSubset(PS1)");
		}

		[Test]
		public void RemovePermission_Null () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert.IsNull (ps.RemovePermission (null));
		}

		[Test]
		public void RemovePermission_None () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert.IsNull (ps.RemovePermission (typeof (SecurityPermission)), "Empty");
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			ps.AddPermission (sp);
			SecurityPermission removed = (SecurityPermission) ps.RemovePermission (typeof (SecurityPermission));
			Assert.IsNotNull (removed, "SecurityPermission");
			Assert.AreEqual (sp.Flags, removed.Flags, "Flags");
			Assert.IsNull (ps.RemovePermission (typeof (SecurityPermission)), "Empty-Again");
		}

		[Test]
		public void RemovePermission_Unrestricted ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			Assert.IsNull (ps.RemovePermission (typeof (SecurityPermission)), "Empty");
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			ps.AddPermission (sp);
			Assert.IsNull (ps.RemovePermission (typeof (SecurityPermission)), "SecurityPermissionn");
			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			ps.AddPermission (zip);
			ZoneIdentityPermission removed = (ZoneIdentityPermission)ps.RemovePermission (typeof (ZoneIdentityPermission));
#if NET_2_0
			// identity permissions aren't added to unrestricted permission sets
			// so they cannot be removed later (hence the null)
			Assert.IsNull (removed, "ZoneIdentityPermission");
#else
			Assert.IsNotNull (removed, "ZoneIdentityPermission");
#endif
		}

		[Test]
		public void SetPermission_Null ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert.IsNull (ps.SetPermission (null));
		}

		[Test]
		public void SetPermission_None ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			Assert.AreEqual (0, ps.Count, "Empty");
			Assert.IsTrue (!ps.IsUnrestricted (), "State-None");

			SecurityPermission sp = new SecurityPermission (PermissionState.Unrestricted);
			SecurityPermission result = (SecurityPermission)ps.SetPermission (sp);
			Assert.AreEqual (1, ps.Count, "SecurityPermission");
			Assert.AreEqual (SecurityPermissionFlag.AllFlags, result.Flags, "Flags");
			Assert.IsTrue (!ps.IsUnrestricted (), "State-None-2");

			sp = new SecurityPermission (SecurityPermissionFlag.ControlAppDomain);
			result = (SecurityPermission)ps.SetPermission (sp);
			Assert.AreEqual (1, ps.Count, "SecurityPermission-2");
			Assert.AreEqual (SecurityPermissionFlag.ControlAppDomain, result.Flags, "Flags");

			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			ZoneIdentityPermission zipr = (ZoneIdentityPermission) ps.SetPermission (zip);
			Assert.AreEqual (2, ps.Count, "ZoneIdentityPermission");
			Assert.AreEqual (SecurityZone.MyComputer, zipr.SecurityZone, "SecurityZone");

			zip = new ZoneIdentityPermission (SecurityZone.Intranet);
			zipr = (ZoneIdentityPermission)ps.SetPermission (zip);
			Assert.AreEqual (2, ps.Count, "ZoneIdentityPermission");
			Assert.AreEqual (SecurityZone.Intranet, zipr.SecurityZone, "SecurityZone");
		}

		[Test]
		public void SetPermission_Unrestricted ()
		{
			SecurityPermission sp = new SecurityPermission (PermissionState.Unrestricted);
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			Assert.AreEqual (0, ps.Count, "Empty");
			Assert.IsTrue (ps.IsUnrestricted (), "State-Unrestricted");

			ZoneIdentityPermission zip = new ZoneIdentityPermission (SecurityZone.MyComputer);
			ZoneIdentityPermission zipr = (ZoneIdentityPermission)ps.SetPermission (zip);
			Assert.AreEqual (1, ps.Count, "ZoneIdentityPermission");
			Assert.AreEqual (SecurityZone.MyComputer, zipr.SecurityZone, "SecurityZone");
#if NET_2_0
			// Adding a non unrestricted identity permission now results in 
			// a permission set loosing it's unrestricted status
			Assert.IsTrue (!ps.IsUnrestricted (), "State-Unrestricted-2");
#else
			Assert.IsTrue (ps.IsUnrestricted (), "State-Unrestricted-2");
#endif
			zip = new ZoneIdentityPermission (SecurityZone.Intranet);
			zipr = (ZoneIdentityPermission)ps.SetPermission (zip);
			Assert.AreEqual (1, ps.Count, "ZoneIdentityPermission-2");
			Assert.AreEqual (SecurityZone.Intranet, zipr.SecurityZone, "SecurityZone-2");

			SecurityPermission result = (SecurityPermission)ps.SetPermission (sp);
			Assert.AreEqual (2, ps.Count, "SecurityPermission");
			Assert.AreEqual (SecurityPermissionFlag.AllFlags, result.Flags, "Flags");
			Assert.IsTrue (!ps.IsUnrestricted (), "State-None");

			sp = new SecurityPermission (SecurityPermissionFlag.ControlAppDomain);
			result = (SecurityPermission)ps.SetPermission (sp);
			Assert.AreEqual (2, ps.Count, "SecurityPermission-2");
			Assert.AreEqual (SecurityPermissionFlag.ControlAppDomain, result.Flags, "Flags-2");
		}

		[Test]
		public void ToXmlNone () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			SecurityElement se = ps.ToXml ();
			Assert.IsTrue (ps.ToString().StartsWith ("<PermissionSet"), "None.ToString().StartsWith");
			Assert.AreEqual ("System.Security.PermissionSet", (se.Attributes ["class"] as string), "None.class");
			Assert.AreEqual ("1", (se.Attributes ["version"] as string), "None.version");
			Assert.IsNull ((se.Attributes ["Unrestricted"] as string), "None.Unrestricted");
		}

		[Test]
		public void ToXmlUnrestricted () 
		{
			PermissionSet ps = new PermissionSet (PermissionState.Unrestricted);
			SecurityElement se = ps.ToXml ();
			Assert.IsTrue (ps.ToString().StartsWith ("<PermissionSet"), "Unrestricted.ToString().StartsWith");
			Assert.AreEqual ("System.Security.PermissionSet", (se.Attributes ["class"] as string), "Unrestricted.class");
			Assert.AreEqual ("1", (se.Attributes ["version"] as string), "Unrestricted.version");
			Assert.AreEqual ("true", (se.Attributes ["Unrestricted"] as string), "Unrestricted.Unrestricted");
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
#if NET_2_0
			// Identity permissions aren't added to unrestricted permission sets in 2.0
			Compare ("PS1 U Unrestricted", ps1.Union (ups1), true, 0);
			Compare ("Unrestricted U PS1", ups1.Union (ps1), true, 0);
			PermissionSet ups2 = new PermissionSet (PermissionState.Unrestricted);
			Compare ("UPS1 U UPS2", ups1.Union (ups1), true, 0);
			Compare ("UPS2 U UPS1", ups2.Union (ups1), true, 0);
			ups2.AddPermission (zip);
			Compare ("UPS1 U UPS2+ZIP", ups1.Union (ups2), true, 0);
			Compare ("UPS2+ZIP U UPS1", ups2.Union (ups1), true, 0);
#else
			Compare ("PS1 U Unrestricted", ps1.Union (ups1), true, 1);
			Compare ("Unrestricted U PS1", ups1.Union (ps1), true, 1);
			PermissionSet ups2 = new PermissionSet (PermissionState.Unrestricted);
			Compare ("UPS1 U UPS2", ups1.Union (ups1), true, 1);
			Compare ("UPS2 U UPS1", ups2.Union (ups1), true, 1);
			ups2.AddPermission (zip);
			Compare ("UPS1 U UPS2+ZIP", ups1.Union (ups2), true, 1);
			Compare ("UPS2+ZIP U UPS1", ups2.Union (ups1), true, 1);
#endif
		}
#if NET_2_0
		[Test]
		[Category ("NotWorking")] // requires imperative stack modifiers
		[ExpectedException (typeof (ExecutionEngineException))]
		public void RevertAssert_WithoutAssertion ()
		{
			PermissionSet.RevertAssert ();
		}

		[Test]
		[Category ("NotWorking")] // requires imperative stack modifiers
		public void RevertAssert_WithAssertion ()
		{
			PermissionSet ups = new PermissionSet (PermissionState.Unrestricted);
			ups.Assert ();
			PermissionSet.RevertAssert ();
		}
#endif
		[Test]
		public void Assert_NonCasPermission ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new PrincipalPermission (PermissionState.None));
			Assert.IsTrue (ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
			Assert.AreEqual (1, ps.Count, "Count");
			ps.Assert ();
			// it's simply ignored
		}

		[Test]
		public void Deny_NonCasPermission ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new PrincipalPermission (PermissionState.None));
			Assert.IsTrue (ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
			Assert.AreEqual (1, ps.Count, "Count");
			ps.Deny ();
			// it's simply ignored
		}

		[Test]
		public void PermitOnly_NonCasPermission ()
		{
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (new PrincipalPermission (PermissionState.None));
			Assert.IsTrue (ps.ContainsNonCodeAccessPermissions (), "ContainsNonCodeAccessPermissions");
			Assert.AreEqual (1, ps.Count, "Count");
			ps.PermitOnly ();
			// it's simply ignored
		}
#if !MOBILE
		// note: this only ensure that the ECMA key support unification (more test required, outside corlib, for other keys, like MS final).
		private const string PermissionPattern = "<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\"><IPermission class=\"System.Security.Permissions.FileDialogPermission, mscorlib, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Access=\"Open\"/></PermissionSet>";
		private const string fx10version = "1.0.3300.0";
		private const string fx11version = "1.0.5000.0";
		private const string fx20version = "2.0.0.0";

		private void Unification (string xml)
		{
			PermissionSetAttribute psa = new PermissionSetAttribute (SecurityAction.Assert);
			psa.XML = xml;
			string pset = psa.CreatePermissionSet ().ToString ();
			string currentVersion = typeof (string).Assembly.GetName ().Version.ToString ();
			Assert.IsTrue (pset.IndexOf (currentVersion) > 0, currentVersion);
		}

		[Test]
		public void Unification_FromFx10 ()
		{
			Unification (String.Format (PermissionPattern, fx10version));
		}

		[Test]
		public void Unification_FromFx11 ()
		{
			Unification (String.Format (PermissionPattern, fx11version));
		}

		[Test]
		public void Unification_FromFx20 ()
		{
			Unification (String.Format (PermissionPattern, fx20version));
		}

		[Test]
		public void Unification_FromFx99 ()
		{
			Unification (String.Format (PermissionPattern, "9.99.999.9999"));
		}
#endif
	}
}

#endif
