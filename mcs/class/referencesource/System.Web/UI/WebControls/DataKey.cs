//------------------------------------------------------------------------------
// <copyright file="DataKey.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;

    /// <devdoc>
    /// </devdoc>
    public class DataKey : IStateManager, IEquatable<DataKey> {

        private IOrderedDictionary _keyTable;
        private bool _isTracking;
        private string[] _keyNames;


        public DataKey(IOrderedDictionary keyTable) {
            _keyTable = keyTable;
        }

        public DataKey(IOrderedDictionary keyTable, string[] keyNames)
            : this(keyTable) {
            _keyNames = keyNames;
        }


        /// <devdoc>
        /// Inheritable overridable implementation of IsTrackingViewState
        /// </devdoc>
        protected virtual bool IsTrackingViewState {
            get {
                return _isTracking;
            }
        }

        public virtual object this[int index] {
            get {
                if (_keyTable != null) {
                    return _keyTable[index];
                }
                return null;
            }
        }

        public virtual object this[string name] {
            get {
                if (_keyTable != null) {
                    return _keyTable[name];
                }
                return null;
            }
        }


        public virtual object Value {
            get {
                if ((_keyTable != null)  && (_keyTable.Count > 0)) {
                    return _keyTable[0];
                }
                return null;
            }
        }

        public virtual IOrderedDictionary Values {
            get {
                if (_keyTable == null) {
                    return null;
                }

                if (_keyTable is OrderedDictionary) {
                    return ((OrderedDictionary)_keyTable).AsReadOnly();
                }

                // don't return the actual table because we don't want the user to mess with it.
                if (_keyTable is ICloneable) {
                    return (IOrderedDictionary)((ICloneable)_keyTable).Clone();
                }
                OrderedDictionary table = new OrderedDictionary();

                foreach (DictionaryEntry entry in _keyTable) {
                    table.Add(entry.Key, entry.Value);
                }
                return table.AsReadOnly();
            }
        }

        /// <devdoc>
        /// Inheritable overridable implementation of LoadViewState
        /// </devdoc>
        protected virtual void LoadViewState(object state) {
            if (state != null) {
                if (_keyNames != null) {
                    object[] myState = (object[])state;
                    // if we have key names, then we only stored values.
                    if (myState[0] != null) {
                        for (int i = 0; i < myState.Length && i < _keyNames.Length; i++) {
                            _keyTable.Add(_keyNames[i], myState[i]);
                        }
                    }
                }
                else {
                    if (state != null) {
                        ArrayList stateArrayList = state as ArrayList;
                        if (stateArrayList == null) {
                            throw new HttpException(SR.GetString(SR.ViewState_InvalidViewState));
                        }
                        OrderedDictionaryStateHelper.LoadViewState(_keyTable, stateArrayList);
                    }
                }
            }
        }


        /// <devdoc>
        /// Inheritable overridable implementation of SaveViewState
        /// </devdoc>
        protected virtual object SaveViewState() {
            int count = _keyTable.Count;
            if (count > 0) {
                object myState;
                if (_keyNames != null) {
                    // if we have key names, then we only have to store values.
                    myState = new object[count];

                    for (int i = 0; i < count; i++) {
                        ((object[])myState)[i] = _keyTable[i];
                    }
                }
                else {
                    myState = OrderedDictionaryStateHelper.SaveViewState(_keyTable);
                }

                return myState;
            }
            return null;
        }


        /// <devdoc>
        /// Inheritable overridable implementation of TrackViewState
        /// </devdoc>
        protected virtual void TrackViewState() {
            _isTracking = true;
        }

        #region IStateManager implementation

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
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }
        #endregion

        #region IEquatable<DataKey> Members

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool Equals(DataKey other) {
            if (other == null) {
                return false;
            }

            string[] aKeys = _keyNames;
            string[] bKeys = other._keyNames;

            if (aKeys == null && _keyTable != null) {
                aKeys = new string[_keyTable.Count];
                _keyTable.Keys.CopyTo(aKeys, 0);
            }

            if (bKeys == null && _keyTable != null) {
                bKeys = new string[other._keyTable.Count];
                other._keyTable.Keys.CopyTo(bKeys, 0);
            }

            // Compare the keys array
            bool hasSameKeyNames = DataBoundControlHelper.CompareStringArrays(aKeys, bKeys);            
            if (hasSameKeyNames) {
                if (aKeys != null && bKeys != null) {
                    foreach (string key in aKeys) {
                        if (!Object.Equals(this[key], other[key])) {
                            return false;
                        }
                    }
                }
                return true;
            }

            return false;
        }

        #endregion
    }
}
