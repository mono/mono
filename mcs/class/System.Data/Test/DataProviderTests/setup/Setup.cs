//
// Setup.cs : Setup class for 
//              - creating, dropping, insert data into database tables
//              - Setup/teardown of stored procedures
//            
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
using System.IO;
using System.Xml;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using ByteFX.Data.MySqlClient;
using Npgsql;
using System.Configuration;

namespace MonoTests.System.Data {
	
	public class Setup {

		string []     databases;
		IDbConnection con;
		IDbCommand    cmd;
		XmlNode       node;
		string        curDatabase;
		TableInfo []  tabinfo;
		string        createStoredProc;
		string        deleteTables;
		string        createTables;
		string        insertData;
		
		public Setup (string [] listOfDbs) 
		{
		
			if (listOfDbs.Length == 0) {
				string dbList = ConfigurationSettings.AppSettings ["Databases"];
				databases = dbList.Split (';');
			} else
				databases = (string []) listOfDbs.Clone ();

			Reset ();      
		}
		
		void Reset () 
		{
			if (con != null)
				con.Close ();

			con = null;
			cmd = null;
			node = null;
			tabinfo = null;
			curDatabase = null;
			createStoredProc = null;
			deleteTables = null;
			createTables = null;
			insertData = null;
		}
		
		bool Initialize (string database) 
		{
		
			curDatabase = database;
			node = (XmlNode) ConfigurationSettings.GetConfig (database);
			con = GetConnection (database);

			try {
				con.Open ();
			} catch (Exception e) {
				createStoredProc = deleteTables = createTables = insertData = null; 
				Console.WriteLine (e.Message);
				Reset ();
				return false;
			}
			
			cmd = con.CreateCommand ();
			
			createStoredProc = ConfigClass.GetElement (node, "createStoredProc");
			deleteTables = ConfigClass.GetElement (node, "deleteTables");
			createTables = ConfigClass.GetElement (node, "createTables");
			insertData = ConfigClass.GetElement (node, "insertData");
			string noOfTables = ConfigClass.GetElement (node, "tables", "numTables");
			int numTables = Convert.ToInt32 (noOfTables);
			
			tabinfo = new TableInfo [numTables];

			for (int i = 1; i <= numTables; i++)
				tabinfo [i - 1].Initialize (node, i);
		
			return true;
		}
		
		public void SetupDatabase () 
		{
			foreach (string db in databases) {

				bool hasErrors = false;
				Console.WriteLine ("\n ******** Doing setup for {0} database ********\n", db);

				if (Initialize (db) != true) {
					Console.WriteLine ("Failed to do the initialisation for {0} database", db);
					Console.WriteLine ("Skipping setup for " + db);
					hasErrors = true;
					continue;
				}

				Console.WriteLine ("  *** Running the following queries ***\n");
				if (deleteTables.Equals ("Y")) {
					if (DeleteTables ()!= true) 
						hasErrors = true;
				}

				if (createTables.Equals ("Y")) {
			  		if (CreateTables () != true)
			    			hasErrors = true;
				}

				if (insertData.Equals ("Y")) {
			  		if (InsertData () != true) {
			    			hasErrors = true;
			  		}
				}

				if (createStoredProc.Equals ("Y")) {

			  		int numStoredProc = Convert.ToInt32 (ConfigClass.GetElement (node, 
								"StoredProc", "NumStoredProc"));
			  		for (int i = 1; i <= numStoredProc; i++) {
			    			if (CreateStoredProc (i) != true)
			      				hasErrors = true;
			  		}

					if (hasErrors == true) 
			    			Console.WriteLine ("There were errors while setting up the {0} database", db);
			  		 else 
			    			Console.WriteLine ("Successfully set up the {0} database", db);
				}
			}
		}
		
		bool CreateTables () 
		{
		
			string createQuery;
			for (int i = 1; i<= tabinfo.Length; i++) {
				string [] constraints = ConfigClass.GetColumnDetails (node, i, "constraint");
			
				createQuery = "create table " + tabinfo [i - 1].name;
				createQuery += "(";
			
				for (int col = 1; col <= tabinfo [i - 1].columns.Length; col++) {
				
					createQuery += tabinfo [i - 1].columns [col - 1];
					createQuery += " " ;
					createQuery += tabinfo [i - 1].types [col - 1];
					createQuery += " " + constraints [col - 1];
					createQuery += ",";
				}
				createQuery = createQuery.Trim (',');
				createQuery += ")";
				Console.WriteLine (createQuery);
				cmd.CommandText = createQuery;
				cmd.ExecuteNonQuery ();
			}
			return true;
		}
		
