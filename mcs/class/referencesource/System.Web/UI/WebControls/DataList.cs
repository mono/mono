//------------------------------------------------------------------------------
// <copyright file="DataList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;
    
    /// <devdoc>
    ///    <para>
    ///       Creates
    ///       a control to display a data-bound list.
    ///    </para>
    /// </devdoc>
    [
    ControlValueProperty("SelectedValue"),
    Editor("System.Web.UI.Design.WebControls.DataListComponentEditor, " + AssemblyRef.SystemDesign, typeof(ComponentEditor)),
    Designer("System.Web.UI.Design.WebControls.DataListDesigner, " + AssemblyRef.SystemDesign)
    ]
    public class DataList : BaseDataList, INamingContainer, IRepeatInfoUser, IWizardSideBarListControl {

        private static readonly object EventItemCreated = new object();
        private static readonly object EventItemDataBound = new object();
        private static readonly object EventItemCommand = new object();
        private static readonly object EventEditCommand = new object();
        private static readonly object EventUpdateCommand = new object();
        private static readonly object EventCancelCommand = new object();
        private static readonly object EventDeleteCommand = new object();
        private static readonly object EventWizardListItemDataBound = new object();


        /// <devdoc>
        /// <para>Specifies the <see langword='Select'/> command. This field is constant.</para>
        /// </devdoc>
        public const string SelectCommandName = "Select";

        /// <devdoc>
        /// <para>Specifies the <see langword='Edit'/> command. This field is constant</para>
        /// </devdoc>
        public const string EditCommandName = "Edit";

        /// <devdoc>
        /// <para>Specifies the <see langword='Update'/> command. This field is constant</para>
        /// </devdoc>
        public const string UpdateCommandName = "Update";

        /// <devdoc>
        /// <para>Specifies the <see langword='Cancel'/> command. This field is constant</para>
        /// </devdoc>
        public const string CancelCommandName = "Cancel";

        /// <devdoc>
        /// <para>Specifies the <see langword='Delete'/> command. This field is constant</para>
        /// </devdoc>
        public const string DeleteCommandName = "Delete";

        private TableItemStyle itemStyle;
        private TableItemStyle alternatingItemStyle;
        private TableItemStyle selectedItemStyle;
        private TableItemStyle editItemStyle;
        private TableItemStyle separatorStyle;
        private TableItemStyle headerStyle;
        private TableItemStyle footerStyle;

        private ITemplate itemTemplate;
        private ITemplate alternatingItemTemplate;
        private ITemplate selectedItemTemplate;
        private ITemplate editItemTemplate;
        private ITemplate separatorTemplate;
        private ITemplate headerTemplate;
        private ITemplate footerTemplate;

        private bool extractTemplateRows;

        private ArrayList itemsArray;
        private DataListItemCollection itemsCollection;
        
        private int offset;
        private int visibleItemCount = -1;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.DataList'/> class.
        ///    </para>
        /// </devdoc>
        public DataList() {
            offset = 0;
            visibleItemCount = -1;
        }



        /// <devdoc>
        /// <para>Gets the style properties for alternating items in the <see cref='System.Web.UI.WebControls.DataList'/>. This 
        ///    property is read-only. </para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DataList_AlternatingItemStyle)
        ]
        public virtual TableItemStyle AlternatingItemStyle {
            get {
                if (alternatingItemStyle == null) {
                    alternatingItemStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)alternatingItemStyle).TrackViewState();
                }
                return alternatingItemStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for alternating items in the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DataListItem)),
        WebSysDescription(SR.DataList_AlternatingItemTemplate)
        ]
        public virtual ITemplate AlternatingItemTemplate {
            get {
                return alternatingItemTemplate;
            }
            set {
                alternatingItemTemplate = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the ordinal index of the item to be edited.</para>
        /// </devdoc>
        [
        WebCategory("Default"),
        DefaultValue(-1),
        WebSysDescription(SR.DataList_EditItemIndex)
        ]
        public virtual int EditItemIndex {
            get {
                object o = ViewState["EditItemIndex"];
                if (o != null) {
                    return (int)o;
                }
                return -1;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["EditItemIndex"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the style properties of the item to be edited.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DataList_EditItemStyle)
        ]
        public virtual TableItemStyle EditItemStyle {
            get {
                if (editItemStyle == null) {
                    editItemStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)editItemStyle).TrackViewState();
                }
                return editItemStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for an item set in edit mode within the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DataListItem)),
        WebSysDescription(SR.DataList_EditItemTemplate)
        ]
        public virtual ITemplate EditItemTemplate {
            get {
                return editItemTemplate;
            }
            set {
                editItemTemplate = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether to extract template rows.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(false),
        WebSysDescription(SR.DataList_ExtractTemplateRows)
        ]
        public virtual bool ExtractTemplateRows {
            get {
                object o = ViewState["ExtractTemplateRows"];
                if (o != null)
                    return(bool)o;
                return false;
            }
            set {
                ViewState["ExtractTemplateRows"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets the style properties of the footer item.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DataControls_FooterStyle)
        ]
        public virtual TableItemStyle FooterStyle {
            get {
                if (footerStyle == null) {
                    footerStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)footerStyle).TrackViewState();
                }
                return footerStyle;
            }
        }


        /// <devdoc>
        /// <para> Indicates the template to use for the footer in the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DataListItem)),
        WebSysDescription(SR.DataList_FooterTemplate)
        ]
        public virtual ITemplate FooterTemplate {
            get {
                return footerTemplate;
            }
            set {
                footerTemplate = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates a value that specifies the grid line style.</para>
        /// </devdoc>
        [
        DefaultValue(GridLines.None)
        ]
        public override GridLines GridLines {
            get {
                if (ControlStyleCreated == false) {
                    return GridLines.None;
                }
                return ((TableStyle)ControlStyle).GridLines;
            }
            set {
                base.GridLines = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets the style properties of the header item.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DataControls_HeaderStyle)
        ]
        public virtual TableItemStyle HeaderStyle {
            get {
                if (headerStyle == null) {
                    headerStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)headerStyle).TrackViewState();
                }
                return headerStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for the header in the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DataListItem)),
        WebSysDescription(SR.DataList_HeaderTemplate)
        ]
        public virtual ITemplate HeaderTemplate {
            get {
                return headerTemplate;
            }
            set {
                headerTemplate = value;
            }
        }


        /// <devdoc>
        /// <para>Gets a collection of <see cref='System.Web.UI.WebControls.DataListItem'/> objects representing the individual 
        ///    items within the control.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.DataList_Items)
        ]
        public virtual DataListItemCollection Items {
            get {
                if (itemsCollection == null) {
                    if (itemsArray == null) {
                        EnsureChildControls();
                    }
                    if (itemsArray == null) {
                        itemsArray = new ArrayList();
                    }
                    itemsCollection = new DataListItemCollection(itemsArray);
                }
                return itemsCollection;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the style properties of the individual items.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DataList_ItemStyle)
        ]
        public virtual TableItemStyle ItemStyle {
            get {
                if (itemStyle == null) {
                    itemStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)itemStyle).TrackViewState();
                }
                return itemStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for an item in the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DataListItem)),
        WebSysDescription(SR.DataList_ItemTemplate),
        SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")
        ]
        public virtual ITemplate ItemTemplate {
            get {
                return itemTemplate;
            }
            set {
                itemTemplate = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the number of columns to repeat.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(0),
        WebSysDescription(SR.DataList_RepeatColumns)
        ]
        public virtual int RepeatColumns {
            get {
                object o = ViewState["RepeatColumns"];
                if (o != null)
                    return(int)o;
                return 0;
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["RepeatColumns"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether the control is displayed vertically or horizontally.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(RepeatDirection.Vertical),
        WebSysDescription(SR.Item_RepeatDirection)
        ]
        public virtual RepeatDirection RepeatDirection {
            get {
                object o = ViewState["RepeatDirection"];
                if (o != null)
                    return(RepeatDirection)o;
                return RepeatDirection.Vertical;
            }
            set {
                if (value < RepeatDirection.Horizontal || value > RepeatDirection.Vertical) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["RepeatDirection"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value that indicates whether the control is displayed in table 
        ///       or flow layout.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(RepeatLayout.Table),
        WebSysDescription(SR.WebControl_RepeatLayout)
        ]
        public virtual RepeatLayout RepeatLayout {
            get {
                object o = ViewState["RepeatLayout"];
                if (o != null)
                    return(RepeatLayout)o;
                return RepeatLayout.Table;
            }
            set {
                if ((value == RepeatLayout.UnorderedList) || 
                    (value == RepeatLayout.OrderedList)) {
                        throw new ArgumentOutOfRangeException("value", SR.GetString(SR.DataList_LayoutNotSupported, value));
                }

                EnumerationRangeValidationUtil.ValidateRepeatLayout(value);
                ViewState["RepeatLayout"] = value;
            }
        }


        /// <devdoc>
        ///    <para> Indicates the index of
        ///       the currently selected item.</para>
        /// </devdoc>
        [
        Bindable(true),
        DefaultValue(-1),
        WebSysDescription(SR.WebControl_SelectedIndex),
        SuppressMessage("Microsoft.Security", "CA2119:SealMethodsThatSatisfyPrivateInterfaces",
            Justification = "Interface denotes existence of property, not used for security.")
        ]
        public virtual int SelectedIndex {
            get {
                object o = ViewState["SelectedIndex"];
                if (o != null) {
                    return (int)o;
                }
                return -1;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                int oldSelectedIndex = SelectedIndex;
                ViewState["SelectedIndex"] = value;

                if (itemsArray != null) {
                    DataListItem item;

                    if ((oldSelectedIndex != -1) && (itemsArray.Count > oldSelectedIndex)) {
                        item = (DataListItem)itemsArray[oldSelectedIndex];

                        if (item.ItemType != ListItemType.EditItem) {
                            ListItemType itemType = ListItemType.Item;
                            if (oldSelectedIndex % 2 != 0)
                                itemType = ListItemType.AlternatingItem;
                            item.SetItemType(itemType);
                        }
                    }
                    if ((value != -1) && (itemsArray.Count > value)) {
                        item = (DataListItem)itemsArray[value];
                        if (item.ItemType != ListItemType.EditItem)
                            item.SetItemType(ListItemType.SelectedItem);
                    }
                }
            }
        }


        /// <devdoc>
        /// <para>Gets the selected item in the <see cref='System.Web.UI.WebControls.DataGrid'/>.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.DataList_SelectedItem)
        ]
        public virtual DataListItem SelectedItem {
            get {
                int index = SelectedIndex;
                DataListItem item = null;

                if (index != -1) {
                    item = Items[index];
                }
                return item;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the style properties of the currently 
        ///       selected item.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DataList_SelectedItemStyle)
        ]
        public virtual TableItemStyle SelectedItemStyle {
            get {
                if (selectedItemStyle == null) {
                    selectedItemStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)selectedItemStyle).TrackViewState();
                }
                return selectedItemStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for the currently selected item in the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DataListItem)),
        WebSysDescription(SR.DataList_SelectedItemTemplate)
        ]
        public virtual ITemplate SelectedItemTemplate {
            get {
                return selectedItemTemplate;
            }
            set {
                selectedItemTemplate = value;
            }
        }


        [
        Browsable(false)
        ]
        public object SelectedValue {
            get {
                if (DataKeyField.Length == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.DataList_DataKeyFieldMustBeSpecified, ID));
                }
                DataKeyCollection keys = DataKeys;
                int selectedIndex = SelectedIndex;
                if (keys != null && selectedIndex < keys.Count && selectedIndex > -1) {
                    return keys[selectedIndex];
                }
                return null;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the style properties of the separator between each item in the 
        ///    <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        [
        WebCategory("Styles"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.DataList_SeparatorStyle)
        ]
        public virtual TableItemStyle SeparatorStyle {
            get {
                if (separatorStyle == null) {
                    separatorStyle = new TableItemStyle();
                    if (IsTrackingViewState)
                        ((IStateManager)separatorStyle).TrackViewState();
                }
                return separatorStyle;
            }
        }


        /// <devdoc>
        /// <para>Indicates the template to use for the separator in the <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(DataListItem)),
        WebSysDescription(SR.DataList_SeparatorTemplate)
        ]
        public virtual ITemplate SeparatorTemplate {
            get {
                return separatorTemplate;
            }
            set {
                separatorTemplate = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value that specifies whether the footer is displayed in the 
        ///    <see cref='System.Web.UI.WebControls.DataList'/>.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(true),
        WebSysDescription(SR.DataControls_ShowFooter)
        ]
        public virtual bool ShowFooter {
            get {
                object o = ViewState["ShowFooter"];
                if (o != null)
                    return(bool)o;
                return true;
            }
            set {
                ViewState["ShowFooter"] = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets a value that specifies whether the header is displayed in the<see cref='System.Web.UI.WebControls.DataGrid'/>.</para>
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
                    return(bool)o;
                return true;
            }
            set {
                ViewState["ShowHeader"] = value;
            }
        }

        protected override HtmlTextWriterTag TagKey {
            get {
                return RepeatLayout == RepeatLayout.Table? 
                    HtmlTextWriterTag.Table : HtmlTextWriterTag.Span;
            }
        }


        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.DataList'/> with a 
        /// <see langword='Command'/> property of <see langword='cancel'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataList_OnCancelCommand)
        ]
        public event DataListCommandEventHandler CancelCommand {
            add {
                Events.AddHandler(EventCancelCommand, value);
            }
            remove {
                Events.RemoveHandler(EventCancelCommand, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.DataList'/> with a 
        /// <see langword='Command'/> property of <see langword='delete'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataList_OnDeleteCommand)
        ]
        public event DataListCommandEventHandler DeleteCommand {
            add {
                Events.AddHandler(EventDeleteCommand, value);
            }
            remove {
                Events.RemoveHandler(EventDeleteCommand, value);
            }
        }



        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.DataList'/> with a 
        /// <see langword='Command'/> property of <see langword='edit'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataList_OnEditCommand)
        ]
        public event DataListCommandEventHandler EditCommand {
            add {
                Events.AddHandler(EventEditCommand, value);
            }
            remove {
                Events.RemoveHandler(EventEditCommand, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.DataList'/> not covered by 
        /// <see langword='edit'/>, <see langword='cancel'/>, <see langword='delete'/> or 
        /// <see langword='update'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataList_OnItemCommand)
        ]
        public event DataListCommandEventHandler ItemCommand {
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
        WebCategory("Behavior"),
        WebSysDescription(SR.DataControls_OnItemCreated)
        ]
        public event DataListItemEventHandler ItemCreated {
            add {
                Events.AddHandler(EventItemCreated, value);
            }
            remove {
                Events.RemoveHandler(EventItemCreated, value);
            }
        }


        /// <devdoc>
        ///    <para>Occurs when an item is data bound to the control.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        WebSysDescription(SR.DataControls_OnItemDataBound)
        ]
        public event DataListItemEventHandler ItemDataBound {
            add {
                Events.AddHandler(EventItemDataBound, value);
            }
            remove {
                Events.RemoveHandler(EventItemDataBound, value);
            }
        }


        /// <devdoc>
        /// <para>Occurs when a control bubbles an event to the <see cref='System.Web.UI.WebControls.DataList'/> with a 
        /// <see langword='Command'/> property of <see langword='update'/>.</para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.DataList_OnUpdateCommand)
        ]
        public event DataListCommandEventHandler UpdateCommand {
            add {
                Events.AddHandler(EventUpdateCommand, value);
            }
            remove {
                Events.RemoveHandler(EventUpdateCommand, value);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void CreateControlHierarchy(bool useDataSource) {
            IEnumerable dataSource = null;
            int count = -1;
            ArrayList keysArray = DataKeysArray;

            // cache this, so we don't need to go to the statebag each time
            extractTemplateRows = this.ExtractTemplateRows;

            if (itemsArray != null) {
                itemsArray.Clear();
            }
            else {
                itemsArray = new ArrayList();
            }

            if (useDataSource == false) {
                // ViewState must have a non-null value for ItemCount because we check for
                // this in CreateChildControls
                count = (int)ViewState[BaseDataList.ItemCountViewStateKey];
                if (count != -1) {
                    dataSource = new DummyDataSource(count);
                    itemsArray.Capacity = count;
                }
            }
            else {
                keysArray.Clear();
                dataSource = GetData();

                ICollection collection = dataSource as ICollection;
                if (collection != null) {
                    keysArray.Capacity = collection.Count;
                    itemsArray.Capacity = collection.Count;
                }
            }

            if (dataSource != null) {
                ControlCollection controls = Controls;
                DataListItem item;
                ListItemType itemType;
                int index = 0;

                bool hasSeparators = (separatorTemplate != null);
                int editItemIndex = EditItemIndex;
                int selectedItemIndex = SelectedIndex;
                string keyField = DataKeyField;
                bool storeKeys = (useDataSource && (keyField.Length != 0));

                count = 0;

                if (headerTemplate != null) {
                    CreateItem(-1, ListItemType.Header, useDataSource, null);
                }

                foreach (object dataItem in dataSource) {
                    if (storeKeys) {
                        object keyValue = DataBinder.GetPropertyValue(dataItem, keyField);
                        keysArray.Add(keyValue);
                    }

                    itemType = ListItemType.Item;
                    if (index == editItemIndex) {
                        itemType = ListItemType.EditItem;
                    }
                    else if (index == selectedItemIndex) {
                        itemType = ListItemType.SelectedItem;
                    }
                    else if (index % 2 != 0) {
                        itemType = ListItemType.AlternatingItem;
                    }

                    item = CreateItem(index, itemType, useDataSource, dataItem);
                    itemsArray.Add(item);

                    if (hasSeparators) {
                        CreateItem(index, ListItemType.Separator, useDataSource, null);
                    }

                    count++;
                    index++;
                }

                if (footerTemplate != null) {
                    CreateItem(-1, ListItemType.Footer, useDataSource, null);
                }
            }

            if (useDataSource) {
                // save the number of items contained in the DataList for use in round-trips
                ViewState[BaseDataList.ItemCountViewStateKey] = ((dataSource != null) ? count : -1);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override Style CreateControlStyle() {
            TableStyle controlStyle = new TableStyle();

            // initialize defaults that are different from TableStyle
            controlStyle.CellSpacing = 0;

            return controlStyle;
        }


        /// <devdoc>
        /// </devdoc>
        private DataListItem CreateItem(int itemIndex, ListItemType itemType, bool dataBind, object dataItem) {
            DataListItem item = CreateItem(itemIndex, itemType);
            DataListItemEventArgs e = new DataListItemEventArgs(item);

            InitializeItem(item);
            if (dataBind) {
                item.DataItem = dataItem;
            }
            OnItemCreated(e);
            Controls.Add(item);

            if (dataBind) {
                item.DataBind();
                OnItemDataBound(e);

                item.DataItem = null;
            }

            return item;
        }


        /// <devdoc>
        /// </devdoc>
        protected virtual DataListItem CreateItem(int itemIndex, ListItemType itemType) {
            return new DataListItem(itemIndex, itemType);
        }


        private DataListItem GetItem(ListItemType itemType, int repeatIndex) {
            DataListItem item = null;

            switch (itemType) {
                case ListItemType.Header:
                    Debug.Assert(((IRepeatInfoUser)this).HasHeader);
                    item = (DataListItem)Controls[0];
                    break;
                case ListItemType.Footer:
                    Debug.Assert(((IRepeatInfoUser)this).HasFooter);
                    item = (DataListItem)Controls[Controls.Count - 1];
                    break;
                case ListItemType.Separator:
                    Debug.Assert(((IRepeatInfoUser)this).HasSeparators);
                    {
                        int controlIndex = repeatIndex * 2 + 1;
                        if (headerTemplate != null) {
                            controlIndex++;
                        }
                        item = (DataListItem)Controls[controlIndex];
                    }
                    break;
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                case ListItemType.EditItem:
                    item = (DataListItem)itemsArray[repeatIndex];
                    break;
            }

            return item;
        }


        /// <devdoc>
        /// </devdoc>
        protected virtual void InitializeItem(DataListItem item) {
            ITemplate contentTemplate = itemTemplate;

            switch (item.ItemType) {
                case ListItemType.Header:
                    contentTemplate = headerTemplate;
                    break;

                case ListItemType.Footer:
                    contentTemplate = footerTemplate;
                    break;

                case ListItemType.AlternatingItem:
                    if (alternatingItemTemplate != null) {
                        contentTemplate = alternatingItemTemplate;
                    }
                    break;

                case ListItemType.SelectedItem:
                    if (selectedItemTemplate != null) {
                        contentTemplate = selectedItemTemplate;
                    }
                    else {
                        if (item.ItemIndex % 2 != 0)
                            goto case ListItemType.AlternatingItem;
                    }
                    break;

                case ListItemType.EditItem:
                    if (editItemTemplate != null) {
                        contentTemplate = editItemTemplate;
                    }
                    else {
                        if (item.ItemIndex == SelectedIndex)
                            goto case ListItemType.SelectedItem;
                        else if (item.ItemIndex % 2 != 0)
                            goto case ListItemType.AlternatingItem;
                    }
                    break;

                case ListItemType.Separator:
                    contentTemplate = separatorTemplate;
                    break;
            }

            if (contentTemplate != null)
                contentTemplate.InstantiateIn(item);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                object[] myState = (object[])savedState;

                if (myState[0] != null)
                    base.LoadViewState(myState[0]);
                if (myState[1] != null)
                    ((IStateManager)ItemStyle).LoadViewState(myState[1]);
                if (myState[2] != null)
                    ((IStateManager)SelectedItemStyle).LoadViewState(myState[2]);
                if (myState[3] != null)
                    ((IStateManager)AlternatingItemStyle).LoadViewState(myState[3]);
                if (myState[4] != null)
                    ((IStateManager)EditItemStyle).LoadViewState(myState[4]);
                if (myState[5] != null)
                    ((IStateManager)SeparatorStyle).LoadViewState(myState[5]);
                if (myState[6] != null)
                    ((IStateManager)HeaderStyle).LoadViewState(myState[6]);
                if (myState[7] != null)
                    ((IStateManager)FooterStyle).LoadViewState(myState[7]);
                if (myState[8] != null)
                    ((IStateManager)ControlStyle).LoadViewState(myState[8]);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            bool handled = false;

            if (e is DataListCommandEventArgs) {
                DataListCommandEventArgs dce = (DataListCommandEventArgs)e;

                OnItemCommand(dce);
                handled = true;

                string command = dce.CommandName;

                if (StringUtil.EqualsIgnoreCase(command, DataList.SelectCommandName)) {
                    SelectedIndex = dce.Item.ItemIndex;
                    OnSelectedIndexChanged(EventArgs.Empty);
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataList.EditCommandName)) {
                    OnEditCommand(dce);
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataList.DeleteCommandName)) {
                    OnDeleteCommand(dce);
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataList.UpdateCommandName)) {
                    OnUpdateCommand(dce);
                }
                else if (StringUtil.EqualsIgnoreCase(command, DataList.CancelCommandName)) {
                    OnCancelCommand(dce);
                }
            }

            return handled;
        }


        /// <devdoc>
        /// <para>Raises the <see langword='CancelCommand '/>event.</para>
        /// </devdoc>
        protected virtual void OnCancelCommand(DataListCommandEventArgs e) {
            DataListCommandEventHandler onCancelCommandHandler = (DataListCommandEventHandler)Events[EventCancelCommand];
            if (onCancelCommandHandler != null) onCancelCommandHandler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='DeleteCommand '/>event.</para>
        /// </devdoc>
        protected virtual void OnDeleteCommand(DataListCommandEventArgs e) {
            DataListCommandEventHandler onDeleteCommandHandler = (DataListCommandEventHandler)Events[EventDeleteCommand];
            if (onDeleteCommandHandler != null) onDeleteCommandHandler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='EditCommand '/>event.</para>
        /// </devdoc>
        protected virtual void OnEditCommand(DataListCommandEventArgs e) {
            DataListCommandEventHandler onEditCommandHandler = (DataListCommandEventHandler)Events[EventEditCommand];
            if (onEditCommandHandler != null) onEditCommandHandler(this, e);
        }


        /// <devdoc>
        /// DataList initialization.
        /// </devdoc>
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);
            if (Page != null && DataKeyField.Length > 0) {
                Page.RegisterRequiresViewStateEncryption();
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ItemCommand '/>event.</para>
        /// </devdoc>
        protected virtual void OnItemCommand(DataListCommandEventArgs e) {
            DataListCommandEventHandler onItemCommandHandler = (DataListCommandEventHandler)Events[EventItemCommand];
            if (onItemCommandHandler != null) onItemCommandHandler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ItemCreated '/>event.</para>
        /// </devdoc>
        protected virtual void OnItemCreated(DataListItemEventArgs e) {
            DataListItemEventHandler onItemCreatedHandler = (DataListItemEventHandler)Events[EventItemCreated];
            if (onItemCreatedHandler != null) onItemCreatedHandler(this, e);
        }


        /// <devdoc>
        /// <para>Raises the <see langword='ItemDataBound '/>event.</para>
        /// </devdoc>
        protected virtual void OnItemDataBound(DataListItemEventArgs e) {
            DataListItemEventHandler onItemDataBoundHandler = (DataListItemEventHandler)Events[EventItemDataBound];
            if (onItemDataBoundHandler != null) onItemDataBoundHandler(this, e);

            // EventWizardListItemDataBound is a key for an internal event declared on IWizardSideBarListControl, which is
            // an interface that is meant to provide a facade to make ListView and DataList look the same. This handler
            // is meant to abstract away the differences between each controls ItemDataBound events.
            var onWizardListItemDataBoundHandler = (EventHandler<WizardSideBarListControlItemEventArgs>)Events[EventWizardListItemDataBound];
            if (onWizardListItemDataBoundHandler != null) {
                var item = e.Item;
                var wizardListEventArgs = new WizardSideBarListControlItemEventArgs(new WizardSideBarListControlItem(item.DataItem, item.ItemType, item.ItemIndex, item));
                onWizardListItemDataBoundHandler(this, wizardListEventArgs);
            }
        }


        /// <devdoc>
        /// <para>Raises the <see langword='UpdateCommand '/>event.</para>
        /// </devdoc>
        protected virtual void OnUpdateCommand(DataListCommandEventArgs e) {
            DataListCommandEventHandler onUpdateCommandHandler = (DataListCommandEventHandler)Events[EventUpdateCommand];
            if (onUpdateCommandHandler != null) onUpdateCommandHandler(this, e);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void PrepareControlHierarchy() {
            ControlCollection controls = Controls;
            int controlCount = controls.Count;

            if (controlCount == 0)
                return;

            // the composite alternating item style, so we need to do just one
            // merge style on the actual item
            Style altItemStyle = null;
            if (alternatingItemStyle != null) {
                altItemStyle = new TableItemStyle();
                altItemStyle.CopyFrom(itemStyle);
                altItemStyle.CopyFrom(alternatingItemStyle);
            }
            else {
                altItemStyle = itemStyle;
            }

            Style compositeStyle;

            for (int i = 0; i < controlCount; i++) {
                DataListItem item = (DataListItem)controls[i];
                compositeStyle = null;

                switch (item.ItemType) {
                    case ListItemType.Header:
                        if (ShowHeader)
                            compositeStyle = headerStyle;
                        break;

                    case ListItemType.Footer:
                        if (ShowFooter)
                            compositeStyle = footerStyle;
                        break;

                    case ListItemType.Separator:
                        compositeStyle = separatorStyle;
                        break;

                    case ListItemType.Item:
                        compositeStyle = itemStyle;
                        break;

                    case ListItemType.AlternatingItem:
                        compositeStyle = altItemStyle;
                        break;

                    case ListItemType.SelectedItem:
                        // When creating the control hierarchy we first check if the
                        // item is in edit mode, so we know this item cannot be in edit
                        // mode. The only special characteristic of this item is that
                        // it is selected.
                        {
                            compositeStyle = new TableItemStyle();

                            if (item.ItemIndex % 2 != 0)
                                compositeStyle.CopyFrom(altItemStyle);
                            else
                                compositeStyle.CopyFrom(itemStyle);
                            compositeStyle.CopyFrom(selectedItemStyle);
                        }
                        break;

                    case ListItemType.EditItem:
                        // When creating the control hierarchy, we first check if the
                        // item is in edit mode. So an item may be selected too, and
                        // so both editItemStyle (more specific) and selectedItemStyle
                        // are applied.
                        {
                            compositeStyle = new TableItemStyle();

                            if (item.ItemIndex % 2 != 0)
                                compositeStyle.CopyFrom(altItemStyle);
                            else
                                compositeStyle.CopyFrom(itemStyle);
                            if (item.ItemIndex == SelectedIndex)
                                compositeStyle.CopyFrom(selectedItemStyle);
                            compositeStyle.CopyFrom(editItemStyle);
                        }
                        break;
                }

                if (compositeStyle != null) {
                    // use the cached value of ExtractTemplateRows as it was at the time of
                    // control creation, so we don't do the wrong thing even if the
                    // user happened to change the property

                    if (extractTemplateRows == false) {
                        item.MergeStyle(compositeStyle);
                    }
                    else {
                        // apply the style on the TRs
                        IEnumerator controlEnum = item.Controls.GetEnumerator();

                        while (controlEnum.MoveNext()) {
                            Control c = (Control)controlEnum.Current;
                            if (c is Table) {
                                IEnumerator rowEnum = ((Table)c).Rows.GetEnumerator();

                                while (rowEnum.MoveNext()) {
                                    // 


                                    ((TableRow)rowEnum.Current).MergeStyle(compositeStyle);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            if (Controls.Count == 0)
                return;

            RepeatInfo repeatInfo = new RepeatInfo();
            Table outerTable = null;

            // NOTE: This will end up creating the ControlStyle... Ideally we would
            //       not create the style just for rendering, but turns out our default
            //       style isn't empty, and does have an effect on rendering, and must
            //       therefore always be created
            Style style = ControlStyle;

            if (extractTemplateRows) {
                // The table tags in the templates are stripped out and only the
                // <tr>'s and <td>'s are assumed to come from the template itself.
                // This is equivalent to a flow layout of <tr>'s in a single
                // vertical column.

                repeatInfo.RepeatDirection = RepeatDirection.Vertical;
                repeatInfo.RepeatLayout = RepeatLayout.Flow;
                repeatInfo.RepeatColumns = 1;

                repeatInfo.OuterTableImplied = true;
                outerTable = new Table();

                // use ClientID (and not ID) since we want to render the fully qualified
                // ID even though the control will not be parented to the control hierarchy
                outerTable.ID = ClientID;

                outerTable.CopyBaseAttributes(this);
                outerTable.Caption = Caption;
                outerTable.CaptionAlign = CaptionAlign;
                outerTable.ApplyStyle(style);
                outerTable.RenderBeginTag(writer);
            }
            else {
                repeatInfo.RepeatDirection = RepeatDirection;
                repeatInfo.RepeatLayout = RepeatLayout;
                repeatInfo.RepeatColumns = RepeatColumns;
                if (repeatInfo.RepeatLayout == RepeatLayout.Table) {
                    repeatInfo.Caption = Caption;
                    repeatInfo.CaptionAlign = CaptionAlign;
                    repeatInfo.UseAccessibleHeader = UseAccessibleHeader;
                }
                else {
                    repeatInfo.EnableLegacyRendering = EnableLegacyRendering;
                }
            }

            repeatInfo.RenderRepeater(writer, (IRepeatInfoUser)this, style, this);
            if (outerTable != null)
                outerTable.RenderEndTag(writer);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override object SaveViewState() {
            object baseState = base.SaveViewState();
            object itemStyleState = (itemStyle != null) ? ((IStateManager)itemStyle).SaveViewState() : null;
            object selectedItemStyleState = (selectedItemStyle != null) ? ((IStateManager)selectedItemStyle).SaveViewState() : null;
            object alternatingItemStyleState = (alternatingItemStyle != null) ? ((IStateManager)alternatingItemStyle).SaveViewState() : null;
            object editItemStyleState = (editItemStyle != null) ? ((IStateManager)editItemStyle).SaveViewState() : null;
            object separatorStyleState = (separatorStyle != null) ? ((IStateManager)separatorStyle).SaveViewState() : null;
            object headerStyleState = (headerStyle != null) ? ((IStateManager)headerStyle).SaveViewState() : null;
            object footerStyleState = (footerStyle != null) ? ((IStateManager)footerStyle).SaveViewState() : null;
            object controlState = ControlStyleCreated ? ((IStateManager)ControlStyle).SaveViewState() : null;

            object[] myState = new object[9];
            myState[0] = baseState;
            myState[1] = itemStyleState;
            myState[2] = selectedItemStyleState;
            myState[3] = alternatingItemStyleState;
            myState[4] = editItemStyleState;
            myState[5] = separatorStyleState;
            myState[6] = headerStyleState;
            myState[7] = footerStyleState;
            myState[8] = controlState;

            // note that we always have some state, atleast the ItemCount
            return myState;
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Marks the starting point to begin tracking and saving changes to the 
        ///       control as part of the control viewstate.</para>
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();

            if (itemStyle != null)
                ((IStateManager)itemStyle).TrackViewState();
            if (selectedItemStyle != null)
                ((IStateManager)selectedItemStyle).TrackViewState();
            if (alternatingItemStyle != null)
                ((IStateManager)alternatingItemStyle).TrackViewState();
            if (editItemStyle != null)
                ((IStateManager)editItemStyle).TrackViewState();
            if (separatorStyle != null)
                ((IStateManager)separatorStyle).TrackViewState();
            if (headerStyle != null)
                ((IStateManager)headerStyle).TrackViewState();
            if (footerStyle != null)
                ((IStateManager)footerStyle).TrackViewState();
            if (ControlStyleCreated)
                ((IStateManager)ControlStyle).TrackViewState();
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        bool IRepeatInfoUser.HasFooter {
            get {
                return ShowFooter && (footerTemplate != null);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        bool IRepeatInfoUser.HasHeader {
            get {
                return ShowHeader && (headerTemplate != null);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        bool IRepeatInfoUser.HasSeparators {
            get {
                return (separatorTemplate != null);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        int IRepeatInfoUser.RepeatedItemCount {
            get {
                if (visibleItemCount == -1) {
                    return itemsArray != null ? itemsArray.Count : 0;
                }
                return visibleItemCount;
            }
        }




        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        Style IRepeatInfoUser.GetItemStyle(ListItemType itemType, int repeatIndex) {
            DataListItem item = GetItem(itemType, repeatIndex);

            if ((item != null) && item.ControlStyleCreated) {
                return item.ControlStyle;
            }
            return null;
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        void IRepeatInfoUser.RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer) {
            DataListItem item = GetItem(itemType, repeatIndex + offset);

            if (item != null) {
                item.RenderItem(writer, extractTemplateRows, repeatInfo.RepeatLayout == RepeatLayout.Table);
            }
        }

        #region IWizardSideBarListControl implementation

        IEnumerable IWizardSideBarListControl.Items {
            get { return Items; }
        }

        event CommandEventHandler IWizardSideBarListControl.ItemCommand {
            add {
                ItemCommand += new DataListCommandEventHandler(value);
            }
            remove {
                ItemCommand -= new DataListCommandEventHandler(value);
            }
        }
        
        event EventHandler<WizardSideBarListControlItemEventArgs> IWizardSideBarListControl.ItemDataBound {
            add {
                Events.AddHandler(EventWizardListItemDataBound , value);
            }
            remove {
                Events.RemoveHandler(EventWizardListItemDataBound, value);
            }
        }

        #endregion
    }
}

