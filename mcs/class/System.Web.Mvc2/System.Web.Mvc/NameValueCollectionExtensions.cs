/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public static class NameValueCollectionExtensions {

        public static void CopyTo(this NameValueCollection collection, IDictionary<string, object> destination) {
            CopyTo(collection, destination, false /* replaceEntries */);
        }

        public static void CopyTo(this NameValueCollection collection, IDictionary<string, object> destination, bool replaceEntries) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }
            if (destination == null) {
                throw new ArgumentNullException("destination");
            }

            foreach (string key in collection.Keys) {
                if (replaceEntries || !destination.ContainsKey(key)) {
                    destination[key] = collection[key];
                }
            }
        }
    }
}
