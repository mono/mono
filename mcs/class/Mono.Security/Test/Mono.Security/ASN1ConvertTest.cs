//
// ASN1ConvertTest.cs - NUnit Test Cases for ASN1Convert
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Security.Cryptography;
using System.Text;

using Mono.Security;
using NUnit.Framework;

namespace MonoTests.Mono.Security {

	[TestFixture]
	public class ASN1ConvertTest : Assertion {

		[Test]
		public void ConvertDateTimeBefore2000 () 
		{
			DateTime expected = DateTime.UtcNow.AddYears (-50);
			ASN1 dt = ASN1Convert.FromDateTime (expected);
			AssertEquals ("UTCTIME", 0x17, dt.Tag);
			DateTime actual = ASN1Convert.ToDateTime (dt);
			AssertEquals ("DateTime", expected.ToString (), actual.ToString ());
		}

		[Test]
		public void ConvertDateTimeAfter2000 () 
		{
			DateTime expected = DateTime.UtcNow;
			ASN1 dt = ASN1Convert.FromDateTime (expected);
			AssertEquals ("UTCTIME", 0x17, dt.Tag);
			DateTime actual = ASN1Convert.ToDateTime (dt);
			AssertEquals ("DateTime", expected.ToString (), actual.ToString ());
		}

		[Test]
		public void ConvertDateTimeAfter2050 () 
		{
			DateTime expected = DateTime.UtcNow.AddYears (50);
			ASN1 dt = ASN1Convert.FromDateTime (expected);
			AssertEquals ("GENERALIZEDTIME", 0x18, dt.Tag);
			DateTime actual = ASN1Convert.ToDateTime (dt);
			AssertEquals ("DateTime", expected.ToString (), actual.ToString ());
		}

		[Test]
		public void ConvertDateTimeInvalidButExistingFormat () 
		{
			string nosecs = "9912312359Z"; 
			ASN1 dt = new ASN1 (0x18, Encoding.ASCII.GetBytes (nosecs));
			DateTime actual = ASN1Convert.ToDateTime (dt);
			AssertEquals ("DateTime", nosecs, actual.ToUniversalTime ().ToString ("yyMMddHHmm") + "Z");
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
			AssertEquals ("Int32_Negative", expected, actual);
		}

		[Test]
		public void ConvertInt32_Zero () 
		{
			Int32 expected = 0;
			ASN1 integer = ASN1Convert.FromInt32 (expected);
			Int32 actual = ASN1Convert.ToInt32 (integer);
			AssertEquals ("Int32_Zero", expected, actual);
		}
		[Test]
		public void ConvertInt32_One () 
		{
			Int32 expected = 1;
			ASN1 integer = ASN1Convert.FromInt32 (expected);
			Int32 actual = ASN1Convert.ToInt32 (integer);
			AssertEquals ("Int32_Zero", expected, actual);
		}

		[Test]
		public void ConvertInt32_Positive () 
		{
			Int32 expected = Int32.MaxValue;
			ASN1 integer = ASN1Convert.FromInt32 (expected);
			Int32 actual = ASN1Convert.ToInt32 (integer);
			AssertEquals ("Int32_Positive", expected, actual);
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
			// one byte added as 0x00 to be sure for sign
			AssertEquals ("BigInteger", 17, bigint.Value.Length);
		}

		[Test]
		public void ConvertOID () 
		{
			string expected = "1.2.840.113549.1.7.6";
			ASN1 oid = ASN1Convert.FromOid (expected);
			string actual = ASN1Convert.ToOid (oid);
			AssertEquals ("OID", expected, actual);
		}

		[Test]
		public void ConvertOID_LargeX () 
		{
			ASN1 asn = new ASN1 (0x06, new byte [] { 0xA8, 0x00, 0x00 });
			string oid = ASN1Convert.ToOid (asn);
			AssertEquals ("ToOID", "2.88.0.0", oid);
			AssertEquals ("FromOID", BitConverter.ToString (asn.GetBytes ()), 
				BitConverter.ToString (ASN1Convert.FromOid (oid).GetBytes ()));
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
