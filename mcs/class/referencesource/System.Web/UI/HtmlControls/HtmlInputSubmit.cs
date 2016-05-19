//------------------------------------------------------------------------------
// <copyright file="HtmlInputSubmit.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HtmlInputSubmit.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Globalization;
    using System.Security.Permissions;
    

/// <devdoc>
///    <para>
///       The <see langword='HtmlInputSubmit'/> class defines the methods,
///       properties, and events for the HTML Input Button control. This class allows
///       programmatic access to the HTML &lt;input type=
///       button&gt;, &lt;input type=
///       submit&gt;,and &lt;input
///       type=
///       reset&gt; elements on
///       the server.
///    </para>
/// </devdoc>
    [
    DefaultEvent("ServerClick"),
    SupportsEventValidation,
    ]
    public class HtmlInputSubmit : HtmlInputButton, IPostBackEventHandler {

        /*
         *  Creates an intrinsic Html INPUT type=submit control.
         */

        /// <devdoc>
        /// <para>Initializes a new instance of a <see cref='System.Web.UI.HtmlControls.HtmlInputSubmit'/> class using 
        ///    default values.</para>
        /// </devdoc>
        public HtmlInputSubmit() : base("submit") {
        }

        /*
         *  Creates an intrinsic Html INPUT type=button,submit,reset control.
         */

        /// <devdoc>
        /// <para>Initializes a new instance of a <see cref='System.Web.UI.HtmlControls.HtmlInputSubmit'/> class using the 
        ///    specified string.</para>
        /// </devdoc>
        public HtmlInputSubmit(string type) : base(type) {
        }

        internal override void RenderAttributesInternal(HtmlTextWriter writer) {
            if (Page != null) {
                Util.WriteOnClickAttribute(
                        writer, this, true /* submitsAutomatically */, false /* submitsProgramatically */,
                        (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0),
                        ValidationGroup);
            }
        }
    }
}
