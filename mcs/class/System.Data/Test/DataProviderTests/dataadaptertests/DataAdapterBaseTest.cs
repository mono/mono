//
// DataAdapterBaseTest.cs : Defines a base class 'BaseAdapter' that provides the common 
//                          functionality of :
//                             1) Reading a config file containing the 
//                                database connection parameters, different 
//                                tables and their description, Values that 
//                                the tables are populated with.
//                             2) Retrieves data from these tables (Fills a dataset).
//                             3) Compares the retrieved values against the ones
//                                contained in the config file.
//
// A class specific to each database (and ODBC) is derived from this class.
// These classes contain code specific to different databases (like establishing 
// a connection, comparing date values, etc).
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
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Text.RegularExpressions;

namespace MonoTests.System.Data {

	public class BaseAdapter {
	
		public IDbConnection con;
		public IDbCommand cmd;
		public DbDataAdapter dataAdapter;
		public DataSet dataset;
		string [,] setOfChanges;
		protected XmlNode configDoc;
		
		public BaseAdapter (string database) 
                {
			con = null;
			cmd = null;
			dataAdapter = null;
			dataset = null;
			setOfChanges = null;
			configDoc = (XmlNode) ConfigurationSettings.GetConfig (database);
		}
	
		void CreateCommand () 
		{
			if (con == null) 
				return;
			cmd = con.CreateCommand ();
		}
	
		// Method that actually runs the entire test : Connects to a database, 
		// retrieves values from different tables, and compares them against 
		// the values that we had entered
		public void RunTest () 
		{
		
			GetConnection ();
			if (con == null)
				return;
		
			CreateCommand ();
			if (cmd == null)
				return;
			
			string noOfQueries = null;
			string errorMsg = "";
			string query = null;
			
			try {
				noOfQueries = ConfigClass.GetElement (configDoc, "queries", "numQueries");
				int numQueries = Convert.ToInt32 (noOfQueries);
				string tableName = null;
				int [] columnNos = null;
				int tableNum = 0;
				Console.WriteLine ("\n**** Testing Data Retrieval using datasets*****\n");

				for (int i = 1; i <= numQueries; i++) {
					errorMsg = "";
					try {
						query = ConfigClass.GetElement (configDoc, "queries", "query" + i);
						query = FrameQuery (query, ref columnNos, ref tableNum);
						tableName = ConfigClass.GetElement (configDoc, "tables", "table" + tableNum, "name");
					} catch (XPathException e) {
						Console.WriteLine (e.Message);
						continue; // need not return here; try with the next one
					} 

					try {
						PopulateDataSetFromTable (query, tableName);
					} catch (Exception e) {
						Console.WriteLine ("Table : {0} : Unable to fill the dataset!!!", tableName);
						Console.WriteLine ("ERROR : " + e.Message);
						Console.WriteLine ("STACKTRACE : " + e.StackTrace);
						continue;
					}
					
					CompareData (tableNum, setOfChanges, columnNos);
				}
		
				string [] columnNames = null;
				string noOfTables = ConfigClass.GetElement (configDoc, "tables", "numTables");
				int numTables = 0;
				if (noOfTables != null)
					numTables = Convert.ToInt32 (noOfTables);

				for (int i = 1; i <= numTables; i++) {

					setOfChanges = null;
					try {
						tableName = ConfigClass.GetElement (configDoc, "tables", "table" + i, "name");
						columnNames = ConfigClass.GetColumnNames (configDoc, i);
					} catch (XPathException e) {
						Console.WriteLine (e.Message);
						continue; // need not return here; try with the next one
					} 

					try {
						query = "Select " + String.Join (",", columnNames) + " from " + tableName;
						PopulateDataSetFromTable (query, tableName);
					} catch (Exception e) {
						Console.WriteLine ("Table : {0} : Unable to fill the dataset after " +
							"updating the database!!!", tableName);
						Console.WriteLine ("ERROR : " + e.Message);
						Console.WriteLine ("STACKTRACE : " + e.StackTrace);
						continue;
					}

					if (dataset == null) {
						Console.WriteLine ("Unable to populate the dataset!!!");
						continue;
					}

					MakeChanges (i, ref errorMsg);

					if (dataset.HasChanges() == false) {
						Console.WriteLine ("\nTable : {0} : No Changes for this table in the config file",
							tableName);
						continue;
					} else {
						if (ReconcileChanges (tableName, ref errorMsg) == false) {
							Console.WriteLine ("Table : {0} : Unable to " +
								"update the database !!!", tableName);
							Console.WriteLine (errorMsg);
							continue;
						} else {
							Console.WriteLine ("\nTable : {0} : Updated " +
								"using datasets", tableName);
						}
					}

					Console.WriteLine ("\nTable : {0} : Refilling the dataset\n", tableName);
					// Clear the data in the dataset
					dataset.Clear ();
					//Fill again from the database
					dataAdapter.Fill (dataset, tableName);
					CompareData (i, setOfChanges, null);
				}
				
			} catch (Exception e) {
				Console.WriteLine ("ERROR : " + e.Message);
				Console.WriteLine ("STACKTRACE : " + e.StackTrace);
			} finally {
				con.Close ();
				con = null;
			}
		
		}

