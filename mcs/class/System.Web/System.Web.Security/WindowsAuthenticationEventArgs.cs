//
// System.Web.Security.WindowsAuthenticationEventArgs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Security.Principal;
using System.Web;

namespace System.Web.Security
{
	public sealed class WindowsAuthenticationEventArgs : EventArgs
	{
		WindowsIdentity identity;
		HttpContext context;
		IPrincipal user;

		public WindowsAuthenticationEventArgs (WindowsIdentity identity, HttpContext context)
		{
			this.identity = identity;
			this.context = context;
		}

		public System.Web.HttpContext Context
		{
			get {
				return context;
			}
		}

		public WindowsIdentity Identity
		{
			get {
				return identity;
			}
		}

		public IPrincipal User
		{
			get {
				return user;
			}

			set {
				user = value;
			}
		}
	}
}

