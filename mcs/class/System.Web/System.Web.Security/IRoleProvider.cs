//
// System.Web.Security.IRoleProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Configuration.Provider;

namespace System.Web.Security {
	public interface IRoleProvider : IProvider {
		void AddUsersToRoles (string [] usernames, string [] rolenames);
		void CreateRole (string rolename);
		void DeleteRole (string rolename);
		string [] GetAllRoles ();
		string [] GetRolesForUser (string username);
		string [] GetUsersInRole (string rolename);
		bool IsUserInRole (string username, string rolename);
		void RemoveUsersFromRoles (string [] usernames, string [] rolenames);
		bool RoleExists (string rolename);
		string ApplicationName { get; set; }
	}
}
#endif

