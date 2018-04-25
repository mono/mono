//------------------------------------------------------------------------------
// <copyright file="StateBag.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System.Text;
    using System.IO;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web.Util;

    /*
     * The StateBag class is a helper class used to manage state of properties.
     * The class stores name/value pairs as string/object and tracks modifications of
     * properties after being 'marked'.  This class is used as the primary storage
     * mechanism for all HtmlControls and WebControls.
     */

    /// <devdoc>
    ///    <para>Manages the state of Web Forms control properties. This 
    ///       class stores attribute/value pairs as string/object and tracks changes to these
    ///       attributes, which are treated as properties, after the Page.Init
    ///       method is executed for a
    ///       page request. </para>
    ///    <note type="note">
    ///       Only values changed after Page.Init
    ///       has executed are persisted to the page's view state.
    ///    </note>
    /// </devdoc>
    public sealed class StateBag : IStateManager, IDictionary {
        private IDictionary bag;
        private bool        marked;
        private bool        ignoreCase;

        /*
         * Constructs an StateBag
         */

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.StateBag'/> class.</para>
        /// </devdoc>
        public StateBag() : this(false) {
        }

        /*
         * Constructs an StateBag
         */

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.StateBag'/> class that allows stored state 
        ///    values to be case-insensitive.</para>
        /// </devdoc>
        public StateBag(bool ignoreCase) {
            marked = false;
            this.ignoreCase = ignoreCase;
            bag = CreateBag();
        }


        /*
         * Return count of number of StateItems in the bag.
         */

        /// <devdoc>
        /// <para>Indicates the number of items in the <see cref='System.Web.UI.StateBag'/> object. This property is 
        ///    read-only.</para>
        /// </devdoc>
        public int Count {
            get {
                return bag.Count;
            }
        }

        /*
         * Returns a collection of keys.
         */

        /// <devdoc>
        /// <para>Indicates a collection of keys representing the items in the <see cref='System.Web.UI.StateBag'/> object. 
        ///    This property is read-only.</para>
        /// </devdoc>
        public ICollection Keys {
            get {
                return bag.Keys;
            }
        }

        /*
         * Returns a collection of values.
         */

        /// <devdoc>
        /// <para>Indicates a collection of view state values in the <see cref='System.Web.UI.StateBag'/> object. 
        ///    This property is read-only.</para>
        /// </devdoc>
        public ICollection Values {
            get {
                return bag.Values;
            }
        }

        /*
         * Get or set value of a StateItem.
         * A set will automatically add a new StateItem for a
         * key which is not already in the bag.  A set to null
         * will remove the item if set before mark, otherwise
         * a null set will be saved to allow tracking of state
         * removed after mark.
         */

        /// <devdoc>
        ///    <para> Indicates the value of an item stored in the 
        ///    <see langword='StateBag'/> 
        ///    object. Setting this property with a key not already stored in the StateBag will
        ///    add an item to the bag. If you set this property to <see langword='null'/> before
        ///    the TrackState method is called on an item will remove it from the bag. Otherwise,
        ///    when you set this property to <see langword='null'/>
        ///    the key will be saved to allow tracking of the item's state.</para>
        /// </devdoc>
        public object this[string key] {
            get {
                if (String.IsNullOrEmpty(key))
                    throw ExceptionUtil.ParameterNullOrEmpty("key");
              
                StateItem item = bag[key] as StateItem;
                if (item != null)
                    return item.Value;
                return null;
            }
            set {
                Add(key,value);
            }
        }

        /*
         * Private implementation of IDictionary item accessor
         */

        /// <internalonly/>
        object IDictionary.this[object key] {
            get { return this[(string) key]; }
            set { this[(string) key] = value; }
        }

        private IDictionary CreateBag() {
            return new HybridDictionary(ignoreCase);
        }

        /*
         * Add a new StateItem or update an existing StateItem in the bag.
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public StateItem Add(string key,object value) {

            if (String.IsNullOrEmpty(key))
                throw ExceptionUtil.ParameterNullOrEmpty("key");
              
            StateItem item = bag[key] as StateItem;

            if (item == null) {
                if (value != null || marked) {
                    item = new StateItem(value);
                    bag.Add(key,item);
                }
            }
            else {
                if (value == null && !marked) {
                    bag.Remove(key);
                }
                else {
                    item.Value = value;
                }
            }
            if (item != null && marked) {
                item.IsDirty = true;
            }
            return item;
        }

        /*
         * Private implementation of IDictionary Add
         */

        /// <internalonly/>
        void IDictionary.Add(object key,object value) {
            Add((string) key, value);
        }

        /*
         * Clear all StateItems from the bag.
         */

        /// <devdoc>
        /// <para>Removes all controls from the <see cref='System.Web.UI.StateBag'/> object.</para>
        /// </devdoc>
        public void Clear() {
            bag.Clear();
        }

        /*
         * Get an enumerator for the StateItems.
         */

        /// <devdoc>
        ///    <para>Returns an enumerator that iterates over the key/value pairs stored in 
        ///       the <see langword='StateBag'/>.</para>
        /// </devdoc>
        public IDictionaryEnumerator GetEnumerator() {
            return bag.GetEnumerator();
        }

        /*
         * Return the dirty flag of the state item.
         * Returns false if there is not an item for given key.
         */

        /// <devdoc>
        /// <para>Checks an item stored in the <see langword='StateBag'/> to see if it has been 
        ///    modified.</para>
        /// </devdoc>
        public bool IsItemDirty(string key) {
            StateItem item = bag[key] as StateItem;
            if (item != null)
                return item.IsDirty;

            return false;
        }

        /*
         * Return true if 'marked' and state changes are being tracked.
         */

        /// <devdoc>
        ///    <para>Determines if state changes in the StateBag object's store are being tracked.</para>
        /// </devdoc>
        internal bool IsTrackingViewState {
            get {
                return marked;
            }
        }

        /*
         * Restore state that was previously saved via SaveViewState.
         */

        /// <devdoc>
        ///    <para>Loads the specified previously saved state information</para>
        /// </devdoc>
        internal void LoadViewState(object state) {
            if (state != null) {
                ArrayList data = (ArrayList)state;

                for (int i = 0; i < data.Count; i += 2) {
#if OBJECTSTATEFORMATTER
                    string key = ((IndexedString)data[i]).Value;
#else
                    string key = (string)data[i];
#endif
                    object value = data[i + 1];

                    Add(key, value);
                }
            }
        }

        /*
         * Start tracking state changes after "mark".
         */

        /// <devdoc>
        ///    <para>Initiates the tracking of state changes for items stored in the 
        ///    <see langword='StateBag'/> object.</para>
        /// </devdoc>
        internal void TrackViewState() {
            marked = true;
        }

        /*
         * Remove a StateItem from the bag altogether regardless of marked.
         * Used internally by controls.
         */

        /// <devdoc>
        /// <para>Removes the specified item from the <see cref='System.Web.UI.StateBag'/> object.</para>
        /// </devdoc>
        public void Remove(string key) {
            bag.Remove(key);
        }

        /*
         * Private implementation of IDictionary Remove
         */

        /// <internalonly/>
        void IDictionary.Remove(object key) {
            Remove((string) key);
        }


        /*
         * Return object containing state that has been modified since "mark".
         * Returns null if there is no modified state.
         */

        /// <devdoc>
        ///    <para>Returns an object that contains all state changes for items stored in the 
        ///    <see langword='StateBag'/> object.</para>
        /// </devdoc>
        internal object SaveViewState() {
            ArrayList data = null;

            // 

            if (bag.Count != 0) {
                IDictionaryEnumerator e = bag.GetEnumerator();
                while (e.MoveNext()) {
                    StateItem item = (StateItem)(e.Value);
                    if (item.IsDirty) {
                        if (data == null) {
                            data = new ArrayList();
                        }
#if OBJECTSTATEFORMATTER
                        data.Add(new IndexedString((string)e.Key));
#else
                        data.Add(e.Key);
#endif
                        data.Add(item.Value);
                    }
                }
            }

            return data;
        }


        /// <devdoc>
        /// Sets the dirty state of all item values currently in the StateBag.
        /// </devdoc>
        public void SetDirty(bool dirty) {
            if (bag.Count != 0) {
                foreach (StateItem item in bag.Values) {
                    item.IsDirty = dirty;
                }
            }
        }

        /*
         * Internal method for setting dirty flag on a state item.
         * Used internallly to prevent state management of certain properties.
         */

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public void SetItemDirty(string key,bool dirty) {
            StateItem item = bag[key] as StateItem;
            if (item != null) {
                item.IsDirty = dirty;
            }
        }

        /// <internalonly/>
        bool IDictionary.IsFixedSize {
            get { return false; }
        }


        /// <internalonly/>
        bool IDictionary.IsReadOnly {
            get { return false; }
        }


        /// <internalonly/>
        bool ICollection.IsSynchronized { 
            get { return false;}
        }


        /// <internalonly/>
        object ICollection.SyncRoot {
            get {return this; }
        }


        /// <internalonly/>
        bool IDictionary.Contains(object key) {
            return bag.Contains((string) key);
        }


        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IDictionary)this).GetEnumerator();
        }


        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            Values.CopyTo(array, index);
        }


        /*
         * Return true if tracking state changes.
         * Method of private interface, IStateManager.
         */

        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        /*
         * Load previously saved state.
         * Method of private interface, IStateManager.
         */

        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }

        /*
         * Start tracking state changes.
         * Method of private interface, IStateManager.
         */

        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }

        /*
         * Return object containing state changes.
         * Method of private interface, IStateManager.
         */

        /// <internalonly/>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }
    }
}
