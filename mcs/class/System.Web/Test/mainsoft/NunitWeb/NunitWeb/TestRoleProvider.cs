#if NET_2_0

using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web.Security;

namespace MonoTests.SystemWeb.Framework
{
	public class TestRoleProvider : RoleProvider
	{
		public override string ApplicationName
		{
			get;
			set;
		}

		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			throw new Exception ("Not implemented yet.");
		}

		public override void CreateRole (string roleName)
		{
			throw new Exception ("Not implemented yet.");
		}

		public override bool DeleteRole (string roleName, bool throwOnPopulatedRole)
		{
			throw new Exception ("Not implemented yet.");
		}

		public override string[] FindUsersInRole (string roleName, string usernameToMatch)
		{
			throw new Exception ("Not implemented yet.");
		}

		public override string[] GetAllRoles ()
		{
			throw new Exception ("Not implemented yet.");
		}

		public override string[] GetRolesForUser (string username)
		{
			throw new Exception ("Not implemented yet.");
		}

		public override string[] GetUsersInRole (string roleName)
		{
			throw new Exception ("Not implemented yet.");
		}

		public override bool IsUserInRole (string username, string roleName)
		{
			if (username == null)
				throw new ArgumentNullException ("Username cannot be null.");
			if (roleName == null)
				throw new ArgumentNullException ("Role name cannot be null.");
			if (username == string.Empty)
				throw new ArgumentException ("Username cannot be empty.");
			if (roleName == string.Empty)
				throw new ArgumentException ("Role name cannot be empty.");
			if (username == "invalid")
				throw new ProviderException ("User does not exist.");
			if (roleName == "invalid")
				throw new ProviderException ("Role does not exist.");
			if (username == "true")
				return true;
			return false;
		}

		public override void RemoveUsersFromRoles (string[] usernames, string[] roleNames)
		{
			throw new Exception ("Not implemented yet.");
		}

		public override bool RoleExists (string roleName)
		{
			throw new Exception ("Not implemented yet.");
		}
	}
}
#endif
