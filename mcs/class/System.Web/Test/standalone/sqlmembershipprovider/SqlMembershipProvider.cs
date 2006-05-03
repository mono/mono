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
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Web.Configuration;
using System.Security.Cryptography;
using System.Web.Security;

namespace Toshok.Web.Security {
	public class SqlMembershipProvider : MembershipProvider {

		const int SALT_BYTES = 16;
		DateTime DefaultDateTime = new DateTime (1754,1,1);

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
		DbConnection connection;

		string applicationName;
		
		static object lockobj = new object();

                static byte ToHexValue (char c, bool high)
                {
                        byte v;
                        if (c >= '0' && c <= '9')
                                v = (byte) (c - '0');
                        else if (c >= 'a' && c <= 'f')
                                v = (byte) (c - 'a' + 10);
                        else if (c >= 'A' && c <= 'F')
                                v = (byte) (c - 'A' + 10);
                        else
                                throw new ArgumentException ("Invalid hex character");

                        if (high)
                                v <<= 4;

                        return v;
                }

                internal static byte [] GetBytes (string key, int len)
                {
                        byte [] result = new byte [len / 2];
                        for (int i = 0; i < len; i += 2)
                                result [i / 2] = (byte) (ToHexValue (key [i], true) + ToHexValue (key [i + 1], false));

                        return result;
                }

		SymmetricAlgorithm GetAlg (out byte[] decryptionKey)
		{
			MachineKeySection section = (MachineKeySection)WebConfigurationManager.GetSection ("system.web/machineKey");

			if (section.DecryptionKey.StartsWith ("AutoGenerate"))
				throw new ProviderException ("You must explicitly specify a decryption key in the <machineKey> section when using encrypted passwords.");

			string alg_type = section.Decryption;
			if (alg_type == "Auto")
				alg_type = "AES";

			SymmetricAlgorithm alg = null;
			if (alg_type == "AES")
				alg = Rijndael.Create ();
			else if (alg_type == "3DES")
				alg = TripleDES.Create ();
			else
				throw new ProviderException (String.Format ("Unsupported decryption attribute '{0}' in <machineKey> configuration section", alg_type));

			decryptionKey = GetBytes (section.DecryptionKey, section.DecryptionKey.Length);
			return alg;
		}

		internal string EncodePassword (string password, MembershipPasswordFormat passwordFormat, string salt)
		{
			byte[] password_bytes;
			byte[] salt_bytes;

			switch (passwordFormat) {
			case MembershipPasswordFormat.Clear:
				return password;
			case MembershipPasswordFormat.Hashed:
				password_bytes = Encoding.Unicode.GetBytes (password);
				salt_bytes = Convert.FromBase64String (salt);

				byte[] hashBytes = new byte[salt_bytes.Length + password_bytes.Length];

				Buffer.BlockCopy (salt_bytes, 0, hashBytes, 0, salt_bytes.Length);
				Buffer.BlockCopy (password_bytes, 0, hashBytes, salt_bytes.Length, password_bytes.Length);

				MembershipSection section = (MembershipSection)WebConfigurationManager.GetSection ("system.web/membership");
				string alg_type = section.HashAlgorithmType;
				if (alg_type == "") {
					MachineKeySection keysection = (MachineKeySection)WebConfigurationManager.GetSection ("system.web/machineKey");
					alg_type = keysection.Validation.ToString ();
				}
				using (HashAlgorithm hash = HashAlgorithm.Create (alg_type)) {
					hash.TransformFinalBlock (hashBytes, 0, hashBytes.Length);
					return Convert.ToBase64String (hash.Hash);
				}
			case MembershipPasswordFormat.Encrypted:
				password_bytes = Encoding.Unicode.GetBytes (password);
				salt_bytes = Convert.FromBase64String (salt);

				byte[] buf = new byte[password_bytes.Length + salt_bytes.Length];

				Array.Copy (salt_bytes, 0, buf, 0, salt_bytes.Length);
				Array.Copy (password_bytes, 0, buf, salt_bytes.Length, password_bytes.Length);

				return Convert.ToBase64String (EncryptPassword (buf));
			default:
				/* not reached.. */
				return null;
			}
		}

		internal string DecodePassword (string password, MembershipPasswordFormat passwordFormat)
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

		internal static DbProviderFactory GetDbProviderFactory (string providerName)
		{
			DbProviderFactory f = null;

			if (providerName != null && providerName != "") {
				try {
					f = DbProviderFactories.GetFactory(providerName);
				}
				catch (Exception e) { Console.WriteLine (e); /* nada */ }
				if (f != null)
					return f;
			}

			return SqlClientFactory.Instance;
		}

		void InitConnection ()
		{
			if (connection == null) {
				lock (lockobj) {
					if (connection != null)
						return;

					factory = GetDbProviderFactory (connectionString.ProviderName);
					connection = factory.CreateConnection();
					connection.ConnectionString = connectionString.ConnectionString;

					connection.Open ();
				}
			}
		}

		void AddParameter (DbCommand command, string parameterName, string parameterValue)
		{
			DbParameter dbp = command.CreateParameter ();
			dbp.ParameterName = parameterName;
			dbp.Value = parameterValue;
			dbp.Direction = ParameterDirection.Input;
			command.Parameters.Add (dbp);
		}

