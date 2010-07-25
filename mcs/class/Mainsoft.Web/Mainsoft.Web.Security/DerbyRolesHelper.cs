//
// Mainsoft.Web.Security.DerbyRolesHelper
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft
//
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
	static class DerbyRolesHelper
	{
		private static OleDbParameter AddParameter (OleDbCommand command, string paramName, object paramValue)
		{
			OleDbParameter prm = new OleDbParameter (paramName, paramValue);
			command.Parameters.Add (prm);
			return prm;
		}

		public static int Roles_CreateRole (DbConnection connection, string applicationName, string rolename)
		{
			string appId = (string) DerbyApplicationsHelper.Applications_CreateApplication (connection, applicationName);
			if (appId == null)
				return 1;

			string querySelect = "SELECT RoleName FROM aspnet_Roles WHERE ApplicationId = ? AND LoweredRoleName = ?";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "ApplicationId", appId);
			AddParameter (cmdSelect, "LoweredRoleName", rolename.ToLowerInvariant ());

			using (OleDbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ())
					return 2; // role already exists
			}

			string queryInsert = "INSERT INTO aspnet_Roles (ApplicationId, RoleId, RoleName, LoweredRoleName) VALUES (?, ?, ?, ?)";
			OleDbCommand cmdInsert = new OleDbCommand (queryInsert, (OleDbConnection) connection);
			AddParameter (cmdInsert, "ApplicationId", appId);
			AddParameter (cmdInsert, "RoleId", Guid.NewGuid ().ToString ());
			AddParameter (cmdInsert, "RoleName", rolename);
			AddParameter (cmdInsert, "LoweredRoleName", rolename.ToLowerInvariant ());
			cmdInsert.ExecuteNonQuery ();

			return 0;
		}

		public static int Roles_DeleteRole (DbConnection connection, string applicationName, string rolename, bool deleteOnlyIfRoleIsEmpty)
		{
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string roleId = GetRoleId (connection, appId, rolename);
			if (roleId == null)
				return 2;

			if (deleteOnlyIfRoleIsEmpty) {
				string querySelect = "SELECT RoleId FROM aspnet_UsersInRoles WHERE RoleId = ?";
				OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
				AddParameter (cmdSelect, "RoleId", roleId);
				using (OleDbDataReader reader = cmdSelect.ExecuteReader ()) {
					if (reader.Read ())
						// role is not empty
						return 3;
				}
			}

			string queryDelUsers = "DELETE FROM aspnet_UsersInRoles WHERE RoleId = ?";
			OleDbCommand cmdDelUsers = new OleDbCommand (queryDelUsers, (OleDbConnection) connection);
			AddParameter (cmdDelUsers, "RoleId", roleId);
			cmdDelUsers.ExecuteNonQuery ();

			string queryDelRole = "DELETE FROM aspnet_Roles WHERE ApplicationId = ? AND RoleId = ? ";
			OleDbCommand cmdDelRole = new OleDbCommand (queryDelRole, (OleDbConnection) connection);
			AddParameter (cmdDelRole, "ApplicationId", appId);
			AddParameter (cmdDelRole, "RoleId", roleId);
			cmdDelRole.ExecuteNonQuery ();

			return 0;
		}

		public static int Roles_GetAllRoles (DbConnection connection, string applicationName, out DbDataReader reader)
		{
			reader = null;
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string querySelect = "SELECT RoleName FROM aspnet_Roles WHERE ApplicationId = ? ORDER BY RoleName";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "ApplicationId", appId);
			reader = cmdSelect.ExecuteReader ();

			return 0;
		}

		public static int Roles_RoleExists (DbConnection connection, string applicationName, string rolename)
		{
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string querySelect = "SELECT RoleName FROM aspnet_Roles WHERE ApplicationId = ? AND LoweredRoleName = ?";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "ApplicationId", appId);
			AddParameter (cmdSelect, "LoweredRoleName", rolename.ToLowerInvariant ());

			using (OleDbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ())
					return 2;
			}
			return 0;
		}

		public static int UsersInRoles_AddUsersToRoles (DbConnection connection, string applicationName, string [] userNames, string [] roleNames, DateTime currentTimeUtc)
		{
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string [] userIds = new string [userNames.Length];
			string [] loweredUsernames = new string [userNames.Length];
			string [] roleIds = new string [roleNames.Length];

			string querySelUsers = "SELECT UserId, LoweredUserName FROM aspnet_Users WHERE ApplicationId = ? AND LoweredUserName in " + GetPrms (userNames.Length);
			OleDbCommand cmdSelUsers = new OleDbCommand (querySelUsers, (OleDbConnection) connection);
			AddParameter (cmdSelUsers, "ApplicationId", appId);
			for (int i = 0; i < userNames.Length; i++)
				AddParameter (cmdSelUsers, "LoweredUserName", userNames [i].ToLowerInvariant ());

			int userIndex = 0;
			using (OleDbDataReader reader = cmdSelUsers.ExecuteReader ()) {
				while (reader.Read ()) {
					userIds [userIndex] = reader.GetString (0);
					loweredUsernames [userIndex] = reader.GetString (1);
					userIndex++;
				}
			}

			if (userNames.Length != userIndex) {
				// find not existing users and create them
				for (int j = 0; j < userNames.Length; j++)
					if (Array.IndexOf (loweredUsernames, userNames [j].ToLowerInvariant ()) < 0) {
						string newUserId = Guid.NewGuid ().ToString ();
						string queryAddUser = "INSERT INTO aspnet_Users (ApplicationId, UserId, UserName, " +
							"LoweredUserName, IsAnonymous, LastActivityDate) VALUES (?, ?, ?, ?, ?, ?)";
						OleDbCommand cmdAddUser = new OleDbCommand (queryAddUser, (OleDbConnection) connection);
						AddParameter (cmdAddUser, "ApplicationId", appId);
						AddParameter (cmdAddUser, "UserId", newUserId);
						AddParameter (cmdAddUser, "UserName", userNames [j]);
						AddParameter (cmdAddUser, "LoweredUserName", userNames [j].ToLowerInvariant ());
						AddParameter (cmdAddUser, "IsAnonymous", 0);
						AddParameter (cmdAddUser, "LastActivityDate", DateTime.UtcNow);
						cmdAddUser.ExecuteNonQuery ();

						userIds [userIndex++] = newUserId;
					}
			}


			string querySelRoles = "SELECT RoleId FROM aspnet_Roles WHERE ApplicationId = ? AND LoweredRoleName in " + GetPrms (roleNames.Length);
			OleDbCommand cmdSelRoles = new OleDbCommand (querySelRoles, (OleDbConnection) connection);
			AddParameter (cmdSelRoles, "ApplicationId", appId);
			for (int i = 0; i < roleNames.Length; i++)
				AddParameter (cmdSelRoles, "LoweredRoleName", roleNames [i].ToLowerInvariant ());

			using (OleDbDataReader reader = cmdSelRoles.ExecuteReader ()) {
				int i = 0;
				while (reader.Read ())
					roleIds [i++] = reader.GetString (0);

				if (roleNames.Length != i)
					return 2; // one or more roles not found
			}

			string querySelCount = "SELECT COUNT(*) FROM aspnet_UsersInRoles WHERE UserId in " + GetPrms (userNames.Length) + " AND RoleId in " + GetPrms (roleNames.Length);
			OleDbCommand cmdSelCount = new OleDbCommand (querySelCount, (OleDbConnection) connection);
			foreach (string userId in userIds)
				AddParameter (cmdSelCount, "UserId", userId);
			foreach (string roleId in roleIds)
				AddParameter (cmdSelCount, "RoleId", roleId);
			using (OleDbDataReader reader = cmdSelCount.ExecuteReader ()) {
				if (reader.Read ())
					if (reader.GetInt32 (0) > 0)
						return 3;
			}

			string valuesExp = string.Empty;
			int pairs = userNames.Length * roleNames.Length;
			for (int i = 0; i < pairs; i++)
				valuesExp += "(?, ?),";

			string queryInsert = "INSERT INTO aspnet_UsersInRoles (UserId, RoleId) VALUES " + valuesExp.Trim (',');
			OleDbCommand cmdInsert = new OleDbCommand (queryInsert, (OleDbConnection) connection);
			foreach (string roleId in roleIds)
				foreach (string userId in userIds) {
					AddParameter (cmdInsert, "UserId", userId);
					AddParameter (cmdInsert, "RoleId", roleId);
				}

			cmdInsert.ExecuteNonQuery ();
			return 0;
		}

		public static int UsersInRoles_FindUsersInRole (DbConnection connection, string applicationName, string rolename, string userNameToMatch, out DbDataReader reader)
		{
			reader = null;
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string roleId = GetRoleId (connection, appId, rolename);
			if (roleId == null)
				return 2;

			string querySelect = "SELECT usr.UserName FROM aspnet_Users usr, aspnet_UsersInRoles uir " +
				"WHERE usr.UserId = uir.UserId AND usr.ApplicationId = ? AND uir.RoleId = ? AND LoweredUserName LIKE ? " +
				"ORDER BY usr.UserName";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "ApplicationId", appId);
			AddParameter (cmdSelect, "RoleId", roleId);
			AddParameter (cmdSelect, "LoweredUserName", "%" + userNameToMatch.ToLowerInvariant() + "%");
			reader = cmdSelect.ExecuteReader ();

			return 0;
		}

		public static int UsersInRoles_GetRolesForUser (DbConnection connection, string applicationName, string username, out DbDataReader reader)
		{
			reader = null;
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string userId = GetUserId (connection, appId, username);
			if (userId == null)
				return 2;

			string querySelect = "SELECT rol.RoleName FROM aspnet_Roles rol, aspnet_UsersInRoles uir " +
				"WHERE rol.RoleId = uir.RoleId AND rol.ApplicationId = ? AND uir.UserId = ? ORDER BY rol.RoleName";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "ApplicationId", appId);
			AddParameter (cmdSelect, "UserId", userId);
			reader = cmdSelect.ExecuteReader ();

			return 0;
		}

		public static int UsersInRoles_GetUsersInRoles (DbConnection connection, string applicationName, string rolename, out DbDataReader reader)
		{
			reader = null;
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string roleId = GetRoleId (connection, appId, rolename);
			if (roleId == null)
				return 2;

			string querySelect = "SELECT usr.UserName FROM aspnet_Users usr, aspnet_UsersInRoles uir " +
				"WHERE usr.UserId = uir.UserId AND usr.ApplicationId = ? AND uir.RoleId = ? ORDER BY usr.UserName";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "ApplicationId", appId);
			AddParameter (cmdSelect, "RoleId", roleId);
			reader = cmdSelect.ExecuteReader ();

			return 0;
		}

		public static int UsersInRoles_IsUserInRole (DbConnection connection, string applicationName, string username, string rolename)
		{
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string userId = GetUserId (connection, appId, username);
			if (userId == null)
				return 2;

			string roleId = GetRoleId (connection, appId, rolename);
			if (roleId == null)
				return 3;

			string querySelect = "SELECT UserId FROM aspnet_UsersInRoles WHERE UserId = ? AND RoleId = ?";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "UserId", userId);
			AddParameter (cmdSelect, "RoleId", roleId);
			using (OleDbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ())
					return 4;
			}
			return 0;
		}

		public static int UsersInRoles_RemoveUsersFromRoles (DbConnection connection, string applicationName, string [] userNames, string [] roleNames)
		{
			string appId = DerbyApplicationsHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string [] userIds = new string [userNames.Length];
			string [] roleIds = new string [roleNames.Length];

			string querySelUsers = "SELECT UserId FROM aspnet_Users WHERE ApplicationId = ? AND LoweredUserName in " + GetPrms (userNames.Length);
			OleDbCommand cmdSelUsers = new OleDbCommand (querySelUsers, (OleDbConnection) connection);
			AddParameter (cmdSelUsers, "ApplicationId", appId);
			for (int i = 0; i < userNames.Length; i++)
				AddParameter (cmdSelUsers, "LoweredUserName", userNames [i].ToLowerInvariant ());

			using (OleDbDataReader reader = cmdSelUsers.ExecuteReader ()) {
				int i = 0;
				while (reader.Read ())
					userIds [i++] = reader.GetString (0);

				if (userNames.Length != i)
					return 2; // one or more users not found
			}

			string querySelRoles = "SELECT RoleId FROM aspnet_Roles WHERE ApplicationId = ? AND LoweredRoleName in " + GetPrms (roleNames.Length);
			OleDbCommand cmdSelRoles = new OleDbCommand (querySelRoles, (OleDbConnection) connection);
			AddParameter (cmdSelRoles, "ApplicationId", appId);
			for (int i = 0; i < roleNames.Length; i++)
				AddParameter (cmdSelRoles, "LoweredRoleName", roleNames [i].ToLowerInvariant ());

			using (OleDbDataReader reader = cmdSelRoles.ExecuteReader ()) {
				int i = 0;
				while (reader.Read ())
					roleIds [i++] = reader.GetString (0);

				if (roleNames.Length != i)
					return 3; // one or more roles not found
			}

			string querySelCount = "SELECT COUNT(*) FROM aspnet_UsersInRoles WHERE UserId in " + GetPrms (userNames.Length) + " AND RoleId in " + GetPrms (roleNames.Length);
			OleDbCommand cmdSelCount = new OleDbCommand (querySelCount, (OleDbConnection) connection);
			foreach (string userId in userIds)
				AddParameter (cmdSelCount, "UserId", userId);
			foreach (string roleId in roleIds)
				AddParameter (cmdSelCount, "RoleId", roleId);
			using (OleDbDataReader reader = cmdSelCount.ExecuteReader ()) {
				if (reader.Read ())
					if (userNames.Length * roleNames.Length > reader.GetInt32 (0))
						return 4;
			}

			string queryDelete = "DELETE FROM aspnet_UsersInRoles WHERE UserId in " + GetPrms (userNames.Length) + " AND RoleId in " + GetPrms (roleNames.Length);
			OleDbCommand cmdDelete = new OleDbCommand (queryDelete, (OleDbConnection) connection);
			foreach (string userId in userIds)
				AddParameter (cmdDelete, "UserId", userId);
			foreach (string roleId in roleIds)
				AddParameter (cmdDelete, "RoleId", roleId);
			cmdDelete.ExecuteNonQuery ();

			return 0;
		}

		private static string GetRoleId (DbConnection connection, string applicationId, string rolename)
		{
			string selectQuery = "SELECT RoleId FROM aspnet_Roles WHERE LoweredRoleName = ? AND ApplicationId = ?";

			OleDbCommand selectCmd = new OleDbCommand (selectQuery, (OleDbConnection) connection);
			AddParameter (selectCmd, "LoweredRoleName", rolename.ToLowerInvariant ());
			AddParameter (selectCmd, "ApplicationId", applicationId);
			using (OleDbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetString (0);
			}

			return null;
		}

		private static string GetUserId (DbConnection connection, string applicationId, string username)
		{
			string selectQuery = "SELECT UserId FROM aspnet_Users WHERE LoweredUserName = ? AND ApplicationId = ?";

			OleDbCommand selectCmd = new OleDbCommand (selectQuery, (OleDbConnection) connection);
			AddParameter (selectCmd, "LoweredUserName", username.ToLowerInvariant ());
			AddParameter (selectCmd, "ApplicationId", applicationId);
			using (OleDbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetString (0);
			}

			return null;
		}

		private static string GetPrms (int n)
		{
			string exp = string.Empty;
			for (int i = 0; i < n; i++)
				exp += "?,";

			exp = "(" + exp.Trim (',') + ")";
			return exp;
		}
	}
}

#endif