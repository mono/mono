/**
 * Namespace: System.Web.Security
 * Class:     DefaultAuthenticationModule
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;

namespace System.Web.Security
{
	public sealed class DefaultAuthenticationModule : IHttpModule
	{
		private DefaultAuthenticationEventHandler authenticate;
		
		public DefaultAuthenticationModule()
		{
		}
		
		public void Dispose()
		{
		}
		
		[MonoTODO]
		public void Init(HttpApplication app)
		{
			//TODO: I have to add a DefaultAuthenticationEventHandler sort of thing
			// Could not find a suitable public delegate in HttpApplication.
			// Also need to define a method.
			throw new NotImplementedException();
		}
		
		public event DefaultAuthenticationEventHandler Authenticate
		{
			add
			{
				authenticate += value;
			}
			remove
			{
				authenticate -= value;
			}
		}
	}
}
