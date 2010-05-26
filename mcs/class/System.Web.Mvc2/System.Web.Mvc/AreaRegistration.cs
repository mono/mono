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
    using System.Collections.Generic;
    using System.Web.Routing;

    public abstract class AreaRegistration {

        private const string _typeCacheName = "MVC-AreaRegistrationTypeCache.xml";

        public abstract string AreaName {
            get;
        }

        internal void CreateContextAndRegister(RouteCollection routes, object state) {
            AreaRegistrationContext context = new AreaRegistrationContext(AreaName, routes, state);

            string thisNamespace = GetType().Namespace;
            if (thisNamespace != null) {
                context.Namespaces.Add(thisNamespace + ".*");
            }

            RegisterArea(context);
        }

        private static bool IsAreaRegistrationType(Type type) {
            return
                typeof(AreaRegistration).IsAssignableFrom(type) &&
                type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static void RegisterAllAreas() {
            RegisterAllAreas(null);
        }

        public static void RegisterAllAreas(object state) {
            RegisterAllAreas(RouteTable.Routes, new BuildManagerWrapper(), state);
        }

        internal static void RegisterAllAreas(RouteCollection routes, IBuildManager buildManager, object state) {
            List<Type> areaRegistrationTypes = TypeCacheUtil.GetFilteredTypesFromAssemblies(_typeCacheName, IsAreaRegistrationType, buildManager);
            foreach (Type areaRegistrationType in areaRegistrationTypes) {
                AreaRegistration registration = (AreaRegistration)Activator.CreateInstance(areaRegistrationType);
                registration.CreateContextAndRegister(routes, state);
            }
        }

        public abstract void RegisterArea(AreaRegistrationContext context);

    }
}
