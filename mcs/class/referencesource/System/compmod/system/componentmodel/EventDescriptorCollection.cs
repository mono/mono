//------------------------------------------------------------------------------
// <copyright file="EventDescriptorCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

/*
 This class has the HostProtectionAttribute. The purpose of this attribute is to enforce host-specific programming model guidelines, not security behavior. 
 Suppress FxCop message - BUT REVISIT IF ADDING NEW SECURITY ATTRIBUTES.
*/
[assembly: SuppressMessage("Microsoft.Security", "CA2112:SecuredTypesShouldNotExposeFields", Scope="type", Target="System.ComponentModel.EventDescriptorCollection")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.get_IsFixedSize():System.Boolean")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.ICollection.get_SyncRoot():System.Object")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.ICollection.get_IsSynchronized():System.Boolean")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.get_IsReadOnly():System.Boolean")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.Clear():System.Void")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IEnumerable.GetEnumerator():System.Collections.IEnumerator")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.ICollection.CopyTo(System.Array,System.Int32):System.Void")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.ICollection.get_Count():System.Int32")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.Contains(System.Object):System.Boolean")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.Remove(System.Object):System.Void")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.get_Item(System.Int32):System.Object")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.set_Item(System.Int32,System.Object):System.Void")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.Add(System.Object):System.Int32")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.IndexOf(System.Object):System.Int32")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.RemoveAt(System.Int32):System.Void")]
[assembly: SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.System.Collections.IList.Insert(System.Int32,System.Object):System.Void")]
[assembly: SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope="member", Target="System.ComponentModel.EventDescriptorCollection..ctor(System.ComponentModel.EventDescriptor[],System.Boolean)")]
[assembly: SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.get_Count():System.Int32")]
[assembly: SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope="member", Target="System.ComponentModel.EventDescriptorCollection.GetEnumerator():System.Collections.IEnumerator")]


namespace System.ComponentModel {
    using System.Runtime.InteropServices;
    

    using System.Diagnostics;

    using Microsoft.Win32;
    using System.Collections;
    using System.Globalization;
    
