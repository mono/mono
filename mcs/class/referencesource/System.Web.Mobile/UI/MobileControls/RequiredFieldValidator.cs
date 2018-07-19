//------------------------------------------------------------------------------
// <copyright file="RequiredFieldValidator.cs" company="Microsoft">
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
using WebCntrls = System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Mobile RequiredFieldValidator class.
     * The RequiredFieldValidator makes the input control it is associated with
     * a required field.  Validation fails if the value of the input control is
     * no different from its initial value.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\RequiredFieldValidator.uex' path='docs/doc[@for="RequiredFieldValidator"]/*' />
    [
        ToolboxData("<{0}:RequiredFieldValidator runat=\"server\" ErrorMessage=\"RequiredFieldValidator\"></{0}:RequiredFieldValidator>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign)
    ]    
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class RequiredFieldValidator : BaseValidator
    {
        private WebCntrls.RequiredFieldValidator _webRequiredFieldValidator;

        /// <include file='doc\RequiredFieldValidator.uex' path='docs/doc[@for="RequiredFieldValidator.CreateWebValidator"]/*' />
        protected override WebCntrls.BaseValidator CreateWebValidator()
        {
            _webRequiredFieldValidator = new WebCntrls.RequiredFieldValidator();
            return _webRequiredFieldValidator;
        }

        ////////////////////////////////////////////////////////////////////////
        // Mimic the properties exposed in the original RequiredFieldValidator.
        // The properties are got and set directly from the original RequiredFieldValidator.
        ////////////////////////////////////////////////////////////////////////

        /// <include file='doc\RequiredFieldValidator.uex' path='docs/doc[@for="RequiredFieldValidator.InitialValue"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.RequiredFieldValidator_InitialValue)
        ]
        public String InitialValue
        {
            get
            { 
                return _webRequiredFieldValidator.InitialValue;
            }
            set
            {
                _webRequiredFieldValidator.InitialValue = value;
            }
        }

        /// <include file='doc\RequiredFieldValidator.uex' path='docs/doc[@for="RequiredFieldValidator.EvaluateIsValid"]/*' />
        protected override bool EvaluateIsValid()
        {
            return EvaluateIsValidInternal();
        }
    }
}
