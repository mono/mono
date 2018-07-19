//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System.Collections.Generic;
    using System.Xaml;

    class NamespaceTable : IXamlNamespaceResolver
    {
        List<NamespaceDeclaration> tempNamespaceList;
        Stack<List<NamespaceDeclaration>> namespaceStack;
        Dictionary<string, NamespaceDeclaration> namespacesCache;

        public NamespaceTable()
        {
            this.tempNamespaceList = new List<NamespaceDeclaration>();
            this.namespaceStack = new Stack<List<NamespaceDeclaration>>();
        }

        public string GetNamespace(string prefix)
        {
            NamespaceDeclaration result;
            if (this.namespacesCache == null)
            {
                ConstructNamespaceCache();
            }

            if (this.namespacesCache.TryGetValue(prefix, out result))
            {
                return result.Namespace;
            }
            else
            {
                return null;
            }
        }

        public void ManageNamespace(XamlReader reader)
        {
            switch (reader.NodeType)
            {
                case XamlNodeType.NamespaceDeclaration:
                    AddNamespace(reader.Namespace);
                    break;
                case XamlNodeType.StartObject:
                case XamlNodeType.StartMember:
                case XamlNodeType.GetObject:
                    EnterScope();
                    break;
                case XamlNodeType.EndMember:
                case XamlNodeType.EndObject:
                    ExitScope();
                    break;
            }
        }

        public void AddNamespace(NamespaceDeclaration xamlNamespace)
        {
            this.tempNamespaceList.Add(xamlNamespace);
            this.namespacesCache = null;
        }

        public void EnterScope()
        {
            if (this.tempNamespaceList != null)
            {
                this.namespaceStack.Push(this.tempNamespaceList);
                this.tempNamespaceList = new List<NamespaceDeclaration>();
            }
        }

        public void ExitScope()
        {
            List<NamespaceDeclaration> namespaceList = this.namespaceStack.Pop();
            if (namespaceList.Count != 0)
            {
                this.namespacesCache = null;
            }
        }

        public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
        {
            if (this.namespacesCache == null)
            {
                ConstructNamespaceCache();
            }

            return this.namespacesCache.Values;
        }

        void ConstructNamespaceCache()
        {
            Dictionary<string, NamespaceDeclaration> localNamespaces = new Dictionary<string, NamespaceDeclaration>();
            if (this.tempNamespaceList != null && this.tempNamespaceList.Count > 0)
            {
                foreach (NamespaceDeclaration tempNamespace in tempNamespaceList)
                {
                    if (!localNamespaces.ContainsKey(tempNamespace.Prefix))
                    {
                        localNamespaces.Add(tempNamespace.Prefix, tempNamespace);
                    }
                }
            }
            foreach (List<NamespaceDeclaration> currentNamespaces in this.namespaceStack)
            {
                foreach (NamespaceDeclaration currentNamespace in currentNamespaces)
                {
                    if (!localNamespaces.ContainsKey(currentNamespace.Prefix))
                    {
                        localNamespaces.Add(currentNamespace.Prefix, currentNamespace);
                    }
                }
            }
            this.namespacesCache = localNamespaces;
        }
    }
}
