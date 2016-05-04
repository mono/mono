//------------------------------------------------------------------------------
// <copyright file="HiddenField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Collections.Specialized;


    /// <devdoc>
    /// Inserts a hidden field into the web form.
    /// </devdoc>
    [
    ControlValueProperty("Value"),
    DefaultEvent("ValueChanged"),
    DefaultProperty("Value"),
    Designer("System.Web.UI.Design.WebControls.HiddenFieldDesigner, " + AssemblyRef.SystemDesign),
    ParseChildren(true),
    PersistChildren(false),
    NonVisualControl(),
    SupportsEventValidation,
    ]
    public class HiddenField : Control, IPostBackDataHandler {

        private static readonly object EventValueChanged = new object();

        [
        DefaultValue(false),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool EnableTheming {
            get {
                return false;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.NoThemingSupport, this.GetType().Name));
            }
        }

        [
        DefaultValue(""),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override string SkinID {
            get {
                return String.Empty;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.NoThemingSupport, this.GetType().Name));
            }
        }


        /// <devdoc>
        /// Gets or sets the value of the hidden field.
        /// </devdoc>
        [
        Bindable(true),
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.HiddenField_Value)
        ]
        public virtual string Value {
            get {
                string s = (string)ViewState["Value"];
                return (s != null) ? s : String.Empty;
            }
            set {
                ViewState["Value"] = value;
            }
        }


        /// <devdoc>
        /// Raised when the value of the hidden field is changed on the client.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.HiddenField_OnValueChanged)
        ]
        public event EventHandler ValueChanged {
            add {
                Events.AddHandler(EventValueChanged, value);
            }
            remove {
                Events.RemoveHandler(EventValueChanged, value);
            }
        }


        protected override ControlCollection CreateControlCollection() {
            return new EmptyControlCollection(this);
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override void Focus() {
            throw new NotSupportedException(SR.GetString(SR.NoFocusSupport, this.GetType().Name));
        }


        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            ValidateEvent(UniqueID);

            string current = Value;
            string postData = postCollection[postDataKey];
            if (!current.Equals(postData, StringComparison.Ordinal)) {
                Value = postData;
                return true;
            }
            return false;
        }


        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            if (SaveValueViewState == false) {
                ViewState.SetItemDirty("Value", false);
            }
        }


        protected virtual void OnValueChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventValueChanged];
            if (handler != null) {
                handler(this, e);
            }
        }


        protected virtual void RaisePostDataChangedEvent() {
            OnValueChanged(EventArgs.Empty);
        }


        protected internal override void Render(HtmlTextWriter writer) {
            string uniqueID = UniqueID;

            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
                Page.ClientScript.RegisterForEventValidation(uniqueID);
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Type, "hidden");

            if (uniqueID != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }

            if (ID != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            }

            string s;
            s = Value;
            if (s.Length > 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, s);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        /// <devdoc>
        ///    Determines whether the Value must be stored in view state, to
        ///    optimize the size of the saved state.
        /// </devdoc>
        private bool SaveValueViewState {
            get {
                // Must be saved when
                // 1. There is a registered event handler for EventValueChanged
                // 2. Control is not visible, because the browser's post data will not include this control
                // 3. The instance is a derived instance, which might be overriding the OnValueChanged method

                if ((Events[EventValueChanged] != null) ||
                    (Visible == false) ||
                    (this.GetType() != typeof(HiddenField))) {
                    return true;
                }

                return false;
            }
        }

        #region Implementation of IPostBackDataHandler

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }


        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }
        #endregion
    }
}
