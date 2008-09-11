//
// UrlRoutingModule.cs
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
using System.Security.Permissions;
using System.Web;

namespace System.Web.Routing
{
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class UrlRoutingModule : IHttpModule
	{
		[MonoTODO]
		public RouteCollection RouteCollection { get; set; }

		protected virtual void Dispose ()
		{
		}

		void IHttpModule.Dispose ()
		{
			Dispose ();
		}

		void IHttpModule.Init (HttpApplication application)
		{
			Init (application);
		}

		[MonoTODO ("FIXME: add correct arguments")]
		protected virtual void Init (HttpApplication application)
		{
			application.PostMapRequestHandler += PostMapRequestHandler;
			application.PostResolveRequestCache += PostResolveRequestCache;
		}

		void PostMapRequestHandler (object o, EventArgs e)
		{
			var app = (HttpApplication) o;
			PostMapRequestHandler (new HttpContextWrapper (app.Context));
		}

		void PostResolveRequestCache (object o, EventArgs e)
		{
			var app = (HttpApplication) o;
			PostResolveRequestCache (new HttpContextWrapper (app.Context));
		}

		[MonoTODO]
		public virtual void PostMapRequestHandler (HttpContextBase context)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void PostResolveRequestCache (HttpContextBase context)
		{
			throw new NotImplementedException ();
		}
	}
}
