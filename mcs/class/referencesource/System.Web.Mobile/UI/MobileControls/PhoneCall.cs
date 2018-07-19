//------------------------------------------------------------------------------
// <copyright file="PhoneCall.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile PhoneCall class.
     * The PhoneCall control is for initiating a voice call on a cell phone.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\PhoneCall.uex' path='docs/doc[@for="PhoneCall"]/*' />
    [
        DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + AssemblyRef.SystemDesign),
        DefaultProperty("Text"),
        Designer(typeof(System.Web.UI.Design.MobileControls.PhoneCallDesigner)),
        DesignerAdapter(typeof(System.Web.UI.Design.MobileControls.Adapters.DesignerLabelAdapter)),
        ToolboxData("<{0}:PhoneCall runat=\"server\">PhoneCall</{0}:PhoneCall>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class PhoneCall : TextControl, IPostBackEventHandler
    {
        /// <include file='doc\PhoneCall.uex' path='docs/doc[@for="PhoneCall.PhoneNumber"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Navigation),
            MobileSysDescription(SR.PhoneCall_PhoneNumber)
        ]
        public String PhoneNumber
        {
            get
            {
                String s = (String) ViewState["PhoneNumber"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                const String PhoneNumberFormat = "[\\+#]?[ \\d\\(\\)\\.-]*\\d[ \\d\\(\\)\\.-]*";

                // Empty string is the default value, no need to check
                // (same as null)
                if (!String.IsNullOrEmpty(value)) {
                    // phone number format checking using RegularExpression
                    Match match = Regex.Match(value, PhoneNumberFormat);

                    // we are looking for an exact match, not just a search hit
                    if (!match.Success || match.Index != 0 ||
                        match.Length != value.Length)
                    {
                        throw new ArgumentException(
                            SR.GetString(SR.PhoneCall_InvalidPhoneNumberFormat,
                                         "PhoneNumber", value));
                    }
                }

                ViewState["PhoneNumber"] = value;
            }
        }

        /// <include file='doc\PhoneCall.uex' path='docs/doc[@for="PhoneCall.AlternateFormat"]/*' />
        [
            Bindable(true),
            DefaultValue("{0} {1}"),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.PhoneCall_AlternateFormat)
        ]
        public String AlternateFormat
        {
            get
            {
                String s = (String) ViewState["AlternateFormat"];
                return((s != null) ? s : "{0} {1}");
            }
            set
            {
                ViewState["AlternateFormat"] = value;
            }
        }

        /// <include file='doc\PhoneCall.uex' path='docs/doc[@for="PhoneCall.AlternateUrl"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Navigation),
            MobileSysDescription(SR.PhoneCall_AlternateUrl),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.NavigateUrlConverter)),
        ]
        public String AlternateUrl
        {
            get
            {
                String s = (String) ViewState["AlternateUrl"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["AlternateUrl"] = value;
            }
        }

        /// <include file='doc\PhoneCall.uex' path='docs/doc[@for="PhoneCall.SoftkeyLabel"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.PhoneCall_SoftkeyLabel)
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

        /// <internalonly/>
        protected void RaisePostBackEvent(String argument)
        {
            MobilePage.ActiveForm = MobilePage.GetForm(argument);
        }

        /// <include file='doc\PhoneCall.uex' path='docs/doc[@for="PhoneCall.OnPreRender"]/*' />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            Debug.Assert(PhoneNumber != null);
            if (PhoneNumber.Length == 0)
            {
                throw new ArgumentException(
                            SR.GetString(SR.PhoneCall_EmptyPhoneNumber, ID));
            }
        }

        /// <include file='doc\PhoneCall.uex' path='docs/doc[@for="PhoneCall.AddLinkedForms"]/*' />
        public override void AddLinkedForms(IList linkedForms)
        {
            String target = AlternateUrl;
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
