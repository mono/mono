//
// System.Web.Security.RolePrincipal
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
using System.Security;
using System.Security.Principal;

namespace System.Web.Security {
	public sealed class RolePrincipal : IPrincipal {
		
		[MonoTODO]
		public RolePrincipal ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public RolePrincipal (bool createFromCookie)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public RolePrincipal (string encryptedTicket)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string [] GetRoles ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Init ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void InitFromCookie (string cookieName)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void InitFromEncryptedTicket (string strTicket)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool IsInRole (string role)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string ToEncryptedTicket ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool CachedListChanged {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string CookiePath {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public bool Expired {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public DateTime ExpireDate {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public IIdentity Identity {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public bool IsRoleListCached {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public DateTime IssueDate {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public string UserData {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public int Version {
			get { throw new NotImplementedException (); }
		}
	}
}
#endif

