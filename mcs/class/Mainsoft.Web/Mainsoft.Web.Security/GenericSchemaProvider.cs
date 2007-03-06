//
// Mainsoft.Web.Security.GenericSchemaProvider
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
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

using Mainsoft.Web.Configuration;

namespace Mainsoft.Web.Security
{
	public class GenericSchemaProvider : GenericSchemaBuilder
	{
		const string _currentSchemaVersion = "1.0";
		static object _lock = "GenericDBSchema";
		ConnectionStringSettings connectionString;
		GenericDatabaseHelper dbHelper;
		
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
			@"CREATE UNIQUE INDEX aspnet_UsersInRolesPrimaryIndex ON aspnet_UsersInRoles (UserId, RoleId)",
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

		public override void Initialize (ConnectionStringSettings connectionString, GenericDatabaseHelper dbHelper)
		{
			this.connectionString = connectionString;
			this.dbHelper = dbHelper;
		}
		
		public override void CheckSchema ()
		{
			string schemaVersion = GetSchemaVersion ();
			if (schemaVersion != null)
				if (string.CompareOrdinal (schemaVersion, _currentSchemaVersion) == 0)
					return;
				else
					throw new Exception ("Incorrect aspnetdb schema version.");

			lock (_lock) {
				if (GetSchemaVersion () != _currentSchemaVersion) {
					InitializeSchema ();
				}
			}
		}

		string GetSchemaVersion ()
		{
			DbConnection connection;

			try {
				connection = dbHelper.NewConnection ();
			} catch (Exception) {
				return null;
			}

			using (connection) {
				DbCommand cmd = dbHelper.NewCommand ("SELECT SchemaVersion FROM aspnet_Version", connection);

				try {
					using (DbDataReader reader = cmd.ExecuteReader ()) {
						if (reader.Read ())
							return reader.GetString (0);
					}
				} catch (Exception) { }
				return null;
			}
		}

		void InitializeSchema ()
		{
			string cs = connectionString.ConnectionString.ToLower ();
			if (cs.IndexOf ("create=true") < 0) {
				if (!cs.Trim ().EndsWith (";"))
					cs += ";";

				cs += "create=true";
			}

			DbConnection connection = dbHelper.NewConnection ();
			using (connection) {
				for (int i = 0; i < schemaElements.Length; i++) {
					DbCommand cmd = dbHelper.NewCommand (schemaElements [i], connection);
					cmd.ExecuteNonQuery ();
				}
			}
		}

		public override void RegisterUnloadHandler ()
		{}
	}
}

#endif
