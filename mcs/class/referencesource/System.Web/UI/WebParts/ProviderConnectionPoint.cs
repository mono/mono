//------------------------------------------------------------------------------
// <copyright file="ProviderConnectionPoint.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;

    public class ProviderConnectionPoint : ConnectionPoint {
        // Used by WebPartManager to verify the custom ConnectionPoint type has
        // the correct constructor signature.
        internal static readonly Type[] ConstructorTypes;

        static ProviderConnectionPoint() {
            ConstructorInfo constructor = typeof(ProviderConnectionPoint).GetConstructors()[0];
            ConstructorTypes = WebPartUtil.GetTypesForConstructor(constructor);
        }

        public ProviderConnectionPoint(MethodInfo callbackMethod, Type interfaceType, Type controlType,
                                       string displayName, string id, bool allowsMultipleConnections) : base(
                                           callbackMethod, interfaceType, controlType, displayName, id, allowsMultipleConnections) {
        }

        /// <devdoc>
        /// The secondary interfaces for this connection point.  An exception will be thrown
        /// if primary interfaces are returned in this collection.
        /// </devdoc>
        public virtual ConnectionInterfaceCollection GetSecondaryInterfaces(Control control) {
            return ConnectionInterfaceCollection.Empty;
        }

        public virtual object GetObject(Control control) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }

            return CallbackMethod.Invoke(control, null);
        }
    }
}

