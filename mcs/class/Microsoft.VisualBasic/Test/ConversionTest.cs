// Collection.cs - NUnit Test Cases for Microsoft.VisualBasic.Collection
//
// Authors:
//   Chris J. Breisch (cjbreisch@altavista.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Chris J. Breisch
// (C) Martin Willemoes Hansen
// 

using NUnit.Framework;
using System;
using Microsoft.VisualBasic;

namespace MonoTests.Microsoft.VisualBasic
{

	[TestFixture]
	public class ConversionTest : Assertion
	{
	
		[SetUp]
		public void GetReady() {}

		[TearDown]
		public void Clear() {}

		[Test]
		public void ErrorToStringEmpty() {
			// FIXME : Do something here, but write the ErrorToString code first :-)
		}

		[Test]
		public void ErrorToStringNumber() {
			// FIXME : Do something here, but write the ErrorToString code first :-)
		}

		// Test the Fix function
		[Test]
		public void Fix() {
			System.Single Sng;
			System.Double Dbl;
			System.Decimal Dec;
			System.String S;
			System.Object O;

			AssertEquals("#F01", System.Int16.MaxValue, Conversion.Fix(System.Int16.MaxValue));
			AssertEquals("#F02", System.Int16.MinValue, Conversion.Fix(System.Int16.MinValue));
			AssertEquals("#F03", System.Int32.MaxValue, Conversion.Fix(System.Int32.MaxValue));
			AssertEquals("#F04", System.Int32.MinValue, Conversion.Fix(System.Int32.MinValue));
			AssertEquals("#F05", System.Int64.MaxValue, Conversion.Fix(System.Int64.MaxValue));
			AssertEquals("#F06", System.Int64.MinValue, Conversion.Fix(System.Int64.MinValue));
			AssertEquals("#F07", (System.Single)Math.Floor(System.Single.MaxValue), Conversion.Fix(System.Single.MaxValue));
			AssertEquals("#F08", -1 * (System.Single)Math.Floor(-1 * System.Single.MinValue), Conversion.Fix(System.Single.MinValue));
			AssertEquals("#F09", Math.Floor(System.Double.MaxValue), Conversion.Fix(System.Double.MaxValue));
			AssertEquals("#F10", -1 * Math.Floor(-1 * System.Double.MinValue), Conversion.Fix(System.Double.MinValue));
			AssertEquals("#F11", Decimal.Floor(System.Decimal.MaxValue), Conversion.Fix(System.Decimal.MaxValue));
			AssertEquals("#F12", -1 * Decimal.Floor(-1 * System.Decimal.MinValue), Conversion.Fix(System.Decimal.MinValue));

			Sng = 99.1F;

			AssertEquals("#F13", 99F, Conversion.Fix(Sng));

			Sng = 99.6F;

			AssertEquals("#F14", 99F, Conversion.Fix(Sng));

			Sng = -99.1F;

			AssertEquals("#F15", -99F, Conversion.Fix(Sng));

			Sng = -99.6F;

			AssertEquals("#F16", -99F, Conversion.Fix(Sng));

			Dbl = 99.1;

			AssertEquals("#F17", 99D, Conversion.Fix(Dbl));

			Dbl = 99.6;

			AssertEquals("#F18", 99D, Conversion.Fix(Dbl));

			Dbl = -99.1;

			AssertEquals("#F19", -99D, Conversion.Fix(Dbl));

			Dbl = -99.6;

			AssertEquals("#F20", -99D, Conversion.Fix(Dbl));

			Dec = 99.1M;

			AssertEquals("#F21", 99M, Conversion.Fix(Dec));

			Dec = 99.6M;

			AssertEquals("#F22", 99M, Conversion.Fix(Dec));

			Dec = -99.1M;

			AssertEquals("#F23", -99M, Conversion.Fix(Dec));

			Dec = -99.6M;

			AssertEquals("#F24", -99M, Conversion.Fix(Dec));

			Dbl = 99.1;
			S = Dbl.ToString();

			AssertEquals("#F25", 99D, Conversion.Fix(S));

			Dbl = 99.6;
			S = Dbl.ToString();

			AssertEquals("#F26", 99D, Conversion.Fix(S));

			Dbl = -99.1;
			S = Dbl.ToString();

			AssertEquals("#F27", -99D, Conversion.Fix(S));

			Dbl = -99.6;
			S = Dbl.ToString();

			AssertEquals("#F28", -99D, Conversion.Fix(S));

			Dbl = 99.1;
			O = Dbl;

			AssertEquals("#F29", 99D, Conversion.Fix(O));

			Sng = 99.6F;
			O = Sng;

			AssertEquals("#F30", (System.Object)99F, Conversion.Fix(O));

			Dbl = -99.1;
			O = Dbl;

			AssertEquals("#F31", -99D, Conversion.Fix(O));

			Dec = -99.6M;
			O = Dec;

			AssertEquals("#F32", (System.Object)(-99M), Conversion.Fix(O));

			O = typeof(int);

			// test for Exceptions
			bool caughtException = false;
			try {
				Conversion.Fix(O);
			}
			catch (Exception e) {
				AssertEquals("#F33", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#F34", true, caughtException);

			caughtException = false;
			try {
				Conversion.Fix(null);
			}
			catch (Exception e) {
				AssertEquals("#F35", typeof(ArgumentNullException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#F36", true, caughtException);

		}
			
		// Test the Int function
		[Test]
		public void Int() {
			System.Single Sng;
			System.Double Dbl;
			System.Decimal Dec;
			System.String S;
			System.Object O;

			AssertEquals("#I01", System.Int16.MaxValue, Conversion.Int(System.Int16.MaxValue));
			AssertEquals("#I02", System.Int16.MinValue, Conversion.Int(System.Int16.MinValue));
			AssertEquals("#I03", System.Int32.MaxValue, Conversion.Int(System.Int32.MaxValue));
			AssertEquals("#I04", System.Int32.MinValue, Conversion.Int(System.Int32.MinValue));
			AssertEquals("#I05", System.Int64.MaxValue, Conversion.Int(System.Int64.MaxValue));
			AssertEquals("#I06", System.Int64.MinValue, Conversion.Int(System.Int64.MinValue));
			AssertEquals("#I07", (System.Single)Math.Floor(System.Single.MaxValue), Conversion.Int(System.Single.MaxValue));
			AssertEquals("#I08", (System.Single)Math.Floor(System.Single.MinValue), Conversion.Int(System.Single.MinValue));
			AssertEquals("#I09", Math.Floor(System.Double.MaxValue), Conversion.Int(System.Double.MaxValue));
			AssertEquals("#I10", Math.Floor(System.Double.MinValue), Conversion.Int(System.Double.MinValue));
			AssertEquals("#I11", Decimal.Floor(System.Decimal.MaxValue), Conversion.Int(System.Decimal.MaxValue));
			AssertEquals("#I12", Decimal.Floor(System.Decimal.MinValue), Conversion.Int(System.Decimal.MinValue));

			Sng = 99.1F;

			AssertEquals("#I13", 99F, Conversion.Int(Sng));

			Sng = 99.6F;

			AssertEquals("#I14", 99F, Conversion.Int(Sng));

			Sng = -99.1F;

			AssertEquals("#I15", -100F, Conversion.Int(Sng));

			Sng = -99.6F;

			AssertEquals("#I16", -100F, Conversion.Int(Sng));

			Dbl = 99.1;

			AssertEquals("#I17", 99D, Conversion.Int(Dbl));

			Dbl = 99.6;

			AssertEquals("#I18", 99D, Conversion.Int(Dbl));

			Dbl = -99.1;

			AssertEquals("#I19", -100D, Conversion.Int(Dbl));

			Dbl = -99.6;

			AssertEquals("#I20", -100D, Conversion.Int(Dbl));

			Dec = 99.1M;

			AssertEquals("#I21", 99M, Conversion.Int(Dec));

			Dec = 99.6M;

			AssertEquals("#I22", 99M, Conversion.Int(Dec));

			Dec = -99.1M;

			AssertEquals("#I23", -100M, Conversion.Int(Dec));

			Dec = -99.6M;

			AssertEquals("#I24", -100M, Conversion.Int(Dec));

			Dbl = 99.1;
			S = Dbl.ToString();

			AssertEquals("#I25", 99D, Conversion.Int(S));

			Dbl = 99.6;
			S = Dbl.ToString();

			AssertEquals("#I26", 99D, Conversion.Int(S));

			Dbl = -99.1;
			S = Dbl.ToString();

			AssertEquals("#I27", -100D, Conversion.Int(S));

			Dbl = -99.6;
			S = Dbl.ToString();

			AssertEquals("#I28", -100D, Conversion.Int(S));

			Dbl = 99.1;
			O = Dbl;

			AssertEquals("#I29", 99D, Conversion.Int(O));

			Sng = 99.6F;
			O = Sng;

			AssertEquals("#I30", 99F, Conversion.Int(O));

			Dbl = -99.1;
			O = Dbl;

			AssertEquals("#I31", -100D, Conversion.Int(O));

			Dec = -99.6M;
			O = Dec;

			AssertEquals("#I32", -100M, Conversion.Int(O));

			// test the exceptions it's supposed to throw

			O = typeof(int);
			bool caughtException = false;

			try {
				Conversion.Fix(O);
			}
			catch (Exception e) {
				AssertEquals("#I33", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#I34", true, caughtException);

			caughtException = false;
			try {
				Conversion.Int(null);
			}
			catch (Exception e) {
				AssertEquals("#I35", typeof(ArgumentNullException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#I36", true, caughtException);


		}	

		// test the Hex function
		[Test]
		public void Hex() {
			AssertEquals("#H01", "FF", Conversion.Hex(System.Byte.MaxValue));
			AssertEquals("#H02", "0", Conversion.Hex(System.Byte.MinValue));
			AssertEquals("#H03", "7FFF", Conversion.Hex(System.Int16.MaxValue));
			AssertEquals("#H04", "8000", Conversion.Hex(System.Int16.MinValue));
			AssertEquals("#H05", "7FFFFFFF", Conversion.Hex(System.Int32.MaxValue));
			AssertEquals("#H06", "80000000", Conversion.Hex(System.Int32.MinValue));
			AssertEquals("#H07", "7FFFFFFFFFFFFFFF", Conversion.Hex(System.Int64.MaxValue));
			AssertEquals("#H08", "8000000000000000", Conversion.Hex(System.Int64.MinValue));

			System.Byte UI8;
			System.Int16 I16;
			System.Int32 I32;
			System.Int64 I64;
			System.Object O;
			System.String S;

			UI8 = 15;
			AssertEquals("#H09", "F", Conversion.Hex(UI8));
			
			I16 = System.Byte.MaxValue;
			AssertEquals("#H10", "FF", Conversion.Hex(I16));

			I16 = (System.Int16)((I16 + 1) * -1);
			AssertEquals("#H11", "FF00", Conversion.Hex(I16));

			I16 = -2;
			AssertEquals("#H12", "FFFE", Conversion.Hex(I16));

			I32 = System.UInt16.MaxValue;
			AssertEquals("#H13", "FFFF", Conversion.Hex(I32));

			I32 = (I32 + 1) * -1;
			AssertEquals("#H14", "FFFF0000", Conversion.Hex(I32));

			I32 = -2;
			AssertEquals("#H15", "FFFFFFFE", Conversion.Hex(I32));

			I64 = System.UInt32.MaxValue;
			AssertEquals("#H16", "FFFFFFFF", Conversion.Hex(I64));

			I64 = (I64 + 1) * -1;
			AssertEquals("#H17", "FFFFFFFF00000000", Conversion.Hex(I64));
			
			I64 = -2;
			AssertEquals("#H18", "FFFFFFFFFFFFFFFE", Conversion.Hex(I64));
			
			I16 = System.Byte.MaxValue;
			S = I16.ToString();
			AssertEquals("#H19", "FF", Conversion.Hex(S));

			I16 = (System.Int16)((I16 + 1) * -1);
			S = I16.ToString();
			AssertEquals("#H20", "FFFFFF00", Conversion.Hex(S));

			I16 = -1;
			S = I16.ToString();
			AssertEquals("#H21", "FFFFFFFF", Conversion.Hex(S));

			I32 = System.UInt16.MaxValue;
			S = I32.ToString();
			AssertEquals("#H22", "FFFF", Conversion.Hex(S));

			I32 = (I32 + 1) * -1;
			S = I32.ToString();
			AssertEquals("#H23", "FFFF0000", Conversion.Hex(S));

			I32 = -2;
			S = I32.ToString();
			AssertEquals("#H24", "FFFFFFFE", Conversion.Hex(S));

			I64 = System.UInt32.MaxValue;
			S = I64.ToString();
			AssertEquals("#H25", "FFFFFFFF", Conversion.Hex(S));

			I64 = (I64 + 1) * -1;
			S = I64.ToString();
			AssertEquals("#H26", "FFFFFFFF00000000", Conversion.Hex(S));
			
			UI8 = System.Byte.MaxValue;
			O = UI8;
			AssertEquals("#H27", "FF", Conversion.Hex(O));

			I16 = System.Byte.MaxValue;
			O = I16;
			AssertEquals("#H28", "FF", Conversion.Hex(O));

			I16 = (System.Int16)((I16 + 1) * -1);
			O = I16;
			AssertEquals("#H29", "FF00", Conversion.Hex(O));

			I16 = -2;
			O = I16;
			AssertEquals("#H30", "FFFE", Conversion.Hex(O));

			I32 = System.UInt16.MaxValue;
			O = I32;
			AssertEquals("#H31", "FFFF", Conversion.Hex(O));

			I32 = (I32 + 1) * -1;
			O = I32;
			AssertEquals("#H32", "FFFF0000", Conversion.Hex(O));

			I32 = -2;
			O = I32;
			AssertEquals("#H33", "FFFFFFFE", Conversion.Hex(O));

			I64 = System.UInt32.MaxValue;
			O = I64;
			AssertEquals("#H34", "FFFFFFFF", Conversion.Hex(O));

			I64 = (I64 + 1) * -1;
			O = I64;
			AssertEquals("#H35", "FFFFFFFF00000000", Conversion.Hex(O));

			I64 = -2;
			O = I64;
			// FIXME : MS doesn't pass this test
			//AssertEquals("#H35", "FFFFFFFFFFFFFFFE", Conversion.Hex(O));

			O = typeof(int);

			bool caughtException = false;
			try {
				Conversion.Hex(O);
			}
			catch (Exception e) {
				AssertEquals("#H36", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#H37", true, caughtException);

			caughtException = false;

			try {
				Conversion.Hex(null);
			}
			catch (Exception e) {
				AssertEquals("#H38", typeof(ArgumentNullException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#H39", true, caughtException);
		}
		
		// test the Oct function
		[Test]
		public void Oct() {
			AssertEquals("#O01", "377", Conversion.Oct(System.Byte.MaxValue));
			AssertEquals("#O02", "0", Conversion.Oct(System.Byte.MinValue));
			AssertEquals("#O03", "77777", Conversion.Oct(System.Int16.MaxValue));
			AssertEquals("#O04", "100000", Conversion.Oct(System.Int16.MinValue));
			AssertEquals("#O05", "17777777777", Conversion.Oct(System.Int32.MaxValue));
			AssertEquals("#O06", "20000000000", Conversion.Oct(System.Int32.MinValue));
			AssertEquals("#O07", "777777777777777777777", Conversion.Oct(System.Int64.MaxValue));
			//AssertEquals("#O08", "1000000000000000000000", Conversion.Oct(System.Int64.MinValue));

			System.Byte UI8;
			System.Int16 I16;
			System.Int32 I32;
			System.Int64 I64;
			System.Object O;
			System.String S;

			UI8 = 15;
			AssertEquals("#O09", "17", Conversion.Oct(UI8));
			
			I16 = System.Byte.MaxValue;
			AssertEquals("#O10", "377", Conversion.Oct(I16));

			I16 = (System.Int16)((I16 + 1) * -1);
			AssertEquals("#O11", "177400", Conversion.Oct(I16));

			I16 = -2;
			AssertEquals("#O12", "177776", Conversion.Oct(I16));

			I32 = System.UInt16.MaxValue;
			AssertEquals("#O13", "177777", Conversion.Oct(I32));

			I32 = (I32 + 1) * -1;
			AssertEquals("#O14", "37777600000", Conversion.Oct(I32));

			I32 = -2;
			AssertEquals("#O15", "37777777776", Conversion.Oct(I32));

			I64 = System.UInt32.MaxValue;
			AssertEquals("#O16", "37777777777", Conversion.Oct(I64));

			I64 = (I64 + 1) * -1;
			AssertEquals("#O17", "1777777777740000000000", Conversion.Oct(I64));
			
			I64 = -2;
			AssertEquals("#O18", "1777777777777777777776", Conversion.Oct(I64));
			
			I16 = System.Byte.MaxValue;
			S = I16.ToString();
			AssertEquals("#O19", "377", Conversion.Oct(S));

			I16 = (System.Int16)((I16 + 1) * -1);
			S = I16.ToString();
			AssertEquals("#O20", "37777777400", Conversion.Oct(S));

			I16 = -2;
			S = I16.ToString();
			AssertEquals("#O21", "37777777776", Conversion.Oct(S));

			I32 = System.UInt16.MaxValue;
			S = I32.ToString();
			AssertEquals("#O22", "177777", Conversion.Oct(S));

			I32 = (I32 + 1) * -1;
			S = I32.ToString();
			AssertEquals("#O23", "37777600000", Conversion.Oct(S));

			I32 = -2;
			S = I32.ToString();
			AssertEquals("#O24", "37777777776", Conversion.Oct(S));

			I64 = System.UInt32.MaxValue;
			S = I64.ToString();
			AssertEquals("#O25", "37777777777", Conversion.Oct(S));

			I64 = (I64 + 1) * -1;
			S = I64.ToString();
			AssertEquals("#O26", "1777777777740000000000", Conversion.Oct(S));
			
			UI8 = System.Byte.MaxValue;
			O = UI8;
			AssertEquals("#O27", "377", Conversion.Oct(O));

			I16 = System.Byte.MaxValue;
			O = I16;
			AssertEquals("#O28", "377", Conversion.Oct(O));

			I16 = (System.Int16)((I16 + 1) * -1);
			O = I16;
			AssertEquals("#O29", "177400", Conversion.Oct(O));

			I16 = -2;
			O = I16;
			AssertEquals("#O29", "177776", Conversion.Oct(O));

			I32 = System.UInt16.MaxValue;
			O = I32;
			AssertEquals("#O30", "177777", Conversion.Oct(O));

			I32 = (I32 + 1) * -1;
			O = I32;
			AssertEquals("#O31", "37777600000", Conversion.Oct(O));

			I32 = -2;
			O = I32;
			AssertEquals("#O32", "37777777776", Conversion.Oct(O));

			I64 = System.UInt32.MaxValue;
			O = I64;
			AssertEquals("#O33", "37777777777", Conversion.Oct(O));

			I64 = (I64 + 1) * -1;
			O = I64;
			AssertEquals("#O34", "1777777777740000000000", Conversion.Oct(O));

			I64 = -2;
			O = I64;

			// FIXME: MS doesn't pass this test
			// AssertEquals("#O35", "1777777777777777777776", Conversion.Oct(O));
		
			O = typeof(int);

			bool caughtException = false;
			try {
				Conversion.Oct(O);
			}
			catch (Exception e) {
				AssertEquals("#O36", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#O37", true, caughtException);
			
			caughtException = false;

			try {
				Conversion.Oct(null);
			}
			catch (Exception e) {
				AssertEquals("#O38", typeof(ArgumentNullException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#O39", true, caughtException);
		}

		// test the Str function
		[Test]
		public void Str() {
			AssertEquals("#S01", "-1", Conversion.Str(-1));
			AssertEquals("#S02", " 1", Conversion.Str(1));

			bool caughtException = false;
			Object O = typeof(int);

			try {
				Conversion.Str(O);
			}
			catch (Exception e) {
				AssertEquals("#S03", typeof(InvalidCastException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#S04", true, caughtException);

			caughtException = false;

			try {
				Conversion.Str(null);
			}
			catch (Exception e) {
				AssertEquals("#S05", typeof(ArgumentNullException), e.GetType());
				caughtException = true;
			}
		}

		// Test the Val function
		[Test]
		public void Val() {
			AssertEquals("#V01", 4, Conversion.Val('4'));
			AssertEquals("#V02", -3542.76, Conversion.Val("    -   3       5   .4   2  7   6E+    0 0 2    "));
			AssertEquals("#V03", 255D, Conversion.Val("&HFF"));
			AssertEquals("#V04", 255D, Conversion.Val("&o377"));

			System.Object O = "    -   3       5   .4     7   6E+    0 0 3";

			AssertEquals("#V05", -35476D, Conversion.Val(O));

			bool caughtException;

			caughtException = false;

			try {
				Conversion.Val("3E+9999999");
			}
			catch (Exception e) {
				AssertEquals("#V06", typeof(OverflowException), e.GetType());
				caughtException = true;
			}

			AssertEquals("#V07", true, caughtException);

			caughtException = false;

			try {
				Conversion.Val(typeof(int));
			}
			catch (Exception e) {
				AssertEquals("#V08", typeof(ArgumentException), e.GetType());
				caughtException = true;
			}
			AssertEquals("#V09", true, caughtException);
		}
	}
}
