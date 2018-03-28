//------------------------------------------------------------------------------
// <copyright file="View.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ParseChildren(false)]
    [
    Designer("System.Web.UI.Design.WebControls.ViewDesigner, " + AssemblyRef.SystemDesign)
    ]

    [ToolboxData("<{0}:View runat=\"server\"></{0}:View>")]
    public class View : Control {

        private static readonly object _eventActivate = new object();
        private static readonly object _eventDeactivate = new object();
        private bool _active = false;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal bool Active {
            get {
                return _active;
            }
            set {
                _active = value;

                // VSWhidbey 297515 - Need to make it visible explicity so views can be added during Render
                base.Visible = true;
            }
        }

        /// <devdoc>
        ///    <para>Gets and sets a value indicating whether theme is enabled.</para>
        /// </devdoc>
        [
        Browsable(true)
        ]
        public override bool EnableTheming {
            get {
                return base.EnableTheming;
            }
            set {
                base.EnableTheming = value;
            }
        }


        /// <devdoc>
        ///    <para>Occurs when the view is deactivated.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.View_Activate)
        ]
        public event EventHandler Activate {
            add {
                Events.AddHandler(_eventActivate, value);
            }
            remove {
                Events.RemoveHandler(_eventActivate, value);
            }
        }


        /// <devdoc>
        ///    <para>Occurs when the view is deactivated.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.View_Deactivate)
        ]
        public event EventHandler Deactivate {
            add {
                Events.AddHandler(_eventDeactivate, value);
            }
            remove {
                Events.RemoveHandler(_eventDeactivate, value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value that indicates whether the view should be rendered on
        ///       the page.
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebCategory("Behavior"),
        WebSysDescription(SR.Control_Visible)
        ]
        public override bool Visible {
            get {
                if (Parent == null) {
                    return Active;
                }
                return Active && Parent.Visible;
            }
            set {
                if (DesignMode) {
                    return;
                }
                throw new InvalidOperationException(SR.GetString(SR.View_CannotSetVisible));
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='OnActivate '/>event.</para>
        /// </devdoc>
        protected internal virtual void OnActivate(EventArgs e) {
            EventHandler handler = (EventHandler)Events[_eventActivate];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='OnDeactivate '/>event.</para>
        /// </devdoc>
        protected internal virtual void OnDeactivate(EventArgs e) {
            EventHandler handler = (EventHandler)Events[_eventDeactivate];
            if (handler != null) {
                handler(this, e);
            }
        }
    }
}
