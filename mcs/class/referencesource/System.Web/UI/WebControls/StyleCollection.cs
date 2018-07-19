//------------------------------------------------------------------------------
// <copyright file="StyleCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;


    /// <devdoc>
    ///     Provides a collection of Style objects
    /// </devdoc>
    public class StyleCollection : StateManagedCollection {
        private static readonly Type[] knownTypes = new Type[] { typeof(Style) };

        internal StyleCollection() {
        }


        /// <devdoc>
        /// Gets the Style at the specified index
        /// </devdoc>
        public Style this[int i] {
            get {
                return (Style)((IList)this)[i];
            }
            set {
                ((IList)this)[i] = value;
            }
        }


        /// <devdoc>
        /// Adds a Style to the collection
        /// </devdoc>
        public int Add(Style style) {
            return ((IList)this).Add(style);
        }


        public bool Contains(Style style) {
            return ((IList)this).Contains(style);
        }


        public void CopyTo(Style[] styleArray, int index) {
            base.CopyTo(styleArray, index);
        }


        public int IndexOf(Style style) {
            return ((IList)this).IndexOf(style);
        }


        /// <devdoc>
        /// Inserts a Treelevel at the specified index
        /// </devdoc>
        public void Insert(int index, Style style) {
            ((IList)this).Insert(index, style);
        }


        protected override object CreateKnownType(int index) {
            return new Style();
        }


        protected override Type[] GetKnownTypes() {
            return knownTypes;
        }


        /// <devdoc>
        /// Removes a Style from the collection.
        /// </devdoc>
        public void Remove(Style style) {
            ((IList)this).Remove(style);
        }


        /// <devdoc>
        /// Removes a Style from the collection at a given index.
        /// </devdoc>
        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }


        protected override void SetDirtyObject(object o) {
            if (o is Style) {
                ((Style)o).SetDirty();
            }
        }
    }
}

