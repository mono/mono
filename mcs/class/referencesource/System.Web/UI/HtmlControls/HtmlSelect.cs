//------------------------------------------------------------------------------
// <copyright file="HtmlSelect.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.HtmlControls {
    using System.Runtime.Serialization.Formatters;
    using System.Text;
    using System.ComponentModel;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Data;
    using System.Web;
    using System.Web.Util;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Globalization;
    using Debug=System.Web.Util.Debug;
    using System.Security.Permissions;

    public class HtmlSelectBuilder : ControlBuilder {


        public override Type GetChildControlType(string tagName, IDictionary attribs) {
            if (StringUtil.EqualsIgnoreCase(tagName, "option"))
                return typeof(ListItem);

            return null;
        }


        public override bool AllowWhitespaceLiterals() {
            return false;
        }
    }


    /// <devdoc>
    ///    <para>
    ///       The <see langword='HtmlSelect'/>
    ///       class defines the methods, properties, and events for the
    ///       HtmlSelect control. This class allows programmatic access to the HTML
    ///       &lt;select&gt; element on the server.
    ///    </para>
    /// </devdoc>
    [
    DefaultEvent("ServerChange"),
    ValidationProperty("Value"),
    ControlBuilderAttribute(typeof(HtmlSelectBuilder)),
    SupportsEventValidation,
    ]
    public class HtmlSelect : HtmlContainerControl, IPostBackDataHandler, IParserAccessor {

        private static readonly object EventServerChange = new object();

        internal const string DataBoundViewStateKey = "_!DataBound";

        private object dataSource;
        private ListItemCollection items;
        private int cachedSelectedIndex;

        private bool _requiresDataBinding;
        private bool _inited;
        private bool _throwOnDataPropertyChange;

        private DataSourceView _currentView;
        private bool _currentViewIsFromDataSourceID;
        private bool _currentViewValid;
        private bool _pagePreLoadFired;

        /*
         * Creates an intrinsic Html SELECT control.
         */

        public HtmlSelect() : base("select") {
            cachedSelectedIndex = -1;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebCategory("Data"),
        WebSysDescription(SR.HtmlSelect_DataMember)
        ]
        public virtual string DataMember {
            get {
                object o = ViewState["DataMember"];
                if (o != null)
                    return (string)o;
                return String.Empty;
            }
            set {
                Attributes["DataMember"] = MapStringAttributeToString(value);
                OnDataPropertyChanged();
            }
        }


        /// <devdoc>
        ///    Gets or sets the data source to databind the list values
        ///    in the <see langword='HtmlSelect'/> control against. This provides data to
        ///    populate the select list with items.
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(null),
        WebSysDescription(SR.BaseDataBoundControl_DataSource),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual object DataSource {
            get {
                return dataSource;
            }
            set {
                if ((value == null) || (value is IListSource) || (value is IEnumerable)) {
                    dataSource = value;
                    OnDataPropertyChanged();
                }
                else {
                    throw new ArgumentException(SR.GetString(SR.Invalid_DataSource_Type, ID));
                }
            }
        }


        /// <summary>
        /// The ID of the DataControl that this control should use to retrieve
        /// its data source. When the control is bound to a DataControl, it
        /// can retrieve a data source instance on-demand, and thereby attempt
        /// to work in auto-DataBind mode.
        /// </summary>
        [
        DefaultValue(""),
        WebCategory("Data"),
        WebSysDescription(SR.BaseDataBoundControl_DataSourceID),
        ]
        public virtual string DataSourceID {
            get {
                object o = ViewState["DataSourceID"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                ViewState["DataSourceID"] = value;
                OnDataPropertyChanged();
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the field in the data source that provides
        ///       the text for an option entry in the HtmlSelect control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        WebSysDescription(SR.HtmlSelect_DataTextField)
        ]
        public virtual string DataTextField {
            get {
                string s = Attributes["DataTextField"];
                return((s == null) ? String.Empty : s);
            }
            set {
                Attributes["DataTextField"] = MapStringAttributeToString(value);
                if (_inited) {
                    RequiresDataBinding = true;
                }
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the field in the data source that provides
        ///       the option item value for the <see langword='HtmlSelect'/>
        ///       control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Data"),
        DefaultValue(""),
        WebSysDescription(SR.HtmlSelect_DataValueField)
        ]
        public virtual string DataValueField {
            get {
                string s = Attributes["DataValueField"];
                return((s == null) ? String.Empty : s);
            }
            set {
                Attributes["DataValueField"] = MapStringAttributeToString(value);
                if (_inited) {
                    RequiresDataBinding = true;
                }
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string InnerHtml {
            get {
                throw new NotSupportedException(SR.GetString(SR.InnerHtml_not_supported, this.GetType().Name));
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.InnerHtml_not_supported, this.GetType().Name));
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string InnerText {
            get {
                throw new NotSupportedException(SR.GetString(SR.InnerText_not_supported, this.GetType().Name));
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.InnerText_not_supported, this.GetType().Name));
            }
        }


        protected bool IsBoundUsingDataSourceID {
            get {
                return (DataSourceID.Length > 0);
            }
        }

        /*
         * A collection containing the list of items.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets the list of option items in an <see langword='HtmlSelect'/> control.
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public ListItemCollection Items {
            get {
                if (items == null) {
                    items = new ListItemCollection();
                    if (IsTrackingViewState)
                        ((IStateManager)items).TrackViewState();
                }
                return items;
            }
        }

        /*
         * Multi-select property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether multiple option items can be selected
        ///       from the list.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool Multiple {
            get {
                string s = Attributes["multiple"];
                return((s != null) ? (s.Equals("multiple")) : false);
            }

            set {
                if (value)
                    Attributes["multiple"] = "multiple";
                else
                    Attributes["multiple"] = null;
            }
        }

        /*
         * Name property.
         */

        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Name {
            get {
                return UniqueID;
                //string s = Attributes["name"];
                //return ((s != null) ? s : "");
            }
            set {
                //Attributes["name"] = MapStringAttributeToString(value);
            }
        }

        // Value that gets rendered for the Name attribute
        internal string RenderedNameAttribute {
            get {
                return Name;
                //string name = Name;
                //if (name.Length == 0)
                //    return UniqueID;

                //return name;
            }
        }


        protected bool RequiresDataBinding {
            get {
                return _requiresDataBinding;
            }
            set {
                _requiresDataBinding = value;
            }
        }

        /*
         * The index of the selected item.
         * Returns the first selected item if list is multi-select.
         * Returns -1 if there is no selected item.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the ordinal index of the selected option item in an
        ///    <see langword='HtmlSelect'/> control. If multiple items are selected, this
        ///       property holds the index of the first item selected in the list.
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        HtmlControlPersistable(false),
        ]
        public virtual int SelectedIndex {
            get {
                for (int i=0; i < Items.Count; i++) {
                    if (Items[i].Selected)
                        return i;
                }
                if (Size <= 1 && !Multiple) {
                    // SELECT as a dropdown must have a selection
                    if (Items.Count > 0)
                        Items[0].Selected = true;
                    return 0;
                }
                return -1;
            }
            set {
                // if we have no items, save the selectedindex
                // for later databinding
                if (Items.Count == 0) {
                    cachedSelectedIndex = value;
                }
                else {
                    if (value < -1 || value >= Items.Count) {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    ClearSelection();
                    if (value >= 0)
                        Items[value].Selected = true;
                }
            }
        }

        /*
         *  SelectedIndices property.
         *  Protected property for getting array of selected indices.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual int[] SelectedIndices {
            get {
                int n = 0;
                int[] temp = new int[3];
                for (int i=0; i < Items.Count; i++) {
                    if (Items[i].Selected == true) {
                        if (n == temp.Length) {
                            int[] t = new int[n+n];
                            temp.CopyTo(t,0);
                            temp = t;
                        }
                        temp[n++] = i;
                    }
                }
                int[] selectedIndices = new int[n];
                Array.Copy(temp,0,selectedIndices,0,n);
                return selectedIndices;
            }
        }

        /*
         * The size of the list.
         * A size of 1 displays a dropdown list.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the number of option items visible in the browser at a time. A
        ///       value greater that one will typically cause browsers to display a scrolling
        ///       list.
        ///    </para>
        /// </devdoc>
        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Size {
            get {
                string s = Attributes["size"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }

            set {
                Attributes["size"] = MapIntegerAttributeToString(value);
            }
        }

        /*
         * Value property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the current item selected in the <see langword='HtmlSelect'/>
        ///       control.
        ///    </para>
        /// </devdoc>
        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Value {
            get {
                int i = SelectedIndex;
                return(i < 0 || i >= Items.Count) ? String.Empty : Items[i].Value;
            }

            set {
                int i = Items.FindByValueInternal(value, true);
                if (i >= 0)
                    SelectedIndex = i;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Occurs when an <see langword='HtmlSelect'/> control is changed on the
        ///       server.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.HtmlSelect_OnServerChange)
        ]
        public event EventHandler ServerChange {
            add {
                Events.AddHandler(EventServerChange, value);
            }
            remove {
                Events.RemoveHandler(EventServerChange, value);
            }
        }


        /// <internalonly/>
        protected override void AddParsedSubObject(object obj) {
            if (obj is ListItem)
                Items.Add((ListItem)obj);
            else
                throw new HttpException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "HtmlSelect", obj.GetType().Name));
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void ClearSelection() {
            for (int i=0; i < Items.Count; i++)
                Items[i].Selected = false;
        }

        /// <devdoc>
        /// Connects this data bound control to the appropriate DataSourceView
        /// and hooks up the appropriate event listener for the
        /// DataSourceViewChanged event. The return value is the new view (if
        /// any) that was connected to. An exception is thrown if there is
        /// a problem finding the requested view or data source.
        /// </devdoc>
        private DataSourceView ConnectToDataSourceView() {
            if (_currentViewValid && !DesignMode) {
                // If the current view is correct, there is no need to reconnect
                return _currentView;
            }

            // Disconnect from old view, if necessary
            if ((_currentView != null) && (_currentViewIsFromDataSourceID)) {
                // We only care about this event if we are bound through the DataSourceID property
                _currentView.DataSourceViewChanged -= new EventHandler(OnDataSourceViewChanged);
            }

            // Connect to new view
            IDataSource ds = null;
            string dataSourceID = DataSourceID;

            if (dataSourceID.Length != 0) {
                // Try to find a DataSource control with the ID specified in DataSourceID
                Control control = DataBoundControlHelper.FindControl(this, dataSourceID);
                if (control == null) {
                    throw new HttpException(SR.GetString(SR.DataControl_DataSourceDoesntExist, ID, dataSourceID));
                }
                ds = control as IDataSource;
                if (ds == null) {
                    throw new HttpException(SR.GetString(SR.DataControl_DataSourceIDMustBeDataControl, ID, dataSourceID));
                }
            }

            if (ds == null) {
                // DataSource control was not found, construct a temporary data source to wrap the data
                ds = new ReadOnlyDataSource(DataSource, DataMember);
            }
            else {
                // Ensure that both DataSourceID as well as DataSource are not set at the same time
                if (DataSource != null) {
                    throw new InvalidOperationException(SR.GetString(SR.DataControl_MultipleDataSources, ID));
                }
            }

            // IDataSource was found, extract the appropriate view and return it
            DataSourceView newView = ds.GetView(DataMember);
            if (newView == null) {
                throw new InvalidOperationException(SR.GetString(SR.DataControl_ViewNotFound, ID));
            }

            _currentViewIsFromDataSourceID = IsBoundUsingDataSourceID;
            _currentView = newView;
            if ((_currentView != null) && (_currentViewIsFromDataSourceID)) {
                // We only care about this event if we are bound through the DataSourceID property
                _currentView.DataSourceViewChanged += new EventHandler(OnDataSourceViewChanged);
            }
            _currentViewValid = true;

            return _currentView;
        }


        protected override ControlCollection CreateControlCollection() {
            return new EmptyControlCollection(this);
        }


        protected void EnsureDataBound() {
            try {
                _throwOnDataPropertyChange = true;
                if (RequiresDataBinding && DataSourceID.Length > 0) {
                    DataBind();
                }
            }
            finally{
                _throwOnDataPropertyChange = false;
            }
        }


        /// <devdoc>
        /// Returns an IEnumerable that is the DataSource, which either came
        /// from the DataSource property or from the control bound via the
        /// DataSourceID property.
        /// </devdoc>
        protected virtual IEnumerable GetData() {
            DataSourceView view = ConnectToDataSourceView();

            Debug.Assert(_currentViewValid);

            if (view != null) {
                return view.ExecuteSelect(DataSourceSelectArguments.Empty);
            }
            return null;
        }

        /*
         * Override to load items and selected indices.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void LoadViewState(object savedState) {
            if (savedState != null) {
                Triplet statetriplet = (Triplet)savedState;
                base.LoadViewState(statetriplet.First);

                // restore state of items
                ((IStateManager)Items).LoadViewState(statetriplet.Second);

                // restore selected indices
                object selectedIndices = statetriplet.Third;
                if (selectedIndices != null)
                    Select((int[])selectedIndices);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void OnDataBinding(EventArgs e) {
            base.OnDataBinding(e);

            // create items using the datasource
            IEnumerable dataSource = GetData();

            // create items using the datasource
            if (dataSource != null) {
                bool fieldsSpecified = false;
                string textField = DataTextField;
                string valueField = DataValueField;

                Items.Clear();
                ICollection collection = dataSource as ICollection;
                if (collection != null) {
                    Items.Capacity = collection.Count;
                }

                if ((textField.Length != 0) || (valueField.Length != 0))
                    fieldsSpecified = true;

                foreach (object dataItem in dataSource) {
                    ListItem item = new ListItem();

                    if (fieldsSpecified) {
                        if (textField.Length > 0) {
                            item.Text = DataBinder.GetPropertyValue(dataItem,textField,null);
                        }
                        if (valueField.Length > 0) {
                            item.Value = DataBinder.GetPropertyValue(dataItem,valueField,null);
                        }
                    }
                    else {
                        item.Text = item.Value = dataItem.ToString();
                    }

                    Items.Add(item);
                }
            }
            // try to apply the cached SelectedIndex now
            if (cachedSelectedIndex != -1) {
                SelectedIndex = cachedSelectedIndex;
                cachedSelectedIndex = -1;
            }
            ViewState[DataBoundViewStateKey] = true;
            RequiresDataBinding = false;
        }


        /// <devdoc>
        ///  This method is called when DataMember, DataSource, or DataSourceID is changed.
        /// </devdoc>
        protected virtual void OnDataPropertyChanged() {
            if (_throwOnDataPropertyChange) {
                throw new HttpException(SR.GetString(SR.DataBoundControl_InvalidDataPropertyChange, ID));
            }
            
            if (_inited) {
                RequiresDataBinding = true;
            }
            _currentViewValid = false;
        }


        protected virtual void OnDataSourceViewChanged(object sender, EventArgs e) {
            RequiresDataBinding = true;
        }


        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);
            
            if (Page != null) {
                Page.PreLoad += new EventHandler(this.OnPagePreLoad);
                if (!IsViewStateEnabled && Page.IsPostBack) {
                    RequiresDataBinding = true;
                }
            }
        }


        protected internal override void OnLoad(EventArgs e) {
            _inited = true; // just in case we were added to the page after PreLoad
            ConnectToDataSourceView();
            if (Page != null && !_pagePreLoadFired && ViewState[DataBoundViewStateKey] == null) {
                // If the control was added after PagePreLoad, we still need to databind it because it missed its
                // first change in PagePreLoad.  If this control was created by a call to a parent control's DataBind
                // in Page_Load (with is relatively common), this control will already have been databound even
                // though pagePreLoad never fired and the page isn't a postback.
                if (!Page.IsPostBack) {
                    RequiresDataBinding = true;
                }

                // If the control was added to the page after page.PreLoad, we'll never get the event and we'll
                // never databind the control.  So if we're catching up and Load happens but PreLoad never happened,
                // call DataBind.  This may make the control get databound twice if the user called DataBind on the control
                // directly in Page.OnLoad, but better to bind twice than never to bind at all.
                else if (IsViewStateEnabled) {
                    RequiresDataBinding = true;
                }
            }

            base.OnLoad(e);
        }

        private void OnPagePreLoad(object sender, EventArgs e) {
            _inited = true;

            if (Page != null) {
                Page.PreLoad -= new EventHandler(this.OnPagePreLoad);

                // Setting RequiresDataBinding to true in OnLoad is too late because the OnLoad page event
                // happens before the control.OnLoad method gets called.  So a page_load handler on the page
                // that calls DataBind won't prevent DataBind from getting called again in PreRender.
                if (!Page.IsPostBack) {
                    RequiresDataBinding = true;
                }
                // If this is a postback and viewstate is enabled, but we have never bound the control
                // before, it is probably because its visibility was changed in the postback.  In this
                // case, we need to bind the control or it will never appear.  This is a common scenario
                // for Wizard and MultiView.
                if (Page.IsPostBack && IsViewStateEnabled && ViewState[DataBoundViewStateKey] == null) {
                    RequiresDataBinding = true;
                }
            }
            _pagePreLoadFired = true;
        }
        
        /*
         * This method is invoked just prior to rendering.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // An Html SELECT does not post when nothing is selected.
            if (Page != null && !Disabled) {
                if (Size > 1) {
                    Page.RegisterRequiresPostBack(this);
                }

                Page.RegisterEnabledControl(this);
            }

            EnsureDataBound();
        }

        /*
         * Method used to raise the OnServerChange event.
         */

        /// <devdoc>
        ///    <para>
        ///       Raised
        ///       on the server when the <see langword='HtmlSelect'/> control list values
        ///       change between postback requests.
        ///    </para>
        /// </devdoc>
        protected virtual void OnServerChange(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventServerChange];
            if (handler != null) handler(this, e);
        }

        /*
         * Override to prevent SelectedIndex from being rendered as an attribute.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderAttributes(HtmlTextWriter writer) {
            if (Page != null) {
                Page.ClientScript.RegisterForEventValidation(RenderedNameAttribute);
            }

            writer.WriteAttribute("name", RenderedNameAttribute);
            Attributes.Remove("name");

            Attributes.Remove("DataValueField");
            Attributes.Remove("DataTextField");
            Attributes.Remove("DataMember");
            Attributes.Remove("DataSourceID");
            base.RenderAttributes(writer);
        }

        /*
         * Render the Items in the list.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void RenderChildren(HtmlTextWriter writer) {
            bool selected = false;
            bool isSingle = !Multiple;

            writer.WriteLine();
            writer.Indent++;
            ListItemCollection liCollection = Items;
            int n = liCollection.Count;
            if (n > 0) {
                for (int i=0; i < n; i++) {
                    ListItem li = liCollection[i];
                    writer.WriteBeginTag("option");
                    if (li.Selected) {
                        if (isSingle)
                        {
                            if (selected)
                                throw new HttpException(SR.GetString(SR.HtmlSelect_Cant_Multiselect_In_Single_Mode));
                            selected=true;
                        }
                        writer.WriteAttribute("selected", "selected");
                    }

                    writer.WriteAttribute("value", li.Value, true /*fEncode*/);

                    // This is to fix the case where the user puts one of these
                    // three values in the AttributeCollection.  Removing them
                    // at least is better than rendering them twice.
                    li.Attributes.Remove("text");
                    li.Attributes.Remove("value");
                    li.Attributes.Remove("selected");

                    li.Attributes.Render(writer);
                    writer.Write(HtmlTextWriter.TagRightChar);
                    HttpUtility.HtmlEncode(li.Text, writer);
                    writer.WriteEndTag("option");
                    writer.WriteLine();
                }
            }
            writer.Indent--;
        }

        /*
         * Save selected indices and modified Items.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override object SaveViewState() {

            object baseState = base.SaveViewState();
            object items = ((IStateManager)Items).SaveViewState();
            object selectedindices = null;

            // only save selection if handler is registered,
            // we are disabled, or we are not visible
            // since selection is always posted back otherwise
            if (Events[EventServerChange] != null || Disabled || !Visible)
                selectedindices = SelectedIndices;

            if (selectedindices  != null || items != null || baseState != null)
                return new Triplet(baseState, items, selectedindices);

            return null;
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void Select(int[] selectedIndices) {
            ClearSelection();
            for (int i=0; i < selectedIndices.Length; i++) {
                int n = selectedIndices[i];
                if (n >= 0 && n < Items.Count)
                    Items[n].Selected = true;
            }
        }

        /*
         * TrackState
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void TrackViewState() {
            base.TrackViewState();
            ((IStateManager)Items).TrackViewState();
        }


        /*
         * Method of IPostBackDataHandler interface to process posted data.
         * SelectList processes a newly posted value.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            string[] selectedItems = postCollection.GetValues(postDataKey);
            bool selectionChanged = false;

            if (selectedItems != null) {
                if (!Multiple) {
                    int n = Items.FindByValueInternal(selectedItems[0], false);
                    if (SelectedIndex != n) {
                        SelectedIndex = n;
                        selectionChanged = true;
                    }
                }
                else { // multiple selection
                    int count = selectedItems.Length;
                    int[] oldSelectedIndices = SelectedIndices;
                    int[] newSelectedIndices = new int[count];
                    for (int i=0; i < count; i++) {
                        // create array of new indices from posted values
                        newSelectedIndices[i] = Items.FindByValueInternal(selectedItems[i], false);
                    }

                    if (oldSelectedIndices.Length == count) {
                        // check new indices against old indices
                        // assumes selected values are posted in order
                        for (int i=0; i < count; i++) {
                            if (newSelectedIndices[i] != oldSelectedIndices[i]) {
                                selectionChanged = true;
                                break;
                            }
                        }
                    }
                    else {
                        // indices must have changed if count is different
                        selectionChanged = true;
                    }

                    if (selectionChanged) {
                        // select new indices
                        Select(newSelectedIndices);
                    }
                }
            }
            else { // no items selected
                if (SelectedIndex != -1) {
                    SelectedIndex = -1;
                    selectionChanged = true;
                }
            }

            if (selectionChanged) {
                ValidateEvent(postDataKey);
            }

            return selectionChanged;
        }

        /*
         * Method of IPostBackDataHandler interface which is invoked whenever posted data
         * for a control has changed.  SelectList fires an OnServerChange event.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected virtual void RaisePostDataChangedEvent() {
            OnServerChange(EventArgs.Empty);            
        }
    }
}
