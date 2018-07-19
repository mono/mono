// OdbcCommandBuilderTest.cs - NUnit Test Cases for testing the
// OdbcCommandBuilder Test.
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
// 
// Copyright (c) 2008 Gert Driesen
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
using System.Data;
using System.Data.Common;
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Odbc
{
	[TestFixture]
	[Category("NotWorking")] // needs https://github.com/dotnet/corefx/pull/22499
	public class OdbcCommandBuilderTest
	{
		[Test]
		public void CatalogLocationTest ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
			Assert.AreEqual (CatalogLocation.Start, cb.CatalogLocation, "#1");
			cb.CatalogLocation = CatalogLocation.End;
			Assert.AreEqual (CatalogLocation.End, cb.CatalogLocation, "#2");
		}

		[Test]
		public void CatalogLocation_Value_Invalid ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
			cb.CatalogLocation = CatalogLocation.End;
			try {
				cb.CatalogLocation = (CatalogLocation) 666;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// The CatalogLocation enumeration value, 666, is invalid
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("CatalogLocation") != -1, "#5:" + ex.Message);
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#6:" + ex.Message);
			}
			Assert.AreEqual (CatalogLocation.End, cb.CatalogLocation, "#6");
		}

		[Test]
		public void CatalogSeparator ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
			Assert.AreEqual (".", cb.CatalogSeparator, "#1");
			cb.CatalogSeparator = "a";
			Assert.AreEqual ("a", cb.CatalogSeparator, "#2");
			cb.CatalogSeparator = null;
			Assert.AreEqual (".", cb.CatalogSeparator, "#3");
			cb.CatalogSeparator = "b";
			Assert.AreEqual ("b", cb.CatalogSeparator, "#4");
			cb.CatalogSeparator = string.Empty;
			Assert.AreEqual (".", cb.CatalogSeparator, "#5");
			cb.CatalogSeparator = " ";
			Assert.AreEqual (" ", cb.CatalogSeparator, "#6");
		}

		[Test]
		public void ConflictOptionTest ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
			Assert.AreEqual (ConflictOption.CompareAllSearchableValues, cb.ConflictOption, "#1");
			cb.ConflictOption = ConflictOption.CompareRowVersion;
			Assert.AreEqual (ConflictOption.CompareRowVersion, cb.ConflictOption, "#2");
		}

		[Test]
		public void ConflictOption_Value_Invalid ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
			cb.ConflictOption = ConflictOption.CompareRowVersion;
			try {
				cb.ConflictOption = (ConflictOption) 666;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				// The ConflictOption enumeration value, 666, is invalid
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("ConflictOption") != -1, "#5:" + ex.Message);
				Assert.IsTrue (ex.Message.IndexOf ("666") != -1, "#6:" + ex.Message);
				Assert.AreEqual ("ConflictOption", ex.ParamName, "#7");
			}
			Assert.AreEqual (ConflictOption.CompareRowVersion, cb.ConflictOption, "#8");
		}

		[Test]
		public void QuotePrefix ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
			Assert.AreEqual (string.Empty, cb.QuotePrefix, "#1");
			cb.QuotePrefix = "mono";
			Assert.AreEqual ("mono", cb.QuotePrefix, "#2");
			cb.QuotePrefix = null;
			Assert.AreEqual (string.Empty, cb.QuotePrefix, "#3");
			cb.QuotePrefix = "'\"";
			Assert.AreEqual ("'\"", cb.QuotePrefix, "#4");
			cb.QuotePrefix = string.Empty;
			Assert.AreEqual (string.Empty, cb.QuotePrefix, "#5");
			cb.QuotePrefix = " ";
			Assert.AreEqual (" ", cb.QuotePrefix, "#6");
		}

		[Test]
		public void QuoteSuffix ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
			Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#1");
			cb.QuoteSuffix = "mono";
			Assert.AreEqual ("mono", cb.QuoteSuffix, "#2");
			cb.QuoteSuffix = null;
			Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#3");
			cb.QuoteSuffix = "'\"";
			Assert.AreEqual ("'\"", cb.QuoteSuffix, "#4");
			cb.QuoteSuffix = string.Empty;
			Assert.AreEqual (string.Empty, cb.QuoteSuffix, "#5");
			cb.QuoteSuffix = " ";
			Assert.AreEqual (" ", cb.QuoteSuffix, "#6");
		}

		[Test] // QuoteIdentifier (String)
		public void QuoteIdentifier1 ()
		{
			OdbcCommandBuilder cb;
		
			cb = new OdbcCommandBuilder ();
			cb.QuotePrefix = "aBc";
			Assert.AreEqual ("aBcmoAbCno", cb.QuoteIdentifier ("moAbCno"), "#A1");
			Assert.AreEqual ("aBc", cb.QuoteIdentifier (string.Empty), "#A2");
			Assert.AreEqual ("aBcZ", cb.QuoteIdentifier ("Z"), "#A3");
			Assert.AreEqual ("aBcabc", cb.QuoteIdentifier ("abc"), "#A4");
			cb.QuoteSuffix = "deF";
			Assert.AreEqual ("aBcmodEfnodeF", cb.QuoteIdentifier ("modEfno"), "#A5");
			Assert.AreEqual ("aBcdeF", cb.QuoteIdentifier (string.Empty), "#A6");
			Assert.AreEqual ("aBcZdeF", cb.QuoteIdentifier ("Z"), "#A7");
			Assert.AreEqual ("aBcabcdeF", cb.QuoteIdentifier ("abc"), "#A8");

			cb = new OdbcCommandBuilder ();
			cb.QuotePrefix = "X";
			Assert.AreEqual ("XmoXno", cb.QuoteIdentifier ("moXno"), "#B1");
			Assert.AreEqual ("X", cb.QuoteIdentifier (string.Empty), "#B2");
			Assert.AreEqual ("XZ", cb.QuoteIdentifier ("Z"), "#B3");
			Assert.AreEqual ("XX", cb.QuoteIdentifier ("X"), "#B4");
			cb.QuoteSuffix = " ";
			Assert.AreEqual ("Xmo  no ", cb.QuoteIdentifier ("mo no"), "#B5");
			Assert.AreEqual ("X ", cb.QuoteIdentifier (string.Empty), "#B6");
			Assert.AreEqual ("XZ ", cb.QuoteIdentifier ("Z"), "#B7");
			Assert.AreEqual ("X   ", cb.QuoteIdentifier (" "), "#B8");

			cb = new OdbcCommandBuilder ();
			cb.QuotePrefix = " ";
			Assert.AreEqual ("mono", cb.QuoteIdentifier ("mono"), "#C1");
			Assert.AreEqual (string.Empty, cb.QuoteIdentifier (string.Empty), "#C2");
			Assert.AreEqual ("Z", cb.QuoteIdentifier ("Z"), "#C3");
			cb.QuoteSuffix = "dEf";
			Assert.AreEqual ("modefno", cb.QuoteIdentifier ("modefno"), "#C4");
			Assert.AreEqual (string.Empty, cb.QuoteIdentifier (string.Empty), "#C5");
			Assert.AreEqual ("Z", cb.QuoteIdentifier ("Z"), "#C6");

			cb = new OdbcCommandBuilder ();
			cb.QuotePrefix = "  ";
			Assert.AreEqual ("  mono", cb.QuoteIdentifier ("mono"), "#D1");
			Assert.AreEqual ("  ", cb.QuoteIdentifier (string.Empty), "#D2");
			Assert.AreEqual ("  Z", cb.QuoteIdentifier ("Z"), "#D3");
			cb.QuoteSuffix = "dEf";
			Assert.AreEqual ("  moDeFnodEf", cb.QuoteIdentifier ("moDeFno"), "#D4");
			Assert.AreEqual ("  modEfdEfnodEf", cb.QuoteIdentifier ("modEfno"), "#D5");
			Assert.AreEqual ("  dEf", cb.QuoteIdentifier (string.Empty), "#D6");
			Assert.AreEqual ("  ZdEf", cb.QuoteIdentifier ("Z"), "#D7");
		}

		[Test] // QuoteIdentifier (String)
		public void QuoteIdentifier1_QuotePrefix_Empty ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
			try {
				cb.QuoteIdentifier ("mono");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// QuoteIdentifier requires open connection when
				// the quote prefix has not been set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test] // QuoteIdentifier (String)
		public void QuoteIdentifier1_UnquotedIdentifier_Null ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
			try {
				cb.QuoteIdentifier ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("unquotedIdentifier", ex.ParamName, "#5");
			}
		}

		[Test] // QuoteIdentifier (String, OdbcConnection)
		public void QuoteIdentifier2_Connection_Null ()
		{
			OdbcCommandBuilder cb;
			OdbcConnection conn = null;

			cb = new OdbcCommandBuilder ();
			try {
				cb.QuoteIdentifier ("mono", conn);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// QuoteIdentifier requires open connection when
				// the quote prefix has not been set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			cb = new OdbcCommandBuilder ();
			cb.QuotePrefix = "abc";
			Assert.AreEqual ("abcmono", cb.QuoteIdentifier ("mono", conn), "#B1");
			Assert.AreEqual ("abcZ", cb.QuoteIdentifier ("Z", conn), "#B2");
			Assert.AreEqual ("abcabc", cb.QuoteIdentifier ("abc", conn), "#B3");
			Assert.AreEqual ("abc", cb.QuoteIdentifier (string.Empty, conn), "#B4");
			Assert.AreEqual ("abc ", cb.QuoteIdentifier (" ", conn), "#B5");
			Assert.AreEqual ("abc\r", cb.QuoteIdentifier ("\r", conn), "#B6");
			cb.QuoteSuffix = "def";
			Assert.AreEqual ("abcmonodef", cb.QuoteIdentifier ("mono", conn), "#B7");
			Assert.AreEqual ("abcZdef", cb.QuoteIdentifier ("Z", conn), "#B8");
			Assert.AreEqual ("abcabcdef", cb.QuoteIdentifier ("abc", conn), "#B9");
			Assert.AreEqual ("abcdef", cb.QuoteIdentifier (string.Empty, conn), "#B10");
			Assert.AreEqual ("abc def", cb.QuoteIdentifier (" ", conn), "#B11");
			Assert.AreEqual ("abc\rdef", cb.QuoteIdentifier ("\r", conn), "#B12");

			cb.QuotePrefix = string.Empty;
			try {
				cb.QuoteIdentifier ("mono", conn);
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// QuoteIdentifier requires open connection when
				// the quote prefix has not been set
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}

			cb = new OdbcCommandBuilder ();
			cb.QuotePrefix = "X";
			Assert.AreEqual ("Xmono", cb.QuoteIdentifier ("mono", conn), "#D1");
			Assert.AreEqual ("XZ", cb.QuoteIdentifier ("Z", conn), "#D2");
			Assert.AreEqual ("XX", cb.QuoteIdentifier ("X", conn), "#D3");
			Assert.AreEqual ("X", cb.QuoteIdentifier (string.Empty, conn), "#D4");
			Assert.AreEqual ("X ", cb.QuoteIdentifier (" ", conn), "#D5");
			Assert.AreEqual ("X\r", cb.QuoteIdentifier ("\r", conn), "#D6");
			cb.QuoteSuffix = " ";
			Assert.AreEqual ("Xmono ", cb.QuoteIdentifier ("mono", conn), "#D7");
			Assert.AreEqual ("XZ ", cb.QuoteIdentifier ("Z", conn), "#D8");
			Assert.AreEqual ("XX ", cb.QuoteIdentifier ("X", conn), "#D9");
			Assert.AreEqual ("X ", cb.QuoteIdentifier (string.Empty, conn), "#D10");
			Assert.AreEqual ("X   ", cb.QuoteIdentifier (" ", conn), "#D11");
			Assert.AreEqual ("X\r ", cb.QuoteIdentifier ("\r", conn), "#D12");

			cb = new OdbcCommandBuilder ();
			cb.QuotePrefix = " ";
			Assert.AreEqual ("mono", cb.QuoteIdentifier ("mono", conn), "#E1");
			Assert.AreEqual ("Z", cb.QuoteIdentifier ("Z", conn), "#E2");
			Assert.AreEqual ("abc", cb.QuoteIdentifier ("abc", conn), "#E3");
			Assert.AreEqual (string.Empty, cb.QuoteIdentifier (string.Empty, conn), "#E4");
			Assert.AreEqual (" ", cb.QuoteIdentifier (" ", conn), "#E5");
			Assert.AreEqual ("\r", cb.QuoteIdentifier ("\r", conn), "#E6");
			cb.QuoteSuffix = "def";
			Assert.AreEqual ("mono", cb.QuoteIdentifier ("mono", conn), "#E7");
			Assert.AreEqual ("Z", cb.QuoteIdentifier ("Z", conn), "#E8");
			Assert.AreEqual ("abc", cb.QuoteIdentifier ("abc", conn), "#E9");
			Assert.AreEqual (string.Empty, cb.QuoteIdentifier (string.Empty, conn), "#E10");
			Assert.AreEqual (" ", cb.QuoteIdentifier (" ", conn), "#E11");
			Assert.AreEqual ("\r", cb.QuoteIdentifier ("\r", conn), "#E12");
		}

		[Test] // QuoteIdentifier (String, OdbcConnection)
		public void QuoteIdentifier2_Connection_Closed ()
		{
			OdbcCommandBuilder cb;
			OdbcConnection conn = new OdbcConnection ();

			cb = new OdbcCommandBuilder ();
			try {
				cb.QuoteIdentifier ("mono", conn);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// QuoteIdentifier requires an open and available
				// Connection. The connection's current state is
				// closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			cb = new OdbcCommandBuilder ();
			cb.QuotePrefix = "abc";
			Assert.AreEqual ("abcmono", cb.QuoteIdentifier ("mono", conn), "#B1");
			Assert.AreEqual ("abcZ", cb.QuoteIdentifier ("Z", conn), "#B2");
			Assert.AreEqual ("abcabc", cb.QuoteIdentifier ("abc", conn), "#B3");
			Assert.AreEqual ("abc", cb.QuoteIdentifier (string.Empty, conn), "#B4");
			Assert.AreEqual ("abc ", cb.QuoteIdentifier (" ", conn), "#B5");
			Assert.AreEqual ("abc\r", cb.QuoteIdentifier ("\r", conn), "#B6");
			cb.QuoteSuffix = "def";
			Assert.AreEqual ("abcmonodef", cb.QuoteIdentifier ("mono", conn), "#B7");
			Assert.AreEqual ("abcZdef", cb.QuoteIdentifier ("Z", conn), "#B8");
			Assert.AreEqual ("abcabcdef", cb.QuoteIdentifier ("abc", conn), "#B9");
			Assert.AreEqual ("abcdef", cb.QuoteIdentifier (string.Empty, conn), "#B10");
			Assert.AreEqual ("abc def", cb.QuoteIdentifier (" ", conn), "#B11");
			Assert.AreEqual ("abc\rdef", cb.QuoteIdentifier ("\r", conn), "#B12");

			cb.QuotePrefix = string.Empty;
			try {
				cb.QuoteIdentifier ("mono");
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// QuoteIdentifier requires an open and available
				// Connection. The connection's current state is
				// closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}

			cb = new OdbcCommandBuilder ();
			cb.QuotePrefix = "X";
			Assert.AreEqual ("Xmono", cb.QuoteIdentifier ("mono"), "#D1");
			Assert.AreEqual ("XZ", cb.QuoteIdentifier ("Z"), "#D2");
			Assert.AreEqual ("XX", cb.QuoteIdentifier ("X"), "#D3");
			Assert.AreEqual ("X", cb.QuoteIdentifier (string.Empty, conn), "#D4");
			Assert.AreEqual ("X ", cb.QuoteIdentifier (" ", conn), "#D5");
			Assert.AreEqual ("X\r", cb.QuoteIdentifier ("\r", conn), "#D6");
			cb.QuoteSuffix = " ";
			Assert.AreEqual ("Xmono ", cb.QuoteIdentifier ("mono"), "#D7");
			Assert.AreEqual ("XZ ", cb.QuoteIdentifier ("Z"), "#D8");
			Assert.AreEqual ("XX ", cb.QuoteIdentifier ("X"), "#D9");
			Assert.AreEqual ("X ", cb.QuoteIdentifier (string.Empty, conn), "#D10");
			Assert.AreEqual ("X   ", cb.QuoteIdentifier (" ", conn), "#D11");
			Assert.AreEqual ("X\r ", cb.QuoteIdentifier ("\r", conn), "#D12");

			cb = new OdbcCommandBuilder ();
			cb.QuotePrefix = " ";
			Assert.AreEqual ("mono", cb.QuoteIdentifier ("mono", conn), "#E1");
			Assert.AreEqual ("Z", cb.QuoteIdentifier ("Z", conn), "#E2");
			Assert.AreEqual ("abc", cb.QuoteIdentifier ("abc", conn), "#E3");
			Assert.AreEqual (string.Empty, cb.QuoteIdentifier (string.Empty, conn), "#E4");
			Assert.AreEqual (" ", cb.QuoteIdentifier (" ", conn), "#E5");
			Assert.AreEqual ("\r", cb.QuoteIdentifier ("\r", conn), "#E6");
			cb.QuoteSuffix = "def";
			Assert.AreEqual ("mono", cb.QuoteIdentifier ("mono", conn), "#E7");
			Assert.AreEqual ("Z", cb.QuoteIdentifier ("Z", conn), "#E8");
			Assert.AreEqual ("abc", cb.QuoteIdentifier ("abc", conn), "#E9");
			Assert.AreEqual (string.Empty, cb.QuoteIdentifier (string.Empty, conn), "#E10");
			Assert.AreEqual (" ", cb.QuoteIdentifier (" ", conn), "#E11");
			Assert.AreEqual ("\r", cb.QuoteIdentifier ("\r", conn), "#E12");
		}

		[Test]
		public void SchemaSeparator ()
		{
			OdbcCommandBuilder cb = new OdbcCommandBuilder ();
			Assert.AreEqual (".", cb.SchemaSeparator, "#1");
			cb.SchemaSeparator = "a";
			Assert.AreEqual ("a", cb.SchemaSeparator, "#2");
			cb.SchemaSeparator = null;
			Assert.AreEqual (".", cb.SchemaSeparator, "#3");
			cb.SchemaSeparator = "b";
			Assert.AreEqual ("b", cb.SchemaSeparator, "#4");
			cb.SchemaSeparator = string.Empty;
			Assert.AreEqual (".", cb.SchemaSeparator, "#5");
			cb.SchemaSeparator = " ";
			Assert.AreEqual (" ", cb.SchemaSeparator, "#6");
		}
	}
}

#endif