//------------------------------------------------------------------------------
// <copyright file="HttpDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Ordered dictionary keyed by string
 * -- Utility class used in Collections
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {

    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.Util;

    internal class HttpDictionary : NameObjectCollectionBase {
        internal HttpDictionary(): base(Misc.CaseInsensitiveInvariantKeyComparer)  {
        }

        internal int Size {
            get { return Count;}
        }

        internal Object GetValue(String key) {
            return BaseGet(key);
        }

        internal void SetValue(String key, Object value) {
            BaseSet(key, value);
        }

        internal Object GetValue(int index) {
            return BaseGet(index);
        }

        internal String GetKey(int index) {
            return BaseGetKey(index);
        }

        internal String[] GetAllKeys() {
            return BaseGetAllKeys();
        }
    }
}
