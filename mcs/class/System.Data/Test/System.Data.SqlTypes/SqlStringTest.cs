// SqlStringTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlString
//
// Authors:
//   Ville Palo (vi64pa@koti.soon.fi)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Ville Palo
// (C) 2003 Martin Willemoes Hansen
// 

using NUnit.Framework;
using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Threading;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlStringTest {

                private SqlString Test1 = null;
                private SqlString Test2 = null;
                private SqlString Test3 = null;

		[SetUp]
                public void GetReady()
                {
                        Test1 = new SqlString ("First TestString");
                        Test2 = new SqlString ("This is just a test SqlString");
                        Test3 = new SqlString ("This is just a test SqlString");
                        Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-AU");
                }

                // Test constructor
		[Test]
                public void Create()
                {

                        // SqlString (String)
                        SqlString  TestString = new SqlString ("Test");
                        Assertion.AssertEquals ("#A01", "Test", TestString.Value);

                        // SqlString (String, int)
                        TestString = new SqlString ("Test", 2057);
                        Assertion.AssertEquals ("#A02", 2057, TestString.LCID);

                        // SqlString (int, SqlCompareOptions, byte[])
                        TestString = new SqlString (2057,
                                                    SqlCompareOptions.BinarySort|SqlCompareOptions.IgnoreCase,
                                                    new byte [2] {123, 221});
                        Assertion.AssertEquals ("#A03", 2057, TestString.CompareInfo.LCID);
                        
                        // SqlString(string, int, SqlCompareOptions)
                        TestString = new SqlString ("Test", 2057, SqlCompareOptions.IgnoreNonSpace);
                        Assertion.Assert ("#A04", !TestString.IsNull);
                        
                        // SqlString (int, SqlCompareOptions, byte[], bool)
                        TestString = new SqlString (2057, SqlCompareOptions.BinarySort, new byte [4] {100, 100, 200, 45}, true);
                        Assertion.AssertEquals ("#A05", (byte)63, TestString.GetNonUnicodeBytes () [0]);
                        TestString = new SqlString (2057, SqlCompareOptions.BinarySort, new byte [2] {113, 100}, false);
                        Assertion.AssertEquals ("#A06", (String)"qd", TestString.Value);
                        
                        // SqlString (int, SqlCompareOptions, byte[], int, int)
                        TestString = new SqlString (2057, SqlCompareOptions.BinarySort, new byte [2] {113, 100}, 0, 2);
                        Assertion.Assert ("#A07", !TestString.IsNull);

                        try {
                                TestString = new SqlString (2057, SqlCompareOptions.BinarySort, new byte [2] {113, 100}, 2, 1);
                                Assertion.Fail ("#A07b");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#A07c", typeof (ArgumentOutOfRangeException), e.GetType ());
                        }

                        try {
                                TestString = new SqlString (2057, SqlCompareOptions.BinarySort, new byte [2] {113, 100}, 0, 4);
                                Assertion.Fail ("#A07d");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#A07e", typeof (ArgumentOutOfRangeException), e.GetType ());
                        }

                        // SqlString (int, SqlCompareOptions, byte[], int, int, bool)
                        TestString = new SqlString (2057, SqlCompareOptions.IgnoreCase, new byte [3] {100, 111, 50}, 1, 2, false);
                        Assertion.AssertEquals ("#A08", "o2", TestString.Value);
                        TestString = new SqlString (2057, SqlCompareOptions.IgnoreCase, new byte [3] {123, 111, 222}, 1, 2, true);
                        Assertion.Assert ("#A09", !TestString.IsNull);                        
                }

                // Test public fields
		[Test]
                public void PublicFields()
                {
                        // BinarySort
                        Assertion.AssertEquals ("#B01", 32768, SqlString.BinarySort);
                        
                        // IgnoreCase
                        Assertion.AssertEquals ("#B02", 1, SqlString.IgnoreCase);
                                      
                        // IgnoreKanaType
                        Assertion.AssertEquals ("#B03", 8, SqlString.IgnoreKanaType);

                        // IgnoreNonSpace
                        Assertion.AssertEquals ("#B04", 2, SqlString.IgnoreNonSpace);
                        
                        // IgnoreWidth
                        Assertion.AssertEquals ("#B05", 16, SqlString.IgnoreWidth);
                        
                        // Null
                        Assertion.Assert ("#B06", SqlString.Null.IsNull);
                }

                // Test properties
		[Test]
                public void Properties()
                {
                        // CompareInfo
                        Assertion.AssertEquals ("#C01", 3081, Test1.CompareInfo.LCID);

                        // CultureInfo
                        Assertion.AssertEquals ("#C02", 3081, Test1.CultureInfo.LCID);                
                        
                        // IsNull
                        Assertion.Assert ("#C03", !Test1.IsNull);
                        Assertion.Assert ("#C04", SqlString.Null.IsNull);
                        
                        // LCID
                        Assertion.AssertEquals ("#C05", 3081, Test1.LCID);
                        
                        // SqlCompareOptions
                        Assertion.AssertEquals ("#C06", "IgnoreCase, IgnoreKanaType, IgnoreWidth", 
                                      Test1.SqlCompareOptions.ToString ());

                        // Value
                        Assertion.AssertEquals ("#C07", "First TestString", Test1.Value);
                }

                // PUBLIC METHODS

		[Test]
                public void CompareTo()
                {
                        SqlByte Test = new SqlByte (1);

                        Assertion.Assert ("#D01", Test1.CompareTo (Test3) < 0);
                        Assertion.Assert ("#D02", Test2.CompareTo (Test1) > 0);
                        Assertion.Assert ("#D03", Test2.CompareTo (Test3) == 0);
                        Assertion.Assert ("#D04", Test3.CompareTo (SqlString.Null) > 0);

                        try {
                                Test1.CompareTo (Test);
                                Assertion.Fail("#D05");
                        } catch(Exception e) {                        
                                Assertion.AssertEquals ("#D06", typeof (ArgumentException), e.GetType ());
                        }
                        
                        SqlString T1 = new SqlString ("test", 2057, SqlCompareOptions.IgnoreCase);
                	SqlString T2 = new SqlString ("TEST", 2057, SqlCompareOptions.None);
                	
                	try {
                		T1.CompareTo (T2);
                		Assertion.Fail ("#D07");
                	} catch (Exception e) {
                		Assertion.AssertEquals ("#D08", typeof (SqlTypeException), e.GetType ());
                	}
                	
                	// IgnoreCase
                	T1 = new SqlString ("test", 2057, SqlCompareOptions.IgnoreCase);
                	T2 = new SqlString ("TEST", 2057, SqlCompareOptions.IgnoreCase);
                	Assertion.Assert ("#D09", T2.CompareTo (T1) == 0);
                
                	T1 = new SqlString ("test", 2057);
                	T2 = new SqlString ("TEST", 2057);
                	Assertion.Assert ("#D10", T2.CompareTo (T1) == 0);

                	T1 = new SqlString ("test", 2057, SqlCompareOptions.None);
                	T2 = new SqlString ("TEST", 2057, SqlCompareOptions.None);
                	Assertion.Assert ("#D11", T2.CompareTo (T1) != 0);

			// IgnoreNonSpace
                        T1 = new SqlString ("TESTñ", 2057, SqlCompareOptions.IgnoreNonSpace);
                	T2 = new SqlString ("TESTn", 2057, SqlCompareOptions.IgnoreNonSpace);
                	Assertion.Assert ("#D12", T2.CompareTo (T1) == 0);
                
                	T1 = new SqlString ("TESTñ", 2057, SqlCompareOptions.None);
                	T2 = new SqlString ("TESTn", 2057, SqlCompareOptions.None);
                	Assertion.Assert ("#D13", T2.CompareTo (T1) != 0);

			// BinarySort
                 	T1 = new SqlString ("01_", 2057, SqlCompareOptions.BinarySort);
                	T2 = new SqlString ("_01", 2057, SqlCompareOptions.BinarySort);
                	Assertion.Assert ("#D14", T1.CompareTo (T2) < 0);
                	
                 	T1 = new SqlString ("01_", 2057, SqlCompareOptions.None);
                	T2 = new SqlString ("_01", 2057, SqlCompareOptions.None);
                	Assertion.Assert ("#D15", T1.CompareTo (T2) > 0);			
                }

		[Test]
                public void EqualsMethods()
                {
                        Assertion.Assert ("#E01", !Test1.Equals (Test2));
                        Assertion.Assert ("#E02", !Test3.Equals (Test1));
                        Assertion.Assert ("#E03", !Test2.Equals (new SqlString ("TEST")));
                        Assertion.Assert ("#E04", Test2.Equals (Test3));

                        // Static Equals()-method
                        Assertion.Assert ("#E05", SqlString.Equals (Test2, Test3).Value);
                        Assertion.Assert ("#E06", !SqlString.Equals (Test1, Test2).Value);
                }

		[Test]
                public void GetHashCodeTest()
                {
                        // FIXME: Better way to test HashCode
                        Assertion.AssertEquals ("#F01", Test1.GetHashCode (), 
                                      Test1.GetHashCode ());
                        Assertion.Assert ("#F02", Test1.GetHashCode () != Test2.GetHashCode ());
                        Assertion.Assert ("#F03", Test2.GetHashCode () == Test2.GetHashCode ());
                }

		[Test]
                public void GetTypeTest()
                {
                        Assertion.AssertEquals ("#G01", "System.Data.SqlTypes.SqlString", 
                                      Test1.GetType ().ToString ());
                        Assertion.AssertEquals ("#G02", "System.String", 
                                      Test1.Value.GetType ().ToString ());
                }

		[Test]
                public void Greaters()
                {

                        // GreateThan ()
                        Assertion.Assert ("#H01", !SqlString.GreaterThan (Test1, Test2).Value);
                        Assertion.Assert ("#H02", SqlString.GreaterThan (Test2, Test1).Value);
                        Assertion.Assert ("#H03", !SqlString.GreaterThan (Test2, Test3).Value);

                        // GreaterTharOrEqual ()
                        Assertion.Assert ("#H04", !SqlString.GreaterThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#H05", SqlString.GreaterThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#H06", SqlString.GreaterThanOrEqual (Test2, Test3).Value);
                }

		[Test]
                public void Lessers()
                {
                        // LessThan()
                        Assertion.Assert ("#I01", !SqlString.LessThan (Test2, Test3).Value);
                        Assertion.Assert ("#I02", !SqlString.LessThan (Test2, Test1).Value);
                        Assertion.Assert ("#I03", SqlString.LessThan (Test1, Test2).Value);

                        // LessThanOrEqual ()
                        Assertion.Assert ("#I04", SqlString.LessThanOrEqual (Test1, Test2).Value);
                        Assertion.Assert ("#I05", !SqlString.LessThanOrEqual (Test2, Test1).Value);
                        Assertion.Assert ("#I06", SqlString.LessThanOrEqual (Test3, Test2).Value);
                        Assertion.Assert ("#I07", SqlString.LessThanOrEqual (Test2, SqlString.Null).IsNull);
                }

		[Test]
                public void NotEquals()
                {
                        Assertion.Assert ("#J01", SqlString.NotEquals (Test1, Test2).Value);
                        Assertion.Assert ("#J02", SqlString.NotEquals (Test2, Test1).Value);
                        Assertion.Assert ("#J03", SqlString.NotEquals (Test3, Test1).Value);
                        Assertion.Assert ("#J04", !SqlString.NotEquals (Test2, Test3).Value);

                        Assertion.Assert ("#J05", SqlString.NotEquals (SqlString.Null, Test3).IsNull);
                }

		[Test]
                public void Concat()
                {
                        Test1 = new SqlString ("First TestString");
                        Test2 = new SqlString ("This is just a test SqlString");
                        Test3 = new SqlString ("This is just a test SqlString");

                        Assertion.AssertEquals ("#K01", 
                              (SqlString)"First TestStringThis is just a test SqlString", 
                              SqlString.Concat (Test1, Test2));

                        Assertion.AssertEquals ("#K02", SqlString.Null, 
                                      SqlString.Concat (Test1, SqlString.Null));
                }

		[Test]
                public void Clone()
                {
                        SqlString TestSqlString  = Test1.Clone ();
                        Assertion.AssertEquals ("#L01", Test1, TestSqlString);
                }

		[Test]
                public void CompareOptionsFromSqlCompareOptions()
                {
                        Assertion.AssertEquals ("#M01", CompareOptions.IgnoreCase,
                                    SqlString.CompareOptionsFromSqlCompareOptions (
                                    SqlCompareOptions.IgnoreCase));
                        Assertion.AssertEquals ("#M02", CompareOptions.IgnoreCase,
                                    SqlString.CompareOptionsFromSqlCompareOptions (
                                    SqlCompareOptions.IgnoreCase));
                        try {
                                
                                CompareOptions test = SqlString.CompareOptionsFromSqlCompareOptions (
                                    SqlCompareOptions.BinarySort);
                                Assertion.Fail ("#M03");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#M04", typeof (ArgumentOutOfRangeException), e.GetType ());
                        }
                }

		[Test]
                public void UnicodeBytes()
                {
                        Assertion.AssertEquals ("#N01", (byte)105, Test1.GetNonUnicodeBytes () [1]);
                        Assertion.AssertEquals ("#N02", (byte)32, Test1.GetNonUnicodeBytes () [5]);

                        Assertion.AssertEquals ("#N03", (byte)70, Test1.GetUnicodeBytes () [0]);
                        Assertion.AssertEquals ("#N03b", (byte)70, Test1.GetNonUnicodeBytes () [0]);
                        Assertion.AssertEquals ("#N03c", (byte)0, Test1.GetUnicodeBytes () [1]);
                        Assertion.AssertEquals ("#N03d", (byte)105, Test1.GetNonUnicodeBytes () [1]);
                        Assertion.AssertEquals ("#N03e", (byte)105, Test1.GetUnicodeBytes () [2]);
                        Assertion.AssertEquals ("#N03f", (byte)114, Test1.GetNonUnicodeBytes () [2]);
                        Assertion.AssertEquals ("#N03g", (byte)0, Test1.GetUnicodeBytes () [3]);
                        Assertion.AssertEquals ("#N03h", (byte)115, Test1.GetNonUnicodeBytes () [3]);
                        Assertion.AssertEquals ("#N03i", (byte)114, Test1.GetUnicodeBytes () [4]);
                        Assertion.AssertEquals ("#N03j", (byte)116, Test1.GetNonUnicodeBytes () [4]);

                        Assertion.AssertEquals ("#N04", (byte)105, Test1.GetUnicodeBytes () [2]);

                        try {
                                byte test = Test1.GetUnicodeBytes () [105];
                                Assertion.Fail ("#N05");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#N06", typeof (IndexOutOfRangeException), e.GetType());                                
                        }
                }
                      
		[Test]          
                public void Conversions()
                {

                        SqlString String250 = new SqlString ("250");
                        SqlString String9E300 = new SqlString ("9E+300");

                        // ToSqlBoolean ()
        
                        try {
                                bool test = Test1.ToSqlBoolean ().Value;                              
                                Assertion.Fail ("#01");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#01.5", typeof (FormatException), e.GetType());                                                                
                        }
                        
                        Assertion.Assert ("#O02", (new SqlString("1")).ToSqlBoolean ().Value);
                        Assertion.Assert ("#O03", !(new SqlString("0")).ToSqlBoolean ().Value);
                        Assertion.Assert ("#O04", (new SqlString("True")).ToSqlBoolean ().Value);
                        Assertion.Assert ("#O05", !(new SqlString("FALSE")).ToSqlBoolean ().Value);
                        Assertion.Assert ("#O06", SqlString.Null.ToSqlBoolean ().IsNull);

                        // ToSqlByte ()
                        try {
                                byte test = Test1.ToSqlByte ().Value;
                                Assertion.Fail ("#07");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#O07.5", typeof (FormatException), e.GetType());    
                        }

                        Assertion.AssertEquals ("#O08", (byte)250, String250.ToSqlByte ().Value);    
                        try {
                                SqlByte b = (byte)(new SqlString ("2500")).ToSqlByte ();
                                Assertion.Fail ("#O09");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#O10", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlDateTime
                        Assertion.AssertEquals ("#O11", 10, 
                                      (new SqlString ("2002-10-10")).ToSqlDateTime ().Value.Day);
                        
                        // ToSqlDecimal ()
                        try {
                                Assertion.AssertEquals ("#O13", (decimal)250, Test1.ToSqlDecimal ().Value);
                                Assertion.Fail ("#O14");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#O15", typeof (FormatException), e.GetType ());
                        }

                        Assertion.AssertEquals ("#O16", (decimal)250, String250.ToSqlDecimal ().Value);

                        try {
                                SqlDecimal test = String9E300.ToSqlDecimal ().Value;
                                Assertion.Fail ("#O17");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#O18", typeof (FormatException), e.GetType ());
                        }      

                        // ToSqlDouble
                        Assertion.AssertEquals ("#O19", (SqlDouble)9E+300, String9E300.ToSqlDouble ());

                        try {
                                SqlDouble test = (new SqlString ("4e400")).ToSqlDouble ();
                                Assertion.Fail ("#O20");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#O21", typeof (OverflowException), e.GetType ());
                        }

                        // ToSqlGuid
                        SqlString TestGuid = new SqlString("11111111-1111-1111-1111-111111111111");
                        Assertion.AssertEquals ("#O22", new SqlGuid("11111111-1111-1111-1111-111111111111"), TestGuid.ToSqlGuid ());

                        try {
                                SqlGuid test = String9E300.ToSqlGuid ();
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#O23", typeof (FormatException), e.GetType ());
                        }
                        
                        // ToSqlInt16 ()
                        Assertion.AssertEquals ("#O24", (short)250, String250.ToSqlInt16 ().Value);

                        try {
                                SqlInt16 test = String9E300.ToSqlInt16().Value;
                                Assertion.Fail ("#O25");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#O26", typeof (FormatException), e.GetType ());
                        }        

                        // ToSqlInt32 ()
                        Assertion.AssertEquals ("#O27", (int)250, String250.ToSqlInt32 ().Value);

                        try {
                                SqlInt32 test = String9E300.ToSqlInt32 ().Value;
                                Assertion.Fail ("#O28");
                        } catch (Exception e) { 
                                Assertion.AssertEquals ("#O29", typeof (FormatException), e.GetType ());
                        }

                        try {
                                SqlInt32 test = Test1.ToSqlInt32 ().Value;
                                Assertion.Fail ("#O30");
                        } catch (Exception e) { 
                                Assertion.AssertEquals ("#O31", typeof (FormatException), e.GetType ());
                        }

                        // ToSqlInt64 ()
                        Assertion.AssertEquals ("#O32", (long)250, String250.ToSqlInt64 ().Value);

                        try {        
                                SqlInt64 test = String9E300.ToSqlInt64 ().Value;
                                Assertion.Fail ("#O33");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#O34", typeof (FormatException), e.GetType ());
                        }        

                        // ToSqlMoney ()
                        Assertion.AssertEquals ("#O35", (decimal)250, String250.ToSqlMoney ().Value);

                        try {
                                SqlMoney test = String9E300.ToSqlMoney ().Value;
                                Assertion.Fail ("#O36");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#O37", typeof (FormatException), e.GetType ());
                        }        

                        // ToSqlSingle ()
                        Assertion.AssertEquals ("#O38", (float)250, String250.ToSqlSingle ().Value);

                        try {
                                SqlSingle test = String9E300.ToSqlSingle().Value;
                                Assertion.Fail ("#O39");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#O40", typeof (OverflowException), e.GetType ());
                        }        

                        // ToString ()
                        Assertion.AssertEquals ("#O41", "First TestString", Test1.ToString ());
                }

                // OPERATORS

		[Test]
                public void ArithmeticOperators()
                {
                        SqlString TestString = new SqlString ("...Testing...");
                        Assertion.AssertEquals ("#P01", (SqlString)"First TestString...Testing...",
                                      Test1 + TestString);
                        Assertion.AssertEquals ("#P02", SqlString.Null,
                                      Test1 + SqlString.Null);
                }

		[Test]
                public void ThanOrEqualOperators()
                {
                        // == -operator
                        Assertion.Assert ("#Q01", (Test2 == Test3).Value);
                        Assertion.Assert ("#Q02", !(Test1 == Test2).Value);
                        Assertion.Assert ("#Q03", (Test1 == SqlString.Null).IsNull);
                        
                        // != -operator
                        Assertion.Assert ("#Q04", !(Test3 != Test2).Value);
                        Assertion.Assert ("#Q05", !(Test2 != Test3).Value);
                        Assertion.Assert ("#Q06", (Test1 != Test3).Value);
                        Assertion.Assert ("#Q07", (Test1 != SqlString.Null).IsNull);

                        // > -operator
                        Assertion.Assert ("#Q08", (Test2 > Test1).Value);
                        Assertion.Assert ("#Q09", !(Test1 > Test3).Value);
                        Assertion.Assert ("#Q10", !(Test2 > Test3).Value);
                        Assertion.Assert ("#Q11", (Test1 > SqlString.Null).IsNull);

                        // >=  -operator
                        Assertion.Assert ("#Q12", !(Test1 >= Test3).Value);
                        Assertion.Assert ("#Q13", (Test3 >= Test1).Value);
                        Assertion.Assert ("#Q14", (Test2 >= Test3).Value);
                        Assertion.Assert ("#Q15", (Test1 >= SqlString.Null).IsNull);

                        // < -operator
                        Assertion.Assert ("#Q16", (Test1 < Test2).Value);
                        Assertion.Assert ("#Q17", (Test1 < Test3).Value);
                        Assertion.Assert ("#Q18", !(Test2 < Test3).Value);
                        Assertion.Assert ("#Q19", (Test1 < SqlString.Null).IsNull);

                        // <= -operator
                        Assertion.Assert ("#Q20", (Test1 <= Test3).Value);
                        Assertion.Assert ("#Q21", !(Test3 <= Test1).Value);
                        Assertion.Assert ("#Q22", (Test2 <= Test3).Value);
                        Assertion.Assert ("#Q23", (Test1 <= SqlString.Null).IsNull);
                }

		[Test]
                public void SqlBooleanToSqlString()
                {
                        SqlBoolean TestBoolean = new SqlBoolean (true);
                        SqlBoolean TestBoolean2 = new SqlBoolean (false);
                        SqlString Result;

                        Result = (SqlString)TestBoolean;
                        Assertion.AssertEquals ("#R01", "True", Result.Value);
                        
                        Result = (SqlString)TestBoolean2;
                        Assertion.AssertEquals ("#R02", "False", Result.Value);
                        
                        Result = (SqlString)SqlBoolean.Null;
                        Assertion.Assert ("#R03", Result.IsNull);
                }

		[Test]
                public void SqlByteToBoolean()
                {
                        SqlByte TestByte = new SqlByte (250);
                        Assertion.AssertEquals ("#S01", "250", ((SqlString)TestByte).Value);
                        try {
                                SqlString test = ((SqlString)SqlByte.Null).Value;
                                Assertion.Fail ("#S02");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#S03", typeof (SqlNullValueException), e.GetType ());
                        }
                }

		[Test]
                public void SqlDateTimeToSqlString()
                {                        
                        Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-AU");
                        SqlDateTime TestTime = new SqlDateTime(2002, 10, 22, 9, 52, 30);
                        Assertion.AssertEquals ("#T01", "22/10/2002 9:52:30 AM", ((SqlString)TestTime).Value);                        
                }
                
		[Test]
                public void SqlDecimalToSqlString()
                {
                        SqlDecimal TestDecimal = new SqlDecimal (1000.2345);
                        Assertion.AssertEquals ("#U01", "1000.2345000000000", ((SqlString)TestDecimal).Value);
                }
                
		[Test]
                public void SqlDoubleToSqlString()
                {
                        SqlDouble TestDouble = new SqlDouble (64E+64);
                        Assertion.AssertEquals ("#V01", "6.4E+65", ((SqlString)TestDouble).Value);
                }

		[Test]
                public void SqlGuidToSqlString()
                {
                        byte [] b = new byte [16];
                        b [0] = 100;
                        b [1] = 64;
                        SqlGuid TestGuid = new SqlGuid (b);
                        
                        Assertion.AssertEquals ("#W01", "00004064-0000-0000-0000-000000000000", 
                                      ((SqlString)TestGuid).Value);
                        try {
                                SqlString test = ((SqlString)SqlGuid.Null).Value;
                                Assertion.Fail ("#W02");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#W03", typeof (SqlNullValueException), e.GetType());
                        }
                }
                
		[Test]
                public void SqlInt16ToSqlString()
                {
                        SqlInt16 TestInt = new SqlInt16(20012);
                        Assertion.AssertEquals ("#X01", "20012", ((SqlString)TestInt).Value);
                        try {
                                SqlString test = ((SqlString)SqlInt16.Null).Value;
                                Assertion.Fail ("#X02");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#X03", typeof (SqlNullValueException), e.GetType ());                                
                        }
                }
                
		[Test]
                public void SqlInt32ToSqlString()
                {
                        SqlInt32 TestInt = new SqlInt32(-12456);
                        Assertion.AssertEquals ("#Y01", "-12456", ((SqlString)TestInt).Value);
                        try {
                                SqlString test = ((SqlString)SqlInt32.Null).Value;
                                Assertion.Fail ("#Y02");
                        } catch (Exception e) {
                                Assertion.AssertEquals ("#Y03", typeof (SqlNullValueException), e.GetType ());                                
                        }
                }
                
		[Test]
                public void SqlInt64ToSqlString()
                {
                        SqlInt64 TestInt = new SqlInt64(10101010);
                        Assertion.AssertEquals ("#Z01", "10101010", ((SqlString)TestInt).Value);
                }
                
		[Test]
                public void SqlMoneyToSqlString()
                {
                        SqlMoney TestMoney = new SqlMoney (646464.6464);
                        Assertion.AssertEquals ("#AA01", "646464.6464", ((SqlString)TestMoney).Value);
                }
                
		[Test]
                public void SqlSingleToSqlString()
                {
                        SqlSingle TestSingle = new SqlSingle (3E+20);
                        Assertion.AssertEquals ("#AB01", "3E+20", ((SqlString)TestSingle).Value);
                }
                      
		[Test]                        
                public void SqlStringToString()
                {
                        Assertion.AssertEquals ("#AC01", "First TestString",(String)Test1);                        
                }

		[Test]
                public void StringToSqlString()
                {
                        String TestString = "Test String";
                        Assertion.AssertEquals ("#AD01", "Test String", ((SqlString)TestString).Value);                        
                }                
        }
}

