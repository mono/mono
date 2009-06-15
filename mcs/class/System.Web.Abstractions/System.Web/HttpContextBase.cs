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
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Profile;

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpContextBase : IServiceProvider
	{
		void NotImplemented ()
		{
			throw new NotImplementedException ();
		}

		public virtual Exception [] AllErrors { get { NotImplemented (); return null; } }

		public virtual HttpApplicationStateBase Application { get { NotImplemented (); return null; } }

		public virtual HttpApplication ApplicationInstance { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual Cache Cache { get { NotImplemented (); return null; } }

		public virtual IHttpHandler CurrentHandler { get { NotImplemented (); return null; } }

		public virtual RequestNotification CurrentNotification { get { NotImplemented (); return default (RequestNotification); } }

		public virtual Exception Error { get { NotImplemented (); return null; } }

		public virtual IHttpHandler Handler { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual bool IsCustomErrorEnabled { get { NotImplemented (); return false; } }

		public virtual bool IsDebuggingEnabled { get { NotImplemented (); return false; } }

		public virtual bool IsPostNotification { get { NotImplemented (); return false; } }

		public virtual IDictionary Items { get { NotImplemented (); return null; } }

		public virtual IHttpHandler PreviousHandler { get { NotImplemented (); return null; } }

		public virtual ProfileBase Profile { get { NotImplemented (); return null; } }

		public virtual HttpRequestBase Request { get { NotImplemented (); return null; } }

		public virtual HttpResponseBase Response { get { NotImplemented (); return null; } }

		public virtual HttpServerUtilityBase Server { get { NotImplemented (); return null; } }

		public virtual HttpSessionStateBase Session { get { NotImplemented (); return null; } }

		public virtual bool SkipAuthorization { get { NotImplemented (); return false; } set { NotImplemented (); } }

		public virtual DateTime Timestamp { get { NotImplemented (); return DateTime.MinValue; } }

		public virtual TraceContext Trace { get { NotImplemented (); return null; } }

		public virtual IPrincipal User { get { NotImplemented (); return null; } set { NotImplemented (); } }

		public virtual void AddError (Exception errorInfo)
		{
			NotImplemented ();
		}

		public virtual void ClearError ()
		{
			NotImplemented ();
		}

		public virtual object GetGlobalResourceObject (string classKey, string resourceKey)
		{
			NotImplemented ();
			return null;
		}

		public virtual object GetGlobalResourceObject (string classKey, string resourceKey, CultureInfo culture)
		{
			NotImplemented ();
			return null;
		}

		public virtual object GetLocalResourceObject (string virtualPath, string resourceKey)
		{
			NotImplemented ();
			return null;
		}

		public virtual object GetLocalResourceObject (string virtualPath, string resourceKey, CultureInfo culture)
		{
			NotImplemented ();
			return null;
		}

		public virtual object GetSection (string sectionName)
		{
			NotImplemented ();
			return null;
		}

		public virtual object GetService (Type serviceType)
		{
			NotImplemented ();
			return null;
		}

		public virtual void RewritePath (string path)
		{
			NotImplemented ();
		}

		public virtual void RewritePath (string path, bool rebaseClientPath)
		{
			NotImplemented ();
		}

		public virtual void RewritePath (string filePath, string pathInfo, string queryString)
		{
			NotImplemented ();
		}

		public virtual void RewritePath (string filePath, string pathInfo, string queryString, bool setClientFilePath)
		{
			NotImplemented ();
		}
	}
}
