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

using NUnit.Framework;
using System;
using System.Data.SqlTypes;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlDateTimeTest : Assertion {

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
			AssertEquals ("#A01", 2002, CTest.Value.Year);
				
			// SqlDateTime (int, int)
                        CTest = new SqlDateTime (0, 0);
			
			// SqlDateTime (int, int, int)
                        AssertEquals ("#A02", 1900, CTest.Value.Year);
                        AssertEquals ("#A03", 1, CTest.Value.Month);
                        AssertEquals ("#A04", 1, CTest.Value.Day);
                        AssertEquals ("#A05", 0, CTest.Value.Hour);

			// SqlDateTime (int, int, int, int, int, int)
			CTest = new SqlDateTime (5000, 12, 31);
			AssertEquals ("#A06", 5000, CTest.Value.Year);
			AssertEquals ("#A07", 12, CTest.Value.Month);
			AssertEquals ("#A08", 31, CTest.Value.Day);

			// SqlDateTime (int, int, int, int, int, int, double)
			CTest = new SqlDateTime (1978, 5, 19, 3, 34, 0);
			AssertEquals ("#A09", 1978, CTest.Value.Year);
			AssertEquals ("#A10", 5, CTest.Value.Month);
			AssertEquals ("#A11", 19, CTest.Value.Day);
			AssertEquals ("#A12", 3, CTest.Value.Hour);
                        AssertEquals ("#A13", 34, CTest.Value.Minute);
			AssertEquals ("#A14", 0, CTest.Value.Second);
			
			try {
				CTest = new SqlDateTime (10000, 12, 31);
				Fail ("#A15");
			} catch (Exception e) {
                                AssertEquals ("#A16", typeof (SqlTypeException),
					      e.GetType ());
			}
			
			// SqlDateTime (int, int, int, int, int, int, int)
			CTest = new SqlDateTime (1978, 5, 19, 3, 34, 0, 12);
			AssertEquals ("#A17", 1978, CTest.Value.Year);
			AssertEquals ("#A18", 5, CTest.Value.Month);
			AssertEquals ("#A19", 19, CTest.Value.Day);
			AssertEquals ("#A20", 3, CTest.Value.Hour);
                        AssertEquals ("#A21", 34, CTest.Value.Minute);
			AssertEquals ("#A22", 0, CTest.Value.Second);
                        AssertEquals ("#A23", 0, CTest.Value.Millisecond);
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
			// MaxValue
			AssertEquals ("#B01", 9999, SqlDateTime.MaxValue.Value.Year);
			AssertEquals ("#B02", 12, SqlDateTime.MaxValue.Value.Month);
			AssertEquals ("#B03", 31, SqlDateTime.MaxValue.Value.Day);
			AssertEquals ("#B04", 23, SqlDateTime.MaxValue.Value.Hour);
			AssertEquals ("#B05", 59, SqlDateTime.MaxValue.Value.Minute);
			AssertEquals ("#B06", 59, SqlDateTime.MaxValue.Value.Second);

			// MinValue
                        AssertEquals ("#B07", 1753, SqlDateTime.MinValue.Value.Year);
                        AssertEquals ("#B08", 1, SqlDateTime.MinValue.Value.Month);
                        AssertEquals ("#B09", 1, SqlDateTime.MinValue.Value.Day);
                        AssertEquals ("#B10", 0, SqlDateTime.MinValue.Value.Hour);
                        AssertEquals ("#B11", 0, SqlDateTime.MinValue.Value.Minute);
                        AssertEquals ("#B12", 0, SqlDateTime.MinValue.Value.Second);

			// Null
			Assert ("#B13", SqlDateTime.Null.IsNull);

			// SQLTicksPerHour
                        AssertEquals ("#B14", 1080000, SqlDateTime.SQLTicksPerHour);

			// SQLTicksPerMinute
                        AssertEquals ("#B15", 18000, SqlDateTime.SQLTicksPerMinute);

			// SQLTicksPerSecond
                        AssertEquals ("#B16", 300, SqlDateTime.SQLTicksPerSecond);
                }

                // Test properties
                [Test]
		public void Properties()
                {
			// DayTicks
                        AssertEquals ("#C01", 37546, Test1.DayTicks);
			
			try {
				int test = SqlDateTime.Null.DayTicks;
				Fail ("#C02");
			} catch (Exception e) {
				AssertEquals ("#C03", typeof (SqlNullValueException), 
					      e.GetType ());
			}
				
			// IsNull
			Assert ("#C04", SqlDateTime.Null.IsNull);
			Assert ("#C05", !Test2.IsNull);

			// TimeTicks
                        AssertEquals ("#C06", 10440000, Test1.TimeTicks);
			
			try {
				int test = SqlDateTime.Null.TimeTicks;
				Fail ("#C07");
			} catch (Exception e) {
				AssertEquals ("#C08", typeof (SqlNullValueException), 
					      e.GetType ());
			}

			// Value
			AssertEquals ("#C09", 2003, Test2.Value.Year);
			AssertEquals ("#C10", 2002, Test1.Value.Year);			
                }

                // PUBLIC METHODS

		[Test]		
                public void CompareTo()
                {
                        SqlString TestString = new SqlString ("This is a test");

                        Assert ("#D01", Test1.CompareTo (Test3) < 0);
                        Assert ("#D02", Test2.CompareTo (Test1) > 0);
                        Assert ("#D03", Test2.CompareTo (Test3) == 0);
                        Assert ("#D04", Test1.CompareTo (SqlDateTime.Null) > 0);

                        try {
                                Test1.CompareTo (TestString);
                                Fail("#D05");
                        } catch(Exception e) {
                                AssertEquals ("#D06", typeof (ArgumentException), e.GetType ());
                        }
                }

		[Test]
                public void EqualsMethods()
                {
                        Assert ("#E01", !Test1.Equals (Test2));
                        Assert ("#E03", !Test2.Equals (new SqlString ("TEST")));
                        Assert ("#E04", Test2.Equals (Test3));

                        // Static Equals()-method
                        Assert ("#E05", SqlDateTime.Equals (Test2, Test3).Value);
                        Assert ("#E06", !SqlDateTime.Equals (Test1, Test2).Value);
                }

		[Test]
                public void GetHashCodeTest()
                {
                        // FIXME: Better way to test HashCode
                        AssertEquals ("#F01", Test1.GetHashCode (), Test1.GetHashCode ());
                        Assert ("#F02", Test2.GetHashCode () != Test1.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
                        AssertEquals ("#G01", "System.Data.SqlTypes.SqlDateTime", 
				      Test1.GetType ().ToString ());
                        AssertEquals ("#G02", "System.DateTime", 
				      Test1.Value.GetType ().ToString ());
                }

		[Test]
                public void Greaters()
                {
                        // GreateThan ()
                        Assert ("#H01", !SqlDateTime.GreaterThan (Test1, Test2).Value);
                        Assert ("#H02", SqlDateTime.GreaterThan (Test2, Test1).Value);
                        Assert ("#H03", !SqlDateTime.GreaterThan (Test2, Test3).Value);

                        // GreaterTharOrEqual ()
                        Assert ("#H04", !SqlDateTime.GreaterThanOrEqual (Test1, Test2).Value);
                        Assert ("#H05", SqlDateTime.GreaterThanOrEqual (Test2, Test1).Value);
                        Assert ("#H06", SqlDateTime.GreaterThanOrEqual (Test2, Test3).Value);
                }

		[Test]
                public void Lessers()
                {
                        // LessThan()
                        Assert ("#I01", !SqlDateTime.LessThan (Test2, Test3).Value);
                        Assert ("#I02", !SqlDateTime.LessThan (Test2, Test1).Value);
                        Assert ("#I03", SqlDateTime.LessThan (Test1, Test3).Value);

                        // LessThanOrEqual ()
                        Assert ("#I04", SqlDateTime.LessThanOrEqual (Test1, Test2).Value);
                        Assert ("#I05", !SqlDateTime.LessThanOrEqual (Test2, Test1).Value);
                        Assert ("#I06", SqlDateTime.LessThanOrEqual (Test3, Test2).Value);
                        Assert ("#I07", SqlDateTime.LessThanOrEqual (Test1, SqlDateTime.Null).IsNull);
                }

		[Test]
                public void NotEquals()
                {
                        Assert ("#J01", SqlDateTime.NotEquals (Test1, Test2).Value);
                        Assert ("#J02", SqlDateTime.NotEquals (Test3, Test1).Value);
                        Assert ("#J03", !SqlDateTime.NotEquals (Test2, Test3).Value);
                        Assert ("#J04", SqlDateTime.NotEquals (SqlDateTime.Null, Test2).IsNull);
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlDateTime.Parse (null);
                                Fail ("#K01");
                        } catch (Exception e) {
                                AssertEquals ("#K02", typeof (ArgumentNullException), 
					      e.GetType ());
                        }

                        try {
                                SqlDateTime.Parse ("not-a-number");
                                Fail ("#K03");
                        } catch (Exception e) {
                                AssertEquals ("#K04", typeof (FormatException), 
					      e.GetType ());
                        }

			SqlDateTime t1 = SqlDateTime.Parse ("02/25/2002");
			AssertEquals ("#K05", myTicks[0], t1.Value.Ticks);

			try {
				t1 = SqlDateTime.Parse ("2002-02-25");
			} catch (Exception e) {
				Fail ("#K06 " + e);
			}

			// Thanks for Martin Baulig for these (DateTimeTest.cs)
			AssertEquals ("#K07", myTicks[0], t1.Value.Ticks);
			t1 = SqlDateTime.Parse ("Monday, 25 February 2002");
			AssertEquals ("#K08", myTicks[0], t1.Value.Ticks);
			t1 = SqlDateTime.Parse ("Monday, 25 February 2002 05:25");
			AssertEquals ("#K09", myTicks[3], t1.Value.Ticks);
			t1 = SqlDateTime.Parse ("Monday, 25 February 2002 05:25:13");
                        AssertEquals ("#K10", myTicks[4], t1.Value.Ticks);
			t1 = SqlDateTime.Parse ("02/25/2002 05:25");
			AssertEquals ("#K11", myTicks[3], t1.Value.Ticks);
			t1 = SqlDateTime.Parse ("02/25/2002 05:25:13");
			AssertEquals ("#K12", myTicks[4], t1.Value.Ticks);
                        t1 = SqlDateTime.Parse ("2002-02-25 04:25:13Z");
                        t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			AssertEquals ("#K13", 2002, t1.Value.Year);
			AssertEquals ("#K14", 02, t1.Value.Month);
			AssertEquals ("#K15", 25, t1.Value.Day);
			AssertEquals ("#K16", 04, t1.Value.Hour);
			AssertEquals ("#K17", 25, t1.Value.Minute);
			AssertEquals ("#K18", 13, t1.Value.Second);
			
			SqlDateTime t2 = new SqlDateTime (DateTime.Today.Year, 2, 25);
			t1 = SqlDateTime.Parse ("February 25");
			AssertEquals ("#K19", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = SqlDateTime.Parse ("February 08");
                        AssertEquals ("#K20", t2.Value.Ticks, t1.Value.Ticks);

			t1 = SqlDateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			AssertEquals ("#K21", 2002, t1.Value.Year);
			AssertEquals ("#K22", 02, t1.Value.Month);
			AssertEquals ("#K23", 25, t1.Value.Day);
			AssertEquals ("#K24", 04, t1.Value.Hour);
			AssertEquals ("#K25", 25, t1.Value.Minute);
			AssertEquals ("#K26", 13, t1.Value.Second);

			t1 = SqlDateTime.Parse ("2002-02-25T05:25:13");
			AssertEquals ("#K27", myTicks[4], t1.Value.Ticks);

                        t2 = DateTime.Today + new TimeSpan (5,25,0);
			t1 = SqlDateTime.Parse ("05:25");
			AssertEquals("#K28", t2.Value.Ticks, t1.Value.Ticks);

                        t2 = DateTime.Today + new TimeSpan (5,25,13);
			t1 = SqlDateTime.Parse ("05:25:13");
			AssertEquals("#K29", t2.Value.Ticks, t1.Value.Ticks);

			t2 = new SqlDateTime (2002, 2, 1);
			t1 = SqlDateTime.Parse ("2002 February");
			AssertEquals ("#K30", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (2002, 2, 1);
			t1 = SqlDateTime.Parse ("2002 February");
			AssertEquals ("#K31", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = SqlDateTime.Parse ("February 8");
			
			AssertEquals ("#K32", t2.Value.Ticks, t1.Value.Ticks);
                }

		[Test]
		public void ToStringTest()
		{
			//
			// Thanks for Marting Baulig for these (DateTimeTest.cs)
			//
			
                        SqlDateTime t1 = new SqlDateTime (2002, 2, 25, 5, 25, 13);
                        SqlDateTime t2 = new SqlDateTime (2002, 2, 25, 15, 25, 13);
			
			// Standard patterns
                        AssertEquals("L01", "25.2.2002 5:25:13", t1.ToString ());
                        AssertEquals("L02", (SqlString)"25.2.2002 5:25:13", t1.ToSqlString ());
		}

                // OPERATORS
		[Test]
                public void ArithmeticOperators()
                {
			TimeSpan TestSpan = new TimeSpan (20, 1, 20, 20);
			SqlDateTime ResultDateTime;

                        // "+"-operator
                        ResultDateTime = Test1 + TestSpan;
			AssertEquals ("#M01", 2002, ResultDateTime.Value.Year);
			AssertEquals ("#M02", 8, ResultDateTime.Value.Day);
			AssertEquals ("#M03", 11, ResultDateTime.Value.Hour);
                        AssertEquals ("#M04", 0, ResultDateTime.Value.Minute);
                        AssertEquals ("#M05", 20, ResultDateTime.Value.Second);     
			Assert ("#M06", (SqlDateTime.Null + TestSpan).IsNull);

                        try {
                                ResultDateTime = SqlDateTime.MaxValue + TestSpan;
                                Fail ("#M07");
                        } catch (Exception e) {
                                AssertEquals ("#M08", typeof (ArgumentOutOfRangeException), e.GetType ());
                        }

                        // "-"-operator
			ResultDateTime = Test1 - TestSpan;
			AssertEquals ("#M09", 2002, ResultDateTime.Value.Year);
                        AssertEquals ("#M10", 29, ResultDateTime.Value.Day);
			AssertEquals ("#M11", 8, ResultDateTime.Value.Hour);
                        AssertEquals ("#M12", 19, ResultDateTime.Value.Minute);
                        AssertEquals ("#M13", 40, ResultDateTime.Value.Second);     
                        Assert ("#M14", (SqlDateTime.Null - TestSpan).IsNull);
			
                        try {
                                ResultDateTime = SqlDateTime.MinValue - TestSpan;
                                Fail ("#M15");
                        } catch  (Exception e) {
                                AssertEquals ("#M16", typeof (SqlTypeException), e.GetType ());
                        }
                }

		[Test]
                public void ThanOrEqualOperators()
                {
                        // == -operator
                        Assert ("#N01", (Test2 == Test3).Value);
                        Assert ("#N02", !(Test1 == Test2).Value);
                        Assert ("#N03", (Test1 == SqlDateTime.Null).IsNull);
                        
                        // != -operator
                        Assert ("#N04", !(Test2 != Test3).Value);
                        Assert ("#N05", (Test1 != Test3).Value);
                        Assert ("#N06", (Test1 != SqlDateTime.Null).IsNull);

                        // > -operator
                        Assert ("#N07", (Test2 > Test1).Value);
                        Assert ("#N08", !(Test3 > Test2).Value);
                        Assert ("#N09", (Test1 > SqlDateTime.Null).IsNull);

                        // >=  -operator
                        Assert ("#N10", !(Test1 >= Test3).Value);
                        Assert ("#N11", (Test3 >= Test1).Value);
                        Assert ("#N12", (Test2 >= Test3).Value);
                        Assert ("#N13", (Test1 >= SqlDateTime.Null).IsNull);

                        // < -operator
                        Assert ("#N14", !(Test2 < Test1).Value);
                        Assert ("#N15", (Test1 < Test3).Value);
                        Assert ("#N16", !(Test2 < Test3).Value);
                        Assert ("#N17", (Test1 < SqlDateTime.Null).IsNull);

                        // <= -operator
                        Assert ("#N18", (Test1 <= Test3).Value);
                        Assert ("#N19", !(Test3 <= Test1).Value);
                        Assert ("#N20", (Test2 <= Test3).Value);
                        Assert ("#N21", (Test1 <= SqlDateTime.Null).IsNull);
                }

		[Test]
		public void SqlDateTimeToDateTime()
		{
			AssertEquals ("O01", 2002, ((DateTime)Test1).Year);
			AssertEquals ("O03", 2003, ((DateTime)Test2).Year);
			AssertEquals ("O04", 10, ((DateTime)Test1).Month);
			AssertEquals ("O05", 19, ((DateTime)Test1).Day);
			AssertEquals ("O06", 9, ((DateTime)Test1).Hour);
			AssertEquals ("O07", 40, ((DateTime)Test1).Minute);
                        AssertEquals ("O08", 0, ((DateTime)Test1).Second);
		}

		[Test]
		public void SqlStringToSqlDateTime()
		{

			SqlString TestString = new SqlString ("02/25/2002");
                        SqlDateTime t1 = (SqlDateTime)TestString;

			AssertEquals ("#P01", myTicks[0], t1.Value.Ticks);

			// Thanks for Martin Baulig for these (DateTimeTest.cs)
			AssertEquals ("#P02", myTicks[0], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("Monday, 25 February 2002");
			AssertEquals ("#P04", myTicks[0], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("Monday, 25 February 2002 05:25");
			AssertEquals ("#P05", myTicks[3], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("Monday, 25 February 2002 05:25:13");
			AssertEquals ("#P05", myTicks[4], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("02/25/2002 05:25");
			AssertEquals ("#P06", myTicks[3], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("02/25/2002 05:25:13");
			AssertEquals ("#P07", myTicks[4], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("2002-02-25 04:25:13Z");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			AssertEquals ("#P08", 2002, t1.Value.Year);
			AssertEquals ("#P09", 02, t1.Value.Month);
			AssertEquals ("#P10", 25, t1.Value.Day);
			AssertEquals ("#P11", 04, t1.Value.Hour);
			AssertEquals ("#P12", 25, t1.Value.Minute);
			AssertEquals ("#P13", 13, t1.Value.Second);
			
			SqlDateTime t2 = new SqlDateTime (DateTime.Today.Year, 2, 25);
			t1 = (SqlDateTime) new SqlString ("February 25");
			AssertEquals ("#P14", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = (SqlDateTime) new SqlString ("February 08");
			AssertEquals ("#P15", t2.Value.Ticks, t1.Value.Ticks);

			t1 = (SqlDateTime) new SqlString ("Mon, 25 Feb 2002 04:25:13 GMT");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			AssertEquals ("#P16", 2002, t1.Value.Year);
			AssertEquals ("#P17", 02, t1.Value.Month);
			AssertEquals ("#P18", 25, t1.Value.Day);
			AssertEquals ("#P19", 04, t1.Value.Hour);
			AssertEquals ("#P20", 25, t1.Value.Minute);
			AssertEquals ("#P21", 13, t1.Value.Second);

			t1 = (SqlDateTime) new SqlString ("2002-02-25T05:25:13");
			AssertEquals ("#P22", myTicks[4], t1.Value.Ticks);

                        t2 = DateTime.Today + new TimeSpan (5,25,0);
			t1 = (SqlDateTime) new SqlString ("05:25");
			AssertEquals("#P23", t2.Value.Ticks, t1.Value.Ticks);

                        t2 = DateTime.Today + new TimeSpan (5,25,13);
			t1 = (SqlDateTime) new SqlString ("05:25:13");
			AssertEquals("#P24", t2.Value.Ticks, t1.Value.Ticks);

			t2 = new SqlDateTime (2002, 2, 1);
			t1 = (SqlDateTime) new SqlString ("2002 February");
			AssertEquals ("#P25", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (2002, 2, 1);
			t1 = (SqlDateTime) new SqlString ("2002 February");
			AssertEquals ("#P26", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = (SqlDateTime) new SqlString ("February 8");
			
			AssertEquals ("#P27", t2.Value.Ticks, t1.Value.Ticks);
		}

		[Test]
		public void DateTimeToSqlDateTime()
		{
			DateTime DateTimeTest = new DateTime (2002, 10, 19, 11, 53, 4);
			SqlDateTime Result = (SqlDateTime)DateTimeTest;
			AssertEquals ("#Q01", 2002, Result.Value.Year);
			AssertEquals ("#Q02", 10, Result.Value.Month);
			AssertEquals ("#Q03", 19, Result.Value.Day);
			AssertEquals ("#Q04", 11, Result.Value.Hour);
       			AssertEquals ("#Q05", 53, Result.Value.Minute);
			AssertEquals ("#Q06", 4, Result.Value.Second);
		}
        }
}

