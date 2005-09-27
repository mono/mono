//
// Pkcs9AttributeTest.cs - NUnit tests for Pkcs9Attribute
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class Pkcs9AttributeObjectTest {

		static string defaultOid = "1.2.840.113549.1.7.1";
		static string defaultName = "PKCS 7 Data";

		[Test]
		public void ConstructorEmpty () 
		{
			Pkcs9AttributeObject a = new Pkcs9AttributeObject ();
			Assert.IsNull (a.Oid, "Oid");
			Assert.IsNull (a.RawData, "RawData");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorAsnEncodedDataNull () 
		{
			Pkcs9AttributeObject a = new Pkcs9AttributeObject (null);
		}

		[Test]
		public void ConstructorOidArray () 
		{
			Oid o = new Oid (defaultOid);
			Pkcs9AttributeObject a = new Pkcs9AttributeObject (o, new byte[0]);
			Assert.AreEqual (defaultName, a.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (defaultOid, a.Oid.Value, "Oid.Value");
			Assert.AreEqual (0, a.RawData.Length, "RawData");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNullArray ()
		{
			Oid o = null;
			Pkcs9AttributeObject a = new Pkcs9AttributeObject (o, new byte[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidArrayNull ()
		{
			Oid o = new Oid (defaultOid);
			Pkcs9AttributeObject a = new Pkcs9AttributeObject (o, null);
		}

		[Test]
		public void ConstructorStringArray ()
		{
			Pkcs9AttributeObject a = new Pkcs9AttributeObject (defaultOid, new byte[0]);
			Assert.AreEqual (defaultName, a.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (defaultOid, a.Oid.Value, "Oid.Value");
			Assert.AreEqual (0, a.RawData.Length, "RawData");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorStringNullArray ()
		{
			string s = null;
			Pkcs9AttributeObject a = new Pkcs9AttributeObject (s, new byte[0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorStringArrayNull ()
		{
			Pkcs9AttributeObject a = new Pkcs9AttributeObject (defaultOid, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			new Pkcs9AttributeObject ().CopyFrom (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyFrom_SigningTime_Raw ()
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (DateTime.UtcNow);
			Pkcs9AttributeObject a = new Pkcs9AttributeObject ();
			a.CopyFrom (new AsnEncodedData (st.RawData));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyFrom_SigningTime_OidRaw ()
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (DateTime.UtcNow);
			Pkcs9AttributeObject a = new Pkcs9AttributeObject ();
			a.CopyFrom (new AsnEncodedData (st.Oid, st.RawData));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyFrom_Self ()
		{
			Pkcs9AttributeObject a = new Pkcs9AttributeObject ("1.2.3.4", new byte[2] { 0x05, 0x00 } );
			a.CopyFrom (new AsnEncodedData (a.Oid, a.RawData));
		}
	}
}

#endif
