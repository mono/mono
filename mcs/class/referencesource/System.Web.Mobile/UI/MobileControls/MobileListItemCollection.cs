//------------------------------------------------------------------------------
// <copyright file="MobileListItemCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile List Item collection class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class MobileListItemCollection : ArrayListCollectionBase, IStateManager
    {
        private bool _marked = false;
        private bool _saveAll = false;
        private bool _saveSelection = false;
        private int _baseIndex = 0;

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.MobileListItemCollection"]/*' />
        public MobileListItemCollection()
        {
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.MobileListItemCollection1"]/*' />
        public MobileListItemCollection(ArrayList items) : base(items)
        {
        }

        internal int BaseIndex
        {
            get
            {
                return _baseIndex;
            }

            set
            {
                _baseIndex = value;
            }
        }

        internal bool SaveSelection {
            get {
                return _saveSelection;
            }
            set {
                _saveSelection = value;
            }
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.GetAll"]/*' />
        public MobileListItem[] GetAll()
        {
            int n = Count;
            MobileListItem[] result = new MobileListItem[n];
            if (n > 0) 
            {
                Items.CopyTo (0, result, 0, n);
            }
            return result;
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.SetAll"]/*' />
        public void SetAll(MobileListItem[] value)
        {
            Items = new ArrayList (value);
            if (_marked)
            {
                _saveAll = true;
            }

            int count = Count;
            for (int i = 0; i < count; i++)
            {
                MobileListItem item = this[i];
                item.SetIndex(i + BaseIndex);
                if (_marked)
                {
                    item.Dirty = true;
                }
            }
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.this"]/*' />
        public MobileListItem this[int index]
        {
            get
            {
                return (MobileListItem)Items[index];
            }
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.Add"]/*' />
        public void Add(MobileListItem item)
        {
            item.SetIndex(Items.Count + BaseIndex);
            Items.Add (item);
            if (_marked)
            {
                item.Dirty = true;
            }
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.Add1"]/*' />
        public virtual void Add(String item)
        {
            Add (new MobileListItem (item));
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.Clear"]/*' />
        public void Clear()
        {
            Items.Clear ();
            if (_marked)
            {
                _saveAll = true;
            }
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.Contains"]/*' />
        public bool Contains(MobileListItem item)
        {
            return Items.Contains (item);
        }


        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.IndexOf"]/*' />
        public int IndexOf(MobileListItem item)
        {
            return Items.IndexOf(item);
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.Insert"]/*' />
        public virtual void Insert(int index, String item) 
        {
            Insert (index, new MobileListItem (item));
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.Insert1"]/*' />
        public void Insert(int index, MobileListItem item) 
        {
            Items.Insert (index, item);
            for (int i = index; i < Items.Count; i++)
            {
                ((MobileListItem)Items[i]).SetIndex(i + BaseIndex);
            }
            if (_marked)
            {
                _saveAll = true;
            }
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.RemoveAt"]/*' />
        public void RemoveAt(int index) 
        {
            Items.RemoveAt (index);
            for (int i = index; i < Items.Count; i++)
            {
                ((MobileListItem)Items[i]).SetIndex(i + BaseIndex);
            }
            if (_marked)
            {
                _saveAll = true;
            }
        }
    
        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.Remove"]/*' />
        public virtual void Remove(String item) 
        {
            int index = IndexOf (new MobileListItem(item));
            if (index >= 0) 
            {
                RemoveAt (index);
            }
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.Remove1"]/*' />
        public void Remove(MobileListItem item) 
        {
            int index = IndexOf(item);
            if (index >= 0) 
            {
                RemoveAt(index);
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  STATE MANAGEMENT
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.IStateManager.IsTrackingViewState"]/*' />
        /// <internalonly/>
        protected bool IsTrackingViewState
        {
            get
            {
                return _marked;
            }
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.IStateManager.TrackViewState"]/*' />
        /// <internalonly/>
        protected void TrackViewState() 
        {
            _marked = true;
            foreach (IStateManager item in Items)
            {
                item.TrackViewState();
            }
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.IStateManager.LoadViewState"]/*' />
        /// <internalonly/>
        protected void LoadViewState(Object state) 
        {
            if (state != null)
            {
                Object[] changes = (Object[]) state;
                int length = changes.Length;

                if (length == 5)
                {
                    int count = (int)changes[0];
                    if (count < Count)
                    {
                        throw new Exception(
                            SR.GetString(SR.MobileListItemCollection_ViewStateManagementError));
                    }
                    EnsureCount ((int)changes[0]);
                    int[] changeIndices = (int[])changes[1];
                    Object[] itemChanges = (Object[])changes[2];
                    for (int i = 0; i < changeIndices.Length; i++)
                    {
                        ((IStateManager)Items[changeIndices[i]]).LoadViewState (itemChanges[i]);
                    }
                }
                else if (length == 3)
                {
                    if (changes[0] == null)
                    {
                        Clear ();
                    }
                    else
                    {
                        Object[] itemChanges = (Object[])changes[0];
                        EnsureCount (itemChanges.Length);
                        int i = 0;
                        foreach (IStateManager item in Items)
                        {
                            item.LoadViewState (itemChanges[i++]);
                        }
                    }
                }

                if (length >= 2 && changes[length - 2] != null && SaveSelection)
                {
                    bool[] selection = (bool[])changes[length - 2];
                    for (int i = selection.Length - 1; i >= 0; i--)
                    {
                        ((MobileListItem)Items[i]).Selected = selection[i];
                    }
                }

                int oldBaseIndex = BaseIndex;
                BaseIndex = (int)changes[length - 1];
                if (oldBaseIndex != BaseIndex)
                {
                    int index = BaseIndex;
                    foreach (MobileListItem item in Items)
                    {
                        item.SetIndex(index++);
                    }
                }
            }
        }

        /// <include file='doc\MobileListItemCollection.uex' path='docs/doc[@for="MobileListItemCollection.IStateManager.SaveViewState"]/*' />
        /// <internalonly/>
        protected Object SaveViewState() 
        {
            Object[] changes;
            int changedCount = 0;
            bool selectionNeedsSaving = false;

            if (!_saveAll)
            {
                foreach (MobileListItem item in Items)
                {
                    if (item.Dirty)
                    {
                        changedCount++;
                    }

                    if (SaveSelection && item.SelectionDirty)
                    {
                        selectionNeedsSaving = true;
                    }
                }
                if (changedCount == Count)
                {
                    _saveAll = true;
                }
                else if (changedCount == 0 && !selectionNeedsSaving && BaseIndex == 0)
                {
                    return null;
                }
            }

            if (_saveAll)
            {
                // Save all items.

                Object[] itemChanges;
                if (Count > 0)
                {
                    itemChanges = new Object[Count];
                    int i = 0;
                    foreach (MobileListItem item in Items)
                    {
                        item.Dirty = true;
                        itemChanges[i++] = ((IStateManager)item).SaveViewState ();
                    }
                }
                else
                {
                    itemChanges = null;
                }

                changes = new Object[3];
                changes[0] = itemChanges;
                selectionNeedsSaving = true;
            }
            else if (changedCount == 0)
            {
                changes = new Object[selectionNeedsSaving ? 2 : 1];
            }
            else
            {
                // Save changed items.

                int[] changeIndices = new int[changedCount];
                Object[] itemChanges = new Object[changedCount];

                changedCount = 0;
                int i = 0;
                foreach (MobileListItem item in Items)
                {
                    if (item.Dirty)
                    {
                        changeIndices[changedCount] = i;
                        itemChanges[changedCount] = ((IStateManager)item).SaveViewState ();
                        changedCount++;
                    }
                    i++;
                }

                changes = new Object[5];
                changes[0] = (int)Count;
                changes[1] = changeIndices;
                changes[2] = itemChanges;
                changes[3] = null;
            }

            if (selectionNeedsSaving)
            {
                bool[] selection = new bool[Count];
                int i = 0;
                foreach (MobileListItem item in Items)
                {
                    selection[i++] = item.Selected;
                }
                changes[changes.Length - 2] = selection;
            }

            changes[changes.Length - 1] = BaseIndex;
            return changes;
        }

        private void EnsureCount(int count)
        {
            int diff = Count - count;
            if (diff > 0)
            {
                Items.RemoveRange (count, diff);
                if (_marked)
                {
                    _saveAll = true;
                }
            }
            else
            {
                for (int i = Count; i < count; i++)
                {
                    MobileListItem item = new MobileListItem ();
                    item.SetIndex(i + BaseIndex);
                    Add (item);
                }
            }
        }

        #region Implementation of IStateManager
        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }

        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }

        /// <internalonly/>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }
        #endregion
    } 
}

