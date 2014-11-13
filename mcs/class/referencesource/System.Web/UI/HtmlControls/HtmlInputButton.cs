//------------------------------------------------------------------------------
// <copyright file="HtmlInputButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HtmlInputButton.cs
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
///       The <see langword='HtmlInputButton'/> class defines the methods,
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
    public class HtmlInputButton : HtmlInputControl, IPostBackEventHandler {

        private static readonly object EventServerClick = new object();

        /*
         *  Creates an intrinsic Html INPUT type=button control.
         */

        /// <devdoc>
        /// <para>Initializes a new instance of a <see cref='System.Web.UI.HtmlControls.HtmlInputButton'/> class using 
        ///    default values.</para>
        /// </devdoc>
        public HtmlInputButton() : base("button") {
        }

        /*
         *  Creates an intrinsic Html INPUT type=button,submit,reset control.
         */

        /// <devdoc>
        /// <para>Initializes a new instance of a <see cref='System.Web.UI.HtmlControls.HtmlInputButton'/> class using the 
        ///    specified string.</para>
        /// </devdoc>
        public HtmlInputButton(string type) : base(type) {
        }


        /// <devdoc>
        ///    <para>Gets or sets whether pressing the button causes page validation to fire. This defaults to True so that when
        ///          using validation controls, the validation state of all controls are updated when the button is clicked, both
        ///          on the client and the server. Setting this to False is useful when defining a cancel or reset button on a page
        ///          that has validators.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        ]
        public virtual bool CausesValidation {
            get {
                object b = ViewState["CausesValidation"];
                return((b == null) ? true : (bool)b);
            }
            set {
                ViewState["CausesValidation"] = value;
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.PostBackControl_ValidationGroup)
        ]
        public virtual string ValidationGroup {
            get {
                string s = (string)ViewState["ValidationGroup"];
                return((s == null) ? String.Empty : s);
            }
            set {
                ViewState["ValidationGroup"] = value;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Occurs when an HTML Input Button control is clicked on the browser.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.HtmlControl_OnServerClick)
        ]
        public event EventHandler ServerClick {
            add {
                Events.AddHandler(EventServerClick, value);
            }
            remove {
                Events.RemoveHandler(EventServerClick, value);
            }
        }


        /// <internalonly/>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            if (Page != null && Events[EventServerClick] != null) {
                Page.RegisterPostBackScript();
            }
        }

        /*
         * Override to generate postback code for onclick.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderAttributes(HtmlTextWriter writer) {
            RenderAttributesInternal(writer);
            base.RenderAttributes(writer);  // this must come last because of the self-closing /
        }

        // VSWhidbey 80882: It needs to be overridden by HtmlInputSubmit
        internal virtual void RenderAttributesInternal(HtmlTextWriter writer) {
            bool submitsProgramatically = Events[EventServerClick] != null;
            if (Page != null) {
                if (submitsProgramatically) {
                    Util.WriteOnClickAttribute(
                            writer, this, false /* submitsAutomatically */, submitsProgramatically,
                            (CausesValidation && Page.GetValidators(ValidationGroup).Count > 0),
                            ValidationGroup);
                }
                else {
                    Page.ClientScript.RegisterForEventValidation(UniqueID);
                }
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ServerClick '/> event.</para>
        /// </devdoc>
        protected virtual void OnServerClick(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventServerClick];
            if (handler != null) handler(this, e);
        }

        /*
         * Method of IPostBackEventHandler interface to raise events on post back.
         * Button fires an OnServerClick event.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(UniqueID, eventArgument);

            if (CausesValidation) {
                Page.Validate(ValidationGroup);
            }
            OnServerClick(EventArgs.Empty);
        }
    }
}
