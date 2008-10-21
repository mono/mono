//
// DynamicDataRouteHandler.cs
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
using System.Collections.Specialized;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.UI;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DynamicDataRouteHandler : IRouteHandler
	{
		[MonoTODO]
		public static RequestContext GetRequestContext (HttpContext httpContext)
		{
			// HttpRequestBase.QueryString
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MetaTable GetRequestMetaTable (HttpContext httpContext)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetRequestMetaTable (HttpContext httpContext, MetaTable table)
		{
			throw new NotImplementedException ();
		}

		public DynamicDataRouteHandler ()
		{
		}

		public MetaModel Model { get; internal set; }

		[MonoTODO]
		public virtual IHttpHandler CreateHandler (DynamicDataRoute route, MetaTable table, string action)
		{
			var vp = String.Concat (String.Concat (HttpContext.Current.Request.ApplicationPath, "DynamicData/PageTemplates/", action, ".aspx"));
			return (IHttpHandler) BuildManager.CreateInstanceFromVirtualPath (vp, typeof (Page));
		}

		[MonoTODO]
		protected virtual string GetCustomPageVirtualPath (MetaTable table, string viewName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual string GetScaffoldPageVirtualPath (MetaTable table, string viewName)
		{
			throw new NotImplementedException ();
		}

		IHttpHandler IRouteHandler.GetHttpHandler (RequestContext requestContext)
		{
			if (requestContext == null)
				throw new ArgumentNullException ("requestContext");
			RouteData rd = requestContext.RouteData;
			DynamicDataRoute dr = rd.Route as DynamicDataRoute;
			if (dr == null)
				throw new ArgumentException ("The argument RequestContext does not have DynamicDataRoute in its RouteData");
			var action = dr.GetActionFromRouteData (rd);
			var mt = dr.GetTableFromRouteData (rd);

			var rc = new RouteContext () { Route = dr, Action = action, Table = mt };
			IHttpHandler h;
			if (handlers.TryGetValue (rc, out h))
				return h;
			h = CreateHandler (dr, mt, action);
			handlers [rc] = h;
			return h;
		}

		class RouteContext
		{
			public DynamicDataRoute Route;
			public string Action;
			public MetaTable Table;

			public override bool Equals (object obj)
			{
				RouteContext other = obj as RouteContext;
				return other.Route == Route & other.Action == Action && other.Table == Table;
			}

			public override int GetHashCode ()
			{
				return (Route.GetHashCode () << 19) + (Action.GetHashCode () << 9) + Table.GetHashCode ();
			}
		}

		Dictionary<RouteContext,IHttpHandler> handlers = new Dictionary<RouteContext,IHttpHandler> ();
	}
}
