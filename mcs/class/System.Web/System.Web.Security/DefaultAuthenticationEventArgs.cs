/**
 * Namespace: System.Web.Security
 * Class:     DefaultAuthenticationEventArgs
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;

namespace System.Web.Security
{
	public sealed class DefaultAuthenticationEventArgs : EventArgs
	{
		HttpContext context;

		public DefaultAuthenticationEventArgs(HttpContext context)
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
	}
}
