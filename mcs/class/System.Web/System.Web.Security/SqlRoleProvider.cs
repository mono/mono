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


using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;

namespace System.Web.Security
{

	public class SqlRoleProvider : RoleProvider
	{

		string applicationName;
		bool schemaIsOk = false;

		ConnectionStringSettings connectionString;
		DbProviderFactory factory;

		DbConnection CreateConnection ()
		{
			if (!schemaIsOk && !(schemaIsOk = AspNetDBSchemaChecker.CheckMembershipSchemaVersion (factory, connectionString.ConnectionString, "role manager", "1")))
				throw new ProviderException ("Incorrect ASP.NET DB Schema Version.");

			DbConnection connection = factory.CreateConnection ();
			connection.ConnectionString = connectionString.ConnectionString;

			connection.Open ();
			return connection;
		}

		static void AddParameter (DbCommand command, string parameterName, object parameterValue)
		{
			AddParameter (command, parameterName, ParameterDirection.Input, parameterValue);
		}

		static DbParameter AddParameter (DbCommand command, string parameterName, ParameterDirection direction, object parameterValue)
		{
			DbParameter dbp = command.CreateParameter ();
			dbp.ParameterName = parameterName;
			dbp.Value = parameterValue;
			dbp.Direction = direction;
			command.Parameters.Add (dbp);
			return dbp;
		}

		static DbParameter AddParameter (DbCommand command, string parameterName, ParameterDirection direction, DbType type, object parameterValue)
		{
			DbParameter dbp = command.CreateParameter ();
			dbp.ParameterName = parameterName;
			dbp.Value = parameterValue;
			dbp.Direction = direction;
			dbp.DbType = type;
			command.Parameters.Add (dbp);
			return dbp;
		}

		public override void AddUsersToRoles (string [] usernames, string [] roleNames)
		{
			Hashtable h = new Hashtable ();

			foreach (string u in usernames) {
				if (u == null)
					throw new ArgumentNullException ("null element in usernames array");
				if (h.ContainsKey (u))
					throw new ArgumentException ("duplicate element in usernames array");
				if (u.Length == 0 || u.Length > 256 || u.IndexOf (',') != -1)
					throw new ArgumentException ("element in usernames array in illegal format");
				h.Add (u, u);
			}

			h = new Hashtable ();
			foreach (string r in roleNames) {
				if (r == null)
					throw new ArgumentNullException ("null element in rolenames array");
				if (h.ContainsKey (r))
					throw new ArgumentException ("duplicate element in rolenames array");
				if (r.Length == 0 || r.Length > 256 || r.IndexOf (',') != -1)
					throw new ArgumentException ("element in rolenames array in illegal format");
				h.Add (r, r);
			} 
			
			using (DbConnection connection = CreateConnection ()) {
				/* add the user/role combination to dbo.aspnet_UsersInRoles */
				DbCommand command = factory.CreateCommand ();
				command.CommandText = @"dbo.aspnet_UsersInRoles_AddUsersToRoles";
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@RoleNames", String.Join (",", roleNames));
				AddParameter (command, "@UserNames", String.Join (",", usernames));
				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@CurrentTimeUtc", DateTime.UtcNow);
				DbParameter dbpr = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				command.ExecuteNonQuery ();

				int returnValue = (int) dbpr.Value;
				if (returnValue == 0)
					return;
				else if (returnValue == 2)
					throw new ProviderException ("One or more of the specified user/role names was not found.");
				else if (returnValue == 3)
					throw new ProviderException ("One or more of the specified user names is already associated with one or more of the specified role names.");
				else
					throw new ProviderException ("Failed to create new user/role association.");
			}
		}

		public override void CreateRole (string roleName)
		{
			if (roleName == null)
				throw new ArgumentNullException ("roleName");

			if (roleName.Length == 0 || roleName.Length > 256 || roleName.IndexOf (',') != -1)
				throw new ArgumentException ("rolename is in invalid format");

			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.CommandText = @"dbo.aspnet_Roles_CreateRole";
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				
				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@RoleName", roleName);
				DbParameter dbpr = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				command.ExecuteNonQuery ();
				int returnValue = (int) dbpr.Value;

				if (returnValue == 1)
					throw new ProviderException (roleName + " already exists in the database");
				else
					return;
			}
		}

