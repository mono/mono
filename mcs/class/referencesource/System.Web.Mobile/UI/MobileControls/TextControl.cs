//------------------------------------------------------------------------------
// <copyright file="TextControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile TextControl class.
     * All controls which contain embedded text extend from this control.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\TextControl.uex' path='docs/doc[@for="TextControl"]/*' />
    [
        ToolboxItem(false)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public abstract class TextControl : MobileControl
    {
        /// <include file='doc\TextControl.uex' path='docs/doc[@for="TextControl.Text"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.TextControl_Text),
            PersistenceMode(PersistenceMode.EncodedInnerDefaultProperty)
        ]
        public String Text
        {
            get
            {
                return InnerText;
            }

            set
            {
                InnerText = value;
            }
        }
    }
}
