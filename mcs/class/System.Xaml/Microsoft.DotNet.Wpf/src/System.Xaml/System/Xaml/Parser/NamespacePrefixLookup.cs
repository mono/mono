// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xaml;

namespace MS.Internal.Xaml.Parser
{
    internal class NamespacePrefixLookup : INamespacePrefixLookup
    {
        private readonly List<NamespaceDeclaration> _newNamespaces;
        private readonly Func<string, string> _nsResolver;
        public NamespacePrefixLookup(out IEnumerable<NamespaceDeclaration> newNamespaces, Func<string, string> nsResolver)
        {
            newNamespaces = _newNamespaces = new List<NamespaceDeclaration>();
            _nsResolver = nsResolver;
        }

        #region INamespacePrefixLookup Members

        private int n = 0;
        public string LookupPrefix(string ns)
        {
            // we really shouldn't generate extraneous new namespaces
            string newPrefix;
            do {
                newPrefix = "prefix" + n++;
            } while (_nsResolver(newPrefix) != null);
            _newNamespaces.Add(new NamespaceDeclaration(ns, newPrefix));
            return newPrefix;
        }

        #endregion
    }
}