		public override bool DeleteRole (string roleName, bool throwOnPopulatedRole)
		{
			if (roleName == null)
				throw new ArgumentNullException ("roleName");

			if (roleName.Length == 0 || roleName.Length > 256 || roleName.IndexOf (',') != -1)
				throw new ArgumentException ("rolename is in invalid format");

			using (DbConnection connection = CreateConnection ()) {

				DbCommand command = factory.CreateCommand ();
				command.CommandText = @"dbo.aspnet_Roles_DeleteRole";
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;
				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@RoleName", roleName);
				AddParameter (command, "@DeleteOnlyIfRoleIsEmpty", throwOnPopulatedRole);
				DbParameter dbpr = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				command.ExecuteNonQuery ();
				int returnValue = (int)dbpr.Value;

				if (returnValue == 0)
					return true;
				if (returnValue == 1)
					return false; //role does not exist
				else if (returnValue == 2 && throwOnPopulatedRole)
					throw new ProviderException (roleName + " is not empty");
				else
					return false;
			}
		}

		public override string [] FindUsersInRole (string roleName, string usernameToMatch)
		{
			if (roleName == null)
				throw new ArgumentNullException ("roleName");
			if (usernameToMatch == null)
				throw new ArgumentNullException ("usernameToMatch");
			if (roleName.Length == 0 || roleName.Length > 256 || roleName.IndexOf (',') != -1)
				throw new ArgumentException ("roleName is in invalid format");
			if (usernameToMatch.Length == 0 || usernameToMatch.Length > 256)
				throw new ArgumentException ("usernameToMatch is in invalid format");

			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.Connection = connection;
				command.CommandText = @"dbo.aspnet_UsersInRoles_FindUsersInRole";
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@RoleName", roleName);
				AddParameter (command, "@UsernameToMatch", usernameToMatch);

				DbDataReader reader = command.ExecuteReader ();
				ArrayList userList = new ArrayList ();
				while (reader.Read ())
					userList.Add (reader.GetString (0));
				reader.Close ();

				return (string []) userList.ToArray (typeof (string));
			}
		}

		public override string [] GetAllRoles ()
		{
			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.CommandText = @"dbo.aspnet_Roles_GetAllRoles";
				command.Connection = connection;

				command.CommandType = CommandType.StoredProcedure;
				AddParameter (command, "@ApplicationName", ApplicationName);

				DbDataReader reader = command.ExecuteReader ();
				ArrayList roleList = new ArrayList ();
				while (reader.Read ())
					roleList.Add (reader.GetString (0));
				reader.Close ();

				return (string []) roleList.ToArray (typeof (string));
			}
		}

		public override string [] GetRolesForUser (string username)
		{
			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.CommandText = @"dbo.aspnet_UsersInRoles_GetRolesForUser";
				command.Connection = connection;

				command.CommandType = CommandType.StoredProcedure;
				AddParameter (command, "@UserName", username);
				AddParameter (command, "@ApplicationName", ApplicationName);

				DbDataReader reader = command.ExecuteReader ();
				ArrayList roleList = new ArrayList ();
				while (reader.Read ())
					roleList.Add (reader.GetString (0));
				reader.Close ();

				return (string []) roleList.ToArray (typeof (string));
			}
		}

		public override string [] GetUsersInRole (string roleName)
		{
			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.CommandText = @"dbo.aspnet_UsersInRoles_GetUsersInRoles";
				command.Connection = connection;

				command.CommandType = CommandType.StoredProcedure;
				AddParameter (command, "@RoleName", roleName);
				AddParameter (command, "@ApplicationName", ApplicationName);

				DbDataReader reader = command.ExecuteReader ();
				ArrayList userList = new ArrayList ();
				while (reader.Read ())
					userList.Add (reader.GetString (0));
				reader.Close ();

				return (string []) userList.ToArray (typeof (string));
			}
		}

		string GetStringConfigValue (NameValueCollection config, string name, string def)
		{
			string rv = def;
			string val = config [name];
			if (val != null)
				rv = val;
			return rv;
		}

		public override void Initialize (string name, NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException ("config");

			base.Initialize (name, config);

			applicationName = GetStringConfigValue (config, "applicationName", "/");
			string connectionStringName = config ["connectionStringName"];

			if (applicationName.Length > 256)
				throw new ProviderException ("The ApplicationName attribute must be 256 characters long or less.");
			if (connectionStringName == null || connectionStringName.Length == 0)
				throw new ProviderException ("The ConnectionStringName attribute must be present and non-zero length.");

			// XXX check connectionStringName and commandTimeout

			connectionString = WebConfigurationManager.ConnectionStrings [connectionStringName];
			if (connectionString == null)
				throw new ProviderException (String.Format("The connection name '{0}' was not found in the applications configuration or the connection string is empty.", connectionStringName));
			factory = String.IsNullOrEmpty (connectionString.ProviderName) ?
				System.Data.SqlClient.SqlClientFactory.Instance :
				ProvidersHelper.GetDbProviderFactory (connectionString.ProviderName);
		}

		public override bool IsUserInRole (string username, string roleName)
		{
			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.CommandText = @"dbo.aspnet_UsersInRoles_IsUserInRole";
				command.Connection = connection;

				command.CommandType = CommandType.StoredProcedure;
				AddParameter (command, "@RoleName", roleName);
				AddParameter (command, "@UserName", username);
				AddParameter (command, "@ApplicationName", ApplicationName);
				DbParameter dbpr = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				command.ExecuteNonQuery ();
				int returnValue = (int) dbpr.Value;

				if (returnValue == 1)
					return true;

				return false;
			}
		}

		public override void RemoveUsersFromRoles (string [] usernames, string [] roleNames)
		{
			Hashtable h = new Hashtable ();

			foreach (string u in usernames) {
				if (u == null)
					throw new ArgumentNullException ("null element in usernames array");
				if (h.ContainsKey (u))
					throw new ArgumentException ("duplicate element in usernames array");
				if (u.Length == 0 || u.Length > 256 || u.IndexOf (',') != -1)
					throw new ArgumentException ("element in usernames array in illegal format");
				h.Add (u, u);
			}

			h = new Hashtable ();
			foreach (string r in roleNames) {
				if (r == null)
					throw new ArgumentNullException ("null element in rolenames array");
				if (h.ContainsKey (r))
					throw new ArgumentException ("duplicate element in rolenames array");
				if (r.Length == 0 || r.Length > 256 || r.IndexOf (',') != -1)
					throw new ArgumentException ("element in rolenames array in illegal format");
				h.Add (r, r);
			} 

			using (DbConnection connection = CreateConnection ()) {
				DbCommand command = factory.CreateCommand ();
				command.CommandText = @"dbo.aspnet_UsersInRoles_RemoveUsersFromRoles";
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@UserNames", String.Join (",", usernames));
				AddParameter (command, "@RoleNames", String.Join (",", roleNames));
				AddParameter (command, "@ApplicationName", ApplicationName);
				DbParameter dbpr = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				command.ExecuteNonQuery ();
				int returnValue = (int) dbpr.Value;

				if (returnValue == 0)
					return;
				else if (returnValue == 1)
					throw new ProviderException ("One or more of the specified user names was not found.");
				else if (returnValue == 2)
					throw new ProviderException ("One or more of the specified role names was not found.");
				else if (returnValue == 3)
					throw new ProviderException ("One or more of the specified user names is not associated with one or more of the specified role names.");
				else
					throw new ProviderException ("Failed to remove users from roles");
			}
		}

		public override bool RoleExists (string roleName)
		{
			using (DbConnection connection = CreateConnection ()) {

				DbCommand command = factory.CreateCommand ();
				command.CommandText = @"dbo.aspnet_Roles_RoleExists";
				command.Connection = connection;
				command.CommandType = CommandType.StoredProcedure;

				AddParameter (command, "@ApplicationName", ApplicationName);
				AddParameter (command, "@RoleName", roleName);
				DbParameter dbpr = AddParameter (command, "@ReturnVal", ParameterDirection.ReturnValue, DbType.Int32, null);

				command.ExecuteNonQuery ();
				int returnValue = (int) dbpr.Value;

				if (returnValue == 1)
					return true;

				return false;
			}
		}

		public override string ApplicationName
		{
			get { return applicationName; }
			set
			{
				applicationName = value;
			}
		}
	}
}

