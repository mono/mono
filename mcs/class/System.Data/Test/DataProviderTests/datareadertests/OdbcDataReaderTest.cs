//
// OdbcDataReaderTest.cs 
//
// Author:
//   Satya Sudha K (ksathyasudha@novell.com)
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
using System.Data.Odbc;

namespace MonoTests.System.Data {
	
	public class MySqlOdbcRetrieve : MySqlRetrieve {
		
		public MySqlOdbcRetrieve (string database) : base (database) 
		{
		}
		
		// returns a Open connection 
		public override void GetConnection () 
                {
			string connectionString = null;
			try {
				connectionString = ConfigClass.GetElement (configDoc, "database", "OdbcConnString");
			} catch (Exception e) {
				Console.WriteLine ("Error reading the config file"); 
				Console.WriteLine (e.Message);
				return;
			}
			
			con = new OdbcConnection (connectionString);
			try {
				con.Open ();
			} catch (Exception e) {
				Console.WriteLine ("Cannot establish connection with the database");
				con = null;
			}
		}
	}
	
	public class MsSqlOdbcRetrieve : MsSqlRetrieve {
		
		public MsSqlOdbcRetrieve (string database) : base (database) 
		{
		}
		
		// returns a Open connection 
		public override void GetConnection () 
		{
			string connectionString = null;
			try {
				connectionString = ConfigClass.GetElement (configDoc, "database", "OdbcConnString");
			} catch (Exception e) {
				Console.WriteLine ("Error reading the config file");
				Console.WriteLine (e.Message);
				return;
			}

			con = new OdbcConnection (connectionString);
			try {
				con.Open ();
			} catch (Exception e) {
				Console.WriteLine ("Cannot establish connection with the database");
				con = null;
			}
		}
	}

	public class OracleOdbcRetrieve : OraRetrieve {
		
		public OracleOdbcRetrieve (string database) : base (database) 
		{
		}
		
		// returns a Open connection 
		public override void GetConnection () 
		{
			string connectionString = null;
			try {
				connectionString = ConfigClass.GetElement (configDoc, "database", "OdbcConnString");
			} catch (Exception e) {
				Console.WriteLine ("Error reading the config file");
				Console.WriteLine (e.Message);
				return;
			}

			con = new OdbcConnection (connectionString);

			try {
				con.Open ();
			} catch (Exception e) {
				Console.WriteLine ("Cannot establish connection with the database");
				con = null;
			}
		}
	}

	public class PostgreOdbcRetrieve : MySqlRetrieve {
		
		public PostgreOdbcRetrieve (string database) : base (database) 
		{
		}
		
		// returns a Open connection 
		public override void GetConnection () 
		{

			string connectionString = null;
			try {
				connectionString = ConfigClass.GetElement (configDoc, "database", "OdbcConnString");
			} catch (Exception e) {
				Console.WriteLine ("Error reading the config file");
				Console.WriteLine (e.Message);
				return;
			}
			
			con = new OdbcConnection (connectionString);
			try {
				con.Open ();
			} catch (Exception e) {
				Console.WriteLine ("Cannot establish connection with the database");
				con = null;
			}
		}
	}
}