		void CheckParam (string pName, string p, int length)
		{
			if (p == null)
				throw new ArgumentNullException (pName);
			if (p.Length == 0 || p.Length > length || p.IndexOf (",") != -1)
				throw new ArgumentException (String.Format ("invalid format for {0}", pName));
		}

                protected override byte[] DecryptPassword (byte[] encodedPassword)
                {
			byte[] decryptionKey;
			SymmetricAlgorithm alg = GetAlg (out decryptionKey);

			alg.Key = decryptionKey;
			ICryptoTransform decryptor = alg.CreateDecryptor ();

			byte[] buf = decryptor.TransformFinalBlock (encodedPassword, 0, encodedPassword.Length);
			byte[] rv = new byte[buf.Length - SALT_BYTES];

			Array.Copy (buf, 16, rv, 0, buf.Length - 16);

			return rv;
                }

                protected override byte[] EncryptPassword (byte[] password)
                {
			byte[] decryptionKey;
			byte[] iv = new byte[SALT_BYTES];

			Array.Copy (password, 0, iv, 0, SALT_BYTES);
			Array.Clear (password, 0, SALT_BYTES);

			SymmetricAlgorithm alg = GetAlg (out decryptionKey);
			ICryptoTransform encryptor = alg.CreateEncryptor (decryptionKey, iv);

			return encryptor.TransformFinalBlock (password, 0, password.Length);
		}

		public override bool ChangePassword (string username, string oldPwd, string newPwd)
		{
			if (username != null) username = username.Trim ();
			if (oldPwd != null) oldPwd = oldPwd.Trim ();
			if (newPwd != null) newPwd = newPwd.Trim ();

			CheckParam ("username", username, 256);
			CheckParam ("oldPwd", oldPwd, 128);
			CheckParam ("newPwd", newPwd, 128);

			MembershipUser user = GetUser (username, false);
			if (user == null) throw new ProviderException ("could not find user in membership database");
			if (user.IsLockedOut) throw new MembershipPasswordException ("user is currently locked out");

			InitConnection();

			DbTransaction trans = connection.BeginTransaction ();

			string commandText;
			DbCommand command;

			try {
				MembershipPasswordFormat passwordFormat;
				string db_salt;

				bool valid = ValidateUsingPassword (trans, username, oldPwd, out passwordFormat, out db_salt);
				if (valid) {

					EmitValidatingPassword (username, newPwd, false);

					string db_password = EncodePassword (newPwd, passwordFormat, db_salt);

					DateTime now = DateTime.Now.ToUniversalTime ();

					commandText = @"
UPDATE m
   SET Password = @Password,
       FailedPasswordAttemptCount = 0,
       FailedPasswordAttemptWindowStart = @DefaultDateTime,
       LastPasswordChangedDate = @Now
  FROM dbo.aspnet_Membership m, dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "UserName", user.UserName);
					AddParameter (command, "Now", now.ToString ());
					AddParameter (command, "Password", db_password);
					AddParameter (command, "ApplicationName", ApplicationName);
					AddParameter (command, "DefaultDateTime", DefaultDateTime.ToString());

					if (1 != (int)command.ExecuteNonQuery ())
						throw new ProviderException ("failed to update Membership table");
				}

				trans.Commit ();
				return valid;
			}
			catch (ProviderException) {
				trans.Rollback ();
				throw;
			}
			catch (Exception e) {
				trans.Rollback ();
				throw new ProviderException ("error changing password", e);
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

			MembershipUser user = GetUser (username, false);
			if (user == null) throw new ProviderException ("could not find user in membership database");
			if (user.IsLockedOut) throw new MembershipPasswordException ("user is currently locked out");

			InitConnection();

			DbTransaction trans = connection.BeginTransaction ();

			string commandText;
			DbCommand command;

			try {
				MembershipPasswordFormat passwordFormat;
				string db_salt;

				bool valid = ValidateUsingPassword (trans, username, password, out passwordFormat, out db_salt);
				if (valid) {

					string db_passwordAnswer = EncodePassword (newPwdAnswer, passwordFormat, db_salt);

					commandText = @"
UPDATE m
   SET PasswordQuestion = @PasswordQuestion,
       PasswordAnswer = @PasswordAnswer,
       FailedPasswordAttemptCount = 0,
       FailedPasswordAttemptWindowStart = @DefaultDateTime,
       FailedPasswordAnswerAttemptCount = 0,
       FailedPasswordAnswerAttemptWindowStart = @DefaultDateTime
  FROM dbo.aspnet_Membership m, dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "UserName", user.UserName);
					AddParameter (command, "PasswordQuestion", newPwdQuestion);
					AddParameter (command, "PasswordAnswer", db_passwordAnswer);
					AddParameter (command, "ApplicationName", ApplicationName);
					AddParameter (command, "DefaultDateTime", DefaultDateTime.ToString());

					if (1 != (int)command.ExecuteNonQuery ())
						throw new ProviderException ("failed to update Membership table");

				}

				trans.Commit ();
				return valid;
			}
			catch (ProviderException) {
				trans.Rollback ();
				throw;
			}
			catch (Exception e) {
				trans.Rollback ();
				throw new ProviderException ("error changing password question and answer", e);
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
			if (providerUserKey != null && ! (providerUserKey is Guid)) {
				status = MembershipCreateStatus.InvalidProviderUserKey;
				return null;
			}

			/* encode our password/answer using the
			 * "passwordFormat" configuration option */
			string passwordSalt = "";

			RandomNumberGenerator rng = RandomNumberGenerator.Create ();
			byte[] salt = new byte[SALT_BYTES];
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

			InitConnection();

			DbTransaction trans = connection.BeginTransaction ();

			string commandText;
			DbCommand command;

			try {

				Guid applicationId;
				Guid userId;

				/* get the application id since it seems that inside transactions we
				   can't insert using subqueries.. */

				commandText = @"
SELECT ApplicationId
  FROM dbo.aspnet_Applications
 WHERE dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
";
				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "ApplicationName", ApplicationName);

				DbDataReader reader = command.ExecuteReader ();
				reader.Read ();
				applicationId = reader.GetGuid (0);
				reader.Close ();

				/* check for unique username, email and
				 * provider user key, if applicable */

				commandText = @"
SELECT COUNT(*)
  FROM dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE u.LoweredUserName = LOWER(@UserName)
   AND u.ApplicationId = a.ApplicationId
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);
				AddParameter (command, "ApplicationName", ApplicationName);

