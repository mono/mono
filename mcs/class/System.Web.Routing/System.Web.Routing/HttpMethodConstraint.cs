//
// HttpMethodConstraint.cs
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
	public class HttpMethodConstraint : IRouteConstraint
	{
		public HttpMethodConstraint (params string[] allowedMethods)
		{
			if (allowedMethods == null)
				throw new ArgumentNullException ("allowedMethods");
			AllowedMethods = allowedMethods;
		}

		public ICollection<string> AllowedMethods { get; private set; }

		bool IRouteConstraint.Match (HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			return Match (httpContext, route, parameterName, values, routeDirection);
		}

		protected virtual bool Match (HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if (httpContext == null)
				throw new ArgumentNullException ("httpContext");
			if (route == null)
				throw new ArgumentNullException ("route");
			if (parameterName == null)
				throw new ArgumentNullException ("parameterName");
			if (values == null)
				throw new ArgumentNullException ("values");

			switch (routeDirection) {
			case RouteDirection.IncomingRequest:
				// LAMESPEC: .NET allows case-insensitive comparison, which violates RFC 2616
				return AllowedMethods.Contains (httpContext.Request.HttpMethod);

			case RouteDirection.UrlGeneration:
				// See: aspnetwebstack's WebAPI equivalent for details.
				object method;

				if (!values.TryGetValue (parameterName, out method))
					return true;

				// LAMESPEC: .NET allows case-insensitive comparison, which violates RFC 2616
				return AllowedMethods.Contains (Convert.ToString (method));

			default:
				throw new ArgumentException ("Invalid routeDirection: " + routeDirection);
			}
		}
	}
}
