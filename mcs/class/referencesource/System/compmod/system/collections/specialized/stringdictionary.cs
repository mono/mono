//------------------------------------------------------------------------------
// <copyright file="StringDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Collections.Specialized {
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    
    /// <devdoc>
    ///    <para>Implements a hashtable with the key strongly typed to be
    ///       a string rather than an object. </para>
    ///    <para>Consider this class obsolete - use Dictionary&lt;String, String&gt; instead
    ///       with a proper StringComparer instance.</para>
    /// </devdoc>
    [Serializable]
    [DesignerSerializer("System.Diagnostics.Design.StringDictionaryCodeDomSerializer, " + AssemblyRef.SystemDesign, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + AssemblyRef.SystemDesign)]
    // @
    public class StringDictionary : IEnumerable {

        // For compatibility, we want the Keys property to return values in lower-case.
        // That means using ToLower in each property on this type.  Also for backwards
        // compatibility, we will be converting strings to lower-case, which has a 
        // problem for some Georgian alphabets.  Can't really fix it now though...
        internal Hashtable contents = new Hashtable();

        
        /// <devdoc>
        /// <para>Initializes a new instance of the StringDictionary class.</para>
        /// <para>If you're using file names, registry keys, etc, you want to use
        /// a Dictionary&lt;String, Object&gt; and use 
        /// StringComparer.OrdinalIgnoreCase.</para>
        /// </devdoc>
        public StringDictionary() {
        }

        /// <devdoc>
        /// <para>Gets the number of key-and-value pairs in the StringDictionary.</para>
        /// </devdoc>
        public virtual int Count {
            get {
                return contents.Count;
            }
        }

        
        /// <devdoc>
        /// <para>Indicates whether access to the StringDictionary is synchronized (thread-safe). This property is 
        ///    read-only.</para>
        /// </devdoc>
        public virtual bool IsSynchronized {
            get {
                return contents.IsSynchronized;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the value associated with the specified key.</para>
        /// </devdoc>
        public virtual string this[string key] {
            get {
                if( key == null ) {
                    throw new ArgumentNullException("key");
                }

                return (string) contents[key.ToLower(CultureInfo.InvariantCulture)];
            }
            set {
                if( key == null ) {
                    throw new ArgumentNullException("key");
                }

                contents[key.ToLower(CultureInfo.InvariantCulture)] = value;
            }
        }

        /// <devdoc>
        /// <para>Gets a collection of keys in the StringDictionary.</para>
        /// </devdoc>
        public virtual ICollection Keys {
            get {
                return contents.Keys;
            }
        }

        
        /// <devdoc>
        /// <para>Gets an object that can be used to synchronize access to the StringDictionary.</para>
        /// </devdoc>
        public virtual object SyncRoot {
            get {
                return contents.SyncRoot;
            }
        }

        /// <devdoc>
        /// <para>Gets a collection of values in the StringDictionary.</para>
        /// </devdoc>
        public virtual ICollection Values {
            get {
                return contents.Values;
            }
        }

        /// <devdoc>
        /// <para>Adds an entry with the specified key and value into the StringDictionary.</para>
        /// </devdoc>
        public virtual void Add(string key, string value) {
            if( key == null ) {
                throw new ArgumentNullException("key");
            }

            contents.Add(key.ToLower(CultureInfo.InvariantCulture), value);
        }

        /// <devdoc>
        /// <para>Removes all entries from the StringDictionary.</para>
        /// </devdoc>
        public virtual void Clear() {
            contents.Clear();
        }

        /// <devdoc>
        ///    <para>Determines if the string dictionary contains a specific key</para>
        /// </devdoc>
        public virtual bool ContainsKey(string key) {
            if( key == null ) {
                throw new ArgumentNullException("key");
            }

            return contents.ContainsKey(key.ToLower(CultureInfo.InvariantCulture));
        }

        /// <devdoc>
        /// <para>Determines if the StringDictionary contains a specific value.</para>
        /// </devdoc>
        public virtual bool ContainsValue(string value) {
            return contents.ContainsValue(value);
        }

        /// <devdoc>
        /// <para>Copies the string dictionary values to a one-dimensional <see cref='System.Array'/> instance at the 
        ///    specified index.</para>
        /// </devdoc>
        public virtual void CopyTo(Array array, int index) {
            contents.CopyTo(array, index);
        }

        /// <devdoc>
        ///    <para>Returns an enumerator that can iterate through the string dictionary.</para>
        /// </devdoc>
        public virtual IEnumerator GetEnumerator() {
            return contents.GetEnumerator();
        }

        /// <devdoc>
        ///    <para>Removes the entry with the specified key from the string dictionary.</para>
        /// </devdoc>
        public virtual void Remove(string key) {
            if( key == null ) {
                throw new ArgumentNullException("key");
            }

            contents.Remove(key.ToLower(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Make this StringDictionary subservient to some other collection.
        /// <para>Some code was replacing the contents field with a Hashtable created elsewhere.
        /// While it may not have been incorrect, we don't want to encourage that pattern, because
        /// it will replace the IEqualityComparer in the Hashtable, and creates a possibly-undesirable
        /// link between two seemingly different collections.  Let's discourage that somewhat by
        /// making this an explicit method call instead of an internal field assignment.</para>
        /// </summary>
        /// <param name="useThisHashtableInstead">Replaces the backing store with another, possibly aliased Hashtable.</param>
        internal void ReplaceHashtable(Hashtable useThisHashtableInstead) {
            contents = useThisHashtableInstead;
        }
    }

}
