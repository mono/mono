//
// Mainsoft.Web.Security.GenericMembershipQueries
//
// Authors:
//      Marek Habersack (grendello@gmail.com)
//
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

using Mainsoft.Web.Configuration;

namespace Mainsoft.Web.Security
{
	public partial class GenericMembershipProvider
	{
		// Query builders
		string GetParamName (DbParameter[] parms, int i)
		{
			return dbHelper.GetParamName (parms, i);
		}
		
		string ChangePasswordQuestionAndAnswerQueryBuilder (DbParameter[] parms, object data)
		{
			if (parms == null)
				throw new ArgumentNullException ("parms");
			
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("UPDATE aspnet_Membership SET ");
			sb.AppendFormat ("PasswordQuestion = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("PasswordAnswer = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.AppendFormat ("WHERE UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 2), bpi));

			return sb.ToString ();
		}

		string CreateUserGetUserIdQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("SELECT UserId FROM aspnet_Membership WHERE ");
			sb.AppendFormat ("UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));

			return sb.ToString ();
		}

		string CreateUserGetByMailQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("SELECT * FROM aspnet_Membership WHERE ");
			sb.AppendFormat ("ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND LoweredEmail = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
			return sb.ToString ();
		}

		string CreateUserUpdateActivityQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("UPDATE aspnet_Users SET ");
			sb.AppendFormat ("LastActivityDate = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("WHERE UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
			return sb.ToString ();
		}

		string CreateUserCreateMemberQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"INSERT INTO aspnet_Membership (ApplicationId, UserId, Password, PasswordFormat, PasswordSalt, Email,
                                  LoweredEmail, PasswordQuestion, PasswordAnswer, IsApproved, IsLockedOut, CreateDate, LastLoginDate,
			  	  LastPasswordChangedDate, LastLockoutDate, FailedPasswordAttemptCount, FailedPwdAttemptWindowStart, 
				  FailedPwdAnswerAttemptCount, FailedPwdAnswerAttWindowStart) VALUES (");

			int count = parms.Length;
			for (int i = 0; i < count; i++) {
				sb.Append (dbHelper.PrepareQueryParameter (GetParamName (parms, i), bpi));
				if (i + 1 < count)
					sb.Append (",");
			}
			sb.Append (")");
			
			return sb.ToString ();
		}

		string FindUsersByEmailSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"SELECT usr.UserName, mbr.UserId, mbr.Email, mbr.PasswordQuestion, mbr.Comment, mbr.IsApproved, 
				mbr.IsLockedOut, mbr.CreateDate, mbr.LastLoginDate, usr.LastActivityDate, mbr.LastPasswordChangedDate,
                                mbr.LastLockoutDate FROM aspnet_Membership mbr, aspnet_Users usr WHERE 
				usr.UserId = mbr.UserId AND mbr.LoweredEmail LIKE ");
			
			sb.AppendFormat (dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.Append (" ORDER BY usr.LoweredUserName");
			
			return sb.ToString ();
		}

		string FindUsersByNameSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"SELECT usr.UserName, mbr.UserId, mbr.Email, mbr.PasswordQuestion, mbr.Comment, mbr.IsApproved, 
				mbr.IsLockedOut, mbr.CreateDate, mbr.LastLoginDate, usr.LastActivityDate, mbr.LastPasswordChangedDate,
                                mbr.LastLockoutDate FROM aspnet_Membership mbr, aspnet_Users usr WHERE usr.UserId = mbr.UserId AND
                                usr.LoweredUserName LIKE ");
			
			sb.AppendFormat (dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.Append (" ORDER BY usr.LoweredUserName");
			
			return sb.ToString ();
		}

		string GetAllUsersSelectQueryBuilder (DbParameter[] parms, object data)
		{
			return @"SELECT usr.UserName, mbr.UserId, mbr.Email, mbr.PasswordQuestion, mbr.Comment, mbr.IsApproved,
				mbr.IsLockedOut, mbr.CreateDate, mbr.LastLoginDate, usr.LastActivityDate, mbr.LastPasswordChangedDate,
                                mbr.LastLockoutDate FROM aspnet_Membership mbr, aspnet_Users usr WHERE usr.UserId = mbr.UserId
                                ORDER BY usr.LoweredUserName";
		}

		string GetNumberOfUsersOnlineSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"SELECT COUNT(*) FROM aspnet_Users usr, aspnet_Applications app, aspnet_Membership mbr
				  WHERE usr.ApplicationId = app.ApplicationId AND usr.LastActivityDate > ");
			
			sb.AppendFormat ("usr.LastActivityDate = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND app.LoweredApplicationName = {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.Append ("AND usr.UserId = mbr.UserId");
			
			return sb.ToString ();
		}

		string GetPasswordSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"SELECT usr.UserId, mbr.Password, mbr.PasswordAnswer, mbr.IsLockedOut,
				mbr.LastLockoutDate, mbr.FailedPwdAnswerAttemptCount, mbr.FailedPwdAnswerAttWindowStart
				FROM aspnet_Applications app, aspnet_Users usr, aspnet_Membership mbr
				WHERE usr.ApplicationId = app.ApplicationId AND usr.UserId = mbr.UserId ");
			
			sb.AppendFormat ("AND app.LoweredApplicationName = {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND usr.LoweredUserName = {0}",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
			return sb.ToString ();
		}

		string GetPasswordUpdateQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("UPDATE aspnet_Membership SET ");
			
			sb.AppendFormat ("IsLockedOut = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("LastLockoutDate = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.AppendFormat ("FailedPwdAnswerAttemptCount = {0}, ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 2), bpi));
			sb.AppendFormat ("FailedPwdAnswerAttWindowStart = {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 3), bpi));
			sb.AppendFormat ("WHERE UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 4), bpi));
			return sb.ToString ();
		}

		string GetPasswordWithFormatSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"SELECT usr.UserId, mbr.IsLockedOut, mbr.IsApproved, mbr.Password, mbr.PasswordFormat,
                                  mbr.PasswordSalt, mbr.FailedPasswordAttemptCount, mbr.FailedPwdAnswerAttemptCount, mbr.LastLoginDate,
                                  usr.LastActivityDate FROM aspnet_Applications app, aspnet_Users usr, aspnet_Membership mbr
				  WHERE usr.ApplicationId = app.ApplicationId AND usr.UserId = mbr.UserId ");
			
			sb.AppendFormat ("AND app.LoweredApplicationName = {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND usr.LoweredUserName = {0}",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
			return sb.ToString ();
		}

		string GetUserByEmailSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"SELECT usr.UserName FROM aspnet_Applications app, aspnet_Users usr, aspnet_Membership mbr
				  WHERE usr.ApplicationId = app.ApplicationId AND usr.UserId = mbr.UserId ");
			
			sb.AppendFormat ("AND app.LoweredApplicationName = {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			if (parms.Length < 2 || parms [1] == null)
				sb.Append ("AND usr.LoweredEmail IS NULL");
			else
				sb.AppendFormat ("AND usr.LoweredEmail = {0}",
						 dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
			return sb.ToString ();
		}

		string GetUserByNameUpdateQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("UPDATE aspnet_Users SET ");
			
			sb.AppendFormat ("LastActivityDate = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("WHERE UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));

			return sb.ToString ();
		}

		string GetUserByUserIdSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"SELECT usr.UserName, mbr.UserId, mbr.Email, mbr.PasswordQuestion, mbr.Comment, mbr.IsApproved, 
				  mbr.IsLockedOut, mbr.CreateDate, mbr.LastLoginDate, usr.LastActivityDate, mbr.LastPasswordChangedDate,
                                  mbr.LastLockoutDate FROM aspnet_Users usr, aspnet_Membership mbr WHERE ");
			
			sb.AppendFormat ("mbr.UserId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.Append ("AND usr.UserId = mbr.UserId");
			
			return sb.ToString ();
		}

		string ResetPasswordSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"SELECT usr.UserId, mbr.Password, mbr.PasswordAnswer, mbr.IsLockedOut,
				  mbr.LastLockoutDate, mbr.FailedPwdAnswerAttemptCount, mbr.FailedPwdAnswerAttWindowStart
				  FROM aspnet_Applications app, aspnet_Users usr, aspnet_Membership mbr WHERE ");

			sb.AppendFormat ("app.LoweredApplicationName = {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.Append ("usr.ApplicationId = app.ApplicationId AND usr.UserId = mbr.UserId ");
			sb.AppendFormat ("AND usr.LoweredUserName = {0}",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
			return sb.ToString ();
		}

		string SetPasswordUserIdUpdateQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("UPDATE aspnet_Membership SET ");

			sb.AppendFormat ("Password = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("PasswordFormat = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.AppendFormat ("PasswordSalt = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 2), bpi));
			sb.AppendFormat ("LastPasswordPasswordChangeDate = {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 3), bpi));
			sb.AppendFormat ("WHERE UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 4), bpi));
			
			return sb.ToString ();
		}

		string UnlockUserUpdateQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"UPDATE aspnet_Membership SET IsLockedOut = 0, FailedPasswordAttemptCount = 0,
                                  FailedPwdAnswerAttemptCount = 0, ");

			sb.AppendFormat ("FailedPwdAttemptWindowStart = {0}, ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("FailedPwdAnswerAttWindowStart = {0}, ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.AppendFormat ("LastLockoutDate = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 2), bpi));
			sb.AppendFormat ("WHERE UserId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 3), bpi));
			
			return sb.ToString ();
		}

		string UniqueMailSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("SELECT * FROM aspnet_Membership WHERE ");

			sb.AppendFormat ("ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND UserId <> {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.AppendFormat ("AND LoweredEmail = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 2), bpi));
			
			return sb.ToString ();
		}

		string UpdateUserUpdateUserQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("UPDATE aspnet_Users SET ");

			sb.AppendFormat ("LastActivityDate = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("WHERE UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
			return sb.ToString ();
		}

		string UpdateUserUpdateMemberQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("UPDATE aspnet_Membership SET ");

			sb.AppendFormat ("Email = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("LoweredEmail = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.AppendFormat ("Comment = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 2), bpi));
			sb.AppendFormat ("IsApproved = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 3), bpi));
			sb.AppendFormat ("LastLoginDate = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 4), bpi));
			sb.AppendFormat ("WHERE UserId = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 5), bpi));
			
			return sb.ToString ();
		}

		string UpdateUserInfoSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"SELECT usr.UserId, mbr.IsApproved, mbr.IsLockedOut, mbr.LastLockoutDate, 
 				  mbr.FailedPasswordAttemptCount, mbr.FailedPwdAttemptWindowStart
				  FROM aspnet_Applications app, aspnet_Users usr, aspnet_Membership mbr	WHERE ");

			sb.AppendFormat ("app.LoweredApplicationName = {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.Append ("AND usr.ApplicationId = app.ApplicationId AND usr.UserId = mbr.UserId ");
			sb.AppendFormat ("usr.LoweredUserName = {0}, ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
			return sb.ToString ();
		}

		string UpdateUserInfoUpdateMemberActivityQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("UPDATE aspnet_Membership SET ");
			
			sb.AppendFormat ("LastLoginDate = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("WHERE UserId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
			return sb.ToString ();
		}

		string UpdateUserInfoUpdateMemberQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("UPDATE aspnet_Membership SET ");
			
			sb.AppendFormat ("IsLockedOut = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("LastLockoutDate = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.AppendFormat ("FailedPasswordAttemptCount = {0}, ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 2), bpi));
			sb.AppendFormat ("FailedPwdAttemptWindowStart = {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 3), bpi));
			sb.AppendFormat ("WHERE UserId = {0}, ", dbHelper.PrepareQueryParameter (GetParamName (parms, 4), bpi));
			
			return sb.ToString ();
		}

		string UsersCreateUserSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("SELECT UserId FROM aspnet_Users WHERE ");
			
			sb.AppendFormat ("LoweredUserName = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
			return sb.ToString ();
		}

		string UsersCreateUserGetUserIdSelectQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("SELECT UserId FROM aspnet_Users WHERE ");
			
			sb.AppendFormat ("UserId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			
			return sb.ToString ();
		}

		string UsersCreateUserInsertUserQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				@"INSERT INTO aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName,
                                  IsAnonymous, LastActivityDate) VALUES (");

			int count = parms.Length;
			for (int i = 0; i < count; i++) {
				sb.AppendFormat (dbHelper.PrepareQueryParameter (GetParamName (parms, i), bpi));
				if (i + 1 < count)
					sb.Append (", ");
			}
			sb.Append (")");
			
			return sb.ToString ();
		}

		string UsersDeleteMemberQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("DELETE FROM aspnet_Membership WHERE ");

			sb.AppendFormat ("UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			
			return sb.ToString ();
		}

		string UsersDeleteRoleQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("DELETE FROM aspnet_UsersInRoles WHERE ");

			sb.AppendFormat ("UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			
			return sb.ToString ();
		}

		string UsersDeleteProfileQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("DELETE FROM aspnet_Profile WHERE ");

			sb.AppendFormat ("UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			
			return sb.ToString ();
		}

		string UsersDeleteUserQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("DELETE FROM aspnet_Users WHERE ");

			sb.AppendFormat ("UserId = {0}", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			
			return sb.ToString ();
		}

		string GetUserIdQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder (
				"SELECT usr.UserId FROM aspnet_Membership mbr, aspnet_Users usr, aspnet_Applications app WHERE ");

			sb.AppendFormat ("LoweredUserName = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND LoweredApplicationName = {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.Append ("AND app.ApplicationId = usr.ApplicationId AND usr.UserId = mbr.UserId");
			
			return sb.ToString ();
		}
	}
}
#endif