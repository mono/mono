//
// ExtendedKeyUsageExtensionTest.cs - NUnit Test Cases for 
//	Mono.Security.X509.Extensions.ExtendedKeyUsageExtension
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
	public class ExtendedKeyUsageExtensionTest {

		static string[] CommonKeyPurposes = {
			// serverAuth
			"1.3.6.1.5.5.7.3.1",
			// clientAuth
			"1.3.6.1.5.5.7.3.2",
			// codeSigning
			"1.3.6.1.5.5.7.3.3",
			// emailProtection
			"1.3.6.1.5.5.7.3.4",
			// timeStamping
			"1.3.6.1.5.5.7.3.8",
			// OCSPSigning
			"1.3.6.1.5.5.7.3.9"
		};

		private void Empty (ExtendedKeyUsageExtension eku)
		{
			Assert.IsFalse (eku.Critical, "Critical");
			Assert.AreEqual ("2.5.29.37", eku.Oid, "Oid");
			Assert.IsNotNull (eku.Name, "Name");
			Assert.IsFalse (eku.Name == eku.Oid, "Name!=Oid");
			Assert.AreEqual (0, eku.KeyPurpose.Count, "KeyPurpose");
		}

		[Test]
		public void Constructor_Empty ()
		{
			ExtendedKeyUsageExtension eku = new ExtendedKeyUsageExtension ();
			Empty (eku);
		}

		[Test]
		public void Constructor_Extension ()
		{
			ExtendedKeyUsageExtension ext = new ExtendedKeyUsageExtension ();
			ExtendedKeyUsageExtension eku = new ExtendedKeyUsageExtension (ext);
			Empty (eku);
		}

		[Test]
		public void Constructor_ASN1 ()
		{
			ExtendedKeyUsageExtension ext = new ExtendedKeyUsageExtension ();
			ExtendedKeyUsageExtension eku = new ExtendedKeyUsageExtension (ext.ASN1);
			Empty (eku);
		}

		[Test]
		public void KeyPurpose_NotCritical ()
		{
			ExtendedKeyUsageExtension eku = new ExtendedKeyUsageExtension ();
			foreach (string oid in CommonKeyPurposes) {
				eku.KeyPurpose.Clear ();
				eku.KeyPurpose.Add (oid);
				Assert.AreEqual ("30-13-06-03-55-1D-25-04-0C-30-0A-06-08-2B-06-01-05-05-07-03-0" 
					+ oid [oid.Length - 1].ToString (), BitConverter.ToString (eku.GetBytes ()), oid);
			}
		}

		[Test]
		public void KeyPurpose_Critical ()
		{
			ExtendedKeyUsageExtension eku = new ExtendedKeyUsageExtension ();
			eku.Critical = true;
			foreach (string oid in CommonKeyPurposes) {
				eku.KeyPurpose.Clear ();
				eku.KeyPurpose.Add (oid);
				Assert.AreEqual ("30-16-06-03-55-1D-25-01-01-FF-04-0C-30-0A-06-08-2B-06-01-05-05-07-03-0" 
					+ oid[oid.Length - 1].ToString (), BitConverter.ToString (eku.GetBytes ()), oid);
			}
		}
	}
}
