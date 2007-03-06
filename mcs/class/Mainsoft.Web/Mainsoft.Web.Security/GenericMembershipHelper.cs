//
// Mainsoft.Web.Security.GenericMembershipHelper
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//      Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Mainsoft
// (C) 2007 Marek Habersack
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
	public partial class GenericMembershipProvider
	{
		static readonly DateTime DefaultDateTime = new DateTime (1754, 1, 1).ToUniversalTime ();
		GenericDatabaseHelper dbHelper;
		
		private void InitializeHelper ()
		{
			dbHelper = new GenericDatabaseHelper (connectionString);
			dbHelper.Initialize ();
			dbHelper.RegisterSchemaUnloadHandler ();
		}
		
		private void AddParameter (List <DbParameter> parms, string paramName, object paramValue)
		{
			DbParameter prm = dbHelper.NewParameter (paramName, paramValue);
			parms.Add (prm);
		}
		
		int Membership_ChangePasswordQuestionAndAnswer (DbConnection connection, string applicationName,
								string username, string newPwdQuestion,
								string newPwdAnswer)
		{
			string userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			List <DbParameter> parms = new List <DbParameter> (3);
			AddParameter (parms, "PasswordQuestion", newPwdQuestion);
			AddParameter (parms, "PasswordAnswer", newPwdAnswer);
			AddParameter (parms, "UserId", userId);
			DbCommand updateCmd = dbHelper.GetCommand ("Membership_ChangePasswordQuestionAndAnswer", connection, parms,
								   ChangePasswordQuestionAndAnswerQueryBuilder);
			updateCmd.ExecuteNonQuery ();
			return 0;
		}

		private int Membership_CreateUser (DbConnection connection, string applicationName, string username,
						   string password, string passwordSalt, string email, string pwdQuestion,
						   string pwdAnswer, bool isApproved, DateTime currentTimeUtc,
						   DateTime createDate, bool uniqueEmail, int passwordFormat,
						   ref object userId)
		{
			string applicationId = (string) dbHelper.CreateApplication (connection, applicationName);
			string newUserId = (string) userId;
			DbTransaction trans = connection.BeginTransaction ();

			try {
				int returnValue = Users_CreateUser (connection, trans, applicationId, username,
								    false, createDate, ref userId);
				if (returnValue == 1) {
					// the user exists in users table, this can occur when user
					// does not have membership information, but has other information
					// like roles, etc.
					if (userId != null && newUserId != null && newUserId != (string) userId) {
						trans.Rollback ();
						return 9; // wrong userid provided
					}
				} else if (returnValue == 2) {
					// another user with provided id already exists
					trans.Rollback ();
					return 10; // wrong userid provided
				}
				newUserId = (string) userId;

				List <DbParameter> parms = new List <DbParameter> (1);
				AddParameter (parms, "UserId", newUserId);
				DbCommand selectCmdMbrUserId = dbHelper.GetCommand ("Membership_CreateUserQueryMbrUserId",
										    connection, trans, parms,
										    CreateUserGetUserIdQueryBuilder);
				
				using (DbDataReader reader = selectCmdMbrUserId.ExecuteReader ()) {
					if (reader.Read ()) {
						trans.Rollback ();
						return 2; // user with such userId already exists
					}
				}

				if (uniqueEmail) {
					parms.Clear ();
					AddParameter (parms, "ApplicationId", applicationId);
					AddParameter (parms, "LoweredEmail", email.ToLower ());
					DbCommand cmdMbrEmail = dbHelper.GetCommand ("Membership_CreateUserQueryByEmail",
										     connection, trans, parms,
										     CreateUserGetByMailQueryBuilder);
					
					using (DbDataReader reader = cmdMbrEmail.ExecuteReader ()) {
						if (reader.Read ()) {
							trans.Rollback ();
							return 3; // user with such email already exists
						}
					}
				}

				if (returnValue == 1) {
					// if user was not created, but found existing and correct
					// update it's activity (membership create) time.
					parms.Clear ();
					AddParameter (parms, "LastActivityDate", createDate);
					AddParameter (parms, "UserId", newUserId);
					DbCommand cmdUpdActivity = dbHelper.GetCommand ("Membership_CreateUserUpdateActivity",
											connection, trans, parms,
											CreateUserUpdateActivityQueryBuilder);
					cmdUpdActivity.ExecuteNonQuery ();
				}

				parms.Clear ();
				AddParameter (parms, "ApplicationId", applicationId);
				AddParameter (parms, "UserId", newUserId);
				AddParameter (parms, "Password", password);
				AddParameter (parms, "PasswordFormat", passwordFormat);
				AddParameter (parms, "PasswordSalt", passwordSalt);
				AddParameter (parms, "Email", email);
				AddParameter (parms, "LoweredEmail", email != null ? email.ToLower () : null);
				AddParameter (parms, "PasswordQuestion", pwdQuestion);
				AddParameter (parms, "PasswordAnswer", pwdAnswer);
				AddParameter (parms, "IsApproved", isApproved);
				AddParameter (parms, "IsLockedOut", 0);
				AddParameter (parms, "CreateDate", createDate);
				AddParameter (parms, "LastLoginDate", DefaultDateTime);
				AddParameter (parms, "LastPasswordChangedDate", createDate);
				AddParameter (parms, "LastLockoutDate", DefaultDateTime);
				AddParameter (parms, "FailedPasswordAttemptCount", 0);
				AddParameter (parms, "FailedPwdAttemptWindowStart", DefaultDateTime);
				AddParameter (parms, "FailedPwdAnswerAttemptCount", 0);
				AddParameter (parms, "FailedPwdAnswerAttWindowStart", DefaultDateTime);
				DbCommand cmdInsertMbr = dbHelper.GetCommand ("Membership_CreateUserCreateMemberActivity",
									      connection, trans, parms,
									      CreateUserCreateMemberQueryBuilder);
				
				cmdInsertMbr.ExecuteNonQuery ();

				trans.Commit ();
			}
			catch (Exception e) {
				trans.Rollback ();
				throw e;
			}

			return 0;
		}

		private int Membership_FindUsersByEmail (DbConnection connection, string applicationName, string emailToMatch,
							 int pageIndex, int pageSize, out DbDataReader reader)
		{
			List <DbParameter> parms = new List <DbParameter> (1);
			AddParameter (parms, "LoweredEmail", emailToMatch.ToLower ());
			DbCommand cmdSelect = dbHelper.GetCommand ("Membership_FindUsersByEmailSelect",
								   connection, parms, FindUsersByEmailSelectQueryBuilder);

			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		private int Membership_FindUsersByName (DbConnection connection, string applicationName,
							string userNameToMatch, int pageIndex, int pageSize,
							out DbDataReader reader)
		{
			List <DbParameter> parms = new List <DbParameter> (1);
			AddParameter (parms, "LoweredUserName", userNameToMatch.ToLower ());
			DbCommand cmdSelect = dbHelper.GetCommand ("Membership_FindUsersByNameSelect",
								   connection, parms, FindUsersByNameSelectQueryBuilder);
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		private int Membership_GetAllUsers (DbConnection connection, string applicationName,
						    int pageIndex, int pageSize, out DbDataReader reader)
		{
			DbCommand cmdSelect = dbHelper.GetCommand ("Membership_GetAllUsersSelect", connection,
								   GetAllUsersSelectQueryBuilder);
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		private int Membership_GetNumberOfUsersOnline (DbConnection connection, string applicationName,
							       int minutesSinceLastInActive, DateTime currentTimeUtc)
		{
			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "LastActivityDate", currentTimeUtc.AddMinutes (-minutesSinceLastInActive));
			AddParameter (parms, "LoweredApplicationName", applicationName.ToLower ());
			DbCommand cmdUsersActive = dbHelper.GetCommand ("Membership_GetNumberOfUsersOnlineSelect",
									connection, parms, GetNumberOfUsersOnlineSelectQueryBuilder);
			using (DbDataReader reader = cmdUsersActive.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetInt32 (0);
			}
			return 0;
		}

		private int Membership_GetPassword (DbConnection connection, string applicationName, string username,
						    string passwordAnswer, int maxInvalidPasswordAttempts,
						    int passwordAttemptWindow, DateTime currentTimeUtc,
						    out string password)
		{
			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "LoweredApplicationName", applicationName.ToLower ());
			AddParameter (parms, "LoweredUserName", username.ToLower ());
			DbCommand cmdSelect = dbHelper.GetCommand ("Membership_GetPasswordSelect",
								   connection, parms, GetPasswordSelectQueryBuilder);

			password = null;
			string dbUserId = null;
			string dbPassword = null;
			string dbPasswordAns = null;
			bool dbLockedOut = false;
			DateTime dbLastLockoutDate;
			int dbFailedPasswordAnswerAttemptCount = 0;
			DateTime dbFailedPasswordAnswerAttemptWindowStart;

			using (DbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ()) {
					dbUserId = reader.GetString (0);
					dbPassword = reader.GetString (1);
					dbPasswordAns = reader.GetString (2);
					dbLockedOut = reader.GetInt32 (3) > 0;
					dbLastLockoutDate = reader.GetDateTime (4);
					dbFailedPasswordAnswerAttemptCount = reader.GetInt32 (5);
					dbFailedPasswordAnswerAttemptWindowStart = reader.GetDateTime (6);
				}
				else {
					return 1; // user not found
				}
			}

			if (dbLockedOut)
				return 2; // locked out

			if (dbPasswordAns != passwordAnswer) {
				if (currentTimeUtc > dbFailedPasswordAnswerAttemptWindowStart.AddMinutes (passwordAttemptWindow)) {
					dbFailedPasswordAnswerAttemptWindowStart = currentTimeUtc;
					dbFailedPasswordAnswerAttemptCount = 1;
				} else {
					dbFailedPasswordAnswerAttemptWindowStart = currentTimeUtc;
					dbFailedPasswordAnswerAttemptCount++;
				}

				if (dbFailedPasswordAnswerAttemptCount > maxInvalidPasswordAttempts) {
					dbLockedOut = true;
					dbLastLockoutDate = currentTimeUtc;
				}
				return 3; // wrong password answer
			} else {
				dbFailedPasswordAnswerAttemptCount = 0;
				dbFailedPasswordAnswerAttemptWindowStart = DefaultDateTime;
				password = dbPassword;
			}

			parms = new List <DbParameter> (5);
			AddParameter (parms, "IsLockedOut", dbLockedOut);
			AddParameter (parms, "LastLockoutDate", dbLastLockoutDate);
			AddParameter (parms, "FailedPwdAnswerAttemptCount", dbFailedPasswordAnswerAttemptCount);
			AddParameter (parms, "FailedPwdAnswerAttWindowStart", dbFailedPasswordAnswerAttemptWindowStart);
			AddParameter (parms, "UserId", dbUserId);
			DbCommand cmdUpdate = dbHelper.GetCommand ("Membership_GetPasswordUpdate",
								   connection, parms, GetPasswordUpdateQueryBuilder);
			
			cmdUpdate.ExecuteNonQuery ();
			
			return 0;
		}

		private int Membership_GetPasswordWithFormat (DbConnection connection, string applicationName,
							      string username, bool updateLastActivity, DateTime currentTimeUtc,
							      out DbDataReader reader)
		{
			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "LoweredApplicationName", applicationName.ToLower ());
			AddParameter (parms, "LoweredUserName", username.ToLower ());
			
			DbCommand cmdSelect = dbHelper.GetCommand ("Membership_GetPasswordWithFormatSelect",
								   connection, parms, GetPasswordWithFormatSelectQueryBuilder);
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		private int Membership_GetUserByEmail (DbConnection connection, string applicationName, string email,
						       out string username)
		{
			List <DbParameter> parms = new List <DbParameter> (1);
			AddParameter (parms, "LoweredApplicationName", applicationName.ToLower ());
			if (email != null)
				AddParameter (parms, "LoweredEmail", email.ToLower ());
			DbCommand cmdSelect = dbHelper.GetCommand ("Membership_GetUserByEmailSelect",
								   connection, parms, GetUserByEmailSelectQueryBuilder);

			username = null;
			using (DbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ()) {
					username = reader.GetString (0);
					if (reader.Read ())
						return 2; // more that one user found with this email
				}
				else
					return 1; // no users found
				return 0;
			}
		}

		private int Membership_GetUserByName (DbConnection connection, string applicationName, string username,
						      bool updateLastActivity, DateTime currentTimeUtc, out DbDataReader reader)
		{
			reader = null;
			object userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			if (updateLastActivity) {
				List <DbParameter> parms = new List <DbParameter> (2);
				AddParameter (parms, "LastActivityDate", currentTimeUtc);
				AddParameter (parms, "UserId", userId);
				
				DbCommand cmdUpdate = dbHelper.GetCommand ("Membership_GetUserByNameUpdate",
									   connection, parms, GetUserByNameUpdateQueryBuilder);
				int records = cmdUpdate.ExecuteNonQuery ();
				if (records == 0)
					return -1; // unknown error
			}

			return Membership_GetUserByUserId (connection, userId, false, currentTimeUtc, out reader);
		}

		private int Membership_GetUserByUserId (DbConnection connection, object userId,
							bool updateLastActivity, DateTime currentTimeUtc,
							out DbDataReader reader)
		{
			reader = null;
			List <DbParameter> parms = new List <DbParameter> (2);
			if (updateLastActivity) {
				AddParameter (parms, "LastActivityDate", currentTimeUtc);
				AddParameter (parms, "UserId", userId);
				DbCommand cmdUpdate = dbHelper.GetCommand ("Membership_GetUserByNameUpdate",
									   connection, parms, GetUserByNameUpdateQueryBuilder);
				int recordsAffected = cmdUpdate.ExecuteNonQuery ();
				if (recordsAffected == 0)
					return 1; // user not found
			}

			parms.Clear ();
			AddParameter (parms, "UserId", userId);
			DbCommand cmdSelect = dbHelper.GetCommand ("Membership_GetUserByUserIdSelect",
								   connection, parms, GetUserByUserIdSelectQueryBuilder);
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		private int Membership_ResetPassword (DbConnection connection, string applicationName, string username,
						      string newPassword, string passwordAnswer, int passwordFormat,
						      string passwordSalt, int maxInvalidPasswordAttempts,
						      int passwordAttemptWindow, DateTime currentTimeUtc)
		{
			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "LoweredApplicationName", applicationName.ToLower ());
			AddParameter (parms, "LoweredUserName", username.ToLower ());
			DbCommand cmdSelect = dbHelper.GetCommand ("Membership_ResetPasswordSelect",
								   connection, parms, ResetPasswordSelectQueryBuilder);

			string dbUserId = null;
			string dbPassword = null;
			string dbPasswordAns = null;
			bool dbLockedOut = false;
			DateTime dbLastLockoutDate;
			int dbFailedPasswordAnswerAttemptCount = 0;
			DateTime dbFailedPasswordAnswerAttemptWindowStart;

			using (DbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ()) {
					dbUserId = reader.GetString (0);
					dbPassword = reader.GetString (1);
					dbPasswordAns = reader.GetString (2);
					dbLockedOut = reader.GetInt32 (3) > 0;
					dbLastLockoutDate = reader.GetDateTime (4);
					dbFailedPasswordAnswerAttemptCount = reader.GetInt32 (5);
					dbFailedPasswordAnswerAttemptWindowStart = reader.GetDateTime (6);
				}
				else {
					return 1; // user not found
				}
			}

			if (dbLockedOut)
				return 2; // locked out

			if (dbPasswordAns != passwordAnswer) {
				if (currentTimeUtc > dbFailedPasswordAnswerAttemptWindowStart.AddMinutes (passwordAttemptWindow)) {
					dbFailedPasswordAnswerAttemptWindowStart = currentTimeUtc;
					dbFailedPasswordAnswerAttemptCount = 1;
				}
				else {
					dbFailedPasswordAnswerAttemptWindowStart = currentTimeUtc;
					dbFailedPasswordAnswerAttemptCount++;
				}

				if (dbFailedPasswordAnswerAttemptCount > maxInvalidPasswordAttempts) {
					dbLockedOut = true;
					dbLastLockoutDate = currentTimeUtc;
				}
				return 3; // passwrod answer is wrong
			}
			else {
				dbFailedPasswordAnswerAttemptCount = 0;
				dbFailedPasswordAnswerAttemptWindowStart = DefaultDateTime;
			}

			return Membership_SetPasswordUserId (connection, dbUserId, newPassword, passwordSalt, passwordFormat, currentTimeUtc);
		}

		private int Membership_SetPassword (DbConnection connection, string applicationName,
						    string username, string newPassword, int passwordFormat,
						    string passwordSalt, DateTime currentTimeUtc)
		{
			string userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			return Membership_SetPasswordUserId (connection, userId, newPassword, passwordSalt, passwordFormat, currentTimeUtc);
		}

		private int Membership_SetPasswordUserId (DbConnection connection, string userId, string newPassword,
							  string passwordSalt, int passwordFormat, DateTime currentTimeUtc)
		{
			List <DbParameter> parms = new List <DbParameter> (5);
			AddParameter (parms, "Password", newPassword);
			AddParameter (parms, "PasswordFormat", passwordFormat);
			AddParameter (parms, "PasswordSalt", passwordSalt);
			AddParameter (parms, "LastPasswordChangedDate", currentTimeUtc);
			AddParameter (parms, "UserId", userId);
			
			DbCommand cmdUpdate = dbHelper.GetCommand ("Membership_SetPasswordUserIdUpdate",
								   connection, parms, ResetPasswordSelectQueryBuilder);
			cmdUpdate.ExecuteNonQuery ();
			return 0;
		}

		private int Membership_UnlockUser (DbConnection connection, string applicationName, string username)
		{
			string userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			List <DbParameter> parms = new List <DbParameter> (4);
			AddParameter (parms, "FailedPwdAttemptWindowStart", DefaultDateTime);
			AddParameter (parms, "FailedPwdAnswerAttWindowStart", DefaultDateTime);
			AddParameter (parms, "LastLockoutDate", DefaultDateTime);
			AddParameter (parms, "UserId", userId);
			DbCommand cmdUnlock = dbHelper.GetCommand ("Membership_UnlockUserUpdate",
								   connection, parms, UnlockUserUpdateQueryBuilder);

			cmdUnlock.ExecuteNonQuery ();
			return 0;
		}

		private int Membership_UpdateUser (DbConnection connection, string applicationName, string username,
						   string email, string comment, bool isApproved, bool uniqueEmail,
						   DateTime lastLoginDate, DateTime lastActivityDate, DateTime currentTimeUtc)
		{
			string userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found
			string applicationId = (string) dbHelper.CreateApplication (connection, applicationName);
			List <DbParameter> parms = new List <DbParameter> (3);
			if (uniqueEmail) {
				AddParameter (parms, "ApplicationId", applicationId);
				AddParameter (parms, "UserId", userId);
				AddParameter (parms, "LoweredEmail", email.ToLower ());
				DbCommand cmdUniqueMail = dbHelper.GetCommand ("Membership_UniqueMailSelect",
									       connection, parms, UniqueMailSelectQueryBuilder);
				
				using (DbDataReader reader = cmdUniqueMail.ExecuteReader ()) {
					if (reader.Read ())
						return 2; // duplicate email
				}
			}
			parms.Clear ();
			AddParameter (parms, "LastActivityDate", lastActivityDate);
			AddParameter (parms, "UserId", userId);
			DbCommand cmdUpdateUser = dbHelper.GetCommand ("Membership_UpdateUserQuery",
								       connection, parms, UpdateUserUpdateUserQueryBuilder);
			cmdUpdateUser.ExecuteNonQuery ();

			parms.Clear ();
			AddParameter (parms, "Email", email);
			AddParameter (parms, "LoweredEmail", email.ToLower ());
			AddParameter (parms, "Comment", comment);
			AddParameter (parms, "IsApproved", isApproved);
			AddParameter (parms, "LastLoginDate", lastLoginDate);
			AddParameter (parms, "UserId", userId);
			DbCommand cmdUpdateMember = dbHelper.GetCommand ("Membership_UpdateUserUpdateMemberQuery",
									 connection, parms, UpdateUserUpdateMemberQueryBuilder);
			cmdUpdateMember.ExecuteNonQuery ();

			return 0;
		}

		private int Membership_UpdateUserInfo (DbConnection connection, string applicationName, string username,
						       bool isPasswordCorrect, bool updateLastLoginActivityDate,
						       int maxInvalidPasswordAttempts, int passwordAttemptWindow,
						       DateTime currentTimeUtc, DateTime lastLoginDate, DateTime lastActivityDate)
		{
			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "LoweredApplicationName", applicationName.ToLower ());
			AddParameter (parms, "LoweredUserName", username.ToLower ());
			DbCommand cmdSelect = dbHelper.GetCommand ("Membership_UpdateUserInfoSelect",
								   connection, parms, UpdateUserInfoSelectQueryBuilder);

			string dbUserId = string.Empty;
			bool dbIsApproved = false;
			bool dbLockedOut = false;
			DateTime dbLastLockoutDate;
			int dbFailedPasswordAttemptCount = 0;
			DateTime dbFailedPasswordAttemptWindowStart;

			using (DbDataReader reader = cmdSelect.ExecuteReader ()) {
				if (reader.Read ()) {
					dbUserId = reader.GetString (0);
					dbIsApproved = reader.GetInt32 (1) > 0;
					dbLockedOut = reader.GetInt32 (2) > 0;
					dbLastLockoutDate = reader.GetDateTime (3);
					dbFailedPasswordAttemptCount = reader.GetInt32 (4);
					dbFailedPasswordAttemptWindowStart = reader.GetDateTime (5);
				}
				else {
					return 1; // user not found
				}
			}

			if (dbLockedOut)
				return 2; // locked out

			if (!isPasswordCorrect) {
				if (currentTimeUtc > dbFailedPasswordAttemptWindowStart.AddMinutes (passwordAttemptWindow)) {
					dbFailedPasswordAttemptWindowStart = currentTimeUtc;
					dbFailedPasswordAttemptCount = 1;
				}
				else {
					dbFailedPasswordAttemptWindowStart = currentTimeUtc;
					dbFailedPasswordAttemptCount++;
				}

				if (dbFailedPasswordAttemptCount > maxInvalidPasswordAttempts) {
					dbLockedOut = true;
					dbLastLockoutDate = currentTimeUtc;
				}
			}
			else {
				dbFailedPasswordAttemptCount = 0;
				dbFailedPasswordAttemptWindowStart = DefaultDateTime;
			}

			if (updateLastLoginActivityDate) {
				parms.Clear ();
				AddParameter (parms, "LastActivityDate", currentTimeUtc);
				AddParameter (parms, "UserId", dbUserId);
				DbCommand cmdUpdUserActivity = dbHelper.GetCommand ("Membership_CreateUserUpdateActivity",
										    connection, parms, CreateUserUpdateActivityQueryBuilder);
				cmdUpdUserActivity.ExecuteNonQuery ();

				parms.Clear ();
				AddParameter (parms, "LastLoginDate", currentTimeUtc);
				AddParameter (parms, "UserId", dbUserId);
				DbCommand cmdUpdMemberActivity = dbHelper.GetCommand ("Membership_UpdateUserInfoUpdateMemberActivity",
										      connection, parms,
										      UpdateUserInfoUpdateMemberActivityQueryBuilder);
				cmdUpdMemberActivity.ExecuteNonQuery ();
			}

			parms.Clear ();
			AddParameter (parms, "IsLockedOut", dbLockedOut);
			AddParameter (parms, "LastLockoutDate", dbLastLockoutDate);
			AddParameter (parms, "FailedPasswordAttemptCount", dbFailedPasswordAttemptCount);
			AddParameter (parms, "FailedPwdAttemptWindowStart", dbFailedPasswordAttemptWindowStart);
			AddParameter (parms, "UserId", dbUserId);
			DbCommand cmdUpdate = dbHelper.GetCommand ("Membership_UpdateUserInfoUpdateMember",
								   connection, parms, ResetPasswordSelectQueryBuilder);
			cmdUpdate.ExecuteNonQuery ();
			return 0;
		}

		private int Users_CreateUser (DbConnection connection, DbTransaction trans, string applicationId,
					      string username, bool isAnonymous, DateTime lastActivityDate,
					      ref object userId)
		{
			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "LoweredUserName", username.ToLower ());
			AddParameter (parms, "ApplicationId", applicationId);
			DbCommand selectCmd = dbHelper.GetCommand ("Membership_CreateUserSelectQuery",
								   connection, trans, parms, UsersCreateUserSelectQueryBuilder);

			string existingUserId = null;
			using (DbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					existingUserId = reader.GetString (0);
			}

			if (existingUserId != null && existingUserId.Length > 0) {
				userId = existingUserId;
				return 1; // user with such username and appid already exists
			}

			if (userId != null) {
				parms.Clear ();
				AddParameter (parms, "UserId", userId);
				DbCommand cmdSelectUserId = dbHelper.GetCommand ("Membership_UsersCreateUserGetUserId",
										 connection, trans, parms,
										 UsersCreateUserGetUserIdSelectQueryBuilder);

				using (DbDataReader reader = cmdSelectUserId.ExecuteReader ()) {
					if (reader.Read ())
						return 2; // user with such userId already exists
				}
			}

			if (userId == null)
				userId = Guid.NewGuid ().ToString ();

			parms.Clear ();
			AddParameter (parms, "ApplicationId", applicationId);
			AddParameter (parms, "UserId", userId);
			AddParameter (parms, "UserName", username);
			AddParameter (parms, "LoweredUserName", username.ToLower ());
			AddParameter (parms, "IsAnonymous", isAnonymous);
			AddParameter (parms, "LastActivityDate", lastActivityDate);
			DbCommand insertCmd = dbHelper.GetCommand ("Membership_UsersCreateUserInsertUser",
								   connection, trans, parms, UsersCreateUserInsertUserQueryBuilder);

			insertCmd.ExecuteNonQuery ();
			return 0;
		}

		private int Users_DeleteUser (DbConnection connection, string applicationName, string username,
					      int tablesToDeleteFrom, ref int numTablesDeletedFrom)
		{
			string userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			numTablesDeletedFrom = 0;
			DbTransaction trans = (DbTransaction) connection.BeginTransaction ();
			List <DbParameter> parms = new List <DbParameter> ();
			
			try {
				if ((tablesToDeleteFrom & 1) == 1) {
					AddParameter (parms, "UserId", userId);
					DbCommand cmdDelete = dbHelper.GetCommand ("Membership_UsersDeleteMember",
										   connection, trans, parms, UsersDeleteMemberQueryBuilder);
					cmdDelete.ExecuteNonQuery ();
					numTablesDeletedFrom++;
				}

				if ((tablesToDeleteFrom & 2) == 2) {
					parms.Clear ();
					AddParameter (parms, "UserId", userId);
					DbCommand cmdDelete = dbHelper.GetCommand ("Membership_UsersDeleteRole",
										   connection, trans, parms, UsersDeleteRoleQueryBuilder);
					cmdDelete.ExecuteNonQuery ();
					numTablesDeletedFrom++;
				}

				if ((tablesToDeleteFrom & 4) == 4) {
					parms.Clear ();
					AddParameter (parms, "UserId", userId);
					DbCommand cmdDelete = dbHelper.GetCommand ("Membership_UsersDeleteProfile",
										   connection, trans, parms, UsersDeleteProfileQueryBuilder);
					cmdDelete.ExecuteNonQuery ();
					numTablesDeletedFrom++;
				}

				if ((tablesToDeleteFrom & 15) == 15) {
					string queryDelete = "DELETE FROM aspnet_Users WHERE UserId = ?";
					parms.Clear ();
					AddParameter (parms, "UserId", userId);
					DbCommand cmdDelete = dbHelper.GetCommand ("Membership_UsersDeleteUser",
										   connection, trans, parms, UsersDeleteUserQueryBuilder);
					cmdDelete.ExecuteNonQuery ();
					numTablesDeletedFrom++;
				}

				trans.Commit ();
			} catch (Exception e) {
				trans.Rollback ();
				throw e;
			}

			return 0;
		}

		private string GetUserId (DbConnection connection, string applicationName, string username)
		{
			List <DbParameter> parms = new List <DbParameter> (2);
			AddParameter (parms, "LoweredUserName", username.ToLower ());
			AddParameter (parms, "PasswordAnswer", applicationName.ToLower ());
			DbCommand selectCmd = dbHelper.GetCommand ("Membership_GetUserId",
								   connection, parms, GetUserIdQueryBuilder);

			using (DbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetString (0);
			}

			return null; // user not found
		}
	}
}
#endif