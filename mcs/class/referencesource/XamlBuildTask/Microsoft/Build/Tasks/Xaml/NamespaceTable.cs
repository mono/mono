using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xaml;

namespace Microsoft.Build.Tasks.Xaml
{
    internal class NamespaceTable : IXamlNamespaceResolver
    {
        Dictionary<string, NamespaceDeclaration> tempNamespaceList = new Dictionary<string, NamespaceDeclaration>();
        Stack<Dictionary<string, NamespaceDeclaration>> namespaceStack = new Stack<Dictionary<string, NamespaceDeclaration>>();
        string localAssemblyName;

        public NamespaceTable(string localAssemblyName)
        {
            this.localAssemblyName = localAssemblyName;
        }

        public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
        {
            List<NamespaceDeclaration> list = new List<NamespaceDeclaration>();
            HashSet<string> prefixSet = new HashSet<string>();
            if (tempNamespaceList != null && tempNamespaceList.Count > 0)
            {
                foreach (NamespaceDeclaration ns in tempNamespaceList.Values)
                {
                    if (!prefixSet.Contains(ns.Prefix))
                    {
                        prefixSet.Add(ns.Prefix);
                        list.Add(ns);
                    }
                }
            }
            foreach (Dictionary<string, NamespaceDeclaration> currentNamespaces in this.namespaceStack)
            {
                foreach (NamespaceDeclaration ns in currentNamespaces.Values)
                {
                    if (!prefixSet.Contains(ns.Prefix))
                    {
                        prefixSet.Add(ns.Prefix);
                        list.Add(ns);
                    }
                }
            }
            return list;
        }

        public string GetNamespace(string prefix)
        {
            NamespaceDeclaration @namespace = null;
            foreach (Dictionary<string, NamespaceDeclaration> currentNamespaces in this.namespaceStack)
            {
                if (null != currentNamespaces && currentNamespaces.TryGetValue(prefix, out @namespace))
                {
                    return @namespace.Namespace;
                }
            }

            if (tempNamespaceList != null && tempNamespaceList.TryGetValue(prefix, out @namespace))
            {
                return @namespace.Namespace;
            }

            return @namespace.Namespace;
        }

        public void ManageNamespace(XamlReader reader)
        {
            switch (reader.NodeType)
            {
                case XamlNodeType.NamespaceDeclaration:
                    tempNamespaceList.Add(reader.Namespace.Prefix,
                        new NamespaceDeclaration(
                        XamlBuildTaskServices.UpdateClrNamespaceUriWithLocalAssembly(reader.Namespace.Namespace, this.localAssemblyName),
                        reader.Namespace.Prefix));
                    break;
                case XamlNodeType.StartObject:
                case XamlNodeType.StartMember:
                case XamlNodeType.GetObject:
                    if (tempNamespaceList != null)
                    {
                        namespaceStack.Push(tempNamespaceList);
                        tempNamespaceList = new Dictionary<string, NamespaceDeclaration>();
                    }
                    break;
                case XamlNodeType.EndMember:
                case XamlNodeType.EndObject:
                    namespaceStack.Pop();
                    break;
                default:
                    break;
            }
        }
    }
}
