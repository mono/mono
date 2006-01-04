//
// BasicConstraintsExtensionTest.cs - NUnit Test Cases for 
//	Mono.Security.X509.Extensions.BasicConstraintsExtension
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;

using Mono.Security;
using Mono.Security.X509;
using Mono.Security.X509.Extensions;

using NUnit.Framework;

namespace MonoTests.Mono.Security.X509.Extensions {

	[TestFixture]
	public class BasicConstraintsExtensionTest {

		private void Save (string filename, X509Extension ext)
		{
			using (FileStream fs = File.OpenWrite (filename)) {
				byte[] raw = ext.GetBytes ();
				fs.Write (raw, 0, raw.Length);
				fs.Close ();
			}
		}

		private void Empty (BasicConstraintsExtension bce)
		{
			Assert.IsFalse (bce.Critical, "Critical");
			Assert.AreEqual ("2.5.29.19", bce.Oid, "Oid");
			Assert.IsNotNull (bce.Name, "Name");
			Assert.IsFalse (bce.Name == bce.Oid, "Name!=Oid");
			Assert.IsFalse (bce.CertificateAuthority, "CertificateAuthority");
			Assert.AreEqual (BasicConstraintsExtension.NoPathLengthConstraint, bce.PathLenConstraint, "PathLenConstraint");
		}

		[Test]
		public void Constructor_Empty ()
		{
			BasicConstraintsExtension bce = new BasicConstraintsExtension ();
			Empty (bce);
		}

		[Test]
		public void Constructor_Extension ()
		{
			BasicConstraintsExtension ext = new BasicConstraintsExtension ();
			BasicConstraintsExtension bce = new BasicConstraintsExtension (ext);
			Empty (bce);
		}

		[Test]
		public void Constructor_ASN1 ()
		{
			BasicConstraintsExtension ext = new BasicConstraintsExtension ();
			BasicConstraintsExtension bce = new BasicConstraintsExtension (ext.ASN1);
			Empty (bce);
		}

		[Test]
		public void CertificateAuthority_Critical ()
		{
			BasicConstraintsExtension bce = new BasicConstraintsExtension ();
			bce.Critical = true;
			bce.CertificateAuthority = true;
			bce.PathLenConstraint = 0;
			Assert.AreEqual ("30-12-06-03-55-1D-13-01-01-FF-04-08-30-06-01-01-FF-02-01-00", BitConverter.ToString (bce.GetBytes ()), "GetBytes");

			BasicConstraintsExtension bce2 = new BasicConstraintsExtension (bce.ASN1);
			Assert.IsTrue (bce2.Critical, "Critical");
			Assert.IsTrue (bce2.CertificateAuthority, "CertificateAuthority");
			Assert.AreEqual (0, bce2.PathLenConstraint, "PathLenConstraint");
		}

		[Test]
		public void CertificateAuthority_NoPathLengthConstraint ()
		{
			BasicConstraintsExtension bce = new BasicConstraintsExtension ();
			bce.CertificateAuthority = true;
			bce.PathLenConstraint = BasicConstraintsExtension.NoPathLengthConstraint;
			Assert.AreEqual ("30-0C-06-03-55-1D-13-04-05-30-03-01-01-FF", BitConverter.ToString (bce.GetBytes ()), "GetBytes");

			BasicConstraintsExtension bce2 = new BasicConstraintsExtension (bce.ASN1);
			Assert.IsFalse (bce2.Critical, "Critical");
			Assert.IsTrue (bce2.CertificateAuthority, "CertificateAuthority");
			Assert.AreEqual (BasicConstraintsExtension.NoPathLengthConstraint, bce2.PathLenConstraint, "PathLenConstraint");
		}

		[Test]
		public void NotCertificateAuthority ()
		{
			BasicConstraintsExtension bce = new BasicConstraintsExtension ();
			bce.CertificateAuthority = false;
			// CertificateAuthority isn't encoded (default value is false)
			bce.PathLenConstraint = Int32.MaxValue;
			// PathLenConstraint is ignored (per RFC3280)
			Assert.AreEqual ("30-09-06-03-55-1D-13-04-02-30-00", BitConverter.ToString (bce.GetBytes ()), "GetBytes");

			BasicConstraintsExtension bce2 = new BasicConstraintsExtension (bce.ASN1);
			Assert.IsFalse (bce2.Critical, "Critical");
			Assert.IsFalse (bce2.CertificateAuthority, "CertificateAuthority");
			Assert.AreEqual (BasicConstraintsExtension.NoPathLengthConstraint, bce2.PathLenConstraint, "PathLenConstraint");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void NegativePathLenConstraint ()
		{
			BasicConstraintsExtension bce = new BasicConstraintsExtension ();
			bce.PathLenConstraint = Int32.MinValue;
		}
	}
}
