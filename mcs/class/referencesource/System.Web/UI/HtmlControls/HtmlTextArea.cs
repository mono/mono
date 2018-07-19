//------------------------------------------------------------------------------
// <copyright file="HtmlTextArea.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HtmlTextArea.cs
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
    using System.Globalization;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>Defines the methods, properties, and events for the
    ///    <see cref='System.Web.UI.HtmlControls.HtmlTextArea'/>
    ///    class that
    ///    allows programmatic access to the HTML &lt;textarea&gt;.</para>
    /// </devdoc>
    [
    DefaultEvent("ServerChange"),
    SupportsEventValidation,
    ValidationProperty("Value")
    ]
    public class HtmlTextArea : HtmlContainerControl, IPostBackDataHandler {

        private static readonly object EventServerChange = new object();

        /*
         *  Creates an intrinsic Html TEXTAREA control.
         */

        /// <devdoc>
        ///    Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlTextArea'/> class.
        /// </devdoc>
        public HtmlTextArea() : base("textarea") {
        }

        /*
         * The property for the number of columns to display.
         */

        /// <devdoc>
        ///    <para> Indicates the display width (in characters) of the
        ///       text area.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Cols {
            get {
                string s = Attributes["cols"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["cols"] = MapIntegerAttributeToString(value);
            }
        }

        /*
         * Name property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets the value of the HTML
        ///       Name attribute that will be rendered to the browser.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual string Name {
            get {
                return UniqueID;
                //string s = Attributes["name"];
                //return ((s != null) ? s : "");
            }
            set {
                //Attributes["name"] = MapStringAttributeToString(value);
            }
        }

        // Value that gets rendered for the Name attribute
        internal string RenderedNameAttribute {
            get {
                return Name;
                //string name = Name;
                //if (name.Length == 0)
                //    return UniqueID;

                //return name;
            }
        }

        /*
         * The property for the number of rows to display.
         */

        /// <devdoc>
        ///    <para>Gets or sets the display height (in rows) of the text area.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Rows {
            get {
                string s = Attributes["rows"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["rows"] = MapIntegerAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the content of the text area.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Value {
            get {
                return InnerText;
            }
            set {
                InnerText = value;
            }
        }


        /// <devdoc>
        /// <para>Occurs when the content of the <see langword='HtmlTextArea'/> control is changed upon server
        ///    postback.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.HtmlTextArea_OnServerChange)
        ]
        public event EventHandler ServerChange {
            add {
                Events.AddHandler(EventServerChange, value);
            }
            remove {
                Events.RemoveHandler(EventServerChange, value);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Overridden to only allow literal controls to be added as Text property.
        /// </devdoc>
        protected override void AddParsedSubObject(object obj) {
            if (obj is LiteralControl || obj is DataBoundLiteralControl)
                base.AddParsedSubObject(obj);
            else
                throw new HttpException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "HtmlTextArea", obj.GetType().Name.ToString(CultureInfo.InvariantCulture)));
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderAttributes(HtmlTextWriter writer) {
            if (Page != null) {
                Page.ClientScript.RegisterForEventValidation(RenderedNameAttribute);
            }

            writer.WriteAttribute("name", RenderedNameAttribute);
            Attributes.Remove("name");

            base.RenderAttributes(writer);
        }


        /// <devdoc>
        /// <para>Raised the <see langword='ServerChange'/>
        /// event.</para>
        /// </devdoc>
        protected virtual void OnServerChange(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventServerChange];
            if (handler != null) handler(this, e);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (!Disabled) {
                // if no change handler, no need to save posted property
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
         * TextArea process a newly posted value.
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

            if (current == null || !current.Equals(text)) {
                ValidateEvent(postDataKey);

                Value = text;
                return true;
            }

            return false;
        }

        /*
         * Method of IPostBackDataHandler interface which is invoked whenever posted data
         * for a control has changed.  TextArea fires an OnServerChange event.
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
