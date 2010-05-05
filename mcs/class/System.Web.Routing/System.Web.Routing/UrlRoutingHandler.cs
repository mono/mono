//
// UrlRoutingHandler.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008-2010 Novell Inc. http://novell.com
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
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Web;

namespace System.Web.Routing
{
#if NET_4_0
	[TypeForwardedFrom ("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class UrlRoutingHandler : IHttpHandler
	{
		RouteCollection routes;

		bool IHttpHandler.IsReusable {
			get { return IsReusable; }
		}

		protected virtual bool IsReusable { get { return false; } }

		public RouteCollection RouteCollection {
			get {
				if (routes == null)
					routes = RouteTable.Routes;
				return routes;
			}
			set { routes = value; }
		}

		void IHttpHandler.ProcessRequest (HttpContext context)
		{
			ProcessRequest (context);
		}

		protected virtual void ProcessRequest (HttpContext httpContext)
		{
			ProcessRequest (new HttpContextWrapper (httpContext));
		}

		protected virtual void ProcessRequest (HttpContextBase httpContext)
		{
			if (httpContext == null)
				throw new ArgumentNullException ("httpContext");

			var rd = RouteCollection.GetRouteData (httpContext);
			if (rd == null)
				throw new HttpException ("The incoming request does not match any route");
			if (rd.RouteHandler == null)
				throw new InvalidOperationException ("No  IRouteHandler is assigned to the selected route");

			RequestContext rc = new RequestContext (httpContext, rd);

			var hh = rd.RouteHandler.GetHttpHandler (rc);
			VerifyAndProcessRequest (hh, httpContext);
		}

		protected abstract void VerifyAndProcessRequest (IHttpHandler httpHandler, HttpContextBase httpContext);
	}
}
