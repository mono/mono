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
    }
}

