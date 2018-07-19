//------------------------------------------------------------------------------
// <copyright file="WebPartActionVerb.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;

    internal abstract class WebPartActionVerb : WebPartVerb {

        [
        Bindable(false),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool Checked {
            get {
                return false;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.WebPartActionVerb_CantSetChecked));
            }
        }

    }
}

