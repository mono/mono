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
				"user=danmorg;" +
				"password=viewsonic";
			
			try {
				con = new SqlConnection(connectionString);
				con.Open();

				sql = 	"select count(*) " + 
					"from sometable";
				cmd = new SqlCommand(sql,con);
				Console.WriteLine("Executing...");
				Int64 rowCount = (Int64) cmd.ExecuteScalar();
				Console.WriteLine("Row Count: " + rowCount);

				sql = 	"select max(tdesc) " + 
					"from sometable";
				cmd = new SqlCommand(sql,con);
				Console.WriteLine("Executing...");
				String maxValue = (string) cmd.ExecuteScalar();
				Console.WriteLine("Max Value: " + maxValue);

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
