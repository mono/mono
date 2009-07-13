//
// SqlDateTimeTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlDateTime
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen
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

using NUnit.Framework;
using System;
using System.Xml;
using System.Data.SqlTypes;
using System.Threading;
using System.Globalization;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlDateTimeTest {

	        private long[] myTicks = {
			631501920000000000L,	// 25 Feb 2002 - 00:00:00
			631502475130080000L,	// 25 Feb 2002 - 15:25:13,8
			631502115130080000L,	// 25 Feb 2002 - 05:25:13,8
			631502115000000000L,	// 25 Feb 2002 - 05:25:00
			631502115130000000L,	// 25 Feb 2002 - 05:25:13
			631502079130000000L,	// 25 Feb 2002 - 04:25:13
			629197085770000000L	// 06 Nov 1994 - 08:49:37 
		};

	        private SqlDateTime Test1;
		private SqlDateTime Test2;
		private SqlDateTime Test3;

		[SetUp]
                public void GetReady() 
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
			Test1 = new SqlDateTime (2002, 10, 19, 9, 40, 0);
			Test2 = new SqlDateTime (2003, 11, 20,10, 50, 1);
			Test3 = new SqlDateTime (2003, 11, 20, 10, 50, 1);
		}

                // Test constructor
		[Test]
                public void Create()
                {
			// SqlDateTime (DateTime)
			SqlDateTime CTest = new SqlDateTime (
				new DateTime (2002, 5, 19, 3, 34, 0));
			Assert.AreEqual (2002, CTest.Value.Year, "#A01");
				
			// SqlDateTime (int, int)
                        CTest = new SqlDateTime (0, 0);
			
			// SqlDateTime (int, int, int)
                        Assert.AreEqual (1900, CTest.Value.Year, "#A02");
                        Assert.AreEqual (1, CTest.Value.Month, "#A03");
                        Assert.AreEqual (1, CTest.Value.Day, "#A04");
                        Assert.AreEqual (0, CTest.Value.Hour, "#A05");

			// SqlDateTime (int, int, int, int, int, int)
			CTest = new SqlDateTime (5000, 12, 31);
			Assert.AreEqual (5000, CTest.Value.Year, "#A06");
			Assert.AreEqual (12, CTest.Value.Month, "#A07");
			Assert.AreEqual (31, CTest.Value.Day, "#A08");

			// SqlDateTime (int, int, int, int, int, int, double)
			CTest = new SqlDateTime (1978, 5, 19, 3, 34, 0);
			Assert.AreEqual (1978, CTest.Value.Year, "#A09");
			Assert.AreEqual (5, CTest.Value.Month, "#A10");
			Assert.AreEqual (19, CTest.Value.Day, "#A11");
			Assert.AreEqual (3, CTest.Value.Hour, "#A12");
                        Assert.AreEqual (34, CTest.Value.Minute, "#A13");
			Assert.AreEqual (0, CTest.Value.Second, "#A14");
			
			try {
				CTest = new SqlDateTime (10000, 12, 31);
				Assert.Fail ("#A15");
			} catch (Exception e) {
                                Assert.AreEqual (typeof (SqlTypeException), e.GetType (), "#A16");
			}
			
			// SqlDateTime (int, int, int, int, int, int, int)
			CTest = new SqlDateTime (1978, 5, 19, 3, 34, 0, 12);
			Assert.AreEqual (1978, CTest.Value.Year, "#A17");
			Assert.AreEqual (5, CTest.Value.Month, "#A18");
			Assert.AreEqual (19, CTest.Value.Day, "#A19");
			Assert.AreEqual (3, CTest.Value.Hour, "#A20");
                        Assert.AreEqual (34, CTest.Value.Minute, "#A21");
			Assert.AreEqual (0, CTest.Value.Second, "#A22");
                        Assert.AreEqual (0, CTest.Value.Millisecond, "#A23");
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
			// MaxValue
			Assert.AreEqual (9999, SqlDateTime.MaxValue.Value.Year, "#B01");
			Assert.AreEqual (12, SqlDateTime.MaxValue.Value.Month, "#B02");
			Assert.AreEqual (31, SqlDateTime.MaxValue.Value.Day, "#B03");
			Assert.AreEqual (23, SqlDateTime.MaxValue.Value.Hour, "#B04");
			Assert.AreEqual (59, SqlDateTime.MaxValue.Value.Minute, "#B05");
			Assert.AreEqual (59, SqlDateTime.MaxValue.Value.Second, "#B06");

			// MinValue
                        Assert.AreEqual (1753, SqlDateTime.MinValue.Value.Year, "#B07");
                        Assert.AreEqual (1, SqlDateTime.MinValue.Value.Month, "#B08");
                        Assert.AreEqual (1, SqlDateTime.MinValue.Value.Day, "#B09");
                        Assert.AreEqual (0, SqlDateTime.MinValue.Value.Hour, "#B10");
                        Assert.AreEqual (0, SqlDateTime.MinValue.Value.Minute, "#B11");
                        Assert.AreEqual (0, SqlDateTime.MinValue.Value.Second, "#B12");

			// Null
			Assert.IsTrue (SqlDateTime.Null.IsNull, "#B13");

			// SQLTicksPerHour
                        Assert.AreEqual (1080000, SqlDateTime.SQLTicksPerHour, "#B14");

			// SQLTicksPerMinute
                        Assert.AreEqual (18000, SqlDateTime.SQLTicksPerMinute, "#B15");

			// SQLTicksPerSecond
                        Assert.AreEqual (300, SqlDateTime.SQLTicksPerSecond, "#B16");
                }

                // Test properties
                [Test]
		public void Properties()
                {
			// DayTicks
                        Assert.AreEqual (37546, Test1.DayTicks, "#C01");
			
			try {
				int test = SqlDateTime.Null.DayTicks;
				Assert.Fail ("#C02");
			} catch (Exception e) {
				Assert.AreEqual (typeof (SqlNullValueException), e.GetType (), "#C03");
			}
				
			// IsNull
			Assert.IsTrue (SqlDateTime.Null.IsNull, "#C04");
			Assert.IsTrue (!Test2.IsNull, "#C05");

			// TimeTicks
                        Assert.AreEqual (10440000, Test1.TimeTicks, "#C06");
			
			try {
				int test = SqlDateTime.Null.TimeTicks;
				Assert.Fail ("#C07");
			} catch (Exception e) {
				Assert.AreEqual (typeof (SqlNullValueException), e.GetType (), "#C08");
			}

			// Value
			Assert.AreEqual (2003, Test2.Value.Year, "#C09");
			Assert.AreEqual (2002, Test1.Value.Year, "#C10");
                }

                // PUBLIC METHODS

		[Test]		
                public void CompareTo()
                {
                        SqlString TestString = new SqlString ("This is a test");

                        Assert.IsTrue (Test1.CompareTo (Test3) < 0, "#D01");
                        Assert.IsTrue (Test2.CompareTo (Test1) > 0, "#D02");
                        Assert.IsTrue (Test2.CompareTo (Test3) == 0, "#D03");
                        Assert.IsTrue (Test1.CompareTo (SqlDateTime.Null) > 0, "#D04");

                        try {
                                Test1.CompareTo (TestString);
                                Assert.Fail("#D05");
                        } catch(Exception e) {
                                Assert.AreEqual (typeof (ArgumentException), e.GetType (), "#D06");
                        }
                }

		[Test]
                public void EqualsMethods()
                {
                        Assert.IsTrue (!Test1.Equals (Test2), "#E01");
                        Assert.IsTrue (!Test2.Equals (new SqlString ("TEST")), "#E03");
                        Assert.IsTrue (Test2.Equals (Test3), "#E04");

                        // Static Equals()-method
                        Assert.IsTrue (SqlDateTime.Equals (Test2, Test3).Value, "#E05");
                        Assert.IsTrue (!SqlDateTime.Equals (Test1, Test2).Value, "#E06");
                }

		[Test]
                public void GetHashCodeTest()
                {
                        // FIXME: Better way to test HashCode
                        Assert.AreEqual (Test1.GetHashCode (), Test1.GetHashCode (), "#F01");
                        Assert.IsTrue (Test2.GetHashCode () != Test1.GetHashCode (), "#F02");
                }

		[Test]
                public void GetTypeTest()
                {
                        Assert.AreEqual ("System.Data.SqlTypes.SqlDateTime", Test1.GetType ().ToString (), "#G01");
                        Assert.AreEqual ("System.DateTime", Test1.Value.GetType ().ToString (), "#G02");
                }

		[Test]
                public void Greaters()
                {
                        // GreateThan ()
                        Assert.IsTrue (!SqlDateTime.GreaterThan (Test1, Test2).Value, "#H01");
                        Assert.IsTrue (SqlDateTime.GreaterThan (Test2, Test1).Value, "#H02");
                        Assert.IsTrue (!SqlDateTime.GreaterThan (Test2, Test3).Value, "#H03");

                        // GreaterTharOrEqual ()
                        Assert.IsTrue (!SqlDateTime.GreaterThanOrEqual (Test1, Test2).Value, "#H04");
                        Assert.IsTrue (SqlDateTime.GreaterThanOrEqual (Test2, Test1).Value, "#H05");
                        Assert.IsTrue (SqlDateTime.GreaterThanOrEqual (Test2, Test3).Value, "#H06");
                }

		[Test]
                public void Lessers()
                {
                        // LessThan()
                        Assert.IsTrue (!SqlDateTime.LessThan (Test2, Test3).Value, "#I01");
                        Assert.IsTrue (!SqlDateTime.LessThan (Test2, Test1).Value, "#I02");
                        Assert.IsTrue (SqlDateTime.LessThan (Test1, Test3).Value, "#I03");

                        // LessThanOrEqual ()
                        Assert.IsTrue (SqlDateTime.LessThanOrEqual (Test1, Test2).Value, "#I04");
                        Assert.IsTrue (!SqlDateTime.LessThanOrEqual (Test2, Test1).Value, "#I05");
                        Assert.IsTrue (SqlDateTime.LessThanOrEqual (Test3, Test2).Value, "#I06");
                        Assert.IsTrue (SqlDateTime.LessThanOrEqual (Test1, SqlDateTime.Null).IsNull, "#I07");
                }

		[Test]
                public void NotEquals()
                {
                        Assert.IsTrue (SqlDateTime.NotEquals (Test1, Test2).Value, "#J01");
                        Assert.IsTrue (SqlDateTime.NotEquals (Test3, Test1).Value, "#J02");
                        Assert.IsTrue (!SqlDateTime.NotEquals (Test2, Test3).Value, "#J03");
                        Assert.IsTrue (SqlDateTime.NotEquals (SqlDateTime.Null, Test2).IsNull, "#J04");
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlDateTime.Parse (null);
                                Assert.Fail ("#K01");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (ArgumentNullException), e.GetType (), "#K02");
                        }

                        try {
                                SqlDateTime.Parse ("not-a-number");
                                Assert.Fail ("#K03");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (FormatException), e.GetType (), "#K04");
                        }

			SqlDateTime t1 = SqlDateTime.Parse ("02/25/2002");
			Assert.AreEqual (myTicks[0], t1.Value.Ticks, "#K05");

			try {
				t1 = SqlDateTime.Parse ("2002-02-25");
			} catch (Exception e) {
				Assert.Fail ("#K06 " + e);
			}

			// Thanks for Martin Baulig for these (DateTimeTest.cs)
			Assert.AreEqual (myTicks[0], t1.Value.Ticks, "#K07");
			t1 = SqlDateTime.Parse ("Monday, 25 February 2002");
			Assert.AreEqual (myTicks[0], t1.Value.Ticks, "#K08");
			t1 = SqlDateTime.Parse ("Monday, 25 February 2002 05:25");
			Assert.AreEqual (myTicks[3], t1.Value.Ticks, "#K09");
			t1 = SqlDateTime.Parse ("Monday, 25 February 2002 05:25:13");
                        Assert.AreEqual (myTicks[4], t1.Value.Ticks, "#K10");
			t1 = SqlDateTime.Parse ("02/25/2002 05:25");
			Assert.AreEqual (myTicks[3], t1.Value.Ticks, "#K11");
			t1 = SqlDateTime.Parse ("02/25/2002 05:25:13");
			Assert.AreEqual (myTicks[4], t1.Value.Ticks, "#K12");
                        t1 = SqlDateTime.Parse ("2002-02-25 04:25:13Z");
                        t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			Assert.AreEqual (2002, t1.Value.Year, "#K13");
			Assert.AreEqual (02, t1.Value.Month, "#K14");
			Assert.AreEqual (25, t1.Value.Day, "#K15");
			Assert.AreEqual (04, t1.Value.Hour, "#K16");
			Assert.AreEqual (25, t1.Value.Minute, "#K17");
			Assert.AreEqual (13, t1.Value.Second, "#K18");
			
			SqlDateTime t2 = new SqlDateTime (DateTime.Today.Year, 2, 25);
			t1 = SqlDateTime.Parse ("February 25");
			Assert.AreEqual (t2.Value.Ticks, t1.Value.Ticks, "#K19");
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = SqlDateTime.Parse ("February 08");
                        Assert.AreEqual (t2.Value.Ticks, t1.Value.Ticks, "#K20");

			t1 = SqlDateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			Assert.AreEqual (2002, t1.Value.Year, "#K21");
			Assert.AreEqual (02, t1.Value.Month, "#K22");
			Assert.AreEqual (25, t1.Value.Day, "#K23");
			Assert.AreEqual (04, t1.Value.Hour, "#K24");
			Assert.AreEqual (25, t1.Value.Minute, "#K25");
			Assert.AreEqual (13, t1.Value.Second, "#K26");

			t1 = SqlDateTime.Parse ("2002-02-25T05:25:13");
			Assert.AreEqual (myTicks[4], t1.Value.Ticks, "#K27");

                        t2 = DateTime.Today + new TimeSpan (5,25,0);
			t1 = SqlDateTime.Parse ("05:25");
			Assert.AreEqual(t2.Value.Ticks, t1.Value.Ticks, "#K28");

                        t2 = DateTime.Today + new TimeSpan (5,25,13);
			t1 = SqlDateTime.Parse ("05:25:13");
			Assert.AreEqual(t2.Value.Ticks, t1.Value.Ticks, "#K29");

			t2 = new SqlDateTime (2002, 2, 1);
			t1 = SqlDateTime.Parse ("2002 February");
			Assert.AreEqual (t2.Value.Ticks, t1.Value.Ticks, "#K30");
			
			t2 = new SqlDateTime (2002, 2, 1);
			t1 = SqlDateTime.Parse ("2002 February");
			Assert.AreEqual (t2.Value.Ticks, t1.Value.Ticks, "#K31");
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = SqlDateTime.Parse ("February 8");
			
			Assert.AreEqual (t2.Value.Ticks, t1.Value.Ticks, "#K32");
                }

		[Test]
		[Ignore ("This test is locale dependent.")]
		public void ToStringTest()
		{
			//
			// Thanks for Marting Baulig for these (DateTimeTest.cs)
			//
			
                        SqlDateTime t1 = new SqlDateTime (2002, 2, 25, 5, 25, 13);
                        SqlDateTime t2 = new SqlDateTime (2002, 2, 25, 15, 25, 13);
			
			// Standard patterns
                        Assert.AreEqual("2/25/2002 5:25:13 AM", t1.ToString (), "L01");
                        Assert.AreEqual((SqlString)"2/25/2002 5:25:13 AM", t1.ToSqlString (), "L02");
		}

                // OPERATORS
		[Test]
                public void ArithmeticOperators()
                {
			TimeSpan TestSpan = new TimeSpan (20, 1, 20, 20);
			SqlDateTime ResultDateTime;

                        // "+"-operator
                        ResultDateTime = Test1 + TestSpan;
			Assert.AreEqual (2002, ResultDateTime.Value.Year, "#M01");
			Assert.AreEqual (8, ResultDateTime.Value.Day, "#M02");
			Assert.AreEqual (11, ResultDateTime.Value.Hour, "#M03");
                        Assert.AreEqual (0, ResultDateTime.Value.Minute, "#M04");
                        Assert.AreEqual (20, ResultDateTime.Value.Second, "#M05");
			Assert.IsTrue ((SqlDateTime.Null + TestSpan).IsNull, "#M06");

                        try {
                                ResultDateTime = SqlDateTime.MaxValue + TestSpan;
                                Assert.Fail ("#M07");
                        } catch (Exception e) {
                                Assert.AreEqual (typeof (ArgumentOutOfRangeException), e.GetType (), "#M08");
                        }

                        // "-"-operator
			ResultDateTime = Test1 - TestSpan;
			Assert.AreEqual (2002, ResultDateTime.Value.Year, "#M09");
                        Assert.AreEqual (29, ResultDateTime.Value.Day, "#M10");
			Assert.AreEqual (8, ResultDateTime.Value.Hour, "#M11");
                        Assert.AreEqual (19, ResultDateTime.Value.Minute, "#M12");
                        Assert.AreEqual (40, ResultDateTime.Value.Second, "#M13");
                        Assert.IsTrue ((SqlDateTime.Null - TestSpan).IsNull, "#M14");
			
                        try {
                                ResultDateTime = SqlDateTime.MinValue - TestSpan;
                                Assert.Fail ("#M15");
                        } catch  (Exception e) {
                                Assert.AreEqual (typeof (SqlTypeException), e.GetType (), "#M16");
                        }
                }

		[Test]
                public void ThanOrEqualOperators()
                {
                        // == -operator
                        Assert.IsTrue ((Test2 == Test3).Value, "#N01");
                        Assert.IsTrue (!(Test1 == Test2).Value, "#N02");
                        Assert.IsTrue ((Test1 == SqlDateTime.Null).IsNull, "#N03");
                        
                        // != -operator
                        Assert.IsTrue (!(Test2 != Test3).Value, "#N04");
                        Assert.IsTrue ((Test1 != Test3).Value, "#N05");
                        Assert.IsTrue ((Test1 != SqlDateTime.Null).IsNull, "#N06");

                        // > -operator
                        Assert.IsTrue ((Test2 > Test1).Value, "#N07");
                        Assert.IsTrue (!(Test3 > Test2).Value, "#N08");
                        Assert.IsTrue ((Test1 > SqlDateTime.Null).IsNull, "#N09");

                        // >=  -operator
                        Assert.IsTrue (!(Test1 >= Test3).Value, "#N10");
                        Assert.IsTrue ((Test3 >= Test1).Value, "#N11");
                        Assert.IsTrue ((Test2 >= Test3).Value, "#N12");
                        Assert.IsTrue ((Test1 >= SqlDateTime.Null).IsNull, "#N13");

                        // < -operator
                        Assert.IsTrue (!(Test2 < Test1).Value, "#N14");
                        Assert.IsTrue ((Test1 < Test3).Value, "#N15");
                        Assert.IsTrue (!(Test2 < Test3).Value, "#N16");
                        Assert.IsTrue ((Test1 < SqlDateTime.Null).IsNull, "#N17");

                        // <= -operator
                        Assert.IsTrue ((Test1 <= Test3).Value, "#N18");
                        Assert.IsTrue (!(Test3 <= Test1).Value, "#N19");
                        Assert.IsTrue ((Test2 <= Test3).Value, "#N20");
                        Assert.IsTrue ((Test1 <= SqlDateTime.Null).IsNull, "#N21");
                }

		[Test]
		public void SqlDateTimeToDateTime()
		{
			Assert.AreEqual (2002, ((DateTime)Test1).Year, "O01");
			Assert.AreEqual (2003, ((DateTime)Test2).Year, "O03");
			Assert.AreEqual (10, ((DateTime)Test1).Month, "O04");
			Assert.AreEqual (19, ((DateTime)Test1).Day, "O05");
			Assert.AreEqual (9, ((DateTime)Test1).Hour, "O06");
			Assert.AreEqual (40, ((DateTime)Test1).Minute, "O07");
                        Assert.AreEqual (0, ((DateTime)Test1).Second, "O08");
		}

		[Test]
		public void SqlStringToSqlDateTime()
		{

			SqlString TestString = new SqlString ("02/25/2002");
                        SqlDateTime t1 = (SqlDateTime)TestString;

			Assert.AreEqual (myTicks[0], t1.Value.Ticks, "#P01");

			// Thanks for Martin Baulig for these (DateTimeTest.cs)
			Assert.AreEqual (myTicks[0], t1.Value.Ticks, "#P02");
			t1 = (SqlDateTime) new SqlString ("Monday, 25 February 2002");
			Assert.AreEqual (myTicks[0], t1.Value.Ticks, "#P04");
			t1 = (SqlDateTime) new SqlString ("Monday, 25 February 2002 05:25");
			Assert.AreEqual (myTicks[3], t1.Value.Ticks, "#P05");
			t1 = (SqlDateTime) new SqlString ("Monday, 25 February 2002 05:25:13");
			Assert.AreEqual (myTicks[4], t1.Value.Ticks, "#P05");
			t1 = (SqlDateTime) new SqlString ("02/25/2002 05:25");
			Assert.AreEqual (myTicks[3], t1.Value.Ticks, "#P06");
			t1 = (SqlDateTime) new SqlString ("02/25/2002 05:25:13");
			Assert.AreEqual (myTicks[4], t1.Value.Ticks, "#P07");
			t1 = (SqlDateTime) new SqlString ("2002-02-25 04:25:13Z");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			Assert.AreEqual (2002, t1.Value.Year, "#P08");
			Assert.AreEqual (02, t1.Value.Month, "#P09");
			Assert.AreEqual (25, t1.Value.Day, "#P10");
			Assert.AreEqual (04, t1.Value.Hour, "#P11");
			Assert.AreEqual (25, t1.Value.Minute, "#P12");
			Assert.AreEqual (13, t1.Value.Second, "#P13");
			
			SqlDateTime t2 = new SqlDateTime (DateTime.Today.Year, 2, 25);
			t1 = (SqlDateTime) new SqlString ("February 25");
			Assert.AreEqual (t2.Value.Ticks, t1.Value.Ticks, "#P14");
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = (SqlDateTime) new SqlString ("February 08");
			Assert.AreEqual (t2.Value.Ticks, t1.Value.Ticks, "#P15");

			t1 = (SqlDateTime) new SqlString ("Mon, 25 Feb 2002 04:25:13 GMT");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			Assert.AreEqual (2002, t1.Value.Year, "#P16");
			Assert.AreEqual (02, t1.Value.Month, "#P17");
			Assert.AreEqual (25, t1.Value.Day, "#P18");
			Assert.AreEqual (04, t1.Value.Hour, "#P19");
			Assert.AreEqual (25, t1.Value.Minute, "#P20");
			Assert.AreEqual (13, t1.Value.Second, "#P21");

			t1 = (SqlDateTime) new SqlString ("2002-02-25T05:25:13");
			Assert.AreEqual (myTicks[4], t1.Value.Ticks, "#P22");

                        t2 = DateTime.Today + new TimeSpan (5,25,0);
			t1 = (SqlDateTime) new SqlString ("05:25");
			Assert.AreEqual(t2.Value.Ticks, t1.Value.Ticks, "#P23");

                        t2 = DateTime.Today + new TimeSpan (5,25,13);
			t1 = (SqlDateTime) new SqlString ("05:25:13");
			Assert.AreEqual(t2.Value.Ticks, t1.Value.Ticks, "#P24");

			t2 = new SqlDateTime (2002, 2, 1);
			t1 = (SqlDateTime) new SqlString ("2002 February");
			Assert.AreEqual (t2.Value.Ticks, t1.Value.Ticks, "#P25");
			
			t2 = new SqlDateTime (2002, 2, 1);
			t1 = (SqlDateTime) new SqlString ("2002 February");
			Assert.AreEqual (t2.Value.Ticks, t1.Value.Ticks, "#P26");
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = (SqlDateTime) new SqlString ("February 8");
			
			Assert.AreEqual (t2.Value.Ticks, t1.Value.Ticks, "#P27");
		}

		[Test]
		public void DateTimeToSqlDateTime()
		{
			DateTime DateTimeTest = new DateTime (2002, 10, 19, 11, 53, 4);
			SqlDateTime Result = (SqlDateTime)DateTimeTest;
			Assert.AreEqual (2002, Result.Value.Year, "#Q01");
			Assert.AreEqual (10, Result.Value.Month, "#Q02");
			Assert.AreEqual (19, Result.Value.Day, "#Q03");
			Assert.AreEqual (11, Result.Value.Hour, "#Q04");
       			Assert.AreEqual (53, Result.Value.Minute, "#Q05");
			Assert.AreEqual (4, Result.Value.Second, "#Q06");
		}

		[Test]
		public void TicksRoundTrip ()
		{
			SqlDateTime d1 = new SqlDateTime (2007, 05, 04, 18, 02, 40, 398.25);
			SqlDateTime d2 = new SqlDateTime (d1.DayTicks, d1.TimeTicks);

			Assert.AreEqual (39204, d1.DayTicks, "#R01");
			Assert.AreEqual (19488119, d1.TimeTicks, "#R02");
			Assert.AreEqual (633138985603970000, d1.Value.Ticks, "#R03");
			Assert.AreEqual (d1.DayTicks, d2.DayTicks, "#R04");
			Assert.AreEqual (d1.TimeTicks, d2.TimeTicks, "#R05");
			Assert.AreEqual (d1.Value.Ticks, d2.Value.Ticks, "#R06");
			Assert.AreEqual (d1, d2, "#R07");
		}

		[Test]
		public void EffingBilisecond ()
		{
			SqlDateTime d1 = new SqlDateTime (2007, 05, 04, 18, 02, 40, 398252);

			Assert.AreEqual (39204, d1.DayTicks, "#S01");
			Assert.AreEqual (19488119, d1.TimeTicks, "#S02");
			Assert.AreEqual (633138985603970000, d1.Value.Ticks, "#R03");
		}

#if NET_2_0
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlDateTime.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("dateTime", qualifiedName.Name, "#A01");
		}
#endif
        }
}

