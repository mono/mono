//------------------------------------------------------------------------------
// <copyright file="DataBindingCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Web.Util;
    using System.Security.Permissions;


    /// <devdoc>
    /// </devdoc>
    public sealed class DataBindingCollection : ICollection {
        private EventHandler changedEvent;

        private Hashtable bindings;
        private Hashtable removedBindings;


        /// <devdoc>
        /// </devdoc>
        public DataBindingCollection() {
            this.bindings = new Hashtable(StringComparer.OrdinalIgnoreCase);
        }


        /// <devdoc>
        /// </devdoc>
        public int Count {
            get {
                return bindings.Count;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public bool IsReadOnly {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public bool IsSynchronized {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public string[] RemovedBindings {
            get {
                int bindingCount = 0;
                ICollection keys = null;

                if (removedBindings != null) {
                    keys = removedBindings.Keys;
                    bindingCount = keys.Count;

                    string[] removedNames = new string[bindingCount];
                    int i = 0;

                    foreach (string s in keys) {
                        removedNames[i++] = s;
                    }

                    removedBindings.Clear();
                    return removedNames;
                }
                else {
                    return new string[0];
                }
            }
        }


        /// <devdoc>
        /// </devdoc>
        private Hashtable RemovedBindingsTable {
            get {
                if (removedBindings == null) {
                    removedBindings = new Hashtable(StringComparer.OrdinalIgnoreCase);
                }
                return removedBindings;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public object SyncRoot {
            get {
                return this;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public DataBinding this[string propertyName] {
            get {
                object o = bindings[propertyName];
                if (o != null)
                    return(DataBinding)o;
                return null;
            }
        }


        public event EventHandler Changed {
            add {
                changedEvent = (EventHandler)Delegate.Combine(changedEvent, value);
            }
            remove {
                changedEvent = (EventHandler)Delegate.Remove(changedEvent, value);
            }
        }



        /// <devdoc>
        /// </devdoc>
        public void Add(DataBinding binding) {
            bindings[binding.PropertyName] = binding;
            RemovedBindingsTable.Remove(binding.PropertyName);

            OnChanged();
        }


        /// <devdoc>
        /// </devdoc>
        public bool Contains(string propertyName) {
            return bindings.Contains(propertyName);
        }


        /// <devdoc>
        /// </devdoc>
        public void Clear() {
            ICollection keys = bindings.Keys;
            if ((keys.Count != 0) && (removedBindings == null)) {
                // ensure the removedBindings hashtable is created
                Hashtable h = RemovedBindingsTable;
            }
            foreach (string s in keys) {
                removedBindings[s] = String.Empty;
            }

            bindings.Clear();

            OnChanged();
        }


        /// <devdoc>
        /// </devdoc>
        public void CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }


        /// <devdoc>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return bindings.Values.GetEnumerator();
        }

        private void OnChanged() {
            if (changedEvent != null) {
                changedEvent(this, EventArgs.Empty);
            }
        }


        /// <devdoc>
        /// </devdoc>
        public void Remove(string propertyName) {
            Remove(propertyName, true);
        }


        /// <devdoc>
        /// </devdoc>
        public void Remove(DataBinding binding) {
            Remove(binding.PropertyName, true);
        }


        /// <devdoc>
        /// </devdoc>
        public void Remove(string propertyName, bool addToRemovedList) {
            if (Contains(propertyName)) {
                bindings.Remove(propertyName);
                if (addToRemovedList) {
                    RemovedBindingsTable[propertyName] = String.Empty;
                }

                OnChanged();
            }
        }
    }
}