		public virtual IDataReader QueryUsingStoredProc (IDbCommand cmd,
								 string storedProcName,
								 string paramName) 
		{
			
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.CommandText = storedProcName;
			IDataReader rdr = null;
			try {
				rdr = cmd.ExecuteReader ();
			} catch (Exception e) {
				Console.WriteLine ("Could not execute command : " + cmd.CommandText);
				Console.WriteLine ("ERROR : " + e.Message);
				Console.WriteLine ("STACKTRACE : " + e.StackTrace);
				return null;
			}

			return rdr;
		}

		protected string FrameQuery (string queryStr, 
					     ref int [] columnNos, 
					     ref int tableNum) 
		{
			string regexp = "\\b(Select|select) (?<columnList>(COLUMNS|((COLUMN\\d+,)*(COLUMN\\d+)))) from (?<tableName>TABLE\\d+)( order by (?<OrderBy>COLUMN\\d+))*";
			Match m = Regex.Match (queryStr, regexp, RegexOptions.ExplicitCapture);

			if (!m.Success) {
				Console.WriteLine ("Incorrect query format!!!");
				return null;
			}

			columnNos = null;
			while (m.Success) {

				string tableTag = m.Result ("${tableName}");
				tableNum = Convert.ToInt32 (tableTag.Replace ("TABLE", ""));
				string tableName = ConfigClass.GetElement (configDoc, "tables", tableTag.ToLower (), "name");
				queryStr = queryStr.Replace (tableTag, tableName);

				for (int i = 0; i < m.Groups.Count; i++) {

					Group g = m.Groups [i];
					CaptureCollection cc = g.Captures;

					for (int j = 0; j < cc.Count; j++) {

						string matchedVal = cc [j].Value;

						if (matchedVal.Equals ("COLUMNS")) {
							string [] columnNames = ConfigClass.GetColumnNames (configDoc, tableNum);
							queryStr = queryStr.Replace ("COLUMNS", String.Join (",", columnNames));
							columnNos = new int [columnNames.Length];
							for (int index = 1; index <= columnNos.Length; index++) {
								columnNos [index - 1] = index;
							}
						} else if (matchedVal.StartsWith ("COLUMN")) {
							// May be a column name or a comma
							// separated list of columns
							string [] listOfColumns = matchedVal.Split (',');

							if (columnNos == null) {

								columnNos = new int [listOfColumns.Length];
								int colIndex = 0;
								foreach (string str in listOfColumns) {
									int columnNo = Convert.ToInt32 (str.Replace ("COLUMN", ""));
									columnNos [colIndex++] = columnNo;
								}
							}

							foreach (string str in listOfColumns) {
								string columnName = ConfigClass.GetElement (configDoc, "tables",
										tableTag.ToLower (), str.ToLower (), "name");
								queryStr = queryStr.Replace (str, columnName);
							}
						}
					}
				}

				m = m.NextMatch ();
			}
		
			return queryStr;
		}
		
		public virtual bool ReconcileChanges (string tableName, ref string errorMsg) 
		{
			return false;
		}
		
		public virtual void PopulateDataSetFromTable (string queryStr, string tableName) 
		{
			return;
		}
		
