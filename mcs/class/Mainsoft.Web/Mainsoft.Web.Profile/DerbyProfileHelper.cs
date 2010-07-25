//
// Mainsoft.Web.Profile.DerbyProfileHelper
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
using System.Web.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;

using Mainsoft.Web.Security;

namespace Mainsoft.Web.Profile
{
	class DerbyProfileHelper
	{
		private static OleDbParameter AddParameter (OleDbCommand command, string paramName, object paramValue)
		{
			OleDbParameter prm = new OleDbParameter (paramName, paramValue);
			command.Parameters.Add (prm);
			return prm;
		}

		public static int Profile_DeleteInactiveProfiles(DbConnection connection, string applicationName, int profileAuthOptions, DateTime inactiveSinceDate)
		{
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 0;

			string queryDelete = @"DELETE FROM aspnet_Profile WHERE UserId IN ( " +
				"SELECT UserId FROM aspnet_Users WHERE ApplicationId = ? AND LastActivityDate <= ? " + 
				GetProfileAuthOptions (profileAuthOptions) + ")";
			OleDbCommand cmdDelete = new OleDbCommand (queryDelete, (OleDbConnection) connection);
			AddParameter (cmdDelete, "ApplicationId", appId);
			AddParameter (cmdDelete, "LastActivityDate", inactiveSinceDate);

			return cmdDelete.ExecuteNonQuery ();
		}

		public static int Profile_DeleteProfiles(DbConnection connection, string applicationName, string [] userNames)
		{
			int deletedUsers = 0;
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 0;

			OleDbTransaction trans = (OleDbTransaction) connection.BeginTransaction ();
			try {
				foreach (string username in userNames) {
					string userId = GetUserId (connection, trans, appId, username);
					if (userId == null)
						continue;

					string queryDelete = "DELETE FROM aspnet_Profile WHERE UserId = ?";
					OleDbCommand cmdDelete = new OleDbCommand (queryDelete, (OleDbConnection) connection);
					cmdDelete.Transaction = trans;
					AddParameter (cmdDelete, "UserId", userId);
					cmdDelete.Transaction = trans;
					deletedUsers += cmdDelete.ExecuteNonQuery ();
				}
				trans.Commit ();
				return deletedUsers;
			}
			catch (Exception e) {
				trans.Rollback ();
				throw e;
			}
		}

		public static int Profile_GetNumberOfInactiveProfiles (DbConnection connection, string applicationName, int profileAuthOptions, DateTime inactiveSinceDate)
		{
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 0;

			string querySelect = @"SELECT COUNT(*) FROM aspnet_Users usr, aspnet_Profile prf WHERE ApplicationId = ? " +
				"AND usr.UserId = prf.UserId AND LastActivityDate <= ? " + GetProfileAuthOptions(profileAuthOptions);
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "ApplicationId", appId);
			AddParameter (cmdSelect, "LastActivityDate", inactiveSinceDate);

			using (OleDbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetInt32 (0);
			}
			return 0;
		}

		public static int Profile_GetInactiveProfiles (DbConnection connection, string applicationName, int profileAuthOptions, int pageIndex, int pageSize, string userNameToMatch, DateTime inactiveSinceDate, out DbDataReader reader)
		{
			reader = null;
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return -1;

			string querySelect = @"SELECT usr.UserName, usr.IsAnonymous, usr.LastActivityDate, prf.LastUpdatedDate, " +
				"LENGTH(prf.PropertyNames) + LENGTH(prf.PropertyValuesString) + LENGTH(prf.PropertyValuesBinary)" +
				"FROM aspnet_Users usr, aspnet_Profile prf WHERE usr.ApplicationId = ? AND usr.UserId = prf.UserId " +
				"AND usr.LastActivityDate <= ? " + GetProfileAuthOptions (profileAuthOptions) +
				(string.IsNullOrEmpty(userNameToMatch) ? "" : " AND usr.LoweredUserName LIKE ?");
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "ApplicationId", appId);
			AddParameter (cmdSelect, "LastActivityDate", inactiveSinceDate);
			if (!string.IsNullOrEmpty (userNameToMatch))
				AddParameter (cmdSelect, "LoweredUserName", userNameToMatch.ToLowerInvariant());
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		public static int Profile_GetProfiles (DbConnection connection, string applicationName, int profileAuthOptions, int pageIndex, int pageSize, string userNameToMatch, out DbDataReader reader)
		{
			reader = null;
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return -1;

			string querySelect = @"SELECT usr.UserName, usr.IsAnonymous, usr.LastActivityDate, prf.LastUpdatedDate, " +
				"LENGTH(prf.PropertyNames) + LENGTH(prf.PropertyValuesString) + LENGTH(prf.PropertyValuesBinary)" +
				"FROM aspnet_Users usr, aspnet_Profile prf WHERE ApplicationId = ? AND usr.UserId = prf.UserId " +
				GetProfileAuthOptions (profileAuthOptions) +
				(string.IsNullOrEmpty (userNameToMatch) ? "" : " AND usr.LoweredUserName LIKE ?");
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "ApplicationId", appId);
			if (!string.IsNullOrEmpty (userNameToMatch))
				AddParameter (cmdSelect, "LoweredUserName", userNameToMatch.ToLowerInvariant ());
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		public static int Profile_GetProperties (DbConnection connection, string applicationName, string username, DateTime currentTimeUtc, out DbDataReader reader)
		{
			reader = null;
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return -1;

			string userId = GetUserId (connection, null, appId, username);
			if (userId == null)
				return -1;

			string queryUpdUser = @"UPDATE aspnet_Users SET LastActivityDate = ? WHERE UserId = ?";
			OleDbCommand cmdUpdUser = new OleDbCommand (queryUpdUser, (OleDbConnection) connection);
			AddParameter (cmdUpdUser, "LastActivityDate", currentTimeUtc);
			AddParameter (cmdUpdUser, "UserId", userId);
			cmdUpdUser.ExecuteNonQuery ();

			string querySelect = @"SELECT PropertyNames, PropertyValuesString, PropertyValuesBinary FROM aspnet_Profile WHERE UserId = ?";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "UserId", userId);
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		public static int Profile_SetProperties (DbConnection connection, string applicationName, string propertyNames, string propertyValuesString, byte [] propertyValuesBinary, string username, bool isUserAnonymous, DateTime currentTimeUtc)
		{
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null) {
				object newAppId = DerbyApplicationsHelper.Applications_CreateApplication (connection, applicationName);
				appId = newAppId as string;
				if (appId == null)
					return -1;
			}

