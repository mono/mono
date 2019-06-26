// TestConvert.cs - NUnit Test Cases for System.Convert class
//
// Krister Hansson (ds99krha@thn.htu.se)
// Andreas Jonsson (ds99anjn@thn.htu.se)
// 
// (C) Krister Hansson & Andreas Jonsson
// 

using NUnit.Framework;
using System;
using System.Globalization;

namespace MonoTests.System {

	[TestFixture]
	public class ConvertTest {

		bool boolTrue;
		bool boolFalse;
		byte tryByte;
		char tryChar;
		DateTime tryDT;
		decimal tryDec;
		double tryDbl;
		short tryInt16;
		int tryInt32;
		long tryInt64;
		object tryObj;
		sbyte trySByte;
		float tryFloat;
		string falseString;
		string trueString;
		string nullString;
		string tryStr;
		ushort tryUI16;
		uint tryUI32;
		ulong tryUI64;
		CultureInfo ci;

		[SetUp]		
		public void SetUp ()
		{
			boolTrue = true;
			boolFalse = false;
			tryByte = 0;
			tryChar = 'a';
			tryDT = new DateTime(2002,1,1);
			tryDec = 1234.2345m;
			tryDbl = 0;
			tryInt16 = 1234;
			tryInt32 = 12345;
			tryInt64 = 123456789012;
			tryObj = new Object();
			trySByte = 123;
			tryFloat = 1234.2345f;
			falseString = "false";
			trueString = "true";
			nullString = "null";
			tryStr = "foobar";
			tryUI16 = 34567;
			tryUI32 = 567891234;
			tryUI64 = 0;
			ci = new CultureInfo("en-US");
			ci.NumberFormat.NumberDecimalDigits = 3;
		}

		[Test]
		public void TestChangeType() {
			int iTest = 1;
			try {
				Assert.AreEqual ((short)12345, Convert.ChangeType(tryInt32, typeof(short)), "#A01");
				iTest++;
				Assert.AreEqual ('A', Convert.ChangeType(65, typeof(char)), "#A02");
				iTest++;
				Assert.AreEqual (66, Convert.ChangeType('B', typeof(int)), "#A03");
				iTest++;
				Assert.AreEqual (((ulong)12345), Convert.ChangeType(tryInt32, typeof(ulong)), "#A04");
				
				iTest++;
				Assert.AreEqual (true, Convert.ChangeType(tryDec, TypeCode.Boolean), "#A05");
				iTest++;
				Assert.AreEqual ('f', Convert.ChangeType("f", TypeCode.Char), "#A06");
				iTest++;
				Assert.AreEqual ((decimal)123456789012, Convert.ChangeType(tryInt64, TypeCode.Decimal), "#A07");
				iTest++;
				Assert.AreEqual ((int)34567, Convert.ChangeType(tryUI16, TypeCode.Int32), "#A08");

				iTest++;
				Assert.AreEqual ((double)567891234, Convert.ChangeType(tryUI32, typeof(double), ci), "#A09");
				iTest++;
				Assert.AreEqual ((ushort)0, Convert.ChangeType(tryByte, typeof(ushort), ci), "#A10");
				iTest++;
				Assert.AreEqual ((decimal)567891234, Convert.ChangeType(tryUI32, typeof(decimal), ci), "#A11");
				iTest++;
				Assert.AreEqual ((float)1234, Convert.ChangeType(tryInt16, typeof(float), ci), "#A12");
				iTest++;
				try {
					Assert.AreEqual (null, Convert.ChangeType(null, null, ci), "#A13");
					Assert.Fail ("#F13");
				} catch (ArgumentNullException) {
				}

				iTest++;
				Assert.AreEqual ((decimal)0, Convert.ChangeType(tryByte, TypeCode.Decimal, ci), "#A14");
				iTest++;
				Assert.AreEqual ("f", Convert.ChangeType('f', TypeCode.String, ci), "#A15");
				iTest++;
				Assert.AreEqual ('D', Convert.ChangeType(68, TypeCode.Char, ci), "#A16");
				iTest++;
				Assert.AreEqual ((long)34567, Convert.ChangeType(tryUI16, TypeCode.Int64, ci), "#A17");
				iTest++;
				Assert.AreEqual (null, Convert.ChangeType(null, TypeCode.Empty, ci), "#A18");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception at iTest = " + iTest + ": e = " + e);
			}
			
			try {
				Convert.ChangeType(boolTrue, typeof(char));
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#A25");
			}
			
			try {
				Convert.ChangeType(tryChar, typeof(DateTime));
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#A26");
			}

			try {
				Convert.ChangeType(ci, TypeCode.String);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#A27");
			}

			try {
				Convert.ChangeType(tryInt32, null);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentNullException), e.GetType(), "#A28");
			}

