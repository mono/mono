//---------------------------------------------------------------------
// <copyright file="EdmToObjectNamespaceMap.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Design.Common;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// The class to hold the map entries for the mapping between Edm Namespace and the Object Namespace
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    public class EdmToObjectNamespaceMap
    {
        private Dictionary<string, string> _map = new Dictionary<string, string>();

        /// <summary>
        /// this is just to keep this class from being creatable outside of this assembly
        /// </summary>
        internal EdmToObjectNamespaceMap()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public void Add(string edmNamespace, string objectNamespace)
        {
            EDesignUtil.CheckStringArgument(edmNamespace, "edmNamespace");
            EDesignUtil.CheckArgumentNull(objectNamespace, "objectNamespace");

            _map.Add(edmNamespace, objectNamespace);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public bool Contains(string edmNamespace)
        {
            return _map.ContainsKey(edmNamespace);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        public ICollection<string> EdmNamespaces
        {
            get { return _map.Keys; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public bool Remove(string edmNamespace)
        {
            return _map.Remove(edmNamespace);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public bool TryGetObjectNamespace(string edmNamespace, out string objectNamespace)
        {
            return _map.TryGetValue(edmNamespace, out objectNamespace);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public string this[string edmNamespace]
        {
            get
            {
                return _map[edmNamespace];
            }
            set
            {
                _map[edmNamespace] = value;
            }
        }

        public void Clear()
        {
            _map.Clear();
        }

        public int Count
        {
            get { return _map.Count; }
        }

        internal Dictionary<string, string> AsDictionary()
        {
            return _map;
        }
    }
}
