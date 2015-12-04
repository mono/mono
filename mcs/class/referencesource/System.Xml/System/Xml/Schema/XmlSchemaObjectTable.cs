//------------------------------------------------------------------------------
// <copyright file="XmlSchemaObjectTable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                 
//------------------------------------------------------------------------------

namespace System.Xml.Schema {

    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <include file='doc\XmlSchemaObjectTable.uex' path='docs/doc[@for="XmlSchemaObjectTable"]/*' />
    public class XmlSchemaObjectTable {
        Dictionary<XmlQualifiedName, XmlSchemaObject> table = new Dictionary<XmlQualifiedName,XmlSchemaObject>();
        List<XmlSchemaObjectEntry> entries = new List<XmlSchemaObjectEntry>();
        
        internal XmlSchemaObjectTable() {
        }

        internal void Add(XmlQualifiedName name,  XmlSchemaObject value) {
            Debug.Assert(!table.ContainsKey(name), "XmlSchemaObjectTable.Add: entry already exists");
            table.Add(name, value);
            entries.Add(new XmlSchemaObjectEntry(name, value));
        }

        internal void Insert(XmlQualifiedName name,  XmlSchemaObject value) {
            XmlSchemaObject oldValue = null;
            if (table.TryGetValue(name, out oldValue)) {
                table[name] = value; //set new value
                Debug.Assert(oldValue != null);
                int matchedIndex = FindIndexByValue(oldValue);
                Debug.Assert(matchedIndex >= 0);
                //set new entry
                Debug.Assert(entries[matchedIndex].qname == name);
                entries[matchedIndex] = new XmlSchemaObjectEntry(name, value);
            }
            else {
                Add(name, value);
            }
            
        }

        internal void Replace(XmlQualifiedName name,  XmlSchemaObject value) {
            XmlSchemaObject oldValue;
            if (table.TryGetValue(name, out oldValue)) {
                table[name] = value; //set new value
                Debug.Assert(oldValue != null);
                int matchedIndex = FindIndexByValue(oldValue);
                Debug.Assert(entries[matchedIndex].qname == name);
                entries[matchedIndex] = new XmlSchemaObjectEntry(name, value);
            }
        }

        internal void Clear() {
            table.Clear();
            entries.Clear();
        }

        internal void Remove(XmlQualifiedName name) {
            XmlSchemaObject value;
            if (table.TryGetValue(name, out value)) {
                table.Remove(name);
                int matchedIndex = FindIndexByValue(value);
                Debug.Assert(matchedIndex >= 0);
                Debug.Assert(entries[matchedIndex].qname == name);
                entries.RemoveAt(matchedIndex);                    
            }
        }

        private int FindIndexByValue(XmlSchemaObject xso) {
            int index;
            for(index = 0; index < entries.Count; index++) {
                if((object)entries[index].xso == (object)xso) {
                    return index;
                }    
            }
            return -1;
        }
        /// <include file='doc\XmlSchemaObjectTable.uex' path='docs/doc[@for="XmlSchemaObjectTable.Count"]/*' />
        public int Count {
            get {
                Debug.Assert(table.Count == entries.Count);
                return table.Count;
            }
        }

        /// <include file='doc\XmlSchemaObjectTable.uex' path='docs/doc[@for="XmlSchemaObjectTable.Contains"]/*' />
        public bool Contains(XmlQualifiedName name) {
            return table.ContainsKey(name);
        }

        /// <include file='doc\XmlSchemaObjectTable.uex' path='docs/doc[@for="XmlSchemaObjectTable.this"]/*' />
        public XmlSchemaObject this[XmlQualifiedName name] {
            get { 
                XmlSchemaObject value;
                if (table.TryGetValue(name, out value)) {
                    return value;
                }
                return null;
            }
        }
        
        /// <include file='doc\XmlSchemaObjectTable.uex' path='docs/doc[@for="XmlSchemaObjectTable.Names"]/*' />
        public ICollection Names {
            get { 
                return new NamesCollection(entries, table.Count);
            }
        }

        /// <include file='doc\XmlSchemaObjectTable.uex' path='docs/doc[@for="XmlSchemaObjectTable.Values"]/*' />
        public ICollection Values {
            get { 
                return new ValuesCollection(entries, table.Count);
            }
        }
        
        /// <include file='doc\XmlSchemaObjectTable.uex' path='docs/doc[@for="XmlSchemaObjectTable.GetEnumerator"]/*' />
        public IDictionaryEnumerator GetEnumerator() {
            return new XSODictionaryEnumerator(this.entries, table.Count, EnumeratorType.DictionaryEntry); 
        }

        internal enum EnumeratorType {
            Keys,
            Values,
            DictionaryEntry,
        }

        internal struct XmlSchemaObjectEntry {
            internal XmlQualifiedName qname;
            internal XmlSchemaObject xso;

            public XmlSchemaObjectEntry(XmlQualifiedName name, XmlSchemaObject value) {
                qname = name;
                xso = value;
            }

            public XmlSchemaObject IsMatch(string localName, string ns) {
                if (localName == qname.Name && ns == qname.Namespace) {
                    return xso;
                }
                return null;
            }

            public void Reset() {
                qname = null;
                xso = null;
            }
        }

        internal class NamesCollection : ICollection {
            private List<XmlSchemaObjectEntry> entries;
            int size;
        
