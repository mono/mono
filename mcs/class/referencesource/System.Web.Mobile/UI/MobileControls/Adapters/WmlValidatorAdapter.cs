//------------------------------------------------------------------------------
// <copyright file="WmlValidatorAdapter.cs" company="Microsoft">
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
     * WmlValidatorAdapter provides the wml device functionality for
     * Validator controls.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\WmlValidatorAdapter.uex' path='docs/doc[@for="WmlValidatorAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlValidatorAdapter : WmlControlAdapter
    {
        /// <include file='doc\WmlValidatorAdapter.uex' path='docs/doc[@for="WmlValidatorAdapter.Control"]/*' />
        protected new BaseValidator Control
        {
            get
            {
                return (BaseValidator)base.Control;
            }
        }

        /// <include file='doc\WmlValidatorAdapter.uex' path='docs/doc[@for="WmlValidatorAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            writer.EnterStyle(Style);
            if (!Control.IsValid && Control.Display != ValidatorDisplay.None)
            {
                String text = Control.Text;
                if (String.IsNullOrEmpty(text))
                {
                    text = Control.ErrorMessage;
                }
                
                if (!String.IsNullOrEmpty(text))
                {
                    writer.RenderText(text, Control.BreakAfter);
                }
            }
            writer.ExitStyle(Style);
        }
    }
}
