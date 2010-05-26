/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    public class DefaultControllerFactory : IControllerFactory {

        private IBuildManager _buildManager;
        private ControllerBuilder _controllerBuilder;
        private ControllerTypeCache _instanceControllerTypeCache;
        private static ControllerTypeCache _staticControllerTypeCache = new ControllerTypeCache();

        internal IBuildManager BuildManager {
            get {
                if (_buildManager == null) {
                    _buildManager = new BuildManagerWrapper();
                }
                return _buildManager;
            }
            set {
                _buildManager = value;
            }
        }

        internal ControllerBuilder ControllerBuilder {
            get {
                return _controllerBuilder ?? ControllerBuilder.Current;
            }
            set {
                _controllerBuilder = value;
            }
        }

        internal ControllerTypeCache ControllerTypeCache {
            get {
                return _instanceControllerTypeCache ?? _staticControllerTypeCache;
            }
            set {
                _instanceControllerTypeCache = value;
            }
        }

        internal static InvalidOperationException CreateAmbiguousControllerException(RouteBase route, string controllerName, ICollection<Type> matchingTypes) {
            // we need to generate an exception containing all the controller types
            StringBuilder typeList = new StringBuilder();
            foreach (Type matchedType in matchingTypes) {
                typeList.AppendLine();
                typeList.Append(matchedType.FullName);
            }

            string errorText;
            Route castRoute = route as Route;
            if (castRoute != null) {
                errorText = String.Format(CultureInfo.CurrentUICulture, MvcResources.DefaultControllerFactory_ControllerNameAmbiguous_WithRouteUrl,
                    controllerName, castRoute.Url, typeList);
            }
            else {
                errorText = String.Format(CultureInfo.CurrentUICulture, MvcResources.DefaultControllerFactory_ControllerNameAmbiguous_WithoutRouteUrl,
                    controllerName, typeList);
            }

            return new InvalidOperationException(errorText);
        }

        public virtual IController CreateController(RequestContext requestContext, string controllerName) {
            if (requestContext == null) {
                throw new ArgumentNullException("requestContext");
            }
            if (String.IsNullOrEmpty(controllerName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "controllerName");
            }
            Type controllerType = GetControllerType(requestContext, controllerName);
            IController controller = GetControllerInstance(requestContext, controllerType);
            return controller;
        }

        protected internal virtual IController GetControllerInstance(RequestContext requestContext, Type controllerType) {
            if (controllerType == null) {
                throw new HttpException(404,
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        MvcResources.DefaultControllerFactory_NoControllerFound,
                        requestContext.HttpContext.Request.Path));
            }
            if (!typeof(IController).IsAssignableFrom(controllerType)) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        MvcResources.DefaultControllerFactory_TypeDoesNotSubclassControllerBase,
                        controllerType),
                    "controllerType");
            }
            try {
                return (IController)Activator.CreateInstance(controllerType);
            }
            catch (Exception ex) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        MvcResources.DefaultControllerFactory_ErrorCreatingController,
                        controllerType),
                    ex);
            }
        }

        protected internal virtual Type GetControllerType(RequestContext requestContext, string controllerName) {
            if (String.IsNullOrEmpty(controllerName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "controllerName");
            }

            // first search in the current route's namespace collection
            object routeNamespacesObj;
            Type match;
            if (requestContext != null && requestContext.RouteData.DataTokens.TryGetValue("Namespaces", out routeNamespacesObj)) {
                IEnumerable<string> routeNamespaces = routeNamespacesObj as IEnumerable<string>;
                if (routeNamespaces != null && routeNamespaces.Any()) {
                    HashSet<string> nsHash = new HashSet<string>(routeNamespaces, StringComparer.OrdinalIgnoreCase);
                    match = GetControllerTypeWithinNamespaces(requestContext.RouteData.Route, controllerName, nsHash);

                    // the UseNamespaceFallback key might not exist, in which case its value is implicitly "true"
                    if (match != null || false.Equals(requestContext.RouteData.DataTokens["UseNamespaceFallback"])) {
                        // got a match or the route requested we stop looking
                        return match;
                    }
                }
            }

            // then search in the application's default namespace collection
            if (ControllerBuilder.DefaultNamespaces.Count > 0) {
                HashSet<string> nsDefaults = new HashSet<string>(ControllerBuilder.DefaultNamespaces, StringComparer.OrdinalIgnoreCase);
                match = GetControllerTypeWithinNamespaces(requestContext.RouteData.Route, controllerName, nsDefaults);
                if (match != null) {
                    return match;
                }
            }

            // if all else fails, search every namespace
            return GetControllerTypeWithinNamespaces(requestContext.RouteData.Route, controllerName, null /* namespaces */);
        }

        private Type GetControllerTypeWithinNamespaces(RouteBase route, string controllerName, HashSet<string> namespaces) {
            // Once the master list of controllers has been created we can quickly index into it
            ControllerTypeCache.EnsureInitialized(BuildManager);

            ICollection<Type> matchingTypes = ControllerTypeCache.GetControllerTypes(controllerName, namespaces);
            switch (matchingTypes.Count) {
                case 0:
                    // no matching types
                    return null;

                case 1:
                    // single matching type
                    return matchingTypes.First();

                default:
                    // multiple matching types
                    throw CreateAmbiguousControllerException(route, controllerName, matchingTypes);
            }
        }

        public virtual void ReleaseController(IController controller) {
            IDisposable disposable = controller as IDisposable;
            if (disposable != null) {
                disposable.Dispose();
            }
        }

    }
}
