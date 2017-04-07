//------------------------------------------------------------------------------
// <copyright file="EventLogEntryCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Text;
    using System;
    using System.Collections;
   
    //Consider, V2, Microsoft: Is there a way to implement Contains
    //and IndexOf, can we live withouth this part of the ReadOnly
    //collection pattern?
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class EventLogEntryCollection : ICollection {
        private EventLogInternal log;

        internal EventLogEntryCollection(EventLogInternal log) {
            this.log = log;
        }
        
        /// <devdoc>
        ///    <para>
        ///       Gets the number of entries in the event log
        ///    </para>
        /// </devdoc>
        public int Count {
            get {
                return log.EntryCount;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets an entry in
        ///       the event log, based on an index starting at 0.
        ///    </para>
        /// </devdoc>
        public virtual EventLogEntry this[int index] {
            get {
                return log.GetEntryAt(index);
            }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(EventLogEntry[] entries, int index) {
            ((ICollection)this).CopyTo((Array)entries, index);
        }       
                       
        /// <devdoc>
        /// </devdoc>
        public IEnumerator GetEnumerator() {                
            return new EntriesEnumerator(this);
        } 

        internal EventLogEntry GetEntryAtNoThrow(int index) {
            return log.GetEntryAtNoThrow(index);
        }
        
        /// <internalonly/>
        bool ICollection.IsSynchronized {
            get {
                return false;
            }
        }

        /// <devdoc>        
        ///    ICollection private interface implementation.        
        /// </devdoc>
        /// <internalonly/>
        object ICollection.SyncRoot {
            get {
                return this;
            }
        }
    
        /// <devdoc>        
        ///    ICollection private interface implementation.        
        /// </devdoc>
        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            EventLogEntry[] entries = log.GetAllEntries();
            Array.Copy(entries, 0, array, index, entries.Length);			       				
        }                     
    
        /// <devdoc>
        ///    <para>
        ///       Holds an System.Diagnostics.EventLog.EventLogEntryCollection that
        ///       consists of the entries in an event
        ///       log.
        ///    </para>
        /// </devdoc>
        private class EntriesEnumerator : IEnumerator {
            private EventLogEntryCollection entries;
            private int num = -1;
            private EventLogEntry cachedEntry = null;

            internal EntriesEnumerator(EventLogEntryCollection entries) {
                this.entries = entries;
            }

            /// <devdoc>
            ///    <para>
            ///       Gets the entry at the current position.
            ///    </para>
            /// </devdoc>
            public object Current {
                get {
                    if (cachedEntry == null)
                        throw new InvalidOperationException(SR.GetString(SR.NoCurrentEntry));
                        
                    return cachedEntry;
                }
            }

            /// <devdoc>
            ///    <para>
            ///       Advances the enumerator to the next entry in the event log.
            ///    </para>
            /// </devdoc>
            public bool MoveNext() {
                num++;
                cachedEntry = entries.GetEntryAtNoThrow(num);
                
                return cachedEntry != null;
            }            
            
            /// <devdoc>
            ///    <para>
            ///       Resets the state of the enumeration.
            ///    </para>
            /// </devdoc>
            public void Reset() {
                num = -1;
            }
        }
    }
}
