//
// System.Web.Security.RolePrincipal
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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

