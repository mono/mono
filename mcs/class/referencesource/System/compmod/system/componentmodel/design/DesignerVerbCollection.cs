//------------------------------------------------------------------------------
// <copyright file="DesignerVerbCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design {
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Security.Permissions;
    
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public class DesignerVerbCollection : CollectionBase {

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DesignerVerbCollection() {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DesignerVerbCollection(DesignerVerb[] value) {
            AddRange(value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DesignerVerb this[int index] {
            get {
                return (DesignerVerb)(List[index]);
            }
            set {
                List[index] = value;
            }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(DesignerVerb value) {
            return List.Add(value);
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void AddRange(DesignerVerb[] value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; ((i) < (value.Length)); i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void AddRange(DesignerVerbCollection value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            int currentCount = value.Count;
            for (int i = 0; i < currentCount; i = ((i) + (1))) {
                this.Add(value[i]);
            }
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, DesignerVerb value) {
            List.Insert(index, value);
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(DesignerVerb value) {
            return List.IndexOf(value);
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(DesignerVerb value) {
            return List.Contains(value);
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(DesignerVerb value) {
            List.Remove(value);
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(DesignerVerb[] array, int index) {
            List.CopyTo(array, index);
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnSet(int index, object oldValue, object newValue) {
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnInsert(int index, object value) {
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnClear() {
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnRemove(int index, object value) {
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnValidate(object value) {
            Debug.Assert(value != null, "Don't add null verbs!");
        }
        
    }
}

