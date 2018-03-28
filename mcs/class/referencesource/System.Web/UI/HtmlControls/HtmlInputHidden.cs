//------------------------------------------------------------------------------
// <copyright file="HtmlInputHidden.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HtmlInputHidden.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {
    using System.ComponentModel;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.UI;
    using System.Security.Permissions;


/// <devdoc>
///    <para>
///       The <see langword='HtmlInputHidden'/> class defines the methods, properties,
///       and events of the HtmlInputHidden control. This class allows programmatic access
///       to the HTML &lt;input type=hidden&gt; element on the server.
///    </para>
/// </devdoc>
    [
    DefaultEvent("ServerChange"),
    SupportsEventValidation,
    ]
    public class HtmlInputHidden : HtmlInputControl, IPostBackDataHandler {

        private static readonly object EventServerChange = new object();

        /*
         * Creates an intrinsic Html INPUT type=hidden control.
         */

        public HtmlInputHidden() : base("hidden") {
        }


        /// <devdoc>
        ///    <para>
        ///       Occurs when the <see langword='HtmlInputHidden'/> control
        ///       is changed on the server.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.HtmlInputHidden_OnServerChange)
        ]
        public event EventHandler ServerChange {
            add {
                Events.AddHandler(EventServerChange, value);
            }
            remove {
                Events.RemoveHandler(EventServerChange, value);
            }
        }

        /*
         * Method used to raise the OnServerChange event.
         */

        /// <devdoc>
        ///    <para>
        ///       Raised on the server when the <see langword='HtmlInputHidden'/> control
        ///       changes between postback requests.
        ///    </para>
        /// </devdoc>
        protected virtual void OnServerChange(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventServerChange];
            if (handler != null) handler(this, e);
        }

        /*
         *
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // if no change handler, no need to save posted property
            if (!Disabled) {
                if (Events[EventServerChange] == null) {
                    ViewState.SetItemDirty("value",false);
                }

                if (Page != null) {
                    Page.RegisterEnabledControl(this);
                }
            }
        }

        /*
         * Method of IPostBackDataHandler interface to process posted data.
         * InputText process a newly posted value.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            string current = Value;
            string text = postCollection.GetValues(postDataKey)[0];

            if (!current.Equals(text)) {
                ValidateEvent(postDataKey);

                Value = text;
                return true;
            }

            return false;
        }

        protected override void RenderAttributes(HtmlTextWriter writer) {
            base.RenderAttributes(writer);

            if (Page != null) {
                Page.ClientScript.RegisterForEventValidation(RenderedNameAttribute);
            }
        }

        /*
         * Method of IPostBackDataHandler interface which is invoked whenever posted data
         * for a control has changed.  TextBox fires an OnTextChanged event.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void RaisePostDataChangedEvent() {
            OnServerChange(EventArgs.Empty);
        }
    }
}
