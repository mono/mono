//
// DynamicDataRouteHandler.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008-2009 Novell Inc. http://novell.com
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
using System.Threading;
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
		static ReaderWriterLockSlim contextsLock = new ReaderWriterLockSlim ();
		
		static Dictionary <HttpContext, RouteContext> contexts = new Dictionary <HttpContext, RouteContext> ();
		Dictionary <RouteContext, IHttpHandler> handlers;

		Dictionary <RouteContext, IHttpHandler> Handlers {
			get {
				if (handlers == null)
					handlers = new Dictionary <RouteContext, IHttpHandler> ();

				return handlers;
			}
		}

		public static RequestContext GetRequestContext (HttpContext httpContext)
		{
			if (httpContext == null)
				throw new ArgumentNullException ("httpContext");
			
			RouteContext rc;
			bool locked = false;
			try {
				contextsLock.EnterReadLock ();
				locked = true;
				if (contexts.TryGetValue (httpContext, out rc) && rc != null)
					return rc.Context;
			} finally {
				if (locked)
					contextsLock.ExitReadLock ();
			}

			var ctxWrapper = new HttpContextWrapper (httpContext);
			RouteData rd = RouteTable.Routes.GetRouteData (ctxWrapper);
			if (rd == null)
				rd = new RouteData ();
			
			var ret = new RequestContext (ctxWrapper, rd);

			locked = false;
			try {
				contextsLock.EnterWriteLock ();
				locked = true;
				contexts.Add (httpContext, MakeRouteContext (ret, null, null, null));
			} finally {
				if (locked)
					contextsLock.ExitWriteLock ();
			}

			return ret;
		}

		public static MetaTable GetRequestMetaTable (HttpContext httpContext)
		{
			if (httpContext == null)
				throw new ArgumentNullException ("httpContext");

			RouteContext rc;
			bool locked = false;
			try {
				contextsLock.EnterReadLock ();
				locked = true;
				if (contexts.TryGetValue (httpContext, out rc) && rc != null)
					return rc.Table;
			} finally {
				if (locked)
					contextsLock.ExitReadLock ();
			}

			return null;
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
			var dr = rd.Route as DynamicDataRoute;
			if (dr == null)
				throw new ArgumentException ("The argument RequestContext does not have DynamicDataRoute in its RouteData");
			string action = dr.GetActionFromRouteData (rd);
			MetaTable mt = dr.GetTableFromRouteData (rd);
			RouteContext rc = MakeRouteContext (requestContext, dr, action, mt);
			IHttpHandler h;
			
			Dictionary <RouteContext, IHttpHandler> handlers = Handlers;
			if (handlers.TryGetValue (rc, out h))
				return h;
			h = CreateHandler (dr, mt, action);
			handlers.Add (rc, h);
			return h;
		}

		static RouteContext MakeRouteContext (RequestContext context, DynamicDataRoute route, string action, MetaTable table)
		{
			RouteData rd = null;
			
			if (route == null) {
				rd = context.RouteData;
				route = rd.Route as DynamicDataRoute;
			}

			if (route != null) {
				if (action == null) {
					if (rd == null)
						rd = context.RouteData;
					action = route.GetActionFromRouteData (rd);
				}
			
				if (table == null) {
					if (rd == null)
						rd = context.RouteData;
				
					table = route.GetTableFromRouteData (rd);
				}
			}
			
			return new RouteContext () {
				Route = route,
				Action = action,
				Table = table,
				Context = context};
		}
		
		sealed class RouteContext
		{
			public DynamicDataRoute Route;
			public string Action;
			public MetaTable Table;
			public RequestContext Context;

			public RouteContext ()
			{
			}
			
			public override bool Equals (object obj)
			{
				RouteContext other = obj as RouteContext;
				return other.Route == Route & other.Action == Action && other.Table == Table && other.Context == Context;
			}

			public override int GetHashCode ()
			{
				return (Route != null ? Route.GetHashCode () << 27 : 0) +
					(Action != null ? Action.GetHashCode () << 19 : 0) +
					(Table != null ? Table.GetHashCode () << 9 : 0) +
					(Context != null ? Context.GetHashCode () : 0);
			}
		}
	}
}
