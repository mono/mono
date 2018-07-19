//------------------------------------------------------------------------------
// <copyright file="BaseValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Diagnostics;
using WebCntrls = System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Mobile BaseValidator class.
     * The BaseValidator class provides a core implementation common to all
     * specific validator controls.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator"]/*' />
    [
        DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
        DefaultProperty("ErrorMessage"),
        Designer(typeof(System.Web.UI.Design.MobileControls.BaseValidatorDesigner)),
        DesignerAdapter(typeof(System.Web.UI.Design.MobileControls.Adapters.DesignerValidatorAdapter)),
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public abstract class BaseValidator : TextControl, IValidator
    {
        private WebCntrls.BaseValidator _webBaseValidator;
        private bool _isValid = true;

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.BaseValidator"]/*' />
        protected BaseValidator()
        {
            _webBaseValidator = CreateWebValidator();
            if (_webBaseValidator == null)
            {
                // Create a default web base validator, mainly for storing
                // property values.
                _webBaseValidator = new DefaultWebValidator();
            }

            Controls.Add(_webBaseValidator);

            // Currently by default we render error message in a dynamic way.
            _webBaseValidator.Display = ValidatorDisplay.Dynamic;
        }

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.VisibleWeight"]/*' />
        public override int VisibleWeight
        {
            get
            {
                return 0;   // validators are not generally visible
            }
        }

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.CreateWebValidator"]/*' />
        /// <summary>
        /// <para>
        /// Virtual method for subclass to create its own aggregated
        /// web validator.  The web validator is for getting and
        /// setting of the common properties of web base validator.
        /// </para>
        /// </summary>
        protected virtual WebCntrls.BaseValidator CreateWebValidator()
        {
            return null;
        }

        protected override Style CreateStyle() {
            Style style = new Style();
            style.StyleReference = Constants.ErrorStyle;

            return style;
        }

        ////////////////////////////////////////////////////////////////////////
        // Mimic the properties exposed in the original BaseValidator.
        ////////////////////////////////////////////////////////////////////////

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.IsValid"]/*' />
        [
            Browsable(false),
            DefaultValue(true),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool IsValid
        {
            // Not getting and setting to the corresponding property in the
            // WebForm Base Validator is to differentiate the valid state for
            // Mobile Validator as Mobile Page can have multiple forms where the
            // Mobile Validator is invalid only when the same form is post back.
            //
            // Also, this property shouldn't be persisted in ViewState[] because
            // error message will show up when the form that contains the
            // validator is revisited again and the validator's state should
            // be reset to true as default.  Same implementation is done in
            // WebForm BaseValidator.
            // e.g. Form1 contains a Validator and a TextBox and a Command to
            // Form2 which contains a ValidationSummary.  After a user enters
            // an invalid value and click Command, ValidationSummary will
            // list the error message and provide a link back to Form1.
            // If IsValid property is persisted via ViewState[], Form1 will show
            // error message since IsValid is false and persisted, while the
            // expected behavior is the validator's state on Form1 is reset
            // to true so no error message will be shown.
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
            }
        }

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.ErrorMessage"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.BaseValidator_ErrorMessage)
        ]
        public String ErrorMessage
        {
            get
            {
                return _webBaseValidator.ErrorMessage;
            }
            set
            {
                _webBaseValidator.ErrorMessage = value;
            }
        }

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.ControlToValidate"]/*' />
        [
            Bindable(false),
            DefaultValue(""),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.BaseValidator_ControlToValidate),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.ValidatedMobileControlConverter))
        ]
        public String ControlToValidate
        {
            get
            {
                return _webBaseValidator.ControlToValidate;
            }
            set
            {
                _webBaseValidator.ControlToValidate = value;
            }
        }

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.Display"]/*' />
        [
            Bindable(true),
            DefaultValue(ValidatorDisplay.Dynamic),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.BaseValidator_Display)
        ]
        public ValidatorDisplay Display
        {
            get
            {
                return _webBaseValidator.Display;
            }
            set
            {
                _webBaseValidator.Display = value;
            }
        }

        // Designer needs to know the correct default value in order to persist it correctly.
        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.StyleReference"]/*' />
        [
            DefaultValue(Constants.ErrorStyle)
        ]
        public override String StyleReference
        {
            get
            {
                return base.StyleReference;
            }
            set
            {
                base.StyleReference = value;
            }
        }

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.OnInit"]/*' />
        protected override void OnInit(EventArgs e)
        {
            // Add itself to the Validator list so the Validate() function
            // will be called to validate control
            Page.Validators.Add(this);
            base.OnInit(e);
        }

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.OnPreRender"]/*' />
        protected override void OnPreRender(EventArgs e)
        {
            if(this.Form == MobilePage.ActiveForm)
            {
                bool isValid = ControlPropertiesValid();
                Debug.Assert(isValid,
                    "Exception should have been thrown instead of returning false!");
            }
            base.OnPreRender(e);
        }

        // Common code shared by other validators in the same assembly
        internal bool EvaluateIsValidInternal()
        {
            // At this point all validator's related info should have been
            // redirected to the aggregated Web Validator.  Simply apply the
            // validation logic in the Web Validator to determine if the
            // checked control is valid.

            String idBuffer;

            try
            {
                _webBaseValidator.Validate();
            }
            catch
            {
                // Swap IDs with aggregate validator so that is Validate() throws
                // a meaningful ID is included in the exception message.
                idBuffer = ID;
                ID = _webBaseValidator.ID;
                _webBaseValidator.ID = idBuffer;
                
                try
                {
                    _webBaseValidator.Validate();
                }
                finally
                {
                    idBuffer = ID;
                    ID = _webBaseValidator.ID;
                    _webBaseValidator.ID = idBuffer;
                }

                // If the exception does not repro with swapped ID, just re-throw it.
                throw;
            }

            return _webBaseValidator.IsValid;
        }

        // Subclass should provide its own logic for validation
        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.EvaluateIsValid"]/*' />
        protected abstract bool EvaluateIsValid();

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.Validate"]/*' />
        public void Validate()
        {
            if (!Visible)
            {
                IsValid = true;
                return;
            }

            // See if we are in an invisible container
            Control parent = Parent;
            while (parent != null)
            {
                if (!parent.Visible)
                {
                    IsValid = true;
                    return;
                }
                parent = parent.Parent;
            }

            IsValid = EvaluateIsValid();
        }

        /////////////////////////////////////////////////////////////////////
        // Helper functions adopted from WebForms base validator
        /////////////////////////////////////////////////////////////////////

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.ControlPropertiesValid"]/*' />
        protected virtual bool ControlPropertiesValid()
        {
            // Check for blank control to validate
            String controlToValidate = ControlToValidate;
            if (controlToValidate.Length == 0)
            {
                throw new ArgumentException(SR.GetString(
                    SR.BaseValidator_ControlToValidateBlank, ID));
            }

            // Check that the property points to a valid control.
            // Will throw an exception if not found
            CheckControlValidationProperty(controlToValidate, "ControlToValidate");
            return true;
        }

        /// <include file='doc\BaseValidator.uex' path='docs/doc[@for="BaseValidator.CheckControlValidationProperty"]/*' />
        protected void CheckControlValidationProperty(String name, String propertyName)
        {
            // Get the control using the relative name
            Control control = NamingContainer.FindControl(name);
            if (control == null)
            {
                throw new ArgumentException(SR.GetString(
                    SR.BaseValidator_ControlNotFound, name, propertyName, ID));
            }

            // Get its validation property
            PropertyDescriptor prop = WebCntrls.BaseValidator.GetValidationProperty(control);
            if (prop == null)
            {
                throw new ArgumentException(SR.GetString(
                    SR.BaseValidator_BadControlType, name, propertyName, ID));
            }
        }

        private class DefaultWebValidator : WebCntrls.BaseValidator
        {
            protected override bool EvaluateIsValid()
            {
                Debug.Assert(false, "Should never be called.");
                return true;
            }
        }
    }
}
