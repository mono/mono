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
			Assertion.AssertEquals ("#A01", 2002, CTest.Value.Year);
				
			// SqlDateTime (int, int)
                        CTest = new SqlDateTime (0, 0);
			
			// SqlDateTime (int, int, int)
                        Assertion.AssertEquals ("#A02", 1900, CTest.Value.Year);
                        Assertion.AssertEquals ("#A03", 1, CTest.Value.Month);
                        Assertion.AssertEquals ("#A04", 1, CTest.Value.Day);
                        Assertion.AssertEquals ("#A05", 0, CTest.Value.Hour);

			// SqlDateTime (int, int, int, int, int, int)
			CTest = new SqlDateTime (5000, 12, 31);
			Assertion.AssertEquals ("#A06", 5000, CTest.Value.Year);
			Assertion.AssertEquals ("#A07", 12, CTest.Value.Month);
			Assertion.AssertEquals ("#A08", 31, CTest.Value.Day);

			// SqlDateTime (int, int, int, int, int, int, double)
			CTest = new SqlDateTime (1978, 5, 19, 3, 34, 0);
			Assertion.AssertEquals ("#A09", 1978, CTest.Value.Year);
			Assertion.AssertEquals ("#A10", 5, CTest.Value.Month);
			Assertion.AssertEquals ("#A11", 19, CTest.Value.Day);
			Assertion.AssertEquals ("#A12", 3, CTest.Value.Hour);
                        Assertion.AssertEquals ("#A13", 34, CTest.Value.Minute);
			Assertion.AssertEquals ("#A14", 0, CTest.Value.Second);
			
			try {
				CTest = new SqlDateTime (10000, 12, 31);
				Assertion.Fail ("#A15");
			} catch (Exception e) {
                                Assertion.AssertEquals ("#A16", typeof (SqlTypeException),
					      e.GetType ());
			}
			
			// SqlDateTime (int, int, int, int, int, int, int)
			CTest = new SqlDateTime (1978, 5, 19, 3, 34, 0, 12);
			Assertion.AssertEquals ("#A17", 1978, CTest.Value.Year);
			Assertion.AssertEquals ("#A18", 5, CTest.Value.Month);
			Assertion.AssertEquals ("#A19", 19, CTest.Value.Day);
			Assertion.AssertEquals ("#A20", 3, CTest.Value.Hour);
                        Assertion.AssertEquals ("#A21", 34, CTest.Value.Minute);
			Assertion.AssertEquals ("#A22", 0, CTest.Value.Second);
                        Assertion.AssertEquals ("#A23", 0, CTest.Value.Millisecond);
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
			// MaxValue
			Assertion.AssertEquals ("#B01", 9999, SqlDateTime.MaxValue.Value.Year);
			Assertion.AssertEquals ("#B02", 12, SqlDateTime.MaxValue.Value.Month);
			Assertion.AssertEquals ("#B03", 31, SqlDateTime.MaxValue.Value.Day);
			Assertion.AssertEquals ("#B04", 23, SqlDateTime.MaxValue.Value.Hour);
			Assertion.AssertEquals ("#B05", 59, SqlDateTime.MaxValue.Value.Minute);
			Assertion.AssertEquals ("#B06", 59, SqlDateTime.MaxValue.Value.Second);

			// MinValue
                        Assertion.AssertEquals ("#B07", 1753, SqlDateTime.MinValue.Value.Year);
                        Assertion.AssertEquals ("#B08", 1, SqlDateTime.MinValue.Value.Month);
                        Assertion.AssertEquals ("#B09", 1, SqlDateTime.MinValue.Value.Day);
                        Assertion.AssertEquals ("#B10", 0, SqlDateTime.MinValue.Value.Hour);
                        Assertion.AssertEquals ("#B11", 0, SqlDateTime.MinValue.Value.Minute);
                        Assertion.AssertEquals ("#B12", 0, SqlDateTime.MinValue.Value.Second);

			// Null
			Assertion.Assert ("#B13", SqlDateTime.Null.IsNull);

			// SQLTicksPerHour
                        Assertion.AssertEquals ("#B14", 1080000, SqlDateTime.SQLTicksPerHour);

			// SQLTicksPerMinute
                        Assertion.AssertEquals ("#B15", 18000, SqlDateTime.SQLTicksPerMinute);

			// SQLTicksPerSecond
                        Assertion.AssertEquals ("#B16", 300, SqlDateTime.SQLTicksPerSecond);
                }

                // Test properties
                [Test]
		public void Properties()
                {
			// DayTicks
                        Assertion.AssertEquals ("#C01", 37546, Test1.DayTicks);
			
			try {
				int test = SqlDateTime.Null.DayTicks;
				Assertion.Fail ("#C02");
			} catch (Exception e) {
				Assertion.AssertEquals ("#C03", typeof (SqlNullValueException), 
					      e.GetType ());
			}
				
			// IsNull
			Assertion.Assert ("#C04", SqlDateTime.Null.IsNull);
			Assertion.Assert ("#C05", !Test2.IsNull);

			// TimeTicks
                        Assertion.AssertEquals ("#C06", 10440000, Test1.TimeTicks);
			
			try {
				int test = SqlDateTime.Null.TimeTicks;
				Assertion.Fail ("#C07");
			} catch (Exception e) {
				Assertion.AssertEquals ("#C08", typeof (SqlNullValueException), 
					      e.GetType ());
			}

			// Value
			Assertion.AssertEquals ("#C09", 2003, Test2.Value.Year);
			Assertion.AssertEquals ("#C10", 2002, Test1.Value.Year);			
                }

                // PUBLIC METHODS

		[Test]		
                public void CompareTo()
                {
                        SqlString TestString = new SqlString ("This is a test");

                        Assertion.Assert ("#D01", Test1.CompareTo (Test3) < 0);
                        Assertion.Assert ("#D02", Test2.CompareTo (Test1) > 0);
                        Assertion.Assert ("#D03", Test2.CompareTo (Test3) == 0);
                        Assertion.Assert ("#D04", Test1.CompareTo (SqlDateTime.Null) > 0);

                        try {
                                Test1.CompareTo (TestString);
                                Assertion.Fail("#D05");
                        } catch(Exception e) {
                                Assertion.AssertEquals ("#D06", typeof (ArgumentException), e.GetType ());
                        }
                }

		[Test]
                public void EqualsMethods()
                {
                        Assertion.Assert ("#E01", !Test1.Equals (Test2));
                        Assertion.Assert ("#E03", !Test2.Equals (new SqlString ("TEST")));
                        Assertion.Assert ("#E04", Test2.Equals (Test3));

                        // Static Equals()-method
                        Assertion.Assert ("#E05", SqlDateTime.Equals (Test2, Test3).Value);
                        Assertion.Assert ("#E06", !SqlDateTime.Equals (Test1, Test2).Value);
                }

		[Test]
                public void GetHashCodeTest()
                {
                        // FIXME: Better way to test HashCode
                        Assertion.AssertEquals ("#F01", Test1.GetHashCode (), Test1.GetHashCode ());
                        Assertion.Assert ("#F02", Test2.GetHashCode () != Test1.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
                        Assertion.AssertEquals ("#G01", "System.Data.SqlTypes.SqlDateTime", 
				      Test1.GetType ().ToString ());
                        Assertion.AssertEquals ("#G02", "System.DateTime", 
				      Test1.Value.GetType ().ToString ());
                }

		[Test]
                public void Greaters()
                {
                        // GreateThan ()
                        Assertion.Assert ("#H01", !SqlDateTime.GreaterThan (Test1, Test2).Value);
                        Assertion.Assert ("#H02", SqlDateTime.GreaterThan (Test2, Test1).Value);
                        Assertion.Assert ("#H03", !SqlDateTime.GreaterThan (Test2, Test3).Value);

                        // GreaterTharOrEqual ()
                        Assertion.Assert ("#H04", !SqlDateTime.GreaterThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#H05", SqlDateTime.GreaterThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#H06", SqlDateTime.GreaterThanOrEqual (Test2, Test3).Value);
                }

		[Test]
                public void Lessers()
                {
                        // LessThan()
                        Assertion.Assert ("#I01", !SqlDateTime.LessThan (Test2, Test3).Value);
                        Assertion.Assert ("#I02", !SqlDateTime.LessThan (Test2, Test1).Value);
                        Assertion.Assert ("#I03", SqlDateTime.LessThan (Test1, Test3).Value);

                        // LessThanOrEqual ()
                        Assertion.Assert ("#I04", SqlDateTime.LessThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#I05", !SqlDateTime.LessThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#I06", SqlDateTime.LessThanOrEqual (Test3, Test2).Value);
                        Assertion.Assert ("#I07", SqlDateTime.LessThanOrEqual (Test1, SqlDateTime.Null).IsNull);
                }

		[Test]
                public void NotEquals()
                {
                        Assertion.Assert ("#J01", SqlDateTime.NotEquals (Test1, Test2).Value);
                        Assertion.Assert ("#J02", SqlDateTime.NotEquals (Test3, Test1).Value);
                        Assertion.Assert ("#J03", !SqlDateTime.NotEquals (Test2, Test3).Value);
                        Assertion.Assert ("#J04", SqlDateTime.NotEquals (SqlDateTime.Null, Test2).IsNull);
                }

		[Test]
                public void Parse()
                {
                        try {
                                SqlDateTime.Parse (null);
                                Assertion.Fail ("#K01");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#K02", typeof (ArgumentNullException), 
					      e.GetType ());
                        }

                        try {
                                SqlDateTime.Parse ("not-a-number");
                                Assertion.Fail ("#K03");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#K04", typeof (FormatException), 
					      e.GetType ());
                        }

			SqlDateTime t1 = SqlDateTime.Parse ("02/25/2002");
			Assertion.AssertEquals ("#K05", myTicks[0], t1.Value.Ticks);

			try {
				t1 = SqlDateTime.Parse ("2002-02-25");
			} catch (Exception e) {
				Assertion.Fail ("#K06 " + e);
			}

			// Thanks for Martin Baulig for these (DateTimeTest.cs)
			Assertion.AssertEquals ("#K07", myTicks[0], t1.Value.Ticks);
			t1 = SqlDateTime.Parse ("Monday, 25 February 2002");
			Assertion.AssertEquals ("#K08", myTicks[0], t1.Value.Ticks);
			t1 = SqlDateTime.Parse ("Monday, 25 February 2002 05:25");
			Assertion.AssertEquals ("#K09", myTicks[3], t1.Value.Ticks);
			t1 = SqlDateTime.Parse ("Monday, 25 February 2002 05:25:13");
                        Assertion.AssertEquals ("#K10", myTicks[4], t1.Value.Ticks);
			t1 = SqlDateTime.Parse ("02/25/2002 05:25");
			Assertion.AssertEquals ("#K11", myTicks[3], t1.Value.Ticks);
			t1 = SqlDateTime.Parse ("02/25/2002 05:25:13");
			Assertion.AssertEquals ("#K12", myTicks[4], t1.Value.Ticks);
                        t1 = SqlDateTime.Parse ("2002-02-25 04:25:13Z");
                        t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			Assertion.AssertEquals ("#K13", 2002, t1.Value.Year);
			Assertion.AssertEquals ("#K14", 02, t1.Value.Month);
			Assertion.AssertEquals ("#K15", 25, t1.Value.Day);
			Assertion.AssertEquals ("#K16", 04, t1.Value.Hour);
			Assertion.AssertEquals ("#K17", 25, t1.Value.Minute);
			Assertion.AssertEquals ("#K18", 13, t1.Value.Second);
			
			SqlDateTime t2 = new SqlDateTime (DateTime.Today.Year, 2, 25);
			t1 = SqlDateTime.Parse ("February 25");
			Assertion.AssertEquals ("#K19", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = SqlDateTime.Parse ("February 08");
                        Assertion.AssertEquals ("#K20", t2.Value.Ticks, t1.Value.Ticks);

			t1 = SqlDateTime.Parse ("Mon, 25 Feb 2002 04:25:13 GMT");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			Assertion.AssertEquals ("#K21", 2002, t1.Value.Year);
			Assertion.AssertEquals ("#K22", 02, t1.Value.Month);
			Assertion.AssertEquals ("#K23", 25, t1.Value.Day);
			Assertion.AssertEquals ("#K24", 04, t1.Value.Hour);
			Assertion.AssertEquals ("#K25", 25, t1.Value.Minute);
			Assertion.AssertEquals ("#K26", 13, t1.Value.Second);

			t1 = SqlDateTime.Parse ("2002-02-25T05:25:13");
			Assertion.AssertEquals ("#K27", myTicks[4], t1.Value.Ticks);

                        t2 = DateTime.Today + new TimeSpan (5,25,0);
			t1 = SqlDateTime.Parse ("05:25");
			Assertion.AssertEquals("#K28", t2.Value.Ticks, t1.Value.Ticks);

                        t2 = DateTime.Today + new TimeSpan (5,25,13);
			t1 = SqlDateTime.Parse ("05:25:13");
			Assertion.AssertEquals("#K29", t2.Value.Ticks, t1.Value.Ticks);

			t2 = new SqlDateTime (2002, 2, 1);
			t1 = SqlDateTime.Parse ("2002 February");
			Assertion.AssertEquals ("#K30", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (2002, 2, 1);
			t1 = SqlDateTime.Parse ("2002 February");
			Assertion.AssertEquals ("#K31", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = SqlDateTime.Parse ("February 8");
			
			Assertion.AssertEquals ("#K32", t2.Value.Ticks, t1.Value.Ticks);
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
                        Assertion.AssertEquals("L01", "25.2.2002 5:25:13", t1.ToString ());
                        Assertion.AssertEquals("L02", (SqlString)"25.2.2002 5:25:13", t1.ToSqlString ());
		}

                // OPERATORS
		[Test]
                public void ArithmeticOperators()
                {
			TimeSpan TestSpan = new TimeSpan (20, 1, 20, 20);
			SqlDateTime ResultDateTime;

                        // "+"-operator
                        ResultDateTime = Test1 + TestSpan;
			Assertion.AssertEquals ("#M01", 2002, ResultDateTime.Value.Year);
			Assertion.AssertEquals ("#M02", 8, ResultDateTime.Value.Day);
			Assertion.AssertEquals ("#M03", 11, ResultDateTime.Value.Hour);
                        Assertion.AssertEquals ("#M04", 0, ResultDateTime.Value.Minute);
                        Assertion.AssertEquals ("#M05", 20, ResultDateTime.Value.Second);     
			Assertion.Assert ("#M06", (SqlDateTime.Null + TestSpan).IsNull);

                        try {
                                ResultDateTime = SqlDateTime.MaxValue + TestSpan;
                                Assertion.Fail ("#M07");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M08", typeof (ArgumentOutOfRangeException), e.GetType ());
                        }

                        // "-"-operator
			ResultDateTime = Test1 - TestSpan;
			Assertion.AssertEquals ("#M09", 2002, ResultDateTime.Value.Year);
                        Assertion.AssertEquals ("#M10", 29, ResultDateTime.Value.Day);
			Assertion.AssertEquals ("#M11", 8, ResultDateTime.Value.Hour);
                        Assertion.AssertEquals ("#M12", 19, ResultDateTime.Value.Minute);
                        Assertion.AssertEquals ("#M13", 40, ResultDateTime.Value.Second);     
                        Assertion.Assert ("#M14", (SqlDateTime.Null - TestSpan).IsNull);
			
                        try {
                                ResultDateTime = SqlDateTime.MinValue - TestSpan;
                                Assertion.Fail ("#M15");
                        } catch  (Exception e) {
                                Assertion.AssertEquals ("#M16", typeof (SqlTypeException), e.GetType ());
                        }
                }

		[Test]
                public void ThanOrEqualOperators()
                {
                        // == -operator
                        Assertion.Assert ("#N01", (Test2 == Test3).Value);
                        Assertion.Assert ("#N02", !(Test1 == Test2).Value);
                        Assertion.Assert ("#N03", (Test1 == SqlDateTime.Null).IsNull);
                        
                        // != -operator
                        Assertion.Assert ("#N04", !(Test2 != Test3).Value);
                        Assertion.Assert ("#N05", (Test1 != Test3).Value);
                        Assertion.Assert ("#N06", (Test1 != SqlDateTime.Null).IsNull);

                        // > -operator
                        Assertion.Assert ("#N07", (Test2 > Test1).Value);
                        Assertion.Assert ("#N08", !(Test3 > Test2).Value);
                        Assertion.Assert ("#N09", (Test1 > SqlDateTime.Null).IsNull);

                        // >=  -operator
                        Assertion.Assert ("#N10", !(Test1 >= Test3).Value);
                        Assertion.Assert ("#N11", (Test3 >= Test1).Value);
                        Assertion.Assert ("#N12", (Test2 >= Test3).Value);
                        Assertion.Assert ("#N13", (Test1 >= SqlDateTime.Null).IsNull);

                        // < -operator
                        Assertion.Assert ("#N14", !(Test2 < Test1).Value);
                        Assertion.Assert ("#N15", (Test1 < Test3).Value);
                        Assertion.Assert ("#N16", !(Test2 < Test3).Value);
                        Assertion.Assert ("#N17", (Test1 < SqlDateTime.Null).IsNull);

                        // <= -operator
                        Assertion.Assert ("#N18", (Test1 <= Test3).Value);
                        Assertion.Assert ("#N19", !(Test3 <= Test1).Value);
                        Assertion.Assert ("#N20", (Test2 <= Test3).Value);
                        Assertion.Assert ("#N21", (Test1 <= SqlDateTime.Null).IsNull);
                }

		[Test]
		public void SqlDateTimeToDateTime()
		{
			Assertion.AssertEquals ("O01", 2002, ((DateTime)Test1).Year);
			Assertion.AssertEquals ("O03", 2003, ((DateTime)Test2).Year);
			Assertion.AssertEquals ("O04", 10, ((DateTime)Test1).Month);
			Assertion.AssertEquals ("O05", 19, ((DateTime)Test1).Day);
			Assertion.AssertEquals ("O06", 9, ((DateTime)Test1).Hour);
			Assertion.AssertEquals ("O07", 40, ((DateTime)Test1).Minute);
                        Assertion.AssertEquals ("O08", 0, ((DateTime)Test1).Second);
		}

		[Test]
		public void SqlStringToSqlDateTime()
		{

			SqlString TestString = new SqlString ("02/25/2002");
                        SqlDateTime t1 = (SqlDateTime)TestString;

			Assertion.AssertEquals ("#P01", myTicks[0], t1.Value.Ticks);

			// Thanks for Martin Baulig for these (DateTimeTest.cs)
			Assertion.AssertEquals ("#P02", myTicks[0], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("Monday, 25 February 2002");
			Assertion.AssertEquals ("#P04", myTicks[0], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("Monday, 25 February 2002 05:25");
			Assertion.AssertEquals ("#P05", myTicks[3], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("Monday, 25 February 2002 05:25:13");
			Assertion.AssertEquals ("#P05", myTicks[4], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("02/25/2002 05:25");
			Assertion.AssertEquals ("#P06", myTicks[3], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("02/25/2002 05:25:13");
			Assertion.AssertEquals ("#P07", myTicks[4], t1.Value.Ticks);
			t1 = (SqlDateTime) new SqlString ("2002-02-25 04:25:13Z");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			Assertion.AssertEquals ("#P08", 2002, t1.Value.Year);
			Assertion.AssertEquals ("#P09", 02, t1.Value.Month);
			Assertion.AssertEquals ("#P10", 25, t1.Value.Day);
			Assertion.AssertEquals ("#P11", 04, t1.Value.Hour);
			Assertion.AssertEquals ("#P12", 25, t1.Value.Minute);
			Assertion.AssertEquals ("#P13", 13, t1.Value.Second);
			
			SqlDateTime t2 = new SqlDateTime (DateTime.Today.Year, 2, 25);
			t1 = (SqlDateTime) new SqlString ("February 25");
			Assertion.AssertEquals ("#P14", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = (SqlDateTime) new SqlString ("February 08");
			Assertion.AssertEquals ("#P15", t2.Value.Ticks, t1.Value.Ticks);

			t1 = (SqlDateTime) new SqlString ("Mon, 25 Feb 2002 04:25:13 GMT");
			t1 = TimeZone.CurrentTimeZone.ToUniversalTime(t1.Value);
			Assertion.AssertEquals ("#P16", 2002, t1.Value.Year);
			Assertion.AssertEquals ("#P17", 02, t1.Value.Month);
			Assertion.AssertEquals ("#P18", 25, t1.Value.Day);
			Assertion.AssertEquals ("#P19", 04, t1.Value.Hour);
			Assertion.AssertEquals ("#P20", 25, t1.Value.Minute);
			Assertion.AssertEquals ("#P21", 13, t1.Value.Second);

			t1 = (SqlDateTime) new SqlString ("2002-02-25T05:25:13");
			Assertion.AssertEquals ("#P22", myTicks[4], t1.Value.Ticks);

                        t2 = DateTime.Today + new TimeSpan (5,25,0);
			t1 = (SqlDateTime) new SqlString ("05:25");
			Assertion.AssertEquals("#P23", t2.Value.Ticks, t1.Value.Ticks);

                        t2 = DateTime.Today + new TimeSpan (5,25,13);
			t1 = (SqlDateTime) new SqlString ("05:25:13");
			Assertion.AssertEquals("#P24", t2.Value.Ticks, t1.Value.Ticks);

			t2 = new SqlDateTime (2002, 2, 1);
			t1 = (SqlDateTime) new SqlString ("2002 February");
			Assertion.AssertEquals ("#P25", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (2002, 2, 1);
			t1 = (SqlDateTime) new SqlString ("2002 February");
			Assertion.AssertEquals ("#P26", t2.Value.Ticks, t1.Value.Ticks);
			
			t2 = new SqlDateTime (DateTime.Today.Year, 2, 8);
			t1 = (SqlDateTime) new SqlString ("February 8");
			
			Assertion.AssertEquals ("#P27", t2.Value.Ticks, t1.Value.Ticks);
		}

		[Test]
		public void DateTimeToSqlDateTime()
		{
			DateTime DateTimeTest = new DateTime (2002, 10, 19, 11, 53, 4);
			SqlDateTime Result = (SqlDateTime)DateTimeTest;
			Assertion.AssertEquals ("#Q01", 2002, Result.Value.Year);
			Assertion.AssertEquals ("#Q02", 10, Result.Value.Month);
			Assertion.AssertEquals ("#Q03", 19, Result.Value.Day);
			Assertion.AssertEquals ("#Q04", 11, Result.Value.Hour);
       			Assertion.AssertEquals ("#Q05", 53, Result.Value.Minute);
			Assertion.AssertEquals ("#Q06", 4, Result.Value.Second);
		}
        }
}

