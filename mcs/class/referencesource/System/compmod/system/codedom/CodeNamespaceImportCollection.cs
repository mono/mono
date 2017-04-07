//------------------------------------------------------------------------------
// <copyright file="CodeNamespaceImportCollection.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom {

    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Globalization;
    
    /// <devdoc>
    ///    <para>
    ///       Manages a collection of <see cref='System.CodeDom.CodeNamespaceImport'/> objects.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeNamespaceImportCollection : IList {
        private ArrayList data = new ArrayList();
        private Hashtable keys = new Hashtable(StringComparer.OrdinalIgnoreCase);

        /// <devdoc>
        ///    <para>
        ///       Indexer method that provides collection access.
        ///    </para>
        /// </devdoc>
        public CodeNamespaceImport this[int index] {
            get {
                return ((CodeNamespaceImport)data[index]);
            }
            set {
                data[index] = value;
                SyncKeys();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the number of namespaces in the collection.
        ///    </para>
        /// </devdoc>
        public int Count {
            get {
                return data.Count;
            }
        }

		/// <internalonly/>
		bool IList.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <internalonly/>
		bool IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}


        /// <devdoc>
        ///    <para>
        ///       Adds a namespace import to the collection.
        ///    </para>
        /// </devdoc>
        public void Add(CodeNamespaceImport value) {
            if (!keys.ContainsKey(value.Namespace)) {
                keys[value.Namespace] = value;
                data.Add(value);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Adds a set of <see cref='System.CodeDom.CodeNamespaceImport'/> objects to the collection.
        ///    </para>
        /// </devdoc>
        public void AddRange(CodeNamespaceImport[] value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            foreach (CodeNamespaceImport c in value) {
                Add(c);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Clears the collection of members.
        ///    </para>
        /// </devdoc>
        public void Clear() {
            data.Clear();
            keys.Clear();
        }

        /// <devdoc>
        ///    <para>
        ///    Makes the collection of keys synchronised with the data.
        ///    </para>
        /// </devdoc>
        private void SyncKeys() {
            keys = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach(CodeNamespaceImport c in this) {
                keys[c.Namespace] = c;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets an enumerator that enumerates the collection members.
        ///    </para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return data.GetEnumerator();
        }

        /// <internalonly/>
        object IList.this[int index] {
            get {
                return this[index];
            }
            set {
                this[index] = (CodeNamespaceImport)value;
                SyncKeys();
            }
        }

        /// <internalonly/>
        int ICollection.Count {
            get {
                return Count;
            }
        }

        /// <internalonly/>
        bool ICollection.IsSynchronized {
            get {
                return false;
            }
        }

        /// <internalonly/>
        object ICollection.SyncRoot {
            get {
                return null;
            }
        }

        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            data.CopyTo(array, index);
        }

        /// <internalonly/>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <internalonly/>
        int IList.Add(object value) {
            return data.Add((CodeNamespaceImport)value);
        }

        /// <internalonly/>
        void IList.Clear() {
            Clear();
        }

        /// <internalonly/>
        bool IList.Contains(object value) {
            return data.Contains(value);
        }

        /// <internalonly/>
        int IList.IndexOf(object value) {
            return data.IndexOf((CodeNamespaceImport)value);
        }

        /// <internalonly/>
        void IList.Insert(int index, object value) {
            data.Insert(index, (CodeNamespaceImport)value);
            SyncKeys();
        }

        /// <internalonly/>
        void IList.Remove(object value) {
            data.Remove((CodeNamespaceImport)value);
            SyncKeys();
        }

        /// <internalonly/>
        void IList.RemoveAt(int index) {
            data.RemoveAt(index);
            SyncKeys();
        }
    }
}


