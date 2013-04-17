// DbConnectionStringBuilderTest.cs - NUnit Test Cases for Testing the 
// DbConnectionStringBuilder class
//
// Author: 
//      Sureshkumar T (tsureshkumar@novell.com)
//	Daniel Morgan (monodanmorg@yahoo.com)
//	Gert Driesen (drieseng@users.sourceforge.net
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2008 Daniel Morgan
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

#if NET_2_0

#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

using NUnit.Framework;

#endregion

namespace MonoTests.System.Data.Common
{
	[TestFixture]
	public class DbConnectionStringBuilderTest
	{
		private DbConnectionStringBuilder builder = null;
		private const string SERVER = "SERVER";
		private const string SERVER_VALUE = "localhost";

		[SetUp]
		public void SetUp ()
		{
			builder = new DbConnectionStringBuilder ();
		}

		[Test]
		public void Add ()
		{
			builder.Add ("driverid", "420");
			builder.Add ("driverid", "560");
			builder.Add ("DriverID", "840");
			Assert.AreEqual ("840", builder ["driverId"], "#A1");
			Assert.IsTrue (builder.ContainsKey ("driverId"), "#A2");
			builder.Add ("Driver", "OdbcDriver");
			Assert.AreEqual ("OdbcDriver", builder ["Driver"], "#B1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#B2");
			builder.Add ("Driver", "{OdbcDriver");
			Assert.AreEqual ("{OdbcDriver", builder ["Driver"], "#C1");
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#C2");
			builder.Add ("Dsn", "MyDsn");
			Assert.AreEqual ("MyDsn", builder ["Dsn"], "#D1");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#D2");
			builder.Add ("dsN", "MyDsn2");
			Assert.AreEqual ("MyDsn2", builder ["Dsn"], "#E1");
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#E2");
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
		public void ConnectionString ()
		{
			DbConnectionStringBuilder sb;

			sb = new DbConnectionStringBuilder ();
			sb.ConnectionString = "A=B";
			Assert.IsTrue (sb.ContainsKey ("A"), "#A1");
			Assert.AreEqual ("a=B", sb.ConnectionString, "#A2");
			Assert.AreEqual (1, sb.Count, "#A3");
			Assert.AreEqual (1, sb.Keys.Count, "#A4");

			sb.ConnectionString = null;
			Assert.IsFalse (sb.ContainsKey ("A"), "#B1");
			Assert.AreEqual (string.Empty, sb.ConnectionString, "#B2");
			Assert.AreEqual (0, sb.Count, "#B3");
			Assert.AreEqual (0, sb.Keys.Count, "#B4");

			sb = new DbConnectionStringBuilder ();
			sb.ConnectionString = "A=B";
			sb.ConnectionString = string.Empty;
			Assert.IsFalse (sb.ContainsKey ("A"), "#C1");
			Assert.AreEqual (string.Empty, sb.ConnectionString, "#C2");
			Assert.AreEqual (0, sb.Count, "#C3");
			Assert.AreEqual (0, sb.Keys.Count, "#C4");

			sb = new DbConnectionStringBuilder ();
			sb.ConnectionString = "A=B";
			sb.ConnectionString = "\r ";
			Assert.IsFalse (sb.ContainsKey ("A"), "#D1");
			Assert.AreEqual (string.Empty, sb.ConnectionString, "#D2");
			Assert.AreEqual (0, sb.Count, "#D3");
			Assert.AreEqual (0, sb.Keys.Count, "#D4");
		}

		[Test]
		public void ConnectionString_Value_Empty ()
		{
			DbConnectionStringBuilder [] sbs = new DbConnectionStringBuilder [] {
				new DbConnectionStringBuilder (),
				new DbConnectionStringBuilder (false),
				new DbConnectionStringBuilder (true)
				};

			foreach (DbConnectionStringBuilder sb in sbs) {
				sb.ConnectionString = "A=";
				Assert.IsFalse (sb.ContainsKey ("A"), "#1");
				Assert.AreEqual (string.Empty, sb.ConnectionString, "#2");
				Assert.AreEqual (0, sb.Count, "#3");
			}
		}

		[Test]
		public void Clear ()
		{
			DbConnectionStringBuilder [] sbs = new DbConnectionStringBuilder [] {
				new DbConnectionStringBuilder (),
				new DbConnectionStringBuilder (false),
				new DbConnectionStringBuilder (true)
				};

			foreach (DbConnectionStringBuilder sb in sbs) {
				sb ["Dbq"] = "C:\\Data.xls";
				sb ["Driver"] = "790";
				sb.Add ("Port", "56");
				sb.Clear ();
				Assert.AreEqual (string.Empty, sb.ConnectionString, "#1");
				Assert.IsFalse (sb.ContainsKey ("Dbq"), "#2");
				Assert.IsFalse (sb.ContainsKey ("Driver"), "#3");
				Assert.IsFalse (sb.ContainsKey ("Port"), "#4");
				Assert.AreEqual (0, sb.Count, "#5");
				Assert.AreEqual (0, sb.Keys.Count, "#6");
				Assert.AreEqual (0, sb.Values.Count, "#7");
			}
		}

                [Test]
                public void AddDuplicateTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Add (SERVER, SERVER_VALUE);
                        // should allow duplicate addition. rather, it should re-assign
                        Assert.AreEqual (SERVER + "=" + SERVER_VALUE, builder.ConnectionString,
                                         "Duplicates addition does not change the value!");
                }

