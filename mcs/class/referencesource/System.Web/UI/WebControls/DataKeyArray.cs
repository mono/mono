//------------------------------------------------------------------------------
// <copyright file="DataKeyArray.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;

    /// <devdoc>
    /// </devdoc>
    public sealed class DataKeyArray : ICollection, IStateManager {

        private ArrayList _keys;
        private bool _isTracking;
        

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DataKeyArray'/> class.</para>
        /// </devdoc>
        public DataKeyArray(ArrayList keys) {
            this._keys = keys;
        }


        /// <devdoc>
        ///    <para>Gets the number of objects in the collection. This property is read-only.</para>
        /// </devdoc>
        public int Count {
            get {
                return _keys.Count;
            }
        }


        /// <devdoc>
        /// <para>Gets the value that specifies whether items in the <see cref='System.Web.UI.WebControls.DataKeyArray'/> can be 
        ///    modified. This property is read-only.</para>
        /// </devdoc>
        public bool IsReadOnly {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// <para>Gets a value that indicates whether the <see cref='System.Web.UI.WebControls.DataKeyArray'/> is 
        ///    thread-safe. This property is read-only.</para>
        /// </devdoc>
        public bool IsSynchronized {
            get {
                return false;
            }
        }


        /// <devdoc>
        ///    <para>Gets the object used to synchronize access to the collection. This property is read-only. </para>
        /// </devdoc>
        public object SyncRoot {
            get {
                return this;
            }
        }


        /// <devdoc>
        /// <para>Gets a <see cref='DataKey' qualify='true'/> at the specified index in the collection. This property is read-only.</para>
        /// </devdoc>
        public DataKey this[int index] {
            get {
                return _keys[index] as DataKey;
            }
        }



        /// <devdoc>
        /// <para>Copies the contents of the entire collection into an <see cref='System.Array' qualify='true'/> appending at 
        ///    the specified index of the <see cref='System.Array' qualify='true'/>.</para>
        /// </devdoc>
        public void CopyTo(DataKey[] array, int index) {
            ((ICollection)this).CopyTo(array, index);
        }


        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }


        /// <devdoc>
        /// <para>Creates an enumerator for the <see cref='System.Web.UI.WebControls.DataKeyArray'/> used to iterate 
        ///    through the collection.</para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return _keys.GetEnumerator();
        }

        #region IStateManager implementation

        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return _isTracking;
            }
        }


        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            if (state != null) {
                object[] keysState = (object[])state;

                for (int i = 0; i < keysState.Length; i++) {
                    if (keysState[i] != null) {
                        ((IStateManager)_keys[i]).LoadViewState(keysState[i]);
                    }
                }
            }
        }


        /// <internalonly/>
        object IStateManager.SaveViewState() {
            int keyCount = _keys.Count;
            object[] keysState = new object[keyCount];
            bool savedState = false;

            for (int i = 0; i < keyCount; i++) {
                keysState[i] = ((IStateManager)_keys[i]).SaveViewState();
                if (keysState[i] != null)
                    savedState = true;
            }

            return savedState ? keysState : null;
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            _isTracking = true;

            int keyCount = _keys.Count;
            for (int i = 0; i < keyCount; i++) {
                ((IStateManager)_keys[i]).TrackViewState();
            }
        }
        #endregion
    }
}