            internal NamesCollection(List<XmlSchemaObjectEntry> entries, int size) {
                this.entries = entries;
                this.size = size;
            }
            
            public int Count { 
    	        get { return size; }
    	    }

            public Object SyncRoot {
    	        get {
                    return ((ICollection)entries).SyncRoot;
                }
    	    }

            public bool IsSynchronized {
                get {
                    return ((ICollection)entries).IsSynchronized;
                }
            }

            public void CopyTo(Array array, int arrayIndex) {
                if (array == null)
                    throw new ArgumentNullException("array");

			    if (arrayIndex < 0) 
                    throw new ArgumentOutOfRangeException("arrayIndex");

                Debug.Assert(array.Length >= size, "array is not big enough to hold all the items in the ICollection");

                for (int i = 0; i < size; i++) {
                    array.SetValue(entries[i].qname, arrayIndex++);
                }
            }

            public IEnumerator GetEnumerator() {
                return new XSOEnumerator(this.entries, this.size, EnumeratorType.Keys);
            }
        }

        //ICollection for Values 
        internal class ValuesCollection : ICollection {
            private List<XmlSchemaObjectEntry> entries;
            int size;
        
            internal ValuesCollection(List<XmlSchemaObjectEntry> entries, int size) {
                this.entries = entries;
                this.size = size;
            }
            
            public int Count { 
    	        get { return size; }
    	    }

            public Object SyncRoot {
    	        get {
                    return ((ICollection)entries).SyncRoot;
                }
    	    }

            public bool IsSynchronized {
                get {
                    return ((ICollection)entries).IsSynchronized;
                }
            }

            public void CopyTo(Array array, int arrayIndex) {
                if (array == null)
                    throw new ArgumentNullException("array");

			    if (arrayIndex < 0) 
                    throw new ArgumentOutOfRangeException("arrayIndex");

                Debug.Assert(array.Length >= size, "array is not big enough to hold all the items in the ICollection");

                for (int i = 0; i < size; i++) {
                    array.SetValue(entries[i].xso, arrayIndex++);
                }
            }
            
            public IEnumerator GetEnumerator() {
                return new XSOEnumerator(this.entries, this.size, EnumeratorType.Values);
            }
        }

        internal class XSOEnumerator : IEnumerator {
            private List<XmlSchemaObjectEntry> entries;
            private EnumeratorType enumType;

            protected int currentIndex;
            protected int size;
            protected XmlQualifiedName currentKey;
            protected XmlSchemaObject currentValue;
            

            internal XSOEnumerator(List<XmlSchemaObjectEntry> entries, int size, EnumeratorType enumType) {
                this.entries = entries;
                this.size = size;
                this.enumType = enumType;
                currentIndex = -1;
            }

            public Object Current {
                get {
                    if (currentIndex == -1) {
                        throw new InvalidOperationException(Res.GetString(Res.Sch_EnumNotStarted, string.Empty));
                    }
                    if (currentIndex >= size) {
                        throw new InvalidOperationException(Res.GetString(Res.Sch_EnumFinished, string.Empty));
                    }
                    switch(enumType) {
                        case EnumeratorType.Keys:
                            return currentKey;
                    
                        case EnumeratorType.Values:
                            return currentValue;

                        case EnumeratorType.DictionaryEntry:
                            return new DictionaryEntry(currentKey, currentValue);

                        default:
                            break;
                    }
                    return null;
                }
            }

            public bool MoveNext() {
                if (currentIndex >= size - 1) {
                    currentValue = null;
                    currentKey = null;
                    return false;
                }
                currentIndex++;
                currentValue = entries[currentIndex].xso;
                currentKey = entries[currentIndex].qname;
                return true;
            }

            public void Reset() {
                currentIndex = -1;
                currentValue = null;
                currentKey = null;
            }
        }

        internal class XSODictionaryEnumerator : XSOEnumerator, IDictionaryEnumerator {
           
            internal XSODictionaryEnumerator(List<XmlSchemaObjectEntry> entries, int size, EnumeratorType enumType) : base(entries, size, enumType) {
            }
            
            //IDictionaryEnumerator members
            public DictionaryEntry Entry {
                get {
                    if (currentIndex == -1) {
                        throw new InvalidOperationException(Res.GetString(Res.Sch_EnumNotStarted, string.Empty));
                    }
                    if (currentIndex >= size) {
                        throw new InvalidOperationException(Res.GetString(Res.Sch_EnumFinished, string.Empty));
                    }
                    return new DictionaryEntry(currentKey, currentValue);
                }
            }

            public object Key {
                get {
                    if (currentIndex == -1) {
                        throw new InvalidOperationException(Res.GetString(Res.Sch_EnumNotStarted, string.Empty));
                    }
                    if (currentIndex >= size) {
                        throw new InvalidOperationException(Res.GetString(Res.Sch_EnumFinished, string.Empty));
                    }
                    return currentKey;
                }
            }

            public object Value {
                get {
                    if (currentIndex == -1) {
                        throw new InvalidOperationException(Res.GetString(Res.Sch_EnumNotStarted, string.Empty));
                    }
                    if (currentIndex >= size) {
                        throw new InvalidOperationException(Res.GetString(Res.Sch_EnumFinished, string.Empty));
                    }
                    return currentValue;
                }
            }
        }

    }
}
