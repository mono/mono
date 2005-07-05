//
// MsSqlSpecificTest.cs :- A class derived from 'BaseRetrieve' class
//                         - Contains code specific to ms sql database
//                           (Retrieves data from the database as sql-specific types)
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
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;


namespace MonoTests.System.Data {

	public class SqlRetrieve : BaseRetrieve {
	
		public SqlRetrieve (string dbConfigFile) : base (dbConfigFile) 
		{
		}
		
		// returns a Open connection 
		public override void GetConnection () 
		{
			string connectionString = null;
			try {
				connectionString = ConfigClass.GetElement (configDoc, "database", "connectionString");
			} catch (Exception e) {
				Console.WriteLine ("Error reading the config file");
				Console.WriteLine (e.Message);
				con = null;
				return;
			}

			con = new SqlConnection (connectionString);
			try {
				con.Open ();
			} catch (SqlException e) {
				Console.WriteLine ("Cannot establish connection with the database");
				Console.WriteLine ("Probably the database is down");
				con = null;
			} catch (InvalidOperationException e) {
				Console.WriteLine ("Cannot open connection!! Probably the connection is already open!!");
				con = null;
                        } catch (Exception e) {
                                Console.WriteLine ("Cannot open connection ");
                                con = null;
			}
		}
		
		public override object GetValue (IDataReader reader, int columnIndex) 
		{
		
			object value = null;
			SqlDataReader rdr = (SqlDataReader) reader;
			if (rdr.IsDBNull (columnIndex))
				return null;
			
			if (rdr.GetDataTypeName (columnIndex) == "money") {
				value = rdr.GetSqlMoney (columnIndex);
				return value;
			}
			
			Type type = rdr.GetFieldType (columnIndex);

			switch (type.Name.ToLower ()) {

			case "byte"    : value = rdr.GetSqlByte (columnIndex);
					break;
			case "sbyte"   : value = rdr.GetSqlInt16 (columnIndex);
					break;
			case "boolean" : value = rdr.GetSqlBoolean (columnIndex);
					break;
			case "int16"   : value = rdr.GetSqlInt16 (columnIndex);
					break;
			case "uint16"  :
			case "int32"   : value = rdr.GetSqlInt32 (columnIndex);
					break;
			case "uint32"  :
			case "int64"   : value = rdr.GetSqlInt64 (columnIndex);
					break;
			case "single"  : value = rdr.GetSqlSingle (columnIndex);
					break;
			case "double"  : value = rdr.GetSqlDouble (columnIndex);
					break;
			case "uint64"  :
			case "decimal" : value = rdr.GetSqlDecimal (columnIndex);
					break;
			case "datetime": value = rdr.GetSqlDateTime (columnIndex);
					break;
			case "string": value = rdr.GetSqlString (columnIndex);
					break;
			default :      value = rdr.GetValue (columnIndex);
					break;
			}

			return value;
		}

		public override object ConvertToByte (Type type, string value, ref string errorMsg) 
		{
			byte byteval;

			try {
				byteval = Convert.ToByte (value);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}

			return new SqlByte (byteval);
		}

		public override object ConvertToBoolean (Type type, string value, ref string errorMsg) 
		{
			bool boolval;
			try {
				boolval = Convert.ToBoolean (Convert.ToInt16 (value));
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}

			return new SqlBoolean (boolval);
		}
		
		public override object ConvertToInt16 (Type type, string value, ref string errorMsg) 
		{
			short shortval;
			try {
				shortval = Convert.ToInt16 (value);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}

			return new SqlInt16 (shortval);
		}
		
		public override object ConvertToInt32 (Type type, string value, ref string errorMsg) 
		{
			int intval;
			try {
				intval = Convert.ToInt32 (value);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}

			return new SqlInt32 (intval);
		}
		
		public override object ConvertToInt64 (Type type, string value, ref string errorMsg) 
		{
			long longval;
			try {
				longval = Convert.ToInt64 (value);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}

			return new SqlInt64 (longval);
		}
		
		public override object ConvertToSingle (Type type, string value, ref string errorMsg) 
		{
			float floatval;
			try {
				floatval = Convert.ToSingle (value);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}

			return new SqlSingle (floatval);
		}
		
		public override object ConvertToDouble (Type type, string value, ref string errorMsg) 
		{
			double doubleval;
			try {
				doubleval = Convert.ToDouble (value);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}

			return new SqlDouble (doubleval);
		}
		
