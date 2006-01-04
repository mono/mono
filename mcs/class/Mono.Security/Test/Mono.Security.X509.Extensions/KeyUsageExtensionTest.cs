//
// KeyUsageExtensionTest.cs - NUnit Test Cases for 
//	Mono.Security.X509.Extensions.KeyUsageExtension
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
	public class KeyUsageExtensionTest {

		private void Empty (KeyUsageExtension kue)
		{
			Assert.IsFalse (kue.Critical, "Critical");
			Assert.AreEqual ("2.5.29.15", kue.Oid, "Oid");
			Assert.IsNotNull (kue.Name, "Name");
			Assert.IsFalse (kue.Name == kue.Oid, "Name!=Oid");
			Assert.AreEqual (KeyUsages.none, kue.KeyUsage, "KeyUsage");
			Assert.IsTrue (kue.Support (KeyUsages.none), "Support(none)");
			Assert.IsFalse (kue.Support (KeyUsages.digitalSignature), "Support(digitalSignature)");
			Assert.IsFalse (kue.Support (KeyUsages.decipherOnly), "Support(decipherOnly)");
		}

		[Test]
		public void Constructor_Empty ()
		{
			KeyUsageExtension kue = new KeyUsageExtension ();
			Empty (kue);
		}

		[Test]
		public void Constructor_Extension ()
		{
			KeyUsageExtension ext = new KeyUsageExtension ();
			KeyUsageExtension kue = new KeyUsageExtension (ext);
			Empty (kue);
		}

		[Test]
		public void Constructor_ASN1 ()
		{
			KeyUsageExtension ext = new KeyUsageExtension ();
			KeyUsageExtension kue = new KeyUsageExtension (ext.ASN1);
			Empty (kue);
		}

		[Test]
		public void KeyUsage ()
		{
			KeyUsageExtension kue = new KeyUsageExtension ();
			foreach (KeyUsages ku in Enum.GetValues (typeof (KeyUsages))) {
				kue.KeyUsage = ku;
				byte[] rawext = kue.GetBytes ();
				int length = 13;
				if ((int) ku > Byte.MaxValue) {
					length++;
					Assert.AreEqual ((byte) ku, rawext[rawext.Length - 2], ku.ToString () + ".Value2");
					Assert.AreEqual ((byte) ((int) ku >> 8), rawext[rawext.Length - 1], ku.ToString () + ".Value1");
				} else {
					Assert.AreEqual ((byte) ku, rawext[rawext.Length - 1], ku.ToString () + ".Value");
				}
				Assert.AreEqual (length, rawext.Length, ku.ToString () + ".Length");
			}
		}

		[Test]
		public void KeyUsage_MaxValue ()
		{
			KeyUsageExtension kue = new KeyUsageExtension ();
			kue.KeyUsage = (KeyUsages) Int32.MaxValue;
			Assert.IsTrue (kue.Support (KeyUsages.none), "Support(none)");
			Assert.IsTrue (kue.Support (KeyUsages.digitalSignature), "Support(digitalSignature)");
			Assert.IsTrue (kue.Support (KeyUsages.nonRepudiation), "Support(nonRepudiation)");
			Assert.IsTrue (kue.Support (KeyUsages.keyEncipherment), "Support(keyEncipherment)");
			Assert.IsTrue (kue.Support (KeyUsages.dataEncipherment), "Support(dataEncipherment)");
			Assert.IsTrue (kue.Support (KeyUsages.keyAgreement), "Support(keyAgreement)");
			Assert.IsTrue (kue.Support (KeyUsages.keyCertSign), "Support(keyCertSign)");
			Assert.IsTrue (kue.Support (KeyUsages.cRLSign), "Support(cRLSign)");
			Assert.IsTrue (kue.Support (KeyUsages.encipherOnly), "Support(encipherOnly)");
			Assert.IsTrue (kue.Support (KeyUsages.decipherOnly), "Support(decipherOnly)");
		}

		[Test]
		public void Critical ()
		{
			KeyUsageExtension kue = new KeyUsageExtension ();
			kue.Critical = true;
			foreach (KeyUsages ku in Enum.GetValues (typeof (KeyUsages))) {
				kue.KeyUsage = ku;
				byte[] rawext = kue.GetBytes ();
				int length = 16;
				if ((int) ku > Byte.MaxValue) {
					length++;
					Assert.AreEqual ((byte) ku, rawext[rawext.Length - 2], ku.ToString () + ".Value2");
					Assert.AreEqual ((byte) ((int)ku >> 8), rawext[rawext.Length - 1], ku.ToString () + ".Value1");
				} else {
					Assert.AreEqual ((byte) ku, rawext[rawext.Length - 1], ku.ToString () + ".Value");
				}
				Assert.AreEqual (length, rawext.Length, ku.ToString () + ".Length");
				Assert.AreEqual (1, rawext[7], ku.ToString () + ".Critical.Tag");
				Assert.AreEqual (1, rawext[8], ku.ToString () + ".Critical.Length");
				Assert.AreEqual (255, rawext[9], ku.ToString () + ".Critical.Value");
			}
		}
	}
}
