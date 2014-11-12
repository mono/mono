//------------------------------------------------------------------------------
// <copyright file="EventLogPermission.cs" company="Microsoft">
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
    public sealed class EventLogPermission : ResourcePermissionBase {    
        private EventLogPermissionEntryCollection innerCollection;
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EventLogPermission() {
            SetNames();
        }                                                                
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EventLogPermission(PermissionState state) 
        : base(state) {
            SetNames();
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EventLogPermission(EventLogPermissionAccess permissionAccess, string machineName) {            
            SetNames();
            this.AddPermissionAccess(new EventLogPermissionEntry(permissionAccess, machineName));              
        }         
         
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EventLogPermission(EventLogPermissionEntry[] permissionAccessEntries) {            
            if (permissionAccessEntries == null)
                throw new ArgumentNullException("permissionAccessEntries");
                
            SetNames();            
            for (int index = 0; index < permissionAccessEntries.Length; ++index)
                this.AddPermissionAccess(permissionAccessEntries[index]);                          
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>                
        public EventLogPermissionEntryCollection PermissionEntries {
            get {
                if (this.innerCollection == null)                     
                    this.innerCollection = new EventLogPermissionEntryCollection(this, base.GetPermissionEntries()); 
                                                                           
                return this.innerCollection;                                                               
            }
        }

        ///<internalonly/> 
        internal void AddPermissionAccess(EventLogPermissionEntry entry) {
            base.AddPermissionAccess(entry.GetBaseEntry());
        }
        
        ///<internalonly/> 
        internal new void Clear() {
            base.Clear();
        }

        ///<internalonly/> 
        internal void RemovePermissionAccess(EventLogPermissionEntry entry) {
            base.RemovePermissionAccess(entry.GetBaseEntry());
        }
                        
        private void SetNames() {
            this.PermissionAccessType = typeof(EventLogPermissionAccess);
            this.TagNames = new string[]{"Machine"};
        }                                
    }
}  
