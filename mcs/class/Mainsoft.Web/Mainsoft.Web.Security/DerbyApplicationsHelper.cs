//
// Mainsoft.Web.Security.DerbyApplicationsHelper
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft
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

#if NET_2_0

using System;
using System.Web.Security;
using System.Data;
using System.Data.OleDb;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;

namespace Mainsoft.Web.Security
{
	static class DerbyApplicationsHelper
	{
		private static OleDbParameter AddParameter (OleDbCommand command, string paramName, object paramValue)
		{
			OleDbParameter prm = new OleDbParameter (paramName, paramValue);
			command.Parameters.Add (prm);
			return prm;
		}

		public static object Applications_CreateApplication (DbConnection connection, string applicationName)
		{
			string selectQuery = "SELECT ApplicationId FROM aspnet_Applications WHERE LoweredApplicationName = ?";
			string insertQuery = "INSERT INTO aspnet_Applications (ApplicationId, ApplicationName, LoweredApplicationName) VALUES  (?, ?, ?)";

			OleDbCommand selectCmd = new OleDbCommand (selectQuery, (OleDbConnection) connection);
			AddParameter (selectCmd, "LoweredApplicationName", applicationName.ToLowerInvariant ());

			using (OleDbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetString (0);
			}

			string applicationId = Guid.NewGuid ().ToString ();
			OleDbCommand insertCmd = new OleDbCommand (insertQuery, (OleDbConnection) connection);
			AddParameter (insertCmd, "ApplicationId", applicationId);
			AddParameter (insertCmd, "ApplicationName", applicationName);
			AddParameter (insertCmd, "LoweredApplicationName", applicationName.ToLowerInvariant ());
			insertCmd.ExecuteNonQuery ();

			return applicationId;
		}

		public static string GetApplicationId (DbConnection connection, string applicationName)
		{
			string selectQuery = "SELECT ApplicationId FROM aspnet_Applications WHERE LoweredApplicationName = ?";

			OleDbCommand selectCmd = new OleDbCommand (selectQuery, (OleDbConnection) connection);
			AddParameter (selectCmd, "LoweredApplicationName", applicationName.ToLowerInvariant ());
			using (OleDbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetString (0);
			}

			return null;
		}
	}
}

#endif