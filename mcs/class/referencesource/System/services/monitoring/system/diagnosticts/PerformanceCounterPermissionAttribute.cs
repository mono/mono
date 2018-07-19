//------------------------------------------------------------------------------
// <copyright file="PerformanceCounterPermissionAttribute.cs" company="Microsoft">
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
    public class PerformanceCounterPermissionAttribute : CodeAccessSecurityAttribute {        
        private string categoryName;
        private string machineName;
        private PerformanceCounterPermissionAccess permissionAccess;
        
        public PerformanceCounterPermissionAttribute(SecurityAction action)
        : base(action) {
            this.categoryName = "*";
            this.machineName = ".";
            this.permissionAccess = PerformanceCounterPermissionAccess.Write;
        }

        public string CategoryName {
            get {
                return this.categoryName;
            }
            
            set {
                if (value == null)
                    throw new ArgumentNullException("value");

                this.categoryName = value;                    
            }
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
        
        public PerformanceCounterPermissionAccess PermissionAccess {            
            get {
                return this.permissionAccess;
            }
            
            set {
                this.permissionAccess = value;
            }
        }
              
        public override IPermission CreatePermission() {            
            if (Unrestricted) 
                return new PerformanceCounterPermission(PermissionState.Unrestricted);
            
            return new PerformanceCounterPermission(this.PermissionAccess, this.MachineName, this.CategoryName);

        }
    }    
}   

