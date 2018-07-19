namespace System.Web.Routing {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpMethodConstraint : IRouteConstraint {

        public HttpMethodConstraint(params string[] allowedMethods) {
            if (allowedMethods == null) {
                throw new ArgumentNullException("allowedMethods");
            }

            AllowedMethods = allowedMethods.ToList().AsReadOnly();
        }

        public ICollection<string> AllowedMethods {
            get;
            private set;
        }

        protected virtual bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (httpContext == null) {
                throw new ArgumentNullException("httpContext");
            }
            if (route == null) {
                throw new ArgumentNullException("route");
            }
            if (parameterName == null) {
                throw new ArgumentNullException("parameterName");
            }
            if (values == null) {
                throw new ArgumentNullException("values");
            }

            switch (routeDirection) {
                case RouteDirection.IncomingRequest:
                    return AllowedMethods.Any(method => String.Equals(method, httpContext.Request.HttpMethod,
                        StringComparison.OrdinalIgnoreCase));

                case RouteDirection.UrlGeneration:
                    // We need to see if the user specified the HTTP method explicitly.  Consider these two routes:
                    //
                    // a) Route: Url = "/{foo}", Constraints = { httpMethod = new HttpMethodConstraint("GET") }
                    // b) Route: Url = "/{foo}", Constraints = { httpMethod = new HttpMethodConstraint("POST") }
                    //
                    // A user might know ahead of time that a URL he is generating might be used with a particular HTTP
                    // method.  If a URL will be used for an HTTP POST but we match on (a) while generating the URL, then
                    // the HTTP GET-specific route will be used for URL generation, which might have undesired behavior.
                    // To prevent this, a user might call RouteCollection.GetVirtualPath(..., { httpMethod = "POST" }) to
                    // signal that he is generating a URL that will be used for an HTTP POST, so he wants the URL
                    // generation to be performed by the (b) route instead of the (a) route, consistent with what would
                    // happen on incoming requests.

                    object parameterValue;
                    if (!values.TryGetValue(parameterName, out parameterValue)) {
                        return true;
                    }

                    string parameterValueString = parameterValue as string;
                    if (parameterValueString == null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentUICulture,
                            SR.GetString(SR.HttpMethodConstraint_ParameterValueMustBeString), parameterName, route.Url));
                    }

                    return AllowedMethods.Any(method => String.Equals(method, parameterValueString,
                        StringComparison.OrdinalIgnoreCase));

                default:
                    return true;
            }
        }

        #region IRouteConstraint Members
        bool IRouteConstraint.Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            return Match(httpContext, route, parameterName, values, routeDirection);
        }
        #endregion

    }
}
