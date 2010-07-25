//
// Test/ExecuteScalar.cs
//
// Test the ExecuteScalar method in the 
// System.Data.SqlClient.SqlCommand class
//
// ExecuteScalar is meant to be lightweight
// compared to ExecuteReader and only
// returns one column and one row as one object.
//
// It is meant for SELECT SQL statements that
// use an aggregate/group by function, such as,
// count(), sum(), avg(), min(), max(), etc...
// 
// The object that is returned you do an
// explicit cast.  For instance, to retrieve a
// Count of rows in a PostgreSQL table, you
// would use "SELECT COUNT(*) FROM SOMETABLE"
// which returns a number of oid type 20 which is 
// a PostgreSQL int8 which maps to 
// the .NET type System.Int64.  You
// have to explicitly convert this returned object
// to the type you are expecting, such as, an Int64
// is returned for a COUNT().
// would be:
//      Int64 myCount = (Int64) cmd.ExecuteScalar(selectStatement);
//
// Author:
//	Daniel Morgan <danmorg@sc.rr.com>
//
// (C) 2002 Daniel Morgan
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Data.SqlClient;

namespace TestSystemDataSqlClient
{
	class TestSqlDataReader
	{

		static void Test() { 
			SqlConnection con = null;
			SqlCommand cmd = null;
						
			String connectionString = null;
			String sql = null;

			connectionString = 
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres";
			
			try {
				string maxStrValue;

				con = new SqlConnection(connectionString);
				con.Open();

				// test SQL Query for an aggregate count(*)
				sql = 	"select count(*) " + 
					"from sometable";
				cmd = new SqlCommand(sql,con);
				Console.WriteLine("Executing: " + sql);
				Int64 rowCount = (Int64) cmd.ExecuteScalar();
				Console.WriteLine("Row Count: " + rowCount);

				// test SQL Query for an aggregate min(text)
				sql = 	"select max(tdesc) " + 
					"from sometable";
				cmd = new SqlCommand(sql,con);
				Console.WriteLine("Executing: " + sql);
				string minValue = (string) cmd.ExecuteScalar();
				Console.WriteLine("Max Value: " + minValue);

				// test SQL Query for an aggregate max(text)
				sql = 	"select min(tdesc) " + 
					"from sometable";
				cmd = new SqlCommand(sql,con);
				Console.WriteLine("Executing: " + sql);
				maxStrValue = (string) cmd.ExecuteScalar();
				Console.WriteLine("Max Value: " + maxStrValue);

				// test SQL Query for an aggregate max(int)
				sql = 	"select min(aint4) " + 
					"from sometable";
				cmd = new SqlCommand(sql,con);
				Console.WriteLine("Executing: " + sql);
				int maxIntValue = (int) cmd.ExecuteScalar();
				Console.WriteLine("Max Value: " + maxIntValue.ToString());

				// test SQL Query for an aggregate avg(int)
				sql = 	"select avg(aint4) " + 
					"from sometable";
				cmd = new SqlCommand(sql,con);
				Console.WriteLine("Executing: " + sql);
				decimal avgDecValue = (decimal) cmd.ExecuteScalar();
				Console.WriteLine("Max Value: " + avgDecValue.ToString());

				// test SQL Query for an aggregate sum(int)
				sql = 	"select sum(aint4) " + 
					"from sometable";
				cmd = new SqlCommand(sql,con);
				Console.WriteLine("Executing: " + sql);
				Int64 summed = (Int64) cmd.ExecuteScalar();
				Console.WriteLine("Max Value: " + summed);

				// test a SQL Command is (INSERT, UPDATE, DELETE)
				sql = 	"insert into sometable " +
					"(tid,tdesc,aint4,atimestamp) " +
					"values('qqq','www',234,NULL)";
				cmd = new SqlCommand(sql,con);
				Console.WriteLine("Executing: " + sql);
				object objResult1 = cmd.ExecuteScalar();
				if(objResult1 == null)
                                        Console.WriteLine("Result is null. (correct)");
				else
					Console.WriteLine("Result is not null. (not correct)");

				// test a SQL Command is not (INSERT, UPDATE, DELETE)
				sql = 	"SET DATESTYLE TO 'ISO'";
				cmd = new SqlCommand(sql,con);
				Console.WriteLine("Executing: " + sql);
				object objResult2 = cmd.ExecuteScalar();
				if(objResult2 == null)
					Console.WriteLine("Result is null. (correct)");
				else
					Console.WriteLine("Result is not null. (not correct)");

			}
			catch(Exception e) {
				Console.WriteLine(e.ToString());
			}
			finally {
				if(con != null)
					if(con.State == ConnectionState.Open)
						con.Close();
			}
		}

		[STAThread]
		static void Main(string[] args)
		{
			Test();
		}

	}
}
