//
// System.Web.Security.SqlRoleProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
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
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;

namespace System.Web.Security {

	public class SqlRoleProvider: RoleProvider {

		string applicationName;
		int commandTimeout;
		string providerName;

		ConnectionStringSettings connectionString;
		DbProviderFactory factory;
		DbConnection connection;

		void InitConnection ()
		{
			if (factory == null)
				factory = ProvidersHelper.GetDbProviderFactory (connectionString.ProviderName);
                        if (connection == null) {
				connection = factory.CreateConnection();
				connection.ConnectionString = connectionString.ConnectionString;
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

		public override void AddUsersToRoles (string [] usernames, string [] rolenames)
		{
			string commandText = @"
INSERT INTO dbo.aspnet_UsersInRoles (UserId, RoleId)
     SELECT dbo.aspnet_Users.UserId, dbo.aspnet_Roles.RoleId 
       FROM dbo.aspnet_Users, dbo.aspnet_Roles, dbo.aspnet_Applications
      WHERE dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
        AND dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
        AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
        AND dbo.aspnet_Users.LoweredUserName = LOWER(@UserName)
        AND dbo.aspnet_Roles.LoweredRoleName = LOWER(@RoleName)
";

			Hashtable h;

			h = new Hashtable();
			foreach (string u in usernames) {
				if (u == null)
					throw new ArgumentNullException ("null element in usernames array");
				if (h.ContainsKey (u))
					throw new ArgumentException ("duplicate element in usernames array");
				if (u.Length == 0 || u.Length > 256 || u.IndexOf (",") != -1)
					throw new ArgumentException ("element in usernames array in illegal format");
				h.Add (u, u);
			}

			h = new Hashtable();
			foreach (string r in usernames) {
				if (r == null)
					throw new ArgumentNullException ("null element in usernames array");
				if (h.ContainsKey (r))
					throw new ArgumentException ("duplicate element in usernames array");
				if (r.Length == 0 || r.Length > 256 || r.IndexOf (",") != -1)
					throw new ArgumentException ("element in usernames array in illegal format");
				h.Add (r, r);
			}

			InitConnection();

			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open();

			DbTransaction trans = connection.BeginTransaction ();

			try {
				foreach (string username in usernames) {

					foreach (string rolename in rolenames) {

						/* add the user/role combination to dbo.aspnet_UsersInRoles */
						DbCommand command = factory.CreateCommand ();
						command.Transaction = trans;
						command.CommandText = commandText;
						command.Connection = connection;
						command.CommandType = CommandType.Text;
						AddParameter (command, "RoleName", rolename);
						AddParameter (command, "UserName", username);
						AddParameter (command, "ApplicationName", ApplicationName);

						if (command.ExecuteNonQuery() != 1)
							throw new ProviderException ("failed to create new user/role association.");
					}
				}
				
				trans.Commit ();
			}
			catch (Exception e) {
				trans.Rollback ();
				if (e is ProviderException)
					throw e;
				else
					throw new ProviderException ("", e);
			}
			finally {
				if (closed)
					connection.Close ();
			}
		}
		
		public override void CreateRole (string rolename)
		{
			string commandText = @"
INSERT INTO dbo.aspnet_Roles 
            (ApplicationId, RoleName, LoweredRoleName)
     VALUES ((SELECT ApplicationId FROM dbo.aspnet_Applications WHERE LoweredApplicationName = LOWER(@ApplicationName)), @RoleName, LOWER(@RoleName))
";
			if (rolename == null)
				throw new ArgumentNullException ("rolename");

			if (rolename.Length == 0 || rolename.Length > 256 || rolename.IndexOf (",") != -1)
				throw new ArgumentException ("rolename is in invalid format");

			InitConnection();
			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "ApplicationName", ApplicationName);
			AddParameter (command, "RoleName", rolename);

			if (command.ExecuteNonQuery() != 1)
				throw new ProviderException ("failed to create new role.");

			if (closed)
				connection.Close ();
		}
		
		[MonoTODO]
		public override bool DeleteRole (string rolename, bool throwOnPopulatedRole)
		{
			if (rolename == null)
				throw new ArgumentNullException ("rolename");

			if (rolename.Length == 0 || rolename.Length > 256 || rolename.IndexOf (",") != -1)
				throw new ArgumentException ("rolename is in invalid format");

			InitConnection();
			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open();

			DbCommand command;
			if (throwOnPopulatedRole) {
				command = factory.CreateCommand ();
				command.CommandText = @"
SELECT COUNT(*) 
  FROM dbo.aspnet_UsersInRoles, dbo.aspnet_Roles, dbo.aspnet_Users, dbo.aspnet_Applications
 WHERE dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_UsersInRoles.RoleId = dbo.aspnet_Roles.RoleId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
   AND dbo.aspnet_Roles.LoweredRoleName = LOWER(@RoleName)";
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "ApplicationName", ApplicationName);
				AddParameter (command, "RoleName", rolename);

				int count = (int)command.ExecuteScalar ();
				if (count != 0)
					throw new ProviderException (String.Format ("The role '{0}' has users in it and can't be deleted", rolename));
			}
			else {
				/* XXX are we really supposed to delete all the user/role associations in this case? */
				command = factory.CreateCommand ();
				command.CommandText = @"
DELETE dbo.aspnet_UsersInRoles FROM dbo.aspnet_UsersInRoles, dbo.aspnet_Roles, dbo.aspnet_Applications
 WHERE dbo.aspnet_UsersInRoles.RoleId = dbo.aspnet_Roles.RoleId
   AND dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Roles.LoweredRoleName = LOWER(@RoleName)
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";
				command.Connection = connection;
				command.CommandType = CommandType.Text;
				AddParameter (command, "RoleName", rolename);
				AddParameter (command, "ApplicationName", ApplicationName);

				command.ExecuteNonQuery ();
			}

			command = factory.CreateCommand ();
			command.CommandText = @"
DELETE dbo.aspnet_Roles FROM dbo.aspnet_Roles, dbo.aspnet_Applications
 WHERE dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
   AND dbo.aspnet_Roles.LoweredRoleName = LOWER(@RoleName)";
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "ApplicationName", ApplicationName);
			AddParameter (command, "RoleName", rolename);

