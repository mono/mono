//
// OracleDbAdapterTest.cs :- Defines a class 'OraAdapter' derived from the
//                           'BaseAdapter' class.
//                             - Contains code specific to oracle database
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
using System.Xml.XPath;
using System.Data;
using System.Data.OracleClient;
using System.Text.RegularExpressions;

namespace MonoTests.System.Data {
	
	public class OraAdapter : BaseAdapter {
		
		public OraAdapter (string database) : base (database) 
		{
			dataAdapter = new OracleDataAdapter ();
		}
		
		// returns a Open connection 
		public override void GetConnection () 
		{
			string connectionString = null;
			try {
				connectionString = ConfigClass.GetElement (configDoc, "database", "connectionString");
			} catch (XPathException e) {
				Console.WriteLine ("Error reading the config file !!");
				Console.WriteLine (e.Message);
				con = null;
				return;
			}
			
			con = new OracleConnection (connectionString);
			
			try {
				con.Open ();
			} catch (OracleException e) {
				Console.WriteLine ("Cannot establish connection with the database");
				Console.WriteLine ("Probably the database is down");
				con = null;
			} catch (InvalidOperationException e) {
				Console.WriteLine ("Cannot open connection");
				Console.WriteLine("Probably the connection is already open");
				con = null;
                        } catch (Exception e) {
                                Console.WriteLine ("Cannot open connection ");
                                con = null;
			}
		}
		
		public override object GetValue (IDataReader reader, int columnIndex) 
		{
		
			object value = null;
			if (reader.IsDBNull (columnIndex)) {
				return null;
			}
		
			OracleDataReader rdr = (OracleDataReader) reader;
			
			Type type = rdr.GetFieldType (columnIndex);
			string datatypeName = rdr.GetDataTypeName (columnIndex);

			if (datatypeName.ToLower ().Equals ("interval year to month")) {
				value = rdr.GetOracleMonthSpan (columnIndex);
				return value;
			}

			switch (type.Name.ToLower ()) {

			case "int32":
					value = rdr.GetInt32 (columnIndex);
					break;
			case "decimal" : 
					try {
						value = rdr.GetDecimal (columnIndex);
					} catch (Exception e) { 
						value = rdr.GetOracleNumber (columnIndex);
					}
					break;
			case "datetime": value = rdr.GetDateTime (columnIndex);
					break;
			case "string": value = rdr.GetString (columnIndex);
					break;
			case "timespan" : value = rdr.GetTimeSpan (columnIndex);
					break;
			default :      value = rdr.GetValue (columnIndex);
					break;
			}

			return value;
		}
			
		public override Boolean AreEqual (object obj, string value, ref string errorMsg) 
		{
			if ((obj == null ) || (value.Equals ("null"))) {
				if (obj == null && value.Equals ("null"))
					return true;
				return false;
			}
	
			if (obj.Equals (DBNull.Value) || (value.Equals ("null"))) {
				if (obj.Equals (DBNull.Value) && value.Equals ("null"))
					return true;
				return false;
			}
			
			Type objType = obj.GetType ();
			object valObj = null;
			
			value = value.Trim ('\'');
			value = value.Trim ('\"');
			
			switch (objType.ToString ()) {

			case "System.Data.OracleClient.OracleNumber" :
				return CompareOracleNumber (obj, value, ref errorMsg);
			case "System.Data.OracleClient.OracleMonthSpan" :
				valObj =  ConvertToOracleMonthSpan (objType, value, ref errorMsg);
				return valObj.Equals (obj);
			case "System.Decimal" :
				valObj =  ConvertToDecimal (objType, value, ref errorMsg);
				return valObj.Equals (obj);
			case "System.String" :
				return value.Equals (obj);
			case "System.TimeSpan" : 
				valObj =  ConvertToTimeSpan (objType, value, ref errorMsg);
				return valObj.Equals (obj);
			case "System.DateTime" :
				valObj =  ConvertToDateTime (objType, value, ref errorMsg);
				return valObj.Equals (obj);
			case "System.Int32" :
				valObj =  ConvertToInt32 (objType, value, ref errorMsg);
				return valObj.Equals (obj);
		
			}
			
			valObj = ConvertValue (objType, value, ref errorMsg);

			if (valObj == null)  {
				Console.WriteLine ("Unable to convert values!!!");
				Console.WriteLine (errorMsg);
				return false;
			}

			return valObj.Equals (obj);
		}
		
		public override object ConvertToDecimal (Type type, string value, ref string errorMsg) 
		{
			decimal decimalval;
			try {
				decimalval = Convert.ToDecimal (value);
			} catch (FormatException e) {
			// This may be bcoz value is of the form 'd.ddEdd'
				Double doubleVal = Convert.ToDouble (value);
				decimalval = Convert.ToDecimal (doubleVal);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}

			return decimalval;
		}
		
