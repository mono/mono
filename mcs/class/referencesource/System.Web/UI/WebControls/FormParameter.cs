//------------------------------------------------------------------------------
// <copyright file="FormParameter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;



    /// <devdoc>
    /// Represents a Parameter that gets its value from the application's form parameters.
    /// </devdoc>
    [
    DefaultProperty("FormField"),
    ]
    public class FormParameter : Parameter {

        /// <devdoc>
        /// Creates an instance of the FormParameter class.
        /// </devdoc>
        public FormParameter() {
        }


        /// <devdoc>
        /// Creates an instance of the FormParameter class with the specified parameter name and form field.
        /// </devdoc>
        public FormParameter(string name, string formField) : base(name) {
            FormField = formField;
        }


        /// <devdoc>
        /// Creates an instance of the FormParameter class with the specified parameter name, database type, and
        /// form field.
        /// </devdoc>
        public FormParameter(string name, DbType dbType, string formField)
            : base(name, dbType) {
            FormField = formField;
        }


        /// <devdoc>
        /// Creates an instance of the FormParameter class with the specified parameter name, type, and form field.
        /// </devdoc>
        public FormParameter(string name, TypeCode type, string formField) : base(name, type) {
            FormField = formField;
        }


        /// <devdoc>
        /// Used to clone a parameter.
        /// </devdoc>
        protected FormParameter(FormParameter original) : base(original) {
            FormField = original.FormField;
            ValidateInput = original.ValidateInput;
        }



        /// <devdoc>
        /// The name of the form parameter to get the value from.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Parameter"),
        WebSysDescription(SR.FormParameter_FormField),
        ]
        public string FormField {
            get {
                object o = ViewState["FormField"];
                if (o == null)
                    return String.Empty;
                return (string)o;
            }
            set {
                if (FormField != value) {
                    ViewState["FormField"] = value;
                    OnParameterChanged();
                }
            }
        }


        /// <devdoc>
        /// Creates a new FormParameter that is a copy of this FormParameter.
        /// </devdoc>
        protected override Parameter Clone() {
            return new FormParameter(this);
        }


        /// <devdoc>
        /// Returns the updated value of the parameter.
        /// </devdoc>
        protected internal override object Evaluate(HttpContext context, Control control) {
            if (context == null || context.Request == null) {
                return null;
            }

            NameValueCollection formCollection = ValidateInput ? context.Request.Form : context.Request.Unvalidated.Form;
            return formCollection[FormField];
        }

        /// <summary>
        /// Determines whether the parameter's value is being validated or not.
        /// </summary>
        [
        WebCategory("Behavior"),
        WebSysDescription(SR.Parameter_ValidateInput),
        DefaultValue(true)
        ]
        public bool ValidateInput {
            get {
                object o = ViewState["ValidateInput"];
                if (o == null)
                    return true;
                return (bool)o;
            }
            set {
                if (ValidateInput != value) {
                    ViewState["ValidateInput"] = value;
                    OnParameterChanged();
                }
            }
        }
    }
}

