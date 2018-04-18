//------------------------------------------------------------------------------
// <copyright file="ProcessModuleCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System;
    using System.Collections;
    using System.Diagnostics;
    
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class ProcessModuleCollection : ReadOnlyCollectionBase {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected ProcessModuleCollection() {
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ProcessModuleCollection(ProcessModule[] processModules) {
            InnerList.AddRange(processModules);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ProcessModule this[int index] {
            get { return (ProcessModule)InnerList[index]; }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(ProcessModule module) {
            return InnerList.IndexOf(module);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(ProcessModule module) {
            return InnerList.Contains(module);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(ProcessModule[] array, int index) {
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
        public void Add (ProcessModule item)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void AddRange (System.Collections.Generic.IEnumerable<ProcessModule> collection)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public System.Collections.ObjectModel.ReadOnlyCollection<ProcessModule> AsReadOnly()
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int BinarySearch(int index, int count, ProcessModule item, System.Collections.Generic.IComparer<ProcessModule> comparer)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int BinarySearch(ProcessModule item)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int BinarySearch(ProcessModule item, System.Collections.Generic.IComparer<ProcessModule> comparer)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Clear()
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public System.Collections.Generic.List<TOutput> ConvertAll<TOutput>(Converter<ProcessModule,TOutput> converter)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void CopyTo(ProcessModule[] array)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void CopyTo(int index, ProcessModule[] array, int arrayIndex, int count)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public bool Exists(Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public ProcessModule Find(Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public System.Collections.Generic.List<ProcessModule> FindAll(Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindIndex(Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindIndex(int startIndex, Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindIndex(int startIndex, int count, Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public ProcessModule FindLast(Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindLastIndex(Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindLastIndex(int startIndex, Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int FindLastIndex(int startIndex, int count, Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void ForEach(Action<ProcessModule> action)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public System.Collections.Generic.List<ProcessModule> GetRange(int index, int count)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int IndexOf(ProcessModule item, int index)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int IndexOf(ProcessModule item, int index, int count)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Insert(int index, ProcessModule item)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void InsertRange(int index, System.Collections.Generic.IEnumerable<ProcessModule> collection)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int LastIndexOf(ProcessModule item)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int LastIndexOf(ProcessModule item, int index)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int LastIndexOf(ProcessModule item, int index, int count)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public bool Remove(ProcessModule item)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public int RemoveAll(Predicate<ProcessModule> match)
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
        public void Sort(System.Collections.Generic.IComparer<ProcessModule> comparer)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Sort(int index, int count, System.Collections.Generic.IComparer<ProcessModule> comparer)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void Sort(Comparison<ProcessModule> comparison)
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public ProcessModule[] ToArray()
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public void TrimExcess()
        {
            throw new NotSupportedException ();
        }

        [Obsolete ("This API is no longer available", true)]
        public bool TrueForAll(Predicate<ProcessModule> match)
        {
            throw new NotSupportedException ();
        }
#endif
    }
}

