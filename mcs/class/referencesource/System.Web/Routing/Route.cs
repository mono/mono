namespace System.Web.Routing {
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class Route : RouteBase {
        private const string HttpMethodParameterName = "httpMethod";

        private string _url;
        private ParsedRoute _parsedRoute;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "This is a URL template with special characters, not just a regular valid URL.")]
        public Route(string url, IRouteHandler routeHandler) {
            Url = url;
            RouteHandler = routeHandler;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "This is a URL template with special characters, not just a regular valid URL.")]
        public Route(string url, RouteValueDictionary defaults, IRouteHandler routeHandler) {
            Url = url;
            Defaults = defaults;
            RouteHandler = routeHandler;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "This is a URL template with special characters, not just a regular valid URL.")]
        public Route(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler) {
            Url = url;
            Defaults = defaults;
            Constraints = constraints;
            RouteHandler = routeHandler;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "This is a URL template with special characters, not just a regular valid URL.")]
        public Route(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler) {
            Url = url;
            Defaults = defaults;
            Constraints = constraints;
            DataTokens = dataTokens;
            RouteHandler = routeHandler;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "This property is settable so people can use object initializers.")]
        public RouteValueDictionary Constraints {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "This property is settable so people can use object initializers.")]
        public RouteValueDictionary DataTokens {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "This property is settable so people can use object initializers.")]
        public RouteValueDictionary Defaults {
            get;
            set;
        }

        public IRouteHandler RouteHandler {
            get;
            set;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
            Justification = "This is a URL template with special characters, not just a regular valid URL.")]
        public string Url {
            get {
                return _url ?? String.Empty;
            }
            set {
                // The parser will throw for invalid routes. We don't have to worry
                // about _parsedRoute getting out of sync with _url since the latter
                // won't get set unless we can parse the route.
                _parsedRoute = RouteParser.Parse(value);

                // If we passed the parsing stage, save the original URL value
                _url = value;
            }
        }

        public override RouteData GetRouteData(HttpContextBase httpContext) {
            // Parse incoming URL (we trim off the first two chars since they're always "~/")
            string requestPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;

            RouteValueDictionary values = _parsedRoute.Match(requestPath, Defaults);

            if (values == null) {
                // If we got back a null value set, that means the URL did not match
                return null;
            }

            RouteData routeData = new RouteData(this, RouteHandler);

            // Validate the values
            if (!ProcessConstraints(httpContext, values, RouteDirection.IncomingRequest)) {
                return null;
            }

            // Copy the matched values
            foreach (var value in values) {
                routeData.Values.Add(value.Key, value.Value);
            }

            // Copy the DataTokens from the Route to the RouteData
            if (DataTokens != null) {
                foreach (var prop in DataTokens) {
                    routeData.DataTokens[prop.Key] = prop.Value;
                }
            }

            return routeData;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values) {
            // Try to generate a URL that represents the values passed in based on current
            // values from the RouteData and new values using the specified Route.

            BoundUrl result = _parsedRoute.Bind(requestContext.RouteData.Values, values, Defaults, Constraints);

            if (result == null) {
                return null;
            }

            // Verify that the route matches the validation rules
            if (!ProcessConstraints(requestContext.HttpContext, result.Values, RouteDirection.UrlGeneration)) {
                return null;
            }

            VirtualPathData vpd = new VirtualPathData(this, result.Url);

            // Add the DataTokens from the Route to the VirtualPathData
            if (DataTokens != null) {
                foreach (var prop in DataTokens) {
                    vpd.DataTokens[prop.Key] = prop.Value;
                }
            }
            return vpd;
        }

        protected virtual bool ProcessConstraint(HttpContextBase httpContext, object constraint, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            IRouteConstraint customConstraint = constraint as IRouteConstraint;
            if (customConstraint != null) {
                return customConstraint.Match(httpContext, this, parameterName, values, routeDirection);
            }

            // If there was no custom constraint, then treat the constraint as a string which represents a Regex.
            string constraintsRule = constraint as string;
            if (constraintsRule == null) {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentUICulture,
                    SR.GetString(SR.Route_ValidationMustBeStringOrCustomConstraint),
                    parameterName,
                    Url));
            }

            object parameterValue;
            values.TryGetValue(parameterName, out parameterValue);
            string parameterValueString = Convert.ToString(parameterValue, CultureInfo.InvariantCulture);
            string constraintsRegEx = "^(" + constraintsRule + ")$";
            return Regex.IsMatch(parameterValueString, constraintsRegEx,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        }

        private bool ProcessConstraints(HttpContextBase httpContext, RouteValueDictionary values, RouteDirection routeDirection) {
            if (Constraints != null) {
                foreach (var constraintsItem in Constraints) {
                    if (!ProcessConstraint(httpContext, constraintsItem.Value, constraintsItem.Key, values, routeDirection)) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
