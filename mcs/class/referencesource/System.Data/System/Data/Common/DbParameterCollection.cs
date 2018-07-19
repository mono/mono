//------------------------------------------------------------------------------
// <copyright file="DbParameterCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.ComponentModel;
    using System.Collections;
    using System.Data;
    
    public abstract class DbParameterCollection : MarshalByRefObject, IDataParameterCollection {
    
        protected DbParameterCollection() : base() {
        }
    
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        abstract public int Count {
            get;
        }
        
        [
        Browsable(false),
        EditorBrowsableAttribute(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        virtual public bool IsFixedSize {
            get { return false; }
        }
        
        [
        Browsable(false),
        EditorBrowsableAttribute(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        virtual public bool IsReadOnly {
            get { return false; }
        }
        
        [
        Browsable(false),
        EditorBrowsableAttribute(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        virtual public bool IsSynchronized {
            get { return false; }
        }
        
        [
        Browsable(false),
        EditorBrowsableAttribute(EditorBrowsableState.Never),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        abstract public object SyncRoot {
            get;
        }
        
        object IList.this[int index] {
            get {
                return GetParameter(index);
            }
            set {
                SetParameter(index, (DbParameter)value);
            }
        }
        
        object IDataParameterCollection.this[string parameterName] {
            get {
                return GetParameter(parameterName);
            }
            set {
                SetParameter(parameterName, (DbParameter)value);
            }
        }
        
        public DbParameter this[int index] {
            get {
                return GetParameter(index);
            }
            set {
                SetParameter(index, value);
            }
        }
        
        public DbParameter this[string parameterName] {
            get {
                return GetParameter(parameterName) as DbParameter;
            }
            set {
                SetParameter(parameterName, value);
            }
        }
                
        abstract public int Add(object value);
        
        // 

        abstract public void AddRange(System.Array values); 

        // 
        
        abstract public bool Contains(object value);
        
        abstract public bool Contains(string value); // WebData 97349
        
        // 
        
        abstract public void CopyTo(System.Array array, int index);

        // 
        
        abstract public void Clear();
        
        [ 
        EditorBrowsableAttribute(EditorBrowsableState.Never) 
        ]
        abstract public IEnumerator GetEnumerator();
        
        abstract protected DbParameter GetParameter(int index);
        
        abstract protected DbParameter GetParameter(string parameterName);
        
        abstract public int IndexOf(object value);
        
        // 
            
        abstract public int IndexOf(string parameterName);
        
        abstract public void Insert(int index, object value);
        
        abstract public void Remove(object value);
        
        // 
        
        // 
        
        abstract public void RemoveAt(int index);
    
        abstract public void RemoveAt(string parameterName);
        
        abstract protected void SetParameter(int index, DbParameter value);
        
        abstract protected void SetParameter(string parameterName, DbParameter value);
         
    }

}