		public virtual void MakeChanges (int tableNum, ref string errorMsg) 
		{
			string numchanges = null;
			try {
				numchanges = ConfigClass.GetElement (configDoc, "values", "table" + tableNum, 							"changes", "numChanges"); 
			} catch (Exception e) {
				return;
			}
			
			int noChanges = Convert.ToInt32 (numchanges);
			string tableName = ConfigClass.GetElement (configDoc, "values", "table" + tableNum, "tableName");
			int numRows = Convert.ToInt32 (ConfigClass.GetElement (configDoc, "values", "table" + tableNum, "numRows"));
			int numCols = Convert.ToInt32 (ConfigClass.GetElement (configDoc, "values", "table" + tableNum, "numCols"));
			setOfChanges = new string [numRows,numCols];

			for (int x = 0; x < numRows; x++) 
				for (int y = 0; y < numCols; y++)
					setOfChanges [x,y] = null;
			
			int dbTableNo = -1;
			
			foreach (DataTable dbTable in dataset.Tables) {

				dbTableNo ++;
				if (tableName.Equals (dbTable.TableName))
					break;
			}
			
			for (int index = 1; index <= noChanges; index++) {

				string tagname = "change" + index;
				int row = Convert.ToInt32 (ConfigClass.GetElement (configDoc, "values", 
					"table" + tableNum, "changes", tagname, "row"));
				int col = Convert.ToInt32 (ConfigClass.GetElement (configDoc, "values", 
					"table" + tableNum, "changes", tagname, "col"));
				string value = ConfigClass.GetElement (configDoc, "values", 
					"table" + tableNum, "changes", tagname, "value");
				setOfChanges [row - 1,col - 1] = value;
				DataRow drow = dataset.Tables [dbTableNo].Rows [row - 1];
				DataColumn dcol = dataset.Tables [dbTableNo].Columns [col - 1];
				object dataSetValue = drow [dcol];
				try {
					drow [dcol] = ConvertToType (dataSetValue.GetType (), value, ref errorMsg);
				} catch (Exception e) {
					drow [dcol] = DBNull.Value;
				}
			}
		}

		public virtual object ConvertValue (string value, Type type) 
		{
			return Convert.ChangeType (value, type);
		}

		void CompareData (int numTable, string [,] setOfChanges, int [] columnNos) 
		{
			int row = 0;
			string errorMsg = "";
			string tableName = null;
			try {
				tableName = ConfigClass.GetElement (configDoc, "tables", "table"+numTable, "name");
			} catch (Exception e) {
				Console.WriteLine ("ERROR : " + e.Message );
				Console.WriteLine ("STACKTRACE : " + e.StackTrace );
				return;
			}
		
			foreach (DataTable dbTable in dataset.Tables) {
				if (!tableName.Equals (dbTable.TableName))
					continue;
			row = 0;
			foreach (DataRow datarow in dbTable.Rows) {
				row ++;
				string columnValue = null;
				int column = 0;
				foreach (DataColumn datacolumn in dbTable.Columns) {
					column ++;
					errorMsg = "";
					int columnNo = column;
					if (columnNos != null) {
						columnNo = columnNos [column - 1];
}
						if ((setOfChanges != null ) && (setOfChanges [row - 1, columnNo - 1] !=null)) {
							columnValue = setOfChanges [row - 1, columnNo - 1];
						} else {
							try {
								columnValue = ConfigClass.GetElement (configDoc, "values", 
									"table" + numTable, "row" + row, "column" + columnNo);
							} catch (Exception e) {
								Console.WriteLine ("ERROR : " + e.Message);
								Console.WriteLine ("STACKTRACE : " + e.StackTrace);
							} 
						}
						
						object obj = null;
						Console.Write ("Table: {0} : ROW: {1}  COL: {2}", tableName, row , columnNo);
						try {
							obj = datarow [datacolumn];
						} catch (Exception e) {
						
							Console.WriteLine ("...FAIL");
							errorMsg = "ERROR : " + e.Message;
							errorMsg += "\nSTACKTRACE : " + e.StackTrace;
							errorMsg += "\nProbably the 'DataType' property returned a wrong type!!";
							Console.WriteLine (errorMsg);
							obj = null;
							continue;
						}

						if (AreEqual (obj, columnValue, ref errorMsg)) {
							Console.WriteLine ("...OK");
						} else {
							Console.WriteLine ("...FAIL");

							if (!errorMsg.Equals ("")) {
								// There was some exception
								Console.WriteLine (errorMsg);
							} else {
								// Comparison failed
								Console.WriteLine ("Expected : {0} Got: {1}", columnValue, obj);
							}
						}
					}
					Console.WriteLine ("======================");
				}
			}
		}

