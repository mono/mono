//
// BitConverterTest.cs - NUnit Test Cases for System.BitConverter
//
// author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
// (C) 2002 Duco Fijma
// 

using NUnit.Framework;
using System;

namespace MonoTests.System
{

public class BitConverterTest : TestCase {
	
	public BitConverterTest() : base("System.BitConverter testsuite") {}
	public BitConverterTest(string name) : base(name) {}

	protected override void SetUp() {}

	protected override void TearDown() {}

	// this property is required.  You need change the parameter for
	// typeof() below to be your class.
	public static ITest Suite {
		get { 
			return new TestSuite(typeof(BitConverterTest)); 
		}
	}

	public void TestIsLittleEndian () {
		byte[] b;

		b = BitConverter.GetBytes(1);
		AssertEquals ("A1", b[0] == 1, BitConverter.IsLittleEndian );
	}

	public void TestDouble() {
		double d = 123.321;

		AssertEquals("A1", d, BitConverter.Int64BitsToDouble (BitConverter.DoubleToInt64Bits (d)));
	}

	public void TestChar () {
		char v1 = 'A';
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
		catch (ArgumentOutOfRangeException) {
			exception = true;
		} 
		AssertEquals ("A4", true, exception);

		try {
			v2 = BitConverter.ToChar ((byte[]) null, 77);
			exception = false;
		}
		catch (ArgumentNullException) {
			exception = true;
		}
		AssertEquals("A5", true, exception);

	}

}
}
