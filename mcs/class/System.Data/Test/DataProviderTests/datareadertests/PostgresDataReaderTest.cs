//
// PostgresDataReaderTest.cs :- Defines a class 'PostgresRetrieve' derived from the
//                             'BaseRetrieve' class
//                               - Contains code specific to postgres database
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
using Npgsql;
using System.Text.RegularExpressions;


namespace MonoTests.System.Data {
	
	public class PostgresRetrieve : BaseRetrieve {
		
		public PostgresRetrieve (string database) : base (database) 
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

			con = new NpgsqlConnection (connectionString);

			try {
				con.Open ();
			} catch (NpgsqlException e) {
				Console.WriteLine ("Cannot establish connection with the database");
				Console.WriteLine ("Probably the Database is down!!");
				con = null;
			} catch (InvalidOperationException e) {
				Console.WriteLine ("Cannot establish connection with the database");
				Console.WriteLine ("Probably connection is already open");
				con = null;
			} catch (Exception e) {
				Console.WriteLine ("Cannot establish connection with the database");
				con = null;
			}
		}
		
		public override object ConvertToDateTime (Type type, string value, ref string errorMsg) 
		{
		
			int month, day, year, hour, min, sec;
			month = day = year = hour = min = sec = 0;
			double msec = 0;
			msec = 0;
			
			Regex re = new Regex ("\\b(?<year>\\d{2,4})\\-(?<month>\\d{1,2})\\-(?<day>\\d{1,2})(\\s+(?<hour>\\d{1,2}):(?<minute>\\d{1,2}):(?<sec>\\d{1,2})(\\.(?<msec>\\d{1,6}))*)*");
			
			value = value.Trim ('\'');
			value = value.Trim ('\"');
			Match m = re.Match (value);
			string matchedVal = null;

			if (m.Success) {
				matchedVal = m.Result ("${year}");
				if (!matchedVal.Equals (""))
					year = Convert.ToInt32 (matchedVal);
		
				matchedVal = m.Result ("${month}");
				if (!matchedVal.Equals ("")) 
					month = Convert.ToInt32 (matchedVal);
				
				matchedVal = m.Result ("${day}");
				if (!matchedVal.Equals (""))
					day = Convert.ToInt32 (matchedVal);
			} else {
				// Only timespan
				re = new Regex("\\b(?<hour>\\d{1,2}):(?<minute>\\d{1,2}):(?<sec>\\d{1,2})(\\.(?<msec>\\d{1,6}))*");
				m = re.Match (value);
			}
			
			try {
			
				matchedVal = m.Result ("${hour}");
				if (!matchedVal.Equals ("")) 
					hour = Convert.ToInt32 (matchedVal);
				
				matchedVal = m.Result ("${minute}");
				if (!matchedVal.Equals (""))
				min = Convert.ToInt32 (matchedVal);
				
				matchedVal = m.Result ("${sec}");
				if (!matchedVal.Equals (""))
				sec = Convert.ToInt32 (matchedVal);
				
				matchedVal = m.Result ("${msec}");
				if (!matchedVal.Equals (""))
					msec = Convert.ToDouble (matchedVal);
				if (msec > 1000) 
			    		msec = msec / 1000;

				if (day == 0 && month == 0 && year == 0) {
					// Time span only
					TimeSpan ts =TimeSpan.Parse(value);
					return ts;

				} else {
					
					DateTime dateObj = new DateTime (year, month, day, hour, min, sec);
					dateObj = dateObj.AddMilliseconds (msec);
					return dateObj;
				}
			
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return  null;
			}
		}
	}
}
