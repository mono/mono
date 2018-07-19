//------------------------------------------------------------------------------
// <copyright file="HtmlInputGenericControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.HtmlControls {
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    ///    <para>
    ///       The <see langword='HtmlInputGenericControl'/>
    ///       class defines the methods, properties, and events for the a generic input
    ///       control. This class allows programmatic access to the HTML5 &lt;input&gt;
    ///       elements on the server.
    ///    </para>
    /// </devdoc>
    [
    DefaultEvent("ServerChange"),
    ValidationProperty("Value"),
    ]
    public class HtmlInputGenericControl : HtmlInputControl, IPostBackDataHandler {
        private static readonly object EventServerChange = new object();

        /// <summary>
        /// Creates an intrinsic Html INPUT type=text control.
        /// </summary>
        public HtmlInputGenericControl() : base("text") {
        }

        /// <summary>
        /// Creates an intrinsic Html INPUT control based on its type.
        /// </summary>
        /// <param name="type">The type of the control</param>
        public HtmlInputGenericControl(string type)
            : base(type) {
        }

        [
        WebCategory("Action"),
        WebSysDescription(SR.HtmlInputText_ServerChange)
        ]
        public event EventHandler ServerChange {
            add {
                Events.AddHandler(EventServerChange, value);
            }
            remove {
                Events.RemoveHandler(EventServerChange, value);
            }
        }

        /// <summary>
        /// Method used to raise the OnServerChange event.
        /// </summary>
        /// <param name="e">Event</param>
        protected virtual void OnServerChange(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventServerChange];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (!Disabled && Page != null) {
                Page.RegisterEnabledControl(this);
            }
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            string current = Value;
            string inputString = postCollection.GetValues(postDataKey)[0];

            if (!current.Equals(inputString)) {
                ValidateEvent(postDataKey);

                Value = inputString;
                return true;
            }

            return false;
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = @"Events are being used correctly for RaisePostBackEvent and RaisePostDataChangedEvent.")]
        protected virtual void RaisePostDataChangedEvent() {
            OnServerChange(EventArgs.Empty);
        }
    }
}
