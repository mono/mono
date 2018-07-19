//------------------------------------------------------------------------------
// <copyright file="RouteParameter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Security.Permissions;
    using System.Web.Routing;

    /// <devdoc>
    /// Represents a Parameter that gets its value from the application's route data.
    /// </devdoc>
    [
    DefaultProperty("RouteKey"),
    ]
    public class RouteParameter : Parameter {

        /// <devdoc>
        /// Creates an instance of the RouteParameter class.
        /// </devdoc>
        public RouteParameter() {
        }

        /// <devdoc>
        /// Creates an instance of the RouteParameter class with the specified parameter name and request field.
        /// </devdoc>
        public RouteParameter(string name, string routeKey) : base(name) {
            RouteKey = routeKey;
        }

        /// <devdoc>
        /// Creates an instance of the routeParameter class with the specified parameter name, database type, and
        /// request field.
        /// </devdoc>
        public RouteParameter(string name, DbType dbType, string routeKey)
            : base(name, dbType) {
            RouteKey = routeKey;
        }


        /// <devdoc>
        /// Creates an instance of the RouteParameter class with the specified parameter name, type, and request field.
        /// </devdoc>
        public RouteParameter(string name, TypeCode type, string routeKey) : base(name, type) {
            RouteKey = routeKey;
        }

        /// <devdoc>
        /// Used to clone a parameter.
        /// </devdoc>
        protected RouteParameter(RouteParameter original) : base(original) {
            RouteKey = original.RouteKey;
        }

        /// <devdoc>
        /// The name of the route value to get the value from.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Parameter"),
        WebSysDescription(SR.RouteParameter_RouteKey),
        ]
        public string RouteKey {
            get {
                object o = ViewState["RouteKey"];
                if (o == null)
                    return String.Empty;
                return (string)o;
            }
            set {
                if (RouteKey != value) {
                    ViewState["RouteKey"] = value;
                    OnParameterChanged();
                }
            }
        }

        /// <devdoc>
        /// Creates a new RouteParameter that is a copy of this RouteParameter.
        /// </devdoc>
        protected override Parameter Clone() {
            return new RouteParameter(this);
        }

        /// <devdoc>
        /// Returns the updated value of the parameter.
        /// </devdoc>
        protected internal override object Evaluate(HttpContext context, Control control) {
            if (context == null || context.Request == null || control == null) {
                return null;
            }
            RouteData routeData = control.Page.RouteData;
            if (routeData == null) {
                return null;
            }
            return routeData.Values[RouteKey];
        }
    }
}

