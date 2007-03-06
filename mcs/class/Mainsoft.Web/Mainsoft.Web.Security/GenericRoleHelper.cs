//
// Mainsoft.Web.Security.GenericRoleHelper
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//      Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Mainsoft
// (C) 2007 Marek Habersack
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
using System.Data.Common;
using System.Collections.Generic;
using System.Text;

namespace Mainsoft.Web.Security
{
	public partial class GenericRoleProvider
	{
		GenericDatabaseHelper dbHelper;
		
		void InitializeHelper ()
                {
                        dbHelper = new GenericDatabaseHelper (connectionString);
			dbHelper.Initialize ();
                        dbHelper.RegisterSchemaUnloadHandler ();
                }		

		void AddParameter (List <DbParameter> parms, string paramName, object paramValue)
                {
                        DbParameter prm = dbHelper.NewParameter (paramName, paramValue);
                        parms.Add (prm);
                }
		
		int Roles_CreateRole (DbConnection connection, string applicationName, string rolename)
		{
			string appId = (string) dbHelper.CreateApplication (connection, applicationName);
			if (appId == null)
				return 1;

			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "ApplicationId", appId);
			AddParameter (parms, "LoweredRoleName", rolename.ToLower ());
			DbCommand cmdSelect = dbHelper.GetCommand ("Roles_CreateRoleGetRoleName", connection, parms,
								   RolesCreateRoleGetRoleNameQueryBuilder);

			using (DbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ())
					return 2; // role already exists
			}

			parms.Clear ();
			AddParameter (parms, "ApplicationId", appId);
			AddParameter (parms, "RoleId", Guid.NewGuid ().ToString ());
			AddParameter (parms, "RoleName", rolename);
			AddParameter (parms, "LoweredRoleName", rolename.ToLower ());
			DbCommand cmdInsert = dbHelper.GetCommand ("Roles_CreateRoleInsertRole", connection, parms,
								   RolesCreateRoleInsertRoleQueryBuilder);
			cmdInsert.ExecuteNonQuery ();

