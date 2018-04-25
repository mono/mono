//------------------------------------------------------------------------------
// <copyright file="ItemPager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Item pager, a class that provides state as items of a control are paginated.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ItemPager.uex' path='docs/doc[@for="ItemPager"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ItemPager
    {

        private MobileControl _control;
        private int _firstPageItemCount;
        private int _fullPageItemCount;
        private int _lastPageItemCount;
        private int _firstPage;
        private int _lastPage;

        /// <include file='doc\ItemPager.uex' path='docs/doc[@for="ItemPager.ItemPager"]/*' />
        public ItemPager()
        {
        }

        /// <include file='doc\ItemPager.uex' path='docs/doc[@for="ItemPager.ItemPager1"]/*' />
        public ItemPager(ControlPager pager, MobileControl control, int itemCount, int itemsPerPage, int itemWeight)
        {
            _control = control;

            if (itemsPerPage > 0)
            {
                // User-specified pagination behavior, always given
                // number of items per page.

                if (itemCount < itemsPerPage)
                {
                    _firstPageItemCount = itemCount;
                    _firstPage = _lastPage = pager.GetPage(itemCount * itemWeight);
                }
                else
                {
                    int numberOfPages = (itemCount - 1) / itemsPerPage + 1;
                    _firstPageItemCount = itemsPerPage;
                    _fullPageItemCount = itemsPerPage;
                    _lastPageItemCount = itemCount - (numberOfPages - 1) * itemsPerPage;
                    _firstPage = pager.GetPage(itemsPerPage * itemWeight);
                    pager.PageCount += numberOfPages - 1;
                    if (numberOfPages > 1)
                    {
                        pager.RemainingWeight = Math.Max(0, pager.PageWeight - _lastPageItemCount * itemWeight);
                    }
                    _lastPage = _firstPage + numberOfPages - 1;
                }
            }
            else
            {
                int totalItemWeight = itemCount * itemWeight;
                if (totalItemWeight <= pager.RemainingWeight)
                {
                    _firstPageItemCount = itemCount;
                    _firstPage = _lastPage = pager.GetPage(totalItemWeight);
                }
                else
                {
                    _firstPageItemCount = pager.RemainingWeight / itemWeight;
                    int remainingItemCount = itemCount - _firstPageItemCount;
                    _fullPageItemCount  = Math.Max(1, pager.PageWeight / itemWeight);
                    int fullPageCount = remainingItemCount / _fullPageItemCount;
                    _lastPageItemCount  = remainingItemCount % _fullPageItemCount;
    
                    _firstPage = pager.PageCount;
    
                    //  increment for first page
                    pager.PageCount++;
                    pager.RemainingWeight = pager.PageWeight;
    
                    //  increment for full pages
                    pager.PageCount += fullPageCount;
    
                    //  remove remaining weight for last page
                    pager.RemainingWeight -= _lastPageItemCount * itemWeight;
    
                    //  correct if first page is empty
                    if (_firstPageItemCount == 0)
                    {
                        _firstPage++;
                        _firstPageItemCount = Math.Min(_fullPageItemCount, itemCount);
                    }
                    //  correct if last page is empty
                    if (_lastPageItemCount == 0)
                    {
                        pager.PageCount--;
                        _lastPageItemCount = Math.Min(_fullPageItemCount, itemCount);
                        pager.RemainingWeight = 0;
                    }
                    _lastPage = pager.PageCount;
                }
            }
            _control.FirstPage = _firstPage;
            _control.LastPage = _lastPage;
        }

        /// <include file='doc\ItemPager.uex' path='docs/doc[@for="ItemPager.ItemIndex"]/*' />
        public int ItemIndex
        {
            get
            {
                int page = _control.Form.CurrentPage;
                if (page < _firstPage || page > _lastPage)
                {
                    return -1;
                }
                if (page == _firstPage)
                {
                    return 0;
                }
                else
                {
                    int fullPageCount = (page - _firstPage) - 1;
                    return fullPageCount * _fullPageItemCount + _firstPageItemCount;
                }
            }
        }

        /// <include file='doc\ItemPager.uex' path='docs/doc[@for="ItemPager.ItemCount"]/*' />
        public int ItemCount
        {
            get
            {
                int page = _control.Form.CurrentPage;
                if (page < _firstPage || page > _lastPage)
                {
                    return -1;
                }
                if (page == _firstPage)
                {
                    return _firstPageItemCount;
                }
                else if (page == _lastPage)
                {
                    return _lastPageItemCount;
                }
                else
                {
                    return _fullPageItemCount;
                }
            }
        }

    }

}
