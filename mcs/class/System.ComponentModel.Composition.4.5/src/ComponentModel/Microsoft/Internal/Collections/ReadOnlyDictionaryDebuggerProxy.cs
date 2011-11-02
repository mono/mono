// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Internal;

namespace System.Collections.ObjectModel
{

    // NOTE: This type cannot be a nested proxy of ReadOnlyDictionary due to a bug 
    // in the Visual Studio Debugger which causes it to ignore nested generic proxies.
    internal class ReadOnlyDictionaryDebuggerProxy<TKey, TValue>
    {
        private readonly ReadOnlyDictionary<TKey, TValue> _dictionary;
        
        public ReadOnlyDictionaryDebuggerProxy(ReadOnlyDictionary<TKey, TValue> dictionary)
        {
            Requires.NotNull(dictionary, "dictionary");
            
            _dictionary = dictionary;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TValue>[] Items
        {
            // NOTE: This shouldn't be cached, so that on every query of
            // the current value of the underlying dictionary is respected.
            get { return this._dictionary.ToArray(); }
        }
    }
}
