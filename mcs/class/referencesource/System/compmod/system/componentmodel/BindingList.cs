//------------------------------------------------------------------------------
// <copyright file="BindingList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope="type", Target="System.ComponentModel.BindingList`1")]

namespace System.ComponentModel
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;
    using CodeAccessPermission = System.Security.CodeAccessPermission;

    /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList"]/*' />
    /// <devdoc>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [Serializable]
    public class BindingList<T> : Collection<T>, IBindingList, ICancelAddNew, IRaiseItemChangedEvents
    {
        private int addNewPos = -1;
        private bool raiseListChangedEvents = true;
        private bool raiseItemChangedEvents = false;
        
        [NonSerialized()]
        private PropertyDescriptorCollection itemTypeProperties = null;

        [NonSerialized()]
        private PropertyChangedEventHandler propertyChangedEventHandler = null;

        [NonSerialized()]
        private AddingNewEventHandler onAddingNew;

        [NonSerialized()]
        private ListChangedEventHandler onListChanged;

        [NonSerialized()]
        private int lastChangeIndex = -1;

        private bool allowNew = true;
        private bool allowEdit = true;
        private bool allowRemove = true;
        private bool userSetAllowNew = false;

        #region Constructors

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.BindingList"]/*' />
        /// <devdoc>
        ///     Default constructor.
        /// </devdoc>
        public BindingList() : base() {
            Initialize();
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.BindingList1"]/*' />
        /// <devdoc>
        ///     Constructor that allows substitution of the inner list with a custom list.
        /// </devdoc>
        public BindingList(IList<T> list) : base(list) {
            Initialize();
        }

        private void Initialize() {
            // Set the default value of AllowNew based on whether type T has a default constructor
            this.allowNew = ItemTypeHasDefaultConstructor;

            // Check for INotifyPropertyChanged
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T))) {
                // Supports INotifyPropertyChanged
                this.raiseItemChangedEvents = true;

                // Loop thru the items already in the collection and hook their change notification.
                foreach (T item in this.Items) {
                    HookPropertyChanged(item);
                }
            }
        }

        private bool ItemTypeHasDefaultConstructor {
            get {
                Type itemType = typeof(T);

                if (itemType.IsPrimitive) {
                    return true;
                }

                if (itemType.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, new Type[0], null) != null) {
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region AddingNew event

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.AddingNew"]/*' />
        /// <devdoc>
        ///     Event that allows a custom item to be provided as the new item added to the list by AddNew().
        /// </devdoc>
        public event AddingNewEventHandler AddingNew {
            add {
                bool allowNewWasTrue = AllowNew;
                onAddingNew += value;
                if (allowNewWasTrue != AllowNew) {
                    FireListChanged(ListChangedType.Reset, -1);
                }
            }
            remove {
                bool allowNewWasTrue = AllowNew;
                onAddingNew -= value;
                if (allowNewWasTrue != AllowNew) {
                    FireListChanged(ListChangedType.Reset, -1);
                }
            }
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.OnAddingNew"]/*' />
        /// <devdoc>
        ///     Raises the AddingNew event.
        /// </devdoc>
        protected virtual void OnAddingNew(AddingNewEventArgs e) {
            if (onAddingNew != null) {
                onAddingNew(this, e);
            }
        }

        // Private helper method
        private object FireAddingNew() {
            AddingNewEventArgs e = new AddingNewEventArgs(null);
            OnAddingNew(e);
            return e.NewObject;
        }

        #endregion

        #region ListChanged event

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.ListChanged"]/*' />
        /// <devdoc>
        ///     Event that reports changes to the list or to items in the list.
        /// </devdoc>
        public event ListChangedEventHandler ListChanged {
            add {
                onListChanged += value;
            }
            remove {
                onListChanged -= value;
            }
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.OnListChanged"]/*' />
        /// <devdoc>
        ///     Raises the ListChanged event.
        /// </devdoc>
        protected virtual void OnListChanged(ListChangedEventArgs e) {
            if (onListChanged != null) {
                onListChanged(this, e);
            }
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.RaiseListChangedEvents"]/* />
        public bool RaiseListChangedEvents {
            get {
                return this.raiseListChangedEvents;
            }

            set {
                if (this.raiseListChangedEvents != value) {
                    this.raiseListChangedEvents = value;
                }
            }
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.ResetBindings"]/*' />
        /// <devdoc>
        /// </devdoc>
        public void ResetBindings() {
            FireListChanged(ListChangedType.Reset, -1);
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.ResetItem"]/*' />
        /// <devdoc>
        /// </devdoc>
        public void ResetItem(int position) {
            FireListChanged(ListChangedType.ItemChanged, position);
        }

        // Private helper method
        private void FireListChanged(ListChangedType type, int index) {
            if (this.raiseListChangedEvents) {
                OnListChanged(new ListChangedEventArgs(type, index));
            }
        }

        #endregion

        #region Collection<T> overrides

        // Collection<T> funnels all list changes through the four virtual methods below.
        // We override these so that we can commit any pending new item and fire the proper ListChanged events.

        protected override void ClearItems() {
            EndNew(addNewPos);

            if (this.raiseItemChangedEvents) {
                foreach (T item in this.Items) {
                    UnhookPropertyChanged(item);
                }
            }

            base.ClearItems();
            FireListChanged(ListChangedType.Reset, -1);
        }

        protected override void InsertItem(int index, T item) {
            EndNew(addNewPos);
            base.InsertItem(index, item);

            if (this.raiseItemChangedEvents) {
                HookPropertyChanged(item);
            }

            FireListChanged(ListChangedType.ItemAdded, index);
        }
        
        protected override void RemoveItem(int index) {
            // Need to all RemoveItem if this on the AddNew item
            if (!this.allowRemove && !(this.addNewPos >= 0 && this.addNewPos == index)) {
                throw new NotSupportedException();
            }

            EndNew(addNewPos);

            if (this.raiseItemChangedEvents) {
                UnhookPropertyChanged(this[index]);
            }

            base.RemoveItem(index);
            FireListChanged(ListChangedType.ItemDeleted, index);
        }

        protected override void SetItem(int index, T item) {

            if (this.raiseItemChangedEvents) {
                UnhookPropertyChanged(this[index]);
            }

            base.SetItem(index, item);
            
            if (this.raiseItemChangedEvents) {
                HookPropertyChanged(item);
            }
            
            FireListChanged(ListChangedType.ItemChanged, index);
        }
        
        #endregion

        #region ICancelAddNew interface

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.CancelNew"]/*' />
        /// <devdoc>
        ///     If item added using AddNew() is still cancellable, then remove that item from the list.
        /// </devdoc>
        public virtual void CancelNew(int itemIndex)
        {
            if (addNewPos >= 0 && addNewPos == itemIndex) {
                RemoveItem(addNewPos);
                addNewPos = -1;
            }
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.EndNew"]/*' />
        /// <devdoc>
        ///     If item added using AddNew() is still cancellable, then commit that item.
        /// </devdoc>
        public virtual void EndNew(int itemIndex)
        {
            if (addNewPos >= 0 && addNewPos == itemIndex) {
                addNewPos = -1;
            }
        }

        #endregion

        #region IBindingList interface

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.AddNew"]/*' />
        /// <devdoc>
        ///     Adds a new item to the list. Calls <see cref='AddNewCore'> to create and add the item.
        ///
        ///     Add operations are cancellable via the <see cref='ICancelAddNew'> interface. The position of the
        ///     new item is tracked until the add operation is either cancelled by a call to <see cref='CancelNew'>,
        ///     explicitly commited by a call to <see cref='EndNew'>, or implicitly commmited some other operation
        ///     that changes the contents of the list (such as an Insert or Remove). When an add operation is
        ///     cancelled, the new item is removed from the list.
        /// </devdoc>
        public T AddNew() {
            return (T)((this as IBindingList).AddNew());
        }

        object IBindingList.AddNew() {
            // Create new item and add it to list
            object newItem = AddNewCore();

            // Record position of new item (to support cancellation later on)
            addNewPos = (newItem != null) ? IndexOf((T) newItem) : -1;

            // Return new item to caller
            return newItem;
        }

        private bool AddingNewHandled {
            get {
                return onAddingNew != null && onAddingNew.GetInvocationList().Length > 0;
            }
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.AddNewCore"]/*' />
        /// <devdoc>
        ///     Creates a new item and adds it to the list.
        ///
        ///     The base implementation raises the AddingNew event to allow an event handler to
        ///     supply a custom item to add to the list. Otherwise an item of type T is created.
        ///     The new item is then added to the end of the list.
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2113:SecureLateBindingMethods")]
        protected virtual object AddNewCore() {
            // Allow event handler to supply the new item for us
            object newItem = FireAddingNew();

            // If event hander did not supply new item, create one ourselves
            if (newItem == null) {

                Type type = typeof(T);                
                newItem = SecurityUtils.SecureCreateInstance(type);
            }

            // Add item to end of list. Note: If event handler returned an item not of type T,
            // the cast below will trigger an InvalidCastException. This is by design.
            Add((T) newItem);

            // Return new item to caller
            return newItem;
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.AllowNew"]/*' />
        /// <devdoc>
        /// </devdoc>
        public bool AllowNew {
            get {
                //If the user set AllowNew, return what they set.  If we have a default constructor, allowNew will be 
                //true and we should just return true.
                if (userSetAllowNew || allowNew)
                {
                    return this.allowNew;
                }
                //Even if the item doesn't have a default constructor, the user can hook AddingNew to provide an item.
                //If there's a handler for this, we should allow new.
                return AddingNewHandled;
            }
            set {
                bool oldAllowNewValue = AllowNew;
                userSetAllowNew = true;
                //Note that we don't want to set allowNew only if AllowNew didn't match value,
                //since AllowNew can depend on onAddingNew handler
                this.allowNew = value;
                if (oldAllowNewValue != value) {
                    FireListChanged(ListChangedType.Reset, -1);
                }
            }
        }

        /* private */ bool IBindingList.AllowNew {
            get {
                return AllowNew;
            }
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.AllowEdit"]/*' />
        /// <devdoc>
        /// </devdoc>
        public bool AllowEdit {
            get {
                return this.allowEdit;
            }
            set {
                if (this.allowEdit != value) {
                    this.allowEdit = value;
                    FireListChanged(ListChangedType.Reset, -1);
                }
            }
        }

        /* private */ bool IBindingList.AllowEdit {
            get {
                return AllowEdit;
            }
        }

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.AllowRemove"]/*' />
        /// <devdoc>
        /// </devdoc>
        public bool AllowRemove {
            get {
                return this.allowRemove;
            }
            set {
                if (this.allowRemove != value) {
                    this.allowRemove = value;
                    FireListChanged(ListChangedType.Reset, -1);
                }
            }
        }

        /* private */ bool IBindingList.AllowRemove {
            get {
                return AllowRemove;
            }
        }

        bool IBindingList.SupportsChangeNotification {
            get {
                return SupportsChangeNotificationCore;
            }
        }

        protected virtual bool SupportsChangeNotificationCore {
            get {
                return true;
            }
        }

        bool IBindingList.SupportsSearching {
            get {
                return SupportsSearchingCore;
            }
        }

        protected virtual bool SupportsSearchingCore {
            get {
                return false;
            }
        }

        bool IBindingList.SupportsSorting {
            get {
                return SupportsSortingCore;
            }
        }

        protected virtual bool SupportsSortingCore {
            get {
                return false;
            }
        }

        bool IBindingList.IsSorted {
            get {
                return IsSortedCore;
            }
        }

        protected virtual bool IsSortedCore {
            get {
                return false;
            }
        }

        PropertyDescriptor IBindingList.SortProperty {
            get {
                return SortPropertyCore;
            }
        }

        protected virtual PropertyDescriptor SortPropertyCore {
            get {
                return null;
            }
        }

        ListSortDirection IBindingList.SortDirection {
            get {
                return SortDirectionCore;
            }
        }

        protected virtual ListSortDirection SortDirectionCore {
            get {
                return ListSortDirection.Ascending;
            }
        }

        void IBindingList.ApplySort(PropertyDescriptor prop, ListSortDirection direction) {
            ApplySortCore(prop, direction);
        }

        protected virtual void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction) {
            throw new NotSupportedException();
        }

        void IBindingList.RemoveSort() {
            RemoveSortCore();
        }

        protected virtual void RemoveSortCore() {
            throw new NotSupportedException();
        }

        int IBindingList.Find(PropertyDescriptor prop, object key) {
            return FindCore(prop, key);
        }

        protected virtual int FindCore(PropertyDescriptor prop, object key) {
            throw new NotSupportedException();
        }

        void IBindingList.AddIndex(PropertyDescriptor prop) {
            // Not supported
        }

        void IBindingList.RemoveIndex(PropertyDescriptor prop) {
            // Not supported
        }

        #endregion

        #region Property Change Support
        
        private void HookPropertyChanged(T item) {
            INotifyPropertyChanged inpc = (item as INotifyPropertyChanged);
            
            // Note: inpc may be null if item is null, so always check.
            if (null != inpc) {
                if (propertyChangedEventHandler == null) {
                    propertyChangedEventHandler = new PropertyChangedEventHandler(Child_PropertyChanged);
                }
                inpc.PropertyChanged += propertyChangedEventHandler;
            }
        }
        
        private void UnhookPropertyChanged(T item) {
            INotifyPropertyChanged inpc = (item as INotifyPropertyChanged);
    
            // Note: inpc may be null if item is null, so always check.
            if (null != inpc && null != propertyChangedEventHandler) {
                inpc.PropertyChanged -= propertyChangedEventHandler;
            }
        }
        
        void Child_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (this.RaiseListChangedEvents) {
                if (sender == null || e == null || string.IsNullOrEmpty(e.PropertyName)) {
                    // Fire reset event (per INotifyPropertyChanged spec)
                    ResetBindings();
                }
                else {
                    // The change event is broken should someone pass an item to us that is not
                    // of type T.  Still, if they do so, detect it and ignore.  It is an incorrect
                    // and rare enough occurrence that we do not want to slow the mainline path
                    // with "is" checks.
                    T item;
                     
                    try {
                        item = (T)sender;
                    }
                    catch(InvalidCastException) {
                        ResetBindings();
                        return;
                    }

                    // Find the position of the item.  This should never be -1.  If it is,
                    // somehow the item has been removed from our list without our knowledge.
                    int pos = lastChangeIndex;
                    
                    if (pos < 0 || pos >= Count || !this[pos].Equals(item)) {
                        pos = this.IndexOf(item);
                        lastChangeIndex = pos;
                    }

                    if (pos == -1) {
                        Debug.Fail("Item is no longer in our list but we are still getting change notifications.");
                        UnhookPropertyChanged(item);
                        ResetBindings();
                    }
                    else {
                        // Get the property descriptor
                        if (null == this.itemTypeProperties) {
                            // Get Shape
                            itemTypeProperties = TypeDescriptor.GetProperties(typeof(T));
                            Debug.Assert(itemTypeProperties != null);
                        }

                        PropertyDescriptor pd = itemTypeProperties.Find(e.PropertyName, true);

                        // Create event args.  If there was no matching property descriptor,
                        // we raise the list changed anyway.
                        ListChangedEventArgs args = new ListChangedEventArgs(ListChangedType.ItemChanged, pos, pd);

                        // Fire the ItemChanged event
                        OnListChanged(args);
                    }
                }
            }
        }
        
        #endregion

        #region IRaiseItemChangedEvents interface

        /// <include file='doc\BindingList.uex' path='docs/doc[@for="BindingList.RaisesItemChangedEvents"]/*' />
        /// <devdoc>
        ///     Returns false to indicate that BindingList<T> does NOT raise ListChanged events
        ///     of type ItemChanged as a result of property changes on individual list items
        ///     unless those items support INotifyPropertyChanged
        /// </devdoc>
        bool IRaiseItemChangedEvents.RaisesItemChangedEvents {
            get {
                return this.raiseItemChangedEvents;
            }
        }

        #endregion

    }
}
