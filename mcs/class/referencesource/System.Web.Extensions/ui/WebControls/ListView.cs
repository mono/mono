//------------------------------------------------------------------------------
// <copyright file="ListView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Web.Resources;
using System.Web.UI.HtmlControls;
using System.Web.Util;


namespace System.Web.UI.WebControls {

    [DefaultProperty("SelectedValue")]
    [Designer("System.Web.UI.Design.WebControls.ListViewDesigner, " + AssemblyRef.SystemWebExtensionsDesign)]
    [ControlValueProperty("SelectedValue")]
    [DefaultEvent("SelectedIndexChanged")]
    [SupportsEventValidation]
    [ToolboxBitmap(typeof(ListView), "ListView.bmp")]
    [DataKeyProperty("SelectedPersistedDataKey")]
    public class ListView : DataBoundControl, INamingContainer, IPageableItemContainer, IPersistedSelector, IDataKeysControl, IDataBoundListControl, IWizardSideBarListControl {
        internal const string ItemCountViewStateKey = "_!ItemCount";

        private ITemplate _itemTemplate;
        private ITemplate _editItemTemplate;
        private ITemplate _insertItemTemplate;
        private ITemplate _layoutTemplate;
        private ITemplate _selectedItemTemplate;
        private ITemplate _groupTemplate;
        private ITemplate _itemSeparatorTemplate;
        private ITemplate _groupSeparatorTemplate;
        private ITemplate _emptyItemTemplate;
        private ITemplate _emptyDataTemplate;
        private ITemplate _alternatingItemTemplate;

        private static readonly object EventTotalRowCountAvailable = new object();
        private static readonly object EventPagePropertiesChanged = new object();
        private static readonly object EventPagePropertiesChanging = new object();
        private static readonly object EventItemCanceling = new object();
        private static readonly object EventItemCommand = new object();
        private static readonly object EventItemCreated = new object();
        private static readonly object EventItemDataBound = new object();
        private static readonly object EventItemDeleted = new object();
        private static readonly object EventItemDeleting = new object();
        private static readonly object EventItemEditing = new object();
        private static readonly object EventItemInserted = new object();
        private static readonly object EventItemInserting = new object();
        private static readonly object EventItemUpdated = new object();
        private static readonly object EventItemUpdating = new object();
        private static readonly object EventLayoutCreated = new object();
        private static readonly object EventSelectedIndexChanging = new object();
        private static readonly object EventSelectedIndexChanged = new object();
        private static readonly object EventSorted = new object();
        private static readonly object EventSorting = new object();
        private static readonly object EventWizardListItemDataBound = new object();

        private bool _performingSelect;
        private int _editIndex = -1;
        private int _selectedIndex = -1;
        private int _groupItemCount = 1;
        private string _modelValidationGroup;
        private string _sortExpression = String.Empty;
        private SortDirection _sortDirection = SortDirection.Ascending;

        private int _startRowIndex = 0;
        private int _maximumRows = -1;
        private int _totalRowCount = -1;

        private IList<ListViewDataItem> _itemList;
        private ListViewItem _insertItem;

        private string[] _dataKeyNames;
        private string[] _clientIDRowSuffix;
        private DataKeyArray _dataKeyArray;
        private ArrayList _dataKeysArrayList;
        private DataKeyArray _clientIDRowSuffixArray;
        private ArrayList _clientIDRowSuffixArrayList;
        private OrderedDictionary _boundFieldValues;
        private DataKey _persistedDataKey;

        private int _deletedItemIndex;
        private IOrderedDictionary _deleteKeys;
        private IOrderedDictionary _deleteValues;
        private IOrderedDictionary _insertValues;
        private IOrderedDictionary _updateKeys;
        private IOrderedDictionary _updateOldValues;
        private IOrderedDictionary _updateNewValues;

        private int _autoIDIndex = 0;
        private const string _automaticIDPrefix = "ctrl";

        private bool _instantiatedEmptyDataTemplate = false;

        // Keep track of where we instantiated templates when we're not using grouping
        private int _noGroupsOriginalIndexOfItemPlaceholderInContainer = -1;
        private int _noGroupsItemCreatedCount;
        private Control _noGroupsItemPlaceholderContainer;

        // Keep track of where we instantiated templates when we're using grouping
        private int _groupsOriginalIndexOfGroupPlaceholderInContainer = -1;
        private int _groupsItemCreatedCount;
        private Control _groupsGroupPlaceholderContainer;

        private string _updateMethod;
        private string _insertMethod;
        private string _deleteMethod;

        public ListView() {
        }

        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override string AccessKey {
            get {
                return base.AccessKey;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListViewDataItem), BindingDirection.TwoWay),
        ResourceDescription("ListView_AlternatingItemTemplate")
        ]
        public virtual ITemplate AlternatingItemTemplate {
            get {
                return _alternatingItemTemplate;
            }
            set {
                _alternatingItemTemplate = value;
            }
        }

        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override Color BackColor {
            get {
                return base.BackColor;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
            }
        }

        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override Color BorderColor {
            get {
                return base.BorderColor;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
            }
        }


        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override Unit BorderWidth {
            get {
                return base.BorderWidth;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
            }
        }


        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override BorderStyle BorderStyle {
            get {
                return base.BorderStyle;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
            }
        }

        private IOrderedDictionary BoundFieldValues {
            get {
                if (_boundFieldValues == null) {
                    _boundFieldValues = new OrderedDictionary();
                }
                return _boundFieldValues;
            }
        }

