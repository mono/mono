//------------------------------------------------------------------------------
// <copyright file="ControlParameter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Web.UI.WebControls;



    /// <devdoc>
    /// Represents a Parameter that gets its value from a control's property.
    /// </devdoc>
    [
    DefaultProperty("ControlID"),
    ]
    public class ControlParameter : Parameter {


        /// <devdoc>
        /// Creates an instance of the ControlParameter class.
        /// </devdoc>
        public ControlParameter() {
        }


        /// <devdoc>
        /// Creates an instance of the ControlParameter class with the specified parameter name and control ID.
        /// </devdoc>
        public ControlParameter(string name, string controlID) : base(name) {
            ControlID = controlID;
        }


        /// <devdoc>
        /// Creates an instance of the ControlParameter class with the specified parameter name, control ID, and property name.
        /// </devdoc>
        public ControlParameter(string name, string controlID, string propertyName) : base(name) {
            ControlID = controlID;
            PropertyName = propertyName;
        }


        /// <devdoc>
        /// Creates an instance of the ControlParameter class with the specified parameter name, database type,
        /// control ID, and property name.
        /// </devdoc>
        public ControlParameter(string name, DbType dbType, string controlID, string propertyName)
            : base(name, dbType) {
            ControlID = controlID;
            PropertyName = propertyName;
        }


        /// <devdoc>
        /// Creates an instance of the ControlParameter class with the specified parameter name, type, control ID, and property name.
        /// </devdoc>
        public ControlParameter(string name, TypeCode type, string controlID, string propertyName) : base(name, type) {
            ControlID = controlID;
            PropertyName = propertyName;
        }


        /// <devdoc>
        /// Used to clone a parameter.
        /// </devdoc>
        protected ControlParameter(ControlParameter original) : base(original) {
            ControlID = original.ControlID;
            PropertyName = original.PropertyName;
        }



        /// <devdoc>
        /// The ID of the control to get the value from.
        /// </devdoc>
        [
        DefaultValue(""),
        IDReferenceProperty(),
        RefreshProperties(RefreshProperties.All),
        TypeConverter(typeof(ControlIDConverter)),
        WebCategory("Control"),
        WebSysDescription(SR.ControlParameter_ControlID),
        ]
        public string ControlID {
            get {
                object o = ViewState["ControlID"];
                if (o == null)
                    return String.Empty;
                return (string)o;
            }
            set {
                if (ControlID != value) {
                    ViewState["ControlID"] = value;
                    OnParameterChanged();
                }
            }
        }


        /// <devdoc>
        /// The name of the control's property to get the value from.
        /// If none is specified, the ControlValueProperty attribute of the control will be examined to determine the default property name.
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter(typeof(ControlPropertyNameConverter)),
        WebCategory("Control"),
        WebSysDescription(SR.ControlParameter_PropertyName),
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
        /// Creates a new ControlParameter that is a copy of this ControlParameter.
        /// </devdoc>
        protected override Parameter Clone() {
            return new ControlParameter(this);
        }


        /// <devdoc>
        /// Returns the updated value of the parameter.
        /// </devdoc>
        protected internal override object Evaluate(HttpContext context, Control control) {
            if (control == null) {
                return null;
            }

            string controlID = ControlID;
            string propertyName = PropertyName;

            if (controlID.Length == 0) {
                throw new ArgumentException(SR.GetString(SR.ControlParameter_ControlIDNotSpecified, Name));
            }

            Control foundControl = DataBoundControlHelper.FindControl(control, controlID);

            if (foundControl == null) {
                throw new InvalidOperationException(SR.GetString(SR.ControlParameter_CouldNotFindControl, controlID, Name));
            }

            ControlValuePropertyAttribute controlValueProp = (ControlValuePropertyAttribute)TypeDescriptor.GetAttributes(foundControl)[typeof(ControlValuePropertyAttribute)];

            // If no property name is specified, use the ControlValuePropertyAttribute to determine which property to use.
            if (propertyName.Length == 0) {
                if ((controlValueProp != null) && (!String.IsNullOrEmpty(controlValueProp.Name))) {
                    propertyName = controlValueProp.Name;
                }
                else {
                    throw new InvalidOperationException(SR.GetString(SR.ControlParameter_PropertyNameNotSpecified, controlID, Name));
                }
            }

            // Get the value of the property
            object value = DataBinder.Eval(foundControl, propertyName);

            // Convert the value to null if this is the default property and the value is the property's default value
            if (controlValueProp != null &&
                String.Equals(controlValueProp.Name, propertyName, StringComparison.OrdinalIgnoreCase) &&
                controlValueProp.DefaultValue != null &&
                controlValueProp.DefaultValue.Equals(value)) {
                return null;
            }
            return value;
        }
    }
}

