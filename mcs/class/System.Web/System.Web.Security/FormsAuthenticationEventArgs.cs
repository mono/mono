/**
 * Namespace: System.Web.Security
 * Class:     FormsAuthenticationEventArgs
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  90%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Security.Principal;

namespace System.Web.Security
{
	public sealed class FormsAuthenticationEventArgs : EventArgs
	{
		HttpContext context;
		IPrincipal  user;

		public FormsAuthenticationEventArgs(HttpContext context)
		{
			this.context = context;
		}

		public HttpContext Context
		{
			get
			{
				return context;
			}
		}

		[MonoTODO]
		public IPrincipal User
		{
			get
			{
				return user;
			}
			set
			{
				// context.User?
				throw new NotImplementedException();
			}
		}
	}
}
