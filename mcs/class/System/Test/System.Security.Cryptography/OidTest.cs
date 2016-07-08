//
// OidTest.cs - NUnit tests for Oid
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
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class OidTest {

		static string invalidOid = "1.0";
		static string invalidName = "friendlyName";
		static string validOid = "1.2.840.113549.1.1.1";
		static string validName = "RSA";

		[Test]
		public void ConstructorEmpty () 
		{
			Oid o = new Oid ();
			Assert.IsNull (o.FriendlyName, "FriendlyName");
			Assert.IsNull (o.Value, "Value");
		}

		[Test]
		public void ConstructorValidString () 
		{
			Oid o = new Oid (validOid);
			Assert.AreEqual (validName, o.FriendlyName, "FriendlyName");
			Assert.AreEqual (validOid, o.Value, "Value");
		}

		[Test]
		public void ConstructorInvalidString ()
		{
			Oid o = new Oid (invalidOid);
			Assert.IsNull (o.FriendlyName, "FriendlyName");
			Assert.AreEqual (invalidOid, o.Value, "Value");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullString ()
		{
			string oid = null; // do not confuse compiler
			Oid o = new Oid (oid);
		}

		[Test]
		public void ConstructorStringString ()
		{
			Oid o = new Oid (validOid, invalidName);
			Assert.AreEqual (invalidName, o.FriendlyName, "FriendlyName");
			Assert.AreEqual (validOid, o.Value, "Value");
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorStringNullString () 
		{
			Oid o = new Oid (null, validName);
			Assert.AreEqual (validName, o.FriendlyName, "FriendlyName");
			Assert.IsNull (o.Value, "Value");
		}

		[Test]
		public void ConstructorStringStringNull () 
		{
			Oid o = new Oid (validOid, null);
			Assert.AreEqual ("RSA", o.FriendlyName, "FriendlyName");
			Assert.AreEqual (validOid, o.Value, "Value");
		}

		[Test]
		public void ConstructorOid ()
		{
			Oid o = new Oid (validOid, invalidName);
			Oid o2 = new Oid (o);
			Assert.AreEqual (invalidName, o.FriendlyName, "FriendlyName==invalid");
			Assert.AreEqual (o.FriendlyName, o2.FriendlyName, "FriendlyName");
			Assert.AreEqual (o.Value, o2.Value, "Value");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNull () 
		{
			Oid onull = null; // do not confuse compiler
			Oid o = new Oid (onull);
		}

		[Test]
		public void FriendlyName () 
		{
			Oid o = new Oid (invalidOid, invalidName);
			Assert.AreEqual (invalidName, o.FriendlyName, "FriendlyName-1");
			Assert.AreEqual (invalidOid, o.Value, "Value-1");
			o.FriendlyName = validName;
			Assert.AreEqual (validName, o.FriendlyName, "FriendlyName-2");
			Assert.AreEqual (validOid, o.Value, "Value-2"); // surprise!
		}

		[Test]
		public void FriendlyNameNull ()
		{
			Oid o = new Oid (validOid, invalidName);
			Assert.AreEqual (invalidName, o.FriendlyName, "FriendlyName");
			o.FriendlyName = null;
			Assert.AreEqual ("RSA", o.FriendlyName, "FriendlyName-Null");
		}

		[Test]
		public void Value () 
		{
			Oid o = new Oid (validOid, invalidName);
			Assert.AreEqual (validOid, o.Value, "Value-1");
			o.Value = invalidName;
			Assert.AreEqual (invalidName, o.Value, "Value-2");
		}

		[Test]
		public void ValueNull () 
		{
			Oid o = new Oid (validOid, invalidName);
			Assert.AreEqual (validOid, o.Value, "Value");
			o.Value = null;
			Assert.IsNull (o.Value, "Value-Null");
		}

		[Test]
		public void WellKnownOid () 
		{
			Oid o = new Oid ("1.2.840.113549.1.1.1");
			Assert.AreEqual ("1.2.840.113549.1.1.1", o.Value, "RSA Value");
			Assert.AreEqual ("RSA", o.FriendlyName, "RSA FriendlyName");
			o = new Oid ();
			o.FriendlyName = "RSA";
			Assert.AreEqual (o.Value, "1.2.840.113549.1.1.1", "RSA Value from FriendlyName");

			o = new Oid ("1.2.840.113549.1.7.1");
			Assert.AreEqual ("1.2.840.113549.1.7.1", o.Value, "PKCS 7 Data Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("PKCS 7 Data", o.FriendlyName, "PKCS 7 Data FriendlyName");

			o = new Oid ("1.2.840.113549.1.9.5");
			Assert.AreEqual ("1.2.840.113549.1.9.5", o.Value, "Signing Time Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual ("Signing Time", o.FriendlyName, "Signing Time FriendlyName");

			o = new Oid ("1.2.840.113549.3.7");
			Assert.AreEqual ("1.2.840.113549.3.7", o.Value, "3des Value");
			Assert.AreEqual ("3des", o.FriendlyName, "3des FriendlyName");
			o = new Oid ();
			o.FriendlyName = "3des";
			Assert.AreEqual (o.Value, "1.2.840.113549.3.7", "3des Value from FriendlyName");

			o = new Oid ("2.16.840.1.101.3.4.1.2");
			Assert.AreEqual ("2.16.840.1.101.3.4.1.2", o.Value, "aes128 Value");
			Assert.AreEqual ("aes128", o.FriendlyName, "aes128 FriendlyName");
			o = new Oid ();
			o.FriendlyName = "aes128";
			Assert.AreEqual (o.Value, "2.16.840.1.101.3.4.1.2", "aes123 Value from FriendlyName");

			o = new Oid ("2.16.840.1.101.3.4.1.42");
			Assert.AreEqual ("2.16.840.1.101.3.4.1.42", o.Value, "aes256 Value");
			Assert.AreEqual ("aes256", o.FriendlyName, "aes256 FriendlyName");
			o = new Oid ();
			o.FriendlyName = "aes256";
			Assert.AreEqual (o.Value, "2.16.840.1.101.3.4.1.42", "aes256 Value from FriendlyName");

			o = new Oid ("2.16.840.1.101.3.4.2.1");
			Assert.AreEqual ("2.16.840.1.101.3.4.2.1", o.Value, "sha256 Value");
			Assert.AreEqual ("sha256", o.FriendlyName, "sha256 FriendlyName");
			o = new Oid ();
			o.FriendlyName = "sha256";
			Assert.AreEqual (o.Value, "2.16.840.1.101.3.4.2.1", "sha256 Value from FriendlyName");

			o = new Oid ("2.16.840.1.101.3.4.2.3");
			Assert.AreEqual ("2.16.840.1.101.3.4.2.3", o.Value, "sha512 Value");
			Assert.AreEqual ("sha512", o.FriendlyName, "sha512 FriendlyName");
			o = new Oid ();
			o.FriendlyName = "sha512";
			Assert.AreEqual (o.Value, "2.16.840.1.101.3.4.2.3", "sha512 Value from FriendlyName");

			o = new Oid ("2.16.840.1.101.3.4.2.2");
			Assert.AreEqual ("2.16.840.1.101.3.4.2.2", o.Value, "sha384 Value");
			Assert.AreEqual ("sha384", o.FriendlyName, "sha384 FriendlyName");

			o = new Oid ("1.2.840.113549.1.1.12");
			Assert.AreEqual ("1.2.840.113549.1.1.12", o.Value, "sha384RSA Value");
			Assert.AreEqual ("sha384RSA", o.FriendlyName, "sha384RSA FriendlyName");

			// TODO: add other well known oid as we find them
		}
	}
}