			OleDbTransaction trans = (OleDbTransaction) connection.BeginTransaction ();
			try {
				string userId = GetUserId (connection, trans, appId, username);
				if (userId == null) {
					object newUserId = null;
					DerbyMembershipHelper.Users_CreateUser (connection, trans, appId, username, true, currentTimeUtc, ref newUserId);
					userId = newUserId as string;
					if (userId == null) {
						trans.Rollback ();
						return -1;
					}
				}

				string queryUpdUser = @"UPDATE aspnet_Users SET LastActivityDate=? WHERE UserId = ?";
				OleDbCommand cmdUpdUser = new OleDbCommand (queryUpdUser, (OleDbConnection) connection);
				cmdUpdUser.Transaction = trans;
				AddParameter (cmdUpdUser, "LastActivityDate", currentTimeUtc);
				AddParameter (cmdUpdUser, "UserId", userId);
				cmdUpdUser.ExecuteNonQuery ();

				string querySelect = @"SELECT * FROM aspnet_Profile WHERE UserId = ?";
				OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
				cmdSelect.Transaction = trans;
				AddParameter (cmdSelect, "UserId", userId);
				bool userHasRecords = false;
				using (OleDbDataReader reader = cmdSelect.ExecuteReader ()) {
					userHasRecords = reader.HasRows;
				}

				if (userHasRecords) {
					string queryUpdate = @"UPDATE aspnet_Profile SET PropertyNames = ?, PropertyValuesString = ?, " +
						"PropertyValuesBinary = ?, LastUpdatedDate = ? WHERE  UserId = ?";
					OleDbCommand cmdUpdate = new OleDbCommand (queryUpdate, (OleDbConnection) connection);
					cmdUpdate.Transaction = trans;
					AddParameter (cmdUpdate, "PropertyNames", propertyNames);
					AddParameter (cmdUpdate, "PropertyValuesString", propertyValuesString);
					AddParameter (cmdUpdate, "PropertyValuesBinary", propertyValuesBinary);
					AddParameter (cmdUpdate, "LastUpdatedDate", currentTimeUtc);
					AddParameter (cmdUpdate, "UserId", userId);
					cmdUpdate.ExecuteNonQuery ();
				}
				else {
					string queryInsert = @"INSERT INTO aspnet_Profile(UserId, PropertyNames, PropertyValuesString, " +
						"PropertyValuesBinary, LastUpdatedDate) VALUES (?, ?, ?, ?, ?)";
					OleDbCommand cmdInsert = new OleDbCommand (queryInsert, (OleDbConnection) connection);
					cmdInsert.Transaction = trans;
					AddParameter (cmdInsert, "UserId", userId);
					AddParameter (cmdInsert, "PropertyNames", propertyNames);
					AddParameter (cmdInsert, "PropertyValuesString", propertyValuesString);
					AddParameter (cmdInsert, "PropertyValuesBinary", propertyValuesBinary);
					AddParameter (cmdInsert, "LastUpdatedDate", currentTimeUtc);
					cmdInsert.ExecuteNonQuery ();
				}
				trans.Commit ();
			}
			catch (Exception e) {
				trans.Rollback ();
				throw e;
			}
			return 0;
		}

		private static string GetUserId (DbConnection connection, DbTransaction trans, string applicationId, string username)
		{
			if (username == null)
				return null;

			string selectQuery = "SELECT UserId FROM aspnet_Users WHERE LoweredUserName = ? AND ApplicationId = ?";

			OleDbCommand selectCmd = new OleDbCommand (selectQuery, (OleDbConnection) connection);
			if (trans != null)
				selectCmd.Transaction = (OleDbTransaction) trans;

			AddParameter (selectCmd, "LoweredUserName", username.ToLowerInvariant ());
			AddParameter (selectCmd, "ApplicationId", applicationId);
			using (OleDbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetString (0);
			}

			return null;
		}
		
		private static string GetProfileAuthOptions (int profileAuthOptions)
		{
			switch (profileAuthOptions) {
				case 1:
					return "AND IsAnonymous = 0";

				case 2:
					return "AND IsAnonymous = 1";
			}
			return string.Empty;
		}
	}
}

#endif