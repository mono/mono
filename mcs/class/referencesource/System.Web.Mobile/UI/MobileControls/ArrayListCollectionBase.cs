//------------------------------------------------------------------------------
// <copyright file="ArrayListCollectionBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * ArrayListCollectionBase class. Used as a base class by all collections that
     * use an array list for its contents.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\ArrayListCollectionBase.uex' path='docs/doc[@for="ArrayListCollectionBase"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class ArrayListCollectionBase : ICollection
    {
        private ArrayList _items;

        /// <include file='doc\ArrayListCollectionBase.uex' path='docs/doc[@for="ArrayListCollectionBase.Items"]/*' />
        protected ArrayList Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ArrayList ();
                }
                return _items;
            }

            set
            {
                _items = value;
            }
        }

        internal ArrayListCollectionBase()
        {
        }

        internal ArrayListCollectionBase(ArrayList items)
        {
            _items = items;
        }

        /// <include file='doc\ArrayListCollectionBase.uex' path='docs/doc[@for="ArrayListCollectionBase.Count"]/*' />
        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        /// <include file='doc\ArrayListCollectionBase.uex' path='docs/doc[@for="ArrayListCollectionBase.IsReadOnly"]/*' />
        public bool IsReadOnly
        {
            get
            {
                return Items.IsReadOnly;
            }
        }

        /// <include file='doc\ArrayListCollectionBase.uex' path='docs/doc[@for="ArrayListCollectionBase.IsSynchronized"]/*' />
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <include file='doc\ArrayListCollectionBase.uex' path='docs/doc[@for="ArrayListCollectionBase.SyncRoot"]/*' />
        public Object SyncRoot 
        {
            get 
            {
                return this;
            }
        }

        /// <include file='doc\ArrayListCollectionBase.uex' path='docs/doc[@for="ArrayListCollectionBase.CopyTo"]/*' />
        public void CopyTo(Array array, int index) 
        {
            foreach (Object item in Items)
            {
                array.SetValue (item, index++);
            }
        }

        /// <include file='doc\ArrayListCollectionBase.uex' path='docs/doc[@for="ArrayListCollectionBase.GetEnumerator"]/*' />
        public IEnumerator GetEnumerator()
        {
            return Items.GetEnumerator ();
        }
    }

}


