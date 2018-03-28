//------------------------------------------------------------------------------
// <copyright file="HotSpot.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web.UI;
    using System.Globalization;

    /// <devdoc>
    /// <para>Defines abstract class from which all HotSpot shapes must inherit.</para>
    /// </devdoc>
    [
    TypeConverter(typeof(ExpandableObjectConverter))
    ]
    public abstract class HotSpot : IStateManager {

        private bool _isTrackingViewState;
        private StateBag _viewState;


        /// <devdoc>
        ///    <para>Gets or sets the keyboard shortcut key (AccessKey) for setting focus to the
        ///       HotSpot.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        Localizable(true),
        WebCategory("Accessibility"),
        WebSysDescription(SR.HotSpot_AccessKey)
        ]
        public virtual string AccessKey {
            get {
                string s = (string)ViewState["AccessKey"];
                if (s != null) {
                    return s;
                }
                return String.Empty;
            }
            set {
                // Valid values are null, String.Empty, and single character strings
                if ((value != null) && (value.Length > 1)) {
                    throw new ArgumentOutOfRangeException("value");
                }

                ViewState["AccessKey"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the tool tip displayed over the
        /// hotspot and the text for device-specific display.</para>
        /// </devdoc>
        [
        Localizable(true),
        Bindable(true),
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.HotSpot_AlternateText),
        NotifyParentProperty(true)
        ]
        public virtual String AlternateText {
            get {
                object text = ViewState["AlternateText"];
                return (text == null)? String.Empty : (string)text;
            }
            set {
                ViewState["AlternateText"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the HotSpotMode to either postback or navigation.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(HotSpotMode.NotSet),
        WebSysDescription(SR.HotSpot_HotSpotMode),
        NotifyParentProperty(true)
        ]
        public virtual HotSpotMode HotSpotMode {
            get {
                object obj = ViewState["HotSpotMode"];
                return (obj == null) ? HotSpotMode.NotSet : (HotSpotMode)obj;
            }
            set {
                if (value < HotSpotMode.NotSet || value > HotSpotMode.Inactive) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["HotSpotMode"] = value;
            }
        }



        /// <devdoc>
        /// <para>Gets or sets the argument for postback event.</para>
        /// </devdoc>
        [
        Bindable(true),
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.HotSpot_PostBackValue),
        NotifyParentProperty(true)
        ]
        public String PostBackValue {
            get {
                object value = ViewState["PostBackValue"];
                return (value == null)? String.Empty : (string)value;
            }
            set {
                ViewState["PostBackValue"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets the markup language string representation of the shape name.</para>
        /// </devdoc>
        protected internal abstract string MarkupName {
            get;
        }


        /// <devdoc>
        /// <para>Gets or sets the navigation url.</para>
        /// </devdoc>
        [
        Bindable(true),
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.HotSpot_NavigateUrl),
        NotifyParentProperty(true),
        UrlProperty(),
        Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        ]
        public String NavigateUrl {
            get {
                object value = ViewState["NavigateUrl"];
                return (value == null)? String.Empty : (string)value;
            }
            set {
                ViewState["NavigateUrl"] = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets the tab index of the HotSpot.</para>
        /// </devdoc>
        [
        DefaultValue((short)0),
        WebCategory("Accessibility"),
        WebSysDescription(SR.HotSpot_TabIndex)
        ]
        public virtual short TabIndex {
            get {
                object o = ViewState["TabIndex"];
                if (o != null) {
                     return (short) o;
                }
                return (short)0;
            }
            set {
                ViewState["TabIndex"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the name of the window for navigation.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        TypeConverter(typeof(TargetConverter)),
        WebSysDescription(SR.HotSpot_Target),
        NotifyParentProperty(true)
        ]
        public virtual string Target {
            get {
                object value = ViewState["Target"];
                return (value == null)? String.Empty : (string)value;
            }
            set {
                ViewState["Target"] = value;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected StateBag ViewState {
            get {
                if (_viewState == null) {
                    _viewState = new StateBag(false);
                    if (_isTrackingViewState) ((IStateManager)_viewState).TrackViewState();
                }
                return _viewState;
            }
        }


        /// <devdoc>
        /// <para>Returns a representation of the coordinates according to HTML standards.</para>
        /// </devdoc>
        public abstract string GetCoordinates();

        internal void SetDirty() {
            if (_viewState != null) {
                _viewState.SetDirty(true);
            }
        }


        public override string ToString () {
            return GetType().Name;
        }

        #region IStatemanager implementation

        /// <devdoc>
        /// <para>Gets a value indicating whether a server control is tracking its view state changes.</para>
        /// </devdoc>
        protected virtual bool IsTrackingViewState {
            get {
                return _isTrackingViewState;
            }
        }


        /// <devdoc>
        /// <para>Restores view-state information that was saved by SaveViewState.</para>
        /// </devdoc>
        protected virtual void LoadViewState(object savedState) {
            if (savedState != null) {
                ViewState.LoadViewState(savedState);
            }
        }


        /// <devdoc>
        /// <para>Saves any server control view-state changes that have
        /// occurred since the time the page was posted back to the server.</para>
        /// </devdoc>
        protected virtual object SaveViewState() {
            if (_viewState != null) {
                return _viewState.SaveViewState();
            }
            return null;
        }

        /// <devdoc>
        /// <para>Causes the tracking of view-state changes to the server control.</para>
        /// </devdoc>
        protected virtual void TrackViewState() {
            _isTrackingViewState = true;

            if (_viewState != null) {
                _viewState.TrackViewState();
            }
        }

        // private implementation of IStateManager


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
