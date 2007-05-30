//
// Mainsoft.Web.Security.DerbyMembershipProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Chris Toshok (toshok@ximian.com)
//	Vladimir Krasnov (vladimirk@mainsoft.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.OleDb;
using System.Data.Common;
using System.Text;
using System.Web.Configuration;
using System.Security;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;

namespace Mainsoft.Web.Security {
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// <para>Manages storage of membership information for an ASP.NET application in a Derby database.</para>
	/// </summary>
	public class DerbyMembershipProvider : MembershipProvider
	{
		const int SALT_BYTES = 16;

		bool enablePasswordReset;
		bool enablePasswordRetrieval;
		int maxInvalidPasswordAttempts;
		MembershipPasswordFormat passwordFormat;
		bool requiresQuestionAndAnswer;
		bool requiresUniqueEmail;
		int minRequiredNonAlphanumericCharacters;
		int minRequiredPasswordLength;
		int passwordAttemptWindow;
		string passwordStrengthRegularExpression;
		TimeSpan userIsOnlineTimeWindow;
		ConnectionStringSettings connectionString;
		bool schemaChecked = false;
		DerbyUnloadManager.DerbyShutDownPolicy shutDownPolicy = DerbyUnloadManager.DerbyShutDownPolicy.Default;

		string applicationName;

		DbConnection CreateConnection ()
		{
			if (!schemaChecked) {
				DerbyDBSchema.CheckSchema (connectionString.ConnectionString);
				schemaChecked = true;

				DerbyUnloadManager.RegisterUnloadHandler (connectionString.ConnectionString, shutDownPolicy);
			}

			OleDbConnection connection = new OleDbConnection (connectionString.ConnectionString);
			connection.Open ();
			return connection;
		}

		void CheckParam (string pName, string p, int length)
		{
			if (p == null)
				throw new ArgumentNullException (pName);
			if (p.Length == 0 || p.Length > length || p.IndexOf (",") != -1)
				throw new ArgumentException (String.Format ("invalid format for {0}", pName));
		}

		public override bool ChangePassword (string username, string oldPwd, string newPwd)
		{
			if (username != null) username = username.Trim ();
			if (oldPwd != null) oldPwd = oldPwd.Trim ();
			if (newPwd != null) newPwd = newPwd.Trim ();

			CheckParam ("username", username, 256);
			CheckParam ("oldPwd", oldPwd, 128);
			CheckParam ("newPwd", newPwd, 128);

			if (!CheckPassword (newPwd))
				throw new ArgumentException (string.Format (
						"New Password invalid. New Password length minimum: {0}. Non-alphanumeric characters required: {1}.",
						MinRequiredPasswordLength,
						MinRequiredNonAlphanumericCharacters));

			using (DbConnection connection = CreateConnection ()) {
				PasswordInfo pi = ValidateUsingPassword (username, oldPwd);

				if (pi != null) {
					EmitValidatingPassword (username, newPwd, false);
					string db_password = EncodePassword (newPwd, pi.PasswordFormat, pi.PasswordSalt);

					int st = DerbyMembershipHelper.Membership_SetPassword (connection, ApplicationName, username, db_password, (int) pi.PasswordFormat, pi.PasswordSalt, DateTime.UtcNow);

					if (st == 0)
						return true;
				}
				return false;
			}
		}

		public override bool ChangePasswordQuestionAndAnswer (string username, string password, string newPwdQuestion, string newPwdAnswer)
		{
			if (username != null) username = username.Trim ();
			if (newPwdQuestion != null) newPwdQuestion = newPwdQuestion.Trim ();
			if (newPwdAnswer != null) newPwdAnswer = newPwdAnswer.Trim ();

			CheckParam ("username", username, 256);
			if (RequiresQuestionAndAnswer)
				CheckParam ("newPwdQuestion", newPwdQuestion, 128);
			if (RequiresQuestionAndAnswer)
				CheckParam ("newPwdAnswer", newPwdAnswer, 128);

			using (DbConnection connection = CreateConnection ()) {
				PasswordInfo pi = ValidateUsingPassword (username, password);

				if (pi != null) {
					string db_passwordAnswer = EncodePassword (newPwdAnswer, pi.PasswordFormat, pi.PasswordSalt);

					int st = DerbyMembershipHelper.Membership_ChangePasswordQuestionAndAnswer (connection, ApplicationName, username, newPwdQuestion, db_passwordAnswer);

					if (st == 0)
						return true;
				}
				return false;
			}
		}

