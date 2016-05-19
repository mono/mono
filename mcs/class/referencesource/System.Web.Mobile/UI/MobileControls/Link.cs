//------------------------------------------------------------------------------
// <copyright file="Link.cs" company="Microsoft">
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
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile Link class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\Link.uex' path='docs/doc[@for="Link"]/*' />
    [
        DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
        DefaultProperty("Text"),
        Designer(typeof(System.Web.UI.Design.MobileControls.LinkDesigner)),
        DesignerAdapter(typeof(System.Web.UI.Design.MobileControls.Adapters.DesignerLinkAdapter)),
        ToolboxData("<{0}:Link runat=server>Link</{0}:Link>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class Link : TextControl, IPostBackEventHandler
    {
        /// <include file='doc\Link.uex' path='docs/doc[@for="Link.NavigateUrl"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Navigation),
            MobileSysDescription(SR.Link_NavigateUrl),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.NavigateUrlConverter))
        ]
        public String NavigateUrl
        {
            get
            {
                String s = (String) ViewState["NavigateUrl"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["NavigateUrl"] = value;
            }
        }

        /// <include file='doc\Link.uex' path='docs/doc[@for="Link.SoftkeyLabel"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.Link_SoftkeyLabel)
        ]
        public String SoftkeyLabel
        {
            get
            {
                String s = (String) ViewState["SoftkeyLabel"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["SoftkeyLabel"] = value;
            }
        }

        // used for linking between panels
        /// <internalonly/>
        protected void RaisePostBackEvent(String argument)
        {
            MobilePage.ActiveForm = MobilePage.GetForm(argument);
        }

        /// <include file='doc\Link.uex' path='docs/doc[@for="Link.AddLinkedForms"]/*' />
        public override void AddLinkedForms(IList linkedForms)
        {
            String target = NavigateUrl;
            String prefix = Constants.FormIDPrefix;
            if (target.StartsWith(prefix, StringComparison.Ordinal))
            {
                String targetID = target.Substring(prefix.Length);
                Form form = ResolveFormReference(targetID);
                if (form != null && !form.HasActivateHandler())
                {
                    linkedForms.Add(form);
                }
            }
        }

        #region IPostBackEventHandler implementation
        void IPostBackEventHandler.RaisePostBackEvent(String eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion 
    }
}
