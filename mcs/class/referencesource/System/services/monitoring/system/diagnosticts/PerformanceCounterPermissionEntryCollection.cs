//----------------------------------------------------
// <copyright file="PerformanceCounterPermissionEntryCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Security.Permissions;
    using System.Collections;
    
    [
    Serializable()
    ]
    public class PerformanceCounterPermissionEntryCollection : CollectionBase {        
        PerformanceCounterPermission owner;
        
        ///<internalonly/>   
        internal PerformanceCounterPermissionEntryCollection(PerformanceCounterPermission owner, ResourcePermissionBaseEntry[] entries) {
            this.owner = owner;
            for (int index = 0; index < entries.Length; ++index)
                this.InnerList.Add(new PerformanceCounterPermissionEntry(entries[index]));
        }                                                                                                              
                                                                                                            
        public PerformanceCounterPermissionEntry this[int index] {
            get {
                return (PerformanceCounterPermissionEntry)List[index];
            }
            set {
                List[index] = value;
            }
            
        }
        
        public int Add(PerformanceCounterPermissionEntry value) {   
            return List.Add(value);
        }
        
        public void AddRange(PerformanceCounterPermissionEntry[] value) {            
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; ((i) < (value.Length)); i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
    
        public void AddRange(PerformanceCounterPermissionEntryCollection value) {            
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            int currentCount = value.Count;
            for (int i = 0; i < currentCount; i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }         
    
        public bool Contains(PerformanceCounterPermissionEntry value) {            
            return List.Contains(value);
        }
    
        public void CopyTo(PerformanceCounterPermissionEntry[] array, int index) {            
            List.CopyTo(array, index);
        }
    
        public int IndexOf(PerformanceCounterPermissionEntry value) {            
            return List.IndexOf(value);
        }
        
        public void Insert(int index, PerformanceCounterPermissionEntry value) {            
            List.Insert(index, value);
        }
                
        public void Remove(PerformanceCounterPermissionEntry value) {
            List.Remove(value);                     
        }
        
        ///<internalonly/>                          
        protected override void OnClear() {   
            this.owner.Clear();         
        }
        
        ///<internalonly/>                          
        protected override void OnInsert(int index, object value) {        
            this.owner.AddPermissionAccess((PerformanceCounterPermissionEntry)value);
        }
        
        ///<internalonly/>                          
        protected override void OnRemove(int index, object value) {
            this.owner.RemovePermissionAccess((PerformanceCounterPermissionEntry)value);
        }
                 
        ///<internalonly/>                          
        protected override void OnSet(int index, object oldValue, object newValue) {     
            this.owner.RemovePermissionAccess((PerformanceCounterPermissionEntry)oldValue);
            this.owner.AddPermissionAccess((PerformanceCounterPermissionEntry)newValue);       
        } 
    }
}

