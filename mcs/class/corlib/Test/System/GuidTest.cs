//
// GuidTest.cs - NUnit Test Cases for the System.Guid struct
//
// Authors:
//	Duco Fijma (duco@lorentz.xs4all.nl)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Duco Fijma
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;

namespace MonoTests.System {

	[TestFixture]
	public class GuidTest {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_ByteArray_Null ()
		{
			new Guid ((byte[]) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_ByteArray_InvalidLength ()
		{
			new Guid (new byte[] {0x00, 0x01, 0x02});
		}

		[Test]
		public void Constructor_ByteArray ()
		{
			Guid g =  new Guid (new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f});
			Assert.AreEqual ("03020100-0504-0706-0809-0a0b0c0d0e0f", g.ToString ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_String_Null ()
		{
			new Guid ((string) null);
		}

		[Test]
		public void Constructor_String ()
		{
			Guid g0 = new Guid ("000102030405060708090a0b0c0d0e0f"); 
			Guid g1 = new Guid ("00010203-0405-0607-0809-0a0b0c0d0e0f"); 
			Guid g2 = new Guid ("{00010203-0405-0607-0809-0A0B0C0D0E0F}"); 
			Guid g3 = new Guid ("{0x00010203,0x0405,0x0607,{0x08,0x09,0x0a,0x0b,0x0c,0x0d,0x0e,0x0f}}");
			Guid g4 = new Guid ("(00010203-0405-0607-0809-0A0B0C0D0E0F)");
			Guid g5 = new Guid ("\n  \r  \n 00010203-0405-0607-0809-0a0b0c0d0e0f \r\n");
			string expected = "00010203-0405-0607-0809-0a0b0c0d0e0f";
			Assert.AreEqual (expected, g0.ToString (), "A0");
			Assert.AreEqual (expected, g1.ToString (), "A1");
			Assert.AreEqual (expected, g2.ToString (), "A2");
			Assert.AreEqual (expected, g3.ToString (), "A3");
			Assert.AreEqual (expected, g4.ToString (), "A4");
			Assert.AreEqual (expected, g5.ToString (), "A5");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Constructor_String_Invalid ()
		{
			new Guid ("invalid");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Constructor_String_MissingAllSeparators ()
		{
			new Guid ("{000102030405060708090A0B0C0D0E0F}");	// missing all -
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Constructor_String_MissingSeparator ()
		{
			new Guid ("000102030405-0607-0809-0a0b0c0d0e0f");	// missing first -
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Constructor_String_Mismatch ()
		{
			new Guid ("(000102030405-0607-0809-0a0b0c0d0e0f}");	// open (, close }
		}

		[Test]
		public void Constructor_Int32_2xInt16_8xByte ()
		{
			Guid g1 = new Guid (0x00010203, (short) 0x0405, (short) 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g2 = new Guid (unchecked ((int) 0xffffffff), unchecked ((short) 0xffff), unchecked((short) 0xffff), 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
			Guid g3 = new Guid (0x00010203u, (ushort) 0x0405u, (ushort) 0x0607u, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g4 = new Guid (0xffffffffu, (ushort) 0xffffu, (ushort) 0xffffu, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
		
			Assert.AreEqual ("00010203-0405-0607-0809-0a0b0c0d0e0f", g1.ToString (), "A1");
			Assert.AreEqual ("ffffffff-ffff-ffff-ffff-ffffffffffff", g2.ToString (), "A2");
			Assert.AreEqual ("00010203-0405-0607-0809-0a0b0c0d0e0f", g1.ToString (), "A3");
			Assert.AreEqual ("ffffffff-ffff-ffff-ffff-ffffffffffff", g2.ToString (), "A4");
		}

		[Test]
		public void Constructor_UInt32_2xUInt16_8xByte ()
		{
			Guid g1 = new Guid (0x00010203u, (ushort) 0x0405u, (ushort) 0x0607u, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g2 = new Guid (0xffffffffu, (ushort) 0xffffu, (ushort) 0xffffu, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
		
			Assert.AreEqual ("00010203-0405-0607-0809-0a0b0c0d0e0f", g1.ToString (), "A1");
			Assert.AreEqual ("ffffffff-ffff-ffff-ffff-ffffffffffff", g2.ToString (), "A2");
		}

		[Test]
		public void Constructor_Int32_2xInt16_ByteArray ()
		{
			Guid g1 = new Guid (0x00010203, (short) 0x0405, (short) 0x0607, new byte[] { 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f });
			Guid g2 = new Guid (unchecked ((int) 0xffffffff), unchecked ((short) 0xffff), unchecked((short) 0xffff), new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
		
			Assert.AreEqual ("00010203-0405-0607-0809-0a0b0c0d0e0f", g1.ToString (), "A1");
			Assert.AreEqual ("ffffffff-ffff-ffff-ffff-ffffffffffff", g2.ToString (), "A2");
		}

		[Test]
		public void Empty ()
		{
			Assert.AreEqual ("00000000-0000-0000-0000-000000000000", Guid.Empty.ToString (), "ToString");
			Assert.AreEqual (new byte [16], Guid.Empty.ToByteArray (), "ToByteArray");
		}

		[Test]
		public void NewGuid ()
		{
			Guid g1 = Guid.NewGuid ();
			Guid g2 = Guid.NewGuid ();
			Assert.IsFalse (g1 == g2);
		}

#pragma warning disable 1718
		[Test]
		public void EqualityOp ()
		{
			Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g2 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g3 = new Guid (0x11223344, 0x5566, 0x6677, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff);

			Assert.IsTrue (g1 == g1, "A1");
			Assert.IsTrue (g1 == g2, "A2");
			Assert.IsFalse (g1 == g3, "A3");
		}

		[Test]
		public void InequalityOp ()
		{
			Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g2 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g3 = new Guid (0x11223344, 0x5566, 0x6677, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff);

			Assert.IsFalse (g1 != g1, "A1");
			Assert.IsFalse (g1 != g2, "A2");
			Assert.IsTrue (g1 != g3, "A3");
		}
#pragma warning restore 1718

		[Test]
		public void EqualsObject ()
		{
			Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g2 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g3 = new Guid (0x11223344, 0x5566, 0x6677, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff);
			// cast everything to object so the test still works under 2.0
			Assert.IsTrue (g1.Equals ((object)g1), "A1");
			Assert.IsTrue (g1.Equals ((object)g2), "A2");
			Assert.IsFalse (g1.Equals ((object)g3), "A3");
			Assert.IsFalse (g1.Equals ((object)null), "A4");
			Assert.IsFalse (g1.Equals ((object)"This is not a Guid!"), "A5");
		}

#if NET_2_0
		[Test]
		public void EqualsGuid ()
		{
			Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g2 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g3 = new Guid (0x11223344, 0x5566, 0x6677, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff);

			Assert.IsTrue (g1.Equals (g1), "A1");
			Assert.IsTrue (g1.Equals (g2), "A2");
			Assert.IsFalse (g1.Equals (g3), "A3");
			Assert.IsFalse (g1.Equals (null), "A4");
			Assert.IsFalse (g1.Equals ("This is not a Guid!"), "A5");
		}
#endif

		[Test]
		public void CompareToObject ()
		{
			Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g2 = new Guid (0x00010204, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g3 = new Guid (0x00010203, 0x0405, 0x0607, 0x09, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g4 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x1f);
			// cast everything to object so the test still works under 2.0
			Assert.IsTrue (g1.CompareTo ((object)g2) < 0, "A1");
			Assert.IsTrue (g1.CompareTo ((object)g3) < 0, "A2");
			Assert.IsTrue (g1.CompareTo ((object)g4) < 0, "A3");
			Assert.IsTrue (g2.CompareTo ((object)g1) > 0, "A4");
			Assert.IsTrue (g3.CompareTo ((object)g1) > 0, "A5");
			Assert.IsTrue (g4.CompareTo ((object)g1) > 0, "A6");
			Assert.IsTrue (g1.CompareTo ((object)g1) == 0, "A7");
			Assert.IsTrue (g1.CompareTo ((object)null) > 0, "A8");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CompareToObject_Invalid ()
		{
			Guid.Empty.CompareTo ("Say what?");
		}

#if NET_2_0
		[Test]
		public void CompareToGuid ()
		{
			Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g2 = new Guid (0x00010204, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g3 = new Guid (0x00010203, 0x0405, 0x0607, 0x09, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Guid g4 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x1f);

			Assert.IsTrue (g1.CompareTo (g2) < 0, "A1");
			Assert.IsTrue (g1.CompareTo (g3) < 0, "A2");
			Assert.IsTrue (g1.CompareTo (g4) < 0, "A3");
			Assert.IsTrue (g2.CompareTo (g1) > 0, "A4");
			Assert.IsTrue (g3.CompareTo (g1) > 0, "A5");
			Assert.IsTrue (g4.CompareTo (g1) > 0, "A6");
			Assert.IsTrue (g1.CompareTo (g1) == 0, "A7");
		}
#endif

		[Test]
		public void GetHashCode_Same ()
		{
			Guid copy = new Guid (Guid.Empty.ToString ());
			Assert.AreEqual (Guid.Empty.GetHashCode (), copy.GetHashCode (), "GetHashCode");
		}

		[Test]
		public void GetHashCode_Different ()
		{
			Guid g = Guid.NewGuid ();
			Assert.IsFalse (Guid.Empty.GetHashCode () == g.GetHashCode (), "GetHashCode");
		}

		[Test]
		public void ToByteArray ()
		{
			Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			byte[] expected = new byte[] { 0x03, 0x02, 0x01, 0x00, 0x05, 0x04, 0x07, 0x06, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };
			Assert.AreEqual (expected, g1.ToByteArray (), "ToByteArray");
		}

		[Test]
		public void ToString_AllFormats ()
		{
			Guid g = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
			Assert.AreEqual ("00010203-0405-0607-0809-0a0b0c0d0e0f", g.ToString (), "A1");
			Assert.AreEqual ("000102030405060708090a0b0c0d0e0f", g.ToString ("N"), "A2");
			Assert.AreEqual ("00010203-0405-0607-0809-0a0b0c0d0e0f", g.ToString ("D"), "A3");
			Assert.AreEqual ("{00010203-0405-0607-0809-0a0b0c0d0e0f}", g.ToString ("B"), "A4");
			Assert.AreEqual ("(00010203-0405-0607-0809-0a0b0c0d0e0f)", g.ToString ("P"), "A5");
			Assert.AreEqual ("00010203-0405-0607-0809-0a0b0c0d0e0f", g.ToString (""), "A6");
			Assert.AreEqual ("00010203-0405-0607-0809-0a0b0c0d0e0f", g.ToString ((string)null), "A7");
			Assert.AreEqual ("{00010203-0405-0607-0809-0a0b0c0d0e0f}", g.ToString ("B", null), "A10");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToString_UnsupportedFormat ()
		{
			new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f).ToString ("X");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToString_InvalidFormat ()
		{
			new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f).ToString ("This is invalid");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseExtraJunkN ()
		{
			new Guid ("000102030405060708090a0b0c0d0e0faaaaaa");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseExtraJunkD ()
		{
			new Guid ("00010203-0405-0607-0809-0a0b0c0d0e0faaaaaa");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseExtraJunkB ()
		{
			new Guid ("{00010203-0405-0607-0809-0A0B0C0D0E0F}aaaa");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseExtraJunkP ()
		{
			new Guid ("(00010203-0405-0607-0809-0A0B0C0D0E0F)aaaa");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseExtraJunkX ()
		{
			new Guid ("{0x00010203,0x0405,0x0607,{0x08,0x09,0x0a,0x0b,0x0c,0x0d,0x0e,0x0f}}aaaa");
		}

#if NET_4_0

		/*
			N = new Guid ("000102030405060708090a0b0c0d0e0f"); 
			D = new Guid ("00010203-0405-0607-0809-0a0b0c0d0e0f"); 
			B = new Guid ("{00010203-0405-0607-0809-0A0B0C0D0E0F}"); 
			P = new Guid ("(00010203-0405-0607-0809-0A0B0C0D0E0F)");
			X = new Guid ("{0x00010203,0x0405,0x0607,{0x08,0x09,0x0a,0x0b,0x0c,0x0d,0x0e,0x0f}}");

			string expected = "00010203-0405-0607-0809-0a0b0c0d0e0f";
		*/

		[Test]
		public void ParseExact ()
		{
			const string expected = "00010203-0405-0607-0809-0a0b0c0d0e0f";

			var guid = Guid.ParseExact ("000102030405060708090a0b0c0d0e0f", "N");
			Assert.AreEqual (expected, guid.ToString ());

			guid = Guid.ParseExact ("00010203-0405-0607-0809-0a0b0c0d0e0f", "D");
			Assert.AreEqual (expected, guid.ToString ());

			guid = Guid.ParseExact ("{00010203-0405-0607-0809-0A0B0C0D0E0F}", "B");
			Assert.AreEqual (expected, guid.ToString ());

			guid = Guid.ParseExact ("(00010203-0405-0607-0809-0A0B0C0D0E0F)", "P");
			Assert.AreEqual (expected, guid.ToString ());

			guid = Guid.ParseExact ("{0x00010203,0x0405,0x0607,{0x08,0x09,0x0a,0x0b,0x0c,0x0d,0x0e,0x0f}}", "X");
			Assert.AreEqual (expected, guid.ToString ());
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseExactN ()
		{
			Guid.ParseExact ("00010203-0405-0607-0809-0a0b0c0d0e0f", "N");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseExactD ()
		{
			Guid.ParseExact ("{0x00010203,0x0405,0x0607,{0x08,0x09,0x0a,0x0b,0x0c,0x0d,0x0e,0x0f}}", "D");
		}
#endif
	}
}
