//
// OdbcDataReaderTest.cs - NUnit Test Cases for testing the
// OdbcDataReader class
//
// Author: 
//      Sureshkumar T (TSureshkumar@novell.com)
// 
// Copyright (c) 2004 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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
	public class OdbcDataReaderTest
	{
		[Test]
		public void OutputParametersTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "call {? = sp_get_age (?,?)}";

				OdbcParameter ret = (OdbcParameter) new OdbcParameter ("ret", OdbcType.Int);
				cmd.Parameters.Add (ret);
				ret.Direction = ParameterDirection.ReturnValue;

				OdbcParameter name = (OdbcParameter) new OdbcParameter ("name", OdbcType.VarChar);
				cmd.Parameters.Add (name);
				name.Direction = ParameterDirection.Input;
				name.Value = "suresh";

				OdbcParameter age = (OdbcParameter) new OdbcParameter ("age", OdbcType.Int);
				cmd.Parameters.Add (age);
				name.Direction = ParameterDirection.Output;

				IDataReader reader = cmd.ExecuteReader ();
				reader.Close ();
				Assert.AreEqual (true, ((int) ret.Value) > 0, "#1");
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
		}
		[Test]
		public void LongTextTest ()
		{
			IDbConnection conn = new OdbcConnection (
						ConnectionManager.Singleton.ConnectionString);
			IDataReader rdr = null; 
			try {
				conn.Open ();
				IDbCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "Select type_text"; 
				cmd.CommandText += " from string_family where id=3";

				rdr = cmd.ExecuteReader ();
				rdr.Read ();
				rdr.GetValue (0);
			}finally {
				if (rdr != null)
					rdr.Close ();
				conn.Close ();
			}
		}
	}
}
