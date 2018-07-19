//------------------------------------------------------------------------------
// <copyright file="CustomValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Web;
    using System.Web.Util;


    /// <devdoc>
    ///    <para> Allows custom code to perform
    ///       validation on the client and/or server.</para>
    /// </devdoc>
    [
    DefaultEvent("ServerValidate"),
    ToolboxData("<{0}:CustomValidator runat=\"server\" ErrorMessage=\"CustomValidator\"></{0}:CustomValidator>")
    ]
    public class CustomValidator : BaseValidator {

        private static readonly object EventServerValidate= new object();


        /// <devdoc>
        ///    <para>Gets and sets the custom client Javascript function used 
        ///       for validation.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        WebSysDescription(SR.CustomValidator_ClientValidationFunction)
        ]                                         
        public string ClientValidationFunction {
            get { 
                object o = ViewState["ClientValidationFunction"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["ClientValidationFunction"] = value;
            }
        }


        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(false),
        WebSysDescription(SR.CustomValidator_ValidateEmptyText),
        ]
        public bool ValidateEmptyText {
            get {
                object o = ViewState["ValidateEmptyText"];
                return((o == null) ? false : (bool)o);
            }
            set {
                ViewState["ValidateEmptyText"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Represents the method that will handle the 
        ///    <see langword='ServerValidate'/> event of a 
        ///    <see cref='System.Web.UI.WebControls.CustomValidator'/>.</para>
        /// </devdoc>
        [
        WebSysDescription(SR.CustomValidator_ServerValidate)
        ]                                         
        public event ServerValidateEventHandler ServerValidate {
            add {
                Events.AddHandler(EventServerValidate, value);
            }
            remove {
                Events.RemoveHandler(EventServerValidate, value);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Adds the properties of the <see cref='System.Web.UI.WebControls.CustomValidator'/> control to the 
        ///    output stream for rendering on the client.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);
            if (RenderUplevel) {
                string id = ClientID;
                HtmlTextWriter expandoAttributeWriter = (EnableLegacyRendering || IsUnobtrusive) ? writer : null;

                AddExpandoAttribute(expandoAttributeWriter, id, "evaluationfunction", "CustomValidatorEvaluateIsValid", false);
                if (ClientValidationFunction.Length > 0) {
                    AddExpandoAttribute(expandoAttributeWriter, id, "clientvalidationfunction", ClientValidationFunction);
                    if (ValidateEmptyText) {
                        AddExpandoAttribute(expandoAttributeWriter, id, "validateemptytext", "true", false);
                    }
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Checks the properties of the control for valid values.</para>
        /// </devdoc>
        protected override bool ControlPropertiesValid() {
            // Need to override the BaseValidator implementation, because for CustomValidator, it is fine
            // for the ControlToValidate to be blank.
            string controlToValidate = ControlToValidate;
            if (controlToValidate.Length > 0) {
                // Check that the property points to a valid control. Will throw and exception if not found
                CheckControlValidationProperty(controlToValidate, "ControlToValidate");
            }
            return true;
        }


        /// <internalonly/>
        /// <devdoc>
        ///    EvaluateIsValid method
        /// </devdoc>
        protected override bool EvaluateIsValid() {

            // If no control is specified, we always fire the event. If they have specified a control, we
            // only fire the event if the input is non-blank.
            string controlValue = String.Empty;
            string controlToValidate = ControlToValidate;
            if (controlToValidate.Length > 0) {
                controlValue = GetControlValidationValue(controlToValidate);
                Debug.Assert(controlValue != null, "Should have been caught be property check");
                // If the text is empty, we return true. Whitespace is ignored for coordination wiht
                // RequiredFieldValidator.
                if ((controlValue == null || controlValue.Trim().Length == 0) &&
                     !ValidateEmptyText) {
                    return true;
                }
            }

            return OnServerValidate(controlValue);
        }            


        /// <devdoc>
        ///    <para>Raises the 
        ///    <see langword='ServerValidate'/> event for the <see cref='System.Web.UI.WebControls.CustomValidator'/>.</para>
        /// </devdoc>
        protected virtual bool OnServerValidate(string value) {
            ServerValidateEventHandler handler = (ServerValidateEventHandler)Events[EventServerValidate];
            ServerValidateEventArgs args = new ServerValidateEventArgs(value, true);
            if (handler != null) {
                handler(this, args);
                return args.IsValid;
            }
            else {
                return true;
            }
        }        
    }
}
