//
// System.Web.Security.RolePrincipal
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ben Maurer
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Security.Permissions;
using System.Security.Principal;

namespace System.Web.Security {

	[Serializable]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class RolePrincipal : IPrincipal {

		private IIdentity identity;
		private string providerName;
		private bool listChanged;
		private bool listCached;
		
		public RolePrincipal (IIdentity identity)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");
			this.identity = identity;
		}

		[MonoTODO]
		public RolePrincipal (IIdentity identity, string encryptedTicket)
			: this (identity)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public RolePrincipal (string providerName, IIdentity identity)
			: this (identity)
		{
			if (providerName == null)
				throw new ArgumentNullException ("providerName");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public RolePrincipal (string providerName, IIdentity identity, string encryptedTicket)
			: this (identity)
		{
			if (providerName == null)
				throw new ArgumentNullException ("providerName");

			throw new NotImplementedException ();
		}
		
		
		[MonoTODO]
		public string [] GetRoles ()
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
		
		public bool CachedListChanged {
			get { return listChanged; }
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
		
		public IIdentity Identity {
			get { return identity; }
		}
		
		public bool IsRoleListCached {
			get { return listCached; }
		}
		
		[MonoTODO]
		public DateTime IssueDate {
			get { throw new NotImplementedException (); }
		}
		
		public string ProviderName {
			get { return providerName; }
		}
		
		[MonoTODO]
		public int Version {
			get { throw new NotImplementedException (); }
		}

		public void SetDirty ()
		{
			listChanged = true;
			listCached = false;
		}
	}
}
#endif

