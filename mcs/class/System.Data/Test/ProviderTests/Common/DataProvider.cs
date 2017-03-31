//
// DataProvider.cs  - Holds the data used for Validating Reader Classes
// Author:
//      Senganal T (tsenganal@novell.com)
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
using System.Data.SqlClient;

namespace MonoTests.System.Data.Connected
{
	public class DataProvider
	{
		private readonly DataSet data;

		// TODO : The Data is now got from the Database.
		// Needs to be modified to get the data from a config file
		public DataProvider ()
		{
			data = new DataSet ();
			string query = "Select * from numeric_family order by id ASC;";
			query += "Select * from string_family order by id ASC;";
			query += "Select * from binary_family order by id ASC;";
			query += "Select * from datetime_family order by id ASC;";

			SqlDataAdapter adapter = new SqlDataAdapter (query,
				ConnectionManager.Instance.Sql.ConnectionString);
			adapter.TableMappings.Add ("Table", "numeric_family");
			adapter.TableMappings.Add ("Table1", "string_family");
			adapter.TableMappings.Add ("Table2", "binary_family");
			adapter.TableMappings.Add ("Table3", "datetime_family");

			data.Tables.Add ("numeric_family");
			data.Tables.Add ("string_family");
			data.Tables.Add ("binary_family");
			data.Tables.Add ("datetime_family");
			adapter.Fill (data);
		}

		public DataSet GetDataSet ()
		{
			return data;
		}
	}
}
