//------------------------------------------------------------------------------
// <copyright file="EmptyCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * EmptyCollection class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Util {

using System.Collections;

/*
 * Fast implementation of an empty collection
 */
internal class EmptyCollection: ICollection, IEnumerator {

    private static EmptyCollection s_theEmptyCollection = new EmptyCollection();

    private EmptyCollection() { }

    // Return the same instance all the time, since it's immutable
    internal static EmptyCollection Instance { get { return s_theEmptyCollection; } }

    // ICollection implementation
    IEnumerator IEnumerable.GetEnumerator() { return this; }
    public int Count { get { return 0; } }
    bool ICollection.IsSynchronized { get { return true; } }
    object ICollection.SyncRoot { get { return this; } }
    public void CopyTo(Array array, int index) { }

    // IEnumerator implementation
    object IEnumerator.Current { get { return null; } }
    bool IEnumerator.MoveNext() { return false; }
    void IEnumerator.Reset() { }
}

}
