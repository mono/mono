namespace System.Web.Routing {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Web.Hosting;

    [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class RouteCollection : Collection<RouteBase> {
        private Dictionary<string, RouteBase> _namedMap = new Dictionary<string, RouteBase>(StringComparer.OrdinalIgnoreCase);
        private VirtualPathProvider _vpp;

        private ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        public RouteCollection() {
        }

        public RouteCollection(VirtualPathProvider virtualPathProvider) {
            VPP = virtualPathProvider;
        }

        public bool AppendTrailingSlash {
            get;
            set;
        }

        public bool LowercaseUrls {
            get;
            set;
        }

        public bool RouteExistingFiles {
            get;
            set;
        }

        private VirtualPathProvider VPP {
            get {
                if (_vpp == null) {
                    return HostingEnvironment.VirtualPathProvider;
                }
                return _vpp;
            }
            set {
                _vpp = value;
            }
        }

        public RouteBase this[string name] {
            get {
                if (String.IsNullOrEmpty(name)) {
                    return null;
                }
                RouteBase route;
                if (_namedMap.TryGetValue(name, out route)) {
                    return route;
                }
                return null;
            }
        }

        public void Add(string name, RouteBase item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            if (!String.IsNullOrEmpty(name)) {
                if (_namedMap.ContainsKey(name)) {
                    throw new ArgumentException(
                        String.Format(
                            CultureInfo.CurrentUICulture,
                            SR.GetString(SR.RouteCollection_DuplicateName),
                            name),
                        "name");
                }
            }

            Add(item);
            if (!String.IsNullOrEmpty(name)) {
                _namedMap[name] = item;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Warning was suppressed for consistency with existing similar routing API")]
        public Route MapPageRoute(string routeName, string routeUrl, string physicalFile) {
            return MapPageRoute(routeName, routeUrl, physicalFile, true /* checkPhysicalUrlAccess */, null, null, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Warning was suppressed for consistency with existing similar routing API")]
        public Route MapPageRoute(string routeName, string routeUrl, string physicalFile, bool checkPhysicalUrlAccess) {
            return MapPageRoute(routeName, routeUrl, physicalFile, checkPhysicalUrlAccess, null, null, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Warning was suppressed for consistency with existing similar routing API")]
        public Route MapPageRoute(string routeName, string routeUrl, string physicalFile, bool checkPhysicalUrlAccess, RouteValueDictionary defaults) {
            return MapPageRoute(routeName, routeUrl, physicalFile, checkPhysicalUrlAccess, defaults, null, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Warning was suppressed for consistency with existing similar routing API")]
        public Route MapPageRoute(string routeName, string routeUrl, string physicalFile, bool checkPhysicalUrlAccess, RouteValueDictionary defaults, RouteValueDictionary constraints) {
            return MapPageRoute(routeName, routeUrl, physicalFile, checkPhysicalUrlAccess, defaults, constraints, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Warning was suppressed for consistency with existing similar routing API")]
        public Route MapPageRoute(string routeName, string routeUrl, string physicalFile, bool checkPhysicalUrlAccess, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens) {
            if (routeUrl == null) {
                throw new ArgumentNullException("routeUrl");
            }
            Route route = new Route(routeUrl, defaults, constraints, dataTokens, new PageRouteHandler(physicalFile, checkPhysicalUrlAccess));
            Add(routeName, route);
            return route;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void ClearItems() {
            _namedMap.Clear();
            base.ClearItems();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not worth a breaking change.")]
        public IDisposable GetReadLock() {
            _rwLock.EnterReadLock();
            return new ReadLockDisposable(_rwLock);
        }

        private RequestContext GetRequestContext(RequestContext requestContext) {
            if (requestContext != null) {
                return requestContext;
            }
            HttpContext httpContext = HttpContext.Current;
            if (httpContext == null) {
                throw new InvalidOperationException(SR.GetString(SR.RouteCollection_RequiresContext));
            }
            return new RequestContext(new HttpContextWrapper(httpContext), new RouteData());
        }

        // Returns true if this is a request to an existing file
        private bool IsRouteToExistingFile(HttpContextBase httpContext) {
            string requestPath = httpContext.Request.AppRelativeCurrentExecutionFilePath;
            return ((requestPath != "~/") &&
                (VPP != null) &&
                (VPP.FileExists(requestPath) ||
                VPP.DirectoryExists(requestPath)));
        }

        public RouteData GetRouteData(HttpContextBase httpContext) {
            if (httpContext == null) {
                throw new ArgumentNullException("httpContext");
            }
            if (httpContext.Request == null) {
                throw new ArgumentException(SR.GetString(SR.RouteTable_ContextMissingRequest), "httpContext");
            }

            // Optimize performance when the route collection is empty.  The main improvement is that we avoid taking
            // a read lock when the collection is empty.  Without this check, the UrlRoutingModule causes a 25%-50%
            // regression in HelloWorld RPS due to lock contention.  The UrlRoutingModule is now in the root web.config,
            // so we need to ensure the module is performant, especially when you are not using routing.
            // This check does introduce a slight 


            if (Count == 0) {
                return null;
            }

            bool isRouteToExistingFile = false;
            bool doneRouteCheck = false; // We only want to do the route check once
            if (!RouteExistingFiles) {
                isRouteToExistingFile = IsRouteToExistingFile(httpContext);
                doneRouteCheck = true;
                if (isRouteToExistingFile) {
                    // If we're not routing existing files and the file exists, we stop processing routes
                    return null;
                }
            }

            // Go through all the configured routes and find the first one that returns a match
            using (GetReadLock()) {
                foreach (RouteBase route in this) {
                    RouteData routeData = route.GetRouteData(httpContext);
                    if (routeData != null) {
                        // If we're not routing existing files on this route and the file exists, we also stop processing routes
                        if (!route.RouteExistingFiles) {
                            if (!doneRouteCheck) {
                                isRouteToExistingFile = IsRouteToExistingFile(httpContext);
                                doneRouteCheck = true;
                            }
                            if (isRouteToExistingFile) {
                                return null;
                            }
                        }
                        return routeData;
                    }
                }
            }

            return null;
        }


        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.EndsWith(System.String)", Justification = @"okay")]
        private string NormalizeVirtualPath(RequestContext requestContext, string virtualPath) {
            string url = System.Web.UI.Util.GetUrlWithApplicationPath(requestContext.HttpContext, virtualPath);

            if (LowercaseUrls || AppendTrailingSlash) {
                int iqs = url.IndexOfAny(new char[] { '?', '#' });
                string urlWithoutQs;
                string qs;
                if (iqs >= 0) {
                    urlWithoutQs = url.Substring(0, iqs);
                    qs = url.Substring(iqs);
                }
                else {
                    urlWithoutQs = url;
                    qs = "";
                }

                // Don't lowercase the query string
                if (LowercaseUrls) {
                    urlWithoutQs = urlWithoutQs.ToLowerInvariant();
                }

                if (AppendTrailingSlash && !urlWithoutQs.EndsWith("/")) {
                    urlWithoutQs += "/";
                }

                url = urlWithoutQs + qs;
            }

            return url;
        }

        public VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values) {
            requestContext = GetRequestContext(requestContext);

            // Go through all the configured routes and find the first one that returns a match
            using (GetReadLock()) {
                foreach (RouteBase route in this) {
                    VirtualPathData vpd = route.GetVirtualPath(requestContext, values);
                    if (vpd != null) {
                        vpd.VirtualPath = NormalizeVirtualPath(requestContext, vpd.VirtualPath);
                        return vpd;
                    }
                }
            }

            return null;
        }

        public VirtualPathData GetVirtualPath(RequestContext requestContext, string name, RouteValueDictionary values) {
            requestContext = GetRequestContext(requestContext);

            if (!String.IsNullOrEmpty(name)) {
                RouteBase namedRoute;
                bool routeFound;
                using (GetReadLock()) {
                    routeFound = _namedMap.TryGetValue(name, out namedRoute);
                }
                if (routeFound) {
                    VirtualPathData vpd = namedRoute.GetVirtualPath(requestContext, values);
                    if (vpd != null) {
                        vpd.VirtualPath = NormalizeVirtualPath(requestContext, vpd.VirtualPath);
                        return vpd;
                    }
                    return null;
                }
                else {
                    throw new ArgumentException(
                        String.Format(
                            CultureInfo.CurrentUICulture,
                            SR.GetString(SR.RouteCollection_NameNotFound),
                            name),
                        "name");
                }
            }
            else {
                return GetVirtualPath(requestContext, values);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not worth a breaking change.")]
        public IDisposable GetWriteLock() {
            _rwLock.EnterWriteLock();
            return new WriteLockDisposable(_rwLock);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", 
            Justification = "This is not a regular URL as it may contain special routing characters.")]
        public void Ignore(string url) {
            Ignore(url, null /* constraints */);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", 
            Justification = "This is not a regular URL as it may contain special routing characters.")]
        public void Ignore(string url, object constraints) {
            if (url == null) {
                throw new ArgumentNullException("url");
            }

            IgnoreRouteInternal route = new IgnoreRouteInternal(url) {
                Constraints = new RouteValueDictionary(constraints)
            };

            Add(route);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void InsertItem(int index, RouteBase item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            if (Contains(item)) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.RouteCollection_DuplicateEntry)),
                    "item");
            }
            base.InsertItem(index, item);
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void RemoveItem(int index) {
            RemoveRouteName(index);
            base.RemoveItem(index);
        }

        private void RemoveRouteName(int index) {
            // Search for the specified route and clear out its name if we have one
            RouteBase route = this[index];
            foreach (KeyValuePair<string, RouteBase> namedRoute in _namedMap) {
                if (namedRoute.Value == route) {
                    _namedMap.Remove(namedRoute.Key);
                    break;
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override void SetItem(int index, RouteBase item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            if (Contains(item)) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        SR.GetString(SR.RouteCollection_DuplicateEntry)),
                    "item");
            }
            RemoveRouteName(index);
            base.SetItem(index, item);
        }

        private class ReadLockDisposable : IDisposable {

            private ReaderWriterLockSlim _rwLock;

            public ReadLockDisposable(ReaderWriterLockSlim rwLock) {
                _rwLock = rwLock;
            }

            [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly",
                Justification = "Type does not have a finalizer.")]
            void IDisposable.Dispose() {
                _rwLock.ExitReadLock();
            }
        }

        private class WriteLockDisposable : IDisposable {

            private ReaderWriterLockSlim _rwLock;

            public WriteLockDisposable(ReaderWriterLockSlim rwLock) {
                _rwLock = rwLock;
            }

            [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly",
                Justification = "Type does not have a finalizer.")]
            void IDisposable.Dispose() {
                _rwLock.ExitWriteLock();
            }
        }

        private sealed class IgnoreRouteInternal : Route {
            public IgnoreRouteInternal(string url)
                : base(url, new StopRoutingHandler()) {
            }

            public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary routeValues) {
                // Never match during route generation. This avoids the scenario where an IgnoreRoute with
                // fairly relaxed constraints ends up eagerly matching all generated URLs.
                return null;
            }
        }
    }
}
