//
// SqlDoubleTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlDouble
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Ville Palo
// (C) 2003 Martin Willemoes Hansen
// 
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Data.SqlTypes;
using System.Globalization;
#if NET_2_0
using System.IO;
#endif
using System.Threading;
using System.Xml;
#if NET_2_0
using System.Xml.Serialization;
#endif

using NUnit.Framework;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
	public class SqlDoubleTest
	{
		private CultureInfo originalCulture;

		[SetUp]
		public void SetUp ()
		{
			originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = originalCulture;
		}

		// Test constructor
		[Test]
		public void Create ()
		{
			SqlDouble Test = new SqlDouble ((double) 34.87);
			Assert.AreEqual (34.87D, Test.Value, "#A01");

			Test = new SqlDouble (-9000.6543);
			Assert.AreEqual (-9000.6543D, Test.Value, "#A02");
		}

		// Test public fields
		[Test]
		public void PublicFields ()
		{
			Assert.AreEqual (1.7976931348623157e+308, SqlDouble.MaxValue.Value, "#B01");
			Assert.AreEqual (-1.7976931348623157e+308, SqlDouble.MinValue.Value, "#B02");
			Assert.IsTrue (SqlDouble.Null.IsNull, "#B03");
			Assert.AreEqual (0d, SqlDouble.Zero.Value, "#B04");
		}

		// Test properties
		[Test]
		public void Properties ()
		{
			SqlDouble Test5443 = new SqlDouble (5443e12);
			SqlDouble Test1 = new SqlDouble (1);

			Assert.IsTrue (SqlDouble.Null.IsNull, "#C01");
			Assert.AreEqual (5443e12, Test5443.Value, "#C02");
			Assert.AreEqual ((double) 1, Test1.Value, "#C03");
		}

		// PUBLIC METHODS

		[Test]
		public void ArithmeticMethods ()
		{
			SqlDouble Test0 = new SqlDouble (0);
			SqlDouble Test1 = new SqlDouble (15E+108);
			SqlDouble Test2 = new SqlDouble (-65E+64);
			SqlDouble Test3 = new SqlDouble (5E+64);
			SqlDouble Test4 = new SqlDouble (5E+108);
			SqlDouble TestMax = new SqlDouble (SqlDouble.MaxValue.Value);

			// Add()
			Assert.AreEqual (15E+108, SqlDouble.Add (Test1, Test0).Value, "#D01A");
			Assert.AreEqual (1.5E+109, SqlDouble.Add (Test1, Test2).Value, "#D02A");

			try {
				SqlDouble test = SqlDouble.Add (SqlDouble.MaxValue, SqlDouble.MaxValue);
				Assert.Fail ("#D03A");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D04A");
			}

			// Divide()
			Assert.AreEqual ((SqlDouble) 3, SqlDouble.Divide (Test1, Test4), "#D01B");
			Assert.AreEqual (-13d, SqlDouble.Divide (Test2, Test3).Value, "#D02B");

			try {
				SqlDouble test = SqlDouble.Divide (Test1, Test0).Value;
				Assert.Fail ("#D03B");
			} catch (DivideByZeroException e) {
				Assert.AreEqual (typeof (DivideByZeroException), e.GetType (), "#D04B");
			}

			// Multiply()
			Assert.AreEqual ((double) (75E+216), SqlDouble.Multiply (Test1, Test4).Value, "#D01D");
			Assert.AreEqual ((double) 0, SqlDouble.Multiply (Test1, Test0).Value, "#D02D");

			try {
				SqlDouble test = SqlDouble.Multiply (TestMax, Test1);
				Assert.Fail ("#D03D");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D04D");
			}


			// Subtract()
			Assert.AreEqual ((double) 1.5E+109, SqlDouble.Subtract (Test1, Test3).Value, "#D01F");

			try {
				SqlDouble test = SqlDouble.Subtract (SqlDouble.MinValue, SqlDouble.MaxValue);
				Assert.Fail ("D02F");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#D03F");
			}
		}

		[Test]
		public void CompareTo ()
		{
			SqlDouble Test1 = new SqlDouble (4e64);
			SqlDouble Test11 = new SqlDouble (4e64);
			SqlDouble Test2 = new SqlDouble (-9e34);
			SqlDouble Test3 = new SqlDouble (10000);
			SqlString TestString = new SqlString ("This is a test");

			Assert.IsTrue (Test1.CompareTo (Test3) > 0, "#E01");
			Assert.IsTrue (Test2.CompareTo (Test3) < 0, "#E02");
			Assert.IsTrue (Test1.CompareTo (Test11) == 0, "#E03");
			Assert.IsTrue (Test11.CompareTo (SqlDouble.Null) > 0, "#E04");

			try {
				Test1.CompareTo (TestString);
				Assert.Fail ("#E05");
			} catch (ArgumentException e) {
				Assert.AreEqual (typeof (ArgumentException), e.GetType (), "#E06");
			}
		}

		[Test]
		public void EqualsMethods ()
		{
			SqlDouble Test0 = new SqlDouble (0);
			SqlDouble Test1 = new SqlDouble (1.58e30);
			SqlDouble Test2 = new SqlDouble (1.8e180);
			SqlDouble Test22 = new SqlDouble (1.8e180);

			Assert.IsTrue (!Test0.Equals (Test1), "#F01");
			Assert.IsTrue (!Test1.Equals (Test2), "#F02");
			Assert.IsTrue (!Test2.Equals (new SqlString ("TEST")), "#F03");
			Assert.IsTrue (Test2.Equals (Test22), "#F04");

			// Static Equals()-method
			Assert.IsTrue (SqlDouble.Equals (Test2, Test22).Value, "#F05");
			Assert.IsTrue (!SqlDouble.Equals (Test1, Test2).Value, "#F06");
		}

		[Test]
		public void GetHashCodeTest ()
		{
			SqlDouble Test15 = new SqlDouble (15);

			// FIXME: Better way to test HashCode
			Assert.AreEqual (Test15.GetHashCode (), Test15.GetHashCode (), "#G01");
		}

		[Test]
		public void GetTypeTest ()
		{
			SqlDouble Test = new SqlDouble (84);
			Assert.AreEqual ("System.Data.SqlTypes.SqlDouble", Test.GetType ().ToString (), "#H01");
			Assert.AreEqual ("System.Double", Test.Value.GetType ().ToString (), "#H02");
		}

		[Test]
		public void Greaters ()
		{
			SqlDouble Test1 = new SqlDouble (1e100);
			SqlDouble Test11 = new SqlDouble (1e100);
			SqlDouble Test2 = new SqlDouble (64e164);

			// GreateThan ()
			Assert.IsTrue (!SqlDouble.GreaterThan (Test1, Test2).Value, "#I01");
			Assert.IsTrue (SqlDouble.GreaterThan (Test2, Test1).Value, "#I02");
			Assert.IsTrue (!SqlDouble.GreaterThan (Test1, Test11).Value, "#I03");

			// GreaterTharOrEqual ()
			Assert.IsTrue (!SqlDouble.GreaterThanOrEqual (Test1, Test2).Value, "#I04");
			Assert.IsTrue (SqlDouble.GreaterThanOrEqual (Test2, Test1).Value, "#I05");
			Assert.IsTrue (SqlDouble.GreaterThanOrEqual (Test1, Test11).Value, "#I06");
		}

		[Test]
		public void Lessers ()
		{
			SqlDouble Test1 = new SqlDouble (1.8e100);
			SqlDouble Test11 = new SqlDouble (1.8e100);
			SqlDouble Test2 = new SqlDouble (64e164);

			// LessThan()
			Assert.IsTrue (!SqlDouble.LessThan (Test1, Test11).Value, "#J01");
			Assert.IsTrue (!SqlDouble.LessThan (Test2, Test1).Value, "#J02");
			Assert.IsTrue (SqlDouble.LessThan (Test11, Test2).Value, "#J03");

			// LessThanOrEqual ()
			Assert.IsTrue (SqlDouble.LessThanOrEqual (Test1, Test2).Value, "#J04");
			Assert.IsTrue (!SqlDouble.LessThanOrEqual (Test2, Test1).Value, "#J05");
			Assert.IsTrue (SqlDouble.LessThanOrEqual (Test11, Test1).Value, "#J06");
			Assert.IsTrue (SqlDouble.LessThanOrEqual (Test11, SqlDouble.Null).IsNull, "#J07");
		}

		[Test]
		public void NotEquals ()
		{
			SqlDouble Test1 = new SqlDouble (1280000000001);
			SqlDouble Test2 = new SqlDouble (128e10);
			SqlDouble Test22 = new SqlDouble (128e10);

			Assert.IsTrue (SqlDouble.NotEquals (Test1, Test2).Value, "#K01");
			Assert.IsTrue (SqlDouble.NotEquals (Test2, Test1).Value, "#K02");
			Assert.IsTrue (SqlDouble.NotEquals (Test22, Test1).Value, "#K03");
			Assert.IsTrue (!SqlDouble.NotEquals (Test22, Test2).Value, "#K04");
			Assert.IsTrue (!SqlDouble.NotEquals (Test2, Test22).Value, "#K05");
			Assert.IsTrue (SqlDouble.NotEquals (SqlDouble.Null, Test22).IsNull, "#K06");
			Assert.IsTrue (SqlDouble.NotEquals (SqlDouble.Null, Test22).IsNull, "#K07");
		}

		[Test]
		public void Parse ()
		{
			try {
				SqlDouble.Parse (null);
				Assert.Fail ("#L01");
			} catch (ArgumentNullException e) {
				Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "#L02");
			}

			try {
				SqlDouble.Parse ("not-a-number");
				Assert.Fail ("#L03");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#L04");
			}

			try {
				SqlDouble.Parse ("9e400");
				Assert.Fail ("#L05");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#L06");
			}

			Assert.AreEqual ((double) 150, SqlDouble.Parse ("150").Value, "#L07");
		}

		[Test]
		public void Conversions ()
		{
			SqlDouble Test0 = new SqlDouble (0);
			SqlDouble Test1 = new SqlDouble (250);
			SqlDouble Test2 = new SqlDouble (64e64);
			SqlDouble Test3 = new SqlDouble (64e164);
			SqlDouble TestNull = SqlDouble.Null;

			// ToSqlBoolean ()
			Assert.IsTrue (Test1.ToSqlBoolean ().Value, "#M01A");
			Assert.IsTrue (!Test0.ToSqlBoolean ().Value, "#M02A");
			Assert.IsTrue (TestNull.ToSqlBoolean ().IsNull, "#M03A");

			// ToSqlByte ()
			Assert.AreEqual ((byte) 250, Test1.ToSqlByte ().Value, "#M01B");
			Assert.AreEqual ((byte) 0, Test0.ToSqlByte ().Value, "#M02B");

			try {
				SqlByte b = (byte) Test2.ToSqlByte ();
				Assert.Fail ("#M03B");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04B");
			}

			// ToSqlDecimal ()
			Assert.AreEqual (250.00000000000000M, Test1.ToSqlDecimal ().Value, "#M01C");
			Assert.AreEqual ((decimal) 0, Test0.ToSqlDecimal ().Value, "#M02C");

			try {
				SqlDecimal test = Test3.ToSqlDecimal ().Value;
				Assert.Fail ("#M03C");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04C");
			}

			// ToSqlInt16 ()
			Assert.AreEqual ((short) 250, Test1.ToSqlInt16 ().Value, "#M01D");
			Assert.AreEqual ((short) 0, Test0.ToSqlInt16 ().Value, "#M02D");

			try {
				SqlInt16 test = Test2.ToSqlInt16 ().Value;
				Assert.Fail ("#M03D");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04D");
			}

			// ToSqlInt32 ()
			Assert.AreEqual ((int) 250, Test1.ToSqlInt32 ().Value, "#M01E");
			Assert.AreEqual ((int) 0, Test0.ToSqlInt32 ().Value, "#M02E");

			try {
				SqlInt32 test = Test2.ToSqlInt32 ().Value;
				Assert.Fail ("#M03E");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04E");
			}

			// ToSqlInt64 ()
			Assert.AreEqual ((long) 250, Test1.ToSqlInt64 ().Value, "#M01F");
			Assert.AreEqual ((long) 0, Test0.ToSqlInt64 ().Value, "#M02F");

			try {
				SqlInt64 test = Test2.ToSqlInt64 ().Value;
				Assert.Fail ("#M03F");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04F");
			}

			// ToSqlMoney ()
			Assert.AreEqual (250.0000M, Test1.ToSqlMoney ().Value, "#M01G");
			Assert.AreEqual ((decimal) 0, Test0.ToSqlMoney ().Value, "#M02G");

			try {
				SqlMoney test = Test2.ToSqlMoney ().Value;
				Assert.Fail ("#M03G");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04G");
			}

			// ToSqlSingle ()
			Assert.AreEqual ((float) 250, Test1.ToSqlSingle ().Value, "#M01H");
			Assert.AreEqual ((float) 0, Test0.ToSqlSingle ().Value, "#M02H");

			try {
				SqlSingle test = Test2.ToSqlSingle ().Value;
				Assert.Fail ("#MO3H");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#M04H");
			}

			// ToSqlString ()
			Assert.AreEqual ("250", Test1.ToSqlString ().Value, "#M01I");
			Assert.AreEqual ("0", Test0.ToSqlString ().Value, "#M02I");
			Assert.AreEqual ("6.4E+65", Test2.ToSqlString ().Value, "#M03I");

			// ToString ()
			Assert.AreEqual ("250", Test1.ToString (), "#M01J");
			Assert.AreEqual ("0", Test0.ToString (), "#M02J");
			Assert.AreEqual ("6.4E+65", Test2.ToString (), "#M03J");
		}

		// OPERATORS

		[Test]
		public void ArithmeticOperators ()
		{
			SqlDouble Test0 = new SqlDouble (0);
			SqlDouble Test1 = new SqlDouble (24E+100);
			SqlDouble Test2 = new SqlDouble (64E+164);
			SqlDouble Test3 = new SqlDouble (12E+100);
			SqlDouble Test4 = new SqlDouble (1E+10);
			SqlDouble Test5 = new SqlDouble (2E+10);

			// "+"-operator
			Assert.AreEqual ((SqlDouble) 3E+10, Test4 + Test5, "#N01");

			try {
				SqlDouble test = SqlDouble.MaxValue + SqlDouble.MaxValue;
				Assert.Fail ("#N02");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N03");
			}

			// "/"-operator
			Assert.AreEqual ((SqlDouble) 2, Test1 / Test3, "#N04");

			try {
				SqlDouble test = Test3 / Test0;
				Assert.Fail ("#N05");
			} catch (DivideByZeroException e) {
				Assert.AreEqual (typeof (DivideByZeroException), e.GetType (), "#N06");
			}

			// "*"-operator
			Assert.AreEqual ((SqlDouble) 2e20, Test4 * Test5, "#N07");

			try {
				SqlDouble test = SqlDouble.MaxValue * Test1;
				Assert.Fail ("#N08");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N09");
			}

			// "-"-operator
			Assert.AreEqual ((SqlDouble) 12e100, Test1 - Test3, "#N10");

			try {
				SqlDouble test = SqlDouble.MinValue - SqlDouble.MaxValue;
				Assert.Fail ("#N11");
			} catch (OverflowException e) {
				Assert.AreEqual (typeof (OverflowException), e.GetType (), "#N12");
			}
		}

		[Test]
		public void ThanOrEqualOperators ()
		{
			SqlDouble Test1 = new SqlDouble (1E+164);
			SqlDouble Test2 = new SqlDouble (9.7E+100);
			SqlDouble Test22 = new SqlDouble (9.7E+100);
			SqlDouble Test3 = new SqlDouble (2E+200);

			// == -operator
			Assert.IsTrue ((Test2 == Test22).Value, "#O01");
			Assert.IsTrue (!(Test1 == Test2).Value, "#O02");
			Assert.IsTrue ((Test1 == SqlDouble.Null).IsNull, "#O03");

			// != -operator
			Assert.IsTrue (!(Test2 != Test22).Value, "#O04");
			Assert.IsTrue ((Test2 != Test3).Value, "#O05");
			Assert.IsTrue ((Test1 != Test3).Value, "#O06");
			Assert.IsTrue ((Test1 != SqlDouble.Null).IsNull, "#O07");

			// > -operator
			Assert.IsTrue ((Test1 > Test2).Value, "#O08");
			Assert.IsTrue (!(Test1 > Test3).Value, "#O09");
			Assert.IsTrue (!(Test2 > Test22).Value, "#O10");
			Assert.IsTrue ((Test1 > SqlDouble.Null).IsNull, "#O11");

			// >=  -operator
			Assert.IsTrue (!(Test1 >= Test3).Value, "#O12");
			Assert.IsTrue ((Test3 >= Test1).Value, "#O13");
			Assert.IsTrue ((Test2 >= Test22).Value, "#O14");
			Assert.IsTrue ((Test1 >= SqlDouble.Null).IsNull, "#O15");

			// < -operator
			Assert.IsTrue (!(Test1 < Test2).Value, "#O16");
			Assert.IsTrue ((Test1 < Test3).Value, "#O17");
			Assert.IsTrue (!(Test2 < Test22).Value, "#O18");
			Assert.IsTrue ((Test1 < SqlDouble.Null).IsNull, "#O19");

			// <= -operator
			Assert.IsTrue ((Test1 <= Test3).Value, "#O20");
			Assert.IsTrue (!(Test3 <= Test1).Value, "#O21");
			Assert.IsTrue ((Test2 <= Test22).Value, "#O22");
			Assert.IsTrue ((Test1 <= SqlDouble.Null).IsNull, "#O23");
		}

		[Test]
		public void UnaryNegation ()
		{
			SqlDouble Test = new SqlDouble (2000000001);
			SqlDouble TestNeg = new SqlDouble (-3000);

			SqlDouble Result = -Test;
			Assert.AreEqual ((double) (-2000000001), Result.Value, "#P01");

			Result = -TestNeg;
			Assert.AreEqual ((double) 3000, Result.Value, "#P02");
		}

		[Test]
		public void SqlBooleanToSqlDouble ()
		{
			SqlBoolean TestBoolean = new SqlBoolean (true);
			SqlDouble Result;

			Result = (SqlDouble) TestBoolean;

			Assert.AreEqual ((double) 1, Result.Value, "#Q01");

			Result = (SqlDouble) SqlBoolean.Null;
			Assert.IsTrue (Result.IsNull, "#Q02");
		}

		[Test]
		public void SqlDoubleToDouble ()
		{
			SqlDouble Test = new SqlDouble (12e12);
			Double Result = (double) Test;
			Assert.AreEqual (12e12, Result, "#R01");
		}

		[Test]
		public void SqlStringToSqlDouble ()
		{
			SqlString TestString = new SqlString ("Test string");
			SqlString TestString100 = new SqlString ("100");

			Assert.AreEqual ((double) 100, ((SqlDouble) TestString100).Value, "#S01");

			try {
				SqlDouble test = (SqlDouble) TestString;
				Assert.Fail ("#S02");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#S03");
			}
		}

		[Test]
		public void DoubleToSqlDouble ()
		{
			double Test1 = 5e64;
			SqlDouble Result = (SqlDouble) Test1;
			Assert.AreEqual (5e64, Result.Value, "#T01");
		}

		[Test]
		public void ByteToSqlDouble ()
		{
			short TestShort = 14;
			Assert.AreEqual ((double) 14, ((SqlDouble) TestShort).Value, "#U01");
		}

		[Test]
		public void SqlDecimalToSqlDouble ()
		{
			SqlDecimal TestDecimal64 = new SqlDecimal (64);

			Assert.AreEqual ((double) 64, ((SqlDouble) TestDecimal64).Value, "#V01");
			Assert.AreEqual (SqlDouble.Null, ((SqlDouble) SqlDecimal.Null), "#V02");
		}

		[Test]
		public void SqlIntToSqlDouble ()
		{
			SqlInt16 Test64 = new SqlInt16 (64);
			SqlInt32 Test640 = new SqlInt32 (640);
			SqlInt64 Test64000 = new SqlInt64 (64000);
			Assert.AreEqual ((double) 64, ((SqlDouble) Test64).Value, "#W01");
			Assert.AreEqual ((double) 640, ((SqlDouble) Test640).Value, "#W02");
			Assert.AreEqual ((double) 64000, ((SqlDouble) Test64000).Value, "#W03");
		}

		[Test]
		public void SqlMoneyToSqlDouble ()
		{
			SqlMoney TestMoney64 = new SqlMoney (64);
			Assert.AreEqual ((double) 64, ((SqlDouble) TestMoney64).Value, "#X01");
		}

		[Test]
		public void SqlSingleToSqlDouble ()
		{
			SqlSingle TestSingle64 = new SqlSingle (64);
			Assert.AreEqual ((double) 64, ((SqlDouble) TestSingle64).Value, "#Y01");
		}
