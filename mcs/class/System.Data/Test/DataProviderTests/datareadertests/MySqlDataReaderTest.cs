//
// MySqlDataReaderTest.cs :- A class derived from the 'BaseRetrieve' class
//                         - Contains code specific to mysql database 
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
using ByteFX.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace MonoTests.System.Data {

	public class MySqlRetrieve : BaseRetrieve {
	
		public MySqlRetrieve (string database) : base (database) 
		{
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
			
			con = new MySqlConnection (connectionString);
			try {
				con.Open ();
			} catch (MySqlException e) {
				Console.WriteLine ("Cannot establish connection with the database");
				Console.WriteLine ("Probably the Database is down ");
				con = null;
			} catch (InvalidOperationException e) {
				Console.WriteLine ("Cannot open connection ");
				Console.WriteLine ("Probably the connection is already open");
				con = null;
                        } catch (Exception e) {
                                Console.WriteLine ("Cannot open connection ");
                                con = null;
			}
		}
		
		public override object ConvertToDateTime (Type type, string value, ref string errorMsg) 
		{
		
			string dateStr = value.Trim ('\'');
			dateStr = dateStr.Trim ('\"');
			int year, month, day, hour, min, sec;
			year = month = day = hour = min = sec = 0;
			char [] splChars = {'!','@','#','$','%','^','&','*','-','+','.',','};
			string [] dateParts = dateStr.Split (' ');

			for  (int index = 0; index < splChars.Length; index++) {
				dateParts [0] = dateParts [0].Replace (splChars [index], '/');
				if (dateParts.Length > 1) 
					dateParts [1] = dateParts [1].Replace (splChars [index], ':');
			}

			dateStr = String.Join (" ", dateParts);
			Regex re = new Regex ("\\b(?<year>\\d{2,4})/(?<month>\\d{1,2})/(?<day>\\d{1,2})(\\s+(?<hour>\\d{1,2}):(?<min>\\d{1,2}):(?<sec>\\d{1,2}))*");
			
			Match m = re.Match (dateStr);
			if (!m.Success) {
				re = new Regex("\\b(?<year>\\d{4})(?<month>\\d{2})(?<day>\\d{2})((?<hour>\\d{2})(?<min>\\d{2})(?<sec>\\d{2}))*");
				m = re.Match (dateStr);
			}
			
			string matchedVal = m.Result ("${year}");
			if (!matchedVal.Equals (""))
				year = Convert.ToInt32 (matchedVal);
			matchedVal = m.Result ("${month}");
			if (!matchedVal.Equals (""))
				month = Convert.ToInt32 (matchedVal);
			matchedVal = m.Result ("${day}");
			if (!matchedVal.Equals (""))
				day = Convert.ToInt32 (matchedVal);
			matchedVal = m.Result ("${hour}");
			if (!matchedVal.Equals (""))
				hour = Convert.ToInt32 (matchedVal);
			matchedVal = m.Result ("${min}");
			if (!matchedVal.Equals (""))
				min = Convert.ToInt32 (matchedVal);
			matchedVal = m.Result ("${sec}");
			if (!matchedVal.Equals (""))
				sec = Convert.ToInt32 (matchedVal);
			
			DateTime dateTime;
			try {
				dateTime = new DateTime (year, month, day, hour, min, sec);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}
			
			return dateTime;
		}
		
		public override object ConvertToTimespan (Type type, string value, ref string errorMsg) 
		{
		
			// Format the input string as 'hh:mm:yy'
			string dateStr = value.Trim ('\'');
			
			if (dateStr.IndexOf (':') == -1) {
				// String in the form 'hhmmss', insert ':'s in between
				dateStr = dateStr.Substring (0,2) + ":" +
				dateStr.Substring (2,2)+ ":" +
				dateStr.Substring (4,2);
			}
		
			TimeSpan timespan;
			try {
				timespan = TimeSpan.Parse (dateStr);
			} catch (Exception e) {
				errorMsg = "ERROR : " + e.Message;
				errorMsg += "\nSTACKTRACE : " + e.StackTrace;
				return null;
			}
			
			return timespan;
		}
	}
}
