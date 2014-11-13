//------------------------------------------------------------------------------
// <copyright file="HtmlValidatorAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.MobileControls;
using System.Web.UI.WebControls;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * HtmlValidatorAdapter provides the html device functionality for
     * Validator controls.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlValidatorAdapter.uex' path='docs/doc[@for="HtmlValidatorAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlValidatorAdapter : HtmlControlAdapter
    {
        /// <include file='doc\HtmlValidatorAdapter.uex' path='docs/doc[@for="HtmlValidatorAdapter.Control"]/*' />
        protected new BaseValidator Control
        {
            get
            {
                return (BaseValidator)base.Control;
            }
        }

        /// <include file='doc\HtmlValidatorAdapter.uex' path='docs/doc[@for="HtmlValidatorAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            if (!Control.IsValid && Control.Display != ValidatorDisplay.None)
            {
                writer.EnterStyle(Style);
                if (!String.IsNullOrEmpty(Control.Text))
                {
                    writer.WriteText(Control.Text, true);
                }
                else if (!String.IsNullOrEmpty(Control.ErrorMessage))
                {
                    writer.WriteText(Control.ErrorMessage, true);
                }
                writer.ExitStyle(Style, Control.BreakAfter);
            }
        }
    }
}
