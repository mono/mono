//
// AlgorithmIdentifierTest.cs - NUnit tests for AlgorithmIdentifier
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using NUnit.Framework;

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class AlgorithmIdentifierTest : Assertion {

		static string defaultOid = "1.2.840.113549.3.7";
		static string defaultName = "3des";
		static string validOid = "1.2.840.113549.1.1.1";

		[Test]
		public void ConstructorEmpty () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			AssertEquals ("KeyLength", 0, ai.KeyLength);
			AssertEquals ("Oid.FriendlyName", defaultName, ai.Oid.FriendlyName);
			AssertEquals ("Oid.Value", defaultOid, ai.Oid.Value);
			AssertEquals ("Parameters", 0, ai.Parameters.Length);
		}

		[Test]
		public void ConstructorOid () 
		{
			Oid o = new Oid (validOid);
			AlgorithmIdentifier ai = new AlgorithmIdentifier (o);
			AssertEquals ("KeyLength", 0, ai.KeyLength);
			AssertEquals ("Oid", validOid, ai.Oid.Value);
			AssertEquals ("Parameters", 0, ai.Parameters.Length);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNull () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier (null);
		}

		[Test]
		public void ConstructorOidKeyLength ()
		{
			Oid o = new Oid (validOid);
			AlgorithmIdentifier ai = new AlgorithmIdentifier (o, 128);
			AssertEquals ("KeyLength", 128, ai.KeyLength);
			AssertEquals ("Oid", validOid, ai.Oid.Value);
			AssertEquals ("Parameters", 0, ai.Parameters.Length);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNullKeyLength () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier (null, 128);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorOidKeyLengthNegative () 
		{
			Oid o = new Oid (validOid);
			AlgorithmIdentifier ai = new AlgorithmIdentifier (o, -1);
		}

		[Test]
		public void KeyLength () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			ai.KeyLength = Int32.MaxValue;
			AssertEquals ("KeyLength-Max", Int32.MaxValue, ai.KeyLength);
			ai.KeyLength = 0;
			AssertEquals ("KeyLength-Zero", 0, ai.KeyLength);
			ai.KeyLength = Int32.MinValue;
			AssertEquals ("KeyLength-Min", Int32.MinValue, ai.KeyLength);
		}

		[Test]
		public void Oid () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			ai.Oid = new Oid (validOid);
			AssertEquals ("Oid", validOid, ai.Oid.Value);
			ai.Oid = null;
			AssertNull ("Oid", ai.Oid);
		}

		[Test]
		public void Parameters () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			ai.Parameters = new byte[2] { 0x05, 0x00 }; // ASN.1 NULL
			AssertEquals ("Oid", "05-00", BitConverter.ToString (ai.Parameters));
			ai.Parameters = null;
			AssertNull ("Parameters", ai.Parameters);
		}
	}
}

#endif
