//
// GuidTest.cs - NUnit Test Cases for the System.Guid struct
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (C) 2002 Duco Fijma
//

using NUnit.Framework;
using System;

namespace MonoTests.System
{

public class GuidTest : TestCase
{
	public GuidTest () {}

	public void TestCtor1 ()
	{
		Guid g =  new Guid (new byte[] {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f});
		bool exception;
		
		if (BitConverter.IsLittleEndian) {
			AssertEquals ("A1", "03020100-0504-0706-0809-0a0b0c0d0e0f", g.ToString ());
		}
		else {
			AssertEquals ("A1", "00010203-0405-0607-0809-0a0b0c0d0e0f", g.ToString ());
		}

		try {
			Guid g1 = new Guid ((byte[]) null);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A2", exception);

		try {
			Guid g1 = new Guid (new byte[] {0x00, 0x01, 0x02});
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		}
		Assert ("A3", exception);
	}

	public void TestCtor2 ()
	{
		Guid g1 = new Guid ("00010203-0405-0607-0809-0a0b0c0d0e0f"); 
		Guid g2 = new Guid ("{00010203-0405-0607-0809-0A0B0C0D0E0F}"); 
		Guid g3 = new Guid ("{0x00010203,0x0405,0x0607,{0x08,0x09,0x0a,0x0b,0x0c,0x0d,0x0e,0x0f}}");
		Guid g4;
		Guid g5;

		bool exception;

		AssertEquals ("A1", "00010203-0405-0607-0809-0a0b0c0d0e0f", g1.ToString ()); 
		AssertEquals ("A2", "00010203-0405-0607-0809-0a0b0c0d0e0f", g2.ToString ()); 
		AssertEquals ("A3", "00010203-0405-0607-0809-0a0b0c0d0e0f", g3.ToString ()); 

		try {
			g4 = new Guid ((string) null);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A4", exception);

		try {
			g5 = new Guid ("invalid");
			exception = false;
		}
		catch (FormatException) {
			exception = true;
		}
		Assert ("A5", exception);
		
	}

	public void TestCtor4 ()
	{
		Guid g1 = new Guid (0x00010203, (short) 0x0405, (short) 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g2 = new Guid (unchecked ((int) 0xffffffff), unchecked ((short) 0xffff), unchecked((short) 0xffff), 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
		
		AssertEquals ("A1", "00010203-0405-0607-0809-0a0b0c0d0e0f", g1.ToString ());
		AssertEquals ("A2", "ffffffff-ffff-ffff-ffff-ffffffffffff", g2.ToString ());

	}

	public void TestCtor5 ()
	{
		Guid g1 = new Guid (0x00010203u, (ushort) 0x0405u, (ushort) 0x0607u, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g2 = new Guid (0xffffffffu, (ushort) 0xffffu, (ushort) 0xffffu, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
		
		AssertEquals ("A1", "00010203-0405-0607-0809-0a0b0c0d0e0f", g1.ToString ());
		AssertEquals ("A2", "ffffffff-ffff-ffff-ffff-ffffffffffff", g2.ToString ());

	}

	public void TestEmpty ()
	{
		AssertEquals ("A1", "00000000-0000-0000-0000-000000000000", Guid.Empty.ToString ());
	}

	public void TestNewGuid ()
	{
		Guid g1 = Guid.NewGuid ();
		Guid g2 = Guid.NewGuid ();
		
		Assert ("A1", g1 != g2);
	}

	public void TestEqualityOp ()
	{
		Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g2 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g3 = new Guid (0x11223344, 0x5566, 0x6677, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff);

		AssertEquals ("A1", true, g1 == g1);
		AssertEquals ("A2", true, g1 == g2);
		AssertEquals ("A3", false, g1 == g3);
	}

	public void TestInequalityOp ()
	{
		Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g2 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g3 = new Guid (0x11223344, 0x5566, 0x6677, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff);

		AssertEquals ("A1", false, g1 != g1);
		AssertEquals ("A2", false, g1 != g2);
		AssertEquals ("A3", true, g1 != g3);
	}

	public void TestEquals ()
	{
		Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g2 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g3 = new Guid (0x11223344, 0x5566, 0x6677, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff);
		string s = "This is not a Guid!";

		AssertEquals ("A1", true, g1.Equals (g1));
		AssertEquals ("A2", true, g1.Equals (g2));
		AssertEquals ("A3", false, g1.Equals (g3));
		AssertEquals ("A4", false, g1.Equals (null));
		AssertEquals ("A5", false, g1.Equals (s));
	}

	public void TestCompareTo ()
	{
		Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g2 = new Guid (0x00010204, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g3 = new Guid (0x00010203, 0x0405, 0x0607, 0x09, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		Guid g4 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x1f);
		bool exception;

		Assert ("A1", g1.CompareTo (g2) < 0);
		Assert ("A2", g1.CompareTo (g3) < 0);
		Assert ("A3", g1.CompareTo (g4) < 0);
		Assert ("A4", g2.CompareTo (g1) > 0);
		Assert ("A5", g3.CompareTo (g1) > 0);
		Assert ("A6", g4.CompareTo (g1) > 0);
		Assert ("A7", g1.CompareTo (g1) == 0);
		Assert ("A8", g1.CompareTo (null) > 0);
		
		try {
			g1.CompareTo ("Say what?");
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		}
		Assert ("A9", exception);
	}

	public void TestGetHashCode ()
	{
		// We don't test anything but the availibility of this member
		int res = Guid.NewGuid ().GetHashCode ();
	
		Assert ("A1", true);
	}

	public void TestToByteArray ()
	{
		Guid g1 = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		byte[] b = g1.ToByteArray ();
		
		AssertEquals ("A1", 0x00010203, BitConverter.ToInt32 (b, 0));
		AssertEquals ("A2", (short) 0x0405, BitConverter.ToInt16 (b, 4));
		AssertEquals ("A3", (short) 0x0607, BitConverter.ToInt16 (b, 6));
		for (int i=8; i<16; ++i) {
			AssertEquals ("A1", (byte) i, b [i]);
		}
		
	}

	public void TestToString ()
	{
		Guid g = new Guid (0x00010203, 0x0405, 0x0607, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f);
		bool exception;
		
		AssertEquals ("A1", "00010203-0405-0607-0809-0a0b0c0d0e0f", g.ToString ());
		AssertEquals ("A2", "000102030405060708090a0b0c0d0e0f", g.ToString ("N"));
		AssertEquals ("A3", "00010203-0405-0607-0809-0a0b0c0d0e0f", g.ToString ("D"));
		AssertEquals ("A4", "{00010203-0405-0607-0809-0a0b0c0d0e0f}", g.ToString ("B"));
		AssertEquals ("A5", "(00010203-0405-0607-0809-0a0b0c0d0e0f)", g.ToString ("P"));
		AssertEquals ("A6", "00010203-0405-0607-0809-0a0b0c0d0e0f", g.ToString (""));
		AssertEquals ("A7", "00010203-0405-0607-0809-0a0b0c0d0e0f", g.ToString ((string)null));

		try {
			g.ToString ("X");
			exception = false;
		}
		catch (FormatException) {
			exception = true;
		}
		Assert ("A8", exception);

		try {
			g.ToString ("This is invalid");
			exception = false;
		}
		catch (FormatException) {
			exception = true;
		}
		Assert ("A9",  exception);

		AssertEquals ("A10", "{00010203-0405-0607-0809-0a0b0c0d0e0f}", g.ToString ("B", null));

	}

}

}
