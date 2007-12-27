//
// Mainsoft.Web.Security.DerbyDBSchema
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
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Provider;
using System.Diagnostics;
using Mainsoft.Web.Hosting;

namespace Mainsoft.Web.Security
{
	internal class DerbyDBSchema
	{
		const string _currentSchemaVersion = "1.0";
		static readonly object _lock = new object ();

		#region schema string array
		static string [] schemaElements = new string [] {
			// Applications table
			@"CREATE TABLE aspnet_Applications (
				ApplicationId                           char(36)            NOT NULL PRIMARY KEY,
				ApplicationName                         varchar(256)        NOT NULL UNIQUE,
				LoweredApplicationName                  varchar(256)        NOT NULL UNIQUE,
				Description                             varchar(256)
			)",
			@"CREATE INDEX aspnet_App_Idx ON aspnet_Applications(LoweredApplicationName)",

			// Users table
			@"CREATE TABLE aspnet_Users (
				ApplicationId                           char(36)            NOT NULL,
				UserId                                  char(36)            NOT NULL PRIMARY KEY,
				UserName                                varchar(256)        NOT NULL,
				LoweredUserName                         varchar(256)        NOT NULL,
				MobileAlias                             varchar(16)         DEFAULT NULL,
				IsAnonymous                             int                 NOT NULL DEFAULT 0,
				LastActivityDate                        timestamp           NOT NULL,

				CONSTRAINT Users_AppId_PK FOREIGN KEY (ApplicationId) REFERENCES aspnet_Applications(ApplicationId)
			)",
			@"CREATE UNIQUE INDEX aspnet_Usr_Idx ON aspnet_Users(ApplicationId, LoweredUserName)",
			@"CREATE INDEX aspnet_Usr_Idx2 ON aspnet_Users(ApplicationId, LastActivityDate)",

			// Membership table
			@"CREATE TABLE aspnet_Membership (
				ApplicationId                           char(36)            NOT NULL,
				UserId                                  char(36)            NOT NULL PRIMARY KEY, 
				Password                                varchar(128)        NOT NULL,
				PasswordFormat                          int                 NOT NULL DEFAULT 0,
				PasswordSalt                            varchar(128)        NOT NULL,
				MobilePIN                               varchar(16),
				Email                                   varchar(256),
				LoweredEmail                            varchar(256),
				PasswordQuestion                        varchar(256),
				PasswordAnswer                          varchar(128),
				IsApproved                              int                 NOT NULL,
				IsLockedOut                             int                 NOT NULL,
				CreateDate                              timestamp           NOT NULL,
				LastLoginDate                           timestamp           NOT NULL,
				LastPasswordChangedDate                 timestamp           NOT NULL,
				LastLockoutDate                         timestamp           NOT NULL,
				FailedPasswordAttemptCount              int                 NOT NULL,
				FailedPwdAttemptWindowStart             timestamp           NOT NULL,
				FailedPwdAnswerAttemptCount             int                 NOT NULL,
				FailedPwdAnswerAttWindowStart           timestamp           NOT NULL,
				Comment                                 varchar(256), 

