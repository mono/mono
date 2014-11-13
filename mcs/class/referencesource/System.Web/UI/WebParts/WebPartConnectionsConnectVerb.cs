//------------------------------------------------------------------------------
// <copyright file="WebPartConnectionsConnectVerb.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    internal sealed class WebPartConnectionsConnectVerb : WebPartActionVerb {

        // Properties must look at viewstate directly instead of the property in the base class,
        // so we can distinguish between an unset property and a property set to String.Empty.
        [
        WebSysDefaultValue(SR.WebPartConnectionsConnectVerb_Description)
        ]
        public override string Description {
            get {
                object o = ViewState["Description"];
                return (o == null) ? SR.GetString(SR.WebPartConnectionsConnectVerb_Description) : (string)o;
            }
            set {
                ViewState["Description"] = value;
            }
        }

        [
        WebSysDefaultValue(SR.WebPartConnectionsConnectVerb_Text)
        ]
        public override string Text {
            get {
                object o = ViewState["Text"];
                return (o == null) ? SR.GetString(SR.WebPartConnectionsConnectVerb_Text) : (string)o;
            }
            set {
                ViewState["Text"] = value;
            }
        }
    }
}
