//------------------------------------------------------------------------------
// <copyright file="StringDictionaryWithComparer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// StringDictionary compares keys by converting them to lowercase first, using the Invariant culture. 
// This is not the right thing to do for file names, registry keys, environment variable etc.  
// This internal version of StringDictionary accepts an IEqualityComparer and enables you to 
// customize the string comparison to be StringComparer.OrdinalIgnoreCase for the above cases.  
   
namespace System.Collections.Specialized {
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    
    [Serializable]
    internal class StringDictionaryWithComparer : StringDictionary {

        public StringDictionaryWithComparer() : this(StringComparer.OrdinalIgnoreCase) {
        }

        public StringDictionaryWithComparer(IEqualityComparer comparer) {
            ReplaceHashtable(new Hashtable(comparer));
        }
        
        public override string this[string key] {
            get {
                if( key == null ) {
                    throw new ArgumentNullException("key");
                }

                return (string) contents[key];
            }
            set {
                if( key == null ) {
                    throw new ArgumentNullException("key");
                }

                contents[key] = value;
            }
        }

        public override void Add(string key, string value) {
            if( key == null ) {
                throw new ArgumentNullException("key");
            }

            contents.Add(key, value);
        }

        public override bool ContainsKey(string key) {
            if( key == null ) {
                throw new ArgumentNullException("key");
            }

            return contents.ContainsKey(key);
        }

        public override void Remove(string key) {
            if( key == null ) {
                throw new ArgumentNullException("key");
            }

            contents.Remove(key);
        }
    }
}
