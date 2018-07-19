//------------------------------------------------------------------------------
// <copyright file="HtmlInputPassword.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HtmlInputPassword.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {
    using System.ComponentModel;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Security.Permissions;


/// <devdoc>
///    <para>
///       The <see langword='HtmlInputPassword'/>
///       class defines the methods, properties, and events for the HtmlInputPassword server
///       control. This class allows programmatic access to the HTML &lt;input type=
///       text&gt;
///       and &lt;input type=
///       password&gt; elements on the server.
///    </para>
/// </devdoc>
    [
    DefaultEvent("ServerChange"),
    ValidationProperty("Value"),
    SupportsEventValidation,
    ]
    public class HtmlInputPassword : HtmlInputText, IPostBackDataHandler {

        private static readonly object EventServerChange = new object();

        /*
         * Creates an intrinsic Html INPUT type=password control.
         */

        public HtmlInputPassword() : base("password") {
        }

        protected override void RenderAttributes(HtmlTextWriter writer) {
            // Remove value from viewstate for input type=password
            ViewState.Remove("value");

            base.RenderAttributes(writer);
        }
    }
}
