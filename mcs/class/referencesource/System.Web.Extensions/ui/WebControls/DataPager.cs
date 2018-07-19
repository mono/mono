//------------------------------------------------------------------------------
// <copyright file="DataPager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Resources;
using System.Web.UI;

namespace System.Web.UI.WebControls {
    [
    ParseChildren(true),
    PersistChildren(false),
    Themeable(true),
    SupportsEventValidation,
    Designer("System.Web.UI.Design.WebControls.DataPagerDesigner, " + AssemblyRef.SystemWebExtensionsDesign),
    ToolboxBitmap(typeof(DataPager), "DataPager.bmp")
    ]
    public class DataPager : Control, IAttributeAccessor, INamingContainer, ICompositeControlDesignerAccessor {
        private readonly new IPage _page;
        private DataPagerFieldCollection _fields;
        private IPageableItemContainer _pageableItemContainer;
        private int _startRowIndex = 0;
        private int _maximumRows = 10;
        private int _totalRowCount;
        private bool _setPageProperties = false;
        private bool _initialized = false;
        private AttributeCollection _attributes;
        private bool _creatingPagerFields;
        private bool _queryStringHandled;
        private string _queryStringNavigateUrl;

        public DataPager() {
        }

        internal DataPager(IPage page) {
            _page = page;
        }

        /// <devdoc>
        ///    <para>Gets the collection of attribute name/value pairs expressed on the list item
        ///       control but not supported by the control's strongly typed properties.</para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public AttributeCollection Attributes {
            get {
                if (_attributes == null)
                    _attributes = new AttributeCollection(new StateBag(true));

                return _attributes;
            }
        }

