//------------------------------------------------------------------------------
// <copyright file="SessionParameter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Data;



    /// <devdoc>
    /// Represents a Parameter that gets its value from the application's session state.
    /// </devdoc>
    [
    DefaultProperty("SessionField"),
    ]
    public class SessionParameter : Parameter {


        /// <devdoc>
        /// Creates an instance of the SessionParameter class.
        /// </devdoc>
        public SessionParameter() {
        }


        /// <devdoc>
        /// Creates an instance of the SessionParameter class with the specified parameter name, and session field.
        /// </devdoc>
        public SessionParameter(string name, string sessionField) : base(name) {
            SessionField = sessionField;
        }


        /// <devdoc>
        /// Creates an instance of the SessionParameter class with the specified parameter name, database type, and
        /// session field.
        /// </devdoc>
        public SessionParameter(string name, DbType dbType, string sessionField)
            : base(name, dbType) {
            SessionField = sessionField;
        }


        /// <devdoc>
        /// Creates an instance of the SessionParameter class with the specified parameter name, type, and session field.
        /// </devdoc>
        public SessionParameter(string name, TypeCode type, string sessionField) : base(name, type) {
            SessionField = sessionField;
        }


        /// <devdoc>
        /// Used to clone a parameter.
        /// </devdoc>
        protected SessionParameter(SessionParameter original) : base(original) {
            SessionField = original.SessionField;
        }



        /// <devdoc>
        /// The name of the session variable to get the value from.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Parameter"),
        WebSysDescription(SR.SessionParameter_SessionField),
        ]
        public string SessionField {
            get {
                object o = ViewState["SessionField"];
                if (o == null)
                    return String.Empty;
                return (string)o;
            }
            set {
                if (SessionField != value) {
                    ViewState["SessionField"] = value;
                    OnParameterChanged();
                }
            }
        }


        /// <devdoc>
        /// Creates a new SessionParameter that is a copy of this SessionParameter.
        /// </devdoc>
        protected override Parameter Clone() {
            return new SessionParameter(this);
        }


        /// <devdoc>
        /// Returns the updated value of the parameter.
        /// </devdoc>
        protected internal override object Evaluate(HttpContext context, Control control) {
            if (context == null || context.Session == null) {
                return null;
            }
            return context.Session[SessionField];
        }
    }
}

