// SqlCommandBuilderTest.cs - NUnit Test Cases for testing SqlCommandBuilder.
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

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.Odbc
{
	[TestFixture]
	public class SqlCommandBuilderTest
	{
		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CatalogLocationTest ()
		{
			SqlCommandBuilder cb = new SqlCommandBuilder ();
			Assert.AreEqual (CatalogLocation.Start, cb.CatalogLocation, "#1");
			cb.CatalogLocation = CatalogLocation.Start;
			Assert.AreEqual (CatalogLocation.Start, cb.CatalogLocation, "#2");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CatalogLocation_Value_Invalid ()
		{
			SqlCommandBuilder cb = new SqlCommandBuilder ();
			try {
				cb.CatalogLocation = (CatalogLocation) 666;
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// The only acceptable value for the property
				// 'CatalogLocation' is 'Start'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("'CatalogLocation'") != -1, "#A5:" + ex.Message);
				Assert.IsTrue (ex.Message.IndexOf ("'Start'") != -1, "#A6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#A7");
			}
			Assert.AreEqual (CatalogLocation.Start, cb.CatalogLocation, "#A7");

			try {
				cb.CatalogLocation = CatalogLocation.End;
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// The only acceptable value for the property
				// 'CatalogLocation' is 'Start'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'CatalogLocation'") != -1, "#B5:" + ex.Message);
				Assert.IsTrue (ex.Message.IndexOf ("'Start'") != -1, "#B6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#B7");
			}
			Assert.AreEqual (CatalogLocation.Start, cb.CatalogLocation, "#B8");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CatalogSeparator ()
		{
			SqlCommandBuilder cb = new SqlCommandBuilder ();
			Assert.AreEqual (".", cb.CatalogSeparator, "#1");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void CatalogSeparator_Value_Invalid ()
		{
			string [] separators = new string [] {
				"x",
				"'",
				"[x",
				string.Empty,
				null
				};

			SqlCommandBuilder cb = new SqlCommandBuilder ();
			for (int i = 0; i < separators.Length; i++) {
				try {
					cb.CatalogSeparator = separators [i];
					Assert.Fail ("#1:" + i);
				} catch (ArgumentException ex) {
					// The acceptable value for the property
					// 'CatalogSeparator' is '.'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + i);
					Assert.IsNull (ex.InnerException, "#3:" + i);
					Assert.IsNotNull (ex.Message, "#4:" + i);
					Assert.IsTrue (ex.Message.IndexOf ("'CatalogSeparator'") != -1, "#5:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#6:" + ex.Message);
					Assert.IsNull (ex.ParamName, "#7:" + i);
				}
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ConflictOptionTest ()
		{
			SqlCommandBuilder cb = new SqlCommandBuilder ();
			Assert.AreEqual (ConflictOption.CompareAllSearchableValues, cb.ConflictOption, "#1");
			cb.ConflictOption = ConflictOption.CompareRowVersion;
			Assert.AreEqual (ConflictOption.CompareRowVersion, cb.ConflictOption, "#2");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void ConflictOption_Value_Invalid ()
		{
			SqlCommandBuilder cb = new SqlCommandBuilder ();
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
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void QuoteIdentifier ()
		{
			SqlCommandBuilder cb;
		
			cb = new SqlCommandBuilder ();
			Assert.AreEqual ("[mono]", cb.QuoteIdentifier ("mono"), "#A1");
			Assert.AreEqual ("[]", cb.QuoteIdentifier (string.Empty), "#A2");
			Assert.AreEqual ("[Z]", cb.QuoteIdentifier ("Z"), "#A3");
			Assert.AreEqual ("[[]", cb.QuoteIdentifier ("["), "#A4");
			Assert.AreEqual ("[A[C]", cb.QuoteIdentifier ("A[C"), "#A5");
			Assert.AreEqual ("[]]]", cb.QuoteIdentifier ("]"), "#A6");
			Assert.AreEqual ("[A]]C]", cb.QuoteIdentifier ("A]C"), "#A7");
			Assert.AreEqual ("[[]]]", cb.QuoteIdentifier ("[]"), "#A8");
			Assert.AreEqual ("[A[]]C]", cb.QuoteIdentifier ("A[]C"), "#A9");

			cb = new SqlCommandBuilder ();
			cb.QuotePrefix = "\"";
			cb.QuoteSuffix = "\"";
			Assert.AreEqual ("\"mono\"", cb.QuoteIdentifier ("mono"), "#B1");
			Assert.AreEqual ("\"\"", cb.QuoteIdentifier (string.Empty), "#B2");
			Assert.AreEqual ("\"Z\"", cb.QuoteIdentifier ("Z"), "#B3");
			Assert.AreEqual ("\"\"\"\"", cb.QuoteIdentifier ("\""), "#B4");
			Assert.AreEqual ("\"A\"\"C\"", cb.QuoteIdentifier ("A\"C"), "#B5");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void QuoteIdentifier_PrefixSuffix_NoMatch ()
		{
			SqlCommandBuilder cb;
		
			cb = new SqlCommandBuilder ();
			cb.QuoteSuffix = "\"";
			try {
				cb.QuoteIdentifier ("mono");
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Specified QuotePrefix and QuoteSuffix values
				// do not match
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("QuotePrefix") != -1, "#A5:" + ex.Message);
				Assert.IsTrue (ex.Message.IndexOf ("QuoteSuffix") != -1, "#A6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#A7");
			}

			cb = new SqlCommandBuilder ();
			cb.QuotePrefix = "\"";
			try {
				cb.QuoteIdentifier ("mono");
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Specified QuotePrefix and QuoteSuffix values
				// do not match
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("QuotePrefix") != -1, "#B5:" + ex.Message);
				Assert.IsTrue (ex.Message.IndexOf ("QuoteSuffix") != -1, "#B6:" + ex.Message);
				Assert.IsNull (ex.ParamName, "#B7");
			}
		}

		[Test] // QuoteIdentifier (String)
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void QuoteIdentifier_UnquotedIdentifier_Null ()
		{
			SqlCommandBuilder cb = new SqlCommandBuilder ();
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

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void QuotePrefix ()
		{
			SqlCommandBuilder cb = new SqlCommandBuilder ();
			Assert.AreEqual ("[", cb.QuotePrefix, "#A1");
			Assert.AreEqual ("]", cb.QuoteSuffix, "#A2");
			cb.QuotePrefix = "\"";
			Assert.AreEqual ("\"", cb.QuotePrefix, "#B1");
			Assert.AreEqual ("]", cb.QuoteSuffix, "#B2");
			cb.QuotePrefix = "[";
			Assert.AreEqual ("[", cb.QuotePrefix, "#C1");
			Assert.AreEqual ("]", cb.QuoteSuffix, "#C2");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void QuotePrefix_Value_Invalid ()
		{
			string [] prefixes = new string [] {
				"x",
				"'",
				"[x",
				string.Empty,
				null,
				"]"
				};

			SqlCommandBuilder cb = new SqlCommandBuilder ();
			for (int i = 0; i < prefixes.Length; i++) {
				try {
					cb.QuotePrefix = prefixes [i];
					Assert.Fail ("#1:" + i);
				} catch (ArgumentException ex) {
					// The acceptable values for the property
					// 'QuoteSuffix' are ']' or '"'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + i);
					Assert.IsNull (ex.InnerException, "#3:" + i);
					Assert.IsNotNull (ex.Message, "#4:" + i);
					Assert.IsNull (ex.ParamName, "#5:" + i);
				}
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void QuoteSuffix ()
		{
			SqlCommandBuilder cb = new SqlCommandBuilder ();
			Assert.AreEqual ("[", cb.QuotePrefix, "#A1");
			Assert.AreEqual ("]", cb.QuoteSuffix, "#A2");
			cb.QuoteSuffix = "\"";
			Assert.AreEqual ("[", cb.QuotePrefix, "#B1");
			Assert.AreEqual ("\"", cb.QuoteSuffix, "#B2");
			cb.QuoteSuffix = "]";
			Assert.AreEqual ("[", cb.QuotePrefix, "#C1");
			Assert.AreEqual ("]", cb.QuoteSuffix, "#C2");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void QuoteSuffix_Value_Invalid ()
		{
			string [] suffixes = new string [] {
				"x",
				"'",
				"[x",
				string.Empty,
				null,
				"["
				};

			SqlCommandBuilder cb = new SqlCommandBuilder ();
			for (int i = 0; i < suffixes.Length; i++) {
				try {
					cb.QuoteSuffix = suffixes [i];
					Assert.Fail ("#1:" + i);
				} catch (ArgumentException ex) {
					// The acceptable values for the property
					// 'QuoteSuffix' are ']' or '"'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + i);
					Assert.IsNull (ex.InnerException, "#3:" + i);
					Assert.IsNotNull (ex.Message, "#4:" + i);
					Assert.IsNull (ex.ParamName, "#5:" + i);
				}
			}
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void SchemaSeparator ()
		{
			SqlCommandBuilder cb = new SqlCommandBuilder ();
			Assert.AreEqual (".", cb.SchemaSeparator, "#1");
			cb.SchemaSeparator = ".";
			Assert.AreEqual (".", cb.SchemaSeparator, "#2");
		}

		[Test]
#if FEATURE_NO_BSD_SOCKETS
		[ExpectedException (typeof (PlatformNotSupportedException))]
#endif
		public void SchemaSeparator_Value_Invalid ()
		{
			string [] separators = new string [] {
				"x",
				"'",
				"[x",
				string.Empty,
				null
				};

			SqlCommandBuilder cb = new SqlCommandBuilder ();
			for (int i = 0; i < separators.Length; i++) {
				try {
					cb.SchemaSeparator = separators [i];
					Assert.Fail ("#1:" + i);
				} catch (ArgumentException ex) {
					// The acceptable value for the property
					// 'SchemaSeparator' is '.'
					Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2:" + i);
					Assert.IsNull (ex.InnerException, "#3:" + i);
					Assert.IsNotNull (ex.Message, "#4:" + i);
					Assert.IsTrue (ex.Message.IndexOf ("'SchemaSeparator'") != -1, "#5:" + ex.Message);
					Assert.IsTrue (ex.Message.IndexOf ("'.'") != -1, "#6:" + ex.Message);
					Assert.IsNull (ex.ParamName, "#7:" + i);
				}
			}
		}
	}
}
