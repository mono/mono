//
// PermissionSetAttributeTest.cs - NUnit Test Cases for PermissionSetAttribute
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

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
#if MOBILE
	[Ignore]
#endif
	public class PermissionSetAttributeTest {

		[Test]
		public void Default () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
			Assert.IsFalse (a.UnicodeEncoded, "UnicodeEncoded");
			Assert.AreEqual (a.ToString (), a.TypeId.ToString (), "TypeId");
			Assert.IsFalse (a.Unrestricted, "Unrestricted");

			IPermission perm = a.CreatePermission ();
			Assert.IsNull (perm, "CreatePermission");

			PermissionSet ps = a.CreatePermissionSet ();
			Assert.AreEqual (0, ps.Count, "CreatePermissionSet");
		}

		[Test]
		public void Action ()
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			Assert.AreEqual (SecurityAction.Assert, a.Action, "Action=Assert");
			a.Action = SecurityAction.Demand;
			Assert.AreEqual (SecurityAction.Demand, a.Action, "Action=Demand");
			a.Action = SecurityAction.Deny;
			Assert.AreEqual (SecurityAction.Deny, a.Action, "Action=Deny");
			a.Action = SecurityAction.InheritanceDemand;
			Assert.AreEqual (SecurityAction.InheritanceDemand, a.Action, "Action=InheritanceDemand");
			a.Action = SecurityAction.LinkDemand;
			Assert.AreEqual (SecurityAction.LinkDemand, a.Action, "Action=LinkDemand");
			a.Action = SecurityAction.PermitOnly;
			Assert.AreEqual (SecurityAction.PermitOnly, a.Action, "Action=PermitOnly");
			a.Action = SecurityAction.RequestMinimum;
			Assert.AreEqual (SecurityAction.RequestMinimum, a.Action, "Action=RequestMinimum");
			a.Action = SecurityAction.RequestOptional;
			Assert.AreEqual (SecurityAction.RequestOptional, a.Action, "Action=RequestOptional");
			a.Action = SecurityAction.RequestRefuse;
			Assert.AreEqual (SecurityAction.RequestRefuse, a.Action, "Action=RequestRefuse");
		}

		[Test]
		public void Action_Invalid ()
		{
			PermissionSetAttribute a = new PermissionSetAttribute ((SecurityAction)Int32.MinValue);
			// no validation in attribute
		}

		[Test]
		public void File () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.File = "mono";
			Assert.AreEqual ("mono", a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
			a.File = null;
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
		}