    /// <devdoc>
    ///    <para>
    ///       Represents a collection of events.
    ///    </para>
    /// </devdoc>
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Security.Permissions.HostProtection(Synchronization=true)]
    public class EventDescriptorCollection : ICollection, IList {
        private EventDescriptor[] events;
        private string[]          namedSort;
        private IComparer         comparer;
        private bool              eventsOwned = true;
        private bool              needSort = false;
        private int               eventCount;
        private bool              readOnly = false;

        
        /// <devdoc>
        /// An empty AttributeCollection that can used instead of creating a new one with no items.
        /// </devdoc>
        public static readonly EventDescriptorCollection Empty = new EventDescriptorCollection(null, true);

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.EventDescriptorCollection'/> class.
        ///    </para>
        /// </devdoc>
        public EventDescriptorCollection(EventDescriptor[] events) {
            this.events = events;
            if (events == null) {
                this.events = new EventDescriptor[0];
                this.eventCount = 0;
            }
            else {
                this.eventCount = this.events.Length;
            }
            this.eventsOwned = true;
        }

        /// <devdoc>
        ///     Initializes a new instance of an event descriptor collection, and allows you to mark the
        ///     collection as read-only so it cannot be modified.
        /// </devdoc>
        public EventDescriptorCollection(EventDescriptor[] events, bool readOnly) : this(events) {
            this.readOnly = readOnly;
        }

        private EventDescriptorCollection(EventDescriptor[] events, int eventCount, string[] namedSort, IComparer comparer) {
            this.eventsOwned = false;
            if (namedSort != null) {
               this.namedSort = (string[])namedSort.Clone();
            }
            this.comparer = comparer;
            this.events = events;
            this.eventCount = eventCount;
            this.needSort = true;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the number
        ///       of event descriptors in the collection.
        ///    </para>
        /// </devdoc>
        public int Count {
            get {
                return eventCount;
            }
        }

        /// <devdoc>
        ///    <para>Gets the event with the specified index 
        ///       number.</para>
        /// </devdoc>
        public virtual EventDescriptor this[int index] {
            get {
                if (index >= eventCount) {
                    throw new IndexOutOfRangeException();
                }
                EnsureEventsOwned();
                return events[index];
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the event with the specified name.
        ///    </para>
        /// </devdoc>
        public virtual EventDescriptor this[string name] {
            get {
                return Find(name, false);
            }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(EventDescriptor value) {
            if (readOnly) {
                throw new NotSupportedException();
            }

            EnsureSize(eventCount + 1);
            events[eventCount++] = value;
            return eventCount - 1;
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Clear() {
            if (readOnly) {
                throw new NotSupportedException();
            }

            eventCount = 0;
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(EventDescriptor value) {
            return IndexOf(value) >= 0;
        }

        /// <internalonly/>
        void ICollection.CopyTo(Array array, int index) {
            EnsureEventsOwned();
            Array.Copy(events, 0, array, index, Count);
        }
        
        private void EnsureEventsOwned() {
            if (!eventsOwned) {
               eventsOwned = true;
               if (events != null) {
                  EventDescriptor[] newEvents = new EventDescriptor[Count];
                  Array.Copy(events, 0, newEvents, 0, Count);
                  this.events = newEvents;
               }
            }
        
            if (needSort) {
               needSort = false;
               InternalSort(this.namedSort);
            }
        }
        
        private void EnsureSize(int sizeNeeded) {
            
            if (sizeNeeded <= events.Length) {
               return;
            }
            
            if (events == null || events.Length == 0) {
                eventCount = 0;
                events = new EventDescriptor[sizeNeeded];
                return;
            }
            
            EnsureEventsOwned();
            
            int newSize = Math.Max(sizeNeeded, events.Length * 2);
            EventDescriptor[] newEvents = new EventDescriptor[newSize];
            Array.Copy(events, 0, newEvents, 0, eventCount);
            events = newEvents;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the description of the event with the specified
        ///       name
        ///       in the collection.
        ///    </para>
        /// </devdoc>
        public virtual EventDescriptor Find(string name, bool ignoreCase) {
            EventDescriptor p = null;
            
            if (ignoreCase) {
                for(int i = 0; i < Count; i++) {
                    if (String.Equals(events[i].Name, name, StringComparison.OrdinalIgnoreCase)) {
                        p = events[i];
                        break;
                    }
                }
            }
            else {
                for(int i = 0; i < Count; i++) {
                    if (String.Equals(events[i].Name, name, StringComparison.Ordinal)) {
                        p = events[i];
                        break;
                    }
                }
            }
            
            return p;
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(EventDescriptor value) {
            return Array.IndexOf(events, value, 0, eventCount);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, EventDescriptor value) {
            if (readOnly) {
                throw new NotSupportedException();
            }

            EnsureSize(eventCount + 1);
            if (index < eventCount) {
                Array.Copy(events, index, events, index + 1, eventCount - index);   
            }
            events[index] = value;
            eventCount++;
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(EventDescriptor value) {
            if (readOnly) {
                throw new NotSupportedException();
            }

            int index = IndexOf(value);
            
            if (index != -1) {
                RemoveAt(index);
            }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void RemoveAt(int index) {
            if (readOnly) {
                throw new NotSupportedException();
            }

            if (index < eventCount - 1) {
                  Array.Copy(events, index + 1, events, index, eventCount - index - 1);
            }
            events[eventCount - 1] = null;
            eventCount--;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets an enumerator for this <see cref='System.ComponentModel.EventDescriptorCollection'/>.
        ///    </para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            // we can only return an enumerator on the events we actually have...
            if (events.Length == eventCount) {
                return events.GetEnumerator();
            }
            else {
                return new ArraySubsetEnumerator(events, eventCount);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Sorts the members of this EventDescriptorCollection, using the default sort for this collection, 
        ///       which is usually alphabetical.
        ///    </para>
        /// </devdoc>
        public virtual EventDescriptorCollection Sort() {
            return new EventDescriptorCollection(this.events, this.eventCount, this.namedSort, this.comparer);
        }
        

        /// <devdoc>
        ///    <para>
        ///       Sorts the members of this EventDescriptorCollection.  Any specified NamedSort arguments will 
        ///       be applied first, followed by sort using the specified IComparer.
        ///    </para>
        /// </devdoc>
        public virtual EventDescriptorCollection Sort(string[] names) {
            return new EventDescriptorCollection(this.events, this.eventCount, names, this.comparer);
        }
        
        /// <devdoc>
        ///    <para>
        ///       Sorts the members of this EventDescriptorCollection.  Any specified NamedSort arguments will 
        ///       be applied first, followed by sort using the specified IComparer.
        ///    </para>
        /// </devdoc>
        public virtual EventDescriptorCollection Sort(string[] names, IComparer comparer) {
            return new EventDescriptorCollection(this.events, this.eventCount, names, comparer);
        }
        
         /// <devdoc>
        ///    <para>
        ///       Sorts the members of this EventDescriptorCollection, using the specified IComparer to compare, 
        ///       the EventDescriptors contained in the collection.
        ///    </para>
        /// </devdoc>
        public virtual EventDescriptorCollection Sort(IComparer comparer) {
            return new EventDescriptorCollection(this.events, this.eventCount, this.namedSort, comparer);
        }
        
        /// <devdoc>
        ///    <para>
        ///       Sorts the members of this EventDescriptorCollection.  Any specified NamedSort arguments will 
        ///       be applied first, followed by sort using the specified IComparer.
        ///    </para>
        /// </devdoc>
        protected void InternalSort(string[] names) {
            if (events == null || events.Length == 0) {
                return;
            }  
            
            this.InternalSort(this.comparer);
            
            if (names != null && names.Length > 0) {
            
               ArrayList eventArrayList = new ArrayList(events);
               int foundCount = 0;
               int eventCount = events.Length;
               
               for (int i = 0; i < names.Length; i++) {
                    for (int j = 0; j < eventCount; j++) {
                        EventDescriptor currentEvent = (EventDescriptor)eventArrayList[j];
                        
                        // Found a matching event.  Here, we add it to our array.  We also
                        // mark it as null in our array list so we don't add it twice later.
                        //
                        if (currentEvent != null && currentEvent.Name.Equals(names[i])) {
                            events[foundCount++] = currentEvent;
                            eventArrayList[j] = null;
                            break;
                        }
                    }
               }
                
               // At this point we have filled in the first "foundCount" number of propeties, one for each
               // name in our name array.  If a name didn't match, then it is ignored.  Next, we must fill
               // in the rest of the properties.  We now have a sparse array containing the remainder, so
               // it's easy.
               //
               for (int i = 0; i < eventCount; i++) {
                   if (eventArrayList[i] != null) {
                       events[foundCount++] = (EventDescriptor)eventArrayList[i];
                   }
               }
               
               Debug.Assert(foundCount == eventCount, "We did not completely fill our event array");
            }
        }
        
        /// <devdoc>
        ///    <para>
        ///       Sorts the members of this EventDescriptorCollection using the specified IComparer.
        ///    </para>
        /// </devdoc>
        protected void InternalSort(IComparer sorter) {
            if (sorter == null) {
                TypeDescriptor.SortDescriptorArray(this);
            }
            else {
                Array.Sort(events, sorter);
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
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        
        /// <internalonly/>
        object IList.this[int index] {
            get {
                return this[index];
            }
            set {
                if (readOnly) {
                    throw new NotSupportedException();
                }

                if (index >= eventCount) {
                    throw new IndexOutOfRangeException();
                }
                EnsureEventsOwned();
                events[index] = (EventDescriptor)value;
            }
        }
        
        /// <internalonly/>
        int IList.Add(object value) {
            return Add((EventDescriptor)value);
        }
        
        /// <internalonly/>
        void IList.Clear() {
            Clear();
        }
        
        /// <internalonly/>
        bool IList.Contains(object value) {
            return Contains((EventDescriptor)value);
        }
        
        /// <internalonly/>
        int IList.IndexOf(object value) {
            return IndexOf((EventDescriptor)value);
        }
        
        /// <internalonly/>
        void IList.Insert(int index, object value) {
            Insert(index, (EventDescriptor)value);
        }
        
        /// <internalonly/>
        void IList.Remove(object value) {
            Remove((EventDescriptor)value);
        }
        
        /// <internalonly/>
        void IList.RemoveAt(int index) {
            RemoveAt(index);
        }

        /// <internalonly/>
        bool IList.IsReadOnly {
            get {
                return readOnly;
            }
        }

        /// <internalonly/>
        bool IList.IsFixedSize {
            get {
                return readOnly;
            }
        }
    }
}

