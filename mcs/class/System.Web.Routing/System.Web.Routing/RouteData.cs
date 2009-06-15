//
// RouteData.cs
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
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Web;

namespace System.Web.Routing
{
#if NET_4_0
	[TypeForwardedFrom ("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class RouteData
	{
		public RouteData ()
			: this (null, null)
		{
		}

		public RouteData (RouteBase route, IRouteHandler routeHandler)
		{
			// arguments can be null.
			Route = route;
			RouteHandler = routeHandler;

			DataTokens = new RouteValueDictionary ();
			Values = new RouteValueDictionary ();
		}

		public RouteValueDictionary DataTokens { get; private set; }

		public RouteBase Route { get; set; }

		public IRouteHandler RouteHandler { get; set; }

		public RouteValueDictionary Values { get; private set; }

		public string GetRequiredString (string valueName)
		{
			object o;
			if (!Values.TryGetValue (valueName, out o))
				throw new InvalidOperationException (String.Format ("value name {0} does not match any of the values.", valueName));
			string s = o as string;
			if (String.IsNullOrEmpty (s))
				throw new InvalidOperationException (String.Format ("The value for the name {0} must be a non-empty string", valueName));
			return s;
		}
	}
}
