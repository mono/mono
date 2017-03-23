// DbDataReaderTest.cs - NUnit Test Cases for testing the
// DbDataReader family of classes
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
using System.Data.SqlTypes;
using System.Globalization;
using NUnit.Framework;

namespace MonoTests.System.Data.Connected
{
	[TestFixture]
	[Category ("sqlserver")]
	public class DbDataReaderTest
	{
		DbConnection conn;
		DbCommand cmd;
		DbDataReader rdr;

		[SetUp]
		public void SetUp ()
		{
			conn = ConnectionManager.Instance.Sql.Connection;
			cmd = conn.CreateCommand ();
		}

		[TearDown]
		public void TearDown ()
		{
			cmd?.Dispose ();
			rdr?.Dispose ();
			ConnectionManager.Instance.Close ();
		}

		[Test]
		public void GetProviderSpecificValues_Reader_Closed ()
		{
			cmd.CommandText = "SELECT * FROM employee";
			rdr = cmd.ExecuteReader ();
			rdr.Close ();

			try {
				rdr.GetProviderSpecificValues (null);
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// Invalid attempt to call MetaData
				// when reader is closed
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void GetProviderSpecificValues_Reader_NoData ()
		{
			cmd.CommandText = "SELECT * FROM employee where id = 6666";
			rdr = cmd.ExecuteReader ();

			try {
				rdr.GetProviderSpecificValues (null);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// Invalid attempt to read when no data
				// is present
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			Assert.IsFalse (rdr.Read (), "B");

			try {
				rdr.GetProviderSpecificValues (null);
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// Invalid attempt to read when no data
				// is present
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}
		}
	}
}