				if (0 != (int)command.ExecuteScalar()) {
					status = MembershipCreateStatus.DuplicateUserName;
					trans.Rollback ();
					return null;
				}


				if (requiresUniqueEmail) {
					commandText = @"
SELECT COUNT(*)
  FROM dbo.aspnet_Membership, dbo.aspnet_Applications
 WHERE dbo.aspnet_Membership.Email = @Email
   AND dbo.aspnet_Membership.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "Email", email);
					AddParameter (command, "ApplicationName", ApplicationName);

					if (0 != (int)command.ExecuteScalar()) {
						status = MembershipCreateStatus.DuplicateEmail;
						trans.Rollback ();
						return null;
					}
		 		}

				if (providerUserKey != null) {
					commandText = @"
SELECT COUNT(*)
  FROM dbo.aspnet_Membership, dbo.aspnet_Applications
 WHERE dbo.aspnet_Membership.UserId = @ProviderUserKey
   AND dbo.aspnet_Membership.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "Email", email);
					AddParameter (command, "ApplicationName", ApplicationName);

					if (0 != (int)command.ExecuteScalar()) {
						status = MembershipCreateStatus.DuplicateProviderUserKey;
						trans.Rollback ();
						return null;
					}
				}

				/* first into the Users table */
				commandText = @"
INSERT into dbo.aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, LastActivityDate)
VALUES (@ApplicationId, NEWID(), @UserName, LOWER(@UserName), GETDATE())
";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);
				AddParameter (command, "ApplicationId", applicationId.ToString());

				if (command.ExecuteNonQuery() != 1) {
					status = MembershipCreateStatus.UserRejected; /* XXX */
					trans.Rollback ();
					return null;
				}

				/* then get the newly created userid */

				commandText = @"
SELECT UserId
  FROM dbo.aspnet_Users
 WHERE dbo.aspnet_Users.LoweredUserName = LOWER(@UserName)
";
				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);

				reader = command.ExecuteReader ();
				reader.Read ();
				userId = reader.GetGuid (0);
				reader.Close ();

				/* then insert into the Membership table */
				commandText = String.Format (@"
INSERT into dbo.aspnet_Membership
VALUES (@ApplicationId,
        @UserId,
        @Password, @PasswordFormat, @PasswordSalt,
        NULL,
        {0}, {1},
        {2}, {3},
        0, 0,
        GETDATE(), GETDATE(), @DefaultDateTime,
        @DefaultDateTime,
        0, @DefaultDateTime, 0, @DefaultDateTime, NULL)",
							     email == null ? "NULL" : "@Email",
							     email == null ? "NULL" : "LOWER(@Email)",
							     pwdQuestion == null ? "NULL" : "@PasswordQuestion",
							     pwdAnswer == null ? "NULL" : "@PasswordAnswer");

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "ApplicationId", applicationId.ToString());
				AddParameter (command, "UserId", userId.ToString());
				if (email != null)
					AddParameter (command, "Email", email);
				AddParameter (command, "Password", password);
				AddParameter (command, "PasswordFormat", ((int)PasswordFormat).ToString());
				AddParameter (command, "PasswordSalt", passwordSalt);
				if (pwdQuestion != null)
					AddParameter (command, "PasswordQuestion", pwdQuestion);
				if (pwdAnswer != null)
					AddParameter (command, "PasswordAnswer", pwdAnswer);
				AddParameter (command, "DefaultDateTime", DefaultDateTime.ToString());

				if (command.ExecuteNonQuery() != 1) {
					status = MembershipCreateStatus.UserRejected; /* XXX */
					return null;
				}

				trans.Commit ();

				status = MembershipCreateStatus.Success;

				return GetUser (username, false);
			}
			catch {
				status = MembershipCreateStatus.ProviderError;
				trans.Rollback ();
				return null;
			}
		}
		
		public override bool DeleteUser (string username, bool deleteAllRelatedData)
		{
			CheckParam ("username", username, 256);

			if (deleteAllRelatedData) {
				/* delete everything from the
				 * following features as well:
				 *
				 * Roles
				 * Profile
				 * WebParts Personalization
				 */
			}

			DbTransaction trans = connection.BeginTransaction ();

			DbCommand command;
			string commandText;

			InitConnection();

			try {
				/* delete from the Membership table */
				commandText = @"
DELETE dbo.aspnet_Membership
  FROM dbo.aspnet_Membership, dbo.aspnet_Users, dbo.aspnet_Applications
 WHERE dbo.aspnet_Membership.UserId = dbo.aspnet_Users.UserId
   AND dbo.aspnet_Membership.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Users.LoweredUserName = LOWER (@UserName)
   AND dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);
				AddParameter (command, "ApplicationName", ApplicationName);

				if (1 != command.ExecuteNonQuery())
					throw new ProviderException ("failed to delete from Membership table");

				/* delete from the User table */
				commandText = @"
DELETE dbo.aspnet_Users
  FROM dbo.aspnet_Users, dbo.aspnet_Applications
 WHERE dbo.aspnet_Users.LoweredUserName = LOWER (@UserName)
   AND dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);
				AddParameter (command, "ApplicationName", ApplicationName);

				if (1 != command.ExecuteNonQuery())
					throw new ProviderException ("failed to delete from User table");

				trans.Commit ();

				return true;
			}
			catch {
				trans.Rollback ();
				return false;
			}
		}
		
		public virtual string GeneratePassword ()
		{
			return Membership.GeneratePassword (minRequiredPasswordLength, minRequiredNonAlphanumericCharacters);
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

			string commandText;

			InitConnection();

			commandText = @"
SELECT u.UserName, m.UserId, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
       m.IsLockedOut, m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       m.LastPasswordChangedDate, m.LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND m.Email LIKE @Email
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "Email", emailToMatch);
			AddParameter (command, "ApplicationName", ApplicationName);

			MembershipUserCollection c = BuildMembershipUserCollection (command, pageIndex, pageSize, out totalRecords);

			return c;
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

			string commandText;

			InitConnection();

			commandText = @"
