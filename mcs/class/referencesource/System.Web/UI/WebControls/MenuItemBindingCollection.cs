//------------------------------------------------------------------------------
// <copyright file="MenuItemBindingCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web;


    /// <devdoc>
    ///     Provides a collection of MenuItemBinding objects
    /// </devdoc>
    public sealed class MenuItemBindingCollection : StateManagedCollection {
        private static readonly Type[] knownTypes = new Type[] { typeof(MenuItemBinding) };

        private Menu _owner;
        private MenuItemBinding _defaultBinding;

        private MenuItemBindingCollection() {
        }

        internal MenuItemBindingCollection(Menu owner) {
            _owner = owner;
        }

        /// <devdoc>
        /// Gets the MenuItemBinding at the specified index
        /// </devdoc>
        public MenuItemBinding this[int i] {
            get {
                return (MenuItemBinding)((IList)this)[i];
            }
            set {
                ((IList)this)[i] = value;
            }
        }

        /// <devdoc>
        /// Adds a MenuItemBinding to the collection
        /// </devdoc>
        public int Add(MenuItemBinding binding) {
            return ((IList)this).Add(binding);
        }

        public bool Contains(MenuItemBinding binding) {
            return ((IList)this).Contains(binding);
        }

        public void CopyTo(MenuItemBinding[] array, int index) {
            ((IList)this).CopyTo(array, index);
        }

        protected override object CreateKnownType(int index) {
            return new MenuItemBinding();
        }

        private void FindDefaultBinding() {
            _defaultBinding = null;
            // Look for another binding that would be a good default
            foreach (MenuItemBinding binding in this) {
                if (binding.Depth == -1 && binding.DataMember.Length == 0) {
                    _defaultBinding = binding;
                    break;
                }
            }
        }

        /// <devdoc>
        ///     Gets a MenuItemBinding data binding definition for the specified depth or datamember
        /// </devdoc>
        internal MenuItemBinding GetBinding(string dataMember, int depth) {
            MenuItemBinding bestMatch = null;
            int match = 0;
            if ((dataMember != null) && (dataMember.Length == 0)) {
                dataMember = null;
            }

            foreach (MenuItemBinding binding in this) {
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

        public int IndexOf(MenuItemBinding value) {
            return ((IList)this).IndexOf(value);
        }

        public void Insert(int index, MenuItemBinding binding) {
            ((IList)this).Insert(index, binding);
        }

        protected override void OnClear() {
            _defaultBinding = null;
        }

        protected override void OnRemoveComplete(int index, object value) {
            if (value == _defaultBinding) {
                FindDefaultBinding();
            }
        }

        protected override void OnValidate(object value) {
            base.OnValidate(value);
            MenuItemBinding binding = value as MenuItemBinding;
            if ((binding != null) && (binding.DataMember.Length == 0) && (binding.Depth == -1)) {
                _defaultBinding = binding;
            }
        }

        /// <devdoc>
        /// Removes a MenuItemBinding from the collection.
        /// </devdoc>
        public void Remove(MenuItemBinding binding) {
            ((IList)this).Remove(binding);
        }

        /// <devdoc>
        /// Removes a MenuItemBinding from the collection at a given index.
        /// </devdoc>
        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o) {
            if (o is MenuItemBinding) {
                ((MenuItemBinding)o).SetDirty();
            }
        }
    }
}

