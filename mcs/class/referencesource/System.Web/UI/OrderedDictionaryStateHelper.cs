// <copyright file="OrderedDictionaryStateHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {
    
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Runtime.Serialization;
    using System.Web.Util;

    internal static class OrderedDictionaryStateHelper {
        public static void LoadViewState(IOrderedDictionary dictionary, ArrayList state) {
            if (dictionary == null) {
                throw new ArgumentNullException("dictionary");
            }
            if (state == null) {
                throw new ArgumentNullException("state");
            }

            if (state != null) {
                for (int i = 0; i < state.Count; i++) {
                    Pair pairEntry = (Pair)state[i];
                    dictionary.Add(pairEntry.First, pairEntry.Second);
                }
            }
        }

        public static ArrayList SaveViewState(IOrderedDictionary dictionary) {
            if (dictionary == null) {
                throw new ArgumentNullException("dictionary");
            }

            ArrayList list = new ArrayList(dictionary.Count);
            foreach (DictionaryEntry entry in dictionary) {
                list.Add(new Pair(entry.Key, entry.Value));
            }
            return list;
        }
    }
}

