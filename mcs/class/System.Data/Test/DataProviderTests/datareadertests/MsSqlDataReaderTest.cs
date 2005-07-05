//
// MsSqlDataReaderTest.cs :- A class derived from the 'BaseRetrieve' class
//                         - Contains code specific to ms sql database
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
using System.Data.SqlClient;
using System.Text.RegularExpressions;


namespace MonoTests.System.Data {

	public class MsSqlRetrieve : BaseRetrieve {
	
		public MsSqlRetrieve (string database) : base (database) 
		{
		}
	
		// returns a Open connection 
		public override void GetConnection () 
		{
			string connectionString = null; 
			try {
				connectionString = ConfigClass.GetElement (configDoc, "database", "connectionString");
			} catch (XPathException e) {
				Console.WriteLine ("Error reading the config file!!!");
				Console.WriteLine (e.Message);
				con = null;
				return;
			}

			con = new SqlConnection (connectionString);

			try {
				con.Open ();
			} catch (SqlException e) {
				Console.WriteLine ("Cannot establish connection with the database " + e);
				Console.WriteLine ("Probably the database is down");
				con = null;
			} catch (InvalidOperationException e) {
				Console.WriteLine ("Cannot open connection ");
				Console.WriteLine ("Probably the connection already open");
				con = null;
			} catch (Exception e) {
				Console.WriteLine ("Cannot open connection ");
				con = null;
			}
		}
		
		public override object ConvertToBoolean (Type type, string value, ref string errorMsg) 
		{
			short boolValue;
			Boolean boolval;
			try {
				boolValue = Convert.ToInt16 (value);
				boolval = Convert.ToBoolean (boolValue);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return false;
			}

			return boolval;
		}
	
		public override object ConvertToDecimal (Type type, string value, ref string errorMsg) 
		{
			Decimal decVal ;
			try {
				decVal = Convert.ToDecimal (value);
			} catch (FormatException e) {
			// This may be bcoz value is of the form 'd.ddEdd'
				Double doubleVal = Convert.ToDouble (value);
				decVal = Convert.ToDecimal (doubleVal);
			}

			return decVal;
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
			DateTime dateObj;

			try {
				if (hour == 0 && min == 0 && sec == 0)
					dateObj = new DateTime (year, month, day);
				else {
					if (msec != 0) 
						dateObj = new DateTime (year, month, day, hour, min, sec, msec);
	  				else
						dateObj = new DateTime (year, month, day, hour, min, sec);
				}
			} catch (Exception e) {
				errorMsg = "Invalid DateTime Value\n";
				errorMsg += "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}

			return dateObj;
		}
	}
}
