//------------------------------------------------------------------------------
// <copyright file="QueryStringParameter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;



    /// <devdoc>
    /// Represents a Parameter that gets its value from the application's QueryString parameters.
    /// </devdoc>
    [
    DefaultProperty("QueryStringField"),
    ]
    public class QueryStringParameter : Parameter {

        /// <devdoc>
        /// Creates an instance of the QueryStringParameter class.
        /// </devdoc>
        public QueryStringParameter() {
        }


        /// <devdoc>
        /// Creates an instance of the QueryStringParameter class with the specified parameter name and QueryString field.
        /// </devdoc>
        public QueryStringParameter(string name, string queryStringField) : base(name) {
            QueryStringField = queryStringField;
        }


        /// <devdoc>
        /// Creates an instance of the QueryStringParameter class with the specified parameter name, database type,
        /// and QueryString field.
        /// </devdoc>
        public QueryStringParameter(string name, DbType dbType, string queryStringField)
            : base(name, dbType) {
            QueryStringField = queryStringField;
        }


        /// <devdoc>
        /// Creates an instance of the QueryStringParameter class with the specified parameter name, type, and QueryString field.
        /// </devdoc>
        public QueryStringParameter(string name, TypeCode type, string queryStringField) : base(name, type) {
            QueryStringField = queryStringField;
        }


        /// <devdoc>
        /// Used to clone a parameter.
        /// </devdoc>
        protected QueryStringParameter(QueryStringParameter original) : base(original) {
            QueryStringField = original.QueryStringField;
            ValidateInput = original.ValidateInput;
        }



        /// <devdoc>
        /// The name of the QueryString parameter to get the value from.
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Parameter"),
        WebSysDescription(SR.QueryStringParameter_QueryStringField),
        ]
        public string QueryStringField {
            get {
                object o = ViewState["QueryStringField"];
                if (o == null)
                    return String.Empty;
                return (string)o;
            }
            set {
                if (QueryStringField != value) {
                    ViewState["QueryStringField"] = value;
                    OnParameterChanged();
                }
            }
        }


        /// <devdoc>
        /// Creates a new QueryStringParameter that is a copy of this QueryStringParameter.
        /// </devdoc>
        protected override Parameter Clone() {
            return new QueryStringParameter(this);
        }


        /// <devdoc>
        /// Returns the updated value of the parameter.
        /// </devdoc>
        protected internal override object Evaluate(HttpContext context, Control control) {
            if (context == null || context.Request == null) {
                return null;
            }

            NameValueCollection queryStringCollection = ValidateInput ? context.Request.QueryString : context.Request.Unvalidated.QueryString;
            return queryStringCollection[QueryStringField];
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

