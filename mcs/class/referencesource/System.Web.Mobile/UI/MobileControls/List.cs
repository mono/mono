//------------------------------------------------------------------------------
// <copyright file="List.cs" company="Microsoft">
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
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Mobile List class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\List.uex' path='docs/doc[@for="List"]/*' />
    [
        ControlBuilderAttribute(typeof(ListControlBuilder)),
        DefaultEvent("ItemCommand"),
        DefaultProperty("DataSource"),
        Designer(typeof(System.Web.UI.Design.MobileControls.ListDesigner)),
        DesignerAdapter(typeof(System.Web.UI.Design.MobileControls.Adapters.DesignerListAdapter)),
        Editor(typeof(System.Web.UI.Design.MobileControls.ListComponentEditor), typeof(ComponentEditor)),
        ToolboxData("<{0}:List runat=\"server\"></{0}:List>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign),
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class List : PagedControl, INamingContainer, IListControl, ITemplateable, IPostBackEventHandler
    {
        private static readonly Object EventItemCommand = new Object();
        private static readonly Object EventItemDataBind = new Object();

        private ListDataHelper _dataHelper;
        private ListDecoration _decoration = ListDecoration.None;

        private ListDataHelper DataHelper {
            get {
                if (_dataHelper == null) {
                    _dataHelper = new ListDataHelper(this, ViewState);
                }

                return _dataHelper;
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.DataSource"]/*' />
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

        /// <include file='doc\List.uex' path='docs/doc[@for="List.DataMember"]/*' />
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

        /// <include file='doc\List.uex' path='docs/doc[@for="List.DataTextField"]/*' />
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

        /// <include file='doc\List.uex' path='docs/doc[@for="List.DataValueField"]/*' />
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


        /// <include file='doc\List.uex' path='docs/doc[@for="List.ItemsAsLinks"]/*' />
        [
            Bindable(true),
            DefaultValue(false),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.List_ItemsAsLinks)
        ]
        public bool ItemsAsLinks
        {
            get
            {
                Object b = ViewState["ItemsAsLinks"];
                return (b != null) ? (bool) b : false;
            }
            set
            {
                ViewState["ItemsAsLinks"] = value;
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.Items"]/*' />
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

        /// <include file='doc\List.uex' path='docs/doc[@for="List.EnsureTemplatedUI"]/*' />
        public override void EnsureTemplatedUI()
        {
            EnsureChildControls();
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.AddParsedSubObject"]/*' />
        protected override void AddParsedSubObject(Object obj)
        {
            if (!(obj is LiteralControl))
            {
                if (obj is MobileListItem)
                {
                    DataHelper.AddItem((MobileListItem)obj);
                } 
                else
                {
                    base.AddParsedSubObject(obj);
                }
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.CreateChildControls"]/*' />
        protected override void CreateChildControls() 
        {
            CreateChildControls(false);
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.EnsureChildControls"]/*' />
        protected override void EnsureChildControls()
        {
            // Whenever EnsureChildControls is called before items are created 
            // (e.g., at LoadPrivateViewState), there are no controls.  
            // Rebuild children for this case by setting ChildControlsCreated to false.
            ChildControlsCreated = ChildControlsCreated && Controls.Count > 0;
            base.EnsureChildControls();
        }

        private void CreateChildControls(bool doDataBind)
        {
            if (IsTemplated && DataHelper.HasItems() && Items.Count > 0)
            {
                Controls.Clear();
                CreateTemplatedUI(doDataBind);
            }
            ChildControlsCreated = true;
        }
        
        /// <include file='doc\List.uex' path='docs/doc[@for="List.OnDataBinding"]/*' />
        protected override void OnDataBinding(EventArgs e) 
        {
            base.OnDataBinding(e);
            if (DataHelper.ResolvedDataSource != null)
            {
                Controls.Clear();
                ClearChildViewState();
                CreateItems(DataHelper.ResolvedDataSource);
                CreateChildControls(true);
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.CreateItems"]/*' />
        protected virtual void CreateItems(IEnumerable dataSource) 
        {
            DataHelper.CreateItems(dataSource);
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.CreateDefaultTemplatedUI"]/*' />
        public override void CreateDefaultTemplatedUI(bool doDataBind) 
        {
            MobileListItemCollection items = Items;

            ITemplate headerTemplate = GetTemplate(Constants.HeaderTemplateTag);
            ITemplate footerTemplate = GetTemplate(Constants.FooterTemplateTag);
            ITemplate itemTemplate = GetTemplate(Constants.ItemTemplateTag);
            ITemplate separatorTemplate = GetTemplate(Constants.SeparatorTemplateTag);
            ITemplate alternatingItemTemplate = GetTemplate(Constants.AlternatingItemTemplateTag);
            if (alternatingItemTemplate == null)
            {
                alternatingItemTemplate = itemTemplate;
            }

            CreateControlItem(MobileListItemType.HeaderItem, 
                              headerTemplate,
                              doDataBind);
            for (int i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    CreateControlItem(MobileListItemType.SeparatorItem, 
                                      separatorTemplate, 
                                      doDataBind);
                }
                AddItemAsControl(i, 
                                 items[i], 
                                 ((i & 1) == 1) ? alternatingItemTemplate : itemTemplate, 
                                 doDataBind);
            }
            CreateControlItem(MobileListItemType.FooterItem, 
                              footerTemplate,
                              doDataBind);
        }

        private void AddItemAsControl(
            int itemIndex,
            MobileListItem item,
            ITemplate itemTemplate, 
            bool doDataBind)
        {
            if (itemTemplate != null)
            {
                item.Controls.Clear();
                CheckedInstantiateTemplate (itemTemplate, item, this);
                Controls.Add(item);
                if (doDataBind)
                {
                    item.DataBind();
                }
            }
        }

        private void CreateControlItem(
            MobileListItemType itemType, 
            ITemplate itemTemplate, 
            bool doDataBind)
        {
            if (itemTemplate != null)
            {
                MobileListItem item = new MobileListItem(itemType);
                AddItemAsControl(-1, item, itemTemplate, doDataBind);
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.OnPreRender"]/*' />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (IsTemplated)
            {
                int firstVisibleItem = FirstVisibleItemIndex;
                int pageSize = VisibleItemCount;
                int lastVisibleItem = firstVisibleItem + pageSize - 1;
                int itemIndex = 0;
                int separatorIndex = 0;
                foreach(MobileListItem item in Controls)
                {
                    if (item.ItemType == MobileListItemType.ListItem)
                    {
                        item.Visible = itemIndex >= firstVisibleItem && itemIndex <= lastVisibleItem;
                        itemIndex++;
                    }
                    else if (item.ItemType == MobileListItemType.SeparatorItem)
                    {
                        item.Visible = separatorIndex >= firstVisibleItem && 
                                            separatorIndex < lastVisibleItem;
                        separatorIndex++;
                    }
                }
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.InternalItemCount"]/*' />
        protected override int InternalItemCount
        {
            get
            {
                if (DataHelper.HasItems())
                {
                    Debug.Assert (Items.Count >= 0);
                    return Items.Count;
                }
                else
                {
                    return 0;
                }
            }
            
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.OnPageChange"]/*' />
        protected override void OnPageChange(int oldPageIndex, int newPageIndex)
        {
            base.OnPageChange(oldPageIndex, newPageIndex);
        }

        /////////////////////////////////////////////////////////////////////////
        //  EVENT HANDLING
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\List.uex' path='docs/doc[@for="List.ItemCommand"]/*' />
        [
            MobileCategory(SR.Category_Action),
            MobileSysDescription(SR.List_OnItemCommand)
        ]
        public event ListCommandEventHandler ItemCommand 
        {
            add 
            {
                Events.AddHandler(EventItemCommand, value);
            }
            remove 
            {
                Events.RemoveHandler(EventItemCommand, value);
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.OnItemCommand"]/*' />
        protected virtual void OnItemCommand(ListCommandEventArgs e) 
        {
            ListCommandEventHandler onItemCommandHandler = (ListCommandEventHandler)Events[EventItemCommand];
            if (onItemCommandHandler != null)
            {
                onItemCommandHandler(this, e);
            }
        }


        /// <include file='doc\List.uex' path='docs/doc[@for="List.HasItemCommandHandler"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool HasItemCommandHandler
        {
            get
            {
                return Events[EventItemCommand] != null;
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.Decoration"]/*' />
        [
            Bindable(true),
            DefaultValue(ListDecoration.None),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.List_Decoration)
        ]
        public ListDecoration Decoration
        {
            get
            {
                return _decoration;
            }
            set
            {
                _decoration = value;
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.ItemDataBind"]/*' />
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

        /// <include file='doc\List.uex' path='docs/doc[@for="List.OnItemDataBind"]/*' />
        protected virtual void OnItemDataBind(ListDataBindEventArgs e) 
        {
            ListDataBindEventHandler onItemDataBindHandler = 
                (ListDataBindEventHandler)Events[EventItemDataBind];
            if (onItemDataBindHandler != null)
            {
                onItemDataBindHandler(this, e);
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.OnBubbleEvent"]/*' />
        protected override bool OnBubbleEvent(Object sender, EventArgs e) 
        {
            bool handled = false;

            if (e is ListCommandEventArgs) 
            {
                OnItemCommand((ListCommandEventArgs)e);
                handled = true;
            }

            return handled;
        }

        /// <internalonly/>
        protected void RaisePostBackEvent(String eventArgument)
        {
            if(!ItemsAsLinks)
            {
                // Non-templated list got a click event. There really
                // isn't a command source, so we'll set it to the 
                // default of null.
                int item = Int32.Parse(eventArgument, CultureInfo.InvariantCulture);
                item = TranslateVirtualItemIndex(item);
                OnItemCommand(new ListCommandEventArgs(Items[item], null));
            }
            else
            {
                MobilePage.ActiveForm = MobilePage.GetForm(eventArgument);
            }
        }

        private int TranslateVirtualItemIndex(int itemIndex)
        {
            // Ensure that the item of the given virtual index is loaded,
            // and translate as necessary to an index within the collection.

            if (ItemCount > 0)
            {
                if (DataHelper.HasItems() && Items.Count > itemIndex - Items.BaseIndex)
                {
                    itemIndex -= Items.BaseIndex;
                }
                else
                {
                    OnLoadItems(new LoadItemsEventArgs(itemIndex, 1));
                    itemIndex = 0;
                }
            }

            return itemIndex;
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.OnLoadItems"]/*' />
        protected override void OnLoadItems(LoadItemsEventArgs e)
        {
            // We should only load items if the base index has changed, or if
            // the desired items do not exist in the list. Otherwise, we are making
            // the app reload the same items over and over.

            if (e.ItemIndex != Items.BaseIndex || e.ItemCount != Items.Count)
            {
                Items.BaseIndex = e.ItemIndex;
                Items.Clear();
                base.OnLoadItems(e);
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  STATE MANAGEMENT
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\List.uex' path='docs/doc[@for="List.TrackViewState"]/*' />
        protected override void TrackViewState() 
        {
            base.TrackViewState();
            if (DataHelper.HasItems())
            {
                ((IStateManager)Items).TrackViewState();
            }
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.SaveViewState"]/*' />
        protected override Object SaveViewState() 
        {
            Object baseState, itemsState;

            if (DataHelper.HasItems())
            {
                itemsState = ((IStateManager)Items).SaveViewState();
            }
            else
            {
                itemsState = null;
            }
            baseState = base.SaveViewState();

            if (itemsState != null || Decoration != ListDecoration.None )
            {
                return new Object[3] { baseState, itemsState, Decoration };
            }
            else if (baseState != null)
            {
                return new Object[1] { baseState };
            }
            return null;
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.LoadViewState"]/*' />
        protected override void LoadViewState(Object savedState) 
        {
            if (savedState != null) 
            {
                Object[] state = (Object[])savedState;
                base.LoadViewState(state[0]);

                if (state.Length > 1)
                {
                    ((IStateManager)Items).LoadViewState(state[1]);
                    Decoration = (ListDecoration) state[2];
                }
            }
        }

        internal override void InternalItemCountChangedHandler(int newItemCount)
        {
            if (newItemCount == 0)
            {
                Items.Clear();
                Controls.Clear();
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  IListControl
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\List.uex' path='docs/doc[@for="List.IListControl.OnItemDataBind"]/*' />
        /// <internalonly/>
        void IListControl.OnItemDataBind(ListDataBindEventArgs e) 
        {
            OnItemDataBind(e);
        }

        /// <include file='doc\List.uex' path='docs/doc[@for="List.IListControl.TrackingViewState"]/*' />
        /// <internalonly/>
        bool IListControl.TrackingViewState
        {
            get
            {
                return IsTrackingViewState;
            }
        }

        #region IPostBackEventHandler implementation
        void IPostBackEventHandler.RaisePostBackEvent(String eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion 
    }
}
