//---------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
// 
//---------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections;
using System.Windows;

#if WINDOWS_BASE
    using MS.Internal.WindowsBase;
#elif PRESENTATION_CORE
    using MS.Internal.PresentationCore;
#elif PRESENTATIONFRAMEWORK
    using MS.Internal.PresentationFramework;
#elif DRT
    using MS.Internal.Drt;
#else
#error Attempt to use FriendAccessAllowedAttribute from an unknown assembly.
using MS.Internal.YourAssemblyName;
#endif

namespace MS.Utility
{
    // These classes implement a frugal storage model for key/value pair data
    // structures. The keys are integers, and the values objects.
    // Performance measurements show that Avalon has many maps that contain a
    // single key/value pair. Therefore these classes are structured to prefer
    // a map that contains a single key/value pair and uses a conservative
    // growth strategy to minimize the steady state memory footprint. To enforce
    // the slow growth the map does not allow the user to set the capacity.
    // Also note that the map uses one fewer objects than the BCL HashTable and
    // does no allocations at all until an item is inserted into the map.
    //
    // The code is also structured to perform well from a CPU standpoint. Perf
    // analysis of DependencyObject showed that we used a single entry 63% of 
    // the time and growth tailed off quickly. Average access times are 8 to 16
    // times faster than a BCL Hashtable. 
    //
    // FrugalMap is appropriate for small maps or maps that grow slowly. Its
    // primary focus is for maps that contain fewer than 64 entries and that
    // usually start with no entries, or a single entry. If you know your map
    // will always have a minimum of 64 or more entires FrugalMap *may* not
    // be the best choice. Choose your collections wisely and pay particular
    // attention to the growth patterns and search methods.

    // This enum controls the growth to successively more complex storage models
    internal enum FrugalMapStoreState
    {
        Success,
        ThreeObjectMap,
        SixObjectMap,
        Array,
        SortedArray,
        Hashtable
    }

    abstract class FrugalMapBase
    {
        public abstract FrugalMapStoreState InsertEntry(int key, Object value);

        public abstract void RemoveEntry(int key);

        /// <summary>
        /// Looks for an entry that contains the given key, null is returned if the
        /// key is not found.
        /// </summary>
        public abstract Object Search(int key);


        /// <summary>
        /// A routine used by enumerators that need a sorted map
        /// </summary>
        public abstract void Sort();

        /// <summary>
        /// A routine used by enumerators to iterate through the map
        /// </summary>
        public abstract void GetKeyValuePair(int index, out int key, out Object value);

        /// <summary>
        /// A routine used to iterate through all the entries in the map
        /// </summary>
        public abstract void Iterate(ArrayList list, FrugalMapIterationCallback callback);

        /// <summary>
        /// Promotes the key/value pairs in the current collection to the next larger
        /// and more complex storage model.
        /// </summary>
        public abstract void Promote(FrugalMapBase newMap);

        /// <summary>
        /// Size of this data store
        /// </summary>
        public abstract int Count
        {
            get;
        }

        protected const int INVALIDKEY = 0x7FFFFFFF;

        internal struct Entry
        {
            public int Key;
            public Object Value;
        }
    }

    /// <summary>
    /// A simple class to handle a single key/value pair
    /// </summary>
    internal sealed class SingleObjectMap : FrugalMapBase
    {
        public SingleObjectMap()
        {
            _loneEntry.Key = INVALIDKEY;
            _loneEntry.Value = DependencyProperty.UnsetValue;
        }

        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            // If we don't have any entries or the existing entry is being overwritten,
            // then we can use this map.  Otherwise we have to promote.
            if ((INVALIDKEY == _loneEntry.Key) || (key == _loneEntry.Key))
            {
                Debug.Assert(INVALIDKEY != key);

                _loneEntry.Key = key;
                _loneEntry.Value = value;
                return FrugalMapStoreState.Success;
            }
            else
            {
                // Entry already used, move to an ThreeObjectMap
                return FrugalMapStoreState.ThreeObjectMap;
            }
        }

        public override void RemoveEntry(int key)
        {
            // Wipe out the info in the only entry if it matches the key.
            if (key == _loneEntry.Key)
            {
                _loneEntry.Key = INVALIDKEY;
                _loneEntry.Value = DependencyProperty.UnsetValue;
            }
        }

        public override Object Search(int key)
        {
            if (key == _loneEntry.Key)
            {
                return _loneEntry.Value;
            }
            return DependencyProperty.UnsetValue;
        }

