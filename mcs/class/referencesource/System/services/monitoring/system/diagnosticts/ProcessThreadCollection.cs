//------------------------------------------------------------------------------
// <copyright file="ProcessThreadCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    using System.Collections;
    using System;
    using System.IO;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class ProcessThreadCollection : ReadOnlyCollectionBase {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected ProcessThreadCollection() {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ProcessThreadCollection(ProcessThread[] processThreads) {
            InnerList.AddRange(processThreads);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ProcessThread this[int index] {
            get { return (ProcessThread)InnerList[index]; }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(ProcessThread thread) {
            return InnerList.Add(thread);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, ProcessThread thread) {
            InnerList.Insert(index, thread);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(ProcessThread thread) {
            return InnerList.IndexOf(thread);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(ProcessThread thread) {
            return InnerList.Contains(thread);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(ProcessThread thread) {
            InnerList.Remove(thread);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(ProcessThread[] array, int index) {
            InnerList.CopyTo(array, index);
        }

#if MOBILE
        [Obsolete ("This API is no longer available", true)]
        public int Capacity {
            get {
                throw new NotSupportedException ();
            }
            set {
                throw new NotSupportedException ();
            }
        }

        [Obsolete ("This API is no longer available", true)]
        public void AddRange (System.Collections.Generic.IEnumerable<ProcessThread> collection)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public System.Collections.ObjectModel.ReadOnlyCollection<ProcessThread> AsReadOnly()
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int BinarySearch(int index, int count, ProcessThread item, System.Collections.Generic.IComparer<ProcessThread> comparer)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int BinarySearch(ProcessThread item)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int BinarySearch(ProcessThread item, System.Collections.Generic.IComparer<ProcessThread> comparer)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Clear()
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public System.Collections.Generic.List<TOutput> ConvertAll<TOutput>(Converter<ProcessThread,TOutput> converter)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void CopyTo(ProcessThread[] array)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void CopyTo(int index, ProcessThread[] array, int arrayIndex, int count)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public bool Exists(Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public ProcessThread Find(Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public System.Collections.Generic.List<ProcessThread> FindAll(Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindIndex(Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindIndex(int startIndex, Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindIndex(int startIndex, int count, Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public ProcessThread FindLast(Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindLastIndex(Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindLastIndex(int startIndex, Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindLastIndex(int startIndex, int count, Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void ForEach(Action<ProcessThread> action)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public System.Collections.Generic.List<ProcessThread> GetRange(int index, int count)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int IndexOf(ProcessThread item, int index)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int IndexOf(ProcessThread item, int index, int count)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void InsertRange(int index, System.Collections.Generic.IEnumerable<ProcessThread> collection)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int LastIndexOf(ProcessThread item)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int LastIndexOf(ProcessThread item, int index)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int LastIndexOf(ProcessThread item, int index, int count)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int RemoveAll(Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void RemoveAt(int index)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void RemoveRange(int index, int count)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Reverse()
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Reverse(int index, int count)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Sort()
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Sort(System.Collections.Generic.IComparer<ProcessThread> comparer)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Sort(int index, int count, System.Collections.Generic.IComparer<ProcessThread> comparer)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Sort(Comparison<ProcessThread> comparison)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public ProcessThread[] ToArray()
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void TrimExcess()
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public bool TrueForAll(Predicate<ProcessThread> match)
        {
            throw new NotSupportedException ();
        }
#endif
        
    }
}

