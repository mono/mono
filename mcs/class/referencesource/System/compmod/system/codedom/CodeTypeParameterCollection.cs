// ------------------------------------------------------------------------------
// <copyright file="CodeTypeParameterCollection.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright> 
// ------------------------------------------------------------------------------
// 
namespace System.CodeDom {
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
        
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeTypeParameterCollection : CollectionBase {
        
        public CodeTypeParameterCollection() {
        }
        
        public CodeTypeParameterCollection(CodeTypeParameterCollection value) {
            this.AddRange(value);
        }
        
        public CodeTypeParameterCollection(CodeTypeParameter[] value) {
            this.AddRange(value);
        }
        
        public CodeTypeParameter this[int index] {
            get {
                return ((CodeTypeParameter)(List[index]));
            }
            set {
                List[index] = value;
            }
        }
        
        public int Add(CodeTypeParameter value) {
            return List.Add(value);
        }

        public void Add(string value) {
            Add(new CodeTypeParameter(value));
        }
        
        public void AddRange(CodeTypeParameter[] value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; ((i) < (value.Length)); i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
        
        public void AddRange(CodeTypeParameterCollection value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            int currentCount = value.Count;
            for (int i = 0; i < currentCount; i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
        
        public bool Contains(CodeTypeParameter value) {
            return List.Contains(value);
        }
        
        public void CopyTo(CodeTypeParameter[] array, int index) {
            List.CopyTo(array, index);
        }
        
        public int IndexOf(CodeTypeParameter value) {
            return List.IndexOf(value);
        }
        
        public void Insert(int index, CodeTypeParameter value) {
            List.Insert(index, value);
        }
        
        public void Remove(CodeTypeParameter value) {
            List.Remove(value);
        }
    }
}

