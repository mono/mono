//
// OidTest.cs - NUnit tests for Oid
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

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class OidTest : Assertion {

		static string invalidOid = "1.0";
		static string invalidName = "friendlyName";
		static string validOid = "1.2.840.113549.1.1.1";
		static string validName = "RSA";

		[Test]
		public void ConstructorEmpty () 
		{
			Oid o = new Oid ();
			AssertNull ("FriendlyName", o.FriendlyName);
			AssertNull ("Value", o.Value);
		}

		[Test]
		public void ConstructorValidValue () 
		{
			Oid o = new Oid (validOid);
			AssertEquals ("FriendlyName", validName, o.FriendlyName);
			AssertEquals ("Value", validOid, o.Value);
		}

		[Test]
		public void ConstructorInvalidValue () 
		{
			Oid o = new Oid (invalidOid);
			AssertNull ("FriendlyName", o.FriendlyName);
			AssertEquals ("Value", invalidOid, o.Value);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorValueNull () 
		{
			string oid = null; // do not confuse compiler
			Oid o = new Oid (oid);
		}

		[Test]
		public void ConstructorValueName ()
		{
			Oid o = new Oid (validOid, invalidName);
			AssertEquals ("FriendlyName", invalidName, o.FriendlyName);
			AssertEquals ("Value", validOid, o.Value);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorValueNullName () 
		{
			Oid o = new Oid (null, validName);
			AssertEquals ("FriendlyName", validName, o.FriendlyName);
			AssertNull ("Value", o.Value);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorValueNameNull () 
		{
			Oid o = new Oid (validOid, null);
			AssertNull ("FriendlyName", o.FriendlyName);
			AssertEquals ("Value", validOid, o.Value);
		}

		[Test]
		public void ConstructorOid ()
		{
			Oid o = new Oid (validOid, invalidName);
			Oid o2 = new Oid (o);
			AssertEquals ("FriendlyName==invalid", invalidName, o.FriendlyName);
			AssertEquals ("FriendlyName", o.FriendlyName, o2.FriendlyName);
			AssertEquals ("Value", o.Value, o2.Value);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorOidNull () 
		{
			Oid onull = null; // do not confuse compiler
			Oid o = new Oid (onull);
		}

		[Test]
		public void FriendlyName () 
		{
			Oid o = new Oid (invalidOid, invalidName);
			AssertEquals ("FriendlyName", invalidName, o.FriendlyName);
			AssertEquals ("Value", invalidOid, o.Value);
			o.FriendlyName = validName;
			AssertEquals ("FriendlyName", validName, o.FriendlyName);
			AssertEquals ("Value", validOid, o.Value); // surprise!
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FriendlyNameNull ()
		{
			Oid o = new Oid (validOid, invalidName);
			AssertEquals ("FriendlyName", invalidName, o.FriendlyName);
			o.FriendlyName = null;
		}

		[Test]
		public void Value () 
		{
			Oid o = new Oid (validOid, invalidName);
			AssertEquals ("Value", validOid, o.Value);
			o.Value = invalidName;
			AssertEquals ("Value", invalidName, o.Value);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ValueNull () 
		{
			Oid o = new Oid (validOid, invalidName);
			AssertEquals ("Value", validOid, o.Value);
			o.Value = null;
			AssertNull ("Value==null", o.Value);
		}
	}
}

#endif