        public override void Sort()
        {
            // Single items are already sorted.
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (0 == index)
            {
                value = _loneEntry.Value;
                key = _loneEntry.Key;
            }
            else
            {
                value = DependencyProperty.UnsetValue;
                key = INVALIDKEY;
                throw new ArgumentOutOfRangeException("index");
            }
        }
        
        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (Count == 1)
            {
                callback(list, _loneEntry.Key, _loneEntry.Value);
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            if (FrugalMapStoreState.Success == newMap.InsertEntry(_loneEntry.Key, _loneEntry.Value))
            {
            }
            else
            {
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
        }

        // Size of this data store
        public override int Count
        {
            get
            {
                if (INVALIDKEY != _loneEntry.Key)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        private Entry _loneEntry;
    }


    /// <summary>
    /// A simple class to handle a single object with 3 key/value pairs.  The pairs are stored unsorted
    /// and uses a linear search.  Perf analysis showed that this yielded better memory locality and
    /// perf than an object and an array.
    /// </summary>
    /// <remarks>
    /// This map inserts at the last position.  Any time we add to the map we set _sorted to false. If you need
    /// to iterate through the map in sorted order you must call Sort before using GetKeyValuePair.
    /// </remarks>
    internal sealed class ThreeObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            // Check to see if we are updating an existing entry
            Debug.Assert(INVALIDKEY != key);

            // First check if the key matches the key of one of the existing entries.
            // If it does, overwrite the existing value and return success.
            switch (_count)
            {
                case 1:
                    if (_entry0.Key == key)
                    {
                        _entry0.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    break;

                case 2:
                    if (_entry0.Key == key)
                    {
                        _entry0.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    break;

                case 3:
                    if (_entry0.Key == key)
                    {
                        _entry0.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    break;

                default:
                    break;
            }

            // If we got past the above switch, that means this key
            // doesn't exist in the map already so we should add it.
            // Only add it if we're not at the size limit; otherwise
            // we have to promote.
            if (SIZE > _count)
            {
                // Space still available to store the value. Insert
                // into the entry at _count (the next available slot).
                switch (_count)
                {
                    case 0:
                        _entry0.Key = key;
                        _entry0.Value = value;
                        _sorted = true;
                        break;

                    case 1:
                        _entry1.Key = key;
                        _entry1.Value = value;
                        // We have added an entry to the array, so we may not be sorted any longer
                        _sorted = false;
                        break;

                    case 2:
                        _entry2.Key = key;
                        _entry2.Value = value;
                        // We have added an entry to the array, so we may not be sorted any longer
                        _sorted = false;
                        break;
                }
                ++_count;

                return FrugalMapStoreState.Success;
            }
            else
            {
                // Array is full, move to a SixObjectMap
                return FrugalMapStoreState.SixObjectMap;
            }
        }

        public override void RemoveEntry(int key)
        {
            // If the key matches an existing entry, wipe out the last 
            // entry and move all the other entries up.  Because we only
            // have three entries we can just unravel all the cases.
            switch (_count)
            {
                case 1:
                    if (_entry0.Key == key)
                    {
                        _entry0.Key = INVALIDKEY;
                        _entry0.Value = DependencyProperty.UnsetValue;
                        --_count;
                        return;
                    }
                    break;

                case 2:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1.Key = INVALIDKEY;
                        _entry1.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1.Key = INVALIDKEY;
                        _entry1.Value = DependencyProperty.UnsetValue;
                        --_count;
                    }
                    break;

                case 3:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1 = _entry2;
                        _entry2.Key = INVALIDKEY;
                        _entry2.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1 = _entry2;
                        _entry2.Key = INVALIDKEY;
                        _entry2.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2.Key = INVALIDKEY;
                        _entry2.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    break;

                default:
                    break;
            }
        }

        public override Object Search(int key)
        {
            Debug.Assert(INVALIDKEY != key);
            if (_count > 0)
            {
                if (_entry0.Key == key)
                {
                    return _entry0.Value;
                }
                if (_count > 1)
                {
                    if (_entry1.Key == key)
                    {
                        return _entry1.Value;
                    }
                    if ((_count > 2) && (_entry2.Key == key))
                    {
                        return _entry2.Value;
                    }
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public override void Sort()
        {
            // If we're unsorted and we have entries to sort, do a simple
            // sort.  Sort the pairs (0,1), (1,2) and then (0,1) again.  
            if ((false == _sorted) && (_count > 1))
            {
                Entry temp;
                if (_entry0.Key > _entry1.Key)
                {
                    temp = _entry0;
                    _entry0 = _entry1;
                    _entry1 = temp;
                }
                if (_count > 2)
                {
                    if (_entry1.Key > _entry2.Key)
                    {
                        temp = _entry1;
                        _entry1 = _entry2;
                        _entry2 = temp;

                        if (_entry0.Key > _entry1.Key)
                        {
                            temp = _entry0;
                            _entry0 = _entry1;
                            _entry1 = temp;
                        }
                    }
                }
                _sorted = true;
            }
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _count)
            {
                switch (index)
                {
                    case 0:
                        key = _entry0.Key;
                        value = _entry0.Value;
                        break;

                    case 1:
                        key = _entry1.Key;
                        value = _entry1.Value;
                        break;

                    case 2:
                        key = _entry2.Key;
                        value = _entry2.Value;
                        break;

                    default:
                        key = INVALIDKEY;
                        value = DependencyProperty.UnsetValue;
                        break;
                }
            }
            else
            {
                key = INVALIDKEY;
                value = DependencyProperty.UnsetValue;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (_count > 0)
            {
                if (_count >= 1)
                {
                    callback(list, _entry0.Key, _entry0.Value);
                }
                if (_count >= 2)
                {
                    callback(list, _entry1.Key, _entry1.Value);
                }
                if (_count == 3)
                {
                    callback(list, _entry2.Key, _entry2.Value);
                }
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry0.Key, _entry0.Value))
            {
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry1.Key, _entry1.Value))
            {
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry2.Key, _entry2.Value))
            {
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
        }

        // Size of this data store
        public override int Count
        {
            get
            {
                return _count;
            }
        }

        private const int SIZE = 3;

        // The number of items in the map.
        private UInt16 _count;

        private bool _sorted;
        private Entry _entry0;
        private Entry _entry1;
        private Entry _entry2;
    }

    /// <summary>
    /// A simple class to handle a single object with 6 key/value pairs.  The pairs are stored unsorted
    /// and uses a linear search.  Perf analysis showed that this yielded better memory locality and
    /// perf than an object and an array.
    /// </summary>
    /// <remarks>
    /// This map inserts at the last position.  Any time we add to the map we set _sorted to false. If you need
    /// to iterate through the map in sorted order you must call Sort before using GetKeyValuePair.
    /// </remarks>
    internal sealed class SixObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            // Check to see if we are updating an existing entry
            Debug.Assert(INVALIDKEY != key);

            // First check if the key matches the key of one of the existing entries.
            // If it does, overwrite the existing value and return success.
            if (_count > 0)
            {
                if (_entry0.Key == key)
                {
                    _entry0.Value = value;
                    return FrugalMapStoreState.Success;
                }
                if (_count > 1)
                {
                    if (_entry1.Key == key)
                    {
                        _entry1.Value = value;
                        return FrugalMapStoreState.Success;
                    }
                    if (_count > 2)
                    {
                        if (_entry2.Key == key)
                        {
                            _entry2.Value = value;
                            return FrugalMapStoreState.Success;
                        }
                        if (_count > 3)
                        {
                            if (_entry3.Key == key)
                            {
                                _entry3.Value = value;
                                return FrugalMapStoreState.Success;
                            }
                            if (_count > 4)
                            {
                                if (_entry4.Key == key)
                                {
                                    _entry4.Value = value;
                                    return FrugalMapStoreState.Success;
                                }
                                if ((_count > 5) && (_entry5.Key == key))
                                {
                                    _entry5.Value = value;
                                    return FrugalMapStoreState.Success;
                                }
                            }
                        }
                    }
                }
            }

            // If we got past the above switch, that means this key
            // doesn't exist in the map already so we should add it.
            // Only add it if we're not at the size limit; otherwise
            // we have to promote.
            if (SIZE > _count)
            {
                // We are adding an entry to the array, so we may not be sorted any longer
                _sorted = false;
                
                // Space still available to store the value. Insert
                // into the entry at _count (the next available slot).
                switch (_count)
                {
                    case 0:
                        _entry0.Key = key;
                        _entry0.Value = value;

                        // Single entries are always sorted
                        _sorted = true;
                        break;

                    case 1:
                        _entry1.Key = key;
                        _entry1.Value = value;
                        break;

                    case 2:
                        _entry2.Key = key;
                        _entry2.Value = value;
                        break;

                    case 3:
                        _entry3.Key = key;
                        _entry3.Value = value;
                        break;

                    case 4:
                        _entry4.Key = key;
                        _entry4.Value = value;
                        break;

                    case 5:
                        _entry5.Key = key;
                        _entry5.Value = value;
                        break;
                }
                ++_count;

                return FrugalMapStoreState.Success;
            }
            else
            {
                // Array is full, move to a Array
                return FrugalMapStoreState.Array;
            }
        }

        public override void RemoveEntry(int key)
        {
            // If the key matches an existing entry, wipe out the last 
            // entry and move all the other entries up.  Because we only
            // have three entries we can just unravel all the cases.
            switch (_count)
            {
                case 1:
                    if (_entry0.Key == key)
                    {
                        _entry0.Key = INVALIDKEY;
                        _entry0.Value = DependencyProperty.UnsetValue;
                        --_count;
                        return;
                    }
                    break;

                case 2:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1.Key = INVALIDKEY;
                        _entry1.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1.Key = INVALIDKEY;
                        _entry1.Value = DependencyProperty.UnsetValue;
                        --_count;
                    }
                    break;

                case 3:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1 = _entry2;
                        _entry2.Key = INVALIDKEY;
                        _entry2.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1 = _entry2;
                        _entry2.Key = INVALIDKEY;
                        _entry2.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2.Key = INVALIDKEY;
                        _entry2.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    break;

                case 4:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3.Key = INVALIDKEY;
                        _entry3.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3.Key = INVALIDKEY;
                        _entry3.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2 = _entry3;
                        _entry3.Key = INVALIDKEY;
                        _entry3.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry3.Key == key)
                    {
                        _entry3.Key = INVALIDKEY;
                        _entry3.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    break;

                case 5:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4.Key = INVALIDKEY;
                        _entry4.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4.Key = INVALIDKEY;
                        _entry4.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4.Key = INVALIDKEY;
                        _entry4.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry3.Key == key)
                    {
                        _entry3 = _entry4;
                        _entry4.Key = INVALIDKEY;
                        _entry4.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry4.Key == key)
                    {
                        _entry4.Key = INVALIDKEY;
                        _entry4.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    break;

                case 6:
                    if (_entry0.Key == key)
                    {
                        _entry0 = _entry1;
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4 = _entry5;
                        _entry5.Key = INVALIDKEY;
                        _entry5.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry1.Key == key)
                    {
                        _entry1 = _entry2;
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4 = _entry5;
                        _entry5.Key = INVALIDKEY;
                        _entry5.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry2.Key == key)
                    {
                        _entry2 = _entry3;
                        _entry3 = _entry4;
                        _entry4 = _entry5;
                        _entry5.Key = INVALIDKEY;
                        _entry5.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry3.Key == key)
                    {
                        _entry3 = _entry4;
                        _entry4 = _entry5;
                        _entry5.Key = INVALIDKEY;
                        _entry5.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry4.Key == key)
                    {
                        _entry4 = _entry5;
                        _entry5.Key = INVALIDKEY;
                        _entry5.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    if (_entry5.Key == key)
                    {
                        _entry5.Key = INVALIDKEY;
                        _entry5.Value = DependencyProperty.UnsetValue;
                        --_count;
                        break;
                    }
                    break;

                default:
                    break;
            }
        }

        public override Object Search(int key)
        {
            Debug.Assert(INVALIDKEY != key);
            if (_count > 0)
            {
                if (_entry0.Key == key)
                {
                    return _entry0.Value;
                }
                if (_count > 1)
                {
                    if (_entry1.Key == key)
                    {
                        return _entry1.Value;
                    }
                    if (_count > 2)
                    {
                        if (_entry2.Key == key)
                        {
                            return _entry2.Value;
                        }
                        if (_count > 3)
                        {
                            if (_entry3.Key == key)
                            {
                                return _entry3.Value;
                            }
                            if (_count > 4)
                            {
                                if (_entry4.Key == key)
                                {
                                    return _entry4.Value;
                                }
                                if ((_count > 5) && (_entry5.Key == key))
                                {
                                    return _entry5.Value;
                                }
                            }
                        }
                    }
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public override void Sort()
        {
            // If we're unsorted and we have entries to sort, do a simple
            // bubble sort. Sort the pairs, 0..5, and then again until we no
            // longer do any swapping.
            if ((false == _sorted) && (_count > 1))
            {
                bool swapped;

                do
                {
                    swapped = false;

                    Entry temp;
                    if (_entry0.Key > _entry1.Key)
                    {
                        temp = _entry0;
                        _entry0 = _entry1;
                        _entry1 = temp;
                        swapped = true;
                    }
                    if (_count > 2)
                    {
                        if (_entry1.Key > _entry2.Key)
                        {
                            temp = _entry1;
                            _entry1 = _entry2;
                            _entry2 = temp;
                            swapped = true;
                        }
                        if (_count > 3)
                        {
                            if (_entry2.Key > _entry3.Key)
                            {
                                temp = _entry2;
                                _entry2 = _entry3;
                                _entry3 = temp;
                                swapped = true;
                            }
                            if (_count > 4)
                            {
                                if (_entry3.Key > _entry4.Key)
                                {
                                    temp = _entry3;
                                    _entry3 = _entry4;
                                    _entry4 = temp;
                                    swapped = true;
                                }
                                if (_count > 5)
                                {
                                    if (_entry4.Key > _entry5.Key)
                                    {
                                        temp = _entry4;
                                        _entry4 = _entry5;
                                        _entry5 = temp;
                                        swapped = true;
                                    }
                                }
                            }
                        }
                    }
                }
                while (swapped);
                _sorted = true;
            }
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _count)
            {
                switch (index)
                {
                    case 0:
                        key = _entry0.Key;
                        value = _entry0.Value;
                        break;

                    case 1:
                        key = _entry1.Key;
                        value = _entry1.Value;
                        break;

                    case 2:
                        key = _entry2.Key;
                        value = _entry2.Value;
                        break;

                    case 3:
                        key = _entry3.Key;
                        value = _entry3.Value;
                        break;

                    case 4:
                        key = _entry4.Key;
                        value = _entry4.Value;
                        break;

                    case 5:
                        key = _entry5.Key;
                        value = _entry5.Value;
                        break;

                    default:
                        key = INVALIDKEY;
                        value = DependencyProperty.UnsetValue;
                        break;
                }
            }
            else
            {
                key = INVALIDKEY;
                value = DependencyProperty.UnsetValue;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (_count > 0)
            {
                if (_count >= 1)
                {
                    callback(list, _entry0.Key, _entry0.Value);
                }
                if (_count >= 2)
                {
                    callback(list, _entry1.Key, _entry1.Value);
                }
                if (_count >= 3)
                {
                    callback(list, _entry2.Key, _entry2.Value);
                }
                if (_count >= 4)
                {
                    callback(list, _entry3.Key, _entry3.Value);
                }
                if (_count >= 5)
                {
                    callback(list, _entry4.Key, _entry4.Value);
                }
                if (_count == 6)
                {
                    callback(list, _entry5.Key, _entry5.Value);
                }
            }
        }
        
        public override void Promote(FrugalMapBase newMap)
        {
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry0.Key, _entry0.Value))
            {
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry1.Key, _entry1.Value))
            {
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry2.Key, _entry2.Value))
            {
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry3.Key, _entry3.Value))
            {
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry4.Key, _entry4.Value))
            {
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
            if (FrugalMapStoreState.Success != newMap.InsertEntry(_entry5.Key, _entry5.Value))
            {
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
        }

        // Size of this data store
        public override int Count
        {
            get
            {
                return _count;
            }
        }

        private const int SIZE = 6;

        // The number of items in the map.
        private UInt16 _count;

        private bool _sorted;
        private Entry _entry0;
        private Entry _entry1;
        private Entry _entry2;
        private Entry _entry3;
        private Entry _entry4;
        private Entry _entry5;
    }

