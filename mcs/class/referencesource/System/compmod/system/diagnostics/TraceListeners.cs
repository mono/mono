//------------------------------------------------------------------------------
// <copyright file="TraceListeners.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Diagnostics {
    using System;
    using System.Collections;
    using Microsoft.Win32;
    
    /// <devdoc>
    /// <para>Provides a thread-safe list of <see cref='System.Diagnostics.TraceListenerCollection'/>. A thread-safe list is synchronized.</para>
    /// </devdoc>
    public class TraceListenerCollection : IList {
        ArrayList list;

        internal TraceListenerCollection() {
            list = new ArrayList(1);
        }

        /// <devdoc>
        /// <para>Gets or sets the <see cref='TraceListener'/> at
        ///    the specified index.</para>
        /// </devdoc>
        public TraceListener this[int i] {
            get {
                return (TraceListener)list[i];
            }

            set {
                InitializeListener(value);
                list[i] = value;
            }            
        }

        /// <devdoc>
        /// <para>Gets the first <see cref='System.Diagnostics.TraceListener'/> in the list with the specified name.</para>
        /// </devdoc>
        public TraceListener this[string name] {
            get {
                foreach (TraceListener listener in this) {
                    if (listener.Name == name)
                        return listener;
                }
                return null;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the number of listeners in the list.
        ///    </para>
        /// </devdoc>
        public int Count { 
            get {
                return list.Count;
            }
        }

        /// <devdoc>
        /// <para>Adds a <see cref='System.Diagnostics.TraceListener'/> to the list.</para>
        /// </devdoc>
        public int Add(TraceListener listener) {                
            InitializeListener(listener);

            lock (TraceInternal.critSec) {
                return list.Add(listener);
            }        
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void AddRange(TraceListener[] value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; ((i) < (value.Length)); i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }        
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void AddRange(TraceListenerCollection value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            int currentCount = value.Count;
            for (int i = 0; i < currentCount; i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
        
        /// <devdoc>
        ///    <para>
        ///       Clears all the listeners from the
        ///       list.
        ///    </para>
        /// </devdoc>
        public void Clear() {
            list = new ArrayList();
        }        

        /// <devdoc>
        ///    <para>Checks whether the list contains the specified 
        ///       listener.</para>
        /// </devdoc>
        public bool Contains(TraceListener listener) {
            return ((IList)this).Contains(listener);
        }

        /// <devdoc>
        /// <para>Copies a section of the current <see cref='System.Diagnostics.TraceListenerCollection'/> list to the specified array at the specified 
        ///    index.</para>
        /// </devdoc>
        public void CopyTo(TraceListener[] listeners, int index) {
            ((ICollection)this).CopyTo((Array) listeners, index);                   
        }
        
        /// <devdoc>
        ///    <para>
        ///       Gets an enumerator for this list.
        ///    </para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return list.GetEnumerator();
        }

        internal void InitializeListener(TraceListener listener) {
            if (listener == null)
                throw new ArgumentNullException("listener");
            
            listener.IndentSize = TraceInternal.IndentSize;
            listener.IndentLevel = TraceInternal.IndentLevel;
        }

        /// <devdoc>
        ///    <para>Gets the index of the specified listener.</para>
        /// </devdoc>
        public int IndexOf(TraceListener listener) {
            return ((IList)this).IndexOf(listener);
        }

        /// <devdoc>
        ///    <para>Inserts the listener at the specified index.</para>
        /// </devdoc>
        public void Insert(int index, TraceListener listener) {
            InitializeListener(listener);
            lock (TraceInternal.critSec) {
                list.Insert(index, listener);
            }            
        }

        /// <devdoc>
        ///    <para>
        ///       Removes the specified instance of the <see cref='System.Diagnostics.TraceListener'/> class from the list.
        ///    </para>
        /// </devdoc>
        public void Remove(TraceListener listener) {
            ((IList)this).Remove(listener);
        }

        /// <devdoc>
        ///    <para>Removes the first listener in the list that has the 
        ///       specified name.</para>
        /// </devdoc>
        public void Remove(string name) {
            TraceListener listener = this[name];
            if (listener != null)
                ((IList)this).Remove(listener);
        }

        /// <devdoc>
        /// <para>Removes the <see cref='System.Diagnostics.TraceListener'/> at the specified index.</para>
        /// </devdoc>
        public void RemoveAt(int index) {
            lock (TraceInternal.critSec) {
                list.RemoveAt(index);
            }
        }

       /// <internalonly/>
       object IList.this[int index] {
            get {
                return list[index];
            }

            set {
                TraceListener listener = value as TraceListener;
                if (listener == null)
                    throw new ArgumentException(SR.GetString(SR.MustAddListener),"value");
                InitializeListener(listener);
                list[index] = listener;
                
            }
        }
        
        /// <internalonly/>
        bool IList.IsReadOnly {
            get {
                return false;
            }
        }

        /// <internalonly/>
        bool IList.IsFixedSize {
            get {
                return false;
            }
        }
        
        /// <internalonly/>
        int IList.Add(object value) {
            TraceListener listener = value as TraceListener;
            if (listener == null)
                throw new ArgumentException(SR.GetString(SR.MustAddListener),"value");
            
            InitializeListener(listener);
            
            lock (TraceInternal.critSec) {
                return list.Add(value);
            }        
        }
        
        /// <internalonly/>
        bool IList.Contains(object value) {
            return list.Contains(value);
        }
        
        /// <internalonly/>
        int IList.IndexOf(object value) {
            return list.IndexOf(value);
        }
        
        /// <internalonly/>
        void IList.Insert(int index, object value) {
            TraceListener listener = value as TraceListener;
            if (listener == null)
                throw new ArgumentException(SR.GetString(SR.MustAddListener),"value");
            
            InitializeListener(listener);
            
            lock (TraceInternal.critSec) {
                list.Insert(index, value);
            }            
        }
        
        /// <internalonly/>
        void IList.Remove(object value) {
            lock (TraceInternal.critSec) {
                list.Remove(value);
            }
        }
        
        /// <internalonly/>
        object ICollection.SyncRoot {
            get {
                return this;
            }
        }
                                                  
        /// <internalonly/>
        bool ICollection.IsSynchronized {
            get {
                return true;
            }
        }
        
        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            lock (TraceInternal.critSec) {
                list.CopyTo(array, index);
            }
        }
    }
}
