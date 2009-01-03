// DbCommandBuilderTest.cs - NUnit Test Cases for DbCommandBuilder class
//
// Author: 
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2008 Gert Driesen
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
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.Data.Common
{
	[TestFixture]
	public class DbCommandBuilderTest
	{
		[Test]
		public void CatalogLocationTest ()
		{
			MyCommandBuilder cb = new MyCommandBuilder ();
			Assert.AreEqual (CatalogLocation.Start, cb.CatalogLocation, "#1");
			cb.CatalogLocation = CatalogLocation.End;
			Assert.AreEqual (CatalogLocation.End, cb.CatalogLocation, "#2");
		}

		[Test]
		public void CatalogLocation_Value_Invalid ()
		{
			MyCommandBuilder cb = new MyCommandBuilder ();
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
				Assert.AreEqual ("CatalogLocation", ex.ParamName, "#7");
			}
			Assert.AreEqual (CatalogLocation.End, cb.CatalogLocation, "#8");
		}

		[Test]
		public void CatalogSeparator ()
		{
			MyCommandBuilder cb = new MyCommandBuilder ();
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
			MyCommandBuilder cb = new MyCommandBuilder ();
			Assert.AreEqual (ConflictOption.CompareAllSearchableValues, cb.ConflictOption, "#1");
			cb.ConflictOption = ConflictOption.CompareRowVersion;
			Assert.AreEqual (ConflictOption.CompareRowVersion, cb.ConflictOption, "#2");
		}

		[Test]
		public void ConflictOption_Value_Invalid ()
		{
			MyCommandBuilder cb = new MyCommandBuilder ();
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

		[Test] // QuoteIdentifier (String)
		public void QuoteIdentifier ()
		{
			MyCommandBuilder cb = new MyCommandBuilder ();
			try {
				cb.QuoteIdentifier ((string) null);
				Assert.Fail ("#A1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.AreEqual ((new NotSupportedException ()).Message, ex.Message, "#A4");
			}

			try {
				cb.QuoteIdentifier ("mono");
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.AreEqual ((new NotSupportedException ()).Message, ex.Message, "#B4");
			}
		}

		[Test]
		public void QuotePrefix ()
		{
			MyCommandBuilder cb = new MyCommandBuilder ();
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
			MyCommandBuilder cb = new MyCommandBuilder ();
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

		[Test]
		public void SchemaSeparator ()
		{
			MyCommandBuilder cb = new MyCommandBuilder ();
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

		private class MyCommandBuilder : DbCommandBuilder
		{
			protected override string GetParameterPlaceholder (int parameterOrdinal)
			{
				return string.Format (CultureInfo.InvariantCulture,
					"@PH:{0}@", parameterOrdinal);
			}

			protected override string GetParameterName (string parameterName)
			{
				return string.Format (CultureInfo.InvariantCulture,
					"@NAME:{0}@", parameterName);
			}

			protected override string GetParameterName (int parameterOrdinal)
			{
				return string.Format (CultureInfo.InvariantCulture,
					"@NAME:{0}@", parameterOrdinal);
			}

			protected override void ApplyParameterInfo (DbParameter parameter, DataRow row, StatementType statementType, bool whereClause)
			{
			}

			protected override void SetRowUpdatingHandler (DbDataAdapter adapter)
			{
			}
		}
	}
}
#endif // NET_2_0
