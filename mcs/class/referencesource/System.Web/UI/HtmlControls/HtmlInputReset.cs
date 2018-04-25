//------------------------------------------------------------------------------
// <copyright file="HtmlInputReset.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HtmlInputReset.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls {

    // VSWhidbey 402612 and linked bugs
    // We had a design change to map <input type="reset"> to a specific Html
    // Input Control.  However, we have to provide backward compat. that would
    // not break older app (e.g. Everett generate HtmlInputButton class in code
    // behind.  So we need to create the new class inheriting from HtmlInputButton
    // but overriding the corresponding properties and event to suppress them in
    // designer (as they should not be in the first place, but it was not fixed
    // in V1 unfortunately)


    [DefaultEvent("")]
    [SupportsEventValidation]
    public class HtmlInputReset : HtmlInputButton {

        /*
         *  Creates an intrinsic Html INPUT type=reset control.
         */

        /// <devdoc>
        /// <para>Initializes a new instance of a <see cref='System.Web.UI.HtmlControls.HtmlInputReset'/> class using 
        ///    default values.</para>
        /// </devdoc>
        public HtmlInputReset() : base("reset") {
        }

        /*
         *  Creates an intrinsic Html INPUT type=reset control.
         */

        /// <devdoc>
        /// <para>Initializes a new instance of a <see cref='System.Web.UI.HtmlControls.HtmlInputReset'/> class using the 
        ///    specified string.</para>
        /// </devdoc>
        public HtmlInputReset(string type) : base(type) {
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool CausesValidation {
            get {
                return base.CausesValidation;
            }
            set {
                base.CausesValidation = value;
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override string ValidationGroup {
            get {
                return base.ValidationGroup;
            }
            set {
                base.ValidationGroup = value;
            }
        }

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public new event EventHandler ServerClick {
            add {
                base.ServerClick += value;
            }
            remove {
                base.ServerClick -= value;
            }
        }

        internal override void RenderAttributesInternal(HtmlTextWriter writer) {
            // We didn't generate any server event for reset button in older version
        }
    }
}
