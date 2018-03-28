//------------------------------------------------------------------------------
// <copyright file="ProfileParameter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Data;



    /// <devdoc>
    /// Represents a Parameter that gets its value from the user's profile data.
    /// </devdoc>
    [
    DefaultProperty("PropertyName"),
    ]
    public class ProfileParameter : Parameter {


        /// <devdoc>
        /// Creates an instance of the ProfileParameter class.
        /// </devdoc>
        public ProfileParameter() {
        }


        /// <devdoc>
        /// Creates an instance of the ProfileParameter class with the specified parameter name and profile property.
        /// </devdoc>
        public ProfileParameter(string name, string propertyName) : base(name) {
            PropertyName = propertyName;
        }


        /// <devdoc>
        /// Creates an instance of the ProfileParameter class with the specified parameter name, type, and profile property.
        /// </devdoc>
        public ProfileParameter(string name, TypeCode type, string propertyName) : base(name, type) {
            PropertyName = propertyName;
        }


        /// <devdoc>
        /// Creates an instance of the ProfileParameter class with the specified parameter name, database type, and
        /// profile property.
        /// </devdoc>
        public ProfileParameter(string name, DbType dbType, string propertyName)
            : base(name, dbType) {
            PropertyName = propertyName;
        }


        /// <devdoc>
        /// Used to clone a parameter.
        /// </devdoc>
        protected ProfileParameter(ProfileParameter original) : base(original) {
            PropertyName = original.PropertyName;
        }



        /// <devdoc>
        /// The name of the Profile property to get the value from.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Parameter"),
        WebSysDescription(SR.ProfileParameter_PropertyName),
        ]
        public string PropertyName {
            get {
                object o = ViewState["PropertyName"];
                if (o == null)
                    return String.Empty;
                return (string)o;
            }
            set {
                if (PropertyName != value) {
                    ViewState["PropertyName"] = value;
                    OnParameterChanged();
                }
            }
        }


        /// <devdoc>
        /// Creates a new ProfileParameter that is a copy of this ProfileParameter.
        /// </devdoc>
        protected override Parameter Clone() {
            return new ProfileParameter(this);
        }


        /// <devdoc>
        /// Returns the updated value of the parameter.
        /// </devdoc>
        protected internal override object Evaluate(HttpContext context, Control control) {
            if (context == null || context.Profile == null) {
                return null;
            }
            return DataBinder.Eval(context.Profile, PropertyName);
        }
    }
}

