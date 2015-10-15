//------------------------------------------------------------------------------
// <copyright file="CheckBoxList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
 
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    /// <devdoc>
    /// <para>Creates a group of <see cref='System.Web.UI.WebControls.CheckBox'/> controls.</para>
    /// </devdoc>
    public class CheckBoxList : ListControl, IRepeatInfoUser, INamingContainer, IPostBackDataHandler {
        private CheckBox _controlToRepeat;
        private string _oldAccessKey;
        private bool _hasNotifiedOfChange;
        private bool _cachedRegisterEnabled;
        private bool _cachedIsEnabled;
 
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.CheckBoxList'/> class.
        ///    </para>
        /// </devdoc>
        public CheckBoxList() {
            _controlToRepeat = new CheckBox();
            _controlToRepeat.EnableViewState = false;

            // Whidbey 28457: We need to set a default numeric ID for the case
            // of an empty checkbox list.  It is because the child CheckBox
            // always registers itself to Page as a PostBackData control and
            // during postback it will invoke LoadPostData in this class and the
            // method always assumes the ID is numeric.  This default ID setting
            // has been done in this way since V1.
            _controlToRepeat.ID = "0";

            Controls.Add(_controlToRepeat);
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the padding between each item.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(-1),
        WebSysDescription(SR.CheckBoxList_CellPadding)
        ]
        public virtual int CellPadding {
            get {
                if (ControlStyleCreated == false) {
                    return -1;
                }
                return ((TableStyle)ControlStyle).CellPadding;
            }
            set {
                ((TableStyle)ControlStyle).CellPadding = value;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the spacing between each item.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(-1),
        WebSysDescription(SR.CheckBoxList_CellSpacing)
        ]
        public virtual int CellSpacing {
            get {
                if (ControlStyleCreated == false) {
                    return -1;
                }
                return ((TableStyle)ControlStyle).CellSpacing;
            }
            set {
                ((TableStyle)ControlStyle).CellSpacing = value;
            }
        }
        
        internal override bool IsMultiSelectInternal  {
            get  {
                // a CheckBoxList is always multiselect.
                return true;
            }
        }


        /// <summary>
        /// <para>Indicates whether the control will be rendered when the data source has no items.</para>
        /// </summary>
        [DefaultValue(false)]
        [Themeable(true)]
        [WebCategory("Behavior")]
        [WebSysDescription(SR.ListControl_RenderWhenDataEmpty)]
        public virtual bool RenderWhenDataEmpty {
            get {
                object o = ViewState["RenderWhenDataEmpty"];
                return ((o == null) ? false : (bool)o);
            }
            set {
                ViewState["RenderWhenDataEmpty"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the number of columns to repeat.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(0),
        WebSysDescription(SR.CheckBoxList_RepeatColumns)
        ]
        public virtual int RepeatColumns {
            get {
                object o = ViewState["RepeatColumns"];
                return((o == null) ? 0 : (int)o);
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["RepeatColumns"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value that indicates whether the control is displayed
        ///       vertically or horizontally.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(RepeatDirection.Vertical),
        WebSysDescription(SR.Item_RepeatDirection)
        ]
        public virtual RepeatDirection RepeatDirection {
            get {
                object o = ViewState["RepeatDirection"];
                return((o == null) ? RepeatDirection.Vertical : (RepeatDirection)o);
            }
            set {
                if (value < RepeatDirection.Horizontal || value > RepeatDirection.Vertical) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["RepeatDirection"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value that indicates whether the control is displayed in
        ///    <see langword='Table '/>or <see langword='Flow '/>layout.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(RepeatLayout.Table),
        WebSysDescription(SR.WebControl_RepeatLayout)
        ]
        public virtual RepeatLayout RepeatLayout {
            get {
                object o = ViewState["RepeatLayout"];
                return((o == null) ? RepeatLayout.Table : (RepeatLayout)o);
            }
            set {
                EnumerationRangeValidationUtil.ValidateRepeatLayout(value);
                ViewState["RepeatLayout"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets
        ///       the alignment of the text label associated with each checkbox.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(TextAlign.Right),
        WebSysDescription(SR.WebControl_TextAlign)
        ]
        public virtual TextAlign TextAlign {
            get {
                object align = ViewState["TextAlign"];
                return((align == null) ? TextAlign.Right : (TextAlign)align);
            }
            set {
                if (value < TextAlign.Left || value > TextAlign.Right) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["TextAlign"] = value;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Creates a new control style object.</para>
        /// </devdoc>
        protected override Style CreateControlStyle() {
            return new TableStyle(ViewState);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Catches post data for each <see cref='System.Web.UI.WebControls.CheckBox'/> in the list.</para>
        /// </devdoc>
        protected override Control FindControl(string id, int pathOffset) {
            return this;
        }

        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (!DesignMode && !String.IsNullOrEmpty(ItemType)) {
                DataBoundControlHelper.EnableDynamicData(this, ItemType);
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Configures the <see cref='System.Web.UI.WebControls.CheckBoxList'/> prior to rendering on the client.</para>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            _controlToRepeat.AutoPostBack = AutoPostBack;
            _controlToRepeat.CausesValidation = CausesValidation;
            _controlToRepeat.ValidationGroup = ValidationGroup;

            if (Page != null) {
                // ensure postback data for those checkboxes which get unchecked or are different from their default value
                for (int i=0; i < Items.Count; i++) {
                    SetControlToRepeatID(this, _controlToRepeat, i);
                    Page.RegisterRequiresPostBack(_controlToRepeat);
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>
        ///       Displays the <see cref='System.Web.UI.WebControls.CheckBoxList'/> on the client.
        ///    </para>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            // Rendering an empty table is not valid xhtml or html 4, so throw
            if (RepeatLayout == RepeatLayout.Table && RenderWhenDataEmpty) {
                throw new InvalidOperationException(SR.GetString(SR.ListControl_RenderWhenDataEmptyNotSupportedWithTableLayout, ID));
            }

            // Don't render anything if the control is empty (unless the developer opts in by setting RenderWhenDataEmpty). 
            // empty table is not xhtml compliant.
            if (Items.Count == 0 && !EnableLegacyRendering && !RenderWhenDataEmpty) {
                return;
            }

            RepeatInfo repeatInfo = new RepeatInfo();
            Style style = (ControlStyleCreated ? ControlStyle : null);
            short tabIndex = TabIndex;
            bool undirtyTabIndex = false;

            // TabIndex here is special... it needs to be applied to the individual
            // checkboxes and not the outer control itself

            // Set the TextAlign property.
            _controlToRepeat.TextAlign = TextAlign;

            // cache away the TabIndex property state
            _controlToRepeat.TabIndex = tabIndex;
            if (tabIndex != 0) {
                if (ViewState.IsItemDirty("TabIndex") == false) {
                    undirtyTabIndex = true;
                }
                TabIndex = 0;
            }

            repeatInfo.RepeatColumns = RepeatColumns;
            repeatInfo.RepeatDirection = RepeatDirection;

            // If the device does not support tables, use the flow layout to render
            if (!DesignMode && !Context.Request.Browser.Tables) {
                repeatInfo.RepeatLayout = RepeatLayout.Flow;
            }
            else {
                repeatInfo.RepeatLayout = RepeatLayout;
            }

            if (repeatInfo.RepeatLayout == RepeatLayout.Flow) {
                repeatInfo.EnableLegacyRendering = EnableLegacyRendering;
            }

            // VSWhidbey 373655
            // Temporarily suppress AccessKey so base does not render it on the outside tag
            _oldAccessKey = AccessKey;
            AccessKey = String.Empty;

            repeatInfo.RenderRepeater(writer, (IRepeatInfoUser)this, style, this);

            // restore the state of AccessKey property
            AccessKey = _oldAccessKey;

            // restore the state of the TabIndex property
            if (tabIndex != 0) {
                TabIndex = tabIndex;
            }
            if (undirtyTabIndex) {
                ViewState.SetItemDirty("TabIndex", false);
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Processes posted data for the <see cref='System.Web.UI.WebControls.CheckBoxList'/> control.</para>
        /// </devdoc>
        bool IPostBackDataHandler.LoadPostData(String postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Processes posted data for the <see cref='System.Web.UI.WebControls.CheckBoxList'/> control.</para>
        /// </devdoc>
        protected virtual bool LoadPostData(String postDataKey, NameValueCollection postCollection) {
            if (IsEnabled == false) {
                // When a CheckBoxList is disabled, then there is no postback
                // data for it. Any checked state information has been loaded
                // via view state.
                return false;
            }

            // postDataKey looks like one of two things:
            // 1. "<uniqueid>$<index>" (EffectiveClientIDMode != Static when rendered last request)
            // 2. "<uniqueid>$<id>_<index>" (EffectiveClientIDMode == Static last request)
            
            string strIndex = postDataKey.Substring(UniqueID.Length + 1);
            // strIndex is now either (1) "<index>" or (2) "<id>_<index>"
            // Detect case #2 by looking for an underscore. Use LastIndexOf in case <id> contains one too.
            // No need to worry about a case that looks like "<uniqueid>$<id>", it would never have been rendered that way.
            int underscoreIndex = strIndex.LastIndexOf('_');
            if (underscoreIndex != -1) {
                strIndex = strIndex.Substring(underscoreIndex + 1);
            }
            // strIndex is now definitely the index as a string, regardless of which case postDataKey was in.
            int index = Int32.Parse(strIndex, CultureInfo.InvariantCulture);

            EnsureDataBoundInLoadPostData();
            
            // Maintain state from the form
            if (index >= 0 && index < Items.Count) {
                ListItem item = Items[index];
                if (item.Enabled == false) {
                    return false;
                }

                bool newCheckState = (postCollection[postDataKey] != null);

                if (item.Selected != newCheckState) {
                    item.Selected = newCheckState;
                    // LoadPostData will be invoked for each CheckBox that changed
                    // Suppress multiple change notification and fire only ONE change event
                    if (!_hasNotifiedOfChange) {
                        _hasNotifiedOfChange = true;
                        return true;
                    }
                }
            }

            return false;
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises when posted data for a control has changed.</para>
        /// </devdoc>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }
        

        /// <internalonly/>
        /// <devdoc>
        /// <para>Raises when posted data for a control has changed.</para>
        /// </devdoc>
        protected virtual void RaisePostDataChangedEvent() {
            if (AutoPostBack && !Page.IsPostBackEventControlRegistered) {
                // VSWhidbey 204824
                Page.AutoPostBackControl = this;

                if (CausesValidation) {
                    Page.Validate(ValidationGroup);
                }
            }
            OnSelectedIndexChanged(EventArgs.Empty);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        bool IRepeatInfoUser.HasFooter {
            get {
                return HasFooter;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual bool HasFooter {
            get {
                return false;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        bool IRepeatInfoUser.HasHeader {
            get {
                return HasHeader;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual bool HasHeader {
            get {
                return false;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        bool IRepeatInfoUser.HasSeparators {
            get {
                return HasSeparators;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual bool HasSeparators {
            get {
                return false;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        int IRepeatInfoUser.RepeatedItemCount {
            get {
                return RepeatedItemCount;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual int RepeatedItemCount {
            get {
                return (Items != null) ? Items.Count : 0;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        Style IRepeatInfoUser.GetItemStyle(ListItemType itemType, int repeatIndex) {
            return GetItemStyle(itemType, repeatIndex);
        }


        protected virtual Style GetItemStyle(ListItemType itemType, int repeatIndex) {
            return null;
        }


        /// <internalonly/>
        /// <devdoc>
        /// Called by the RepeatInfo helper to render each item
        /// </devdoc>
        void IRepeatInfoUser.RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer) {
            RenderItem(itemType, repeatIndex, repeatInfo, writer);
        }


        /// <internalonly/>
        /// <devdoc>
        /// Called by the RepeatInfo helper to render each item
        /// </devdoc>
        protected virtual void RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer) {
            if (repeatIndex == 0) {
                _cachedIsEnabled = IsEnabled;
                _cachedRegisterEnabled = (Page != null) && IsEnabled && (SaveSelectedIndicesViewState == false);
            }

            int repeatIndexOffset = repeatIndex;

            ListItem item = Items[repeatIndexOffset];

            // VSWhidbey 403433 Render expando attributes.
            _controlToRepeat.Attributes.Clear();
            if (item.HasAttributes) {
                foreach (string key in item.Attributes.Keys) {
                    _controlToRepeat.Attributes[key] = item.Attributes[key];
                }
            }

            // Dev10 684108: reset the CssClass for each item.
            if (!string.IsNullOrEmpty(_controlToRepeat.CssClass)) {
                _controlToRepeat.CssClass = "";
            }

            if (RenderingCompatibility >= VersionUtil.Framework40) {
                _controlToRepeat.InputAttributes.Add("value", item.Value);
            }
            SetControlToRepeatID(this, _controlToRepeat, repeatIndexOffset);
            _controlToRepeat.Text = item.Text;
            _controlToRepeat.Checked = item.Selected;
            _controlToRepeat.Enabled = _cachedIsEnabled && item.Enabled;
            _controlToRepeat.AccessKey = _oldAccessKey;

            if (_cachedRegisterEnabled && _controlToRepeat.Enabled) {
                // Store a client-side array of enabled control, so we can re-enable them on
                // postback (in case they are disabled client-side)
                // Postback is needed when SelectedIndices is not saved in view state
                Page.RegisterEnabledControl(_controlToRepeat);
            }

            _controlToRepeat.RenderControl(writer);
        }
    }
}
