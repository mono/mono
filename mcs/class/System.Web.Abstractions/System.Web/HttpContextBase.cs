//
// HttpContextBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
//

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Profile;

namespace System.Web
{
	[MonoTODO]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpContextBase : IServiceProvider
	{
		[MonoTODO]
		public virtual Exception [] AllErrors { get; private set; }
		[MonoTODO]
		public virtual HttpApplicationStateBase Application { get; private set; }
		[MonoTODO]
		public virtual HttpApplication ApplicationInstance { get; set; }
		[MonoTODO]
		public virtual Cache Cache { get; private set; }
		[MonoTODO]
		public virtual IHttpHandler CurrentHandler { get; private set; }
		[MonoTODO]
		public virtual RequestNotification CurrentNotification { get; private set; }
		[MonoTODO]
		public virtual Exception Error { get; private set; }
		[MonoTODO]
		public virtual IHttpHandler Handler { get; set; }
		[MonoTODO]
		public virtual bool IsCustomErrorEnabled { get; private set; }
		[MonoTODO]
		public virtual bool IsDebuggingEnabled { get; private set; }
		[MonoTODO]
		public virtual bool IsPostNotification { get; private set; }
		[MonoTODO]
		public virtual IDictionary Items { get; private set; }
		[MonoTODO]
		public virtual IHttpHandler PreviousHandler { get; private set; }
		[MonoTODO]
		public virtual ProfileBase Profile { get; private set; }
		[MonoTODO]
		public virtual HttpRequestBase Request { get; private set; }
		[MonoTODO]
		public virtual HttpResponseBase Response { get; private set; }
		[MonoTODO]
		public virtual HttpServerUtilityBase Server { get; private set; }
		[MonoTODO]
		public virtual HttpSessionStateBase Session { get; private set; }
		[MonoTODO]
		public virtual bool SkipAuthorization { get; set; }
		[MonoTODO]
		public virtual DateTime Timestamp { get; private set; }
		[MonoTODO]
		public virtual TraceContext Trace { get; private set; }
		[MonoTODO]
		public virtual IPrincipal User { get; set; }

		[MonoTODO]
		public virtual void AddError (Exception errorInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ClearError ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetGlobalResourceObject (string classKey, string resourceKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetGlobalResourceObject (string classKey, string resourceKey, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetLocalResourceObject (string virtualPath, string resourceKey)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetLocalResourceObject (string virtualPath, string resourceKey, CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetSection (string sectionName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetService (Type serviceType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RewritePath (string path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RewritePath (string path, bool rebaseClientPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RewritePath (string filePath, string pathInfo, string queryString)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RewritePath (string filePath, string pathInfo, string queryString, bool setClientFilePath)
		{
			throw new NotImplementedException ();
		}
	}
}
