//
// Mainsoft.Web.Security.DerbyMembershipHelper
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
	static class DerbyMembershipHelper
	{
		static readonly DateTime DefaultDateTime = new DateTime (1754, 1, 1).ToUniversalTime ();

		private static OleDbParameter AddParameter (OleDbCommand command, string paramName, object paramValue)
		{
			OleDbParameter prm = new OleDbParameter (paramName, paramValue);
			command.Parameters.Add (prm);
			return prm;
		}

		public static int Membership_ChangePasswordQuestionAndAnswer (DbConnection connection, string applicationName, string username, string newPwdQuestion, string newPwdAnswer)
		{
			string updateQuery = "UPDATE aspnet_Membership SET PasswordQuestion = ?, PasswordAnswer = ? WHERE UserId = ?";

			string userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			OleDbCommand updateCmd = new OleDbCommand (updateQuery, (OleDbConnection) connection);
			AddParameter (updateCmd, "PasswordQuestion", newPwdQuestion);
			AddParameter (updateCmd, "PasswordAnswer", newPwdAnswer);
			AddParameter (updateCmd, "UserId", userId);
			updateCmd.ExecuteNonQuery ();
			return 0;
		}

		public static int Membership_CreateUser (DbConnection connection, string applicationName, string username, string password, string passwordSalt, string email, string pwdQuestion, string pwdAnswer, bool isApproved, DateTime currentTimeUtc, DateTime createDate, bool uniqueEmail, int passwordFormat, ref object userId)
		{
			string applicationId = (string) DerbyApplicationsHelper.Applications_CreateApplication (connection, applicationName);
			string newUserId = (string) userId;

			OleDbTransaction trans = (OleDbTransaction) connection.BeginTransaction ();

			try {
				int returnValue = Users_CreateUser (connection, trans, applicationId, username, false, createDate, ref userId);
				if (returnValue == 1) {
					// the user exists in users table, this can occure when user
					// does not have membership information, but has other information
					// like roles, etc.
					if (userId != null && newUserId != null && newUserId != (string) userId) {
						trans.Rollback ();
						return 9; // wrong userid provided
					}
				}
				else if (returnValue == 2) {
					// another user with provided id already exists
					trans.Rollback ();
					return 10; // wrong userid provided
				}
				newUserId = (string) userId;

				string selectQueryMbrUserId = "SELECT UserId FROM aspnet_Membership WHERE UserId = ?";
				OleDbCommand selectCmdMbrUserId = new OleDbCommand (selectQueryMbrUserId, (OleDbConnection) connection);
				selectCmdMbrUserId.Transaction = trans;
				AddParameter (selectCmdMbrUserId, "UserId", newUserId);
				using (OleDbDataReader reader = selectCmdMbrUserId.ExecuteReader ()) {
					if (reader.Read ()) {
						trans.Rollback ();
						return 2; // user with such userId already exists
					}
				}

				if (uniqueEmail) {
					string queryMbrEmail = "SELECT * FROM  aspnet_Membership WHERE ApplicationId = ? AND LoweredEmail = ?";
					OleDbCommand cmdMbrEmail = new OleDbCommand (queryMbrEmail, (OleDbConnection) connection);
					cmdMbrEmail.Transaction = trans;
					AddParameter (cmdMbrEmail, "ApplicationId", applicationId);
					AddParameter (cmdMbrEmail, "LoweredEmail", email.ToLowerInvariant ());
					using (OleDbDataReader reader = cmdMbrEmail.ExecuteReader ()) {
						if (reader.Read ()) {
							trans.Rollback ();
							return 3; // user with such email already exists
						}
					}
				}

				if (returnValue == 1) {
					// if user was not created, but found existing and correct
					// update it's activity (membership create) time.
					string queryUpdActivity = "UPDATE aspnet_Users SET LastActivityDate = ? WHERE UserId = ?";
					OleDbCommand cmdUpdActivity = new OleDbCommand (queryUpdActivity, (OleDbConnection) connection);
					cmdUpdActivity.Transaction = trans;
					AddParameter (cmdUpdActivity, "LastActivityDate", createDate);
					AddParameter (cmdUpdActivity, "UserId", newUserId);
					cmdUpdActivity.ExecuteNonQuery ();
				}

				string queryInsertMbr = "INSERT INTO aspnet_Membership (ApplicationId, UserId, Password, PasswordFormat, PasswordSalt, Email, " +
					"LoweredEmail, PasswordQuestion, PasswordAnswer, IsApproved, IsLockedOut, CreateDate, LastLoginDate, " +
					"LastPasswordChangedDate, LastLockoutDate, FailedPasswordAttemptCount, FailedPwdAttemptWindowStart, " +
					"FailedPwdAnswerAttemptCount, FailedPwdAnswerAttWindowStart) " +
					"VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
				OleDbCommand cmdInsertMbr = new OleDbCommand (queryInsertMbr, (OleDbConnection) connection);
				cmdInsertMbr.Transaction = trans;
				AddParameter (cmdInsertMbr, "ApplicationId", applicationId);
				AddParameter (cmdInsertMbr, "UserId", newUserId);
				AddParameter (cmdInsertMbr, "Password", password);
				AddParameter (cmdInsertMbr, "PasswordFormat", passwordFormat);
				AddParameter (cmdInsertMbr, "PasswordSalt", passwordSalt);
				AddParameter (cmdInsertMbr, "Email", email);
				AddParameter (cmdInsertMbr, "LoweredEmail", email != null ? email.ToLowerInvariant () : null);
				AddParameter (cmdInsertMbr, "PasswordQuestion", pwdQuestion);
				AddParameter (cmdInsertMbr, "PasswordAnswer", pwdAnswer);
				AddParameter (cmdInsertMbr, "IsApproved", isApproved);
				AddParameter (cmdInsertMbr, "IsLockedOut", 0);
				AddParameter (cmdInsertMbr, "CreateDate", createDate);
				AddParameter (cmdInsertMbr, "LastLoginDate", DefaultDateTime);
				AddParameter (cmdInsertMbr, "LastPasswordChangedDate", createDate);
				AddParameter (cmdInsertMbr, "LastLockoutDate", DefaultDateTime);
				AddParameter (cmdInsertMbr, "FailedPasswordAttemptCount", 0);
				AddParameter (cmdInsertMbr, "FailedPwdAttemptWindowStart", DefaultDateTime);
				AddParameter (cmdInsertMbr, "FailedPwdAnswerAttemptCount", 0);
				AddParameter (cmdInsertMbr, "FailedPwdAnswerAttWindowStart", DefaultDateTime);
				cmdInsertMbr.ExecuteNonQuery ();

				trans.Commit ();
			}
			catch (Exception e) {
				trans.Rollback ();
				throw e;
			}

			return 0;
		}

		public static int Membership_FindUsersByEmail (DbConnection connection, string applicationName, string emailToMatch, int pageIndex, int pageSize, out DbDataReader reader)
		{
			string querySelect = "SELECT usr.UserName, mbr.UserId, mbr.Email, mbr.PasswordQuestion, mbr.Comment, mbr.IsApproved, " +
				"mbr.IsLockedOut, mbr.CreateDate, mbr.LastLoginDate, usr.LastActivityDate, mbr.LastPasswordChangedDate, mbr.LastLockoutDate " +
				"FROM aspnet_Membership mbr, aspnet_Users usr " +
				"WHERE usr.UserId = mbr.UserId AND mbr.LoweredEmail LIKE ? ORDER BY usr.LoweredUserName";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "LoweredEmail", emailToMatch.ToLowerInvariant ());
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		public static int Membership_FindUsersByName (DbConnection connection, string applicationName, string userNameToMatch, int pageIndex, int pageSize, out DbDataReader reader)
		{
			string querySelect = "SELECT usr.UserName, mbr.UserId, mbr.Email, mbr.PasswordQuestion, mbr.Comment, mbr.IsApproved, " +
				"mbr.IsLockedOut, mbr.CreateDate, mbr.LastLoginDate, usr.LastActivityDate, mbr.LastPasswordChangedDate, mbr.LastLockoutDate " +
				"FROM aspnet_Membership mbr, aspnet_Users usr " +
				"WHERE usr.UserId = mbr.UserId AND usr.LoweredUserName LIKE ? ORDER BY usr.LoweredUserName";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "LoweredUserName", userNameToMatch.ToLowerInvariant ());
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		public static int Membership_GetAllUsers (DbConnection connection, string applicationName, int pageIndex, int pageSize, out DbDataReader reader)
		{
			string querySelect = "SELECT usr.UserName, mbr.UserId, mbr.Email, mbr.PasswordQuestion, mbr.Comment, mbr.IsApproved, " +
				"mbr.IsLockedOut, mbr.CreateDate, mbr.LastLoginDate, usr.LastActivityDate, mbr.LastPasswordChangedDate, mbr.LastLockoutDate " + 
				"FROM aspnet_Membership mbr, aspnet_Users usr " +
				"WHERE usr.UserId = mbr.UserId ORDER BY usr.LoweredUserName";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		public static int Membership_GetNumberOfUsersOnline (DbConnection connection, string applicationName, int minutesSinceLastInActive, DateTime currentTimeUtc)
		{
			string queryUsersActive = "SELECT COUNT(*) FROM aspnet_Users usr, aspnet_Applications app, aspnet_Membership mbr " +
				"WHERE usr.ApplicationId = app.ApplicationId AND usr.LastActivityDate > ? AND " +
				"app.LoweredApplicationName = ? AND usr.UserId = mbr.UserId";


			OleDbCommand cmdUsersActive = new OleDbCommand (queryUsersActive, (OleDbConnection) connection);
			AddParameter (cmdUsersActive, "LastActivityDate", currentTimeUtc.AddMinutes (-minutesSinceLastInActive));
			AddParameter (cmdUsersActive, "LoweredApplicationName", applicationName.ToLowerInvariant ());
			using (OleDbDataReader reader = cmdUsersActive.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetInt32 (0);
			}
			return 0;
		}

		public static int Membership_GetPassword (DbConnection connection, string applicationName, string username, string passwordAnswer, int maxInvalidPasswordAttempts, int passwordAttemptWindow, DateTime currentTimeUtc, out string password)
		{
			string querySelect = "SELECT usr.UserId, mbr.Password, mbr.PasswordAnswer, mbr.IsLockedOut, " +
				"mbr.LastLockoutDate, mbr.FailedPwdAnswerAttemptCount, mbr.FailedPwdAnswerAttWindowStart " +
				"FROM aspnet_Applications app, aspnet_Users usr, aspnet_Membership mbr " +
				"WHERE app.LoweredApplicationName = ? AND usr.ApplicationId = app.ApplicationId " +
				"AND usr.UserId = mbr.UserId AND usr.LoweredUserName = ?";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "LoweredApplicationName", applicationName.ToLowerInvariant ());
			AddParameter (cmdSelect, "LoweredUserName", username.ToLowerInvariant ());

			password = null;
			string dbUserId = null;
			string dbPassword = null;
			string dbPasswordAns = null;
			bool dbLockedOut = false;
			DateTime dbLastLockoutDate;
			int dbFailedPasswordAnswerAttemptCount = 0;
			DateTime dbFailedPasswordAnswerAttemptWindowStart;

			using (OleDbDataReader reader = cmdSelect.ExecuteReader ()) {
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
				return 3; // wrong password answer
			}
			else {
				dbFailedPasswordAnswerAttemptCount = 0;
				dbFailedPasswordAnswerAttemptWindowStart = DefaultDateTime;
				password = dbPassword;
			}

			string queryUpdate = "UPDATE aspnet_Membership SET IsLockedOut = ?, LastLockoutDate = ?, " +
				"FailedPwdAnswerAttemptCount = ?, FailedPwdAnswerAttWindowStart = ? " +
				"WHERE UserId = ?";
			OleDbCommand cmdUpdate = new OleDbCommand (queryUpdate, (OleDbConnection) connection);
			AddParameter (cmdUpdate, "IsLockedOut", dbLockedOut);
			AddParameter (cmdUpdate, "LastLockoutDate", dbLastLockoutDate);
			AddParameter (cmdUpdate, "FailedPwdAnswerAttemptCount", dbFailedPasswordAnswerAttemptCount);
			AddParameter (cmdUpdate, "FailedPwdAnswerAttWindowStart", dbFailedPasswordAnswerAttemptWindowStart);
			AddParameter (cmdUpdate, "UserId", dbUserId);
			cmdUpdate.ExecuteNonQuery ();
			
			return 0;
		}

		public static int Membership_GetPasswordWithFormat (DbConnection connection, string applicationName, string username, bool updateLastActivity, DateTime currentTimeUtc, out DbDataReader reader)
		{
			string querySelect = "SELECT usr.UserId, mbr.IsLockedOut, mbr.IsApproved, mbr.Password, mbr.PasswordFormat, mbr.PasswordSalt, " +
				"mbr.FailedPasswordAttemptCount, mbr.FailedPwdAnswerAttemptCount, mbr.LastLoginDate, usr.LastActivityDate " +
				"FROM aspnet_Applications app, aspnet_Users usr, aspnet_Membership mbr " +
				"WHERE app.LoweredApplicationName = ? AND usr.ApplicationId = app.ApplicationId " +
				"AND usr.UserId = mbr.UserId AND usr.LoweredUserName = ?";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "LoweredApplicationName", applicationName.ToLowerInvariant ());
			AddParameter (cmdSelect, "LoweredUserName", username.ToLowerInvariant ());
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		public static int Membership_GetUserByEmail (DbConnection connection, string applicationName, string email, out string username)
		{
			string querySelect = "SELECT usr.UserName FROM aspnet_Applications app, aspnet_Users usr, aspnet_Membership mbr " +
				"WHERE app.LoweredApplicationName = ? AND usr.ApplicationId = app.ApplicationId AND " +
				"usr.UserId = mbr.UserId AND mbr.LoweredEmail " + (email == null ? "IS NULL" : "= ?");

			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "LoweredApplicationName", applicationName.ToLowerInvariant ());
			if (email != null)
				AddParameter (cmdSelect, "LoweredEmail", email.ToLowerInvariant ());

			username = null;
			using (OleDbDataReader reader = cmdSelect.ExecuteReader ()) {
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

		public static int Membership_GetUserByName (DbConnection connection, string applicationName, string username, bool updateLastActivity, DateTime currentTimeUtc, out DbDataReader reader)
		{
			reader = null;
			object userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			if (updateLastActivity) {
				string queryUpdate = "UPDATE aspnet_Users SET LastActivityDate = ? WHERE UserId = ?";
				OleDbCommand cmdUpdate = new OleDbCommand (queryUpdate, (OleDbConnection) connection);
				AddParameter (cmdUpdate, "LastActivityDate", currentTimeUtc);
				AddParameter (cmdUpdate, "UserId", userId);
				int records = cmdUpdate.ExecuteNonQuery ();
				if (records == 0)
					return -1; // unknown error
			}

			return Membership_GetUserByUserId (connection, userId, updateLastActivity, currentTimeUtc, out reader);
		}

		public static int Membership_GetUserByUserId (DbConnection connection, object userId, bool updateLastActivity, DateTime currentTimeUtc, out DbDataReader reader)
		{
			reader = null;
			if (updateLastActivity) {
				string queryUpdate = "UPDATE aspnet_Users SET LastActivityDate = ? WHERE UserId = ?";
				OleDbCommand cmdUpdate = new OleDbCommand (queryUpdate, (OleDbConnection) connection);
				AddParameter (cmdUpdate, "LastActivityDate", currentTimeUtc);
				AddParameter (cmdUpdate, "UserId", userId);
				int recordsAffected = cmdUpdate.ExecuteNonQuery ();
				if (recordsAffected == 0)
					return 1; // user not found
			}

			string querySelect = "SELECT usr.UserName, mbr.UserId, mbr.Email, mbr.PasswordQuestion, mbr.Comment, mbr.IsApproved, " + 
				"mbr.IsLockedOut, mbr.CreateDate, mbr.LastLoginDate, usr.LastActivityDate, mbr.LastPasswordChangedDate, mbr.LastLockoutDate " +
				"FROM aspnet_Users usr, aspnet_Membership mbr " +
				"WHERE usr.UserId = ? AND usr.UserId = mbr.UserId";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "UserId", userId);
			reader = cmdSelect.ExecuteReader ();
			return 0;
		}

		public static int Membership_ResetPassword (DbConnection connection, string applicationName, string username, string newPassword, string passwordAnswer, int passwordFormat, string passwordSalt, int maxInvalidPasswordAttempts, int passwordAttemptWindow, DateTime currentTimeUtc)
		{
			string querySelect = "SELECT usr.UserId, mbr.Password, mbr.PasswordAnswer, mbr.IsLockedOut, " +
				"mbr.LastLockoutDate, mbr.FailedPwdAnswerAttemptCount, mbr.FailedPwdAnswerAttWindowStart " +
				"FROM aspnet_Applications app, aspnet_Users usr, aspnet_Membership mbr " +
				"WHERE app.LoweredApplicationName = ? AND usr.ApplicationId = app.ApplicationId " +
				"AND usr.UserId = mbr.UserId AND usr.LoweredUserName = ?";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "LoweredApplicationName", applicationName.ToLowerInvariant ());
			AddParameter (cmdSelect, "LoweredUserName", username.ToLowerInvariant ());

			string dbUserId = null;
			string dbPassword = null;
			string dbPasswordAns = null;
			bool dbLockedOut = false;
			DateTime dbLastLockoutDate;
			int dbFailedPasswordAnswerAttemptCount = 0;
			DateTime dbFailedPasswordAnswerAttemptWindowStart;

			using (OleDbDataReader reader = cmdSelect.ExecuteReader ()) {
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

		public static int Membership_SetPassword (DbConnection connection, string applicationName, string username, string newPassword, int passwordFormat, string passwordSalt, DateTime currentTimeUtc)
		{
			string userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			return Membership_SetPasswordUserId (connection, userId, newPassword, passwordSalt, passwordFormat, currentTimeUtc);
		}

		private static int Membership_SetPasswordUserId (DbConnection connection, string userId, string newPassword, string passwordSalt, int passwordFormat, DateTime currentTimeUtc)
		{
			string queryUpdate = "UPDATE aspnet_Membership SET Password = ?, PasswordFormat = ?, PasswordSalt = ?, " +
				"LastPasswordChangedDate = ? WHERE UserId = ?";
			OleDbCommand cmdUpdate = new OleDbCommand (queryUpdate, (OleDbConnection) connection);
			AddParameter (cmdUpdate, "Password", newPassword);
			AddParameter (cmdUpdate, "PasswordFormat", passwordFormat);
			AddParameter (cmdUpdate, "PasswordSalt", passwordSalt);
			AddParameter (cmdUpdate, "LastPasswordChangedDate", currentTimeUtc);
			AddParameter (cmdUpdate, "UserId", userId);
			
			cmdUpdate.ExecuteNonQuery ();
			return 0;
		}

		public static int Membership_UnlockUser (DbConnection connection, string applicationName, string username)
		{
			string userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			string queryUnlock = "UPDATE aspnet_Membership SET IsLockedOut = 0, " +
				"FailedPasswordAttemptCount = 0, FailedPwdAttemptWindowStart = ?, " +
				"FailedPwdAnswerAttemptCount = 0, FailedPwdAnswerAttWindowStart = ?, " +
				"LastLockoutDate = ? WHERE UserId = ?";
			OleDbCommand cmdUnlock = new OleDbCommand (queryUnlock, (OleDbConnection) connection);
			AddParameter (cmdUnlock, "FailedPwdAttemptWindowStart", DefaultDateTime);
			AddParameter (cmdUnlock, "FailedPwdAnswerAttWindowStart", DefaultDateTime);
			AddParameter (cmdUnlock, "LastLockoutDate", DefaultDateTime);
			AddParameter (cmdUnlock, "UserId", userId);

			cmdUnlock.ExecuteNonQuery ();
			return 0;
		}

		public static int Membership_UpdateUser (DbConnection connection, string applicationName, string username, string email, string comment, bool isApproved, bool uniqueEmail, DateTime lastLoginDate, DateTime lastActivityDate, DateTime currentTimeUtc)
		{
			string userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			if (uniqueEmail) {
				string queryUniqueMail = "SELECT * FROM aspnet_Membership WHERE ApplicationId = ? " +
					"AND UserId <> ? AND LoweredEmail = ?";
				OleDbCommand cmdUniqueMail = new OleDbCommand (queryUniqueMail, (OleDbConnection) connection);
				AddParameter (cmdUniqueMail, "ApplicationId", email);
				AddParameter (cmdUniqueMail, "UserId", userId);
				using (OleDbDataReader reader = cmdUniqueMail.ExecuteReader ()) {
					if (reader.Read ())
						return 2; // duplicate email
				}
			}
			string queryUpdateUser = "UPDATE aspnet_Users SET LastActivityDate = ? WHERE UserId = ?";
			OleDbCommand cmdUpdateUser = new OleDbCommand (queryUpdateUser, (OleDbConnection) connection);
			AddParameter (cmdUpdateUser, "LastActivityDate", lastActivityDate);
			AddParameter (cmdUpdateUser, "UserId", userId);
			cmdUpdateUser.ExecuteNonQuery ();

			string queryUpdateMember = "UPDATE aspnet_Membership SET Email = ?, LoweredEmail = ?, Comment = ?, " +
				"IsApproved = ?, LastLoginDate = ? WHERE UserId = ?";
			OleDbCommand cmdUpdateMember = new OleDbCommand (queryUpdateMember, (OleDbConnection) connection);
			AddParameter (cmdUpdateMember, "Email", email);
			AddParameter (cmdUpdateMember, "LoweredEmail", email.ToLowerInvariant ());
			AddParameter (cmdUpdateMember, "Comment", comment);
			AddParameter (cmdUpdateMember, "IsApproved", isApproved);
			AddParameter (cmdUpdateMember, "LastLoginDate", lastLoginDate);
			AddParameter (cmdUpdateMember, "UserId", userId);
			cmdUpdateMember.ExecuteNonQuery ();

			return 0;
		}

		public static int Membership_UpdateUserInfo (DbConnection connection, string applicationName, string username, bool isPasswordCorrect, bool updateLastLoginActivityDate, int maxInvalidPasswordAttempts, int passwordAttemptWindow, DateTime currentTimeUtc, DateTime lastLoginDate, DateTime lastActivityDate)
		{
			string querySelect = "SELECT usr.UserId, mbr.IsApproved, mbr.IsLockedOut, mbr.LastLockoutDate, " +
							"mbr.FailedPasswordAttemptCount, mbr.FailedPwdAttemptWindowStart " +
							"FROM aspnet_Applications app, aspnet_Users usr, aspnet_Membership mbr " +
							"WHERE app.LoweredApplicationName = ? AND usr.ApplicationId = app.ApplicationId " +
							"AND usr.UserId = mbr.UserId AND usr.LoweredUserName = ?";
			OleDbCommand cmdSelect = new OleDbCommand (querySelect, (OleDbConnection) connection);
			AddParameter (cmdSelect, "LoweredApplicationName", applicationName.ToLowerInvariant ());
			AddParameter (cmdSelect, "LoweredUserName", username.ToLowerInvariant ());

			string dbUserId = string.Empty;
			bool dbIsApproved = false;
			bool dbLockedOut = false;
			DateTime dbLastLockoutDate;
			int dbFailedPasswordAttemptCount = 0;
			DateTime dbFailedPasswordAttemptWindowStart;

			using (OleDbDataReader reader = cmdSelect.ExecuteReader ()) {
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
				string queryUpdUserActivity = "UPDATE aspnet_Users SET LastActivityDate = ? WHERE UserId = ?";
				OleDbCommand cmdUpdUserActivity = new OleDbCommand (queryUpdUserActivity, (OleDbConnection) connection);
				AddParameter (cmdUpdUserActivity, "LastActivityDate", currentTimeUtc);
				AddParameter (cmdUpdUserActivity, "UserId", dbUserId);
				cmdUpdUserActivity.ExecuteNonQuery ();

				string queryUpdMemberActivity = "UPDATE aspnet_Membership SET LastLoginDate = ? WHERE UserId = ?";
				OleDbCommand cmdUpdMemberActivity = new OleDbCommand (queryUpdMemberActivity, (OleDbConnection) connection);
				AddParameter (cmdUpdMemberActivity, "LastLoginDate", currentTimeUtc);
				AddParameter (cmdUpdMemberActivity, "UserId", dbUserId);
				cmdUpdMemberActivity.ExecuteNonQuery ();
			}

			string queryUpdate = "UPDATE aspnet_Membership SET IsLockedOut = ?, LastLockoutDate = ?, " +
				"FailedPasswordAttemptCount = ?, FailedPwdAttemptWindowStart = ? " +
				"WHERE UserId = ?";
			OleDbCommand cmdUpdate = new OleDbCommand (queryUpdate, (OleDbConnection) connection);
			AddParameter (cmdUpdate, "IsLockedOut", dbLockedOut);
			AddParameter (cmdUpdate, "LastLockoutDate", dbLastLockoutDate);
			AddParameter (cmdUpdate, "FailedPasswordAttemptCount", dbFailedPasswordAttemptCount);
			AddParameter (cmdUpdate, "FailedPwdAttemptWindowStart", dbFailedPasswordAttemptWindowStart);
			AddParameter (cmdUpdate, "UserId", dbUserId);
			cmdUpdate.ExecuteNonQuery ();
			return 0;
		}

		public static int Users_CreateUser (DbConnection connection, DbTransaction trans, string applicationId, string username, bool isAnonymous, DateTime lastActivityDate, ref object userId)
		{
			string selectQuery = "SELECT UserId FROM aspnet_Users WHERE LoweredUserName = ? AND ApplicationId = ?";
			OleDbCommand selectCmd = new OleDbCommand (selectQuery, (OleDbConnection) connection);
			AddParameter (selectCmd, "LoweredUserName", username.ToLowerInvariant ());
			AddParameter (selectCmd, "ApplicationId", applicationId);
			if (trans != null)
				selectCmd.Transaction = (OleDbTransaction) trans;

			string existingUserId = null;
			using (OleDbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					existingUserId = reader.GetString (0);
			}

			if (existingUserId != null && existingUserId.Length > 0) {
				userId = existingUserId;
				return 1; // user with such username and appid already exists
			}

			if (userId != null) {
				string querySelectUserId = "SELECT UserId FROM aspnet_Users WHERE UserId = ?";
				OleDbCommand cmdSelectUserId = new OleDbCommand (querySelectUserId, (OleDbConnection) connection);
				AddParameter (cmdSelectUserId, "UserId", userId);
				if (trans != null)
					cmdSelectUserId.Transaction = (OleDbTransaction) trans;

				using (OleDbDataReader reader = cmdSelectUserId.ExecuteReader ()) {
					if (reader.Read ())
						return 2; // user with such userId already exists
				}
			}

			if (userId == null)
				userId = Guid.NewGuid ().ToString ();

			string insertQuery = "INSERT INTO aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, IsAnonymous, LastActivityDate) VALUES (?, ?, ?, ?, ?, ?)";
			OleDbCommand insertCmd = new OleDbCommand (insertQuery, (OleDbConnection) connection);
			AddParameter (insertCmd, "ApplicationId", applicationId);
			AddParameter (insertCmd, "UserId", userId);
			AddParameter (insertCmd, "UserName", username);
			AddParameter (insertCmd, "LoweredUserName", username.ToLowerInvariant ());
			AddParameter (insertCmd, "IsAnonymous", isAnonymous);
			AddParameter (insertCmd, "LastActivityDate", lastActivityDate);
			if (trans != null)
				insertCmd.Transaction = (OleDbTransaction) trans;

			insertCmd.ExecuteNonQuery ();
			return 0;
		}

		public static int Users_DeleteUser (DbConnection connection, string applicationName, string username, int tablesToDeleteFrom, ref int numTablesDeletedFrom)
		{
			string userId = GetUserId (connection, applicationName, username);
			if (userId == null)
				return 1; // user not found

			numTablesDeletedFrom = 0;
			OleDbTransaction trans = (OleDbTransaction) connection.BeginTransaction ();

			try {
				if ((tablesToDeleteFrom & 1) == 1) {
					string queryDelete = "DELETE FROM aspnet_Membership WHERE UserId = ?";
					OleDbCommand cmdDelete = new OleDbCommand (queryDelete, (OleDbConnection) connection);
					AddParameter (cmdDelete, "UserId", userId);
					cmdDelete.Transaction = trans;
					cmdDelete.ExecuteNonQuery ();

					numTablesDeletedFrom++;
				}

				if ((tablesToDeleteFrom & 2) == 2) {
					string queryDelete = "DELETE FROM aspnet_UsersInRoles WHERE UserId = ?";
					OleDbCommand cmdDelete = new OleDbCommand (queryDelete, (OleDbConnection) connection);
					AddParameter (cmdDelete, "UserId", userId);
					cmdDelete.Transaction = trans;
					cmdDelete.ExecuteNonQuery ();

					numTablesDeletedFrom++;
				}

				if ((tablesToDeleteFrom & 4) == 4) {
					string queryDelete = "DELETE FROM aspnet_Profile WHERE UserId = ?";
					OleDbCommand cmdDelete = new OleDbCommand (queryDelete, (OleDbConnection) connection);
					AddParameter (cmdDelete, "UserId", userId);
					cmdDelete.Transaction = trans;
					cmdDelete.ExecuteNonQuery ();

					numTablesDeletedFrom++;
				}

				// this table was removed  from schema
				//if ((tablesToDeleteFrom & 8) == 8) {
				//    string queryDelete = "DELETE FROM aspnet_PersonalizationPerUser WHERE UserId = ?";
				//    OleDbCommand cmdDelete = new OleDbCommand (queryDelete, (OleDbConnection) connection);
				//    AddParameter (cmdDelete, "UserId", userId);
				//    cmdDelete.Transaction = trans;
				//    cmdDelete.ExecuteNonQuery ();

				//    numTablesDeletedFrom++;
				//}

				if ((tablesToDeleteFrom & 15) == 15) {
					string queryDelete = "DELETE FROM aspnet_Users WHERE UserId = ?";
					OleDbCommand cmdDelete = new OleDbCommand (queryDelete, (OleDbConnection) connection);
					AddParameter (cmdDelete, "UserId", userId);
					cmdDelete.Transaction = trans;
					cmdDelete.ExecuteNonQuery ();

					numTablesDeletedFrom++;
				}

				trans.Commit ();
			}
			catch (Exception e) {
				trans.Rollback ();
				throw e;
			}

			return 0;
		}

		private static string GetUserId (DbConnection connection, string applicationName, string username)
		{
			string selectQuery = "SELECT usr.UserId FROM aspnet_Membership mbr, aspnet_Users usr, aspnet_Applications app WHERE " +
				"usr.LoweredUserName = ? AND app.LoweredApplicationName = ? " +
				"AND usr.ApplicationId = app.ApplicationId " +
				"AND usr.UserId = mbr.UserId";

			OleDbCommand selectCmd = new OleDbCommand (selectQuery, (OleDbConnection) connection);
			AddParameter (selectCmd, "LoweredUserName", username.ToLowerInvariant ());
			AddParameter (selectCmd, "PasswordAnswer", applicationName.ToLowerInvariant ());

			using (OleDbDataReader reader = selectCmd.ExecuteReader ()) {
				if (reader.Read ())
					return reader.GetString (0);
			}

			return null; // user not found
		}
	}
}

#endif