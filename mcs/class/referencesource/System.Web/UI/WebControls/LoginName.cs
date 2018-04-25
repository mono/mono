//------------------------------------------------------------------------------
// <copyright file="LoginName.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    /// Renders a Label containing the name of the current user, as defined by the FormatString property.
    /// Renders nothing if the current user is anonymous.
    /// </devdoc>
    [
    Bindable(false),
    Designer("System.Web.UI.Design.WebControls.LoginNameDesigner," + AssemblyRef.SystemDesign),
    DefaultProperty("FormatString")
    ]
    public class LoginName : WebControl {

        private const string _defaultFormatString = "{0}";


        /// <devdoc>
        /// The format specification.  {0} is replaced with the user name of the logged in user.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(_defaultFormatString),
        Localizable(true),
        WebSysDescription(SR.LoginName_FormatString)
        ]
        public virtual string FormatString {
            get {
                object obj = ViewState["FormatString"];
                return (obj == null) ? _defaultFormatString : (string) obj;
            }
            set {
                ViewState["FormatString"] = value;
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return RenderingCompatibility < VersionUtil.Framework40;
            }
        }

        internal string UserName {
            get {
                if (DesignMode) {
                    return SR.GetString(SR.LoginName_DesignModeUserName);
                }
                else {
                    return LoginUtil.GetUserName(this);
                }
            }
        }
        
        protected internal override void Render(HtmlTextWriter writer) {
            if (!String.IsNullOrEmpty(UserName)) {
                base.Render(writer);
            }
        }

        public override void RenderBeginTag(HtmlTextWriter writer) {
            // Needed for adapter case to prevent empty span tags
            if (!String.IsNullOrEmpty(UserName)) {
                base.RenderBeginTag(writer);
            }
        }

        public override void RenderEndTag(HtmlTextWriter writer) {
            // Needed for adapter case to prevent empty span tags
            if (!String.IsNullOrEmpty(UserName)) {
                base.RenderEndTag(writer);
            }
        }


        /// <devdoc>
        /// Styles would be rendered by the WebControl base class.
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            string userName = UserName;
            if (!String.IsNullOrEmpty(userName)) {
                // VSWhidbey 304890 HTMLEncode the username
                userName = HttpUtility.HtmlEncode(userName);
                string formatString = FormatString;
                if (formatString.Length == 0) {
                    writer.Write(userName);
                }
                else {
                    try {
                        writer.Write(String.Format(CultureInfo.CurrentCulture, formatString, userName));
                    }
                    catch (FormatException e) {
                        throw new FormatException(SR.GetString(SR.LoginName_InvalidFormatString), e);
                    }
                }
            }
        }
    }
}