		[Test]
		public void Indexer ()
		{
			builder ["abc Def"] = "xa 34";
			Assert.AreEqual ("xa 34", builder ["abc def"], "#A1");
			Assert.AreEqual ("abc Def=\"xa 34\"", builder.ConnectionString, "#A2");
			builder ["na;"] = "abc;";
			Assert.AreEqual ("abc;", builder ["na;"], "#B1");
			Assert.AreEqual ("abc Def=\"xa 34\";na;=\"abc;\"", builder.ConnectionString, "#B2");
			builder ["Na;"] = "de\rfg";
			Assert.AreEqual ("de\rfg", builder ["na;"], "#C1");
			Assert.AreEqual ("abc Def=\"xa 34\";na;=\"de\rfg\"", builder.ConnectionString, "#C2");
			builder ["val"] = ";xyz";
			Assert.AreEqual (";xyz", builder ["val"], "#D1");
			Assert.AreEqual ("abc Def=\"xa 34\";na;=\"de\rfg\";val=\";xyz\"", builder.ConnectionString, "#D2");
			builder ["name"] = string.Empty;
			Assert.AreEqual (string.Empty, builder ["name"], "#E1");
			Assert.AreEqual ("abc Def=\"xa 34\";na;=\"de\rfg\";val=\";xyz\";name=", builder.ConnectionString, "#E2");
			builder ["name"] = " ";
			Assert.AreEqual (" ", builder ["name"], "#F1");
			Assert.AreEqual ("abc Def=\"xa 34\";na;=\"de\rfg\";val=\";xyz\";name=\" \"", builder.ConnectionString, "#F2");

			builder = new DbConnectionStringBuilder (false);
			builder ["abc Def"] = "xa 34";
			Assert.AreEqual ("xa 34", builder ["abc def"], "#A1");
			Assert.AreEqual ("abc Def=\"xa 34\"", builder.ConnectionString, "#A2");
			builder ["na;"] = "abc;";
			Assert.AreEqual ("abc;", builder ["na;"], "#B1");
			Assert.AreEqual ("abc Def=\"xa 34\";na;=\"abc;\"", builder.ConnectionString, "#B2");
			builder ["Na;"] = "de\rfg";
			Assert.AreEqual ("de\rfg", builder ["na;"], "#C1");
			Assert.AreEqual ("abc Def=\"xa 34\";na;=\"de\rfg\"", builder.ConnectionString, "#C2");
			builder ["val"] = ";xyz";
			Assert.AreEqual (";xyz", builder ["val"], "#D1");
			Assert.AreEqual ("abc Def=\"xa 34\";na;=\"de\rfg\";val=\";xyz\"", builder.ConnectionString, "#D2");
			builder ["name"] = string.Empty;
			Assert.AreEqual (string.Empty, builder ["name"], "#E1");
			Assert.AreEqual ("abc Def=\"xa 34\";na;=\"de\rfg\";val=\";xyz\";name=", builder.ConnectionString, "#E2");
			builder ["name"] = " ";
			Assert.AreEqual (" ", builder ["name"], "#F1");
			Assert.AreEqual ("abc Def=\"xa 34\";na;=\"de\rfg\";val=\";xyz\";name=\" \"", builder.ConnectionString, "#F2");

			builder = new DbConnectionStringBuilder (true);
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
					Assert.Fail ("#C1:" + i);
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
			try {
				object value = builder ["abc"];
				Assert.Fail ("#1");
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
				builder [null] = null;
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("keyword", ex.ParamName, "#B5");
			}

			try {
				object value = builder [null];
				Assert.Fail ("#C1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.AreEqual ("keyword", ex.ParamName, "#C5");
			}
		}

		[Test]
		public void Indexer_Value_Null ()
		{
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
		public void Remove ()
		{
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
			builder ["Dsn"] = "myDsn";
			Assert.IsTrue (builder.Remove ("Dsn"), "#C1");
			Assert.IsFalse (builder.ContainsKey ("Dsn"), "#C2");
			Assert.IsFalse (builder.Remove ("Dsn"), "#C3");
			builder ["Driver"] = "SQL Server";
			Assert.IsTrue (builder.Remove ("Driver"), "#D1");
			Assert.IsFalse (builder.ContainsKey ("Driver"), "#D2");
			Assert.IsFalse (builder.Remove ("Driver"), "#D3");

			builder = new DbConnectionStringBuilder (false);
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
			builder ["Dsn"] = "myDsn";
			Assert.IsTrue (builder.Remove ("Dsn"), "#C1");
			Assert.IsFalse (builder.ContainsKey ("Dsn"), "#C2");
			Assert.IsFalse (builder.Remove ("Dsn"), "#C3");
			builder ["Driver"] = "SQL Server";
			Assert.IsTrue (builder.Remove ("Driver"), "#D1");
			Assert.IsFalse (builder.ContainsKey ("Driver"), "#D2");
			Assert.IsFalse (builder.Remove ("Driver"), "#D3");

			builder = new DbConnectionStringBuilder (true);
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
			builder ["Dsn"] = "myDsn";
			Assert.IsTrue (builder.Remove ("Dsn"), "#C1");
			Assert.IsFalse (builder.ContainsKey ("Dsn"), "#C2");
			Assert.IsFalse (builder.Remove ("Dsn"), "#C3");
			builder ["Driver"] = "SQL Server";
			Assert.IsTrue (builder.Remove ("Driver"), "#D1");
			Assert.IsFalse (builder.ContainsKey ("Driver"), "#D2");
			Assert.IsFalse (builder.Remove ("Driver"), "#D3");
		}

		[Test]
		public void Remove_Keyword_Null ()
		{
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
		public void ContainsKey ()
		{
			builder ["SourceType"] = "DBC";
			builder.Add ("Port", "56");
			Assert.IsTrue (builder.ContainsKey ("SourceType"), "#A1");
			Assert.IsTrue (builder.ContainsKey ("Port"), "#A2");
			Assert.IsFalse (builder.ContainsKey ("Dsn"), "#A3");
			Assert.IsFalse (builder.ContainsKey ("Driver"), "#A4");
			Assert.IsFalse (builder.ContainsKey ("xyz"), "#A5");
			builder ["Dsn"] = "myDsn";
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#A6");
			builder ["Driver"] = "SQL Server";
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#A7");
			builder ["abc"] = "pqr";
			Assert.IsTrue (builder.ContainsKey ("ABC"), "#A8");
			Assert.IsFalse (builder.ContainsKey (string.Empty), "#A9");

			builder = new DbConnectionStringBuilder (false);
			builder ["SourceType"] = "DBC";
			builder.Add ("Port", "56");
			Assert.IsTrue (builder.ContainsKey ("SourceType"), "#A1");
			Assert.IsTrue (builder.ContainsKey ("Port"), "#A2");
			Assert.IsFalse (builder.ContainsKey ("Dsn"), "#A3");
			Assert.IsFalse (builder.ContainsKey ("Driver"), "#A4");
			Assert.IsFalse (builder.ContainsKey ("xyz"), "#A5");
			builder ["Dsn"] = "myDsn";
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#A6");
			builder ["Driver"] = "SQL Server";
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#A7");
			builder ["abc"] = "pqr";
			Assert.IsTrue (builder.ContainsKey ("ABC"), "#A8");
			Assert.IsFalse (builder.ContainsKey (string.Empty), "#A9");

			builder = new DbConnectionStringBuilder (true);
			builder ["SourceType"] = "DBC";
			builder.Add ("Port", "56");
			Assert.IsTrue (builder.ContainsKey ("SourceType"), "#A1");
			Assert.IsTrue (builder.ContainsKey ("Port"), "#A2");
			Assert.IsFalse (builder.ContainsKey ("Dsn"), "#A3");
			Assert.IsFalse (builder.ContainsKey ("Driver"), "#A4");
			Assert.IsFalse (builder.ContainsKey ("xyz"), "#A5");
			builder ["Dsn"] = "myDsn";
			Assert.IsTrue (builder.ContainsKey ("Dsn"), "#A6");
			builder ["Driver"] = "SQL Server";
			Assert.IsTrue (builder.ContainsKey ("Driver"), "#A7");
			builder ["abc"] = "pqr";
			Assert.IsTrue (builder.ContainsKey ("ABC"), "#A8");
			Assert.IsFalse (builder.ContainsKey (string.Empty), "#A9");
		}

		[Test]
		public void ContainsKey_Keyword_Null ()
		{
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
                public void EquivalentToTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        DbConnectionStringBuilder sb2 = new DbConnectionStringBuilder ();
                        sb2.Add (SERVER, SERVER_VALUE);
                        bool value = builder.EquivalentTo (sb2);
                        Assert.IsTrue (value, "builder comparision does not work!");

                        // negative tests
                        sb2.Add (SERVER + "1", SERVER_VALUE);
                        value = builder.EquivalentTo (sb2);
                        Assert.IsFalse (value, "builder comparision does not work for not equivalent strings!");
                }

		[Test] // AppendKeyValuePair (StringBuilder, String, String)
		public void AppendKeyValuePair1 ()
		{
			StringBuilder sb = new StringBuilder ();
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure");
			Assert.AreEqual ("Database=Adventure", sb.ToString (), "#A1");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven'ture");
			Assert.AreEqual ("Database=\"Adven'ture\"", sb.ToString (), "#A2");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven\"ture");
			Assert.AreEqual ("Database='Adven\"ture'", sb.ToString (), "#A3");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adventure\"");
			Assert.AreEqual ("Database='\"Adventure\"'", sb.ToString (), "#A4");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven'ture\"");
			Assert.AreEqual ("Database=\"\"\"Adven'ture\"\"\"", sb.ToString (), "#A5");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven;ture");
			Assert.AreEqual ("Database=\"Adven;ture\"", sb.ToString (), "#B1");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure;");
			Assert.AreEqual ("Database=\"Adventure;\"", sb.ToString (), "#B2");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", ";Adventure");
			Assert.AreEqual ("Database=\";Adventure\"", sb.ToString (), "#B3");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en;ture");
			Assert.AreEqual ("Database=\"Adv'en;ture\"", sb.ToString (), "#B4");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en;ture");
			Assert.AreEqual ("Database='Adv\"en;ture'", sb.ToString (), "#B5");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en;ture");
			Assert.AreEqual ("Database=\"A'dv\"\"en;ture\"", sb.ToString (), "#B6");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven;ture\"");
			Assert.AreEqual ("Database='\"Adven;ture\"'", sb.ToString (), "#B7");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven=ture");
			Assert.AreEqual ("Database=\"Adven=ture\"", sb.ToString (), "#C1");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en=ture");
			Assert.AreEqual ("Database=\"Adv'en=ture\"", sb.ToString (), "#C2");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en=ture");
			Assert.AreEqual ("Database='Adv\"en=ture'", sb.ToString (), "#C3");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en=ture");
			Assert.AreEqual ("Database=\"A'dv\"\"en=ture\"", sb.ToString (), "#C4");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven=ture\"");
			Assert.AreEqual ("Database='\"Adven=ture\"'", sb.ToString (), "#C5");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven{ture");
			Assert.AreEqual ("Database=Adven{ture", sb.ToString (), "#D1");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven}ture");
			Assert.AreEqual ("Database=Adven}ture", sb.ToString (), "#D2");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventure");
			Assert.AreEqual ("Database={Adventure", sb.ToString (), "#D3");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "}Adventure");
			Assert.AreEqual ("Database=}Adventure", sb.ToString (), "#D4");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure{");
			Assert.AreEqual ("Database=Adventure{", sb.ToString (), "#D5");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure}");
			Assert.AreEqual ("Database=Adventure}", sb.ToString (), "#D6");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en{ture");
			Assert.AreEqual ("Database=\"Adv'en{ture\"", sb.ToString (), "#D7");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en}ture");
			Assert.AreEqual ("Database=\"Adv'en}ture\"", sb.ToString (), "#D8");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en{ture");
			Assert.AreEqual ("Database='Adv\"en{ture'", sb.ToString (), "#D9");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en}ture");
			Assert.AreEqual ("Database='Adv\"en}ture'", sb.ToString (), "#D10");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en{ture");
			Assert.AreEqual ("Database=\"A'dv\"\"en{ture\"", sb.ToString (), "#D11");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en}ture");
			Assert.AreEqual ("Database=\"A'dv\"\"en}ture\"", sb.ToString (), "#D12");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven{ture\"");
			Assert.AreEqual ("Database='\"Adven{ture\"'", sb.ToString (), "#D13");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven}ture\"");
			Assert.AreEqual ("Database='\"Adven}ture\"'", sb.ToString (), "#D14");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure");
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Server", "localhost");
			Assert.AreEqual ("Database=Adventure;Server=localhost", sb.ToString (), "#E1");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", string.Empty);
			Assert.AreEqual ("Database=", sb.ToString (), "#E2");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", null);
			Assert.AreEqual ("Database=", sb.ToString (), "#E3");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Datab=ase", "Adven=ture", false);
			Assert.AreEqual ("Datab==ase=\"Adven=ture\"", sb.ToString (), "#F1");
		}

		[Test] // AppendKeyValuePair (StringBuilder, String, String)
		public void AppendKeyValuePair1_Builder_Null ()
		{
			try {
				DbConnectionStringBuilder.AppendKeyValuePair (
					(StringBuilder) null, "Server",
					"localhost");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("builder", ex.ParamName, "#5");
			}
		}

		[Test] // AppendKeyValuePair (StringBuilder, String, String)
		public void AppendKeyValuePair1_Keyword_Empty ()
		{
			StringBuilder sb = new StringBuilder ();
			try {
				DbConnectionStringBuilder.AppendKeyValuePair (
					sb, string.Empty, "localhost");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Expecting non-empty string for 'keyName'
				// parameter
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // AppendKeyValuePair (StringBuilder, String, String)
		public void AppendKeyValuePair1_Keyword_Null ()
		{
			StringBuilder sb = new StringBuilder ();
			try {
				DbConnectionStringBuilder.AppendKeyValuePair (
					sb, (string) null, "localhost");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("keyName", ex.ParamName, "#5");
			}
		}

		[Test] // AppendKeyValuePair (StringBuilder, String, String, Boolean)
		public void AppendKeyValuePair2_UseOdbcRules_False ()
		{
			StringBuilder sb = new StringBuilder ();
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure Works", false);
			Assert.AreEqual ("Database=\"Adventure Works\"", sb.ToString (), "#A1");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure", false);
			Assert.AreEqual ("Database=Adventure", sb.ToString (), "#A2");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven'ture Works", false);
			Assert.AreEqual ("Database=\"Adven'ture Works\"", sb.ToString (), "#A3");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven'ture", false);
			Assert.AreEqual ("Database=\"Adven'ture\"", sb.ToString (), "#A4");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven\"ture Works", false);
			Assert.AreEqual ("Database='Adven\"ture Works'", sb.ToString (), "#A5");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven\"ture", false);
			Assert.AreEqual ("Database='Adven\"ture'", sb.ToString (), "#A6");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adventure Works\"", false);
			Assert.AreEqual ("Database='\"Adventure Works\"'", sb.ToString (), "#A7");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adventure\"", false);
			Assert.AreEqual ("Database='\"Adventure\"'", sb.ToString (), "#A8");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven'ture Works\"", false);
			Assert.AreEqual ("Database=\"\"\"Adven'ture Works\"\"\"", sb.ToString (), "#A9");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven'ture\"", false);
			Assert.AreEqual ("Database=\"\"\"Adven'ture\"\"\"", sb.ToString (), "#A10");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven;ture Works", false);
			Assert.AreEqual ("Database=\"Adven;ture Works\"", sb.ToString (), "#B1");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven;ture", false);
			Assert.AreEqual ("Database=\"Adven;ture\"", sb.ToString (), "#B2");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure Works;", false);
			Assert.AreEqual ("Database=\"Adventure Works;\"", sb.ToString (), "#B3");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure;", false);
			Assert.AreEqual ("Database=\"Adventure;\"", sb.ToString (), "#B4");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", ";Adventure Works", false);
			Assert.AreEqual ("Database=\";Adventure Works\"", sb.ToString (), "#B5");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", ";Adventure", false);
			Assert.AreEqual ("Database=\";Adventure\"", sb.ToString (), "#B6");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en;ture Works", false);
			Assert.AreEqual ("Database=\"Adv'en;ture Works\"", sb.ToString (), "#B7");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en;ture", false);
			Assert.AreEqual ("Database=\"Adv'en;ture\"", sb.ToString (), "#B8");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en;ture Works", false);
			Assert.AreEqual ("Database='Adv\"en;ture Works'", sb.ToString (), "#B9");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en;ture", false);
			Assert.AreEqual ("Database='Adv\"en;ture'", sb.ToString (), "#B10");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en;ture Works", false);
			Assert.AreEqual ("Database=\"A'dv\"\"en;ture Works\"", sb.ToString (), "#B11");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en;ture", false);
			Assert.AreEqual ("Database=\"A'dv\"\"en;ture\"", sb.ToString (), "#B12");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven;ture Works\"", false);
			Assert.AreEqual ("Database='\"Adven;ture Works\"'", sb.ToString (), "#B13");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven;ture\"", false);
			Assert.AreEqual ("Database='\"Adven;ture\"'", sb.ToString (), "#B14");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven=ture Works", false);
			Assert.AreEqual ("Database=\"Adven=ture Works\"", sb.ToString (), "#C1");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven=ture", false);
			Assert.AreEqual ("Database=\"Adven=ture\"", sb.ToString (), "#C2");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en=ture Works", false);
			Assert.AreEqual ("Database=\"Adv'en=ture Works\"", sb.ToString (), "#C3");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en=ture", false);
			Assert.AreEqual ("Database=\"Adv'en=ture\"", sb.ToString (), "#C4");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en=ture Works", false);
			Assert.AreEqual ("Database='Adv\"en=ture Works'", sb.ToString (), "#C5");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en=ture", false);
			Assert.AreEqual ("Database='Adv\"en=ture'", sb.ToString (), "#C6");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en=ture Works", false);
			Assert.AreEqual ("Database=\"A'dv\"\"en=ture Works\"", sb.ToString (), "#C7");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en=ture", false);
			Assert.AreEqual ("Database=\"A'dv\"\"en=ture\"", sb.ToString (), "#C8");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven=ture Works\"", false);
			Assert.AreEqual ("Database='\"Adven=ture Works\"'", sb.ToString (), "#C9");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven=ture\"", false);
			Assert.AreEqual ("Database='\"Adven=ture\"'", sb.ToString (), "#C10");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven{ture Works", false);
			Assert.AreEqual ("Database=\"Adven{ture Works\"", sb.ToString (), "#D1");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven{ture", false);
			Assert.AreEqual ("Database=Adven{ture", sb.ToString (), "#D2");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven}ture Works", false);
			Assert.AreEqual ("Database=\"Adven}ture Works\"", sb.ToString (), "#D3");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven}ture", false);
			Assert.AreEqual ("Database=Adven}ture", sb.ToString (), "#D4");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventure Works", false);
			Assert.AreEqual ("Database=\"{Adventure Works\"", sb.ToString (), "#D5");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventure", false);
			Assert.AreEqual ("Database={Adventure", sb.ToString (), "#D6");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "}Adventure Works", false);
			Assert.AreEqual ("Database=\"}Adventure Works\"", sb.ToString (), "#D7");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "}Adventure", false);
			Assert.AreEqual ("Database=}Adventure", sb.ToString (), "#D8");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure Works{", false);
			Assert.AreEqual ("Database=\"Adventure Works{\"", sb.ToString (), "#D9");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure{", false);
			Assert.AreEqual ("Database=Adventure{", sb.ToString (), "#D10");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure Works}", false);
			Assert.AreEqual ("Database=\"Adventure Works}\"", sb.ToString (), "#D11");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure}", false);
			Assert.AreEqual ("Database=Adventure}", sb.ToString (), "#D12");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en{ture Works", false);
			Assert.AreEqual ("Database=\"Adv'en{ture Works\"", sb.ToString (), "#D13");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en{ture", false);
			Assert.AreEqual ("Database=\"Adv'en{ture\"", sb.ToString (), "#D14");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en}ture Works", false);
			Assert.AreEqual ("Database=\"Adv'en}ture Works\"", sb.ToString (), "#D15");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en}ture", false);
			Assert.AreEqual ("Database=\"Adv'en}ture\"", sb.ToString (), "#D16");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en{ture Works", false);
			Assert.AreEqual ("Database='Adv\"en{ture Works'", sb.ToString (), "#D17");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en{ture", false);
			Assert.AreEqual ("Database='Adv\"en{ture'", sb.ToString (), "#D18");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en}ture Works", false);
			Assert.AreEqual ("Database='Adv\"en}ture Works'", sb.ToString (), "#D19");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en}ture", false);
			Assert.AreEqual ("Database='Adv\"en}ture'", sb.ToString (), "#D20");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en{ture Works", false);
			Assert.AreEqual ("Database=\"A'dv\"\"en{ture Works\"", sb.ToString (), "#D21");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en{ture", false);
			Assert.AreEqual ("Database=\"A'dv\"\"en{ture\"", sb.ToString (), "#D22");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en}ture Works", false);
			Assert.AreEqual ("Database=\"A'dv\"\"en}ture Works\"", sb.ToString (), "#D23");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en}ture", false);
			Assert.AreEqual ("Database=\"A'dv\"\"en}ture\"", sb.ToString (), "#D24");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven{ture Works\"", false);
			Assert.AreEqual ("Database='\"Adven{ture Works\"'", sb.ToString (), "#D25");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven{ture\"", false);
			Assert.AreEqual ("Database='\"Adven{ture\"'", sb.ToString (), "#D26");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven}ture Works\"", false);
			Assert.AreEqual ("Database='\"Adven}ture Works\"'", sb.ToString (), "#D27");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven}ture\"", false);
			Assert.AreEqual ("Database='\"Adven}ture\"'", sb.ToString (), "#D28");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{{{B}}}", false);
			Assert.AreEqual ("Database={{{B}}}", sb.ToString (), "#D33a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{{{B}}}", false);
			Assert.AreEqual ("Driver={{{B}}}", sb.ToString (), "#D33b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{A{B{C}D}E}", false);
			Assert.AreEqual ("Database={A{B{C}D}E}", sb.ToString (), "#D33c");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{A{B{C}D}E}", false);
			Assert.AreEqual ("Driver={A{B{C}D}E}", sb.ToString (), "#D33d");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{{{B}}", false);
			Assert.AreEqual ("Database={{{B}}", sb.ToString (), "#D33e");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{{{B}}", false);
			Assert.AreEqual ("Driver={{{B}}", sb.ToString (), "#D33f");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{{{B}", false);
			Assert.AreEqual ("Database={{{B}", sb.ToString (), "#D33g");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{{{B}", false);
			Assert.AreEqual ("Driver={{{B}", sb.ToString (), "#D33h");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{{B}", false);
			Assert.AreEqual ("Database={{B}", sb.ToString (), "#D33i");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{{B}", false);
			Assert.AreEqual ("Driver={{B}", sb.ToString (), "#D33j");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{B}}", false);
			Assert.AreEqual ("Database={B}}", sb.ToString (), "#D33k");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{B}}", false);
			Assert.AreEqual ("Driver={B}}", sb.ToString (), "#D33l");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{B}}C", false);
			Assert.AreEqual ("Database={B}}C", sb.ToString (), "#D33k");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{B}}C", false);
			Assert.AreEqual ("Driver={B}}C", sb.ToString (), "#D33l");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A{B}}", false);
			Assert.AreEqual ("Database=A{B}}", sb.ToString (), "#D33k");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "A{B}}", false);
			Assert.AreEqual ("Driver=A{B}}", sb.ToString (), "#D33l");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", " {B}} ", false);
			Assert.AreEqual ("Database=\" {B}} \"", sb.ToString (), "#D33k");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", " {B}} ", false);
			Assert.AreEqual ("Driver=\" {B}} \"", sb.ToString (), "#D33l");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{{B}}", false);
			Assert.AreEqual ("Database={{B}}", sb.ToString (), "#D33a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{{B}}", false);
			Assert.AreEqual ("Driver={{B}}", sb.ToString (), "#D33b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "}}", false);
			Assert.AreEqual ("Database=}}", sb.ToString (), "#D33a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "}}", false);
			Assert.AreEqual ("Driver=}}", sb.ToString (), "#D33b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "}", false);
			Assert.AreEqual ("Database=}", sb.ToString (), "#D33a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "}", false);
			Assert.AreEqual ("Driver=}", sb.ToString (), "#D33b");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure Works", false);
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Server", "localhost", false);
			Assert.AreEqual ("Database=\"Adventure Works\";Server=localhost", sb.ToString (), "#E1");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure", false);
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Server", "localhost", false);
			Assert.AreEqual ("Database=Adventure;Server=localhost", sb.ToString (), "#E2");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", string.Empty, false);
			Assert.AreEqual ("Database=", sb.ToString (), "#E3");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", null, false);
			Assert.AreEqual ("Database=", sb.ToString (), "#E4");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Datab=ase", "Adven=ture", false);
			Assert.AreEqual ("Datab==ase=\"Adven=ture\"", sb.ToString (), "#F1");
		}

		[Test] // AppendKeyValuePair (StringBuilder, String, String, Boolean)
		public void AppendKeyValuePair2_UseOdbcRules_True ()
		{
			StringBuilder sb = new StringBuilder ();
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure Works", true);
			Assert.AreEqual ("Database=Adventure Works", sb.ToString (), "#A1a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adventure Works", true);
			Assert.AreEqual ("Driver={Adventure Works}", sb.ToString (), "#A1b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure", true);
			Assert.AreEqual ("Database=Adventure", sb.ToString (), "#A2a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adventure", true);
			Assert.AreEqual ("Driver={Adventure}", sb.ToString (), "#A2b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven'ture Works", true);
			Assert.AreEqual ("Database=Adven'ture Works", sb.ToString (), "#A3a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven'ture Works", true);
			Assert.AreEqual ("Driver={Adven'ture Works}", sb.ToString (), "#A3b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven'ture", true);
			Assert.AreEqual ("Database=Adven'ture", sb.ToString (), "#A4a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven'ture", true);
			Assert.AreEqual ("Driver={Adven'ture}", sb.ToString (), "#A4b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven\"ture Works", true);
			Assert.AreEqual ("Database=Adven\"ture Works", sb.ToString (), "#A5a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven\"ture Works", true);
			Assert.AreEqual ("Driver={Adven\"ture Works}", sb.ToString (), "#A5b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven\"ture", true);
			Assert.AreEqual ("Database=Adven\"ture", sb.ToString (), "#A6a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven\"ture", true);
			Assert.AreEqual ("Driver={Adven\"ture}", sb.ToString (), "#A6b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adventure Works\"", true);
			Assert.AreEqual ("Database=\"Adventure Works\"", sb.ToString (), "#A7a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adventure Works\"", true);
			Assert.AreEqual ("Driver={\"Adventure Works\"}", sb.ToString (), "#A7b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adventure\"", true);
			Assert.AreEqual ("Database=\"Adventure\"", sb.ToString (), "#A8a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adventure\"", true);
			Assert.AreEqual ("Driver={\"Adventure\"}", sb.ToString (), "#A8b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven'ture Works\"", true);
			Assert.AreEqual ("Database=\"Adven'ture Works\"", sb.ToString (), "#A9a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adven'ture Works\"", true);
			Assert.AreEqual ("Driver={\"Adven'ture Works\"}", sb.ToString (), "#A9b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven'ture\"", true);
			Assert.AreEqual ("Database=\"Adven'ture\"", sb.ToString (), "#A10a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adven'ture\"", true);
			Assert.AreEqual ("Driver={\"Adven'ture\"}", sb.ToString (), "#A10b");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven;ture Works", true);
			Assert.AreEqual ("Database={Adven;ture Works}", sb.ToString (), "#B1a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven;ture Works", true);
			Assert.AreEqual ("Driver={Adven;ture Works}", sb.ToString (), "#B1b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven;ture", true);
			Assert.AreEqual ("Database={Adven;ture}", sb.ToString (), "#B2a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven;ture", true);
			Assert.AreEqual ("Driver={Adven;ture}", sb.ToString (), "#B2b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure Works;", true);
			Assert.AreEqual ("Database={Adventure Works;}", sb.ToString (), "#B3a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adventure Works;", true);
			Assert.AreEqual ("Driver={Adventure Works;}", sb.ToString (), "#B3b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure;", true);
			Assert.AreEqual ("Database={Adventure;}", sb.ToString (), "#B4a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adventure;", true);
			Assert.AreEqual ("Driver={Adventure;}", sb.ToString (), "#B4b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", ";Adventure Works", true);
			Assert.AreEqual ("Database={;Adventure Works}", sb.ToString (), "#B5a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", ";Adventure Works", true);
			Assert.AreEqual ("Driver={;Adventure Works}", sb.ToString (), "#B5b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", ";Adventure", true);
			Assert.AreEqual ("Database={;Adventure}", sb.ToString (), "#B6a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", ";Adventure", true);
			Assert.AreEqual ("Driver={;Adventure}", sb.ToString (), "#B6b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en;ture Works", true);
			Assert.AreEqual ("Database={Adv'en;ture Works}", sb.ToString (), "#B7a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv'en;ture Works", true);
			Assert.AreEqual ("Driver={Adv'en;ture Works}", sb.ToString (), "#B7b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en;ture", true);
			Assert.AreEqual ("Database={Adv'en;ture}", sb.ToString (), "#B8a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv'en;ture", true);
			Assert.AreEqual ("Driver={Adv'en;ture}", sb.ToString (), "#B8b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en;ture Works", true);
			Assert.AreEqual ("Database={Adv\"en;ture Works}", sb.ToString (), "#B9A");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv\"en;ture Works", true);
			Assert.AreEqual ("Driver={Adv\"en;ture Works}", sb.ToString (), "#B9b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en;ture", true);
			Assert.AreEqual ("Database={Adv\"en;ture}", sb.ToString (), "#B10a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv\"en;ture", true);
			Assert.AreEqual ("Driver={Adv\"en;ture}", sb.ToString (), "#B10b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en;ture Works", true);
			Assert.AreEqual ("Database={A'dv\"en;ture Works}", sb.ToString (), "#B11a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "A'dv\"en;ture Works", true);
			Assert.AreEqual ("Driver={A'dv\"en;ture Works}", sb.ToString (), "#B11b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en;ture", true);
			Assert.AreEqual ("Database={A'dv\"en;ture}", sb.ToString (), "#B12a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "A'dv\"en;ture", true);
			Assert.AreEqual ("Driver={A'dv\"en;ture}", sb.ToString (), "#B12b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven;ture Works\"", true);
			Assert.AreEqual ("Database={\"Adven;ture Works\"}", sb.ToString (), "#B13a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adven;ture Works\"", true);
			Assert.AreEqual ("Driver={\"Adven;ture Works\"}", sb.ToString (), "#B13b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven;ture\"", true);
			Assert.AreEqual ("Database={\"Adven;ture\"}", sb.ToString (), "#B14a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adven;ture\"", true);
			Assert.AreEqual ("Driver={\"Adven;ture\"}", sb.ToString (), "#B14b");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven=ture Works", true);
			Assert.AreEqual ("Database=Adven=ture Works", sb.ToString (), "#C1a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven=ture Works", true);
			Assert.AreEqual ("Driver={Adven=ture Works}", sb.ToString (), "#C1b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven=ture", true);
			Assert.AreEqual ("Database=Adven=ture", sb.ToString (), "#C2a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven=ture", true);
			Assert.AreEqual ("Driver={Adven=ture}", sb.ToString (), "#C2b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en=ture Works", true);
			Assert.AreEqual ("Database=Adv'en=ture Works", sb.ToString (), "#C3a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv'en=ture Works", true);
			Assert.AreEqual ("Driver={Adv'en=ture Works}", sb.ToString (), "#C3b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en=ture", true);
			Assert.AreEqual ("Database=Adv'en=ture", sb.ToString (), "#C4a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv'en=ture", true);
			Assert.AreEqual ("Driver={Adv'en=ture}", sb.ToString (), "#C4b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en=ture Works", true);
			Assert.AreEqual ("Database=Adv\"en=ture Works", sb.ToString (), "#C5a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv\"en=ture Works", true);
			Assert.AreEqual ("Driver={Adv\"en=ture Works}", sb.ToString (), "#C5b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en=ture", true);
			Assert.AreEqual ("Database=Adv\"en=ture", sb.ToString (), "#C6a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv\"en=ture", true);
			Assert.AreEqual ("Driver={Adv\"en=ture}", sb.ToString (), "#C6b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en=ture Works", true);
			Assert.AreEqual ("Database=A'dv\"en=ture Works", sb.ToString (), "#C7a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "A'dv\"en=ture Works", true);
			Assert.AreEqual ("Driver={A'dv\"en=ture Works}", sb.ToString (), "#C7b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en=ture", true);
			Assert.AreEqual ("Database=A'dv\"en=ture", sb.ToString (), "#C8a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "A'dv\"en=ture", true);
			Assert.AreEqual ("Driver={A'dv\"en=ture}", sb.ToString (), "#C8b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven=ture Works\"", true);
			Assert.AreEqual ("Database=\"Adven=ture Works\"", sb.ToString (), "#C9a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adven=ture Works\"", true);
			Assert.AreEqual ("Driver={\"Adven=ture Works\"}", sb.ToString (), "#C9b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven=ture\"", true);
			Assert.AreEqual ("Database=\"Adven=ture\"", sb.ToString (), "#C10a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adven=ture\"", true);
			Assert.AreEqual ("Driver={\"Adven=ture\"}", sb.ToString (), "#C10b");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven{ture Works", true);
			Assert.AreEqual ("Database=Adven{ture Works", sb.ToString (), "#D1a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven{ture Works", true);
			Assert.AreEqual ("Driver={Adven{ture Works}", sb.ToString (), "#D1b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven{tu}re Works", true);
			Assert.AreEqual ("Database=Adven{tu}re Works", sb.ToString (), "#D1c");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven{tu}re Works", true);
			Assert.AreEqual ("Driver={Adven{tu}}re Works}", sb.ToString (), "#D1d");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven{ture", true);
			Assert.AreEqual ("Database=Adven{ture", sb.ToString (), "#D2a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven{ture", true);
			Assert.AreEqual ("Driver={Adven{ture}", sb.ToString (), "#D2b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven{tu}re", true);
			Assert.AreEqual ("Database=Adven{tu}re", sb.ToString (), "#D2c");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven{tu}re", true);
			Assert.AreEqual ("Driver={Adven{tu}}re}", sb.ToString (), "#D2d");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven}ture Works", true);
			Assert.AreEqual ("Database=Adven}ture Works", sb.ToString (), "#D3a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven}ture Works", true);
			Assert.AreEqual ("Driver={Adven}}ture Works}", sb.ToString (), "#D3b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adven}ture", true);
			Assert.AreEqual ("Database=Adven}ture", sb.ToString (), "#D4a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adven}ture", true);
			Assert.AreEqual ("Driver={Adven}}ture}", sb.ToString (), "#D4b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventure Works", true);
			Assert.AreEqual ("Database={{Adventure Works}", sb.ToString (), "#D5a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{Adventure Works", true);
			Assert.AreEqual ("Driver={{Adventure Works}", sb.ToString (), "#D5b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventure", true);
			Assert.AreEqual ("Database={{Adventure}", sb.ToString (), "#D6a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{Adventure", true);
			Assert.AreEqual ("Driver={{Adventure}", sb.ToString (), "#D6b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventure Works}", true);
			Assert.AreEqual ("Database={Adventure Works}", sb.ToString (), "#D7a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{Adventure Works}", true);
			Assert.AreEqual ("Driver={Adventure Works}", sb.ToString (), "#D7b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventu{re Works}", true);
			Assert.AreEqual ("Database={Adventu{re Works}", sb.ToString (), "#D7c");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{Adventu{re Works}", true);
			Assert.AreEqual ("Driver={Adventu{re Works}", sb.ToString (), "#D7d");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventu}re Works}", true);
			Assert.AreEqual ("Database={{Adventu}}re Works}}}", sb.ToString (), "#D7e");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{Adventu}re Works}", true);
			Assert.AreEqual ("Driver={{Adventu}}re Works}}}", sb.ToString (), "#D7f");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventure}", true);
			Assert.AreEqual ("Database={Adventure}", sb.ToString (), "#D8a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{Adventure}", true);
			Assert.AreEqual ("Driver={Adventure}", sb.ToString (), "#D8b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventu{re}", true);
			Assert.AreEqual ("Database={Adventu{re}", sb.ToString (), "#D8c");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{Adventu{re}", true);
			Assert.AreEqual ("Driver={Adventu{re}", sb.ToString (), "#D8d");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventu}re}", true);
			Assert.AreEqual ("Database={{Adventu}}re}}}", sb.ToString (), "#D8e");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{Adventu}re}", true);
			Assert.AreEqual ("Driver={{Adventu}}re}}}", sb.ToString (), "#D8f");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adventure }Works", true);
			Assert.AreEqual ("Database={{Adventure }}Works}", sb.ToString (), "#D9a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{Adventure }Works", true);
			Assert.AreEqual ("Driver={{Adventure }}Works}", sb.ToString (), "#D9b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{Adven}ture", true);
			Assert.AreEqual ("Database={{Adven}}ture}", sb.ToString (), "#D10a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{Adven}ture", true);
			Assert.AreEqual ("Driver={{Adven}}ture}", sb.ToString (), "#D10b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "}Adventure Works", true);
			Assert.AreEqual ("Database=}Adventure Works", sb.ToString (), "#D11a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "}Adventure Works", true);
			Assert.AreEqual ("Driver={}}Adventure Works}", sb.ToString (), "#D11b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "}Adventure", true);
			Assert.AreEqual ("Database=}Adventure", sb.ToString (), "#D12a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "}Adventure", true);
			Assert.AreEqual ("Driver={}}Adventure}", sb.ToString (), "#D12b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure Works{", true);
			Assert.AreEqual ("Database=Adventure Works{", sb.ToString (), "#D13a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adventure Works{", true);
			Assert.AreEqual ("Driver={Adventure Works{}", sb.ToString (), "#D13b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure{", true);
			Assert.AreEqual ("Database=Adventure{", sb.ToString (), "#D14a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adventure{", true);
			Assert.AreEqual ("Driver={Adventure{}", sb.ToString (), "#D14b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure Works}", true);
			Assert.AreEqual ("Database=Adventure Works}", sb.ToString (), "#D15a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adventure Works}", true);
			Assert.AreEqual ("Driver={Adventure Works}}}", sb.ToString (), "#D15b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure}", true);
			Assert.AreEqual ("Database=Adventure}", sb.ToString (), "#D16a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adventure}", true);
			Assert.AreEqual ("Driver={Adventure}}}", sb.ToString (), "#D16b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en{ture Works", true);
			Assert.AreEqual ("Database=Adv'en{ture Works", sb.ToString (), "#D17a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv'en{ture Works", true);
			Assert.AreEqual ("Driver={Adv'en{ture Works}", sb.ToString (), "#D17b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en{ture", true);
			Assert.AreEqual ("Database=Adv'en{ture", sb.ToString (), "#D18a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv'en{ture", true);
			Assert.AreEqual ("Driver={Adv'en{ture}", sb.ToString (), "#D18b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en}ture Works", true);
			Assert.AreEqual ("Database=Adv'en}ture Works", sb.ToString (), "#D19a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv'en}ture Works", true);
			Assert.AreEqual ("Driver={Adv'en}}ture Works}", sb.ToString (), "#D19b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv'en}ture", true);
			Assert.AreEqual ("Database=Adv'en}ture", sb.ToString (), "#D20a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv'en}ture", true);
			Assert.AreEqual ("Driver={Adv'en}}ture}", sb.ToString (), "#D20b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en{ture Works", true);
			Assert.AreEqual ("Database=Adv\"en{ture Works", sb.ToString (), "#D21a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv\"en{ture Works", true);
			Assert.AreEqual ("Driver={Adv\"en{ture Works}", sb.ToString (), "#D21b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en{ture", true);
			Assert.AreEqual ("Database=Adv\"en{ture", sb.ToString (), "#D22a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv\"en{ture", true);
			Assert.AreEqual ("Driver={Adv\"en{ture}", sb.ToString (), "#D22b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en}ture Works", true);
			Assert.AreEqual ("Database=Adv\"en}ture Works", sb.ToString (), "#D23a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv\"en}ture Works", true);
			Assert.AreEqual ("Driver={Adv\"en}}ture Works}", sb.ToString (), "#D23b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adv\"en}ture", true);
			Assert.AreEqual ("Database=Adv\"en}ture", sb.ToString (), "#D24a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adv\"en}ture", true);
			Assert.AreEqual ("Driver={Adv\"en}}ture}", sb.ToString (), "#D24b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en{ture Works", true);
			Assert.AreEqual ("Database=A'dv\"en{ture Works", sb.ToString (), "#D25a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "A'dv\"en{ture Works", true);
			Assert.AreEqual ("Driver={A'dv\"en{ture Works}", sb.ToString (), "#D25b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en{ture", true);
			Assert.AreEqual ("Database=A'dv\"en{ture", sb.ToString (), "#D26a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "A'dv\"en{ture", true);
			Assert.AreEqual ("Driver={A'dv\"en{ture}", sb.ToString (), "#D26b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en}ture Works", true);
			Assert.AreEqual ("Database=A'dv\"en}ture Works", sb.ToString (), "#D27a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "A'dv\"en}ture Works", true);
			Assert.AreEqual ("Driver={A'dv\"en}}ture Works}", sb.ToString (), "#D27b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A'dv\"en}ture", true);
			Assert.AreEqual ("Database=A'dv\"en}ture", sb.ToString (), "#D28a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "A'dv\"en}ture", true);
			Assert.AreEqual ("Driver={A'dv\"en}}ture}", sb.ToString (), "#D28b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven{ture Works\"", true);
			Assert.AreEqual ("Database=\"Adven{ture Works\"", sb.ToString (), "#D29a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adven{ture Works\"", true);
			Assert.AreEqual ("Driver={\"Adven{ture Works\"}", sb.ToString (), "#D29b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven{ture\"", true);
			Assert.AreEqual ("Database=\"Adven{ture\"", sb.ToString (), "#D30a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adven{ture\"", true);
			Assert.AreEqual ("Driver={\"Adven{ture\"}", sb.ToString (), "#D30b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven}ture Works\"", true);
			Assert.AreEqual ("Database=\"Adven}ture Works\"", sb.ToString (), "#D31a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adven}ture Works\"", true);
			Assert.AreEqual ("Driver={\"Adven}}ture Works\"}", sb.ToString (), "#D31b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "\"Adven}ture\"", true);
			Assert.AreEqual ("Database=\"Adven}ture\"", sb.ToString (), "#D32a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "\"Adven}ture\"", true);
			Assert.AreEqual ("Driver={\"Adven}}ture\"}", sb.ToString (), "#D32b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{{{B}}}", true);
			Assert.AreEqual ("Database={{{B}}}", sb.ToString (), "#D33a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{{{B}}}", true);
			Assert.AreEqual ("Driver={{{B}}}", sb.ToString (), "#D33b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{A{B{C}D}E}", true);
			Assert.AreEqual ("Database={{A{B{C}}D}}E}}}", sb.ToString (), "#D33c");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{A{B{C}D}E}", true);
			Assert.AreEqual ("Driver={{A{B{C}}D}}E}}}", sb.ToString (), "#D33d");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{{{B}}", true);
			Assert.AreEqual ("Database={{{{B}}}}}", sb.ToString (), "#D33e");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{{{B}}", true);
			Assert.AreEqual ("Driver={{{{B}}}}}", sb.ToString (), "#D33f");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{{{B}", true);
			Assert.AreEqual ("Database={{{B}", sb.ToString (), "#D33g");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{{{B}", true);
			Assert.AreEqual ("Driver={{{B}", sb.ToString (), "#D33h");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{{B}", true);
			Assert.AreEqual ("Database={{B}", sb.ToString (), "#D33i");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{{B}", true);
			Assert.AreEqual ("Driver={{B}", sb.ToString (), "#D33j");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{B}}", true);
			Assert.AreEqual ("Database={{B}}}}}", sb.ToString (), "#D33k");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{B}}", true);
			Assert.AreEqual ("Driver={{B}}}}}", sb.ToString (), "#D33l");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{B}}C", true);
			Assert.AreEqual ("Database={{B}}}}C}", sb.ToString (), "#D33k");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{B}}C", true);
			Assert.AreEqual ("Driver={{B}}}}C}", sb.ToString (), "#D33l");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "A{B}}", true);
			Assert.AreEqual ("Database=A{B}}", sb.ToString (), "#D33k");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "A{B}}", true);
			Assert.AreEqual ("Driver={A{B}}}}}", sb.ToString (), "#D33l");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", " {B}} ", true);
			Assert.AreEqual ("Database= {B}} ", sb.ToString (), "#D33k");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", " {B}} ", true);
			Assert.AreEqual ("Driver={ {B}}}} }", sb.ToString (), "#D33l");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "{{B}}", true);
			Assert.AreEqual ("Database={{{B}}}}}", sb.ToString (), "#D33a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "{{B}}", true);
			Assert.AreEqual ("Driver={{{B}}}}}", sb.ToString (), "#D33b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "}}", true);
			Assert.AreEqual ("Database=}}", sb.ToString (), "#D33a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "}}", true);
			Assert.AreEqual ("Driver={}}}}}", sb.ToString (), "#D33b");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "}", true);
			Assert.AreEqual ("Database=}", sb.ToString (), "#D33a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "}", true);
			Assert.AreEqual ("Driver={}}}", sb.ToString (), "#D33b");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure Works", true);
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Server", "localhost", true);
			Assert.AreEqual ("Database=Adventure Works;Server=localhost", sb.ToString (), "#E1a", true);
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adventure Works", true);
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Server", "localhost", true);
			Assert.AreEqual ("Driver={Adventure Works};Server=localhost", sb.ToString (), "#E1b", true);
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", "Adventure", true);
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Server", "localhost", true);
			Assert.AreEqual ("Database=Adventure;Server=localhost", sb.ToString (), "#E2a", true);
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", "Adventure", true);
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Server", "localhost", true);
			Assert.AreEqual ("Driver={Adventure};Server=localhost", sb.ToString (), "#E2b", true);
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", string.Empty, true);
			Assert.AreEqual ("Database=", sb.ToString (), "#E3a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", string.Empty, true);
			Assert.AreEqual ("Driver=", sb.ToString (), "#E3a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Database", null, true);
			Assert.AreEqual ("Database=", sb.ToString (), "#E4a");
			sb.Length = 0;
			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Driver", null, true);
			Assert.AreEqual ("Driver=", sb.ToString (), "#E4b");
			sb.Length = 0;

			DbConnectionStringBuilder.AppendKeyValuePair (sb, "Datab=ase", "Adven=ture", true);
			Assert.AreEqual ("Datab=ase=Adven=ture", sb.ToString (), "#F1");
		}

		[Test] // AppendKeyValuePair (StringBuilder, String, String, Boolean)
		public void AppendKeyValuePair2_Builder_Null ()
		{
			try {
				DbConnectionStringBuilder.AppendKeyValuePair (
					(StringBuilder) null, "Server",
					"localhost", true);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("builder", ex.ParamName, "#A5");
			}

			try {
				DbConnectionStringBuilder.AppendKeyValuePair (
					(StringBuilder) null, "Server",
					"localhost", false);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("builder", ex.ParamName, "#B5");
			}
		}

		[Test] // AppendKeyValuePair (StringBuilder, String, String, Boolean)
		public void AppendKeyValuePair2_Keyword_Empty ()
		{
			StringBuilder sb = new StringBuilder ();
			try {
				DbConnectionStringBuilder.AppendKeyValuePair (
					sb, string.Empty, "localhost", true);
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Expecting non-empty string for 'keyName'
				// parameter
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsNull (ex.ParamName, "#A5");
			}

			try {
				DbConnectionStringBuilder.AppendKeyValuePair (
					sb, string.Empty, "localhost", false);
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Expecting non-empty string for 'keyName'
				// parameter
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsNull (ex.ParamName, "#B5");
			}
		}

		[Test] // AppendKeyValuePair (StringBuilder, String, String, Boolean)
		public void AppendKeyValuePair2_Keyword_Null ()
		{
			StringBuilder sb = new StringBuilder ();
			try {
				DbConnectionStringBuilder.AppendKeyValuePair (
					sb, (string) null, "localhost", true);
				Assert.Fail ("#A1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.AreEqual ("keyName", ex.ParamName, "#A5");
			}

			try {
				DbConnectionStringBuilder.AppendKeyValuePair (
					sb, (string) null, "localhost", false);
				Assert.Fail ("#B1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.AreEqual ("keyName", ex.ParamName, "#B5");
			}
		}

                [Test]
                public void ToStringTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        string str = builder.ToString ();
                        string value = builder.ConnectionString;
                        Assert.AreEqual (value, str,
                                         "ToString shoud return ConnectionString!");
                }

                [Test]
                public void ItemTest ()
                {
                        builder.Add (SERVER, SERVER_VALUE);
                        string value = (string) builder [SERVER];
                        Assert.AreEqual (SERVER_VALUE, value,
                                         "Item indexor does not retrun correct value!");
                }

                [Test]
                public void ICollectionCopyToTest ()
                {
                        KeyValuePair<string, object> [] dict = new KeyValuePair<string, object> [2];
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Add (SERVER + "1", SERVER_VALUE + "1");

			int i = 0;
			int j = 1;
			((ICollection) builder).CopyTo (dict, 0);
                        Assert.AreEqual (SERVER, dict [i].Key, "not equal");
                        Assert.AreEqual (SERVER_VALUE, dict [i].Value, "not equal");
                        Assert.AreEqual (SERVER + "1", dict [j].Key, "not equal");
                        Assert.AreEqual (SERVER_VALUE + "1", dict [j].Value, "not equal");
                }

                [Test]
                [ExpectedException (typeof (ArgumentException))]
                public void NegICollectionCopyToTest ()
                {
                        KeyValuePair<string, object> [] dict = new KeyValuePair<string, object> [1];
                        builder.Add (SERVER, SERVER_VALUE);
                        builder.Add (SERVER + "1", SERVER_VALUE + "1");
			((ICollection) builder).CopyTo (dict, 0);
                        Assert.Fail ("Exception Destination Array not enough is not thrown!");
                }

		[Test]
		public void TryGetValueTest ()
		{
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
			Assert.IsFalse (builder.TryGetValue ("Driver", out value), "#H1");
			Assert.IsNull (value, "#H2");
			Assert.IsFalse (builder.TryGetValue ("Dsn", out value), "#I1");
			Assert.IsNull (value, "#I2");

			builder = new DbConnectionStringBuilder (false);
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
			Assert.IsFalse (builder.TryGetValue ("Driver", out value), "#H1");
			Assert.IsNull (value, "#H2");
			Assert.IsFalse (builder.TryGetValue ("Dsn", out value), "#I1");
			Assert.IsNull (value, "#I2");

			builder = new DbConnectionStringBuilder (true);
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
			Assert.IsFalse (builder.TryGetValue ("Driver", out value), "#H1");
			Assert.IsNull (value, "#H2");
			Assert.IsFalse (builder.TryGetValue ("Dsn", out value), "#I1");
			Assert.IsNull (value, "#I2");
		}

		[Test]
		public void TryGetValue_Keyword_Null ()
		{
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

                [Test]
				[NUnit.Framework.Category ("MobileNotWorking")] // DefaultMemberAttribute is removed by the tuner, causing #3 to fail
                public void ICTD_GetClassNameTest ()
                {
                        ICustomTypeDescriptor ictd = (ICustomTypeDescriptor) builder;
                        string className = ictd.GetClassName ();
                        Assert.AreEqual (builder.GetType ().ToString (), className, "#1");

                        AttributeCollection collection = ictd.GetAttributes ();
                        Assert.AreEqual (2, collection.Count, "#2");
                        object [] attr = builder.GetType ().GetCustomAttributes (typeof (DefaultMemberAttribute), false);
                        if (attr.Length > 0) {
                                DefaultMemberAttribute defAtt = (DefaultMemberAttribute) attr [0];
                                Assert.AreEqual ("Item", defAtt.MemberName, "#3");
                        } else
                                Assert.Fail ("#3");

                        string compName = ictd.GetComponentName ();
                        Assert.IsNull (compName, "#4");

                        TypeConverter converter = ictd.GetConverter ();
                        Assert.AreEqual (typeof (CollectionConverter), converter.GetType (), "#5");

                        EventDescriptor evtDesc = ictd.GetDefaultEvent ();
                        Assert.IsNull (evtDesc, "#6");

                        PropertyDescriptor property = ictd.GetDefaultProperty ();
                        Assert.IsNull (property, "#7");
                }

		[Test]
		public void EmbeddedCharTest1 ()
		{
			// Notice how the keywords show up in the connection string
			//  in the order they were added.
			// And notice the case of the keyword when added is preserved
			//  in the connection string.

			DbConnectionStringBuilder sb = new DbConnectionStringBuilder ();

			sb["Data Source"] = "testdb";
			sb["User ID"] = "someuser";
			sb["Password"] = "abcdef";
			Assert.AreEqual ("Data Source=testdb;User ID=someuser;Password=abcdef", 
				sb.ConnectionString, "cs#1");

			sb["Password"] = "abcdef#";
			Assert.AreEqual ("Data Source=testdb;User ID=someuser;Password=abcdef#", 
				sb.ConnectionString, "cs#2");

			// an embedded single-quote value will result in the value being delimieted with double quotes
			sb["Password"] = "abc\'def";
			Assert.AreEqual ("Data Source=testdb;User ID=someuser;Password=\"abc\'def\"", 
				sb.ConnectionString, "cs#3");

			// an embedded double-quote value will result in the value being delimieted with single quotes
			sb["Password"] = "abc\"def";
			Assert.AreEqual ("Data Source=testdb;User ID=someuser;Password=\'abc\"def\'", 
				sb.ConnectionString, "cs#4");

			// an embedded single-quote and double-quote in the value
			// will result in the value being delimited by double-quotes
			// with the embedded double quote being escaped with two double-quotes
			sb["Password"] = "abc\"d\'ef";
			Assert.AreEqual ("Data Source=testdb;User ID=someuser;Password=\"abc\"\"d\'ef\"", 
				sb.ConnectionString, "cs#5");

			sb = new DbConnectionStringBuilder ();
			sb["PASSWORD"] = "abcdef1";
			sb["user id"] = "someuser";
			sb["Data Source"] = "testdb";
			Assert.AreEqual ("PASSWORD=abcdef1;user id=someuser;Data Source=testdb", 
				sb.ConnectionString, "cs#6");

			// case is preserved for a keyword that was added the first time
			sb = new DbConnectionStringBuilder ();
			sb["PassWord"] = "abcdef2";
			sb["uSER iD"] = "someuser";
			sb["DaTa SoUrCe"] = "testdb";
			Assert.AreEqual ("PassWord=abcdef2;uSER iD=someuser;DaTa SoUrCe=testdb", 
				sb.ConnectionString, "cs#7");
			sb["passWORD"] = "abc123";
			Assert.AreEqual ("PassWord=abc123;uSER iD=someuser;DaTa SoUrCe=testdb", 
				sb.ConnectionString, "cs#8");

			// embedded equal sign in the value will cause the value to be
			// delimited with double-quotes
			sb = new DbConnectionStringBuilder ();
			sb["Password"] = "abc=def";
			sb["Data Source"] = "testdb";
			sb["User ID"] = "someuser";
			Assert.AreEqual ("Password=\"abc=def\";Data Source=testdb;User ID=someuser", 
				sb.ConnectionString, "cs#9");

			// embedded semicolon in the value will cause the value to be
			// delimited with double-quotes
			sb = new DbConnectionStringBuilder ();
			sb["Password"] = "abc;def";
			sb["Data Source"] = "testdb";
			sb["User ID"] = "someuser";
			Assert.AreEqual ("Password=\"abc;def\";Data Source=testdb;User ID=someuser", 
				sb.ConnectionString, "cs#10");

			// more right parentheses then left parentheses - happily takes it
			sb = new DbConnectionStringBuilder();
			sb.ConnectionString = "Data Source=(((Blah=Something))))))";
			Assert.AreEqual ("data source=\"(((Blah=Something))))))\"", 
				sb.ConnectionString, "cs#11");

			// more left curly braces then right curly braces - happily takes it
			sb = new DbConnectionStringBuilder();
			sb.ConnectionString = "Data Source={{{{Blah=Something}}";
			Assert.AreEqual ("data source=\"{{{{Blah=Something}}\"", 
				sb.ConnectionString, "cs#12");

			// spaces, empty string, null are treated like an empty string
			// and any previous settings is cleared
			sb.ConnectionString = "   ";
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#13");

			sb.ConnectionString = " ";
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#14");

			sb.ConnectionString = "";
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#15");

			sb.ConnectionString = String.Empty;
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#16");

			sb.ConnectionString = null;
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#17");

			sb = new DbConnectionStringBuilder();
			Assert.AreEqual (String.Empty, 
				sb.ConnectionString, "cs#18");
		}

		[Test]
		public void EmbeddedCharTest2 ()
		{
			DbConnectionStringBuilder sb;

			sb = new DbConnectionStringBuilder ();
			sb.ConnectionString = "Driver={SQL Server};Server=(local host);" +
				"Trusted_Connection=Yes Or No;Database=Adventure Works;";
			Assert.AreEqual ("{SQL Server}", sb["Driver"], "#A1");
			Assert.AreEqual ("(local host)", sb["Server"], "#A2");
			Assert.AreEqual ("Yes Or No", sb["Trusted_Connection"], "#A3");
			Assert.AreEqual ("driver=\"{SQL Server}\";server=\"(local host)\";" +
				"trusted_connection=\"Yes Or No\";database=\"Adventure Works\"",
				sb.ConnectionString, "#A4");

			sb = new DbConnectionStringBuilder ();
			sb.ConnectionString = "Driver={SQLServer};Server=(local);" +
				"Trusted_Connection=Yes;Database=AdventureWorks;";
			Assert.AreEqual ("{SQLServer}", sb["Driver"], "#B1");
			Assert.AreEqual ("(local)", sb["Server"], "#B2");
			Assert.AreEqual ("Yes", sb["Trusted_Connection"], "#B3");
			Assert.AreEqual ("driver={SQLServer};server=(local);" +
				"trusted_connection=Yes;database=AdventureWorks",
				sb.ConnectionString, "#B4");

			sb = new DbConnectionStringBuilder (false);
			sb.ConnectionString = "Driver={SQL Server};Server=(local host);" +
				"Trusted_Connection=Yes Or No;Database=Adventure Works;";
			Assert.AreEqual ("{SQL Server}", sb["Driver"], "#C1");
			Assert.AreEqual ("(local host)", sb["Server"], "#C2");
			Assert.AreEqual ("Yes Or No", sb["Trusted_Connection"], "#C3");
			Assert.AreEqual ("driver=\"{SQL Server}\";server=\"(local host)\";" +
				"trusted_connection=\"Yes Or No\";database=\"Adventure Works\"",
				sb.ConnectionString, "#C4");

			sb = new DbConnectionStringBuilder (false);
			sb.ConnectionString = "Driver={SQLServer};Server=(local);" +
				"Trusted_Connection=Yes;Database=AdventureWorks;";
			Assert.AreEqual ("{SQLServer}", sb["Driver"], "#D1");
			Assert.AreEqual ("(local)", sb["Server"], "#D2");
			Assert.AreEqual ("Yes", sb["Trusted_Connection"], "#D3");
			Assert.AreEqual ("driver={SQLServer};server=(local);" +
				"trusted_connection=Yes;database=AdventureWorks",
				sb.ConnectionString, "#D4");

			sb = new DbConnectionStringBuilder (true);
			sb.ConnectionString = "Driver={SQL Server};Server=(local host);" +
				"Trusted_Connection=Yes Or No;Database=Adventure Works;";
			Assert.AreEqual ("{SQL Server}", sb["Driver"], "#E1");
			Assert.AreEqual ("(local host)", sb["Server"], "#E2");
			Assert.AreEqual ("Yes Or No", sb["Trusted_Connection"], "#E3");
			Assert.AreEqual ("driver={SQL Server};server=(local host);" +
				"trusted_connection=Yes Or No;database=Adventure Works",
				sb.ConnectionString, "#E4");

			sb = new DbConnectionStringBuilder (true);
			sb.ConnectionString = "Driver={SQLServer};Server=(local);" +
				"Trusted_Connection=Yes;Database=AdventureWorks;";
			Assert.AreEqual ("{SQLServer}", sb["Driver"], "#F1");
			Assert.AreEqual ("(local)", sb["Server"], "#F2");
			Assert.AreEqual ("Yes", sb["Trusted_Connection"], "#F3");
			Assert.AreEqual ("driver={SQLServer};server=(local);" +
				"trusted_connection=Yes;database=AdventureWorks",
				sb.ConnectionString, "#F4");
		}

		[Test]
		public void EmbeddedCharTest3 ()
		{
			string dataSource = "(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.1.101)" + 
				"(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=TESTDB)))";
			DbConnectionStringBuilder sb;

			sb = new DbConnectionStringBuilder ();
			sb.ConnectionString = "User ID=SCOTT;Password=TiGeR;Data Source=" + dataSource;
			Assert.AreEqual (dataSource, sb["Data Source"], "#A1");
			Assert.AreEqual ("SCOTT", sb["User ID"], "#A2");
			Assert.AreEqual ("TiGeR", sb["Password"], "#A3");
			Assert.AreEqual ( 
				"user id=SCOTT;password=TiGeR;data source=\"(DESCRIPTION=(ADDRESS=(PROTOCOL=" +
				"TCP)(HOST=192.168.1.101)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)" +
				"(SERVICE_NAME=TESTDB)))\"", sb.ConnectionString, "#A4");

			sb = new DbConnectionStringBuilder (false);
			sb.ConnectionString = "User ID=SCOTT;Password=TiGeR;Data Source=" + dataSource;
			Assert.AreEqual (dataSource, sb["Data Source"], "#B1");
			Assert.AreEqual ("SCOTT", sb["User ID"], "#B2");
			Assert.AreEqual ("TiGeR", sb["Password"], "#B3");
			Assert.AreEqual ( 
				"user id=SCOTT;password=TiGeR;data source=\"(DESCRIPTION=(ADDRESS=(PROTOCOL=" +
				"TCP)(HOST=192.168.1.101)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)" +
				"(SERVICE_NAME=TESTDB)))\"", sb.ConnectionString, "#B4");

			sb = new DbConnectionStringBuilder (true);
			sb.ConnectionString = "User ID=SCOTT;Password=TiGeR;Data Source=" + dataSource;
			Assert.AreEqual (dataSource, sb["Data Source"], "#C1");
			Assert.AreEqual ("SCOTT", sb["User ID"], "#C2");
			Assert.AreEqual ("TiGeR", sb["Password"], "#C3");
			Assert.AreEqual ( 
				"user id=SCOTT;password=TiGeR;data source=(DESCRIPTION=(ADDRESS=(PROTOCOL=" +
				"TCP)(HOST=192.168.1.101)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)" +
				"(SERVICE_NAME=TESTDB)))", sb.ConnectionString, "#C4");
		}

		[Test]
		public void EmbeddedCharTest4 ()
		{
			DbConnectionStringBuilder sb;

			sb = new DbConnectionStringBuilder ();
			sb.ConnectionString = "PassWord=abcdef2;uSER iD=someuser;DaTa SoUrCe=testdb";
			sb["Integrated Security"] = "False";
			Assert.AreEqual ( 
				"password=abcdef2;user id=someuser;data source=testdb;Integrated Security=False",
				sb.ConnectionString, "#A");

			sb = new DbConnectionStringBuilder (false);
			sb.ConnectionString = "PassWord=abcdef2;uSER iD=someuser;DaTa SoUrCe=testdb";
			sb["Integrated Security"] = "False";
			Assert.AreEqual ( 
				"password=abcdef2;user id=someuser;data source=testdb;Integrated Security=False",
				sb.ConnectionString, "#B");

			sb = new DbConnectionStringBuilder (true);
			sb.ConnectionString = "PassWord=abcdef2;uSER iD=someuser;DaTa SoUrCe=testdb";
			sb["Integrated Security"] = "False";
			Assert.AreEqual ( 
				"password=abcdef2;user id=someuser;data source=testdb;Integrated Security=False",
				sb.ConnectionString, "#C");
		}

		[Test]
		public void EmbeddedCharTest5 ()
		{
			string connectionString = "A={abcdef2};B=some{us;C=test}db;D=12\"3;E=\"45;6\";F=AB==C;G{A'}\";F={1='\"2};G==C=B==C;Z=ABC";
			DbConnectionStringBuilder sb;

			sb = new DbConnectionStringBuilder ();
			sb.ConnectionString = connectionString;
			Assert.AreEqual ("a={abcdef2};b=some{us;c=test}db;d='12\"3';e=\"45;6\";f=\"AB==C\";g{a'}\";f=\"{1='\"\"2}\";g==c=\"B==C\";z=ABC", sb.ConnectionString, "#A1");
			Assert.AreEqual ("{abcdef2}", sb ["A"], "#A2");
			Assert.AreEqual ("some{us", sb ["B"], "#A3");
			Assert.AreEqual ("test}db", sb ["C"], "#A4");
			Assert.AreEqual ("12\"3", sb ["D"], "#A5");
			Assert.AreEqual ("45;6", sb ["E"], "#A6");
			Assert.AreEqual ("AB==C", sb ["F"], "#A7");
			Assert.AreEqual ("{1='\"2}", sb ["g{a'}\";f"], "#A8");
			Assert.AreEqual ("ABC", sb ["Z"], "#A9");
			Assert.AreEqual ("B==C", sb ["g=c"], "#A10");

			sb = new DbConnectionStringBuilder (false);
			sb.ConnectionString = connectionString;
			Assert.AreEqual ("a={abcdef2};b=some{us;c=test}db;d='12\"3';e=\"45;6\";f=\"AB==C\";g{a'}\";f=\"{1='\"\"2}\";g==c=\"B==C\";z=ABC", sb.ConnectionString, "#B1");
			Assert.AreEqual ("{abcdef2}", sb ["A"], "#B2");
			Assert.AreEqual ("some{us", sb ["B"], "#B3");
			Assert.AreEqual ("test}db", sb ["C"], "#B4");
			Assert.AreEqual ("12\"3", sb ["D"], "#B5");
			Assert.AreEqual ("45;6", sb ["E"], "#B6");
			Assert.AreEqual ("AB==C", sb ["F"], "#B7");
			Assert.AreEqual ("{1='\"2}", sb ["g{a'}\";f"], "#B8");
			Assert.AreEqual ("ABC", sb ["Z"], "#B9");
			Assert.AreEqual ("B==C", sb ["g=c"], "#B10");

			sb = new DbConnectionStringBuilder (true);
			sb.ConnectionString = connectionString;
			Assert.AreEqual ("a={abcdef2};b=some{us;c=test}db;d=12\"3;e=\"45;6\";f=AB==C;g{a'}\";f={1='\"2};g==C=B==C;z=ABC", sb.ConnectionString, "#C1");
			Assert.AreEqual ("{abcdef2}", sb ["A"], "#C2");
			Assert.AreEqual ("some{us", sb ["B"], "#C3");
			Assert.AreEqual ("test}db", sb ["C"], "#C4");
			Assert.AreEqual ("12\"3", sb ["D"], "#C5");
			Assert.AreEqual ("\"45", sb ["E"], "#C6");
			Assert.AreEqual ("AB==C", sb ["6\";f"], "#C7");
			Assert.AreEqual ("{1='\"2}", sb ["g{a'}\";f"], "#C8");
			Assert.AreEqual ("ABC", sb ["Z"], "#C9");
			Assert.AreEqual ("=C=B==C", sb ["g"], "#C10");
		}

		[Test]
		public void EmbeddedCharTest6 ()
		{
			string [][] shared_tests = new string [][] {
				new string [] { "A=(B;", "A", "(B", "a=(B" },
				new string [] { "A={B{}", "A", "{B{}", "a={B{}" },
				new string [] { "A={B{{}", "A", "{B{{}", "a={B{{}" },
				new string [] { " A =B{C", "A", "B{C", "a=B{C" },
				new string [] { " A =B{{C}", "A", "B{{C}", "a=B{{C}" },
				new string [] { "A={{{B}}}", "A", "{{{B}}}", "a={{{B}}}" },
				new string [] { "A={B}", "A", "{B}", "a={B}" },
				new string [] { "A= {B}", "A", "{B}", "a={B}" },
				new string [] { " A =BC",  "a", "BC", "a=BC" },
				new string [] { "\rA\t=BC",  "a", "BC", "a=BC" },
				new string [] { "\rA\t=BC",  "a", "BC", "a=BC" },
				new string [] { "A;B=BC",  "a;b", "BC", "a;b=BC" },
				};

			string [][] non_odbc_tests = new string [][] {
				new string [] { "A=''", "A", "", "a=" },
				new string [] { "A='BC;D'", "A", "BC;D", "a=\"BC;D\"" },
				new string [] { "A=BC''D", "A", "BC''D", "a=\"BC''D\"" },
				new string [] { "A='\"'", "A", "\"", "a='\"'" },
				new string [] { "A=B\"\"C;", "A", "B\"\"C", "a='B\"\"C'" },
				new string [] { "A={B{", "A", "{B{", "a={B{" },
				new string [] { "A={B}C", "A", "{B}C", "a={B}C" },
				new string [] { "A=B'C", "A", "B'C", "a=\"B'C\"" },
				new string [] { "A=B''C", "A", "B''C", "a=\"B''C\"" },
				new string [] { "A=  B C ;", "A", "B C", "a=\"B C\"" },
				new string [] { "A={B { }} }", "A", "{B { }} }", "a=\"{B { }} }\"" },
				new string [] { "A={B {{ }} }", "A", "{B {{ }} }", "a=\"{B {{ }} }\"" },
				new string [] { "A= B {C ", "A", "B {C", "a=\"B {C\"" },
				new string [] { "A= B }C ", "A", "B }C", "a=\"B }C\"" },
				new string [] { "A=B }C", "A", "B }C", "a=\"B }C\"" },
				new string [] { "A=B { }C", "A", "B { }C", "a=\"B { }C\"" },
				new string [] { "A= B{C {}}", "A", "B{C {}}", "a=\"B{C {}}\"" },
				new string [] { "A= {C {};B=A", "A", "{C {}", "a=\"{C {}\";b=A" },
				new string [] { "A= {C {}  ", "A", "{C {}", "a=\"{C {}\"" },
				new string [] { "A= {C {}  ;B=A", "A", "{C {}", "a=\"{C {}\";b=A" },
				new string [] { "A= {C {}}}", "A", "{C {}}}", "a=\"{C {}}}\"" },
				new string [] { "A={B=C}", "A", "{B=C}", "a=\"{B=C}\"" },
				new string [] { "A={B==C}", "A", "{B==C}", "a=\"{B==C}\"" },
				new string [] { "A=B==C", "A", "B==C", "a=\"B==C\"" },
				new string [] { "A={=}", "A", "{=}", "a=\"{=}\"" },
				new string [] { "A={==}", "A", "{==}", "a=\"{==}\"" },
				new string [] { "A=\"B;(C)'\"", "A", "B;(C)'", "a=\"B;(C)'\"" },
				new string [] { "A=B(=)C", "A", "B(=)C", "a=\"B(=)C\"" },
				new string [] { "A=B=C", "A", "B=C", "a=\"B=C\"" },
				new string [] { "A=B(==)C", "A", "B(==)C", "a=\"B(==)C\"" },
				new string [] { "A=B  C", "A", "B  C", "a=\"B  C\"" },
				new string [] { "A= B  C ", "A", "B  C", "a=\"B  C\"" },
				new string [] { "A=  B  C  ", "A", "B  C", "a=\"B  C\"" },
				new string [] { "A='  B C '", "A", "  B C ", "a=\"  B C \"" },
				new string [] { "A=\"  B C \"", "A", "  B C ", "a=\"  B C \"" },
				new string [] { "A={  B C }", "A", "{  B C }", "a=\"{  B C }\"" },
				new string [] { "A=  B C  ;", "A", "B C", "a=\"B C\"" },
				new string [] { "A=  B\rC\r\t;", "A", "B\rC", "a=\"B\rC\"" },
				new string [] { "A=\"\"\"B;C\"\"\"", "A", "\"B;C\"", "a='\"B;C\"'" },
				new string [] { "A= \"\"\"B;C\"\"\" ", "A", "\"B;C\"", "a='\"B;C\"'" },
				new string [] { "A='''B;C'''", "A", "'B;C'", "a=\"'B;C'\"" },
				new string [] { "A= '''B;C''' ", "A", "'B;C'", "a=\"'B;C'\"" },
				new string [] { "A={{", "A", "{{", "a={{" },
				new string [] { "A={B C}", "A", "{B C}", "a=\"{B C}\"" },
				new string [] { "A={ B C }", "A", "{ B C }", "a=\"{ B C }\"" },
				new string [] { "A={B {{ } }", "A", "{B {{ } }", "a=\"{B {{ } }\"" },
				new string [] { "A='='", "A", "=", "a=\"=\"" },
				new string [] { "A='=='", "A", "==", "a=\"==\"" },
				new string [] { "A=\"=\"", "A", "=", "a=\"=\"" },
				new string [] { "A=\"==\"", "A", "==", "a=\"==\"" },
				new string [] { "A={B}}", "A", "{B}}", "a={B}}" },
				new string [] { "A=\";\"", "A", ";", "a=\";\"" },
				new string [] { "A(=)=B", "A(", ")=B", "a(=\")=B\"" },
				new string [] { "A==B=C",  "A=B", "C", "a==b=C" },
				new string [] { "A===B=C",  "A=", "B=C", "a===\"B=C\"" },
				new string [] { "(A=)=BC",  "(a", ")=BC", "(a=\")=BC\"" },
				new string [] { "A==C=B==C", "a=c", "B==C", "a==c=\"B==C\"" },
				};
			DbConnectionStringBuilder sb;

			for (int i = 0; i < non_odbc_tests.Length; i++) {
				string [] test = non_odbc_tests [i];
				sb = new DbConnectionStringBuilder ();
				sb.ConnectionString = test [0];
				Assert.AreEqual (test [3], sb.ConnectionString, "#A1:" + i);
				Assert.AreEqual (test [2], sb [test [1]], "#A2:" + i);
			}

			for (int i = 0; i < non_odbc_tests.Length; i++) {
				string [] test = non_odbc_tests [i];
				sb = new DbConnectionStringBuilder (false);
				sb.ConnectionString = test [0];
				Assert.AreEqual (test [3], sb.ConnectionString, "#B1:" + i);
				Assert.AreEqual (test [2], sb [test [1]], "#B2:" + i);
			}

			for (int i = 0; i < shared_tests.Length; i++) {
				string [] test = shared_tests [i];
				sb = new DbConnectionStringBuilder ();
				sb.ConnectionString = test [0];
				Assert.AreEqual (test [3], sb.ConnectionString, "#C1:" + i);
				Assert.AreEqual (test [2], sb [test [1]], "#C2:" + i);
			}

			for (int i = 0; i < shared_tests.Length; i++) {
				string [] test = shared_tests [i];
				sb = new DbConnectionStringBuilder (false);
				sb.ConnectionString = test [0];
				Assert.AreEqual (test [3], sb.ConnectionString, "#D1:" + i);
				Assert.AreEqual (test [2], sb [test [1]], "#D2:" + i);
			}

			string [][] odbc_tests = new string [][] {
				new string [] { "A=B(=)C", "A", "B(=)C", "a=B(=)C" },
				new string [] { "A=B(==)C", "A", "B(==)C", "a=B(==)C" },
				new string [] { "A=  B C  ;", "A", "B C", "a=B C" },
				new string [] { "A=  B\rC\r\t;", "A", "B\rC", "a=B\rC" },
				new string [] { "A='''", "A", "'''", "a='''" },
				new string [] { "A=''", "A", "''", "a=''" },
				new string [] { "A=''B", "A", "''B", "a=''B" },
				new string [] { "A=BC''D", "A", "BC''D", "a=BC''D" },
				new string [] { "A='\"'", "A", "'\"'", "a='\"'" },
				new string [] { "A=\"\"B", "A", "\"\"B", "a=\"\"B"},
				new string [] { "A=B\"\"C;", "A", "B\"\"C", "a=B\"\"C" },
				new string [] { "A=\"B", "A", "\"B", "a=\"B" },
				new string [] { "A=\"", "A", "\"", "a=\"" },
				new string [] { "A=B'C", "A", "B'C", "a=B'C" },
				new string [] { "A=B''C", "A", "B''C", "a=B''C" },
				new string [] { "A='A'C", "A", "'A'C", "a='A'C" },
				new string [] { "A=B  C", "A", "B  C", "a=B  C" },
				new string [] { "A= B  C ", "A", "B  C", "a=B  C" },
				new string [] { "A=  B  C  ", "A", "B  C", "a=B  C" },
				new string [] { "A='  B C '", "A", "'  B C '", "a='  B C '" },
				new string [] { "A=\"  B C \"", "A", "\"  B C \"", "a=\"  B C \"" },
				new string [] { "A={  B C }", "A", "{  B C }", "a={  B C }" },
				new string [] { "A=  B C ;", "A", "B C", "a=B C" },
				new string [] { "A=\"\"BC\"\"", "A", "\"\"BC\"\"", "a=\"\"BC\"\"" },
				new string [] { "A=\"\"B\"C\"\";", "A", "\"\"B\"C\"\"", "a=\"\"B\"C\"\"" },
				new string [] { "A= \"\"B\"C\"\" ", "A", "\"\"B\"C\"\"", "a=\"\"B\"C\"\"" },
				new string [] { "A=''BC''", "A", "''BC''", "a=''BC''" },
				new string [] { "A=''B'C'';", "A", "''B'C''", "a=''B'C''" },
				new string [] { "A= ''B'C'' ", "A", "''B'C''", "a=''B'C''" },
				new string [] { "A={B C}", "A", "{B C}", "a={B C}" },
				new string [] { "A={ B C }", "A", "{ B C }", "a={ B C }" },
				new string [] { "A={ B;C }", "A", "{ B;C }", "a={ B;C }" },
				new string [] { "A={B { }} }", "A", "{B { }} }", "a={B { }} }" },
				new string [] { "A={ B;= {;=}};= }", "A", "{ B;= {;=}};= }", "a={ B;= {;=}};= }" },
				new string [] { "A={B {{ }} }", "A", "{B {{ }} }", "a={B {{ }} }" },
				new string [] { "A={ B;= {{:= }};= }", "A", "{ B;= {{:= }};= }", "a={ B;= {{:= }};= }" },
				new string [] { "A= B {C ", "A", "B {C", "a=B {C" },
				new string [] { "A= B }C ", "A", "B }C", "a=B }C" },
				new string [] { "A=B }C", "A", "B }C", "a=B }C" },
				new string [] { "A=B { }C", "A", "B { }C", "a=B { }C" },
				new string [] { "A= {B;{}", "A", "{B;{}", "a={B;{}" },
				new string [] { "A= {B;{}}}", "A", "{B;{}}}", "a={B;{}}}" },
				new string [] { "A= B{C {}}", "A", "B{C {}}", "a=B{C {}}" },
				new string [] { "A= {C {};B=A", "A", "{C {}", "a={C {};b=A" },
				new string [] { "A= {C {}  ", "A", "{C {}", "a={C {}" },
				new string [] { "A= {C {}  ;B=A", "A", "{C {}", "a={C {};b=A" },
				new string [] { "A= {C {}}}", "A", "{C {}}}", "a={C {}}}" },
				new string [] { "A={B=C}", "A", "{B=C}", "a={B=C}" },
				new string [] { "A={B==C}", "A", "{B==C}", "a={B==C}" },
				new string [] { "A=B==C", "A", "B==C", "a=B==C" },
				new string [] { "A='='", "A", "'='", "a='='" },
				new string [] { "A='=='", "A", "'=='", "a='=='" },
				new string [] { "A=\"=\"", "A", "\"=\"", "a=\"=\"" },
				new string [] { "A=\"==\"", "A", "\"==\"", "a=\"==\"" },
				new string [] { "A={=}", "A", "{=}", "a={=}" },
				new string [] { "A={==}", "A", "{==}", "a={==}" },
				new string [] { "A=B=C", "A", "B=C", "a=B=C" },
				new string [] { "A(=)=B", "A(", ")=B", "a(=)=B" },
				new string [] { "A==B=C",  "A", "=B=C", "a==B=C" },
				new string [] { "A===B=C",  "A", "==B=C", "a===B=C" },
				new string [] { "A'='=B=C",  "A'", "'=B=C", "a'='=B=C" },
				new string [] { "A\"=\"=B=C",  "A\"", "\"=B=C", "a\"=\"=B=C" },
				new string [] { "\"A=\"=BC",  "\"a", "\"=BC", "\"a=\"=BC" },
				new string [] { "(A=)=BC",  "(a", ")=BC", "(a=)=BC" },
				new string [] { "A==C=B==C", "A", "=C=B==C", "a==C=B==C" },
				};

			for (int i = 0; i < odbc_tests.Length; i++) {
				string [] test = odbc_tests [i];
				sb = new DbConnectionStringBuilder (true);
				sb.ConnectionString = test [0];
				Assert.AreEqual (test [3], sb.ConnectionString, "#E1:" + i);
				Assert.AreEqual (test [2], sb [test [1]], "#E2:" + i);
			}

			for (int i = 0; i < shared_tests.Length; i++) {
				string [] test = shared_tests [i];
				sb = new DbConnectionStringBuilder (true);
				sb.ConnectionString = test [0];
				Assert.AreEqual (test [3], sb.ConnectionString, "#F1:" + i);
				Assert.AreEqual (test [2], sb [test [1]], "#F2:" + i);
			}

			// each test that is in odbc_tests and not in non_odbc_tests
			// (or vice versa) should result in an ArgumentException
			AssertValueTest (non_odbc_tests, odbc_tests, true, "#G:");
			AssertValueTest (odbc_tests, non_odbc_tests, false, "#H:");
		}

		[Test]
		public void EmbeddedChar_ConnectionString_Invalid ()
		{
			string [] tests = new string [] {
				" =",
				"=",
				"=;",
				"=ABC;",
				"='A'",
				"A",
				"A=(B;)",
				"A=B';'C",
				"A=B { {;} }",
				"A=B { ; }C",
				"A=BC'E;F'D",
				};

			DbConnectionStringBuilder [] cbs = new DbConnectionStringBuilder [] {
				new DbConnectionStringBuilder (),
				new DbConnectionStringBuilder (false),
				new DbConnectionStringBuilder (true)
				};

			for (int i = 0; i < tests.Length; i++) {
				for (int j = 0; j < cbs.Length; j++) {
					DbConnectionStringBuilder cb =cbs [j];
					try {
						cb.ConnectionString = tests [i];
						Assert.Fail ("#1:" + i + " (" + j + ")");
					} catch (ArgumentException ex) {
						// Format of the initialization string does
						// not conform to specification starting
						// at index 0
						Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:"+ i + " (" + j + ")");
						Assert.IsNull (ex.InnerException, "#3:" + i + " (" + j + ")");
						Assert.IsNotNull (ex.Message, "#4:" + i + " (" + j + ")");
						Assert.IsNull (ex.ParamName, "#5:" + i + " (" + j + ")");
					}
				}
			}
		}

		void AssertValueTest (string [][] tests1, string [][] tests2, bool useOdbc, string prefix)
		{
			DbConnectionStringBuilder sb = new DbConnectionStringBuilder (useOdbc);
			for (int i = 0; i < tests1.Length; i++) {
				string [] test1 = tests1 [i];
				bool found = false;
				for (int j = 0; j < tests2.Length; j++) {
					string [] test2 = tests2 [j];
					if (test2 [0] == test1 [0]) {
						found = true;

						if (test2 [1] != test1 [1])
							continue;
						if (test2 [2] != test1 [2])
							continue;
						if (test2 [3] != test1 [3])
							continue;

						Assert.Fail (string.Format (
							"{0}test1 {1} and test2 {2} " +
							"should be moved to shared_tests.",
							prefix, i, j));
					}
				}
				if (found)
					continue;

				try {
					sb.ConnectionString = test1 [0];
					Assert.Fail (prefix + "1 (" + i + ")");
				} catch (ArgumentException ex) {
					// Format of the initialization string does
					// not conform to specification starting
					// at index 0
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), prefix + "2 (" + i + ")");
					Assert.IsNull (ex.InnerException, prefix + "3 (" + i + ")");
					Assert.IsNotNull (ex.Message, prefix + "4 (" + i + ")");
					Assert.IsNull (ex.ParamName, prefix + "5 (" + i + ")");
				}
			}

			// check uniqueness of tests
			for (int i = 0; i < tests1.Length; i++) {
				for (int j = 0; j < tests1.Length; j++) {
					if (i == j)
						continue;
					if (tests1 [i] == tests1 [j])
						Assert.Fail (string.Format (
							"{0}Duplicate test in test1 " +
							"{1} and {2}.", prefix, i, j));
				}
			}

			// check uniqueness of tests
			for (int i = 0; i < tests2.Length; i++) {
				for (int j = 0; j < tests2.Length; j++) {
					if (i == j)
						continue;
					if (tests2 [i] == tests2 [j])
						Assert.Fail (string.Format (
							"{0}Duplicate test in test2 " +
							"{1} and {2}.", prefix, i, j));
				}
			}
		}
	}
}

#endif // NET_2_0
