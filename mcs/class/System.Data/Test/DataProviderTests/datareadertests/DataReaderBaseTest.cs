//
// DataProviderBaseTest.cs : A base class that provides the common 
//                           functionality of :
//                             1) Reading a config file containing the 
//                                database connection parameters, different 
//                                tables and their description, Values that 
//                                the tables are populated with.
//                             2) Retrieves data from these tables;
//                             3) Compares the retrieved values against the ones
//                                contained in the config file.
//
// A class specific to each database (and ODBC) are derived from this class.
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
using System.Configuration;
using System.Text.RegularExpressions;

namespace MonoTests.System.Data {

	public class BaseRetrieve {
	
		public IDbConnection con;
		public IDbCommand    cmd;
		public IDataReader   rdr;
		protected XmlNode    configDoc;
		
		public BaseRetrieve (string database) 
		{
			con = null;
			cmd = null;
			rdr = null;
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
			
			string noOfTables = null;
			string tableName = null;
			int [] columnNos = null;
			
			try {
				noOfTables = ConfigClass.GetElement (configDoc, "tables", "numTables");
				short numTables = Convert.ToInt16 (noOfTables);
				string noOfQueries = ConfigClass.GetElement (configDoc, "queries", "numQueries"); 
				Console.WriteLine ("**** Running Queries ****");

				if (noOfQueries != null) {

	  				int numQueries = Convert.ToInt32 (noOfQueries);

	  				for (int index = 1; index <= numQueries; index++) {
	    					string queryStr = ConfigClass.GetElement (configDoc, "queries", "query" + index);
	    					int tableNum = 0;
	    					rdr = RunQuery (queryStr, ref columnNos, ref tableNum);
						if (rdr == null) 
							continue;

						CompareData (rdr, configDoc, columnNos, tableNum);
						rdr.Close ();
	  				}
				}
				
				string storedProc = null;
				try {
					storedProc = ConfigClass.GetElement (configDoc, "StoredProcExists");
				} catch (Exception e) {
					return;
				}

				if (storedProc.Equals ("Y")) {

					Console.WriteLine ("\n**** Running tests for stored procedures *****\n");
	  				int numStoredProc = Convert.ToInt32 (ConfigClass.GetElement(configDoc,
								 "StoredProc", "NumStoredProc"));
	  				for (int index = 1; index <= numStoredProc; index++) {

	     					string storedProcTag = "StoredProc" + index;
	     					string type = ConfigClass.GetElement (configDoc, "StoredProc",
								 storedProcTag, "type");
	     					string nameTemplate = ConfigClass.GetElement (configDoc, "StoredProc", 
									storedProcTag, "name");
	     					if (type.Equals("generic")) {

		       					// There is stored proc correspoding to each table
						       // Run all such stored proc
							for (short i = 1; i <= numTables; i++) {

								try {
									tableName = ConfigClass.GetElement (configDoc, "tables", 
										"table"+i, "name");
								} catch (XPathException e) {
									Console.WriteLine (e.Message);
									continue; // need not return here; try with the next one
								} 
								
		 						string storedProcName = nameTemplate.Replace ("{{TABLE}}", tableName);
								rdr = QueryUsingStoredProc (cmd, storedProcName, null);
								if (rdr == null)
									continue;

								CompareData (rdr, configDoc, null, i);
								rdr.Close ();
							} 
	     					}
	  				}
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
		
		IDataReader RunQuery (string queryStr, ref int [] columnNos, ref int tableNum) 
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

				for (int i = 0; i<m.Groups.Count; i++) {

					Group g = m.Groups [i];
					CaptureCollection cc = g.Captures;

					for (int j = 0; j < cc.Count; j++) {

						string matchedVal = cc [j].Value;

	    					if (matchedVal.Equals ("COLUMNS")) {
	      						string [] columnNames = ConfigClass.GetColumnNames (configDoc, tableNum);
	      						queryStr = queryStr.Replace ("COLUMNS", String.Join (",", columnNames));
							columnNos = new int [columnNames.Length];
	      						for (int index = 1; index <= columnNos.Length; index++)
	       		 					columnNos [index - 1] = index;

	    					} else if (matchedVal.StartsWith ("COLUMN")) {
	      						// May be a column name or a comma 
	      						// separated list of columns
	      						string [] listOfColumns = matchedVal.Split (',');
	      						if (columnNos == null) {

	        						columnNos = new int [listOfColumns.Length];
								int colIndex = 0;
	        						foreach (string str in listOfColumns) {
	          							int columnNo = Convert.ToInt32 (str.Replace("COLUMN", ""));
									columnNos [colIndex ++] = columnNo;
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
	
			IDataReader rdr = null;
			cmd.CommandText = queryStr;
			try {
				rdr = cmd.ExecuteReader ();
			} catch (Exception e) {
				Console.WriteLine ("ERROR : " + e.Message);
				Console.WriteLine ("\nSTACKTRACE : " + e.StackTrace);
				return null;
			}

			return rdr;
			
		}

		void CompareData (IDataReader rdr, 
				  XmlNode doc,
		     		  int [] columnNos,
				  int numTable) 
		{
			int rowNum = 0;
			string errorMsg = "";
			string tableName = null;
			try {
				tableName = ConfigClass.GetElement (doc, "tables", "table"+numTable, "name");
			} catch (Exception e) {
				Console.WriteLine ("ERROR : " + e.Message );
				Console.WriteLine ("STACKTRACE : " + e.StackTrace );
				return;
			}

			while (rdr.Read()) {
				rowNum ++;
				string columnValue = null;
				for (int i = 0; i < rdr.FieldCount; i++) {
					errorMsg = "";
	  				int columnNum = 0;
					try {
	    					if (columnNos == null) 
	      						columnNum = i+1;
						else 
	      						columnNum = columnNos [i];
							
						columnValue = ConfigClass.GetElement (doc, "values", "table" + numTable,
								 "row" + rowNum, "column" + columnNum);
					} catch (Exception e) {
						Console.WriteLine ("ERROR : " + e.Message);
						Console.WriteLine ("STACKTRACE : " + e.StackTrace);
					} 
					
					object obj = null;
					Console.Write ("Table : {0} : ROW: {1} COL: {2}", tableName, rowNum, columnNum);
					try {
						obj = GetValue (rdr, i);
					} catch (Exception e) {

						Console.WriteLine ("...FAIL");
						errorMsg = "ERROR : " + e.Message;
						errorMsg += "\nSTACKTRACE : " + e.StackTrace;
						errorMsg += "\nProbably the 'GetFieldType()' method returned a wrong type!!";
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
							Console.WriteLine ("Expected : "+columnValue+" Got : "+obj);
						}
					}
				}
				Console.WriteLine ("======================");
			}
		}

		public virtual object GetValue (IDataReader rdr, int columnIndex) 
		{
		
			object value = null;
			
			if (rdr.IsDBNull (columnIndex)) {
				return null;
			}
			
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

		public virtual Boolean AreEqual (object obj, string value, ref string errorMsg) 
		{
		
			if ((obj == null) || (value.Equals("null"))) {
				if (obj == null && value.Equals ("null"))
					return true;
				return false;
			}
			
			object valObj = ConvertToValueType (obj.GetType (), value, ref errorMsg);
			if (valObj == null) {
				errorMsg = "Could not convert values!!\n" + errorMsg;
				return false;
			}
			return valObj.Equals (obj);
		}

		public virtual object ConvertToValueType (Type objType, string value, ref string errorMsg) 
		{
		
			value = value.Trim ('\'');
			value = value.Trim ('\"');

			switch (Type.GetTypeCode (objType)) {

			case TypeCode.Int16 :
				return ConvertToInt16 (objType, value, ref errorMsg);
			case TypeCode.Int32 :
				return ConvertToInt32 (objType, value, ref errorMsg);
			case TypeCode.Int64 :
				return ConvertToInt64 (objType, value, ref errorMsg);
			case TypeCode.Boolean :
				return ConvertToBoolean (objType, value, ref errorMsg);
			case TypeCode.Byte :
				return ConvertToByte (objType, value, ref errorMsg);
			case TypeCode.DateTime :
				return ConvertToDateTime (objType, value, ref errorMsg);
			case TypeCode.Decimal :
				return ConvertToDecimal (objType, value, ref errorMsg);
			case TypeCode.Double :
				return ConvertToDouble (objType, value, ref errorMsg);
			case TypeCode.Single :
				return ConvertToSingle (objType, value, ref errorMsg);

			}

			if ( objType.ToString () == "System.TimeSpan")
				return ConvertToTimespan (objType, value, ref errorMsg);
			
			return ConvertValue (objType, value, ref errorMsg);
		}

		public virtual object ConvertValue (Type type, string value, ref string errorMsg) 
		{
			object valObj = null;
			
			try {
				valObj = Convert.ChangeType (value, type);
			} catch (InvalidCastException e) {
				errorMsg = "Cant compare values!! \n";
				errorMsg += "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace; 
				return false;
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
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
		
		public virtual object ConvertToTimespan (Type type, string value, ref string errorMsg) 
		{
			return ConvertValue (type, value, ref errorMsg);
		}
		
		public virtual void GetConnection() 
		{
		}
	}
}