		public override MembershipUser CreateUser (string username,
							   string password,
							   string email,
							   string pwdQuestion,
							   string pwdAnswer,
							   bool isApproved,
							   object providerUserKey,
							   out MembershipCreateStatus status)
		{
			if (username != null) username = username.Trim ();
			if (password != null) password = password.Trim ();
			if (email != null) email = email.Trim ();
			if (pwdQuestion != null) pwdQuestion = pwdQuestion.Trim ();
			if (pwdAnswer != null) pwdAnswer = pwdAnswer.Trim ();

			/* some initial validation */
			if (username == null || username.Length == 0 || username.Length > 256 || username.IndexOf (",") != -1) {
				status = MembershipCreateStatus.InvalidUserName;
				return null;
			}
			if (password == null || password.Length == 0 || password.Length > 128) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if (!CheckPassword (password)) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}
			EmitValidatingPassword (username, password, true);

			if (RequiresUniqueEmail && (email == null || email.Length == 0)) {
				status = MembershipCreateStatus.InvalidEmail;
				return null;
			}
			if (RequiresQuestionAndAnswer &&
				(pwdQuestion == null ||
				 pwdQuestion.Length == 0 || pwdQuestion.Length > 256)) {
				status = MembershipCreateStatus.InvalidQuestion;
				return null;
			}
			if (RequiresQuestionAndAnswer &&
				(pwdAnswer == null ||
				 pwdAnswer.Length == 0 || pwdAnswer.Length > 128)) {
				status = MembershipCreateStatus.InvalidAnswer;
				return null;
			}
			if (providerUserKey != null && !(providerUserKey is Guid)) {
				status = MembershipCreateStatus.InvalidProviderUserKey;
				return null;
			}

			/* encode our password/answer using the
			 * "passwordFormat" configuration option */
			string passwordSalt = "";

			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			byte [] salt = new byte [SALT_BYTES];
			rng.GetBytes (salt);
			passwordSalt = Convert.ToBase64String (salt);

			password = EncodePassword (password, PasswordFormat, passwordSalt);
			if (RequiresQuestionAndAnswer)
				pwdAnswer = EncodePassword (pwdAnswer, PasswordFormat, passwordSalt);

			/* make sure the hashed/encrypted password and
			 * answer are still under 128 characters. */
			if (password.Length > 128) {
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if (RequiresQuestionAndAnswer) {
				if (pwdAnswer.Length > 128) {
					status = MembershipCreateStatus.InvalidAnswer;
					return null;
				}
			}
			status = MembershipCreateStatus.Success;

			using (DbConnection connection = CreateConnection ()) {
				try {

					object helperUserKey = providerUserKey != null ? providerUserKey.ToString () : null;
					DateTime Now = DateTime.UtcNow;
					int st = DerbyMembershipHelper.Membership_CreateUser (connection, ApplicationName, username, password, passwordSalt, email,
						pwdQuestion, pwdAnswer, isApproved, Now, Now, RequiresUniqueEmail, (int) PasswordFormat, ref helperUserKey);

					providerUserKey = new Guid ((string) helperUserKey);
					if (st == 0)
						return GetUser (providerUserKey, false);
					else if (st == 2)
					    status = MembershipCreateStatus.DuplicateUserName;
					else if (st == 3)
					    status = MembershipCreateStatus.DuplicateEmail;
					else if (st == 9)
						status = MembershipCreateStatus.InvalidProviderUserKey;
					else if (st == 10)
					    status = MembershipCreateStatus.DuplicateProviderUserKey;
					else
					    status = MembershipCreateStatus.ProviderError;

					return null;
				}
				catch (Exception) {
					status = MembershipCreateStatus.ProviderError;
					return null;
				}
			}
		}

