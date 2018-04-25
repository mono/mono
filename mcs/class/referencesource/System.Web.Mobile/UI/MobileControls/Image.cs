//------------------------------------------------------------------------------
// <copyright file="Image.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.Design.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile Image class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\Image.uex' path='docs/doc[@for="Image"]/*' />
    [
        DefaultProperty("ImageUrl"),
        Designer(typeof(System.Web.UI.Design.MobileControls.ImageDesigner)),
        DesignerAdapter(typeof(System.Web.UI.Design.MobileControls.Adapters.DesignerImageAdapter)),
        ToolboxData("<{0}:Image runat=\"server\"></{0}:Image>"),
        ToolboxItem(typeof(System.Web.UI.Design.WebControlToolboxItem))
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class Image : MobileControl, IPostBackEventHandler
    {

        // used for linking between panels
        /// <internalonly/>
        protected void RaisePostBackEvent(String argument)
        {
            MobilePage.ActiveForm = MobilePage.GetForm(argument);
        }
        
        /// <include file='doc\Image.uex' path='docs/doc[@for="Image.AlternateText"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Image_AlternateText)
        ]
        public String AlternateText
        {
            get
            {
                return ToString(ViewState["AlternateText"]);
            }
            set
            {
                ViewState["AlternateText"] = value;
            }
        }

        /// <include file='doc\Image.uex' path='docs/doc[@for="Image.ImageUrl"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            Editor(typeof(System.Web.UI.Design.MobileControls.ImageUrlEditor),
                   typeof(UITypeEditor)),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Image_ImageUrl)
        ]
        public String ImageUrl
        {
            get
            {
                return ToString(ViewState["ImageUrl"]);
            }
            set
            {
                ViewState["ImageUrl"] = value;
            }
        }

        /// <include file='doc\Image.uex' path='docs/doc[@for="Image.NavigateUrl"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Navigation),
            MobileSysDescription(SR.Image_NavigateUrl),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.NavigateUrlConverter))
        ]
        public String NavigateUrl
        {
            get
            {
                return ToString(ViewState["NavigateUrl"]);
            }
            set
            {
                ViewState["NavigateUrl"] = value;
            }
        }

        /// <include file='doc\Image.uex' path='docs/doc[@for="Image.SoftkeyLabel"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.Image_SoftkeyLabel)
        ]
        public String SoftkeyLabel
        {
            get
            {
                String s = (String) ViewState["Softkeylabel"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["Softkeylabel"] = value;
            }
        }

        #region IPostBackEventHandler implementation
        void IPostBackEventHandler.RaisePostBackEvent(String eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion 
    }
}
