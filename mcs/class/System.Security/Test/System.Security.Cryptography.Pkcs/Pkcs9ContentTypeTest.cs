//
// Pkcs9ContentTypeTest.cs - NUnit tests for Pkcs9ContentType
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
	public class Pkcs9ContentTypeTest {

		[Test]
		public void Constructor_Empty ()
		{
			Pkcs9ContentType ct = new Pkcs9ContentType ();
			Assert.AreEqual ("1.2.840.113549.1.9.3", ct.Oid.Value, "Oid.Value");
			Assert.AreEqual ("Content Type", ct.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.IsNull (ct.RawData, "RawData");
			Assert.AreEqual (String.Empty, ct.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, ct.Format (false), "Format(false)");
		}

		[Test]
		[Category ("NotDotNet")] // MS throws [ExpectedException (typeof (NullReferenceException))]
		public void Constructor_Empty_ContentType ()
		{
			Pkcs9ContentType ct = new Pkcs9ContentType ();
			Assert.IsNull (ct.ContentType, "ContentType");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			new Pkcs9ContentType ().CopyFrom (null);
		}

		// TODO - more tests were Pkcs9ContentType is created indirectly
	}
}

#endif