#if NET_2_0
		[Test]
		public void Hex ()
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			Assert.IsNull (a.Hex, "Hex-1");
			a.Hex = String.Empty;
			Assert.AreEqual (String.Empty, a.Hex, "Hex-2");
			a.Hex = null;
			Assert.IsNull (a.Hex, "Hex-3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Hex_Bad ()
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.Hex = "g";
			Assert.AreEqual ("g", a.Hex, "Bad Hex");
			a.CreatePermissionSet ();
		}

		[Test]
		public void Hex_ASCII_Permission ()
		{
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (sp);

			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.Hex = "3C5065726D697373696F6E53657420636C6173733D2253797374656D2E53656375726974792E5065726D697373696F6E536574220D0A76657273696F6E3D2231223E0D0A3C495065726D697373696F6E20636C6173733D2253797374656D2E53656375726974792E5065726D697373696F6E732E53656375726974795065726D697373696F6E2C206D73636F726C69622C2056657273696F6E3D322E302E333630302E302C2043756C747572653D6E65757472616C2C205075626C69634B6579546F6B656E3D62373761356335363139333465303839220D0A76657273696F6E3D2231220D0A466C6167733D22417373657274696F6E222F3E0D0A3C2F5065726D697373696F6E5365743E0D0A";
			PermissionSet psa = a.CreatePermissionSet ();
			Assert.IsTrue (ps.Equals (psa), "HEX-ASCII");
		}

		[Test]
		[ExpectedException (typeof (XmlSyntaxException))]
		public void Hex_Unicode_Permission ()
		{
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (sp);

			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.UnicodeEncoded = true;

			a.Hex = "3C005000650072006D0069007300730069006F006E00530065007400200063006C006100730073003D002200530079007300740065006D002E00530065006300750072006900740079002E005000650072006D0069007300730069006F006E0053006500740022000D000A00760065007200730069006F006E003D002200310022003E000D000A003C0049005000650072006D0069007300730069006F006E00200063006C006100730073003D002200530079007300740065006D002E00530065006300750072006900740079002E005000650072006D0069007300730069006F006E0073002E00530065006300750072006900740079005000650072006D0069007300730069006F006E002C0020006D00730063006F0072006C00690062002C002000560065007200730069006F006E003D0032002E0030002E0033003600300030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D00620037003700610035006300350036003100390033003400650030003800390022000D000A00760065007200730069006F006E003D002200310022000D000A0046006C006100670073003D00220041007300730065007200740069006F006E0022002F003E000D000A003C002F005000650072006D0069007300730069006F006E005300650074003E000D000A00";
			PermissionSet psu = a.CreatePermissionSet();
			Assert.IsTrue(ps.Equals(psu), "HEX-UNICODE");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Hex_BigEndianUnicode_Permission ()
		{
			SecurityPermission sp = new SecurityPermission (SecurityPermissionFlag.Assertion);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			ps.AddPermission (sp);

			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.UnicodeEncoded = true;
			a.Hex = "003C005000650072006D0069007300730069006F006E00530065007400200063006C006100730073003D002200530079007300740065006D002E00530065006300750072006900740079002E005000650072006D0069007300730069006F006E0053006500740022000D000A00760065007200730069006F006E003D002200310022003E000D000A003C0049005000650072006D0069007300730069006F006E00200063006C006100730073003D002200530079007300740065006D002E00530065006300750072006900740079002E005000650072006D0069007300730069006F006E0073002E00530065006300750072006900740079005000650072006D0069007300730069006F006E002C0020006D00730063006F0072006C00690062002C002000560065007200730069006F006E003D0032002E0030002E0033003600300030002E0030002C002000430075006C0074007500720065003D006E00650075007400720061006C002C0020005000750062006C00690063004B006500790054006F006B0065006E003D00620037003700610035006300350036003100390033003400650030003800390022000D000A00760065007200730069006F006E003D002200310022000D000A0046006C006100670073003D00220041007300730065007200740069006F006E0022002F003E000D000A003C002F005000650072006D0069007300730069006F006E005300650074003E000D000";
			PermissionSet psbeu = a.CreatePermissionSet ();
			Assert.IsTrue (ps.Equals (psbeu), "HEX-BIGENDIAN-UNICODE");
		}
#endif

		[Test]
		public void Name () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.Name = "mono";
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.AreEqual ("mono", a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
			a.Name = null;
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
		}

		[Test]
		public void XML () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.XML = "mono";
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.AreEqual ("mono", a.XML, "XML");
			a.XML = null;
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");
		}

		[Test]
		public void UnicodeEncoded () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.UnicodeEncoded = true;
			Assert.IsTrue (a.UnicodeEncoded, "UnicodeEncoded-true");
			a.UnicodeEncoded = false;
			Assert.IsFalse (a.UnicodeEncoded, "UnicodeEncoded-false");
		}

		[Test]
		public void Unrestricted () 
		{
			PermissionSetAttribute a = new PermissionSetAttribute (SecurityAction.Assert);
			a.Unrestricted = true;
			Assert.IsNull (a.File, "File");
#if NET_2_0
			Assert.IsNull (a.Hex, "Hex");
#endif
			Assert.IsNull (a.Name, "Name");
			Assert.IsNull (a.XML, "XML");

			PermissionSet ps = a.CreatePermissionSet ();
			Assert.IsTrue (ps.IsUnrestricted (), "CreatePermissionSet.IsUnrestricted");
		}

		[Test]
		public void Attributes ()
		{
			Type t = typeof (PermissionSetAttribute);
			Assert.IsTrue (t.IsSerializable, "IsSerializable");

			object [] attrs = t.GetCustomAttributes (typeof (AttributeUsageAttribute), false);
			Assert.AreEqual (1, attrs.Length, "AttributeUsage");
			AttributeUsageAttribute aua = (AttributeUsageAttribute)attrs [0];
			Assert.IsTrue (aua.AllowMultiple, "AllowMultiple");
			Assert.IsFalse (aua.Inherited, "Inherited");
			AttributeTargets at = (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method);
			Assert.AreEqual (at, aua.ValidOn, "ValidOn");
		}
	}
}