SELECT u.UserName, m.UserId, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
       m.IsLockedOut, m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       m.LastPasswordChangedDate, m.LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.UserName LIKE @UserName
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserName", nameToMatch);
			AddParameter (command, "ApplicationName", ApplicationName);

			MembershipUserCollection c = BuildMembershipUserCollection (command, pageIndex, pageSize, out totalRecords);

			return c;
		}
		
		public override MembershipUserCollection GetAllUsers (int pageIndex, int pageSize, out int totalRecords)
		{
			if (pageIndex < 0)
				throw new ArgumentException ("pageIndex must be >= 0");
			if (pageSize < 0)
				throw new ArgumentException ("pageSize must be >= 0");
			if (pageIndex * pageSize + pageSize - 1 > Int32.MaxValue)
				throw new ArgumentException ("pageIndex and pageSize are too large");

			string commandText;

			InitConnection();

			commandText = @"
SELECT u.UserName, m.UserId, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
       m.IsLockedOut, m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       m.LastPasswordChangedDate, m.LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "ApplicationName", ApplicationName);

			MembershipUserCollection c = BuildMembershipUserCollection (command, pageIndex, pageSize, out totalRecords);

			return c;
		}

		MembershipUserCollection BuildMembershipUserCollection (DbCommand command, int pageIndex, int pageSize, out int totalRecords)
		{
			DbDataReader reader = null;
			try {
				int num_read = 0;
				int num_added = 0;
				int num_to_skip = pageIndex * pageSize;
				MembershipUserCollection users = new MembershipUserCollection ();
				reader = command.ExecuteReader ();
				while (reader.Read()) {
					if (num_read >= num_to_skip) {
						if (num_added < pageSize) {
							users.Add (GetUserFromReader (reader));
							num_added ++;
						}
						num_read ++;
					}
				}
				totalRecords = num_read;
				return users;
			}
			catch {
				totalRecords = 0;
				return null; /* should we let the exception through? */
			}
			finally {
				if (reader != null)
					reader.Close();
			}
		}
		
		
		public override int GetNumberOfUsersOnline ()
		{
			string commandText;

			InitConnection();

			DateTime now = DateTime.Now.ToUniversalTime ();

			commandText = String.Format (@"
SELECT COUNT (*)
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND DATEADD(minute,{0},u.LastActivityDate) >= @Now
   AND a.LoweredApplicationName = LOWER(@ApplicationName)",
						     userIsOnlineTimeWindow.Minutes);

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "Now", now.ToString ());
			AddParameter (command, "ApplicationName", ApplicationName);

			try { 
				return (int)command.ExecuteScalar ();
			}
			catch (Exception e) {
				Console.WriteLine (e);
				return -1;
			}
		}
		
		public override string GetPassword (string username, string answer)
		{
			if (!enablePasswordRetrieval)
				throw new NotSupportedException ("this provider has not been configured to allow the retrieval of passwords");

			CheckParam ("username", username, 256);
			if (RequiresQuestionAndAnswer)
				CheckParam ("answer", answer, 128);

			MembershipUser user = GetUser (username, false);
			if (user == null) throw new ProviderException ("could not find user in membership database");
			if (user.IsLockedOut) throw new MembershipPasswordException ("user is currently locked out");

			InitConnection();

			DbTransaction trans = connection.BeginTransaction ();

			try {
				MembershipPasswordFormat passwordFormat;
				string salt;
				string password = null;

				if (ValidateUsingPasswordAnswer (trans, username, answer,
								 out passwordFormat, out salt)) {


					/* if the validation succeeds:

					   set LastLoginDate to DateTime.Now
					   set FailedPasswordAnswerAttemptCount to 0
					   set FailedPasswordAnswerAttemptWindowStart to DefaultDateTime
					*/

					string commandText = @"
SELECT m.Password
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

					DbCommand command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "UserName", username);
					AddParameter (command, "ApplicationName", ApplicationName);

					DbDataReader reader = command.ExecuteReader ();
					reader.Read ();
					password = reader.GetString (0);
					reader.Close();

					password = DecodePassword (password, passwordFormat);
				}
				else {
					throw new MembershipPasswordException ("The password-answer supplied is wrong.");
				}

				trans.Commit ();
				return password;
			}
			catch (MembershipPasswordException) {
				trans.Commit ();
				throw;
			}
			catch {
				trans.Rollback ();
				throw;
			}
		}

		MembershipUser GetUserFromReader (DbDataReader reader)
		{
			return new MembershipUser (this.Name, /* XXX is this right?  */
						   reader.GetString (0), /* name */
						   reader.GetGuid (1), /* providerUserKey */
						   reader.IsDBNull (2) ? null : reader.GetString (2), /* email */
						   reader.IsDBNull (3) ? null : reader.GetString (3), /* passwordQuestion */
						   reader.IsDBNull (4) ? null : reader.GetString (4), /* comment */
						   reader.GetBoolean (5), /* isApproved */
						   reader.GetBoolean (6), /* isLockedOut */
						   reader.GetDateTime (7).ToLocalTime (), /* creationDate */
						   reader.GetDateTime (8).ToLocalTime (), /* lastLoginDate */
						   reader.GetDateTime (9).ToLocalTime (), /* lastActivityDate */
						   reader.GetDateTime (10).ToLocalTime (), /* lastPasswordChangedDate */
						   reader.GetDateTime (11).ToLocalTime () /* lastLockoutDate */);
		}

		MembershipUser BuildMembershipUser (DbCommand query, bool userIsOnline)
		{
			DbDataReader reader = null;
			try {
				reader = query.ExecuteReader ();
				if (!reader.Read ())
					return null;

				MembershipUser user = GetUserFromReader (reader);

				if (user != null && userIsOnline) {

					string commandText;
					DbCommand command;

					commandText = @"
UPDATE dbo.aspnet_Users u, dbo.aspnet_Application a
   SET u.LastActivityDate = GETDATE()
 WHERE u.ApplicationId = a.ApplicationId
   AND u.UserName = @UserName
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "UserName", user.UserName);
					AddParameter (command, "ApplicationName", ApplicationName);

					command.ExecuteNonQuery();
				}

				return user;
			}
			catch {
				return null; /* should we let the exception through? */
			}
			finally {
				if (reader != null)
					reader.Close ();
			}
		}

		public override MembershipUser GetUser (string username, bool userIsOnline)
		{
			CheckParam ("username", username, 256);

			string commandText;
			DbCommand command;

			InitConnection();

			commandText = @"
SELECT u.UserName, m.UserId, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
       m.IsLockedOut, m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       m.LastPasswordChangedDate, m.LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.UserName = @UserName
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserName", username);
			AddParameter (command, "ApplicationName", ApplicationName);

			MembershipUser u = BuildMembershipUser (command, userIsOnline);

			return u;
		}
		
		public override MembershipUser GetUser (object providerUserKey, bool userIsOnline)
		{
			string commandText;
			DbCommand command;

			InitConnection();

			commandText = @"
SELECT u.UserName, m.UserId, m.Email, m.PasswordQuestion, m.Comment, m.IsApproved,
       m.IsLockedOut, m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       m.LastPasswordChangedDate, m.LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.UserId = @UserKey
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserKey", providerUserKey.ToString());
			AddParameter (command, "ApplicationName", ApplicationName);

			MembershipUser u = BuildMembershipUser (command, userIsOnline);

			return u;
		}
		
		public override string GetUserNameByEmail (string email)
		{
			CheckParam ("email", email, 256);

			string commandText;
			DbCommand command;

			InitConnection();

			commandText = @"
SELECT u.UserName
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND m.Email = @Email
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "Email", email);
			AddParameter (command, "ApplicationName", ApplicationName);

			try {
				DbDataReader reader = command.ExecuteReader ();
				string rv = null;
				while (reader.Read())
					rv = reader.GetString(0);
				reader.Close();
				return rv;
			}
			catch {
				return null; /* should we allow the exception through? */
			}
		}

		bool GetBoolConfigValue (NameValueCollection config, string name, bool def)
		{
			bool rv = def;
			string val = config[name];
			if (val != null) {
				try { rv = Boolean.Parse (val); }
				catch (Exception e) {
					throw new ProviderException (String.Format ("{0} must be true or false", name), e); }
			}
			return rv;
		}

		int GetIntConfigValue (NameValueCollection config, string name, int def)
		{
			int rv = def;
			string val = config[name];
			if (val != null) {
				try { rv = Int32.Parse (val); }
				catch (Exception e) {
					throw new ProviderException (String.Format ("{0} must be an integer", name), e); }
			}
			return rv;
		}

		int GetEnumConfigValue (NameValueCollection config, string name, Type enumType, int def)
		{
			int rv = def;
			string val = config[name];
			if (val != null) {
				try { rv = (int)Enum.Parse (enumType, val); }
				catch (Exception e) {
					throw new ProviderException (String.Format ("{0} must be one of the following values: {1}", name, String.Join (",", Enum.GetNames (enumType))), e); }
			}
			return rv;
		}

		string GetStringConfigValue (NameValueCollection config, string name, string def)
		{
			string rv = def;
			string val = config[name];
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
			passwordFormat = (MembershipPasswordFormat)GetEnumConfigValue (config, "passwordFormat", typeof (MembershipPasswordFormat),
										       (int)MembershipPasswordFormat.Hashed);
			maxInvalidPasswordAttempts = GetIntConfigValue (config, "maxInvalidPasswordAttempts", 5);
			minRequiredPasswordLength = GetIntConfigValue (config, "minRequiredPasswordLength", 7);
			minRequiredNonAlphanumericCharacters = GetIntConfigValue (config, "minRequiredNonAlphanumericCharacters", 1);
			passwordAttemptWindow = GetIntConfigValue (config, "passwordAttemptWindow", 10);
			passwordStrengthRegularExpression = GetStringConfigValue (config, "passwordStrengthRegularExpression", "");

			MembershipSection section = (MembershipSection)WebConfigurationManager.GetSection ("system.web/membership");
			
			userIsOnlineTimeWindow = section.UserIsOnlineTimeWindow;

			/* we can't support password retrieval with hashed passwords */
			if (passwordFormat == MembershipPasswordFormat.Hashed && enablePasswordRetrieval)
				throw new ProviderException ("password retrieval cannot be used with hashed passwords");

			string connectionStringName = config["connectionStringName"];

			if (applicationName.Length > 256)
				throw new ProviderException ("The ApplicationName attribute must be 256 characters long or less.");
			if (connectionStringName == null || connectionStringName.Length == 0)
				throw new ProviderException ("The ConnectionStringName attribute must be present and non-zero length.");

			connectionString = WebConfigurationManager.ConnectionStrings[connectionStringName];
		}

		public override string ResetPassword (string username, string answer)
		{
			if (!enablePasswordReset)
				throw new NotSupportedException ("this provider has not been configured to allow the resetting of passwords");

			CheckParam ("username", username, 256);
			if (RequiresQuestionAndAnswer)
				CheckParam ("answer", answer, 128);

			MembershipUser user = GetUser (username, false);
			if (user == null) throw new ProviderException ("could not find user in membership database");
			if (user.IsLockedOut) throw new MembershipPasswordException ("user is currently locked out");

			InitConnection();

			string commandText;
			DbCommand command;

			DbTransaction trans = connection.BeginTransaction ();

			try {
				MembershipPasswordFormat db_passwordFormat;
				string db_salt;
				string newPassword = null;

				if (ValidateUsingPasswordAnswer (trans, user.UserName, answer, out db_passwordFormat, out db_salt)) {

					newPassword = GeneratePassword ();
					string db_password;

					EmitValidatingPassword (username, newPassword, false);

					/* otherwise update the user's password in the db */

					db_password = EncodePassword (newPassword, db_passwordFormat, db_salt);

					commandText = @"
UPDATE m
   SET Password = @Password,
       FailedPasswordAnswerAttemptCount = 0,
       FailedPasswordAnswerAttemptWindowStart = @DefaultDateTime
  FROM dbo.aspnet_Membership m, dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "UserName", user.UserName);
					AddParameter (command, "Password", db_password);
					AddParameter (command, "ApplicationName", ApplicationName);
					AddParameter (command, "DefaultDateTime", DefaultDateTime.ToString());

					if (1 != (int)command.ExecuteNonQuery ())
						throw new ProviderException ("failed to update Membership table");

					trans.Commit ();
				}
				else {
					throw new MembershipPasswordException ("The password-answer supplied is wrong.");
				}

				return newPassword;
			}
			catch (MembershipPasswordException) {
				trans.Commit ();
				throw;
			}
			catch (ProviderException) {
				trans.Rollback ();
				throw;
			}
			catch (Exception e) {
				trans.Rollback ();

				throw new ProviderException ("Failed to reset password", e);
			}
		}
		
		public override void UpdateUser (MembershipUser user)
		{
			if (user == null) throw new ArgumentNullException ("user");
			if (user.UserName == null) throw new ArgumentNullException ("user.UserName");
			if (RequiresUniqueEmail && user.Email == null) throw new ArgumentNullException ("user.Email");

			CheckParam ("user.UserName", user.UserName, 256);
			if (user.Email.Length > 256 || (RequiresUniqueEmail && user.Email.Length == 0))
				throw new ArgumentException ("invalid format for user.Email");

			DbTransaction trans = connection.BeginTransaction ();

			string commandText;
			DbCommand command;

			InitConnection();

			try {
				DateTime now = DateTime.Now.ToUniversalTime ();

				commandText = String.Format (@"
UPDATE m
   SET Email = {0},
       Comment = {1},
       IsApproved = @IsApproved,
       LastLoginDate = @Now
  FROM dbo.aspnet_Membership m, dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)",
							     user.Email == null ? "NULL" : "@Email",
							     user.Comment == null ? "NULL" : "@Comment");

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				if (user.Email != null)
					AddParameter (command, "Email", user.Email);
				if (user.Comment != null)
					AddParameter (command, "Comment", user.Comment);
				AddParameter (command, "IsApproved", user.IsApproved.ToString());
				AddParameter (command, "UserName", user.UserName);
				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "Now", now.ToString ());

				if (0 == command.ExecuteNonQuery())
					throw new ProviderException ("failed to membership table");


				commandText = @"