#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlDouble.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("double", qualifiedName.Name, "#A01");
		}

		internal void ReadWriteXmlTestInternal (string xml, 
						       double testval, 
						       string unit_test_id)
		{
			SqlDouble test;
			SqlDouble test1;
			XmlSerializer ser;
			StringWriter sw;
			XmlTextWriter xw;
			StringReader sr;
			XmlTextReader xr;

			test = new SqlDouble (testval);
			ser = new XmlSerializer(typeof(SqlDouble));
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			
			ser.Serialize (xw, test);

			// Assert.AreEqual (xml, sw.ToString (), unit_test_id);

			sr = new StringReader (xml);
			xr = new XmlTextReader (sr);
			test1 = (SqlDouble)ser.Deserialize (xr);

			Assert.AreEqual (testval, test1.Value, unit_test_id);
		}

		[Test]
		public void ReadWriteXmlTest ()
		{
			string xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><double>4556.99999999999999999988</double>";
			string xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><double>-6445.8888888888899999999</double>";
			string xml3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><double>0x455687AB3E4D56F</double>";
			double test1 = 4556.99999999999999999988;
			double test2 = -6445.8888888888899999999;
			double test3 = 0x4F56;

			ReadWriteXmlTestInternal (xml1, test1, "BA01");
			ReadWriteXmlTestInternal (xml2, test2, "BA02");
		
			try {
				ReadWriteXmlTestInternal (xml3, test3, "BA03");
				Assert.Fail ("BA03");
			} catch (FormatException e) {
				Assert.AreEqual (typeof (FormatException), e.GetType (), "#BA03");
			}
		}
#endif
	}
}
