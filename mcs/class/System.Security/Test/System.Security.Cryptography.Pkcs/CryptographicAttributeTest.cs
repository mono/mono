//
// CryptographicAttributeTest.cs - NUnit tests for CryptographicAttribute
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_2_0

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class CryptographicAttributeTest {

		static string defaultOid = "1.2.840.113549.1.7.1";
		static string defaultName = "PKCS 7 Data";

		[Test]
		public void ConstructorOid () 
		{
			Oid o = new Oid (defaultOid);
			CryptographicAttribute ca = new CryptographicAttribute (o);
			Assert.AreEqual (defaultName, ca.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (defaultOid, ca.Oid.Value, "Oid.Value");
			Assert.AreEqual (0, ca.Values.Count, "Values");
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNull () 
		{
			CryptographicAttribute ca = new CryptographicAttribute (null);
		}

		[Test]
		public void ConstructorOidArrayList () 
		{
			Oid o = new Oid (defaultOid);
			ArrayList al = new ArrayList ();
			CryptographicAttribute ca = new CryptographicAttribute (o, al);
			Assert.AreEqual (defaultName, ca.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (defaultOid, ca.Oid.Value, "Oid.Value");
			Assert.AreEqual (0, ca.Values.Count, "Values");
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNullArrayList () 
		{
			ArrayList al = new ArrayList ();
			CryptographicAttribute ca = new CryptographicAttribute (null, al);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorOidArrayListNull () 
		{
			Oid o = new Oid (defaultOid);
			ArrayList al = null; // do not confuse compiler
			CryptographicAttribute ca = new CryptographicAttribute (o, al);
		}

		[Test]
		public void ConstructorOidObject () 
		{
			Oid o = new Oid (defaultOid);
			CryptographicAttribute ca = new CryptographicAttribute (o, o);
			Assert.AreEqual (defaultName, ca.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (defaultOid, ca.Oid.Value, "Oid.Value");
			Assert.AreEqual (1, ca.Values.Count, "Values");
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNullObject () 
		{
			Oid o = new Oid (defaultOid);
			CryptographicAttribute ca = new CryptographicAttribute (null, o);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidObjectNull () 
		{
			Oid o = new Oid (defaultOid);
			object obj = null; // do not confuse compiler
			CryptographicAttribute ca = new CryptographicAttribute (o, obj);
		}
	}
}

#endif
