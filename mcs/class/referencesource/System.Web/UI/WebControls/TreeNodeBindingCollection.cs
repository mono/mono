//------------------------------------------------------------------------------
// <copyright file="TreeNodeBindingCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;


    /// <devdoc>
    ///     Provides a collection of TreeNodeBinding objects
    /// </devdoc>
    public sealed class TreeNodeBindingCollection : StateManagedCollection {
        private static readonly Type[] knownTypes = new Type[] { typeof(TreeNodeBinding) };

        private TreeNodeBinding _defaultBinding;

        internal TreeNodeBindingCollection() {
        }

        /// <devdoc>
        /// Gets the TreeNodeBinding at the specified index
        /// </devdoc>
        public TreeNodeBinding this[int i] {
            get {
                return (TreeNodeBinding)((IList)this)[i];
            }
            set {
                ((IList)this)[i] = value;
            }
        }

        /// <devdoc>
        /// Adds a TreeNodeBinding to the collection
        /// </devdoc>
        public int Add(TreeNodeBinding binding) {
            return ((IList)this).Add(binding);
        }

        public bool Contains(TreeNodeBinding binding) {
            return ((IList)this).Contains(binding);
        }

        public void CopyTo(TreeNodeBinding[] bindingArray, int index) {
            base.CopyTo(bindingArray, index);
        }

        protected override object CreateKnownType(int index) {
            return new TreeNodeBinding();
        }

        private void FindDefaultBinding() {
            _defaultBinding = null;
            // Look for another binding that would be a good default
            foreach (TreeNodeBinding binding in this) {
                if (binding.Depth == -1 && binding.DataMember.Length == 0) {
                    _defaultBinding = binding;
                    break;
                }
            }
        }

        /// <devdoc>
        ///     Gets a TreeNodeBinding data binding definition for the specified depth or datamember
        /// </devdoc>
        internal TreeNodeBinding GetBinding(string dataMember, int depth) {
            TreeNodeBinding bestMatch = null;
            int match = 0;
            if ((dataMember != null) && (dataMember.Length == 0)) {
                dataMember = null;
            }

            foreach (TreeNodeBinding binding in this) {
                if ((binding.Depth == depth)) {
                    if (String.Equals(binding.DataMember, dataMember, StringComparison.CurrentCultureIgnoreCase)) {
                        return binding;
                    }
                    else if ((match < 1) && (binding.DataMember.Length == 0)) {
                        bestMatch = binding;
                        match = 1;
                    }
                }
                else if (String.Equals(binding.DataMember, dataMember, StringComparison.CurrentCultureIgnoreCase) &&
                    (match < 2) &&
                    (binding.Depth == -1)) {

                    bestMatch = binding;
                    match = 2;
                }
            }

            if (bestMatch == null) {
                // Check that the default binding is still suitable (VSWhidbey 358817)
                if (_defaultBinding != null) {
                    if (_defaultBinding.Depth != -1 || _defaultBinding.DataMember.Length != 0) {
                        // Look for another binding that would be a good default
                        FindDefaultBinding();
                    }
                    bestMatch = _defaultBinding;
                }
            }

            return bestMatch;
        }

        protected override Type[] GetKnownTypes() {
            return knownTypes;
        }

        public int IndexOf(TreeNodeBinding binding) {
            return ((IList)this).IndexOf(binding);
        }

        public void Insert(int index, TreeNodeBinding binding) {
            ((IList)this).Insert(index, binding);
        }

        protected override void OnClear() {
            base.OnClear();
            _defaultBinding = null;
        }

        protected override void OnRemoveComplete(int index, object value) {
            if (value == _defaultBinding) {
                FindDefaultBinding();
            }
        }

        protected override void OnValidate(object value) {
            base.OnValidate(value);
            TreeNodeBinding binding = value as TreeNodeBinding;
            if ((binding != null) && (binding.DataMember.Length == 0) && (binding.Depth == -1)) {
                _defaultBinding = binding;
            }
        }

        /// <devdoc>
        /// Removes a TreeNodeBinding from the collection.
        /// </devdoc>
        public void Remove(TreeNodeBinding binding) {
            ((IList)this).Remove(binding);
        }

        /// <devdoc>
        /// Removes a TreeNodeBinding from the collection at a given index.
        /// </devdoc>
        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o) {
            if (o is TreeNodeBinding) {
                ((TreeNodeBinding)o).SetDirty();
            }
        }
    }
}

