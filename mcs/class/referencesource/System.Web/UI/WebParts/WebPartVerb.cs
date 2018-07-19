//------------------------------------------------------------------------------
// <copyright file="WebPartVerb.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    [
    TypeConverter(typeof(EmptyStringExpandableObjectConverter))
    ]
    public class WebPartVerb : IStateManager {
        private bool _isTrackingViewState;
        private StateBag _viewState;

        private bool _visible = true;

        private string _id;
        private string _clientClickHandler;
        private WebPartEventHandler _serverClickHandler;

        private string _eventArgument;

        // PERF: Don't create the EventArgument string until needed, when the verb is actually rendered.
        private string _eventArgumentPrefix;

        // Used when creating default verbs in WebPartZoneBase, CatalogZoneBase, EditorZoneBase. For these
        // verbs, the clientClickHandler and serverClickHandler are null. The zone has references
        // to these verbs and can render the verb and handle events by knowing which verb it is
        // rendering.
        internal WebPartVerb() {
        }

        private WebPartVerb(string id) {
            if (String.IsNullOrEmpty(id)) {
                throw ExceptionUtil.ParameterNullOrEmpty("id");
            }

            _id = id;
        }

        public WebPartVerb(string id, WebPartEventHandler serverClickHandler) : this(id) {
            if (serverClickHandler == null) {
                throw new ArgumentNullException("serverClickHandler");
            }
            _serverClickHandler = serverClickHandler;
        }

        public WebPartVerb(string id, string clientClickHandler) : this(id) {
            if (String.IsNullOrEmpty(clientClickHandler)) {
                throw new ArgumentNullException("clientClickHandler");
            }
            _clientClickHandler = clientClickHandler;
        }

        public WebPartVerb(string id, WebPartEventHandler serverClickHandler, string clientClickHandler) : this(id) {
            if (serverClickHandler == null) {
                throw new ArgumentNullException("serverClickHandler");
            }
            if (String.IsNullOrEmpty(clientClickHandler)) {
                throw new ArgumentNullException("clientClickHandler");
            }
            _serverClickHandler = serverClickHandler;
            _clientClickHandler = clientClickHandler;
        }

        [
        DefaultValue(false),
        NotifyParentProperty(true),
        Themeable(false),
        WebSysDescription(SR.WebPartVerb_Checked),
        ]
        public virtual bool Checked {
            get {
                object b = ViewState["Checked"];
                return (b != null) ? (bool)b : false  ;
            }
            set {
                ViewState["Checked"] = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string ClientClickHandler {
            get {
                return (_clientClickHandler == null) ? String.Empty: _clientClickHandler;
            }
        }

        [
        Localizable(true),
        NotifyParentProperty(true),
        // Must use WebSysDefaultValue instead of DefaultValue, since it is overridden in extending classes
        WebSysDefaultValue(""),
        WebSysDescription(SR.WebPartVerb_Description),
        ]
        public virtual string Description {
            get {
                object o = ViewState["Description"];
                return (o == null) ? String.Empty : (string)o;
            }
            set {
                ViewState["Description"] = value;
            }
        }

        [
        DefaultValue(true),
        NotifyParentProperty(true),
        Themeable(false),
        WebSysDescription(SR.WebPartVerb_Enabled),
        ]
        public virtual bool Enabled {
            get {
                object b = ViewState["Enabled"];
                return (b != null) ? (bool)b : true;
            }
            set {
                ViewState["Enabled"] = value;
            }
        }

        /// <devdoc>
        /// Used to assign an event argument to the verb that will be rendered later.
        /// </devdoc>
        internal string EventArgument {
            get {
                return (_eventArgument != null) ? _eventArgument : String.Empty;
            }
            set {
                _eventArgument = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string ID {
            get {
                return (_id != null) ? _id : String.Empty;
            }
        }

        [
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        NotifyParentProperty(true),
        UrlProperty(),
        WebSysDescription(SR.WebPartVerb_ImageUrl),
        ]
        public virtual string ImageUrl {
            get {
                object o = ViewState["ImageUrl"];
                return (o == null) ? String.Empty : (string)o;
            }
            set {
                ViewState["ImageUrl"] = value;
            }
        }

        protected virtual bool IsTrackingViewState {
            get {
                return _isTrackingViewState;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public WebPartEventHandler ServerClickHandler {
            get {
                return _serverClickHandler;
            }
        }

        /// <devdoc>
        /// The text to be displayed for the webPartVerb.
        /// </devdoc>
        [
        Localizable(true),
        NotifyParentProperty(true),
        // Must use WebSysDefaultValue instead of DefaultValue, since it is overridden in extending classes
        WebSysDefaultValue(""),
        WebSysDescription(SR.WebPartVerb_Text),
        ]
        public virtual string Text {
            get {
                object o = ViewState["Text"];
                return (o == null) ? String.Empty : (string)o;
            }
            set {
                ViewState["Text"] = value;
            }
        }

        [
        DefaultValue(true),
        NotifyParentProperty(true),
        Themeable(false),
        WebSysDescription(SR.WebPartVerb_Visible),
        ]
        public virtual bool Visible {
            get {
                return _visible;
            }
            set {
                _visible = value;
                ViewState["Visible"] = value;
            }
        }

        protected StateBag ViewState {
            get {
                if (_viewState == null) {
                    _viewState = new StateBag(false);
                    if (_isTrackingViewState) {
                        ((IStateManager)_viewState).TrackViewState();
                    }
                }
                return _viewState;
            }
        }

        // PERF: Don't create the EventArgument string until needed, when the verb is actually rendered.
        // Only called by WebPartChrome and WebPartMenu.  The verbs in CatalogZone, EditorZone, and
        // ConnectionsZone just use the EventArgument property.
        internal string GetEventArgument(string webPartID) {
            // If the prefix was never set, it means we are a user-specified verb with only a
            // client click handler.  So we should return String.Empty for our EventArgument.
            if (String.IsNullOrEmpty(_eventArgumentPrefix)) {
                return String.Empty;
            }
            else {
                if (_id == null) {
                    return _eventArgumentPrefix + webPartID;
                }
                else {
                    return _eventArgumentPrefix + _id +
                        WebPartZoneBase.EventArgumentSeparator + webPartID;
                }
            }
        }

        protected virtual void LoadViewState(object savedState) {
            if (savedState != null) {
                ((IStateManager)ViewState).LoadViewState(savedState);

                // Cache value of Visible
                object b = ViewState["Visible"];
                if (b != null) {
                    _visible = (bool)b;
                }
            }
        }

        protected virtual object SaveViewState() {
            if (_viewState != null) {
                return ((IStateManager)_viewState).SaveViewState();
            }
            return null;
        }

        // Only used by WebPartZoneBase for verbs rendered on a WebPart.
        internal void SetEventArgumentPrefix(string eventArgumentPrefix) {
            _eventArgumentPrefix = eventArgumentPrefix;
        }

        protected virtual void TrackViewState() {
            _isTrackingViewState = true;
            if (_viewState != null) {
                ((IStateManager)_viewState).TrackViewState();
            }
        }

        #region Implementation of IStateManager
        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        /// <internalonly/>
        void IStateManager.LoadViewState(object savedState) {
            LoadViewState(savedState);
        }

        /// <internalonly/>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }

        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }
        #endregion
    }
}
