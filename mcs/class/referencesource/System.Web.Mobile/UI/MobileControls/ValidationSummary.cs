//------------------------------------------------------------------------------
// <copyright file="ValidationSummary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Mobile ValidationSummary class.
     * The ValidationSummary shows all the validation errors in a Form in a
     * summary view.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\ValidationSummary.uex' path='docs/doc[@for="ValidationSummary"]/*' />
    [
        DefaultProperty("FormToValidate"),
        Designer(typeof(System.Web.UI.Design.MobileControls.ValidationSummaryDesigner)),
        DesignerAdapter(typeof(System.Web.UI.Design.MobileControls.Adapters.DesignerValidationSummaryAdapter)),
        ToolboxData("<{0}:ValidationSummary runat=\"server\"></{0}:ValidationSummary>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ValidationSummary : MobileControl
    {
        private bool _callValidate = true;

        /// <include file='doc\ValidationSummary.uex' path='docs/doc[@for="ValidationSummary.ValidationSummary"]/*' />
        public ValidationSummary()
        {
            StyleReference = Constants.ErrorStyle;
        }

        ////////////////////////////////////////////////////////////////////////
        // Mimic some properties exposed in the original ValidatorSummary.
        ////////////////////////////////////////////////////////////////////////

        /// <include file='doc\ValidationSummary.uex' path='docs/doc[@for="ValidationSummary.HeaderText"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.ValidationSummary_HeaderText)
        ]
        public String HeaderText
        {
            get
            {
                String s = (String) ViewState["HeaderText"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["HeaderText"] = value;
            }
        }

        /// <include file='doc\ValidationSummary.uex' path='docs/doc[@for="ValidationSummary.FormToValidate"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.ValidationSummary_FormToValidate),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.FormConverter))
        ]
        public String FormToValidate
        {
            get
            {
                String s = (String) ViewState["FormToValidate"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["FormToValidate"] = value;
            }
        }

        /// <include file='doc\ValidationSummary.uex' path='docs/doc[@for="ValidationSummary.BackLabel"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.ValidationSummary_BackLabel)
        ]
        public String BackLabel
        {
            get
            {
                return ToString(ViewState["BackLabel"]);
            }
            set
            {
                ViewState["BackLabel"] = value;
            }
        }

        // Designer needs to know the correct default value in order to persist it correctly.
        /// <include file='doc\ValidationSummary.uex' path='docs/doc[@for="ValidationSummary.StyleReference"]/*' />
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

        /// <include file='doc\ValidationSummary.uex' path='docs/doc[@for="ValidationSummary.OnLoad"]/*' />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // There are cases that we don't want to call the Validate()
            // method on individual validators that the ValidationSummary is
            // targeting to.
            // This first case is when the page is hit at the first time.
            // And the second case is when FormToValidate is the same as the
            // form ValidationSummary is on and this form is a postback from
            // the same form.  In this case, the validators' validate()
            // method should have been called by MobilePage if needed.

            if (!MobilePage.IsPostBack ||
                String.Compare(Form.UniqueID, FormToValidate, StringComparison.OrdinalIgnoreCase) == 0)
            {
                _callValidate = false;
            }
        }

        private void GetErrorValidators_Helper(Control parent, ArrayList errorValidators)
        {
            foreach(Control control in parent.Controls)
            {
                BaseValidator baseVal = control as BaseValidator;
                if (baseVal != null && baseVal.ErrorMessage.Length != 0)
                {
                    if (_callValidate)
                    {
                        baseVal.Validate();
                    }

                    if (!baseVal.IsValid)
                    {
                        errorValidators.Add(baseVal);
                    }
                }
                GetErrorValidators_Helper(control, errorValidators);
            }
        }
        
        /// <include file='doc\ValidationSummary.uex' path='docs/doc[@for="ValidationSummary.GetErrorMessages"]/*' />
        public String[] GetErrorMessages()
        {
            String[] errorDescriptions = null;
            ArrayList errorValidators = new ArrayList();

            Form targetForm = ResolveFormReferenceNoThrow(FormToValidate);

            if (targetForm == null)
            {
                throw new ArgumentException(
                    SR.GetString(SR.ValidationSummary_InvalidFormToValidate,
                                 FormToValidate,
                                 ID));
            }
            // Recursively find all validators with error messages to display.
            GetErrorValidators_Helper(targetForm, errorValidators);

            int count = errorValidators.Count;
            
            if (count > 0)
            {
                // get the messages;
                errorDescriptions = new String[count];
                int iMessage = 0;
                foreach (BaseValidator val in errorValidators)
                {
                    Debug.Assert(val != null, "Null reference unexpected!");
                    Debug.Assert(val.ErrorMessage.Length != 0, "Programmatic error: error message here shouldn't be empty!");
                    errorDescriptions[iMessage] = String.Copy(val.ErrorMessage);
                    iMessage++;
                }
                Debug.Assert(count == iMessage, "Not all messages were found!");
            }

            return errorDescriptions;
        }
    }
}
