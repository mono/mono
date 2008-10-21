//
// DynamicDataRoute.cs
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
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.Routing;

namespace System.Web.DynamicData
{
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DynamicDataRoute : Route
	{
		public DynamicDataRoute (string url)
			: base (url, null)
		{
			Model = MetaModel.Default;
			RouteHandler = new DynamicDataRouteHandler ();
		}

		public string Action { get; set; }

		public MetaModel Model { get; set; }

		public DynamicDataRouteHandler RouteHandler { 
			get { return (DynamicDataRouteHandler) base.RouteHandler; }
			set { base.RouteHandler = value; }
		}

		public string Table { get; set; }

		public string ViewName { get; set; }

		public string GetActionFromRouteData (RouteData routeData)
		{
			if (routeData == null)
				throw new ArgumentNullException ("routeData");
			return routeData.GetRequiredString ("Action");
		}

		public override RouteData GetRouteData (HttpContextBase httpContext)
		{
			var rd = base.GetRouteData (httpContext);
			// FIXME: something to do here?
			return rd;
		}

		public MetaTable GetTableFromRouteData (RouteData routeData)
		{
			if (routeData == null)
				throw new ArgumentNullException ("routeData");
			var t = routeData.GetRequiredString ("Table");
			if (Model == null)
				throw new InvalidOperationException ("MetaModel must be set to the DynamicDataRoute before retrieving MetaTable");
			MetaTable mt;
			return Model.GetTable (t);
		}

		public override VirtualPathData GetVirtualPath (RequestContext requestContext, RouteValueDictionary values)
		{
			var rd = requestContext.RouteData;
			var t = GetTableFromRouteData (rd);
			var a = GetActionFromRouteData (rd);
			var vp = String.Concat (t.GetActionPath (a));
			return new VirtualPathData (this, vp);
		}
	}
}
