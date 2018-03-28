//----------------------------------------------------
// <copyright file="EventLogPermissionEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.ComponentModel;
    using System.Security.Permissions;

    [
    Serializable()
    ]     
    public class EventLogPermissionEntry {
        private string machineName;
        private EventLogPermissionAccess permissionAccess;
            
        public EventLogPermissionEntry(EventLogPermissionAccess permissionAccess, string machineName) {
            if (!SyntaxCheck.CheckMachineName(machineName))
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "MachineName", machineName));
                                    
            this.permissionAccess = permissionAccess;
            this.machineName = machineName;
        }  
        
        ///<internalonly/> 
        internal EventLogPermissionEntry(ResourcePermissionBaseEntry baseEntry) {
            this.permissionAccess = (EventLogPermissionAccess)baseEntry.PermissionAccess;
            this.machineName = baseEntry.PermissionAccessPath[0]; 
        }
        
        public string MachineName {
            get {                
                return this.machineName;                
            }                        
        }
        
        public EventLogPermissionAccess PermissionAccess {
            get {
                return this.permissionAccess;
            }                        
        }      
        
        ///<internalonly/> 
        internal ResourcePermissionBaseEntry GetBaseEntry() {
            ResourcePermissionBaseEntry baseEntry = new ResourcePermissionBaseEntry((int)this.PermissionAccess, new string[] {this.MachineName});            
            return baseEntry;
        }
    }
}