        /// <devdoc>
        /// Ensure that the child controls have been created before returning the controls collection
        /// </devdoc>
        public override ControlCollection Controls {
            get {
                EnsureChildControls();
                return base.Controls;
            }
        }

        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.DataPagerFieldTypeEditor, " + AssemblyRef.SystemWebExtensionsDesign, typeof(UITypeEditor)),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        Category("Default"),
        ResourceDescription("DataPager_Fields"),
        ]
        public virtual DataPagerFieldCollection Fields {
            get {
                if (_fields == null) {
                    _fields = new DataPagerFieldCollection(this);
                    if (IsTrackingViewState) {
                        ((IStateManager)_fields).TrackViewState();
                    }
                    _fields.FieldsChanged += new EventHandler(OnFieldsChanged);
                }
                return _fields;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int MaximumRows {
            get {
                return _maximumRows;
            }
        }

        [
        DefaultValue(""),
        IDReferenceProperty(typeof(IPageableItemContainer)),
        WebCategory("Paging"),
        ResourceDescription("DataPager_PagedControlID"),
        SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID"),
        Themeable(false),
        ]
        public virtual string PagedControlID {
            get {
                object o = ViewState["PagedControlID"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                ViewState["PagedControlID"] = value;
            }
        }

        internal IPage IPage {
            get {
                if (_page != null) {
                    return _page;
                }
                else {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    return new PageWrapper(page);
                }
            }
        }

        [
        DefaultValue(10),
        WebCategory("Paging"),
        ResourceDescription("DataPager_PageSize"),
        ]
        public int PageSize {
            get {
                return _maximumRows;
            }
            set {
                if (value < 1) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != _maximumRows) {
                    _maximumRows = value;
                    if (_initialized) {
                        CreatePagerFields();
                        SetPageProperties(_startRowIndex, _maximumRows, true);
                    }
                }
            }
        }

        [
        WebCategory("Paging"),
        DefaultValue(""),
        ResourceDescription("DataPager_QueryStringField")
        ]
        public string QueryStringField {
            get {
                object o = ViewState["QueryStringField"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                ViewState["QueryStringField"] = value;
            }
        }

        internal bool QueryStringHandled {
            get {
                return _queryStringHandled;
            }
            set {
                _queryStringHandled = value;
            }
        }

        internal string QueryStringValue {
            get {
                if (DesignMode) {
                    return String.Empty;
                }
                return IPage.Request.QueryString[QueryStringField];
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int StartRowIndex {
            get {
                return _startRowIndex;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected virtual HtmlTextWriterTag TagKey {
            get {
                return HtmlTextWriterTag.Span;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int TotalRowCount {
            get {
                return _totalRowCount;
            }
        }

        protected virtual void AddAttributesToRender(HtmlTextWriter writer) {
            if (this.ID != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            }

            // add expando attributes
            if (_attributes != null) {
                AttributeCollection atrColl = Attributes;
                IEnumerator keys = atrColl.Keys.GetEnumerator();
                while (keys.MoveNext()) {
                    string attrName = (string)(keys.Current);
                    writer.AddAttribute(attrName, atrColl[attrName]);
                }
            }
        }

        protected virtual void ConnectToEvents(IPageableItemContainer container) {
            if (container == null) {
                throw new ArgumentNullException("container");
            }

            // 
            _pageableItemContainer.TotalRowCountAvailable += new EventHandler<PageEventArgs>(OnTotalRowCountAvailable);
        }

        protected virtual void CreatePagerFields() {
            _creatingPagerFields = true;
            Controls.Clear();
            if (_fields != null) {
                foreach (DataPagerField field in _fields) {
                    DataPagerFieldItem fieldItem = new DataPagerFieldItem(field, this);
                    Controls.Add(fieldItem);
                    if (field.Visible) {
                        field.CreateDataPagers(fieldItem, _startRowIndex, _maximumRows, _totalRowCount, _fields.IndexOf(field));
                        fieldItem.DataBind();
                    }
                }
            }
            _creatingPagerFields = false;
        }

        /// <devdoc>
        /// Perform our own databinding, then perform our child controls' databinding.
        /// Does not call Base.DataBind(), since we need to call EnsureChildControls() between
        /// OnDataBinding() and DataBindChildren().
        /// </devdoc>
        public override void DataBind() {
            OnDataBinding(EventArgs.Empty);

            EnsureChildControls();

            DataBindChildren();
        }

        protected virtual IPageableItemContainer FindPageableItemContainer() {
            // the PagedControlID can be specified for finding a control within the same naming container
            // when the pager control isn't inside the IPageableItemContainter.
            if (!String.IsNullOrEmpty(PagedControlID)) {
                // The IPageableItemContainer is found by FindControl if specified.
                Control control = DataBoundControlHelper.FindControl(this, PagedControlID);
                if (control == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.DataPager_PageableItemContainerNotFound, PagedControlID));
                }
                IPageableItemContainer container = control as IPageableItemContainer;
                if (container == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.DataPager_ControlIsntPageable, PagedControlID));
                }
                return container;
            }
            else {
                // Look to see if parent container is IPageableItemContainer
                Control currentContainer = this.NamingContainer;
                IPageableItemContainer foundContainer = null;

                while (foundContainer == null && currentContainer != this.Page) {
                    if (currentContainer == null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.DataPager_NoNamingContainer, ID));
                    }
                    foundContainer = currentContainer as IPageableItemContainer;
                    currentContainer = currentContainer.NamingContainer;
                }

                return foundContainer;
            }
        }

        internal string GetQueryStringNavigateUrl(int pageNumber) {
            if (_queryStringNavigateUrl == null) {
                string queryStringField = QueryStringField;
                StringBuilder sb = new StringBuilder();
                if (DesignMode) {
                    sb.Append("?");
                }
                else {
                    bool methodGet = (IPage.Form != null) &&
                        IPage.Form.Method.Equals("GET", StringComparison.OrdinalIgnoreCase);
                    HttpRequestBase request = IPage.Request;
                    sb.Append(request.Path);
                    sb.Append("?");
                    foreach (string field in request.QueryString.AllKeys) {
                        //skip null/empty query string fields
                        if (String.IsNullOrEmpty(field)) {
                            continue;
                        }
                        // exclude query string postback data.
                        if (methodGet && ControlUtil.IsBuiltInHiddenField(field)) {
                            continue;
                        }
                        // append all query string fields except the current page.                        
                        if (!field.Equals(queryStringField, StringComparison.OrdinalIgnoreCase)) {
                            sb.Append(HttpUtility.UrlEncode(field));
                            sb.Append("=");
                            sb.Append(HttpUtility.UrlEncode(request.QueryString[field]));
                            sb.Append("&");
                        }
                    }
                }
                sb.Append(queryStringField);
                sb.Append("=");
                _queryStringNavigateUrl = sb.ToString();
            }

            return _queryStringNavigateUrl + pageNumber.ToString(CultureInfo.InvariantCulture);
        }

        /// <devdoc>
        /// <para>Loads the control state for those properties that should persist across postbacks
        ///   even when EnableViewState=false.</para>
        /// </devdoc>
        protected internal override void LoadControlState(object savedState) {
            // Any properties that could have been set in the persistance need to be
            // restored to their defaults if they're not in ControlState, or they will
            // be restored to their persisted state instead of their empty state.
            _startRowIndex = 0;
            _maximumRows = 10;
            _totalRowCount = -1;
            object[] state = savedState as object[];

            if (state != null) {
                base.LoadControlState(state[0]);

                if (state[1] != null) {
                    _startRowIndex = (int)state[1];
                }

                if (state[2] != null) {
                    _maximumRows = (int)state[2];
                }

                if (state[3] != null) {
                    _totalRowCount = (int)state[3];
                }
            }
            else {
                base.LoadControlState(null);
            }

            if (_pageableItemContainer == null) {
                _pageableItemContainer = FindPageableItemContainer();
                if (_pageableItemContainer == null) {
                    throw new InvalidOperationException(AtlasWeb.DataPager_NoPageableItemContainer);
                }
                ConnectToEvents(_pageableItemContainer);
            }

            _pageableItemContainer.SetPageProperties(_startRowIndex, _maximumRows, false);
            _setPageProperties = true;
        }

        bool HasAttributes {
            get {
                return (_attributes != null && _attributes.Count > 0);
            }
        }

        protected override void LoadViewState(object savedState) {
            if (savedState == null)
                return;

            object[] state = (object[])savedState;

            base.LoadViewState(state[0]);
            if (state[1] != null) {
                ((IStateManager)Fields).LoadViewState(state[1]);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "1#")]
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            DataPagerFieldCommandEventArgs cea = e as DataPagerFieldCommandEventArgs;
            bool handled = false;

            if (cea != null) {
                DataPagerFieldItem item = cea.Item;
                if (item != null && item.PagerField != null) {
                    item.PagerField.HandleEvent(cea);
                    handled = true;
                }
            }
            return handled;
        }

        private void OnFieldsChanged(object source, EventArgs e) {
            // force the paged control to rebind to pick up the changes.
            if (_initialized) {
                SetPageProperties(_startRowIndex, _maximumRows, true);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);
            // We can't try to find another control in the designer in Init.
            if (!DesignMode) {
                _pageableItemContainer = FindPageableItemContainer();
                if (_pageableItemContainer != null) {
                    ConnectToEvents(_pageableItemContainer);

                    if (!String.IsNullOrEmpty(QueryStringField)) {
                        _startRowIndex = GetStartRowIndexFromQueryString();
                    }

                    _pageableItemContainer.SetPageProperties(_startRowIndex, _maximumRows, false);
                    _setPageProperties = true;
                }

                if (Page != null) {
                    Page.RegisterRequiresControlState(this);
                }
            }


            _initialized = true;
        }

        private int GetStartRowIndexFromQueryString() {
            int startRowIndex = 0;
            int pageIndex = 0;
            if (Int32.TryParse(QueryStringValue, out pageIndex)) {                
                startRowIndex = (pageIndex - 1) * _maximumRows;
            }
            return startRowIndex;
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected internal override void OnLoad(EventArgs e) {
            if (_pageableItemContainer == null) {
                _pageableItemContainer = FindPageableItemContainer();
            }
            if (_pageableItemContainer == null) {
                throw new InvalidOperationException(AtlasWeb.DataPager_NoPageableItemContainer);
            }

            // Page properties may not have been set in OnInit because the IPageableItemContainer 
            // could have been added in OnLoad
            if (!_setPageProperties) {
                ConnectToEvents(_pageableItemContainer);
                if (!String.IsNullOrEmpty(QueryStringField)) {
                    _startRowIndex = GetStartRowIndexFromQueryString();
                }
                _pageableItemContainer.SetPageProperties(_startRowIndex, _maximumRows, false);
                _setPageProperties = true;
            }

            base.OnLoad(e);
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "1#")]
        protected virtual void OnTotalRowCountAvailable(object sender, PageEventArgs e) {
            _totalRowCount = e.TotalRowCount;
            _startRowIndex = e.StartRowIndex;
            _maximumRows = e.MaximumRows;

            if (_totalRowCount <= _startRowIndex && _totalRowCount > 0) {
                // The last item got deleted or the results set changed and has fewer items.  Move to the prior page.
                int newStartIndex = _startRowIndex - _maximumRows;
                if (newStartIndex < 0) {
                    newStartIndex = 0;
                }

                // If we can't just go back one page, go back to the first page.  Most likely it's a new
                // results set.
                if (newStartIndex >= _totalRowCount) {
                    newStartIndex = 0;
                }

                // Rebind the IPageableItemContainer with the corrected values
                _pageableItemContainer.SetPageProperties(newStartIndex, _maximumRows, true);
                return;
            }

            if (!_creatingPagerFields) {
                CreatePagerFields();
            }
        }

        protected virtual void RecreateChildControls() {
            ChildControlsCreated = false;
            EnsureChildControls();
        }

        protected internal override void Render(HtmlTextWriter writer) {
            if (DesignMode) {
                EnsureChildControls();
                // Call OnTotalRowCountAvailable at design time so we see the pager.
                OnTotalRowCountAvailable(null, new PageEventArgs(0, PageSize, 101));
            }

            RenderBeginTag(writer);
            RenderContents(writer);
            writer.RenderEndTag();
        }

        public virtual void RenderBeginTag(HtmlTextWriter writer) {
            AddAttributesToRender(writer);
            writer.RenderBeginTag(TagKey);
        }

        protected virtual void RenderContents(HtmlTextWriter writer) {
            base.Render(writer);
        }

        /// <devdoc>
        /// <para>Saves the control state for those properties that should persist across postbacks
        ///   even when EnableViewState=false.</para>
        /// </devdoc>
        protected internal override object SaveControlState() {
            object baseState = base.SaveControlState();
            if (baseState != null ||
                _startRowIndex != 0 ||
                _maximumRows != 10 ||
                _totalRowCount != -1) {

                object[] state = new object[4];

                state[0] = baseState;
                state[1] = (_startRowIndex == 0) ? null : (object)_startRowIndex;
                state[2] = (_maximumRows == 10) ? null : (object)_maximumRows;
                state[3] = (_totalRowCount == -1) ? null : (object)_totalRowCount;

                return state;
            }
            return null;
        }

        protected override object SaveViewState() {
            // call base
            object baseState = base.SaveViewState();
            object myState = (_fields != null) ? ((IStateManager)_fields).SaveViewState() : null;
            return new object[2] { baseState, myState };
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "databind",
            Justification = "Cannot change to 'dataBind' as would break binary compatibility with legacy code.")]
        public virtual void SetPageProperties(int startRowIndex, int maximumRows, bool databind) {
            if (DesignMode) {
                return;
            }

            if (_pageableItemContainer == null) {
                throw new InvalidOperationException(AtlasWeb.DataPager_PagePropertiesCannotBeSet);
            }
            _startRowIndex = startRowIndex;
            _maximumRows = maximumRows;
            _pageableItemContainer.SetPageProperties(startRowIndex, maximumRows, databind);
        }

        protected override void TrackViewState() {
            base.TrackViewState();
            if (_fields != null) {
                ((IStateManager)_fields).TrackViewState();
            }
        }

        #region ICompositeControlDesignerAccessor implementation
        void ICompositeControlDesignerAccessor.RecreateChildControls() {
            RecreateChildControls();
        }
        #endregion

        #region IAttributeAccessor
        /// <internalonly/>
        /// <devdoc>
        /// Returns the attribute value of the list item control
        /// having the specified attribute name.
        /// </devdoc>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
                         Justification = "Equivalent functionality is provided by public Attributes property.")]
        string IAttributeAccessor.GetAttribute(string name) {
            return Attributes[name];
        }


        /// <internalonly/>
        /// <devdoc>
        /// <para>Sets an attribute of the list
        /// item control with the specified name and value.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes",
                         Justification = "Equivalent functionality is provided by public Attributes property.")]
        void IAttributeAccessor.SetAttribute(string name, string value) {
            Attributes[name] = value;
        }
        #endregion IAttributeAccessor
    }
}
