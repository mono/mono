//------------------------------------------------------------------------------
// <copyright file="EventLogPermissionAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.ComponentModel;
    using System.Security;
    using System.Security.Permissions;   
         
    [
    AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly | AttributeTargets.Event, AllowMultiple = true, Inherited = false ),
    Serializable()
    ]     
    public class EventLogPermissionAttribute : CodeAccessSecurityAttribute {
        private string machineName;
        private EventLogPermissionAccess permissionAccess;
        
        public EventLogPermissionAttribute(SecurityAction action)
        : base(action) {
            this.machineName = ".";
            this.permissionAccess = EventLogPermissionAccess.Write;
        }

        public string MachineName {
            get {                
                return this.machineName;                
            }
            
            set {
                if (!SyntaxCheck.CheckMachineName(value))
                    throw new ArgumentException(SR.GetString(SR.InvalidProperty, "MachineName", value));
                    
                this.machineName = value;                                    
            }
        }
        
        public EventLogPermissionAccess PermissionAccess {
            get {
                return this.permissionAccess;
            }
            
            set {
                this.permissionAccess = value;
            }
        }                                                    
              
        public override IPermission CreatePermission() {            
             if (Unrestricted) 
                return new EventLogPermission(PermissionState.Unrestricted);
            
            return new EventLogPermission(this.PermissionAccess, this.MachineName);

        }
    }    
}   

