//------------------------------------------------------------------------------
// <copyright file="CounterCreationDataCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System;
    using System.ComponentModel;
    using System.Collections;
 
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [Serializable()]
    public class CounterCreationDataCollection : CollectionBase {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CounterCreationDataCollection() {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CounterCreationDataCollection(CounterCreationDataCollection value) {
            this.AddRange(value);
        } 
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CounterCreationDataCollection(CounterCreationData[] value) {
            this.AddRange(value);
        }
         
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CounterCreationData this[int index] {
            get {
                return ((CounterCreationData)(List[index]));
            }
            set {
                List[index] = value;
            }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(CounterCreationData value) {
            return List.Add(value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void AddRange(CounterCreationData[] value) {
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
        public void AddRange(CounterCreationDataCollection value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            int currentCount = value.Count;
            for (int i = 0; i < currentCount; i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
                
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(CounterCreationData value) {
            return List.Contains(value);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(CounterCreationData[] array, int index) {
            List.CopyTo(array, index);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(CounterCreationData value) {
            return List.IndexOf(value);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, CounterCreationData value) {
            List.Insert(index, value);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void Remove(CounterCreationData value) {
            List.Remove(value);
        }       

        protected override void OnValidate(object value) {
            if (value == null)
                throw new ArgumentNullException("value");
            
            CounterCreationData dataToAdd = value as CounterCreationData;
            if (dataToAdd == null) 
                throw new ArgumentException(SR.GetString(SR.MustAddCounterCreationData));
        }
        
    }    
}
  
