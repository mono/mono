//------------------------------------------------------------------------------
// <copyright file="InstanceDataCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.Globalization;
    
    /// <devdoc>
    ///     A collection containing all the instance data for a counter.  This collection is contained in the 
    ///     <see cref='System.Diagnostics.InstanceDataCollectionCollection'/> when using the 
    ///     <see cref='System.Diagnostics.PerformanceCounterCategory.ReadCategory'/> method.  
    /// </devdoc>    
    public class InstanceDataCollection : DictionaryBase {
        private string counterName;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [Obsolete("This constructor has been deprecated.  Please use System.Diagnostics.InstanceDataCollectionCollection.get_Item to get an instance of this collection instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public InstanceDataCollection(string counterName) {
            if (counterName == null)
                throw new ArgumentNullException("counterName");
            this.counterName = counterName;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string CounterName {
            get {
                return counterName;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICollection Keys {
            get { return Dictionary.Keys; }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICollection Values {
            get {
                return Dictionary.Values;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public InstanceData this[string instanceName] {
            get {
                if (instanceName == null)
                    throw new ArgumentNullException("instanceName");

                if (instanceName.Length == 0)
                    instanceName = PerformanceCounterLib.SingleInstanceName;
                    
                object objectName = instanceName.ToLower(CultureInfo.InvariantCulture);
                return (InstanceData) Dictionary[objectName];
            }
        }

        internal void Add(string instanceName, InstanceData value) {
            object objectName = instanceName.ToLower(CultureInfo.InvariantCulture); 
            Dictionary.Add(objectName, value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(string instanceName) {
            if (instanceName == null)
                    throw new ArgumentNullException("instanceName");
                    
            object objectName = instanceName.ToLower(CultureInfo.InvariantCulture);
            return Dictionary.Contains(objectName);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(InstanceData[] instances, int index) {
            Dictionary.Values.CopyTo((Array)instances, index);
        }
    }
}


