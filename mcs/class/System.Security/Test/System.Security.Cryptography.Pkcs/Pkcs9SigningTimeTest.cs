//
// Pkcs9SigningTimeTest.cs - NUnit tests for Pkcs9SigningTime
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
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class Pkcs9SigningTimeTest {

		static string signingTimeOid = "1.2.840.113549.1.9.5";
		static string signingTimeName = "Signing Time";
		static DateTime mono10release = new DateTime (632241648000000000);

		[Test]
		public void Constructor_Empty () 
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime ();
			Assert.AreEqual (signingTimeName, st.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (signingTimeOid, st.Oid.Value, "Oid.Value");
			Assert.AreEqual (15, st.RawData.Length, "RawData.Length");
		}

		[Test]
		public void Constructor_DateTime_Now () 
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (DateTime.UtcNow);
			Assert.AreEqual (signingTimeName, st.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (signingTimeOid, st.Oid.Value, "Oid.Value");
			Assert.AreEqual (15, st.RawData.Length, "RawData.Length");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_DateTime_MinValue () 
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (DateTime.MinValue);
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void Constructor_DateTime_MaxValue ()
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (DateTime.MaxValue);
		}

		[Test]
		public void Constructor_DateTime ()
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (mono10release);
			Assert.AreEqual (signingTimeName, st.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (signingTimeOid, st.Oid.Value, "Oid.Value");
			Assert.AreEqual (15, st.RawData.Length, "RawData.Length");
			Assert.AreEqual ("17-0D-30-34-30-36-33-30-30-34-30-30-30-30-5A", BitConverter.ToString (st.RawData), "RawData");
			Assert.AreEqual (mono10release, st.SigningTime, "st.SigningTime");
		}

		[Test]
		public void Constructor_Bytes () 
		{
			byte[] date = new byte [15] { 0x17, 0x0D, 0x30, 0x34, 0x30, 0x36, 0x33, 0x30, 0x30, 0x34, 0x30, 0x30, 0x30, 0x30, 0x5A };
			Pkcs9SigningTime st = new Pkcs9SigningTime (date);
			Assert.AreEqual (signingTimeName, st.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (signingTimeOid, st.Oid.Value, "Oid.Value");
			Assert.AreEqual (15, st.RawData.Length, "RawData.Length");
			Assert.AreEqual ("17-0D-30-34-30-36-33-30-30-34-30-30-30-30-5A", BitConverter.ToString (st.RawData), "RawData");
			Assert.AreEqual (mono10release, st.SigningTime, "st.SigningTime");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Bytes_Null ()
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (null);
		}

		[Test]
		public void CopyFrom () 
		{
			Pkcs9SigningTime st1 = new Pkcs9SigningTime (mono10release);
			Pkcs9SigningTime st2 = new Pkcs9SigningTime (DateTime.UtcNow);
			st1.CopyFrom (st2);
			Assert.AreEqual (st2.Oid.FriendlyName, st1.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (st2.Oid.Value, st1.Oid.Value, "Oid.Value");
			Assert.AreEqual (BitConverter.ToString (st2.RawData), BitConverter.ToString (st1.RawData), "RawData");
			// BUG - SigningTime isn't updated
			Assert.AreEqual (st2.SigningTime, st1.SigningTime, "SigningTime");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			new Pkcs9SigningTime (mono10release).CopyFrom (null);
		}

		[Test]
		public void CopyFrom_Bad ()
		{
			Pkcs9SigningTime st = new Pkcs9SigningTime (mono10release);
			Pkcs9DocumentName dn = new Pkcs9DocumentName ("Mono");
			st.CopyFrom (dn);
			Assert.AreEqual (dn.Oid.FriendlyName, st.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (dn.Oid.Value, st.Oid.Value, "Oid.Value");
			Assert.AreEqual (BitConverter.ToString (dn.RawData), BitConverter.ToString (st.RawData), "RawData");
			// BUG ???
			Assert.AreEqual (mono10release, st.SigningTime, "SigningTime");
		}
	}
}

#endif
