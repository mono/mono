//------------------------------------------------------------------------------
// <copyright file="PagerSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;

    /// <devdoc>
    /// <para>Specifies the <see cref='System.Web.UI.WebControls.GridView'/> pager setting for the control. This class cannot be inherited.</para>
    /// </devdoc>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class PagerSettings : IStateManager {

        private StateBag _viewState;
        private bool _isTracking;



        [
        Browsable(false)
        ]
        public event EventHandler PropertyChanged;


        /// <devdoc>
        ///   Creates a new instance of PagerSettings.
        /// </devdoc>
        public PagerSettings() {
            _viewState = new StateBag();
        }


        /// <devdoc>
        ///    <para>Gets or sets the image path to be used for the First
        ///       page button.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        NotifyParentProperty(true),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.PagerSettings_FirstPageImageUrl)
        ]
        public string FirstPageImageUrl {
            get {
                object o = ViewState["FirstPageImageUrl"];
                if (o != null) {
                    return(string)o;
                }
                return String.Empty;
            }
            set {
                string oldValue = FirstPageImageUrl;
                if (oldValue != value) {
                    ViewState["FirstPageImageUrl"] = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <devdoc>
        ///    Gets or sets the text to be used for the First page
        ///    button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue("&lt;&lt;"),
        NotifyParentProperty(true),
        WebSysDescription(SR.PagerSettings_FirstPageText)
        ]
        public string FirstPageText {
            get {
                object o = ViewState["FirstPageText"];
                if (o != null) {
                    return(string)o;
                }
                return "&lt;&lt;";
            }
            set {
                string oldValue = FirstPageText;
                if (oldValue != value) {
                    ViewState["FirstPageText"] = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <devdoc>
        /// </devdoc>
        internal bool IsPagerOnBottom {
            get {
                PagerPosition position = Position;

                return(position == PagerPosition.Bottom) ||
                (position == PagerPosition.TopAndBottom);
            }
        }

        /// <devdoc>
        /// </devdoc>
        internal bool IsPagerOnTop {
            get {
                PagerPosition position = Position;

                return(position == PagerPosition.Top) ||
                (position == PagerPosition.TopAndBottom);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the image path to be used for the Last
        ///       page button.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        NotifyParentProperty(true),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.PagerSettings_LastPageImageUrl)
        ]
        public string LastPageImageUrl {
            get {
                object o = ViewState["LastPageImageUrl"];
                if (o != null) {
                    return(string)o;
                }
                return String.Empty;
            }
            set {
                string oldValue = LastPageImageUrl;
                if (oldValue != value) {
                    ViewState["LastPageImageUrl"] = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <devdoc>
        /// Gets or sets the text to be used for the Last page
        /// button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue("&gt;&gt;"),
        NotifyParentProperty(true),
        WebSysDescription(SR.PagerSettings_LastPageText)
        ]
        public string LastPageText {
            get {
                object o = ViewState["LastPageText"];
                if (o != null) {
                    return(string)o;
                }
                return "&gt;&gt;";
            }
            set {
                string oldValue = LastPageText;
                if (oldValue != value) {
                    ViewState["LastPageText"] = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <devdoc>
        ///    Gets or sets the type of Paging UI to use.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(PagerButtons.Numeric),
        NotifyParentProperty(true),
        WebSysDescription(SR.PagerSettings_Mode)
        ]
        public PagerButtons Mode {
            get {
                object o = ViewState["PagerMode"];
                if (o != null) {
                    return(PagerButtons)o;
                }
                return PagerButtons.Numeric;
            }
            set {
                if (value < PagerButtons.NextPrevious || value > PagerButtons.NumericFirstLast) {
                    throw new ArgumentOutOfRangeException("value");
                }
                PagerButtons oldValue = Mode;
                if (oldValue != value) {
                    ViewState["PagerMode"] = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the image path to be used for the Next
        ///       page button.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        NotifyParentProperty(true),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.PagerSettings_NextPageImageUrl)
        ]
        public string NextPageImageUrl {
            get {
                object o = ViewState["NextPageImageUrl"];
                if (o != null) {
                    return(string)o;
                }
                return String.Empty;
            }
            set {
                string oldValue = NextPageImageUrl;
                if (oldValue != value) {
                    ViewState["NextPageImageUrl"] = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <devdoc>
        ///    Gets or sets the text to be used for the Next page
        ///    button.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue("&gt;"),
        NotifyParentProperty(true),
        WebSysDescription(SR.PagerSettings_NextPageText)
        ]
        public string NextPageText {
            get {
                object o = ViewState["NextPageText"];
                if (o != null) {
                    return(string)o;
                }
                return "&gt;";
            }
            set {
                string oldValue = NextPageText;
                if (oldValue != value) {
                    ViewState["NextPageText"] = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the number of pages to show in the 
        ///       paging UI when the mode is <see langword='PagerMode.NumericPages'/>
        ///       .</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(10),
        NotifyParentProperty(true),
        WebSysDescription(SR.PagerSettings_PageButtonCount)
        ]
        public int PageButtonCount {
            get {
                object o = ViewState["PageButtonCount"];
                if (o != null) {
                    return(int)o;
                }
                return 10;
            }
            set {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                int oldValue = PageButtonCount;
                if (oldValue != value) {
                    ViewState["PageButtonCount"] = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <devdoc>
        ///    <para> Gets or sets the vertical
        ///       position of the paging UI bar with
        ///       respect to its associated control.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(PagerPosition.Bottom),
        NotifyParentProperty(true),
        WebSysDescription(SR.PagerStyle_Position)
        ]
        public PagerPosition Position {
            get {
                object o = ViewState["Position"];
                if (o != null) {
                    return(PagerPosition)o;
                }
                return PagerPosition.Bottom;
            }
            set {
                if (value < PagerPosition.Bottom || value > PagerPosition.TopAndBottom) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Position"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the image path to be used for the Previous
        ///       page button.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        NotifyParentProperty(true),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.PagerSettings_PreviousPageImageUrl)
        ]
        public string PreviousPageImageUrl {
            get {
                object o = ViewState["PreviousPageImageUrl"];
                if (o != null) {
                    return(string)o;
                }
                return String.Empty;
            }
            set {
                string oldValue = PreviousPageImageUrl;
                if (oldValue != value) {
                    ViewState["PreviousPageImageUrl"] = value;
                    OnPropertyChanged();
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the text to be used for the Previous
        ///       page button.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue("&lt;"),
        NotifyParentProperty(true),
        WebSysDescription(SR.PagerSettings_PreviousPageText)
        ]
        public string PreviousPageText {
            get {
                object o = ViewState["PreviousPageText"];
                if (o != null) {
                    return(string)o;
                }
                return "&lt;";
            }
            set {
                string oldValue = PreviousPageText;
                if (oldValue != value) {
                    ViewState["PreviousPageText"] = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <devdoc>
        ///    <para> Gets or set whether the paging
        ///       UI is to be shown.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(true),
        NotifyParentProperty(true),
        WebSysDescription(SR.PagerStyle_Visible)
        ]
        public bool Visible {
            get {
                object o = ViewState["PagerVisible"];
                if (o != null) {
                    return(bool)o;
                }
                return true;
            }
            set {
                ViewState["PagerVisible"] = value;
            }
        }

        /// <devdoc>
        /// <para>Gets the statebag for the PagerSettings. This property is read-only.</para>
        /// </devdoc>
        private StateBag ViewState {
            get {
                return _viewState;
            }
        }

        /// <devdoc>
        /// DataBound Controls use this event to rebind when settings have changed.
        /// </devdoc>
        void OnPropertyChanged() {
            if (PropertyChanged != null) {
                PropertyChanged(this, EventArgs.Empty);
            }
        }


        /// <devdoc>
        /// The propertyGrid uses ToString() to determine what text should be in the PagerSetting's edit box.
        /// </devdoc>
        public override string ToString() {
            return String.Empty;
        }

        #region IStateManager implementation

        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return _isTracking;
            }
        }


        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            if (state != null) {
                ((IStateManager)ViewState).LoadViewState(state);
            }
        }


        /// <internalonly/>
        object IStateManager.SaveViewState() {
            object state = ((IStateManager)ViewState).SaveViewState();
            return state;
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            _isTracking = true;
            ViewState.TrackViewState();
        }
        #endregion

    }
}