		bool InsertData () 
		{
			int numTables = Convert.ToInt32 (ConfigClass.GetElement (node, "values", "numTables"));
			string tableName;

			for (int i = 1; i <= numTables; i++) {

				string tableTag = "table" + i;
				tableName = ConfigClass.GetElement (node, "values", tableTag, "tableName");
				int numRows = Convert.ToInt32 (ConfigClass.GetElement (node, "values", tableTag, "numRows"));
				int numCols = Convert.ToInt32 (ConfigClass.GetElement (node, "values", tableTag, "numCols"));
				for (int j = 1; j <= numRows; j++) {

					string rowTag = "row" + j;
					string insertQuery = "Insert into " + tableName + " values (";

					for (int k = 1; k <= numCols; k++) {
						string colTag = "column"+k;
						insertQuery += ConfigClass.GetElement (node, "values", tableTag, rowTag, colTag);
						insertQuery += ",";
					}

					insertQuery = insertQuery.Trim (',');
					insertQuery += ")";
					Console.WriteLine (insertQuery);
					cmd.CommandText = insertQuery;

					try {
						cmd.ExecuteNonQuery ();
					} catch (Exception e) {
						Console.WriteLine ("Failed to insert row into the table:" +
							tableName + " " + e.Message);
						return false;
					}
				}
			}

			return true;
		}
		
		bool DeleteTables () 
		{
		
			string deleteQuery;
			bool retval = true;

			for (int i = 1; i <= tabinfo.Length; i++) {

				deleteQuery = "drop table " + tabinfo [i - 1].name;
				Console.WriteLine (deleteQuery);
				cmd.CommandText = deleteQuery;

				try {
					cmd.ExecuteNonQuery ();
				} catch (Exception e) {
					Console.WriteLine ("Unable to drop table :" + tabinfo [i - 1].name + ":" + e.Message);
					retval = false;
				}
			}

			return retval;
		}
		
		bool CreateStoredProc (int storedProcNum) 
		{
			
			string name = ConfigClass.GetElement (node, "StoredProc", "StoredProc" + storedProcNum, "name");
			string type = ConfigClass.GetElement (node, "StoredProc", "StoredProc" + storedProcNum, "type");
			int numStatements = Convert.ToInt32 (ConfigClass.GetElement (node, "StoredProc", 
						"StoredProc" + storedProcNum, "template", "numStmts"));

			string [] templates = new string [numStatements];

			for (int i = 1; i <= numStatements; i++)
				templates [i - 1] = ConfigClass.GetElement (node, "StoredProc", "StoredProc" + storedProcNum, "template", "stmt"+i);
			
			if (type.Equals ("generic")) {
				// To be created for all tables
				for (int tableNum = 0; tableNum < tabinfo.Length; tableNum ++) {

					string storedProcName = name.Replace ("{{TABLE}}", tabinfo [tableNum].name);
					Console.WriteLine ("Creating : " + storedProcName);

					for (int index = 1; index <= numStatements; index++) {

						string SPtemplate = templates [index - 1];
						SPtemplate = SPtemplate.Replace ("{{TABLE}}", tabinfo [tableNum].name);
						string listOfColumns = String.Join (",", tabinfo [tableNum].columns);
						SPtemplate = SPtemplate.Replace ("{{TABLE}}", tabinfo [tableNum].name);
						SPtemplate = SPtemplate.Replace ("{{COLUMNS}}", listOfColumns);
						int beg = 0;
						while ((beg = SPtemplate.IndexOf ("{{COLUMN_")) >= 0) {
							int end = SPtemplate.IndexOf ("}}", beg + 9);
							string strToBeReplaced = SPtemplate.Substring (beg, end - beg + 2);
							beg += 9;
							int columnNum = Convert.ToInt32 (SPtemplate.Substring (beg, end - beg));
							SPtemplate = SPtemplate.Replace (strToBeReplaced, 
									tabinfo [tableNum].columns [columnNum]);
						}

						SPtemplate = SPtemplate.Replace ("\r", "");
						cmd.CommandText = SPtemplate;
						Console.WriteLine (SPtemplate);
						cmd.ExecuteNonQuery ();
					}
				}

			} else {
			// To be implemented
			}

			return true;
		}
		
		IDbConnection GetConnection (string database) 
		{
		
			IDbConnection con = null;
			string connStr = ConfigClass.GetElement (node, "database", "connectionString");
			if (database == "oracle") {
				con = new OracleConnection (connStr);
			} else if (database == "mysql") {
				con = new MySqlConnection (connStr);
			} else if (database == "mssql") {
				con = new SqlConnection (connStr);
			} else if (database == "postgres") {
				con = new NpgsqlConnection (connStr);
			}

			return con;
		}
		
	}
}
