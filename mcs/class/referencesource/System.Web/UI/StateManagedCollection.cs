//------------------------------------------------------------------------------
// <copyright file="StateManagedCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    /// <devdoc>
    /// Manages state for an arbitrary collection of items that implement IStateManager.
    /// The collection differentiates between known types and unknown types.
    /// Known types take up less space in ViewState because only an index needs to be stored instead of a fully qualified type name.
    /// Unknown types need to have their fully qualified type name stored in ViewState so they take up more space.
    /// </devdoc>
    public abstract class StateManagedCollection : IList, IStateManager {

        private ArrayList _collectionItems;

        private bool _tracking;
        private bool _saveAll;

        // We want to know if the collection had items to begin with 
        // so we don't put empty collections in the ViewState unnecessarily
        private bool _hadItems;



        /// <devdoc>
        /// Creates a new instance of StateManagedCollection.
        /// </devdoc>
        protected StateManagedCollection() {
            _collectionItems = new ArrayList();
        }



        /// <devdoc>
        /// Returns the number of items in the collection.
        /// </devdoc>
        public int Count {
            get {
                return _collectionItems.Count;
            }
        }


        /// <devdoc>
        /// Removes all the items from the collection.
        /// </devdoc>
        public void Clear() {
            OnClear();
            _collectionItems.Clear();
            OnClearComplete();

            if (_tracking) {
                _saveAll = true;
            }
        }


        public void CopyTo(Array array, int index) {
            _collectionItems.CopyTo(array, index);
        }


        /// <devdoc>
        /// Creates an object of a known type based on an index into an array of types.
        /// Indexes passed into CreateKnownType() must mach the indexes of the ArrayList returned
        /// by GetKnownTypes().
        /// </devdoc>
        protected virtual object CreateKnownType(int index) {
            throw new InvalidOperationException(SR.GetString(SR.StateManagedCollection_NoKnownTypes));
        }


        /// <devdoc>
        /// Returns the IEnumerator for the collection.
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return _collectionItems.GetEnumerator();
        }


        /// <devdoc>
        /// Returns an ordered list of known types.
        /// </devdoc>
        protected virtual Type[] GetKnownTypes() {
            return null;
        }

        /// <devdoc>
        /// Returns the number of known types.
        /// </devdoc>
        private int GetKnownTypeCount() {
            Type[] types = GetKnownTypes();

            if (types == null)
                return 0;

            return types.Length;
        }

        /// <devdoc>
        /// Inserts a new object into the collection at a given index.
        /// If the index is -1 then the object is appended to the end of the collection.
        /// </devdoc>
        private void InsertInternal(int index, object o) {
            Debug.Assert(index >= -1 && index <= Count, "Expected index to be at least -1 and less than or equal to Count.");
            if (o == null) {
                throw new ArgumentNullException("o");
            }

            if (((IStateManager)this).IsTrackingViewState) {
                ((IStateManager)o).TrackViewState();
                SetDirtyObject(o);
            }

            OnInsert(index, o);

            int trueIndex;

            if (index == -1) {
                trueIndex = _collectionItems.Add(o);
            }
            else {
                trueIndex = index;
                _collectionItems.Insert(index, o);
            }

            try {
                OnInsertComplete(index, o);
            }
            catch {
                _collectionItems.RemoveAt(trueIndex);
                throw;
            }
        }

        /// <devdoc>
        /// Loads all items from view state.
        /// </devdoc>
        private void LoadAllItemsFromViewState(object savedState) {
            Debug.Assert(savedState != null);
            Debug.Assert(savedState is Pair);

            Pair p1 = (Pair)savedState;

            if (p1.Second is Pair) {
                Pair p2 = (Pair)p1.Second;

                // save all mode; some objects are typed
                object[] states = (object[])p1.First;
                int[] typeIndices = (int[])p2.First;
                ArrayList typedObjectTypeNames = (ArrayList)p2.Second;

                Clear();

                for (int i = 0; i < states.Length; i++) {
                    object o;

                    // If there is only one known type, we don't need type indices
                    if (typeIndices == null) {
                        // Create known type
                        o = CreateKnownType(0);
                    }
                    else {
                        int typeIndex = typeIndices[i];

                        if (typeIndex < GetKnownTypeCount()) {
                            // Create known type
                            o = CreateKnownType(typeIndex);
                        }
                        else {
                            string typeName = (string)typedObjectTypeNames[typeIndex - GetKnownTypeCount()];
                            Type type = Type.GetType(typeName);

                            o = Activator.CreateInstance(type);
                        }
                    }

                    ((IStateManager)o).TrackViewState();
                    ((IStateManager)o).LoadViewState(states[i]);

                    ((IList)this).Add(o);
                }
            }
            else {
                Debug.Assert(p1.First is object[]);

                // save all mode; all objects are instances of known types
                object[] states = (object[])p1.First;
                int[] typeIndices = (int[])p1.Second;

                Clear();

                for (int i = 0; i < states.Length; i++) {
                    // Create known type
                    int typeIndex = 0;
                    if (typeIndices != null) {
                        typeIndex = (int)typeIndices[i];
                    }
                    object o = CreateKnownType(typeIndex);

                    ((IStateManager)o).TrackViewState();
                    ((IStateManager)o).LoadViewState(states[i]);

                    ((IList)this).Add(o);
                }
            }
        }

        /// <devdoc>
        /// Loads only changed items from view state.
        /// </devdoc>
        private void LoadChangedItemsFromViewState(object savedState) {
            Debug.Assert(savedState != null);
            Debug.Assert(savedState is Triplet);

            Triplet t = (Triplet)savedState;

            if (t.Third is Pair) {
                // save some mode; some new objects are typed
                Pair p = (Pair)t.Third;

                ArrayList indices = (ArrayList)t.First;
                ArrayList states = (ArrayList)t.Second;
                ArrayList typeIndices = (ArrayList)p.First;
                ArrayList typedObjectTypeNames = (ArrayList)p.Second;

                for (int i = 0; i < indices.Count; i++) {
                    int index = (int)indices[i];

                    if (index < Count) {
                        ((IStateManager)((IList)this)[index]).LoadViewState(states[i]);
                    }
                    else {
                        object o;

                        // If there is only one known type, we don't need type indices
                        if (typeIndices == null) {
                            // Create known type
                            o = CreateKnownType(0);
                        }
                        else {
                            int typeIndex = (int)typeIndices[i];

                            if (typeIndex < GetKnownTypeCount()) {
                                // Create known type
                                o = CreateKnownType(typeIndex);
                            }
                            else {
                                string typeName = (string)typedObjectTypeNames[typeIndex - GetKnownTypeCount()];
                                Type type = Type.GetType(typeName);

                                o = Activator.CreateInstance(type);
                            }
                        }

                        ((IStateManager)o).TrackViewState();
                        ((IStateManager)o).LoadViewState(states[i]);

                        ((IList)this).Add(o);
                    }
                }
            }
            else {
                // save some mode; all new objects are instances of known types
                ArrayList indices = (ArrayList)t.First;
                ArrayList states = (ArrayList)t.Second;
                ArrayList typeIndices = (ArrayList)t.Third;

                for (int i = 0; i < indices.Count; i++) {
                    int index = (int)indices[i];

                    if (index < Count) {
                        ((IStateManager)((IList)this)[index]).LoadViewState(states[i]);
                    }
                    else {
                        // Create known type
                        int typeIndex = 0;
                        if (typeIndices != null) {
                            typeIndex = (int)typeIndices[i];
                        }
                        object o = CreateKnownType(typeIndex);

                        ((IStateManager)o).TrackViewState();
                        ((IStateManager)o).LoadViewState(states[i]);

                        ((IList)this).Add(o);
                    }
                }
            }
        }


        /// <devdoc>
        /// Called when the Clear() method is starting.
        /// </devdoc>
        protected virtual void OnClear() {
        }


        /// <devdoc>
        /// Called when the Clear() method is complete.
        /// </devdoc>
        protected virtual void OnClearComplete() {
        }


        /// <devdoc>
        /// Called when an object must be validated.
        /// </devdoc>
        protected virtual void OnValidate(object value) {
            if (value == null) throw new ArgumentNullException("value");
        }


        /// <devdoc>
        /// Called when the Insert() method is starting.
        /// </devdoc>
        protected virtual void OnInsert(int index, object value) {
        }


        /// <devdoc>
        /// Called when the Insert() method is complete.
        /// </devdoc>
        protected virtual void OnInsertComplete(int index, object value) {
        }


        /// <devdoc>
        /// Called when the Remove() method is starting.
        /// </devdoc>
        protected virtual void OnRemove(int index, object value) {
        }


        /// <devdoc>
        /// Called when the Remove() method is complete.
        /// </devdoc>
        protected virtual void OnRemoveComplete(int index, object value) {
        }

        /// <devdoc>
        /// Saves all items in the collection to view state.
        /// </devdoc>
        private object SaveAllItemsToViewState() {
            Debug.Assert(_saveAll);

            bool hasState = false;

            int count = _collectionItems.Count;

            int[] typeIndices = new int[count];
            object[] states = new object[count];

            ArrayList typedObjectTypeNames = null;
            IDictionary typedObjectTracker = null;

            int knownTypeCount = GetKnownTypeCount();


            for (int i = 0; i < count; i++) {
                object o = _collectionItems[i];
                SetDirtyObject(o);

                states[i] = ((IStateManager)o).SaveViewState();

                if (states[i] != null)
                    hasState = true;

                Type objectType = o.GetType();

                int knownTypeIndex = -1;

                // If there are known types, find index
                if (knownTypeCount != 0) {
                    knownTypeIndex = ((IList)GetKnownTypes()).IndexOf(objectType);
                }

                if (knownTypeIndex != -1) {
                    // Type is known
                    typeIndices[i] = knownTypeIndex;
                }
                else {
                    // Type is unknown
                    if (typedObjectTypeNames == null) {
                        typedObjectTypeNames = new ArrayList();
                        typedObjectTracker = new HybridDictionary();
                    }

                    // Get index of type
                    object index = typedObjectTracker[objectType];

                    // If type is not in list, add it to the list
                    if (index == null) {
                        typedObjectTypeNames.Add(objectType.AssemblyQualifiedName);

                        // Offset the index by the known type count
                        index = typedObjectTypeNames.Count + knownTypeCount - 1;
                        typedObjectTracker[objectType] = index;
                    }

                    typeIndices[i] = (int)index;
                }
            }

            // If the collection didn't have items to begin with don't save the state
            if (!_hadItems && !hasState) {
                return null;
            }

            if (typedObjectTypeNames == null) {
                // all objects are instances known types

                // If there is only one known type, then all objects are of that type so the indices are not needed
                if (knownTypeCount == 1)
                    typeIndices = null;

                return new Pair(states, typeIndices);
            }
            else {
                return new Pair(states, new Pair(typeIndices, typedObjectTypeNames));
            }
        }

        /// <devdoc>
        /// Saves changed items to view state.
        /// </devdoc>
        private object SaveChangedItemsToViewState() {
            Debug.Assert(_saveAll == false);

            bool hasState = false;

            int count = _collectionItems.Count;

            ArrayList indices = new ArrayList();
            ArrayList states = new ArrayList();
            ArrayList typeIndices = new ArrayList();

            ArrayList typedObjectTypeNames = null;
            IDictionary typedObjectTracker = null;

            int knownTypeCount = GetKnownTypeCount();


            for (int i = 0; i < count; i++) {
                object o = _collectionItems[i];

                object state = ((IStateManager)o).SaveViewState();
                if (state != null) {
                    hasState = true;

                    indices.Add(i);
                    states.Add(state);

                    Type objectType = o.GetType();

                    int knownTypeIndex = -1;

                    // If there are known types, find index
                    if (knownTypeCount != 0) {
                        knownTypeIndex = ((IList)GetKnownTypes()).IndexOf(objectType);
                    }

                    if (knownTypeIndex != -1) {
                        // Type is known
                        typeIndices.Add(knownTypeIndex);
                    }
                    else {
                        // Type is unknown
                        if (typedObjectTypeNames == null) {
                            typedObjectTypeNames = new ArrayList();
                            typedObjectTracker = new HybridDictionary();
                        }

                        object index = typedObjectTracker[objectType];
                        if (index == null) {
                            typedObjectTypeNames.Add(objectType.AssemblyQualifiedName);

                            // Offset the index by the known type count
                            index = typedObjectTypeNames.Count + knownTypeCount - 1;
                            typedObjectTracker[objectType] = index;
                        }

                        typeIndices.Add(index);
                    }
                }
            }

            // If the collection didn't have items to begin with don't save the state
            if (!_hadItems && !hasState) {
                return null;
            }

            if (typedObjectTypeNames == null) {
                // all objects are instances known types

                // If there is only one known type, then all objects are of that type so the indices are not needed
                if (knownTypeCount == 1)
                    typeIndices = null;

                return new Triplet(indices, states, typeIndices);
            }
            else {
                return new Triplet(indices, states, new Pair(typeIndices, typedObjectTypeNames));
            }
        }

        /// <devdoc>
        /// Forces the entire collection to be serialized into viewstate, not just
        /// the change-information. This is useful when a collection has changed in
        /// a significant way and change information would be insufficient to
        /// recreate the object.
        /// </devdoc>
        public void SetDirty() {
            _saveAll = true;
        }


        /// <devdoc>
        /// Flags an object to record its entire state instead of just changed parts.
        /// </devdoc>
        protected abstract void SetDirtyObject(object o);



        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }



        /// <internalonly/>
        int ICollection.Count {
            get {
                return Count;
            }
        }


        /// <internalonly/>
        bool ICollection.IsSynchronized {
            get {
                return false;
            }
        }


        /// <internalonly/>
        object ICollection.SyncRoot {
            get {
                return null;
            }
        }


        /// <internalonly/>
        bool IList.IsFixedSize {
            get {
                return false;
            }
        }


        /// <internalonly/>
        bool IList.IsReadOnly {
            get {
                return _collectionItems.IsReadOnly;
            }
        }


        /// <internalonly/>
        object IList.this[int index] {
            get {
                return _collectionItems[index];
            }
            set {
                if (index < 0 || index >= Count) {
                    throw new ArgumentOutOfRangeException("index", SR.GetString(SR.StateManagedCollection_InvalidIndex));
                }
                ((IList)this).RemoveAt(index);
                ((IList)this).Insert(index, value);
            }
        }


        /// <internalonly/>
        int IList.Add(object value) {
            OnValidate(value);

            InsertInternal(-1, value);

            return _collectionItems.Count - 1;
        }


        /// <internalonly/>
        void IList.Clear() {
            Clear();
        }


        /// <internalonly/>
        bool IList.Contains(object value) {
            if (value == null) {
                return false;
            }

            OnValidate(value);

            return _collectionItems.Contains(value);
        }


        /// <internalonly/>
        int IList.IndexOf(object value) {
            if (value == null) {
                return -1;
            }

            OnValidate(value);

            return _collectionItems.IndexOf(value);
        }


        /// <internalonly/>
        void IList.Insert(int index, object value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            if (index < 0 || index > Count) {
                throw new ArgumentOutOfRangeException("index", SR.GetString(SR.StateManagedCollection_InvalidIndex));
            }

            OnValidate(value);

            InsertInternal(index, value);

            if (_tracking) {
                _saveAll = true;
            }
        }


        /// <internalonly/>
        void IList.Remove(object value) {
            if (value == null) {
                return;
            }

            OnValidate(value);

            ((IList)this).RemoveAt(((IList)this).IndexOf(value));
        }


        /// <internalonly/>
        void IList.RemoveAt(int index) {
            object o = _collectionItems[index];

            OnRemove(index, o);
            _collectionItems.RemoveAt(index);
            try {
                OnRemoveComplete(index, o);
            }
            catch {
                _collectionItems.Insert(index, o);
                throw;
            }

            if (_tracking) {
                _saveAll = true;
            }
        }



        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return _tracking;
            }
        }


        /// <internalonly/>
        void IStateManager.LoadViewState(object savedState) {

            if (savedState != null) {
                if (savedState is Triplet) {
                    LoadChangedItemsFromViewState(savedState);
                }
                else {
                    LoadAllItemsFromViewState(savedState);
                }
            }
        }


        /// <internalonly/>
        object IStateManager.SaveViewState() {

            if (_saveAll) {
                return SaveAllItemsToViewState();
            }
            else {
                return SaveChangedItemsToViewState();
            }
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            if (!((IStateManager)this).IsTrackingViewState) {
                _hadItems = Count > 0;

                _tracking = true;

                foreach (IStateManager o in _collectionItems) {
                    o.TrackViewState();
                }
            }
        }
    }
}

