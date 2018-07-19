//------------------------------------------------------------------------------
// <copyright file="SimpleRecyclingCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * SimpleRecyclingCache class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Util {

using System.Collections;

/*
 * The purpose of this class is to provide a simple and efficient cache.
 * Instead of using fancy logic to expire items, it uses a simple heuristic:
 * when the number of entry reaches a fixed limit, the cache is reset.
 */
internal class SimpleRecyclingCache {

    private const int MAX_SIZE = 100;
    private static Hashtable _hashtable;

    internal SimpleRecyclingCache() {
        CreateHashtable();
    }

    // Create or recreate the hastable
    private void CreateHashtable() {
        _hashtable = new Hashtable(MAX_SIZE, StringComparer.OrdinalIgnoreCase);
    }

    internal object this[object key] {
        get {
            return _hashtable[key];
        }
        set {
            lock (this) {
                // The limit was reached, so reset everything
                if (_hashtable.Count >= MAX_SIZE)
                    _hashtable.Clear();

                _hashtable[key] = value;
            }
        }
    }

}

}