		private bool CheckPassword (string password)
		{
			if (password.Length < MinRequiredPasswordLength)
				return false;

			if (MinRequiredNonAlphanumericCharacters > 0) {
				int nonAlphanumeric = 0;
				for (int i = 0; i < password.Length; i++) {
					if (!Char.IsLetterOrDigit (password [i]))
						nonAlphanumeric++;
				}
				return nonAlphanumeric >= MinRequiredNonAlphanumericCharacters;
			}
			return true;
		}

		public override bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			CheckParam ("username", username, 256);

			DeleteUserTableMask deleteBitmask = DeleteUserTableMask.MembershipUsers;

			if (deleteAllRelatedData)
				deleteBitmask |=
					DeleteUserTableMask.Profiles |
					DeleteUserTableMask.UsersInRoles |
					DeleteUserTableMask.WebPartStateUser;
			
			int num = 0;
			using (DbConnection connection = CreateConnection ()) {
				int st = DerbyMembershipHelper.Users_DeleteUser (connection, ApplicationName, username, (int) deleteBitmask, ref num);

				if (num == 0)
					return false;

				if (st == 0)
					return true;

				return false;
			}
		}

		public virtual string GeneratePassword ()
		{
			return Membership.GeneratePassword (MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);
		}

		public override MembershipUserCollection FindUsersByEmail (string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			CheckParam ("emailToMatch", emailToMatch, 256);

			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex must be >= 0");
			if (pageSize < 0)
				throw new ArgumentException ("pageSize must be >= 0");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			totalRecords = 0;
			using (DbConnection connection = CreateConnection ()) {
				DbDataReader reader = null;

				DerbyMembershipHelper.Membership_FindUsersByEmail (connection, ApplicationName, emailToMatch, pageSize, pageIndex, out reader);
				if (reader == null)
					return null;

				using (reader) {
					return BuildMembershipUserCollection (reader, pageIndex, pageSize, out totalRecords);
				}
			}
		}

		public override MembershipUserCollection FindUsersByName (string nameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			CheckParam ("nameToMatch", nameToMatch, 256);

			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex must be >= 0");
			if (pageSize < 0)
				throw new ArgumentException ("pageSize must be >= 0");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			totalRecords = 0;
			using (DbConnection connection = CreateConnection ()) {
				DbDataReader reader = null;

				DerbyMembershipHelper.Membership_FindUsersByName (connection, ApplicationName, nameToMatch, pageSize, pageIndex, out reader);
				if (reader == null)
					return null;

				using (reader) {
					return BuildMembershipUserCollection (reader, pageIndex, pageSize, out totalRecords);
				}
			}
		}

