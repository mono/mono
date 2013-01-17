//
// DynamicDataRoute.cs
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
		static readonly object initLock = new object ();
		bool initDone;
		
		public DynamicDataRoute (string url)
			: base (url, null)
		{
			Model = MetaModel.Default;
			RouteHandler = new DynamicDataRouteHandler ();
		}

		public string Action { get; set; }

		public MetaModel Model { get; set; }

		public new DynamicDataRouteHandler RouteHandler { 
			get { return base.RouteHandler as DynamicDataRouteHandler; }
			set { base.RouteHandler = value; }
		}

		public string Table { get; set; }

		public string ViewName { get; set; }

		void EnsureInitialized ()
		{
			if (initDone)
				return;
			
			// We need to lock since we might be stored in the RouteTable.Routes
			// collection which might be accessed from many concurrent requests.
			lock (initLock) {
				if (initDone)
					return;
				
				initDone = true;

				DynamicDataRouteHandler rh = RouteHandler;
				if (rh != null)
					rh.Model = Model;
				
				string action = Action, table = Table;
				if (action == null && table == null)
					return;

				RouteValueDictionary defaults = Defaults;
				if (defaults == null)
					Defaults = defaults = new RouteValueDictionary ();

				if (table != null) {
					// Force check for table existence
					MetaModel model = Model ?? MetaModel.Default;
					if (model != null)
						Model.GetTable (table);
					
					if (defaults.ContainsKey ("Table"))
						defaults ["Table"] = table;
					else
						defaults.Add ("Table", table);
				}
				
				if (action != null) {
					if (defaults.ContainsKey ("Action"))
						defaults ["Action"] = action;
					else
						defaults.Add ("Action", action);
				}
			}
		}
		
		public string GetActionFromRouteData (RouteData routeData)
		{
			if (routeData == null)
				throw new ArgumentNullException ("routeData");
			return routeData.GetRequiredString ("Action");
		}

		public override RouteData GetRouteData (HttpContextBase httpContext)
		{
			EnsureInitialized ();
			RouteData rd = base.GetRouteData (httpContext);

			if (rd == null)
				return null;

			MetaModel model = Model ?? MetaModel.Default;
			MetaTable table;
			if (model == null || !model.TryGetTable (rd.GetRequiredString ("Table"), out table))
				return null;
			
			return rd;
		}

		public MetaTable GetTableFromRouteData (RouteData routeData)
		{
			if (routeData == null)
				throw new ArgumentNullException ("routeData");
			var t = routeData.GetRequiredString ("Table");
			if (Model == null)
				throw new InvalidOperationException ("MetaModel must be set to the DynamicDataRoute before retrieving MetaTable");

			return Model.GetTable (t);
		}

		public override VirtualPathData GetVirtualPath (RequestContext requestContext, RouteValueDictionary values)
		{
			EnsureInitialized ();
			return base.GetVirtualPath (requestContext, values);
		}
	}
}
