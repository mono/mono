//------------------------------------------------------------------------------
// <copyright file="MenuItemBinding.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;


    /// <devdoc>
    ///     Provides a data mapping definition for a Menu
    /// </devdoc>
    [DefaultProperty("TextField")]
    public sealed class MenuItemBinding : IStateManager, ICloneable, IDataSourceViewSchemaAccessor {
        private bool _isTrackingViewState;
        private StateBag _viewState;


        /// <devdoc>
        ///     The data member to use in the mapping
        /// </devdoc>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.Binding_DataMember),
        ]
        public string DataMember {
            get {
                object s = ViewState["DataMember"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return (string)s;
                }
            }
            set {
                ViewState["DataMember"] = value;
            }
        }


        /// <devdoc>
        ///     The depth of the level for which this MenuItemBinding is defining a data mapping
        /// </devdoc>
        [
        DefaultValue(-1),
        TypeConverter("System.Web.UI.Design.WebControls.TreeNodeBindingDepthConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Data"),
        WebSysDescription(SR.MenuItemBinding_Depth),
        ]
        public int Depth {
            get {
                object o = ViewState["Depth"];
                if (o == null) {
                    return -1;
                }
                return (int)o;
            }
            set {
                ViewState["Depth"] = value;
            }
        }

        [DefaultValue(true)]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.MenuItemBinding_Enabled)]
        public bool Enabled {
            get {
                object o = ViewState["Enabled"];
                return (o == null ? true : (bool)o);
            }
            set {
                ViewState["Enabled"] = value;
            }
        }

        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.MenuItemBinding_EnabledField),
        ]
        public string EnabledField {
            get {
                object s = ViewState["EnabledField"];
                return (s == null ? String.Empty : (string)s);
            }
            set {
                ViewState["EnabledField"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the format string used to render the bound data for this node
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebCategory("Databindings")]
        [WebSysDescription(SR.MenuItemBinding_FormatString)]
        public string FormatString {
            get {
                object s = ViewState["FormatString"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["FormatString"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the image URl to be rendered for this node
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.MenuItemBinding_ImageUrl)]
        public string ImageUrl {
            get {
                object s = ViewState["ImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["ImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Get and sets the fieldname to use for the ImageUrl property in a MenuItem
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.MenuItemBinding_ImageUrlField),
        ]
        public string ImageUrlField {
            get {
                object s = ViewState["ImageUrlField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return (string)s;
                }
            }
            set {
                ViewState["ImageUrlField"] = value;
            }
        }

        [DefaultValue("")]
        [Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.MenuItemBinding_NavigateUrl)]
        public string NavigateUrl {
            get {
                object s = ViewState["NavigateUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["NavigateUrl"] = value;
            }
        }

        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.MenuItemBinding_NavigateUrlField),
        ]
        public string NavigateUrlField {
            get {
                object s = ViewState["NavigateUrlField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return (string)s;
                }
            }
            set {
                ViewState["NavigateUrlField"] = value;
            }
        }

        [DefaultValue("")]
        [Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.MenuItemBinding_PopOutImageUrl)]
        public string PopOutImageUrl {
            get {
                object s = ViewState["PopOutImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["PopOutImageUrl"] = value;
            }
        }

        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.MenuItemBinding_PopOutImageUrlField),
        ]
        public string PopOutImageUrlField {
            get {
                object s = ViewState["PopOutImageUrlField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return (string)s;
                }
            }
            set {
                ViewState["PopOutImageUrlField"] = value;
            }
        }

        [DefaultValue(true)]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.MenuItemBinding_Selectable)]
        public bool Selectable {
            get {
                object o = ViewState["Selectable"];
                return (o == null ? true : (bool)o);
            }
            set {
                ViewState["Selectable"] = value;
            }
        }

        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.MenuItemBinding_SelectableField),
        ]
        public string SelectableField {
            get {
                object s = ViewState["SelectableField"];
                return (s == null ? String.Empty : (string)s);
            }
            set {
                ViewState["SelectableField"] = value;
            }
        }

        [DefaultValue("")]
        [Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.MenuItemBinding_SeparatorImageUrl)]
        public string SeparatorImageUrl {
            get {
                object s = ViewState["SeparatorImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["SeparatorImageUrl"] = value;
            }
        }

        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.MenuItemBinding_SeparatorImageUrlField),
        ]
        public string SeparatorImageUrlField {
            get {
                object s = ViewState["SeparatorImageUrlField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return (string)s;
                }
            }
            set {
                ViewState["SeparatorImageUrlField"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the target window that the MenuItemBinding will browse to if selected
        /// </devdoc>
        [DefaultValue("")]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.MenuItemBinding_Target)]
        public string Target {
            get {
                object s = ViewState["Target"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["Target"] = value;
            }
        }

        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.MenuItemBinding_TargetField),
        ]
        public string TargetField {
            get {
                string s = (string)ViewState["TargetField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return s;
                }
            }
            set {
                ViewState["TargetField"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the display text
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.MenuItemBinding_Text)]
        public string Text {
            get {
                object s = ViewState["Text"];
                if (s == null) {
                    s = ViewState["Value"];
                    if (s == null) {
                        return String.Empty;
                    }
                }
                return (string)s;
            }
            set {
                ViewState["Text"] = value;
            }
        }


        /// <devdoc>
        ///     Get and sets the fieldname to use for the Text property in a MenuItem
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.MenuItemBinding_TextField),
        ]
        public string TextField {
            get {
                object s = ViewState["TextField"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["TextField"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the MenuItemBinding tooltip
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.MenuItemBinding_ToolTip)]
        public string ToolTip {
            get {
                object s = ViewState["ToolTip"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["ToolTip"] = value;
            }
        }


        /// <devdoc>
        ///     Get and sets the fieldname to use for the ToolTip property in a MenuItem
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.MenuItemBinding_ToolTipField),
        ]
        public string ToolTipField {
            get {
                object s = ViewState["ToolTipField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return (string)s;
                }
            }
            set {
                ViewState["ToolTipField"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the value
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.MenuItemBinding_Value)]
        public string Value {
            get {
                object s = ViewState["Value"];
                if (s == null) {
                    s = ViewState["Text"];
                    if (s == null) {
                        return String.Empty;
                    }
                }
                return (string)s;
            }
            set {
                ViewState["Value"] = value;
            }
        }


        /// <devdoc>
        ///     Get and sets the fieldname to use for the Value property in a MenuItem
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.MenuItemBinding_ValueField),
        ]
        public string ValueField {
            get {
                object s = ViewState["ValueField"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["ValueField"] = value;
            }
        }


        /// <devdoc>
        ///     The state for this MenuItemBinding
        /// </devdoc>
        private StateBag ViewState {
            get {
                if (_viewState == null) {
                    _viewState = new StateBag();
                    if (_isTrackingViewState) {
                        ((IStateManager)_viewState).TrackViewState();
                    }
                }
                return _viewState;
            }
        }

        internal void SetDirty() {
            ViewState.SetDirty(true);
        }

        public override string ToString() {
            return (String.IsNullOrEmpty(DataMember) ?
                    SR.GetString(SR.TreeNodeBinding_EmptyBindingText) :
                    DataMember);
        }

        #region ICloneable implemention

        /// <internalonly/>
        /// <devdoc>
        /// Creates a clone of the MenuItemBinding.
        /// </devdoc>
        object ICloneable.Clone() {
            MenuItemBinding clone = new MenuItemBinding();
            clone.DataMember = DataMember;
            clone.Depth = Depth;
            clone.Enabled = Enabled;
            clone.EnabledField = EnabledField;
            clone.FormatString = FormatString;
            clone.ImageUrl = ImageUrl;
            clone.ImageUrlField = ImageUrlField;
            clone.NavigateUrl = NavigateUrl;
            clone.NavigateUrlField = NavigateUrlField;
            clone.PopOutImageUrl = PopOutImageUrl;
            clone.PopOutImageUrlField = PopOutImageUrlField;
            clone.Selectable = Selectable;
            clone.SelectableField = SelectableField;
            clone.SeparatorImageUrl = SeparatorImageUrl;
            clone.SeparatorImageUrlField = SeparatorImageUrlField;
            clone.Target = Target;
            clone.TargetField = TargetField;
            clone.Text = Text;
            clone.TextField = TextField;
            clone.ToolTip = ToolTip;
            clone.ToolTipField = ToolTipField;
            clone.Value = Value;
            clone.ValueField = ValueField;

            return clone;
        }
        #endregion

        #region IStateManager implementation

        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return _isTrackingViewState;
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
            if (_viewState != null) {
                return ((IStateManager)_viewState).SaveViewState();
            }

            return null;
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            _isTrackingViewState = true;

            if (_viewState != null) {
                ((IStateManager)_viewState).TrackViewState();
            }
        }
        #endregion

        #region IDataSourceViewSchemaAccessor implementation

        /// <internalonly/>
        object IDataSourceViewSchemaAccessor.DataSourceViewSchema {
            get {
                return ViewState["IDataSourceViewSchemaAccessor.DataSourceViewSchema"];
            }
            set {
                ViewState["IDataSourceViewSchemaAccessor.DataSourceViewSchema"] = value;
            }
        }
        #endregion
    }
}