			bool rv = command.ExecuteNonQuery() == 1;

			if (closed)
				connection.Close ();

			return rv;
		}
		
		public override string[] FindUsersInRole (string roleName, string usernameToMatch)
		{
			string commandTextFormat = @"
SELECT dbo.aspnet_Users.UserName
  FROM dbo.aspnet_Users, dbo.aspnet_Roles, dbo.aspnet_UsersInRoles, dbo.aspnet_Applications
 WHERE dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_UsersInRoles.UserId = dbo.aspnet_Users.UserId
   AND dbo.aspnet_UsersInRoles.RoleId = dbo.aspnet_Roles.RoleId
   AND dbo.aspnet_Roles.LoweredRoleName = LOWER(@RoleName)
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
   AND dbo.aspnet_Users.UserName {0} @UsernameToMatch
";
			if (roleName == null)
				throw new ArgumentNullException ("roleName");
			if (usernameToMatch == null)
				throw new ArgumentNullException ("usernameToMatch");

			if (roleName.Length == 0 || roleName.Length > 256 || roleName.IndexOf (",") != -1)
				throw new ArgumentException ("roleName is in invalid format");
			if (usernameToMatch.Length == 0 || usernameToMatch.Length > 256)
				throw new ArgumentException ("usernameToMatch is in invalid format");

			InitConnection();

			bool useLike = usernameToMatch.IndexOf ("%") != -1;
			DbCommand command = factory.CreateCommand ();
			command.CommandText = String.Format(commandTextFormat, useLike ? "LIKE" : "=");
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "ApplicationName", ApplicationName);
			AddParameter (command, "RoleName", roleName);
			AddParameter (command, "UsernameToMatch", usernameToMatch);

			DbDataReader reader = command.ExecuteReader ();
			ArrayList userList = new ArrayList();
			while (reader.Read())
				userList.Add (reader.GetString(0));
			reader.Close();

