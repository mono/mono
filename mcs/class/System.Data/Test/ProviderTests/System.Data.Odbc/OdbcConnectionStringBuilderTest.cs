// OdbcConnectionStringBuilderTest.cs - NUnit Test Cases for testing the
// OdbcConnectionStringBuilder Class.
//
// Authors:
//      Nidhi Rawal (rawalnidhi_rawal@yahoo.com)
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

using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using Mono.Data;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcConnectionStringBuilderTest
	{
		[Test]
		public void ClearTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			string temp = "";

			builder ["Dbq"] = "C:\\Data.xls";
			builder ["DriverID"] = "790";
			builder ["DefaultDir"] = "C:\\";

			foreach (string key in builder.Keys)
				temp += key + " = " + builder [key].ToString () + "  ";
			Assert.AreEqual ("Dbq = C:\\Data.xls  DriverID = 790  DefaultDir = C:\\  ", temp, "#1 All the keys and their values before clearing");

			builder.Clear ();
			temp = "";

			foreach (string key in builder.Keys)
				temp += key + " = " + builder [key].ToString () + "\n";
			Assert.AreEqual ("", temp, "#2 All the keys and their values after clearing");
		}

		[Test]
		public void ContainsKeyTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["SourceType"] = "DBC";
			Assert.AreEqual (true, builder.ContainsKey ("SourceType"), "#1 Returns true for the key explicitly added");
			Assert.AreEqual (false, builder.ContainsKey ("DSN"), "#2 Returns false for the key not explicitly added");
			Assert.AreEqual (false, builder.ContainsKey ("xyz"), "#3 Returns false for any invalid string");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ContainsKeyNullArgumentTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["SourceType"] = "DBC";
			builder.ContainsKey (null);
		}

		[Test]
		public void RemoveTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["DriverID"] = "790";
			builder ["DefaultDir"] = "C:\\";

			Assert.AreEqual (true, builder.Remove ("DriverID"), "#1 Removes and returns true for the key explicitly added");
			Assert.AreEqual (false, builder.Remove ("userid"), "#2 Unable to find key, returns false");
			Assert.AreEqual (false, builder.Remove ("DriverID"), "#3 Cannot find the key previously removed");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveNullArgumentTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder.Remove (null);
		}

		[Test]
		public void TryGetValueTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			object value = null;
			builder ["DriverID"] = "790";
			builder ["Server"] = "C:\\";
			Assert.AreEqual (true, builder.TryGetValue ("DriverID", out value), "#1 Gets the value and returns true");
			Assert.AreEqual (false, builder.TryGetValue ("SERVER", out value), "#2 Unable to find the key");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryGetValueNullArgumentTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			object value = null;
			builder.TryGetValue (null, out value);
		}
	}
}