        public override ControlCollection Controls {
            get {
                EnsureChildControls();
                return base.Controls;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the property that determines whether the control treats empty string as
        ///    null when the item values are extracted.</para>
        /// </devdoc>
        [
        Category("Behavior"),
        DefaultValue(true),
        ResourceDescription("ListView_ConvertEmptyStringToNull"),
        ]
        public virtual bool ConvertEmptyStringToNull {
            get {
                object o = ViewState["ConvertEmptyStringToNull"];
                if (o != null) {
                    return (bool)o;
                }
                return true;
            }
            set {
                ViewState["ConvertEmptyStringToNull"] = value;
            }
        }

        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never),
        CssClassPropertyAttribute
        ]
        public override string CssClass {
            get {
                return base.CssClass;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
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

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResourceDescription("ListView_DataKeys")
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

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID"),
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
        Category("Data"),
        ResourceDescription("ListView_DataKeyNames"),
        SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
                        Justification = "Required by ASP.NET parser."),
        TypeConverterAttribute(typeof(StringArrayConverter)),
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
                    SetRequiresDataBindingIfInitialized();
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


        [
        Category("Default"),
        DefaultValue(-1),
        ResourceDescription("ListView_EditIndex")
        ]
        public virtual int EditIndex {
            get {
                return _editIndex;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != _editIndex) {
                    if (value == -1) {
                        BoundFieldValues.Clear();
                    }
                    _editIndex = value;
                    SetRequiresDataBindingIfInitialized();
                }
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResourceDescription("ListView_EditItem")
        ]
        public virtual ListViewItem EditItem {
            get {
                if (_editIndex > -1 && _editIndex < Items.Count) {
                    return Items[_editIndex];
                }
                return null;
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListViewDataItem), BindingDirection.TwoWay),
        ResourceDescription("ListView_EditItemTemplate"),
        ]
        public virtual ITemplate EditItemTemplate {
            get {
                return _editItemTemplate;
            }
            set {
                _editItemTemplate = value;
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListView)),
        ResourceDescription("ListView_EmptyDataTemplate"),
        ]
        public virtual ITemplate EmptyDataTemplate {
            get {
                return _emptyDataTemplate;
            }
            set {
                _emptyDataTemplate = value;
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListViewItem)),
        ResourceDescription("ListView_EmptyItemTemplate"),
        ]
        public virtual ITemplate EmptyItemTemplate {
            get {
                return _emptyItemTemplate;
            }
            set {
                _emptyItemTemplate = value;
            }
        }

        [
        WebCategory("Behavior"),
        DefaultValue(true),
        ResourceDescription("ListView_EnableModelValidation")
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
        ResourceDescription("ListView_EnablePersistedSelection")
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

        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override FontInfo Font {
            get {
                return base.Font;
            }
        }

        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override Color ForeColor {
            get {
                return base.ForeColor;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
            }
        }

        [
        DefaultValue("groupPlaceholder"),
        Category("Behavior"),
        ResourceDescription("ListView_GroupPlaceholderID"),
        SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")
        ]
        public virtual String GroupPlaceholderID {
            get {
                object o = ViewState["GroupPlaceholderID"];
                if (o != null) {
                    return (String)o;
                }
                return "groupPlaceholder";
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_ContainerNameMustNotBeEmpty, "GroupPlaceholderID"));
                }
                ViewState["GroupPlaceholderID"] = value;
            }
        }

        [
        Category("Default"),
        DefaultValue(1),
        ResourceDescription("ListView_GroupItemCount"),
        ]
        public virtual int GroupItemCount {
            get {
                return _groupItemCount;
            }
            set {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _groupItemCount = value;
                SetRequiresDataBindingIfInitialized();
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListViewItem)),
        ResourceDescription("ListView_GroupSeparatorTemplate"),
        ]
        public virtual ITemplate GroupSeparatorTemplate {
            get {
                return _groupSeparatorTemplate;
            }
            set {
                _groupSeparatorTemplate = value;
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListViewItem)),
        ResourceDescription("ListView_GroupTemplate"),
        ]
        public virtual ITemplate GroupTemplate {
            get {
                return _groupTemplate;
            }
            set {
                _groupTemplate = value;
            }
        }

        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override Unit Height {
            get {
                return base.Height;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResourceDescription("ListView_InsertItem")
        ]
        public virtual ListViewItem InsertItem {
            get {
                return _insertItem;
            }
        }

        [
        Category("Default"),
        DefaultValue(InsertItemPosition.None),
        ResourceDescription("ListView_InsertItemPosition")
        ]
        public virtual InsertItemPosition InsertItemPosition {
            get {
                object o = ViewState["InsertItemPosition"];
                if (o != null) {
                    return (InsertItemPosition)o;
                }
                return InsertItemPosition.None;
            }
            set {
                if (InsertItemPosition != value) {
                    ViewState["InsertItemPosition"] = value;
                    SetRequiresDataBindingIfInitialized();
                }
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListViewItem), BindingDirection.TwoWay),
        ResourceDescription("ListView_InsertItemTemplate"),
        ]
        public virtual ITemplate InsertItemTemplate {
            get {
                return _insertItemTemplate;
            }
            set {
                _insertItemTemplate = value;
            }
        }

        [
        DefaultValue("itemPlaceholder"),
        Category("Behavior"),
        ResourceDescription("ListView_ItemPlaceholderID"),
        SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")
        ]
        public virtual String ItemPlaceholderID {
            get {
                object o = ViewState["ItemPlaceholderID"];
                if (o != null) {
                    return (String)o;
                }
                return "itemPlaceholder";
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    throw new ArgumentOutOfRangeException("value", String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_ContainerNameMustNotBeEmpty, "ItemPlaceholderID"));
                }
                ViewState["ItemPlaceholderID"] = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResourceDescription("ListView_Items")
        ]
        public virtual IList<ListViewDataItem> Items {
            get {
                if (_itemList == null) {
                    _itemList = new List<ListViewDataItem>();
                }
                return _itemList;
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListViewItem)),
        ResourceDescription("ListView_ItemSeparatorTemplate"),
        ]
        public virtual ITemplate ItemSeparatorTemplate {
            get {
                return _itemSeparatorTemplate;
            }
            set {
                _itemSeparatorTemplate = value;
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListViewDataItem), BindingDirection.TwoWay),
        ResourceDescription("ListView_ItemTemplate"),
        SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")
        ]
        public virtual ITemplate ItemTemplate {
            get {
                return _itemTemplate;
            }
            set {
                _itemTemplate = value;
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListView)),
        ResourceDescription("ListView_LayoutTemplate"),
        ]
        public virtual ITemplate LayoutTemplate {
            get {
                return _layoutTemplate;
            }
            set {
                _layoutTemplate = value;
            }
        }

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

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual DataKey SelectedDataKey {
            get {
                if (DataKeyNamesInternal == null || DataKeyNamesInternal.Length == 0) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_DataKeyNamesMustBeSpecified, ID));
                }

                DataKeyArray keys = DataKeys;
                int selectedIndex = SelectedIndex;
                if (keys != null && selectedIndex < keys.Count && selectedIndex > -1) {
                    return keys[selectedIndex];
                }
                return null;
            }
        }

        [
        Category("Default"),
        DefaultValue(-1),
        ResourceDescription("ListView_SelectedIndex"),
        SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")
        ]
        public virtual int SelectedIndex {
            get {
                return _selectedIndex;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != _selectedIndex) {
                    // update the virtual selection to use the new selection
                    _selectedIndex = value;

                    if (EnablePersistedSelection && (DataKeyNamesInternal.Length > 0)) {
                        SelectedPersistedDataKey = SelectedDataKey;
                    }

                    // we're going to rebind here for a new template
                    SetRequiresDataBindingIfInitialized();
                }
            }
        }

        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(ListViewDataItem), BindingDirection.TwoWay),
        ResourceDescription("ListView_SelectedItemTemplate"),
        ]
        public virtual ITemplate SelectedItemTemplate {
            get {
                return _selectedItemTemplate;
            }
            set {
                _selectedItemTemplate = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
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

        [
        Browsable(false),
        DefaultValue(SortDirection.Ascending),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        PersistenceMode(PersistenceMode.InnerProperty),
        ResourceDescription("ListView_SortDirection"),
        ResourceCategory("Sorting"),
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
                    SetRequiresDataBindingIfInitialized();
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
        ResourceDescription("ListView_SortExpression"),
        ResourceCategory("Sorting"),
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
                    SetRequiresDataBindingIfInitialized();
                }
            }
        }

        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override short TabIndex {
            get {
                return base.TabIndex;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
            }
        }

        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override string ToolTip {
            get {
                return base.ToolTip;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
            }
        }

        [
        Browsable(false)
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

        // Override style properties and throw from setter, and set Browsable(false).
        // Don't throw from getters because designer calls getters through reflection.
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override Unit Width {
            get {
                return base.Width;
            }
            set {
                throw new NotSupportedException(AtlasWeb.ListView_StylePropertiesNotSupported);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.ListView'/> with a
        /// <see langword='delete'/>.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnItemDeleted")
        ]
        public event EventHandler<ListViewDeletedEventArgs> ItemDeleted {
            add {
                Events.AddHandler(EventItemDeleted, value);
            }
            remove {
                Events.RemoveHandler(EventItemDeleted, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.ListView'/> with a
        /// <see langword='insert'/>.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnItemInserted")
        ]
        public event EventHandler<ListViewInsertedEventArgs> ItemInserted {
            add {
                Events.AddHandler(EventItemInserted, value);
            }
            remove {
                Events.RemoveHandler(EventItemInserted, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.ListView'/> with a
        /// <see langword='update'/>.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnItemUpdated")
        ]
        public event EventHandler<ListViewUpdatedEventArgs> ItemUpdated {
            add {
                Events.AddHandler(EventItemUpdated, value);
            }
            remove {
                Events.RemoveHandler(EventItemUpdated, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.ListView'/> with a
        /// <see langword='Command'/> property of
        /// <see langword='cancel'/>.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnItemCanceling")
        ]
        public event EventHandler<ListViewCancelEventArgs> ItemCanceling {
            add {
                Events.AddHandler(EventItemCanceling, value);
            }
            remove {
                Events.RemoveHandler(EventItemCanceling, value);
            }
        }

        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.ListView'/> not covered by
        /// <see langword='edit'/>, <see langword='cancel'/>, <see langword='delete'/> or
        /// <see langword='update'/>.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnItemCommand")
        ]
        public event EventHandler<ListViewCommandEventArgs> ItemCommand {
            add {
                Events.AddHandler(EventItemCommand, value);
            }
            remove {
                Events.RemoveHandler(EventItemCommand, value);
            }
        }

        /// <devdoc>
        ///    <para>Occurs on the server when a control a created.</para>
        /// </devdoc>
        [
        Category("Behavior"),
        ResourceDescription("ListView_OnItemCreated")
        ]
        public event EventHandler<ListViewItemEventArgs> ItemCreated {
            add {
                Events.AddHandler(EventItemCreated, value);
            }
            remove {
                Events.RemoveHandler(EventItemCreated, value);
            }
        }

        /// <devdoc>
        ///    <para>Occurs when an Item is data bound to the control.</para>
        /// </devdoc>
        [
        Category("Data"),
        ResourceDescription("ListView_OnItemDataBound")
        ]
        public event EventHandler<ListViewItemEventArgs> ItemDataBound {
            add {
                Events.AddHandler(EventItemDataBound, value);
            }
            remove {
                Events.RemoveHandler(EventItemDataBound, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.ListView'/> with a
        /// <see langword='delete'/>.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnItemDeleting")
        ]
        public event EventHandler<ListViewDeleteEventArgs> ItemDeleting {
            add {
                Events.AddHandler(EventItemDeleting, value);
            }
            remove {
                Events.RemoveHandler(EventItemDeleting, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.ListView'/> with a
        /// <see langword='Command'/> property of
        /// <see langword='edit'/>.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnItemEditing")
        ]
        public event EventHandler<ListViewEditEventArgs> ItemEditing {
            add {
                Events.AddHandler(EventItemEditing, value);
            }
            remove {
                Events.RemoveHandler(EventItemEditing, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.ListView'/> with a
        /// <see langword='insert'/>.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnItemInserting")
        ]
        public event EventHandler<ListViewInsertEventArgs> ItemInserting {
            add {
                Events.AddHandler(EventItemInserting, value);
            }
            remove {
                Events.RemoveHandler(EventItemInserting, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.ListView'/> with a
        /// <see langword='update'/>.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnItemUpdating")
        ]
        public event EventHandler<ListViewUpdateEventArgs> ItemUpdating {
            add {
                Events.AddHandler(EventItemUpdating, value);
            }
            remove {
                Events.RemoveHandler(EventItemUpdating, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs on the server when a control layout is created.</para>
        /// </devdoc>
        [
        Category("Behavior"),
        ResourceDescription("ListView_OnLayoutCreated")
        ]
        public event EventHandler LayoutCreated {
            add {
                Events.AddHandler(EventLayoutCreated, value);
            }
            remove {
                Events.RemoveHandler(EventLayoutCreated, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs on the server when the page properties have changed.</para>
        /// </devdoc>
        [
        Category("Behavior"),
        ResourceDescription("ListView_OnPagePropertiesChanged")
        ]
        public event EventHandler PagePropertiesChanged {
            add {
                Events.AddHandler(EventPagePropertiesChanged, value);
            }
            remove {
                Events.RemoveHandler(EventPagePropertiesChanged, value);
            }
        }

        /// <devdoc>
        /// <para>Occurs on the server when the page properties are changing.</para>
        /// </devdoc>
        [
        Category("Behavior"),
        ResourceDescription("ListView_OnPagePropertiesChanging")
        ]
        public event EventHandler<PagePropertiesChangingEventArgs> PagePropertiesChanging {
            add {
                Events.AddHandler(EventPagePropertiesChanging, value);
            }
            remove {
                Events.RemoveHandler(EventPagePropertiesChanging, value);
            }
        }

        /// <devdoc>
        ///    <para>Occurs when an Item on the list is selected.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnSelectedIndexChanged")
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
        ///    <para>Occurs when an Item on the list is selected.</para>
        /// </devdoc>
        [
        Category("Action"),
        ResourceDescription("ListView_OnSelectedIndexChanging")
        ]
        public event EventHandler<ListViewSelectEventArgs> SelectedIndexChanging {
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
        Category("Action"),
        ResourceDescription("ListView_OnSorted")
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
        Category("Action"),
        ResourceDescription("ListView_OnSorting")
        ]
        public event EventHandler<ListViewSortEventArgs> Sorting {
            add {
                Events.AddHandler(EventSorting, value);
            }
            remove {
                Events.RemoveHandler(EventSorting, value);
            }
        }

        protected override bool IsUsingModelBinders {
            get {
                return !String.IsNullOrEmpty(SelectMethod) ||
                       !String.IsNullOrEmpty(UpdateMethod) ||
                       !String.IsNullOrEmpty(DeleteMethod) ||
                       !String.IsNullOrEmpty(InsertMethod);
            }
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
        public virtual string UpdateMethod {
            get {
                return _updateMethod ?? String.Empty;
            }
            set {
                if (!String.Equals(_updateMethod, value, StringComparison.OrdinalIgnoreCase)) {
                    _updateMethod = value;
                    OnDataPropertyChanged();
                }
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
        public virtual string DeleteMethod {
            get {
                return _deleteMethod ?? String.Empty;
            }
            set {
                if (!String.Equals(_deleteMethod, value, StringComparison.OrdinalIgnoreCase)) {
                    _deleteMethod = value;
                    OnDataPropertyChanged();
                }
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
        public virtual string InsertMethod {
            get {
                return _insertMethod ?? String.Empty;
            }
            set {
                if (!String.Equals(_insertMethod, value, StringComparison.OrdinalIgnoreCase)) {
                    _insertMethod = value;
                    OnDataPropertyChanged();
                }
            }
        }

        protected virtual void AddControlToContainer(Control control, Control container, int addLocation) {
            // The ListView packages up everything in the ItemTemplate in a ListViewDataItem or ListViewItem.
            // The ListViewItem is being added to the control tree.  Since ListViewItems can't be children of HtmlTables or
            // HtmlTableRows, we put them in a derived HtmlTable or HtmlTableRow, which just renders out its children.
            // Since ListViewItems don't have any rendering, only the child HtmlTableRow or HtmlTableCell will be rendered.

            if (container is HtmlTable) {
                ListViewTableRow listViewTableRow = new ListViewTableRow();
                container.Controls.AddAt(addLocation, listViewTableRow);
                listViewTableRow.Controls.Add(control);
            }
            else {
                if (container is HtmlTableRow) {
                    ListViewTableCell listViewTableCell = new ListViewTableCell();
                    container.Controls.AddAt(addLocation, listViewTableCell);
                    listViewTableCell.Controls.Add(control);
                }
                else {
                    container.Controls.AddAt(addLocation, control);
                }
            }
        }

        private void AutoIDControl(Control control) {
            // We have to do our own auto-id'ing because we create the LayoutTemplate, add controls
            // to the item or group container, then clear those controls out when we bind again.
            // Because the item or group container isn't necessarily a naming container, clearing
            // out its controls collection doesn't reset the auto-id counter and you get new ids.
            // On postback, controls that have post data won't be found because their generated
            // ids won't match the prior ones.  By creating our own auto-id'ing, we don't get
            // Control's auto-id, and we can reset the auto id index when we remove all items from
            // the item or group container.
            control.ID = _automaticIDPrefix + _autoIDIndex++.ToString(CultureInfo.InvariantCulture);
        }

        private void ClearDataKeys() {
            _dataKeysArrayList = null;
        }

        /// <summary>
        /// Overriden by DataBoundControl to determine if the control should
        /// recreate its control hierarchy based on values in view state.
        /// If the control hierarchy should be created, i.e. view state does
        /// exist, it calls CreateChildControls with a dummy (empty) data source
        /// which is usable for enumeration purposes only.
        /// </summary>
        protected internal override void CreateChildControls() {
            object controlCount = ViewState[ItemCountViewStateKey];

            if (controlCount == null && RequiresDataBinding) {
                EnsureDataBound();
            }

            if (controlCount != null && ((int)controlCount) != -1) {
                object[] dummyDataSource = new object[(int)controlCount];
                CreateChildControls(dummyDataSource, false);
                ClearChildViewState();
            }
        }

        /// <summary>
        /// Performs the work of creating the control hierarchy based on a data source.
        /// When dataBinding is true, the specified data source contains real
        /// data, and the data is supposed to be pushed into the UI.
        /// When dataBinding is false, the specified data source is a dummy data
        /// source, that allows enumerating the right number of items, but the items
        /// themselves are null and do not contain data. In this case, the recreated
        /// control hierarchy reinitializes its state from view state.
        /// It enables a DataBoundControl to encapsulate the logic of creating its
        /// control hierarchy in both modes into a single code path.
        /// </summary>
        /// <param name="dataSource">
        /// The data source to be used to enumerate items.
        /// </param>
        /// <param name="dataBinding">
        /// Whether the method has been called from DataBind or not.
        /// </param>
        /// <returns>
        /// The number of items created based on the data source. Put another way, its
        /// the number of items enumerated from the data source.
        /// </returns>
        protected virtual int CreateChildControls(IEnumerable dataSource, bool dataBinding) {
            ListViewPagedDataSource pagedDataSource = null;

            // Create the LayoutTemplate so the pager control in it can set page properties.
            // We'll only create the layout template once.

            EnsureLayoutTemplate();
            RemoveItems();

            // if we should render the insert item, make a dummy empty datasource and go through
            // the regular code path.
            if (dataSource == null && InsertItemPosition != InsertItemPosition.None) {
                dataSource = new object[0];
            }

            bool usePaging = (_startRowIndex > 0 || _maximumRows > 0);

            if (dataBinding) {
                DataSourceView view = GetData();
                DataSourceSelectArguments arguments = SelectArguments;
                if (view == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_NullView, ID));
                }

                bool useServerPaging = view.CanPage && usePaging;

                if (!view.CanPage && useServerPaging) {
                    if (dataSource != null && !(dataSource is ICollection)) {
                        arguments.StartRowIndex = _startRowIndex;
                        arguments.MaximumRows = _maximumRows;
                        // This should throw an exception saying the data source can't page.
                        // We do this because the data source can provide a better error message than we can.
                        view.Select(arguments, SelectCallback);
                    }
                }

                if (useServerPaging) {
                    int totalRowCount;
                    if (view.CanRetrieveTotalRowCount) {
                        totalRowCount = arguments.TotalRowCount;
                    }
                    else {
                        ICollection dataSourceCollection = dataSource as ICollection;
                        if (dataSourceCollection == null) {
                            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_NeedICollectionOrTotalRowCount, GetType().Name));
                        }
                        totalRowCount = checked(_startRowIndex + dataSourceCollection.Count);
                    }
                    pagedDataSource = CreateServerPagedDataSource(totalRowCount);

                }
                else {
                    pagedDataSource = CreatePagedDataSource();
                }
            }
            else {
                pagedDataSource = CreatePagedDataSource();
            }

            ArrayList keyArray = DataKeysArrayList;
            ArrayList suffixArray = ClientIDRowSuffixArrayList;
            _dataKeyArray = null;
            _clientIDRowSuffixArray = null;

            ICollection collection = dataSource as ICollection;

            if (dataBinding) {
                keyArray.Clear();
                suffixArray.Clear();
                if ((dataSource != null) && (collection == null) && !pagedDataSource.IsServerPagingEnabled && usePaging) {
                    // If we got to here, it's because the data source view said it could page, but then returned
                    // something that wasn't an ICollection.  Probably a data source control author error.
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_Missing_VirtualItemCount, ID));
                }
            }
            else {
                if (collection == null) {
                    throw new InvalidOperationException(AtlasWeb.ListView_DataSourceMustBeCollectionWhenNotDataBinding);
                }
            }

            if (dataSource != null) {
                pagedDataSource.DataSource = dataSource;
                if (dataBinding && usePaging) {
                    keyArray.Capacity = pagedDataSource.DataSourceCount;
                    suffixArray.Capacity = pagedDataSource.DataSourceCount;
                }

                if (_groupTemplate != null) {
                    _itemList = CreateItemsInGroups(pagedDataSource, dataBinding, InsertItemPosition, keyArray);
                    if (dataBinding && ClientIDRowSuffixInternal != null && ClientIDRowSuffixInternal.Length != 0) {
                        CreateSuffixArrayList(pagedDataSource, suffixArray);
                    }
                }
                else {
                    if (GroupItemCount != 1) {
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_GroupItemCountNoGroupTemplate, ID, GroupPlaceholderID));
                    }

                    _itemList = CreateItemsWithoutGroups(pagedDataSource, dataBinding, InsertItemPosition, keyArray);
                    if(dataBinding && ClientIDRowSuffixInternal != null && ClientIDRowSuffixInternal.Length != 0) {
                        CreateSuffixArrayList(pagedDataSource, suffixArray);
                    }
                }

                _totalRowCount = usePaging ? pagedDataSource.DataSourceCount : _itemList.Count;
                OnTotalRowCountAvailable(new PageEventArgs(_startRowIndex, _maximumRows, _totalRowCount));

                if (_itemList.Count == 0) {
                    if (InsertItemPosition == InsertItemPosition.None) {
                        // remove the layout template
                        Controls.Clear();
                        CreateEmptyDataItem();
                    }
                }
            }
            else {
                // remove the layout template
                Controls.Clear();
                CreateEmptyDataItem();
            }

            return _totalRowCount;
        }

        // Style properties won't be honored on ListView, so throw if someone tries to set any style properties
        protected override Style CreateControlStyle() {
            // The designer reflects on properties at design time.  Don't throw then.
            if (!DesignMode) {
                throw new NotSupportedException(AtlasWeb.ListView_StyleNotSupported);
            }
            return base.CreateControlStyle();
        }

        protected override DataSourceSelectArguments CreateDataSourceSelectArguments() {
            DataSourceSelectArguments arguments = new DataSourceSelectArguments();
            DataSourceView view = GetData();
            bool useServerPaging = view.CanPage;

            string sortExpression = SortExpressionInternal;
            if (SortDirectionInternal == SortDirection.Descending && !String.IsNullOrEmpty(sortExpression)) {
                sortExpression += " DESC";
            }
            arguments.SortExpression = sortExpression;

            // decide if we should use server-side paging
            if (useServerPaging) {
                if (view.CanRetrieveTotalRowCount) {
                    arguments.RetrieveTotalRowCount = true;
                    arguments.MaximumRows = _maximumRows;
                }
                else {
                    arguments.MaximumRows = -1;
                }
                arguments.StartRowIndex = _startRowIndex;
            }
            return arguments;
        }

        protected virtual void CreateEmptyDataItem() {
            if (_emptyDataTemplate != null) {
                _instantiatedEmptyDataTemplate = true;
                ListViewItem item = CreateItem(ListViewItemType.EmptyItem);
                AutoIDControl(item);
                InstantiateEmptyDataTemplate(item);
                OnItemCreated(new ListViewItemEventArgs(item));
                AddControlToContainer(item, this, 0);
            }
        }

        protected virtual ListViewItem CreateEmptyItem() {
            if (_emptyItemTemplate != null) {
                ListViewItem emptyItem = CreateItem(ListViewItemType.EmptyItem);
                AutoIDControl(emptyItem);
                InstantiateEmptyItemTemplate(emptyItem);
                OnItemCreated(new ListViewItemEventArgs(emptyItem));
                return emptyItem;
            }
            return null;
        }

        protected virtual ListViewItem CreateInsertItem() {
            if (InsertItemTemplate == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_InsertTemplateRequired, ID));
            }

            ListViewItem item = CreateItem(ListViewItemType.InsertItem);
            AutoIDControl(item);
            InstantiateInsertItemTemplate(item);
            OnItemCreated(new ListViewItemEventArgs(item));
            return item;
        }

        protected virtual ListViewItem CreateItem(ListViewItemType itemType) {
            ListViewItem item = new ListViewItem(itemType);
            if (itemType == ListViewItemType.InsertItem) {
                _insertItem = item;
            }
            return item;
        }

        protected virtual ListViewDataItem CreateDataItem(int dataItemIndex, int displayIndex) {
            return new ListViewDataItem(dataItemIndex, displayIndex);
        }

        protected virtual IList<ListViewDataItem> CreateItemsWithoutGroups(ListViewPagedDataSource dataSource, bool dataBinding, InsertItemPosition insertPosition, ArrayList keyArray) {
            // If this is the first time we're creating the control items, we need
            // to locate the itemPlaceholder container.
            // If this is a scenario where we are recreating control items, we already
            // have the cached itemPlaceholder container.
            if (_noGroupsOriginalIndexOfItemPlaceholderInContainer == -1) {
                _noGroupsItemPlaceholderContainer = GetPreparedContainerInfo(this, true, out _noGroupsOriginalIndexOfItemPlaceholderInContainer);
            }

            // We need to keep track of where we're inserting items and how many items we have
            // inserted so that if we need to remove them we know what to do.
            int itemInsertLocation = _noGroupsOriginalIndexOfItemPlaceholderInContainer;

            List<ListViewDataItem> items = new List<ListViewDataItem>();
            int itemIndex = 0;
            int dataItemIndex = 0;

            if (insertPosition == InsertItemPosition.FirstItem) {
                ListViewItem insertItem = CreateInsertItem();
                AddControlToContainer(insertItem, _noGroupsItemPlaceholderContainer, itemInsertLocation);
                insertItem.DataBind();
                itemInsertLocation++;
                itemIndex++;
            }
            // Reset the selected index if we have a persisted datakey so we
            // can figure out what index to select based on the key
            ResetPersistedSelectedIndex();

            foreach (object o in dataSource) {
                if (itemIndex != 0 && _itemSeparatorTemplate != null) {
                    ListViewContainer itemSeparatorContainer = new ListViewContainer();
                    AutoIDControl(itemSeparatorContainer);
                    InstantiateItemSeparatorTemplate(itemSeparatorContainer);
                    AddControlToContainer(itemSeparatorContainer, _noGroupsItemPlaceholderContainer, itemInsertLocation);
                    itemInsertLocation++;
                }

                ListViewDataItem item = CreateDataItem(dataItemIndex + dataSource.StartRowIndex, dataItemIndex);
                AutoIDControl(item);

                if (dataBinding) {
                    item.DataItem = o;
                    OrderedDictionary keyTable = new OrderedDictionary(DataKeyNamesInternal.Length);
                    foreach (string keyName in DataKeyNamesInternal) {
                        object keyValue = DataBinder.GetPropertyValue(o, keyName);
                        keyTable.Add(keyName, keyValue);
                    }
                    if (keyArray.Count == dataItemIndex) {
                        keyArray.Add(new DataKey(keyTable, DataKeyNamesInternal));
                    }
                    else {
                        keyArray[dataItemIndex] = new DataKey(keyTable, DataKeyNamesInternal);
                    }
                }

                // If persisted selection is enabled and we have a data key then compare it to get the selected index
                if (EnablePersistedSelection) {
                    if (dataItemIndex < keyArray.Count) {
                        DataKey currentKey = (DataKey)keyArray[dataItemIndex];
                        SetPersistedDataKey(dataItemIndex, currentKey);
                    }
                }
                
                InstantiateItemTemplate(item, dataItemIndex);
                

                OnItemCreated(new ListViewItemEventArgs(item));
                AddControlToContainer(item, _noGroupsItemPlaceholderContainer, itemInsertLocation);
                itemInsertLocation++;
                items.Add(item);

                if (dataBinding) {
                    item.DataBind();
                    OnItemDataBound(new ListViewItemEventArgs(item));
                    item.DataItem = null;
                }

                dataItemIndex++;
                itemIndex++;
            }

            if (insertPosition == InsertItemPosition.LastItem) {
                if (_itemSeparatorTemplate != null) {
                    ListViewContainer itemSeparatorContainer = new ListViewContainer();
                    AutoIDControl(itemSeparatorContainer);
                    InstantiateItemSeparatorTemplate(itemSeparatorContainer);
                    AddControlToContainer(itemSeparatorContainer, _noGroupsItemPlaceholderContainer, itemInsertLocation);
                    itemInsertLocation++;
                }

                ListViewItem insertItem = CreateInsertItem();
                AddControlToContainer(insertItem, _noGroupsItemPlaceholderContainer, itemInsertLocation);
                insertItem.DataBind();
                itemInsertLocation++;
                itemIndex++;
            }

            _noGroupsItemCreatedCount = itemInsertLocation - _noGroupsOriginalIndexOfItemPlaceholderInContainer;

            return items;
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
                // Persist the selection by picking the selected index where DataKeys match
                _selectedIndex = dataItemIndex;
            }
        }

        protected virtual IList<ListViewDataItem> CreateItemsInGroups(ListViewPagedDataSource dataSource, bool dataBinding, InsertItemPosition insertPosition, ArrayList keyArray) {
            // If this is the first time we're creating the control items, we need
            // to locate the groupPlaceholder container.
            // If this is a scenario where we are recreating control items, we already
            // have the cached groupPlaceholder container.
            if (_groupsOriginalIndexOfGroupPlaceholderInContainer == -1) {
                _groupsGroupPlaceholderContainer = GetPreparedContainerInfo(this, false, out _groupsOriginalIndexOfGroupPlaceholderInContainer);
            }

            int groupInsertLocation = _groupsOriginalIndexOfGroupPlaceholderInContainer;
            _groupsItemCreatedCount = 0;

            int itemInsertLocation = 0;
            Control itemPlaceholderContainer = null;

            // Reset the selected index if we have a persisted datakey so we
            // can figure out what index to select based on the key
            ResetPersistedSelectedIndex();

            List<ListViewDataItem> items = new List<ListViewDataItem>();
            int itemIndex = 0;
            int dataItemIndex = 0;

            if (insertPosition == InsertItemPosition.FirstItem) {
                ListViewContainer groupContainer = new ListViewContainer();
                AutoIDControl(groupContainer);
                InstantiateGroupTemplate(groupContainer);
                AddControlToContainer(groupContainer, _groupsGroupPlaceholderContainer, groupInsertLocation);
                groupInsertLocation++;

                itemPlaceholderContainer = GetPreparedContainerInfo(groupContainer, true, out itemInsertLocation);

                ListViewItem insertItem = CreateInsertItem();
                AddControlToContainer(insertItem, itemPlaceholderContainer, itemInsertLocation);
                insertItem.DataBind();
                itemInsertLocation++;
                itemIndex++;
            }

            foreach (object o in dataSource) {
                if (itemIndex % _groupItemCount == 0) {
                    if (itemIndex != 0 && _groupSeparatorTemplate != null) {
                        ListViewContainer groupSeparatorContainer = new ListViewContainer();
                        AutoIDControl(groupSeparatorContainer);
                        InstantiateGroupSeparatorTemplate(groupSeparatorContainer);
                        AddControlToContainer(groupSeparatorContainer, _groupsGroupPlaceholderContainer, groupInsertLocation);
                        groupInsertLocation++;
                    }
                    ListViewContainer groupContainer = new ListViewContainer();
                    AutoIDControl(groupContainer);
                    InstantiateGroupTemplate(groupContainer);
                    AddControlToContainer(groupContainer, _groupsGroupPlaceholderContainer, groupInsertLocation);
                    groupInsertLocation++;

                    itemPlaceholderContainer = GetPreparedContainerInfo(groupContainer, true, out itemInsertLocation);
                }

                ListViewDataItem item = CreateDataItem(dataItemIndex + StartRowIndex, dataItemIndex);

                if (dataBinding) {
                    item.DataItem = o;
                    OrderedDictionary keyTable = new OrderedDictionary(DataKeyNamesInternal.Length);
                    foreach (string keyName in DataKeyNamesInternal) {
                        object keyValue = DataBinder.GetPropertyValue(o, keyName);
                        keyTable.Add(keyName, keyValue);
                    }
                    if (keyArray.Count == dataItemIndex) {
                        keyArray.Add(new DataKey(keyTable, DataKeyNamesInternal));
                    }
                    else {
                        keyArray[dataItemIndex] = new DataKey(keyTable, DataKeyNamesInternal);
                    }
                }
                // If persisted selection is enabled and we have a data key then compare it to get the selected index
                if (EnablePersistedSelection) {
                    if (dataItemIndex < keyArray.Count) {
                        DataKey currentKey = (DataKey)keyArray[dataItemIndex];
                        SetPersistedDataKey(dataItemIndex, currentKey);
                    }
                }

                InstantiateItemTemplate(item, dataItemIndex);

                OnItemCreated(new ListViewItemEventArgs(item));

                if (itemIndex % _groupItemCount != 0 && _itemSeparatorTemplate != null) {
                    ListViewContainer itemSeparatorContainer = new ListViewContainer();
                    InstantiateItemSeparatorTemplate(itemSeparatorContainer);
                    AddControlToContainer(itemSeparatorContainer, itemPlaceholderContainer, itemInsertLocation);
                    itemInsertLocation++;
                }


                AddControlToContainer(item, itemPlaceholderContainer, itemInsertLocation);
                itemInsertLocation++;
                items.Add(item);

                if (dataBinding) {
                    item.DataBind();
                    OnItemDataBound(new ListViewItemEventArgs(item));
                    item.DataItem = null;
                }

                itemIndex++;
                dataItemIndex++;
            }

            if (insertPosition == InsertItemPosition.LastItem) {
                if (itemIndex % _groupItemCount == 0) {
                    // start a new group
                    if (itemIndex != 0 && _groupSeparatorTemplate != null) {
                        ListViewContainer groupSeparatorContainer = new ListViewContainer();
                        AutoIDControl(groupSeparatorContainer);
                        InstantiateGroupSeparatorTemplate(groupSeparatorContainer);
                        AddControlToContainer(groupSeparatorContainer, _groupsGroupPlaceholderContainer, groupInsertLocation);
                        groupInsertLocation++;
                    }
                    ListViewContainer groupContainer = new ListViewContainer();
                    AutoIDControl(groupContainer);
                    InstantiateGroupTemplate(groupContainer);
                    AddControlToContainer(groupContainer, _groupsGroupPlaceholderContainer, groupInsertLocation);
                    groupInsertLocation++;

                    itemPlaceholderContainer = GetPreparedContainerInfo(groupContainer, true, out itemInsertLocation);
                }

                // use the existing group
                if (itemIndex % _groupItemCount != 0 && _itemSeparatorTemplate != null) {
                    ListViewContainer itemSeparatorContainer = new ListViewContainer();
                    InstantiateItemSeparatorTemplate(itemSeparatorContainer);
                    AddControlToContainer(itemSeparatorContainer, itemPlaceholderContainer, itemInsertLocation);
                    itemInsertLocation++;
                }

                ListViewItem insertItem = CreateInsertItem();
                AddControlToContainer(insertItem, itemPlaceholderContainer, itemInsertLocation);
                insertItem.DataBind();
                itemInsertLocation++;
                itemIndex++;
            }

            // fill in the rest of the items if there's an emptyItemTemplate
            if (_emptyItemTemplate != null) {
                while (itemIndex % _groupItemCount != 0) {
                    if (_itemSeparatorTemplate != null) {
                        ListViewContainer itemSeparatorContainer = new ListViewContainer();
                        InstantiateItemSeparatorTemplate(itemSeparatorContainer);
                        AddControlToContainer(itemSeparatorContainer, itemPlaceholderContainer, itemInsertLocation);
                        itemInsertLocation++;
                    }

                    ListViewItem emptyItem = CreateEmptyItem();
                    AddControlToContainer(emptyItem, itemPlaceholderContainer, itemInsertLocation);
                    itemInsertLocation++;
                    itemIndex++;
                }
            }

            _groupsItemCreatedCount = groupInsertLocation - _groupsOriginalIndexOfGroupPlaceholderInContainer;

            return items;
        }

        protected virtual void CreateSuffixArrayList(ListViewPagedDataSource dataSource, ArrayList suffixArray) {
            int dataItemIndex = 0;
            foreach (object o in dataSource) {
                OrderedDictionary suffixTable = new OrderedDictionary(ClientIDRowSuffixInternal.Length);
                foreach (string suffixName in ClientIDRowSuffixInternal) {
                    object suffixValue = DataBinder.GetPropertyValue(o, suffixName);
                    suffixTable.Add(suffixName, suffixValue);
                }
                if (suffixArray.Count == dataItemIndex) {
                    suffixArray.Add(new DataKey(suffixTable, ClientIDRowSuffixInternal));
                }
                else {
                    suffixArray[dataItemIndex] = new DataKey(suffixTable, ClientIDRowSuffixInternal);
                }
                dataItemIndex++;
            }
        }


        protected virtual void CreateLayoutTemplate() {
            // Reset data concerning where things are in the layout template since we're about to recreate it
            _noGroupsOriginalIndexOfItemPlaceholderInContainer = -1;
            _noGroupsItemCreatedCount = 0;
            _noGroupsItemPlaceholderContainer = null;

            _groupsOriginalIndexOfGroupPlaceholderInContainer = -1;
            _groupsItemCreatedCount = 0;
            _groupsGroupPlaceholderContainer = null;

            Control containerControl = new Control();
            if (_layoutTemplate != null) {
                _layoutTemplate.InstantiateIn(containerControl);
                Controls.Add(containerControl);
            }
            OnLayoutCreated(new EventArgs());
        }

        private ListViewPagedDataSource CreatePagedDataSource() {
            ListViewPagedDataSource pagedDataSource = new ListViewPagedDataSource();

            pagedDataSource.StartRowIndex = _startRowIndex;
            pagedDataSource.MaximumRows = _maximumRows;
            pagedDataSource.AllowServerPaging = false;
            pagedDataSource.TotalRowCount = 0;

            return pagedDataSource;
        }

        private ListViewPagedDataSource CreateServerPagedDataSource(int totalRowCount) {
            ListViewPagedDataSource pagedDataSource = new ListViewPagedDataSource();

            pagedDataSource.StartRowIndex = _startRowIndex;
            pagedDataSource.MaximumRows = _maximumRows;
            pagedDataSource.AllowServerPaging = true;
            pagedDataSource.TotalRowCount = totalRowCount;

            return pagedDataSource;
        }

        public virtual void DeleteItem(int itemIndex) {
            // use EnableModelVadliation as the causesValdiation param because the hosting page should not
            // be validated unless model validation is going to be used
            ResetModelValidationGroup(EnableModelValidation, String.Empty);
            HandleDelete(null, itemIndex);
        }

        protected virtual void EnsureLayoutTemplate() {
            if (this.Controls.Count == 0 || _instantiatedEmptyDataTemplate) {
                Controls.Clear();
                CreateLayoutTemplate();
            }
        }

        public virtual void ExtractItemValues(IOrderedDictionary itemValues, ListViewItem item, bool includePrimaryKey) {
            if (itemValues == null) {
                throw new ArgumentNullException("itemValues");
            }

            DataBoundControlHelper.ExtractValuesFromBindableControls(itemValues, item);

            IBindableTemplate bindableTemplate = null;
            if (item.ItemType == ListViewItemType.DataItem) {
                ListViewDataItem dataItem = item as ListViewDataItem;
                if (dataItem == null) {
                    throw new InvalidOperationException(AtlasWeb.ListView_ItemsNotDataItems);
                }

                if (dataItem.DisplayIndex == EditIndex) {
                    bindableTemplate = EditItemTemplate as IBindableTemplate;
                }
                else if (dataItem.DisplayIndex == SelectedIndex) {
                    bindableTemplate = SelectedItemTemplate as IBindableTemplate;
                }
                else if (dataItem.DisplayIndex % 2 == 1 && AlternatingItemTemplate != null) {
                    bindableTemplate = AlternatingItemTemplate as IBindableTemplate;
                }
                else {
                    bindableTemplate = ItemTemplate as IBindableTemplate;
                }
            }
            else if (item.ItemType == ListViewItemType.InsertItem) {
                if (InsertItemTemplate != null) {
                    bindableTemplate = InsertItemTemplate as IBindableTemplate;
                }
            }

            if (bindableTemplate != null) {
                OrderedDictionary newValues = new OrderedDictionary();

                bool convertEmptyStringToNull = ConvertEmptyStringToNull;
                foreach (DictionaryEntry entry in bindableTemplate.ExtractValues(item)) {
                    object value = entry.Value;
                    if (convertEmptyStringToNull && value is string && ((string)value).Length == 0) {
                        newValues[entry.Key] = null;
                    }
                    else {
                        newValues[entry.Key] = value;
                    }
                }

                foreach (DictionaryEntry entry in newValues) {
                    if (includePrimaryKey || (Array.IndexOf(DataKeyNamesInternal, entry.Key) == -1)) {
                        itemValues[entry.Key] = entry.Value;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        protected virtual Control FindPlaceholder(string containerID, Control container) {
            return container.FindControl(containerID);
        }

        private DataPager FindDataPager(Control control) {
            foreach (Control c in control.Controls) {
                DataPager pager = c as DataPager;
                if (pager != null) {
                    return pager;
                }
            }

            foreach (Control c in control.Controls) {
                if (c is IPageableItemContainer) {
                    // Exit out if we've ventured into another ListView or pageable container, since that is the likely
                    // target of any embedded pagers.
                    return null;
                }

                DataPager pager = FindDataPager(c);
                if (pager != null) {
                    return pager;
                }
            }
            return null;
        }

        private int GetItemIndex(ListViewItem item, string commandArgument) {
            if (item != null) {
                ListViewDataItem dataItem = item as ListViewDataItem;
                if (dataItem != null) {
                    return dataItem.DisplayIndex;
                }
                return -1;
            }
            return Convert.ToInt32(commandArgument, CultureInfo.InvariantCulture);
        }

        private bool TryGetItemIndex(ListViewItem item, string commandArgument, out int itemIndex) {
            if (item != null) {
                ListViewDataItem dataItem = item as ListViewDataItem;
                itemIndex = (dataItem != null) ? dataItem.DisplayIndex : -1;
                // HandleCommand will throw detailed exception when item is not data item
                return true;
            }
            return Int32.TryParse(commandArgument, NumberStyles.Integer, CultureInfo.InvariantCulture, out itemIndex);
        }

        private Control GetPreparedContainerInfo(Control outerContainer, bool isItem, out int placeholderIndex) {
            // This function locates the ItemPlaceholder for a given container and prepares
            // it for child controls. Strategy:            
            // - Locate ItemPlaceholder 
            // - If it's not found and the user defined a layout template throw
            // - If it's not found and the user didn't define a layout/group template, add a default placeholder with the placeholder ID
            // - Store the placeholder's container and the placeholder's location in the container
            // - Remove the ItemPlaceholder

            string placeholderID = isItem ? ItemPlaceholderID : GroupPlaceholderID;
            Control placeholder = FindPlaceholder(placeholderID, outerContainer);
            if (placeholder == null) {
                //add a default placeholder
                if (_layoutTemplate == null) {
                    placeholder = new PlaceHolder();
                    placeholder.ID = placeholderID;
                }

                if (isItem) {
                    //throw if the user defined a layout/group template and didn't specify an item placeholder
                    if ((_layoutTemplate != null) || (_groupTemplate != null)) {
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_NoItemPlaceholder, ID, ItemPlaceholderID));
                    }
                }
                else {
                    //throw if the user defined a layout template and didn't specify an group placeholder
                    if (_layoutTemplate != null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_NoGroupPlaceholder, ID, GroupPlaceholderID));
                    }
                }

                Controls.Add(placeholder);
            }

            // Save the information about where we found the itemPlaceholder because
            // in RemoveItems() we need to know where to delete whatever we create.
            // This only applies to certain usages of this function.
            Control placeholderContainer = placeholder.Parent;
            placeholderIndex = placeholderContainer.Controls.IndexOf(placeholder);

            // Remove the item placeholder since we're going to be
            // adding real items starting at the index where it used to be.
            placeholderContainer.Controls.Remove(placeholder);

            return placeholderContainer;
        }

        private void HandleCancel(int itemIndex) {
            ListViewCancelMode cancelMode = ListViewCancelMode.CancelingInsert;
            if (itemIndex == EditIndex && itemIndex >= 0) {
                cancelMode = ListViewCancelMode.CancelingEdit;
            }
            else if (itemIndex != -1) {
                throw new InvalidOperationException(AtlasWeb.ListView_InvalidCancel);
            }

            ListViewCancelEventArgs e = new ListViewCancelEventArgs(itemIndex, cancelMode);
            OnItemCanceling(e);

            if (e.Cancel) {
                return;
            }

            if (IsDataBindingAutomatic) {
                if (e.CancelMode == ListViewCancelMode.CancelingEdit) {
                    EditIndex = -1;
                }
                else {
                    // cancel on an insert is simply "redatabind to clear"?
                }
            }

            RequiresDataBinding = true;
        }

        private void HandleDelete(ListViewItem item, int itemIndex) {
            ListViewDataItem dataItem = item as ListViewDataItem;
            if (itemIndex < 0 && dataItem == null) {
                throw new InvalidOperationException(AtlasWeb.ListView_InvalidDelete);
            }

            DataSourceView view = null;
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            if (isBoundToDataSourceControl) {
                view = GetData();
                if (view == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_NullView, ID));
                }
            }

            if (item == null && itemIndex < Items.Count) {
                item = Items[itemIndex];
            }

            ListViewDeleteEventArgs e = new ListViewDeleteEventArgs(itemIndex);


            if (item != null) {
                ExtractItemValues(e.Values, item, false/*includePrimaryKey*/);
            }
            if (DataKeys.Count > itemIndex) {
                foreach (DictionaryEntry entry in DataKeys[itemIndex].Values) {
                    e.Keys.Add(entry.Key, entry.Value);
                    if (e.Values.Contains(entry.Key)) {
                        e.Values.Remove(entry.Key);
                    }
                }
            }
            

            OnItemDeleting(e);

            if (e.Cancel) {
                return;
            }

            _deletedItemIndex = itemIndex;

            if (isBoundToDataSourceControl) {
                _deleteKeys = e.Keys;
                _deleteValues = e.Values;

                view.Delete(e.Keys, e.Values, HandleDeleteCallback);
            }
        }

        private bool HandleDeleteCallback(int affectedRows, Exception ex) {
            ListViewDeletedEventArgs e = new ListViewDeletedEventArgs(affectedRows, ex);
            e.SetKeys(_deleteKeys);
            e.SetValues(_deleteValues);

            OnItemDeleted(e);
            _deleteKeys = null;
            _deleteValues = null;

            if (ex != null && !e.ExceptionHandled) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
            }
            EditIndex = -1;

            if (affectedRows > 0) {
                // Patch up the selected index if we deleted the last item on the last page.
                if ((_totalRowCount > 0) &&
                    (_deletedItemIndex == SelectedIndex) &&
                    (_deletedItemIndex + _startRowIndex == _totalRowCount)) {
                    SelectedIndex--;
                }
            }
            _deletedItemIndex = -1;

            RequiresDataBinding = true;
            return true;
        }

        private void HandleEdit(int itemIndex) {
            if (itemIndex < 0) {
                throw new InvalidOperationException(AtlasWeb.ListView_InvalidEdit);
            }

            ListViewEditEventArgs e = new ListViewEditEventArgs(itemIndex);
            OnItemEditing(e);

            if (e.Cancel) {
                return;
            }

            EditIndex = e.NewEditIndex;

            RequiresDataBinding = true;
        }

        private bool HandleEvent(EventArgs e, bool causesValidation, string validationGroup) {
            bool handled = false;

            ResetModelValidationGroup(causesValidation, validationGroup);

            ListViewCommandEventArgs dce = e as ListViewCommandEventArgs;

            if (dce != null) {

                OnItemCommand(dce);
                if (dce.Handled) {
                    return true;
                }
                handled = true;

                string command = dce.CommandName;

                if (String.Equals(command, DataControlCommands.SelectCommandName, StringComparison.OrdinalIgnoreCase)) {
                    HandleSelect(GetItemIndex(dce.Item, (string)dce.CommandArgument));
                }
                else if (String.Equals(command, DataControlCommands.SortCommandName, StringComparison.OrdinalIgnoreCase)) {
                    HandleSort((string)dce.CommandArgument);
                }
                else if (String.Equals(command, DataControlCommands.EditCommandName, StringComparison.OrdinalIgnoreCase)) {
                    HandleEdit(GetItemIndex(dce.Item, (string)dce.CommandArgument));
                }
                else if (String.Equals(command, DataControlCommands.CancelCommandName, StringComparison.OrdinalIgnoreCase)) {
                    HandleCancel(GetItemIndex(dce.Item, (string)dce.CommandArgument));
                }
                else if (String.Equals(command, DataControlCommands.UpdateCommandName, StringComparison.OrdinalIgnoreCase)) {
                    HandleUpdate(dce.Item, GetItemIndex(dce.Item, (string)dce.CommandArgument), causesValidation);
                }
                else if (String.Equals(command, DataControlCommands.DeleteCommandName, StringComparison.OrdinalIgnoreCase)) {
                    HandleDelete(dce.Item, GetItemIndex(dce.Item, (string)dce.CommandArgument));
                }
                else if (String.Equals(command, DataControlCommands.InsertCommandName, StringComparison.OrdinalIgnoreCase)) {
                    HandleInsert(dce.Item, causesValidation);
                }
                else {
                    int itemIndex;
                    if (TryGetItemIndex(dce.Item, (string)dce.CommandArgument, out itemIndex)) {
                        handled = HandleCommand(dce.Item, itemIndex, command);
                    }
                }
            }

            return handled;
        }

        private bool HandleCommand(ListViewItem item, int itemIndex, string commandName) {
            DataSourceView view = null;

            if (IsDataBindingAutomatic) {
                view = GetData();
                if (view == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_NullView, ID));
                }
            }
            else {
                return false;
            }

            if (!view.CanExecute(commandName)) {
                return false;
            }

            ListViewDataItem dataItem = item as ListViewDataItem;
            if (itemIndex < 0 && dataItem == null) {
                throw new InvalidOperationException(AtlasWeb.ListView_InvalidCommand);
            }

            OrderedDictionary values = new OrderedDictionary();
            OrderedDictionary keys = new OrderedDictionary();
            if (item != null) {
                ExtractItemValues(values, item, false /*includePrimaryKey*/);
            }

            if (DataKeys.Count > itemIndex) {
                foreach (DictionaryEntry entry in DataKeys[itemIndex].Values) {
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

        private void HandleInsert(ListViewItem item, bool causesValidation) {
            if (item != null && item.ItemType != ListViewItemType.InsertItem) {
                throw new InvalidOperationException(AtlasWeb.ListView_InvalidInsert);
            }

            if (causesValidation && Page != null && !Page.IsValid) {
                return;
            }

            if (item == null) {
                item = _insertItem;
            }
            if (item == null) {
                throw new InvalidOperationException(AtlasWeb.ListView_NoInsertItem);
            }

            DataSourceView view = null;
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            if (isBoundToDataSourceControl) {
                view = GetData();
                if (view == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_NullView, ID));
                }
            }

            ListViewInsertEventArgs e = new ListViewInsertEventArgs(item);


            ExtractItemValues(e.Values, item, true/*includeKeys*/);
            

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
            ListViewInsertedEventArgs e = new ListViewInsertedEventArgs(affectedRows, ex);
            e.SetValues(_insertValues);

            OnItemInserted(e);

            _insertValues = null;
            if (ex != null && !e.ExceptionHandled) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
                e.KeepInInsertMode = true;
            }

            if (IsUsingModelBinders && !Page.ModelState.IsValid) {
                e.KeepInInsertMode = true;
            }

            if (!e.KeepInInsertMode) {
                RequiresDataBinding = true;
            }
            return true;
        }

        private void HandleSelect(int itemIndex) {
            if (itemIndex < 0) {
                throw new InvalidOperationException(AtlasWeb.ListView_InvalidSelect);
            }

            ListViewSelectEventArgs e = new ListViewSelectEventArgs(itemIndex);
            OnSelectedIndexChanging(e);

            if (e.Cancel) {
                return;
            }

            SelectedIndex = e.NewSelectedIndex;

            OnSelectedIndexChanged(EventArgs.Empty);
            RequiresDataBinding = true;
        }

        private void HandleSort(string sortExpression) {
            SortDirection futureSortDirection = SortDirection.Ascending;

            if ((SortExpressionInternal == sortExpression) && (SortDirectionInternal == SortDirection.Ascending)) {
                // switch direction
                futureSortDirection = SortDirection.Descending;
            }
            HandleSort(sortExpression, futureSortDirection);
        }

        private void HandleSort(string sortExpression, SortDirection sortDirection) {
            ListViewSortEventArgs e = new ListViewSortEventArgs(sortExpression, sortDirection);
            OnSorting(e);

            if (e.Cancel) {
                return;
            }

            if (IsDataBindingAutomatic) {
                ClearDataKeys();
                DataSourceView view = GetData();
                if (view == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_NullView, ID));
                }

                EditIndex = -1;

                SortExpressionInternal = e.SortExpression;
                SortDirectionInternal = e.SortDirection;
                _startRowIndex = 0;
            }

            OnSorted(EventArgs.Empty);
            RequiresDataBinding = true;
        }

        private void HandleUpdate(ListViewItem item, int itemIndex, bool causesValidation) {
            ListViewDataItem dataItem = item as ListViewDataItem;
            if (itemIndex < 0 && dataItem == null) {
                throw new InvalidOperationException(AtlasWeb.ListView_InvalidUpdate);
            }

            if (causesValidation && Page != null && !Page.IsValid) {
                return;
            }

            DataSourceView view = null;
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            if (isBoundToDataSourceControl) {
                view = GetData();
                if (view == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_NullView, ID));
                }
            }

            ListViewUpdateEventArgs e = new ListViewUpdateEventArgs(itemIndex);


            foreach (DictionaryEntry entry in BoundFieldValues) {
                e.OldValues.Add(entry.Key, entry.Value);
            }

            if (DataKeys.Count > itemIndex) {
                foreach (DictionaryEntry entry in DataKeys[itemIndex].Values) {
                    e.Keys.Add(entry.Key, entry.Value);
                }
            }

            if (dataItem == null && Items.Count > itemIndex) {
                dataItem = Items[itemIndex];
            }

            if (dataItem != null) {
                ExtractItemValues(e.NewValues, dataItem, true/*includePrimaryKey*/);
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
            ListViewUpdatedEventArgs e = new ListViewUpdatedEventArgs(affectedRows, ex);
            e.SetKeys(_updateKeys);
            e.SetOldValues(_updateOldValues);
            e.SetNewValues(_updateNewValues);

            OnItemUpdated(e);
            _updateKeys = null;
            _updateOldValues = null;
            _updateNewValues = null;
            if (ex != null && !e.ExceptionHandled) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
                e.KeepInEditMode = true;
            }

            if (IsUsingModelBinders && !Page.ModelState.IsValid) {
                e.KeepInEditMode = true;
            }

            // We need to databind here event if no records were affected because
            // changing the EditIndex required a rebind.  The event args give the programmer
            // the chance to cancel the bind so the edits aren't lost.
            if (!e.KeepInEditMode) {
                EditIndex = -1;
                RequiresDataBinding = true;
            }
            return true;
        }

        public virtual void InsertNewItem(bool causesValidation) {
            ResetModelValidationGroup(causesValidation, String.Empty);
            HandleInsert(null, causesValidation);
        }

        protected virtual void InstantiateEmptyDataTemplate(Control container) {
            if (_emptyDataTemplate != null) {
                _emptyDataTemplate.InstantiateIn(container);
            }
        }

        protected virtual void InstantiateEmptyItemTemplate(Control container) {
            if (_emptyItemTemplate != null) {
                _emptyItemTemplate.InstantiateIn(container);
            }
        }

        protected virtual void InstantiateGroupTemplate(Control container) {
            if (_groupTemplate != null) {
                _groupTemplate.InstantiateIn(container);
            }
        }

        protected virtual void InstantiateGroupSeparatorTemplate(Control container) {
            if (_groupSeparatorTemplate != null) {
                _groupSeparatorTemplate.InstantiateIn(container);
            }
        }

        protected virtual void InstantiateInsertItemTemplate(Control container) {
            if (_insertItemTemplate != null) {
                _insertItemTemplate.InstantiateIn(container);
            }
        }

        protected virtual void InstantiateItemSeparatorTemplate(Control container) {
            if (_itemSeparatorTemplate != null) {
                _itemSeparatorTemplate.InstantiateIn(container);
            }
        }

        protected virtual void InstantiateItemTemplate(Control container, int displayIndex) {
            ITemplate contentTemplate = _itemTemplate;

            if (contentTemplate == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_ItemTemplateRequired, ID));
            }

            if (displayIndex % 2 == 1 && _alternatingItemTemplate != null) {
                contentTemplate = _alternatingItemTemplate;
            }
            if (displayIndex == _selectedIndex && _selectedItemTemplate != null) {
                contentTemplate = _selectedItemTemplate;
            }
            if (displayIndex == _editIndex && _editItemTemplate != null) {
                contentTemplate = _editItemTemplate;
            }

            contentTemplate.InstantiateIn(container);
        }

        protected internal override void LoadControlState(object savedState) {
            // Any properties that could have been set in the persistance need to be
            // restored to their defaults if they're not in ControlState, or they will
            // be restored to their persisted state instead of their empty state.
            _startRowIndex = 0;
            _maximumRows = -1;
            _editIndex = -1;
            _selectedIndex = -1;
            _groupItemCount = 1;
            _sortExpression = String.Empty;
            _sortDirection = SortDirection.Ascending;
            _dataKeyNames = new string[0];
            object[] state = savedState as object[];

            if (state != null) {
                base.LoadControlState(state[0]);

                if (state[1] != null) {
                    _editIndex = (int)state[1];
                }

                if (state[2] != null) {
                    _selectedIndex = (int)state[2];
                }

                if (state[3] != null) {
                    _groupItemCount = (int)state[3];
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
                    _totalRowCount = (int)state[8];
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
                if (state[12] != null) {
                    _startRowIndex = (int)state[12];
                }
                if (state[13] != null) {
                    _maximumRows = (int)state[13];
                }

            }
            else {
                base.LoadControlState(null);
            }
            // DataPager handles the TotalRowCountAvailable event in order to create its pager fields.  Normally this
            // is fired from CreateChildControls, but when ViewState is disabled this is not getting called until after
            // postback data has been handled.  In this case the event will be fired using the control count from the
            // last request so that the pager fields can be initialized.
            if (!IsViewStateEnabled) {
                OnTotalRowCountAvailable(new PageEventArgs(_startRowIndex, _maximumRows, _totalRowCount));
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

        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                object[] state = (object[])savedState;

                base.LoadViewState(state[0]);
                if (state[1] != null) {
                    OrderedDictionaryStateHelper.LoadViewState((OrderedDictionary)BoundFieldValues, (ArrayList)state[1]);
                }
            }
            else {
                base.LoadViewState(savedState);
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

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "1#")]
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            bool causesValidation = false;
            string validationGroup = String.Empty;

            ListViewCommandEventArgs commandEventArgs = e as ListViewCommandEventArgs;
            // todo: rethink this.  Should everything in the layout template be bubbled
            // up as a ListViewCommand?
            if (commandEventArgs == null && e is CommandEventArgs) {
                // Use a new EmptyItem ListViewItem here so when HandleEvent tries to parse out the data item index,
                // the user gets a nice message about how this button should be in a data item.
                commandEventArgs = new ListViewCommandEventArgs(new ListViewItem(ListViewItemType.EmptyItem), source, (CommandEventArgs)e);
            }

            if (commandEventArgs != null) {
                IButtonControl button = commandEventArgs.CommandSource as IButtonControl;
                if (button != null) {
                    causesValidation = button.CausesValidation;
                    validationGroup = button.ValidationGroup;
                }
            }
            return HandleEvent(commandEventArgs, causesValidation, validationGroup);
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
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
        /// <para>Raises the <see langword='CancelCommand '/>event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemCanceling(ListViewCancelEventArgs e) {
            EventHandler<ListViewCancelEventArgs> handler = (EventHandler<ListViewCancelEventArgs>)Events[EventItemCanceling];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (IsDataBindingAutomatic == false && e.Cancel == false) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_UnhandledEvent, ID, "ItemCanceling"));
                }
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='ItemCommand'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemCommand(ListViewCommandEventArgs e) {
            EventHandler<ListViewCommandEventArgs> handler = (EventHandler<ListViewCommandEventArgs>)Events[EventItemCommand];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='ItemCreated'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemCreated(ListViewItemEventArgs e) {
            EventHandler<ListViewItemEventArgs> handler = (EventHandler<ListViewItemEventArgs>)Events[EventItemCreated];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='ItemDataBound'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemDataBound(ListViewItemEventArgs e) {
            EventHandler<ListViewItemEventArgs> handler = (EventHandler<ListViewItemEventArgs>)Events[EventItemDataBound];
            if (handler != null) {
                handler(this, e);
            }

            // EventWizardListItemDataBound is a key for an internal event declared on IWizardSideBarListControl, which is
            // an interface that is meant to provide a facade to make ListView and DataList look the same. This handler
            // is meant to abstract away the differences between each controls ItemDataBound events.
            var wizardListHandler = (EventHandler<WizardSideBarListControlItemEventArgs>)Events[EventWizardListItemDataBound];
            if (wizardListHandler != null) {
                var item = e.Item;
                var wizardListEventArgs = new WizardSideBarListControlItemEventArgs(new WizardSideBarListControlItem(item.DataItem, ListItemType.Item, item.DataItemIndex, item));
                wizardListHandler(this, wizardListEventArgs);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='ItemDeleted '/>event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemDeleted(ListViewDeletedEventArgs e) {
            EventHandler<ListViewDeletedEventArgs> handler = (EventHandler<ListViewDeletedEventArgs>)Events[EventItemDeleted];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='Delete'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemDeleting(ListViewDeleteEventArgs e) {
            EventHandler<ListViewDeleteEventArgs> handler = (EventHandler<ListViewDeleteEventArgs>)Events[EventItemDeleting];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (IsDataBindingAutomatic == false && e.Cancel == false) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_UnhandledEvent, ID, "ItemDeleting"));
                }
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='EditCommand'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemEditing(ListViewEditEventArgs e) {
            EventHandler<ListViewEditEventArgs> handler = (EventHandler<ListViewEditEventArgs>)Events[EventItemEditing];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (IsDataBindingAutomatic == false && e.Cancel == false) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_UnhandledEvent, ID, "ItemEditing"));
                }
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='ItemInserted '/>event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemInserted(ListViewInsertedEventArgs e) {
            EventHandler<ListViewInsertedEventArgs> handler = (EventHandler<ListViewInsertedEventArgs>)Events[EventItemInserted];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='Delete'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemInserting(ListViewInsertEventArgs e) {
            EventHandler<ListViewInsertEventArgs> handler = (EventHandler<ListViewInsertEventArgs>)Events[EventItemInserting];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (IsDataBindingAutomatic == false && e.Cancel == false) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_UnhandledEvent, ID, "ItemInserting"));
                }
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='ItemUpdated '/>event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemUpdated(ListViewUpdatedEventArgs e) {
            EventHandler<ListViewUpdatedEventArgs> handler = (EventHandler<ListViewUpdatedEventArgs>)Events[EventItemUpdated];
            if (handler != null) handler(this, e);
        }

        /// <devdoc>
        /// <para>Raises the <see langword='UpdateCommand'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnItemUpdating(ListViewUpdateEventArgs e) {
            EventHandler<ListViewUpdateEventArgs> handler = (EventHandler<ListViewUpdateEventArgs>)Events[EventItemUpdating];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (IsDataBindingAutomatic == false && e.Cancel == false) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_UnhandledEvent, ID, "ItemUpdating"));
                }
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='LayoutCreated'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnLayoutCreated(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventLayoutCreated];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='PagePropertiesChanged'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnPagePropertiesChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventPagePropertiesChanged];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='PagePropertiesChanging'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnPagePropertiesChanging(PagePropertiesChangingEventArgs e) {
            EventHandler<PagePropertiesChangingEventArgs> handler = (EventHandler<PagePropertiesChangingEventArgs>)Events[EventPagePropertiesChanging];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see cref='System.Web.UI.WebControls.ListView.TotalRowCountAvailable'/>event of a <see cref='System.Web.UI.WebControls.ListView'/>.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnTotalRowCountAvailable(PageEventArgs e) {
            EventHandler<PageEventArgs> handler = (EventHandler<PageEventArgs>)Events[EventTotalRowCountAvailable];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see cref='System.Web.UI.WebControls.ListView.SelectedIndexChanged'/>event of a <see cref='System.Web.UI.WebControls.ListView'/>.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnSelectedIndexChanged(EventArgs e) {            
            EventHandler handler = (EventHandler)Events[EventSelectedIndexChanged];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see cref='System.Web.UI.WebControls.ListView.SelectedIndexChanging'/>event of a <see cref='System.Web.UI.WebControls.ListView'/>.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnSelectedIndexChanging(ListViewSelectEventArgs e) {
            EventHandler<ListViewSelectEventArgs> handler = (EventHandler<ListViewSelectEventArgs>)Events[EventSelectedIndexChanging];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (IsDataBindingAutomatic == false && e.Cancel == false) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_UnhandledEvent, ID, "SelectedIndexChanging"));
                }
            }
        }

        /// <devdoc>
        /// <para>Raises the <see cref='System.Web.UI.WebControls.ListView.Sorted'/>event of a <see cref='System.Web.UI.WebControls.ListView'/>.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnSorted(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventSorted];
            if (handler != null) {
                handler(this, e);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see langword='SortCommand'/> event.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnSorting(ListViewSortEventArgs e) {
            EventHandler<ListViewSortEventArgs> handler = (EventHandler<ListViewSortEventArgs>)Events[EventSorting];
            if (handler != null) {
                handler(this, e);
            }
            else {
                if (IsDataBindingAutomatic == false && e.Cancel == false) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_UnhandledEvent, ID, "Sorting"));
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

        /// <summary>
        /// Overriden by DataBoundControl to use its properties to determine the real
        /// data source that the control should bind to. It then clears the existing
        /// control hierarchy, and calls createChildControls to create a new control
        /// hierarchy based on the resolved data source.
        /// The implementation resolves various data source related properties to
        /// arrive at the appropriate IEnumerable implementation to use as the real
        /// data source.
        /// When resolving data sources, the DataSourceControlID takes highest precedence.
        /// In this mode, DataMember is used to access the appropriate list from the
        /// DataControl.
        /// If DataSourceControlID is not set, the value of the DataSource property is used.
        /// In this second alternative, DataMember is used to extract the appropriate
        /// list if the control has been handed an IListSource as a data source.
        /// </summary>
        protected internal override void PerformDataBinding(IEnumerable data) {
            base.PerformDataBinding(data);

            TrackViewState();

            int controlCount = CreateChildControls(data, true);
            ChildControlsCreated = true;
            ViewState[ItemCountViewStateKey] = controlCount;

            int editIndex = EditIndex;
            if (IsDataBindingAutomatic && editIndex != -1 && editIndex < Items.Count && IsViewStateEnabled) {
                BoundFieldValues.Clear();
                ExtractItemValues(BoundFieldValues, Items[editIndex], false/*includePrimaryKey*/);
            }

            if (EnablePersistedSelection) {
                string[] keyNames = DataKeyNamesInternal;
                //we can't have persisted selection without having at least one key name
                if ((keyNames == null) || (keyNames.Length == 0)) {
                    throw new InvalidOperationException(AtlasWeb.ListView_PersistedSelectionRequiresDataKeysNames);
                }
            }
        }

        protected override void PerformSelect() {
            if (_performingSelect) {
                // Guard against databinding twice if we're currently databinding.
                // This happens when the ListView is nested within a databound control, and the call
                // To EnsureLayoutTemplate triggers a recursive DataBind.
                return;
            }

            try {
                _performingSelect = true;

                // If there is a DataPager in the layout template, we need the ListView's paging properties
                // to be set before we go to the datasource
                EnsureLayoutTemplate();

                if (DesignMode) {
                    // Try to find a pager that will control the paging on this control.
                    // In design mode, an embedded pager will not have a designer but will
                    // be the runtime control itself.  We want the max rows so the ListView
                    // renders with the right page size.
                    DataPager pager = FindDataPager(this);
                    if (pager != null) {
                        _maximumRows = pager.PageSize;
                    }
                }

                base.PerformSelect();
            }
            finally {
                _performingSelect = false;
            }
        }

        protected virtual void RemoveItems() {
            if (_groupTemplate != null) {
                // If we're in grouped mode, delete all the items created in CreateItemsInGroups().
                if (_groupsItemCreatedCount > 0) {
                    for (int i = 0; i < _groupsItemCreatedCount; i++) {
                        _groupsGroupPlaceholderContainer.Controls.RemoveAt(_groupsOriginalIndexOfGroupPlaceholderInContainer);
                    }
                    _groupsItemCreatedCount = 0;
                }
            }
            else {
                // If we're not in grouped mode, delete all the items
                // created in CreateItemsWithoutGroups().
                if (_noGroupsItemCreatedCount > 0) {
                    for (int i = 0; i < _noGroupsItemCreatedCount; i++) {
                        _noGroupsItemPlaceholderContainer.Controls.RemoveAt(_noGroupsOriginalIndexOfItemPlaceholderInContainer);
                    }
                    _noGroupsItemCreatedCount = 0;
                }
            }
            _autoIDIndex = 0;
        }

        protected internal override void Render(HtmlTextWriter writer) {
            // Render only the contents.  We don't want a rendered span tag around the control.
            RenderContents(writer);
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
                _startRowIndex > 0 ||
                _maximumRows != -1 ||
                _editIndex != -1 ||
                _selectedIndex != -1 ||
                _groupItemCount != 1 ||
                (_sortExpression != null && _sortExpression.Length != 0) ||
                _sortDirection != SortDirection.Ascending ||
                _totalRowCount != -1 ||
                (_dataKeyNames != null && _dataKeyNames.Length != 0) ||
                (_dataKeysArrayList != null && _dataKeysArrayList.Count > 0)) {

                object[] state = new object[14];

                state[0] = baseState;
                state[1] = (_editIndex == -1) ? null : (object)_editIndex;
                state[2] = (_selectedIndex == -1) ? null : (object)_selectedIndex;
                state[3] = (_groupItemCount == 1) ? null : (object)_groupItemCount;
                state[4] = (_sortExpression == null || _sortExpression.Length == 0) ? null : (object)_sortExpression;
                state[5] = (_sortDirection == SortDirection.Ascending) ? null : (object)((int)_sortDirection);
                state[6] = (_dataKeyNames == null || _dataKeyNames.Length == 0) ? null : (object)_dataKeyNames;
                state[7] = SaveDataKeysState();
                state[8] = (_totalRowCount == -1) ? null : (object)_totalRowCount;
                state[9] = (_persistedDataKey == null) ? null :
                    ((IStateManager)_persistedDataKey).SaveViewState();
                state[10] = (_clientIDRowSuffix == null || _clientIDRowSuffix.Length == 0) ? null : (object)_clientIDRowSuffix;
                state[11] = SaveClientIDRowSuffixDataKeysState();
                state[12] = _startRowIndex;
                state[13] = _maximumRows;

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

        protected override object SaveViewState() {
            object baseState = base.SaveViewState();
            object boundFieldValuesState = (_boundFieldValues != null) ? OrderedDictionaryStateHelper.SaveViewState(_boundFieldValues) : null;

            object[] state = new object[2];
            state[0] = baseState;
            state[1] = boundFieldValuesState;

            return state;
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

        private void SelectCallback(IEnumerable data) {
            // The data source should have thrown.  If we're here, it didn't.  We'll throw for it
            // with a generic message.
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ListView_DataSourceDoesntSupportPaging, DataSourceID));
        }

        private void SetRequiresDataBindingIfInitialized() {
            if (Initialized) {
                RequiresDataBinding = true;
            }
        }

        public virtual void Sort(string sortExpression, SortDirection sortDirection) {
            HandleSort(sortExpression, sortDirection);
        }

        public virtual void UpdateItem(int itemIndex, bool causesValidation) {
            ResetModelValidationGroup(causesValidation, String.Empty);
            HandleUpdate(null, itemIndex, causesValidation);
        }

        internal override void UpdateModelDataSourceProperties(ModelDataSource modelDataSource) {
            Debug.Assert(modelDataSource != null, "A non-null ModelDataSource should be passed in");
            string dataKeyName = DataKeyNamesInternal.Length > 0 ? DataKeyNamesInternal[0] : "";
            modelDataSource.UpdateProperties(ItemType, SelectMethod, UpdateMethod, InsertMethod, DeleteMethod, dataKeyName);
        }

        #region IPageableItemContainer
        int IPageableItemContainer.StartRowIndex {
            get {
                return StartRowIndex;
            }
        }

        // Overridable version
        protected virtual int StartRowIndex {
            get {
                return _startRowIndex;
            }
        }

        int IPageableItemContainer.MaximumRows {
            get {
                return MaximumRows;
            }
        }

        // Overridable version
        protected virtual int MaximumRows {
            get {
                return _maximumRows;
            }
        }

        void IPageableItemContainer.SetPageProperties(int startRowIndex, int maximumRows, bool databind) {
            SetPageProperties(startRowIndex, maximumRows, databind);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "A property already exists. This method does additional work.")]
        public void SelectItem(int rowIndex) {
            HandleSelect(rowIndex);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "A property already exists. This method does additional work.")]
        public void SetEditItem(int rowIndex) {
            HandleEdit(rowIndex);
        }

        // Overridable version
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "databind",
            Justification = "Cannot change to 'dataBind' as would break binary compatibility with legacy code.")]
        protected virtual void SetPageProperties(int startRowIndex, int maximumRows, bool databind) {
            if (maximumRows < 1) {
                throw new ArgumentOutOfRangeException("maximumRows");
            }
            if (startRowIndex < 0) {
                throw new ArgumentOutOfRangeException("startRowIndex");
            }

            if (_startRowIndex != startRowIndex || _maximumRows != maximumRows) {
                PagePropertiesChangingEventArgs args = new PagePropertiesChangingEventArgs(startRowIndex, maximumRows);
                if (databind) {
                    // This event is cancellable, and its properties aren't settable, because changing them would
                    // create a strange disconnect between the pager and the ListView.  You have to set
                    // these properties on the pager if you want to change them.  This is a notification event.
                    OnPagePropertiesChanging(args);
                }

                _startRowIndex = args.StartRowIndex;
                _maximumRows = args.MaximumRows;

                if (databind) {
                    OnPagePropertiesChanged(EventArgs.Empty);
                }
            }

            if (databind) {
                RequiresDataBinding = true;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
                         Justification = "Unlikely that a derived type would re-implement this event.")]
        event EventHandler<PageEventArgs> IPageableItemContainer.TotalRowCountAvailable {
            add {
                Events.AddHandler(EventTotalRowCountAvailable, value);
            }
            remove {
                Events.RemoveHandler(EventTotalRowCountAvailable, value);
            }
        }
        #endregion IPageableItemContainer

        #region IPersistedSelector implementation

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

        string IDataBoundControl.DataSourceID {
            get {
                return DataSourceID;
            }
            set {
                DataSourceID = value;
            }
        }

        IDataSource IDataBoundControl.DataSourceObject {
            get { 
                return DataSourceObject; 
            }
        }

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

        string IDataBoundControl.DataMember {
            get {
                return DataMember;
            }
            set {
                DataMember = value;
            }
        }

        #endregion

        #region IWizardSideBarListControl implementation

        IEnumerable IWizardSideBarListControl.Items {
            get { return Items; }
        }

        event CommandEventHandler IWizardSideBarListControl.ItemCommand {
            add {
                ItemCommand += new EventHandler<ListViewCommandEventArgs>(value);
            }
            remove {
                ItemCommand -= new EventHandler<ListViewCommandEventArgs>(value);
            }

        }

        event EventHandler<WizardSideBarListControlItemEventArgs> IWizardSideBarListControl.ItemDataBound {
            add {
                Events.AddHandler(EventWizardListItemDataBound, value);
            }
            remove {
                Events.RemoveHandler(EventWizardListItemDataBound, value);
            }
        }

        #endregion
    }
}
