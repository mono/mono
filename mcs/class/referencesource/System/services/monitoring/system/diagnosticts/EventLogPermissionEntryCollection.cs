//----------------------------------------------------
// <copyright file="EventLogPermissionEntryCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Security.Permissions;
    using System.Collections;
    
    [
    Serializable()
    ]
    public class EventLogPermissionEntryCollection : CollectionBase {
        EventLogPermission owner;
        
        ///<internalonly/>           
        internal EventLogPermissionEntryCollection(EventLogPermission owner, ResourcePermissionBaseEntry[] entries) {
            this.owner = owner;
            for (int index = 0; index < entries.Length; ++index)
                this.InnerList.Add(new EventLogPermissionEntry(entries[index]));
        }                                                                                                            
                                                                                                                              
        public EventLogPermissionEntry this[int index] {
            get {
                return (EventLogPermissionEntry)List[index];
            }
            set {
                List[index] = value;
            }            
        }
        
        public int Add(EventLogPermissionEntry value) {   
            return List.Add(value);
        }
        
        public void AddRange(EventLogPermissionEntry[] value) {            
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; ((i) < (value.Length)); i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
    
        public void AddRange(EventLogPermissionEntryCollection value) {            
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            int currentCount = value.Count;
            for (int i = 0; i < currentCount; i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }         
    
        public bool Contains(EventLogPermissionEntry value) {            
            return List.Contains(value);
        }
    
        public void CopyTo(EventLogPermissionEntry[] array, int index) {            
            List.CopyTo(array, index);
        }
    
        public int IndexOf(EventLogPermissionEntry value) {            
            return List.IndexOf(value);
        }
        
        public void Insert(int index, EventLogPermissionEntry value) {            
            List.Insert(index, value);
        }
                
        public void Remove(EventLogPermissionEntry value) {
            List.Remove(value);                     
        }
        
        ///<internalonly/>                          
        protected override void OnClear() {   
            this.owner.Clear();         
        }
        
        ///<internalonly/>                          
        protected override void OnInsert(int index, object value) {        
            this.owner.AddPermissionAccess((EventLogPermissionEntry)value);
        }
        
        ///<internalonly/>                          
        protected override void OnRemove(int index, object value) {
            this.owner.RemovePermissionAccess((EventLogPermissionEntry)value);
        }
                 
        ///<internalonly/>                          
        protected override void OnSet(int index, object oldValue, object newValue) {     
            this.owner.RemovePermissionAccess((EventLogPermissionEntry)oldValue);
            this.owner.AddPermissionAccess((EventLogPermissionEntry)newValue);       
        } 
    }
}