				CONSTRAINT Member_AppId_PK FOREIGN KEY (ApplicationId) REFERENCES aspnet_Applications(ApplicationId),
				CONSTRAINT UserId_PK FOREIGN KEY (UserId) REFERENCES aspnet_Users(UserId)
			)",
			@"CREATE INDEX aspnet_Mbr_idx ON aspnet_Membership(ApplicationId, LoweredEmail)",

			// Roles table
			@"CREATE TABLE aspnet_Roles (
				ApplicationId                           char(36)            NOT NULL,
				RoleId                                  char(36)            NOT NULL PRIMARY KEY,
				RoleName                                varchar(256)        NOT NULL,
				LoweredRoleName                         varchar(256)        NOT NULL,
				Description                             varchar(256),

				CONSTRAINT Roles_AppId_PK FOREIGN KEY (ApplicationId) REFERENCES aspnet_Applications(ApplicationId)
			)",
			@"CREATE UNIQUE INDEX aspnet_Rls_idx ON aspnet_Roles(ApplicationId, LoweredRoleName)",

			// UsersInRoles table
			@"CREATE TABLE aspnet_UsersInRoles (
				UserId                                  char(36)            NOT NULL, 
				RoleId                                  char(36)            NOT NULL,

				CONSTRAINT RoleId_UserId_PK FOREIGN KEY (UserId) REFERENCES aspnet_Users (UserId),
				CONSTRAINT UserId_RoleId_PK FOREIGN KEY (RoleId) REFERENCES aspnet_Roles (RoleId)
			)",
			@"ALTER TABLE aspnet_UsersInRoles ADD PRIMARY KEY (UserId, RoleId)",
			@"CREATE INDEX aspnet_UsrRls_idx ON aspnet_UsersInRoles(RoleId)",

			// Profile table
			@"CREATE TABLE aspnet_Profile (
				UserId                                  char(36)            NOT NULL PRIMARY KEY,
				PropertyNames                           long varchar        NOT NULL,
				PropertyValuesString                    long varchar        NOT NULL,
				PropertyValuesBinary                    blob                NOT NULL,
				LastUpdatedDate                         timestamp           NOT NULL,

				CONSTRAINT Profile_UserId_PK FOREIGN KEY (UserId) REFERENCES aspnet_Users (UserId)
			)",

			// Pathes table
			//@"CREATE TABLE aspnet_Paths (
			//	ApplicationId                           char(36)            NOT NULL,
			//	PathId                                  char(36)            NOT NULL PRIMARY KEY,
			//	Path                                    varchar(256)        NOT NULL,
			//	LoweredPath                             varchar(256)        NOT NULL,
			//
			//	CONSTRAINT Paths_AppId_FK FOREIGN KEY (ApplicationId) REFERENCES aspnet_Applications(ApplicationId)
			//)",
			//@"CREATE UNIQUE INDEX aspnet_Pth_idx ON aspnet_Paths(ApplicationId, LoweredPath)",

			// Personalization tables
			//@"CREATE TABLE aspnet_PersonalizationAllUsers (
			//	PathId                                  char(36)            NOT NULL PRIMARY KEY,
			//	PageSettings                            blob                NOT NULL,
			//	LastUpdatedDate                         timestamp           NOT NULL,
			//
			//	CONSTRAINT PrsUsr_PathId_PK FOREIGN KEY (PathId) REFERENCES aspnet_Paths (PathId)
			//)",
			//@"CREATE TABLE aspnet_PersonalizationPerUser (
			//	Id                                      char(36)            NOT NULL PRIMARY KEY,
			//	PathId                                  char(36)            NOT NULL,
			//	UserId                                  char(36)            NOT NULL,
			//	PageSettings                            blob                NOT NULL,
			//	LastUpdatedDate                         timestamp           NOT NULL,
			//
			//	CONSTRAINT PrsPUser_PathId_FK FOREIGN KEY (PathId) REFERENCES aspnet_Paths (PathId),
			//	CONSTRAINT PrsPUser_UserId_FK FOREIGN KEY (UserId) REFERENCES aspnet_Users (UserId)
			//)",
			//@"CREATE UNIQUE INDEX PrsPUser_idx1 ON aspnet_PersonalizationPerUser(PathId,UserId)",
			//@"CREATE UNIQUE INDEX PrsPUser_idx2 ON aspnet_PersonalizationPerUser(UserId,PathId)"

			// Version table
			@"CREATE TABLE aspnet_Version (
				SchemaVersion                           varchar(10)             NOT NULL
			)",
			@"CREATE INDEX aspnet_Version_Idx ON aspnet_Version(SchemaVersion)",
			@"INSERT INTO aspnet_Version VALUES ('1.0')"
		};
		#endregion

		public static void CheckSchema (string connectionString) {
			string schemaVersion = GetSchemaVersion (connectionString);
			if (schemaVersion != null) {
				if (string.CompareOrdinal (schemaVersion, _currentSchemaVersion) == 0)
					return;
			}
			else {
				lock (_lock) {
					schemaVersion = GetSchemaVersion (connectionString);
					if (schemaVersion == null) {
						InitializeSchema (connectionString);
						return;
					}
				}
			}

			throw new ProviderException (String.Format ("Incorrect aspnetdb schema version: found '{0}', expected '{1}'.", schemaVersion, _currentSchemaVersion));
		}

		static string GetSchemaVersion (string connectionString)
		{
			OleDbConnection connection = new OleDbConnection (connectionString);

			connection.Open ();

			using (connection) {
				OleDbCommand cmd = new OleDbCommand ("SELECT SchemaVersion FROM aspnet_Version", connection);

				try {
					using (OleDbDataReader reader = cmd.ExecuteReader ()) {
						if (reader.Read ())
							return reader.GetString (0);
					}
				}
				catch { }

				return null;
			}
		}

		static void InitializeSchema (string connectionString)
		{
			OleDbConnection connection = new OleDbConnection ();
			connection.ConnectionString = connectionString;

			connection.Open ();

			using (connection) {
				for (int i = 0; i < schemaElements.Length; i++) {
					OleDbCommand cmd = new OleDbCommand (schemaElements [i], connection);
					cmd.ExecuteNonQuery ();
				}
			}
		}
	}

	internal class DerbyUnloadManager
	{
		public enum DerbyShutDownPolicy
		{
			Default,
			Never,
			Database,
			System
		}

		readonly string _connectionString;
		readonly DerbyShutDownPolicy _policy;

		DerbyUnloadManager (string connectionString, DerbyShutDownPolicy policy) {
			_connectionString = connectionString;
			_policy = policy;
		}

		public static void RegisterUnloadHandler (string connectionString, DerbyShutDownPolicy policy) {
			if (policy == DerbyShutDownPolicy.Never)
				return;

			if (connectionString.IndexOf("org.apache.derby.jdbc.EmbeddedDriver", StringComparison.Ordinal) < 0)
				return;

			DerbyUnloadManager derbyMan = new DerbyUnloadManager (connectionString, policy);
			AppDomain.CurrentDomain.DomainUnload += new EventHandler (derbyMan.UnloadHandler);
		}

		public void UnloadHandler (object sender, EventArgs e)
		{
			string shutUrl;

			switch (_policy) {
			case DerbyShutDownPolicy.Never:
				return;
			case DerbyShutDownPolicy.Database:
				shutUrl = GetConnectionProperty (_connectionString, "JdbcURL");
				break;
			case DerbyShutDownPolicy.System:
				shutUrl = "JdbcURL=jdbc:derby:";
				break;
			default:
			case DerbyShutDownPolicy.Default:
				java.lang.ClassLoader contextLoader = (java.lang.ClassLoader) AppDomain.CurrentDomain.GetData (J2EEConsts.CLASS_LOADER);
				java.lang.Class klass = contextLoader.loadClass ("org.apache.derby.jdbc.EmbeddedDriver");
				if (klass == null)
					return;

				shutUrl = (klass.getClassLoader () == contextLoader) ?
					"JdbcURL=jdbc:derby:" : GetConnectionProperty (_connectionString, "JdbcURL");

				break;
			}

			const string shuttingConnection = "JdbcDriverClassName=org.apache.derby.jdbc.EmbeddedDriver;{0};shutdown=true";

			if (!String.IsNullOrEmpty (shutUrl)) {
				try {
					new OleDbConnection (String.Format (shuttingConnection, shutUrl)).Open ();
				}
				catch (Exception ex) {
					Trace.Write (ex.ToString ());
				}
			}
		}

		static string GetConnectionProperty (string connectionString, string name) {
			if (String.IsNullOrEmpty (connectionString))
				return null;

			string [] parts = connectionString.Split (';');
			foreach (string part in parts)
				if (part.StartsWith (name, StringComparison.OrdinalIgnoreCase))
					return part;

			return null;
		}
	}
}

#endif
