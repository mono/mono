//
// BitConverterTest.cs - NUnit Test Cases for System.BitConverter
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
// (C) 2002 Duco Fijma
// Copyright (C) 2004 Novell (http://www.novell.com)
// 

using NUnit.Framework;
using System;

namespace MonoTests.System
{

[TestFixture]
public class BitConverterTest : Assertion {
	
	public void TestIsLittleEndian ()
	{
		byte[] b;

		b = BitConverter.GetBytes (1);
		AssertEquals ("A1", b[0] == 1, BitConverter.IsLittleEndian );
	}

	private void PrivateTestSingle (float v1)
	{
		float v2;
		byte[] b;
		byte[] larger = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }; 
		bool exception;
		
		b = BitConverter.GetBytes (v1);
		AssertEquals("A1", 4, b.Length);

		v2 = BitConverter.ToSingle (b, 0);
		AssertEquals("A2", v1, v2);	

		b.CopyTo (larger, 1);
		v2 = BitConverter.ToSingle (larger, 1);
		AssertEquals("A3", v1, v2);

		try {
			v2 = BitConverter.ToSingle (larger, 8);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A4", exception);

		try {
			v2 = BitConverter.ToSingle ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A5", exception);
	}

	public void TestSingle()
	{
		PrivateTestSingle (0.1f);
		PrivateTestSingle (24.1e30f);
	}

	private void PrivateTestDouble (double v1)
	{
		double v2;
		byte[] b;
		byte[] larger = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }; 
		bool exception;
		
		b = BitConverter.GetBytes (v1);
		AssertEquals("A1", 8, b.Length);

		v2 = BitConverter.ToDouble (b, 0);
		AssertEquals("A2", v1, v2);	

		b.CopyTo (larger, 1);
		v2 = BitConverter.ToDouble (larger, 1);
		AssertEquals("A3", v1, v2);

		try {
			v2 = BitConverter.ToDouble (larger, 3);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A4", exception);

		try {
			v2 = BitConverter.ToDouble ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A5", exception);
	}

	public void TestDouble ()
	{
		double d = 123.321;

		AssertEquals("A1", d, BitConverter.Int64BitsToDouble (BitConverter.DoubleToInt64Bits (d)));

		PrivateTestDouble (0.1);
		PrivateTestDouble (24.1e77);
	}

	private void PrivateTestBool (bool v1)
	{
		bool v2;
		byte[] b;
		byte[] larger = new byte[] { 0x00, 0x01, 0x02, 0x03 }; 
		bool exception;
		
		b = BitConverter.GetBytes (v1);
		AssertEquals("A1", 1, b.Length);

		v2 = BitConverter.ToBoolean (b, 0);
		AssertEquals("A2", v1, v2);	

		b.CopyTo (larger, 1);
		v2 = BitConverter.ToBoolean (larger, 1);
		AssertEquals("A3", v1, v2);

		try {
			v2 = BitConverter.ToBoolean (larger, 4);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A4", exception);

		try {
			v2 = BitConverter.ToBoolean ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A5", exception);
	}

	public void TestBool () {
		PrivateTestBool(true);
		PrivateTestBool(false);
	}

	private void PrivateTestChar (char v1)
	{
		char v2;
		byte[] b;
		byte[] larger = new byte[] { 0x00, 0x01, 0x02, 0x03 }; 
		bool exception;
		
		b = BitConverter.GetBytes (v1);
		AssertEquals("A1", 2, b.Length);

		v2 = BitConverter.ToChar (b, 0);
		AssertEquals("A2", v1, v2);	

		b.CopyTo (larger, 1);
		v2 = BitConverter.ToChar (larger, 1);
		AssertEquals("A3", v1, v2);

		try {
			v2 = BitConverter.ToChar (larger, 3);
			exception = false;
		}
		// LAMESPEC:
		// the docs say it should be ArgumentOutOfRangeException, but
		// the mscorlib throws an ArgumentException.
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A4", exception);

		try {
			v2 = BitConverter.ToChar ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A5", exception);
	}

	public void TestChar ()
	{
		PrivateTestChar('A');
		PrivateTestChar('\x01ff');
	}

	private void PrivateTestInt16 (short v1)
	{
		short v2;
		byte[] b;
		byte[] larger = new byte[] { 0x00, 0x01, 0x02, 0x03 }; 
		bool exception;
		
		b = BitConverter.GetBytes (v1);
		AssertEquals("A1", 2, b.Length);

		v2 = BitConverter.ToInt16 (b, 0);
		AssertEquals("A2", v1, v2);	

		b.CopyTo (larger, 1);
		v2 = BitConverter.ToInt16 (larger, 1);
		AssertEquals("A3", v1, v2);

		try {
			v2 = BitConverter.ToInt16 (larger, 3);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A4", exception);

		try {
			v2 = BitConverter.ToInt16 ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A5", exception);
	}

	public void TestInt16 ()
	{
		PrivateTestInt16 (0);
		PrivateTestInt16 (1000);
		PrivateTestInt16 (-32768);
		PrivateTestInt16 (32767);
	}

	private void PrivateTestUInt16 (ushort v1)
	{
		ushort v2;
		byte[] b;
		byte[] larger = new byte[] { 0x00, 0x01, 0x02, 0x03 }; 
		bool exception;
		
		b = BitConverter.GetBytes (v1);
		AssertEquals("A1", 2, b.Length);

		v2 = BitConverter.ToUInt16 (b, 0);
		AssertEquals("A2", v1, v2);	

		b.CopyTo (larger, 1);
		v2 = BitConverter.ToUInt16 (larger, 1);
		AssertEquals("A3", v1, v2);

		try {
			v2 = BitConverter.ToUInt16 (larger, 3);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A4", exception);

		try {
			v2 = BitConverter.ToUInt16 ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A5", exception);
	}

	public void TestUInt16 ()
	{
		PrivateTestUInt16 (0);
		PrivateTestUInt16 (1000);
		PrivateTestUInt16 (65535);
	}

	private void PrivateTestInt32 (int v1)
	{
		int v2;
		byte[] b;
		byte[] larger = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }; 
		bool exception;
		
		b = BitConverter.GetBytes (v1);
		AssertEquals("A1", 4, b.Length);

		v2 = BitConverter.ToInt32 (b, 0);
		AssertEquals("A2", v1, v2);	

		b.CopyTo (larger, 1);
		v2 = BitConverter.ToInt32 (larger, 1);
		AssertEquals("A3", v1, v2);

		try {
			v2 = BitConverter.ToInt32 (larger, 8);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A4", exception);

		try {
			v2 = BitConverter.ToInt32 ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A5", exception);
	}

	public void TestInt32 ()
	{
		PrivateTestInt32 (0);
		PrivateTestInt32 (1000);
		PrivateTestInt32 (-2147483648);
		PrivateTestInt32 (2147483647);
	}

	private void PrivateTestUInt32 (uint v1)
	{
		uint v2;
		byte[] b;
		byte[] larger = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }; 
		bool exception;
		
		b = BitConverter.GetBytes (v1);
		AssertEquals ("A1", 4, b.Length);

		v2 = BitConverter.ToUInt32 (b, 0);
		AssertEquals ("A2", v1, v2);	

		b.CopyTo (larger, 1);
		v2 = BitConverter.ToUInt32 (larger, 1);
		AssertEquals ("A3", v1, v2);

		try {
			v2 = BitConverter.ToUInt32 (larger, 8);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A4", exception);

		try {
			v2 = BitConverter.ToUInt32 ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A5", exception);
	}

	public void TestUInt32 ()
	{
		PrivateTestUInt32 (0u);
		PrivateTestUInt32 (1000u);
		PrivateTestUInt32 (4294967295u);
	}

	private void PrivateTestInt64 (long v1)
	{
		long v2;
		byte[] b;
		byte[] larger = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }; 
		bool exception;
		
		b = BitConverter.GetBytes (v1);
		AssertEquals ("A1", 8, b.Length);

		v2 = BitConverter.ToInt64 (b, 0);
		AssertEquals ("A2", v1, v2);	

		b.CopyTo (larger, 1);
		v2 = BitConverter.ToInt64 (larger, 1);
		AssertEquals ("A3", v1, v2);

		try {
			v2 = BitConverter.ToInt64 (larger, 8);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A4", exception);

		try {
			v2 = BitConverter.ToInt64 ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A5", exception);
	}

	public void TestInt64 ()
	{
		PrivateTestInt64 (0);
		PrivateTestInt64 (1000);
		PrivateTestInt64 (-9223372036854775808);
		PrivateTestInt64 (9223372036854775807);
	}

	private void PrivateTestUInt64 (ulong v1)
	{
		ulong v2;
		byte[] b;
		byte[] larger = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }; 
		bool exception;
		
		b = BitConverter.GetBytes (v1);
		AssertEquals("A1", 8, b.Length);

		v2 = BitConverter.ToUInt64 (b, 0);
		AssertEquals("A2", v1, v2);	

		b.CopyTo (larger, 1);
		v2 = BitConverter.ToUInt64 (larger, 1);
		AssertEquals("A3", v1, v2);

		try {
			v2 = BitConverter.ToUInt64 (larger, 8);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A4", exception);

		try {
			v2 = BitConverter.ToUInt64 ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A5", exception);
	}

	public void TestUInt64 ()
	{
		PrivateTestUInt64 (0);
		PrivateTestUInt64 (1000);
		PrivateTestUInt64 (18446744073709551615);
	}

	public void TestToString ()
	{
		string s;
		bool exception;

		byte[] b = new byte[] {0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff};

		AssertEquals ("A1", "00-11-22-33-44-55-66-77-88-99-AA-BB-CC-DD-EE-FF", BitConverter.ToString (b));
		AssertEquals ("A2", "66-77-88-99-AA-BB-CC-DD-EE-FF", BitConverter.ToString (b, 6));
		AssertEquals ("A3", "66-77-88", BitConverter.ToString (b, 6, 3));

		try {
			s = BitConverter.ToString ((byte[]) null);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A4", exception);

		try {
			s = BitConverter.ToString (b, 20);
			exception = false;
		}
		catch (ArgumentException) {
			exception = true;
		} 
		Assert ("A5", exception);

		try {
			s = BitConverter.ToString ((byte[]) null, 20);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		Assert ("A6", exception);

                try {
                        s = BitConverter.ToString (b, 20, 3);
                        exception = false;
                }
                catch (ArgumentOutOfRangeException) {
                        exception = true;
                }
                Assert ("A7", exception);

                try {
                	s = BitConverter.ToString ((byte[]) null, 20, 3);
                        exception = false;
                }
                catch (ArgumentNullException) {
                        exception = true;
                }
                Assert ("A8", exception);

                try {
                        s = BitConverter.ToString (b, 16, 0);
                        exception = false;
                }
                catch (ArgumentOutOfRangeException) {
                        exception = true;
                }
                Assert ("A9", exception);


	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void ToString_StartIndexOverflow () 
	{
		byte[] array = new byte [4];
		BitConverter.ToString (array, Int32.MaxValue, 1);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void ToString_LengthOverflow () 
	{
		byte[] array = new byte [4];
		BitConverter.ToString (array, 1, Int32.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void ToUpperLimit () 
	{
		byte[] array = new byte [4];
		BitConverter.ToInt32 (array, Int32.MaxValue);
	}

	[Test]
	[ExpectedException (typeof (ArgumentOutOfRangeException))]
	public void ToLowerLimit () 
	{
		byte[] array = new byte [4];
		BitConverter.ToInt32 (array, Int32.MinValue);
	}

	[Test]
	public void ToString_Empty ()
	{
		byte[] empty = new byte [0];
		AssertEquals ("Empty", String.Empty, BitConverter.ToString (empty));
	}

	[Test]
	public void ToBoolean () 
	{
		byte[] array = new byte [2] { 0x02, 0x00 };
		Assert ("True", BitConverter.ToBoolean (array, 0));
		AssertEquals ("True==True", true, BitConverter.ToBoolean (array, 0));
		Assert ("False", !BitConverter.ToBoolean (array, 1));
	}
}
}
