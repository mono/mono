//------------------------------------------------------------------------------
// <copyright file="ConfigurationElementCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace System.Configuration {

    [System.Diagnostics.DebuggerDisplay("Count = {Count}")]
    public abstract class ConfigurationElementCollection : ConfigurationElement, ICollection {
        internal const string DefaultAddItemName = "add";
        internal const string DefaultRemoveItemName = "remove";
        internal const string DefaultClearItemsName = "clear";

        private int _removedItemCount = 0;    // Number of items removed for this collection (not including parent)
        private int _inheritedCount = 0;      // Total number of inherited items
        private ArrayList _items = new ArrayList();
        private String _addElement = DefaultAddItemName;
        private String _removeElement = DefaultRemoveItemName;
        private String _clearElement = DefaultClearItemsName;
        private bool bEmitClearTag = false;
        private bool bCollectionCleared = false;
        private bool bModified = false;
        private bool bReadOnly = false;
        private IComparer _comparer;
        internal bool internalAddToEnd = false;
        internal String internalElementTagName = string.Empty;

        protected ConfigurationElementCollection() {
        }

        protected ConfigurationElementCollection(IComparer comparer) {
            if (comparer == null) {
                throw new ArgumentNullException("comparer");
            }

            _comparer = comparer;
        }

        private ArrayList Items {
            get {
                return _items;
            }
        }

        private enum InheritedType {
            inNeither = 0,
            inParent = 1,
            inSelf = 2,
            inBothSame = 3,
            inBothDiff = 4,
            inBothCopyNoRemove = 5,
        }

        protected internal string AddElementName {
            get {
                return _addElement;
            }
            set {
                _addElement = value;
                if (BaseConfigurationRecord.IsReservedAttributeName(value)) {
                    throw new ArgumentException(SR.GetString(SR.Item_name_reserved, DefaultAddItemName, value));
                }
            }
        }

        protected internal string RemoveElementName {
            get {
                return _removeElement;
            }
            set {
                if (BaseConfigurationRecord.IsReservedAttributeName(value)) {
                    throw new ArgumentException(SR.GetString(SR.Item_name_reserved, DefaultRemoveItemName, value));
                }
                _removeElement = value;
            }
        }

        protected internal string ClearElementName {
            get {
                return _clearElement;
            }
            set {
                if (BaseConfigurationRecord.IsReservedAttributeName(value)) {
                    throw new ArgumentException(SR.GetString(SR.Item_name_reserved, DefaultClearItemsName, value));
                }
                _clearElement = value;
            }
        }

        // AssociateContext
        //
        // Associate a collection of values with a configRecord
        //
        internal override void AssociateContext(BaseConfigurationRecord configRecord) {
            base.AssociateContext(configRecord);

            foreach (Entry entry in _items) {
                if (entry._value != null) {
                    entry._value.AssociateContext(configRecord);
                }
            }
        }

        protected internal override bool IsModified() {
            if (bModified == true) {
                return true;
            }

            if (base.IsModified() == true) {
                return true;
            }

            foreach (Entry entry in _items) {
                if (entry._entryType != EntryType.Removed) {
                    ConfigurationElement elem = entry._value;
                    if (elem.IsModified()) {
                        return true;
                    }
                }
            }
            return false;
        }

        protected internal override void ResetModified() {
            bModified = false;
            base.ResetModified();

            foreach (Entry entry in _items) {
                if (entry._entryType != EntryType.Removed) {
                    ConfigurationElement elem = entry._value;
                    elem.ResetModified();
                }
            }
        }

        public override bool IsReadOnly() {
            return bReadOnly;
        }

        protected internal override void SetReadOnly() {
            bReadOnly = true;
            foreach (Entry entry in _items) {
                if (entry._entryType != EntryType.Removed) {
                    ConfigurationElement elem = entry._value;
                    elem.SetReadOnly();
                }
            }
        }

        internal virtual IEnumerator GetEnumeratorImpl() {
            return new Enumerator(_items, this);
        }

        internal IEnumerator GetElementsEnumerator() {
            // Return an enumerator over the collection's config elements.
            // This is different then the std GetEnumerator because the second one
            // can return different set of items if overriden in a derived class

            return new Enumerator(_items, this);
        }

        public override bool Equals(object compareTo) {
            if (compareTo.GetType() == this.GetType()) {
                ConfigurationElementCollection compareToElem = (ConfigurationElementCollection)compareTo;
                if (this.Count != compareToElem.Count) {
                    return false;
                }

                foreach (Entry thisEntry in Items) {
                    bool found = false;
                    foreach (Entry compareEntry in compareToElem.Items) {
                        if (Object.Equals(thisEntry._value, compareEntry._value)) {
                            found = true;
                            break;
                        }
                    }
                    if (found == false) {
                        // not in the collection must be different
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode() {
            int hHashCode = 0;
            foreach (Entry thisEntry in Items) {
                ConfigurationElement elem = thisEntry._value;
                hHashCode ^= elem.GetHashCode();
            }
            return hHashCode;
        }


        protected internal override void Unmerge(ConfigurationElement sourceElement,
                                                 ConfigurationElement parentElement,
                                                 ConfigurationSaveMode saveMode) {

            base.Unmerge(sourceElement, parentElement, saveMode);
            if (sourceElement != null) {
                ConfigurationElementCollection parentCollection = parentElement as ConfigurationElementCollection;
                ConfigurationElementCollection sourceCollection = sourceElement as ConfigurationElementCollection;
                Hashtable Inheritance = new Hashtable();
                _lockedAllExceptAttributesList = sourceElement._lockedAllExceptAttributesList;
                _lockedAllExceptElementsList = sourceElement._lockedAllExceptElementsList;
                _fItemLocked = sourceElement._fItemLocked;
                _lockedAttributesList = sourceElement._lockedAttributesList;
                _lockedElementsList = sourceElement._lockedElementsList;

                AssociateContext(sourceElement._configRecord);

                if (parentElement != null) {
                    if (parentElement._lockedAttributesList != null)
                        _lockedAttributesList = UnMergeLockList(sourceElement._lockedAttributesList,
                            parentElement._lockedAttributesList, saveMode);
                    if (parentElement._lockedElementsList != null)
                        _lockedElementsList = UnMergeLockList(sourceElement._lockedElementsList,
                            parentElement._lockedElementsList, saveMode);
                    if (parentElement._lockedAllExceptAttributesList != null)
                        _lockedAllExceptAttributesList = UnMergeLockList(sourceElement._lockedAllExceptAttributesList,
                            parentElement._lockedAllExceptAttributesList, saveMode);
                    if (parentElement._lockedAllExceptElementsList != null)
                        _lockedAllExceptElementsList = UnMergeLockList(sourceElement._lockedAllExceptElementsList,
                            parentElement._lockedAllExceptElementsList, saveMode);
                }

                if (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap ||
                    CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                    // When writing out portable configurations the <clear/> tag should be written
                    bCollectionCleared = sourceCollection.bCollectionCleared;
                    EmitClear = (saveMode == ConfigurationSaveMode.Full && (_clearElement.Length != 0)) ||
                                (saveMode == ConfigurationSaveMode.Modified && bCollectionCleared) ?
                                    true : sourceCollection.EmitClear;

                    if ((parentCollection != null) && (EmitClear != true)) {
                        foreach (Entry entry in parentCollection.Items) {
                            if (entry._entryType != EntryType.Removed) {
                                Inheritance[entry.GetKey(this)] = InheritedType.inParent;
                            }
                        }
                    }

                    foreach (Entry entry in sourceCollection.Items) {
                        if (entry._entryType != EntryType.Removed) {
                            if (Inheritance.Contains(entry.GetKey(this))) {
                                Entry parentEntry = (Entry)parentCollection.Items[parentCollection.RealIndexOf(entry._value)];

                                ConfigurationElement elem = entry._value;
                                if (elem.Equals(parentEntry._value)) {
                                    // in modified mode we consider any change to be different than the parent
                                    Inheritance[entry.GetKey(this)] = InheritedType.inBothSame;
                                    if (saveMode == ConfigurationSaveMode.Modified) {
                                        if (elem.IsModified()) {
                                            Inheritance[entry.GetKey(this)] = InheritedType.inBothDiff;
                                        } else
                                        if (elem.ElementPresent) {
                                            // This is when the source file contained the entry but it was an
                                            // exact copy.  We don't want to emit a remove so we treat it as
                                            // a special case.
                                            Inheritance[entry.GetKey(this)] = InheritedType.inBothCopyNoRemove;
                                        }
                                    }
                                }
                                else {
                                    Inheritance[entry.GetKey(this)] = InheritedType.inBothDiff;
                                    if (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate
                                        && entry._entryType == EntryType.Added) {
                                        // this is a special case for deailing with defect number 529517
                                        // this code allow the config to write out the same xml when no remove was
                                        // present during deserialization.
                                        Inheritance[entry.GetKey(this)] = InheritedType.inBothCopyNoRemove;
                                    }
                                }
                            }
                            else { // not in parent
                                Inheritance[entry.GetKey(this)] = InheritedType.inSelf;
                            }
                        }
                    }

                    //
                    if ((parentCollection != null) && (EmitClear != true)) {
                        foreach (Entry entry in parentCollection.Items) {
                            if (entry._entryType != EntryType.Removed) {

                                InheritedType tp = (InheritedType)Inheritance[entry.GetKey(this)];
                                if (tp == InheritedType.inParent || tp == InheritedType.inBothDiff) {
                                    ConfigurationElement elem = CallCreateNewElement(entry.GetKey(this).ToString());

                                    elem.Reset(entry._value); // copy this entry
                                    BaseAdd(elem,ThrowOnDuplicate,true);                          // Add it (so that is once existed in the temp
                                    BaseRemove(entry.GetKey(this), false);   // now remove it to for a remove instruction
                                }
                            }
                        }
                    }

                    foreach (Entry entry in sourceCollection.Items) {
                        if (entry._entryType != EntryType.Removed) {
                            InheritedType tp = (InheritedType)Inheritance[entry.GetKey(this)];

                            if (tp == InheritedType.inSelf || tp == InheritedType.inBothDiff ||
                                tp == InheritedType.inBothCopyNoRemove) {
                                ConfigurationElement elem = CallCreateNewElement(entry.GetKey(this).ToString());

                                elem.Unmerge(entry._value, null, saveMode);

                                if (tp == InheritedType.inSelf) {
                                    elem.RemoveAllInheritedLocks(); // If the key changed only local locks are kept
                                }

                                BaseAdd(elem,ThrowOnDuplicate,true);                     // Add it
                            }
                        }
                    }
                }
                else {
                    if (CollectionType == ConfigurationElementCollectionType.BasicMap ||
                        CollectionType == ConfigurationElementCollectionType.BasicMapAlternate) {
                        foreach (Entry entry in sourceCollection.Items) {
                            bool FoundKeyInParent = false;
                            Entry parentEntrySaved = null;

                            if (entry._entryType == EntryType.Added ||
                                entry._entryType == EntryType.Replaced) {
                                bool InParent = false;

                                if (parentCollection != null) {
                                    foreach (Entry parentEntry in parentCollection.Items) {
                                        if (Object.Equals(entry.GetKey(this), parentEntry.GetKey(this))) {
                                            // for basic map collection where the key is actually an element
                                            // we do not want the merging behavior or data will not get written
                                            // out for the properties if they match the first element deamed to be a parent
                                            // For example <allow verbs="NewVerb" users="*"/> will loose the users because
                                            // an entry exists in the root element.
                                            if (!IsElementName(entry.GetKey(this).ToString())) {
                                                // For elements which are not keyed by the element name
                                                // need to be unmerged
                                                FoundKeyInParent = true;
                                                parentEntrySaved = parentEntry;
                                            }
                                        }

                                        if (Object.Equals(entry._value, parentEntry._value)) {
                                            FoundKeyInParent = true;
                                            InParent = true;  // in parent and the same exact values
                                            parentEntrySaved = parentEntry;
                                            break;
                                        }
                                    }
                                }

                                ConfigurationElement elem = CallCreateNewElement(entry.GetKey(this).ToString());

                                if (!FoundKeyInParent) {   // Unmerge is similar to a reset when used like this
                                    // except that it handles the different update modes
                                    // which Reset does not understand
                                    elem.Unmerge(entry._value, null, saveMode); // copy this entry
                                    BaseAdd(-1, elem,true);                          // Add it
                                }
                                else {
                                    ConfigurationElement sourceItem = entry._value;
                                    if (!InParent ||
                                        (saveMode == ConfigurationSaveMode.Modified && sourceItem.IsModified()) ||
                                        (saveMode == ConfigurationSaveMode.Full)) {
                                        elem.Unmerge(entry._value, parentEntrySaved._value, saveMode);
                                        BaseAdd(-1, elem,true);                          // Add it
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected internal override void Reset(ConfigurationElement parentElement) {
            ConfigurationElementCollection parentCollection = parentElement as ConfigurationElementCollection;
            ResetLockLists(parentElement);

            if (parentCollection != null) {
                foreach (Entry entry in parentCollection.Items) {
                    ConfigurationElement elem = CallCreateNewElement(entry.GetKey(this).ToString());
                    elem.Reset(entry._value);

                    if ((CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap ||
                        CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) &&
                       (entry._entryType == EntryType.Added ||
                        entry._entryType == EntryType.Replaced)) { // do not add removed items from the parent
                        BaseAdd(elem, true, true); // This version combines dups and throws (unless overridden)
                    }
                    else {
                        if (CollectionType == ConfigurationElementCollectionType.BasicMap ||
                            CollectionType == ConfigurationElementCollectionType.BasicMapAlternate) {
                            BaseAdd(-1, elem, true); // this version appends regardless of if it is a dup.
                        }
                    }
                }
                _inheritedCount = Count; // After reset the count is the number of items actually inherited.
            }
        }

        public int Count {
            get {
                return _items.Count - _removedItemCount;
            }
        }

        public bool EmitClear {
            get {
                return bEmitClearTag;
            }
            set {
                if (IsReadOnly()) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_read_only));
                }
                if (value == true) {
                    CheckLockedElement(_clearElement, null);  // has clear been locked?
                    CheckLockedElement(_removeElement, null); // has remove been locked? Clear implies remove
                }
                bModified = true;
                bEmitClearTag = value;
            }
        }

        public bool IsSynchronized {
            get {
                return false;
            }
        }

        public Object SyncRoot {
            get {
                return null;
            }
        }

        public void CopyTo(ConfigurationElement[] array, int index) {
            ((ICollection)this).CopyTo(array, index);
        }

        void ICollection.CopyTo(Array arr, int index) {
            foreach (Entry entry in _items) {
                if (entry._entryType != EntryType.Removed) {
                    arr.SetValue(entry._value, index++);
                }
            }
        }

        public IEnumerator GetEnumerator() {
            return GetEnumeratorImpl();
        }

        protected virtual void BaseAdd(ConfigurationElement element) {
            BaseAdd(element, ThrowOnDuplicate);
        }

        protected internal void BaseAdd(ConfigurationElement element, bool throwIfExists) {
            BaseAdd(element, throwIfExists, false);
        }

        private void BaseAdd(ConfigurationElement element, bool throwIfExists, bool ignoreLocks) {
            bool flagAsReplaced = false;
            bool localAddToEnd = internalAddToEnd;

            if (IsReadOnly()) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_read_only));
            }

            if (LockItem == true && ignoreLocks == false) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_element_locked, _addElement));
            }

            Object key = GetElementKeyInternal(element);
            int iFoundItem = -1;
            for (int index = 0; index < _items.Count; index++) {
                Entry entry = (Entry)_items[index];
                if (CompareKeys(key, entry.GetKey(this))) {
                    if (entry._value != null && entry._value.LockItem == true && ignoreLocks == false) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_item_locked));
                    }
                    if (entry._entryType != EntryType.Removed && throwIfExists) {
                        if (!element.Equals(entry._value)) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_entry_already_exists, key),
                                element.PropertyFileName(""), element.PropertyLineNumber(""));
                        }
                        else {
                            entry._value = element;
                        }
                        return;
                    }
                    if (entry._entryType != EntryType.Added) {
                        if ((CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap ||
                             CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) &&
                            entry._entryType == EntryType.Removed &&
                            _removedItemCount > 0) {
                            _removedItemCount--; // account for the value
                        }
                        entry._entryType = EntryType.Replaced;
                        flagAsReplaced = true;
                    }
                    if (localAddToEnd ||
                        CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                        iFoundItem = index;
                        if (entry._entryType == EntryType.Added) {
                            // this is a special case for defect number 529517 to emulate everett behavior
                            localAddToEnd = true;
                        }
                        break;
                    }
                    else {
                        // check to see if the element is trying to set a locked property.
                        if (ignoreLocks == false) {
                            element.HandleLockedAttributes(entry._value);
                            // copy the lock from the removed element before setting the new element
                            element.MergeLocks(entry._value);
                        }
                        entry._value = element;
                        bModified = true;
                        return;
                    }
                }
            }

            // Brand new item.
            if (iFoundItem >= 0) {
                _items.RemoveAt(iFoundItem);

                // if the item being removed was inherited adjust the cout
                if (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate &&
                    iFoundItem > Count + _removedItemCount - _inheritedCount) {
                    _inheritedCount--;
                }
            }
            BaseAddInternal(localAddToEnd ? -1 : iFoundItem, element, flagAsReplaced, ignoreLocks);
            bModified = true;
        }

        protected int BaseIndexOf(ConfigurationElement element) {
            int index = 0;
            Object key = GetElementKeyInternal(element);
            foreach (Entry entry in _items) {
                if (entry._entryType != EntryType.Removed) {
                    if (CompareKeys(key, entry.GetKey(this))) {
                        return index;
                    }
                    index++;
                }
            }
            return -1;
        }

        internal int RealIndexOf(ConfigurationElement element) {
            int index = 0;
            Object key = GetElementKeyInternal(element);
            foreach (Entry entry in _items) {
                if (CompareKeys(key, entry.GetKey(this))) {
                    return index;
                }
                index++;
            }
            return -1;
        }

        private void BaseAddInternal(int index, ConfigurationElement element, bool flagAsReplaced, bool ignoreLocks) {
            // Allow the element to initialize itself after its
            // constructor has been run so that it may access
            // virtual methods.

            element.AssociateContext(_configRecord);
            if (element != null) {
                element.CallInit();
            }

            if (IsReadOnly()) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_read_only));
            }

            if (!ignoreLocks) { // during reset we ignore locks so we can copy the elements
                if(CollectionType == ConfigurationElementCollectionType.BasicMap ||
                    CollectionType == ConfigurationElementCollectionType.BasicMapAlternate) {
                    if (BaseConfigurationRecord.IsReservedAttributeName(ElementName)) {
                        throw new ArgumentException(SR.GetString(SR.Basicmap_item_name_reserved, ElementName));
                    }
                    CheckLockedElement(ElementName, null);
                }
                if(CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap ||
                    CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {

                    CheckLockedElement(_addElement, null);
                }
            }

            if (CollectionType == ConfigurationElementCollectionType.BasicMapAlternate ||
                CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                if (index == -1) {
                    index = Count + _removedItemCount - _inheritedCount; // insert before inherited, but after any removed
                }
                else {
                    if (index > Count + _removedItemCount - _inheritedCount && flagAsReplaced == false) {
                        throw (new ConfigurationErrorsException(SR.GetString(SR.Config_base_cannot_add_items_below_inherited_items)));
                    }
                }
            }

            if (CollectionType == ConfigurationElementCollectionType.BasicMap &&
                index >= 0 &&
                index < _inheritedCount) {
                throw (new ConfigurationErrorsException(SR.GetString(SR.Config_base_cannot_add_items_above_inherited_items)));
            }

            EntryType entryType = (flagAsReplaced == false) ? EntryType.Added : EntryType.Replaced;

            Object key = GetElementKeyInternal(element);

            if (index >= 0) {
                if (index > _items.Count) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.IndexOutOfRange, index));
                }
                _items.Insert(index, new Entry(entryType, key, element));
            }
            else {
                _items.Add(new Entry(entryType, key, element));
            }

            bModified = true;
        }


        protected virtual void BaseAdd(int index, ConfigurationElement element) {
            BaseAdd(index, element, false);
        }

        private void BaseAdd(int index, ConfigurationElement element, bool ignoreLocks) {
            if (IsReadOnly()) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_read_only));
            }
            if (index < -1) {
                throw new ConfigurationErrorsException(SR.GetString(SR.IndexOutOfRange, index));
            }

            if ((index != -1) &&
                (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap ||
                CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate)) {

                // If it's an AddRemoveClearMap*** collection, turn the index passed into into a real internal index
                int realIndex = 0;

                if (index > 0) {
                    foreach (Entry entryfound in _items) {
                        if (entryfound._entryType != EntryType.Removed) {
                            index--;
                        }
                        if (index == 0) {
                            break;
                        }
                        realIndex++;
                    }
                    index = ++realIndex;
                }
                // check for duplicates
                Object key = GetElementKeyInternal(element);
                foreach (Entry entry in _items) {
                    if (CompareKeys(key, entry.GetKey(this))) {
                        if (entry._entryType != EntryType.Removed) {
                            if (!element.Equals(entry._value)) {
                                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_entry_already_exists, key),
                                        element.PropertyFileName(""), element.PropertyLineNumber(""));
                            }
                            return;
                        }
                    }
                }

            }

            BaseAddInternal(index, element, false, ignoreLocks);
        }

        protected internal void BaseRemove(Object key) {
            BaseRemove(key, false);
        }

        private void BaseRemove(Object key, bool throwIfMissing) {
            if (IsReadOnly())
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_read_only));
            
            int index = 0;
            bool foundEntry = false;
            foreach (Entry entry in _items) {
                if (CompareKeys(key, entry.GetKey(this))) {
                    foundEntry = true;

                    if (entry._value == null) // A phoney delete is already present
                    {
                        if (throwIfMissing) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_entry_not_found, key));
                        }
                        else {
                            return;
                        }
                    }

                    if (entry._value.LockItem == true) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_attribute_locked, key));
                    }

                    if (entry._value.ElementPresent == false) {
                        CheckLockedElement(_removeElement, null); // has remove been locked?
                    }

                    switch (entry._entryType) {
                        case EntryType.Added:
                            if (CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap &&
                                CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                                if (CollectionType == ConfigurationElementCollectionType.BasicMapAlternate) {
                                    if (index >= Count - _inheritedCount) {
                                        throw (new ConfigurationErrorsException(SR.GetString(SR.Config_base_cannot_remove_inherited_items)));
                                    }
                                }
                                if (CollectionType == ConfigurationElementCollectionType.BasicMap) {
                                    if (index < _inheritedCount) {
                                        throw (new ConfigurationErrorsException(SR.GetString(SR.Config_base_cannot_remove_inherited_items)));
                                    }
                                }

                                _items.RemoveAt(index);
                            }
                            else {
                                // don't really remove it from the collection just mark it removed
                                entry._entryType = EntryType.Removed;
                                _removedItemCount++;
                            }
                            break;
                        case EntryType.Removed:
                            if (throwIfMissing) {
                                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_entry_already_removed));
                            }
                            break;
                        default:
                            if (CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap &&
                                CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_elements_may_not_be_removed));
                            }
                            entry._entryType = EntryType.Removed;
                            _removedItemCount++;
                            break;
                    }
                    bModified = true;
                    return;
                }
                index++;
            }
            // Note because it is possible for removes to get orphaned by the API they will
            // not cause a throw from the base classes.  The scenerio is:
            //  Add an item in a parent level
            //  remove the item in a child level
            //  remove the item at the parent level.
            //
            // throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_entry_not_found));
            if (foundEntry == false) {
                if (throwIfMissing) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_entry_not_found, key));
                }

                if (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap ||
                    CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                    if (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                        _items.Insert(Count + _removedItemCount - _inheritedCount, new Entry(EntryType.Removed, key, null));
                    }
                    else {
                        _items.Add(new Entry(EntryType.Removed, key, null));
                    }
                    _removedItemCount++;
                }

            }
        }

        protected internal ConfigurationElement BaseGet(Object key) {
            foreach (Entry entry in _items) {
                if (entry._entryType != EntryType.Removed) {
                    if (CompareKeys(key, entry.GetKey(this))) {
                        return entry._value;
                    }
                }
            }
            return null;
        }

        protected internal bool BaseIsRemoved(Object key) {
            foreach (Entry entry in _items) {
                if (CompareKeys(key, entry.GetKey(this))) {
                    if (entry._entryType == EntryType.Removed) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
            return false;
        }

        protected internal ConfigurationElement BaseGet(int index) {
            if (index < 0) {
                throw new ConfigurationErrorsException(SR.GetString(SR.IndexOutOfRange, index));
            }

            int VirtualIndex = 0;
            Entry entry = (Entry)null;

            foreach (Entry entryfound in _items) {
                if (VirtualIndex == index && (entryfound._entryType != EntryType.Removed)) {
                    entry = entryfound;
                    break;
                }
                if (entryfound._entryType != EntryType.Removed) {
                    VirtualIndex++;
                }
            }

            if (entry != null) {
                return entry._value;
            }
            else {
                throw new ConfigurationErrorsException(SR.GetString(SR.IndexOutOfRange, index));
            }
        }

        protected internal object[] BaseGetAllKeys() {
            object[] keys = new object[Count];
            int index = 0;
            foreach (Entry entry in _items) {
                if (entry._entryType != EntryType.Removed) {
                    keys[index] = entry.GetKey(this);
                    index++;
                }
            }
            return keys;
        }

        protected internal object BaseGetKey(int index) {
            int VirtualIndex = 0;
            Entry entry = (Entry)null;
            if (index < 0) {
                throw new ConfigurationErrorsException(SR.GetString(SR.IndexOutOfRange, index));
            }

            foreach (Entry entryfound in _items) {
                if (VirtualIndex == index && (entryfound._entryType != EntryType.Removed)) {
                    entry = entryfound;
                    break;
                }

                if (entryfound._entryType != EntryType.Removed) {
                    VirtualIndex++;
                }
            }

            // Entry entry = (Entry)_items[index];
            if (entry != null) {
                object key = entry.GetKey(this);

                return key;
            }
            else {
                throw new ConfigurationErrorsException(SR.GetString(SR.IndexOutOfRange, index));
            }
        }

        protected internal void BaseClear() {
            if (IsReadOnly()) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_read_only));
            }

            CheckLockedElement(_clearElement, null);  // has clear been locked?
            CheckLockedElement(_removeElement, null); // has remove been locked? Clear implies remove

            bModified = true;
            bCollectionCleared = true;
            if ((CollectionType == ConfigurationElementCollectionType.BasicMap ||
                 CollectionType == ConfigurationElementCollectionType.BasicMapAlternate)
                && _inheritedCount > 0) {
                int RemoveIndex = 0;
                if (CollectionType == ConfigurationElementCollectionType.BasicMapAlternate) {
                    RemoveIndex = 0; // Inherited items are at the bottom and cannot be removed
                }
                if (CollectionType == ConfigurationElementCollectionType.BasicMap) {
                    RemoveIndex = _inheritedCount; // inherited items are at the top and cannot be removed
                }
                while (Count - _inheritedCount > 0) {
                    _items.RemoveAt(RemoveIndex);
                }
            }
            else {
                // do not clear any locked items
                // _items.Clear();
                int inheritedRemoved = 0;
                int removedRemoved = 0;
                int initialCount = Count;
                
                // check for locks before removing any items from the collection
                for (int CheckIndex = 0; CheckIndex < _items.Count; CheckIndex++) {
                    Entry entry = (Entry)_items[CheckIndex];
                    if (entry._value != null && entry._value.LockItem == true) {
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_item_locked_cannot_clear));
                    }
                }

                for (int RemoveIndex = _items.Count - 1; RemoveIndex >= 0; RemoveIndex--) {
                    Entry entry = (Entry)_items[RemoveIndex];
                    if ((CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap &&
                            RemoveIndex < _inheritedCount) ||
                        (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate &&
                            (RemoveIndex >= initialCount - _inheritedCount))) {
                        inheritedRemoved++;
                    }
                    if (entry._entryType == EntryType.Removed) {
                        removedRemoved++;
                    }

                    _items.RemoveAt(RemoveIndex);
                }
                _inheritedCount -= inheritedRemoved;
                _removedItemCount -= removedRemoved;
            }
        }

        protected internal void BaseRemoveAt(int index) {
            if (IsReadOnly()) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_read_only));
            }
            int VirtualIndex = 0;
            Entry entry = (Entry)null;

            foreach (Entry entryfound in _items) {
                if (VirtualIndex == index && (entryfound._entryType != EntryType.Removed)) {
                    entry = entryfound;
                    break;
                }

                if (entryfound._entryType != EntryType.Removed) {
                    VirtualIndex++;
                }
            }

            if (entry == null) {
                throw new ConfigurationErrorsException(SR.GetString(SR.IndexOutOfRange, index));
            }
            else {
                if (entry._value.LockItem == true) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_attribute_locked, entry.GetKey(this)));
                }

                if (entry._value.ElementPresent == false) {
                    CheckLockedElement(_removeElement, null); // has remove been locked?
                }

                switch (entry._entryType) {
                    case EntryType.Added:
                        if (CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap &&
                            CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                            if (CollectionType == ConfigurationElementCollectionType.BasicMapAlternate) {
                                if (index >= Count - _inheritedCount) {
                                    throw (new ConfigurationErrorsException(SR.GetString(SR.Config_base_cannot_remove_inherited_items)));
                                }
                            }

                            if (CollectionType == ConfigurationElementCollectionType.BasicMap) {
                                if (index < _inheritedCount) {
                                    throw (new ConfigurationErrorsException(SR.GetString(SR.Config_base_cannot_remove_inherited_items)));
                                }
                            }

                            _items.RemoveAt(index);

                        }
                        else {
                            // don't really remove it from the collection just mark it removed
                            if (entry._value.ElementPresent == false) {
                                CheckLockedElement(_removeElement, null); // has remove been locked?
                            }

                            entry._entryType = EntryType.Removed;
                            _removedItemCount++;
                        }

                        break;

                    case EntryType.Removed:
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_entry_already_removed));

                    default:
                        if (CollectionType != ConfigurationElementCollectionType.AddRemoveClearMap &&
                            CollectionType != ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_collection_elements_may_not_be_removed));
                        }

                        entry._entryType = EntryType.Removed;
                        _removedItemCount++;
                        break;
                }
                bModified = true;
            }
        }



        protected internal override bool SerializeElement(XmlWriter writer, bool serializeCollectionKey) {
            ConfigurationElementCollectionType type = CollectionType;
            bool DataToWrite = false;

            DataToWrite |= base.SerializeElement(writer, serializeCollectionKey);

            if (type == ConfigurationElementCollectionType.AddRemoveClearMap ||
                type == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                // it is possible that the collection only has to be cleared and contains
                // no real elements
                if (bEmitClearTag == true && (_clearElement.Length != 0)) {
                    if (writer != null) {
                        writer.WriteStartElement(_clearElement);
                        writer.WriteEndElement();
                    }
                    DataToWrite = true;
                }
            }

            foreach (Entry entry in _items) {
                if (type == ConfigurationElementCollectionType.BasicMap ||
                    type == ConfigurationElementCollectionType.BasicMapAlternate) {
                    if (entry._entryType == EntryType.Added || entry._entryType == EntryType.Replaced) {
                        if (ElementName != null && ElementName.Length != 0) {
                            if (BaseConfigurationRecord.IsReservedAttributeName(ElementName)) {
                                throw new ArgumentException(SR.GetString(SR.Basicmap_item_name_reserved, ElementName));
                            }
                            DataToWrite |= entry._value.SerializeToXmlElement(writer, ElementName);
                        }
                        else {
                            DataToWrite |= entry._value.SerializeElement(writer, false);
                        }
                    }
                }
                else if (type == ConfigurationElementCollectionType.AddRemoveClearMap ||
                         type == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                    if ((entry._entryType == EntryType.Removed ||
                         entry._entryType == EntryType.Replaced) &&
                        entry._value != null) {

                        if (writer != null) {
                            writer.WriteStartElement(_removeElement);
                        }

                        DataToWrite |= entry._value.SerializeElement(writer, true);

                        if (writer != null) {
                            writer.WriteEndElement();
                        }

                        DataToWrite = true;
                    }
                    if (entry._entryType == EntryType.Added || entry._entryType == EntryType.Replaced) {
                        DataToWrite |= entry._value.SerializeToXmlElement(writer, _addElement);
                    }
                }
            }
            return DataToWrite;
        }

        protected override bool OnDeserializeUnrecognizedElement(String elementName, XmlReader reader) {
            bool handled = false; //
            if (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap ||
                CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                if (elementName == _addElement) {
                    ConfigurationElement elem = CallCreateNewElement();
                    elem.ResetLockLists(this);
                    elem.DeserializeElement(reader, false);
                    BaseAdd(elem);
                    handled = true;
                }
                else if (elementName == _removeElement) {
                    ConfigurationElement elem = CallCreateNewElement();
                    elem.ResetLockLists(this);
                    elem.DeserializeElement(reader, true);
                    if (IsElementRemovable(elem) == true) {
                        BaseRemove(GetElementKeyInternal(elem), false);
                    }

                    handled = true;
                }
                else if (elementName == _clearElement) {
                    if (reader.AttributeCount > 0) {
                        while (reader.MoveToNextAttribute()) {
                            String propertyName = reader.Name;
                            throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_unrecognized_attribute, propertyName), reader);
                        }
                    }
                    CheckLockedElement(elementName, reader);
                    reader.MoveToElement();
                    BaseClear(); //
                    bEmitClearTag = true;
                    handled = true;
                }
            }
            else if (elementName == ElementName) {
                if (BaseConfigurationRecord.IsReservedAttributeName(elementName)) {
                    throw new ArgumentException(SR.GetString(SR.Basicmap_item_name_reserved, elementName));
                }
                ConfigurationElement elem = CallCreateNewElement();
                elem.ResetLockLists(this);
                elem.DeserializeElement(reader, false);
                BaseAdd(elem);

                handled = true;
            }
            else if (IsElementName(elementName)) {   // this section handle the collection like the allow deny senario which
                if (BaseConfigurationRecord.IsReservedAttributeName(elementName)) {
                    throw new ArgumentException(SR.GetString(SR.Basicmap_item_name_reserved, elementName));
                }
                // have multiple tags for the collection
                ConfigurationElement elem = CallCreateNewElement(elementName);
                elem.ResetLockLists(this);
                elem.DeserializeElement(reader, false);
                BaseAdd(-1, elem);
                handled = true;
            }
            return handled;
        }

        private ConfigurationElement CallCreateNewElement(string elementName) {
            ConfigurationElement elem = CreateNewElement(elementName);
            elem.AssociateContext(_configRecord);
            elem.CallInit();
            return elem;
        }

        private ConfigurationElement CallCreateNewElement() {
            ConfigurationElement elem = CreateNewElement();
            elem.AssociateContext(_configRecord);
            elem.CallInit();
            return elem;
        }

        protected virtual ConfigurationElement CreateNewElement(string elementName) {
            return CreateNewElement();
        }
        protected abstract ConfigurationElement CreateNewElement();
        protected abstract Object GetElementKey(ConfigurationElement element);
        internal Object GetElementKeyInternal(ConfigurationElement element) {
            Object key = GetElementKey(element);
            if (key == null)
                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_invalid_element_key));
            return key;
        }

        protected virtual bool IsElementRemovable(ConfigurationElement element) {
            return true;
        }

        private bool CompareKeys(Object key1, Object key2) {
            if (_comparer != null) {
                return (_comparer.Compare(key1, key2) == 0);
            }
            else {
                return key1.Equals(key2);
            }
        }

        protected virtual String ElementName {
            get {
                return "";
            }
        }

        protected virtual bool IsElementName(string elementName) {
            return false;
        }

        internal bool IsLockableElement(string elementName) {
            if (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap ||
                CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                return (elementName == AddElementName ||
                        elementName == RemoveElementName ||
                        elementName == ClearElementName);
            }
            else {
                return (elementName == ElementName) || IsElementName(elementName);
            }
        }

        internal string LockableElements {
            get {
                if (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap ||
                    CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {

                    string ElementNames = "'" + AddElementName + "'"; // Must have an add
                    if (RemoveElementName.Length != 0)
                        ElementNames += ", '" + RemoveElementName + "'";
                    if (ClearElementName.Length != 0)
                        ElementNames += ", '" + ClearElementName + "'";
                    return ElementNames;
                }
                else {
                    if (!String.IsNullOrEmpty(ElementName)) {
                        return "'" + ElementName + "'";
                    }
                    return String.Empty;
                }
            }
        }

        protected virtual bool ThrowOnDuplicate {
            get {
                if (CollectionType == ConfigurationElementCollectionType.AddRemoveClearMap ||
                    CollectionType == ConfigurationElementCollectionType.AddRemoveClearMapAlternate) {
                    return true;
                }
                return false;
            }
        }

        public virtual ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        private enum EntryType {
            Inherited,
            Replaced,
            Removed,
            Added,
        }

        private class Entry {

            internal EntryType _entryType;
            internal Object _key;
            internal ConfigurationElement _value;

            internal Object GetKey(ConfigurationElementCollection ThisCollection) {
                // For items that have been really inserted...
                if (_value != null) {
                    return ThisCollection.GetElementKeyInternal(_value);
                }
                else {
                    return _key; // These are items that only have been removed
                }

            }

            internal Entry(EntryType type, Object key, ConfigurationElement value) {
                _entryType = type;
                _key = key;
                _value = value;
            }
        }

        private class Enumerator : IDictionaryEnumerator {

            private IEnumerator _itemsEnumerator;
            private DictionaryEntry _current = new DictionaryEntry();
            private ConfigurationElementCollection ThisCollection;


            internal Enumerator(ArrayList items, ConfigurationElementCollection collection) {
                _itemsEnumerator = items.GetEnumerator();
                ThisCollection = collection;
            }

            bool IEnumerator.MoveNext() {
                while (_itemsEnumerator.MoveNext()) {
                    Entry entry = (Entry)_itemsEnumerator.Current;
                    if (entry._entryType != EntryType.Removed) {
                        _current.Key = (entry.GetKey(ThisCollection) != null) ? entry.GetKey(ThisCollection) : "key";
                        _current.Value = entry._value;
                        return true;
                    }
                }
                return false;
            }

            void IEnumerator.Reset() {
                _itemsEnumerator.Reset();
            }

            Object IEnumerator.Current {
                get {
                    return _current.Value;
                }
            }

            DictionaryEntry IDictionaryEnumerator.Entry {
                get {
                    return _current;
                }
            }

            Object IDictionaryEnumerator.Key {
                get {
                    return _current.Key;
                }
            }

            Object IDictionaryEnumerator.Value {
                get {
                    return _current.Value;
                }
            }
        }
    }
}
