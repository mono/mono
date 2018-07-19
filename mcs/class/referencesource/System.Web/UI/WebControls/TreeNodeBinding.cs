//------------------------------------------------------------------------------
// <copyright file="TreeNodeBinding.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;


    /// <devdoc>
    ///     Provides a data mapping definition for a TreeView
    /// </devdoc>
    [DefaultProperty("TextField")]
    public sealed class TreeNodeBinding : IStateManager, ICloneable, IDataSourceViewSchemaAccessor {
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
                string s = (string)ViewState["DataMember"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return s;
                }
            }
            set {
                ViewState["DataMember"] = value;
            }
        }


        /// <devdoc>
        ///     The depth of the level for which this TreeNodeBinding is defining a data mapping
        /// </devdoc>
        [
        DefaultValue(-1),
        TypeConverter("System.Web.UI.Design.WebControls.TreeNodeBindingDepthConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Data"),
        WebSysDescription(SR.TreeNodeBinding_Depth),
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

        [DefaultValue("")]
        [Localizable(true)]
        [WebCategory("Databindings")]
        [WebSysDescription(SR.TreeNodeBinding_FormatString)]
        public string FormatString {
            get {
                string s = (string)ViewState["FormatString"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["FormatString"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the TreeNodeBinding ImageToolTip
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.TreeNodeBinding_ImageToolTip)]
        public string ImageToolTip {
            get {
                string s = (string)ViewState["ImageToolTip"];

                if (s == null) {
                    return String.Empty;
                }

                return s;
            }
            set {
                ViewState["ImageToolTip"] = value;
            }
        }


        /// <devdoc>
        ///     Get and sets the fieldname to use for the ImageToolTip property in a TreeNode
        /// </devdoc>
        [DefaultValue("")]
        [TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign), WebSysDescription(SR.TreeNodeBinding_ImageToolTipField)]
        [WebCategory("Databindings")]
        public string ImageToolTipField {
            get {
                string s = (string)ViewState["ImageToolTipField"];

                if (s == null) {
                    return String.Empty;
                }
                else {
                    return s;
                }
            }
            set {
                ViewState["ImageToolTipField"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the image URl to be rendered for this node
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.TreeNodeBinding_ImageUrl)]
        public string ImageUrl {
            get {
                string s = (string)ViewState["ImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["ImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Get and sets the fieldname to use for the ImageUrl property in a TreeNode
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.TreeNodeBinding_ImageUrlField),
        ]
        public string ImageUrlField {
            get {
                string s = (string)ViewState["ImageUrlField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return s;
                }
            }
            set {
                ViewState["ImageUrlField"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the URL to navigate to when the node is clicked
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.TreeNodeBinding_NavigateUrl)]
        public string NavigateUrl {
            get {
                string s = (string)ViewState["NavigateUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["NavigateUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Get and sets the fieldname to use for the NavigateUrl property in a TreeNode
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.TreeNodeBinding_NavigateUrlField),
        ]
        public string NavigateUrlField {
            get {
                string s = (string)ViewState["NavigateUrlField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return s;
                }
            }
            set {
                ViewState["NavigateUrlField"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets whether to populate this binding immediately or on the next request for population
        /// </devdoc>
        [
        DefaultValue(false),
        WebCategory("DefaultProperties"),
        WebSysDescription(SR.TreeNodeBinding_PopulateOnDemand),
        ]
        public bool PopulateOnDemand {
            get {
                object o = ViewState["PopulateOnDemand"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
            set {
                ViewState["PopulateOnDemand"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the action which the TreeNodeBinding will perform when selected
        /// </devdoc>
        [DefaultValue(TreeNodeSelectAction.Select)]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.TreeNodeBinding_SelectAction)]
        public TreeNodeSelectAction SelectAction {
            get {
                object o = ViewState["SelectAction"];
                if (o == null) {
                    return TreeNodeSelectAction.Select;
                }
                return (TreeNodeSelectAction)o;
            }
            set {
                ViewState["SelectAction"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets whether the TreeNodeBinding has a CheckBox
        /// </devdoc>
        [DefaultValue(typeof(Nullable<bool>), "")]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.TreeNodeBinding_ShowCheckBox)]
        public bool? ShowCheckBox {
            get {
                object o = ViewState["ShowCheckBox"];
                if (o == null) {
                    return null;
                }
                return (bool?)o;
            }
            set {
                ViewState["ShowCheckBox"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the target window that the TreeNodeBinding will browse to if selected
        /// </devdoc>
        [DefaultValue("")]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.TreeNodeBinding_Target)]
        public string Target {
            get {
                string s = (string)ViewState["Target"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["Target"] = value;
            }
        }

        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.TreeNodeBinding_TargetField),
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
        [WebSysDescription(SR.TreeNodeBinding_Text)]
        public string Text {
            get {
                string s = (string)ViewState["Text"];
                if (s == null) {
                    s = (string)ViewState["Value"];
                    if (s == null) {
                        return String.Empty;
                    }
                }
                return s;
            }
            set {
                ViewState["Text"] = value;
            }
        }


        /// <devdoc>
        ///     Get and sets the fieldname to use for the Text property in a TreeNode
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.TreeNodeBinding_TextField),
        ]
        public string TextField {
            get {
                string s = (string)ViewState["TextField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return s;
                }
            }
            set {
                ViewState["TextField"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the TreeNodeBinding tooltip
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebCategory("DefaultProperties")]
        [WebSysDescription(SR.TreeNodeBinding_ToolTip)]
        public string ToolTip {
            get {
                string s = (string)ViewState["ToolTip"];
                if (s == null) {
                    return String.Empty;
                }
                return s;
            }
            set {
                ViewState["ToolTip"] = value;
            }
        }


        /// <devdoc>
        ///     Get and sets the fieldname to use for the ToolTip property in a TreeNode
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.TreeNodeBinding_ToolTipField),
        ]
        public string ToolTipField {
            get {
                string s = (string)ViewState["ToolTipField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return s;
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
        [WebSysDescription(SR.TreeNodeBinding_Value)]
        public string Value {
            get {
                string s = (string)ViewState["Value"];
                if (s == null) {
                    s = (string)ViewState["Text"];
                    if (s == null) {
                        return String.Empty;
                    }
                }
                return s;
            }
            set {
                ViewState["Value"] = value;
            }
        }


        /// <devdoc>
        ///     Get and sets the fieldname to use for the Value property in a TreeNode
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Databindings"),
        WebSysDescription(SR.TreeNodeBinding_ValueField),
        ]
        public string ValueField {
            get {
                string s = (string)ViewState["ValueField"];
                if (s == null) {
                    return String.Empty;
                }
                else {
                    return s;
                }
            }
            set {
                ViewState["ValueField"] = value;
            }
        }


        /// <devdoc>
        ///     The state for this TreeNodeBinding
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
        /// Creates a clone of the TreeNodeBinding.
        /// </devdoc>
        object ICloneable.Clone() {
            TreeNodeBinding clone = new TreeNodeBinding();
            clone.DataMember = DataMember;
            clone.Depth = Depth;
            clone.FormatString = FormatString;
            clone.ImageToolTip = ImageToolTip;
            clone.ImageToolTipField = ImageToolTipField;
            clone.ImageUrl = ImageUrl;
            clone.ImageUrlField = ImageUrlField;
            clone.NavigateUrl = NavigateUrl;
            clone.NavigateUrlField = NavigateUrlField;
            clone.PopulateOnDemand = PopulateOnDemand;
            clone.SelectAction = SelectAction;
            clone.ShowCheckBox = ShowCheckBox;
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
