//
// Mainsoft.Web.Security.DerbyRoleProvider
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
using System.Data;
using System.Data.OleDb;
using System.Data.Common;
using System.Configuration;
using System.Configuration.Provider;
using System.Web.Configuration;
using System.Web.Security;

namespace Mainsoft.Web.Security
{
	/// <summary>
	/// <para>This class supports the Framework infrastructure and is not intended to be used directly from your code.</para>
	/// <para>Manages storage of role membership information for an ASP.NET application in a Derby database.</para>
	/// </summary>
	public class DerbyRoleProvider : RoleProvider
	{
		ConnectionStringSettings connectionString;
		string applicationName;
		bool schemaChecked = false;
		DerbyUnloadManager.DerbyShutDownPolicy shutDownPolicy = DerbyUnloadManager.DerbyShutDownPolicy.Default;

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

		public override void AddUsersToRoles (string [] usernames, string [] rolenames)
		{
			Hashtable h = new Hashtable ();

			foreach (string u in usernames) {
				if (u == null)
					throw new ArgumentNullException ("null element in usernames array");
				if (h.ContainsKey (u))
					throw new ArgumentException ("duplicate element in usernames array");
				if (u.Length == 0 || u.Length > 256 || u.IndexOf (",") != -1)
					throw new ArgumentException ("element in usernames array in illegal format");
				h.Add (u, u);
			}

			h = new Hashtable ();
			foreach (string r in rolenames) {
				if (r == null)
					throw new ArgumentNullException ("null element in rolenames array");
				if (h.ContainsKey (r))
					throw new ArgumentException ("duplicate element in rolenames array");
				if (r.Length == 0 || r.Length > 256 || r.IndexOf (",") != -1)
					throw new ArgumentException ("element in rolenames array in illegal format");
				h.Add (r, r);
			} 
			
			using (DbConnection connection = CreateConnection ()) {
				int returnValue = DerbyRolesHelper.UsersInRoles_AddUsersToRoles (connection, ApplicationName, usernames, rolenames, DateTime.UtcNow);

				if (returnValue == 0)
					return;
				else if (returnValue == 2)
					throw new ProviderException ("One or more of the specified role names was not found.");
				else if (returnValue == 3)
					throw new ProviderException ("One or more of the specified user names is already associated with one or more of the specified role names.");
				else
					throw new ProviderException ("Failed to create new user/role association.");
			}
		}

		public override void CreateRole (string rolename)
		{
			if (rolename == null)
				throw new ArgumentNullException ("rolename");

			if (rolename.Length == 0 || rolename.Length > 256 || rolename.IndexOf (",") != -1)
				throw new ArgumentException ("rolename is in invalid format");

			using (DbConnection connection = CreateConnection ()) {
				int returnValue = DerbyRolesHelper.Roles_CreateRole (connection, ApplicationName, rolename);
				
				if (returnValue == 2)
					throw new ProviderException (rolename + " already exists in the database");
				else
					return;
			}
		}

