//
// ASN1ConvertTest.cs - NUnit Test Cases for ASN1Convert
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Copyright (C) 2004-2007 Novell, Inc (http://www.novell.com)
//

using System;
using System.Security.Cryptography;
using System.Text;

using Mono.Security;
using NUnit.Framework;

namespace MonoTests.Mono.Security {

	[TestFixture]
	public class ASN1ConvertTest {

		// UtcNow has more precision than the ASN.1 encoded has
		// we need to ignore the extra subseconds but using ToString
		// won't work under 2.0 (Utc string compared to Local string)
		private void AssertDate (DateTime expected, DateTime actual, string message)
		{
			// expected is full precision, actual has second-level precision
			double seconds = new TimeSpan (expected.Ticks - actual.Ticks).TotalSeconds;
			if (seconds >= 1.0) {
				Assert.Fail ("Expected {0} has more than one second ({1}) difference with actual data {2}",
					expected, seconds, actual);
			}
		}

		[Test]
		public void ConvertDateTimeBefore2000 () 
		{
			DateTime expected = DateTime.Now.AddYears (-50);
			ASN1 dt = ASN1Convert.FromDateTime (expected);
			Assert.AreEqual (0x17, dt.Tag, "UTCTIME");
			DateTime actual = ASN1Convert.ToDateTime (dt);
#if NET_2_0
			Assert.AreEqual (DateTimeKind.Utc, actual.Kind, "Kind");
#endif
			AssertDate (expected, actual, "DateTime");
		}

		[Test]
		public void ConvertDateTimeAfter2000 () 
		{
			DateTime expected = DateTime.Now;
			ASN1 dt = ASN1Convert.FromDateTime (expected);
			Assert.AreEqual (0x17, dt.Tag, "UTCTIME");
			DateTime actual = ASN1Convert.ToDateTime (dt);
#if NET_2_0
			Assert.AreEqual (DateTimeKind.Utc, actual.Kind, "Kind");
#endif
			AssertDate (expected, actual, "DateTime");
		}

		[Test]
		public void ConvertDateTimeAfter2050 () 
		{
			DateTime expected = DateTime.Now.AddYears (50);
			ASN1 dt = ASN1Convert.FromDateTime (expected);
			Assert.AreEqual (0x18, dt.Tag, "GENERALIZEDTIME");
			DateTime actual = ASN1Convert.ToDateTime (dt);
#if NET_2_0
			Assert.AreEqual (DateTimeKind.Utc, actual.Kind, "Kind");
#endif
			AssertDate (expected, actual, "DateTime");
		}

		[Test]
		public void ConvertDateTimeInvalidButExistingFormat () 
		{
			string nosecs = "9912312359Z"; 
			ASN1 dt = new ASN1 (0x18, Encoding.ASCII.GetBytes (nosecs));
			DateTime actual = ASN1Convert.ToDateTime (dt);
#if NET_2_0
			Assert.AreEqual (DateTimeKind.Utc, actual.Kind, "Kind");
#endif
			Assert.AreEqual (nosecs, actual.ToUniversalTime ().ToString ("yyMMddHHmm") + "Z", "DateTime");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertToDate_Null () 
		{
			ASN1Convert.ToDateTime (null);
		}

		[Test]
		public void ConvertInt32_Negative () 
		{
			Int32 expected = -1;
			ASN1 integer = ASN1Convert.FromInt32 (expected);
			Int32 actual = ASN1Convert.ToInt32 (integer);
			Assert.AreEqual (expected, actual, "Int32_Negative");
		}

		[Test]
		public void ConvertInt32_Zero () 
		{
			Int32 expected = 0;
			ASN1 integer = ASN1Convert.FromInt32 (expected);
			Int32 actual = ASN1Convert.ToInt32 (integer);
			Assert.AreEqual (expected, actual, "Int32_Zero");
		}
		[Test]
		public void ConvertInt32_One () 
		{
			Int32 expected = 1;
			ASN1 integer = ASN1Convert.FromInt32 (expected);
			Int32 actual = ASN1Convert.ToInt32 (integer);
			Assert.AreEqual (expected, actual, "Int32_Zero");
		}

		[Test]
		public void ConvertInt32_Positive () 
		{
			Int32 expected = Int32.MaxValue;
			ASN1 integer = ASN1Convert.FromInt32 (expected);
			Int32 actual = ASN1Convert.ToInt32 (integer);
			Assert.AreEqual (expected, actual, "Int32_Positive");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ConvertToInt32_WrongTag () 
		{
			ASN1 nul = new ASN1 (0x05);
			Int32 actual = ASN1Convert.ToInt32 (nul);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertToInt32_Null () 
		{
			ASN1Convert.ToInt32 (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertFromUnsignedBigInteger_Null () 
		{
			ASN1Convert.FromUnsignedBigInteger (null);
		}

		[Test]
		public void ConvertFromUnsignedBigInteger () 
		{
			byte[] big = new byte [16];

			big [0] = 1;
			ASN1 bigint = ASN1Convert.FromUnsignedBigInteger (big);
			// no byte is required for the sign (100% positive it's positive ;-)
			Assert.AreEqual (16, bigint.Value.Length, "BigInteger-NotExtended");

			big [0] = 0x80;
			bigint = ASN1Convert.FromUnsignedBigInteger (big);
			// one byte added as 0x00 to be sure it's not interpreted as a negative
			Assert.AreEqual (17, bigint.Value.Length, "BigInteger-SignExtended");
		}

		[Test]
		public void ConvertOID () 
		{
			string expected = "1.2.840.113549.1.7.6";
			ASN1 oid = ASN1Convert.FromOid (expected);
			string actual = ASN1Convert.ToOid (oid);
			Assert.AreEqual (expected, actual, "OID");
		}

		[Test]
#if NET_2_0
		// the large X test tries to encode an invalid OID (second part being > 40).
		// In 1.x CryptoConfig.EncodeOID just encoded the binary (so we copied the 
		// *bad* behaviour) but 2.0 encode it differently (sigh)
		[Category ("NotDotNet")]
#endif
		public void ConvertOID_LargeX () 
		{
			ASN1 asn = new ASN1 (0x06, new byte [] { 0xA8, 0x00, 0x00 });
			string oid = ASN1Convert.ToOid (asn);
			Assert.AreEqual ("2.88.0.0", oid, "ToOID");
			Assert.AreEqual (BitConverter.ToString (asn.GetBytes ()),
				BitConverter.ToString (ASN1Convert.FromOid (oid).GetBytes ()), "FromOID");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertFromOid_Null () 
		{
			ASN1Convert.FromOid (null);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertToOid_Null () 
		{
			ASN1Convert.ToOid (null);
		}
	}
}
