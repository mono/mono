//------------------------------------------------------------------------------
// <copyright file="RegularExpressionValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using WebCntrls = System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Mobile RegularExpressionValidator class.
     * The RegularExpressionValidator provides validation using a regular
     * expression as the validation criteria.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\RegularExpressionValidator.uex' path='docs/doc[@for="RegularExpressionValidator"]/*' />
    [
        ToolboxData("<{0}:RegularExpressionValidator runat=\"server\" ErrorMessage=\"RegularExpressionValidator\"></{0}:RegularExpressionValidator>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign)
    ]    
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class RegularExpressionValidator : BaseValidator
    {
        private WebCntrls.RegularExpressionValidator _webRegularExpressionValidator;

        /// <include file='doc\RegularExpressionValidator.uex' path='docs/doc[@for="RegularExpressionValidator.CreateWebValidator"]/*' />
        protected override WebCntrls.BaseValidator CreateWebValidator()
        {
            _webRegularExpressionValidator = new WebCntrls.RegularExpressionValidator();
            return _webRegularExpressionValidator;
        }

        ////////////////////////////////////////////////////////////////////////
        // Mimic the properties exposed in the original RegularExpressionValidator.
        // The properties are got and set directly from the original RegularExpressionValidator.
        ////////////////////////////////////////////////////////////////////////

        /// <include file='doc\RegularExpressionValidator.uex' path='docs/doc[@for="RegularExpressionValidator.ValidationExpression"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            Editor("System.Web.UI.Design.WebControls.RegexTypeEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.RegularExpressionValidator_ValidationExpression)
        ]
        public String ValidationExpression
        {
            get
            {
                return _webRegularExpressionValidator.ValidationExpression;
            }
            set
            {
                _webRegularExpressionValidator.ValidationExpression = value;
            }
        }

        /// <include file='doc\RegularExpressionValidator.uex' path='docs/doc[@for="RegularExpressionValidator.EvaluateIsValid"]/*' />
        protected override bool EvaluateIsValid()
        {
            return EvaluateIsValidInternal();
        }
    }
}
