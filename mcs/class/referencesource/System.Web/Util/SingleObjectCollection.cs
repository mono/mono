//------------------------------------------------------------------------------
// <copyright file="SingleObjectCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * SingleObjectCollection class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Util {

using System.Collections;

/*
 * Fast implementation of a collection with a single object
 */
internal class SingleObjectCollection: ICollection {

    private class SingleObjectEnumerator: IEnumerator {
        private object _object;
        private bool done;

        public SingleObjectEnumerator(object o) { _object = o; }
        public object Current { get { return _object; } }
        public bool MoveNext() {
            if (!done) {
                done = true;
                return true;
            }

            return false;
        }
        public void Reset() { done = false; }
    }

    private object _object;

    public SingleObjectCollection(object o) { _object = o; }

    IEnumerator IEnumerable.GetEnumerator() { return new SingleObjectEnumerator(_object); }
    public int Count { get { return 1; } }
    bool ICollection.IsSynchronized { get { return true; } }
    object ICollection.SyncRoot { get { return this; } }

    public void CopyTo(Array array, int index) {
        array.SetValue(_object, index);
    }
}

}
