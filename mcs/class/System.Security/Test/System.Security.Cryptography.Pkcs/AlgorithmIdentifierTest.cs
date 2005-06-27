//
// AlgorithmIdentifierTest.cs - NUnit tests for AlgorithmIdentifier
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

#if NET_2_0

using NUnit.Framework;

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class AlgorithmIdentifierTest {

		static string defaultOid = "1.2.840.113549.3.7";
		static string defaultName = "3des";
		static string validOid = "1.2.840.113549.1.1.1";

		[Test]
		public void ConstructorEmpty () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			Assert.AreEqual (0, ai.KeyLength, "KeyLength");
			Assert.AreEqual (defaultName, ai.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (defaultOid, ai.Oid.Value, "Oid.Value");
			Assert.AreEqual (0, ai.Parameters.Length, "Parameters");
		}

		[Test]
		public void ConstructorOid () 
		{
			Oid o = new Oid (validOid);
			AlgorithmIdentifier ai = new AlgorithmIdentifier (o);
			Assert.AreEqual (0, ai.KeyLength, "KeyLength");
			Assert.AreEqual (validOid, ai.Oid.Value, "Oid");
			Assert.AreEqual (0, ai.Parameters.Length, "Parameters");
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNull () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier (null);
			Assert.IsNull (ai.Oid, "Oid");
			Assert.AreEqual (0, ai.KeyLength, "KeyLength");
			Assert.AreEqual (0, ai.Parameters.Length, "Parameters");
		}

		[Test]
		public void ConstructorOidKeyLength ()
		{
			Oid o = new Oid (validOid);
			AlgorithmIdentifier ai = new AlgorithmIdentifier (o, 128);
			Assert.AreEqual (128, ai.KeyLength, "KeyLength");
			Assert.AreEqual (validOid, ai.Oid.Value, "Oid");
			Assert.AreEqual (0, ai.Parameters.Length, "Parameters");
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorOidNullKeyLength () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier (null, 128);
			Assert.IsNull (ai.Oid, "Oid");
			Assert.AreEqual (128, ai.KeyLength, "KeyLength");
			Assert.AreEqual (0, ai.Parameters.Length, "Parameters");
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ConstructorOidKeyLengthNegative () 
		{
			Oid o = new Oid (validOid);
			AlgorithmIdentifier ai = new AlgorithmIdentifier (o, -1);
			Assert.AreEqual (-1, ai.KeyLength, "KeyLength");
			Assert.AreEqual (validOid, ai.Oid.Value, "Oid");
			Assert.AreEqual (0, ai.Parameters.Length, "Parameters");
		}

		[Test]
		public void KeyLength () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			ai.KeyLength = Int32.MaxValue;
			Assert.AreEqual (Int32.MaxValue, ai.KeyLength, "KeyLength-Max");
			ai.KeyLength = 0;
			Assert.AreEqual (0, ai.KeyLength, "KeyLength-Zero");
			ai.KeyLength = Int32.MinValue;
			Assert.AreEqual (Int32.MinValue, ai.KeyLength, "KeyLength-Min");
		}

		[Test]
		public void Oid () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			ai.Oid = new Oid (validOid);
			Assert.AreEqual (validOid, ai.Oid.Value, "Oid");
			ai.Oid = null;
			Assert.IsNull (ai.Oid, "Oid-Null");
		}

		[Test]
		public void Parameters () 
		{
			AlgorithmIdentifier ai = new AlgorithmIdentifier ();
			ai.Parameters = new byte[2] { 0x05, 0x00 }; // ASN.1 NULL
			Assert.AreEqual ("05-00", BitConverter.ToString (ai.Parameters), "Parameters");
			ai.Parameters = null;
			Assert.IsNull (ai.Parameters, "Parameters-Null");
		}
	}
}

#endif
