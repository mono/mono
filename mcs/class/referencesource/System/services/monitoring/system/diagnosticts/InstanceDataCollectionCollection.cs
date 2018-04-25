//------------------------------------------------------------------------------
// <copyright file="InstanceDataCollectionCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {

    using System.Diagnostics;
    using System;
    using System.Collections;
    using System.Globalization;
    
    /// <devdoc>
    ///     The collection returned from  the <see cref='System.Diagnostics.PerformanceCounterCategory.ReadCategory'/> method.  
    ///     that contains all the counter and instance data.
    ///     The collection contains an InstanceDataCollection object for each counter.  Each InstanceDataCollection
    ///     object contains the performance data for all counters for that instance.  In other words the data is
    ///     indexed by counter name and then by instance name.
    /// </devdoc>    
    public class InstanceDataCollectionCollection : DictionaryBase {

    
        [Obsolete("This constructor has been deprecated.  Please use System.Diagnostics.PerformanceCounterCategory.ReadCategory() to get an instance of this collection instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public InstanceDataCollectionCollection() : base() {}
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public InstanceDataCollection this[string counterName] {
            get {
                if (counterName == null)
                    throw new ArgumentNullException("counterName");
                    
                object objectName = counterName.ToLower(CultureInfo.InvariantCulture);
                return (InstanceDataCollection) Dictionary[objectName];
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

        internal void Add(string counterName, InstanceDataCollection value) {
            object objectName = counterName.ToLower(CultureInfo.InvariantCulture); 
            Dictionary.Add(objectName, value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(string counterName) {    
            if (counterName == null)
                    throw new ArgumentNullException("counterName");
                    
            object objectName = counterName.ToLower(CultureInfo.InvariantCulture);
            return Dictionary.Contains(objectName);
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(InstanceDataCollection[] counters, int index) {
            Dictionary.Values.CopyTo((Array)counters, index);
        }
    }
}
