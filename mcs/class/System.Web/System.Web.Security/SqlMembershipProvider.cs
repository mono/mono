//
// System.Web.Security.SqlMembershipProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2003 Ben Maurer
// Copyright (c) 2005,2006 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Web.Configuration;
using System.Security.Cryptography;

namespace System.Web.Security {
	public class SqlMembershipProvider : MembershipProvider
	{
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
		DbProviderFactory factory;

		string applicationName;
		bool schemaIsOk = false;

		DbConnection CreateConnection ()
		{
			if (!schemaIsOk && !(schemaIsOk = AspNetDBSchemaChecker.CheckMembershipSchemaVersion (factory, connectionString.ConnectionString, "membership", "1")))
				throw new ProviderException ("Incorrect ASP.NET DB Schema Version.");

			DbConnection connection;

			if (connectionString == null)
				throw new ProviderException ("Connection string for the SQL Membership Provider has not been provided.");
			
			try {
				connection = factory.CreateConnection ();
				connection.ConnectionString = connectionString.ConnectionString;
				connection.Open ();
			} catch (Exception ex) {
				throw new ProviderException ("Unable to open SQL connection for the SQL Membership Provider.",
							     ex);
			}
			
			return connection;
		}

		DbParameter AddParameter (DbCommand command, string parameterName, object parameterValue)
		{
			return AddParameter (command, parameterName, ParameterDirection.Input, parameterValue);
		}

		DbParameter AddParameter (DbCommand command, string parameterName, ParameterDirection direction, object parameterValue)
		{
			DbParameter dbp = command.CreateParameter ();
			dbp.ParameterName = parameterName;
			dbp.Value = parameterValue;
			dbp.Direction = direction;
			command.Parameters.Add (dbp);
			return dbp;
		}

		DbParameter AddParameter (DbCommand command, string parameterName, ParameterDirection direction, DbType type, object parameterValue)
		{
			DbParameter dbp = command.CreateParameter ();
			dbp.ParameterName = parameterName;
			dbp.Value = parameterValue;
			dbp.Direction = direction;
			dbp.DbType = type;
			command.Parameters.Add (dbp);
			return dbp;
		}

		static int GetReturnValue (DbParameter returnValue)
		{
			object value = returnValue.Value;
			return value is int ? (int) value : -1;
		}

