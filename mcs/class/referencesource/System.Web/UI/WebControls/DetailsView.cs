//------------------------------------------------------------------------------
// <copyright file="DetailsView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.UI.Adapters;
    using System.Web.Util;
    using System.Web.UI.WebControls.Adapters;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Security.Cryptography;

    /// <devdoc>
    ///    <para>
    ///       Displays a data record from a data source in a table layout. The data source
    ///       is any object that implements IEnumerable or IListSource, which includes ADO.NET data,
    ///       arrays, ArrayLists, DataSourceControl, etc.
    ///    </para>
    /// </devdoc>
    [
    Designer("System.Web.UI.Design.WebControls.DetailsViewDesigner, " + AssemblyRef.SystemDesign),
    ControlValueProperty("SelectedValue"),
    DefaultEvent("PageIndexChanging"),
    ToolboxData("<{0}:DetailsView runat=\"server\" Width=\"125px\" Height=\"50px\"></{0}:DetailsView>"),
    SupportsEventValidation
    ]
    [DataKeyProperty("DataKey")]
    public class DetailsView : CompositeDataBoundControl, IDataItemContainer, ICallbackContainer, ICallbackEventHandler, IPostBackEventHandler, IPostBackContainer, IDataBoundItemControl, IFieldControl {

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
        private static readonly object EventPageIndexChanged = new object();
        private static readonly object EventPageIndexChanging = new object();

        private ITemplate _headerTemplate;
        private ITemplate _footerTemplate;
        private ITemplate _pagerTemplate;
        private ITemplate _emptyDataTemplate;

        private TableItemStyle _rowStyle;
        private TableItemStyle _headerStyle;
        private TableItemStyle _footerStyle;
        private TableItemStyle _editRowStyle;
        private TableItemStyle _alternatingRowStyle;
        private TableItemStyle _commandRowStyle;
        private TableItemStyle _insertRowStyle;
        private TableItemStyle _emptyDataRowStyle;
        private TableItemStyle _fieldHeaderStyle;

        private DetailsViewRow _bottomPagerRow;
        private DetailsViewRow _footerRow;
        private DetailsViewRow _headerRow;
        private DetailsViewRow _topPagerRow;

        private TableItemStyle _pagerStyle;
        private PagerSettings _pagerSettings;

        private ArrayList _rowsArray;
        private DataControlFieldCollection _fieldCollection;
        private DetailsViewRowCollection _rowsCollection;
        private int _pageCount;
        private object _dataItem;
        private int _dataItemIndex;
        private OrderedDictionary _boundFieldValues;
        private DataKey _dataKey;
        private OrderedDictionary _keyTable;
        private string[] _dataKeyNames;

        private int _pageIndex;
        private DetailsViewMode _defaultMode = DetailsViewMode.ReadOnly;
        private DetailsViewMode _mode;
        private bool _modeSet;
        private bool _useServerPaging;
        private string _modelValidationGroup;

        private bool _renderClientScript;
        private bool _renderClientScriptValid = false;

        private IAutoFieldGenerator _rowsGenerator;
        private DetailsViewRowsGenerator _defaultRowsGenerator = new DetailsViewRowsGenerator();

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
        WebSysDescription(SR.DetailsView_AllowPaging)
        ]
        public virtual bool AllowPaging {
            get {
                object o = ViewState["AllowPaging"];
                if (o != null)
                    return(bool)o;
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
        /// <para>Indicates the style properties of alternating rows.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DetailsView_AlternatingRowStyle)
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
        /// <para>Gets or sets a value that indicates whether a button field for deleting will automatically
        /// be created.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.DetailsView_AutoGenerateDeleteButton)
        ]
        public virtual bool AutoGenerateDeleteButton {
            get {
                object o = ViewState["AutoGenerateDeleteButton"];
                if (o != null)
                    return(bool)o;
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
        /// <para>Gets or sets a value that indicates whether an edit field will automatically
        /// be created.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.DetailsView_AutoGenerateEditButton)
        ]
        public virtual bool AutoGenerateEditButton {
            get {
                object o = ViewState["AutoGenerateEditButton"];
                if (o != null)
                    return(bool)o;
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
        /// <para>Gets or sets a value that indicates whether an insert field will automatically
        /// be created.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.DetailsView_AutoGenerateInsertButton)
        ]
        public virtual bool AutoGenerateInsertButton {
            get {
                object o = ViewState["AutoGenerateInsertButton"];
                if (o != null)
                    return(bool)o;
                return false;
            }
            set {
                bool oldValue = AutoGenerateInsertButton;
                if (value != oldValue) {
                    ViewState["AutoGenerateInsertButton"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets a value that indicates whether fields will automatically
        /// be created for each bound data field.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(true),
        WebSysDescription(SR.DetailsView_AutoGenerateRows)
        ]
        public virtual bool AutoGenerateRows {
            get {
                object o = ViewState["AutoGenerateRows"];
                if (o != null)
                    return(bool)o;
                return true;
            }
            set {
                bool oldValue = AutoGenerateRows;
                if (value != oldValue) {
                    ViewState["AutoGenerateRows"] = value;
                    if (Initialized) {
                        RequiresDataBinding = true;
                    }
                }
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the URL of an image to display in the
        /// background of the <see cref='System.Web.UI.WebControls.DetailsView'/>.</para>
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
                return((TableStyle)ControlStyle).BackImageUrl;
            }
            set {
                ((TableStyle)ControlStyle).BackImageUrl = value;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual DetailsViewRow BottomPagerRow {
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
                    int capacity = Fields.Count;
                    if (AutoGenerateRows) {
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
        /// <para>Indicates the amount of space between cells.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(-1),
        WebSysDescription(SR.DetailsView_CellPadding)
        ]
        public virtual int CellPadding {
            get {
                if (ControlStyleCreated == false) {
                    return -1;
                }
                return((TableStyle)ControlStyle).CellPadding;
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
        WebSysDescription(SR.DetailsView_CellSpacing)
        ]
        public virtual int CellSpacing {
            get {
                if (ControlStyleCreated == false) {
                    return 0;
                }
                return((TableStyle)ControlStyle).CellSpacing;
            }
            set {
                ((TableStyle)ControlStyle).CellSpacing = value;
            }
        }


        /// <devdoc>
        /// <para>Indicates the style properties of command rows.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DetailsView_CommandRowStyle)
        ]
        public TableItemStyle CommandRowStyle {
            get {
                if (_commandRowStyle == null) {
                    _commandRowStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_commandRowStyle).TrackViewState();
                }
                return _commandRowStyle;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public DetailsViewMode CurrentMode {
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
                if (CurrentMode == DetailsViewMode.Insert) {
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
                if (CurrentMode == DetailsViewMode.Insert) {
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
                    return(string[])((string[])o).Clone();
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
        WebSysDescription(SR.DetailsView_DataKey)
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
        DefaultValue(DetailsViewMode.ReadOnly),
        WebSysDescription(SR.View_DefaultMode)
        ]
        public virtual DetailsViewMode DefaultMode {
            get {
                return _defaultMode;
            }
            set {
                if (value < DetailsViewMode.ReadOnly || value > DetailsViewMode.Insert) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _defaultMode = value;
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
        /// <para>Indicates the template to use when no records are returned from the datasource within the DetailsView.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DetailsView)),
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
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.DetailsView_EnablePagingCallbacks)
        ]
        public virtual bool EnablePagingCallbacks {
            get {
                object o = ViewState["EnablePagingCallbacks"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                ViewState["EnablePagingCallbacks"] = value;
            }
        }


        /// <devdoc>
        /// <para>Indicates the style properties of the header column.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DetailsView_FieldHeaderStyle)
        ]
        public TableItemStyle FieldHeaderStyle {
            get {
                if (_fieldHeaderStyle == null) {
                    _fieldHeaderStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)_fieldHeaderStyle).TrackViewState();
                }
                return _fieldHeaderStyle;
            }
        }


        /// <devdoc>
        /// <para>Gets a collection of <see cref='System.Web.UI.WebControls.DataControlField'/> controls in the <see cref='System.Web.UI.WebControls.DetailsView'/>. This property is read-only.</para>
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.DataControlFieldTypeEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Default"),
        WebSysDescription(SR.DetailsView_Fields)
        ]
        public virtual DataControlFieldCollection Fields {
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
        public virtual DetailsViewRow FooterRow {
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
        WebSysDescription(SR.DetailsView_FooterStyle)
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
        /// <para>Indicates the template to use for a footer item within the DetailsView.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DetailsView)),
        WebSysDescription(SR.DetailsView_FooterTemplate)
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
        DefaultValue(GridLines.Both),
        WebSysDescription(SR.DataControls_GridLines)
        ]
        public virtual GridLines GridLines {
            get {
                if (ControlStyleCreated == false) {
                    return GridLines.Both;
                }
                return((TableStyle)ControlStyle).GridLines;
            }
            set {
                ((TableStyle)ControlStyle).GridLines = value;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual DetailsViewRow HeaderRow {
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
        /// <para>Indicates the template to use for a header item within the DetailsView.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DetailsView)),
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
                return((TableStyle)ControlStyle).HorizontalAlign;
            }
            set {
                ((TableStyle)ControlStyle).HorizontalAlign = value;
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

        private OrderedDictionary KeyTable {
            get {
                if (_keyTable == null) {
                    _keyTable = new OrderedDictionary(DataKeyNamesInternal.Length);
                }
                return _keyTable;
            }
        }


        private DetailsViewMode Mode {
            get {
                // if the mode wasn't explicitly set by LoadControlState or by the user, the mode is the DefaultMode.
                if (!_modeSet || DesignMode) {
                    _mode = DefaultMode;
                    _modeSet = true;
                }
                return _mode;
            }
            set {
                if (value < DetailsViewMode.ReadOnly || value > DetailsViewMode.Insert) {
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
        WebSysDescription(SR.DetailsView_PageIndex)
        ]
        public virtual int PageIndex {
            get {
                // if we're in design mode, we don't want a change to the mode to set the PageIndex to -1.
                if (Mode == DetailsViewMode.Insert && !DesignMode) {
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
        /// <see cref='System.Web.UI.WebControls.DetailsView'/>. This
        /// property is read-only.</para>
        /// </devdoc>
        [
        WebCategory("Paging"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DetailsView_PagerSettings)
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
        /// <see cref='System.Web.UI.WebControls.DetailsView'/>. This
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
        /// <para>Indicates the template to use for a pager item within the DetailsView.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DetailsView)),
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
        /// <para>Gets a collection of <see cref='System.Web.UI.WebControls.DetailsViewRow'/> objects representing the individual
        /// rows within the control.
        /// This property is read-only.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.DetailsView_Rows)
        ]
        public virtual DetailsViewRowCollection Rows {
            get {
                if (_rowsCollection == null) {
                    if (_rowsArray == null) {
                        EnsureChildControls();
                    }
                    if (_rowsArray == null) {
                        _rowsArray = new ArrayList();
                    }
                    _rowsCollection = new DetailsViewRowCollection(_rowsArray);
                }
                return _rowsCollection;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public IAutoFieldGenerator RowsGenerator {
            get {
                return _rowsGenerator;
            }
            set {
                _rowsGenerator = value;
            }
        }

        private IAutoFieldGenerator RowsGeneratorInternal {
            get {
                return RowsGenerator ?? _defaultRowsGenerator;
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
                return EnablePagingCallbacks ? 
                    HtmlTextWriterTag.Div : HtmlTextWriterTag.Table;
            }
        }


        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual DetailsViewRow TopPagerRow {
            get {
                if (_topPagerRow == null) {
                    EnsureChildControls();
                }
                return _topPagerRow;
            }
        }


        /// <devdoc>
        /// <para>Occurs when a command is issued from the DetailsView.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DetailsView_OnItemCommand)
        ]
        public event DetailsViewCommandEventHandler ItemCommand {
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
        WebSysDescription(SR.DetailsView_OnItemCreated)
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
        /// <para>Occurs when the DetailsView item has been deleted.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemDeleted)
        ]
        public event DetailsViewDeletedEventHandler ItemDeleted {
            add {
                Events.AddHandler(EventItemDeleted, value);
            }
            remove {
                Events.RemoveHandler(EventItemDeleted, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the DetailsView item is being deleted.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemDeleting)
        ]
        public event DetailsViewDeleteEventHandler ItemDeleting {
            add {
                Events.AddHandler(EventItemDeleting, value);
            }
            remove {
                Events.RemoveHandler(EventItemDeleting, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the DetailsView item has been inserted.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemInserted)
        ]
        public event DetailsViewInsertedEventHandler ItemInserted {
            add {
                Events.AddHandler(EventItemInserted, value);
            }
            remove {
                Events.RemoveHandler(EventItemInserted, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the DetailsView item is being inserted.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemInserting)
        ]
        public event DetailsViewInsertEventHandler ItemInserting {
            add {
                Events.AddHandler(EventItemInserting, value);
            }
            remove {
                Events.RemoveHandler(EventItemInserting, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the DetailsView item has been updated.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemUpdated)
        ]
        public event DetailsViewUpdatedEventHandler ItemUpdated {
            add {
                Events.AddHandler(EventItemUpdated, value);
            }
            remove {
                Events.RemoveHandler(EventItemUpdated, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the DetailsView item is being updated.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataControls_OnItemUpdating)
        ]
        public event DetailsViewUpdateEventHandler ItemUpdating {
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
        WebSysDescription(SR.DetailsView_OnModeChanged)
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
        WebSysDescription(SR.DetailsView_OnModeChanging)
        ]
        public event DetailsViewModeEventHandler ModeChanging {
            add {
                Events.AddHandler(EventModeChanging, value);
            }
            remove {
                Events.RemoveHandler(EventModeChanging, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when the DetailsView PageIndex has been changed.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DetailsView_OnPageIndexChanged)
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
        /// <para>Occurs when the DetailsView PageIndex is changing.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DetailsView_OnPageIndexChanging)
        ]
        public event DetailsViewPageEventHandler PageIndexChanging {
            add {
                Events.AddHandler(EventPageIndexChanging, value);
            }
            remove {
                Events.RemoveHandler(EventPageIndexChanging, value);
            }
        }
        
        /// <devdoc>
        /// <para>Builds the callback argument used in DataControlLinkButtons.</para>
        /// </devdoc>
        private string BuildCallbackArgument(int pageIndex) {
            return "\"" + Convert.ToString(pageIndex, CultureInfo.InvariantCulture) + "|\"";
        }


        public void ChangeMode(DetailsViewMode newMode) {
            Mode = newMode;
        }

        /// <devdoc>
        /// Create a single autogenerated row.  This function can be overridden to create a different AutoGeneratedField.
        /// </devdoc>
        protected virtual AutoGeneratedField CreateAutoGeneratedRow(AutoGeneratedFieldProperties fieldProperties) {
            AutoGeneratedField field = new AutoGeneratedField(fieldProperties.DataField);
            string name = fieldProperties.Name;
            ((IStateManager)field).TrackViewState();

            field.HeaderText = name;
            field.SortExpression = name;
            field.ReadOnly = fieldProperties.IsReadOnly;
            field.DataType = fieldProperties.Type;

            return field;
        }


        /// <summary>
        /// Creates the set of AutoGenerated rows.  This function cannot be overridden because then if someone
        /// overrides it to add another type of DataControlField to the control, we have to manage the states of those
        /// fields along with their types.
        /// This could become Obsolete in future versions.
        /// </summary>
        protected virtual ICollection CreateAutoGeneratedRows(object dataItem) {
            return _defaultRowsGenerator.CreateAutoGeneratedFields(dataItem, this);
        }


        /// <devdoc>
        /// <para>Creates the control hierarchy that is used to render the DetailsView.
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
            DetailsViewMode mode = Mode;

            // if we're in design mode, PageIndex doesn't return -1
            if (DesignMode && mode == DetailsViewMode.Insert) {
                itemIndex = -1;
            }

            if (dataBinding) {
                DataSourceView view = GetData();
                DataSourceSelectArguments arguments = SelectArguments;
                
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.DataBoundControl_NullView, ID));
                }

                if (mode != DetailsViewMode.Insert) {
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
                        }
                        else {
                            ICollection dataSourceCollection = dataSource as ICollection;
                            if (dataSourceCollection == null) {
                                throw new HttpException(SR.GetString(SR.DataBoundControl_NeedICollectionOrTotalRowCount, GetType().Name));
                            }
                            pagedDataSource = CreateServerPagedDataSource(checked(PageIndex + dataSourceCollection.Count));
                        }
                    }
                    else {
                        pagedDataSource = CreatePagedDataSource();
                    }
                }
            }
            else {
                pagedDataSource = CreatePagedDataSource();
            }

            if (mode != DetailsViewMode.Insert) {
                pagedDataSource.DataSource = dataSource;
            }
            
            IEnumerator dataSourceEnumerator = null;
            OrderedDictionary keyTable = KeyTable;

            _rowsArray = new ArrayList();
            _rowsCollection = null;

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
                    if (mode != DetailsViewMode.Insert) {
                        ICollection collection = dataSource as ICollection;
                        if ((collection == null) && (pagedDataSource.IsPagingEnabled && !pagedDataSource.IsServerPagingEnabled)) {
                            throw new HttpException(SR.GetString(SR.DetailsView_DataSourceMustBeCollection, ID));
                        }

                        if (pagedDataSource.IsPagingEnabled) {
                            itemCount = pagedDataSource.DataSourceCount;
                        }
                        else if (collection != null) {
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
            if (!moveNextSucceeded && mode != DetailsViewMode.Insert) {
                // if we're in insert mode and we're not autogenerating rows, render the rows in insert mode
                if (itemIndex >= 0 || AutoGenerateRows) {
                    if (EmptyDataText.Length > 0 || _emptyDataTemplate != null) {
                        _rowsArray.Add(CreateRow(0, DataControlRowType.EmptyDataRow, DataControlRowState.Normal, null, rows, null));
                    }
                    itemCount = 0;
                }
            }
            else {
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
                            break;  // never throw if the PageIndex is out of range: just fix up the current page and goto the last item.
                        }
                    }
                }

                if (moveNextSucceeded) {
                    _dataItem = dataSourceEnumerator.Current;
                }
                else {
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
                if (allowPaging && PagerSettings.Visible && _pagerSettings.IsPagerOnTop && !singlePage && mode != DetailsViewMode.Insert) {
                    // top pager
                    _topPagerRow = CreateRow(-1, DataControlRowType.Pager, DataControlRowState.Normal, null, rows, pagedDataSource);
                }

                _headerRow = CreateRow(-1, DataControlRowType.Header, DataControlRowState.Normal, null, rows, null);
                if (_headerTemplate == null && HeaderText.Length == 0) {
                    _headerRow.Visible = false;
                }

                _rowsArray.AddRange(CreateDataRows(dataBinding, rows, _dataItem));

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

                _footerRow = CreateRow(-1, DataControlRowType.Footer, DataControlRowState.Normal, null, rows, null);
                if (_footerTemplate == null && FooterText.Length == 0) {
                    _footerRow.Visible = false;
                }

                if (allowPaging && PagerSettings.Visible && _pagerSettings.IsPagerOnBottom && !singlePage && mode != DetailsViewMode.Insert) {
                    // bottom pager
                    _bottomPagerRow = CreateRow(-1, DataControlRowType.Pager, DataControlRowState.Normal, null, rows, pagedDataSource);
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
            controlStyle.GridLines = GridLines.Both;
            controlStyle.CellSpacing = 0;

            return controlStyle;
        }

        private ICollection CreateDataRows(bool dataBinding, TableRowCollection rows, object dataItem) {
            ArrayList rowsArray = new ArrayList();
            rowsArray.AddRange(CreateDataRowsFromFields(dataItem, dataBinding, rows));
            return rowsArray;
        }

        private ICollection CreateDataRowsFromFields(object dataItem, bool dataBinding, TableRowCollection rows) {
            int fieldCount = 0;
            ICollection fields = CreateFieldSet(dataItem, dataBinding);
            ArrayList rowsArray = new ArrayList();
            if (fields != null)
                fieldCount = fields.Count;

            if (fieldCount > 0) {
                DataControlRowType rowType = DataControlRowType.DataRow;
                DataControlRowState masterRowState = DataControlRowState.Normal;
                int dataRowIndex = 0;
                DetailsViewMode mode = Mode;

                if (mode == DetailsViewMode.Edit)
                    masterRowState |= DataControlRowState.Edit;
                else if (mode == DetailsViewMode.Insert)
                    masterRowState |= DataControlRowState.Insert;

                bool requiresDataBinding = false;
                foreach (DataControlField field in fields) {
                    if (field.Initialize(false, this)) {
                        requiresDataBinding = true;
                    }
                    if (DetermineRenderClientScript()) {
                        field.ValidateSupportsCallback();
                    }

                    DataControlRowState rowState = masterRowState;

                    if (dataRowIndex % 2 != 0) {
                        rowState |= DataControlRowState.Alternate;
                    }

                    rowsArray.Add(CreateRow(dataRowIndex, rowType, rowState, field, rows, null));

                    dataRowIndex++;
                }
                if (requiresDataBinding) {
                    RequiresDataBinding = true;
                }
            }
            return rowsArray;
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
                }
                else {
                    arguments.MaximumRows = -1;
                }
            }

            return arguments;
        }


        /// <devdoc>
        /// Creates the set of fields to be used to build up the control
        /// hierarchy.
        /// When AutoGenerateRows is true, the fields are created to match the
        /// datasource and are appended to the set of fields defined in the Fields
        /// collection.
        /// </devdoc>
        protected virtual ICollection CreateFieldSet(object dataItem, bool useDataSource) {
            ArrayList fieldsArray = new ArrayList();

            if (AutoGenerateRows == true) {
                if (RowsGeneratorInternal is DetailsViewRowsGenerator) {
                    ((DetailsViewRowsGenerator)RowsGeneratorInternal).DataItem = dataItem;
                    ((DetailsViewRowsGenerator)RowsGeneratorInternal).InDataBinding = useDataSource;
                }
                fieldsArray.AddRange(RowsGeneratorInternal.GenerateFields(this));
            }

            foreach (DataControlField f in Fields) {
                fieldsArray.Add(f);
            }

            if (AutoGenerateInsertButton || AutoGenerateDeleteButton || AutoGenerateEditButton) {
                CommandField commandField = new CommandField();
                commandField.ButtonType = ButtonType.Link;

                if (AutoGenerateInsertButton) {
                    commandField.ShowInsertButton = true;
                }

                if (AutoGenerateDeleteButton) {
                    commandField.ShowDeleteButton = true;
                }

                if (AutoGenerateEditButton) {
                    commandField.ShowEditButton = true;
                }
                fieldsArray.Add(commandField);
            }

            return fieldsArray;
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
                    ((DataControlImageButton)firstButton).ImageUrl = firstPageImageUrl;
                    ((DataControlImageButton)firstButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.FirstPageText);
                    ((DataControlImageButton)firstButton).EnableCallback(BuildCallbackArgument(0));
                } else {
                    firstButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)firstButton).Text = pagerSettings.FirstPageText;
                    ((DataControlPagerLinkButton)firstButton).EnableCallback(BuildCallbackArgument(0));
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
                    ((DataControlImageButton)prevButton).ImageUrl = prevPageImageUrl;
                    ((DataControlImageButton)prevButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.PreviousPageText);
                    ((DataControlImageButton)prevButton).EnableCallback(BuildCallbackArgument(PageIndex - 1));
                } else {
                    prevButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)prevButton).Text = pagerSettings.PreviousPageText;
                    ((DataControlPagerLinkButton)prevButton).EnableCallback(BuildCallbackArgument(PageIndex - 1));
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
                    ((DataControlImageButton)nextButton).ImageUrl = nextPageImageUrl;
                    ((DataControlImageButton)nextButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.NextPageText);
                    ((DataControlImageButton)nextButton).EnableCallback(BuildCallbackArgument(PageIndex + 1));
                } else {
                    nextButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)nextButton).Text = pagerSettings.NextPageText;
                    ((DataControlPagerLinkButton)nextButton).EnableCallback(BuildCallbackArgument(PageIndex + 1));
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
                    ((DataControlImageButton)lastButton).ImageUrl = lastPageImageUrl;
                    ((DataControlImageButton)lastButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.LastPageText);
                    ((DataControlImageButton)lastButton).EnableCallback(BuildCallbackArgument(pagedDataSource.PageCount - 1));
                } else {
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
                int currentPageSet = (currentPage - 1) / pageSetSize;
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
                string firstPageImageUrl = pagerSettings.FirstPageImageUrl;
                IButtonControl firstButton;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);
                
                if (firstPageImageUrl.Length > 0) {
                    firstButton = new DataControlImageButton(this);
                    ((DataControlImageButton)firstButton).ImageUrl = firstPageImageUrl;
                    ((DataControlImageButton)firstButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.FirstPageText);
                    ((DataControlImageButton)firstButton).EnableCallback(BuildCallbackArgument(0));
                } else {
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
                } else {
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
                string lastPageImageUrl = pagerSettings.LastPageImageUrl;
                TableCell cell = new TableCell();
                row.Cells.Add(cell);

                IButtonControl lastButton;
                if (lastPageImageUrl.Length > 0) {
                    lastButton = new DataControlImageButton(this);
                    ((DataControlImageButton)lastButton).ImageUrl = lastPageImageUrl;
                    ((DataControlImageButton)lastButton).AlternateText = HttpUtility.HtmlDecode(pagerSettings.LastPageText);
                    ((DataControlImageButton)lastButton).EnableCallback(BuildCallbackArgument(pagedDataSource.PageCount - 1));
                } else {
                    lastButton = new DataControlPagerLinkButton(this);
                    ((DataControlPagerLinkButton)lastButton).Text = pagerSettings.LastPageText;
                    ((DataControlPagerLinkButton)lastButton).EnableCallback(BuildCallbackArgument(pagedDataSource.PageCount - 1));
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

        private DetailsViewRow CreateRow(int rowIndex, DataControlRowType rowType, DataControlRowState rowState, DataControlField field, TableRowCollection rows, PagedDataSource pagedDataSource) {
            DetailsViewRow row = CreateRow(rowIndex, rowType, rowState);

            rows.Add(row);
            if (rowType != DataControlRowType.Pager) {
                InitializeRow(row, field);
            } else {
                InitializePager(row, pagedDataSource);
            }

            return row;
        }


        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual DetailsViewRow CreateRow(int rowIndex, DataControlRowType rowType, DataControlRowState rowState) {
            if (rowType == DataControlRowType.Pager) {
                return new DetailsViewPagerRow(rowIndex, rowType, rowState);
            }
            return new DetailsViewRow(rowIndex, rowType, rowState);
        }


        /// <devdoc>
        /// Creates a new Table, which is the containing table
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

        private bool DetermineRenderClientScript() {
            // In a client script-enabled control, always determine whether to render the
            // client script-based functionality.
            // The decision should be based on browser capabilities.

            if (!_renderClientScriptValid) {
                _renderClientScript = false;
    
                if (EnablePagingCallbacks && (Context != null) && (Page != null) && (Page.RequestInternal != null) && Page.Request.Browser.SupportsCallback && !IsParentedToUpdatePanel) {
                    HttpBrowserCapabilities browserCaps = Page.Request.Browser;
                    bool hasEcmaScript = browserCaps.EcmaScriptVersion.Major > 0;
                    bool hasDOM = browserCaps.W3CDomVersion.Major > 0;
                    bool isHtml4 = (!StringUtil.EqualsIgnoreCase(browserCaps["tagwriter"], typeof(Html32TextWriter).FullName));
                    _renderClientScript = hasEcmaScript && hasDOM && isHtml4;
                }
                _renderClientScriptValid = true;
            }
            return _renderClientScript;
        }

        /// <devdoc>
        /// Override EnsureDataBound because we don't want to databind when we're in insert mode
        /// </devdoc>
        protected override void EnsureDataBound() {
            // We don't have to databind if we're using a RowsGenerator
            if (RequiresDataBinding && Mode == DetailsViewMode.Insert && (!AutoGenerateRows || (AutoGenerateRows && RowsGenerator != null))) {
                OnDataBinding(EventArgs.Empty);

                RequiresDataBinding = false;
                MarkAsDataBound();
                if (AdapterInternal != null) {
                    DataBoundControlAdapter dataBoundControlAdapter = AdapterInternal as DataBoundControlAdapter;
                    if(dataBoundControlAdapter != null) {
                        dataBoundControlAdapter.PerformDataBinding(null);
                    }
                    else {
                        PerformDataBinding(null);
                    }
                }
                else {
                    PerformDataBinding(null);
                }

                OnDataBound(EventArgs.Empty);
            }
            else {
                base.EnsureDataBound();
            }
        }

        internal static void ExtractRowValues(object[] fields, DetailsViewRowCollection rows, string[] dataKeyNames, IOrderedDictionary fieldValues, bool includeReadOnlyFields, bool includeKeys) {
            int cellIndex;
            // Field and row count should match, but if there was no data, or if the user removed some rows,
            // these may no longer match.  Make sure we don't exceed the bounds.
            for (int i = 0; i < fields.Length && i < rows.Count; i++) {
                // If the row isn't a DataRow then skip it
                if (rows[i].RowType != DataControlRowType.DataRow) {
                    continue;
                }
                cellIndex = 0;
                if (((DataControlField)fields[i]).ShowHeader) {
                    cellIndex = 1;
                }

                if (!((DataControlField)fields[i]).Visible) {
                    continue;
                }

                OrderedDictionary newValues = new OrderedDictionary();

                ((DataControlField)fields[i]).ExtractValuesFromCell(newValues, rows[i].Cells[cellIndex] as DataControlFieldCell, rows[i].RowState, includeReadOnlyFields);
                foreach (DictionaryEntry entry in newValues) {
                    if (includeKeys || (Array.IndexOf(dataKeyNames, entry.Key) == -1)) {
                        fieldValues[entry.Key] = entry.Value;
                    }
                }

            }
        }

        protected virtual void ExtractRowValues(IOrderedDictionary fieldValues, bool includeReadOnlyFields, bool includeKeys) {
            if (fieldValues == null) {
                Debug.Assert(false, "DetailsView::ExtractRowValues- must hand in a valid reference to an IDictionary.");
                return;
            }

            ICollection fieldSet = CreateFieldSet(null, false);
            object[] fields = new object[fieldSet.Count];
            fieldSet.CopyTo(fields, 0);
            DetailsViewRowCollection rows = Rows;
            string[] dataKeyNames = DataKeyNamesInternal;

            ExtractRowValues(fields, rows, DataKeyNamesInternal, fieldValues, includeReadOnlyFields, includeKeys);
        }

        protected virtual string GetCallbackResult() {
            StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
            // 
            HtmlTextWriter writer = new HtmlTextWriter(stringWriter);
            IStateFormatter2 formatter = Page.CreateStateFormatter();

            RenderTableContents(writer);

            writer.Flush();
            writer.Close();

            object dataKeyState = OrderedDictionaryStateHelper.SaveViewState(KeyTable);
            string dataKeyString = formatter.Serialize(dataKeyState, Purpose.WebForms_DetailsView_KeyTable);

            // this should return the html that goes in the panel, plus the new page info.
            return Convert.ToString(PageIndex, CultureInfo.InvariantCulture) + "|" + dataKeyString + "|" + stringWriter.ToString();
        }
        
        protected virtual string GetCallbackScript(IButtonControl buttonControl, string argument) {
            if (DetermineRenderClientScript()) {
                if (!String.IsNullOrEmpty(argument)) {
                    if (Page != null) {
                        Page.ClientScript.RegisterForEventValidation(UniqueID, argument);
                    }

                    string clientCallbackReference = "javascript:__dv" + ClientID + ".callback";
                    return clientCallbackReference + "(" + argument + "); return false;";
                }
            }
            return null;
        }
        
        private void HandleCancel() {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            DetailsViewModeEventArgs e = new DetailsViewModeEventArgs(DefaultMode, true);
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
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            if (isBoundToDataSourceControl) {
                view = GetData();
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.View_DataSourceReturnedNullView, ID));
                }
            }

            DetailsViewDeleteEventArgs e = new DetailsViewDeleteEventArgs(pageIndex);


            ExtractRowValues(e.Values, true/*includeReadOnlyFields*/, false/*includePrimaryKey*/);
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
            DetailsViewDeletedEventArgs dea = new DetailsViewDeletedEventArgs(affectedRows, ex);
            dea.SetKeys(_deleteKeys);
            dea.SetValues(_deleteValues);

            OnItemDeleted(dea);

            _deleteKeys = null;
            _deleteValues = null;

            if (ex != null && !dea.ExceptionHandled) {
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

            DetailsViewModeEventArgs e = new DetailsViewModeEventArgs(DetailsViewMode.Edit, false);
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

            DetailsViewCommandEventArgs dce = e as DetailsViewCommandEventArgs;

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
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataControlCommands.NewCommandName)) {
                    HandleNew();
                }
                else {
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
                return false;
            }

            if (!view.CanExecute(commandName)) {
                return false;
            }
            
            OrderedDictionary values = new OrderedDictionary();
            OrderedDictionary keys = new OrderedDictionary();

            ExtractRowValues(values, true /*includeReadOnlyFields*/, false /*includePrimaryKey*/);
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

            if (Mode != DetailsViewMode.Insert) {
                throw new HttpException(SR.GetString(SR.DetailsViewFormView_ControlMustBeInInsertMode, "DetailsView", ID));
            }

            DataSourceView view = null;
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            if (isBoundToDataSourceControl) {
                view = GetData();
                if (view == null) {
                    throw new HttpException(SR.GetString(SR.View_DataSourceReturnedNullView, ID));
                }
            }

            DetailsViewInsertEventArgs e = new DetailsViewInsertEventArgs(commandArg);


            ExtractRowValues(e.Values, false/*includeReadOnlyFields*/, true/*includePrimaryKey*/);
            

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
            DetailsViewInsertedEventArgs dea = new DetailsViewInsertedEventArgs(affectedRows, ex);
            dea.SetValues(_insertValues);
            OnItemInserted(dea);

            _insertValues = null;
            if (ex != null && !dea.ExceptionHandled) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
                dea.KeepInInsertMode = true;
            }

            if (IsUsingModelBinders && !Page.ModelState.IsValid) {
                dea.KeepInInsertMode = true;
            }

            if (!dea.KeepInInsertMode) {
                DetailsViewModeEventArgs eMode = new DetailsViewModeEventArgs(DefaultMode, false);
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
            DetailsViewModeEventArgs e = new DetailsViewModeEventArgs(DetailsViewMode.Insert, false);
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

            DetailsViewPageEventArgs e = new DetailsViewPageEventArgs(newPage);
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
                    // DevDiv Bugs 188830: Don't clear key table if page is out of range, since control won't be rebound.
                    _keyTable = null;
                    _pageIndex = e.NewPageIndex;
                }
                else {
                    return;
                }
            }

            OnPageIndexChanged(EventArgs.Empty);
            RequiresDataBinding = true;
        }

        private void HandleUpdate(string commandArg, bool causesValidation) {
            if (causesValidation && Page != null && !Page.IsValid) {
                return;
            }

            if (Mode != DetailsViewMode.Edit) {
                throw new HttpException(SR.GetString(SR.DetailsViewFormView_ControlMustBeInEditMode, "DetailsView", ID));
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

            DetailsViewUpdateEventArgs e = new DetailsViewUpdateEventArgs(commandArg);


            foreach (DictionaryEntry entry in BoundFieldValues) {
                e.OldValues.Add(entry.Key, entry.Value);
            }

            ExtractRowValues(e.NewValues, false/*includeReadOnlyFields*/, true/*includePrimaryKey*/);
            foreach (DictionaryEntry entry in DataKey.Values) {
                e.Keys.Add(entry.Key, entry.Value);
            }

            

            OnItemUpdating(e);

            if (e.Cancel) {
                return;
            }

            if (isBoundToDataSourceControl) {
                _updateKeys = e.Keys;
                _updateNewValues = e.NewValues;
                _updateOldValues = e.OldValues;

                view.Update(e.Keys, e.NewValues, e.OldValues, HandleUpdateCallback);
            }
        }

        private bool HandleUpdateCallback(int affectedRows, Exception ex) {
            DetailsViewUpdatedEventArgs dea = new DetailsViewUpdatedEventArgs(affectedRows, ex);
            dea.SetOldValues(_updateOldValues);
            dea.SetNewValues(_updateNewValues);
            dea.SetKeys(_updateKeys);

            OnItemUpdated(dea);

            _updateKeys = null;
            _updateOldValues = null;
            _updateNewValues = null;

            if (ex != null && !dea.ExceptionHandled) {
                // If there is no validator in the validation group that could make sense
                // of the error, return false to proceed with standard exception handling.
                // But if there is one, we want to let it display its error instead of throwing.
                if (PageIsValidAfterModelException()) {
                    return false;
                }
                dea.KeepInEditMode = true;
            }

            if (IsUsingModelBinders && !Page.ModelState.IsValid) {
                dea.KeepInEditMode = true;
            }

            if (!dea.KeepInEditMode) {
                DetailsViewModeEventArgs eMode = new DetailsViewModeEventArgs(DefaultMode, false);
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
        /// Creates a DetailsViewRow that contains the paging UI.
        /// The paging UI is a navigation bar that is a built into a single TableCell that
        /// spans across all fields of the DetailsView.
        /// </para>
        /// </devdoc>
        protected virtual void InitializePager(DetailsViewRow row, PagedDataSource pagedDataSource) {
            TableCell cell = new TableCell();

            PagerSettings pagerSettings = PagerSettings;

            if (_pagerTemplate != null) {
                _pagerTemplate.InstantiateIn(cell);
            }
            else {
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
        protected virtual void InitializeRow(DetailsViewRow row, DataControlField field) {
            TableCellCollection cells = row.Cells;
            DataControlFieldCell contentCell = new DataControlFieldCell(field);
            ITemplate contentTemplate = null;
            int itemIndex = DataItemIndex;
            DataControlRowState rowState = row.RowState;

            switch (row.RowType) {
                case DataControlRowType.DataRow:
                    if (field.ShowHeader) {
                        DataControlFieldCell headerTextCell = new DataControlFieldCell(field);
                        field.InitializeCell(headerTextCell, DataControlCellType.Header, rowState, itemIndex);
                        cells.Add(headerTextCell);
                    }
                    else {
                        contentCell.ColumnSpan = 2;
                    }
                    field.InitializeCell(contentCell, DataControlCellType.DataCell, rowState, itemIndex);
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
        /// Determines if the specified data type can be bound to.
        /// Note : Staring from 4.5, This method is no more being used as a criteria for generating rows when AutoGenerateRows=true.
        /// This could become obsolete in future versions.
        /// </summary>
        public virtual bool IsBindableType(Type type) {
            return DataBoundControlHelper.IsBindableType(type, enableEnums: RenderingCompatibility >= VersionUtil.Framework45);
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
            _defaultMode = DetailsViewMode.ReadOnly;
            _dataKeyNames = new string[0];
            _pageCount = 0;

            object[] state = savedState as object[];
            
            if (state != null) {
                base.LoadControlState(state[0]);
                if (state[1] != null) {
                    _pageIndex = (int)state[1];
                }

                if (state[2] != null) {
                    _defaultMode = (DetailsViewMode)state[2];
                }

                // if Mode isn't saved, it should be restored to DefaultMode.  That will happen in Mode's getter,
                // since the persistance state hasn't been loaded yet.
                if (state[3] != null) {
                    Mode = (DetailsViewMode)state[3];
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
            }
            else {
                base.LoadControlState(null);
            }

        }

        private bool LoadHiddenFieldState(string pageIndex, string dataKey) {
            bool propertyChanged = false;
            int oldPageIndex = Int32.Parse(pageIndex, CultureInfo.InvariantCulture);

            if (PageIndex != oldPageIndex) {
                propertyChanged = true;

                // since we can't go into insert mode in a callback, oldPageIndex should never be -1 and different from PageIndex
                Debug.Assert(oldPageIndex >= 0, "Page indeces are out of [....] from callback hidden field state");
                _pageIndex = oldPageIndex;
                
                string oldDataKeyString = dataKey;

                if (!String.IsNullOrEmpty(oldDataKeyString)) {
                    IStateFormatter2 formatter = Page.CreateStateFormatter();
                    ArrayList oldDataKeyState = formatter.Deserialize(oldDataKeyString, Purpose.WebForms_DetailsView_KeyTable) as ArrayList;
                    if (_keyTable != null) {
                        _keyTable.Clear();
                    }
                    OrderedDictionaryStateHelper.LoadViewState(KeyTable, oldDataKeyState);
                }
            }
            return propertyChanged;
        }


        /// <devdoc>
        /// <para>Loads a saved state of the <see cref='System.Web.UI.WebControls.DetailsView'/>.</para>
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
                    ((IStateManager)AlternatingRowStyle).LoadViewState(myState[5]);
                if (myState[6] != null)
                    ((IStateManager)CommandRowStyle).LoadViewState(myState[6]);
                if (myState[7] != null)
                    ((IStateManager)EditRowStyle).LoadViewState(myState[7]);
                if (myState[8] != null)
                    ((IStateManager)InsertRowStyle).LoadViewState(myState[8]);
                if (myState[9] != null)
                    ((IStateManager)FieldHeaderStyle).LoadViewState(myState[9]);
                if (myState[10] != null)
                    ((IStateManager)Fields).LoadViewState(myState[10]);
                if (myState[11] != null)
                    OrderedDictionaryStateHelper.LoadViewState((OrderedDictionary)BoundFieldValues, (ArrayList)myState[11]);
                if (myState[12] != null)
                    ((IStateManager)PagerSettings).LoadViewState(myState[12]);
                if (myState[13] != null)
                    ((IStateManager)ControlStyle).LoadViewState(myState[13]);
                if (myState[14] != null) {
                    Debug.Assert(RowsGeneratorInternal is IStateManager);
                    ((IStateManager)RowsGeneratorInternal).LoadViewState(myState[14]);
                }
            }
            else {
                base.LoadViewState(null);
            }
        }


        /// <devdoc>
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            bool causesValidation = false;
            string validationGroup = String.Empty;
            
            DetailsViewCommandEventArgs dvcea = e as DetailsViewCommandEventArgs;
            if (dvcea != null) {
                IButtonControl button = dvcea.CommandSource as IButtonControl;
                if (button != null) {
                    causesValidation = button.CausesValidation;
                    validationGroup = button.ValidationGroup;
                }
            }
            return HandleEvent(e, causesValidation, validationGroup);
        }

        protected override void OnDataSourceViewChanged(object sender, EventArgs e) {
            _keyTable = null;
            base.OnDataSourceViewChanged(sender, e);
        }

        private void OnFieldsChanged(object sender, EventArgs e) {
            if (Initialized) {
                RequiresDataBinding = true;
            }
        }


        /// <devdoc>
        /// DetailsView initialization.
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (Page != null) {
                if (DataKeyNames.Length > 0 && !AutoGenerateRows) {
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
        protected virtual void OnItemCommand(DetailsViewCommandEventArgs e) {
            DetailsViewCommandEventHandler handler = (DetailsViewCommandEventHandler)Events[EventItemCommand];
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
        protected virtual void OnItemDeleted(DetailsViewDeletedEventArgs e) {
            DetailsViewDeletedEventHandler handler = (DetailsViewDeletedEventHandler)Events[EventItemDeleted];
            if (handler != null) handler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Delete'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemDeleting(DetailsViewDeleteEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            DetailsViewDeleteEventHandler handler = (DetailsViewDeleteEventHandler)Events[EventItemDeleting];
            if (handler != null) {
                handler(this, e);
            } else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.DetailsView_UnhandledEvent, ID, "ItemDeleting"));
                }
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ItemInserted '/>event.</para>
        /// </devdoc>
        protected virtual void OnItemInserted(DetailsViewInsertedEventArgs e) {
            DetailsViewInsertedEventHandler handler = (DetailsViewInsertedEventHandler)Events[EventItemInserted];
            if (handler != null) handler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Insert'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemInserting(DetailsViewInsertEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            DetailsViewInsertEventHandler handler = (DetailsViewInsertEventHandler)Events[EventItemInserting];
            if (handler != null) {
                handler(this, e);
            } else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.DetailsView_UnhandledEvent, ID, "ItemInserting"));
                }
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ItemUpdated '/>event.</para>
        /// </devdoc>
        protected virtual void OnItemUpdated(DetailsViewUpdatedEventArgs e) {
            DetailsViewUpdatedEventHandler handler = (DetailsViewUpdatedEventHandler)Events[EventItemUpdated];
            if (handler != null) handler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='Update'/> event.</para>
        /// </devdoc>
        protected virtual void OnItemUpdating(DetailsViewUpdateEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            DetailsViewUpdateEventHandler handler = (DetailsViewUpdateEventHandler)Events[EventItemUpdating];
            if (handler != null) {
                handler(this, e);
            } else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.DetailsView_UnhandledEvent, ID, "ItemUpdating"));
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
        protected virtual void OnModeChanging(DetailsViewModeEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            DetailsViewModeEventHandler handler = (DetailsViewModeEventHandler)Events[EventModeChanging];
            if (handler != null) {
                handler(this, e);
            } else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.DetailsView_UnhandledEvent, ID, "ModeChanging"));
                }
            }
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
        protected virtual void OnPageIndexChanging(DetailsViewPageEventArgs e) {
            bool isBoundToDataSourceControl = IsDataBindingAutomatic;

            DetailsViewPageEventHandler handler = (DetailsViewPageEventHandler)Events[EventPageIndexChanging];
            if (handler != null) {
                handler(this, e);
            } else {
                if (isBoundToDataSourceControl == false && e.Cancel == false) {
                    throw new HttpException(SR.GetString(SR.DetailsView_UnhandledEvent, ID, "PageIndexChanging"));
                }
            }
        }
        
        protected override void OnPagePreLoad(object sender, EventArgs e) {
            // Load hidden field state here to overwrite control state properties.  LoadViewState and LoadControlState
            // may not get called if there's no state in them.  We should allow the user to
            // set EnablePagingCallbacks in Page_Load, so don't request from DetermineRenderClientScript here.
            if (Page != null && !Page.IsCallback && Page.RequestValueCollection != null) {
                string hiddenFieldID = "__dv" + ClientID + "__hidden";
                string hiddenFieldState = Page.RequestValueCollection[hiddenFieldID];
                if (!String.IsNullOrEmpty(hiddenFieldState)) {
                    if (ParseHiddenFieldState(hiddenFieldState)) {
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

        private const string startupScriptFormat = @"
var {0} = new DetailsView();
{0}.stateField = document.getElementById('{1}');
{0}.panelElement = document.getElementById('{0}__div');
{0}.pageIndex = {3};
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
                string clientReference = "__dv" + ClientID;
                ClientScriptManager scriptOM = Page.ClientScript;

                scriptOM.RegisterClientScriptResource(typeof(DetailsView), "DetailsView.js");

                // The return value of GetCallbackEventReference looks like this:
                // "__doCallBack(controlname, script fx that returns arg, "DetailsView_OnCallback, context, errorMethod)"
                string doCallBackCall = scriptOM.GetCallbackEventReference(this, clientReference + ".getHiddenFieldContents(arg)", "DetailsView_OnCallback", clientReference);

                // Hidden field used to post content from DetailsView
                // back to the server
                string hiddenFieldID = clientReference + "__hidden";
                scriptOM.RegisterHiddenField(hiddenFieldID, String.Empty);

                string startupScript = String.Format(CultureInfo.InvariantCulture, startupScriptFormat, clientReference, hiddenFieldID, doCallBackCall, PageIndex);
                scriptOM.RegisterStartupScript(typeof(DetailsView), clientReference, startupScript, true);
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
            string[] arguments = state.Split(new char[] {'|'});
            if (arguments.Length == 2) {
                return LoadHiddenFieldState(arguments[0], arguments[1]);
            }
            return false;
        }


        protected internal override void PerformDataBinding(IEnumerable data) {
            base.PerformDataBinding(data);
            if (IsDataBindingAutomatic && Mode == DetailsViewMode.Edit && IsViewStateEnabled) {
                BoundFieldValues.Clear();
                ExtractRowValues(BoundFieldValues, true/*includeReadOnlyFields*/, false/*includePrimaryKey*/);
            }
        }

        /// <devdoc>
        /// </devdoc>
        protected internal virtual void PrepareControlHierarchy() {
            // The order of rows is autogenerated data rows, declared rows, then autogenerated command rows
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

                childTable.GridLines = GridLines.Both;
                childTable.CellSpacing = 0;
            }
            childTable.Caption = Caption;
            childTable.CaptionAlign = CaptionAlign;

            // the composite alternating item style, so we need to do just one
            // merge style on the actual item
            Style altRowStyle = new TableItemStyle();
            altRowStyle.CopyFrom(_rowStyle);
            if (_alternatingRowStyle != null) {
                altRowStyle = new TableItemStyle();
                altRowStyle.CopyFrom(_alternatingRowStyle);
            }

            Style compositeStyle;

            TableRowCollection rows = childTable.Rows;

            foreach (DetailsViewRow row in rows) {
                compositeStyle = new TableItemStyle();
                DataControlRowState rowState = row.RowState;
                DataControlRowType rowType = row.RowType;
                DataControlFieldCell headerFieldCell = row.Cells[0] as DataControlFieldCell;
                DataControlField field = null;

                if (headerFieldCell != null) {
                    field = headerFieldCell.ContainingField;
                }

                switch (rowType) {
                    case DataControlRowType.Header:
                        compositeStyle = _headerStyle;
                        break;

                    case DataControlRowType.Footer:
                        compositeStyle = _footerStyle;
                        break;

                    case DataControlRowType.DataRow:
                        compositeStyle.CopyFrom(_rowStyle);


                        if ((rowState & DataControlRowState.Alternate) != 0) {
                            compositeStyle.CopyFrom(altRowStyle);
                        }
                        if (field is ButtonFieldBase) {
                            compositeStyle.CopyFrom(_commandRowStyle);
                            break;
                        }
                        if ((rowState & DataControlRowState.Edit) != 0) {
                            compositeStyle.CopyFrom(_editRowStyle);
                        }
                        if ((rowState & DataControlRowState.Insert) != 0) {
                            if (_insertRowStyle != null) {
                                compositeStyle.CopyFrom(_insertRowStyle);
                            }
                            else {
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

                if (rowType == DataControlRowType.DataRow && field != null) {
                    if (!field.Visible ||
                        (Mode == DetailsViewMode.Insert &&  !field.InsertVisible)) {
                        row.Visible = false;
                    }
                    else {
                        int contentCellIndex = 0;
                        DataControlFieldCell contentFieldCell = null;

                        if (headerFieldCell != null && headerFieldCell.ContainingField.ShowHeader) {
                            headerFieldCell.MergeStyle(field.HeaderStyleInternal);
                            headerFieldCell.MergeStyle(_fieldHeaderStyle);
                            contentCellIndex = 1;
                        }
                        contentFieldCell = row.Cells[contentCellIndex] as DataControlFieldCell;
                        if (contentFieldCell != null) {
                            contentFieldCell.MergeStyle(field.ItemStyleInternal);
                        }

                        foreach (Control control in contentFieldCell.Controls) {
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

        protected virtual void RaiseCallbackEvent(string eventArgument) {
            string[] arguments = eventArgument.Split(new char[] {'|'});
            Debug.Assert((arguments != null && (arguments.Length == 4)), "An unexpected number of params came through");

            ValidateEvent(UniqueID, "\"" + arguments[0] + "|" + arguments[1] + "\"");

            LoadHiddenFieldState(arguments[2], arguments[3]);

            int pageNumber = Int32.Parse(arguments[0], CultureInfo.InvariantCulture);
            _pageIndex = pageNumber;

            DataBind();
        }

        protected virtual void RaisePostBackEvent(string eventArgument) {
            ValidateEvent(UniqueID, eventArgument);

            int separatorIndex = eventArgument.IndexOf('$');
            if (separatorIndex < 0) {
                return;
            }

            CommandEventArgs cea = new CommandEventArgs(eventArgument.Substring(0, separatorIndex), eventArgument.Substring(separatorIndex + 1));

            DetailsViewCommandEventArgs dvcea = new DetailsViewCommandEventArgs(this, cea);
            HandleEvent(dvcea, false, String.Empty);
		}


        /// <devdoc>
        /// <para>Displays the control on the client.</para>
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
                if (DetermineRenderClientScript()) {
                    string clientID = ClientID;
                    if (clientID == null) {
                        throw new HttpException(SR.GetString(SR.DetailsView_MustBeParented));
                    }
                    else {
                        StringBuilder clientPanelNameBuilder = new StringBuilder("__dv", 9 + clientID.Length);
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
            // LoadControlState won't get called if SaveControlState returned null.  We need to restore
            // values that are defaults but different from declarative property sets.
            if (baseState != null ||
                _pageIndex != 0 || 
                _mode != _defaultMode || 
                _defaultMode != DetailsViewMode.ReadOnly || 
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
                if (_defaultMode != DetailsViewMode.ReadOnly) {
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
        /// <para>Saves the current state of the <see cref='System.Web.UI.WebControls.DetailsView'/>.</para>
        /// </devdoc>
        protected override object SaveViewState() {
            object baseState = base.SaveViewState();
            object pagerStyleState = (_pagerStyle != null) ? ((IStateManager)_pagerStyle).SaveViewState() : null;
            object headerStyleState = (_headerStyle != null) ? ((IStateManager)_headerStyle).SaveViewState() : null;
            object footerStyleState = (_footerStyle != null) ? ((IStateManager)_footerStyle).SaveViewState() : null;
            object rowStyleState = (_rowStyle != null) ? ((IStateManager)_rowStyle).SaveViewState() : null;
            object alternatingRowStyleState = (_alternatingRowStyle != null) ? ((IStateManager)_alternatingRowStyle).SaveViewState() : null;
            object commandRowStyleState = (_commandRowStyle != null) ? ((IStateManager)_commandRowStyle).SaveViewState() : null;
            object editRowStyleState = (_editRowStyle != null) ? ((IStateManager)_editRowStyle).SaveViewState() : null;
            object insertRowStyleState = (_insertRowStyle != null) ? ((IStateManager)_insertRowStyle).SaveViewState() : null;
            object fieldHeaderStyleState = (_fieldHeaderStyle != null) ? ((IStateManager)_fieldHeaderStyle).SaveViewState() : null;
            object fieldsState = (_fieldCollection != null) ? ((IStateManager)_fieldCollection).SaveViewState() : null;
            object boundFieldValuesState = (_boundFieldValues != null) ? OrderedDictionaryStateHelper.SaveViewState(_boundFieldValues) : null;
            object pagerSettingsState = (_pagerSettings != null) ? ((IStateManager)_pagerSettings).SaveViewState() : null;
            object controlState = ControlStyleCreated ? ((IStateManager)ControlStyle).SaveViewState() : null;

            object[] myState = new object[15];
            myState[0] = baseState;
            myState[1] = pagerStyleState;
            myState[2] = headerStyleState;
            myState[3] = footerStyleState;
            myState[4] = rowStyleState;
            myState[5] = alternatingRowStyleState;
            myState[6] = commandRowStyleState;
            myState[7] = editRowStyleState;
            myState[8] = insertRowStyleState;
            myState[9] = fieldHeaderStyleState;
            myState[10] = fieldsState;
            myState[11] = boundFieldValuesState;
            myState[12] = pagerSettingsState;
            myState[13] = controlState;
            myState[14] = RowsGeneratorInternal is IStateManager ? ((IStateManager)RowsGeneratorInternal).SaveViewState() : null;

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
            if (_alternatingRowStyle != null)
                ((IStateManager)_alternatingRowStyle).TrackViewState();
            if (_commandRowStyle != null)
                ((IStateManager)_commandRowStyle).TrackViewState();
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

        #region IPostBackEventHandler implementation
        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion

        #region IPostBackContainer implementation
        PostBackOptions IPostBackContainer.GetPostBackOptions(IButtonControl buttonControl) {
            if (buttonControl == null) {
                throw new ArgumentNullException("buttonControl");
            }

            if (buttonControl.CausesValidation) {
                throw new InvalidOperationException(SR.GetString(SR.CannotUseParentPostBackWhenValidating, this.GetType().Name, ID));
            }

            PostBackOptions options = new PostBackOptions(this, (buttonControl.CommandName + "$" + buttonControl.CommandArgument));
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
                    case DetailsViewMode.Edit:
                        return DataBoundControlMode.Edit;
                    case DetailsViewMode.Insert:
                        return DataBoundControlMode.Insert;
                    case DetailsViewMode.ReadOnly:
                        return DataBoundControlMode.ReadOnly;
                    default:
                        Debug.Fail("Shouldn't get here!");
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

        #region IFieldControl implementation

        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
            Justification = "The underlying implementation is not meant to be overridden.")]
        IAutoFieldGenerator IFieldControl.FieldsGenerator {
            get {
                return RowsGenerator;
            }
            set {
                RowsGenerator = value;
            }
        }

        #endregion
    }
}
