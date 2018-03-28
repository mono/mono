//------------------------------------------------------------------------------
// <copyright file="RequiredFieldValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Web;
    using System.Web.Util;


    /// <devdoc>
    ///    <para> Checks if the value of
    ///       the associated input control is different from its initial value.</para>
    /// </devdoc>
    [
    ToolboxData("<{0}:RequiredFieldValidator runat=\"server\" ErrorMessage=\"RequiredFieldValidator\"></{0}:RequiredFieldValidator>")
    ]
    public class RequiredFieldValidator : BaseValidator {


        /// <devdoc>
        ///    <para>Gets or sets the initial value of the associated input control.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        WebSysDescription(SR.RequiredFieldValidator_InitialValue)
        ]                                         
        public string InitialValue {
            get { 
                object o = ViewState["InitialValue"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["InitialValue"] = value;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    AddAttributesToRender method
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);
            if (RenderUplevel) {
                string id = ClientID;
                HtmlTextWriter expandoAttributeWriter = (EnableLegacyRendering || IsUnobtrusive) ? writer : null;
                AddExpandoAttribute(expandoAttributeWriter, id, "evaluationfunction", "RequiredFieldValidatorEvaluateIsValid", false);
                AddExpandoAttribute(expandoAttributeWriter, id, "initialvalue", InitialValue);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    EvaluateIsValid method
        /// </devdoc>
        protected override bool EvaluateIsValid() {

            // Get the control value, return true if it is not found
            string controlValue = GetControlValidationValue(ControlToValidate);
            if (controlValue == null) {
                Debug.Fail("Should have been caught by PropertiesValid check");
                return true;
            }

            // See if the control has changed
            return(!controlValue.Trim().Equals(InitialValue.Trim()));
        }                
    }
}