			return 0;
		}

		int Roles_DeleteRole (DbConnection connection, string applicationName, string rolename,
				      bool deleteOnlyIfRoleIsEmpty)
		{
			string appId = dbHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string roleId = GetRoleId (connection, appId, rolename);
			if (roleId == null)
				return 2;

			List <DbParameter> parms = new List <DbParameter> (1);
			if (deleteOnlyIfRoleIsEmpty) {
				AddParameter (parms, "RoleId", roleId);
				DbCommand cmdSelect = dbHelper.GetCommand ("Roles_AnyUsersInRole", connection, parms,
									   RolesAnyUsersInRoleQueryBuilder);
				using (DbDataReader reader = cmdSelect.ExecuteReader ()) {
					if (reader.Read ())
						// role is not empty
						return 3;
				}
			}

			parms.Clear ();
			AddParameter (parms, "RoleId", roleId);			
			DbCommand cmdDelUsers = dbHelper.GetCommand ("Roles_DeleteRoleUsers", connection, parms,
								     RolesDeleteRoleUsersQueryBuilder);
			cmdDelUsers.ExecuteNonQuery ();

			parms.Clear ();
			AddParameter (parms, "ApplicationId", appId);
			AddParameter (parms, "RoleId", roleId);			
			DbCommand cmdDelRole = dbHelper.GetCommand ("Roles_DeleteRole", connection, parms,
								    RolesDeleteRoleQueryBuilder);

			cmdDelRole.ExecuteNonQuery ();

			return 0;
		}

		int Roles_GetAllRoles (DbConnection connection, string applicationName, out DbDataReader reader)
		{
			reader = null;
			
			string appId = dbHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			List <DbParameter> parms = new List <DbParameter> (1);
			AddParameter (parms, "ApplicationId", appId);
			DbCommand cmdSelect = dbHelper.GetCommand ("Roles_GetAllRoles", connection, parms,
								   RolesGetAllRolesQueryBuilder);

			reader = cmdSelect.ExecuteReader ();

			return 0;
		}

		int Roles_RoleExists (DbConnection connection, string applicationName, string rolename)
		{
			
			string appId = dbHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "ApplicationId", appId);
			AddParameter (parms, "LoweredRoleName", rolename.ToLower ());			
			DbCommand cmdSelect = dbHelper.GetCommand ("Roles_RoleExists", connection, parms,
								   RolesRoleExistsQueryBuilder);

			using (DbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ())
					return 2;
			}
			return 0;
		}

		int UsersInRoles_AddUsersToRoles (DbConnection connection, string applicationName,
						  string [] userNames, string [] roleNames, DateTime currentTimeUtc)
		{
			string appId = dbHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string [] userIds = new string [userNames.Length];
			string [] loweredUsernames = new string [userNames.Length];
			string [] roleIds = new string [roleNames.Length];

			List <DbParameter> parms = new List <DbParameter> (userNames.Length + 1);
			AddParameter (parms, "ApplicationId", appId);
			for (int i = 0; i < userNames.Length; i++)
				AddParameter (parms, String.Format ("LoweredUserName{0}", i), userNames [i].ToLower ());
			DbCommand cmdSelUsers = dbHelper.GetCommand ("Roles_AddUsersToRolesGetUsers", connection, parms,
								     RolesAddUsersToRolesGetUsersQueryBuilder);			

			int userIndex = 0;
			using (DbDataReader reader = cmdSelUsers.ExecuteReader ()) {
				while (reader.Read ()) {
					userIds [userIndex] = reader.GetString (0);
					loweredUsernames [userIndex] = reader.GetString (1);
					userIndex++;
				}
			}

			if (userNames.Length != userIndex) {
				// find not existing users and create them
				parms.Clear ();
				for (int j = 0; j < userNames.Length; j++)
					if (Array.IndexOf (loweredUsernames, userNames [j].ToLower ()) < 0) {
						string newUserId = Guid.NewGuid ().ToString ();
						parms.Clear ();
						AddParameter (parms, "ApplicationId", appId);
						AddParameter (parms, "UserId", newUserId);
						AddParameter (parms, "UserName", userNames [j]);
						AddParameter (parms, "LoweredUserName", userNames [j].ToLower ());
						AddParameter (parms, "IsAnonymous", 0);
						AddParameter (parms, "LastActivityDate", DateTime.UtcNow);
						DbCommand cmdAddUser = dbHelper.GetCommand ("Roles_AddUsersToRolesCreateUser",
											    connection, parms,
											    RolesAddUsersToRolesCreateUserQueryBuilder);
						cmdAddUser.ExecuteNonQuery ();
						userIds [userIndex++] = newUserId;
					}
			}

			parms.Clear ();
			AddParameter (parms, "ApplicationId", appId);
			for (int i = 0; i < roleNames.Length; i++)
				AddParameter (parms, String.Format ("LoweredRoleName{0}", i), roleNames [i].ToLower ());
			DbCommand cmdSelRoles = dbHelper.GetCommand ("Roles_AddUsersToRolesGetRoles", connection, parms,
								     RolesAddUsersToRolesGetRolesQueryBuilder);

			using (DbDataReader reader = cmdSelRoles.ExecuteReader ()) {
				int i = 0;
				while (reader.Read ())
					roleIds [i++] = reader.GetString (0);

				if (roleNames.Length != i)
					return 2; // one or more roles not found
			}

			parms.Clear ();
			foreach (string userId in userIds)
				AddParameter (parms, "UserId", userId);
			foreach (string roleId in roleIds)
				AddParameter (parms, "RoleId", roleId);
			DbCommand cmdSelCount = dbHelper.GetCommand ("Roles_CountUsers", connection, parms,
								     RolesCountUsersQueryBuilder, userNames.Length);
			
			using (DbDataReader reader = cmdSelCount.ExecuteReader ()) {
				if (reader.Read ())
					if (reader.GetInt32 (0) > 0)
						return 3;
			}

			parms.Clear ();
			foreach (string roleId in roleIds)
				foreach (string userId in userIds) {
					AddParameter (parms, "UserId", userId);
					AddParameter (parms, "RoleId", roleId);
				}
			
			DbCommand cmdInsert = dbHelper.GetCommand ("Roles_AddUsersToRoles", connection, parms,
								   RolesAddUsersInsertUsersToRolesQueryBuilder);

			cmdInsert.ExecuteNonQuery ();
			return 0;
		}

		int UsersInRoles_FindUsersInRole (DbConnection connection, string applicationName,
						  string rolename, string userNameToMatch, out DbDataReader reader)
		{
			reader = null;
			
			string appId = dbHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string roleId = GetRoleId (connection, appId, rolename);
			if (roleId == null)
				return 2;

			List <DbParameter> parms = new List <DbParameter> (3);
			AddParameter (parms, "ApplicationId", appId);
			AddParameter (parms, "RoleId", roleId);
			AddParameter (parms, "LoweredUserName", "%" + userNameToMatch.ToLower() + "%");
			DbCommand cmdSelect = dbHelper.GetCommand ("Roles_FindUsersInRole", connection, parms,
								   RolesFindUsersInRoleQueryBuilder);
			reader = cmdSelect.ExecuteReader ();

			return 0;
		}

		int UsersInRoles_GetRolesForUser (DbConnection connection, string applicationName,
						  string username, out DbDataReader reader)
		{
			reader = null;
			string appId = dbHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string userId = GetUserId (connection, appId, username);
			if (userId == null)
				return 2;

			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "ApplicationId", appId);
			AddParameter (parms, "UserId", userId);
			DbCommand cmdSelect = dbHelper.GetCommand ("Roles_GetRolesForUser", connection, parms,
								   RolesGetRolesForUserQueryBuilder);
			reader = cmdSelect.ExecuteReader ();

			return 0;
		}

		int UsersInRoles_GetUsersInRoles (DbConnection connection, string applicationName,
						  string rolename, out DbDataReader reader)
		{
			reader = null;
			string appId = dbHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string roleId = GetRoleId (connection, appId, rolename);
			if (roleId == null)
				return 2;

			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "ApplicationId", appId);
			AddParameter (parms, "RoleId", roleId);
			DbCommand cmdSelect = dbHelper.GetCommand ("Roles_GetUsersInRoles", connection, parms,
								   RolesGetUsersInRolesQueryBuilder);
			reader = cmdSelect.ExecuteReader ();

			return 0;
		}

		int UsersInRoles_IsUserInRole (DbConnection connection, string applicationName,
					       string username, string rolename)
		{
			string appId = dbHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string userId = GetUserId (connection, appId, username);
			if (userId == null)
				return 2;

			string roleId = GetRoleId (connection, appId, rolename);
			if (roleId == null)
				return 3;

			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "UserId", userId);
			AddParameter (parms, "RoleId", roleId);
			DbCommand cmdSelect = dbHelper.GetCommand ("Roles_IsUserInRole", connection, parms,
								   RolesIsUserInRoleQueryBuilder);
			
			using (DbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ())
					return 4;
			}
			return 0;
		}

		int UsersInRoles_RemoveUsersFromRoles (DbConnection connection, string applicationName,
						       string [] userNames, string [] roleNames)
		{
			string appId = dbHelper.GetApplicationId (connection, applicationName);
			if (appId == null)
				return 1;

			string [] userIds = new string [userNames.Length];
			string [] roleIds = new string [roleNames.Length];

			List <DbParameter> parms = new List <DbParameter> (userNames.Length + 1);
			AddParameter (parms, "ApplicationId", appId);
			for (int i = 0; i < userNames.Length; i++)
				AddParameter (parms, "LoweredUserName", userNames [i].ToLower ());
			DbCommand cmdSelUsers = dbHelper.GetCommand ("Roles_RemoveUsersFromRolesSelUsers", connection, parms,
								     RolesRemoveUsersFromRolesSelUsersQueryBuilder);

			using (DbDataReader reader = cmdSelUsers.ExecuteReader ()) {
				int i = 0;
				while (reader.Read ())
					userIds [i++] = reader.GetString (0);

				if (userNames.Length != i)
					return 2; // one or more users not found
			}

			parms.Clear ();
			AddParameter (parms, "ApplicationId", appId);
			for (int i = 0; i < roleNames.Length; i++)
				AddParameter (parms, "LoweredRoleName", roleNames [i].ToLower ());
			DbCommand cmdSelRoles = dbHelper.GetCommand ("Roles_RemoveUsersFromRolesSelRoles", connection, parms,
								     RolesRemoveUsersFromRolesSelRolesQueryBuilder);
			
			using (DbDataReader reader = cmdSelRoles.ExecuteReader ()) {
				int i = 0;
				while (reader.Read ())
					roleIds [i++] = reader.GetString (0);

				if (roleNames.Length != i)
					return 3; // one or more roles not found
			}

			parms.Clear ();
			foreach (string userId in userIds)
				AddParameter (parms, "UserId", userId);
			foreach (string roleId in roleIds)
				AddParameter (parms, "RoleId", roleId);
			DbCommand cmdSelCount = dbHelper.GetCommand ("Roles_CountUsers", connection, parms,
								     RolesCountUsersQueryBuilder, userNames.Length);
			
			using (DbDataReader reader = cmdSelCount.ExecuteReader ()) {
				if (reader.Read ())
					if (userNames.Length * roleNames.Length > reader.GetInt32 (0))
						return 4;
			}

			parms.Clear ();
			foreach (string userId in userIds)
				AddParameter (parms, "UserId", userId);
			foreach (string roleId in roleIds)
				AddParameter (parms, "RoleId", roleId);
			DbCommand cmdDelete = dbHelper.GetCommand ("Roles_RemoveUsersFromRolesDelete", connection, parms,
								   RolesRemoveUsersFromRolesDeleteQueryBuilder, userNames.Length);
			cmdDelete.ExecuteNonQuery ();

			return 0;
		}

		string GetRoleId (DbConnection connection, string applicationId, string rolename)
		{
			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "LoweredRoleName", rolename.ToLower ());
			AddParameter (parms, "ApplicationId", applicationId);
			DbCommand selectCmd = dbHelper.GetCommand ("Roles_GetRoleId", connection, parms,
								   RolesGetRoleIdQueryBuilder);
			
			using (DbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetString (0);
			}

			return null;
		}

		string GetUserId (DbConnection connection, string applicationId, string username)
		{
			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "LoweredUserName", username.ToLower ());
			AddParameter (parms, "ApplicationId", applicationId);
			DbCommand selectCmd = dbHelper.GetCommand ("Roles_GetUserId", connection, parms,
								   RolesGetUserIdQueryBuilder);
			
			using (DbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetString (0);
			}

			return null;
		}
	}
}

#endif