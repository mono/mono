//
// System.Web.Security.PassportAuthenticationEventArgs
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
	public sealed class PassportAuthenticationEventArgs : EventArgs
	{
		PassportIdentity identity;
		HttpContext context;
		IPrincipal user;

		public PassportAuthenticationEventArgs (PassportIdentity identity, HttpContext context)
		{
			this.identity = identity;
			this.context = context;
		}

		public HttpContext Context
		{
			get {
				return context;
			}
		}

		public PassportIdentity Identity
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

