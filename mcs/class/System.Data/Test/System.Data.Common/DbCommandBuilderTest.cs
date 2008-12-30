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