		public override MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex must be >= 0");
			if (pageSize < 0)
				throw new ArgumentException ("pageSize must be >= 0");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			using (DbConnection connection = CreateConnection ()) {
				DbDataReader reader = null;
				totalRecords = DerbyMembershipHelper.Membership_GetAllUsers (connection, ApplicationName, pageIndex, pageSize, out reader);
				return BuildMembershipUserCollection (reader, pageIndex, pageSize, out totalRecords);
			}
		}

		MembershipUserCollection BuildMembershipUserCollection (DbDataReader reader, int pageIndex, int pageSize, out int totalRecords)
		{
			int num_read = 0;
			int num_added = 0;
			int num_to_skip = pageIndex * pageSize;
			MembershipUserCollection users = new MembershipUserCollection ();
			try {
				while (reader.Read ()) {
					if (num_read >= num_to_skip) {
						if (num_added < pageSize) {
							users.Add (GetUserFromReader (reader));
							num_added++;
						}
					}
					num_read++;
				}
				totalRecords = num_read;
				return users;
			}
			catch (Exception) {
				totalRecords = 0;
				return null; /* should we let the exception through? */
			}
			finally {
				if (reader != null)
					reader.Close ();
			}
		}

		public override int GetNumberOfUsersOnline ()
		{
			using (DbConnection connection = CreateConnection ()) {
				return DerbyMembershipHelper.Membership_GetNumberOfUsersOnline (connection, ApplicationName, userIsOnlineTimeWindow.Minutes, DateTime.UtcNow);
			}
		}

		public override string GetPassword (string username, string answer)
		{
			if (!EnablePasswordRetrieval)
				throw new NotSupportedException ("this provider has not been configured to allow the retrieval of passwords");

			CheckParam ("username", username, 256);
			if (RequiresQuestionAndAnswer)
				CheckParam ("answer", answer, 128);

			PasswordInfo pi = GetPasswordInfo (username);
			if (pi == null)
				throw new ProviderException ("An error occurred while retrieving the password from the database");

			string user_answer = EncodePassword (answer, pi.PasswordFormat, pi.PasswordSalt);
			string password = null;

			using (DbConnection connection = CreateConnection ()) {
				int st = DerbyMembershipHelper.Membership_GetPassword (connection, ApplicationName, username, user_answer, MaxInvalidPasswordAttempts, PasswordAttemptWindow, DateTime.UtcNow, out password);

				if (st == 1)
					throw new ProviderException ("User specified by username is not found in the membership database");

				if (st == 2)
					throw new MembershipPasswordException ("The membership user identified by username is locked out");
				
				if (st == 3)
					throw new MembershipPasswordException ("Password Answer is invalid");

				return DecodePassword (password, pi.PasswordFormat);
			}
		}

		MembershipUser GetUserFromReader (DbDataReader reader)
		{
			return new MembershipUser (
				this.Name,                                          /* XXX is this right?  */
				reader.GetString (0),                               /* name */
				new Guid (reader.GetString (1)),                    /* providerUserKey */
				reader.IsDBNull (2) ? null : reader.GetString (2),  /* email */
				reader.IsDBNull (3) ? null : reader.GetString (3),  /* passwordQuestion */
				reader.IsDBNull (4) ? null : reader.GetString (4),  /* comment */
				reader.GetInt32 (5) > 0,                            /* isApproved */
				reader.GetInt32 (6) > 0,                            /* isLockedOut */
				reader.GetDateTime (7).ToLocalTime (),              /* creationDate */
				reader.GetDateTime (8).ToLocalTime (),              /* lastLoginDate */
				reader.GetDateTime (9).ToLocalTime (),              /* lastActivityDate */
				reader.GetDateTime (10).ToLocalTime (),             /* lastPasswordChangedDate */
				reader.GetDateTime (11).ToLocalTime ()              /* lastLockoutDate */);
		}

		public override MembershipUser GetUser (string username, bool userIsOnline)
		{
			if (username.Length == 0)
				return null;

			CheckParam ("username", username, 256);

			using (DbConnection connection = CreateConnection ()) {
				DbDataReader reader = null;
				int st = DerbyMembershipHelper.Membership_GetUserByName (connection, ApplicationName, username, userIsOnline, DateTime.UtcNow, out reader);
				using (reader) {
					if (st == 0 && reader != null) {
						MembershipUser u = GetUserFromReader (reader);
						return u;
					}
				}
			}
			return null;
		}

		public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			if (providerUserKey == null)
				throw new ArgumentNullException ("providerUserKey");

			if (!(providerUserKey is Guid))
				throw new ArgumentException ("providerUserKey is not of type Guid", "providerUserKey");

			using (DbConnection connection = CreateConnection ()) {
				DbDataReader reader = null;
				int st = DerbyMembershipHelper.Membership_GetUserByUserId (connection, providerUserKey.ToString (), userIsOnline, DateTime.UtcNow, out reader);
				using (reader) {
					if (st == 0 && reader != null) {
						MembershipUser u = GetUserFromReader (reader);
						return u;
					}
				}
			}
			return null;
		}

		public override string GetUserNameByEmail (string email)
		{
			CheckParam ("email", email, 256);

			string username = null;

			using (DbConnection connection = CreateConnection ()) {
				int st = DerbyMembershipHelper.Membership_GetUserByEmail (connection, ApplicationName, email, out username);

				if (st == 1)
					return null;
				
				if (st == 2 && RequiresUniqueEmail)
					throw new ProviderException ("More than one user with the same e-mail address exists in the database and RequiresUniqueEmail is true");
			}
			return username;
		}

		bool GetBoolConfigValue (NameValueCollection config, string name, bool def)
		{
			bool rv = def;
			string val = config [name];
			if (val != null) {
				try { rv = Boolean.Parse (val); }
				catch (Exception e) {
					throw new ProviderException (String.Format ("{0} must be true or false", name), e);
				}
			}
			return rv;
		}

		int GetIntConfigValue (NameValueCollection config, string name, int def)
		{
			int rv = def;
			string val = config [name];
			if (val != null) {
				try { rv = Int32.Parse (val); }
				catch (Exception e) {
					throw new ProviderException (String.Format ("{0} must be an integer", name), e);
				}
			}
			return rv;
		}

		int GetEnumConfigValue (NameValueCollection config, string name, Type enumType, int def)
		{
			int rv = def;
			string val = config [name];
			if (val != null) {
				try { rv = (int) Enum.Parse (enumType, val); }
				catch (Exception e) {
					throw new ProviderException (String.Format ("{0} must be one of the following values: {1}", name, String.Join (",", Enum.GetNames (enumType))), e);
				}
			}
			return rv;
		}

		string GetStringConfigValue (NameValueCollection config, string name, string def)
		{
			string rv = def;
			string val = config [name];
			if (val != null)
				rv = val;
			return rv;
		}

		void EmitValidatingPassword (string username, string password, bool isNewUser)
		{
			ValidatePasswordEventArgs args = new ValidatePasswordEventArgs (username, password, isNewUser);
			OnValidatingPassword (args);

			/* if we're canceled.. */
			if (args.Cancel) {
				if (args.FailureInformation == null)
					throw new ProviderException ("Password validation canceled");
				else
					throw args.FailureInformation;
			}
		}

		public override void Initialize (string name, NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException ("config");

			base.Initialize (name, config);

			applicationName = GetStringConfigValue (config, "applicationName", "/");
			enablePasswordReset = GetBoolConfigValue (config, "enablePasswordReset", true);
			enablePasswordRetrieval = GetBoolConfigValue (config, "enablePasswordRetrieval", false);
			requiresQuestionAndAnswer = GetBoolConfigValue (config, "requiresQuestionAndAnswer", true);
			requiresUniqueEmail = GetBoolConfigValue (config, "requiresUniqueEmail", false);
			passwordFormat = (MembershipPasswordFormat) GetEnumConfigValue (config, "passwordFormat", typeof (MembershipPasswordFormat),
											   (int) MembershipPasswordFormat.Hashed);
			maxInvalidPasswordAttempts = GetIntConfigValue (config, "maxInvalidPasswordAttempts", 5);
			minRequiredPasswordLength = GetIntConfigValue (config, "minRequiredPasswordLength", 7);
			minRequiredNonAlphanumericCharacters = GetIntConfigValue (config, "minRequiredNonAlphanumericCharacters", 1);
			passwordAttemptWindow = GetIntConfigValue (config, "passwordAttemptWindow", 10);
			passwordStrengthRegularExpression = GetStringConfigValue (config, "passwordStrengthRegularExpression", "");

			MembershipSection section = (MembershipSection) WebConfigurationManager.GetSection ("system.web/membership");

			userIsOnlineTimeWindow = section.UserIsOnlineTimeWindow;

			/* we can't support password retrieval with hashed passwords */
			if (passwordFormat == MembershipPasswordFormat.Hashed && enablePasswordRetrieval)
				throw new ProviderException ("password retrieval cannot be used with hashed passwords");

			string connectionStringName = config ["connectionStringName"];

			if (applicationName.Length > 256)
				throw new ProviderException ("The ApplicationName attribute must be 256 characters long or less.");
			if (connectionStringName == null || connectionStringName.Length == 0)
				throw new ProviderException ("The ConnectionStringName attribute must be present and non-zero length.");

			connectionString = WebConfigurationManager.ConnectionStrings [connectionStringName];
			if (connectionString == null)
				throw new ProviderException (String.Format ("The connection name '{0}' was not found in the applications configuration or the connection string is empty.", connectionStringName));

			if (connectionString == null)
				throw new ProviderException (String.Format ("The connection name '{0}' was not found in the applications configuration or the connection string is empty.", connectionStringName));

			string shutdown = config ["shutdown"];
			if (!String.IsNullOrEmpty (shutdown))
				shutDownPolicy = (DerbyUnloadManager.DerbyShutDownPolicy) Enum.Parse (typeof (DerbyUnloadManager.DerbyShutDownPolicy), shutdown, true);
		}

		public override string ResetPassword (string username, string answer)
		{
			if (!EnablePasswordReset)
				throw new NotSupportedException ("this provider has not been configured to allow the resetting of passwords");

			CheckParam ("username", username, 256);

			if (RequiresQuestionAndAnswer)
				CheckParam ("answer", answer, 128);

			using (DbConnection connection = CreateConnection ()) {
				PasswordInfo pi = GetPasswordInfo (username);
				if (pi == null)
					throw new ProviderException (username + "is not found in the membership database");

				string newPassword = GeneratePassword ();
				EmitValidatingPassword (username, newPassword, false);

				string db_password = EncodePassword (newPassword, pi.PasswordFormat, pi.PasswordSalt);
				string db_answer = EncodePassword (answer, pi.PasswordFormat, pi.PasswordSalt);

				int st = DerbyMembershipHelper.Membership_ResetPassword (connection, ApplicationName, username, db_password, db_answer, (int) pi.PasswordFormat, pi.PasswordSalt, MaxInvalidPasswordAttempts, PasswordAttemptWindow, DateTime.UtcNow);

				if (st == 0)
					return newPassword;
				else if (st == 1)
					throw new ProviderException (username + " is not found in the membership database");
				else if (st == 2)
					throw new MembershipPasswordException ("The user account is currently locked out");
				else if (st == 3)
					throw new MembershipPasswordException ("Password Answer is invalid");
				else
					throw new ProviderException ("Failed to reset password");
			}
		}

		public override void UpdateUser (MembershipUser user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			if (user.UserName == null)
				throw new ArgumentNullException ("user.UserName");

			if (RequiresUniqueEmail && user.Email == null)
				throw new ArgumentNullException ("user.Email");

			CheckParam ("user.UserName", user.UserName, 256);

			if (user.Email.Length > 256 || (RequiresUniqueEmail && user.Email.Length == 0))
				throw new ArgumentException ("invalid format for user.Email");

			using (DbConnection connection = CreateConnection ()) {
				int st = DerbyMembershipHelper.Membership_UpdateUser (connection, ApplicationName, user.UserName, user.Email, user.Comment, user.IsApproved, RequiresUniqueEmail, user.LastLoginDate, DateTime.UtcNow, DateTime.UtcNow);

				if (st == 1)
					throw new ProviderException ("The UserName property of user was not found in the database.");
				if (st == 2)
					throw new ProviderException ("The Email property of user was equal to an existing e-mail address in the database and RequiresUniqueEmail is set to true.");
				if (st != 0)
					throw new ProviderException ("Failed to update user");
			}
		}

		public override bool ValidateUser (string username, string password)
		{
			if (username.Length == 0)
				return false;

			CheckParam ("username", username, 256);
			EmitValidatingPassword (username, password, false);

			PasswordInfo pi = ValidateUsingPassword (username, password);
			if (pi != null) {
				pi.LastLoginDate = DateTime.UtcNow;
				UpdateUserInfo (username, pi, true, true);
				return true;
			}
			return false;
		}

		public override bool UnlockUser (string username)
		{
			CheckParam ("username", username, 256);

			using (DbConnection connection = CreateConnection ()) {
				try {
					int st = DerbyMembershipHelper.Membership_UnlockUser (connection, ApplicationName, username);

					if (st == 0)
						return true;
				}
				catch (Exception e) {
					throw new ProviderException ("Failed to unlock user", e);
				}
			}
			return false;
		}

		void UpdateUserInfo (string username, PasswordInfo pi, bool isPasswordCorrect, bool updateLoginActivity)
		{
			CheckParam ("username", username, 256);

			using (DbConnection connection = CreateConnection ()) {
				try {
					int st = DerbyMembershipHelper.Membership_UpdateUserInfo (connection, ApplicationName, username, isPasswordCorrect, updateLoginActivity,
						MaxInvalidPasswordAttempts, PasswordAttemptWindow, DateTime.UtcNow, pi.LastLoginDate, pi.LastActivityDate);

					if (st == 0)
						return;
				}
				catch (Exception e) {
					throw new ProviderException ("Failed to update Membership table", e);
				}

			}
		}

		PasswordInfo ValidateUsingPassword (string username, string password)
		{
			MembershipUser user = GetUser (username, true);
			if (user == null)
				return null;

			if (!user.IsApproved || user.IsLockedOut)
				return null;

			PasswordInfo pi = GetPasswordInfo (username);

			if (pi == null)
				return null;

			/* do the actual validation */
			string user_password = EncodePassword (password, pi.PasswordFormat, pi.PasswordSalt);

			if (user_password != pi.Password) {
				UpdateUserInfo (username, pi, false, false);
				return null;
			}

			return pi;
		}

		private PasswordInfo GetPasswordInfo (string username)
		{
			using (DbConnection connection = CreateConnection ()) {
				DbDataReader reader = null;
				DerbyMembershipHelper.Membership_GetPasswordWithFormat (connection, ApplicationName, username, false, DateTime.UtcNow, out reader);

				PasswordInfo pi = null;
				if (reader == null)
					return null;

				using (reader) {
					if (reader.Read ()) {
						int isLockedOut = reader.GetInt32 (1);
						if (isLockedOut > 0)
							return null;

						pi = new PasswordInfo (
							reader.GetString (3),
							(MembershipPasswordFormat) reader.GetInt32 (4),
							reader.GetString (5),
							reader.GetInt32 (6),
							reader.GetInt32 (7),
							reader.GetInt32 (2) > 0,
							reader.GetDateTime (8),
							reader.GetDateTime (9));
					}
				}
				return pi;
			}
		}

		private string EncodePassword (string password, MembershipPasswordFormat passwordFormat, string salt)
		{
			byte [] password_bytes;
			byte [] salt_bytes;

			switch (passwordFormat) {
				case MembershipPasswordFormat.Clear:
					return password;
				case MembershipPasswordFormat.Hashed:
					password_bytes = Encoding.Unicode.GetBytes (password);
					salt_bytes = Convert.FromBase64String (salt);

					byte [] hashBytes = new byte [salt_bytes.Length + password_bytes.Length];

					Buffer.BlockCopy (salt_bytes, 0, hashBytes, 0, salt_bytes.Length);
					Buffer.BlockCopy (password_bytes, 0, hashBytes, salt_bytes.Length, password_bytes.Length);

					MembershipSection section = (MembershipSection) WebConfigurationManager.GetSection ("system.web/membership");
					string alg_type = section.HashAlgorithmType;
					if (alg_type == "") {
						MachineKeySection keysection = (MachineKeySection) WebConfigurationManager.GetSection ("system.web/machineKey");
						alg_type = keysection.Validation.ToString ();
					}
					using (HashAlgorithm hash = HashAlgorithm.Create (alg_type)) {
						hash.TransformFinalBlock (hashBytes, 0, hashBytes.Length);
						return Convert.ToBase64String (hash.Hash);
					}
				case MembershipPasswordFormat.Encrypted:
					password_bytes = Encoding.Unicode.GetBytes (password);
					salt_bytes = Convert.FromBase64String (salt);

					byte [] buf = new byte [password_bytes.Length + salt_bytes.Length];

					Array.Copy (salt_bytes, 0, buf, 0, salt_bytes.Length);
					Array.Copy (password_bytes, 0, buf, salt_bytes.Length, password_bytes.Length);

					return Convert.ToBase64String (EncryptPassword (buf));
				default:
					/* not reached.. */
					return null;
			}
		}

		private string DecodePassword (string password, MembershipPasswordFormat passwordFormat)
		{
			switch (passwordFormat) {
				case MembershipPasswordFormat.Clear:
					return password;
				case MembershipPasswordFormat.Hashed:
					throw new ProviderException ("Hashed passwords cannot be decoded.");
				case MembershipPasswordFormat.Encrypted:
					return Encoding.Unicode.GetString (DecryptPassword (Convert.FromBase64String (password)));
				default:
					/* not reached.. */
					return null;
			}
		}

		public override string ApplicationName
		{
			get { return applicationName; }
			set { applicationName = value; }
		}

		public override bool EnablePasswordReset
		{
			get { return enablePasswordReset; }
		}

		public override bool EnablePasswordRetrieval
		{
			get { return enablePasswordRetrieval; }
		}

		public override MembershipPasswordFormat PasswordFormat
		{
			get { return passwordFormat; }
		}

		public override bool RequiresQuestionAndAnswer
		{
			get { return requiresQuestionAndAnswer; }
		}

		public override bool RequiresUniqueEmail
		{
			get { return requiresUniqueEmail; }
		}

		public override int MaxInvalidPasswordAttempts
		{
			get { return maxInvalidPasswordAttempts; }
		}

		public override int MinRequiredNonAlphanumericCharacters
		{
			get { return minRequiredNonAlphanumericCharacters; }
		}

		public override int MinRequiredPasswordLength
		{
			get { return minRequiredPasswordLength; }
		}

		public override int PasswordAttemptWindow
		{
			get { return passwordAttemptWindow; }
		}

		public override string PasswordStrengthRegularExpression
		{
			get { return passwordStrengthRegularExpression; }
		}

		[Flags]
		private enum DeleteUserTableMask
		{
			MembershipUsers = 1,
			UsersInRoles = 2,
			Profiles = 4,
			WebPartStateUser = 8
		}

		private sealed class PasswordInfo
		{
			private string _password;
			private MembershipPasswordFormat _passwordFormat;
			private string _passwordSalt;
			private int _failedPasswordAttemptCount;
			private int _failedPasswordAnswerAttemptCount;
			private bool _isApproved;
			private DateTime _lastLoginDate;
			private DateTime _lastActivityDate;

			internal PasswordInfo (
				string password,
				MembershipPasswordFormat passwordFormat,
				string passwordSalt,
				int failedPasswordAttemptCount,
				int failedPasswordAnswerAttemptCount,
				bool isApproved,
				DateTime lastLoginDate,
				DateTime lastActivityDate)
			{
				_password = password;
				_passwordFormat = passwordFormat;
				_passwordSalt = passwordSalt;
				_failedPasswordAttemptCount = failedPasswordAttemptCount;
				_failedPasswordAnswerAttemptCount = failedPasswordAnswerAttemptCount;
				_isApproved = isApproved;
				_lastLoginDate = lastLoginDate;
				_lastActivityDate = lastActivityDate;
			}

			public string Password
			{
				get { return _password; }
				set { _password = value; }
			}
			public MembershipPasswordFormat PasswordFormat
			{
				get { return _passwordFormat; }
				set { _passwordFormat = value; }
			}
			public string PasswordSalt
			{
				get { return _passwordSalt; }
				set { _passwordSalt = value; }
			}
			public int FailedPasswordAttemptCount
			{
				get { return _failedPasswordAttemptCount; }
				set { _failedPasswordAttemptCount = value; }
			}
			public int FailedPasswordAnswerAttemptCount
			{
				get { return _failedPasswordAnswerAttemptCount; }
				set { _failedPasswordAnswerAttemptCount = value; }
			}
			public bool IsApproved
			{
				get { return _isApproved; }
				set { _isApproved = value; }
			}
			public DateTime LastLoginDate
			{
				get { return _lastLoginDate; }
				set { _lastLoginDate = value; }
			}
			public DateTime LastActivityDate
			{
				get { return _lastActivityDate; }
				set { _lastActivityDate = value; }
			}
		}
	}
}
#endif

