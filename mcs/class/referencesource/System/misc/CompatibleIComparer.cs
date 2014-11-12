//------------------------------------------------------------------------------
// <copyright file="CompatibleComparer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * This class is used to create hashcodes that are Everett Compatible.
 * 
 * Copyright (c) 2004 Microsoft Corporation
 */

namespace System.Collections.Specialized {

    using Microsoft.Win32;
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Globalization;

    internal class BackCompatibleStringComparer : IEqualityComparer {

        static internal IEqualityComparer Default = new BackCompatibleStringComparer();

        internal BackCompatibleStringComparer() {
        }

        //This comes from VS# 434837 and is specifically written to get backcompat
        public static int GetHashCode(string obj) {
            unsafe {
                fixed (char* src = obj) {
                    int hash = 5381;
                    int c;
                    char* szStr = src;

                    while ((c = *szStr) != 0) {
                        hash = ((hash << 5) + hash) ^ c;
                        ++szStr;
                    }
                    return hash;
                }
            }
        }

        bool IEqualityComparer.Equals(Object a, Object b) {
            return Object.Equals(a, b);
        }

        public virtual int GetHashCode(Object o) {
            String obj = o as string;
            if (obj == null) {
                return o.GetHashCode();
            }

            return BackCompatibleStringComparer.GetHashCode(obj);
        }
    }
}
