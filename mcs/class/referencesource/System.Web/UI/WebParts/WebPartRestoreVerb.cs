//------------------------------------------------------------------------------
// <copyright file="WebPartRestoreVerb.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    internal sealed class WebPartRestoreVerb : WebPartActionVerb {

        private string _defaultDescription;
        private string _defaultText;

        private string DefaultDescription {
            get {
                if (_defaultDescription == null) {
                    _defaultDescription = SR.GetString(SR.WebPartRestoreVerb_Description);
                }
                return _defaultDescription;
            }
        }

        private string DefaultText {
            get {
                if (_defaultText == null) {
                    _defaultText = SR.GetString(SR.WebPartRestoreVerb_Text);
                }
                return _defaultText;
            }
        }

        // Properties must look at viewstate directly instead of the property in the base class,
        // so we can distinguish between an unset property and a property set to String.Empty.
        [
        WebSysDefaultValue(SR.WebPartRestoreVerb_Description)
        ]
        public override string Description {
            get {
                object o = ViewState["Description"];
                return (o == null) ? DefaultDescription : (string)o;
            }
            set {
                ViewState["Description"] = value;
            }
        }

        [
        WebSysDefaultValue(SR.WebPartRestoreVerb_Text)
        ]
        public override string Text {
            get {
                object o = ViewState["Text"];
                return (o == null) ? DefaultText : (string)o;
            }
            set {
                ViewState["Text"] = value;
            }
        }
    }
}