		void CheckParam (string pName, string p, int length)
		{
			if (p == null)
				throw new ArgumentNullException (pName);
			if (p.Length == 0 || p.Length > length || p.IndexOf (',') != -1)
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

					DbCommand command = factory.CreateCommand ();
					command.Connection = connection;
					command.CommandText = @"aspnet_Membership_SetPassword";
					command.CommandType = CommandType.StoredProcedure;

					AddParameter (command, "@ApplicationName", ApplicationName);
					AddParameter (command, "@UserName", username);
					AddParameter (command, "@NewPassword", db_password);
					AddParameter (command, "@PasswordFormat", (int) pi.PasswordFormat);
					AddParameter (command, "@PasswordSalt", pi.PasswordSalt);
					AddParameter (command, "@CurrentTimeUtc", DateTime.UtcNow);
					DbParameter returnValue = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

					command.ExecuteNonQuery ();

					if (GetReturnValue (returnValue) != 0)
						return false;

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

					DbCommand command = factory.CreateCommand ();
					command.Connection = connection;
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = @"aspnet_Membership_ChangePasswordQuestionAndAnswer";

					AddParameter (command, "@ApplicationName", ApplicationName);
					AddParameter (command, "@UserName", username);
					AddParameter (command, "@NewPasswordQuestion", newPwdQuestion);
					AddParameter (command, "@NewPasswordAnswer", db_passwordAnswer);
					DbParameter returnValue = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

					command.ExecuteNonQuery ();

					if (GetReturnValue (returnValue) != 0)
						return false;

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
			if (username == null || username.Length == 0 || username.Length > 256 || username.IndexOf (',') != -1) {
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

			if (providerUserKey == null)
				providerUserKey = Guid.NewGuid();

			/* encode our password/answer using the
			 * "passwordFormat" configuration option */
			string passwordSalt = "";

			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			byte [] salt = new byte [MembershipHelper.SALT_BYTES];
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
					DbCommand command = factory.CreateCommand ();
					command.Connection = connection;
					command.CommandText = @"aspnet_Membership_CreateUser";
					command.CommandType = CommandType.StoredProcedure;

					DateTime Now = DateTime.UtcNow;

					AddParameter (command, "@ApplicationName", ApplicationName);
					AddParameter (command, "@UserName", username);
					AddParameter (command, "@Password", password);
					AddParameter (command, "@PasswordSalt", passwordSalt);
					AddParameter (command, "@Email", email);
					AddParameter (command, "@PasswordQuestion", pwdQuestion);
					AddParameter (command, "@PasswordAnswer", pwdAnswer);
					AddParameter (command, "@IsApproved", isApproved);
					AddParameter (command, "@CurrentTimeUtc", Now);
					AddParameter (command, "@CreateDate", Now);
					AddParameter (command, "@UniqueEmail", RequiresUniqueEmail);
					AddParameter (command, "@PasswordFormat", (int) PasswordFormat);
					AddParameter (command, "@UserId", ParameterDirection.InputOutput, providerUserKey);
					DbParameter returnValue = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

					command.ExecuteNonQuery ();

					int st = GetReturnValue (returnValue);

					if (st == 0)
						return GetUser (username, false);
					else if (st == 6)
						status = MembershipCreateStatus.DuplicateUserName;
					else if (st == 7)
						status = MembershipCreateStatus.DuplicateEmail;
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

		bool CheckPassword (string password)
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

			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"aspnet_Users_DeleteUser";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@UserName", username);
				AddParameter (command, "@TablesToDeleteFrom", (int) deleteBitmask);
				AddParameter (command, "@NumTablesDeletedFrom", ParameterDirection.Output, 0);
				DbParameter returnValue = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				command.ExecuteNonQuery ();

				if (((int) command.Parameters ["@NumTablesDeletedFrom"].Value) == 0)
					return false;

				if (GetReturnValue (returnValue) == 0)
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

			using (DbConnection connection = CreateConnection ()) {

				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"aspnet_Membership_FindUsersByEmail";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@PageIndex", pageIndex);
				AddParameter (command, "@PageSize", pageSize);
				AddParameter (command, "@EmailToMatch", emailToMatch);
				AddParameter (command, "@ApplicationName", ApplicationName);
				// return value
				AddParameter (command, "@ReturnValue", ParameterDirection.ReturnValue, null);

				MembershipUserCollection c = BuildMembershipUserCollection (command, pageIndex, pageSize, out totalRecords);

				return c;
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

			using (DbConnection connection = CreateConnection ()) {

				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"aspnet_Membership_FindUsersByName";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@PageIndex", pageIndex);
				AddParameter (command, "@PageSize", pageSize);
				AddParameter (command, "@UserNameToMatch", nameToMatch);
				AddParameter (command, "@ApplicationName", ApplicationName);
				// return value
				AddParameter (command, "@ReturnValue", ParameterDirection.ReturnValue, null);

				MembershipUserCollection c = BuildMembershipUserCollection (command, pageIndex, pageSize, out totalRecords);

				return c;
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
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"aspnet_Membership_GetAllUsers";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@PageIndex", pageIndex);
				AddParameter (command, "@PageSize", pageSize);
				// return value
				AddParameter (command, "@ReturnValue", ParameterDirection.ReturnValue, null);

				MembershipUserCollection c = BuildMembershipUserCollection (command, pageIndex, pageSize, out totalRecords);

				return c;
			}
		}

		MembershipUserCollection BuildMembershipUserCollection (DbCommand command, int pageIndex, int pageSize, out int totalRecords)
		{
			DbDataReader reader = null;
			try {
				MembershipUserCollection users = new MembershipUserCollection ();
				reader = command.ExecuteReader ();
				while (reader.Read ())
					users.Add (GetUserFromReader (reader, null, null));

				totalRecords = Convert.ToInt32 (command.Parameters ["@ReturnValue"].Value);
				return users;
			} catch (Exception) {
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
				DateTime now = DateTime.UtcNow;

				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"aspnet_Membership_GetNumberOfUsersOnline";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@CurrentTimeUtc", now.ToString ());
				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@MinutesSinceLastInActive", userIsOnlineTimeWindow.Minutes);
				DbParameter returnValue = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				command.ExecuteScalar ();
				return GetReturnValue (returnValue);
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
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"aspnet_Membership_GetPassword";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@UserName", username);
				AddParameter (command, "@MaxInvalidPasswordAttempts", MaxInvalidPasswordAttempts);
				AddParameter (command, "@PasswordAttemptWindow", PasswordAttemptWindow);
				AddParameter (command, "@CurrentTimeUtc", DateTime.UtcNow);
				AddParameter (command, "@PasswordAnswer", user_answer);
				DbParameter retValue = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				DbDataReader reader = command.ExecuteReader ();

				int returnValue = GetReturnValue (retValue);
				if (returnValue == 3)
					throw new MembershipPasswordException ("Password Answer is invalid");
				if (returnValue == 99)
					throw new MembershipPasswordException ("The user account is currently locked out");

				if (reader.Read ()) {
					password = reader.GetString (0);
					reader.Close ();
				}

				if (pi.PasswordFormat == MembershipPasswordFormat.Clear)
					return password;
				else if (pi.PasswordFormat == MembershipPasswordFormat.Encrypted)
					return DecodePassword (password, pi.PasswordFormat);

				return password;
			}
		}

		MembershipUser GetUserFromReader (DbDataReader reader, string username, object userId)
		{
			int i = 0;
			if (username == null)
				i = 1;

			if (userId != null)
				username = reader.GetString (8);

			return new MembershipUser (this.Name, /* XXX is this right?  */
				(username == null ? reader.GetString (0) : username), /* name */
				(userId == null ? reader.GetGuid (8 + i) : userId), /* providerUserKey */
				reader.IsDBNull (0 + i) ? null : reader.GetString (0 + i), /* email */
				reader.IsDBNull (1 + i) ? null : reader.GetString (1 + i), /* passwordQuestion */
				reader.IsDBNull (2 + i) ? null : reader.GetString (2 + i), /* comment */
				reader.GetBoolean (3 + i), /* isApproved */
				reader.GetBoolean (9 + i), /* isLockedOut */
				reader.GetDateTime (4 + i).ToLocalTime (), /* creationDate */
				reader.GetDateTime (5 + i).ToLocalTime (), /* lastLoginDate */
				reader.GetDateTime (6 + i).ToLocalTime (), /* lastActivityDate */
				reader.GetDateTime (7 + i).ToLocalTime (), /* lastPasswordChangedDate */
				reader.GetDateTime (10 + i).ToLocalTime () /* lastLockoutDate */);
		}

		MembershipUser BuildMembershipUser (DbCommand query, string username, object userId)
		{
			try {
				using (DbConnection connection = CreateConnection ()) {
					query.Connection = connection;
					using (DbDataReader reader = query.ExecuteReader ()) {
						if (!reader.Read ())
							return null;

						return GetUserFromReader (reader, username, userId);
					}
				}
			} catch (Exception) {
				return null; /* should we let the exception through? */
			}
			finally {
				query.Connection = null;
			}
		}

		public override MembershipUser GetUser (string username, bool userIsOnline)
		{
			if (username == null)
				throw new ArgumentNullException ("username");

			if (username.Length == 0)
				return null;

			CheckParam ("username", username, 256);

			DbCommand command = factory.CreateCommand ();

			command.CommandText = @"aspnet_Membership_GetUserByName";
			command.CommandType = CommandType.StoredProcedure;

			AddParameter (command, "@UserName", username);
			AddParameter (command, "@ApplicationName", ApplicationName);
			AddParameter (command, "@CurrentTimeUtc", DateTime.Now);
			AddParameter (command, "@UpdateLastActivity", userIsOnline);

			MembershipUser u = BuildMembershipUser (command, username, null);

			return u;
		}

		public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			DbCommand command = factory.CreateCommand ();
			command.CommandText = @"aspnet_Membership_GetUserByUserId";
			command.CommandType = CommandType.StoredProcedure;

			AddParameter (command, "@UserId", providerUserKey);
			AddParameter (command, "@CurrentTimeUtc", DateTime.Now);
			AddParameter (command, "@UpdateLastActivity", userIsOnline);

			MembershipUser u = BuildMembershipUser (command, string.Empty, providerUserKey);
			return u;
		}

		public override string GetUserNameByEmail (string email)
		{
			CheckParam ("email", email, 256);

			using (DbConnection connection = CreateConnection ()) {

				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"aspnet_Membership_GetUserByEmail";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@Email", email);

				DbDataReader reader = command.ExecuteReader ();
				string rv = null;
				if (reader.Read ())
					rv = reader.GetString (0);
				reader.Close ();
				return rv;
			}
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
			minRequiredNonAlphanumericCharacters = GetIntConfigValue (config, "minRequiredNonalphanumericCharacters", 1);
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
			factory = connectionString == null || String.IsNullOrEmpty (connectionString.ProviderName) ?
				System.Data.SqlClient.SqlClientFactory.Instance :
				ProvidersHelper.GetDbProviderFactory (connectionString.ProviderName);
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

				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"aspnet_Membership_ResetPassword";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@UserName", username);
				AddParameter (command, "@NewPassword", db_password);
				AddParameter (command, "@MaxInvalidPasswordAttempts", MaxInvalidPasswordAttempts);
				AddParameter (command, "@PasswordAttemptWindow", PasswordAttemptWindow);
				AddParameter (command, "@PasswordSalt", pi.PasswordSalt);
				AddParameter (command, "@CurrentTimeUtc", DateTime.UtcNow);
				AddParameter (command, "@PasswordFormat", (int) pi.PasswordFormat);
				AddParameter (command, "@PasswordAnswer", db_answer);
				DbParameter retValue = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				command.ExecuteNonQuery ();

				int returnValue = GetReturnValue (retValue);

				if (returnValue == 0)
					return newPassword;
				else if (returnValue == 3)
					throw new MembershipPasswordException ("Password Answer is invalid");
				else if (returnValue == 99)
					throw new MembershipPasswordException ("The user account is currently locked out");
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
				int returnValue = 0;

				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"aspnet_Membership_UpdateUser";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@UserName", user.UserName);
				AddParameter (command, "@Email", user.Email == null ? (object) DBNull.Value : (object) user.Email);
				AddParameter (command, "@Comment", user.Comment == null ? (object) DBNull.Value : (object) user.Comment);
				AddParameter (command, "@IsApproved", user.IsApproved);
				AddParameter (command, "@LastLoginDate", DateTime.UtcNow);
				AddParameter (command, "@LastActivityDate", DateTime.UtcNow);
				AddParameter (command, "@UniqueEmail", RequiresUniqueEmail);
				AddParameter (command, "@CurrentTimeUtc", DateTime.UtcNow);
				DbParameter retValue = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				command.ExecuteNonQuery ();

				returnValue = GetReturnValue (retValue);

				if (returnValue == 1)
					throw new ProviderException ("The UserName property of user was not found in the database.");
				if (returnValue == 7)
					throw new ProviderException ("The Email property of user was equal to an existing e-mail address in the database and RequiresUniqueEmail is set to true.");
				if (returnValue != 0)
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
					DbCommand command = factory.CreateCommand ();
					command.Connection = connection;
					command.CommandText = @"aspnet_Membership_UnlockUser"; ;
					command.CommandType = CommandType.StoredProcedure;

					AddParameter (command, "@ApplicationName", ApplicationName);
					AddParameter (command, "@UserName", username);
					DbParameter returnValue = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

					command.ExecuteNonQuery ();
					if (GetReturnValue (returnValue) != 0)
						return false;
				}
				catch (Exception e) {
					throw new ProviderException ("Failed to unlock user", e);
				}
			}
			return true;
		}