			return (string[])userList.ToArray(typeof (string));
		}

		public override string [] GetAllRoles ()
		{
			string commandText = @"
SELECT dbo.aspnet_Roles.RoleName
  FROM dbo.aspnet_Roles, dbo.aspnet_Applications
 WHERE dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
";
			InitConnection();
			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "ApplicationName", ApplicationName);

			DbDataReader reader = command.ExecuteReader ();
			ArrayList roleList = new ArrayList();
			while (reader.Read())
				roleList.Add (reader.GetString(0));
			reader.Close();

			if (closed)
				connection.Close ();

			return (string[])roleList.ToArray(typeof (string));
		}
		
		public override string [] GetRolesForUser (string username)
		{
			string commandText = @"
SELECT dbo.aspnet_Roles.RoleName
  FROM dbo.aspnet_Roles, dbo.aspnet_UsersInRoles, dbo.aspnet_Users, dbo.aspnet_Applications
WHERE dbo.aspnet_Roles.RoleId = dbo.aspnet_UsersInRoles.RoleId
  AND dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
  AND dbo.aspnet_UsersInRoles.UserId = dbo.aspnet_Users.UserId
  AND dbo.aspnet_Users.LoweredUserName = LOWER(@UserName)
  AND dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
  AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
";

			InitConnection();
			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "UserName", username);
			AddParameter (command, "ApplicationName", ApplicationName);

			DbDataReader reader = command.ExecuteReader ();
			ArrayList roleList = new ArrayList();
			while (reader.Read())
				roleList.Add (reader.GetString(0));
			reader.Close();

			if (closed)
				connection.Close ();

			return (string[])roleList.ToArray(typeof (string));
		}
		
		public override string [] GetUsersInRole (string rolename)
		{
			string commandText = @"
SELECT dbo.aspnet_Users.UserName
  FROM dbo.aspnet_Roles, dbo.aspnet_UsersInRoles, dbo.aspnet_Users, dbo.aspnet_Applications
WHERE dbo.aspnet_Roles.RoleId = dbo.aspnet_UsersInRoles.RoleId
  AND dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
  AND dbo.aspnet_UsersInRoles.UserId = dbo.aspnet_Users.UserId
  AND dbo.aspnet_Roles.LoweredRoleName = LOWER(@RoleName)
  AND dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
  AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
";

			InitConnection();
			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "RoleName", rolename);
			AddParameter (command, "ApplicationName", ApplicationName);

			DbDataReader reader = command.ExecuteReader ();
			ArrayList userList = new ArrayList();
			while (reader.Read())
				userList.Add (reader.GetString(0));
			reader.Close();

			if (closed)
				connection.Close ();

			return (string[])userList.ToArray(typeof (string));
		}
		
		[MonoTODO]
		public override void Initialize (string name, NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException ("config");

			base.Initialize (name, config);

#if false
			ApplicationName = config["applicationName"];
#else
			ApplicationName = "/";
#endif
			string connectionStringName = config["connectionStringName"];
			string commandTimeout = config["commandTimeout"];

			if (applicationName.Length > 256)
				throw new ProviderException ("The ApplicationName attribute must be 256 characters long or less.");
			if (connectionStringName == null || connectionStringName.Length == 0)
				throw new ProviderException ("The ConnectionStringName attribute must be present and non-zero length.");

			// XXX check connectionStringName and commandTimeout

			connectionString = WebConfigurationManager.ConnectionStrings[connectionStringName];
		}
		
		public override bool IsUserInRole (string username, string rolename)
		{
			string commandText = @"
SELECT COUNT(*)
  FROM dbo.aspnet_Users, dbo.aspnet_UsersInRoles, dbo.aspnet_Roles, dbo.aspnet_Applications
 WHERE dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_UsersInRoles.RoleId = dbo.aspnet_Roles.RoleId
   AND dbo.aspnet_UsersInRoles.UserId = dbo.aspnet_Users.UserId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
   AND dbo.aspnet_Roles.LoweredRoleName = LOWER(@RoleName)
   AND dbo.aspnet_Users.LoweredUserName = LOWER(@UserName)
";

			InitConnection();
			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "RoleName", rolename);
			AddParameter (command, "UserName", username);
			AddParameter (command, "ApplicationName", ApplicationName);

			bool rv = ((int)command.ExecuteScalar ()) != 0;

			if (closed)
				connection.Close ();

			return rv;
		}
		
		public override void RemoveUsersFromRoles (string [] usernames, string [] rolenames)
		{
			string commandText = @"
DELETE dbo.aspnet_UsersInRoles 
  FROM dbo.aspnet_UsersInRoles, dbo.aspnet_Users, dbo.aspnet_Roles, dbo.aspnet_Applications
 WHERE dbo.aspnet_UsersInRoles.UserId = dbo.aspnet_Users.UserId
   AND dbo.aspnet_UsersInRoles.RoleId = dbo.aspnet_Roles.RoleId
   AND dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Users.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Users.LoweredUserName = LOWER(@UserName)
   AND dbo.aspnet_Roles.LoweredRoleName = LOWER(@RoleName)
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)";

			Hashtable h;

			h = new Hashtable();
			foreach (string u in usernames) {
				if (u == null)
					throw new ArgumentNullException ("null element in usernames array");
				if (h.ContainsKey (u))
					throw new ArgumentException ("duplicate element in usernames array");
				if (u.Length == 0 || u.Length > 256 || u.IndexOf (",") != -1)
					throw new ArgumentException ("element in usernames array in illegal format");
				h.Add (u, u);
			}

			h = new Hashtable();
			foreach (string r in usernames) {
				if (r == null)
					throw new ArgumentNullException ("null element in usernames array");
				if (h.ContainsKey (r))
					throw new ArgumentException ("duplicate element in usernames array");
				if (r.Length == 0 || r.Length > 256 || r.IndexOf (",") != -1)
					throw new ArgumentException ("element in usernames array in illegal format");
				h.Add (r, r);
			}

			InitConnection();

			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open();

			DbTransaction trans = connection.BeginTransaction ();

			try {
				foreach (string username in usernames) {
					foreach (string rolename in rolenames) {
						DbCommand command = factory.CreateCommand ();
						command.Transaction = trans;
						command.CommandText = commandText;
						command.Connection = connection;
						command.CommandType = CommandType.Text;
						AddParameter (command, "UserName", username);
						AddParameter (command, "RoleName", rolename);
						AddParameter (command, "ApplicationName", ApplicationName);

						if (command.ExecuteNonQuery() != 1)
							throw new ProviderException (String.Format ("failed to remove users from role '{0}'.", rolename));
					}
				}
				
				trans.Commit ();
			}
			catch (Exception e) {
				trans.Rollback ();
				if (e is ProviderException)
					throw e;
				else
					throw new ProviderException ("", e);
			}
			finally {
				if (closed)
					connection.Close ();
			}
		}
		
		public override bool RoleExists (string rolename)
		{
			string commandText = @"
SELECT COUNT(*)
  FROM dbo.aspnet_Roles, dbo.aspnet_Applications
 WHERE dbo.aspnet_Roles.ApplicationId = dbo.aspnet_Applications.ApplicationId
   AND dbo.aspnet_Applications.LoweredApplicationName = LOWER(@ApplicationName)
   AND dbo.aspnet_Roles.LoweredRoleName = LOWER(@RoleName)
";

			InitConnection();
			bool closed = connection.State == ConnectionState.Closed;
			if (closed)
				connection.Open();

			DbCommand command = factory.CreateCommand ();
			command.CommandText = commandText;
			command.Connection = connection;
			command.CommandType = CommandType.Text;
			AddParameter (command, "ApplicationName", ApplicationName);
			AddParameter (command, "RoleName", rolename);

			bool rv = ((int)command.ExecuteScalar ()) != 0;

			if (closed)
				connection.Close ();

			return rv;
		}
		
		[MonoTODO]
		public override string ApplicationName {
			get { return applicationName; }
			set {
				applicationName = value;
			}
		}
	}
}
#endif

