// ------------------------------------------------------------------------------
// <copyright file="CodeDirectiveCollection.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
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
        Serializable()
    ]
    public class CodeDirectiveCollection : CollectionBase {
        
        public CodeDirectiveCollection() {
        }
        
        public CodeDirectiveCollection(CodeDirectiveCollection value) {
            this.AddRange(value);
        }
        
        public CodeDirectiveCollection(CodeDirective[] value) {
            this.AddRange(value);
        }
        
        public CodeDirective this[int index] {
            get {
                return ((CodeDirective)(List[index]));
            }
            set {
                List[index] = value;
            }
        }
        
        public int Add(CodeDirective value) {
            return List.Add(value);
        }
        
        public void AddRange(CodeDirective[] value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; ((i) < (value.Length)); i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
        
        public void AddRange(CodeDirectiveCollection value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            int currentCount = value.Count;
            for (int i = 0; i < currentCount; i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
        
        public bool Contains(CodeDirective value) {
            return List.Contains(value);
        }
        
        public void CopyTo(CodeDirective[] array, int index) {
            List.CopyTo(array, index);
        }
        
        public int IndexOf(CodeDirective value) {
            return List.IndexOf(value);
        }
        
        public void Insert(int index, CodeDirective value) {
            List.Insert(index, value);
        }
        
        public void Remove(CodeDirective value) {
            List.Remove(value);
        }
    }
}