UPDATE dbo.aspnet_Users
   SET LastActivityDate = @Now
  FROM dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE a.ApplicationId = a.ApplicationId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", user.UserName);
				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "Now", now.ToString ());

				if (0 == command.ExecuteNonQuery())
					throw new ProviderException ("failed to user table");

				trans.Commit ();
			}
			catch (ProviderException) {
				trans.Rollback ();
				throw;
			}
			catch (Exception e) {
				trans.Rollback ();
				throw new ProviderException ("failed to update user", e);
			}
		}
		
		public override bool ValidateUser (string username, string password)
		{
			MembershipUser user = GetUser (username, false);

			/* if the user is locked out, return false immediately */
			if (user.IsLockedOut)
				return false;

			/* if the user is not yet approved, return false */
			if (!user.IsApproved)
				return false;

			EmitValidatingPassword (username, password, false);

			InitConnection();

			DbTransaction trans = connection.BeginTransaction ();

			string commandText;
			DbCommand command;

			try {
				MembershipPasswordFormat passwordFormat;
				string salt;

				bool valid = ValidateUsingPassword (trans, username, password, out passwordFormat, out salt);
				if (valid) {

					DateTime now = DateTime.Now.ToUniversalTime ();

					/* if the validation succeeds:
					   set LastLoginDate to DateTime.Now
					   set FailedPasswordAttemptCount to 0
					   set FailedPasswordAttemptWindow to DefaultDateTime
					   set FailedPasswordAnswerAttemptCount to 0
					   set FailedPasswordAnswerAttemptWindowStart to DefaultDateTime
					*/

					commandText = @"
UPDATE dbo.aspnet_Membership
   SET LastLoginDate = @Now,
       FailedPasswordAttemptCount = 0,
       FailedPasswordAttemptWindowStart = @DefaultDateTime,
       FailedPasswordAnswerAttemptCount = 0,
       FailedPasswordAnswerAttemptWindowStart = @DefaultDateTime
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "UserName", user.UserName);
					AddParameter (command, "ApplicationName", ApplicationName);
					AddParameter (command, "Now", now.ToString ());
					AddParameter (command, "DefaultDateTime", DefaultDateTime.ToString());

					if (1 != (int)command.ExecuteNonQuery ())
						throw new ProviderException ("failed to update Membership table");

					commandText = @"
UPDATE dbo.aspnet_Users
   SET LastActivityDate = @Now
  FROM dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE u.ApplicationId = a.ApplicationId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

					command = factory.CreateCommand ();
					command.Transaction = trans;
					command.CommandText = commandText;
					command.Connection = connection;
					command.CommandType = CommandType.Text;
					AddParameter (command, "UserName", user.UserName);
					AddParameter (command, "ApplicationName", ApplicationName);
					AddParameter (command, "Now", now.ToString ());

					if (1 != (int)command.ExecuteNonQuery ())
						throw new ProviderException ("failed to update User table");
				}

				trans.Commit ();

				return valid;
			}
			catch (Exception e) {
				Console.WriteLine (e);

				trans.Rollback ();

				throw;
			}
		}

		public override bool UnlockUser (string userName)
		{
			CheckParam ("userName", userName, 256);

			string commandText = @"
UPDATE dbo.aspnet_Membership
   SET IsLockedOut = 0,
       LastLockoutDate = @DefaultDateTime,
       FailedPasswordAttemptCount = 0,
       FailedPasswordAttemptWindowStart = @DefaultDateTime,
       FailedPasswordAnswerAttemptCount = 0,
       FailedPasswordAnswerAttemptWindowStart = @DefaultDateTime
  FROM dbo.aspnet_Membership m, dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE m.UserId = u.UserId
   AND m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND u.LoweredUserName = LOWER (@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			InitConnection();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserName", userName);
			AddParameter (command, "ApplicationName", ApplicationName);
			AddParameter (command, "DefaultDateTime", DefaultDateTime.ToString());

			return command.ExecuteNonQuery() == 1;
		}

		void IncrementFailureAndMaybeLockout (DbTransaction trans, string username,
						      string failureCountAttribute, string failureWindowAttribute)
		{
			DateTime now = DateTime.Now;

			/* if validation fails:
			   if (FailedPasswordAttemptWindowStart - DateTime.Now < PasswordAttemptWindow)
			     increment FailedPasswordAttemptCount
			   FailedPasswordAttemptWindowStart = DateTime.Now
			   if (FailedPasswordAttemptCount > MaxInvalidPasswordAttempts)
			     set IsLockedOut = true.
			     set LastLockoutDate = DateTime.Now
			*/

			string commandText = String.Format (@"
SELECT m.{0}, m.{1}
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)",
						     failureCountAttribute, failureWindowAttribute);

			DbCommand command = factory.CreateCommand ();
			command.Transaction = trans;
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserName", username);
			AddParameter (command, "ApplicationName", ApplicationName);

			DateTime db_FailedWindowStart;
			int db_FailedCount;

			DbDataReader reader = command.ExecuteReader ();
			reader.Read ();
			db_FailedCount = reader.GetInt32 (0);
			db_FailedWindowStart = reader.GetDateTime (1).ToLocalTime ();
			reader.Close();

			TimeSpan diff = now.Subtract (db_FailedWindowStart);
			if ((db_FailedWindowStart == DefaultDateTime.ToLocalTime ())
			    || diff.Minutes < PasswordAttemptWindow)
				db_FailedCount ++;

			if (db_FailedCount > MaxInvalidPasswordAttempts) {
				/* lock the user out */
				commandText = @"
UPDATE dbo.aspnet_Membership
   SET IsLockedOut = 1,
       LastLockoutDate = @LastLockoutDate
  FROM dbo.aspnet_Membership m, dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);
				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "LastLockoutDate", now.ToUniversalTime().ToString ());
			}
			else {
				/* just store back the updated window start and count */
				commandText = String.Format (@"
UPDATE dbo.aspnet_Membership
   SET {0} = @{0},
       {1} = @{1}
  FROM dbo.aspnet_Membership m, dbo.aspnet_Users u, dbo.aspnet_Applications a
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)",
						     failureCountAttribute, failureWindowAttribute);

				command = factory.CreateCommand ();
				command.Transaction = trans;
				command.CommandText = commandText;
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "UserName", username);
				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, failureCountAttribute, db_FailedCount.ToString());
				AddParameter (command, failureWindowAttribute, now.ToUniversalTime().ToString ());
			}

			if (1 != (int)command.ExecuteNonQuery ())
				throw new ProviderException ("failed to update Membership table");
		}

		bool ValidateUsingPassword (DbTransaction trans, string username, string password,
					    out MembershipPasswordFormat passwordFormat,
					    out string salt)
		{
			string commandText = @"
SELECT m.Password, m.PasswordFormat, m.PasswordSalt
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			DbCommand command = factory.CreateCommand ();
			command.Transaction = trans;
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserName", username);
			AddParameter (command, "ApplicationName", ApplicationName);

			string db_password;

			DbDataReader reader = command.ExecuteReader ();
			reader.Read ();
			db_password = reader.GetString (0);
			passwordFormat = (MembershipPasswordFormat)reader.GetInt32 (1);
			salt = reader.GetString (2);
			reader.Close();

			/* do the actual validation */
			password = EncodePassword (password, passwordFormat, salt);

			bool valid = (password == db_password);

			if (!valid)
				IncrementFailureAndMaybeLockout (trans, username,
								 "FailedPasswordAttemptCount", "FailedPasswordAttemptWindowStart");

			return valid;
		}


		bool ValidateUsingPasswordAnswer (DbTransaction trans, string username, string answer,
						  out MembershipPasswordFormat passwordFormat,
						  out string salt)
		{
			string commandText = @"
SELECT m.PasswordAnswer, m.PasswordFormat, m.PasswordSalt
  FROM dbo.aspnet_Membership m, dbo.aspnet_Applications a, dbo.aspnet_Users u
 WHERE m.ApplicationId = a.ApplicationId
   AND u.ApplicationId = a.ApplicationId
   AND m.UserId = u.UserId
   AND u.LoweredUserName = LOWER(@UserName)
   AND a.LoweredApplicationName = LOWER(@ApplicationName)";

			DbCommand command = factory.CreateCommand ();
			command.Transaction = trans;
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserName", username);
			AddParameter (command, "ApplicationName", ApplicationName);

			string db_answer;

			DbDataReader reader = command.ExecuteReader ();
			reader.Read ();
			db_answer = reader.GetString (0);
			passwordFormat = (MembershipPasswordFormat)reader.GetInt32 (1);
			salt = reader.GetString (2);
			reader.Close();

			/* do the actual password answer check */
			answer = EncodePassword (answer, passwordFormat, salt);

			if (answer.Length > 128)
				throw new ArgumentException (String.Format ("password answer hashed to longer than 128 characters"));

			bool valid = (answer == db_answer);

			if (!valid)
				IncrementFailureAndMaybeLockout (trans, username,
								 "FailedPasswordAnswerAttemptCount",
								 "FailedPasswordAnswerAttemptWindowStart");

			return valid;
		}

		public override string ApplicationName {
			get { return applicationName; }
			set { applicationName = value; }
		}
		
		public override bool EnablePasswordReset {
			get { return enablePasswordReset; }
		}
		
		public override bool EnablePasswordRetrieval {
			get { return enablePasswordRetrieval; }
		}
		
		public override MembershipPasswordFormat PasswordFormat {
			get { return passwordFormat; }
		}
		
		public override bool RequiresQuestionAndAnswer {
			get { return requiresQuestionAndAnswer; }
		}
		
		public override bool RequiresUniqueEmail {
			get { return requiresUniqueEmail; }
		}
		
		public override int MaxInvalidPasswordAttempts {
			get { return maxInvalidPasswordAttempts; }
		}
		
		public override int MinRequiredNonAlphanumericCharacters {
			get { return minRequiredNonAlphanumericCharacters; }
		}
		
		public override int MinRequiredPasswordLength {
			get { return minRequiredPasswordLength; }
		}
		
		public override int PasswordAttemptWindow {
			get { return passwordAttemptWindow; }
		}
		
		public override string PasswordStrengthRegularExpression {
			get { return passwordStrengthRegularExpression; }
		}
	}
}
#endif