    /// <summary>
    /// A simple class to handle an array of between 6 and 12 key/value pairs.  It is unsorted
    /// and uses a linear search.  Perf analysis showed that this was the optimal size for both
    /// memory and perf.  The values may need to be adjusted as the CLR and Avalon evolve.
    /// </summary>
    internal sealed class ArrayObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            // Check to see if we are updating an existing entry
            for (int index = 0; index < _count; ++index)
            {
                Debug.Assert(INVALIDKEY != key);

                if (_entries[index].Key == key)
                {
                    _entries[index].Value = value;
                    return FrugalMapStoreState.Success;
                }
            }

            // New key/value pair
            if (MAXSIZE > _count)
            {
                // Space still available to store the value
                if (null != _entries)
                {
                    // We are adding an entry to the array, so we may not be sorted any longer
                    _sorted = false;

                    if (_entries.Length > _count)
                    {
                        // Have empty entries, just set the first available
                    }
                    else
                    {
                        Entry[] destEntries = new Entry[_entries.Length + GROWTH];

                        // Copy old array
                        Array.Copy(_entries, 0, destEntries, 0, _entries.Length);
                        _entries = destEntries;
                    }
                }
                else
                {
                    _entries = new Entry[MINSIZE];

                    // No entries, must be sorted
                    _sorted = true;
                }

                // Stuff in the new key/value pair
                _entries[_count].Key = key;
                _entries[_count].Value = value;

                // Bump the count for the entry just added.
                ++_count;

                return FrugalMapStoreState.Success;
            }
            else
            {
                // Array is full, move to a SortedArray
                return FrugalMapStoreState.SortedArray;
            }
        }

        public override void RemoveEntry(int key)
        {
            for (int index = 0; index < _count; ++index)
            {
                if (_entries[index].Key == key)
                {
                    // Shift entries down
                    int numToCopy = (_count - index) - 1;
                    if (numToCopy > 0)
                    {
                        Array.Copy(_entries, index + 1, _entries, index, numToCopy);
                    }

                    // Wipe out the last entry
                    _entries[_count - 1].Key = INVALIDKEY;
                    _entries[_count - 1].Value = DependencyProperty.UnsetValue;
                    --_count;
                    break;
                }
            }
        }

        public override Object Search(int key)
        {
            for (int index = 0; index < _count; ++index)
            {
                if (key == _entries[index].Key)
                {
                    return _entries[index].Value;
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public override void Sort()
        {
            if ((false == _sorted) && (_count > 1))
            {
                QSort(0, (_count - 1));
                _sorted = true;
            }
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _count)
            {
                value = _entries[index].Value;
                key = _entries[index].Key;
            }
            else
            {
                value = DependencyProperty.UnsetValue;
                key = INVALIDKEY;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (_count > 0)
            {
                for (int i=0; i< _count; i++)
                {
                    callback(list, _entries[i].Key, _entries[i].Value);
                }
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            for (int index = 0; index < _entries.Length; ++index)
            {
                if (FrugalMapStoreState.Success == newMap.InsertEntry(_entries[index].Key, _entries[index].Value))
                {
                    continue;
                }
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
        }

        // Size of this data store
        public override int Count
        {
            get
            {
                return _count;
            }
        }

        // Compare two Entry nodes in the _entries array
        private int Compare(int left, int right)
        {
            return (_entries[left].Key - _entries[right].Key);
        }

        // Partition the _entries array for QuickSort
        private int Partition(int left, int right)
        {
            int pivot = right;
            int i = left - 1;
            int j = right;
            Entry temp;

            for (;;)
            {
                while (Compare(++i, pivot) < 0);
                while (Compare(pivot, --j) < 0)
                {
                    if (j == left)
                    {
                        break;
                    }
                }
                if (i >= j)
                {
                    break;
                }
                temp = _entries[j];
                _entries[j] = _entries[i];
                _entries[i] = temp;
            }
            temp = _entries[right];
            _entries[right] = _entries[i];
            _entries[i] = temp;
            return i;
        }

        // Sort the _entries array using an index based QuickSort
        private void QSort(int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(left, right);
                QSort(left, pivot - 1);
                QSort(pivot + 1, right);
            }
        }

        // MINSIZE and GROWTH chosen to minimize memory footprint
        private const int MINSIZE = 9;
        private const int MAXSIZE = 15;
        private const int GROWTH = 3;

        // The number of items in the map.
        private UInt16 _count;

        private bool _sorted;
        private Entry[] _entries;
    }

    // A sorted array of key/value pairs. A binary search is used to minimize the cost of insert/search.

    internal sealed class SortedObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            bool found;

            Debug.Assert(INVALIDKEY != key);

            // Check to see if we are updating an existing entry
            int index = FindInsertIndex(key, out found);
            if (found)
            {
                _entries[index].Value = value;
                return FrugalMapStoreState.Success;
            }
            else
            {
                // New key/value pair
                if (MAXSIZE > _count)
                {
                    // Less than the maximum array size
                    if (null != _entries)
                    {
                        if (_entries.Length > _count)
                        {
                            // Have empty entries, just set the first available
                        }
                        else
                        {
                            Entry[] destEntries = new Entry[_entries.Length + GROWTH];

                            // Copy old array
                            Array.Copy(_entries, 0, destEntries, 0, _entries.Length);
                            _entries = destEntries;
                        }
                    }
                    else
                    {
                        _entries = new Entry[MINSIZE];
                    }

                    // Inserting into the middle of the existing entries?
                    if (index < _count)
                    {
                        // Move higher valued keys to make room for the new key
                        Array.Copy(_entries, index, _entries, index + 1, (_count - index));
                    }
                    else
                    {
                        _lastKey = key;
                    }

                    // Stuff in the new key/value pair
                    _entries[index].Key = key;
                    _entries[index].Value = value;
                    ++_count;
                    return FrugalMapStoreState.Success;
                }
                else
                {
                    // SortedArray is full, move to a hashtable
                    return FrugalMapStoreState.Hashtable;
                }
            }
        }

        public override void RemoveEntry(int key)
        {
            bool found;

            Debug.Assert(INVALIDKEY != key);

            int index = FindInsertIndex(key, out found);

            if (found)
            {
                // Shift entries down
                int numToCopy = (_count - index) - 1;
                if (numToCopy > 0)
                {
                    Array.Copy(_entries, index + 1, _entries, index, numToCopy);
                }
                else 
                {
                    // If we're not copying anything, then it means we are 
                    //  going to remove the last entry.  Update _lastKey so
                    //  that it reflects the key of the new "last entry"
                    if( _count > 1 )
                    {
                        // Next-to-last entry will be the new last entry
                        _lastKey = _entries[_count - 2].Key;
                    }
                    else
                    {
                        // Unless there isn't a next-to-last entry, in which
                        //  case the key is reset to INVALIDKEY.
                        _lastKey = INVALIDKEY;
                    }
                }

                // Wipe out the last entry
                _entries[_count - 1].Key = INVALIDKEY;
                _entries[_count - 1].Value = DependencyProperty.UnsetValue;

                --_count;
            }
        }

        public override Object Search(int key)
        {
            bool found;

            int index = FindInsertIndex(key, out found);
            if (found)
            {
                return _entries[index].Value;
            }
            return DependencyProperty.UnsetValue;
        }

        public override void Sort()
        {
            // Always sorted.
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _count)
            {
                value = _entries[index].Value;
                key = _entries[index].Key;
            }
            else
            {
                value = DependencyProperty.UnsetValue;
                key = INVALIDKEY;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (_count > 0)
            {
                for (int i=0; i< _count; i++)
                {
                    callback(list, _entries[i].Key, _entries[i].Value);
                }
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            for (int index = 0; index < _entries.Length; ++index)
            {
                if (FrugalMapStoreState.Success == newMap.InsertEntry(_entries[index].Key, _entries[index].Value))
                {
                    continue;
                }
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
        }

        private int FindInsertIndex(int key, out bool found)
        {
            int iLo = 0;

            // Only do the binary search if there is a chance of finding the key
            // This also speeds insertion because we tend to insert at the end.
            if ((_count > 0) && (key <= _lastKey))
            {
                // The array index used for insertion is somewhere between 0 
                //  and _count-1 inclusive
                int iHi = _count-1;

                // Do a binary search to find the insertion point
                do
                {
                    int iPv = (iHi + iLo) / 2;
                    if (key <= _entries[iPv].Key)
                    {
                        iHi = iPv;
                    }
                    else
                    {
                        iLo = iPv + 1;
                    }
                }
                while (iLo < iHi);
                found = (key == _entries[iLo].Key);
            }
            else
            {
                // Insert point is at the end
                iLo = _count;
                found = false;
            }
            return iLo;
        }

        public override int Count
        {
            get
            {
                return _count;
            }
        }

        // MINSIZE chosen to be larger than MAXSIZE of the ArrayObjectMap with some extra space for new values
        // The MAXSIZE and GROWTH are chosen to minimize memory usage as we grow the array
        private const int MINSIZE = 16;
        private const int MAXSIZE = 128;
        private const int GROWTH = 8;

        // The number of items in the map.
        internal int _count;

        private int _lastKey = INVALIDKEY;
        private Entry[] _entries;
    }

    internal sealed class HashObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            Debug.Assert(INVALIDKEY != key);

            if (null != _entries)
            {
                // This is done because forward branches
                // default prediction is not to be taken
                // making this a CPU win because insert
                // is a common operation.
            }
            else
            {
                _entries = new Hashtable(MINSIZE);
            }

            _entries[key] = ((value != NullValue) && (value != null)) ? value : NullValue;
            return FrugalMapStoreState.Success;
        }

        public override void RemoveEntry(int key)
        {
            _entries.Remove(key);
        }

        public override Object Search(int key)
        {
            object value = _entries[key];

            return ((value != NullValue) && (value != null)) ? value : DependencyProperty.UnsetValue;
        }

        public override void Sort()
        {
            // Always sorted.
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _entries.Count)
            {
                IDictionaryEnumerator myEnumerator = _entries.GetEnumerator();

                // Move to first valid value
                myEnumerator.MoveNext();

                for (int i = 0; i < index; ++i)
                {
                    myEnumerator.MoveNext();
                }
                key = (int)myEnumerator.Key;
                if ((myEnumerator.Value != NullValue) && (myEnumerator.Value != null))
                {
                    value = myEnumerator.Value;
                }
                else
                {
                    value = DependencyProperty.UnsetValue;
                }
            }
            else
            {
                value = DependencyProperty.UnsetValue;
                key = INVALIDKEY;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            IDictionaryEnumerator myEnumerator = _entries.GetEnumerator();
            
            while (myEnumerator.MoveNext())
            {
                int key = (int)myEnumerator.Key;
                object value;
                if ((myEnumerator.Value != NullValue) && (myEnumerator.Value != null))
                {
                    value = myEnumerator.Value;
                }
                else
                {
                    value = DependencyProperty.UnsetValue;
                }
            
                callback(list, key, value);
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            // Should never get here
            throw new InvalidOperationException(SR.Get(SRID.FrugalMap_CannotPromoteBeyondHashtable));
        }

        // Size of this data store
        public override int Count
        {
            get
            {
                return _entries.Count;
            }
        }

        // 163 is chosen because it is the first prime larger than 128, the MAXSIZE of SortedObjectMap
        internal const int MINSIZE = 163;

        // Hashtable will return null from its indexer if the key is not
        // found OR if the value is null.  To distinguish between these
        // two cases we insert NullValue instead of null.
        private static object NullValue = new object();

        internal Hashtable _entries;
    }

    [FriendAccessAllowed]
    internal struct FrugalMap
    {
        public object this[int key]
        {
            get
            {
                // If no entry, DependencyProperty.UnsetValue is returned
                if (null != _mapStore)
                {
                    return _mapStore.Search(key);
                }
                return DependencyProperty.UnsetValue;
            }

            set
            {
                if (value != DependencyProperty.UnsetValue)
                {
                    // If not unset value, ensure write success
                    if (null != _mapStore)
                    {
                        // This is done because forward branches
                        // default prediction is not to be taken
                        // making this a CPU win because set is
                        // a common operation.
                    }
                    else
                    {
                        _mapStore = new SingleObjectMap();
                    }

                    FrugalMapStoreState myState = _mapStore.InsertEntry(key, value);
                    if (FrugalMapStoreState.Success == myState)
                    {
                        return;
                    }
                    else
                    {
                        // Need to move to a more complex storage
                        FrugalMapBase newStore;

                        if (FrugalMapStoreState.ThreeObjectMap == myState)
                        {
                            newStore = new ThreeObjectMap();
                        }
                        else if (FrugalMapStoreState.SixObjectMap == myState)
                        {
                            newStore = new SixObjectMap();
                        }
                        else if (FrugalMapStoreState.Array == myState)
                        {
                            newStore = new ArrayObjectMap();
                        }
                        else if (FrugalMapStoreState.SortedArray == myState)
                        {
                            newStore = new SortedObjectMap();
                        }
                        else if (FrugalMapStoreState.Hashtable == myState)
                        {
                            newStore = new HashObjectMap();
                        }
                        else
                        {
                            throw new InvalidOperationException(SR.Get(SRID.FrugalMap_CannotPromoteBeyondHashtable));
                        }

                        // Extract the values from the old store and insert them into the new store
                        _mapStore.Promote(newStore);

                        // Insert the new value
                        _mapStore = newStore;
                        _mapStore.InsertEntry(key, value);
                    }
                }
                else
                {
                    // DependencyProperty.UnsetValue means remove the value
                    if (null != _mapStore)
                    {
                        _mapStore.RemoveEntry(key);
                        if (_mapStore.Count == 0)
                        {
                            // Map Store is now empty ... throw it away
                            _mapStore = null;
                        }
                    }
                }
            }
        }

        public void Sort()
        {
            if (null != _mapStore)
            {
                _mapStore.Sort();
            }
        }

        public void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (null != _mapStore)
            {
                _mapStore.GetKeyValuePair(index, out key, out value);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }
        
        public void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (null != callback)
            {
                if (null != list)
                {
                    if (_mapStore != null)
                    {
                        _mapStore.Iterate(list, callback);
                    }
                }
                else
                {
                    throw new ArgumentNullException("list");
                }
            }
            else
            {
                throw new ArgumentNullException("callback");
            }
        }

        public int Count
        {
            get
            {
                if (null != _mapStore)
                {
                    return _mapStore.Count;
                }
                return 0;
            }
        }

        internal FrugalMapBase _mapStore;
    }

    // A sorted array of key/value pairs. A binary search is used to minimize the cost of insert/search.

    internal sealed class LargeSortedObjectMap : FrugalMapBase
    {
        public override FrugalMapStoreState InsertEntry(int key, Object value)
        {
            bool found;

            Debug.Assert(INVALIDKEY != key);

            // Check to see if we are updating an existing entry
            int index = FindInsertIndex(key, out found);
            if (found)
            {
                _entries[index].Value = value;
                return FrugalMapStoreState.Success;
            }
            else
            {
                // New key/value pair
                if (null != _entries)
                {
                    if (_entries.Length > _count)
                    {
                        // Have empty entries, just set the first available
                    }
                    else
                    {
                        int size = _entries.Length;
                        Entry[] destEntries = new Entry[size + (size >> 1)];

                        // Copy old array
                        Array.Copy(_entries, 0, destEntries, 0, _entries.Length);
                        _entries = destEntries;
                    }
                }
                else
                {
                    _entries = new Entry[MINSIZE];
                }

                // Inserting into the middle of the existing entries?
                if (index < _count)
                {
                    // Move higher valued keys to make room for the new key
                    Array.Copy(_entries, index, _entries, index + 1, (_count - index));
                }
                else
                {
                    _lastKey = key;
                }

                // Stuff in the new key/value pair
                _entries[index].Key = key;
                _entries[index].Value = value;
                ++_count;
                return FrugalMapStoreState.Success;
            }
        }

        public override void RemoveEntry(int key)
        {
            bool found;

            Debug.Assert(INVALIDKEY != key);

            int index = FindInsertIndex(key, out found);

            if (found)
            {
                // Shift entries down
                int numToCopy = (_count - index) - 1;
                if (numToCopy > 0)
                {
                    Array.Copy(_entries, index + 1, _entries, index, numToCopy);
                }
                else 
                {
                    // If we're not copying anything, then it means we are 
                    //  going to remove the last entry.  Update _lastKey so
                    //  that it reflects the key of the new "last entry"
                    if( _count > 1 )
                    {
                        // Next-to-last entry will be the new last entry
                        _lastKey = _entries[_count - 2].Key;
                    }
                    else
                    {
                        // Unless there isn't a next-to-last entry, in which
                        //  case the key is reset to INVALIDKEY.
                        _lastKey = INVALIDKEY;
                    }
                }

                // Wipe out the last entry
                _entries[_count - 1].Key = INVALIDKEY;
                _entries[_count - 1].Value = DependencyProperty.UnsetValue;

                --_count;
            }
        }

        public override Object Search(int key)
        {
            bool found;

            int index = FindInsertIndex(key, out found);
            if (found)
            {
                return _entries[index].Value;
            }
            return DependencyProperty.UnsetValue;
        }

        public override void Sort()
        {
            // Always sorted.
        }

        public override void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (index < _count)
            {
                value = _entries[index].Value;
                key = _entries[index].Key;
            }
            else
            {
                value = DependencyProperty.UnsetValue;
                key = INVALIDKEY;
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public override void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (_count > 0)
            {
                for (int i=0; i< _count; i++)
                {
                    callback(list, _entries[i].Key, _entries[i].Value);
                }
            }
        }

        public override void Promote(FrugalMapBase newMap)
        {
            for (int index = 0; index < _entries.Length; ++index)
            {
                if (FrugalMapStoreState.Success == newMap.InsertEntry(_entries[index].Key, _entries[index].Value))
                {
                    continue;
                }
                // newMap is smaller than previous map
                throw new ArgumentException(SR.Get(SRID.FrugalMap_TargetMapCannotHoldAllData, this.ToString(), newMap.ToString()), "newMap");
            }
        }

        private int FindInsertIndex(int key, out bool found)
        {
            int iLo = 0;

            // Only do the binary search if there is a chance of finding the key
            // This also speeds insertion because we tend to insert at the end.
            if ((_count > 0) && (key <= _lastKey))
            {
                // The array index used for insertion is somewhere between 0 
                //  and _count-1 inclusive
                int iHi = _count-1;

                // Do a binary search to find the insertion point
                do
                {
                    int iPv = (iHi + iLo) / 2;
                    if (key <= _entries[iPv].Key)
                    {
                        iHi = iPv;
                    }
                    else
                    {
                        iLo = iPv + 1;
                    }
                }
                while (iLo < iHi);
                found = (key == _entries[iLo].Key);
            }
            else
            {
                // Insert point is at the end
                iLo = _count;
                found = false;
            }
            return iLo;
        }

        public override int Count
        {
            get
            {
                return _count;
            }
        }

        // MINSIZE chosen to be small, growth rate of 1.5 is slow at small sizes, but increasingly agressive as
        // the array grows
        private const int MINSIZE = 2;

        // The number of items in the map.
        internal int _count;

        private int _lastKey = INVALIDKEY;
        private Entry[] _entries;
    }

    // This is a variant of FrugalMap that always uses an array as the underlying store.
    // This avoids the virtual method calls that are present when the store morphs through
    // the size efficient store classes normally used. It is appropriate only when we know the
    // store will always be populated and individual elements will be accessed in a tight loop.
    internal struct InsertionSortMap
    {
        public object this[int key]
        {
            get
            {
                // If no entry, DependencyProperty.UnsetValue is returned
                if (null != _mapStore)
                {
                    return _mapStore.Search(key);
                }
                return DependencyProperty.UnsetValue;
            }

            set
            {
                if (value != DependencyProperty.UnsetValue)
                {
                    // If not unset value, ensure write success
                    if (null != _mapStore)
                    {
                        // This is done because forward branches
                        // default prediction is not to be taken
                        // making this a CPU win because set is
                        // a common operation.
                    }
                    else
                    {
                        _mapStore = new LargeSortedObjectMap();
                    }

                    FrugalMapStoreState myState = _mapStore.InsertEntry(key, value);
                    if (FrugalMapStoreState.Success == myState)
                    {
                        return;
                    }
                    else
                    {
                        // Need to move to a more complex storage
                        LargeSortedObjectMap newStore;

                        if (FrugalMapStoreState.SortedArray == myState)
                        {
                            newStore = new LargeSortedObjectMap();
                        }
                        else
                        {
                            throw new InvalidOperationException(SR.Get(SRID.FrugalMap_CannotPromoteBeyondHashtable));
                        }

                        // Extract the values from the old store and insert them into the new store
                        _mapStore.Promote(newStore);

                        // Insert the new value
                        _mapStore = newStore;
                        _mapStore.InsertEntry(key, value);
                    }
                }
                else
                {
                    // DependencyProperty.UnsetValue means remove the value
                    if (null != _mapStore)
                    {
                        _mapStore.RemoveEntry(key);
                        if (_mapStore.Count == 0)
                        {
                            // Map Store is now empty ... throw it away
                            _mapStore = null;
                        }
                    }
                }
            }
        }

        public void Sort()
        {
            if (null != _mapStore)
            {
                _mapStore.Sort();
            }
        }

        public void GetKeyValuePair(int index, out int key, out Object value)
        {
            if (null != _mapStore)
            {
                _mapStore.GetKeyValuePair(index, out key, out value);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public void Iterate(ArrayList list, FrugalMapIterationCallback callback)
        {
            if (null != callback)
            {
                if (null != list)
                {
                    if (_mapStore != null)
                    {
                        _mapStore.Iterate(list, callback);
                    }
                }
                else
                {
                    throw new ArgumentNullException("list");
                }
            }
            else
            {
                throw new ArgumentNullException("callback");
            }
        }

        public int Count
        {
            get
            {
                if (null != _mapStore)
                {
                    return _mapStore.Count;
                }
                return 0;
            }
        }

        internal LargeSortedObjectMap _mapStore;
    }

    /// <summary>
    ///     FrugalMapIterationCallback
    /// </summary>
    internal delegate void FrugalMapIterationCallback(ArrayList list, int key, object value);
}

