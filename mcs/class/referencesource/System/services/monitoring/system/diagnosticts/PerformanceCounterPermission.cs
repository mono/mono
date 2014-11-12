//------------------------------------------------------------------------------
// <copyright file="PerformanceCounterPermission.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System;    
    using System.Security.Permissions;
                                                                        
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [
    Serializable()
    ]
    public sealed class PerformanceCounterPermission : ResourcePermissionBase {      
        private PerformanceCounterPermissionEntryCollection innerCollection;
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PerformanceCounterPermission() {
            SetNames();
        }                                                                
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PerformanceCounterPermission(PermissionState state) 
        : base(state) {
            SetNames();
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PerformanceCounterPermission(PerformanceCounterPermissionAccess permissionAccess, string machineName, string categoryName) {            
            SetNames();
            this.AddPermissionAccess(new PerformanceCounterPermissionEntry(permissionAccess, machineName, categoryName));              
        }         
         
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PerformanceCounterPermission(PerformanceCounterPermissionEntry[] permissionAccessEntries) {            
            if (permissionAccessEntries == null)
                throw new ArgumentNullException("permissionAccessEntries");
                
            SetNames();            
            for (int index = 0; index < permissionAccessEntries.Length; ++index)
                this.AddPermissionAccess(permissionAccessEntries[index]);                          
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>                
        public PerformanceCounterPermissionEntryCollection PermissionEntries {
            get {
                if (this.innerCollection == null)                     
                    this.innerCollection = new PerformanceCounterPermissionEntryCollection(this, base.GetPermissionEntries()); 
                                                                           
                return this.innerCollection;                                                               
            }
        }

        ///<internalonly/> 
        internal void AddPermissionAccess(PerformanceCounterPermissionEntry entry) {
            base.AddPermissionAccess(entry.GetBaseEntry());
        }
        
        ///<internalonly/> 
        internal new void Clear() {
            base.Clear();
        }

        ///<internalonly/> 
        internal void RemovePermissionAccess(PerformanceCounterPermissionEntry entry) {
            base.RemovePermissionAccess(entry.GetBaseEntry());
        }                                                                      
        
        private void SetNames() {
            this.PermissionAccessType = typeof(PerformanceCounterPermissionAccess);       
            this.TagNames = new string[]{"Machine", "Category"};
        } 
    }
}                        
