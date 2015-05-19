//
// Route.cs
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
using System.Text.RegularExpressions;
using System.Web;
using System.Globalization;

namespace System.Web.Routing
{
	[TypeForwardedFrom ("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class Route : RouteBase
	{
		static readonly Type httpRequestBaseType = typeof (HttpRequestBase);
		PatternParser url;

		public RouteValueDictionary Constraints { get; set; }

		public RouteValueDictionary DataTokens { get; set; }

		public RouteValueDictionary Defaults { get; set; }

		public IRouteHandler RouteHandler { get; set; }

		public string Url {
			get { return url != null ? url.Url : String.Empty; }
			set { url = value != null ? new PatternParser (value) : new PatternParser (String.Empty); }
		}

		public Route (string url, IRouteHandler routeHandler)
			: this (url, null, routeHandler)
		{
		}

		public Route (string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
			: this (url, defaults, null, routeHandler)
		{
		}

		public Route (string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
			: this (url, defaults, constraints, null, routeHandler)
		{
		}

		public Route (string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
		{
			Url = url;
			Defaults = defaults;
			Constraints = constraints;
			DataTokens = dataTokens;
			RouteHandler = routeHandler;
		}

		public override RouteData GetRouteData (HttpContextBase httpContext)
		{
			var path = httpContext.Request.AppRelativeCurrentExecutionFilePath;
			var pathInfo = httpContext.Request.PathInfo;

			if (!String.IsNullOrEmpty (pathInfo))
				path += pathInfo;

			// probably code like this causes ArgumentOutOfRangeException under .NET.
			// It somehow allows such path that is completely equivalent to the Url. Dunno why.
			if (Url != path && path.Substring (0, 2) != "~/")
				return null;
			path = path.Substring (2);

			var values = url.Match (path, Defaults);
			if (values == null)
				return null;

			if (!ProcessConstraints (httpContext, values, RouteDirection.IncomingRequest))
				return null;
			
			var rd = new RouteData (this, RouteHandler);
			RouteValueDictionary rdValues = rd.Values;
			
			foreach (var p in values)
				rdValues.Add (p.Key, p.Value);

			RouteValueDictionary dataTokens = DataTokens;
			if (dataTokens != null) {
				RouteValueDictionary rdDataTokens = rd.DataTokens;
				foreach (var token in dataTokens)
					rdDataTokens.Add (token.Key, token.Value);
			}
			
			return rd;
		}

		public override VirtualPathData GetVirtualPath (RequestContext requestContext, RouteValueDictionary values)
		{
			if (requestContext == null)
				throw new ArgumentNullException ("requestContext");
			if (url == null)
				return new VirtualPathData (this, String.Empty);

			// null values is allowed.
			// if (values == null)
			// 	values = requestContext.RouteData.Values;

			RouteValueDictionary usedValues;
			string resultUrl = url.BuildUrl (this, requestContext, values, Constraints, out usedValues);

			if (resultUrl == null)
				return null;

			if (!ProcessConstraints (requestContext.HttpContext, usedValues, RouteDirection.UrlGeneration))
				return null;

			var result = new VirtualPathData (this, resultUrl);

			RouteValueDictionary dataTokens = DataTokens;
			if (dataTokens != null) {
				foreach (var item in dataTokens)
					result.DataTokens[item.Key] = item.Value;
			}

			return result;
		}

		private bool ProcessConstraintInternal (HttpContextBase httpContext, Route route, object constraint, string parameterName,
								RouteValueDictionary values, RouteDirection routeDirection, RequestContext reqContext,
								out bool invalidConstraint)
		{
			invalidConstraint = false;
			IRouteConstraint irc = constraint as IRouteConstraint;
			if (irc != null)
				return irc.Match (httpContext, route, parameterName, values, routeDirection);

			string s = constraint as string;
			if (s != null) {
				string v = null;
				object o;

				// NOTE: If constraint was not an IRouteConstraint, is is asumed
				// to be an object 'convertible' to string, or at least this is how
				// ASP.NET seems to work by the tests i've done latelly. (pruiz)

				if (values != null && values.TryGetValue (parameterName, out o))
					v = Convert.ToString (o, CultureInfo.InvariantCulture);

				if (!String.IsNullOrEmpty (v))
					return MatchConstraintRegex (v, s);
				else if (reqContext != null) {
					RouteData rd = reqContext != null ? reqContext.RouteData : null;
					RouteValueDictionary rdValues = rd != null ? rd.Values : null;

					if (rdValues == null || rdValues.Count == 0)
						return false;
					
					if (!rdValues.TryGetValue (parameterName, out o))
						return false;

					v = Convert.ToString (o, CultureInfo.InvariantCulture);
					if (String.IsNullOrEmpty (v))
						return false;

					return MatchConstraintRegex (v, s);
				}
				return false;
			}

			invalidConstraint = true;
			return false;
		}

		static bool MatchConstraintRegex (string value, string constraint)
		{
			int len = constraint.Length;
			if (len > 0) {
				// Bug #651966 - regexp constraints must be treated
				// as absolute expressions
				if (constraint [0] != '^') {
					constraint = "^" + constraint;
					len++;
				}

				if (constraint [len - 1] != '$')
					constraint += "$";
			}

			return Regex.IsMatch (value, constraint, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}
		
		protected virtual bool ProcessConstraint (HttpContextBase httpContext, object constraint, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if (parameterName == null)
				throw new ArgumentNullException ("parameterName");

			// .NET "compatibility"
			if (values == null)
				throw new NullReferenceException ();

			RequestContext reqContext;
			reqContext = SafeGetContext (httpContext != null ? httpContext.Request : null);
			bool invalidConstraint;
			bool ret = ProcessConstraintInternal (httpContext, this, constraint, parameterName, values, routeDirection, reqContext, out invalidConstraint);
			
			if (invalidConstraint)
				throw new InvalidOperationException (
					String.Format (
						"Constraint parameter '{0}' on the route with URL '{1}' must have a string value type or be a type which implements IRouteConstraint",
						parameterName, Url
					)
				);

			return ret;
		}

		private bool ProcessConstraints (HttpContextBase httpContext, RouteValueDictionary values, RouteDirection routeDirection)
		{
			var constraints = Constraints;

			if (Constraints != null) {
				foreach (var p in constraints)
					if (!ProcessConstraint (httpContext, p.Value, p.Key, values, routeDirection))
						return false;
			}

			return true;
		}

		RequestContext SafeGetContext (HttpRequestBase req)
		{
			if (req == null || req.GetType () != httpRequestBaseType)
				return null;
				
			return req.RequestContext;
		}
	}
}