		public virtual object GetValue (IDataReader rdr, int columnIndex) 
		{
		
			object value = null;
			
			if (rdr.IsDBNull (columnIndex))
				return null;
		
			Type type = rdr.GetFieldType (columnIndex);
			
			switch (type.Name.ToLower ()) {
		
			case "byte"    : value = rdr.GetByte (columnIndex);
					break;
			case "sbyte"   : value = rdr.GetInt16 (columnIndex);
					break;
			case "boolean" : value = rdr.GetBoolean (columnIndex);
					break;
			case "int16"   : value = rdr.GetInt16 (columnIndex);
					break;
			case "uint16"  : 
			case "int32"   : value = rdr.GetInt32 (columnIndex);
					break;
			case "uint32"  : 
			case "int64"   : value = rdr.GetInt64 (columnIndex);
					break;
			case "single"  : value = rdr.GetFloat (columnIndex);
					break;
			case "double"  : value = rdr.GetDouble (columnIndex);
					break;
			case "uint64"  : 
			case "decimal" : value = rdr.GetDecimal (columnIndex);
					break;
			case "datetime": value = rdr.GetDateTime (columnIndex);
					break;
			case "string": value = rdr.GetString (columnIndex);
					break;
			default :      value = rdr.GetValue (columnIndex);
					break;
			}
		
			return value;
		
		}

		public virtual object ConvertToType (Type type, string value, ref string errorMsg) 
		{
			if (value.Equals ("null"))
				return DBNull.Value;
			
			switch (Type.GetTypeCode (type)) {
			case TypeCode.Int16 :
				return ConvertToInt16 (type, value, ref errorMsg);
			case TypeCode.Int32 :
				return  ConvertToInt32 (type, value, ref errorMsg);
			case TypeCode.Int64 :
				return ConvertToInt64 (type, value, ref errorMsg);
			case TypeCode.String :
				return value;
			case TypeCode.Boolean :
				return ConvertToBoolean (type, value, ref errorMsg);
			case TypeCode.Byte :
				return ConvertToByte (type, value, ref errorMsg);
			case TypeCode.DateTime :
				return ConvertToDateTime (type, value, ref errorMsg);
			case TypeCode.Decimal :
				return ConvertToDecimal (type, value, ref errorMsg);
			case TypeCode.Double :
				return ConvertToDouble (type, value, ref errorMsg);
			case TypeCode.Single :
				return ConvertToSingle (type, value, ref errorMsg);
			}
			
			if (type.ToString () == "System.TimeSpan")
				return ConvertToTimeSpan (type, value, ref errorMsg);
			
			return ConvertValue (type, value, ref errorMsg);
		}

		public virtual Boolean AreEqual (object obj, string value, ref string errorMsg) 
		{
		
			if (obj.Equals (DBNull.Value)  || (value.Equals ("null"))) {
				if (obj.Equals (DBNull.Value) && value.Equals ("null"))
					return true;
				return false;
			}
			Type objType = obj.GetType (); 
			value = value.Trim ('\'');
			value = value.Trim ('\"');
			object valObj = ConvertToType (objType, value, ref errorMsg);
			return valObj.Equals (obj);
		}
		
		public virtual object ConvertValue (Type type, string value, ref string errorMsg) 
		{
		
			object valObj = null;
		
			try {
				valObj = Convert.ChangeType (value, type);
			} catch (InvalidCastException e) {
				errorMsg = "Cant convert values!! \n";
				errorMsg += "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace; 
				return false;
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return false;
			}
			
			return valObj;
		
		}

		public virtual object ConvertToInt16 (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}
		
		public virtual object ConvertToInt32 (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}
		
		public virtual object ConvertToInt64 (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}
		
		public virtual object ConvertToBoolean (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}
		
		public virtual object ConvertToByte (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}

		public virtual object ConvertToDateTime (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}
		
		public virtual object ConvertToDecimal (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}
				
		public virtual object ConvertToDouble (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}
		
		public virtual object ConvertToSingle (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}
		
		public virtual object ConvertToTimeSpan (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}
		
		public virtual void GetConnection () 
		{
		}
	}
}
