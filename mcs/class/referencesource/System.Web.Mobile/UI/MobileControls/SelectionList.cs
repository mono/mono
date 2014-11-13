//------------------------------------------------------------------------------
// <copyright file="SelectionList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Mobile SelectionList class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList"]/*' />
    [
        ControlBuilderAttribute(typeof(ListControlBuilder)),
        DefaultEvent("SelectedIndexChanged"),
        DefaultProperty("DataSource"),
        Designer(typeof(System.Web.UI.Design.MobileControls.SelectionListDesigner)),
        DesignerAdapter(typeof(System.Web.UI.Design.MobileControls.Adapters.DesignerSelectionListAdapter)),
        Editor(typeof(System.Web.UI.Design.MobileControls.SelectionListComponentEditor), typeof(ComponentEditor)),
        ToolboxData("<{0}:SelectionList runat=\"server\"></{0}:SelectionList>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign),
        ValidationProperty("Selection")
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class SelectionList : MobileControl, IPostBackDataHandler, IListControl
    {
        private static readonly Object EventItemDataBind = new Object();
        private static readonly Object EventSelectedIndexChanged = new Object();

        private ListDataHelper _dataHelper;
        private int _cachedSelectedIndex = -1;
        
        private ListDataHelper DataHelper {
            get {
                if (_dataHelper == null) {
                    _dataHelper = new ListDataHelper(this, ViewState);
                }

                return _dataHelper;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  IListControl
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.IListControl.OnItemDataBind"]/*' />
        /// <internalonly/>
        void IListControl.OnItemDataBind(ListDataBindEventArgs e) 
        {
            OnItemDataBind(e);
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.IListControl.TrackingViewState"]/*' />
        /// <internalonly/>
        bool IListControl.TrackingViewState
        {
            get
            {
                return IsTrackingViewState;
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.DataSource"]/*' />
        /// <summary>
        /// <para>
        /// Gets or sets the <see langword='DataSource'/> property of the control which is used to populate
        /// the items within the control.
        /// </para>
        /// </summary>
        [
            Bindable(true),
            DefaultValue(null),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            MobileCategory(SR.Category_Data),
            MobileSysDescription(SR.List_DataSource)
        ]
        public virtual Object DataSource 
        {
            get 
            {
                return DataHelper.DataSource;
            }

            set 
            {
                DataHelper.DataSource = value;
            }
        }

        private IEnumerable ResolvedDataSource
        {
            get
            {
                return DataHelper.ResolvedDataSource;
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.DataMember"]/*' />
        [
            Bindable(false),
            DefaultValue(""),
            MobileCategory(SR.Category_Data),
            MobileSysDescription(SR.List_DataMember),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.DataMemberConverter))
        ]
        public virtual String DataMember
        {
            get 
            {
                return DataHelper.DataMember;
            }

            set 
            {
                DataHelper.DataMember = value;
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.DataTextField"]/*' />
        [
            DefaultValue(""),
            MobileCategory(SR.Category_Data),
            MobileSysDescription(SR.List_DataTextField),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.DataFieldConverter))
        ]
        public String DataTextField 
        {
            get 
            {
                return DataHelper.DataTextField;
            }
            set 
            {
                DataHelper.DataTextField = value;
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.DataValueField"]/*' />
        [
            DefaultValue(""),
            MobileCategory(SR.Category_Data),
            MobileSysDescription(SR.List_DataValueField),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.DataFieldConverter))
        ]
        public String DataValueField 
        {
            get 
            {
                return DataHelper.DataValueField;
            }
            set 
            {
                DataHelper.DataValueField = value;
            }
        }
        
        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.IsMultiSelect"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool IsMultiSelect
        {
            get
            {
                return (SelectType == ListSelectType.MultiSelectListBox  || 
                        SelectType == ListSelectType.CheckBox);
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.Rows"]/*' />
        [
            Bindable(true),
            Browsable(true),
            DefaultValue(4),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.SelectionList_Rows)
        ]
        public int Rows
        {
            get
            {
                Object o = ViewState["Rows"];
                return o != null ? (int)o : 4;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Rows"] = value;
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.SelectedIndex"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int SelectedIndex 
        {
            get 
            {
                for (int i = 0; i < Items.Count; i++) 
                {
                    if (Items[i].Selected)
                    {
                        return i;
                    }
                }
                return -1;
            }
            set 
            {
                // if we have no items, save the selectedindex
                // for later databinding
                if (Items.Count == 0) 
                {
                    _cachedSelectedIndex = value;
                }
                else 
                {
                    if (value < -1 || value >= Items.Count)
                    {
                        throw new ArgumentOutOfRangeException(
                            "SelectedIndex",
                            SR.GetString(SR.SelectionList_OutOfRange,value));
                    }
                    ClearSelection();
                    if (value >= 0)
                    {
                        Items[value].Selected = true;
                    }
                }
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.Selection"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public MobileListItem Selection
        {
            get
            {
                int selectedIndex = SelectedIndex;
                if (selectedIndex >= 0 && selectedIndex < Items.Count)
                {
                    return Items[selectedIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.Items"]/*' />
        [
            Bindable(false),
            DefaultValue(null),
            Editor(typeof(System.Web.UI.Design.MobileControls.ItemCollectionEditor), typeof(UITypeEditor)),
            MergableProperty(false),
            MobileSysDescription(SR.List_Items),
            PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public MobileListItemCollection Items
        {
            get
            {
                return DataHelper.Items;
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.AddParsedSubObject"]/*' />
        protected override void AddParsedSubObject(Object obj)
        {
            if (!(obj is LiteralControl))
            {
                if (obj is MobileListItem)
                {
                    DataHelper.AddItem((MobileListItem)obj);
                    if(_cachedSelectedIndex != -1 &&
                       DataHelper.Items.Count > _cachedSelectedIndex)
                    {
                        SelectedIndex = _cachedSelectedIndex;
                        _cachedSelectedIndex = -1;
                    }
                } 
                else
                {
                    base.AddParsedSubObject(obj);
                }
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.OnPreRender"]/*' />
        protected override void OnPreRender(EventArgs e) 
        {
            base.OnPreRender(e);
            if (Page != null && IsMultiSelect) 
            {
                // ensure postback when no item is selected
                Page.RegisterRequiresPostBack(this);
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.OnDataBinding"]/*' />
        protected override void OnDataBinding(EventArgs e) 
        {
            base.OnDataBinding(e);

            if (ResolvedDataSource != null)
            {
                CreateItems(ResolvedDataSource);
            }

            if (_cachedSelectedIndex != -1) 
            {
                SelectedIndex = _cachedSelectedIndex;
                _cachedSelectedIndex = -1;
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.CreateItems"]/*' />
        protected virtual void CreateItems(IEnumerable dataSource) 
        {
            DataHelper.CreateItems(dataSource);
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.SelectedIndexChanged"]/*' />
        [
            MobileCategory(SR.Category_Action),
            MobileSysDescription(SR.SelectionList_OnSelectedIndexChanged)
        ]
        public event EventHandler SelectedIndexChanged
        {
            add 
            {
                Events.AddHandler(EventSelectedIndexChanged, value);
            }
            remove 
            {
                Events.RemoveHandler(EventSelectedIndexChanged, value);
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.OnSelectedIndexChanged"]/*' />
        protected virtual void OnSelectedIndexChanged(EventArgs e) 
        {
            EventHandler onSelectedIndexChangedHandler = (EventHandler)Events[EventSelectedIndexChanged];
            if (onSelectedIndexChangedHandler != null)
            {
                onSelectedIndexChangedHandler(this, e);
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.ItemDataBind"]/*' />
        [
            MobileCategory(SR.Category_Action),
            MobileSysDescription(SR.List_OnItemDataBind)
        ]
        public event ListDataBindEventHandler ItemDataBind 
        {
            add 
            {
                Events.AddHandler(EventItemDataBind, value);
            }
            remove 
            {
                Events.RemoveHandler(EventItemDataBind, value);
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.OnItemDataBind"]/*' />
        protected virtual void OnItemDataBind(ListDataBindEventArgs e) 
        {
            ListDataBindEventHandler onItemDataBindHandler = (ListDataBindEventHandler)Events[EventItemDataBind];
            if (onItemDataBindHandler != null)
            {
                onItemDataBindHandler(this, e);
            }
        }

        /// <internalonly/>
        protected bool LoadPostData(String postDataKey, NameValueCollection postCollection) 
        {
            bool dataChanged;
            bool handledByAdapter =
                Adapter.LoadPostData(postDataKey,
                                     postCollection,
                                     SelectedIndicesInternal.ToArray(typeof(int)),
                                     out dataChanged);

            if (!handledByAdapter)
            {
                throw new
                    Exception(SR.GetString(SR.SelectionList_AdapterNotHandlingLoadPostData));
            }

            return dataChanged;
        }

        /// <internalonly/>
        protected void RaisePostDataChangedEvent() 
        {
            OnSelectedIndexChanged(EventArgs.Empty);
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.SelectType"]/*' />
        [
            Bindable(true),
            DefaultValue(ListSelectType.DropDown),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.SelectionList_SelectType)
        ]
        public ListSelectType SelectType
        {
            get
            {
                Object o = ViewState["SelectType"];
                return (o != null) ? (ListSelectType)o : ListSelectType.DropDown;
            }
            set
            {
                ViewState["SelectType"] = value;
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.Title"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Input_Title)
        ]
        public String Title
        {
            get
            {
                return ToString(ViewState["Title"]);
            }
            set
            {
                ViewState["Title"] = value;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  STATE MANAGEMENT
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.TrackViewState"]/*' />
        protected override void TrackViewState() 
        {
            base.TrackViewState();
            if (DataHelper.HasItems())
            {
                ((IStateManager)Items).TrackViewState();
            }
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.SaveViewState"]/*' />
        protected override Object SaveViewState() 
        {
            Object baseState = base.SaveViewState();
            Items.SaveSelection = true;
            Object items = ((IStateManager)Items).SaveViewState();

            if (items != null || baseState != null)
            {
                return new Object[] { baseState, items };
            }
            
            return null;
        }

        /// <include file='doc\SelectionList.uex' path='docs/doc[@for="SelectionList.LoadViewState"]/*' />
        protected override void LoadViewState(Object savedState) 
        {
            if (savedState != null) 
            {
                Object[] state = (Object[])savedState;
                if (state[0] != null)
                {
                    base.LoadViewState(state[0]);
                }
                
                // restore state of items
                Items.SaveSelection = true;
                ((IStateManager)Items).LoadViewState(state[1]);
            }
        }

        private ArrayList SelectedIndicesInternal 
        {
            get 
            {
                int count = Items.Count;
                ArrayList selectedIndices = new ArrayList(count); 
                for (int i = 0; i < count; i++) 
                {
                    if (Items[i].Selected)  
                    {
                        selectedIndices.Add(i);
                    }
                }
                return selectedIndices;
            }
        }

        internal void ClearSelection() 
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Selected = false;
            }
        }

        #region IPostBackDataHandler implementation
        bool IPostBackDataHandler.LoadPostData(String key, NameValueCollection data) {
            return LoadPostData(key, data);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }
        #endregion
    }
}