		public object ConvertToMoney (Type type, string value, ref string errorMsg) 
		{
			decimal decimalval;
			try {
				decimalval = Convert.ToDecimal (value);
			} catch (FormatException e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}
			
			return new SqlMoney (decimalval);
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
			}
			
			return new SqlDecimal (decimalval);
		}

		public override object ConvertToDateTime (Type type, string value, ref string errorMsg) 
		{
		
			Regex re = new Regex ("\\b(?<month>\\d{1,2})/(?<day>\\d{1,2})/(?<year>\\d{2,4})\\s+(?<hour>\\d{1,2}):(?<min>\\d{1,2})(:(?<sec>\\d{1,2})(\\.(?<msec>\\d{1,3}))*)*");
			
			value = value.Trim ('\'');                                                                          
			Match m = re.Match (value);
			
			int month, day, year, hour, min, sec, msec;
			month = day = year = hour = min = sec = msec = 0;
			month  = Convert.ToInt32 (m.Result ("${month}"));
			day = Convert.ToInt32 (m.Result ("${day}"));
			year = Convert.ToInt32 (m.Result ("${year}"));
			string str = m.Result ("${hour}");
			if (!str.Equals (""))
				hour = Convert.ToInt32 (str);
			str = m.Result ("${min}");
			if (!str.Equals (""))
				min = Convert.ToInt32 (str);
			str = m.Result ("${sec}");
			if (!str.Equals (""))
				sec = Convert.ToInt32 (str);
			str = m.Result ("${msec}");
			if (!str.Equals (""))
				msec = Convert.ToInt32 (str);
			SqlDateTime dateObj;
			try {
				if (hour == 0 && min == 0 && sec == 0)
					dateObj = new SqlDateTime (year, month, day);
				else {
					if (msec != 0) {
						dateObj = new SqlDateTime (year, month, day, hour, min, sec, msec);
			  		} else {
						dateObj = new SqlDateTime (year, month, day, hour, min, sec);
			  		}
				}
			} catch (Exception e) {
				errorMsg = "Invalid date time\n";
				errorMsg += "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}
			
			return dateObj;
		}
		
		public override Boolean AreEqual (object obj, string value, ref string errorMsg) 
		{
		
			if ((obj == null ) || (value.Equals ("null"))) {
				if (obj == null && value.Equals ("null"))
					return true;
				return false;
			}

			object valObj = ConvertToValueType (obj.GetType (), value, ref errorMsg);
			return obj.Equals (valObj);
		}
		
		public override object ConvertToValueType (Type objType, string value, ref string errorMsg) 
		{
		
			value = value.Trim ('\'');
			value = value.Trim ('\"');
		
			switch (objType.ToString ()) {
			case "System.Data.SqlTypes.SqlInt16" :
				return ConvertToInt16 (objType, value, ref errorMsg);
			case "System.Data.SqlTypes.SqlInt32" :
				return ConvertToInt32 (objType, value, ref errorMsg);
			case "System.Data.SqlTypes.SqlInt64" :
				return ConvertToInt64 (objType, value, ref errorMsg);
			case "System.Data.SqlTypes.SqlString" :
				return new SqlString (value);
			case "System.Data.SqlTypes.SqlBoolean" :
				return ConvertToBoolean (objType, value, ref errorMsg);
			case "System.Data.SqlTypes.SqlByte" :
				return ConvertToByte (objType, value, ref errorMsg);
			case "System.Data.SqlTypes.SqlDateTime" :
				return ConvertToDateTime (objType, value, ref errorMsg);
			case "System.Data.SqlTypes.SqlDecimal" :
				return ConvertToDecimal (objType, value, ref errorMsg);
			case "System.Data.SqlTypes.SqlDouble" :
				return ConvertToDouble (objType, value, ref errorMsg);
			case "System.Data.SqlTypes.SqlSingle" :
				return ConvertToSingle (objType, value, ref errorMsg);
			case "System.Data.SqlTypes.SqlMoney" :
				return ConvertToMoney (objType, value, ref errorMsg);
			}
		
			if (objType.ToString () == "System.TimeSpan")
				return ConvertToTimespan (objType, value, ref errorMsg);
		
			return ConvertValue (objType, value, ref errorMsg);
		}
		
	}
}
