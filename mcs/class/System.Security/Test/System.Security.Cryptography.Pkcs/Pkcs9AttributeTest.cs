//
// Pkcs9AttributeTest.cs - NUnit tests for Pkcs9Attribute
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class Pkcs9AttributeTest : Assertion {

		static string defaultOid = "1.2.840.113549.1.7.1";
		static string defaultName = "PKCS 7 Data";

		[Test]
		public void ConstructorOid () 
		{
			Oid o = new Oid (defaultOid);
			Pkcs9Attribute a = new Pkcs9Attribute (o);
			AssertEquals ("Oid.FriendlyName", defaultName, a.Oid.FriendlyName);
			AssertEquals ("Oid.Value", defaultOid, a.Oid.Value);
			AssertEquals ("Values", 0, a.Values.Count);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNull () 
		{
			Pkcs9Attribute a = new Pkcs9Attribute (null);
		}

		[Test]
		public void ConstructorOidArrayList () 
		{
			Oid o = new Oid (defaultOid);
			ArrayList al = new ArrayList ();
			Pkcs9Attribute a = new Pkcs9Attribute (o, al);
			AssertEquals ("Oid.FriendlyName", defaultName, a.Oid.FriendlyName);
			AssertEquals ("Oid.Value", defaultOid, a.Oid.Value);
			AssertEquals ("Values", 0, a.Values.Count);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNullArrayList () 
		{
			ArrayList al = new ArrayList ();
			Pkcs9Attribute a = new Pkcs9Attribute (null, al);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorOidArrayListNull () 
		{
			Oid o = new Oid (defaultOid);
			ArrayList al = null; // do not confuse compiler
			Pkcs9Attribute a = new Pkcs9Attribute (o, al);
		}

		[Test]
		public void ConstructorOidObject () 
		{
			Oid o = new Oid (defaultOid);
			Pkcs9Attribute a = new Pkcs9Attribute (o, o);
			AssertEquals ("Oid.FriendlyName", defaultName, a.Oid.FriendlyName);
			AssertEquals ("Oid.Value", defaultOid, a.Oid.Value);
			AssertEquals ("Values", 1, a.Values.Count);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNullObject () 
		{
			Oid o = new Oid (defaultOid);
			Pkcs9Attribute a = new Pkcs9Attribute (null, o);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidObjectNull () 
		{
			Oid o = new Oid (defaultOid);
			object obj = null; // do not confuse compiler
			Pkcs9Attribute a = new Pkcs9Attribute (o, obj);
		}
	}
}

#endif
