//
// X520AttributesTest.cs - NUnit Test Cases for the X520Attributes class
//
// Authors:
//	Daniel Granath  <dgranath@gmail.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using Mono.Security.X509;

using NUnit.Framework;


namespace MonoTests.Mono.Security.X509 {

	[TestFixture]
	public class X520AttributesTest {

		// Make sure 7-bit values is encoded as PRINTABLESTRING.
		[Test]
		public void sevenBit ()
		{
			X520.CommonName cn = new X520.CommonName ();
			cn.Value = "abcd";
			byte[] encoding = cn.ASN1[1].GetBytes ();
			Assert.AreEqual (0x13, encoding[0]);
		}

		// Make sure 8-bit values is encoded as BMPSTRING.
		[Test]
		public void eightBit ()
		{
			X520.CommonName cn = new X520.CommonName ();
			cn.Value = "aböd";
			byte[] encoding = cn.ASN1[1].GetBytes ();
			Assert.AreEqual (0x1e, encoding[0]);
		}
	}
}
