/**
 * Namespace: System.Web.Security
 * Class:     PassportAuthenticationEventArgs
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
using System.Security.Principal;
using System.Web;

namespace System.Web.Security
{
	public sealed class PassportAuthenticationEventArgs : EventArgs
	{
		HttpContext      context;
		PassportIdentity identity;
		IPrincipal       user;

		public PassportAuthenticationEventArgs(PassportIdentity identity, HttpContext context)
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

		public PassportIdentity Identity
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
