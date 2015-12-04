//------------------------------------------------------------------------------
// <copyright file="OdbcErrorCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Odbc {

    using System;
    using System.Collections;
    using System.Data;

    [Serializable]
    public sealed class OdbcErrorCollection : ICollection {
        private ArrayList _items = new ArrayList();

        internal OdbcErrorCollection() {
        }

        Object System.Collections.ICollection.SyncRoot {
            get { return this; }
        }

        bool System.Collections.ICollection.IsSynchronized {
            get { return false; }
        }

        public int Count {
            get {
                return _items.Count;
            }
        }

        public OdbcError this[int i] {
            get {
                return (OdbcError)_items[i];
            }
        }

        internal void Add(OdbcError error) {
            _items.Add(error);
        }

        public void CopyTo (Array array, int i) {
            _items.CopyTo(array, i);
        }

        public void CopyTo (OdbcError[] array, int i) {
            _items.CopyTo(array, i);
        }

        public IEnumerator GetEnumerator() {
            return _items.GetEnumerator();
        }

        internal void SetSource (string Source) {
            foreach (object error in _items) {
                ((OdbcError)error).SetSource(Source);
            }
        }
    }
}
