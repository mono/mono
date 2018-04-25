//------------------------------------------------------------------------------
// <copyright file="XmlNamedNodeMap.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">lionelu</owner>
//------------------------------------------------------------------------------

namespace System.Xml {
    using System.Collections;

    partial class XmlNamedNodeMap {

        // Optimized to minimize space in the zero or one element cases.
        internal struct SmallXmlNodeList {

            // If field is null, that represents an empty list.
            // If field is non-null, but not an ArrayList, then the 'list' contains a single
            // object.
            // Otherwise, field is an ArrayList. Once the field upgrades to an ArrayList, it
            // never degrades back, even if all elements are removed.
            private object field;

            public int Count {
                get {
                    if (field == null)
                        return 0;

                    ArrayList list = field as ArrayList;
                    if (list != null)
                        return list.Count;

                    return 1;
                }
            }

            public object this[int index] {
                get {
                    if (field == null)
                        throw new ArgumentOutOfRangeException("index");

                    ArrayList list = field as ArrayList;
                    if (list != null)
                        return list[index];

                    if (index != 0)
                        throw new ArgumentOutOfRangeException("index");

                    return field;
                }
            }

            public void Add(object value) {
                if (field == null) {
                    if (value == null) {
                        // If a single null value needs to be stored, then
                        // upgrade to an ArrayList
                        ArrayList temp = new ArrayList();
                        temp.Add(null);
                        field = temp;
                    }
                    else
                        field = value;

                    return;
                }

                ArrayList list = field as ArrayList;
                if (list != null) {
                    list.Add(value);
                }
                else {
                    list = new ArrayList();
                    list.Add(field);
                    list.Add(value);
                    field = list;
                }
            }

            public void RemoveAt(int index) {
                if (field == null)
                    throw new ArgumentOutOfRangeException("index");

                ArrayList list = field as ArrayList;
                if (list != null) {
                    list.RemoveAt(index);
                    return;
                }

                if (index != 0)
                    throw new ArgumentOutOfRangeException("index");

                field = null;
            }

            public void Insert(int index, object value) {
                if (field == null) {
                    if (index != 0)
                        throw new ArgumentOutOfRangeException("index");
                    Add(value);
                    return;
                }

                ArrayList list = field as ArrayList;
                if (list != null) {
                    list.Insert(index, value);
                    return;
                }

                if (index == 0) {
                    list = new ArrayList();
                    list.Add(value);
                    list.Add(field);
                    field = list;
                }
                else if (index == 1) {
                    list = new ArrayList();
                    list.Add(field);
                    list.Add(value);
                    field = list;
                }
                else {
                    throw new ArgumentOutOfRangeException("index");
                }
            }

            class SingleObjectEnumerator : IEnumerator {
                object loneValue;
                int position = -1;

                public SingleObjectEnumerator(object value) {
                    loneValue = value;
                }

                public object Current {
                    get {
                        if (position != 0) {
                            throw new InvalidOperationException();
                        }
                        return this.loneValue;
                    }
                }

                public bool MoveNext() {
                    if (position < 0) {
                        position = 0;
                        return true;
                    }
                    position = 1;
                    return false;
                }

                public void Reset() {
                    position = -1;
                }
            }

            public IEnumerator GetEnumerator() {
                if (field == null) {
                    return XmlDocument.EmptyEnumerator;
                }

                ArrayList list = field as ArrayList;
                if (list != null) {
                    return list.GetEnumerator();
                }

                return new SingleObjectEnumerator(field);
            }
        }
    }
}
