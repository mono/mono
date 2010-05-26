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
    using System.Linq;

    internal static class ValueProviderUtil {

        // Given "foo.bar[baz].quux", this method will return:
        // - "foo.bar[baz].quux"
        // - "foo.bar[baz]"
        // - "foo.bar"
        // - "foo"
        public static IEnumerable<string> GetPrefixes(string key) {
            yield return key;
            for (int i = key.Length - 1; i >= 0; i--) {
                switch (key[i]) {
                    case '.':
                    case '[':
                        yield return key.Substring(0, i);
                        break;
                }
            }
        }

        public static bool CollectionContainsPrefix(IEnumerable<string> collection, string prefix) {
            foreach (string key in collection) {
                if (key != null) {
                    if (prefix.Length == 0) {
                        return true; // shortcut - non-null key matches empty prefix
                    }

                    if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                        if (key.Length == prefix.Length) {
                            return true; // exact match
                        }
                        else {
                            switch (key[prefix.Length]) {
                                case '.': // known separator characters
                                case '[':
                                    return true;
                            }
                        }
                    }
                }
            }

            return false; // nothing found
        }

    }
}