		public override bool DeleteRole (string rolename, bool throwOnPopulatedRole)
		{
			if (rolename == null)
				throw new ArgumentNullException ("rolename");

			if (rolename.Length == 0 || rolename.Length > 256 || rolename.IndexOf (",") != -1)
				throw new ArgumentException ("rolename is in invalid format");

			using (DbConnection connection = CreateConnection ()) {
				int returnValue = DerbyRolesHelper.Roles_DeleteRole (connection, ApplicationName, rolename, throwOnPopulatedRole);

				if (returnValue == 0)
					return true;
				if (returnValue == 2)
					return false; //role does not exists
				else if (returnValue == 3 && throwOnPopulatedRole)
					throw new ProviderException (rolename + " is not empty");
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
			if (roleName.Length == 0 || roleName.Length > 256 || roleName.IndexOf (",") != -1)
				throw new ArgumentException ("roleName is in invalid format");
			if (usernameToMatch.Length == 0 || usernameToMatch.Length > 256)
				throw new ArgumentException ("usernameToMatch is in invalid format");

			using (DbConnection connection = CreateConnection ()) {
				DbDataReader reader;
				ArrayList userList = new ArrayList ();
				int returnValue = DerbyRolesHelper.UsersInRoles_FindUsersInRole (connection, applicationName, roleName, usernameToMatch, out reader);

				if (returnValue == 2)
					throw new ProviderException ("The role '" + roleName + "' was not found.");

				using (reader) {
					if (reader == null)
						return new string [] { };

					while (reader.Read ())
						userList.Add (reader.GetString (0));
				}
				return (string []) userList.ToArray (typeof (string));
			}
		}

		public override string [] GetAllRoles ()
		{
			using (DbConnection connection = CreateConnection ()) {
				DbDataReader reader;
				ArrayList roleList = new ArrayList ();
				DerbyRolesHelper.Roles_GetAllRoles (connection, applicationName, out reader);
				using (reader) {
					if (reader == null)
						return new string [] { };

					while (reader.Read ())
						roleList.Add (reader.GetString (0));
				}
				return (string []) roleList.ToArray (typeof (string));
			}
		}

		public override string [] GetRolesForUser (string username)
		{
			if (username == null)
				throw new ArgumentNullException ("rolename");

			if (username.Length == 0 || username.Length > 256 || username.IndexOf (",") != -1)
				throw new ArgumentException ("username is in invalid format");

			using (DbConnection connection = CreateConnection ()) {
				DbDataReader reader;
				ArrayList roleList = new ArrayList ();
				int returnValue = DerbyRolesHelper.UsersInRoles_GetRolesForUser (connection, applicationName, username, out reader);

				if (returnValue == 2)
					throw new ProviderException ("username was not found in the database");

				using (reader) {
					if (reader == null)
						return new string [] { };

					while (reader.Read ())
						roleList.Add (reader.GetString (0));
				}
				return (string []) roleList.ToArray (typeof (string));
			}
		}

		public override string [] GetUsersInRole (string rolename)
		{
			if (rolename == null)
				throw new ArgumentNullException ("rolename");

			if (rolename.Length == 0 || rolename.Length > 256 || rolename.IndexOf (",") != -1)
				throw new ArgumentException ("rolename is in invalid format");

			using (DbConnection connection = CreateConnection ()) {
				DbDataReader reader;
				ArrayList roleList = new ArrayList ();
				int returnValue = DerbyRolesHelper.UsersInRoles_GetUsersInRoles (connection, applicationName, rolename, out reader);

				if (returnValue == 2)
					throw new ProviderException ("The role '" + rolename + "' was not found.");

				using (reader) {
					if (reader == null)
						return new string [] { };

					while (reader.Read ())
						roleList.Add (reader.GetString (0));
				}
				return (string []) roleList.ToArray (typeof (string));
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

			applicationName = config ["applicationName"];
			string connectionStringName = config ["connectionStringName"];

			if (applicationName.Length > 256)
				throw new ProviderException ("The ApplicationName attribute must be 256 characters long or less.");
			if (connectionStringName == null || connectionStringName.Length == 0)
				throw new ProviderException ("The ConnectionStringName attribute must be present and non-zero length.");

			// XXX check connectionStringName and commandTimeout

			connectionString = WebConfigurationManager.ConnectionStrings [connectionStringName];
			if (connectionString == null)
				throw new ProviderException (String.Format("The connection name '{0}' was not found in the applications configuration or the connection string is empty.", connectionStringName));

			string shutdown = config ["shutdown"];
			if (!String.IsNullOrEmpty (shutdown))
				shutDownPolicy = (DerbyUnloadManager.DerbyShutDownPolicy) Enum.Parse (typeof (DerbyUnloadManager.DerbyShutDownPolicy), shutdown, true);
		}

		public override bool IsUserInRole (string username, string rolename)
		{
			if (username == null)
				throw new ArgumentNullException ("rolename");
			if (username.Length == 0 || username.Length > 256 || username.IndexOf (",") != -1)
				throw new ArgumentException ("username is in invalid format");
			if (rolename == null)
				throw new ArgumentNullException ("rolename");
			if (rolename.Length == 0 || rolename.Length > 256 || rolename.IndexOf (",") != -1)
				throw new ArgumentException ("rolename is in invalid format");

			using (DbConnection connection = CreateConnection ()) {
				int returnValue = DerbyRolesHelper.UsersInRoles_IsUserInRole (connection, ApplicationName, username, rolename);

				if (returnValue == 4)
					return true;

				return false;
			}
		}

		public override void RemoveUsersFromRoles (string [] usernames, string [] rolenames)
		{
			Hashtable h = new Hashtable ();

			foreach (string u in usernames) {
				if (u == null)
					throw new ArgumentNullException ("null element in usernames array");
				if (h.ContainsKey (u))
					throw new ArgumentException ("duplicate element in usernames array");
				if (u.Length == 0 || u.Length > 256 || u.IndexOf (",") != -1)
					throw new ArgumentException ("element in usernames array in illegal format");
				h.Add (u, u);
			}

			h = new Hashtable ();
			foreach (string r in rolenames) {
				if (r == null)
					throw new ArgumentNullException ("null element in rolenames array");
				if (h.ContainsKey (r))
					throw new ArgumentException ("duplicate element in rolenames array");
				if (r.Length == 0 || r.Length > 256 || r.IndexOf (",") != -1)
					throw new ArgumentException ("element in rolenames array in illegal format");
				h.Add (r, r);
			} 

			using (DbConnection connection = CreateConnection ()) {
				int returnValue = DerbyRolesHelper.UsersInRoles_RemoveUsersFromRoles (connection, ApplicationName, usernames, rolenames);

				if (returnValue == 0)
					return;
				else if (returnValue == 2)
					throw new ProviderException ("One or more of the specified user names was not found.");
				else if (returnValue == 3)
					throw new ProviderException ("One or more of the specified role names was not found.");
				else if (returnValue == 4)
					throw new ProviderException ("One or more of the specified user names is not associated with one or more of the specified role names.");
				else
					throw new ProviderException ("Failed to remove users from roles");
			}
		}

		public override bool RoleExists (string rolename)
		{
			if (rolename == null)
				throw new ArgumentNullException ("rolename");

			if (rolename.Length == 0 || rolename.Length > 256 || rolename.IndexOf (",") != -1)
				throw new ArgumentException ("rolename is in invalid format");

			using (DbConnection connection = CreateConnection ()) {
				int returnValue = DerbyRolesHelper.Roles_RoleExists (connection, ApplicationName, rolename);

				if (returnValue == 2)
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
#endif

