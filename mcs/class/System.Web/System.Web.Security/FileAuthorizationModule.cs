/**
 * Namespace: System.Web.Security
 * Class:     FileAuthorizationModule
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
	public sealed class FileAuthorizationModule : IHttpModule
	{
		public FileAuthorizationModule()
		{
		}

		public void Dispose()
		{
		}

		[MonoTODO]
		public void Init(HttpApplication app)
		{
			throw new NotImplementedException();
		}
	}
}
