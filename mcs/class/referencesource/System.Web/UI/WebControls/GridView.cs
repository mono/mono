//------------------------------------------------------------------------------
// <copyright file="GridView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;   //for NameValueCollection
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Web.Security.Cryptography;
    using System.Web.UI.Adapters;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>
    ///       Displays data from a data source in a tabular grid. The data source
    ///       is any object that implements IEnumerable, which includes ADO.NET data,
    ///       arrays, ArrayLists, DataSourceControl, etc.
    ///    </para>
    /// </devdoc>
    [
        //Editor("System.Web.UI.Design.WebControls.GridViewComponentEditor, " + AssemblyRef.SystemDesign, typeof(ComponentEditor)),
    Designer("System.Web.UI.Design.WebControls.GridViewDesigner, " + AssemblyRef.SystemDesign),
    ControlValueProperty("SelectedValue"),
    DefaultEvent("SelectedIndexChanged"),
    SupportsEventValidation
    ]
    [DataKeyProperty("SelectedPersistedDataKey")]
    public class GridView : CompositeDataBoundControl, IPostBackContainer, IPostBackEventHandler, ICallbackContainer,
                ICallbackEventHandler, IPersistedSelector, IDataKeysControl, IDataBoundListControl, IFieldControl {

        private static readonly object EventPageIndexChanging = new object();
        private static readonly object EventPageIndexChanged = new object();
        private static readonly object EventRowCancelingEdit = new object();
        private static readonly object EventRowCommand = new object();
        private static readonly object EventRowCreated = new object();
        private static readonly object EventRowDataBound = new object();
        private static readonly object EventRowDeleted = new object();
        private static readonly object EventRowDeleting = new object();
        private static readonly object EventRowEditing = new object();
        private static readonly object EventRowUpdated = new object();
        private static readonly object EventRowUpdating = new object();
        private static readonly object EventSelectedIndexChanging = new object();
        private static readonly object EventSelectedIndexChanged = new object();
        private static readonly object EventSorted = new object();
        private static readonly object EventSorting = new object();


        private IEnumerator _storedData;
        private object _firstDataRow;
        private bool _storedDataValid;
        private int _pageCount = -1;

        private DataControlFieldCollection _fieldCollection;

        private TableItemStyle _headerStyle;
        private TableItemStyle _footerStyle;
        private TableItemStyle _rowStyle;
        private TableItemStyle _alternatingRowStyle;
        private TableItemStyle _selectedRowStyle;
        private TableItemStyle _editRowStyle;
        private TableItemStyle _emptyDataRowStyle;
        private TableItemStyle _pagerStyle;
        private TableItemStyle _sortedAscendingCellStyle;
        private TableItemStyle _sortedDescendingCellStyle;
        private TableItemStyle _sortedAscendingHeaderStyle;
        private TableItemStyle _sortedDescendingHeaderStyle;

        private PagerSettings _pagerSettings;

        private ITemplate _pagerTemplate;
        private ITemplate _emptyDataTemplate;

        private GridViewRow _bottomPagerRow;
        private GridViewRow _footerRow;
        private GridViewRow _headerRow;
        private GridViewRow _topPagerRow;

        private ArrayList _rowsArray;
        private GridViewRowCollection _rowsCollection;

        private DataKeyArray _dataKeyArray;
        private ArrayList _dataKeysArrayList;
        private DataKeyArray _clientIDRowSuffixArray;
        private ArrayList _clientIDRowSuffixArrayList;
        private OrderedDictionary _boundFieldValues;
        private string[] _dataKeyNames;
        private string[] _clientIDRowSuffix;
        private DataKey _persistedDataKey;

        private int _editIndex = -1;
        private int _selectedIndex = -1;
        private int _pageIndex;
        private string _sortExpression = String.Empty;
        private SortDirection _sortDirection = SortDirection.Ascending;
        private string _sortExpressionSerialized;
        private string _modelValidationGroup;

        private IAutoFieldGenerator _columnsGenerator;
        private GridViewColumnsGenerator _defaultColumnsGenerator = new GridViewColumnsGenerator();

        private IOrderedDictionary _updateKeys;
        private IOrderedDictionary _updateOldValues;
        private IOrderedDictionary _updateNewValues;
        private IOrderedDictionary _deleteKeys;
        private IOrderedDictionary _deleteValues;
        private int _deletedRowIndex;

        private bool _renderClientScript;
        private bool _renderClientScriptValid = false;

        IStateFormatter2 _stateFormatter;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.GridView'/> class.
        ///    </para>
        /// </devdoc>
        public GridView() {
        }

        internal GridView(IStateFormatter2 stateFormatter) {
            _stateFormatter = stateFormatter;
        }


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

        /// <devdoc>
        ///    <para>Gets or sets a value that indicates whether custom paging is allowed.</para>
        /// </devdoc>
        [
        WebCategory("Paging"),
        DefaultValue(false),
        WebSysDescription(SR.GridView_AllowCustomPaging)
        ]
        public virtual bool AllowCustomPaging {
            get {
                object o = ViewState["AllowCustomPaging"];
                if (o != null)
                    return (bool)o;
                return false;
            }
            set {
                ViewState["AllowCustomPaging"] = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value that indicates whether paging is allowed.</para>
        /// </devdoc>
        [
        WebCategory("Paging"),
        DefaultValue(false),
        WebSysDescription(SR.GridView_AllowPaging)
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
        ///    <para>Gets or sets a value that indicates whether sorting is allowed.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.GridView_AllowSorting)
        ]
        public virtual bool AllowSorting {
            get {
                object o = ViewState["AllowSorting"];
                if (o != null)
                    return (bool)o;
                return false;
            }
            set {
                bool oldValue = AllowSorting;
                if (value != oldValue) {
                    ViewState["AllowSorting"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets the style properties for alternating rows in the
        ///    <see cref='System.Web.UI.WebControls.GridView'/>. This
        ///       property is read-only. </para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.GridView_AlternatingRowStyle)
        ]
        public TableItemStyle AlternatingRowStyle {
            get {
                if (_alternatingRowStyle == null) {
                    _alternatingRowStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_alternatingRowStyle).TrackViewState();
                }
                return _alternatingRowStyle;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value that indicates whether a delete field will automatically
        ///       be created.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.GridView_AutoGenerateDeleteButton)
        ]
        public virtual bool AutoGenerateDeleteButton {
            get {
                object o = ViewState["AutoGenerateDeleteButton"];
                if (o != null)
                    return (bool)o;
                return false;
            }
            set {
                bool oldValue = AutoGenerateDeleteButton;
                if (value != oldValue) {
                    ViewState["AutoGenerateDeleteButton"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value that indicates whether an edit field will automatically
        ///       be created.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.GridView_AutoGenerateEditButton)
        ]
        public virtual bool AutoGenerateEditButton {
            get {
                object o = ViewState["AutoGenerateEditButton"];
                if (o != null)
                    return (bool)o;
                return false;
            }
            set {
                bool oldValue = AutoGenerateEditButton;
                if (value != oldValue) {
                    ViewState["AutoGenerateEditButton"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value that indicates whether a select button will automatically
        ///       be created.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.GridView_AutoGenerateSelectButton)
        ]
        public virtual bool AutoGenerateSelectButton {
            get {
                object o = ViewState["AutoGenerateSelectButton"];
                if (o != null)
                    return (bool)o;
                return false;
            }
            set {
                bool oldValue = AutoGenerateSelectButton;
                if (value != oldValue) {
                    ViewState["AutoGenerateSelectButton"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value that indicates whether fields will automatically
        ///       be created for each bound data field.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.DataControls_AutoGenerateColumns)
        ]
        public virtual bool AutoGenerateColumns {
            get {
                object o = ViewState["AutoGenerateColumns"];
                if (o != null)
                    return (bool)o;
                return true;
            }
            set {
                bool oldValue = AutoGenerateColumns;
                if (value != oldValue) {
                    ViewState["AutoGenerateColumns"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the URL of an image to display in the
        ///       background of the <see cref='System.Web.UI.WebControls.GridView'/>.</para>
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
        public virtual GridViewRow BottomPagerRow {
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
                    int capacity = Columns.Count;
                    if (AutoGenerateColumns) {
                        capacity += 10;
                    }
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
        ///    <para>Indicates the amount of space between cells.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(-1),
        WebSysDescription(SR.GridView_CellPadding)
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
        ///    <para>Gets or sets the amount of space between the contents of
        ///       a cell and the cell's border.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(0),
        WebSysDescription(SR.GridView_CellSpacing)
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

        /// <devdoc>
        /// <para>Gets a collection of <see cref='System.Web.UI.WebControls.DataControlField'/> controls in the <see cref='System.Web.UI.WebControls.GridView'/>. This property is read-only.</para>
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.DataControlFieldTypeEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Default"),
        WebSysDescription(SR.DataControls_Columns)
        ]
        public virtual DataControlFieldCollection Columns {
            get {
                if (_fieldCollection == null) {
                    _fieldCollection = new DataControlFieldCollection();
                    _fieldCollection.FieldsChanged += new EventHandler(OnFieldsChanged);
                    if (IsTrackingViewState)
                        ((IStateManager)_fieldCollection).TrackViewState();
                }
                return _fieldCollection;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public IAutoFieldGenerator ColumnsGenerator {
            get {
                return _columnsGenerator;
            }
            set {
                _columnsGenerator = value;
            }
        }

        private IAutoFieldGenerator ColumnsGeneratorInternal {
            get {
                return ColumnsGenerator ?? _defaultColumnsGenerator;
            }
        }

        /// <devdoc>
        /// An array of ordered dictionaries that represents each key
        /// </devdoc>
        private ArrayList DataKeysArrayList {
            get {
                if (_dataKeysArrayList == null) {
                    _dataKeysArrayList = new ArrayList();
                }
                return _dataKeysArrayList;
            }
        }

        private ArrayList ClientIDRowSuffixArrayList {
            get {
                if (_clientIDRowSuffixArrayList == null) {
                    _clientIDRowSuffixArrayList = new ArrayList();
                }
                return _clientIDRowSuffixArrayList;
            }
        }

        /// <devdoc>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.GridView_DataKeys)
        ]
        public virtual DataKeyArray DataKeys {
            get {
                if (_dataKeyArray == null) {
                    _dataKeyArray = new DataKeyArray(this.DataKeysArrayList);
                    if (IsTrackingViewState)
                        ((IStateManager)_dataKeyArray).TrackViewState();
                }
                return _dataKeyArray;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public DataKeyArray ClientIDRowSuffixDataKeys {
            get {
                if (_clientIDRowSuffixArray == null) {
                    _clientIDRowSuffixArray = new DataKeyArray(this.ClientIDRowSuffixArrayList);
                }
                return _clientIDRowSuffixArray;
            }
        }

        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.DataFieldEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        TypeConverterAttribute(typeof(StringArrayConverter)),
        WebCategory("Data"),
        WebSysDescription(SR.DataControls_DataKeyNames),
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
                    }
                    else {
                        _dataKeyNames = null;
                    }

                    ClearDataKeys();
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
        ///    <para>Gets or sets the ordinal index of the row to be edited.</para>
        /// </devdoc>
        [
        WebCategory("Default"),
        DefaultValue(-1),
        WebSysDescription(SR.GridView_EditIndex)
        ]
        public virtual int EditIndex {
            get {
                return _editIndex;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }

                int oldValue = EditIndex;
                if (oldValue != value) {
                    if (value == -1) {
                        BoundFieldValues.Clear();
                    }
                    _editIndex = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets the style properties of the row to be edited. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.GridView_EditRowStyle)
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
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.GridView_EnablePersistedSelection)
        ]
        public virtual bool EnablePersistedSelection {
            get {
                object o = ViewState["EnablePersistedSelection"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                ViewState["EnablePersistedSelection"] = value;
            }
        }

        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.GridView_EnableSortingAndPagingCallbacks)
        ]
        public virtual bool EnableSortingAndPagingCallbacks {
            get {
                object o = ViewState["EnableSortingAndPagingCallbacks"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                ViewState["EnableSortingAndPagingCallbacks"] = value;
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

        /// <devdoc>
        /// <para>Gets the style properties for null rows in the
        /// <see cref='System.Web.UI.WebControls.GridView'/>. This
        /// property is read-only. </para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.GridView_EmptyDataRowStyle)
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
        /// <para>Indicates the template to use when no records are returned from the datasource within the GridView.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(GridViewRow)),
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
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual GridViewRow FooterRow {
            get {
                if (_footerRow == null) {
                    EnsureChildControls();
                }
                return _footerRow;
            }
        }

        /// <devdoc>
        ///    <para>Gets the style properties of the footer row. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DataControls_FooterStyle),
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
        ///    <para>Gets or sets a value that specifies the grid line style.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(GridLines.Both),
        WebSysDescription(SR.DataControls_GridLines)
        ]
        public virtual GridLines GridLines {
            get {
                if (ControlStyleCreated == false) {
                    return GridLines.Both;
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
        public virtual GridViewRow HeaderRow {
            get {
                if (_headerRow == null) {
                    EnsureChildControls();
                }
                return _headerRow;
            }
        }

        /// <devdoc>
        ///    <para>Gets the style properties of the header row. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DataControls_HeaderStyle)
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
        ///    <para>Gets or sets a value that specifies the alignment of a rows with respect
        ///       surrounding text.</para>
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

        /// <summary>
        /// Determines if the specified data type can be bound to.
        /// Note : Staring from 4.5, This method is no more being used as a criteria for generating rows when AutoGenerateColumns=true.
        /// This could become obsolete in future versions.
        /// </summary>
        public virtual bool IsBindableType(Type type) {
            return DataBoundControlHelper.IsBindableType(type, enableEnums: RenderingCompatibility >= VersionUtil.Framework45);
        }

        /// <devdoc>
        ///    <para>Gets the total number of pages to be displayed. This property is read-only.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.GridView_PageCount)
        ]
        public virtual int PageCount {
            get {
                if (_pageCount < 0) {
                    // If someone reads this value before it is initialized we just return 0
                    return 0;
                }
                return _pageCount;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the ordinal index of the currently displayed page. </para>
        /// </devdoc>
        [
        Browsable(true),
        DefaultValue(0),
        WebCategory("Paging"),
        WebSysDescription(SR.GridView_PageIndex)
        ]
        public virtual int PageIndex {
            get {
                return _pageIndex;
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }

                int oldValue = PageIndex;
                if (oldValue != value) {
                    _pageIndex = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        /// <devdoc>
        /// <para>Gets the settings of the pager buttons for the
        /// <see cref='System.Web.UI.WebControls.GridView'/>. This
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
        ///    <para>Gets the style properties of the pager rows for the
        ///    <see cref='System.Web.UI.WebControls.GridView'/>. This
        ///       property is read-only.</para>
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
        /// <para>Indicates the template to use for a pager item within the GridView.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(GridViewRow)),
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
        ///    <para>Gets or sets the number of rows to display on a single page.</para>
        /// </devdoc>
        [
        WebCategory("Paging"),
        DefaultValue(10),
        WebSysDescription(SR.GridView_PageSize),
        ]
        public virtual int PageSize {
            get {
                object o = ViewState["PageSize"];
                if (o != null)
                    return (int)o;
                return 10;
            }
            set {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                int oldValue = PageSize;
                if (oldValue != value) {
                    ViewState["PageSize"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
        [
        DefaultValue(null),
        TypeConverterAttribute(typeof(StringArrayConverter)),
        WebCategory("Data"),
        ]
        public virtual string[] ClientIDRowSuffix {
            get {
                object o = _clientIDRowSuffix;
                if (o != null) {
                    return (string[])((string[])o).Clone();
                }
                return new string[0];
            }
            set {
                if (!DataBoundControlHelper.CompareStringArrays(value, ClientIDRowSuffixInternal)) {
                    if (value != null) {
                        _clientIDRowSuffix = (string[])value.Clone();
                    }
                    else {
                        _clientIDRowSuffix = null;
                    }
                    _clientIDRowSuffixArrayList = null;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        private string[] ClientIDRowSuffixInternal {
            get {
                object o = _clientIDRowSuffix;
                if (o != null) {
                    return (string[])o;
                }
                return new string[0];
            }
        }

        /// <devdoc>
        /// <para>The column to be used as the scope=row header.</para>
        /// </devdoc>
        [
        DefaultValue(""),
        TypeConverterAttribute("System.Web.UI.Design.DataColumnSelectionConverter, " + AssemblyRef.SystemDesign),
        WebCategory("Accessibility"),
        WebSysDescription(SR.GridView_RowHeaderColumn)
        ]
        public virtual string RowHeaderColumn {
            get {
                object o = ViewState["RowHeaderColumn"];
                return o == null ? String.Empty : (string)o;
            }
            set {
                ViewState["RowHeaderColumn"] = value;
            }
        }

        /// <devdoc>
        /// <para>Gets a collection of <see cref='System.Web.UI.WebControls.GridViewRow'/> objects representing the individual
        ///    rows within the control.
        ///    This property is read-only.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.GridView_Rows)
        ]
        public virtual GridViewRowCollection Rows {
            get {
                if (_rowsCollection == null) {
                    if (_rowsArray == null) {
                        EnsureChildControls();
                    }
                    if (_rowsArray == null) {
                        _rowsArray = new ArrayList();
                    }
                    _rowsCollection = new GridViewRowCollection(_rowsArray);
                }
                return _rowsCollection;
            }
        }

        /// <devdoc>
        ///    <para>Gets the style properties of the individual rows. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.View_RowStyle),
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
        public virtual DataKey SelectedDataKey {
            get {
                if (DataKeyNamesInternal == null || DataKeyNamesInternal.Length == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.GridView_DataKeyNamesMustBeSpecified, ID));
                }

                DataKeyArray keys = DataKeys;
                int selectedIndex = SelectedIndex;
                if (keys != null && selectedIndex < keys.Count && selectedIndex > -1) {
                    return keys[selectedIndex];
                }
                return null;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the index of the currently selected row.</para>
        /// </devdoc>
        [
        Bindable(true),
        DefaultValue(-1),
        WebSysDescription(SR.GridView_SelectedIndex)
        ]
        public virtual int SelectedIndex {
            get {
                return _selectedIndex;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                int oldSelectedIndex = _selectedIndex;
                _selectedIndex = value;

                if ((DataKeyNamesInternal.Length > 0) && EnablePersistedSelection) {
                    // update the virtual selection to use the new selection
                    SelectedPersistedDataKey = SelectedDataKey;
                }

                if (_rowsArray != null) {
                    GridViewRow row;

                    if ((oldSelectedIndex != -1) && (_rowsArray.Count > oldSelectedIndex)) {
                        row = (GridViewRow)_rowsArray[oldSelectedIndex];
                        row.RowType = DataControlRowType.DataRow;
                        row.RowState &= ~DataControlRowState.Selected;  // turn off selected bit
                    }
                    if ((value != -1) && (_rowsArray.Count > value)) {
                        row = (GridViewRow)_rowsArray[value];
                        row.RowState |= DataControlRowState.Selected;   // turn on selected bit
                    }
                }
            }
        }

        [
        Browsable(false)
        ]
        public object SelectedValue {
            get {
                DataKey selectedDataKey = SelectedDataKey;
                if (selectedDataKey != null) {
                    return SelectedDataKey.Value;
                }
                return null;
            }
        }

        /// <devdoc>
        /// <para>Gets the selected row in the <see cref='System.Web.UI.WebControls.GridView'/>. This property is read-only.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.GridView_SelectedRow)
        ]
        public virtual GridViewRow SelectedRow {
            get {
                int index = SelectedIndex;
                GridViewRow row = null;

                if (index != -1) {
                    row = Rows[index];
                }
                return row;
            }
        }

        /// <devdoc>
        ///    <para>Gets the style properties of the currently selected row. This property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.GridView_SelectedRowStyle)
        ]
        public TableItemStyle SelectedRowStyle {
            get {
                if (_selectedRowStyle == null) {
                    _selectedRowStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_selectedRowStyle).TrackViewState();
                }
                return _selectedRowStyle;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value that specifies whether the footer is displayed in the
        ///    <see cref='System.Web.UI.WebControls.GridView'/>.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(false),
        WebSysDescription(SR.DataControls_ShowFooter)
        ]
        public virtual bool ShowFooter {
            get {
                object o = ViewState["ShowFooter"];
                if (o != null)
                    return (bool)o;
                return false;
            }
            set {
                bool oldValue = ShowFooter;
                if (value != oldValue) {
                    ViewState["ShowFooter"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value that specifies whether the header is displayed in the
        ///    <see cref='System.Web.UI.WebControls.GridView'/>.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(true),
        WebSysDescription(SR.DataControls_ShowHeader)
        ]
        public virtual bool ShowHeader {
            get {
                object o = ViewState["ShowHeader"];
                if (o != null)
                    return (bool)o;
                return true;
            }
            set {
                bool oldValue = ShowHeader;
                if (value != oldValue) {
                    ViewState["ShowHeader"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        [
        WebCategory("Appearance"),
        DefaultValue(false),
        WebSysDescription(SR.GridView_ShowHeaderWhenEmpty)
        ]
        public virtual bool ShowHeaderWhenEmpty {
            get {
                object o = ViewState["ShowHeaderWhenEmpty"];
                if (o != null)
                    return (bool)o;
                return false;
            }
            set {
                bool oldValue = ShowHeaderWhenEmpty;
                if (value != oldValue) {
                    ViewState["ShowHeaderWhenEmpty"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        /// <summary>
        /// Indicates the direction of the sort for the current column in the
        /// <see cref='System.Web.UI.WebControls.GridView'/>.
        /// </summary>
        [
        Browsable(false),
        DefaultValue(SortDirection.Ascending),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.GridView_SortDirection)
        ]
        public virtual SortDirection SortDirection {
            get {
                return SortDirectionInternal;
            }
        }

        /// <summary>
        ///    Internal member for setting sort direction
        /// </summary>
        private SortDirection SortDirectionInternal {
            get {
                return _sortDirection;
            }
            set {
                if (value < SortDirection.Ascending || value > SortDirection.Descending) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (_sortDirection != value) {
                    _sortDirection = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value that specifies the current column being sorted on in the
        /// <see cref='System.Web.UI.WebControls.GridView'/>.
        /// </summary>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.GridView_SortExpression)
        ]
        public virtual string SortExpression {
            get {
                return SortExpressionInternal;
            }
        }

        /// <summary>
        ///    Internal member for setting sort expression
        /// </summary>
        private string SortExpressionInternal {
            get {
                return _sortExpression;
            }
            set {
                if (_sortExpression != value) {
                    _sortExpression = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }


        [
        WebCategory("Styles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.GridView_SortedAscendingCellStyle)
        ]
        public TableItemStyle SortedAscendingCellStyle {
            get {
                if (_sortedAscendingCellStyle == null) {
                    _sortedAscendingCellStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_sortedAscendingCellStyle).TrackViewState();
                }
                return _sortedAscendingCellStyle;
            }
        }

        [
        WebCategory("Styles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.GridView_SortedAscendingHeaderStyle)
        ]
        public TableItemStyle SortedAscendingHeaderStyle {
            get {
                if (_sortedAscendingHeaderStyle == null) {
                    _sortedAscendingHeaderStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_sortedAscendingHeaderStyle).TrackViewState();
                }
                return _sortedAscendingHeaderStyle;
            }
        }

        [
        WebCategory("Styles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.GridView_SortedDescendingCellStyle)
        ]
        public TableItemStyle SortedDescendingCellStyle {
            get {
                if (_sortedDescendingCellStyle == null) {
                    _sortedDescendingCellStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_sortedDescendingCellStyle).TrackViewState();
                }
                return _sortedDescendingCellStyle;
            }
        }

        [
        WebCategory("Styles"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.GridView_SortedDescendingHeaderStyle)
        ]
        public TableItemStyle SortedDescendingHeaderStyle {
            get {
                if (_sortedDescendingHeaderStyle == null) {
                    _sortedDescendingHeaderStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_sortedDescendingHeaderStyle).TrackViewState();
                }
                return _sortedDescendingHeaderStyle;
            }
        }


        private IStateFormatter2 StateFormatter {
            get {
                if (_stateFormatter == null) {
                    _stateFormatter = Page.CreateStateFormatter();
                }
                return _stateFormatter;
            }
        }

        protected override HtmlTextWriterTag TagKey {
            get {
                return EnableSortingAndPagingCallbacks ?
                    HtmlTextWriterTag.Div : HtmlTextWriterTag.Table;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual GridViewRow TopPagerRow {
            get {
                if (_topPagerRow == null) {
                    EnsureChildControls();
                }
                return _topPagerRow;
            }
        }

        [
        DefaultValue(true),
        WebCategory("Accessibility"),
        WebSysDescription(SR.Table_UseAccessibleHeader)
        ]
        public virtual bool UseAccessibleHeader {
            get {
                object o = ViewState["UseAccessibleHeader"];
                if (o != null)
                    return (bool)o;
                return true;
            }
            set {
                bool oldValue = UseAccessibleHeader;
                if (oldValue != value) {
                    ViewState["UseAccessibleHeader"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }


        /// <devdoc>
        ///    Gets or sets the number of rows to display in the
        /// <see cref='System.Web.UI.WebControls.DataGrid'/>.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.GridView_VirtualItemCount)
        ]
        public virtual int VirtualItemCount {
            get {
                object o = ViewState["VirtualItemCount"];
                if (o != null)
                    return (int)o;
                return 0;
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["VirtualItemCount"] = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual DataKey SelectedPersistedDataKey {
            get {
                return _persistedDataKey;
            }
            set {
                _persistedDataKey = value;
                if (IsTrackingViewState && (_persistedDataKey != null)) {
                    ((IStateManager)_persistedDataKey).TrackViewState();
                }
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.GridView'/> with a
        /// <see langword='delete'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnRowDeleted)
        ]
        public event GridViewDeletedEventHandler RowDeleted {
            add {
                Events.AddHandler(EventRowDeleted, value);
            }
            remove {
                Events.RemoveHandler(EventRowDeleted, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.GridView'/> with a
        /// <see langword='update'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemUpdated)
        ]
        public event GridViewUpdatedEventHandler RowUpdated {
            add {
                Events.AddHandler(EventRowUpdated, value);
            }
            remove {
                Events.RemoveHandler(EventRowUpdated, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.GridView'/> with a
        /// <see langword='Command'/> property of
        /// <see langword='cancel'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.GridView_OnRowCancelingEdit)
        ]
        public event GridViewCancelEditEventHandler RowCancelingEdit {
            add {
                Events.AddHandler(EventRowCancelingEdit, value);
            }
            remove {
                Events.RemoveHandler(EventRowCancelingEdit, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.GridView'/> with a
        /// <see langword='delete'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemDeleting)
        ]
        public event GridViewDeleteEventHandler RowDeleting {
            add {
                Events.AddHandler(EventRowDeleting, value);
            }
            remove {
                Events.RemoveHandler(EventRowDeleting, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.GridView'/> with a
        /// <see langword='Command'/> property of
        /// <see langword='edit'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.GridView_OnRowEditing)
        ]
        public event GridViewEditEventHandler RowEditing {
            add {
                Events.AddHandler(EventRowEditing, value);
            }
            remove {
                Events.RemoveHandler(EventRowEditing, value);
            }
        }

        /// <devdoc>
        ///    <para>Occurs the one of the pager buttons is clicked.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.GridView_OnPageIndexChanged)
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
        ///    <para>Occurs the one of the pager buttons is clicked.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.GridView_OnPageIndexChanging)
        ]
        public event GridViewPageEventHandler PageIndexChanging {
            add {
                Events.AddHandler(EventPageIndexChanging, value);
            }
            remove {
                Events.RemoveHandler(EventPageIndexChanging, value);
            }
        }

        /// <devdoc>
        ///    <para>Occurs when an row on the list is selected.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.GridView_OnSelectedIndexChanged)
        ]
        public event EventHandler SelectedIndexChanged {
            add {
                Events.AddHandler(EventSelectedIndexChanged, value);
            }
            remove {
                Events.RemoveHandler(EventSelectedIndexChanged, value);
            }
        }

        /// <devdoc>
        ///    <para>Occurs when an row on the list is selected.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.GridView_OnSelectedIndexChanging)
        ]
        public event GridViewSelectEventHandler SelectedIndexChanging {
            add {
                Events.AddHandler(EventSelectedIndexChanging, value);
            }
            remove {
                Events.RemoveHandler(EventSelectedIndexChanging, value);
            }
        }

        /// <devdoc>
        ///    <para>Occurs when a field is sorted.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.GridView_OnSorted)
        ]
        public event EventHandler Sorted {
            add {
                Events.AddHandler(EventSorted, value);
            }
            remove {
                Events.RemoveHandler(EventSorted, value);
            }
        }

        /// <devdoc>
        ///    <para>Occurs when a field is sorting.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.GridView_OnSorting)
        ]
        public event GridViewSortEventHandler Sorting {
            add {
                Events.AddHandler(EventSorting, value);
            }
            remove {
                Events.RemoveHandler(EventSorting, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.GridView'/> with a
        /// <see langword='update'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemUpdating)
        ]
        public event GridViewUpdateEventHandler RowUpdating {
            add {
                Events.AddHandler(EventRowUpdating, value);
            }
            remove {
                Events.RemoveHandler(EventRowUpdating, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.GridView'/> not covered by
        /// <see langword='edit'/>, <see langword='cancel'/>, <see langword='delete'/> or
        /// <see langword='update'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.GridView_OnRowCommand)
        ]
        public event GridViewCommandEventHandler RowCommand {
            add {
                Events.AddHandler(EventRowCommand, value);
            }
            remove {
                Events.RemoveHandler(EventRowCommand, value);
            }
        }


        /// <devdoc>
        ///    <para>Occurs on the server when a control a created.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        WebSysDescription(SR.GridView_OnRowCreated)
        ]
        public event GridViewRowEventHandler RowCreated {
            add {
                Events.AddHandler(EventRowCreated, value);
            }
            remove {
                Events.RemoveHandler(EventRowCreated, value);
            }
        }

        /// <devdoc>
        ///    <para>Occurs when an row is data bound to the control.</para>
        /// </devdoc>
        [
        WebCategory("Data"),
        WebSysDescription(SR.GridView_OnRowDataBound)
        ]
        public event GridViewRowEventHandler RowDataBound {
            add {
                Events.AddHandler(EventRowDataBound, value);
            }
            remove {
                Events.RemoveHandler(EventRowDataBound, value);
            }
        }

        /// <internalonly/>
        /// <devdoc>
        ///  Caches the fact that we have already consumed the first row from the enumeration
        ///  and must use it first during our row creation.
        /// </devdoc>
        internal void StoreEnumerator(IEnumerator dataSource, object firstDataRow) {
            this._storedData = dataSource;
            this._firstDataRow = firstDataRow;
            this._storedDataValid = true;
        }

        /// <devdoc>
        /// <para>Builds the callback argument used in DataControlLinkButtons.</para>
        /// </devdoc>
        private string BuildCallbackArgument(string sortExpression, SortDirection sortDirection) {
            return "\"" + PageIndex + "|" + (int)sortDirection + "|" + StateFormatter.Serialize(sortExpression, Purpose.WebForms_GridView_SortExpression) + "|\"";
        }

        /// <devdoc>
        /// <para>Builds the callback argument used in DataControlLinkButtons.</para>
        /// </devdoc>
        private string BuildCallbackArgument(int pageIndex) {
            if (String.IsNullOrEmpty(_sortExpressionSerialized)) {
                _sortExpressionSerialized = StateFormatter.Serialize(SortExpression, Purpose.WebForms_GridView_SortExpression);
            }
            return "\"" + pageIndex + "|" + (int)SortDirection + "|" + _sortExpressionSerialized + "|\"";
        }

        private void ClearDataKeys() {
            _dataKeysArrayList = null;
        }

        /// <devdoc>
        /// Create a single autogenerated row.  This function can be overridden to create a different AutoGeneratedField.
        /// </devdoc>
        [Obsolete("This is kept for backward compatibility - this API is no more used")]
        protected virtual AutoGeneratedField CreateAutoGeneratedColumn(AutoGeneratedFieldProperties fieldProperties) {
            AutoGeneratedField field = new AutoGeneratedField(fieldProperties.DataField);
            string name = fieldProperties.Name;
            ((IStateManager)field).TrackViewState();

            field.HeaderText = name;
            field.SortExpression = name;
            field.ReadOnly = fieldProperties.IsReadOnly;
            field.DataType = fieldProperties.Type;

            return field;
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Creates the control hierarchy that is used to render the Smart.
        ///       This is called whenever a control hierarchy is needed and the
        ///       ChildControlsCreated property is false.
        ///       The implementation assumes that all the children in the controls
        ///       collection have already been cleared.</para>
        /// </devdoc>
        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding) {
            PagedDataSource pagedDataSource = null;

            if (dataBinding) {
                bool allowPaging = AllowPaging;
                DataSourceView view = GetData();
                DataSourceSelectArguments arguments = SelectArguments;
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.DataBoundControl_NullView, ID));
                }

                bool useServerPaging = allowPaging && view.CanPage;

                if (allowPaging && !view.CanPage) {
                    if (dataSource != null && !(dataSource is ICollection)) {
                        arguments.StartRowIndex = checked(PageSize * PageIndex);
                        arguments.MaximumRows = PageSize;
                        // This should throw an exception saying the data source can't page.
                        // We do this because the data source can provide a better error message than we can.
                        view.Select(arguments, SelectCallback);
                    }
                }

                if (useServerPaging) {
                    if (view.CanRetrieveTotalRowCount) {
                        pagedDataSource = CreateServerPagedDataSource(arguments.TotalRowCount);
                    }
                    else {
                        ICollection dataSourceCollection = dataSource as ICollection;
                        if (dataSourceCollection == null) {
                            throw new HttpException(SR.GetString(SR.DataBoundControl_NeedICollectionOrTotalRowCount, GetType().Name));
                        }
                        int priorPagesRecordCount = checked(PageIndex * PageSize);
                        pagedDataSource = CreateServerPagedDataSource(checked(priorPagesRecordCount + dataSourceCollection.Count));
                    }
                }
                else {
                    pagedDataSource = CreatePagedDataSource();
                }
            }
            else {
                pagedDataSource = CreatePagedDataSource();
            }

            IEnumerator pagedDataSourceEnumerator = null;
            int count = 0;
            ArrayList keyArray = DataKeysArrayList;
            ArrayList suffixArray = ClientIDRowSuffixArrayList;
            ICollection fields = null;
            int itemCount = -1; // number of items in the collection.  We need to know to decide if we need a null row.
            int rowsArrayCapacity = 0;
            ICollection collection = dataSource as ICollection;

            if (dataBinding) {
                keyArray.Clear();
                suffixArray.Clear();
                if (dataSource != null) {
                    // If we got to here, it's because the data source view said it could page, but then returned
                    // something that wasn't an ICollection.  Probably a data source control author error.
                    if ((collection == null) && (pagedDataSource.IsPagingEnabled && !pagedDataSource.IsServerPagingEnabled)) {
                        throw new HttpException(SR.GetString(SR.GridView_Missing_VirtualItemCount, ID));
                    }
                }
            }
            else {
                if (collection == null) {
                    throw new HttpException(SR.GetString(SR.DataControls_DataSourceMustBeCollectionWhenNotDataBinding));
                }
            }

            _pageCount = 0;
            if (dataSource != null) {
                pagedDataSource.DataSource = dataSource;
                if (pagedDataSource.IsPagingEnabled && dataBinding) {
                    // Fix up the page index if we have gone past the page count
                    int pagedDataSourcePageCount = pagedDataSource.PageCount;
                    Debug.Assert(pagedDataSource.CurrentPageIndex >= 0);
                    if (pagedDataSource.CurrentPageIndex >= pagedDataSourcePageCount) {
                        int lastPageIndex = pagedDataSourcePageCount - 1;
                        pagedDataSource.CurrentPageIndex = _pageIndex = lastPageIndex;
                    }
                }
                fields = CreateColumns(dataBinding ? pagedDataSource : null, dataBinding);

                if (collection != null) {
                    itemCount = collection.Count;
                    int pageSize = pagedDataSource.IsPagingEnabled ? pagedDataSource.PageSize : collection.Count;
                    rowsArrayCapacity = pageSize;
                    if (dataBinding) {
                        keyArray.Capacity = pageSize;
                        suffixArray.Capacity = pageSize;
                    }
                    // PagedDataSource has strange nehavior here.  If DataSourceCount is 0 but paging is enabled,
                    // it returns a PageCount of 1, which is inconsistent with DetailsView and FormView.
                    // We don't want to change PagedDataSource for back compat reasons.
                    if (pagedDataSource.DataSourceCount == 0) {
                        _pageCount = 0;
                    }
                    else {
                        _pageCount = pagedDataSource.PageCount;
                    }
                }
            }
            _rowsArray = new ArrayList(rowsArrayCapacity);
            _rowsCollection = null;
            _dataKeyArray = null;
            _clientIDRowSuffixArray = null;

            Table table = CreateChildTable();
            Controls.Add(table);

            TableRowCollection rows = table.Rows;

            // 
            if (dataSource == null) {
                if (EmptyDataTemplate != null || EmptyDataText.Length > 0) {
                    CreateRow(-1, -1, DataControlRowType.EmptyDataRow, DataControlRowState.Normal, dataBinding, null, new DataControlField[0], rows, null);
                }
                else {
                    Controls.Clear();
                }
                return 0;
            }

            int fieldCount = 0;
            if (fields != null)
                fieldCount = fields.Count;

            DataControlField[] displayFields = new DataControlField[fieldCount];
            if (fieldCount > 0) {
                fields.CopyTo(displayFields, 0);

                bool requiresDataBinding = false;

                for (int c = 0; c < displayFields.Length; c++) {
                    if (displayFields[c].Initialize(AllowSorting, this)) {
                        requiresDataBinding = true;
                    }

                    if (DetermineRenderClientScript()) {
                        displayFields[c].ValidateSupportsCallback();
                    }
                }

                if (requiresDataBinding) {
                    RequiresDataBinding = true;
                }
            }

            GridViewRow row;
            DataControlRowType rowType;
            DataControlRowState rowState;
            int index = 0;
            int dataSourceIndex = 0;

            string[] dataKeyNames = DataKeyNamesInternal;
            bool storeKeys = (dataBinding && (dataKeyNames.Length != 0));
            bool storeSuffix = (dataBinding && ClientIDRowSuffixInternal.Length != 0);
            bool createPager = pagedDataSource.IsPagingEnabled;
            int editIndex = EditIndex;

            if (itemCount == -1) {
                if (_storedDataValid) {
                    if (_firstDataRow != null) {
                        itemCount = 1;
                    }
                    else {
                        itemCount = 0;
                    }
                }
                else {
                    // make sure there's at least one item in the source.
                    IEnumerator e = dataSource.GetEnumerator();

                    if (e.MoveNext()) {
                        object sampleItem = e.Current;
                        StoreEnumerator(e, sampleItem);
                        itemCount = 1;
                    }
                    else {
                        itemCount = 0;
                    }
                }
            }
            if (itemCount == 0) {
                bool controlsCreated = false;

                if (ShowHeader && ShowHeaderWhenEmpty && displayFields.Length > 0) {
                    _headerRow = CreateRow(-1, -1, DataControlRowType.Header, DataControlRowState.Normal, dataBinding, null, displayFields, rows, null);
                    controlsCreated = true;
                }
                if (EmptyDataTemplate != null || EmptyDataText.Length > 0) {
                    CreateRow(-1, -1, DataControlRowType.EmptyDataRow, DataControlRowState.Normal, dataBinding, null, displayFields, rows, null);
                    controlsCreated = true;
                }

                if (!controlsCreated) {
                    Controls.Clear();
                }
                _storedDataValid = false;
                _firstDataRow = null;
                return 0;
            }

            if (fieldCount > 0) {
                if (pagedDataSource.IsPagingEnabled)
                    dataSourceIndex = pagedDataSource.FirstIndexInPage;

                if (createPager && PagerSettings.Visible && _pagerSettings.IsPagerOnTop) {
                    _topPagerRow = CreateRow(-1, -1, DataControlRowType.Pager, DataControlRowState.Normal, dataBinding, null, displayFields, rows, pagedDataSource);
                }

                _headerRow = CreateRow(-1, -1, DataControlRowType.Header, DataControlRowState.Normal, dataBinding, null, displayFields, rows, null);
                if (!ShowHeader) {
                    _headerRow.Visible = false;
                }

                if (storeKeys) {
                    // Reset the selected index if we have a persisted datakey so we
                    // can figure out what index to select based on the key
                    ResetPersistedSelectedIndex();
                }

                if (_storedDataValid) {
                    pagedDataSourceEnumerator = _storedData;
                    if (_firstDataRow != null) {
                        if (storeKeys) {
                            OrderedDictionary keyTable = new OrderedDictionary(dataKeyNames.Length);
                            foreach (string keyName in dataKeyNames) {
                                object keyValue = DataBinder.GetPropertyValue(_firstDataRow, keyName);
                                keyTable.Add(keyName, keyValue);
                            }
                            if (keyArray.Count == index) {
                                keyArray.Add(new DataKey(keyTable, dataKeyNames));
                            }
                            else {
                                keyArray[index] = new DataKey(keyTable, dataKeyNames);
                            }
                        }

                        if (storeSuffix) {
                            OrderedDictionary suffixTable = new OrderedDictionary(ClientIDRowSuffixInternal.Length);
                            foreach (string suffixName in ClientIDRowSuffixInternal) {
                                object suffixValue = DataBinder.GetPropertyValue(_firstDataRow, suffixName);
                                suffixTable.Add(suffixName, suffixValue);
                            }
                            if (suffixArray.Count == index) {
                                suffixArray.Add(new DataKey(suffixTable, ClientIDRowSuffixInternal));
                            }
                            else {
                                suffixArray[index] = new DataKey(suffixTable, ClientIDRowSuffixInternal);
                            }
                        }

                        if (storeKeys && EnablePersistedSelection) {
                            if (index < keyArray.Count) {
                                SetPersistedDataKey(index, (DataKey)keyArray[index]);
                            }
                        }

                        rowType = DataControlRowType.DataRow;
                        rowState = DataControlRowState.Normal;
                        if (index == editIndex)
                            rowState |= DataControlRowState.Edit;
                        if (index == _selectedIndex)
                            rowState |= DataControlRowState.Selected;

                        row = CreateRow(0, dataSourceIndex, rowType, rowState, dataBinding, _firstDataRow, displayFields, rows, null);
                        _rowsArray.Add(row);

                        count++;
                        index++;
                        dataSourceIndex++;

                        _storedDataValid = false;
                        _firstDataRow = null;
                    }
                }
                else {
                    pagedDataSourceEnumerator = pagedDataSource.GetEnumerator();
                }

                rowType = DataControlRowType.DataRow;
                while (pagedDataSourceEnumerator.MoveNext()) {
                    object dataRow = pagedDataSourceEnumerator.Current;

                    if (storeKeys) {
                        OrderedDictionary keyTable = new OrderedDictionary(dataKeyNames.Length);
                        foreach (string keyName in dataKeyNames) {
                            object keyValue = DataBinder.GetPropertyValue(dataRow, keyName);
                            keyTable.Add(keyName, keyValue);
                        }
                        if (keyArray.Count == index) {
                            keyArray.Add(new DataKey(keyTable, dataKeyNames));
                        }
                        else {
                            keyArray[index] = new DataKey(keyTable, dataKeyNames);
                        }
                    }

                    if (storeSuffix) {
                        OrderedDictionary suffixTable = new OrderedDictionary(ClientIDRowSuffixInternal.Length);
                        foreach (string suffixName in ClientIDRowSuffixInternal) {
                            object suffixValue = DataBinder.GetPropertyValue(dataRow, suffixName);
                            suffixTable.Add(suffixName, suffixValue);
                        }
                        if (suffixArray.Count == index) {
                            suffixArray.Add(new DataKey(suffixTable, ClientIDRowSuffixInternal));
                        }
                        else {
                            suffixArray[index] = new DataKey(suffixTable, ClientIDRowSuffixInternal);
                        }
                    }

                    if (storeKeys && EnablePersistedSelection) {
                        if (index < keyArray.Count) {
                            SetPersistedDataKey(index, (DataKey)keyArray[index]);
                        }
                    }

                    rowState = DataControlRowState.Normal;
                    if (index == editIndex)
                        rowState |= DataControlRowState.Edit;
                    if (index == _selectedIndex)
                        rowState |= DataControlRowState.Selected;
                    if (index % 2 != 0) {
                        rowState |= DataControlRowState.Alternate;
                    }

                    row = CreateRow(index, dataSourceIndex, rowType, rowState, dataBinding, dataRow, displayFields, rows, null);
                    _rowsArray.Add(row);

                    count++;
                    dataSourceIndex++;
                    index++;
                }

                if (index == 0) {
                    CreateRow(-1, -1, DataControlRowType.EmptyDataRow, DataControlRowState.Normal, dataBinding, null, displayFields, rows, null);
                }

                _footerRow = CreateRow(-1, -1, DataControlRowType.Footer, DataControlRowState.Normal, dataBinding, null, displayFields, rows, null);
                if (!ShowFooter) {
                    _footerRow.Visible = false;
                }

                if (createPager && PagerSettings.Visible && _pagerSettings.IsPagerOnBottom) {
                    _bottomPagerRow = CreateRow(-1, -1, DataControlRowType.Pager, DataControlRowState.Normal, dataBinding, null, displayFields, rows, pagedDataSource);
                }
            }

            int createdRowsCount = -1;
            if (dataBinding) {
                if (pagedDataSourceEnumerator != null) {
                    if (pagedDataSource.IsPagingEnabled) {
                        _pageCount = pagedDataSource.PageCount;
                        if (pagedDataSource.IsCustomPagingEnabled) {
                            // DevDiv 782891: We didn't well handle GridView paging in the scenario 
                            // in which custom paging was enabled. The root cause was that we mixed up 
                            // createdRowsCount with pagedDataSource.DataSourceCount.
                            createdRowsCount = count;
                        }
                        else {
                            createdRowsCount = pagedDataSource.DataSourceCount;
                        }
                    }
                    else {
                        _pageCount = 1;
                        createdRowsCount = count;
                    }
                }
                else {
                    _pageCount = 0;
                }
            }

            if (PageCount == 1) {   // don't show the pager if there's just one row.
                if (_topPagerRow != null) {
                    _topPagerRow.Visible = false;
                }
                if (_bottomPagerRow != null) {
                    _bottomPagerRow.Visible = false;
                }
            }
            return createdRowsCount;

        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Creates new child table, which contains all rows and controls.</para>
        /// </devdoc>
        protected virtual Table CreateChildTable() {
            return new ChildTable(String.IsNullOrEmpty(ID) ? null : ClientID);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Creates new control style.</para>
        /// </devdoc>
        protected override Style CreateControlStyle() {
            TableStyle controlStyle = new TableStyle();

            // initialize defaults that are different from TableStyle
            controlStyle.GridLines = GridLines.Both;
            controlStyle.CellSpacing = 0;

            return controlStyle;
        }

        /// <devdoc>
        ///   Creates the set of fields to be used to build up the control
        ///   hierarchy.
        ///   When AutoGenerateColumns is true, the fields are created to match the
        ///   datasource and are appended to the set of fields defined in the Fields
        ///   collection.
        /// </devdoc>
        protected virtual ICollection CreateColumns(PagedDataSource dataSource, bool useDataSource) {
            ArrayList fieldsArray = new ArrayList();
            bool autoGenEditButton = AutoGenerateEditButton;
            bool autoGenDeleteButton = AutoGenerateDeleteButton;
            bool autoGenSelectButton = AutoGenerateSelectButton;


            if (autoGenEditButton || autoGenDeleteButton || autoGenSelectButton) {
                CommandField commandField = new CommandField();
                commandField.ButtonType = ButtonType.Link;

                if (autoGenEditButton) {
                    commandField.ShowEditButton = true;
                }
                if (autoGenDeleteButton) {
                    commandField.ShowDeleteButton = true;
                }
                if (autoGenSelectButton) {
                    commandField.ShowSelectButton = true;
                }

                fieldsArray.Add(commandField);
            }


            foreach (DataControlField f in Columns) {
                fieldsArray.Add(f);
            }

            if (AutoGenerateColumns == true) {
                if (ColumnsGeneratorInternal is GridViewColumnsGenerator) {
                    ((GridViewColumnsGenerator)ColumnsGeneratorInternal).DataItem = dataSource;
                    ((GridViewColumnsGenerator)ColumnsGeneratorInternal).InDataBinding = useDataSource;
                }

                fieldsArray.AddRange(ColumnsGeneratorInternal.GenerateFields(this));
            }

            return fieldsArray;
        }

        protected override DataSourceSelectArguments CreateDataSourceSelectArguments() {
            DataSourceSelectArguments arguments = new DataSourceSelectArguments();
            DataSourceView view = GetData();
            bool useServerPaging = AllowPaging && view.CanPage;

            string sortExpression = SortExpressionInternal;
            if (SortDirectionInternal == SortDirection.Descending && !String.IsNullOrEmpty(sortExpression)) {
                sortExpression += " DESC";
            }
            arguments.SortExpression = sortExpression;

            // decide if we should use server-side paging
            if (useServerPaging) {
                if (view.CanRetrieveTotalRowCount) {
                    arguments.RetrieveTotalRowCount = true;
                    arguments.MaximumRows = PageSize;
                }
                else {
                    arguments.MaximumRows = -1;
                }
                arguments.StartRowIndex = checked(PageSize * PageIndex);
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
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                string firstPageImageUrl = pagerSettings.FirstPageImageUrl;

                IButtonControl firstButton;
                if (firstPageImageUrl.Length > 0) {
                    firstButton = new DataControlImageButton(this);
                    ((DataControlImageButton)firstButton).ImageUrl = firstPageImageUrl;
                    ((DataControlImageButton)firstButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.FirstPageText);
                    ((DataControlImageButton)firstButton).EnableCallback(BuildCallbackArgument(0));
                }
                else {
                    firstButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)firstButton).Text = pagerSettings.FirstPageText;
                    ((DataControlPagerLinkButton)firstButton).EnableCallback(BuildCallbackArgument(0));
                }
                firstButton.CommandName = DataControlCommands.PageCommandName;
                firstButton.CommandArgument = DataControlCommands.FirstPageCommandArgument;
                cell.Controls.Add((Control)firstButton);
            }

            if (!isFirstPage) {
                TableCell cell = new TableCell();
                row.Cells.Add(cell);

                IButtonControl prevButton;
                if (prevPageImageUrl.Length > 0) {
                    prevButton = new DataControlImageButton(this);
                    ((DataControlImageButton)prevButton).ImageUrl = prevPageImageUrl;
                    ((DataControlImageButton)prevButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.PreviousPageText);
                    ((DataControlImageButton)prevButton).EnableCallback(BuildCallbackArgument(PageIndex - 1));
                }
                else {
                    prevButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)prevButton).Text = pagerSettings.PreviousPageText;
                    ((DataControlPagerLinkButton)prevButton).EnableCallback(BuildCallbackArgument(PageIndex - 1));
                }
                prevButton.CommandName = DataControlCommands.PageCommandName;
                prevButton.CommandArgument = DataControlCommands.PreviousPageCommandArgument;
                cell.Controls.Add((Control)prevButton);
            }

            if (!isLastPage) {
                TableCell cell = new TableCell();
                row.Cells.Add(cell);

                IButtonControl nextButton;
                if (nextPageImageUrl.Length > 0) {
                    nextButton = new DataControlImageButton(this);
                    ((DataControlImageButton)nextButton).ImageUrl = nextPageImageUrl;
                    ((DataControlImageButton)nextButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.NextPageText);
                    ((DataControlImageButton)nextButton).EnableCallback(BuildCallbackArgument(PageIndex + 1));
                }
                else {
                    nextButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)nextButton).Text = pagerSettings.NextPageText;
                    ((DataControlPagerLinkButton)nextButton).EnableCallback(BuildCallbackArgument(PageIndex + 1));
                }
                nextButton.CommandName = DataControlCommands.PageCommandName;
                nextButton.CommandArgument = DataControlCommands.NextPageCommandArgument;
                cell.Controls.Add((Control)nextButton);
            }

            if (addFirstLastPageButtons && !isLastPage) {
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                string lastPageImageUrl = pagerSettings.LastPageImageUrl;
                IButtonControl lastButton;
                if (lastPageImageUrl.Length > 0) {
                    lastButton = new DataControlImageButton(this);
                    ((DataControlImageButton)lastButton).ImageUrl = lastPageImageUrl;
                    ((DataControlImageButton)lastButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.LastPageText);
                    ((DataControlImageButton)lastButton).EnableCallback(BuildCallbackArgument(pagedDataSource.PageCount - 1));
                }
                else {
                    lastButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)lastButton).Text = pagerSettings.LastPageText;
                    ((DataControlPagerLinkButton)lastButton).EnableCallback(BuildCallbackArgument(pagedDataSource.PageCount - 1));
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
                int currentPageSet = pagedDataSource.CurrentPageIndex / pageSetSize;
                bool currentPageInLastDisplayRange = currentPage - firstDisplayedPage >= 0 && currentPage - firstDisplayedPage < pageSetSize;
                if (firstDisplayedPage > 0 && currentPageInLastDisplayRange) {
                    firstPage = firstDisplayedPage;
                }
                else {
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
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                string firstPageImageUrl = pagerSettings.FirstPageImageUrl;

                IButtonControl firstButton;
                if (firstPageImageUrl.Length > 0) {
                    firstButton = new DataControlImageButton(this);
                    ((DataControlImageButton)firstButton).ImageUrl = firstPageImageUrl;
                    ((DataControlImageButton)firstButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.FirstPageText);
                    ((DataControlImageButton)firstButton).EnableCallback(BuildCallbackArgument(0));
                }
                else {
                    firstButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)firstButton).Text = pagerSettings.FirstPageText;
                    ((DataControlPagerLinkButton)firstButton).EnableCallback(BuildCallbackArgument(0));
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
                ((DataControlPagerLinkButton)button).EnableCallback(BuildCallbackArgument(firstPage - 2));
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
                }
                else {
                    button = new DataControlPagerLinkButton(this);
                    button.Text = pageString;
                    button.CommandName = DataControlCommands.PageCommandName;
                    button.CommandArgument = pageString;
                    ((DataControlPagerLinkButton)button).EnableCallback(BuildCallbackArgument(i - 1));
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
                ((DataControlPagerLinkButton)button).EnableCallback(BuildCallbackArgument(lastPage));
                cell.Controls.Add(button);
            }

            bool isLastPageShown = lastPage == pages;
            if (addFirstLastPageButtons && currentPage != pages && !isLastPageShown) {
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                string lastPageImageUrl = pagerSettings.LastPageImageUrl;

                IButtonControl lastButton;
                if (lastPageImageUrl.Length > 0) {
                    lastButton = new DataControlImageButton(this);
                    ((DataControlImageButton)lastButton).ImageUrl = lastPageImageUrl;
                    ((DataControlImageButton)lastButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.LastPageText);
                    ((DataControlImageButton)lastButton).EnableCallback(BuildCallbackArgument(pagedDataSource.PageCount - 1));
                }
                else {
                    lastButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)lastButton).Text = pagerSettings.LastPageText;
                    ((DataControlPagerLinkButton)lastButton).EnableCallback(BuildCallbackArgument(pagedDataSource.PageCount - 1));
                }
                lastButton.CommandName = DataControlCommands.PageCommandName;
                lastButton.CommandArgument = DataControlCommands.LastPageCommandArgument;
                cell.Controls.Add((Control)lastButton);
            }
        }

        private GridViewRow CreateRow(int rowIndex, int dataSourceIndex, DataControlRowType rowType, DataControlRowState rowState, bool dataBind, object dataItem, DataControlField[] fields, TableRowCollection rows, PagedDataSource pagedDataSource) {
            GridViewRow row = CreateRow(rowIndex, dataSourceIndex, rowType, rowState);
            GridViewRowEventArgs e = new GridViewRowEventArgs(row);

            if (rowType != DataControlRowType.Pager) {
                InitializeRow(row, fields);
            }
            else {
                InitializePager(row, fields.Length, pagedDataSource);
            }

            if (dataBind) {
                row.DataItem = dataItem;
            }

            OnRowCreated(e);
            rows.Add(row);

            if (dataBind) {
                row.DataBind();
                OnRowDataBound(e);
                row.DataItem = null;
            }

            return row;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual GridViewRow CreateRow(int rowIndex, int dataSourceIndex, DataControlRowType rowType, DataControlRowState rowState) {
            return new GridViewRow(rowIndex, dataSourceIndex, rowType, rowState);
        }

        private PagedDataSource CreatePagedDataSource() {
            PagedDataSource pagedDataSource = new PagedDataSource();

            pagedDataSource.CurrentPageIndex = PageIndex;
            pagedDataSource.PageSize = PageSize;
            pagedDataSource.AllowPaging = AllowPaging;
            pagedDataSource.AllowCustomPaging = AllowCustomPaging;
            pagedDataSource.AllowServerPaging = false;
            pagedDataSource.VirtualCount = VirtualItemCount;

            return pagedDataSource;
        }

        private PagedDataSource CreateServerPagedDataSource(int totalRowCount) {
            PagedDataSource pagedDataSource = new PagedDataSource();

            pagedDataSource.CurrentPageIndex = PageIndex;
            pagedDataSource.PageSize = PageSize;
            pagedDataSource.AllowPaging = AllowPaging;
            pagedDataSource.AllowCustomPaging = false;
            pagedDataSource.AllowServerPaging = true;
            pagedDataSource.VirtualCount = totalRowCount;

            return pagedDataSource;
        }

        /// Data bound controls should override PerformDataBinding instead
        /// of DataBind.  If DataBind if overridden, the OnDataBinding and OnDataBound events will
        /// fire in the wrong order.  However, for backwards compat on ListControl and AdRotator, we 
        /// can't seal this method.  It is sealed on all new BaseDataBoundControl-derived controls.
        public override sealed void DataBind() {
            base.DataBind();
        }

        public virtual void DeleteRow(int rowIndex) {
            // use EnableModelVadliation as the causesValdiation param because the hosting page should not
            // be validated unless model validation is going to be used
            ResetModelValidationGroup(EnableModelValidation, String.Empty);
            HandleDelete(null, rowIndex);
        }

        private bool DetermineRenderClientScript() {
            // In a client script-enabled control, always determine whether to render the
            // client script-based functionality.
            // The decision should be based on browser capabilities.

            if (!_renderClientScriptValid) {
                _renderClientScript = false;

                if (EnableSortingAndPagingCallbacks && (Context != null) && (Page != null) && (Page.RequestInternal != null) && Page.Request.Browser.SupportsCallback && !IsParentedToUpdatePanel) {
                    HttpBrowserCapabilities browserCaps = Page.Request.Browser;
                    bool hasEcmaScript = browserCaps.EcmaScriptVersion.Major > 0;
                    bool hasDOM = browserCaps.W3CDomVersion.Major > 0;
                    bool isHtml4 = !StringUtil.EqualsIgnoreCase(browserCaps["tagwriter"], typeof(Html32TextWriter).FullName);
                    _renderClientScript = hasEcmaScript && hasDOM && isHtml4;
                }
                _renderClientScriptValid = true;
            }
            return _renderClientScript;
        }

        protected virtual void ExtractRowValues(IOrderedDictionary fieldValues, GridViewRow row, bool includeReadOnlyFields, bool includePrimaryKey) {
            if (fieldValues == null) {
                Debug.Assert(false, "GridView::ExtractRowValues- must hand in a valid reference to an IDictionary.");
                return;
            }

            ICollection fields = CreateColumns(null, false);
            int fieldCount = fields.Count;
            object[] fieldsArray = new object[fieldCount];
            string[] dataKeyNames = DataKeyNamesInternal;
            fields.CopyTo(fieldsArray, 0);

            // Field and row cell count should match, but if there was no data, or if the user removed some row cells,
            // these may no longer match.  Make sure we don't exceed the bounds.
            for (int i = 0; i < fieldCount && i < row.Cells.Count; i++) {
                if (!((DataControlField)fieldsArray[i]).Visible) {
                    continue;
                }

                OrderedDictionary newValues = new OrderedDictionary();

                ((DataControlField)fieldsArray[i]).ExtractValuesFromCell(newValues, row.Cells[i] as DataControlFieldCell, row.RowState, includeReadOnlyFields);
                foreach (DictionaryEntry entry in newValues) {
                    if (includePrimaryKey || (Array.IndexOf(dataKeyNames, entry.Key) == -1)) {
                        fieldValues[entry.Key] = entry.Value;
                    }
                }
            }
        }

        protected virtual string GetCallbackResult() {
            StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
            // 
            HtmlTextWriter writer = new HtmlTextWriter(stringWriter);
            IStateFormatter2 formatter = StateFormatter;

            RenderTableContents(writer);

            writer.Flush();
            writer.Close();

            string dataKeysString = formatter.Serialize(SaveDataKeysState(), Purpose.WebForms_GridView_DataKeys);
            string sortExpressionString = formatter.Serialize(SortExpression, Purpose.WebForms_GridView_SortExpression);

            // this should return the html that goes in the panel, plus the new page and sort info.
            return Convert.ToString(PageIndex, CultureInfo.InvariantCulture) + "|" + Convert.ToString((int)SortDirection, CultureInfo.InvariantCulture) + "|" + sortExpressionString + "|" + dataKeysString + "|" + stringWriter.ToString();
        }

        protected virtual string GetCallbackScript(IButtonControl buttonControl, string argument) {
            if (DetermineRenderClientScript()) {
                if (String.IsNullOrEmpty(argument)) {
                    if (buttonControl.CommandName == DataControlCommands.SortCommandName) {
                        argument = BuildCallbackArgument(buttonControl.CommandArgument, SortDirection);
                    }
                }

                if (Page != null) {
                    Page.ClientScript.RegisterForEventValidation(UniqueID, argument);
                }

                string clientCallbackReference = "javascript:__gv" + ClientID + ".callback";
                return clientCallbackReference + "(" + argument + "); return false;";
            }
            return null;
        }

        private int GetRowIndex(GridViewRow row, string commandArgument) {
            if (row != null) {
                return row.RowIndex;
            }
            return Convert.ToInt32(commandArgument, CultureInfo.InvariantCulture);
        }

        private bool TryGetRowIndex(GridViewRow row, string commandArgument, out int rowIndex) {
            if (row != null) {
                rowIndex = row.RowIndex;
                return true;
            }
            return Int32.TryParse(commandArgument, NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex);
        }

        private void HandleCancel(int rowIndex) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            GridViewCancelEditEventArgs e = new GridViewCancelEditEventArgs(rowIndex);
            OnRowCancelingEdit(e);

            if (e.Cancel) {
                return;
            }

            if (isBoundToDataSourceControl) {
                EditIndex = -1;
            }

            RequiresDataBinding = true;
        }

        private void HandleDelete(GridViewRow row, int rowIndex) {
            DataSourceView view = null;
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            if (isBoundToDataSourceControl) {
                view = GetData();
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.GridView_DataSourceReturnedNullView, ID));
                }
            }

            if (row == null && rowIndex < Rows.Count) {
                row = Rows[rowIndex];
            }

            GridViewDeleteEventArgs e = new GridViewDeleteEventArgs(rowIndex);

            if (row != null) {
                ExtractRowValues(e.Values, row, true/*includeReadOnlyFields*/, false/*includePrimaryKey*/);
            }
            if (DataKeys.Count > rowIndex) {
                foreach (DictionaryEntry entry in DataKeys[rowIndex].Values) {
                    e.Keys.Add(entry.Key, entry.Value);
                    if (e.Values.Contains(entry.Key)) {
                        e.Values.Remove(entry.Key);
                    }
                }
            }


            OnRowDeleting(e);

            if (e.Cancel) {
                return;
            }

            _deletedRowIndex = rowIndex;

            if (isBoundToDataSourceControl) {
                _deleteKeys = e.Keys;
                _deleteValues = e.Values;

                view.Delete(e.Keys, e.Values, HandleDeleteCallback);
            }
        }

        private bool HandleDeleteCallback(int affectedRows, Exception ex) {
            GridViewDeletedEventArgs sea = new GridViewDeletedEventArgs(affectedRows, ex);
            sea.SetKeys(_deleteKeys);
            sea.SetValues(_deleteValues);

            OnRowDeleted(sea);
            _deleteKeys = null;
            _deleteValues = null;

            if (ex != null && !sea.ExceptionHandled) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
            }
            EditIndex = -1;

            if (affectedRows > 0) {
                // Patch up the current page.  We might have deleted the last records on the last page, so move
                // to the right page.
                // DevDiv 782891: We didn't handle GridView paging well in the scenario 
                // in which custom paging was enabled. The root cause was that we mixed up 
                // createdRowsCount with pagedDataSource.DataSourceCount.
                int rowCount;
                if (this.AllowPaging && this.AllowCustomPaging) {
                    // Under this condition, the value of ViewState[ItemCountViewStateKey] indicates
                    // created rows count other than total row count.
                    rowCount = (int)VirtualItemCount;
                }
                else {
                    rowCount = (int)ViewState[ItemCountViewStateKey];
                }

                // Can't have negative rowCount
                int expectedRowCount = Math.Max(0, rowCount - affectedRows);

                if (AllowPaging) {
                    // Calculate the expected page count
                    int expectedPageCount = 0;
                    checked {
                        // Correctly calculate the expectedPageCount. Special case: We want there to always be at least one page even if there are no items
                        // so that we can always *safely* calculate the pageindex as expectedRowCount - 1
                        expectedPageCount = Math.Max(1, (expectedRowCount + PageSize - 1) / PageSize);
                    }

                    // Adjust the pageIndex based on the expected page count
                    _pageIndex = Math.Min(_pageIndex, expectedPageCount - 1);
                    Debug.Assert(_pageIndex >= 0, "Page index should never be negative!");
                }

                if (SelectedIndex >= 0) {
                    if (expectedRowCount == 0) {
                        // There is nothing to select
                        SelectedIndex = -1;
                    }
                    else {
                        // Calculate the selected index in terms of the row number in the total rows
                        int selectedRow = AllowPaging ? (PageIndex * PageSize) + SelectedIndex : SelectedIndex;
                        // If the selected index is no longer valid make it the last index
                        if (selectedRow > expectedRowCount) {
                            int lastIndex = AllowPaging ? expectedRowCount % PageSize : expectedRowCount;
                            SelectedIndex = lastIndex;
                        }
                    }
                }
            }
            _deletedRowIndex = -1;

            RequiresDataBinding = true;
            return true;
        }

        private void HandleEdit(int rowIndex) {
            GridViewEditEventArgs e = new GridViewEditEventArgs(rowIndex);
            OnRowEditing(e);

            if (e.Cancel) {
                return;
            }

            EditIndex = e.NewEditIndex;

            RequiresDataBinding = true;
        }

        private bool HandleEvent(EventArgs e, bool causesValidation, string validationGroup) {
            bool handled = false;

            ResetModelValidationGroup(causesValidation, validationGroup);

            GridViewCommandEventArgs dce = e as GridViewCommandEventArgs;

            if (dce != null) {

                OnRowCommand(dce);
                if (dce.Handled) {
                    return true;
                }
                handled = true;

                string command = dce.CommandName;

                if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.SelectCommandName)) {
                    HandleSelect(GetRowIndex(dce.Row, (string)dce.CommandArgument));
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.PageCommandName)) {
                    string pageNumberArg = (string)dce.CommandArgument;

                    int newPage = PageIndex;

                    if (StringUtil.EqualsIgnoreCase(pageNumberArg, DataControlCommands.NextPageCommandArgument)) {
                        newPage++;
                    }
                    else if (StringUtil.EqualsIgnoreCase(pageNumberArg, DataControlCommands.PreviousPageCommandArgument)) {
                        newPage--;
                    }
                    else if (StringUtil.EqualsIgnoreCase(pageNumberArg, DataControlCommands.FirstPageCommandArgument)) {
                        newPage = 0;
                    }
                    else if (StringUtil.EqualsIgnoreCase(pageNumberArg, DataControlCommands.LastPageCommandArgument)) {
                        newPage = PageCount - 1;
                    }
                    else {
                        // argument is page number, and page index is 1 less than that
                        newPage = Convert.ToInt32(pageNumberArg, CultureInfo.InvariantCulture) - 1;
                    }
                    HandlePage(newPage);
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.SortCommandName)) {
                    HandleSort((string)dce.CommandArgument);
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.EditCommandName)) {
                    HandleEdit(GetRowIndex(dce.Row, (string)dce.CommandArgument));
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.UpdateCommandName)) {
                    HandleUpdate(dce.Row, GetRowIndex(dce.Row, (string)dce.CommandArgument), causesValidation);
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.CancelCommandName)) {
                    HandleCancel(GetRowIndex(dce.Row, (string)dce.CommandArgument));
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.DeleteCommandName)) {
                    HandleDelete(dce.Row, GetRowIndex(dce.Row, (string)dce.CommandArgument));
                }
                else {
                    int rowIndex;
                    if (TryGetRowIndex(dce.Row, (String)dce.CommandArgument, out rowIndex)) {
                        handled = HandleCommand(dce.Row, rowIndex, command);
                    }
                }
            }

            return handled;
        }

        private bool HandleCommand(GridViewRow row, int rowIndex, string commandName) {
            DataSourceView view = null;
            if (IsDataBindingAutomatic) {
                view = GetData();
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.GridView_DataSourceReturnedNullView, ID));
                }
            }
            else {
                return false;
            }

            if (row == null && rowIndex < Rows.Count) {
                row = Rows[rowIndex];
            }

            if (!view.CanExecute(commandName)) {
                return false;
            }

            OrderedDictionary values = new OrderedDictionary();
            OrderedDictionary keys = new OrderedDictionary();

            if (row != null) {
                ExtractRowValues(values, row, true/*includeReadOnlyFields*/, false/*includePrimaryKey*/);
            }

            if (DataKeys.Count > rowIndex) {
                foreach (DictionaryEntry entry in DataKeys[rowIndex].Values) {
                    keys.Add(entry.Key, entry.Value);
                    if (values.Contains(entry.Key)) {
                        values.Remove(entry.Key);
                    }
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
            EditIndex = -1;

            RequiresDataBinding = true;
            return true;
        }

        private void HandlePage(int newPage) {
            if (!AllowPaging) {
                return;
            }

            GridViewPageEventArgs e = new GridViewPageEventArgs(newPage);

            OnPageIndexChanging(e);

            if (e.Cancel) {
                return;
            }

            if (IsDataBindingAutomatic) {
                if (e.NewPageIndex > -1) {
                    // if the requested page is out of range and we're already on the last page, don't rebind
                    if ((e.NewPageIndex >= PageCount && _pageIndex == PageCount - 1)) {
                        return;
                    }
                    // DevDiv Bugs 188830: Don't clear data keys if page is out of range, since control won't be rebound.
                    ClearDataKeys();
                    EditIndex = -1;
                    _pageIndex = e.NewPageIndex;
                }
                else {
                    return;
                }
            }

            OnPageIndexChanged(EventArgs.Empty);
            RequiresDataBinding = true;
        }

        private void HandleSelect(int rowIndex) {
            GridViewSelectEventArgs e = new GridViewSelectEventArgs(rowIndex);
            OnSelectedIndexChanging(e);

            if (e.Cancel) {
                return;
            }

            SelectedIndex = e.NewSelectedIndex;

            OnSelectedIndexChanged(EventArgs.Empty);
        }

        private void HandleSort(string sortExpression) {
            if (!AllowSorting) {
                return;
            }

            SortDirection futureSortDirection = SortDirection.Ascending;

            if ((SortExpressionInternal == sortExpression) && (SortDirectionInternal == SortDirection.Ascending)) {
                // switch direction
                futureSortDirection = SortDirection.Descending;
            }
            HandleSort(sortExpression, futureSortDirection);
        }

        private void HandleSort(string sortExpression, SortDirection sortDirection) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;
            GridViewSortEventArgs e = new GridViewSortEventArgs(sortExpression, sortDirection);

            OnSorting(e);

            if (e.Cancel) {
                return;
            }

            if (isBoundToDataSourceControl) {
                ClearDataKeys();
                DataSourceView view = GetData();
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.GridView_DataSourceReturnedNullView, ID));
                }

                EditIndex = -1;

                SortExpressionInternal = e.SortExpression;
                SortDirectionInternal = e.SortDirection;
                _pageIndex = 0;
            }

            OnSorted(EventArgs.Empty);
            RequiresDataBinding = true;
        }

        private void HandleUpdate(GridViewRow row, int rowIndex, bool causesValidation) {
            if (causesValidation && Page != null && !Page.IsValid) {
                return;
            }

            DataSourceView view = null;
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            if (isBoundToDataSourceControl) {
                view = GetData();
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.GridView_DataSourceReturnedNullView, ID));
                }
            }

            GridViewUpdateEventArgs e = new GridViewUpdateEventArgs(rowIndex);

            foreach (DictionaryEntry entry in BoundFieldValues) {
                e.OldValues.Add(entry.Key, entry.Value);
            }

            if (DataKeys.Count > rowIndex) {
                foreach (DictionaryEntry entry in DataKeys[rowIndex].Values) {
                    e.Keys.Add(entry.Key, entry.Value);
                }
            }

            if (row == null && Rows.Count > rowIndex) {
                row = Rows[rowIndex];
            }

            if (row != null) {
                ExtractRowValues(e.NewValues, row, false/*includeReadOnlyFields*/, true/*includePrimaryKey*/);
            }

            OnRowUpdating(e);

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
            GridViewUpdatedEventArgs sea = new GridViewUpdatedEventArgs(affectedRows, ex);
            sea.SetKeys(_updateKeys);
            sea.SetOldValues(_updateOldValues);
            sea.SetNewValues(_updateNewValues);

            OnRowUpdated(sea);
            _updateKeys = null;
            _updateOldValues = null;
            _updateNewValues = null;
            if (ex != null && !sea.ExceptionHandled) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
                sea.KeepInEditMode = true;
            }

            if (IsUsingModelBinders && !Page.ModelState.IsValid) {
                sea.KeepInEditMode = true;
            }

            // we need to databind here event if no records were affected because
            // changing the EditIndex required a rebind.  The event args give the programmer
            // the chance to cancel the bind so the edits aren't lost.
            if (!sea.KeepInEditMode) {
                EditIndex = -1;
                RequiresDataBinding = true;
            }
            return true;
        }

        /// <devdoc>
        ///    <para>
        ///   Creates a GridViewRow that contains the paging UI.
        ///   The paging UI is a navigation bar that is a built into a single TableCell that
        ///   spans across all fields of the GridView.
        ///    </para>
        /// </devdoc>
        protected virtual void InitializePager(GridViewRow row, int columnSpan, PagedDataSource pagedDataSource) {
            TableCell cell = new TableCell();
            if (columnSpan > 1) {
                cell.ColumnSpan = columnSpan;
            }

            PagerSettings pagerSettings = PagerSettings;

            if (_pagerTemplate != null) {
                InitializeTemplateRow(row, columnSpan);
            }
            else {
                PagerTable pagerTable = new PagerTable();
                TableRow pagerTableRow = new TableRow();
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
                cell.Controls.Add(pagerTable);
                pagerTable.Rows.Add(pagerTableRow);
                row.Cells.Add(cell);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void InitializeRow(GridViewRow row, DataControlField[] fields) {
            DataControlRowType rowType = row.RowType;
            DataControlRowState rowState = row.RowState;
            int rowIndex = row.RowIndex;
            bool useAccessibleHeader = false;

            if (rowType == DataControlRowType.EmptyDataRow) {
                InitializeTemplateRow(row, fields.Length);
            }
            else {
                TableCellCollection cells = row.Cells;
                string rowHeaderColumn = RowHeaderColumn;

                if (rowType == DataControlRowType.Header) {
                    useAccessibleHeader = UseAccessibleHeader;
                }

                for (int i = 0; i < fields.Length; i++) {
                    DataControlFieldCell cell;

                    if ((rowType == DataControlRowType.Header) && useAccessibleHeader) {
                        cell = new DataControlFieldHeaderCell(fields[i]);
                        ((DataControlFieldHeaderCell)cell).Scope = TableHeaderScope.Column;
                        ((DataControlFieldHeaderCell)cell).AbbreviatedText = fields[i].AccessibleHeaderText;
                    }
                    else {
                        BoundField boundField = fields[i] as BoundField;
                        if (rowHeaderColumn.Length > 0 && boundField != null && boundField.DataField == rowHeaderColumn) {
                            cell = new DataControlFieldHeaderCell(fields[i]);
                            ((DataControlFieldHeaderCell)cell).Scope = TableHeaderScope.Row;
                        }
                        else {
                            cell = new DataControlFieldCell(fields[i]);
                        }
                    }

                    DataControlCellType cellType;
                    switch (rowType) {
                        case DataControlRowType.Header:
                            cellType = DataControlCellType.Header;
                            break;
                        case DataControlRowType.Footer:
                            cellType = DataControlCellType.Footer;
                            break;
                        default:
                            cellType = DataControlCellType.DataCell;
                            break;
                    }
                    fields[i].InitializeCell(cell, cellType, rowState, rowIndex);
                    cells.Add(cell);
                }
            }
        }

        private void InitializeTemplateRow(GridViewRow row, int columnSpan) {
            TableCell contentCell = null;
            ITemplate contentTemplate = null;

            switch (row.RowType) {
                case DataControlRowType.EmptyDataRow:
                    if (_emptyDataTemplate != null) {
                        contentCell = new TableCell();
                        contentTemplate = _emptyDataTemplate;
                    }
                    else {
                        contentCell = new TableCell();
                        string emptyDataText = EmptyDataText;
                        if (emptyDataText.Length > 0) {
                            contentCell.Text = emptyDataText;
                        }
                    }
                    break;
                case DataControlRowType.Pager:
                    if (_pagerTemplate != null) {
                        contentCell = new TableCell();
                        contentTemplate = _pagerTemplate;
                    }
                    break;
            }

            if (contentCell != null) {
                if (columnSpan > 1) {
                    contentCell.ColumnSpan = columnSpan;
                }
                if (contentTemplate != null) {
                    contentTemplate.InstantiateIn(contentCell);
                }
                row.Cells.Add(contentCell);
            }
        }

        /// <devdoc>
        /// <para>Loads the control state for those properties that should persist across postbacks
        ///   even when EnableViewState=false.</para>
        /// </devdoc>
        protected internal override void LoadControlState(object savedState) {
            // Any properties that could have been set in the persistance need to be
            // restored to their defaults if they're not in ControlState, or they will
            // be restored to their persisted state instead of their empty state.
            _editIndex = -1;
            _pageIndex = 0;
            _selectedIndex = -1;
            _sortExpression = String.Empty;
            _sortDirection = SortDirection.Ascending;
            _dataKeyNames = new string[0];
            _pageCount = -1;

            object[] state = savedState as object[];

            if (state != null) {
                base.LoadControlState(state[0]);

                if (state[1] != null) {
                    _editIndex = (int)state[1];
                }

                if (state[2] != null) {
                    _pageIndex = (int)state[2];
                }

                if (state[3] != null) {
                    _selectedIndex = (int)state[3];
                }

                if (state[4] != null) {
                    _sortExpression = (string)state[4];
                }

                if (state[5] != null) {
                    _sortDirection = (SortDirection)state[5];
                }

                if (state[6] != null) {
                    _dataKeyNames = (string[])state[6];
                }

                if (state[7] != null) {
                    LoadDataKeysState(state[7]);
                }

                if (state[8] != null) {
                    _pageCount = (int)state[8];
                }

                if (state[9] != null) {
                    if ((_dataKeyNames != null) && (_dataKeyNames.Length > 0)) {
                        _persistedDataKey = new DataKey(new OrderedDictionary(_dataKeyNames.Length), _dataKeyNames);
                        ((IStateManager)_persistedDataKey).LoadViewState(state[9]);
                    }
                }

                if (state[10] != null) {
                    _clientIDRowSuffix = (string[])state[10];
                }

                if (state[11] != null) {
                    LoadClientIDRowSuffixDataKeysState(state[11]);
                }

            }
            else {
                base.LoadControlState(null);
            }

        }

        private void LoadDataKeysState(object state) {
            if (state != null) {
                object[] dataKeysState = (object[])state;
                string[] dataKeyNames = DataKeyNamesInternal;
                int dataKeyNamesLength = dataKeyNames.Length;

                ClearDataKeys();
                for (int i = 0; i < dataKeysState.Length; i++) {
                    DataKeysArrayList.Add(new DataKey(new OrderedDictionary(dataKeyNamesLength), dataKeyNames));
                    ((IStateManager)DataKeysArrayList[i]).LoadViewState(dataKeysState[i]);
                }
            }
        }

        private void LoadClientIDRowSuffixDataKeysState(object state) {
            if (state != null) {
                object[] ClientIDRowSuffixDataKeysState = (object[])state;
                string[] ClientIDRowSuffix = ClientIDRowSuffixInternal;
                int ClientIDRowSuffixLength = ClientIDRowSuffix.Length;

                _clientIDRowSuffixArrayList = null;

                for (int i = 0; i < ClientIDRowSuffixDataKeysState.Length; i++) {
                    ClientIDRowSuffixArrayList.Add(new DataKey(new OrderedDictionary(ClientIDRowSuffixLength), ClientIDRowSuffix));
                    ((IStateManager)ClientIDRowSuffixArrayList[i]).LoadViewState(ClientIDRowSuffixDataKeysState[i]);
                }
            }
        }

        private bool LoadHiddenFieldState(string pageIndex, string sortDirection, string sortExpressionSerialized, string dataKeysSerialized) {
            bool propertiesChanged = false;
            int oldPageIndex = Int32.Parse(pageIndex, CultureInfo.InvariantCulture);
            SortDirection oldSortDirection = ((SortDirection)Int32.Parse(sortDirection, CultureInfo.InvariantCulture));

            string oldSortExpression = String.Empty;
            object dataKeys = null;
            if (!String.IsNullOrEmpty(sortExpressionSerialized) || !String.IsNullOrEmpty(dataKeysSerialized)) {
                if (Page == null) {
                    throw new InvalidOperationException();
                }

                if (!String.IsNullOrEmpty(sortExpressionSerialized)) {
                    oldSortExpression = (string)StateFormatter.Deserialize(sortExpressionSerialized, Purpose.WebForms_GridView_SortExpression);
                }
                if (!String.IsNullOrEmpty(dataKeysSerialized)) {
                    dataKeys = StateFormatter.Deserialize(dataKeysSerialized, Purpose.WebForms_GridView_DataKeys);
                }
            }


            if (_pageIndex != oldPageIndex || _sortDirection != oldSortDirection || _sortExpression != oldSortExpression) {
                propertiesChanged = true;

                _pageIndex = oldPageIndex;
                _sortExpression = oldSortExpression;
                _sortDirection = oldSortDirection;

                if (dataKeys != null) {
                    if (_dataKeysArrayList != null) {
                        _dataKeysArrayList.Clear();
                    }
                    LoadDataKeysState(dataKeys);

                }
            }
            return propertiesChanged;
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Loads a saved state of the <see cref='System.Web.UI.WebControls.GridView'/>.</para>
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                object[] myState = (object[])savedState;

                base.LoadViewState(myState[0]);
                if (myState[1] != null)
                    ((IStateManager)Columns).LoadViewState(myState[1]);
                if (myState[2] != null)
                    ((IStateManager)PagerStyle).LoadViewState(myState[2]);
                if (myState[3] != null)
                    ((IStateManager)HeaderStyle).LoadViewState(myState[3]);
                if (myState[4] != null)
                    ((IStateManager)FooterStyle).LoadViewState(myState[4]);
                if (myState[5] != null)
                    ((IStateManager)RowStyle).LoadViewState(myState[5]);
                if (myState[6] != null)
                    ((IStateManager)AlternatingRowStyle).LoadViewState(myState[6]);
                if (myState[7] != null)
                    ((IStateManager)SelectedRowStyle).LoadViewState(myState[7]);
                if (myState[8] != null)
                    ((IStateManager)EditRowStyle).LoadViewState(myState[8]);
                if (myState[9] != null)
                    ((IStateManager)PagerSettings).LoadViewState(myState[9]);
                if (myState[10] != null)
                    OrderedDictionaryStateHelper.LoadViewState((OrderedDictionary)BoundFieldValues, (ArrayList)myState[10]);
                if (myState[11] != null)
                    ((IStateManager)ControlStyle).LoadViewState(myState[11]);
                if (myState[12] != null) {
                    Debug.Assert(ColumnsGeneratorInternal is IStateManager);
                    ((IStateManager)ColumnsGeneratorInternal).LoadViewState(myState[12]);
                }
                if (myState[13] != null)
                    ((IStateManager)SortedAscendingCellStyle).LoadViewState(myState[13]);
                if (myState[14] != null)
                    ((IStateManager)SortedDescendingCellStyle).LoadViewState(myState[14]);
                if (myState[15] != null)
                    ((IStateManager)SortedAscendingHeaderStyle).LoadViewState(myState[15]);
                if (myState[16] != null)
                    ((IStateManager)SortedDescendingHeaderStyle).LoadViewState(myState[16]);
            }
            else {
                base.LoadViewState(null);
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            bool causesValidation = false;
            string validationGroup = String.Empty;

            GridViewCommandEventArgs gvcea = e as GridViewCommandEventArgs;
            if (gvcea != null) {
                IButtonControl button = gvcea.CommandSource as IButtonControl;
                if (button != null) {
                    causesValidation = button.CausesValidation;
                    validationGroup = button.ValidationGroup;
                }
            }
            return HandleEvent(e, causesValidation, validationGroup);
        }

        /// <devdoc>
        /// This method is called when DataMember, DataSource, or DataSourceID is changed.
        /// </devdoc>
        protected override void OnDataPropertyChanged() {
            _storedDataValid = false;
            base.OnDataPropertyChanged();
        }

        protected override void OnDataSourceViewChanged(object sender, EventArgs e) {
            ClearDataKeys();
            base.OnDataSourceViewChanged(sender, e);
        }

        private void OnFieldsChanged(object sender, EventArgs e) {
            if (Initialized) {
                RequiresDataBinding = true;
            }
        }

        /// <devdoc>
        /// GridView initialization.
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (Page != null) {
                if (DataKeyNames.Length > 0 && !AutoGenerateColumns) {
                    Page.RegisterRequiresViewStateEncryption();
                }
                Page.RegisterRequiresControlState(this);
            }

            if (!DesignMode && !String.IsNullOrEmpty(ItemType)) {
                DataBoundControlHelper.EnableDynamicData(this, ItemType);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='PageIndexChanged'/> event.</para>
        /// </devdoc>
        protected virtual void OnPageIndexChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventPageIndexChanged];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected override void OnPagePreLoad(object sender, EventArgs e) {
            // Load hidden field state here to overwrite control state properties.  LoadViewState and LoadControlState
            // may not get called if there's no state in them.  We should allow the user to
            // set EnablePagingCallbacks in Page_Load, so don't request from DetermineRenderClientScript here.
            if (Page != null && !Page.IsCallback && Page.RequestValueCollection != null) {
                string hiddenFieldID = "__gv" + ClientID + "__hidden";
                string hiddenFieldState = Page.RequestValueCollection[hiddenFieldID];
                if (!String.IsNullOrEmpty(hiddenFieldState)) {
                    if (ParseHiddenFieldState(hiddenFieldState)) {
                        _editIndex = -1;
                        RequiresDataBinding = true;
                    }
                }
            }

            base.OnPagePreLoad(sender, e);
        }

        private void OnPagerPropertyChanged(object sender, EventArgs e) {
            if (Initialized) {
                RequiresDataBinding = true;
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='PageIndexChanging'/> event.</para>
        /// </devdoc>
        protected virtual void OnPageIndexChanging(GridViewPageEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            GridViewPageEventHandler handler = (GridViewPageEventHandler)Events[EventPageIndexChanging];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.GridView_UnhandledEvent, ID, "PageIndexChanging"));
                }
            }
        }

        private const string startupScriptFormat = @"
var {0} = new GridView();
{0}.stateField = document.getElementById('{1}');
{0}.panelElement = document.getElementById('{0}__div');
{0}.pageIndex = {3};
{0}.sortExpression = ""{4}"";
{0}.sortDirection = {5};
{0}.setStateField();
{0}.callback = function(arg) {{
    {2};
}};";

        /// <devdoc>
        /// <para>Sets up the callback scripts if client script is supported on the client</para>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            if (DetermineRenderClientScript() && Page != null) {
                string clientReference = "__gv" + ClientID;
                ClientScriptManager scriptOM = Page.ClientScript;

                scriptOM.RegisterClientScriptResource(typeof(GridView), "GridView.js");

                // The return value of GetCallbackEventReference looks like this:
                // "__doCallBack(controlname, script fx that returns arg, "GridView_OnCallback, context, errorMethod)"
                string doCallBackCall = scriptOM.GetCallbackEventReference(this, clientReference + ".getHiddenFieldContents(arg)", "GridView_OnCallback", clientReference);

                // Hidden field used to post content from GridView
                // back to the server
                string hiddenFieldID = clientReference + "__hidden";
                scriptOM.RegisterHiddenField(hiddenFieldID, String.Empty);

                IStateFormatter2 formatter = StateFormatter;
                string sortExpressionSerialized = formatter.Serialize(SortExpression, Purpose.WebForms_GridView_SortExpression);
                string startupScript = String.Format(CultureInfo.InvariantCulture, startupScriptFormat, clientReference, hiddenFieldID, doCallBackCall, PageIndex, sortExpressionSerialized, (int)SortDirection);
                scriptOM.RegisterStartupScript(typeof(GridView), clientReference, startupScript, true);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='CancelCommand '/>event.</para>
        /// </devdoc>
        protected virtual void OnRowCancelingEdit(GridViewCancelEditEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            GridViewCancelEditEventHandler handler = (GridViewCancelEditEventHandler)Events[EventRowCancelingEdit];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.GridView_UnhandledEvent, ID, "RowCancelingEdit"));
                }
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='RowCommand'/> event.</para>
        /// </devdoc>
        protected virtual void OnRowCommand(GridViewCommandEventArgs e) {
            GridViewCommandEventHandler handler = (GridViewCommandEventHandler)Events[EventRowCommand];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='RowCreated'/> event.</para>
        /// </devdoc>
        protected virtual void OnRowCreated(GridViewRowEventArgs e) {
            GridViewRowEventHandler handler = (GridViewRowEventHandler)Events[EventRowCreated];
            if (handler != null) handler(this, e);
        }

        /// <devdoc>
        /// <para>Raises the <see langword='RowDataBound'/> event.</para>
        /// </devdoc>
        protected virtual void OnRowDataBound(GridViewRowEventArgs e) {
            GridViewRowEventHandler handler = (GridViewRowEventHandler)Events[EventRowDataBound];
            if (handler != null) handler(this, e);
        }

        /// <devdoc>
        /// <para>Raises the <see langword='RowDeleted '/>event.</para>
        /// </devdoc>
        protected virtual void OnRowDeleted(GridViewDeletedEventArgs e) {
            GridViewDeletedEventHandler handler = (GridViewDeletedEventHandler)Events[EventRowDeleted];
            if (handler != null) handler(this, e);
        }

        /// <devdoc>
        /// <para>Raises the <see langword='Delete'/> event.</para>
        /// </devdoc>
        protected virtual void OnRowDeleting(GridViewDeleteEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            GridViewDeleteEventHandler handler = (GridViewDeleteEventHandler)Events[EventRowDeleting];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.GridView_UnhandledEvent, ID, "RowDeleting"));
                }
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='EditCommand'/> event.</para>
        /// </devdoc>
        protected virtual void OnRowEditing(GridViewEditEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            GridViewEditEventHandler handler = (GridViewEditEventHandler)Events[EventRowEditing];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.GridView_UnhandledEvent, ID, "RowEditing"));
                }
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='RowUpdated '/>event.</para>
        /// </devdoc>
        protected virtual void OnRowUpdated(GridViewUpdatedEventArgs e) {
            GridViewUpdatedEventHandler handler = (GridViewUpdatedEventHandler)Events[EventRowUpdated];
            if (handler != null) handler(this, e);
        }

        /// <devdoc>
        /// <para>Raises the <see langword='UpdateCommand'/> event.</para>
        /// </devdoc>
        protected virtual void OnRowUpdating(GridViewUpdateEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            GridViewUpdateEventHandler handler = (GridViewUpdateEventHandler)Events[EventRowUpdating];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.GridView_UnhandledEvent, ID, "RowUpdating"));
                }
            }
        }

        /// <devdoc>
        /// <para>Raises the <see cref='System.Web.UI.WebControls.GridView.SelectedIndexChanged'/>event of a <see cref='System.Web.UI.WebControls.GridView'/>.</para>
        /// </devdoc>
        protected virtual void OnSelectedIndexChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventSelectedIndexChanged];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see cref='System.Web.UI.WebControls.GridView.SelectedIndexChanging'/>event of a <see cref='System.Web.UI.WebControls.GridView'/>.</para>
        /// </devdoc>
        protected virtual void OnSelectedIndexChanging(GridViewSelectEventArgs e) {
            GridViewSelectEventHandler handler = (GridViewSelectEventHandler)Events[EventSelectedIndexChanging];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see cref='System.Web.UI.WebControls.GridView.Sorted'/>event of a <see cref='System.Web.UI.WebControls.GridView'/>.</para>
        /// </devdoc>
        protected virtual void OnSorted(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventSorted];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='SortCommand'/> event.</para>
        /// </devdoc>
        protected virtual void OnSorting(GridViewSortEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            GridViewSortEventHandler handler = (GridViewSortEventHandler)Events[EventSorting];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.GridView_UnhandledEvent, ID, "Sorting"));
                }
            }
        }

        private bool PageIsValidAfterModelException() {
            if (_modelValidationGroup == null) {
                return true;
            }
            Page.Validate(_modelValidationGroup);
            return Page.IsValid;
        }

        /// <devdoc>
        /// <para>Parses the information in the hidden field for callbacks and sets members to the values
        ///    in the hidden field.  Returns whether properties changed from what was retrieved from controlstate.</para>
        /// </devdoc>
        private bool ParseHiddenFieldState(string state) {
            string[] arguments = state.Split(new char[] { '|' });
            if (arguments.Length == 4) {
                return LoadHiddenFieldState(arguments[0], arguments[1], arguments[2], arguments[3]);
            }
            return false;
        }

        protected internal override void PerformDataBinding(IEnumerable data) {
            base.PerformDataBinding(data);

            int editIndex = EditIndex;
            if (IsDataBindingAutomatic && editIndex != -1 && editIndex < Rows.Count && IsViewStateEnabled) {
                BoundFieldValues.Clear();
                ExtractRowValues(BoundFieldValues, Rows[editIndex], true/*includeReadOnlyFields*/, false/*includePrimaryKey*/);
            }

            if (EnablePersistedSelection) {
                string[] keyNames = DataKeyNamesInternal;
                //we can't have persisted selection without having at least one key name
                if ((keyNames == null) || (keyNames.Length == 0)) {
                    throw new InvalidOperationException(SR.GetString(SR.GridView_PersistedSelectionRequiresDataKeysNames));
                }
            }

        }

        private void ApplySortingStyle(TableCell cell, DataControlField field, TableItemStyle ascendingStyle, TableItemStyle descendingStyle) {
            if (!String.IsNullOrEmpty(SortExpression) && String.Equals(field.SortExpression, SortExpression, StringComparison.OrdinalIgnoreCase)) {
                if (SortDirection == SortDirection.Ascending) {
                    cell.MergeStyle(ascendingStyle);
                }
                else {
                    cell.MergeStyle(descendingStyle);
                }
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal virtual void PrepareControlHierarchy() {
            if (Controls.Count == 0)
                return;

            bool controlStyleCreated = ControlStyleCreated;
            Table childTable = (Table)Controls[0];
            childTable.CopyBaseAttributes(this);
            if (controlStyleCreated && !ControlStyle.IsEmpty) {
                childTable.ApplyStyle(ControlStyle);
            }
            else {
                // Since we didn't create a ControlStyle yet, the default
                // settings for the default style of the control need to be applied
                // to the child table control directly
                // 

                childTable.GridLines = GridLines.Both;
                childTable.CellSpacing = 0;
            }
            childTable.Caption = Caption;
            childTable.CaptionAlign = CaptionAlign;

            TableRowCollection rows = childTable.Rows;

            // the composite alternating row style, so we need to do just one
            // merge style on the actual row
            Style altRowStyle = null;
            if (_alternatingRowStyle != null) {
                altRowStyle = new TableItemStyle();
                altRowStyle.CopyFrom(_rowStyle);
                altRowStyle.CopyFrom(_alternatingRowStyle);
            }
            else {
                altRowStyle = _rowStyle;
            }

            int visibleColumns = 0;
            bool calculateColumns = true;
            foreach (GridViewRow row in rows) {
                switch (row.RowType) {
                    case DataControlRowType.Header:
                        if (ShowHeader && _headerStyle != null) {
                            row.MergeStyle(_headerStyle);
                        }
                        break;

                    case DataControlRowType.Footer:
                        if (ShowFooter && _footerStyle != null) {
                            row.MergeStyle(_footerStyle);
                        }
                        break;

                    case DataControlRowType.Pager:
                        if (row.Visible && _pagerStyle != null) {
                            row.MergeStyle(_pagerStyle);
                        }
                        break;

                    case DataControlRowType.DataRow:
                        if ((row.RowState & DataControlRowState.Edit) != 0) {
                            // When creating the control hierarchy, we first check if the
                            // row is in edit mode. So an row may be selected too, and
                            // so both editRowStyle (more specific) and selectedRowStyle
                            // are applied.
                            {
                                Style s = new TableItemStyle();

                                if (row.RowIndex % 2 != 0)
                                    s.CopyFrom(altRowStyle);
                                else
                                    s.CopyFrom(_rowStyle);
                                if (row.RowIndex == SelectedIndex)
                                    s.CopyFrom(_selectedRowStyle);
                                s.CopyFrom(_editRowStyle);
                                row.MergeStyle(s);
                            }
                        }
                        else if ((row.RowState & DataControlRowState.Selected) != 0) {
                            // When creating the control hierarchy we first check if the
                            // row is in edit mode, so we know this row cannot be in edit
                            // mode. The only special characteristic of this row is that
                            // it is selected.
                            {
                                Style s = new TableItemStyle();

                                if (row.RowIndex % 2 != 0)
                                    s.CopyFrom(altRowStyle);
                                else
                                    s.CopyFrom(_rowStyle);
                                s.CopyFrom(_selectedRowStyle);
                                row.MergeStyle(s);
                            }
                        }
                        else if ((row.RowState & DataControlRowState.Alternate) != 0) {
                            row.MergeStyle(altRowStyle);
                        }
                        else {
                            row.MergeStyle(_rowStyle);
                        }
                        break;
                    case DataControlRowType.EmptyDataRow:
                        row.MergeStyle(_emptyDataRowStyle);
                        break;
                }

                // Apply the sorting style if the row is not selected or the row is selected and there was no specified SelectedRowStyle
                bool applyCellSortingStyles = (row.RowState & DataControlRowState.Selected) == 0 ||
                                              ((row.RowState & DataControlRowState.Selected) != 0 && _selectedRowStyle == null);

                if ((row.RowType != DataControlRowType.Pager) && (row.RowType != DataControlRowType.EmptyDataRow)) {
                    foreach (TableCell cell in row.Cells) {
                        DataControlFieldCell fieldCell = cell as DataControlFieldCell;
                        if (fieldCell != null) {
                            DataControlField field = fieldCell.ContainingField;
                            if (field != null) {
                                if (field.Visible == false) {
                                    cell.Visible = false;
                                }
                                else {
                                    if (row.RowType == DataControlRowType.DataRow && calculateColumns) {
                                        visibleColumns++;
                                    }
                                    Style cellStyle = null;

                                    switch (row.RowType) {
                                        case DataControlRowType.Header:
                                            cellStyle = field.HeaderStyleInternal;
                                            ApplySortingStyle(cell, field, _sortedAscendingHeaderStyle, _sortedDescendingHeaderStyle);
                                            break;
                                        case DataControlRowType.Footer:
                                            cellStyle = field.FooterStyleInternal;
                                            break;
                                        case DataControlRowType.DataRow:
                                            cellStyle = field.ItemStyleInternal;
                                            if (applyCellSortingStyles) {
                                                ApplySortingStyle(cell, field, _sortedAscendingCellStyle, _sortedDescendingCellStyle);
                                            }
                                            break;
                                        default:
                                            cellStyle = field.ItemStyleInternal;
                                            break;
                                    }
                                    if (cellStyle != null) {
                                        cell.MergeStyle(cellStyle);
                                    }

                                    if (row.RowType == DataControlRowType.DataRow) {
                                        foreach (Control control in cell.Controls) {
                                            WebControl webControl = control as WebControl;
                                            Style fieldControlStyle = field.ControlStyleInternal;
                                            if (webControl != null && fieldControlStyle != null && !fieldControlStyle.IsEmpty) {
                                                webControl.ControlStyle.CopyFrom(fieldControlStyle);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (row.RowType == DataControlRowType.DataRow) {
                        calculateColumns = false;
                    }
                }
            }
            if (Rows.Count > 0 && visibleColumns != Rows[0].Cells.Count) {
                if (_topPagerRow != null && _topPagerRow.Cells.Count > 0) {
                    _topPagerRow.Cells[0].ColumnSpan = visibleColumns;
                }
                if (_bottomPagerRow != null && _bottomPagerRow.Cells.Count > 0) {
                    _bottomPagerRow.Cells[0].ColumnSpan = visibleColumns;
                }
            }
        }

        protected virtual void RaiseCallbackEvent(string eventArgument) {
            string[] arguments = eventArgument.Split(new char[] { '|' });
            Debug.Assert((arguments != null && (arguments.Length == 8)), "An unexpected number of params came through on " + eventArgument);
            IStateFormatter2 formatter = StateFormatter;

            ValidateEvent(UniqueID, "\"" + arguments[0] + "|" + arguments[1] + "|" + arguments[2] + "|" + arguments[3] + "\"");

            LoadHiddenFieldState(arguments[4], arguments[5], arguments[6], arguments[7]);

            int pageNumber = Int32.Parse(arguments[0], CultureInfo.InvariantCulture);
            string sortExpressionSerialized = arguments[2];
            int sortDirection = Int32.Parse(arguments[1], CultureInfo.InvariantCulture);

            if (pageNumber == PageIndex) {
                // just the sortDirection or sortExpression changed
                SortDirection newSortDirection = SortDirection.Ascending;
                string sortExpression = (string)formatter.Deserialize(sortExpressionSerialized, Purpose.WebForms_GridView_SortExpression);
                if ((sortExpression == SortExpressionInternal) && (SortDirectionInternal == SortDirection.Ascending)) {
                    newSortDirection = SortDirection.Descending;
                }
                SortExpressionInternal = sortExpression;
                SortDirectionInternal = newSortDirection;
                _pageIndex = 0;
            }
            else {
                EditIndex = -1;
                _pageIndex = pageNumber;
            }

            DataBind();
        }

        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(UniqueID, eventArgument);

            int separatorIndex = eventArgument.IndexOf('$');
            if (separatorIndex < 0) {
                return;
            }

            CommandEventArgs cea = new CommandEventArgs(eventArgument.Substring(0, separatorIndex), eventArgument.Substring(separatorIndex + 1));

            GridViewCommandEventArgs gvcea = new GridViewCommandEventArgs(null, this, cea);
            HandleEvent(gvcea, false, String.Empty);
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Displays the control on the client.</para>
        /// </devdoc>
        protected internal override void Render(HtmlTextWriter writer) {
            // we don't render the outer div at design time because the designer surface 
            // needs a top-level layout element
            Render(writer, !DesignMode);
        }

        private void Render(HtmlTextWriter writer, bool renderPanel) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }
            PrepareControlHierarchy();
            if (renderPanel) {
                string clientID = ClientID;
                if (DetermineRenderClientScript()) {
                    if (clientID == null) {
                        throw new HttpException(SR.GetString(SR.GridView_MustBeParented));
                    }
                    else {
                        StringBuilder clientPanelNameBuilder = new StringBuilder("__gv", clientID.Length + 9);
                        clientPanelNameBuilder.Append(clientID);
                        clientPanelNameBuilder.Append("__div");
                        writer.AddAttribute(HtmlTextWriterAttribute.Id, clientPanelNameBuilder.ToString(), true);
                    }
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
            }
            RenderContents(writer);
            if (renderPanel) {
                writer.RenderEndTag();
            }
        }

        private void RenderTableContents(HtmlTextWriter writer) {
            Render(writer, false);
        }

        private void ResetModelValidationGroup(bool causesValidation, string validationGroup) {
            _modelValidationGroup = null;
            if (causesValidation) {
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
                _editIndex != -1 ||
                _selectedIndex != -1 ||
                (_sortExpression != null && _sortExpression.Length != 0) ||
                _sortDirection != SortDirection.Ascending ||
                (_dataKeyNames != null && _dataKeyNames.Length != 0) ||
                (_dataKeysArrayList != null && _dataKeysArrayList.Count > 0) ||
                _pageCount != -1) {

                object[] state = new object[12];

                state[0] = baseState;
                state[1] = (_editIndex == -1) ? null : (object)_editIndex;
                state[2] = (_pageIndex == 0) ? null : (object)_pageIndex;
                state[3] = (_selectedIndex == -1) ? null : (object)_selectedIndex;
                state[4] = (_sortExpression == null || _sortExpression.Length == 0) ? null : (object)_sortExpression;
                state[5] = (_sortDirection == SortDirection.Ascending) ? null : (object)((int)_sortDirection);
                state[6] = (_dataKeyNames == null || _dataKeyNames.Length == 0) ? null : (object)_dataKeyNames;
                state[7] = SaveDataKeysState();
                state[8] = _pageCount;
                state[9] = (_persistedDataKey == null) ? null : ((IStateManager)_persistedDataKey).SaveViewState();
                state[10] = (_clientIDRowSuffix == null || _clientIDRowSuffix.Length == 0) ? null : (object)_clientIDRowSuffix;
                state[11] = SaveClientIDRowSuffixDataKeysState();

                return state;
            }
            return true;    // return a dummy that ensures LoadControlState gets called but minimizes persisted size.
        }

        private object SaveDataKeysState() {
            object keyState = new object();
            int dataKeyCount = 0;

            if (_dataKeysArrayList != null && _dataKeysArrayList.Count > 0) {
                dataKeyCount = _dataKeysArrayList.Count;
                keyState = new object[dataKeyCount];
                for (int i = 0; i < dataKeyCount; i++) {
                    ((object[])keyState)[i] = ((IStateManager)_dataKeysArrayList[i]).SaveViewState();
                }
            }
            return (_dataKeysArrayList == null || dataKeyCount == 0) ? null : keyState;
        }

        private object SaveClientIDRowSuffixDataKeysState() {
            object keyState = new object();
            int dataKeyCount = 0;
            if (_clientIDRowSuffixArrayList != null && _clientIDRowSuffixArrayList.Count > 0) {
                dataKeyCount = _clientIDRowSuffixArrayList.Count;
                keyState = new object[dataKeyCount];
                for (int i = 0; i < dataKeyCount; i++) {
                    ((object[])keyState)[i] = ((IStateManager)_clientIDRowSuffixArrayList[i]).SaveViewState();
                }
            }
            return (_clientIDRowSuffixArrayList == null || dataKeyCount == 0) ? null : keyState;
        }

        /// <internalonly/>
        /// <devdoc>
        /// <para>Saves the current state of the <see cref='System.Web.UI.WebControls.GridView'/>.</para>
        /// </devdoc>
        protected override object SaveViewState() {
            object baseState = base.SaveViewState();
            object fieldState = (_fieldCollection != null) ? ((IStateManager)_fieldCollection).SaveViewState() : null;
            object pagerStyleState = (_pagerStyle != null) ? ((IStateManager)_pagerStyle).SaveViewState() : null;
            object headerStyleState = (_headerStyle != null) ? ((IStateManager)_headerStyle).SaveViewState() : null;
            object footerStyleState = (_footerStyle != null) ? ((IStateManager)_footerStyle).SaveViewState() : null;
            object rowStyleState = (_rowStyle != null) ? ((IStateManager)_rowStyle).SaveViewState() : null;
            object alternatingRowStyleState = (_alternatingRowStyle != null) ? ((IStateManager)_alternatingRowStyle).SaveViewState() : null;
            object selectedRowStyleState = (_selectedRowStyle != null) ? ((IStateManager)_selectedRowStyle).SaveViewState() : null;
            object editRowStyleState = (_editRowStyle != null) ? ((IStateManager)_editRowStyle).SaveViewState() : null;
            object boundFieldValuesState = (_boundFieldValues != null) ? OrderedDictionaryStateHelper.SaveViewState(_boundFieldValues) : null;
            object pagerSettingsState = (_pagerSettings != null) ? ((IStateManager)_pagerSettings).SaveViewState() : null;
            object controlState = ControlStyleCreated ? ((IStateManager)ControlStyle).SaveViewState() : null;
            object sortedAscendingCellStyleState = (_sortedAscendingCellStyle != null) ? ((IStateManager)_sortedAscendingCellStyle).SaveViewState() : null;
            object sortedDescendingCellStyleState = (_sortedDescendingCellStyle != null) ? ((IStateManager)_sortedDescendingCellStyle).SaveViewState() : null;
            object sortedAscendingHeaderStyleState = (_sortedAscendingHeaderStyle != null) ? ((IStateManager)_sortedAscendingHeaderStyle).SaveViewState() : null;
            object sortedDescendingHeaderStyleState = (_sortedDescendingHeaderStyle != null) ? ((IStateManager)_sortedDescendingHeaderStyle).SaveViewState() : null;

            object[] myState = new object[17];
            myState[0] = baseState;
            myState[1] = fieldState;
            myState[2] = pagerStyleState;
            myState[3] = headerStyleState;
            myState[4] = footerStyleState;
            myState[5] = rowStyleState;
            myState[6] = alternatingRowStyleState;
            myState[7] = selectedRowStyleState;
            myState[8] = editRowStyleState;
            myState[9] = pagerSettingsState;
            myState[10] = boundFieldValuesState;
            myState[11] = controlState;
            myState[12] = ColumnsGeneratorInternal is IStateManager ? ((IStateManager)ColumnsGeneratorInternal).SaveViewState() : null;
            myState[13] = sortedAscendingCellStyleState;
            myState[14] = sortedDescendingCellStyleState;
            myState[15] = sortedAscendingHeaderStyleState;
            myState[16] = sortedDescendingHeaderStyleState;

            // note that we always have some state, atleast the RowCount
            return myState;
        }

        private void ResetPersistedSelectedIndex() {
            // If there is already a persisted DataKey then we should reset
            // the selected index so that we pick a selected index base on 
            // a row that matches the DataKey if any
            if (EnablePersistedSelection && (_persistedDataKey != null)) {
                _selectedIndex = -1;
            }
        }

        private void SetPersistedDataKey(int dataItemIndex, DataKey currentKey) {
            if (_persistedDataKey == null) {
                // If there is no persisted DataKey then set it to the DataKey at the
                // the selected index
                if (_selectedIndex == dataItemIndex) {
                    _persistedDataKey = currentKey;
                }
            }
            else if (_persistedDataKey.Equals(currentKey)) {
                // Persist the selection by picking the selected index where DataKeys match.
                _selectedIndex = dataItemIndex;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
          Justification = "A property already exists. This method does additional work.")]
        public void SetPageIndex(int rowIndex) {
            HandlePage(rowIndex);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
          Justification = "A property already exists. This method does additional work.")]
        public void SelectRow(int rowIndex) {
            HandleSelect(rowIndex);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
          Justification = "A property already exists. This method does additional work.")]
        public void SetEditRow(int rowIndex) {
            HandleEdit(rowIndex);
        }

        private void SelectCallback(IEnumerable data) {
            // The data source should have thrown.  If we're here, it didn't.  We'll throw for it
            // with a generic message.
            throw new HttpException(SR.GetString(SR.DataBoundControl_DataSourceDoesntSupportPaging, DataSourceID));
        }

        public virtual void Sort(string sortExpression, SortDirection sortDirection) {
            HandleSort(sortExpression, sortDirection);
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Marks the starting point to begin tracking and saving changes to the
        ///       control as part of the control viewstate.</para>
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (_fieldCollection != null)
                ((IStateManager)_fieldCollection).TrackViewState();
            if (_pagerStyle != null)
                ((IStateManager)_pagerStyle).TrackViewState();
            if (_headerStyle != null)
                ((IStateManager)_headerStyle).TrackViewState();
            if (_footerStyle != null)
                ((IStateManager)_footerStyle).TrackViewState();
            if (_rowStyle != null)
                ((IStateManager)_rowStyle).TrackViewState();
            if (_sortedAscendingCellStyle != null)
                ((IStateManager)_sortedAscendingCellStyle).TrackViewState();
            if (_sortedDescendingCellStyle != null)
                ((IStateManager)_sortedDescendingCellStyle).TrackViewState();
            if (_sortedAscendingHeaderStyle != null)
                ((IStateManager)_sortedAscendingHeaderStyle).TrackViewState();
            if (_sortedDescendingHeaderStyle != null)
                ((IStateManager)_sortedDescendingHeaderStyle).TrackViewState();
            if (_alternatingRowStyle != null)
                ((IStateManager)_alternatingRowStyle).TrackViewState();
            if (_selectedRowStyle != null)
                ((IStateManager)_selectedRowStyle).TrackViewState();
            if (_editRowStyle != null)
                ((IStateManager)_editRowStyle).TrackViewState();
            if (_pagerSettings != null)
                ((IStateManager)_pagerSettings).TrackViewState();
            if (ControlStyleCreated)
                ((IStateManager)ControlStyle).TrackViewState();
            if (_dataKeyArray != null)
                ((IStateManager)_dataKeyArray).TrackViewState();
        }

        internal override void UpdateModelDataSourceProperties(ModelDataSource modelDataSource) {
            Debug.Assert(modelDataSource != null, "A non-null ModelDataSource should be passed in");
            string dataKeyName = DataKeyNamesInternal.Length > 0 ? DataKeyNamesInternal[0] : "";
            modelDataSource.UpdateProperties(ItemType, SelectMethod, UpdateMethod, InsertMethod, DeleteMethod, dataKeyName);
        }

        public virtual void UpdateRow(int rowIndex, bool causesValidation) {
            ResetModelValidationGroup(causesValidation, String.Empty);
            HandleUpdate(null, rowIndex, causesValidation);
        }

        #region IPostBackEventHandler implementation
        /// <devdoc>
        /// Called when a post back event is being raised.  GridView uses this to handle creating CommandEventArgs out of our
        /// shortened commandargument notation.  This prevents us from having to rebuild the control tree just to handle an
        /// event.
        /// </devdoc>
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion

        #region IPostBackContainer implementation
        /// <internalonly/>
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

        #region ICallbackContainer implementation
        string ICallbackContainer.GetCallbackScript(IButtonControl buttonControl, string argument) {
            return GetCallbackScript(buttonControl, argument);
        }
        #endregion

        #region ICallbackEventHandler implementation
        void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument) {
            RaiseCallbackEvent(eventArgument);
        }

        // The return value of this function is the argument to the callback handler in
        // GetCallbackEventReference.
        string ICallbackEventHandler.GetCallbackResult() {
            return GetCallbackResult();
        }
        #endregion

        #region IPersistedSelector implementation

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
                         Justification = "SelectedPersistedDataKey provides the functionality.")]
        DataKey IPersistedSelector.DataKey {
            get {
                return SelectedPersistedDataKey;
            }
            set {
                SelectedPersistedDataKey = value;
            }
        }

        #endregion

        #region IDataKeysControl implementation
        DataKeyArray IDataKeysControl.ClientIDRowSuffixDataKeys {
            get {
                return ClientIDRowSuffixDataKeys;
            }
        }
        #endregion

        #region IDataBoundListControl implementation

        DataKeyArray IDataBoundListControl.DataKeys {
            get {
                return DataKeys;
            }
        }

        DataKey IDataBoundListControl.SelectedDataKey {
            get {
                return SelectedDataKey;
            }
        }

        int IDataBoundListControl.SelectedIndex {
            get {
                return SelectedIndex;
            }
            set {
                SelectedIndex = value;
            }
        }

        string[] IDataBoundListControl.ClientIDRowSuffix {
            get {
                return ClientIDRowSuffix;
            }
            set {
                ClientIDRowSuffix = value;
            }
        }

        bool IDataBoundListControl.EnablePersistedSelection {
            get {
                return EnablePersistedSelection;
            }
            set {
                EnablePersistedSelection = value;
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

        #region IFieldControl implementation

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "The underlying implementation is not meant to be overridden.")]
        IAutoFieldGenerator IFieldControl.FieldsGenerator {
            get {
                return ColumnsGenerator;
            }
            set {
                ColumnsGenerator = value;
            }
        }

        #endregion
    }
}
