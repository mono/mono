//
// System.Web.Security.AccessRoleProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.Security {
	public class AccessRoleProvider {
		
		[MonoTODO]
		public void AddUsersToRoles (string [] usernames, string [] rolenames)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void CreateRole (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void DeleteRole (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string [] GetAllRoles ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string [] GetRolesForUser (string username)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string [] GetUsersInRole (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void Initialize (string name, NameValueCollection config)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool IsUserInRole (string username, string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void RemoveUsersFromRoles (string [] usernames, string [] rolenames)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool RoleExists (string rolename)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string ApplicationName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual string Description {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual string Name {
			get { throw new NotImplementedException (); }
		}
	}
}
#endif

