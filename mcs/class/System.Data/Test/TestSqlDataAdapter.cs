//
// TestPgSqlDataAdapter - tests PgSqlDataAdapter, DbDataAdapter, DataSet, DataTable,
//                      DataRow, and DataRowCollection by retrieving data
//
// Authors:
//      Tim Coleman <tim@timcoleman.com>
//      Daniel Morgan <danmorg@sc.rr.com>
//
// (c)copyright 2002 Tim Coleman
// (c)copyright 2002 Daniel Morgan
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
using System.Collections;
using System.Data;
using Mono.Data.PostgreSqlClient;

namespace TestSystemDataPgSqlClient 
{
	public class TestPgSqlDataAdapter 
	{
		public static void Test() 
		{
			string connectionString;
			string sqlQuery;
			PgSqlDataAdapter adapter;
			DataSet dataSet = null;

			connectionString =
				"host=localhost;" +
				"dbname=test;" +
				"user=postgres";
						
			sqlQuery = "select * from pg_tables";

			System.Console.WriteLine ("new PgSqlDataAdapter...");
			adapter = new PgSqlDataAdapter (sqlQuery, 
					connectionString);

			System.Console.WriteLine ("new DataSet...");
			dataSet = new DataSet ();

			try {
				System.Console.WriteLine("Fill...");
				adapter.Fill (dataSet);

			}
			catch (NotImplementedException e) {
				Console.WriteLine("Exception Caught: " + e);
			}		
			
			System.Console.WriteLine ("get row...");
			if (dataSet != null) {
				foreach (DataRow row in dataSet.Tables["Table"].Rows)
					Console.WriteLine("tablename: " + row["tablename"]);
				System.Console.WriteLine("Done.");
			}

		}

		public static void Main() 
		{
			Test();
		}
	}
}