			try 
			{
				Convert.ChangeType(boolTrue, typeof(DateTime), ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#A29");
			}
			
			try {
				Convert.ChangeType(ci, typeof(DateTime), ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#A30");
			}

			/* Should throw ArgumentException but throws InvalidCastException
			try {
				Convert.ChangeType(tryUI32, typeof(FormatException), ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#A??");
			}*/

			try {
				Convert.ChangeType(tryUI32, TypeCode.Byte, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#A31");
			}

			try {
				Convert.ChangeType(boolTrue, TypeCode.Char, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#A32");
			}

			try {
				Convert.ChangeType(boolTrue, null, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentNullException), e.GetType(), "#A33");
			}

			try {
				/* should fail to convert string to any enumeration type. */
				Convert.ChangeType("random string", typeof(DayOfWeek));
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#A34");
			}

		}		

		[Test]
		public void TestGetTypeCode() {
			int marker = 1;
			try {
				Assert.AreEqual (TypeCode.String, Convert.GetTypeCode(tryStr), "#B01");
				marker++;
				Assert.AreEqual (TypeCode.UInt16, Convert.GetTypeCode(tryUI16), "#B02");
				marker++;
				Assert.AreEqual (TypeCode.UInt32, Convert.GetTypeCode(tryUI32), "#B03");
				marker++;
				Assert.AreEqual (TypeCode.UInt64, Convert.GetTypeCode(tryUI64), "#B04");
				marker++;
				Assert.AreEqual (TypeCode.Double, Convert.GetTypeCode(tryDbl), "#B05");
				marker++;
				Assert.AreEqual (TypeCode.Int16, Convert.GetTypeCode(tryInt16), "#B06");
				marker++;
				Assert.AreEqual (TypeCode.Int64, Convert.GetTypeCode(tryInt64), "#B07");
				marker++;
				Assert.AreEqual (TypeCode.Object, Convert.GetTypeCode(tryObj), "#B08");
				marker++;
				Assert.AreEqual (TypeCode.SByte, Convert.GetTypeCode(trySByte), "#B09");
				marker++;
				Assert.AreEqual (TypeCode.Single, Convert.GetTypeCode(tryFloat), "#B10");
				marker++;
				Assert.AreEqual (TypeCode.Byte, Convert.GetTypeCode(tryByte), "#B11");
				marker++;
				Assert.AreEqual (TypeCode.Char, Convert.GetTypeCode(tryChar), "#B12");
				marker++;
//				Assert.AreEqual (TypeCode.DateTime, Convert.GetTypeCode(tryDT), "#B13");
				marker++;
				Assert.AreEqual (TypeCode.Decimal, Convert.GetTypeCode(tryDec), "#B14");
				marker++;
				Assert.AreEqual (TypeCode.Int32, Convert.GetTypeCode(tryInt32), "#B15");
				marker++;
				Assert.AreEqual (TypeCode.Boolean, Convert.GetTypeCode(boolTrue), "#B16");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception at " + marker + ": " + e);
			}
		}

		[Test]
		public void TestIsDBNull() {
			Assert.AreEqual (false, Convert.IsDBNull(tryInt32), "#C01");
			Assert.AreEqual (true, Convert.IsDBNull(Convert.DBNull), "#C02");
			Assert.AreEqual (false, Convert.IsDBNull(boolTrue), "#C03");
			Assert.AreEqual (false, Convert.IsDBNull(tryChar), "#C04");
			Assert.AreEqual (false, Convert.IsDBNull(tryFloat), "#C05");
		}
		
		[Test]
		public void TestToBoolean() {
			tryObj = (object)tryDbl;
			
			Assert.AreEqual (true, Convert.ToBoolean(boolTrue), "#D01");
			Assert.AreEqual (false, Convert.ToBoolean(tryByte), "#D02");
			Assert.AreEqual (true, Convert.ToBoolean(tryDec), "#D03");
			Assert.AreEqual (false, Convert.ToBoolean(tryDbl), "#D04");
			Assert.AreEqual (true, Convert.ToBoolean(tryInt16), "#D05");
			Assert.AreEqual (true, Convert.ToBoolean(tryInt32), "#D06");
			Assert.AreEqual (true, Convert.ToBoolean(tryInt64), "#D07");
			Assert.AreEqual (false, Convert.ToBoolean(tryObj), "#D08");
			Assert.AreEqual (true, Convert.ToBoolean(trySByte), "#D09");
			Assert.AreEqual (true, Convert.ToBoolean(tryFloat), "#D10");
			Assert.AreEqual (true, Convert.ToBoolean(trueString), "#D11");
			Assert.AreEqual (false, Convert.ToBoolean(falseString), "#D12");
			Assert.AreEqual (true, Convert.ToBoolean(tryUI16), "#D13");
			Assert.AreEqual (true, Convert.ToBoolean(tryUI32), "#D14");
			Assert.AreEqual (false, Convert.ToBoolean(tryUI64), "#D15");
			Assert.AreEqual (false, Convert.ToBoolean(tryObj, ci), "#D16");
			Assert.AreEqual (true, Convert.ToBoolean(trueString, ci), "#D17");
			Assert.AreEqual (false, Convert.ToBoolean(falseString, ci), "#D18");
			
			try {
				Convert.ToBoolean(tryChar);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#D20");
			}
			
			try {
				Convert.ToBoolean(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#D21");
			}

			try {
				Convert.ToBoolean(tryStr);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#D22");
			}

			try {
				Convert.ToBoolean(nullString);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#D23");
			}
		}

		[Test]
		public void TestToByte() {
			
			Assert.AreEqual ((byte)1, Convert.ToByte(boolTrue), "#E01");
			Assert.AreEqual ((byte)0, Convert.ToByte(boolFalse), "#E02");
			Assert.AreEqual (tryByte, Convert.ToByte(tryByte), "#E03");
			Assert.AreEqual ((byte)114, Convert.ToByte('r'), "#E04");
			Assert.AreEqual ((byte)201, Convert.ToByte((decimal)200.6), "#E05");
			Assert.AreEqual ((byte)125, Convert.ToByte((double)125.4), "#E06");
			Assert.AreEqual ((byte)255, Convert.ToByte((short)255), "#E07");
			Assert.AreEqual ((byte)254, Convert.ToByte((int)254), "#E08");
			Assert.AreEqual ((byte)34, Convert.ToByte((long)34), "#E09");
			Assert.AreEqual ((byte)1, Convert.ToByte((object)boolTrue), "#E10");
			Assert.AreEqual ((byte)123, Convert.ToByte((float)123.49f), "#E11");
			Assert.AreEqual ((byte)57, Convert.ToByte("57"), "#E12");
			Assert.AreEqual ((byte)75, Convert.ToByte((ushort)75), "#E13");
			Assert.AreEqual ((byte)184, Convert.ToByte((uint)184), "#E14");
			Assert.AreEqual ((byte)241, Convert.ToByte((ulong)241), "#E15");
			Assert.AreEqual ((byte)123, Convert.ToByte(trySByte, ci), "#E16");
			Assert.AreEqual ((byte)27, Convert.ToByte("011011", 2), "#E17");
			Assert.AreEqual ((byte)13, Convert.ToByte("15", 8), "#E18");
			Assert.AreEqual ((byte)27, Convert.ToByte("27", 10), "#E19");
			Assert.AreEqual ((byte)250, Convert.ToByte("FA", 16), "#E20");

			try {
				Convert.ToByte('\u03A9'); // sign of Omega on Win2k
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E25");
			}

			try {
				Convert.ToByte(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#D26");
			}

			try {
				Convert.ToByte((decimal)22000);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E27");
			}

			try {
				Convert.ToByte((double)255.5);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E28");
			}

			try {
				Convert.ToByte(-tryInt16);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E29");
			}

			try {
				Convert.ToByte((int)-256);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E30");
			}

			try {
				Convert.ToByte(tryInt64);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E31");
			}

			try {
				Convert.ToByte((object)ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#E32");
			}

			try {
				Convert.ToByte((sbyte)-1);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E33");
			}

			try {
				Convert.ToByte((float)-0.6f);		
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E34");
			}

			try {
				Convert.ToByte("1a1");		
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#E35");
			}

			try {
				Convert.ToByte("457");		
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E36");
			}

			try {
				Convert.ToByte((ushort)30000);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E37");
			}

			try {
				Convert.ToByte((uint)300);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E38");
			}

			try {
				Convert.ToByte((ulong)987654321321);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E39");
			}

			try {
				Convert.ToByte("10010111", 3);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#E40");
			}

			try {
				Convert.ToByte("3F3", 16);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#E41");
			}
		}

		[Test]
		public void TestToChar(){
			tryByte = 58;
			Assert.AreEqual (':', Convert.ToChar(tryByte), "#F01");
			Assert.AreEqual ('a', Convert.ToChar(tryChar), "#F02");
			Assert.AreEqual ('A', Convert.ToChar((short)65), "#F03");
			Assert.AreEqual ('x', Convert.ToChar((int)120), "#F04");
			Assert.AreEqual ('"', Convert.ToChar((long)34), "#F05");
			Assert.AreEqual ('-', Convert.ToChar((sbyte)45), "#F06");
			Assert.AreEqual ('@', Convert.ToChar("@"), "#F07");
			Assert.AreEqual ('K', Convert.ToChar((ushort)75), "#F08");
			Assert.AreEqual ('=', Convert.ToChar((uint)61), "#F09");
			// Assert.AreEqual ('E', Convert.ToChar((ulong)200), "#F10");
			Assert.AreEqual ('{', Convert.ToChar((object)trySByte, ci), "#F11");
			Assert.AreEqual ('o', Convert.ToChar(tryStr.Substring(1, 1), ci), "#F12");
			
			try {
				Convert.ToChar(boolTrue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#F20");
			}

			try {
				Convert.ToChar(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#F21");
			}

			try {
				Convert.ToChar(tryDec);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#F22");
			}

			try {
				Convert.ToChar(tryDbl);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#F23");
			}

			try {
				Convert.ToChar((short)-1);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#F24");
			}

			try {
				Convert.ToChar(Int32.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#F25");
			}

			try {
				Convert.ToChar(Int32.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#F26");
			}

			try {
				Convert.ToChar(tryInt64);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#F27");
			}

			try {
				Convert.ToChar((long)-123);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#F28");
			}

			try {
				Convert.ToChar(ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#F29");
			}

			try {
				Convert.ToChar(-trySByte);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#F30");
			}

			try {
				Convert.ToChar(tryFloat);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#F31");
			}

			try {
				Convert.ToChar("foo");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#F32");
			}
			
			try {
				Convert.ToChar(null);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentNullException), e.GetType(), "#F33");
			}

			try {
				Convert.ToChar(new Exception(), ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#F34");
			}

			try {
				Convert.ToChar(null, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentNullException), e.GetType(), "#F35");
			}

			try {
				Convert.ToChar("", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#F36");
			}

			try {
				Convert.ToChar(tryStr, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#F37");
			}
		}

		/*[Ignore ("http://bugzilla.ximian.com/show_bug.cgi?id=45286")]
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void G22 () {
			Convert.ToDateTime("20002-25-01");
		} */

		[Test]
		public void TestToDateTime() {
			string dateString = "01/01/2002";
			int iTest = 1;
			try {
				Assert.AreEqual (tryDT, Convert.ToDateTime(tryDT), "#G01");
				iTest++;
				Assert.AreEqual (tryDT, Convert.ToDateTime(dateString), "#G02");
				iTest++;
				Assert.AreEqual (tryDT, Convert.ToDateTime(dateString, ci), "#G03");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception at iTest = " + iTest + ": e = " + e);
			}

			try {
				Convert.ToDateTime(boolTrue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G10");
			}

			try {
				Convert.ToDateTime(tryByte);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G11");
			}

			try {
				Convert.ToDateTime(tryChar);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G12");
			}

			try {
				Convert.ToDateTime(tryDec);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G13");
			}

			try {
				Convert.ToDateTime(tryDbl);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G14");
			}

			try {
				Convert.ToDateTime(tryInt16);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G15");
			}

			try {
				Convert.ToDateTime(tryInt32);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G16");
			}

			try {
				Convert.ToDateTime(tryInt64);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G17");
			}

			try {
				Convert.ToDateTime(ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G18");
			}

			try {
				Convert.ToDateTime(trySByte);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G19");
			}

			try {
				Convert.ToDateTime(tryFloat);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G20");
			}

			try {
				Convert.ToDateTime("20a2-01-01");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#G21");
			}

			try {
				Convert.ToDateTime(tryUI16);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G23");
			}

			try {
				Convert.ToDateTime(tryUI32);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G24");
			}

			try {
				Convert.ToDateTime(tryUI64);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G25");
			}

			try {
				Convert.ToDateTime(ci, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#G26");
			}

			try {
				Convert.ToDateTime("20a2-01-01", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#G27");
			}

			// this is supported by .net 1.1 (defect 41845)
			try {
				Convert.ToDateTime("20022-01-01");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#G28");
			}

			try {
				Convert.ToDateTime("2002-21-01");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#G29");
			}

			try {
				Convert.ToDateTime("2002-111-01");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#G30");
			}

			try {
				Convert.ToDateTime("2002-01-41");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#G31");
			}

			try {
				Convert.ToDateTime("2002-01-111");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#G32");
			}

			try {
				Assert.AreEqual (tryDT, Convert.ToDateTime("2002-01-01"), "#G33");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception at #G33 " + e);
			}

			try {
				Convert.ToDateTime("2002-01-11 34:11:11");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#G34");
			}

			try {
				Convert.ToDateTime("2002-01-11 11:70:11");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#G35");
			}

			try {
				Convert.ToDateTime("2002-01-11 11:11:70");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#G36");
			}

		}

		[Test]
		public void TestToDecimal() {
			Assert.AreEqual ((decimal)1, Convert.ToDecimal(boolTrue), "#H01");
			Assert.AreEqual ((decimal)0, Convert.ToDecimal(boolFalse), "#H02");
			Assert.AreEqual ((decimal)tryByte, Convert.ToDecimal(tryByte), "#H03");
			Assert.AreEqual (tryDec, Convert.ToDecimal(tryDec), "#H04");
			Assert.AreEqual ((decimal)tryDbl, Convert.ToDecimal(tryDbl), "#H05");
			Assert.AreEqual ((decimal)tryInt16, Convert.ToDecimal(tryInt16), "#H06");
			Assert.AreEqual ((decimal)tryInt32, Convert.ToDecimal(tryInt32), "#H07");
			Assert.AreEqual ((decimal)tryInt64, Convert.ToDecimal(tryInt64), "#H08");
			Assert.AreEqual ((decimal)trySByte, Convert.ToDecimal(trySByte), "#H09");
			Assert.AreEqual ((decimal)tryFloat, Convert.ToDecimal(tryFloat), "#H10");
			string sep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
//			Assert.AreEqual ((decimal)23456.432, Convert.ToDecimal("23456" + sep + "432"), "#H11");
//			Note: changed because the number were the same but with a different base
//			and this isn't a Convert bug (but a Decimal bug). See #60227 for more details.
//			http://bugzilla.ximian.com/show_bug.cgi?id=60227
			Assert.IsTrue (Decimal.Equals (23456.432m, Convert.ToDecimal ("23456" + sep + "432")), "#H11");
			Assert.AreEqual ((decimal)tryUI16, Convert.ToDecimal(tryUI16), "#H12");
			Assert.AreEqual ((decimal)tryUI32, Convert.ToDecimal(tryUI32), "#H13");
			Assert.AreEqual ((decimal)tryUI64, Convert.ToDecimal(tryUI64), "#H14");
			Assert.AreEqual ((decimal)63784, Convert.ToDecimal("63784", ci), "#H15");
			
			try {
				Convert.ToDecimal(tryChar);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#H20");
			}

			try {
				Convert.ToDecimal(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#H21");
			}

			try {
				Convert.ToDecimal(double.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#H22");
			}

			try {
				Convert.ToDecimal(double.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#H23");
			}

			try {
				Convert.ToDecimal(ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#H24");
			}
			
			try {
				Convert.ToDecimal(tryStr);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#H25");
			}
			
			try {
				string maxDec = decimal.MaxValue.ToString();
				maxDec = maxDec + "1";				
				Convert.ToDecimal(maxDec);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#H26");
			}

			try {
				Convert.ToDecimal(ci, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#H27");
			}

			try {
				Convert.ToDecimal(tryStr, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#H28");
			}
			
			try {
				string maxDec = decimal.MaxValue.ToString();
				maxDec = maxDec + "1";
				Convert.ToDecimal(maxDec, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#H29");
			}
		}
		
		[Test]
		public void TestToDouble() {
			int iTest = 1;
			try {
				Assert.AreEqual ((double)1, Convert.ToDouble(boolTrue), "#I01");
				iTest++;
				Assert.AreEqual ((double)0, Convert.ToDouble(boolFalse), "#I02");
				iTest++;
				Assert.AreEqual ((double)tryByte, Convert.ToDouble(tryByte), "#I03");
				iTest++;
				Assert.AreEqual (tryDbl, Convert.ToDouble(tryDbl), "#I04");
				iTest++;
				Assert.AreEqual ((double)tryDec, Convert.ToDouble(tryDec), "#I05");
				iTest++;
				Assert.AreEqual ((double)tryInt16, Convert.ToDouble(tryInt16), "#I06");
				iTest++;
				Assert.AreEqual ((double)tryInt32, Convert.ToDouble(tryInt32), "#I07");
				iTest++;
				Assert.AreEqual ((double)tryInt64, Convert.ToDouble(tryInt64), "#I08");
				iTest++;
				Assert.AreEqual ((double)trySByte, Convert.ToDouble(trySByte), "#I09");
				iTest++;
				Assert.AreEqual ((double)tryFloat, Convert.ToDouble(tryFloat), "#I10");
				iTest++;
				string sep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
				Assert.AreEqual ((double)23456.432, Convert.ToDouble("23456" + sep + "432"), "#I11");
				iTest++;
				Assert.AreEqual ((double)tryUI16, Convert.ToDouble(tryUI16), "#I12");
				iTest++;
				Assert.AreEqual ((double)tryUI32, Convert.ToDouble(tryUI32), "#I13");
				iTest++;
				Assert.AreEqual ((double)tryUI64, Convert.ToDouble(tryUI64), "#I14");
				iTest++;
				Assert.AreEqual ((double)63784, Convert.ToDouble("63784", ci), "#H15");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception at iTest = " + iTest + ": e = " + e);
			}
			
			try {
				Convert.ToDouble(tryChar);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#I20");
			}

			try {
				Convert.ToDouble(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#I21");
			}

			try {
				Convert.ToDouble(ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#I22");
			}
			
			try {
				Convert.ToDouble(tryStr);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#I23");
			}
			
			try {
				string maxDec = double.MaxValue.ToString();
				maxDec = maxDec + "1";				
				Convert.ToDouble(maxDec);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#I24");
			}

			try {
				Convert.ToDouble(ci, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#I25");
			}

			try {
				Convert.ToDouble(tryStr, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#I26");
			}
			
			try {
				string maxDec = double.MaxValue.ToString();
				maxDec = maxDec + "1";
				Convert.ToDouble(maxDec, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#I27");
			}

			try {
				Convert.ToDouble(tryObj, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#I28");
			}
		}

		[Test]
		public void TestToInt16() {
			Assert.AreEqual ((short)0, Convert.ToInt16(boolFalse), "#J01");
			Assert.AreEqual ((short)1, Convert.ToInt16(boolTrue), "#J02");
			Assert.AreEqual ((short)97, Convert.ToInt16(tryChar), "#J03");
			Assert.AreEqual ((short)1234, Convert.ToInt16(tryDec), "#J04");
			Assert.AreEqual ((short)0, Convert.ToInt16(tryDbl), "#J05");
			Assert.AreEqual ((short)1234, Convert.ToInt16(tryInt16), "#J06");
			Assert.AreEqual ((short)12345, Convert.ToInt16(tryInt32), "#J07");
			Assert.AreEqual ((short)30000, Convert.ToInt16((long)30000), "#J08");
			Assert.AreEqual ((short)123, Convert.ToInt16(trySByte), "#J09");
			Assert.AreEqual ((short)1234, Convert.ToInt16(tryFloat), "#J10");
			Assert.AreEqual ((short)578, Convert.ToInt16("578"), "#J11");
			Assert.AreEqual ((short)15500, Convert.ToInt16((ushort)15500), "#J12");
			Assert.AreEqual ((short)5489, Convert.ToInt16((uint)5489), "#J13");
			Assert.AreEqual ((short)9876, Convert.ToInt16((ulong)9876), "#J14");
			Assert.AreEqual ((short)14, Convert.ToInt16("14", ci), "#J15");
			Assert.AreEqual ((short)11, Convert.ToInt16("01011", 2), "#J16");
			Assert.AreEqual ((short)1540, Convert.ToInt16("3004", 8), "#J17");
			Assert.AreEqual ((short)321, Convert.ToInt16("321", 10), "#J18");
			Assert.AreEqual ((short)2748, Convert.ToInt16("ABC", 16), "#J19");

			try {
				Convert.ToInt16(char.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J25");
			}

			try {
				Convert.ToInt16(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#J26");
			}

			try {
				Convert.ToInt16((decimal)(short.MaxValue + 1));
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J27");
			}

			try {
				Convert.ToInt16((decimal)(short.MinValue - 1));
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J28");
			}

			try {
				Convert.ToInt16((double)(short.MaxValue + 1));
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J29");
			}

			try {
				Convert.ToInt16((double)(short.MinValue - 1));
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J30");
			}

			try {
				Convert.ToInt16(50000);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J31");
			}

			try {
				Convert.ToInt16(-50000);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J32");
			}

			try {
				Convert.ToInt16(tryInt64);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J33");
			}

			try {
				Convert.ToInt16(-tryInt64);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J34");
			}

			try {
				Convert.ToInt16(tryObj);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#J35");
			}

			try {
				Convert.ToInt16((float)32767.5);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J36");
			}

			try {
				Convert.ToInt16((float)-33000.54);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J37");
			}

			try {
				Convert.ToInt16(tryStr);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#J38");
			}
			
			try {
				Convert.ToInt16("-33000");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J39");
			}

			try {							
				Convert.ToInt16(ushort.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J40");
			}

			try {							
				Convert.ToInt16(uint.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J41");
			}

			try {							
				Convert.ToInt16(ulong.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J42");
			}

			try {
				Convert.ToInt16(tryObj, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#J43");
			}

			try {
				Convert.ToInt16(tryStr, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#J44");
			}
			
			try {
				Convert.ToInt16("-33000", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J45");
			}

			try {
				Convert.ToInt16("321", 11);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#J46");
			}

			try {
				Convert.ToInt16("D8BF1", 16);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#J47");
			}
		}

		[Test]
		public void TestToInt32() {
			long tryMax = long.MaxValue;
			long tryMin = long.MinValue;
			Assert.AreEqual ((int)0, Convert.ToInt32(boolFalse), "#K01");
			Assert.AreEqual ((int)1, Convert.ToInt32(boolTrue), "#K02");
			Assert.AreEqual ((int)0, Convert.ToInt32(tryByte), "#K03");
			Assert.AreEqual ((int)97, Convert.ToInt32(tryChar), "#K04");
			Assert.AreEqual ((int)1234, Convert.ToInt32(tryDec), "#K05");
			Assert.AreEqual ((int)0, Convert.ToInt32(tryDbl), "#K06");
			Assert.AreEqual ((int)1234, Convert.ToInt32(tryInt16), "#K07");
			Assert.AreEqual ((int)12345, Convert.ToInt32(tryInt32), "#K08");
			Assert.AreEqual ((int)60000, Convert.ToInt32((long)60000), "#K09");
			Assert.AreEqual ((int)123, Convert.ToInt32(trySByte), "#K10");
			Assert.AreEqual ((int)1234, Convert.ToInt32(tryFloat), "#K11");
			Assert.AreEqual ((int)9876, Convert.ToInt32((string)"9876"), "#K12");
			Assert.AreEqual ((int)34567, Convert.ToInt32(tryUI16), "#K13");
			Assert.AreEqual ((int)567891234, Convert.ToInt32(tryUI32), "#K14");
			Assert.AreEqual ((int)0, Convert.ToInt32(tryUI64), "#K15");
			Assert.AreEqual ((int)123, Convert.ToInt32("123", ci), "#K16");
			Assert.AreEqual ((int)128, Convert.ToInt32("10000000", 2), "#K17");
			Assert.AreEqual ((int)302, Convert.ToInt32("456", 8), "#K18");
			Assert.AreEqual ((int)456, Convert.ToInt32("456", 10), "#K19");
			Assert.AreEqual ((int)1110, Convert.ToInt32("456", 16), "#K20");

			try {							
				Convert.ToInt32(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#K25");
			}

			try {				
				Convert.ToInt32((decimal)tryMax);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K26");
			}

			try {
				Convert.ToInt32((decimal)tryMin);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K27");
			}

			try {
				Convert.ToInt32((double)tryMax);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K28");
			}

			try {
				Convert.ToInt32((double)tryMin);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K29");
			}

			try {							
				Convert.ToInt32(tryInt64);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K30");
			}

			try {
				Convert.ToInt32(-tryInt64);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K31");
			}

			try {
				Convert.ToInt32(tryObj);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#K32");
			}

			try {
				Convert.ToInt32((float)tryMax);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K33");
			}

			try {
				Convert.ToInt32((float)tryMin);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K34");
			}
			
			try {
				Convert.ToInt32(tryStr, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#K35");
			}

			try {
				Convert.ToInt32("-46565465123");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K36");
			}

			try {
				Convert.ToInt32("46565465123");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K37");
			}

			try {
				Convert.ToInt32((uint)tryMax);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K38");
			}

			try {
				Convert.ToInt32((ulong)tryMax);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K39");
			}

			try {
				Convert.ToInt32(tryObj, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#K40");
			}

			try {
				Convert.ToInt32(tryStr, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#K41");
			}

			try {
				Convert.ToInt32("-46565465123", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#K42");
			}
			
			try {
				Convert.ToInt32("654", 9);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#K43");
			}
		}

		[Test]
		public void TestToInt64() {
			decimal longMax = long.MaxValue;
			longMax += 1000000;
			decimal longMin = long.MinValue;
			longMin -= 1000000;

			Assert.AreEqual ((long)0, Convert.ToInt64(boolFalse), "#L01");
			Assert.AreEqual ((long)1, Convert.ToInt64(boolTrue), "#L02");
			Assert.AreEqual ((long)97, Convert.ToInt64(tryChar), "#L03");
			Assert.AreEqual ((long)1234, Convert.ToInt64(tryDec), "#L04");
			Assert.AreEqual ((long)0, Convert.ToInt64(tryDbl), "#L05");
			Assert.AreEqual ((long)1234, Convert.ToInt64(tryInt16), "#L06");
			Assert.AreEqual ((long)12345, Convert.ToInt64(tryInt32), "#L07");
			Assert.AreEqual ((long)123456789012, Convert.ToInt64(tryInt64), "#L08");
			Assert.AreEqual ((long)123, Convert.ToInt64(trySByte), "#L09");
			Assert.AreEqual ((long)1234, Convert.ToInt64(tryFloat), "#L10");
			Assert.AreEqual ((long)564897, Convert.ToInt64("564897"), "#L11");
			Assert.AreEqual ((long)34567, Convert.ToInt64(tryUI16), "#L12");
			Assert.AreEqual ((long)567891234, Convert.ToInt64(tryUI32), "#L13");
			Assert.AreEqual ((long)0, Convert.ToInt64(tryUI64), "#L14");
			Assert.AreEqual ((long)-2548751, Convert.ToInt64("-2548751", ci), "#L15");
			Assert.AreEqual ((long)24987562, Convert.ToInt64("1011111010100011110101010", 2), "#L16");
			Assert.AreEqual ((long)-24578965, Convert.ToInt64("1777777777777642172153", 8), "#L17");
			Assert.AreEqual ((long)248759757, Convert.ToInt64("248759757", 10), "#L18");
			Assert.AreEqual ((long)256, Convert.ToInt64("100", 16), "#L19");

			try {
				Convert.ToInt64(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#L20");
			}

			try {
				Convert.ToInt64((decimal)longMax + 1);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#L21");
			}

			try {
				Convert.ToInt64((decimal)longMin);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#L24");
			}

			try {
				Convert.ToInt64((double)longMax);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#L25:"+longMax);
			}

			try {
				Convert.ToInt64((double)longMin);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#L26");
			}

			try {
				Convert.ToInt64(new Exception());
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#L27");
			}

			try {
				Convert.ToInt64(((float)longMax)*100);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#L28:"+longMax);
			}

			try {
				Convert.ToInt64(((float)longMin)*100);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#L29");
			}

			try {
				Convert.ToInt64("-567b3");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#L30");
			}

			try {
				Convert.ToInt64(longMax.ToString());
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#L31:");
			}

			try {
				Convert.ToInt64(ulong.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#L32");
			}

			try {
				Convert.ToInt64(tryStr, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#L32b");
			}
			
			try {
				Convert.ToInt64(longMin.ToString(), ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#L33");
			}

			try {
				Convert.ToInt64("321", 11);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#L34");
			}
		}

		[Test]
		public void TestToSByte() {
			int iTest = 1;
			try {
				Assert.AreEqual ((sbyte)0, Convert.ToSByte(boolFalse), "#M01");
				iTest++;
				Assert.AreEqual ((sbyte)1, Convert.ToSByte(boolTrue), "#M02");
				iTest++;
				Assert.AreEqual ((sbyte)97, Convert.ToSByte(tryChar), "#M03");
				iTest++;
				Assert.AreEqual ((sbyte)15, Convert.ToSByte((decimal)15), "#M04");
				iTest++;
				Assert.AreEqual ((sbyte)0, Convert.ToSByte(tryDbl), "#M05");
				iTest++;
				Assert.AreEqual ((sbyte)127, Convert.ToSByte((short)127), "#M06");
				iTest++;
				Assert.AreEqual ((sbyte)-128, Convert.ToSByte((int)-128), "#M07");
				iTest++;
				Assert.AreEqual ((sbyte)30, Convert.ToSByte((long)30), "#M08");
				iTest++;
				Assert.AreEqual ((sbyte)123, Convert.ToSByte(trySByte), "#M09");
				iTest++;
				Assert.AreEqual ((sbyte)12, Convert.ToSByte((float)12.46987f), "#M10");
				iTest++;
				Assert.AreEqual ((sbyte)1, Convert.ToSByte("1"), "#M11");
				iTest++;
				Assert.AreEqual ((sbyte)99, Convert.ToSByte((ushort)99), "#M12");
				iTest++;
				Assert.AreEqual ((sbyte)54, Convert.ToSByte((uint)54), "#M13");
				iTest++;
				Assert.AreEqual ((sbyte)127, Convert.ToSByte((ulong)127), "#M14");
				iTest++;
				Assert.AreEqual ((sbyte)14, Convert.ToSByte("14", ci), "#M15");
				iTest++;
				Assert.AreEqual ((sbyte)11, Convert.ToSByte("01011", 2), "#M16");
				iTest++;
				Assert.AreEqual ((sbyte)5, Convert.ToSByte("5", 8), "#M17");
				iTest++;
				Assert.AreEqual ((sbyte)100, Convert.ToSByte("100", 10), "#M18");
				iTest++;
				Assert.AreEqual ((sbyte)-1, Convert.ToSByte("FF", 16), "#M19");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception at iTest = " + iTest + ": e = " + e);
			}

			try {
				Convert.ToSByte((byte)200);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M25");
			}

			try {
				Convert.ToSByte((char)130);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M26");
			}

			try {
				Convert.ToSByte(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#M27");
			}

			try {
				Convert.ToSByte((decimal)127.5m);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M28");
			}

			try {
				Convert.ToSByte((decimal)-200m);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M29");
			}

			try {
				Convert.ToSByte((double)150);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M30");
			}

			try {
				Convert.ToSByte((double)-128.6);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M31");
			}

			try {
				Convert.ToSByte((short)150);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M32");
			}

			try {
				Convert.ToSByte((short)-300);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M33");
			}

			try {
				Convert.ToSByte((int)1500);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M34");
			}

			try {
				Convert.ToSByte((int)-1286);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M35");
			}

			try {
				Convert.ToSByte((long)128);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M36");
			}

			try {
				Convert.ToSByte((long)-129);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M37");
			}

			try {
				Convert.ToSByte(new NumberFormatInfo());
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#M38");
			}

			try {
				Convert.ToSByte((float)333);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M39");
			}

			try {
				Convert.ToSByte((float)-666);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M40");
			}

			try {
				Convert.ToSByte("B3");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#M41");
			}

			try {
				Convert.ToSByte("251");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M42");
			}

			try {
				Convert.ToSByte(ushort.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M43");
			}

			try {
				Convert.ToSByte((uint)600);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M44");
			}

			try {
				Convert.ToSByte(ulong.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M45");
			}

			try {
				Convert.ToSByte(ci, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#M46");
			}

			try {
				Convert.ToSByte(tryStr, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#M47");
			}
			
			try {
				Convert.ToSByte("325", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M48");
			}

			try {
				Convert.ToSByte("5D", 15);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#M49");
			}
			
			try {							
				Convert.ToSByte("111111111", 2);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#M50");
			}
		}

		[Test]
		public void TestToSingle() {
			int iTest = 1;
			try {
				Assert.AreEqual ((float)0, Convert.ToSingle(boolFalse), "#N01");
				iTest++;
				Assert.AreEqual ((float)1, Convert.ToSingle(boolTrue), "#N02");
				iTest++;
				Assert.AreEqual ((float)0, Convert.ToSingle(tryByte), "#N03");
				iTest++;
				Assert.AreEqual ((float)1234, (double)234, Convert.ToSingle(tryDec), "#N04");
				iTest++;
				Assert.AreEqual ((float)0, Convert.ToSingle(tryDbl), "#N05");
				iTest++;
				Assert.AreEqual ((float)1234, Convert.ToSingle(tryInt16), "#N06");
				iTest++;
				Assert.AreEqual ((float)12345, Convert.ToSingle(tryInt32), "#N07");
				iTest++;
				Assert.AreEqual ((float)123456789012, Convert.ToSingle(tryInt64), "#N08");
				iTest++;
				Assert.AreEqual ((float)123, Convert.ToSingle(trySByte), "#N09");
				iTest++;
				Assert.AreEqual ((float)1234, (double)2345, Convert.ToSingle(tryFloat), "#N10");
				iTest++;
				Assert.AreEqual ((float)987, Convert.ToSingle("987"), "#N11");
				iTest++;
				Assert.AreEqual ((float)34567, Convert.ToSingle(tryUI16), "#N12");
				iTest++;
				Assert.AreEqual ((float)567891234, Convert.ToSingle(tryUI32), "#N13");
				iTest++;
				Assert.AreEqual ((float)0, Convert.ToSingle(tryUI64), "#N14");
				iTest++;
				Assert.AreEqual ((float)654.234, Convert.ToSingle("654.234", ci), "#N15");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception at iTest = " + iTest + ": e = " + e);
			}

			try {
				Convert.ToSingle(tryChar);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#N25");
			}

			try {
				Convert.ToSingle(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#N26");
			}

			try {
				Convert.ToSingle(tryObj);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#N27");
			}
			
			try {
				Convert.ToSingle("A345H");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#N28");
			}
			
			try {
				Convert.ToSingle(double.MaxValue.ToString());
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#N29");
			}

			try {
				Convert.ToSingle(tryObj, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#N30");
			}

			try {
				Convert.ToSingle("J345K", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#N31");
			}

			try {
				Convert.ToSingle("11000000000000000000000000000000000000000000000", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#N32");
			}
		}

		[Test]
		public void TestToString() {
			
			tryByte = 123;
			Assert.AreEqual ("False", Convert.ToString(boolFalse), "#O01");
			Assert.AreEqual ("True", Convert.ToString(boolTrue), "#O02");
			Assert.AreEqual ("123", Convert.ToString(tryByte), "#O03");
			Assert.AreEqual ("a", Convert.ToString(tryChar), "#O04");
			Assert.AreEqual (tryDT.ToString(), Convert.ToString(tryDT), "#O05");
			Assert.AreEqual (tryDec.ToString(), Convert.ToString(tryDec), "#O06");
			Assert.AreEqual (tryDbl.ToString(), Convert.ToString(tryDbl), "#O07");
			Assert.AreEqual ("1234", Convert.ToString(tryInt16), "#O08");
			Assert.AreEqual ("12345", Convert.ToString(tryInt32), "#O09");
			Assert.AreEqual ("123456789012", Convert.ToString(tryInt64), "#O10");
			Assert.AreEqual ("123", Convert.ToString(trySByte), "#O11");
			Assert.AreEqual (tryFloat.ToString(), Convert.ToString(tryFloat), "#O12");
			Assert.AreEqual ("foobar", Convert.ToString(tryStr), "#O13");
			Assert.AreEqual ("34567", Convert.ToString(tryUI16), "#O14");
			Assert.AreEqual ("567891234", Convert.ToString(tryUI32), "#O15");
			Assert.AreEqual ("True", Convert.ToString(boolTrue, ci), "#O16");
			Assert.AreEqual ("False", Convert.ToString(boolFalse, ci), "#O17");
			Assert.AreEqual ("123", Convert.ToString(tryByte, ci), "#O18");
			Assert.AreEqual ("1111011", Convert.ToString(tryByte, 2), "#O19");
			Assert.AreEqual ("173", Convert.ToString(tryByte, 8), "#O20");
			Assert.AreEqual ("123", Convert.ToString(tryByte, 10), "#O21");
			Assert.AreEqual ("7b", Convert.ToString(tryByte, 16), "#O22");
			Assert.AreEqual ("a", Convert.ToString(tryChar, ci), "#O23");
			Assert.AreEqual (tryDT.ToString(ci), Convert.ToString(tryDT, ci), "#O24");
			Assert.AreEqual (tryDec.ToString(ci), Convert.ToString(tryDec, ci), "#O25");
			Assert.AreEqual (tryDbl.ToString(ci), Convert.ToString(tryDbl, ci), "#O26");
			Assert.AreEqual ("1234", Convert.ToString(tryInt16, ci), "#O27");
			Assert.AreEqual ("10011010010", Convert.ToString(tryInt16, 2), "#O28");
			Assert.AreEqual ("2322", Convert.ToString(tryInt16, 8), "#O29");
			Assert.AreEqual ("1234", Convert.ToString(tryInt16, 10), "#O30");
			Assert.AreEqual ("4d2", Convert.ToString(tryInt16, 16), "#O31");
			Assert.AreEqual ("12345", Convert.ToString(tryInt32, ci), "#O32");
			Assert.AreEqual ("11000000111001", Convert.ToString(tryInt32, 2), "#O33");
			Assert.AreEqual ("30071", Convert.ToString(tryInt32, 8), "#O34");
			Assert.AreEqual ("12345", Convert.ToString(tryInt32, 10), "#O35");
			Assert.AreEqual ("3039", Convert.ToString(tryInt32, 16), "#O36");
			Assert.AreEqual ("123456789012", Convert.ToString(tryInt64, ci), "#O37");
			Assert.AreEqual ("1110010111110100110010001101000010100", Convert.ToString(tryInt64, 2), "#O38");
			Assert.AreEqual ("1627646215024", Convert.ToString(tryInt64, 8), "#O39");
			Assert.AreEqual ("123456789012", Convert.ToString(tryInt64, 10), "#O40");
			Assert.AreEqual ("1cbe991a14", Convert.ToString(tryInt64, 16), "#O41");
			Assert.AreEqual ("123", Convert.ToString((trySByte), ci), "#O42");
			Assert.AreEqual (tryFloat.ToString(ci), Convert.ToString((tryFloat), ci), "#O43");
			Assert.AreEqual ("foobar", Convert.ToString((tryStr), ci), "#O44");
			Assert.AreEqual ("34567", Convert.ToString((tryUI16), ci), "#O45");
			Assert.AreEqual ("567891234", Convert.ToString((tryUI32), ci), "#O46");
			Assert.AreEqual ("0", Convert.ToString(tryUI64), "#O47");
			Assert.AreEqual ("0", Convert.ToString((tryUI64), ci), "#O48");

			try {
				Convert.ToString(tryInt16, 5);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#O55");
			}

			try {
				Convert.ToString(tryInt32, 17);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#O56");
			}

			try {
				Convert.ToString(tryInt64, 1);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#O57");
			}			
		}

		[Test]
		public void TestToUInt16() {
			Assert.AreEqual ((ushort)0, Convert.ToUInt16(boolFalse), "#P01");
			Assert.AreEqual ((ushort)1, Convert.ToUInt16(boolTrue), "#P02");
			Assert.AreEqual ((ushort)0, Convert.ToUInt16(tryByte), "#P03");
			Assert.AreEqual ((ushort)97, Convert.ToUInt16(tryChar), "#P04");
			Assert.AreEqual ((ushort)1234, Convert.ToUInt16(tryDec), "#P05");
			Assert.AreEqual ((ushort)0, Convert.ToUInt16(tryDbl), "#P06");
			Assert.AreEqual ((ushort)1234, Convert.ToUInt16(tryInt16), "#P07");
			Assert.AreEqual ((ushort)12345, Convert.ToUInt16(tryInt32), "#P08");
			Assert.AreEqual ((ushort)43752, Convert.ToUInt16((long)43752), "#P09");
			Assert.AreEqual ((ushort)123, Convert.ToUInt16(trySByte), "#P10");
			Assert.AreEqual ((ushort)1234, Convert.ToUInt16(tryFloat), "#P11");
			Assert.AreEqual ((ushort)123, Convert.ToUInt16((string)"123"), "#P12");
			Assert.AreEqual ((ushort)34567, Convert.ToUInt16(tryUI16), "#P13");
			Assert.AreEqual ((ushort)56789, Convert.ToUInt16((uint)56789), "#P14");
			Assert.AreEqual ((ushort)0, Convert.ToUInt16(tryUI64), "#P15");
			Assert.AreEqual ((ushort)31, Convert.ToUInt16("31", ci), "#P16");
			Assert.AreEqual ((ushort)14, Convert.ToUInt16("1110", 2), "#P17");
			Assert.AreEqual ((ushort)32, Convert.ToUInt16("40", 8), "#P18");
			Assert.AreEqual ((ushort)40, Convert.ToUInt16("40", 10), "#P19");
			Assert.AreEqual ((ushort)64, Convert.ToUInt16("40", 16), "#P20");


			try {
				Convert.ToUInt16(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#P25");
			}

			try {
				Convert.ToUInt16(decimal.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P26");
			}

			try {
				Convert.ToUInt16(decimal.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P27");
			}

			try {
				Convert.ToUInt16(double.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P28");
			}

			try {
				Convert.ToUInt16(double.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P29");
			}

			try {
				Convert.ToUInt16(short.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P30");
			}

			try {
				Convert.ToUInt16(int.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P31");
			}

			try {
				Convert.ToUInt16(int.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P32");
			}

			try {
				Convert.ToUInt16(long.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P33");
			}

			try {
				Convert.ToUInt16(long.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P34");
			}

			try {
				Convert.ToUInt16(tryObj);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#P35");
			}

			try {
				Convert.ToUInt16(sbyte.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P36");
			}

			try {
				Convert.ToUInt16(float.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P37");
			}

			try {
				Convert.ToUInt16(float.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P38");
			}
			
			try {
				Convert.ToUInt16("1A2");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#P39");
			}

			try {
				Convert.ToUInt16("-32800");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P40");
			}

			try {
				Convert.ToUInt16(int.MaxValue.ToString());
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P41");
			}

			try {
				Convert.ToUInt16(ulong.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P42");
			}

			try {
				Convert.ToUInt16("1A2", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#P43");
			}

			try {
				Convert.ToUInt16("-32800", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P44");
			}

			try {
				Convert.ToUInt16("456987", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#P45");
			}

			try {
				Convert.ToUInt16("40", 9);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#P46");
			}

			try {
				Convert.ToUInt16 ("abcde", 16);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#P47");
			}
		}

		[Test]
		public void TestSignedToInt() {
		  //  String cannot contain a minus sign if the base is not 10.
		  // But can if it is ten, and + is allowed everywhere.
		  Assert.AreEqual (-1, Convert.ToInt32 ("-1", 10), "Signed0");
		  Assert.AreEqual (1, Convert.ToInt32 ("+1", 10), "Signed1");
		  Assert.AreEqual (1, Convert.ToInt32 ("+1", 2), "Signed2");
		  Assert.AreEqual (1, Convert.ToInt32 ("+1", 8), "Signed3");
		  Assert.AreEqual (1, Convert.ToInt32 ("+1", 16), "Signed4");
		  
		  try {
			Convert.ToInt32("-1", 2);
			Assert.Fail ();
		  }
		  catch (Exception) {
		  }
		  try {
			Convert.ToInt32("-1", 8);
			Assert.Fail ();
		  }
		  catch (Exception) {
		  }
		  try {
			Convert.ToInt32("-1", 16);
			Assert.Fail ();
		  }
		  catch (Exception) {
		  }


		}
	
		[Test]
		public void TestToUInt32() {
			Assert.AreEqual ((uint)1, Convert.ToUInt32(boolTrue), "#Q01");
			Assert.AreEqual ((uint)0, Convert.ToUInt32(boolFalse), "#Q02");
			Assert.AreEqual ((uint)0, Convert.ToUInt32(tryByte), "#Q03");
			Assert.AreEqual ((uint)97, Convert.ToUInt32(tryChar), "#Q04");
			Assert.AreEqual ((uint)1234, Convert.ToUInt32(tryDec), "#Q05");
			Assert.AreEqual ((uint)0, Convert.ToUInt32(tryDbl), "#Q06");
			Assert.AreEqual ((uint)1234, Convert.ToUInt32(tryInt16), "#Q07");
			Assert.AreEqual ((uint)12345, Convert.ToUInt32(tryInt32), "#Q08");
			Assert.AreEqual ((uint)1234567890, Convert.ToUInt32((long)1234567890), "#Q09");
			Assert.AreEqual ((uint)123, Convert.ToUInt32(trySByte), "#Q10");
			Assert.AreEqual ((uint)1234, Convert.ToUInt32(tryFloat), "#Q11");
			Assert.AreEqual ((uint)3456789, Convert.ToUInt32("3456789"), "#Q12");
			Assert.AreEqual ((uint)34567, Convert.ToUInt32(tryUI16), "#Q13");
			Assert.AreEqual ((uint)567891234, Convert.ToUInt32(tryUI32), "#Q14");
			Assert.AreEqual ((uint)0, Convert.ToUInt32(tryUI64), "#Q15");
			Assert.AreEqual ((uint)415, Convert.ToUInt32("110011111", 2), "#Q16");
			Assert.AreEqual ((uint)156, Convert.ToUInt32("234", 8), "#Q17");
			Assert.AreEqual ((uint)234, Convert.ToUInt32("234", 10), "#Q18");
			Assert.AreEqual ((uint)564, Convert.ToUInt32("234", 16), "#Q19");
			

			try {
				Convert.ToUInt32(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#Q25");
			}

			try {
				Convert.ToUInt32(decimal.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q26");
			}

			try {
				Convert.ToUInt32((decimal)-150);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q27");
			}

			try {
				Convert.ToUInt32(double.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q28");
			}

			try {
				Convert.ToUInt32((double)-1);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q29");
			}

			try {
				Convert.ToUInt32(short.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q30");
			}

			try {
				Convert.ToUInt32(int.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q31");
			}

			try {
				Convert.ToUInt32(long.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q32");
			}

			try {
				Convert.ToUInt32((long)-50000);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q33");
			}

			try {
				Convert.ToUInt32(new Exception());
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#Q34");
			}

			try {
				Convert.ToUInt32(sbyte.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q35");
			}

			try {
				Convert.ToUInt32(float.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q36");
			}

			try {
				Convert.ToUInt32(float.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q37");
			}

			try {
				Convert.ToUInt32("45t54");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#Q38");
			}

			try {
				Convert.ToUInt32("-55");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q39");
			}

			try {
				Convert.ToUInt32(ulong.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q40");
			}

			try {
				Convert.ToUInt32(new Exception(), ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#Q41");
			}

			try {
				Convert.ToUInt32(tryStr, ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#Q42");
			}

			try {
				Convert.ToUInt32("-50", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q43");
			}

			try {
				Convert.ToUInt32(decimal.MaxValue.ToString(), ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#Q44");
			}

			try {
				Convert.ToUInt32("1001110", 1);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#Q45");
			}
		}

		[Test]
		public void TestToUInt64() 
		{
			int iTest = 1;
			try {
				Assert.AreEqual ((ulong)1, Convert.ToUInt64(boolTrue), "#R01");
				iTest++;
				Assert.AreEqual ((ulong)0, Convert.ToUInt64(boolFalse), "#R02");
				iTest++;
				Assert.AreEqual ((ulong)0, Convert.ToUInt64(tryByte), "#R03");
				iTest++;
				Assert.AreEqual ((ulong)97, Convert.ToUInt64(tryChar), "#R04");
				iTest++;
				Assert.AreEqual ((ulong)1234, Convert.ToUInt64(tryDec), "#R05");
				iTest++;
				Assert.AreEqual ((ulong)0, Convert.ToUInt64(tryDbl), "#R06");
				iTest++;
				Assert.AreEqual ((ulong)1234, Convert.ToUInt64(tryInt16), "#R07");
				iTest++;
				Assert.AreEqual ((ulong)12345, Convert.ToUInt64(tryInt32), "#R08");
				iTest++;
				Assert.AreEqual ((ulong)123456789012, Convert.ToUInt64(tryInt64), "#R09");
				iTest++;
				Assert.AreEqual ((ulong)123, Convert.ToUInt64(trySByte), "#R10");
				iTest++;
				Assert.AreEqual ((ulong)1234, Convert.ToUInt64(tryFloat), "#R11");
				iTest++;
				Assert.AreEqual ((ulong)345678, Convert.ToUInt64("345678"), "#R12");
				iTest++;
				Assert.AreEqual ((ulong)34567, Convert.ToUInt64(tryUI16), "#R13");
				iTest++;
				Assert.AreEqual ((ulong)567891234, Convert.ToUInt64(tryUI32), "#R14");
				iTest++;
				Assert.AreEqual ((ulong)0, Convert.ToUInt64(tryUI64), "#R15");
				iTest++;
				Assert.AreEqual ((ulong)123, Convert.ToUInt64("123", ci), "#R16");
				iTest++;
				Assert.AreEqual ((ulong)4, Convert.ToUInt64("100", 2), "#R17");
				iTest++;
				Assert.AreEqual ((ulong)64, Convert.ToUInt64("100", 8), "#R18");
				iTest++;
				Assert.AreEqual ((ulong)100, Convert.ToUInt64("100", 10), "#R19");
				iTest++;
				Assert.AreEqual ((ulong)256, Convert.ToUInt64("100", 16), "#R20");
			} catch (Exception e) {
				Assert.Fail ("Unexpected exception caught when iTest = " + iTest + ": e = " + e);
			}

			try {
				Convert.ToUInt64(tryDT);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#R25");
			}

			try {
				Convert.ToUInt64(decimal.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R26");
			}

			try {
				Convert.ToUInt64((decimal)-140);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R27");
			}

			try {
				Convert.ToUInt64(double.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R28");
			}

			try {
				Convert.ToUInt64((double)-1);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R29");
			}

			try {
				Convert.ToUInt64(short.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R30");
			}

			try {
				Convert.ToUInt64(int.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R31");
			}

			try {
				Convert.ToUInt64(long.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R32");
			}

			try {
				Convert.ToUInt64(tryObj);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(InvalidCastException), e.GetType(), "#R33");
			}

			try {
				Convert.ToUInt64(sbyte.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R34");
			}

			try {
				Convert.ToUInt64(float.MinValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R35");
			}

			try {
				Convert.ToUInt64(float.MaxValue);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R36");
			}

			try {
				Convert.ToUInt64("234rt78");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#R37");
			}

			try {
				Convert.ToUInt64("-68");
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R38");
			}

			try {
				Convert.ToUInt64(decimal.MaxValue.ToString());
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R39");
			}

			try {
				Convert.ToUInt64("23rd2", ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(FormatException), e.GetType(), "#R40");
			}

			try {
				Convert.ToUInt64(decimal.MinValue.ToString(), ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R41");
			}

			try {
				Convert.ToUInt64(decimal.MaxValue.ToString(), ci);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(OverflowException), e.GetType(), "#R42");
			}

			try {
				Convert.ToUInt64("132", 9);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentException), e.GetType(), "#R43");
			}


			Assert.AreEqual ((ulong) 256, Convert.ToUInt64 ("0x100", 16), "#L35");
			Assert.AreEqual ((ulong) 256, Convert.ToUInt64 ("0X100", 16), "#L36");
			Assert.AreEqual (ulong.MaxValue, Convert.ToUInt64 ("0xFFFFFFFFFFFFFFFF", 16), "#L37");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void TestInvalidBase64() {
		  // This has to be a multiple of 4 characters, otherwise you 
		  // are testing something else. Ideally one will become a byte
		  // > 128
		  //
		  // This test is designed to see what happens with invalid bytes
		  string brokenB64 = "AB~\u00a3";
		  Convert.FromBase64String(brokenB64);
		}

		[Test] // bug #5464
		[ExpectedException (typeof (FormatException))]
		public void TestInvalidBase64_Bug5464 ()
		{
			Convert.FromBase64String ("dGVzdA==DQo=");
		}

		[Test] // bug #5464
		public void TestValidBase64_Bug5464 ()
		{
			byte[] result = Convert.FromBase64String ("dGVzdA==");
			Assert.AreEqual(4, result.Length, "Array.Length expected to be 4.");
			Assert.AreEqual(116, result[0], "#A01");
			Assert.AreEqual(101, result[1], "#A02");
			Assert.AreEqual(115, result[2], "#A03");
			Assert.AreEqual(116, result[3], "#A04");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void TestInvalidBase64_TooManyPaddings ()
		{
			Convert.FromBase64String ("dGVzd===");
		}

		[Test]
		public void TestBeginWithSpaces ()
		{
			byte[] bb = new byte[] { 1, 2, 3};
			string s = Convert.ToBase64String (bb);
			byte [] b2 = Convert.FromBase64String ("     " + s);
			Assert.AreEqual (3, b2.Length, "#01");
			for (int i = 0; i < 3; i++)
				Assert.AreEqual (bb [i], b2 [i], "#0" + (i + 2));
		}
		
		[Test]
		public void TestToBase64CharArray ()
		{
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			//						   0	1	 2	  3    4	5	 6	  7
			char[] expectedCharArr = {'I', 'X', '/', '/', 'b', 'a', 'o', '2'};
			char[] result = new Char[8];
			
			Convert.ToBase64CharArray(byteArr, 0, byteArr.Length, result, 0);

			for (int i = 0; i < expectedCharArr.Length; i++) {
				Assert.AreEqual (expectedCharArr[i], result[i], "#S0" + i);
			}
		}

		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void ToBase64CharArray_InNull ()
		{
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			char[] result = new Char[8];
			Convert.ToBase64CharArray (null, 0, byteArr.Length, result, 0);
		}

		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void ToBase64CharArray_OutNull ()
		{
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			Convert.ToBase64CharArray (byteArr, 0, byteArr.Length, null, 0);
		}

		[Test]
		[ExpectedException (typeof(ArgumentOutOfRangeException))]
		public void ToBase64CharArray_OffsetInNegative ()
		{
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			char[] result = new Char[8];
			Convert.ToBase64CharArray (byteArr, -1, byteArr.Length, result, 0);
		}

		[Test]
		[ExpectedException (typeof(ArgumentOutOfRangeException))]
		public void ToBase64CharArray_LengthNegative ()
		{
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			char[] result = new Char[8];
			Convert.ToBase64CharArray (byteArr, 0, -5, result, 0);
		}

		[Test]
		[ExpectedException (typeof(ArgumentOutOfRangeException))]
		public void ToBase64CharArray_OffsetOutNegative ()
		{
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			char[] result = new Char[8];
			Convert.ToBase64CharArray (byteArr, 0, byteArr.Length, result, -2);
		}

		[Test]
		[ExpectedException (typeof(ArgumentOutOfRangeException))]
		public void ToBase64CharArray_TotalIn ()
		{
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			char[] result = new Char[8];
			Convert.ToBase64CharArray (byteArr, 4, byteArr.Length, result, 0);
		}

		[Test]
		[ExpectedException (typeof(ArgumentOutOfRangeException))]
		public void ToBase64CharArray_TotalInOverflow ()
		{
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			char[] result = new Char[8];
			Convert.ToBase64CharArray (byteArr, Int32.MaxValue, byteArr.Length, result, 0);
		}

		[Test]
		[ExpectedException (typeof(ArgumentOutOfRangeException))]
		public void ToBase64CharArray_TotalOut ()
		{
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			char[] result = new Char[8];
			Convert.ToBase64CharArray (byteArr, 0, byteArr.Length, result, 2);
		}

		[Test]
		[ExpectedException (typeof(ArgumentOutOfRangeException))]
		public void ToBase64CharArray_TotalOutOverflow ()
		{
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			char[] result = new Char[8];
			Convert.ToBase64CharArray (byteArr, 0, byteArr.Length, result, Int32.MaxValue);
		}

		[Test]
		public void TestToBase64String() {
			byte[] byteArr = {33, 127, 255, 109, 170, 54};
			string expectedStr = "IX//bao2";
			string result1;
			string result2;
			
			result1 = Convert.ToBase64String(byteArr);
			result2 = Convert.ToBase64String(byteArr, 0, byteArr.Length);

			Assert.AreEqual (expectedStr, result1, "#T01");
			Assert.AreEqual (expectedStr, result2, "#T02");

			try {
				Convert.ToBase64String(null);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentNullException), e.GetType(), "#T05");
			}
			
			try {
				Convert.ToBase64String(byteArr, -1, byteArr.Length);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentOutOfRangeException), e.GetType(), "#T06");
			}

			try {
				Convert.ToBase64String(byteArr, 0, -10);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentOutOfRangeException), e.GetType(), "#T07");
			}

			try {
				Convert.ToBase64String(byteArr, 4, byteArr.Length);
				Assert.Fail ();
			}
			catch (Exception e) {
				Assert.AreEqual (typeof(ArgumentOutOfRangeException), e.GetType(), "#T08");
			}		
		}

		[Test]
		public void ToBase64String_Bug76876 ()
		{
			byte[] bs = Convert.FromBase64String ("ZuBZ7PESb3VRXgrl/KSRJd/hTGBvaEvEplqH3izPomDv5nBjS9MzcD1h8tOWzS7/wYGnaip8\nbhBfCrpWxivi8G7R08oBcADIiclpZeqRxai9kG4WoBUzJ6MCbxuvb1k757q+D9nqoL0p9Rer\n+5+vNodYkHYwqnKKyMBSQ11sspw=\n");
			string s = Convert.ToBase64String (bs, Base64FormattingOptions.None);
			Assert.IsTrue (!s.Contains ("\n"), "no new line");
		}

		static string ToBase64 (int len, Base64FormattingOptions options)
		{
			return Convert.ToBase64String (new byte [len], options);
		}

		[Test]
		public void Base64String_LineEnds_InsertLineBreaks ()
		{
			string base64 = ToBase64 (0, Base64FormattingOptions.InsertLineBreaks);
			Assert.IsFalse (base64.EndsWith (Environment.NewLine), "0-le");
			Assert.AreEqual (String.Empty, base64, "0");

			base64 = ToBase64 (1, Base64FormattingOptions.InsertLineBreaks);
			Assert.IsFalse (base64.EndsWith (Environment.NewLine), "1-le");
			Assert.AreEqual ("AA==", base64, "1");

			base64 = ToBase64 (57, Base64FormattingOptions.InsertLineBreaks);
			Assert.IsFalse (base64.Contains (Environment.NewLine), "57-nl"); // one lines
			Assert.IsFalse (base64.EndsWith (Environment.NewLine), "57-le");
			Assert.AreEqual ("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", base64, "55");

			base64 = ToBase64 (58, Base64FormattingOptions.InsertLineBreaks);
			Assert.IsTrue (base64.Contains (Environment.NewLine), "58-nl"); // two lines
			Assert.IsTrue (base64.EndsWith ("AA=="), "58-le"); // no NewLine
		}

		[Test]
		public void Base64String_LineEnds_None ()
		{
			string base64 = ToBase64 (0, Base64FormattingOptions.None);
			Assert.IsFalse (base64.EndsWith (Environment.NewLine), "0-le");
			Assert.AreEqual (String.Empty, base64, "0");

			base64 = ToBase64 (1, Base64FormattingOptions.None);
			Assert.IsFalse (base64.EndsWith (Environment.NewLine), "1-le");
			Assert.AreEqual ("AA==", base64, "1");

			base64 = ToBase64 (57, Base64FormattingOptions.None);
			Assert.IsFalse (base64.Contains (Environment.NewLine), "57-nl"); // one lines
			Assert.IsFalse (base64.EndsWith (Environment.NewLine), "57-le");
			Assert.AreEqual ("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", base64, "55");

			base64 = ToBase64 (58, Base64FormattingOptions.None);
			Assert.IsFalse (base64.Contains (Environment.NewLine), "58-nl"); // one lines
			Assert.IsTrue (base64.EndsWith ("AA=="), "58-le"); // no NewLine
		}

		/* Have experienced some problems with FromBase64CharArray using mono. Something 
		 * about error in a unicode file.
		 *
		 * However the test seems to run fine using mono in a cygwin environment
		 */

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromBase64CharArray_Null ()
		{
			Convert.FromBase64CharArray (null, 1, 5);
		}

		[Test]
		public void FromBase64CharArray_Empty ()
		{
			Assert.AreEqual (new byte [0], Convert.FromBase64CharArray (new char[0], 0, 0));
		}

		[Test]
		public void FormatBase64CharArray_OnlyWhitespace ()
		{
			Assert.AreEqual (new byte [0], Convert.FromBase64CharArray (new char[3] {' ', 
				'\r', '\t'}, 0, 3));
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void FromBase64CharArray_OutOfRangeStart () 
		{
			Convert.FromBase64CharArray (new char [4], -1, 4);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void FromBase64CharArray_OutOfRangeLength () 
		{
			Convert.FromBase64CharArray (new char [4], 2, 4);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void FromBase64CharArray_Overflow () 
		{
			Convert.FromBase64CharArray (new char [4], Int32.MaxValue, 4);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FromBase64CharArray_InvalidLength () 
		{
			Convert.FromBase64CharArray (new char [4], 0, 3);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FromBase64CharArray_WideChar () 
		{
			char[] c = new char [4] { 'A', 'A', 'A', (char) Char.MaxValue };
			Convert.FromBase64CharArray (c, 0, 4);
		}

		[Test]
		public void FromBase64CharArray ()
		{
			char[] charArr = {'M','o','n','o','m','o','n','o'};
			byte[] expectedByteArr = {50, 137, 232, 154, 137, 232};
			
			byte[] fromCharArr = Convert.FromBase64CharArray(charArr, 0, 8);			

			for (int i = 0; i < fromCharArr.Length; i++){
				Assert.AreEqual (expectedByteArr[i], fromCharArr[i], "#U0" + i);
			}
		}

		/* Have experienced some problems with FromBase64String using mono. Something about 
		 * error in a unicode file.
		 *
		 * However the test seems to run fine using mono in a cygwin environment
		 */

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromBase64String_Null () 
		{
			Convert.FromBase64String (null);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FromBase64String_InvalidLength () 
		{
			Convert.FromBase64String ("foo");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FromBase64String_InvalidLength2 () 
		{
			Convert.FromBase64String (tryStr);
		}

		[Test]
		public void FromBase64String_InvalidLengthBecauseOfIgnoredChars () 
		{
			byte[] result = Convert.FromBase64String ("AAAA\t");
			Assert.AreEqual (3, result.Length, "InvalidLengthBecauseOfIgnoredChars");
		}

		private const string ignored = "\t\r\n ";
		private const string base64data = "AAAAAAAAAAAAAAAAAAAA"; // 15 bytes 0x00

		[Test]
		public void FromBase64_IgnoreCharsBefore ()
		{
			string s = ignored + base64data;
			byte[] data = Convert.FromBase64String (s);
			Assert.AreEqual (15, data.Length, "String-IgnoreCharsBefore-Ignored");

			char[] c = s.ToCharArray ();
			data = Convert.FromBase64CharArray (c, 0, c.Length);
			Assert.AreEqual (15, data.Length, "CharArray-IgnoreCharsBefore-Ignored");
		}

		[Test]
		public void FromBase64_IgnoreCharsInside () 
		{
			string s = base64data + ignored + base64data;
			byte[] data = Convert.FromBase64String (s);
			Assert.AreEqual (30, data.Length, "String-IgnoreCharsInside-Ignored");

			char[] c = s.ToCharArray ();
			data = Convert.FromBase64CharArray (c, 0, c.Length);
			Assert.AreEqual (30, data.Length, "CharArray-IgnoreCharsInside-Ignored");
		}

		[Test]
		public void FromBase64_IgnoreCharsAfter () 
		{
			string s = base64data + ignored;
			byte[] data = Convert.FromBase64String (s);
			Assert.AreEqual (15, data.Length, "String-IgnoreCharsAfter-Ignored");

			char[] c = s.ToCharArray ();
			data = Convert.FromBase64CharArray (c, 0, c.Length);
			Assert.AreEqual (15, data.Length, "CharArray-IgnoreCharsAfter-Ignored");
		}

		[Test]
		public void FromBase64_Empty ()
		{
			Assert.AreEqual (new byte[0], Convert.FromBase64String (string.Empty));
		}

		[Test]
		public void FromBase64_OnlyWhiteSpace ()
		{
			Assert.AreEqual (new byte[0], Convert.FromBase64String ("  \r\t"));
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FromBase64_InvalidChar ()
		{
			Convert.FromBase64String ("amVsb3U=\u0100");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FromBase64_Min ()
		{
			Convert.FromBase64String ("amVsb3U=   \r \n\u007B");
		}

		[Test]
		public void FromBase64_TrailingEqualAndSpaces () // From bug #75840.
		{
			string base64 = "\n     fdy6S2NLpnT4fMdokUHSHsmpcvo=    ";
			byte [] bytes = Convert.FromBase64String (base64);
			Assert.AreEqual (20, bytes.Length, "#01");
			byte [] target = new byte [] { 0x7D, 0xDC, 0xBA, 0x4B, 0x63, 0x4B, 0xA6, 0x74, 0xF8, 0x7C, 0xC7,
							0x68, 0x91, 0x41, 0xD2, 0x1E, 0xC9, 0xA9, 0x72, 0xFA };

			for (int i = 0; i < 20; i++) {
				if (bytes [i] != target [i])
					Assert.Fail ("Item #" + i);
			}
		}

		[Test]
		public void TestConvertFromNull() {
			
			Assert.AreEqual (false, Convert.ToBoolean (null as object), "#W1");
			Assert.AreEqual (0, Convert.ToByte (null as object), "#W2");
			Assert.AreEqual (0, Convert.ToChar (null as object), "#W3");
			Assert.AreEqual (new DateTime (1, 1, 1, 0, 0, 0), Convert.ToDateTime (null as object), "#W4");
			Assert.AreEqual (0, Convert.ToDecimal (null as object), "#W5");
			Assert.AreEqual (0, Convert.ToDouble (null as object), "#W6");
			Assert.AreEqual (0, Convert.ToInt16 (null as object), "#W7");
			Assert.AreEqual (0, Convert.ToInt32 (null as object), "#W8");
			Assert.AreEqual (0, Convert.ToInt64 (null as object), "#W9");
			Assert.AreEqual (0, Convert.ToSByte (null as object), "#W10");
			Assert.AreEqual (0, Convert.ToSingle (null as object), "#W11");
			Assert.AreEqual ("", Convert.ToString (null as object), "#W12");
			Assert.AreEqual (0, Convert.ToUInt16 (null as object), "#W13");
			Assert.AreEqual (0, Convert.ToUInt32 (null as object), "#W14");
			Assert.AreEqual (0, Convert.ToUInt64 (null as object), "#W15");
			Assert.AreEqual (false, Convert.ToBoolean (null as string), "#W16");
			Assert.AreEqual (0, Convert.ToByte (null as string), "#W17");

			try {
				Convert.ToChar (null as string);
				Assert.Fail ();
			} catch (Exception e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "#W18");
			}
			
			Assert.AreEqual (new DateTime (1, 1, 1, 0, 0, 0), Convert.ToDateTime (null as string), "#W19");
			Assert.AreEqual (0, Convert.ToDecimal (null as string), "#W20");
			Assert.AreEqual (0, Convert.ToDouble (null as string), "#W21");
			Assert.AreEqual (0, Convert.ToInt16 (null as string), "#W22");
			Assert.AreEqual (0, Convert.ToInt32 (null as string), "#W23");
			Assert.AreEqual (0, Convert.ToInt64 (null as string), "#W24");
			Assert.AreEqual (0, Convert.ToSByte (null as string), "#W25");
			Assert.AreEqual (0, Convert.ToSingle (null as string), "#W26");
			Assert.AreEqual (null, Convert.ToString (null as string), "#W27");
			Assert.AreEqual (0, Convert.ToUInt16 (null as string), "#W28");
			Assert.AreEqual (0, Convert.ToUInt32 (null as string), "#W29");
			Assert.AreEqual (0, Convert.ToUInt64 (null as string), "#W30");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FromBase64StringInvalidFormat ()
		{
			Convert.FromBase64String ("Tgtm+DBN====");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void FromBase64StringInvalidFormat2 ()
		{
			Convert.FromBase64String ("Tgtm+DBN========");
		}

		[Test]
		public void ToByte_PrefixedHexStringInBase16 () 
		{
			Assert.AreEqual (255, Convert.ToByte ("0xff", 16), "0xff");
			Assert.AreEqual (255, Convert.ToByte ("0xfF", 16), "0xfF");
			Assert.AreEqual (255, Convert.ToByte ("0xFf", 16), "0xFf");
			Assert.AreEqual (255, Convert.ToByte ("0xFF", 16), "0xFF");

			Assert.AreEqual (255, Convert.ToByte ("0Xff", 16), "0Xff");
			Assert.AreEqual (255, Convert.ToByte ("0XfF", 16), "0XfF");
			Assert.AreEqual (255, Convert.ToByte ("0XFf", 16), "0XFf");
			Assert.AreEqual (255, Convert.ToByte ("0XFF", 16), "0XFF");

			Assert.AreEqual (Byte.MinValue, Convert.ToByte ("0x0", 16), "0x0");
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToByte_NegativeString () 
		{
			Convert.ToByte ("-1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToByte_NegativeStringNonBase10 () 
		{
			Convert.ToByte ("-0", 16);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToByte_NegativeString_Base10 ()
		{
			Convert.ToByte ("-0", 10);
		}

		[Test]
		public void ToByte_NegativeZeroString () 
		{
			Convert.ToByte ("-0");
			Convert.ToByte ("-0", null);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt16_NegativeString () 
		{
			Convert.ToUInt16 ("-1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt16_NegativeStringNonBase10 () 
		{
			Convert.ToUInt16 ("-0", 16);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt16_NegativeString_Base10 ()
		{
			Convert.ToUInt16 ("-0", 10);
		}

		[Test]
		public void ToUInt16_NegativeZeroString () 
		{
			Convert.ToUInt16 ("-0");
			Convert.ToUInt16 ("-0", null);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt32_NegativeString () 
		{
			Convert.ToUInt32 ("-1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt32_NegativeStringNonBase10 () 
		{
			Convert.ToUInt32 ("-0", 16);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt32_NegativeString_Base10 ()
		{
			Convert.ToUInt32 ("-0", 10);
		}

		[Test]
		public void ToUInt32_NegativeZeroString () 
		{
			Convert.ToUInt32 ("-0");
			Convert.ToUInt32 ("-0", null);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt64_NegativeString () 
		{
			Convert.ToUInt64 ("-1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt64_NegativeStringNonBase10 () 
		{
			Convert.ToUInt64 ("-0", 16);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt64_NegativeString_Base10 ()
		{
			Convert.ToUInt64 ("-0", 10);
		}

		[Test]
		public void ToUInt64_NegativeZeroString () 
		{
			Convert.ToUInt64 ("-0");
			Convert.ToUInt64 ("-0", null);
		}

		// min/max unsigned

		[Test]
		public void ToByte_MaxValue ()
		{
			Assert.AreEqual (Byte.MaxValue, Convert.ToByte ("ff", 16), "ff,16");
			Assert.AreEqual (Byte.MaxValue, Convert.ToByte ("0XFF", 16), "0xFF,16");
			Assert.AreEqual (Byte.MaxValue, Convert.ToByte ("255", 10), "255,10");
			Assert.AreEqual (Byte.MaxValue, Convert.ToByte ("377", 8), "377,8");
			Assert.AreEqual (Byte.MaxValue, Convert.ToByte ("11111111", 2), "11111111,2");
		}

		[Test]
		public void ToByte_MinValue ()
		{
			Assert.AreEqual (Byte.MinValue, Convert.ToByte ("0", 16), "0,16");
			Assert.AreEqual (Byte.MinValue, Convert.ToByte ("0x0", 16), "0x0,16");
			Assert.AreEqual (Byte.MinValue, Convert.ToByte ("0", 10), "0,10");
			Assert.AreEqual (Byte.MinValue, Convert.ToByte ("0", 8), "0,8");
			Assert.AreEqual (Byte.MinValue, Convert.ToByte ("0", 2), "0,2");
		}

		[Test]
		public void ToUInt16_MaxValue ()
		{
			Assert.AreEqual (UInt16.MaxValue, Convert.ToUInt16 ("ffff", 16), "ffff,16");
			Assert.AreEqual (UInt16.MaxValue, Convert.ToUInt16 ("0XFFFF", 16), "0XFFFF,16");
			Assert.AreEqual (UInt16.MaxValue, Convert.ToUInt16 ("65535", 10), "65535,10");
			Assert.AreEqual (UInt16.MaxValue, Convert.ToUInt16 ("177777", 8), "177777,8");
			Assert.AreEqual (UInt16.MaxValue, Convert.ToUInt16 ("1111111111111111", 2), "1111111111111111,2");
		}

		[Test]
		public void ToUInt16_MinValue ()
		{
			Assert.AreEqual (UInt16.MinValue, Convert.ToUInt16 ("0", 16), "0,16");
			Assert.AreEqual (UInt16.MinValue, Convert.ToUInt16 ("0x0", 16), "0x0,16");
			Assert.AreEqual (UInt16.MinValue, Convert.ToUInt16 ("0", 10), "0,10");
			Assert.AreEqual (UInt16.MinValue, Convert.ToUInt16 ("0", 8), "0,8");
			Assert.AreEqual (UInt16.MinValue, Convert.ToUInt16 ("0", 2), "0,2");
		}

		[Test]
		public void ToUInt32_MaxValue ()
		{
			Assert.AreEqual (UInt32.MaxValue, Convert.ToUInt32 ("ffffffff", 16), "ffffffff,16");
			Assert.AreEqual (UInt32.MaxValue, Convert.ToUInt32 ("0XFFFFFFFF", 16), "0XFFFFFFFF,16");
			Assert.AreEqual (UInt32.MaxValue, Convert.ToUInt32 ("4294967295", 10), "4294967295,10");
			Assert.AreEqual (UInt32.MaxValue, Convert.ToUInt32 ("37777777777", 8), "37777777777,8");
			Assert.AreEqual (UInt32.MaxValue, Convert.ToUInt32 ("11111111111111111111111111111111", 2), "11111111111111111111111111111111,2");
		}

		[Test]
		public void ToUInt32_MinValue ()
		{
			Assert.AreEqual (UInt32.MinValue, Convert.ToUInt32 ("0", 16), "0,16");
			Assert.AreEqual (UInt32.MinValue, Convert.ToUInt32 ("0x0", 16), "0x0,16");
			Assert.AreEqual (UInt32.MinValue, Convert.ToUInt32 ("0", 10), "0,10");
			Assert.AreEqual (UInt32.MinValue, Convert.ToUInt32 ("0", 8), "0,8");
			Assert.AreEqual (UInt32.MinValue, Convert.ToUInt32 ("0", 2), "0,2");
		}

		[Test]
		public void ToUInt64_MaxValue ()
		{
			Assert.AreEqual (UInt64.MaxValue, Convert.ToUInt64 ("ffffffffffffffff", 16), "ffffffffffffffff,16");
			Assert.AreEqual (UInt64.MaxValue, Convert.ToUInt64 ("0XFFFFFFFFFFFFFFFF", 16), "0XFFFFFFFFFFFFFFFF,16");
			Assert.AreEqual (UInt64.MaxValue, Convert.ToUInt64 ("18446744073709551615", 10), "18446744073709551615,10");
			Assert.AreEqual (UInt64.MaxValue, Convert.ToUInt64 ("1777777777777777777777", 8), "1777777777777777777777,8");
			Assert.AreEqual (UInt64.MaxValue, Convert.ToUInt64 ("1111111111111111111111111111111111111111111111111111111111111111", 2), "1111111111111111111111111111111111111111111111111111111111111111,2");
		}

		[Test]
		public void ToUInt64_MinValue ()
		{
			Assert.AreEqual (UInt64.MinValue, Convert.ToUInt64 ("0", 16), "0,16");
			Assert.AreEqual (UInt64.MinValue, Convert.ToUInt64 ("0x0", 16), "0x0,16");
			Assert.AreEqual (UInt64.MinValue, Convert.ToUInt64 ("0", 10), "0,10");
			Assert.AreEqual (UInt64.MinValue, Convert.ToUInt64 ("0", 8), "0,8");
			Assert.AreEqual (UInt64.MinValue, Convert.ToUInt64 ("0", 2), "0,2");
		}

		// min/max signed

		[Test]
		public void ToSByte_MaxValue ()
		{
			Assert.AreEqual (SByte.MaxValue, Convert.ToSByte ("7f", 16), "7F,16");
			Assert.AreEqual (SByte.MaxValue, Convert.ToSByte ("0X7F", 16), "0X7F,16");
			Assert.AreEqual (SByte.MaxValue, Convert.ToSByte ("127", 10), "127,10");
			Assert.AreEqual (SByte.MaxValue, Convert.ToSByte ("177", 8), "177,8");
			Assert.AreEqual (SByte.MaxValue, Convert.ToSByte ("1111111", 2), "1111111,2");
		}

		[Test]
		public void ToSByte_MinValue ()
		{
			Assert.AreEqual (SByte.MinValue, Convert.ToSByte ("80", 16), "80,16");
			Assert.AreEqual (SByte.MinValue, Convert.ToSByte ("-128", 10), "-128,10");
			Assert.AreEqual (SByte.MinValue, Convert.ToSByte ("200", 8), "200,8");
			Assert.AreEqual (SByte.MinValue, Convert.ToSByte ("10000000", 2), "10000000,2");
		}

		[Test]
		public void ToInt16_MaxValue ()
		{
			Assert.AreEqual (Int16.MaxValue, Convert.ToInt16 ("7fff", 16), "7FFF,16");
			Assert.AreEqual (Int16.MaxValue, Convert.ToInt16 ("0X7FFF", 16), "0X7FFF,16");
			Assert.AreEqual (Int16.MaxValue, Convert.ToInt16 ("32767", 10), "32767,10");
			Assert.AreEqual (Int16.MaxValue, Convert.ToInt16 ("77777", 8), "77777,8");
			Assert.AreEqual (Int16.MaxValue, Convert.ToInt16 ("111111111111111", 2), "111111111111111,2");
		}

		[Test]
		public void ToInt16_MinValue ()
		{
			Assert.AreEqual (Int16.MinValue, Convert.ToInt16 ("8000", 16), "8000,16");
			Assert.AreEqual (Int16.MinValue, Convert.ToInt16 ("-32768", 10), "-32768,10");
			Assert.AreEqual (Int16.MinValue, Convert.ToInt16 ("100000", 8), "100000,8");
			Assert.AreEqual (Int16.MinValue, Convert.ToInt16 ("1000000000000000", 2), "1000000000000000,2");
		}

		[Test]
		public void ToInt32_MaxValue ()
		{
			Assert.AreEqual (Int32.MaxValue, Convert.ToInt32 ("7fffffff", 16), "7fffffff,16");
			Assert.AreEqual (Int32.MaxValue, Convert.ToInt32 ("0X7FFFFFFF", 16), "0X7FFFFFFF,16");
			Assert.AreEqual (Int32.MaxValue, Convert.ToInt32 ("2147483647", 10), "2147483647,10");
			Assert.AreEqual (Int32.MaxValue, Convert.ToInt32 ("17777777777", 8), "17777777777,8");
			Assert.AreEqual (Int32.MaxValue, Convert.ToInt32 ("1111111111111111111111111111111", 2), "1111111111111111111111111111111,2");
		}

		[Test]
		public void ToInt32_MinValue ()
		{
			Assert.AreEqual (Int32.MinValue, Convert.ToInt32 ("80000000", 16), "80000000,16");
			Assert.AreEqual (Int32.MinValue, Convert.ToInt32 ("-2147483648", 10), "-2147483648,10");
			Assert.AreEqual (Int32.MinValue, Convert.ToInt32 ("20000000000", 8), "20000000000,8");
			Assert.AreEqual (Int32.MinValue, Convert.ToInt32 ("10000000000000000000000000000000", 2), "10000000000000000000000000000000,2");
		}

		[Test]
		public void ToInt64_MaxValue ()
		{
			Assert.AreEqual (Int64.MaxValue, Convert.ToInt64 ("7fffffffffffffff", 16), "7fffffffffffffff,16");
			Assert.AreEqual (Int64.MaxValue, Convert.ToInt64 ("0X7FFFFFFFFFFFFFFF", 16), "0X7FFFFFFFFFFFFFFF,16");
			Assert.AreEqual (Int64.MaxValue, Convert.ToInt64 ("9223372036854775807", 10), "9223372036854775807,10");
			Assert.AreEqual (Int64.MaxValue, Convert.ToInt64 ("777777777777777777777", 8), "777777777777777777777,8");
			Assert.AreEqual (Int64.MaxValue, Convert.ToInt64 ("111111111111111111111111111111111111111111111111111111111111111", 2), "111111111111111111111111111111111111111111111111111111111111111,2");
		}

		[Test]
		public void ToInt64_MinValue ()
		{
			Assert.AreEqual (Int64.MinValue, Convert.ToInt64 ("8000000000000000", 16), "8000000000000000,16");
			Assert.AreEqual (Int64.MinValue, Convert.ToInt64 ("0x8000000000000000", 16), "0x8000000000000000,16");
			Assert.AreEqual (Int64.MinValue, Convert.ToInt64 ("-9223372036854775808", 10), "-9223372036854775808,10");
			Assert.AreEqual (Int64.MinValue, Convert.ToInt64 ("1000000000000000000000", 8), "1000000000000000000000,8");
			Assert.AreEqual (Int64.MinValue, Convert.ToInt64 ("1000000000000000000000000000000000000000000000000000000000000000", 2), "1000000000000000000000000000000000000000000000000000000000000000,2");
		}

		[Test]
		public void MoreOverflows ()
		{
			try {
				Convert.ToInt16 ("ffff7fff", 16);
				Assert.Fail ("#1");
			} catch (OverflowException) {
			}

			try {
				Convert.ToSByte ("ffff7fff", 16);
				Assert.Fail ("#2");
			} catch (OverflowException) {
			}

			try {
				Convert.ToUInt32 ("4294967298", 10);
				Assert.Fail ("#3");
			} catch (OverflowException) {
			}
		}

		// signed types

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToSByte_OverMaxValue ()
		{
			string max_plus1 = "128";
			Convert.ToSByte (max_plus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToSByte_OverMinValue ()
		{
			string min_minus1 = "-129";
			Convert.ToSByte (min_minus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt16_OverMaxValue ()
		{
			string max_plus1 = "32768";
			Convert.ToInt16 (max_plus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt16_OverMinValue ()
		{
			string min_minus1 = "-32769";
			Convert.ToInt16 (min_minus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt32_OverMaxValue ()
		{
			string max_plus1 = "2147483648";
			Convert.ToInt32 (max_plus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt32_OverMinValue ()
		{
			string min_minus1 = "-2147483649";
			Convert.ToInt32 (min_minus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt64_OverMaxValue ()
		{
			string max_plus1 = "9223372036854775808";
			Convert.ToInt64 (max_plus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt64_OverMinValue ()
		{
			string min_minus1 = "-9223372036854775809";
			Convert.ToInt64 (min_minus1);
		}

		// unsigned types

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToByte_OverMaxValue ()
		{
			string max_plus1 = "257";
			Convert.ToByte (max_plus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToByte_OverMinValue ()
		{
			string min_minus1 = "-1";
			Convert.ToByte (min_minus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt16_OverMaxValue ()
		{
			string max_plus1 = "65536";
			Convert.ToUInt16 (max_plus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt16_OverMinValue ()
		{
			string min_minus1 = "-1";
			Convert.ToUInt16 (min_minus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt32_OverMaxValue ()
		{
			string max_plus1 = "4294967296";
			Convert.ToUInt32 (max_plus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt32_OverMinValue ()
		{
			string min_minus1 = "-1";
			Convert.ToUInt32 (min_minus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt64_OverMaxValue ()
		{
			string max_plus1 = "18446744073709551616";
			Convert.ToUInt64 (max_plus1);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt64_OverMinValue ()
		{
			string min_minus1 = "-1";
			Convert.ToUInt64 (min_minus1);
		}

		[Test]
		public void To_NullString () 
		{
			string s = null;
			// signed
			Assert.AreEqual (0, Convert.ToSByte (s), "ToSByte");
			Assert.AreEqual (0, Convert.ToSByte (s, 10), "ToSByte+base");
			Assert.AreEqual (0, Convert.ToInt16 (s), "ToInt16");
			Assert.AreEqual (0, Convert.ToInt16 (s, 10), "ToInt16+base");
			Assert.AreEqual (0, Convert.ToInt32 (s), "ToInt32");
			Assert.AreEqual (0, Convert.ToInt32 (s, 10), "ToInt32+base");
			Assert.AreEqual (0, Convert.ToInt64 (s), "ToInt64");
			Assert.AreEqual (0, Convert.ToInt64 (s, 10), "ToInt64+base");
			// unsigned
			Assert.AreEqual (0, Convert.ToByte (s), "ToByte");
			Assert.AreEqual (0, Convert.ToByte (s, 10), "ToByte+base");
			Assert.AreEqual (0, Convert.ToUInt16 (s), "ToUInt16");
			Assert.AreEqual (0, Convert.ToUInt16 (s, 10), "ToUInt16+base");
			Assert.AreEqual (0, Convert.ToUInt32 (s), "ToUInt32");
			Assert.AreEqual (0, Convert.ToUInt32 (s, 10), "ToUInt32+base");
			Assert.AreEqual (0, Convert.ToUInt64 (s), "ToUInt64");
			Assert.AreEqual (0, Convert.ToUInt64 (s, 10), "ToUInt64+base");
		}

		[Test]
		public void To_NullObject () 
		{
			object o = null;
			// signed
			Assert.AreEqual (0, Convert.ToSByte (o), "ToSByte");
			Assert.AreEqual (0, Convert.ToInt16 (o), "ToInt16");
			Assert.AreEqual (0, Convert.ToInt32 (o), "ToInt32");
			Assert.AreEqual (0, Convert.ToInt64 (o), "ToInt64");
			// unsigned
			Assert.AreEqual (0, Convert.ToByte (o), "ToByte");
			Assert.AreEqual (0, Convert.ToUInt16 (o), "ToUInt16");
			Assert.AreEqual (0, Convert.ToUInt32 (o), "ToUInt32");
			Assert.AreEqual (0, Convert.ToUInt64 (o), "ToUInt64");
		}

		[Test]
		public void To_NullObjectFormatProvider () 
		{
			object o = null;
			IFormatProvider fp = (IFormatProvider) new NumberFormatInfo ();
			// signed
			Assert.AreEqual (0, Convert.ToSByte (o, fp), "ToSByte");
			Assert.AreEqual (0, Convert.ToInt16 (o, fp), "ToInt16");
			Assert.AreEqual (0, Convert.ToInt32 (o, fp), "ToInt32");
			Assert.AreEqual (0, Convert.ToInt64 (o, fp), "ToInt64");
			// unsigned
			Assert.AreEqual (0, Convert.ToByte (o, fp), "ToByte");
			Assert.AreEqual (0, Convert.ToUInt16 (o, fp), "ToUInt16");
			Assert.AreEqual (0, Convert.ToUInt32 (o, fp), "ToUInt32");
			Assert.AreEqual (0, Convert.ToUInt64 (o, fp), "ToUInt64");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ToSByte_NullStringFormatProvider () 
		{
			string s = null;
			// SByte is a "special" case ???
			Convert.ToSByte (s, new NumberFormatInfo ());
		}

		[Test]
		public void To_NullStringFormatProvider () 
		{
			string s = null;
			IFormatProvider fp = (IFormatProvider) new NumberFormatInfo ();
			// signed
			// No SByte here
			Assert.AreEqual (0, Convert.ToInt16 (s, fp), "ToInt16");
			Assert.AreEqual (0, Convert.ToInt32 (s, fp), "ToInt32");
			Assert.AreEqual (0, Convert.ToInt64 (s, fp), "ToInt64");
			// unsigned
			Assert.AreEqual (0, Convert.ToByte (s, fp), "ToByte");
			Assert.AreEqual (0, Convert.ToUInt16 (s, fp), "ToUInt16");
			Assert.AreEqual (0, Convert.ToUInt32 (s, fp), "ToUInt32");
			Assert.AreEqual (0, Convert.ToUInt64 (s, fp), "ToUInt64");
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ChangeTypeToTypeCodeEmpty ()
		{
			Convert.ChangeType (true, TypeCode.Empty);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CharChangeTypeNullNull ()
		{
			char a = 'a';
			IConvertible convert = a;
			convert.ToType (null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void StringChangeTypeNullNull ()
		{
			string a = "a";
			IConvertible convert = a;
			convert.ToType (null, null);
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ChangeTypeNullToValuetype ()
		{
			Convert.ChangeType (null, typeof (int));
		}

		[Test]
		public void ToString_MinMax_WithBase () 
		{
			Assert.AreEqual ("0", Convert.ToString (Byte.MinValue, 2), "Byte.MinValue base 2");
			Assert.AreEqual ("0", Convert.ToString (Byte.MinValue, 8), "Byte.MinValue base 8");
			Assert.AreEqual ("0", Convert.ToString (Byte.MinValue, 10), "Byte.MinValue base 10");
			Assert.AreEqual ("0", Convert.ToString (Byte.MinValue, 16), "Byte.MinValue base 16");

			Assert.AreEqual ("11111111", Convert.ToString (Byte.MaxValue, 2), "Byte.MaxValue base 2");
			Assert.AreEqual ("377", Convert.ToString (Byte.MaxValue, 8), "Byte.MaxValue base 8");
			Assert.AreEqual ("255", Convert.ToString (Byte.MaxValue, 10), "Byte.MaxValue base 10");
			Assert.AreEqual ("ff", Convert.ToString (Byte.MaxValue, 16), "Byte.MaxValue base 16");

			Assert.AreEqual ("1000000000000000", Convert.ToString (Int16.MinValue, 2), "Int16.MinValue base 2");
			Assert.AreEqual ("100000", Convert.ToString (Int16.MinValue, 8), "Int16.MinValue base 8");
			Assert.AreEqual ("-32768", Convert.ToString (Int16.MinValue, 10), "Int16.MinValue base 10");
			Assert.AreEqual ("8000", Convert.ToString (Int16.MinValue, 16), "Int16.MinValue base 16");

			Assert.AreEqual ("111111111111111", Convert.ToString (Int16.MaxValue, 2), "Int16.MaxValue base 2");
			Assert.AreEqual ("77777", Convert.ToString (Int16.MaxValue, 8), "Int16.MaxValue base 8");
			Assert.AreEqual ("32767", Convert.ToString (Int16.MaxValue, 10), "Int16.MaxValue base 10");
			Assert.AreEqual ("7fff", Convert.ToString (Int16.MaxValue, 16), "Int16.MaxValue base 16");

			Assert.AreEqual ("10000000000000000000000000000000", Convert.ToString (Int32.MinValue, 2), "Int32.MinValue base 2");
			Assert.AreEqual ("20000000000", Convert.ToString (Int32.MinValue, 8), "Int32.MinValue base 8");
			Assert.AreEqual ("-2147483648", Convert.ToString (Int32.MinValue, 10), "Int32.MinValue base 10");
			Assert.AreEqual ("80000000", Convert.ToString (Int32.MinValue, 16), "Int32.MinValue base 16");

			Assert.AreEqual ("1111111111111111111111111111111", Convert.ToString (Int32.MaxValue, 2), "Int32.MaxValue base 2");
			Assert.AreEqual ("17777777777", Convert.ToString (Int32.MaxValue, 8), "Int32.MaxValue base 8");
			Assert.AreEqual ("2147483647", Convert.ToString (Int32.MaxValue, 10), "Int32.MaxValue base 10");
			Assert.AreEqual ("7fffffff", Convert.ToString (Int32.MaxValue, 16), "Int32.MaxValue base 16");

			Assert.AreEqual ("1000000000000000000000000000000000000000000000000000000000000000", Convert.ToString (Int64.MinValue, 2), "Int64.MinValue base 2");
			Assert.AreEqual ("1000000000000000000000", Convert.ToString (Int64.MinValue, 8), "Int64.MinValue base 8");
			Assert.AreEqual ("-9223372036854775808", Convert.ToString (Int64.MinValue, 10), "Int64.MinValue base 10");
			Assert.AreEqual ("8000000000000000", Convert.ToString (Int64.MinValue, 16), "Int64.MinValue base 16");

			Assert.AreEqual ("111111111111111111111111111111111111111111111111111111111111111", Convert.ToString (Int64.MaxValue, 2), "Int64.MaxValue base 2");
			Assert.AreEqual ("777777777777777777777", Convert.ToString (Int64.MaxValue, 8), "Int64.MaxValue base 8");
			Assert.AreEqual ("9223372036854775807", Convert.ToString (Int64.MaxValue, 10), "Int64.MaxValue base 10");
			Assert.AreEqual ("7fffffffffffffff", Convert.ToString (Int64.MaxValue, 16), "Int64.MaxValue base 16");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToByte_BadHexPrefix1 ()
		{
			Convert.ToByte ("#10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToByte_BadHexPrefix2 ()
		{
			Convert.ToByte ("&H10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToByte_BadHexPrefix3 ()
		{
			Convert.ToByte ("&h10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt16_BadHexPrefix1 ()
		{
			Convert.ToInt16 ("#10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt16_BadHexPrefix2 ()
		{
			Convert.ToInt16 ("&H10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt16_BadHexPrefix3 ()
		{
			Convert.ToInt16 ("&h10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt32_BadHexPrefix1 ()
		{
			Convert.ToInt32 ("#10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt32_BadHexPrefix2 ()
		{
			Convert.ToInt32 ("&H10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt32_BadHexPrefix3 ()
		{
			Convert.ToInt32 ("&h10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt64_BadHexPrefix1 ()
		{
			Convert.ToInt64 ("#10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt64_BadHexPrefix2 ()
		{
			Convert.ToInt64 ("&H10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt64_BadHexPrefix3 ()
		{
			Convert.ToInt64 ("&h10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToSByte_BadHexPrefix1 ()
		{
			Convert.ToSByte ("#10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToSByte_BadHexPrefix2 ()
		{
			Convert.ToSByte ("&H10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToSByte_BadHexPrefix3 ()
		{
			Convert.ToSByte ("&h10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt16_BadHexPrefix1 ()
		{
			Convert.ToUInt16 ("#10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt16_BadHexPrefix2 ()
		{
			Convert.ToUInt16 ("&H10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt16_BadHexPrefix3 ()
		{
			Convert.ToUInt16 ("&h10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt32_BadHexPrefix1 ()
		{
			Convert.ToUInt32 ("#10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt32_BadHexPrefix2 ()
		{
			Convert.ToUInt32 ("&H10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt32_BadHexPrefix3 ()
		{
			Convert.ToUInt32 ("&h10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt64_BadHexPrefix1 ()
		{
			Convert.ToUInt64 ("#10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt64_BadHexPrefix2 ()
		{
			Convert.ToUInt64 ("&H10", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt64_BadHexPrefix3 ()
		{
			Convert.ToUInt64 ("&h10", 16);
		}

		[Test]
		public void ToSByte_Base16_MinMax ()
		{
			Assert.AreEqual (SByte.MinValue, Convert.ToSByte ("80", 16), "80,16");
			Assert.AreEqual (SByte.MinValue, Convert.ToSByte ("0x80", 16), "0x80,16");
			Assert.AreEqual (SByte.MinValue, Convert.ToSByte ("0X80", 16), "0X80,16");

			Assert.AreEqual (SByte.MaxValue, Convert.ToSByte ("7f", 16), "7f,16");
			Assert.AreEqual (SByte.MaxValue, Convert.ToSByte ("7F", 16), "7F,16");
			Assert.AreEqual (SByte.MaxValue, Convert.ToSByte ("0x7f", 16), "0x7f,16");
			Assert.AreEqual (SByte.MaxValue, Convert.ToSByte ("0X7F", 16), "0X7F,16");
		}

		[Test]
		public void ToInt16_Base16_MinMax ()
		{
			Assert.AreEqual (short.MinValue, Convert.ToInt16 ("8000", 16), "8000,16");
			Assert.AreEqual (short.MinValue, Convert.ToInt16 ("0x8000", 16), "0x8000,16");
			Assert.AreEqual (short.MinValue, Convert.ToInt16 ("0X8000", 16), "0X8000,16");

			Assert.AreEqual (short.MaxValue, Convert.ToInt16 ("7fff", 16), "7fff,16");
			Assert.AreEqual (short.MaxValue, Convert.ToInt16 ("7FFF", 16), "7FFF,16");
			Assert.AreEqual (short.MaxValue, Convert.ToInt16 ("0x7fff", 16), "0x7fff,16");
			Assert.AreEqual (short.MaxValue, Convert.ToInt16 ("0X7FFF", 16), "0X7FFF,16");
		}

		[Test]
		public void ToInt32_Base16_MinMax ()
		{
			Assert.AreEqual (int.MinValue, Convert.ToInt32 ("80000000", 16), "80000000,16");
			Assert.AreEqual (int.MinValue, Convert.ToInt32 ("0x80000000", 16), "0x80000000,16");
			Assert.AreEqual (int.MinValue, Convert.ToInt32 ("0X80000000", 16), "0X80000000,16");

			Assert.AreEqual (int.MaxValue, Convert.ToInt32 ("7fffffff", 16), "7fffffff,16");
			Assert.AreEqual (int.MaxValue, Convert.ToInt32 ("7FFFFFFF", 16), "7FFFFFFF,16");
			Assert.AreEqual (int.MaxValue, Convert.ToInt32 ("0x7fffffff", 16), "0x7fffffff,16");
			Assert.AreEqual (int.MaxValue, Convert.ToInt32 ("0X7FFFFFFF", 16), "0X7FFFFFFF,16");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToByte_Base10_InvalidChars1 ()
		{
			Convert.ToByte ("0-1", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToByte_Base10_InvalidChars2 ()
		{
			Convert.ToByte ("FF", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt16_Base10_InvalidChars1 ()
		{
			Convert.ToInt16 ("0-1", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt16_Base10_InvalidChars2 ()
		{
			Convert.ToInt16 ("FF", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt32_Base10_InvalidChars1 ()
		{
			Convert.ToInt32 ("0-1", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt32_Base10_InvalidChars2 ()
		{
			Convert.ToInt32 ("FF", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt64_Base10_InvalidChars1 ()
		{
			Convert.ToInt64 ("0-1", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt64_Base10_InvalidChars2 ()
		{
			Convert.ToInt64 ("FF", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToSByte_Base10_InvalidChars1 ()
		{
			Convert.ToSByte ("0-1", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToSByte_Base10_InvalidChars2 ()
		{
			Convert.ToSByte ("FF", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt16_Base10_InvalidChars1 ()
		{
			Convert.ToUInt16 ("0-1", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt16_Base10_InvalidChars2 ()
		{
			Convert.ToUInt16 ("FF", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt32_Base10_InvalidChars1 ()
		{
			Convert.ToUInt32 ("0-1", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt32_Base10_InvalidChars2 ()
		{
			Convert.ToUInt32 ("FF", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt64_Base10_InvalidChars1 ()
		{
			Convert.ToUInt64 ("0-1", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt64_Base10_InvalidChars2 ()
		{
			Convert.ToUInt64 ("FF", 10);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToByte_Base16_InvalidChars1 ()
		{
			Convert.ToByte ("0-1", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToByte_Base16_InvalidChars2 ()
		{
			Convert.ToByte ("GG", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt16_Base16_InvalidChars1 ()
		{
			Convert.ToInt16 ("0-1", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt16_Base16_InvalidChars2 ()
		{
			Convert.ToInt16 ("GG", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt32_Base16_InvalidChars1 ()
		{
			Convert.ToInt32 ("0-1", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt32_Base16_InvalidChars2 ()
		{
			Convert.ToInt32 ("GG", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt64_Base16_InvalidChars1 ()
		{
			Convert.ToInt64 ("0-1", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt64_Base16_InvalidChars2 ()
		{
			Convert.ToInt64 ("GG", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToSByte_Base16_InvalidChars1 ()
		{
			Convert.ToSByte ("0-1", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToSByte_Base16_InvalidChars2 ()
		{
			Convert.ToSByte ("GG", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt16_Base16_InvalidChars1 ()
		{
			Convert.ToUInt16 ("0-1", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt16_Base16_InvalidChars2 ()
		{
			Convert.ToUInt16 ("GG", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt32_Base16_InvalidChars1 ()
		{
			Convert.ToUInt32 ("0-1", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt32_Base16_InvalidChars2 ()
		{
			Convert.ToUInt32 ("GG", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt64_Base16_InvalidChars1 ()
		{
			Convert.ToUInt64 ("0-1", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt64_Base16_InvalidChars2 ()
		{
			Convert.ToUInt64 ("GG", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToByte_Base2_Empty ()
		{
			Convert.ToByte ("", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToByte_Base8_Empty ()
		{
			Convert.ToByte ("", 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToByte_Base10_Empty ()
		{
			Convert.ToByte ("", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToByte_Base16_Empty ()
		{
			Convert.ToByte ("", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt16_Base2_Empty ()
		{
			Convert.ToInt16 ("", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt16_Base8_Empty ()
		{
			Convert.ToInt16 ("", 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt16_Base10_Empty ()
		{
			Convert.ToInt16 ("", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt16_Base16_Empty ()
		{
			Convert.ToInt16 ("", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt32_Base2_Empty ()
		{
			Convert.ToInt32 ("", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt32_Base8_Empty ()
		{
			Convert.ToInt32 ("", 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt32_Base10_Empty ()
		{
			Convert.ToInt32 ("", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt32_Base16_Empty ()
		{
			Convert.ToInt32 ("", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt64_Base2_Empty ()
		{
			Convert.ToInt64 ("", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt64_Base8_Empty ()
		{
			Convert.ToInt64 ("", 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt64_Base10_Empty ()
		{
			Convert.ToInt64 ("", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToInt64_Base16_Empty ()
		{
			Convert.ToInt64 ("", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToSByte_Base2_Empty ()
		{
			Convert.ToSByte ("", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToSByte_Base8_Empty ()
		{
			Convert.ToSByte ("", 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToSByte_Base10_Empty ()
		{
			Convert.ToSByte ("", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToSByte_Base16_Empty ()
		{
			Convert.ToSByte ("", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt16_Base2_Empty ()
		{
			Convert.ToUInt16 ("", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt16_Base8_Empty ()
		{
			Convert.ToUInt16 ("", 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt16_Base10_Empty ()
		{
			Convert.ToUInt16 ("", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt16_Base16_Empty ()
		{
			Convert.ToUInt16 ("", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt32_Base2_Empty ()
		{
			Convert.ToUInt32 ("", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt32_Base8_Empty ()
		{
			Convert.ToUInt32 ("", 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt32_Base10_Empty ()
		{
			Convert.ToUInt32 ("", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt32_Base16_Empty ()
		{
			Convert.ToUInt32 ("", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt64_Base2_Empty ()
		{
			Convert.ToUInt64 ("", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt64_Base8_Empty ()
		{
			Convert.ToUInt64 ("", 8);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt64_Base10_Empty ()
		{
			Convert.ToUInt64 ("", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ToUInt64_Base16_Empty ()
		{
			Convert.ToUInt64 ("", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToByte_HexPrefixOnly ()
		{
			Convert.ToByte ("0x", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt16_HexPrefixOnly ()
		{
			Convert.ToInt16 ("0x", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt32_HexPrefixOnly ()
		{
			Convert.ToInt32 ("0x", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt64_HexPrefixOnly ()
		{
			Convert.ToInt64 ("0x", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToSByte_HexPrefixOnly ()
		{
			Convert.ToSByte ("0x", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt16_HexPrefixOnly ()
		{
			Convert.ToUInt16 ("0x", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt32_HexPrefixOnly ()
		{
			Convert.ToUInt32 ("0x", 16);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToUInt64_HexPrefixOnly ()
		{
			Convert.ToUInt64 ("0x", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToByte_Base2_NegativeSignOnly ()
		{
			Convert.ToByte ("-", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToByte_Base8_NegativeSignOnly ()
		{
			Convert.ToByte ("-", 8);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToByte_Base10_NegativeSignOnly ()
		{
			Convert.ToByte ("-", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToByte_Base16_NegativeSignOnly ()
		{
			Convert.ToByte ("-", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToInt16_Base2_NegativeSignOnly ()
		{
			Convert.ToInt16 ("-", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToInt16_Base8_NegativeSignOnly ()
		{
			Convert.ToInt16 ("-", 8);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt16_Base10_NegativeSignOnly ()
		{
			Convert.ToInt16 ("-", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToInt16_Base16_NegativeSignOnly ()
		{
			Convert.ToInt16 ("-", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToInt32_Base2_NegativeSignOnly ()
		{
			Convert.ToInt32 ("-", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToInt32_Base8_NegativeSignOnly ()
		{
			Convert.ToInt32 ("-", 8);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt32_Base10_NegativeSignOnly ()
		{
			Convert.ToInt32 ("-", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToInt32_Base16_NegativeSignOnly ()
		{
			Convert.ToInt32 ("-", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToInt64_Base2_NegativeSignOnly ()
		{
			Convert.ToInt64 ("-", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToInt64_Base8_NegativeSignOnly ()
		{
			Convert.ToInt64 ("-", 8);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToInt64_Base10_NegativeSignOnly ()
		{
			Convert.ToInt64 ("-", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToInt64_Base16_NegativeSignOnly ()
		{
			Convert.ToInt64 ("-", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToSByte_Base2_NegativeSignOnly ()
		{
			Convert.ToSByte ("-", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToSByte_Base8_NegativeSignOnly ()
		{
			Convert.ToSByte ("-", 8);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ToSByte_Base10_NegativeSignOnly ()
		{
			Convert.ToSByte ("-", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToSByte_Base16_NegativeSignOnly ()
		{
			Convert.ToSByte ("-", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt16_Base2_NegativeSignOnly ()
		{
			Convert.ToUInt16 ("-", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt16_Base8_NegativeSignOnly ()
		{
			Convert.ToUInt16 ("-", 8);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt16_Base10_NegativeSignOnly ()
		{
			Convert.ToUInt16 ("-", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt16_Base16_NegativeSignOnly ()
		{
			Convert.ToUInt16 ("-", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt32_Base2_NegativeSignOnly ()
		{
			Convert.ToUInt32 ("-", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt32_Base8_NegativeSignOnly ()
		{
			Convert.ToUInt32 ("-", 8);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt32_Base10_NegativeSignOnly ()
		{
			Convert.ToUInt32 ("-", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt32_Base16_NegativeSignOnly ()
		{
			Convert.ToUInt32 ("-", 16);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt64_Base2_NegativeSignOnly ()
		{
			Convert.ToUInt64 ("-", 2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt64_Base8_NegativeSignOnly ()
		{
			Convert.ToUInt64 ("-", 8);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt64_Base10_NegativeSignOnly ()
		{
			Convert.ToUInt64 ("-", 10);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToUInt64_Base16_NegativeSignOnly ()
		{
			Convert.ToUInt64 ("-", 16);
		}

		[Test]
		public void ToInt32_Base10_MaxValue ()
		{
			Assert.AreEqual (Int32.MaxValue, Convert.ToInt32 (Int32.MaxValue.ToString(), 10));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		[Category ("NotWorking")] // FIXME: this should throw an OverflowException but currently doesn't
		public void ToInt32_Base10_MaxValueOverflow ()
		{
			var overflowValue = ((UInt32) Int32.MaxValue) + 1;
			Convert.ToInt32 (overflowValue.ToString (), 10);
		}

		[Test]
		public void ToInt32_Base10_MinValue ()
		{
			Assert.AreEqual (Int32.MinValue, Convert.ToInt32 (Int32.MinValue.ToString(), 10));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		[Category ("NotWorking")] // FIXME: this should throw an OverflowException but currently doesn't		
		public void ToInt32_Base10_MinValueOverflow ()
		{
			var overflowValue = ((UInt32) Int32.MaxValue) + 2;
			Convert.ToInt32 ("-" + overflowValue.ToString (), 10);
		}

		[Test]
		public void ToInt32_Base16_MaxValue ()
		{
			Assert.AreEqual (Int32.MaxValue, Convert.ToInt32 (Int32.MaxValue.ToString("x"), 16));
		}

		[Test]
		public void ToInt32_Base16_MaxValueOverflow ()
		{
			var overflowValue = ((UInt32) Int32.MaxValue) + 1;
			Assert.AreEqual (-2147483648, Convert.ToInt32 (overflowValue.ToString("x"), 16));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt32_Base16_MaxValueOverflow2 ()
		{
			Convert.ToInt32 (UInt32.MaxValue.ToString ("x") + "0", 16);
		}

		[Test]
		public void ToInt32_Base16_MinValue ()
		{
			Assert.AreEqual (Int32.MinValue, Convert.ToInt32 (Int32.MinValue.ToString ("x"), 16));
		}

		[Test]
		public void ToUInt32_Base10_MaxValue ()
		{
			Assert.AreEqual (UInt32.MaxValue, Convert.ToUInt32 (UInt32.MaxValue.ToString (), 10));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt32_Base10_MaxValueOverflow ()
		{
			Convert.ToUInt32 (UInt32.MaxValue.ToString () + "0", 10);
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt32_Base10_MaxValueOverflow2 ()
		{
			Convert.ToUInt32 ("4933891728", 10);
		}

		[Test]
		public void ToUInt32_Base16_MaxValue ()
		{
			Assert.AreEqual (UInt32.MaxValue, Convert.ToUInt32 (UInt32.MaxValue.ToString ("x"), 16));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt32_Base16_MaxValueOverflow ()
		{
			Convert.ToUInt32 (UInt32.MaxValue.ToString ("x") + "0", 16);
		}

		[Test]
		public void ToInt64_Base10_MaxValue ()
		{
			Assert.AreEqual (Int64.MaxValue, Convert.ToInt64 (Int64.MaxValue.ToString(), 10));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt64_Base10_MaxValueOverflow ()
		{
			var overflowValue = ((UInt64) Int64.MaxValue) + 1;
			Convert.ToInt64 (overflowValue.ToString (), 10);
		}

		[Test]
		public void ToInt64_Base10_MinValue ()
		{
			Assert.AreEqual (Int64.MinValue, Convert.ToInt64 (Int64.MinValue.ToString(), 10));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt64_Base10_MinValueOverflow ()
		{
			var overflowValue = ((UInt64) Int64.MaxValue) + 2;
			Convert.ToInt64 ("-" + overflowValue.ToString (), 10);
		}

		[Test]
		public void ToInt64_Base16_MaxValue ()
		{
			Assert.AreEqual (Int64.MaxValue, Convert.ToInt64 (Int64.MaxValue.ToString("x"), 16));
		}

		[Test]
		public void ToInt64_Base16_MaxValueOverflow ()
		{
			var overflowValue = ((UInt64) Int64.MaxValue) + 1;
			Assert.AreEqual (-9223372036854775808, Convert.ToInt64 (overflowValue.ToString("x"), 16));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt64_Base16_MaxValueOverflow2 ()
		{
			Convert.ToInt64 (UInt64.MaxValue.ToString ("x") + "0", 16);
		}

		[Test]
		public void ToInt64_Base16_MinValue ()
		{
			Assert.AreEqual (Int64.MinValue, Convert.ToInt64 (Int64.MinValue.ToString ("x"), 16));
		}

		[Test]
		public void ToUInt64_Base10_MaxValue ()
		{
			Assert.AreEqual (UInt64.MaxValue, Convert.ToUInt64 (UInt64.MaxValue.ToString (), 10));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt64_Base10_MaxValueOverflow ()
		{
			Convert.ToUInt64 (UInt64.MaxValue.ToString () + "0", 10);
		}

		[Test]
		public void ToUInt64_Base16_MaxValue ()
		{
			Assert.AreEqual (UInt64.MaxValue, Convert.ToUInt64 (UInt64.MaxValue.ToString ("x"), 16));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToUInt64_Base16_MaxValueOverflow ()
		{
			Convert.ToUInt64 (UInt64.MaxValue.ToString ("x") + "0", 16);
		}

		[Test] // bug #481687
		public void ChangeType_Value_IConvertible ()
		{
			BitmapStatus bitmapStatus = new BitmapStatus (3);
			Image c1 = Convert.ChangeType (bitmapStatus, typeof (Image)) as Image;
			Assert.IsNotNull (c1, "#A1");
			Assert.AreEqual (32, c1.ColorDepth, "#A2");

			bitmapStatus.ConvertToImage = false;
			object c2 = Convert.ChangeType (bitmapStatus, typeof (Image));
			Assert.IsNull (c2, "#B");

			object c3 = Convert.ChangeType (bitmapStatus, typeof (int));
			Assert.IsNotNull (c3, "#C1");
			Assert.AreEqual (3, c3, "#C2");
		}

		// This is a simple and happy struct.
		struct Foo {
		}
		
		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void ChangeType_ShouldThrowOnString ()
		{
			Convert.ChangeType ("this-is-a-string", typeof (Foo));
		}

		[Test]
		[ExpectedException (typeof (OverflowException))]
		public void ToInt32_NaN ()
		{
			Convert.ToInt32 (Double.NaN);
		}

		[Test]
		public void ChangeTypeFromInvalidDouble ()
		{
			// types which should generate OverflowException from double.NaN, etc.
			Type[] types = new Type []{
				typeof (byte), typeof (sbyte), typeof (decimal), 
				typeof (short), typeof (int), typeof (long),
				typeof (ushort), typeof (uint), typeof (ulong),
			};

			CheckChangeTypeOverflow ("double.NaN",              double.NaN,               types);
			CheckChangeTypeOverflow ("double.PositiveInfinity", double.PositiveInfinity,  types);
			CheckChangeTypeOverflow ("double.NegativeInfinity", double.NegativeInfinity,  types);
			CheckChangeTypeOverflow ("float.NaN",               float.NaN,                types);
			CheckChangeTypeOverflow ("float.PositiveInfinity",  float.PositiveInfinity,   types);
			CheckChangeTypeOverflow ("float.NegativeInfinity",  float.NegativeInfinity,   types);
		}

		static void CheckChangeTypeOverflow (string svalue, object value, Type[] types)
		{
			foreach (Type type in types) {
				string message = string.Format (" when converting {0} to {1}", svalue, type.FullName);
				try {
					Convert.ChangeType (value, type);
					Assert.Fail ("Expected System.OverflowException " + message);
				}
				catch (OverflowException) {
					// success
				}
				catch (Exception e) {
					Assert.Fail ("Unexpected exception type " + e.GetType ().FullName + message);
				}
			}
		}

		[Test]
		public void ToInt32_InvalidFormatProvider ()
		{
			Assert.AreEqual (5, Convert.ToInt32 ("5", new InvalidFormatProvider ()));
		}
	}

	public class Image
	{
		private int colorDepth;

		public Image ()
		{
			colorDepth = 8;
		}

		public Image (int colorDepth)
		{
			this.colorDepth = colorDepth;
		}

		public int ColorDepth {
			get { return colorDepth; }
		}
	}

	public class BitmapStatus : IConvertible
	{
		protected int m_Status;
		private bool convertToImage;

		public BitmapStatus (int status)
		{
			m_Status = status;
			convertToImage = true;
		}

		public bool ConvertToImage {
			get { return convertToImage; }
			set { convertToImage = value; }
		}

		TypeCode IConvertible.GetTypeCode ()
		{
			return TypeCode.Int32;
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return (bool)((IConvertible)this).ToType (typeof (bool), provider);
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return (byte)((IConvertible)this).ToType (typeof (byte), provider);
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			return (char)((IConvertible)this).ToType (typeof (char), provider);
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			return (DateTime)((IConvertible)this).ToType (typeof (DateTime), provider);
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return (decimal)((IConvertible)this).ToType (typeof (decimal), provider);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return (double)((IConvertible)this).ToType (typeof (double), provider);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return (short)((IConvertible)this).ToType (typeof (short), provider);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return (int)m_Status;
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return (long)((IConvertible)this).ToType (typeof (long), provider);
		}

		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return (sbyte)((IConvertible)this).ToType (typeof (sbyte), provider);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return (float)((IConvertible)this).ToType (typeof (float), provider);
		}

		string IConvertible.ToString (IFormatProvider provider)
		{
			return (string)((IConvertible)this).ToType (typeof (string), provider);
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			if (ConvertToImage && conversionType == typeof (Image))
				return new Image (32);
			else if (conversionType.IsAssignableFrom (typeof (int)))
				return Convert.ChangeType (1, conversionType, provider);
			return null;
		}

		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return (ushort)((IConvertible)this).ToType (typeof (ushort), provider);
		}

		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return (uint)((IConvertible)this).ToType (typeof (uint), provider);
		}

		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return (ulong)((IConvertible)this).ToType (typeof (ulong), provider);
		}
	}

	class InvalidFormatProvider : IFormatProvider
	{
		public object GetFormat (Type formatType)
		{
			return "";
		}
	}
}