		public override object ConvertToInt32 (Type type, string value, ref string errorMsg)  
		{
			int intval;

			try {
				intval = Convert.ToInt32 (value);
			} catch (FormatException e) {
				// This might be interval year to month
				bool isNegative = false;
				if (value.StartsWith ("-"))  {
					isNegative = true;
					value = value.Trim ('-');
				}

				string [] intParts = value.Split ('-');
				intval = Convert.ToInt32 (intParts [0]) * 12 + Convert.ToInt32 (intParts [1]);
				if (isNegative)
					intval *= -1;
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}

			return intval;
			
		}

		public override object ConvertToDateTime (Type type, string value, ref string errorMsg) 
		{
		
			value = value.ToLower ().Trim ();
			string format = null, date = null;

			if (value.StartsWith ("to_date")) {

				value = value.Remove (0, 8);
				value = value.Remove (value.Length - 1, 1);
				string[] dateParts = value.Split(',');
				date = dateParts [0].Trim ();
				format = dateParts [1].Trim ();
				
				date = date.Trim ('\'');
				format = format.Trim ('\'');
				// Assuming that date will be in yyyy-mm-dd hh24:mi:ss only
				format = format.Replace ("yyyy", "(?<yyyy>\\d{4})");
				format = format.Replace ("mm", "(?<mm>\\d{2})");
				format = format.Replace ("dd", "(?<dd>\\d{2})");
				format = format.Replace ("hh24", "(?<hh24>\\d{2})");
				format = format.Replace ("mi", "(?<mi>\\d{2})");
				format = format.Replace ("ss", "(?<ss>\\d{2})");

			} else if (value.ToLower ().StartsWith ("timestamp")) {
				value = value.Remove (0, 9);
				value = value.Trim ();
				date = value.Trim ('\'');
				format = "\\b(?<yyyy>\\d{4})\\-(?<mm>\\d{2})\\-(?<dd>\\d{2}) (?<hh24>\\d{2}):(?<mi>\\d{2}):(?<ss>\\d{2})";
				
			}
		
			Regex re = new Regex (format);
			Match m = re.Match (date);

			if (!m.Success)
				return false;

			int year = Convert.ToInt32 (m.Result ("${yyyy}"));
			int month = Convert.ToInt32 (m.Result ("${mm}"));
			int day = Convert.ToInt32 (m.Result ("${dd}"));
			int hour = Convert.ToInt32 (m.Result ("${hh24}"));
			int min = Convert.ToInt32 (m.Result ("${mi}"));
			int sec = Convert.ToInt32 (m.Result ("${ss}"));
			
			DateTime newDt ;

			try {
				newDt = new DateTime (year, month, day, hour, min, sec);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return false;
			}

			return newDt;
		}
		
		Boolean CompareOracleNumber (object obj, string value, ref string errorMsg) 
		{
			return value.Equals (obj.ToString ());
		}
		
		object ConvertToOracleMonthSpan (Type type, string value, ref string errorMsg) 
		{
			int intervalInMonths = 0;
			bool isNegative = false;

			if (value.StartsWith ("-")) {
				isNegative = true;
				value = value.Trim ('-');
			}

			string [] intParts = value.Split ('-');

			if (intParts.Length > 0) {
				intervalInMonths = Convert.ToInt32 (intParts [0]) * 12;
				if (intParts.Length > 1)
					intervalInMonths += Convert.ToInt32 (intParts [1]);
			} else {// Should not come here
				return false;
			}

			if (isNegative) 
				intervalInMonths *= -1;

			return new OracleMonthSpan (intervalInMonths);
		}
		
		public override object ConvertToTimeSpan (Type type, string value, ref string errorMsg) 
		{
			// Input in the form '[-]dd hh:mi:ss'
			value = value.Replace (" ", ".");
			TimeSpan ts = TimeSpan.Parse (value);
			return ts;
		}
		
		public override void PopulateDataSetFromTable (string queryStr, string tableName) 
		{
			cmd.CommandText = queryStr;
			OracleDataAdapter da = (OracleDataAdapter) dataAdapter;
			da.SelectCommand = (OracleCommand) cmd;
			dataset = new DataSet ();
			da.Fill (dataset, tableName);
		}
		
		public override bool ReconcileChanges (string tableName, ref string errorMsg) 
		{
			try {
				new OracleCommandBuilder ((OracleDataAdapter)dataAdapter);
				dataAdapter.Update (dataset, tableName);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return false;
			}

			return true;
		}
	}
}
