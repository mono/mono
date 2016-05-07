//------------------------------------------------------------------------------
// <copyright file="IOrderedDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Collections.Specialized {
    
    using System;
    using System.Collections;
    
    /// <devdoc>
    /// <para> 
    /// This interface adds indexing on the IDictionary keyed table concept.  Objects
    /// added or inserted in an IOrderedDictionary must have both a key and an index, and 
    /// can be retrieved by either.
    /// This interface is useful when preserving easy IDictionary access semantics via a key is
    /// desired while ordering is necessary.
    /// </para>
    /// </devdoc>
    public interface IOrderedDictionary : IDictionary {
    
        // properties
        /// <devdoc>
        /// Returns the object at the given index
        /// </devdoc>
        object this[int index] { get; set; }

        // Returns an IDictionaryEnumerator for this dictionary.
        new IDictionaryEnumerator GetEnumerator();

        // methods
        /// <devdoc>
        /// Inserts the given object, with the given key, at the given index
        /// </devdoc>
        void Insert(int index, object key, object value);

        /// <devdoc>
        /// Removes the object and key at the given index
        /// </devdoc>
        void RemoveAt(int index);
    }
}

