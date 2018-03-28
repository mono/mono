//------------------------------------------------------------------------------
// <copyright file="PagedControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * Paged control. Abstract control class inherited by all classes 
     * that are internally paginated.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public abstract class PagedControl : MobileControl
    {
        private static readonly Object EventLoadItems = new Object();
        private const String _itemCountViewStateKey = "_!PagedControlItemCount";
        private bool _pagingCharacteristicsChanged = false;
        int _lastItemIndexLoaded = -1;
        int _lastItemCountLoaded = -1;
        Pair _stateLoadItemsArgs = null;

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.LoadItems"]/*' />
        [
            MobileCategory(SR.Category_Action),
            MobileSysDescription(SR.PagedControl_OnLoadItems)
        ]
        public event LoadItemsEventHandler LoadItems
        {
            add
            {
                Events.AddHandler(EventLoadItems, value);
            }
            remove 
            {
                Events.RemoveHandler(EventLoadItems, value);
            }
        }

        private void OnLoadItems()
        {
            int index, count;

            if (_itemPager != null)
            {
                index = PagerItemIndex;
                count = PagerItemCount;
            }
            else
            {
                index = 0;
                count = ItemCount;
            }

            OnLoadItems(new LoadItemsEventArgs(index, count));
        }

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.OnLoadItems"]/*' />
        protected virtual void OnLoadItems(LoadItemsEventArgs e)
        {
            if (LoadItemsHandler != null)
            {
                _lastItemIndexLoaded = e.ItemIndex;
                _lastItemCountLoaded = e.ItemCount;
                LoadItemsHandler(this, e);
            }
        }

        private bool IsCustomPaging
        {
            get
            {
                return ItemCount > 0;
            }
        }

        private LoadItemsEventHandler LoadItemsHandler
        {
            get
            {
                return (LoadItemsEventHandler)Events[EventLoadItems];
            }
        }

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.InternalItemCount"]/*' />
        protected abstract int InternalItemCount { get; }
        

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.ItemCount"]/*' />
        [
            Bindable(true),
            DefaultValue(0),
            MobileCategory(SR.Category_Paging),
            MobileSysDescription(SR.PagedControl_ItemCount)
        ]
        public int ItemCount
        {
            get
            {
                Object count = ViewState[_itemCountViewStateKey];
                return  count == null ? 0 : (int) count;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ItemCount",
                        SR.GetString(SR.PagedControl_ItemCountCantBeNegative,
                                     value));
                }
                bool changed = (ItemCount != value);
                ViewState[_itemCountViewStateKey] = value;
                if (changed)
                {
                    InternalItemCountChangedHandler(value);
                }
                ConditionalLoadItemsFromPersistedArgs();
            }
        }

        // Allows special handling of set_ItemCount in derived class, 
        // while maintaining API backward compatibility.
        internal virtual void InternalItemCountChangedHandler(int newItemCount)
        {}

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.ItemsPerPage"]/*' />
        [
            Bindable(true),
            DefaultValue(0),
            MobileCategory(SR.Category_Paging),
            MobileSysDescription(SR.PagedControl_ItemsPerPage)
        ]
        public int ItemsPerPage
        {
            get
            {
                object o = ViewState["ItemsPerPage"];
                return (o != null) ? (int)o : 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("ItemsPerPage",
                        SR.GetString(SR.PagedControl_ItemsPerPageCantBeNegative,
                                     value));
                }
                ViewState["ItemsPerPage"] = value;
            }
        }

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.ItemWeight"]/*' />
        protected virtual int ItemWeight
        {
            get
            {
                return ControlPager.DefaultWeight;
            }
        }

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.FirstVisibleItemIndex"]/*' />
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public int FirstVisibleItemIndex
        {
            get
            {
                return (IsCustomPaging || !EnablePagination) ? 0 : PagerItemIndex;
            }
        }

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.VisibleItemCount"]/*' />
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public int VisibleItemCount
        {
            get
            {
                return (IsCustomPaging || !EnablePagination) ? InternalItemCount : PagerItemCount;
            }
        }

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.VisibleWeight"]/*' />
        public override int VisibleWeight
        {
            get
            {
                // Paged control should count the weight as sum of its items
                int count = VisibleItemCount;
                if (count == -1)
                {
                    return 0;  // -1 means control not on the current page
                }
                else
                {
                    return count * GetItemWeight();
                }
            }
        }

        private ItemPager _itemPager;
        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.PaginateRecursive"]/*' />
        public override void PaginateRecursive(ControlPager pager)
        {
            int itemCount = IsCustomPaging ? ItemCount : InternalItemCount;
            int itemsPerPage = ItemsPerPage;
            _itemPager = pager.GetItemPager(this, itemCount, itemsPerPage, GetItemWeight());
        }

        internal int GetItemWeight()
        {
            int weight = Adapter.ItemWeight;
            if (weight == ControlPager.UseDefaultWeight)
            {
                weight = ItemWeight;
            }
            return weight;
        }

        private int PagerItemIndex
        {
            get
            {
                return (_itemPager == null) ? 0 : _itemPager.ItemIndex;
            }
        }

        private int PagerItemCount
        {
            get
            {
                return (_itemPager == null) ? InternalItemCount : _itemPager.ItemCount;
            }
        }

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.OnPageChange"]/*' />
        protected override void OnPageChange(int oldPageIndex, int newPageIndex)
        {
            _pagingCharacteristicsChanged = true;
        }

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.OnPreRender"]/*' />
        protected override void OnPreRender(EventArgs e)
        {
            if (IsCustomPaging && 
                IsVisibleOnPage(Form.CurrentPage) &&
                    (!Page.IsPostBack || 
                    Form.PaginationStateChanged || 
                    _pagingCharacteristicsChanged ||
                    !IsViewStateEnabled()))
            {
                OnLoadItems();
            }

            base.OnPreRender(e);
        }

        internal override void SetControlPage(int page)
        {
            // Custom pagination being turned off.

            _itemPager = null;
            _pagingCharacteristicsChanged = true;
            base.SetControlPage(page);
        }

        private new bool IsViewStateEnabled()
        {
            for (Control ctl = this; ctl != null; ctl = ctl.Parent)
            {
                if (!ctl.EnableViewState)
                {
                    return false;
                }
            }
            return true;
        }

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.SavePrivateViewState"]/*' />
        protected override Object SavePrivateViewState()
        {
            // If the base state is non-null, we always return a Pair with the base state as the
            // first item.
            Object baseState = base.SavePrivateViewState();            
            if (IsCustomPaging && IsTemplated && !IsViewStateEnabled() && _lastItemCountLoaded != -1)
            {
                return new Pair(baseState, new Pair(_lastItemIndexLoaded, _lastItemCountLoaded));
            }

            if (baseState != null)
            {
                return new Pair(baseState, null);
            }

            // baseState is null
            return null;
        }

        /// <include file='doc\PagedControl.uex' path='docs/doc[@for="PagedControl.LoadPrivateViewState"]/*' />
        protected override void LoadPrivateViewState(Object state)
        {
            Debug.Assert (state == null || state.GetType() == typeof(Pair), 
               "If the base state is non-null, private viewstate should always be saved as a pair");

            Pair statePair = state as Pair;
            if (statePair != null)
            {
                base.LoadPrivateViewState(statePair.First); 
                _stateLoadItemsArgs = statePair.Second as Pair;
                ConditionalLoadItemsFromPersistedArgs();
            }
        }

        // Loads items using view state pair in templated case where custom paging 
        // on, view state off.  In this special case, load items early as 
        // possible to enable events to be raised.
        private void ConditionalLoadItemsFromPersistedArgs()
        {
            if (_stateLoadItemsArgs != null && IsCustomPaging && IsTemplated && !IsViewStateEnabled())
            {
                OnLoadItems(
                    new LoadItemsEventArgs((int) _stateLoadItemsArgs.First, (int) _stateLoadItemsArgs.Second));
                _stateLoadItemsArgs = null;
            }
        }
    }
}
