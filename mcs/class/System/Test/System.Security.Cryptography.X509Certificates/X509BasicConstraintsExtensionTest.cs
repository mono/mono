//
// X509BasicConstraintsExtensionTest.cs 
//	- NUnit tests for X509BasicConstraintsExtension
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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509BasicConstraintsExtensionTest {

		private const string oid = "2.5.29.19";
		private const string fname = "Basic Constraints";

		[Test]
		public void ConstructorEmpty ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension ();
			Assert.IsFalse (bc.Critical, "Critical");
			Assert.IsNull (bc.RawData, "RawData");
			Assert.AreEqual (oid, bc.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, bc.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.AreEqual (String.Empty, bc.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, bc.Format (false), "Format(false)");
		}

		[Test]
		public void ConstructorEmpty_CertificateAuthority ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension ();
			Assert.AreEqual (false, bc.CertificateAuthority, "CertificateAuthority");
		}

		[Test]
		public void ConstructorEmpty_HasPathLengthConstraint ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension ();
			Assert.AreEqual (false, bc.HasPathLengthConstraint, "HasPathLengthConstraint");
		}

		[Test]
		public void ConstructorEmpty_PathLengthConstraint ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension ();
			Assert.AreEqual (0, bc.PathLengthConstraint, "PathLengthConstraint");
		}

		[Test]
		public void ConstructorAsnEncodedData ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x30, 0x06, 0x01, 0x01, 0xFF, 0x02, 0x01, 0x01 });
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (aed, true);
			Assert.IsTrue (bc.Critical, "Critical");
			Assert.AreEqual (8, bc.RawData.Length, "RawData");	// original Oid ignored
			Assert.AreEqual (oid, bc.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, bc.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.IsTrue (bc.CertificateAuthority, "CertificateAuthority");
			Assert.IsTrue (bc.HasPathLengthConstraint, "HasPathLengthConstraint");
			Assert.AreEqual (1, bc.PathLengthConstraint, "PathLengthConstraint");
			Assert.AreEqual ("Subject Type=CA" + Environment.NewLine + "Path Length Constraint=1" + Environment.NewLine, bc.Format (true), "Format(true)");
			Assert.AreEqual ("Subject Type=CA, Path Length Constraint=1", bc.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsn ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[0]);
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (aed, true);
			Assert.AreEqual (String.Empty, bc.Format (true), "Format(true)");
			Assert.AreEqual (String.Empty, bc.Format (false), "Format(false)");
			bool b = bc.CertificateAuthority;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsnTag ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x05, 0x00 });
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (aed, true);
			Assert.AreEqual ("0500", bc.Format (true), "Format(true)");
			Assert.AreEqual ("0500", bc.Format (false), "Format(false)");
			bool b = bc.CertificateAuthority;
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void ConstructorAsnEncodedData_BadAsnLength ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x30, 0x01 });
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (aed, true);
			Assert.AreEqual ("3001", bc.Format (true), "Format(true)");
			Assert.AreEqual ("3001", bc.Format (false), "Format(false)");
			bool b = bc.CertificateAuthority;
		}

		[Test]
		public void ConstructorAsnEncodedData_SmallestValid ()
		{
			AsnEncodedData aed = new AsnEncodedData ("1.2.3", new byte[] { 0x30, 0x00 });
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (aed, true);
			Assert.IsFalse (bc.CertificateAuthority, "CertificateAuthority");
			Assert.IsFalse (bc.HasPathLengthConstraint, "HasPathLengthConstraint");
			Assert.AreEqual (0, bc.PathLengthConstraint, "PathLengthConstraint");
			Assert.AreEqual ("30-00", BitConverter.ToString (bc.RawData), "RawData");
			Assert.AreEqual ("Subject Type=End Entity" + Environment.NewLine + "Path Length Constraint=None" + Environment.NewLine, bc.Format (true), "Format(true)");
			Assert.AreEqual ("Subject Type=End Entity, Path Length Constraint=None", bc.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorAsnEncodedData_Null ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (null, true);
		}

		[Test]
		public void Constructor_TrueTrueZero ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (true, true, 0, true);
			Assert.IsTrue (bc.CertificateAuthority, "CertificateAuthority");
			Assert.IsTrue (bc.HasPathLengthConstraint, "HasPathLengthConstraint");
			Assert.AreEqual (0, bc.PathLengthConstraint, "PathLengthConstraint");
			Assert.AreEqual ("30-06-01-01-FF-02-01-00", BitConverter.ToString (bc.RawData), "RawData");
			Assert.AreEqual ("Subject Type=CA" + Environment.NewLine + "Path Length Constraint=0" + Environment.NewLine, bc.Format (true), "Format(true)");
			Assert.AreEqual ("Subject Type=CA, Path Length Constraint=0", bc.Format (false), "Format(false)");
		}

		[Test]
		public void Constructor_TrueTrueMaxInt ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (true, true, Int32.MaxValue, true);
			Assert.IsTrue (bc.CertificateAuthority, "CertificateAuthority");
			Assert.IsTrue (bc.HasPathLengthConstraint, "HasPathLengthConstraint");
			Assert.AreEqual (Int32.MaxValue, bc.PathLengthConstraint, "PathLengthConstraint");
			Assert.AreEqual ("30-09-01-01-FF-02-04-7F-FF-FF-FF", BitConverter.ToString (bc.RawData), "RawData");
			Assert.AreEqual ("Subject Type=CA" + Environment.NewLine + "Path Length Constraint=2147483647" + Environment.NewLine, bc.Format (true), "Format(true)");
			Assert.AreEqual ("Subject Type=CA, Path Length Constraint=2147483647", bc.Format (false), "Format(false)");
		}

		[Test]
		public void Constructor_TrueFalseNegative ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (true, false, -1, true);
			Assert.IsTrue (bc.CertificateAuthority, "CertificateAuthority");
			Assert.IsFalse (bc.HasPathLengthConstraint, "HasPathLengthConstraint");
			Assert.AreEqual (0, bc.PathLengthConstraint, "PathLengthConstraint");
			Assert.AreEqual ("30-03-01-01-FF", BitConverter.ToString (bc.RawData), "RawData");
			Assert.AreEqual ("Subject Type=CA" + Environment.NewLine + "Path Length Constraint=None" + Environment.NewLine, bc.Format (true), "Format(true)");
			Assert.AreEqual ("Subject Type=CA, Path Length Constraint=None", bc.Format (false), "Format(false)");
		}

		[Test]
		public void Constructor_FalseTruePositive ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (false, true, 1, true);
			Assert.IsFalse (bc.CertificateAuthority, "CertificateAuthority");
			Assert.IsTrue (bc.HasPathLengthConstraint, "HasPathLengthConstraint");
			Assert.AreEqual (1, bc.PathLengthConstraint, "PathLengthConstraint");
			Assert.AreEqual ("30-03-02-01-01", BitConverter.ToString (bc.RawData), "RawData");
			Assert.AreEqual ("Subject Type=End Entity" + Environment.NewLine + "Path Length Constraint=1" + Environment.NewLine, bc.Format (true), "Format(true)");
			Assert.AreEqual ("Subject Type=End Entity, Path Length Constraint=1", bc.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_FalseTrueNegative ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (false, true, -1, true);
		}

		[Test]
		public void Constructor_FalseFalseNegative ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (false, false, -1, true);
			Assert.IsFalse (bc.CertificateAuthority, "CertificateAuthority");
			Assert.IsFalse (bc.HasPathLengthConstraint, "HasPathLengthConstraint");
			Assert.AreEqual (0, bc.PathLengthConstraint, "PathLengthConstraint");
			Assert.AreEqual ("30-00", BitConverter.ToString (bc.RawData), "RawData");
			Assert.AreEqual ("Subject Type=End Entity" + Environment.NewLine + "Path Length Constraint=None" + Environment.NewLine, bc.Format (true), "Format(true)");
			Assert.AreEqual ("Subject Type=End Entity, Path Length Constraint=None", bc.Format (false), "Format(false)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WrongExtension_X509KeyUsageExtension ()
		{
			X509KeyUsageExtension ku = new X509KeyUsageExtension ();
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension ();
			bc.CopyFrom (ku);
		}

		[Test]
		public void WrongExtension_X509Extension ()
		{
			X509Extension ex = new X509Extension ("1.2.3", new byte[0], true);
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (false, true, 1, false);
			Assert.IsFalse (bc.Critical, "Critical");
			bc.CopyFrom (ex);
			Assert.IsTrue (bc.Critical, "Critical");
			Assert.AreEqual (String.Empty, BitConverter.ToString (bc.RawData), "RawData");
			Assert.AreEqual ("1.2.3", bc.Oid.Value, "Oid.Value");
			Assert.IsNull (bc.Oid.FriendlyName, "Oid.FriendlyName");
		}

		[Test]
		[ExpectedException (typeof (CryptographicException))]
		public void WrongExtension_X509Extension_CertificateAuthority ()
		{
			X509Extension ex = new X509Extension ("1.2.3", new byte[0], true);
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension ();
			bc.CopyFrom (ex);
			bool b = bc.CertificateAuthority;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WrongAsnEncodedData ()
		{
			AsnEncodedData aed = new AsnEncodedData (new byte[0]);
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (false, true, 1, false);
			bc.CopyFrom (aed); // note: not the same behaviour than using the constructor!
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyFrom_Null ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension ();
			bc.CopyFrom (null);
		}

		[Test]
		public void CopyFrom_Self ()
		{
			X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension (false, false, -1, true);
			Assert.IsTrue (bc.Critical, "Critical");
			byte[] raw = bc.RawData;
			Assert.AreEqual ("30-00", BitConverter.ToString (raw), "RawData");

			AsnEncodedData aed = new AsnEncodedData (raw);
			X509BasicConstraintsExtension copy = new X509BasicConstraintsExtension (aed, false);
			Assert.IsFalse (copy.Critical, "Critical");
			Assert.AreEqual (2, copy.RawData.Length, "RawData");	// original Oid ignored
			Assert.AreEqual (oid, copy.Oid.Value, "Oid.Value");
			// FIXME: Don't expect that FriendlyName is English. This test fails under non-English Windows.
			//Assert.AreEqual (fname, copy.Oid.FriendlyName, "Oid.FriendlyName");
			Assert.IsFalse (copy.CertificateAuthority, "CertificateAuthority");
			Assert.IsFalse (copy.HasPathLengthConstraint, "HasPathLengthConstraint");
			Assert.AreEqual (0, copy.PathLengthConstraint, "PathLengthConstraint");
		}

#if !MOBILE
		[Test]
		public void CreateViaCryptoConfig ()
		{
			// extensions can be created with CryptoConfig
			AsnEncodedData aed = new AsnEncodedData (new byte[] { 0x30, 0x00 });
			X509BasicConstraintsExtension bc = (X509BasicConstraintsExtension) CryptoConfig.CreateFromName (oid, new object[2] { aed, false });
			Assert.IsFalse (bc.CertificateAuthority, "CertificateAuthority");
			Assert.IsFalse (bc.HasPathLengthConstraint, "HasPathLengthConstraint");
			Assert.AreEqual (0, bc.PathLengthConstraint, "PathLengthConstraint");
			Assert.AreEqual ("30-00", BitConverter.ToString (bc.RawData), "RawData");
		}
#endif
	}
}

#endif
