//
// System.Web.Security.FormsAuthenticationEventArgs
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
	public sealed class FormsAuthenticationEventArgs : EventArgs
	{
		IPrincipal user;
		HttpContext context;

		public FormsAuthenticationEventArgs (HttpContext context)
		{
			this.context = context;
		}

		public HttpContext Context
		{
			get {
				return context;
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

