//------------------------------------------------------------------------------
// <copyright file="VirtualDirectoryMappingCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Web.Util;
    using System.Security.Permissions;

    //
    // Collection of VirtualDirectoryMappings.
    // Follows the standard pattern for collections derived from NameObjectCollectionBase.
    //
    [Serializable()]
    public sealed class VirtualDirectoryMappingCollection : NameObjectCollectionBase {
        public VirtualDirectoryMappingCollection() : base(StringComparer.OrdinalIgnoreCase) {
        }

        public ICollection AllKeys {
            get {
                return BaseGetAllKeys();
            }
        }                                       

        public VirtualDirectoryMapping this[string virtualDirectory] {
            get {
                virtualDirectory = ValidateVirtualDirectoryParameter(virtualDirectory);

                return Get(virtualDirectory);
            }
        }

        public VirtualDirectoryMapping this[int index] {
            get {
                return Get(index);
            }
        }

        public void Add(string virtualDirectory, VirtualDirectoryMapping mapping) {
            virtualDirectory = ValidateVirtualDirectoryParameter(virtualDirectory);

            if (mapping == null) {
                throw new ArgumentNullException("mapping");
            }

            if (Get(virtualDirectory) != null) {
                throw ExceptionUtil.ParameterInvalid("virtualDirectory");
            }

            mapping.SetVirtualDirectory(VirtualPath.CreateAbsoluteAllowNull(virtualDirectory));
            BaseAdd(virtualDirectory, mapping);
        }

        public void Clear() {
            BaseClear();
        }

        public void CopyTo(VirtualDirectoryMapping[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }

            int c = Count;
            if (array.Length < c + index) {
                throw new ArgumentOutOfRangeException("index");
            }

            for (int i = 0, j = index; i < c; i++, j++) {
                array[j] = Get(i);
            }
        }

        public VirtualDirectoryMapping Get(int index) {
            return (VirtualDirectoryMapping) BaseGet(index);
        }

        public VirtualDirectoryMapping Get(string virtualDirectory) {
            virtualDirectory = ValidateVirtualDirectoryParameter(virtualDirectory);

            return (VirtualDirectoryMapping) BaseGet(virtualDirectory);
        }

        public string GetKey(int index) {
            return BaseGetKey(index);
        }

        public void Remove(string virtualDirectory) {
            virtualDirectory = ValidateVirtualDirectoryParameter(virtualDirectory);

            BaseRemove(virtualDirectory);
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        internal VirtualDirectoryMappingCollection Clone() {
            VirtualDirectoryMappingCollection col = new VirtualDirectoryMappingCollection();
            for (int i = 0; i < this.Count; i++) {
                VirtualDirectoryMapping mapping = this[i];
                col.Add(mapping.VirtualDirectory, mapping.Clone());
            }

            return col;
        }

        private static string ValidateVirtualDirectoryParameter(string virtualDirectory) {
            // Create a VirtualPath object to validate the path
            VirtualPath v = VirtualPath.CreateAbsoluteAllowNull(virtualDirectory);
            return VirtualPath.GetVirtualPathString(v);
        }
    }
}
