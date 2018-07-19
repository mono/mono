//
// SignerInfoCollectionTest.cs - NUnit tests for SignerInfoCollection
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
#if !MOBILE

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class SignerInfoCollectionTest {

		private SignerInfoCollection GetCollection ()
		{
			SignerInfo si = SignerInfoTest.GetSignerInfo (SignerInfoTest.subjectKeyIdentifierSignature);
			return si.CounterSignerInfos;
		}

		[Test]
		public void EmptyCollection ()
		{
			SignerInfoCollection sic = GetCollection ();
			Assert.AreEqual (0, sic.Count, "Count");
			Assert.IsFalse (sic.IsSynchronized, "IsSynchronized");
			Assert.IsNotNull (sic.SyncRoot, "SyncRoot");
			Assert.IsNotNull (sic.GetEnumerator (), "GetEnumerator");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Indexer_MinusOne ()
		{
			SignerInfoCollection sic = GetCollection ();
			Assert.IsNotNull (sic[-1]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Indexer_One ()
		{
			SignerInfoCollection sic = GetCollection ();
			Assert.IsNotNull (sic[1]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyTo_ArrayInt_Null ()
		{
			SignerInfoCollection sic = GetCollection ();
			sic.CopyTo ((Array)null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CopyTo_ArrayInt_MinusOne ()
		{
			ArrayList al = new ArrayList ();
			SignerInfoCollection sic = GetCollection ();
			sic.CopyTo (al.ToArray (), -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CopyTo_ArrayInt_One ()
		{
			ArrayList al = new ArrayList ();
			SignerInfoCollection sic = GetCollection ();
			sic.CopyTo (al.ToArray (), 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyTo_SignerInfoInt_Null ()
		{
			SignerInfoCollection sic = GetCollection ();
			sic.CopyTo ((SignerInfo[])null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CopyTo_SignerInfoInt_MinusOne ()
		{
			SignerInfo[] sis = new SignerInfo[1];
			SignerInfoCollection sic = GetCollection ();
			sic.CopyTo (sis, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CopyTo_SignerInfoInt_One ()
		{
			SignerInfo[] sis = new SignerInfo[1];
			SignerInfoCollection sic = GetCollection ();
			sic.CopyTo (sis, 1);
		}

		[Test]
		public void CopyTo_SignerInfoInt_Zero ()
		{
			SignerInfo[] sis = new SignerInfo[1];
			SignerInfoCollection sic = GetCollection ();
			sic.CopyTo (sis, 0);
		}
	}
}
#endif
