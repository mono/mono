//
// Pkcs9DocumentNameTest.cs - NUnit tests for Pkcs9DocumentName
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
	public class Pkcs9DocumentNameTest {

		[Test]
		public void Constructor_Empty ()
		{
			Pkcs9DocumentName dn = new Pkcs9DocumentName ();
			Assert.IsNull (dn.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual ("1.3.6.1.4.1.311.88.2.1", dn.Oid.Value, "Oid.Value");
			Assert.IsNull (dn.RawData, "RawData");
			Assert.AreEqual (String.Empty, dn.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, dn.Format (false), "Format(false)");
		}

		[Test]
		// FIXME: throws a NullReferenceException in beta 1 - still true in Dec CTP
		[Category ("NotDotNet")] // MS throws [ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Empty_MessageDigest ()
		{
			Pkcs9DocumentName dn = new Pkcs9DocumentName ();
			Assert.IsNull (dn.DocumentName, "DocumentName");
		}

		[Test]
		public void Constructor_String () 
		{
			Pkcs9DocumentName dn = new Pkcs9DocumentName ("mono");
			Assert.IsNull (dn.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual ("1.3.6.1.4.1.311.88.2.1", dn.Oid.Value, "Oid.Value");
			Assert.AreEqual ("mono", dn.DocumentName, "DocumentName");
			Assert.AreEqual (12, dn.RawData.Length, "RawData.Length");
			Assert.AreEqual ("04-0A-6D-00-6F-00-6E-00-6F-00-00-00", BitConverter.ToString (dn.RawData), "RawData");
			Assert.AreEqual ("04 0a 6d 00 6f 00 6e 00 6f 00 00 00", dn.Format (true), "Format(true)");
			Assert.AreEqual ("04 0a 6d 00 6f 00 6e 00 6f 00 00 00", dn.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_StringNull ()
		{
			string name = null;
			Pkcs9DocumentName dn = new Pkcs9DocumentName (name);
		}

		[Test]
		public void Constructor_Array ()
		{
			byte[] name = { 0x04, 0x0A, 0x6D, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x6F, 0x00, 0x00, 0x00 };
			Pkcs9DocumentName dn = new Pkcs9DocumentName (name);
			Assert.AreEqual ("1.3.6.1.4.1.311.88.2.1", dn.Oid.Value, "Oid.Value");
			Assert.IsNull (dn.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual ("mono", dn.DocumentName, "DocumentName");
			Assert.AreEqual (12, dn.RawData.Length, "RawData.Length");
			Assert.AreEqual ("04-0A-6D-00-6F-00-6E-00-6F-00-00-00", BitConverter.ToString (dn.RawData), "RawData");
			Assert.AreEqual ("04 0a 6d 00 6f 00 6e 00 6f 00 00 00", dn.Format (true), "Format(true)");
			Assert.AreEqual ("04 0a 6d 00 6f 00 6e 00 6f 00 00 00", dn.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_ArrayNull ()
		{
			byte[] name = null;
			Pkcs9DocumentName dn = new Pkcs9DocumentName (name);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			new Pkcs9DocumentName ().CopyFrom (null);
		}
	}
}

#endif