		void UpdateUserInfo (string username, PasswordInfo pi, bool isPasswordCorrect, bool updateLoginActivity)
		{
			CheckParam ("username", username, 256);

			using (DbConnection connection = CreateConnection ()) {
				try {
					DbCommand command = factory.CreateCommand ();
					command.Connection = connection;
					command.CommandText = @"aspnet_Membership_UpdateUserInfo"; ;
					command.CommandType = CommandType.StoredProcedure;

					AddParameter (command, "@ApplicationName", ApplicationName);
					AddParameter (command, "@UserName", username);
					AddParameter (command, "@IsPasswordCorrect", isPasswordCorrect);
					AddParameter (command, "@UpdateLastLoginActivityDate", updateLoginActivity);
					AddParameter (command, "@MaxInvalidPasswordAttempts", MaxInvalidPasswordAttempts);
					AddParameter (command, "@PasswordAttemptWindow", PasswordAttemptWindow);
					AddParameter (command, "@CurrentTimeUtc", DateTime.UtcNow);
					AddParameter (command, "@LastLoginDate", pi.LastLoginDate);
					AddParameter (command, "@LastActivityDate", pi.LastActivityDate);
					DbParameter retValue = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

					command.ExecuteNonQuery ();

					int returnValue = GetReturnValue (retValue);
					if (returnValue != 0)
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

		PasswordInfo GetPasswordInfo (string username)
		{
			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = @"aspnet_Membership_GetPasswordWithFormat";

				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@UserName", username);
				AddParameter (command, "@UpdateLastLoginActivityDate", false);
				AddParameter (command, "@CurrentTimeUtc", DateTime.Now);
				// return value
				AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				DbDataReader reader = command.ExecuteReader ();
				if (!reader.Read ())
					return null;

				PasswordInfo pi = new PasswordInfo (
					reader.GetString (0),
					(MembershipPasswordFormat) reader.GetInt32 (1),
					reader.GetString (2),
					reader.GetInt32 (3),
					reader.GetInt32 (4),
					reader.GetBoolean (5),
					reader.GetDateTime (6),
					reader.GetDateTime (7));

				return pi;
			}
		}

		string EncodePassword (string password, MembershipPasswordFormat passwordFormat, string salt)
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

		string DecodePassword (string password, MembershipPasswordFormat passwordFormat)
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
		enum DeleteUserTableMask
		{
			MembershipUsers = 1,
			UsersInRoles = 2,
			Profiles = 4,
			WebPartStateUser = 8
		}

		sealed class PasswordInfo
		{
			string _password;
			MembershipPasswordFormat _passwordFormat;
			string _passwordSalt;
			int _failedPasswordAttemptCount;
			int _failedPasswordAnswerAttemptCount;
			bool _isApproved;
			DateTime _lastLoginDate;
			DateTime _lastActivityDate;

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

