// ------------------------------------------------------------------------------
// <copyright file="CodeCatchClauseCollection.cs" company="Microsoft">
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
    
    
    /// <devdoc>
    ///     <para>
    ///       A collection that stores <see cref='System.CodeDom.CodeCatchClause'/> objects.
    ///    </para>
    /// </devdoc>
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeCatchClauseCollection : CollectionBase {
        
        /// <devdoc>
        ///     <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeCatchClauseCollection'/>.
        ///    </para>
        /// </devdoc>
        public CodeCatchClauseCollection() {
        }
        
        /// <devdoc>
        ///     <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeCatchClauseCollection'/> based on another <see cref='System.CodeDom.CodeCatchClauseCollection'/>.
        ///    </para>
        /// </devdoc>
        public CodeCatchClauseCollection(CodeCatchClauseCollection value) {
            this.AddRange(value);
        }
        
        /// <devdoc>
        ///     <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.CodeCatchClauseCollection'/> containing any array of <see cref='System.CodeDom.CodeCatchClause'/> objects.
        ///    </para>
        /// </devdoc>
        public CodeCatchClauseCollection(CodeCatchClause[] value) {
            this.AddRange(value);
        }
        
        /// <devdoc>
        /// <para>Represents the entry at the specified index of the <see cref='System.CodeDom.CodeCatchClause'/>.</para>
        /// </devdoc>
        public CodeCatchClause this[int index] {
            get {
                return ((CodeCatchClause)(List[index]));
            }
            set {
                List[index] = value;
            }
        }
        
        /// <devdoc>
        ///    <para>Adds a <see cref='System.CodeDom.CodeCatchClause'/> with the specified value to the 
        ///    <see cref='System.CodeDom.CodeCatchClauseCollection'/> .</para>
        /// </devdoc>
        public int Add(CodeCatchClause value) {
            return List.Add(value);
        }
        
        /// <devdoc>
        /// <para>Copies the elements of an array to the end of the <see cref='System.CodeDom.CodeCatchClauseCollection'/>.</para>
        /// </devdoc>
        public void AddRange(CodeCatchClause[] value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; ((i) < (value.Length)); i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
        
        /// <devdoc>
        ///     <para>
        ///       Adds the contents of another <see cref='System.CodeDom.CodeCatchClauseCollection'/> to the end of the collection.
        ///    </para>
        /// </devdoc>
        public void AddRange(CodeCatchClauseCollection value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            int currentCount = value.Count;
            for (int i = 0; i < currentCount; i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
        
        /// <devdoc>
        /// <para>Gets a value indicating whether the 
        ///    <see cref='System.CodeDom.CodeCatchClauseCollection'/> contains the specified <see cref='System.CodeDom.CodeCatchClause'/>.</para>
        /// </devdoc>
        public bool Contains(CodeCatchClause value) {
            return List.Contains(value);
        }
        
        /// <devdoc>
        /// <para>Copies the <see cref='System.CodeDom.CodeCatchClauseCollection'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
        ///    specified index.</para>
        /// </devdoc>
        public void CopyTo(CodeCatchClause[] array, int index) {
            List.CopyTo(array, index);
        }
        
        /// <devdoc>
        ///    <para>Returns the index of a <see cref='System.CodeDom.CodeCatchClause'/> in 
        ///       the <see cref='System.CodeDom.CodeCatchClauseCollection'/> .</para>
        /// </devdoc>
        public int IndexOf(CodeCatchClause value) {
            return List.IndexOf(value);
        }
        
        /// <devdoc>
        /// <para>Inserts a <see cref='System.CodeDom.CodeCatchClause'/> into the <see cref='System.CodeDom.CodeCatchClauseCollection'/> at the specified index.</para>
        /// </devdoc>
        public void Insert(int index, CodeCatchClause value) {
            List.Insert(index, value);
        }
        
        /// <devdoc>
        ///    <para> Removes a specific <see cref='System.CodeDom.CodeCatchClause'/> from the 
        ///    <see cref='System.CodeDom.CodeCatchClauseCollection'/> .</para>
        /// </devdoc>
        public void Remove(CodeCatchClause value) {
            List.Remove(value);
        }
    }
}
