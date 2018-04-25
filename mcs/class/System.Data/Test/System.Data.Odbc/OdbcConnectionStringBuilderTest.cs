// OdbcConnectionStringBuilderTest.cs - NUnit Test Cases for testing the
// OdbcConnectionStringBuilder Class.
//
// Authors:
//      Nidhi Rawal (rawalnidhi_rawal@yahoo.com)
//	Gert Driesen (drieseng@users.sourceforge.net)
// 
// Copyright (c) 2007 Novell Inc., and the individuals listed on the
// ChangeLog entries.
//
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#if !NO_ODBC
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Odbc
{
	[TestFixture]
	public class OdbcConnectionStringBuilderTest
	{
		[Test]
		public void ConnectionStringConstructorTest ()
		{
			OdbcConnectionStringBuilder builder;

			builder = new OdbcConnectionStringBuilder ("Driver={SQL Server};Server=(local);Database=AdventureWorks;Uid=ab;Pwd=pass@word1");
			Assert.AreEqual ("AdventureWorks", builder ["Database"],"#A1");
			Assert.AreEqual ("pass@word1", builder ["Pwd"], "#A2");
			Assert.AreEqual ("ab", builder ["Uid"], "#A3");
			Assert.AreEqual ("{SQL Server}", builder ["Driver"], "#A4");
			Assert.AreEqual ("(local)", builder ["Server"],"#A5");
			Assert.AreEqual ("Driver={SQL Server};server=(local);database=AdventureWorks;uid=ab;pwd=pass@word1", builder.ConnectionString,"#A5");

			builder = new OdbcConnectionStringBuilder ("Driver=");
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#B");

			builder = new OdbcConnectionStringBuilder ("Dsn=");
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#C");

			builder = new OdbcConnectionStringBuilder (null);
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#D"); 

			builder = new OdbcConnectionStringBuilder (string.Empty);
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#E");

			builder = new OdbcConnectionStringBuilder ("Driver=SQL {Server;Dsn=Adventu{re");
			Assert.AreEqual ("SQL {Server", builder ["Driver"], "#F1");
			Assert.AreEqual ("SQL {Server", builder.Driver, "#F2");
			Assert.AreEqual ("Adventu{re", builder ["Dsn"], "#F3");
			Assert.AreEqual ("Adventu{re", builder.Dsn, "#F4");
			Assert.AreEqual ("Dsn=Adventu{re;Driver={SQL {Server}", builder.ConnectionString, "#F5");
		}

		[Test]
		public void Add ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder.Add ("driverid", "420");
			builder.Add ("driverid", "560");
			builder.Add ("DriverID", "840");
			Assert.AreEqual ("840", builder ["driverId"], "#A1");
			Assert.IsTrue (builder.ContainsKey ("driverId"), "#A2");
			builder.Add ("Driver", "OdbcDriver");
			Assert.AreEqual ("OdbcDriver", builder.Driver, "#B1");
			Assert.AreEqual ("OdbcDriver", builder ["Driver"], "#B2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#B3");
			builder.Add ("Driver", "{OdbcDriver");
			Assert.AreEqual ("{OdbcDriver", builder.Driver, "#C1");
			Assert.AreEqual ("{OdbcDriver", builder ["Driver"], "#C2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#C3");
			builder.Add ("Dsn", "MyDsn");
			Assert.AreEqual ("MyDsn", builder.Dsn, "#D1");
			Assert.AreEqual ("MyDsn", builder ["Dsn"], "#D2");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#D3");
			builder.Add ("dsN", "MyDsn2");
			Assert.AreEqual ("MyDsn2", builder.Dsn, "#E1");
			Assert.AreEqual ("MyDsn2", builder ["Dsn"], "#E2");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#E3");
		}

		[Test]
		public void Add_Keyword_Invalid ()
		{
			string [] invalid_keywords = new string [] {
				string.Empty,
				" ",
				" abc",
				"abc ",
				"\r",
				"ab\rc",
				";abc",
				"a\0b"
				};

			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			for (int i = 0; i < invalid_keywords.Length; i++) {
				string keyword = invalid_keywords [i];
				try {
					builder.Add (keyword, "abc");
					Assert.Fail ("#1:" + i);
				} catch (ArgumentException ex) {
					// Invalid keyword, contain one or more of 'no characters',
					// 'control characters', 'leading or trailing whitespace'
					// or 'leading semicolons'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:"+ i);
					Assert.IsNull (ex.InnerException, "#3:" + i);
					Assert.IsNotNull (ex.Message, "#4:" + i);
					Assert.IsTrue (ex.Message.IndexOf ("'" + keyword + "'") == -1, "#5:" + i);
					Assert.AreEqual (keyword, ex.ParamName, "#6:" + i);
				}
			}
		}

		[Test]
		public void Add_Keyword_Null ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			try {
				builder.Add (null, "abc");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("keyword", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Clear ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["Dbq"] = "C:\\Data.xls";
			builder.Driver = "SQL Server";
			builder.Dsn = "AdventureWorks";
			builder.Add ("Port", "56");
			builder.Clear ();
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#1");
			Assert.IsFalse (builder.ContainsKey ("Dbq"), "#2");
			Assert.AreEqual (string.Empty, builder.Driver, "#3");
			Assert.AreEqual (string.Empty, builder.Dsn, "#4");
			Assert.IsFalse (builder.ContainsKey ("Port"), "#5");
		}

		[Test]
		public void ConnectionString ()
		{
			OdbcConnectionStringBuilder builder;

			builder = new OdbcConnectionStringBuilder (@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\Northwind.mdb;Jet OLEDB:System Database=|DataDirectory|\System.mdw;");
			Assert.AreEqual ("Microsoft.Jet.OLEDB.4.0", builder ["Provider"], "#A1");
			Assert.AreEqual (@"|DataDirectory|\Northwind.mdb", builder ["Data Source"], "#A2");
			Assert.AreEqual (@"|DataDirectory|\System.mdw", builder ["Jet OLEDB:System Database"], "#A3");

			builder = new OdbcConnectionStringBuilder ();
			builder ["Data SourcE"] = "(local)";
			builder ["Integrated SecuritY"] = true;
			builder.Driver = "SQL Server";
			builder ["Initial Catalog"] = "AdventureWorks;NewValue=Bad";
			Assert.AreEqual ("Driver={SQL Server};Data SourcE=(local);Integrated SecuritY=True;Initial Catalog={AdventureWorks;NewValue=Bad}",
				builder.ConnectionString, "#B");

			builder = new OdbcConnectionStringBuilder ();
			builder ["Integrated SecuritY"] = false;
			builder.Driver = "SQL Server";
			builder ["Data SourcE"] = "mother";
			builder ["Initial Catalog"] = "AdventureWorks;NewValue=OK";
			Assert.AreEqual ("Driver={SQL Server};Integrated SecuritY=False;Data SourcE=mother;Initial Catalog={AdventureWorks;NewValue=OK}",
				builder.ConnectionString, "#C");

			builder = new OdbcConnectionStringBuilder ();
			builder ["Initial Catalog"] = "AdventureWorks;NewValue=OK";
			builder.Driver = "SQL Server";
			builder.Dsn = "NorthWind";
			builder ["Data Source"] = "mother";
			Assert.AreEqual ("Dsn=NorthWind;Driver={SQL Server};Initial Catalog={AdventureWorks;NewValue=OK};Data Source=mother",
				builder.ConnectionString, "#D1");
			builder.Driver = string.Empty;
			Assert.AreEqual ("Dsn=NorthWind;Driver=;Initial Catalog={AdventureWorks;NewValue=OK};Data Source=mother",
				builder.ConnectionString, "#D2");

			builder = new OdbcConnectionStringBuilder ();
			builder ["Driver"] = "MySQL";
			builder.Driver = "SQL Server";
			builder.Dsn = "NorthWind";
			builder ["Dsn"] = "AdventureWorks";
			Assert.AreEqual ("Dsn=AdventureWorks;Driver={SQL Server}", builder.ConnectionString, "#E1");
			builder ["Dsn"] = string.Empty;
			Assert.AreEqual ("Dsn=;Driver={SQL Server}", builder.ConnectionString, "#E2");

			builder = new OdbcConnectionStringBuilder (@"Driver={Microsoft Excel Driver (*.xls)};DBQ=c:\bin\book1.xls"); 
			Assert.AreEqual ("{Microsoft Excel Driver (*.xls)}", builder ["Driver"], "#F1");
			Assert.AreEqual (@"c:\bin\book1.xls", builder ["DBQ"], "#F2");
			
			builder = new OdbcConnectionStringBuilder (@"Driver={Microsoft Text Driver (*.txt; *.csv)};DBQ=c:\bin");
			Assert.AreEqual ("{Microsoft Text Driver (*.txt; *.csv)}", builder ["Driver"], "#G1");
			Assert.AreEqual (@"c:\bin", builder ["DBQ"], "#G2");
		}

		[Test]
		public void ContainsKey ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["SourceType"] = "DBC";
			builder.Add ("Port", "56");
			Assert.IsTrue (builder.ContainsKey ("SourceType"), "#1");
			Assert.IsTrue (builder.ContainsKey ("Port"), "#2");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#3");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#4");
			Assert.IsFalse (builder.ContainsKey ("xyz"), "#5");
			builder.Dsn = "myDsn";
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#6");
			builder.Driver = "SQL Server";
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#7");
			builder ["Dsn"] = "myDsn";
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#8");
			builder ["Driver"] = "SQL Server";
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#9");
			builder ["abc"] = "pqr";
			Assert.IsTrue (builder.ContainsKey ("ABC"), "#10");
			Assert.IsFalse (builder.ContainsKey (string.Empty), "#11");
		}

		[Test]
		public void ContainsKey_Keyword_Null ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["SourceType"] = "DBC";
			try {
				builder.ContainsKey (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("keyword", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Indexer ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["abc Def"] = "xa 34";
			Assert.AreEqual ("xa 34", builder ["abc def"], "#A1");
			Assert.AreEqual ("abc Def=xa 34", builder.ConnectionString, "#A2");
			builder ["na;"] = "abc;";
			Assert.AreEqual ("abc;", builder ["na;"], "#B1");
			Assert.AreEqual ("abc Def=xa 34;na;={abc;}", builder.ConnectionString, "#B2");
			builder ["Na;"] = "de\rfg";
			Assert.AreEqual ("de\rfg", builder ["na;"], "#C1");
			Assert.AreEqual ("abc Def=xa 34;na;=de\rfg", builder.ConnectionString, "#C2");
			builder ["val"] = ";xyz";
			Assert.AreEqual (";xyz", builder ["val"], "#D1");
			Assert.AreEqual ("abc Def=xa 34;na;=de\rfg;val={;xyz}", builder.ConnectionString, "#D2");
			builder ["name"] = string.Empty;
			Assert.AreEqual (string.Empty, builder ["name"], "#E1");
			Assert.AreEqual ("abc Def=xa 34;na;=de\rfg;val={;xyz};name=", builder.ConnectionString, "#E2");
			builder ["name"] = " ";
			Assert.AreEqual (" ", builder ["name"], "#F1");
			Assert.AreEqual ("abc Def=xa 34;na;=de\rfg;val={;xyz};name= ", builder.ConnectionString, "#F2");
		}

		[Test]
		public void Indexer_Keyword_Duplicate ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["DriverID"] = "120";
			builder ["DriverID"] = "790";
			Assert.AreEqual ("790", builder ["DriverID"], "#1");
			Assert.AreEqual ("DriverID=790", builder.ConnectionString, "#2");
		}

		[Test]
		public void Indexer_Keyword_Invalid ()
		{
			string [] invalid_keywords = new string [] {
				string.Empty,
				" ",
				" abc",
				"abc ",
				"\r",
				"ab\rc",
				";abc",
				"a\0b"
				};

			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			for (int i = 0; i < invalid_keywords.Length; i++) {
				string keyword = invalid_keywords [i];
				try {
					builder [keyword] = "abc";
					Assert.Fail ("#A1:" + i);
				} catch (ArgumentException ex) {
					// Invalid keyword, contain one or more of 'no characters',
					// 'control characters', 'leading or trailing whitespace'
					// or 'leading semicolons'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2:"+ i);
					Assert.IsNull (ex.InnerException, "#A3:" + i);
					Assert.IsNotNull (ex.Message, "#A4:" + i);
					Assert.IsTrue (ex.Message.IndexOf ("'" + keyword + "'") == -1, "#A5:" + i);
					Assert.AreEqual (keyword, ex.ParamName, "#A6:" + i);
				}

				builder [keyword] = null;
				Assert.IsFalse (builder.ContainsKey (keyword), "#B");

				try {
					object value = builder [keyword];
					Assert.Fail ("#C1:" + value + " (" + i + ")");
				} catch (ArgumentException ex) {
					// Keyword not supported: '...'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2:"+ i);
					Assert.IsNull (ex.InnerException, "#C3:" + i);
					Assert.IsNotNull (ex.Message, "#C4:" + i);
					Assert.IsTrue (ex.Message.IndexOf ("'" + keyword + "'") != -1, "#C5:" + i);
					Assert.IsNull (ex.ParamName, "#C6:" + i);
				}
			}
		}

		[Test]
		public void Indexer_Keyword_NotSupported ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			try {
				object value = builder ["abc"];
				Assert.Fail ("#1:" + value);
			} catch (ArgumentException ex) {
				// Keyword not supported: 'abc'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("'abc'") != -1, "#5");
				Assert.IsNull (ex.ParamName, "#6");
			}
		}

		[Test]
		public void Indexer_Keyword_Null ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			try {
				builder [null] = "abc";
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("keyword", ex.ParamName, "#A5");
			}

			try {
				object value = builder [null];
				Assert.Fail ("#B1:"+ value);
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("keyword", ex.ParamName, "#B5");
			}
		}

		[Test]
		public void Indexer_Value_Null ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["DriverID"] = null;
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#A1");
			try {
				object value = builder ["DriverID"];
				Assert.Fail ("#A2:" + value);
			} catch (ArgumentException ex) {
				// Keyword not supported: 'DriverID'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A3");
				Assert.IsNull (ex.InnerException, "#A4");
				Assert.IsNotNull (ex.Message, "#A5");
				Assert.IsTrue (ex.Message.IndexOf ("'DriverID'") != -1, "#A6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#A7");
			}
			Assert.IsFalse (builder.ContainsKey ("DriverID"), "#A8");
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#A9");

			builder ["DriverID"] = "A";
			Assert.AreEqual ("DriverID=A", builder.ConnectionString, "#B1");
			builder ["DriverID"] = null;
			Assert.IsFalse (builder.ContainsKey ("DriverID"), "#B2");
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#B3");
		}

		[Test]
		public void Driver ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#A1");
			Assert.AreEqual (string.Empty, builder ["Driver"], "#A2");
			Assert.AreEqual (string.Empty, builder.Driver, "#A3");

			builder.Driver = "SQL Server";
			Assert.AreEqual ("Driver={SQL Server}", builder.ConnectionString, "#B1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#B2");
			Assert.AreEqual ("SQL Server", builder ["Driver"], "#B3");
			Assert.AreEqual ("SQL Server", builder.Driver, "#B4");

			builder.Clear ();

			builder.Driver = "{SQL Server";
			Assert.AreEqual ("Driver={{SQL Server}", builder.ConnectionString, "#C1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#C2");
			Assert.AreEqual ("{SQL Server", builder ["Driver"], "#C3");
			Assert.AreEqual ("{SQL Server", builder.Driver, "#C4");

			builder.Clear ();

			builder.Driver = "{SQL Server}";
			Assert.AreEqual ("Driver={SQL Server}", builder.ConnectionString, "#D1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#D2");
			Assert.AreEqual ("{SQL Server}", builder ["Driver"], "#D3");
			Assert.AreEqual ("{SQL Server}", builder.Driver, "#D4");

			builder.Clear ();

			builder.Driver = string.Empty;
			Assert.AreEqual ("Driver=", builder.ConnectionString, "#E1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#E2");
			Assert.AreEqual (string.Empty, builder ["Driver"], "#E3");
			Assert.AreEqual (string.Empty, builder.Driver, "#E4");

			builder.Clear ();

			builder ["Driver"] = "SQL Server";
			Assert.AreEqual ("Driver={SQL Server}", builder.ConnectionString, "#F1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#F2");
			Assert.AreEqual ("SQL Server", builder ["Driver"], "#F3");
			Assert.AreEqual ("SQL Server", builder.Driver, "#F4");

			builder.Clear ();

			builder ["Driver"] = "{SQL Server";
			Assert.AreEqual ("Driver={{SQL Server}", builder.ConnectionString, "#G1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#G2");
			Assert.AreEqual ("{SQL Server", builder ["Driver"], "#G3");
			Assert.AreEqual ("{SQL Server", builder.Driver, "#G4");

			builder.Clear ();

			builder ["Driver"] = "{SQL Server}";
			Assert.AreEqual ("Driver={SQL Server}", builder.ConnectionString, "#H1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#H2");
			Assert.AreEqual ("{SQL Server}", builder ["Driver"], "#H3");
			Assert.AreEqual ("{SQL Server}", builder.Driver, "#H4");

			builder.Clear ();

			builder ["Driver"] = string.Empty;
			Assert.AreEqual ("Driver=", builder.ConnectionString, "#I1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#I2");
			Assert.AreEqual (string.Empty, builder ["Driver"], "#I3");
			Assert.AreEqual (string.Empty, builder.Driver, "#I4");
		}

		[Test]
		public void Driver_Value_Null ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			try {
				builder.Driver = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("Driver", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Indexer_Driver_Empty ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["Driver"] = string.Empty;
			Assert.AreEqual (string.Empty, builder.Driver, "#A1");
			Assert.AreEqual (string.Empty, builder ["Driver"], "#A2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#A3");
			Assert.AreEqual ("Driver=", builder.ConnectionString, "#A4");
			builder.Driver = "X";
			Assert.AreEqual ("X", builder.Driver, "#B1");
			Assert.AreEqual ("X", builder ["Driver"], "#B2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#B3");
			Assert.AreEqual ("Driver={X}", builder.ConnectionString, "#B4");
			builder ["Driver"] = string.Empty;
			Assert.AreEqual (string.Empty, builder.Driver, "#C1");
			Assert.AreEqual (string.Empty, builder ["Driver"], "#C2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#C3");
			Assert.AreEqual ("Driver=", builder.ConnectionString, "#C4");
			builder.Driver = "A";
			Assert.AreEqual ("A", builder.Driver, "#D1");
			Assert.AreEqual ("A", builder ["Driver"], "#D2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#D3");
			Assert.AreEqual ("Driver={A}", builder.ConnectionString, "#D4");
			builder ["Driver"] = " ";
			Assert.AreEqual (" ", builder.Driver, "#E1");
			Assert.AreEqual (" ", builder ["Driver"], "#E2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#E3");
			Assert.AreEqual ("Driver={ }", builder.ConnectionString, "#E4");
		}

		[Test]
		public void Indexer_Driver_Null ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["Driver"] = null;
			Assert.AreEqual (string.Empty, builder.Driver, "#A1");
			Assert.AreEqual (string.Empty, builder ["Driver"], "#A2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#A3");
			Assert.IsTrue (builder.ContainsKey ("drivEr"), "#A4");
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#A5");
			builder.Driver = "X";
			Assert.AreEqual ("X", builder.Driver, "#B1");
			Assert.AreEqual ("X", builder ["Driver"], "#B2");
			Assert.AreEqual ("X", builder ["driVer"], "#B3");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#B4");
			Assert.IsTrue (builder.ContainsKey ("drivEr"), "#B5");
			Assert.AreEqual ("Driver={X}", builder.ConnectionString, "#B6");
			builder ["Driver"] = null;
			Assert.AreEqual (string.Empty, builder.Driver, "#C1");
			Assert.AreEqual (string.Empty, builder ["Driver"], "#C2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#C3");
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#C4");
			builder ["Driver"] = "A";
			Assert.AreEqual ("A", builder.Driver, "#D1");
			Assert.AreEqual ("A", builder ["Driver"], "#D2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#D3");
			Assert.AreEqual ("Driver={A}", builder.ConnectionString, "#D4");
			builder ["Driver"] = null;
			Assert.AreEqual (string.Empty, builder.Driver, "#E1");
			Assert.AreEqual (string.Empty, builder ["Driver"], "#E2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#E3");
			Assert.AreEqual (string.Empty, builder.ConnectionString, "#E4");
		}

		[Test]
		public void Dsn ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#A1");
			Assert.AreEqual (string.Empty, builder ["Dsn"], "#A2");
			Assert.AreEqual (string.Empty, builder.Dsn, "#A3");

			builder.Dsn = "myDsn";
			Assert.AreEqual ("Dsn=myDsn", builder.ConnectionString, "#B1");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#B2");
			Assert.AreEqual ("myDsn", builder ["Dsn"], "#B3");
			Assert.AreEqual ("myDsn", builder.Dsn, "#B4");

			builder.Clear ();

			builder ["Dsn"] = "myDsn";
			Assert.AreEqual ("Dsn=myDsn", builder.ConnectionString, "#C1");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#C2");
			Assert.AreEqual ("myDsn", builder ["Dsn"], "#C3");
			Assert.AreEqual ("myDsn", builder.Dsn, "#C4");
		}
		
		[Test]
		public void Dsn_Value_Null ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			try {
				builder.Dsn = null;
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("Dsn", ex.ParamName, "#5");
			}
		}

		[Test]
		public void Indexer_Dsn_Empty ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["Dsn"] = string.Empty;
			Assert.AreEqual (string.Empty, builder.Dsn, "#A1");
			Assert.AreEqual (string.Empty, builder ["Dsn"], "#A2");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#A3");
			builder.Dsn = "X";
			Assert.AreEqual ("X", builder.Dsn, "#B1");
			Assert.AreEqual ("X", builder ["Dsn"], "#B2");
			Assert.AreEqual ("X", builder ["dsN"], "#B3");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#B4");
			Assert.IsTrue (builder.ContainsKey ("dSn"), "#B5");
			builder ["Dsn"] = string.Empty;
			Assert.AreEqual (string.Empty, builder.Dsn, "#C1");
			Assert.AreEqual (string.Empty, builder ["Dsn"], "#C2");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#C3");
			builder.Dsn = "A";
			Assert.AreEqual ("A", builder.Dsn, "#D1");
			Assert.AreEqual ("A", builder ["Dsn"], "#D2");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#D3");
			builder ["Dsn"] = " ";
			Assert.AreEqual (" ", builder.Dsn, "#E1");
			Assert.AreEqual (" ", builder ["Dsn"], "#E2");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#E3");
		}

		[Test]
		public void Indexer_Dsn_Null ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["Dsn"] = null;
			Assert.AreEqual (string.Empty, builder.Dsn, "#A1");
			Assert.AreEqual (string.Empty, builder ["Dsn"], "#A2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#A3");
			builder.Dsn = "X";
			Assert.AreEqual ("X", builder.Dsn, "#B1");
			Assert.AreEqual ("X", builder ["Dsn"], "#B2");
			Assert.AreEqual ("X", builder ["dsN"], "#B3");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#B4");
			Assert.IsTrue (builder.ContainsKey ("drivEr"), "#B5");
			builder ["Dsn"] = null;
			Assert.AreEqual (string.Empty, builder.Dsn, "#C1");
			Assert.AreEqual (string.Empty, builder ["Dsn"], "#C2");
			builder ["Dsn"] = "A";
			Assert.AreEqual ("A", builder.Dsn, "#D1");
			Assert.AreEqual ("A", builder ["Dsn"], "#D2");
			builder ["Dsn"] = null;
			Assert.AreEqual (string.Empty, builder.Dsn, "#E1");
			Assert.AreEqual (string.Empty, builder ["Dsn"], "#E2");
		}

		[Test]
		public void Keys ()
		{
			OdbcConnectionStringBuilder builder;
			ICollection keys;
			object [] keylist;

			builder = new OdbcConnectionStringBuilder ();
			keys = builder.Keys;
			Assert.IsNotNull (keys, "#A1");
			Assert.AreEqual (2, keys.Count, "#A2");
			keylist = new object [keys.Count];
			keys.CopyTo (keylist, 0);
			Assert.AreEqual (2, keylist.Length, "#A3");
			Assert.AreEqual ("Dsn", keylist [0], "#A4");
			Assert.AreEqual ("Driver", keylist [1], "#A5");

			builder = new OdbcConnectionStringBuilder ("Database=test;Driver=SQL Server;dsn=AdventureWorks");
			keys = builder.Keys;
			Assert.IsNotNull (keys, "#B1");
			Assert.AreEqual (3, keys.Count, "#B2");
			keylist = new object [keys.Count];
			keys.CopyTo (keylist, 0);
			Assert.AreEqual (3, keylist.Length, "#B3");
			Assert.AreEqual ("Dsn", keylist [0], "#B4");
			Assert.AreEqual ("Driver", keylist [1], "#B5");
			Assert.AreEqual ("database", keylist [2], "#B6");

			builder = new OdbcConnectionStringBuilder ("Driver=SQL Server;dsn=AdventureWorks;Database=test;Port=");
			keys = builder.Keys;
			Assert.IsNotNull (keys, "#C1");
			Assert.AreEqual (3, keys.Count, "#C2");
			keylist = new object [keys.Count];
			keys.CopyTo (keylist, 0);
			Assert.AreEqual (3, keylist.Length, "#C3");
			Assert.AreEqual ("Dsn", keylist [0], "#C4");
			Assert.AreEqual ("Driver", keylist [1], "#C5");
			Assert.AreEqual ("database", keylist [2], "#C6");

			builder = new OdbcConnectionStringBuilder ();
			builder ["DataBase"] = "test";
			builder.Driver = "SQL Server";
			builder ["User"] = "sa";
			builder ["porT"] = "25";
			keys = builder.Keys;
			Assert.IsNotNull (keys, "#D1");
			Assert.AreEqual (5, keys.Count, "#D2");
			keylist = new object [keys.Count];
			keys.CopyTo (keylist, 0);
			Assert.AreEqual (5, keylist.Length, "#D3");
			Assert.AreEqual ("Dsn", keylist [0], "#D4");
			Assert.AreEqual ("Driver", keylist [1], "#D5");
			Assert.AreEqual ("DataBase", keylist [2], "#D6");
			Assert.AreEqual ("User", keylist [3], "#D7");
			Assert.AreEqual ("porT", keylist [4], "#D8");

			builder.Clear ();

			keys = builder.Keys;
			Assert.IsNotNull (keys, "#E1");
			Assert.AreEqual (2, keys.Count, "#E2");
			keylist = new object [keys.Count];
			keys.CopyTo (keylist, 0);
			Assert.AreEqual (2, keylist.Length, "#E3");
			Assert.AreEqual ("Dsn", keylist [0], "#E4");
			Assert.AreEqual ("Driver", keylist [1], "#E5");
		}

		[Test]
		public void Remove ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			Assert.IsFalse (builder.Remove ("Dsn"), "#A1");
			Assert.IsFalse (builder.Remove ("Driver"), "#A2");
			builder.Add ("DriverID", "790");
			builder ["DefaultDir"] = "C:\\";
			Assert.IsTrue (builder.Remove ("DriverID"), "#B1");
			Assert.IsFalse (builder.ContainsKey ("DriverID"), "#B2");
			Assert.IsFalse (builder.Remove ("DriverID"), "#B3");
			Assert.IsFalse (builder.ContainsKey ("DriverID"), "#B4");
			Assert.IsTrue (builder.Remove ("defaulTdIr"), "#B5");
			Assert.IsFalse (builder.ContainsKey ("DefaultDir"), "#B6");
			Assert.IsFalse (builder.Remove ("defaulTdIr"), "#B7");
			Assert.IsFalse (builder.Remove ("userid"), "#B8");
			Assert.IsFalse (builder.Remove (string.Empty), "#B9");
			Assert.IsFalse (builder.Remove ("\r"), "#B10");
			Assert.IsFalse (builder.Remove ("a;"), "#B11");
			builder.Dsn = "myDsn";
			Assert.IsTrue (builder.Remove ("dSn"), "#C1");
			Assert.IsTrue (builder.ContainsKey ("dSn"), "#C2");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#C3");
			Assert.AreEqual (string.Empty, builder.Dsn, "#C4");
			Assert.IsFalse (builder.Remove ("Dsn"), "#C5");
			builder.Driver = "SQL Server";
			Assert.IsTrue (builder.Remove ("driVer"), "#D1");
			Assert.IsTrue (builder.ContainsKey ("driVer"), "#D2");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#D3");
			Assert.AreEqual (string.Empty, builder.Driver, "#D4");
			Assert.IsFalse (builder.Remove ("Driver"), "#D5");
			builder ["Dsn"] = "myDsn";
			Assert.IsTrue (builder.Remove ("Dsn"), "#E1");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#E2");
			Assert.AreEqual (string.Empty, builder.Dsn, "#E3");
			Assert.IsFalse (builder.Remove ("Dsn"), "#E4");
			builder ["Driver"] = "SQL Server";
			Assert.IsTrue (builder.Remove ("Driver"), "#F1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#F2");
			Assert.AreEqual (string.Empty, builder.Driver, "#F3");
			Assert.IsFalse (builder.Remove ("Driver"), "#F4");
		}

		[Test]
		public void Remove_Keyword_Null ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			try {
				builder.Remove (null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("keyword", ex.ParamName, "#5");
			}
		}

		[Test]
		public void TryGetValue ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			object value = null;

			builder ["DriverID"] = "790";
			builder.Add ("Server", "C:\\");
			Assert.IsTrue (builder.TryGetValue ("DriverID", out value), "#A1");
			Assert.AreEqual ("790", value, "#A2");
			Assert.IsTrue (builder.TryGetValue ("SERVER", out value), "#B1");
			Assert.AreEqual ("C:\\", value, "#B2");
			Assert.IsFalse (builder.TryGetValue (string.Empty, out value), "#C1");
			Assert.IsNull (value, "#C2");
			Assert.IsFalse (builder.TryGetValue ("a;", out value), "#D1");
			Assert.IsNull (value, "#D2");
			Assert.IsFalse (builder.TryGetValue ("\r", out value), "#E1");
			Assert.IsNull (value, "#E2");
			Assert.IsFalse (builder.TryGetValue (" ", out value), "#F1");
			Assert.IsNull (value, "#F2");
			Assert.IsFalse (builder.TryGetValue ("doesnotexist", out value), "#G1");
			Assert.IsNull (value, "#G2");
			Assert.IsTrue (builder.TryGetValue ("Driver", out value), "#H1");
			Assert.AreEqual (string.Empty, value, "#H2");
			Assert.IsTrue (builder.TryGetValue ("Dsn", out value), "#I1");
			Assert.AreEqual (string.Empty, value, "#I2");
			builder ["Driver"] = "SQL Server";
			Assert.IsTrue (builder.TryGetValue ("Driver", out value), "#J1");
			Assert.AreEqual ("SQL Server", value, "#J2");
			builder.Dsn = "AdventureWorks";
			Assert.IsTrue (builder.TryGetValue ("Dsn", out value), "#K1");
			Assert.AreEqual ("AdventureWorks", value, "#K2");
		}

		[Test]
		public void TryGetValue_Keyword_Null ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			object value = null;
			try {
				builder.TryGetValue (null, out value);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("keyword", ex.ParamName, "#5");
			}
		}
	}
}

#endif