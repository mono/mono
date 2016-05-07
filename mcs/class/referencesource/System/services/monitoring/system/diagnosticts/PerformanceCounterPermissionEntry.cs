//----------------------------------------------------
// <copyright file="PerformanceCounterPermissionEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.ComponentModel;
    using System.Security.Permissions;
    
    [
    Serializable()
    ]
    public class PerformanceCounterPermissionEntry {
        private string categoryName;
        private string machineName;
        private PerformanceCounterPermissionAccess permissionAccess;
                    
        public PerformanceCounterPermissionEntry(PerformanceCounterPermissionAccess permissionAccess, string machineName, string categoryName) {
            if (categoryName == null)
                throw new ArgumentNullException("categoryName");
            if (( (int) permissionAccess & ~(0x7)) != 0)
                throw new ArgumentException(SR.GetString(SR.InvalidParameter,  "permissionAccess", permissionAccess));

            if (machineName == null)
                throw new ArgumentNullException("machineName");
            if (!SyntaxCheck.CheckMachineName(machineName))
                throw new ArgumentException(SR.GetString(SR.InvalidParameter, "MachineName", machineName));
                                            
            this.permissionAccess = permissionAccess;
            this.machineName = machineName;
            this.categoryName = categoryName;
        }
        
        ///<internalonly/> 
        internal PerformanceCounterPermissionEntry(ResourcePermissionBaseEntry baseEntry) {
            this.permissionAccess = (PerformanceCounterPermissionAccess)baseEntry.PermissionAccess;
            this.machineName = baseEntry.PermissionAccessPath[0]; 
            this.categoryName = baseEntry.PermissionAccessPath[1]; 
        }
        
        public string CategoryName {
            get {
                return this.categoryName;
            }                        
        }

        public string MachineName {
            get {
                return this.machineName;
            }            
        }

        public PerformanceCounterPermissionAccess PermissionAccess {            
            get {
                return this.permissionAccess;
            }            
        }           
        
        ///<internalonly/> 
        internal ResourcePermissionBaseEntry GetBaseEntry() {
            ResourcePermissionBaseEntry baseEntry = new ResourcePermissionBaseEntry((int)this.PermissionAccess, new string[] {this.MachineName, this.CategoryName});            
            return baseEntry;
        }  
    }
}    

