/**
 * Namespace: System.Web.Security
 * Class:     WindowsAuthenticationEventArgs
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
	public sealed class WindowsAuthenticationEventArgs : EventArgs
	{
		HttpContext      context;
		WindowsIdentity identity;
		IPrincipal       user;

		public WindowsAuthenticationEventArgs(WindowsIdentity identity, HttpContext context)
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

		public WindowsIdentity Identity
		{
			get
			{
				return identity;
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
				throw new NotImplementedException();
			}
		}
	}
}
