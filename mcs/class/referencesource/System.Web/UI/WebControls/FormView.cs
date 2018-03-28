//------------------------------------------------------------------------------
// <copyright file="FormView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web.UI.WebControls.Adapters;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>
    ///       Displays a data record from a data source in a table layout. The data source
    ///       is any object that implements IEnumerable or IListSource, which includes ADO.NET data,
    ///       arrays, ArrayLists, DataSourceControl, etc.
    ///    </para>
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.FormViewDesigner, " + AssemblyRef.SystemDesign),
    ControlValueProperty("SelectedValue"),
    DefaultEvent("PageIndexChanging"),
    SupportsEventValidation
    ]
    [DataKeyProperty("DataKey")]
    public class FormView : CompositeDataBoundControl, IDataItemContainer, IPostBackEventHandler, 
                            IPostBackContainer, IDataBoundItemControl, IRenderOuterTableControl {

        private static readonly object EventPageIndexChanged = new object();
        private static readonly object EventPageIndexChanging = new object();
        private static readonly object EventItemCommand = new object();
        private static readonly object EventItemCreated = new object(); 
        private static readonly object EventItemDeleted = new object();
        private static readonly object EventItemDeleting = new object();
        private static readonly object EventItemInserting = new object();
        private static readonly object EventItemInserted = new object();
        private static readonly object EventItemUpdating = new object();
        private static readonly object EventItemUpdated = new object();
        private static readonly object EventModeChanged = new object();
        private static readonly object EventModeChanging = new object();

        private ITemplate _itemTemplate;
        private ITemplate _editItemTemplate;
        private ITemplate _insertItemTemplate;
        private ITemplate _headerTemplate;
        private ITemplate _footerTemplate;
        private ITemplate _pagerTemplate;
        private ITemplate _emptyDataTemplate;

        private TableItemStyle _rowStyle;
        private TableItemStyle _headerStyle;
        private TableItemStyle _footerStyle;
        private TableItemStyle _editRowStyle;
        private TableItemStyle _insertRowStyle;
        private TableItemStyle _emptyDataRowStyle;

        private FormViewRow _bottomPagerRow;
        private FormViewRow _footerRow;
        private FormViewRow _headerRow;
        private FormViewRow _topPagerRow;
        private FormViewRow _row;

        private TableItemStyle _pagerStyle;
        private PagerSettings _pagerSettings;

        private int _pageCount;
        private object _dataItem;
        private int _dataItemIndex;
        private OrderedDictionary _boundFieldValues;
        private DataKey _dataKey;
        private OrderedDictionary _keyTable;
        private string[] _dataKeyNames;

        private int _pageIndex;
        private FormViewMode _defaultMode = FormViewMode.ReadOnly;
        private FormViewMode _mode;
        private bool _modeSet;
        private bool _useServerPaging;
        private string _modelValidationGroup;

        private IOrderedDictionary _deleteKeys;
        private IOrderedDictionary _deleteValues;
        private IOrderedDictionary _insertValues;
        private IOrderedDictionary _updateKeys;
        private IOrderedDictionary _updateOldValues;
        private IOrderedDictionary _updateNewValues;

        /// <summary>
        /// The name of the method on the page which is called when this Control does an update operation.
        /// </summary>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.DataBoundControl_UpdateMethod)
        ]
        public new virtual string UpdateMethod {
            get {
                return base.UpdateMethod;
            }
            set {
                base.UpdateMethod = value;
            }
        }

        /// <summary>
        /// The name of the method on the page which is called when this Control does a delete operation.
        /// </summary>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.DataBoundControl_DeleteMethod)
        ]
        public new virtual string DeleteMethod {
            get {
                return base.DeleteMethod;
            }
            set {
                base.DeleteMethod = value;
            }
        }

        /// <summary>
        /// The name of the method on the page which is called when this Control does an insert operation.
        /// </summary>
        [
        DefaultValue(""),
        Themeable(false),
        WebCategory("Data"),
        WebSysDescription(SR.DataBoundControl_InsertMethod)
        ]
        public new virtual string InsertMethod {
            get {
                return base.InsertMethod;
            }
            set {
                base.InsertMethod = value;
            }
        }

        /// <devdoc>
        /// <para>Gets or sets a value that indicates whether paging is allowed.</para>
        /// </devdoc>
        [
        WebCategory("Paging"),
        DefaultValue(false),
        WebSysDescription(SR.FormView_AllowPaging)
        ]
        public virtual bool AllowPaging {
            get {
                object o = ViewState["AllowPaging"];
                if (o != null)
                    return (bool)o;
                return false;
            }
            set {
                bool oldValue = AllowPaging;
                if (value != oldValue) {
                    ViewState["AllowPaging"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }        

        /// <devdoc>
        /// <para>Gets or sets the URL of an image to display in the
        /// background of the <see cref='System.Web.UI.WebControls.FormView'/>.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.WebControl_BackImageUrl)
        ]
        public virtual string BackImageUrl {
            get {
                if (ControlStyleCreated == false) {
                    return String.Empty;
                }
                return ((TableStyle)ControlStyle).BackImageUrl;
            }
            set {
                ((TableStyle)ControlStyle).BackImageUrl = value;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual FormViewRow BottomPagerRow {
            get {
                if (_bottomPagerRow == null) {
                    EnsureChildControls();
                }
                return _bottomPagerRow;
            }
        }

        private IOrderedDictionary BoundFieldValues {
            get {
                if (_boundFieldValues == null) {
                    int capacity = 25;
                    _boundFieldValues = new OrderedDictionary(capacity);
                }
                return _boundFieldValues;
            }
        }


        [
        Localizable(true),
        DefaultValue(""),
        WebCategory("Accessibility"),
        WebSysDescription(SR.DataControls_Caption)
        ]
        public virtual string Caption {
            get {
                string s = (string)ViewState["Caption"];
                return (s != null) ? s : String.Empty;
            }
            set {
                ViewState["Caption"] = value;
            }
        }


        [
        DefaultValue(TableCaptionAlign.NotSet),
        WebCategory("Accessibility"),
        WebSysDescription(SR.WebControl_CaptionAlign)
        ]
        public virtual TableCaptionAlign CaptionAlign {
            get {
                object o = ViewState["CaptionAlign"];
                return (o != null) ? (TableCaptionAlign)o : TableCaptionAlign.NotSet;
            }
            set {
                if ((value < TableCaptionAlign.NotSet) ||
                    (value > TableCaptionAlign.Right)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["CaptionAlign"] = value;
            }
        }



        /// <devdoc>
        /// <para>Indicates the amount of space between cells.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(-1),
        WebSysDescription(SR.FormView_CellPadding)
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
        /// <para>Gets or sets the amount of space between the contents of
        /// a cell and the cell's border.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(0),
        WebSysDescription(SR.FormView_CellSpacing)
        ]
        public virtual int CellSpacing {
            get {
                if (ControlStyleCreated == false) {
                    return 0;
                }
                return ((TableStyle)ControlStyle).CellSpacing;
            }
            set {
                ((TableStyle)ControlStyle).CellSpacing = value;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public FormViewMode CurrentMode {
            get {
                return Mode;
            }
        }

        // implement this publicly so DataBinder.Eval(container.DataItem, "x") still works.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual object DataItem {
            get {
                if (CurrentMode == FormViewMode.Insert) {
                    return null;
                }
                return _dataItem;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int DataItemCount {
            get {
                return PageCount;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual int DataItemIndex {
            get {
                if (CurrentMode == FormViewMode.Insert) {
                    return -1;
                }
                return _dataItemIndex;
            }
        }


        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.DataFieldEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        TypeConverterAttribute(typeof(StringArrayConverter)),
        WebCategory("Data"),
        WebSysDescription(SR.DataControls_DataKeyNames)
        ]
        public virtual string[] DataKeyNames {
            get {
                object o = _dataKeyNames;
                if (o != null) {
                    return (string[])((string[])o).Clone();
                }
                return new string[0];
            }
            set {
                if (!DataBoundControlHelper.CompareStringArrays(value, DataKeyNamesInternal)) {
                    if (value != null) {
                        _dataKeyNames = (string[])value.Clone();
                    } else {
                        _dataKeyNames = null;
                    }

                    _keyTable = null;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        // This version doesn't clone the array
        private string[] DataKeyNamesInternal {
            get {
                object o = _dataKeyNames;
                if (o != null) {
                    return (string[])o;
                }
                return new string[0];
            }
        }


        /// <devdoc>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.FormView_DataKey)
        ]
        public virtual DataKey DataKey {
            get {
                if (_dataKey == null) {
                    _dataKey = new DataKey(KeyTable);
                }
                return _dataKey;
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(FormViewMode.ReadOnly),
        WebSysDescription(SR.View_DefaultMode)
        ]
        public virtual FormViewMode DefaultMode {
            get {
                return _defaultMode;
            }
            set {
                if (value < FormViewMode.ReadOnly || value > FormViewMode.Insert) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _defaultMode = value;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for an item set in edit mode within the FormView.
        /// This template is also used for Insert if no InsertItemTemplate is defined.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(FormView), BindingDirection.TwoWay),
        WebSysDescription(SR.FormView_EditItemTemplate)
        ]
        public virtual ITemplate EditItemTemplate {
            get {
                return _editItemTemplate;
            }
            set {
                _editItemTemplate = value;
            }
        }


        /// <devdoc>
        /// <para>Indicates the style properties of each row when in edit mode.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.View_EditRowStyle)
        ]
        public TableItemStyle EditRowStyle {
            get {
                if (_editRowStyle == null) {
                    _editRowStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_editRowStyle).TrackViewState();
                }
                return _editRowStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the style properties of null rows.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.View_EmptyDataRowStyle)
        ]
        public TableItemStyle EmptyDataRowStyle {
            get {
                if (_emptyDataRowStyle == null) {
                    _emptyDataRowStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_emptyDataRowStyle).TrackViewState();
                }
                return _emptyDataRowStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use when no records are returned from the datasource within the FormView.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(FormView)),
        WebSysDescription(SR.View_EmptyDataTemplate)
        ]
        public virtual ITemplate EmptyDataTemplate {
            get {
                return _emptyDataTemplate;
            }
            set {
                _emptyDataTemplate = value;
            }
        }


        /// <devdoc>
        /// <para>The header text displayed if no EmptyDataTemplate is defined.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.View_EmptyDataText),
        ]
        public virtual String EmptyDataText {
            get {
                object o = ViewState["EmptyDataText"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                ViewState["EmptyDataText"] = value;
            }
        }

        [
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.DataBoundControl_EnableModelValidation)
        ]
        public virtual bool EnableModelValidation {
            get {
                object o = ViewState["EnableModelValidation"];
                if (o != null) {
                    return (bool)o;
                }
                return true;
            }
            set {
                ViewState["EnableModelValidation"] = value;
            }
        }

        [
        WebCategory("Layout"),
        DefaultValue(true),
        WebSysDescription(SR.FormView_RenderOuterTable),
        SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")
        ]
        public virtual bool RenderOuterTable {
            get {
                object o = ViewState["RenderOuterTable"];
                return (o != null) ? (bool)o : true;
            }
            set {
                ViewState["RenderOuterTable"] = value;
            }
        }

        private int FirstDisplayedPageIndex {
            get {
                object o = ViewState["FirstDisplayedPageIndex"];
                if (o != null) {
                    return (int)o;
                }
                return -1;
            }
            set {
                ViewState["FirstDisplayedPageIndex"] = value;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual FormViewRow FooterRow {
            get {
                if (_footerRow == null) {
                    EnsureChildControls();
                }
                return _footerRow;
            }
        }


        /// <devdoc>
        /// <para>Indicates the style properties of the footer row.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.FormView_FooterStyle)
        ]
        public TableItemStyle FooterStyle {
            get {
                if (_footerStyle == null) {
                    _footerStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_footerStyle).TrackViewState();
                }
                return _footerStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for a footer item within the FormView.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(FormView)),
        WebSysDescription(SR.FormView_FooterTemplate)
        ]
        public virtual ITemplate FooterTemplate {
            get {
                return _footerTemplate;
            }
            set {
                _footerTemplate = value;
            }
        }


        /// <devdoc>
        /// <para>The header text displayed if no FooterTemplate is defined.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.View_FooterText),
        ]
        public virtual String FooterText {
            get {
                object o = ViewState["FooterText"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                ViewState["FooterText"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets a value that specifies the grid line style.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(GridLines.None),
        WebSysDescription(SR.DataControls_GridLines)
        ]
        public virtual GridLines GridLines {
            get {
                if (ControlStyleCreated == false) {
                    return GridLines.None;
                }
                return ((TableStyle)ControlStyle).GridLines;
            }
            set {
                ((TableStyle)ControlStyle).GridLines = value;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual FormViewRow HeaderRow {
            get {
                if (_headerRow == null) {
                    EnsureChildControls();
                }
                return _headerRow;
            }
        }


        /// <devdoc>
        /// <para>Indicates the style properties of the header row.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.WebControl_HeaderStyle)
        ]
        public TableItemStyle HeaderStyle {
            get {
                if (_headerStyle == null) {
                    _headerStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_headerStyle).TrackViewState();
                }
                return _headerStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for a header item within the FormView.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(FormView)),
        WebSysDescription(SR.WebControl_HeaderTemplate)
        ]
        public virtual ITemplate HeaderTemplate {
            get {
                return _headerTemplate;
            }
            set {
                _headerTemplate = value;
            }
        }


        /// <devdoc>
        /// <para>The header text displayed if no HeaderTemplate is defined.
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.View_HeaderText),
        ]
        public virtual String HeaderText {
            get {
                object o = ViewState["HeaderText"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                ViewState["HeaderText"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets a value that specifies the alignment of a rows with respect
        /// surrounding text.</para>
        /// </devdoc>
        [
        Category("Layout"),
        DefaultValue(HorizontalAlign.NotSet),
        WebSysDescription(SR.WebControl_HorizontalAlign)
        ]
        public virtual HorizontalAlign HorizontalAlign {
            get {
                if (ControlStyleCreated == false) {
                    return HorizontalAlign.NotSet;
                }
                return ((TableStyle)ControlStyle).HorizontalAlign;
            }
            set {
                ((TableStyle)ControlStyle).HorizontalAlign = value;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for an item set in insert mode within the FormView.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(FormView), BindingDirection.TwoWay),
        WebSysDescription(SR.FormView_InsertItemTemplate)
        ]
        public virtual ITemplate InsertItemTemplate {
            get {
                return _insertItemTemplate;
            }
            set {
                _insertItemTemplate = value;
            }
        }


        /// <devdoc>
        /// <para>Indicates the style properties of each row when in insert mode.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.View_InsertRowStyle)
        ]
        public TableItemStyle InsertRowStyle {
            get {
                if (_insertRowStyle == null) {
                    _insertRowStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_insertRowStyle).TrackViewState();
                }
                return _insertRowStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for an item within the FormView.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(FormView), BindingDirection.TwoWay),
        WebSysDescription(SR.View_InsertRowStyle)
        ]
        public virtual ITemplate ItemTemplate {
            get {
                return _itemTemplate;
            }
            set {
                _itemTemplate = value;
            }
        }

        private OrderedDictionary KeyTable {
            get {
                if (_keyTable == null) {
                    _keyTable = new OrderedDictionary(DataKeyNamesInternal.Length);
                }
                return _keyTable;
            }
        }


        private FormViewMode Mode {
            get {
                // if the mode wasn't explicitly set by LoadControlState or by the user, the mode is the DefaultMode.
                if (!_modeSet || DesignMode) {
                    _mode = DefaultMode;
                    _modeSet = true;
                }
                return _mode;
            }
            set {
                if (value < FormViewMode.ReadOnly || value > FormViewMode.Insert) {
                    throw new ArgumentOutOfRangeException("value");
                }

                _modeSet = true;
                if (_mode != value) {
                    _mode = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual int PageCount {
            get {
                return _pageCount;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the index of the currently displayed record.
        ///     This property echos the public one so that we can set PageIndex to -1
        ///     internally when we switch to insert mode, but users should never do that.</para>
        /// </devdoc>
        private int PageIndexInternal {
            get {
                return _pageIndex;
            }
            set {
                int currentPageIndex = PageIndexInternal;
                if (value != currentPageIndex) {
                    _pageIndex = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the index of the currently displayed record.</para>
        /// </devdoc>
        [
        Bindable(true),
        DefaultValue(0),
        WebCategory("Data"),
        WebSysDescription(SR.FormView_PageIndex)
        ]
        public virtual int PageIndex {
            get {
                // if we're in design mode, we don't want a change to the mode to set the PageIndex to -1.
                if (Mode == FormViewMode.Insert && !DesignMode) {
                    return -1;
                }
                return PageIndexInternal;
            }
            set {
                // since we don't know at property set time how many DataItems we'll have,
                // don't throw if we're above PageCount
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value >= 0) {
                    PageIndexInternal = value;
                }
            }
        }


        /// <devdoc>
        /// <para>Gets the settings of the pager buttons for the
        /// <see cref='System.Web.UI.WebControls.FormView'/>. This
        /// property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Paging"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.GridView_PagerSettings)
        ]
        public virtual PagerSettings PagerSettings {
            get {
                if (_pagerSettings == null) {
                    _pagerSettings = new PagerSettings();
                    if (IsTrackingViewState) {
                        ((IStateManager)_pagerSettings).TrackViewState();
                    }
                    _pagerSettings.PropertyChanged += new EventHandler(OnPagerPropertyChanged);
                }
                return _pagerSettings;
            }
        }


        /// <devdoc>
        /// <para>Gets the style properties of the pager rows for the
        /// <see cref='System.Web.UI.WebControls.FormView'/>. This
        /// property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.WebControl_PagerStyle)
        ]
        public TableItemStyle PagerStyle {
            get {
                if (_pagerStyle == null) {
                    _pagerStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_pagerStyle).TrackViewState();
                }
                return _pagerStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for a pager item within the FormView.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(FormView)),
        WebSysDescription(SR.View_PagerTemplate)
        ]
        public virtual ITemplate PagerTemplate {
            get {
                return _pagerTemplate;
            }
            set {
                _pagerTemplate = value;
            }
        }


        /// <devdoc>
        /// <para>Gets a collection of <see cref='System.Web.UI.WebControls.FormViewRow'/> objects representing the individual
        /// rows within the control.
        /// This property is read-only.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.FormView_Rows)
        ]
        public virtual FormViewRow Row {
            get {
                if (_row == null) {
                    EnsureChildControls();
                }
                return _row;
            }
        }


        /// <devdoc>
        /// <para>Indicates the style properties of each row.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.View_RowStyle)
        ]
        public TableItemStyle RowStyle {
            get {
                if (_rowStyle == null) {
                    _rowStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_rowStyle).TrackViewState();
                }
                return _rowStyle;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public object SelectedValue {
            get {
                return DataKey.Value;
            }
        }

        protected override HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.Table;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual FormViewRow TopPagerRow {
            get {
                if (_topPagerRow == null) {
                    EnsureChildControls();
                }
                return _topPagerRow;
            }
        }


        /// <devdoc>
        /// <para>Occurs when the FormView PageIndex has been changed.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.FormView_OnPageIndexChanged)
        ]
        public event EventHandler PageIndexChanged {
            add {
                Events.AddHandler(EventPageIndexChanged, value);
            }
            remove {
                Events.RemoveHandler(EventPageIndexChanged, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the FormView PageIndex is changing.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.FormView_OnPageIndexChanging)
        ]
        public event FormViewPageEventHandler PageIndexChanging {
            add {
                Events.AddHandler(EventPageIndexChanging, value);
            }
            remove {
                Events.RemoveHandler(EventPageIndexChanging, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when a command is issued from the FormView.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.FormView_OnItemCommand)
        ]
        public event FormViewCommandEventHandler ItemCommand {
            add {
                Events.AddHandler(EventItemCommand, value);
            }
            remove {
                Events.RemoveHandler(EventItemCommand, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when a row is created.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        WebSysDescription(SR.FormView_OnItemCreated)
        ]
        public event EventHandler ItemCreated {
            add {
                Events.AddHandler(EventItemCreated, value);
            }
            remove {
                Events.RemoveHandler(EventItemCreated, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the FormView item has been deleted.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemDeleted)
        ]
        public event FormViewDeletedEventHandler ItemDeleted {
            add {
                Events.AddHandler(EventItemDeleted, value);
            }
            remove {
                Events.RemoveHandler(EventItemDeleted, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the FormView item is being deleted.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemDeleting)
        ]
        public event FormViewDeleteEventHandler ItemDeleting {
            add {
                Events.AddHandler(EventItemDeleting, value);
            }
            remove {
                Events.RemoveHandler(EventItemDeleting, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the FormView item has been inserted.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemInserted)
        ]
        public event FormViewInsertedEventHandler ItemInserted {
            add {
                Events.AddHandler(EventItemInserted, value);
            }
            remove {
                Events.RemoveHandler(EventItemInserted, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the FormView item is being inserted.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemInserting)
        ]
        public event FormViewInsertEventHandler ItemInserting {
            add {
                Events.AddHandler(EventItemInserting, value);
            }
            remove {
                Events.RemoveHandler(EventItemInserting, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the FormView item has been updated.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemUpdated)
        ]
        public event FormViewUpdatedEventHandler ItemUpdated {
            add {
                Events.AddHandler(EventItemUpdated, value);
            }
            remove {
                Events.RemoveHandler(EventItemUpdated, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the FormView item is being updated.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemUpdating)
        ]
        public event FormViewUpdateEventHandler ItemUpdating {
            add {
                Events.AddHandler(EventItemUpdating, value);
            }
            remove {
                Events.RemoveHandler(EventItemUpdating, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the ViewMode has changed.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.FormView_OnModeChanged)
        ]
        public event EventHandler ModeChanged {
            add {
                Events.AddHandler(EventModeChanged, value);
            }
            remove {
                Events.RemoveHandler(EventModeChanged, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the ViewMode is changing.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.FormView_OnModeChanging)
        ]
        public event FormViewModeEventHandler ModeChanging {
            add {
                Events.AddHandler(EventModeChanging, value);
            }
            remove {
                Events.RemoveHandler(EventModeChanging, value);
            }
        }


        public void ChangeMode(FormViewMode newMode) {
            Mode = newMode;
        }

        /// <devdoc>
        /// <para>Creates the control hierarchy that is used to render the FormView.
        /// This is called whenever a control hierarchy is needed and the
        /// ChildControlsCreated property is false.
        /// The implementation assumes that all the children in the controls
        /// collection have already been cleared.</para>
        /// </devdoc>
        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding) {
            PagedDataSource pagedDataSource = null;
            int itemIndex = PageIndex;
            bool allowPaging = AllowPaging;
            int itemCount = 0;
            FormViewMode mode = Mode;

            // if we're in design mode, PageIndex doesn't return -1
            if (DesignMode && mode == FormViewMode.Insert) {
                itemIndex = -1;
            }

            if (dataBinding) {
                DataSourceView view = GetData();
                DataSourceSelectArguments arguments = SelectArguments;

                if (view == null) {
                    throw new HttpException(SR.GetString(SR.DataBoundControl_NullView, ID));
                }

                if (mode != FormViewMode.Insert) {
                    if (allowPaging && !view.CanPage) {
                        if (dataSource != null && !(dataSource is ICollection)) {
                            arguments.StartRowIndex = itemIndex;
                            arguments.MaximumRows = 1;
                            // This should throw an exception saying the data source can't page.
                            // We do this because the data source can provide a better error message than we can.
                            view.Select(arguments, SelectCallback);
                        }
                    }

                    if (_useServerPaging) {
                        if (view.CanRetrieveTotalRowCount) {
                            pagedDataSource = CreateServerPagedDataSource(arguments.TotalRowCount);
                        } else {
                            ICollection dataSourceCollection = dataSource as ICollection;
                            if (dataSourceCollection == null) {
                                throw new HttpException(SR.GetString(SR.DataBoundControl_NeedICollectionOrTotalRowCount, GetType().Name));
                            }

                            pagedDataSource = CreateServerPagedDataSource(checked(PageIndex + dataSourceCollection.Count));
                        }
                    } else {
                        pagedDataSource = CreatePagedDataSource();
                    }
                }
            } else {
                pagedDataSource = CreatePagedDataSource();
            }

            if (mode != FormViewMode.Insert) {
                pagedDataSource.DataSource = dataSource;
            }

            IEnumerator dataSourceEnumerator = null;
            OrderedDictionary keyTable = KeyTable;

            if (dataBinding == false) {
                dataSourceEnumerator = dataSource.GetEnumerator();

                ICollection collection = dataSource as ICollection;
                if (collection == null) {
                    throw new HttpException(SR.GetString(SR.DataControls_DataSourceMustBeCollectionWhenNotDataBinding));
                }
                itemCount = collection.Count;
            } else {
                keyTable.Clear();
                if (dataSource != null) {
                    if (mode != FormViewMode.Insert) {
                        ICollection collection = dataSource as ICollection;
                        if ((collection == null) && (pagedDataSource.IsPagingEnabled && !pagedDataSource.IsServerPagingEnabled)) {
                            throw new HttpException(SR.GetString(SR.FormView_DataSourceMustBeCollection, ID));
                        }

                        if (pagedDataSource.IsPagingEnabled) {
                            itemCount = pagedDataSource.DataSourceCount;
                        } else if (collection != null) {
                            itemCount = collection.Count;
                        }
                    }
                    dataSourceEnumerator = dataSource.GetEnumerator();
                }
            }

            Table table = CreateTable();
            TableRowCollection rows = table.Rows;
            bool moveNextSucceeded = false;
            object lastItem = null;

            Controls.Add(table);

            if (dataSourceEnumerator != null) {
                moveNextSucceeded = dataSourceEnumerator.MoveNext();    // goto the first item
            }

            // if there are no items, only add the tablerow if there's a null template or null text
            if (!moveNextSucceeded && mode != FormViewMode.Insert) {
                if (EmptyDataText.Length > 0 || _emptyDataTemplate != null) {
                    _row = CreateRow(0, DataControlRowType.EmptyDataRow, DataControlRowState.Normal, rows, null);
                }
                itemCount = 0;
            } else {
                int currentItemIndex = 0;
                if (!_useServerPaging) {
                    // skip over the first records that are before the page we're showing
                    for (; currentItemIndex < itemIndex; currentItemIndex++) {
                        lastItem = dataSourceEnumerator.Current;
                        moveNextSucceeded = dataSourceEnumerator.MoveNext();
                        if (!moveNextSucceeded) {
                            _pageIndex = currentItemIndex;
                            pagedDataSource.CurrentPageIndex = currentItemIndex;
                            itemIndex = currentItemIndex;
                            break;  // never throw if the PageIndex is out of range: just fix up PageIndex and goto the last item.
                        }
                    }
                }

                if (moveNextSucceeded) {
                    _dataItem = dataSourceEnumerator.Current;
                } else {
                    _dataItem = lastItem;   // if we broke out of the above loop, the current item will be invalid
                }


                // If we're not using server paging and this isn't a collection, or server paging doesn't return a page count, our _pageCount isn't accurate.
                // Loop through the rest of the enumeration to figure out how many items are in it.
                if ((!_useServerPaging && !(dataSource is ICollection)) || (_useServerPaging && itemCount < 0)) {
                    itemCount = currentItemIndex;
                    while (moveNextSucceeded) {
                        itemCount++;
                        moveNextSucceeded = dataSourceEnumerator.MoveNext();
                    }
                }

                _dataItemIndex = currentItemIndex;

                bool singlePage = itemCount <= 1 && !_useServerPaging; // hide pagers if there's only one item
                if (allowPaging && PagerSettings.Visible && _pagerSettings.IsPagerOnTop && mode != FormViewMode.Insert && !singlePage) {
                    // top pager
                    _topPagerRow = CreateRow(itemIndex, DataControlRowType.Pager, DataControlRowState.Normal, rows, pagedDataSource);
                }

                _headerRow = CreateRow(itemIndex, DataControlRowType.Header, DataControlRowState.Normal, rows, null);
                if (_headerTemplate == null && HeaderText.Length == 0) {
                    _headerRow.Visible = false;
                }

                _row = CreateDataRow(dataBinding, rows, _dataItem);

                if (itemIndex >= 0) {
                    string[] keyFields = DataKeyNamesInternal;
                    if (dataBinding && (keyFields.Length != 0)) {
                        foreach (string keyName in keyFields) {
                            object keyValue = DataBinder.GetPropertyValue(_dataItem, keyName);
                            keyTable.Add(keyName, keyValue);
                        }
                        _dataKey = new DataKey(keyTable);
                    }
                }

                _footerRow = CreateRow(itemIndex, DataControlRowType.Footer, DataControlRowState.Normal, rows, null);
                if (_footerTemplate == null && FooterText.Length == 0) {
                    _footerRow.Visible = false;
                }

                if (allowPaging && PagerSettings.Visible && _pagerSettings.IsPagerOnBottom && mode != FormViewMode.Insert && !singlePage) {
                    // bottom pager
                    _bottomPagerRow = CreateRow(itemIndex, DataControlRowType.Pager, DataControlRowState.Normal, rows, pagedDataSource);
                }
            }

            _pageCount = itemCount;

            OnItemCreated(EventArgs.Empty);

            if (dataBinding) {
                DataBind(false);
            }

            return itemCount;
        }


        /// <devdoc>
        /// <para>Creates new control style.</para>
        /// </devdoc>
        protected override Style CreateControlStyle() {
            TableStyle controlStyle = new TableStyle();

            // initialize defaults that are different from TableStyle
            controlStyle.CellSpacing = 0;

            return controlStyle;
        }

        private FormViewRow CreateDataRow(bool dataBinding, TableRowCollection rows, object dataItem) {
            ITemplate modeTemplate = null;

            switch (Mode) {
                case FormViewMode.Edit:
                    modeTemplate = _editItemTemplate;
                    break;
                case FormViewMode.Insert:
                    if (_insertItemTemplate != null) {
                        modeTemplate = _insertItemTemplate;
                    } else {
                        modeTemplate = _editItemTemplate;
                    }
                    break;
                case FormViewMode.ReadOnly:
                    modeTemplate = _itemTemplate;
                    break;
            }

            if (modeTemplate != null) {
                return CreateDataRowFromTemplates(dataBinding, rows);
            }
            return null;
        }

        private FormViewRow CreateDataRowFromTemplates(bool dataBinding, TableRowCollection rows) {
            DataControlRowState rowState = DataControlRowState.Normal;
            int itemIndex = PageIndex;
            FormViewMode mode = Mode;

            rowState = DataControlRowState.Normal;
            if (mode == FormViewMode.Edit)
                rowState |= DataControlRowState.Edit;
            else if (mode == FormViewMode.Insert)
                rowState |= DataControlRowState.Insert;

            return CreateRow(PageIndex, DataControlRowType.DataRow, rowState, rows, null);
        }

        protected override DataSourceSelectArguments CreateDataSourceSelectArguments() {
            DataSourceSelectArguments arguments = new DataSourceSelectArguments();
            DataSourceView view = GetData();

            _useServerPaging = AllowPaging && view.CanPage;

            // decide if we should use server-side paging
            if (_useServerPaging) {
                arguments.StartRowIndex = PageIndex;
                if (view.CanRetrieveTotalRowCount) {
                    arguments.RetrieveTotalRowCount = true;
                    arguments.MaximumRows = 1;
                } else {
                    arguments.MaximumRows = -1;
                }
            }

            return arguments;
        }

        /// <devdoc>
        /// Creates the pager for NextPrev and NextPrev with First and Last styles
        /// </devdoc>
        private void CreateNextPrevPager(TableRow row, PagedDataSource pagedDataSource, bool addFirstLastPageButtons) {
            PagerSettings pagerSettings = PagerSettings;
            string prevPageImageUrl = pagerSettings.PreviousPageImageUrl;
            string nextPageImageUrl = pagerSettings.NextPageImageUrl;
            bool isFirstPage = pagedDataSource.IsFirstPage;
            bool isLastPage = pagedDataSource.IsLastPage;


            if (addFirstLastPageButtons && !isFirstPage) {
                string firstPageImageUrl = pagerSettings.FirstPageImageUrl;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                IButtonControl firstButton;
                if (firstPageImageUrl.Length > 0) {
                    firstButton = new DataControlImageButton(this);
                    ((ImageButton)firstButton).ImageUrl = firstPageImageUrl;
                    ((ImageButton)firstButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.FirstPageText);
                } else {
                    firstButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)firstButton).Text = pagerSettings.FirstPageText;
                }
                firstButton.CommandName = DataControlCommands.PageCommandName;
                firstButton.CommandArgument = DataControlCommands.FirstPageCommandArgument;
                cell.Controls.Add((Control)firstButton);
            }

            if (!isFirstPage) {
                IButtonControl prevButton;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                if (prevPageImageUrl.Length > 0) {
                    prevButton = new DataControlImageButton(this);
                    ((ImageButton)prevButton).ImageUrl = prevPageImageUrl;
                    ((ImageButton)prevButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.PreviousPageText);
                } else {
                    prevButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)prevButton).Text = pagerSettings.PreviousPageText;
                }
                prevButton.CommandName = DataControlCommands.PageCommandName;
                prevButton.CommandArgument = DataControlCommands.PreviousPageCommandArgument;
                cell.Controls.Add((Control)prevButton);
            }


            if (!isLastPage) {
                IButtonControl nextButton;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                if (nextPageImageUrl.Length > 0) {
                    nextButton = new DataControlImageButton(this);
                    ((ImageButton)nextButton).ImageUrl = nextPageImageUrl;
                    ((ImageButton)nextButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.NextPageText);
                } else {
                    nextButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)nextButton).Text = pagerSettings.NextPageText;
                }
                nextButton.CommandName = DataControlCommands.PageCommandName;
                nextButton.CommandArgument = DataControlCommands.NextPageCommandArgument;
                cell.Controls.Add((Control)nextButton);
            }

            if (addFirstLastPageButtons && !isLastPage) {
                string lastPageImageUrl = pagerSettings.LastPageImageUrl;
                IButtonControl lastButton;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                if (lastPageImageUrl.Length > 0) {
                    lastButton = new DataControlImageButton(this);
                    ((ImageButton)lastButton).ImageUrl = lastPageImageUrl;
                    ((ImageButton)lastButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.LastPageText);

                } else {
                    lastButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)lastButton).Text = pagerSettings.LastPageText;
                }
                lastButton.CommandName = DataControlCommands.PageCommandName;
                lastButton.CommandArgument = DataControlCommands.LastPageCommandArgument;
                cell.Controls.Add((Control)lastButton);
            }
        }

        /// <devdoc>
        /// Creates the pager for NextPrev and NextPrev with First and Last styles
        /// </devdoc>
        private void CreateNumericPager(TableRow row, PagedDataSource pagedDataSource, bool addFirstLastPageButtons) {
            PagerSettings pagerSettings = PagerSettings;

            int pages = pagedDataSource.PageCount;
            int currentPage = pagedDataSource.CurrentPageIndex + 1;
            int pageSetSize = pagerSettings.PageButtonCount;
            int pagesShown = pageSetSize;
            int firstDisplayedPage = FirstDisplayedPageIndex + 1;   // first page displayed on last postback

            // ensure the number of pages we show isn't more than the number of pages that do exist
            if (pages < pagesShown)
                pagesShown = pages;

            // initialize to the first page set, i.e., pages 1 through number of pages shown
            int firstPage = 1;
            int lastPage = pagesShown;

            if (currentPage > lastPage) {
                // The current page is not in the first page set, then we need to slide the
                // range of pages shown by adjusting firstPage and lastPage
                int currentPageSet = (currentPage - 1) / pageSetSize;
                bool currentPageInLastDisplayRange = currentPage - firstDisplayedPage >= 0 && currentPage - firstDisplayedPage < pageSetSize;
                if (firstDisplayedPage > 0 && currentPageInLastDisplayRange) {
                    firstPage = firstDisplayedPage;
                } else {
                    firstPage = currentPageSet * pageSetSize + 1;
                }
                lastPage = firstPage + pageSetSize - 1;

                // now bring back lastPage into the range if its exceeded the number of pages
                if (lastPage > pages)
                    lastPage = pages;

                // if theres room to show more pages from the previous page set, then adjust
                // the first page accordingly
                if (lastPage - firstPage + 1 < pageSetSize) {
                    firstPage = Math.Max(1, lastPage - pageSetSize + 1);
                }
                FirstDisplayedPageIndex = firstPage - 1;
            }

            LinkButton button;

            if (addFirstLastPageButtons && currentPage != 1 && firstPage != 1) {
                string firstPageImageUrl = pagerSettings.FirstPageImageUrl;
                IButtonControl firstButton;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);

                if (firstPageImageUrl.Length > 0) {
                    firstButton = new DataControlImageButton(this);
                    ((ImageButton)firstButton).ImageUrl = firstPageImageUrl;
                    ((ImageButton)firstButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.FirstPageText);
                } else {
                    firstButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)firstButton).Text = pagerSettings.FirstPageText;
                }
                firstButton.CommandName = DataControlCommands.PageCommandName;
                firstButton.CommandArgument = DataControlCommands.FirstPageCommandArgument;
                cell.Controls.Add((Control)firstButton);
            }

            if (firstPage != 1) {
                TableCell cell = new TableCell();
                row.Cells.Add(cell);

                button = new DataControlPagerLinkButton(this);
                button.Text = "...";
                button.CommandName = DataControlCommands.PageCommandName;
                button.CommandArgument = (firstPage - 1).ToString(NumberFormatInfo.InvariantInfo);
                cell.Controls.Add(button);
            }

            for (int i = firstPage; i <= lastPage; i++) {
                TableCell cell = new TableCell();
                row.Cells.Add(cell);

                string pageString = (i).ToString(NumberFormatInfo.InvariantInfo);
                if (i == currentPage) {
                    Label label = new Label();

                    label.Text = pageString;
                    cell.Controls.Add(label);
                } else {
                    button = new DataControlPagerLinkButton(this);

                    button.Text = pageString;
                    button.CommandName = DataControlCommands.PageCommandName;
                    button.CommandArgument = pageString;
                    cell.Controls.Add(button);
                }
            }

            if (pages > lastPage) {
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                button = new DataControlPagerLinkButton(this);

                button.Text = "...";
                button.CommandName = DataControlCommands.PageCommandName;
                button.CommandArgument = (lastPage + 1).ToString(NumberFormatInfo.InvariantInfo);
                cell.Controls.Add(button);
            }

            bool isLastPageShown = lastPage == pages;
            if (addFirstLastPageButtons && currentPage != pages && !isLastPageShown) {
                string lastPageImageUrl = pagerSettings.LastPageImageUrl;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);

                IButtonControl lastButton;
                if (lastPageImageUrl.Length > 0) {
                    lastButton = new DataControlImageButton(this);
                    ((ImageButton)lastButton).ImageUrl = lastPageImageUrl;
                    ((ImageButton)lastButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.LastPageText);
                } else {
                    lastButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)lastButton).Text = pagerSettings.LastPageText;
                }
                lastButton.CommandName = DataControlCommands.PageCommandName;
                lastButton.CommandArgument = DataControlCommands.LastPageCommandArgument;
                cell.Controls.Add((Control)lastButton);
            }
        }

        private PagedDataSource CreatePagedDataSource() {
            PagedDataSource pagedDataSource = new PagedDataSource();

            pagedDataSource.CurrentPageIndex = PageIndex;
            pagedDataSource.PageSize = 1;
            pagedDataSource.AllowPaging = AllowPaging;
            pagedDataSource.AllowCustomPaging = false;
            pagedDataSource.AllowServerPaging = false;
            pagedDataSource.VirtualCount = 0;

            return pagedDataSource;
        }

        private PagedDataSource CreateServerPagedDataSource(int totalRowCount) {
            PagedDataSource pagedDataSource = new PagedDataSource();

            pagedDataSource.CurrentPageIndex = PageIndex;
            pagedDataSource.PageSize = 1;
            pagedDataSource.AllowPaging = AllowPaging;
            pagedDataSource.AllowCustomPaging = false;
            pagedDataSource.AllowServerPaging = true;
            pagedDataSource.VirtualCount = totalRowCount;

            return pagedDataSource;
        }

        private FormViewRow CreateRow(int itemIndex, DataControlRowType rowType, DataControlRowState rowState, TableRowCollection rows, PagedDataSource pagedDataSource) {
            FormViewRow row = CreateRow(itemIndex, rowType, rowState);
            row.RenderTemplateContainer = RenderOuterTable;

            rows.Add(row);

            if (rowType != DataControlRowType.Pager) {
                InitializeRow(row);
            } else {
                InitializePager(row, pagedDataSource);
            }

            return row;
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual FormViewRow CreateRow(int itemIndex, DataControlRowType rowType, DataControlRowState rowState) {
            if (rowType == DataControlRowType.Pager) {
                return new FormViewPagerRow(itemIndex, rowType, rowState);
            }
            return new FormViewRow(itemIndex, rowType, rowState);
        }


        /// <devdoc>
        /// Creates a new ChildTable, which is the containing table
        /// </devdoc>
        protected virtual Table CreateTable() {
            return new ChildTable(String.IsNullOrEmpty(ID) ? null : ClientID);            
        }

        /// Data bound controls should override PerformDataBinding instead
        /// of DataBind.  If DataBind if overridden, the OnDataBinding and OnDataBound events will
        /// fire in the wrong order.  However, for backwards compat on ListControl and AdRotator, we 
        /// can't seal this method.  It is sealed on all new BaseDataBoundControl-derived controls.
        public override sealed void DataBind() {
            base.DataBind();
        }

        public virtual void DeleteItem() {
            // use EnableModelVadliation as the causesValdiation param because the hosting page should not
            // be validated unless model validation is going to be used
            ResetModelValidationGroup(EnableModelValidation, String.Empty);
            HandleDelete(String.Empty);
        }

        /// <devdoc>
        /// Override EnsureDataBound because we don't want to databind when we're in insert mode
        /// </devdoc>
        protected override void EnsureDataBound() {
            if (RequiresDataBinding && Mode == FormViewMode.Insert) {
                OnDataBinding(EventArgs.Empty);

                RequiresDataBinding = false;
                MarkAsDataBound();
                if (AdapterInternal != null) {
                    DataBoundControlAdapter dataBoundControlAdapter = AdapterInternal as DataBoundControlAdapter;
                    if (dataBoundControlAdapter != null) {
                        dataBoundControlAdapter.PerformDataBinding(null);
                    } else {
                        PerformDataBinding(null);
                    }
                } else {
                    PerformDataBinding(null);
                }

                OnDataBound(EventArgs.Empty);
            } else {
                base.EnsureDataBound();
            }
        }


        protected virtual void ExtractRowValues(IOrderedDictionary fieldValues, bool includeKeys) {
            if (fieldValues == null) {
                Debug.Assert(false, "FormView::ExtractRowValues- must hand in a valid reference to an IDictionary.");
                return;
            }

            DataBoundControlHelper.ExtractValuesFromBindableControls(fieldValues, this);

            IBindableTemplate bindableTemplate = null;
            if (Mode == FormViewMode.ReadOnly && ItemTemplate != null) {
                bindableTemplate = ItemTemplate as IBindableTemplate;
            } else if ((Mode == FormViewMode.Edit || (Mode == FormViewMode.Insert && InsertItemTemplate == null)) && EditItemTemplate != null) {
                bindableTemplate = EditItemTemplate as IBindableTemplate;
            } else if (Mode == FormViewMode.Insert && InsertItemTemplate != null) {
                bindableTemplate = InsertItemTemplate as IBindableTemplate;
            }
            string[] dataKeyNames = DataKeyNamesInternal;

            if (bindableTemplate != null) {
                FormView container = this;
                if (container != null && bindableTemplate != null) {
                    foreach (DictionaryEntry entry in bindableTemplate.ExtractValues(container)) {
                        if (!includeKeys && Array.IndexOf(dataKeyNames, entry.Key) != -1) {
                            continue;
                        }
                        fieldValues[entry.Key] = entry.Value;
                    }
                }
            }

            return;
        }

        private void HandleCancel() {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            FormViewModeEventArgs e = new FormViewModeEventArgs(DefaultMode, true);
            OnModeChanging(e);

            if (e.Cancel) {
                return;
            }

            if (isBoundToDataSourceControl) {
                Mode = e.NewMode;
                OnModeChanged(EventArgs.Empty);
            }

            RequiresDataBinding = true;
        }

        private void HandleDelete(string commandArg) {
            int pageIndex = PageIndex;

            if (pageIndex < 0) {    // don't attempt to delete in Insert mode
                return;
            }

            DataSourceView view = null;
            int itemIndex = PageIndex;
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            if (isBoundToDataSourceControl) {
                view = GetData();
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.View_DataSourceReturnedNullView, ID));
                }
            }

            FormViewDeleteEventArgs e = new FormViewDeleteEventArgs(itemIndex);


            ExtractRowValues(e.Values, false/*includeKeys*/);
            foreach (DictionaryEntry entry in DataKey.Values) {
                e.Keys.Add(entry.Key, entry.Value);
                if (e.Values.Contains(entry.Key)) {
                    e.Values.Remove(entry.Key);
                }
            }
            

            OnItemDeleting(e);

            if (e.Cancel) {
                return;
            }

            if (isBoundToDataSourceControl) {
                _deleteKeys = e.Keys;
                _deleteValues = e.Values;
                view.Delete(e.Keys, e.Values, HandleDeleteCallback);
            }
        }

        private bool HandleDeleteCallback(int affectedRows, Exception ex) {
            int pageIndex = PageIndex;
            FormViewDeletedEventArgs fea = new FormViewDeletedEventArgs(affectedRows, ex);
            fea.SetKeys(_deleteKeys);
            fea.SetValues(_deleteValues);

            OnItemDeleted(fea);

            _deleteKeys = null;
            _deleteValues = null;

            if (ex != null && !fea.ExceptionHandled) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
            }

            if (pageIndex == _pageCount - 1) {
                HandlePage(pageIndex - 1);
            }

            RequiresDataBinding = true;
            return true;
        }

        private void HandleEdit() {
            if (PageIndex < 0) {
                return;
            }

            FormViewModeEventArgs e = new FormViewModeEventArgs(FormViewMode.Edit, false);
            OnModeChanging(e);

            if (e.Cancel) {
                return;
            }

            if (IsDataBindingAutomatic) {
                Mode = e.NewMode;
                OnModeChanged(EventArgs.Empty);
            }

            RequiresDataBinding = true;
        }

        private bool HandleEvent(EventArgs e, bool causesValidation, string validationGroup) {
            bool handled = false;

            ResetModelValidationGroup(causesValidation, validationGroup);

            FormViewCommandEventArgs dce = e as FormViewCommandEventArgs;

            if (dce != null) {

                OnItemCommand(dce);
                if (dce.Handled) {
                    return true;
                }
                handled = true;

                string command = dce.CommandName;
                int newItemIndex = PageIndex;

                if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.PageCommandName)) {
                    string itemIndexArg = (string)dce.CommandArgument;

                    if (StringUtil.EqualsIgnoreCase(itemIndexArg, DataControlCommands.NextPageCommandArgument)) {
                        newItemIndex++;
                    } else if (StringUtil.EqualsIgnoreCase(itemIndexArg, DataControlCommands.PreviousPageCommandArgument)) {
                        newItemIndex--;
                    } else if (StringUtil.EqualsIgnoreCase(itemIndexArg, DataControlCommands.FirstPageCommandArgument)) {
                        newItemIndex = 0;
                    } else if (StringUtil.EqualsIgnoreCase(itemIndexArg, DataControlCommands.LastPageCommandArgument)) {
                        newItemIndex = PageCount - 1;
                    } else {
                        // argument is page number, and page index is 1 less than that
                        newItemIndex = Convert.ToInt32(itemIndexArg, CultureInfo.InvariantCulture) - 1;
                    }
                    HandlePage(newItemIndex);
                } else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.EditCommandName)) {
                    HandleEdit();
                } else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.UpdateCommandName)) {
                    HandleUpdate((string)dce.CommandArgument, causesValidation);
                } else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.CancelCommandName)) {
                    HandleCancel();
                } else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.DeleteCommandName)) {
                    HandleDelete((string)dce.CommandArgument);
                } else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.InsertCommandName)) {
                    HandleInsert((string)dce.CommandArgument, causesValidation);
                } else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.NewCommandName)) {
                    HandleNew();
                } else {
                    // unhandled event should be bubbled up here. (DevDiv Bugs 161011)
                    handled = HandleCommand(command);
                }
            }

            return handled;
        }

        private bool HandleCommand(string commandName) {
            DataSourceView view = null;

            if (IsDataBindingAutomatic) {
                view = GetData();
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.View_DataSourceReturnedNullView, ID));
                }
            }
            else {
                // This feature is only for data sources
                return false;
            }
            
            if (!view.CanExecute(commandName)) {
                return false;
            }

            OrderedDictionary values = new OrderedDictionary();
            OrderedDictionary keys = new OrderedDictionary();
            
            ExtractRowValues(values, false /*includeKey*/);

            foreach (DictionaryEntry entry in DataKey.Values) {
                keys.Add(entry.Key, entry.Value);
                if (values.Contains(entry.Key)) {
                    values.Remove(entry.Key);
                }
            }

            view.ExecuteCommand(commandName, keys, values, HandleCommandCallback);
            return true;
        }

        private bool HandleCommandCallback(int affectedRows, Exception ex) {
            if (ex != null) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
            }

            RequiresDataBinding = true;
            return true;
        }

        private void HandleInsert(string commandArg, bool causesValidation) {
            if (causesValidation && Page != null && !Page.IsValid) {
                return;
            }

            if (Mode != FormViewMode.Insert) {
                throw new HttpException(SR.GetString(SR.DetailsViewFormView_ControlMustBeInInsertMode, "FormView", ID));
            }

            DataSourceView view = null;
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            if (isBoundToDataSourceControl) {
                view = GetData();
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.View_DataSourceReturnedNullView, ID));
                }
            }

            FormViewInsertEventArgs e = new FormViewInsertEventArgs(commandArg);


            ExtractRowValues(e.Values, true/*includeKeys*/);
            

            OnItemInserting(e);

            if (e.Cancel) {
                return;
            }

            if (isBoundToDataSourceControl) {
                _insertValues = e.Values;
                view.Insert(e.Values, HandleInsertCallback);
            }
        }

        private bool HandleInsertCallback(int affectedRows, Exception ex) {
            FormViewInsertedEventArgs fea = new FormViewInsertedEventArgs(affectedRows, ex);
            fea.SetValues(_insertValues);

            OnItemInserted(fea);

            _insertValues = null;
            if (ex != null && !fea.ExceptionHandled) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
                fea.KeepInInsertMode = true;
            }

            if (IsUsingModelBinders && !Page.ModelState.IsValid) {
                fea.KeepInInsertMode = true;
            }

            if (!fea.KeepInInsertMode) {
                FormViewModeEventArgs eMode = new FormViewModeEventArgs(DefaultMode, false);
                OnModeChanging(eMode);
                if (!eMode.Cancel) {
                    Mode = eMode.NewMode;
                    OnModeChanged(EventArgs.Empty);
                    RequiresDataBinding = true;
                }
            }
            return true;
        }

        private void HandleNew() {
            FormViewModeEventArgs e = new FormViewModeEventArgs(FormViewMode.Insert, false);
            OnModeChanging(e);

            if (e.Cancel) {
                return;
            }

            if (IsDataBindingAutomatic) {
                Mode = e.NewMode;
                OnModeChanged(EventArgs.Empty);
            }

            RequiresDataBinding = true;
        }

        private void HandlePage(int newPage) {
            if (!AllowPaging) {
                return;
            }

            if (PageIndex < 0) {
                return;
            }

            FormViewPageEventArgs e = new FormViewPageEventArgs(newPage);
            OnPageIndexChanging(e);

            if (e.Cancel) {
                return;
            }

            if (e.NewPageIndex > -1) {
                // if the requested page is out of range and we're already on the last page, don't rebind
                if ((e.NewPageIndex >= PageCount && _pageIndex == PageCount - 1)) {
                    return;
                }
                // DevDiv Bugs 188830: Don't clear key table if the page is out of range, since control won't be rebound.
                _keyTable = null;
                _pageIndex = e.NewPageIndex;
            }
            else {
                return;
            }

            OnPageIndexChanged(EventArgs.Empty);
            RequiresDataBinding = true;
        }

        private void HandleUpdate(string commandArg, bool causesValidation) {
            if (causesValidation && Page != null && !Page.IsValid) {
                return;
            }

            if (Mode != FormViewMode.Edit) {
                throw new HttpException(SR.GetString(SR.DetailsViewFormView_ControlMustBeInEditMode, "FormView", ID));
            }

            if (PageIndex < 0) {
                return;
            }

            DataSourceView view = null;
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            if (isBoundToDataSourceControl) {
                view = GetData();
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.View_DataSourceReturnedNullView, ID));
                }
            }

            FormViewUpdateEventArgs e = new FormViewUpdateEventArgs(commandArg);


            foreach (DictionaryEntry entry in BoundFieldValues) {
                e.OldValues.Add(entry.Key, entry.Value);
            }

            ExtractRowValues(e.NewValues, true/*includeKeys*/);
            foreach (DictionaryEntry entry in DataKey.Values) {
                e.Keys.Add(entry.Key, entry.Value);
            }

            

            OnItemUpdating(e);

            if (e.Cancel) {
                return;
            }

            if (isBoundToDataSourceControl) {
                _updateKeys = e.Keys;
                _updateOldValues = e.OldValues;
                _updateNewValues = e.NewValues;

                view.Update(e.Keys, e.NewValues, e.OldValues, HandleUpdateCallback);
            }
        }

        private bool HandleUpdateCallback(int affectedRows, Exception ex) {
            FormViewUpdatedEventArgs fea = new FormViewUpdatedEventArgs(affectedRows, ex);
            fea.SetOldValues(_updateOldValues);
            fea.SetNewValues(_updateNewValues);
            fea.SetKeys(_updateKeys);

            OnItemUpdated(fea);
            _updateKeys = null;
            _updateOldValues = null;
            _updateNewValues = null;

            if (ex != null && !fea.ExceptionHandled) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
                fea.KeepInEditMode = true;
            }

            if (IsUsingModelBinders && !Page.ModelState.IsValid) {
                fea.KeepInEditMode = true;
            }

            if (!fea.KeepInEditMode) {
                FormViewModeEventArgs eMode = new FormViewModeEventArgs(DefaultMode, false);
                OnModeChanging(eMode);
                if (!eMode.Cancel) {
                    Mode = eMode.NewMode;
                    OnModeChanged(EventArgs.Empty);
                    RequiresDataBinding = true;
                }
            }
            return true;
        }


        /// <devdoc>
        /// <para>
        /// Creates a FormViewRow that contains the paging UI.
        /// The paging UI is a navigation bar that is a built into a single TableCell that
        /// spans across all fields of the FormView.
        /// </para>
        /// </devdoc>
        protected virtual void InitializePager(FormViewRow row, PagedDataSource pagedDataSource) {
            TableCell cell = new TableCell();

            PagerSettings pagerSettings = PagerSettings;

            if (_pagerTemplate != null) {
                _pagerTemplate.InstantiateIn(cell);
            } else {
                PagerTable pagerTable = new PagerTable();
                TableRow pagerTableRow = new TableRow();
                cell.Controls.Add(pagerTable);
                pagerTable.Rows.Add(pagerTableRow);
                switch (pagerSettings.Mode) {
                    case PagerButtons.NextPrevious:
                        CreateNextPrevPager(pagerTableRow, pagedDataSource, false);
                        break;
                    case PagerButtons.Numeric:
                        CreateNumericPager(pagerTableRow, pagedDataSource, false);
                        break;
                    case PagerButtons.NextPreviousFirstLast:
                        CreateNextPrevPager(pagerTableRow, pagedDataSource, true);
                        break;
                    case PagerButtons.NumericFirstLast:
                        CreateNumericPager(pagerTableRow, pagedDataSource, true);
                        break;
                }
            }
            cell.ColumnSpan = 2;
            row.Cells.Add(cell);
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void InitializeRow(FormViewRow row) {
            TableCellCollection cells = row.Cells;
            TableCell contentCell = new TableCell();
            ITemplate contentTemplate = _itemTemplate;
            int itemIndex = row.ItemIndex;
            DataControlRowState rowState = row.RowState;

            switch (row.RowType) {
                case DataControlRowType.DataRow:
                    contentCell.ColumnSpan = 2;
                    if (((rowState & DataControlRowState.Edit) != 0) && _editItemTemplate != null) {
                        contentTemplate = _editItemTemplate;
                    }
                    if ((rowState & DataControlRowState.Insert) != 0) {
                        if (_insertItemTemplate != null) {
                            contentTemplate = _insertItemTemplate;
                        } else {
                            contentTemplate = _editItemTemplate;
                        }
                    }
                    break;
                case DataControlRowType.Header:
                    contentTemplate = _headerTemplate;
                    contentCell.ColumnSpan = 2;
                    string headerText = HeaderText;
                    if (_headerTemplate == null && headerText.Length > 0) {
                        contentCell.Text = headerText;
                    }
                    break;
                case DataControlRowType.Footer:
                    contentTemplate = _footerTemplate;
                    contentCell.ColumnSpan = 2;
                    string footerText = FooterText;
                    if (_footerTemplate == null && footerText.Length > 0) {
                        contentCell.Text = footerText;
                    }
                    break;
                case DataControlRowType.EmptyDataRow:
                    contentTemplate = _emptyDataTemplate;
                    string emptyDataText = EmptyDataText;
                    if (_emptyDataTemplate == null && emptyDataText.Length > 0) {
                        contentCell.Text = emptyDataText;
                    }
                    break;
            }

            if (contentTemplate != null) {
                contentTemplate.InstantiateIn(contentCell);
            }
            cells.Add(contentCell);
        }

        public virtual void InsertItem(bool causesValidation) {
            ResetModelValidationGroup(causesValidation, String.Empty);
            HandleInsert(String.Empty, causesValidation);
        }


        /// <summary>
        /// This could become obsolete in future versions.
        /// </summary>
        public virtual bool IsBindableType(Type type) {
            // NOTE: No one ever calls this function, but we have to keep it for back compat
            // since it's public.
            return DataBoundControlHelper.IsBindableType(type, enableEnums: this.RenderingCompatibility >= VersionUtil.Framework45);
        }


        /// <devdoc>
        /// <para>Loads the control state for those properties that should persist across postbacks
        ///   even when EnableViewState=false.</para>
        /// </devdoc>
        protected internal override void LoadControlState(object savedState) {
            // Any properties that could have been set in the persistance need to be
            // restored to their defaults if they're not in ControlState, or they will
            // be restored to their persisted state instead of their empty state.
            _pageIndex = 0;
            _defaultMode = FormViewMode.ReadOnly;
            _dataKeyNames = new string[0];
            _pageCount = 0;

            object[] state = savedState as object[];
            if (state != null) {
                base.LoadControlState(state[0]);
                if (state[1] != null) {
                    _pageIndex = (int)state[1];
                }

                if (state[2] != null) {
                    _defaultMode = (FormViewMode)state[2];
                }

                // if Mode isn't saved, it should be restored to DefaultMode.  That will happen in Mode's getter,
                // since the persistance state hasn't been loaded yet.
                if (state[3] != null) {
                    Mode = (FormViewMode)state[3];
                }

                if (state[4] != null) {
                    _dataKeyNames = (string[])state[4];
                }

                if (state[5] != null) {
                    KeyTable.Clear();
                    OrderedDictionaryStateHelper.LoadViewState((OrderedDictionary)KeyTable, (ArrayList)state[5]);
                }

                if (state[6] != null) {
                    _pageCount = (int)state[6];
                }
            } else {
                base.LoadControlState(null);
            }

        }


        /// <devdoc>
        /// <para>Loads a saved state of the <see cref='System.Web.UI.WebControls.FormView'/>.</para>
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                object[] myState = (object[])savedState;

                base.LoadViewState(myState[0]);
                if (myState[1] != null)
                    ((IStateManager)PagerStyle).LoadViewState(myState[1]);
                if (myState[2] != null)
                    ((IStateManager)HeaderStyle).LoadViewState(myState[2]);
                if (myState[3] != null)
                    ((IStateManager)FooterStyle).LoadViewState(myState[3]);
                if (myState[4] != null)
                    ((IStateManager)RowStyle).LoadViewState(myState[4]);
                if (myState[5] != null)
                    ((IStateManager)EditRowStyle).LoadViewState(myState[5]);
                if (myState[6] != null)
                    ((IStateManager)InsertRowStyle).LoadViewState(myState[6]);
                if (myState[7] != null)
                    OrderedDictionaryStateHelper.LoadViewState((OrderedDictionary)BoundFieldValues, (ArrayList)myState[7]);
                if (myState[8] != null)
                    ((IStateManager)PagerSettings).LoadViewState(myState[8]);
                if (myState[9] != null)
                    ((IStateManager)ControlStyle).LoadViewState(myState[9]);
            } else {
                base.LoadViewState(null);
            }
        }

        protected internal virtual string ModifiedOuterTableStylePropertyName() {
            // Verify that table specific and basic style properties are not not set (not different than their defaults).
            if (!String.IsNullOrEmpty(BackImageUrl)) {
                return "BackImageUrl";
            }
            if (CellPadding != -1) {
                return "CellPadding";
            }
            if (CellSpacing != 0) {
                return "CellSpacing";
            }
            if (GridLines != GridLines.None) {
                return "GridLines";
            }
            if (HorizontalAlign != HorizontalAlign.NotSet) {
                return "HorizontalAlign";
            }

            // Font styles.
            if (Font.Bold ||
                Font.Italic ||
                !String.IsNullOrEmpty(Font.Name) ||
                (Font.Names.Length != 0) ||
                Font.Overline ||
                (Font.Size != FontUnit.Empty) ||
                Font.Strikeout ||
                Font.Underline) {
                return "Font";
            }

            return LoginUtil.ModifiedOuterTableBasicStylePropertyName(this);
        }

        /// <devdoc>
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            bool causesValidation = false;
            string validationGroup = String.Empty;

            FormViewCommandEventArgs fvcea = e as FormViewCommandEventArgs;
            if (fvcea != null) {
                IButtonControl button = fvcea.CommandSource as IButtonControl;
                if (button != null) {
                    causesValidation = button.CausesValidation;
                    validationGroup = button.ValidationGroup;
                }
            }
            return HandleEvent(e, causesValidation, validationGroup);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='PageIndexChanged'/>event.</para>
        /// </devdoc>
        protected virtual void OnPageIndexChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventPageIndexChanged];
            if (handler != null) handler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ModeChanging'/> event.</para>
        /// </devdoc>
        protected virtual void OnPageIndexChanging(FormViewPageEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            FormViewPageEventHandler handler = (FormViewPageEventHandler)Events[EventPageIndexChanging];
            if (handler != null) {
                handler(this, e);
            } else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.FormView_UnhandledEvent, ID, "PageIndexChanging"));
                }
            }
        }


        /// <devdoc>
        /// FormView initialization.
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (Page != null) {
                if (DataKeyNames.Length > 0) {
                    Page.RegisterRequiresViewStateEncryption();
                }
                Page.RegisterRequiresControlState(this);
            }

            if (!DesignMode && !String.IsNullOrEmpty(ItemType)) {
                DataBoundControlHelper.EnableDynamicData(this, ItemType);
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ItemCommand'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemCommand(FormViewCommandEventArgs e) {
            FormViewCommandEventHandler handler = (FormViewCommandEventHandler)Events[EventItemCommand];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ItemCreated'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemCreated(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventItemCreated];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ItemDeleted '/>event.</para>
        /// </devdoc>
        protected virtual void OnItemDeleted(FormViewDeletedEventArgs e) {
            FormViewDeletedEventHandler handler = (FormViewDeletedEventHandler)Events[EventItemDeleted];
            if (handler != null) handler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Delete'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemDeleting(FormViewDeleteEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            FormViewDeleteEventHandler handler = (FormViewDeleteEventHandler)Events[EventItemDeleting];
            if (handler != null) {
                handler(this, e);
            } else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.FormView_UnhandledEvent, ID, "ItemDeleting"));
                }
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ItemInserted '/>event.</para>
        /// </devdoc>
        protected virtual void OnItemInserted(FormViewInsertedEventArgs e) {
            FormViewInsertedEventHandler handler = (FormViewInsertedEventHandler)Events[EventItemInserted];
            if (handler != null) handler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Insert'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemInserting(FormViewInsertEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            FormViewInsertEventHandler handler = (FormViewInsertEventHandler)Events[EventItemInserting];
            if (handler != null) {
                handler(this, e);
            } else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.FormView_UnhandledEvent, ID, "ItemInserting"));
                }
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ItemUpdated '/>event.</para>
        /// </devdoc>
        protected virtual void OnItemUpdated(FormViewUpdatedEventArgs e) {
            FormViewUpdatedEventHandler handler = (FormViewUpdatedEventHandler)Events[EventItemUpdated];
            if (handler != null) handler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Update'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemUpdating(FormViewUpdateEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            FormViewUpdateEventHandler handler = (FormViewUpdateEventHandler)Events[EventItemUpdating];
            if (handler != null) {
                handler(this, e);
            } else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.FormView_UnhandledEvent, ID, "ItemUpdating"));
                }
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ModeChanged'/>event.</para>
        /// </devdoc>
        protected virtual void OnModeChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventModeChanged];
            if (handler != null) handler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ModeChanging'/> event.</para>
        /// </devdoc>
        protected virtual void OnModeChanging(FormViewModeEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            FormViewModeEventHandler handler = (FormViewModeEventHandler)Events[EventModeChanging];
            if (handler != null) {
                handler(this, e);
            } else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.FormView_UnhandledEvent, ID, "ModeChanging"));
                }
            }
        }

        private void OnPagerPropertyChanged(object sender, EventArgs e) {
            if (Initialized) {
                RequiresDataBinding = true;
            }
        }

        private bool PageIsValidAfterModelException() {
            if (_modelValidationGroup == null) {
                return true;
            }
            Page.Validate(_modelValidationGroup);
            return Page.IsValid;
        }

        protected internal override void PerformDataBinding(IEnumerable data) {
            base.PerformDataBinding(data);
            if (IsDataBindingAutomatic && Mode == FormViewMode.Edit && IsViewStateEnabled) {
                ExtractRowValues(BoundFieldValues, false/*includeKeys*/);
            }
        }


        /// <devdoc>
        /// </devdoc>
        protected internal virtual void PrepareControlHierarchy() {
            if (Controls.Count < 1) {
                return;
            }

            Debug.Assert(Controls[0] is Table);

            Table childTable = (Table)Controls[0];

            childTable.CopyBaseAttributes(this);
            if (ControlStyleCreated && !ControlStyle.IsEmpty) {
                childTable.ApplyStyle(ControlStyle);
            } else {
                // Since we didn't create a ControlStyle yet, the default
                // settings for the default style of the control need to be applied
                // to the child table control directly
                // 

                childTable.GridLines = GridLines.None;
                childTable.CellSpacing = 0;
            }
            childTable.Caption = Caption;
            childTable.CaptionAlign = CaptionAlign;


            Style compositeStyle;
            TableRowCollection rows = childTable.Rows;

            foreach (FormViewRow row in rows) {
                compositeStyle = new TableItemStyle();
                DataControlRowState rowState = row.RowState;
                DataControlRowType rowType = row.RowType;

                switch (rowType) {
                    case DataControlRowType.Header:
                        compositeStyle = _headerStyle;
                        break;

                    case DataControlRowType.Footer:
                        compositeStyle = _footerStyle;
                        break;

                    case DataControlRowType.DataRow:
                        compositeStyle.CopyFrom(_rowStyle);

                        if ((rowState & DataControlRowState.Edit) != 0) {
                            compositeStyle.CopyFrom(_editRowStyle);
                        }
                        if ((rowState & DataControlRowState.Insert) != 0) {
                            if (_insertRowStyle != null) {
                                compositeStyle.CopyFrom(_insertRowStyle);
                            } else {
                                compositeStyle.CopyFrom(_editRowStyle);
                            }
                        }
                        break;

                    case DataControlRowType.Pager:
                        compositeStyle = _pagerStyle;
                        break;
                    case DataControlRowType.EmptyDataRow:
                        compositeStyle = _emptyDataRowStyle;
                        break;
                }

                if (compositeStyle != null && row.Visible) {
                    row.MergeStyle(compositeStyle);
                }
            }
        }

        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(UniqueID, eventArgument);

            int separatorIndex = eventArgument.IndexOf('$');
            if (separatorIndex < 0) {
                return;
            }

            CommandEventArgs cea = new CommandEventArgs(eventArgument.Substring(0, separatorIndex), eventArgument.Substring(separatorIndex + 1));

            FormViewCommandEventArgs dvcea = new FormViewCommandEventArgs(this, cea);
            HandleEvent(dvcea, false, String.Empty);
        }

        /// <devdoc>
        /// <para>Displays the control on the client.</para>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }
            
            if (RenderOuterTable) {
                PrepareControlHierarchy();
                RenderContents(writer);
            } else {
                string propertyName = ModifiedOuterTableStylePropertyName();
                if (!string.IsNullOrEmpty(propertyName)) {
                    throw new InvalidOperationException(SR.GetString(SR.IRenderOuterTableControl_CannotSetStyleWhenDisableRenderOuterTable,
                        propertyName, GetType().Name, ID));
                }

                if (Controls.Count > 0) {
                    //render the children of the inner table
                    Controls[0].RenderChildren(writer);
                }
            }
        }

        private void ResetModelValidationGroup(bool causesValidation, string validationGroup) {
            _modelValidationGroup = null;
            if (causesValidation && Page != null) {
                Page.Validate(validationGroup);
                if (EnableModelValidation) {
                    _modelValidationGroup = validationGroup;
                }
            }
        }

        /// <devdoc>
        /// <para>Saves the control state for those properties that should persist across postbacks
        ///   even when EnableViewState=false.</para>
        /// </devdoc>
        protected internal override object SaveControlState() {
            object baseState = base.SaveControlState();
            if (baseState != null ||
                _pageIndex != 0 ||
                _mode != _defaultMode ||
                _defaultMode != FormViewMode.ReadOnly ||
                (_dataKeyNames != null && _dataKeyNames.Length > 0) ||
                (_keyTable != null && _keyTable.Count > 0) ||
                _pageCount != 0) {
                object[] state = new object[7];
                object pageIndexState = null;
                object modeState = null;
                object defaultModeState = null;
                object keyNamesState = null;
                object keyTableState = null;
                object pageCountState = null;

                if (_pageIndex != 0) {
                    pageIndexState = _pageIndex;
                }
                if (_defaultMode != FormViewMode.ReadOnly) {
                    defaultModeState = (int)_defaultMode;
                }
                // Only save the mode if it's different from the DefaultMode.  Otherwise, the Mode
                // getter will restore it to the DefaultMode value.
                if (_mode != _defaultMode && _modeSet) {
                    modeState = (int)_mode;
                }

                if (_dataKeyNames != null && _dataKeyNames.Length > 0) {
                    keyNamesState = _dataKeyNames;
                }

                if (_keyTable != null) {
                    keyTableState = OrderedDictionaryStateHelper.SaveViewState(_keyTable);
                }

                if (_pageCount != 0) {
                    pageCountState = _pageCount;
                }

                state[0] = baseState;
                state[1] = pageIndexState;
                state[2] = defaultModeState;
                state[3] = modeState;
                state[4] = keyNamesState;
                state[5] = keyTableState;
                state[6] = pageCountState;

                return state;
            }
            return true;    // return a dummy that ensures LoadControlState gets called but minimizes persisted size.
        }


        /// <devdoc>
        /// <para>Saves the current state of the <see cref='System.Web.UI.WebControls.FormView'/>.</para>
        /// </devdoc>
        protected override object SaveViewState() {
            object baseState = base.SaveViewState();
            object pagerStyleState = (_pagerStyle != null) ? ((IStateManager)_pagerStyle).SaveViewState() : null;
            object headerStyleState = (_headerStyle != null) ? ((IStateManager)_headerStyle).SaveViewState() : null;
            object footerStyleState = (_footerStyle != null) ? ((IStateManager)_footerStyle).SaveViewState() : null;
            object rowStyleState = (_rowStyle != null) ? ((IStateManager)_rowStyle).SaveViewState() : null;
            object editRowStyleState = (_editRowStyle != null) ? ((IStateManager)_editRowStyle).SaveViewState() : null;
            object insertRowStyleState = (_insertRowStyle != null) ? ((IStateManager)_insertRowStyle).SaveViewState() : null;
            object boundFieldValuesState = (_boundFieldValues != null) ? OrderedDictionaryStateHelper.SaveViewState(_boundFieldValues) : null;
            object pagerSettingsState = (_pagerSettings != null) ? ((IStateManager)_pagerSettings).SaveViewState() : null;
            object controlState = ControlStyleCreated ? ((IStateManager)ControlStyle).SaveViewState() : null;

            object[] myState = new object[10];
            myState[0] = baseState;
            myState[1] = pagerStyleState;
            myState[2] = headerStyleState;
            myState[3] = footerStyleState;
            myState[4] = rowStyleState;
            myState[5] = editRowStyleState;
            myState[6] = insertRowStyleState;
            myState[7] = boundFieldValuesState;
            myState[8] = pagerSettingsState;
            myState[9] = controlState;

            // note that we always have some state, atleast the RowCount
            return myState;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", 
          Justification = "A property already exists. This method does additional work.")]
        public void SetPageIndex(int index) {
            HandlePage(index);
        }

        private void SelectCallback(IEnumerable data) {
            // The data source should have thrown.  If we're here, it didn't.  We'll throw for it
            // with a generic message.
            throw new HttpException(SR.GetString(SR.DataBoundControl_DataSourceDoesntSupportPaging));
        }

        /// <devdoc>
        /// <para>Marks the starting point to begin tracking and saving changes to the
        /// control as part of the control viewstate.</para>
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (_pagerStyle != null)
                ((IStateManager)_pagerStyle).TrackViewState();
            if (_headerStyle != null)
                ((IStateManager)_headerStyle).TrackViewState();
            if (_footerStyle != null)
                ((IStateManager)_footerStyle).TrackViewState();
            if (_rowStyle != null)
                ((IStateManager)_rowStyle).TrackViewState();
            if (_editRowStyle != null)
                ((IStateManager)_editRowStyle).TrackViewState();
            if (_insertRowStyle != null)
                ((IStateManager)_insertRowStyle).TrackViewState();
            if (_pagerSettings != null)
                ((IStateManager)_pagerSettings).TrackViewState();
            if (ControlStyleCreated)
                ((IStateManager)ControlStyle).TrackViewState();
        }

        public virtual void UpdateItem(bool causesValidation) {
            ResetModelValidationGroup(causesValidation, String.Empty);
            HandleUpdate(String.Empty, causesValidation);
        }

        internal override void UpdateModelDataSourceProperties(ModelDataSource modelDataSource) {
            Debug.Assert(modelDataSource != null, "A non-null ModelDataSource should be passed in");
            string dataKeyName = DataKeyNamesInternal.Length > 0 ? DataKeyNamesInternal[0] : "";
            modelDataSource.UpdateProperties(ItemType, SelectMethod, UpdateMethod, InsertMethod, DeleteMethod, dataKeyName);
        }

        #region IPostBackContainer implementation
        PostBackOptions IPostBackContainer.GetPostBackOptions(IButtonControl buttonControl) {
            if (buttonControl == null) {
                throw new ArgumentNullException("buttonControl");
            }

            if (buttonControl.CausesValidation) {
                throw new InvalidOperationException(SR.GetString(SR.CannotUseParentPostBackWhenValidating, this.GetType().Name, ID));
            }

            PostBackOptions options = new PostBackOptions(this, buttonControl.CommandName + "$" + buttonControl.CommandArgument);
            options.RequiresJavaScriptProtocol = true;

            return options;
        }
        #endregion

        #region IPostBackEventHandler implementation
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion

        #region IDataItemContainer implementation
        int IDataItemContainer.DataItemIndex {
            get {
                return DataItemIndex;
            }
        }

        int IDataItemContainer.DisplayIndex {
            get {
                return 0;
            }
        }
        #endregion

        #region IDataBoundItemControl implementation

        DataKey IDataBoundItemControl.DataKey {
            get {
                return DataKey;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "The property is only used to genericly access the databound control's mode, and should only be accessed through the interface")]
        DataBoundControlMode IDataBoundItemControl.Mode {
            get {
                switch (Mode) {
                    case FormViewMode.Edit:
                        return DataBoundControlMode.Edit;
                    case FormViewMode.Insert:
                        return DataBoundControlMode.Insert;
                    case FormViewMode.ReadOnly:
                        return DataBoundControlMode.ReadOnly;
                    default:
                        Debug.Fail("shouldn't get here!");
                        return DataBoundControlMode.ReadOnly;
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "The property is accessible through the DataBoundControl")]
        string IDataBoundControl.DataSourceID {
            get {
                return DataSourceID;
            }
            set {
                DataSourceID = value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "The property is accessible through the DataBoundControl")]
        IDataSource IDataBoundControl.DataSourceObject {
            get {
                return DataSourceObject;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "The property is accessible through the DataBoundControl")]
        object IDataBoundControl.DataSource {
            get {
                return DataSource;
            }
            set {
                DataSource = value;
            }
        }

        string[] IDataBoundControl.DataKeyNames {
            get {
                return DataKeyNames;
            }
            set {
                DataKeyNames = value;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "The property is accessible through the DataBoundControl")]
        string IDataBoundControl.DataMember {
            get {
                return DataMember;
            }
            set {
                DataMember = value;
            }
        }

        #endregion
    }
}
