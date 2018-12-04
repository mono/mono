// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Xaml;
using MS.Internal.Xaml.Parser;

namespace System.Xaml.Schema
{
    [DebuggerDisplay("{ToString()}")]
    public class XamlTypeName
    {
        List<XamlTypeName> _typeArguments;

        public string Name { get; set; }
        public string Namespace { get; set; }

        public XamlTypeName()
        {
        }

        public XamlTypeName(string xamlNamespace, string name)
            : this(xamlNamespace, name, null)
        {
        }

        public XamlTypeName(string xamlNamespace, string name, IEnumerable<XamlTypeName> typeArguments)
        {
            Name = name;
            Namespace = xamlNamespace;
            if (typeArguments != null)
            {
                List<XamlTypeName> typeArgList = new List<XamlTypeName>(typeArguments);
                _typeArguments = typeArgList;
            }
        }

        public XamlTypeName(XamlType xamlType)
        {
            if (xamlType == null)
            {
                throw new ArgumentNullException("xamlType");
            }
            this.Name = xamlType.Name;
            this.Namespace = xamlType.GetXamlNamespaces()[0];
            if (xamlType.TypeArguments != null)
            {
                foreach (XamlType argumentType in xamlType.TypeArguments)
                {
                    TypeArguments.Add(new XamlTypeName(argumentType));
                }
            }
        }

        public IList<XamlTypeName> TypeArguments
        {
            get
            {
                if (_typeArguments == null)
                {
                    _typeArguments = new List<XamlTypeName>();
                }
                return _typeArguments;
            }
        }

        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(INamespacePrefixLookup prefixLookup)
        {
            if (prefixLookup == null)
            {
                return ConvertToStringInternal(null);
            }
            else
            {
                return ConvertToStringInternal(prefixLookup.LookupPrefix);
            }
        }

        public static string ToString(IList<XamlTypeName> typeNameList, INamespacePrefixLookup prefixLookup)
        {
            if (typeNameList == null)
            {
                throw new ArgumentNullException("typeNameList");
            }
            if (prefixLookup == null)
            {
                throw new ArgumentNullException("prefixLookup");
            }
            return ConvertListToStringInternal(typeNameList, prefixLookup.LookupPrefix);
        }

        public static XamlTypeName Parse(string typeName, IXamlNamespaceResolver namespaceResolver)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (namespaceResolver == null)
            {
                throw new ArgumentNullException("namespaceResolver");
            }

            string error;
            XamlTypeName result = ParseInternal(typeName, namespaceResolver.GetNamespace, out error);
            if (result == null)
            {
                throw new FormatException(error);
            }
            return result;
        }

        public static IList<XamlTypeName> ParseList(string typeNameList, IXamlNamespaceResolver namespaceResolver)
        {
            if (typeNameList == null)
            {
                throw new ArgumentNullException("typeNameList");
            }
            if (namespaceResolver == null)
            {
                throw new ArgumentNullException("namespaceResolver");
            }

            string error;
            IList<XamlTypeName> result = ParseListInternal(typeNameList, namespaceResolver.GetNamespace, out error);
            if (result == null)
            {
                throw new FormatException(error);
            }
            return result;
        }

        public static bool TryParse(string typeName, IXamlNamespaceResolver namespaceResolver,
            out XamlTypeName result)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (namespaceResolver == null)
            {
                throw new ArgumentNullException("namespaceResolver");
            }

            string error;
            result = ParseInternal(typeName, namespaceResolver.GetNamespace, out error);
            return (result != null);
        }

        public static bool TryParseList(string typeNameList, IXamlNamespaceResolver namespaceResolver,
            out IList<XamlTypeName> result)
        {
            if (typeNameList == null)
            {
                throw new ArgumentNullException("typeNameList");
            }
            if (namespaceResolver == null)
            {
                throw new ArgumentNullException("namespaceResolver");
            }

            string error;
            result = ParseListInternal(typeNameList, namespaceResolver.GetNamespace, out error);
            return (result != null);
        }

        internal bool HasTypeArgs
        {
            get
            {
                return _typeArguments != null && _typeArguments.Count > 0;
            }
        }

        internal static string ConvertListToStringInternal(IList<XamlTypeName> typeNameList,
            Func<string, string> prefixGenerator)
        {
            StringBuilder result = new StringBuilder();
            ConvertListToStringInternal(result, typeNameList, prefixGenerator);
            return result.ToString();
        }

        internal static void ConvertListToStringInternal(StringBuilder result, IList<XamlTypeName> typeNameList,
            Func<string, string> prefixGenerator)
        {
            bool first = true;
            foreach (XamlTypeName typeName in typeNameList)
            {
                if (!first)
                {
                    result.Append(", ");
                }
                else
                {
                    first = false;
                }
                typeName.ConvertToStringInternal(result, prefixGenerator);
            }
        }

        internal static XamlTypeName ParseInternal(string typeName, Func<string, string> prefixResolver, out string error)
        {
            XamlTypeName xamlTypeName = GenericTypeNameParser.ParseIfTrivalName(typeName, prefixResolver, out error);
            if (xamlTypeName != null)
            {
                return xamlTypeName;
            }

            GenericTypeNameParser nameParser = new GenericTypeNameParser(prefixResolver);
            xamlTypeName = nameParser.ParseName(typeName, out error);
            return xamlTypeName;
        }

        internal static IList<XamlTypeName> ParseListInternal(string typeNameList, 
            Func<string, string> prefixResolver, out string error)
        {
            GenericTypeNameParser nameParser = new GenericTypeNameParser(prefixResolver);
            IList<XamlTypeName> xamlTypeName = nameParser.ParseList(typeNameList, out error);
            return xamlTypeName;
        }

        internal string ConvertToStringInternal(Func<string, string> prefixGenerator)
        {
            StringBuilder result = new StringBuilder();
            ConvertToStringInternal(result, prefixGenerator);
            return result.ToString();
        }

        internal void ConvertToStringInternal(StringBuilder result, Func<string, string> prefixGenerator)
        {
            if (Namespace == null)
            {
                throw new InvalidOperationException(SR.Get(SRID.XamlTypeNameNamespaceIsNull));
            }
            if (string.IsNullOrEmpty(Name))
            {
                throw new InvalidOperationException(SR.Get(SRID.XamlTypeNameNameIsNullOrEmpty));
            }
            if (prefixGenerator == null)
            {
                result.Append("{");
                result.Append(Namespace);
                result.Append("}");
            }
            else
            {
                string prefix = prefixGenerator.Invoke(Namespace);
                if (prefix == null)
                {
                    throw new InvalidOperationException(SR.Get(SRID.XamlTypeNameCannotGetPrefix, Namespace));
                }
                if (prefix != string.Empty)
                {
                    result.Append(prefix);
                    result.Append(":");
                }
            }
            if (HasTypeArgs)
            {
                // The subscript goes after the type args
                string subscript;
                string name = GenericTypeNameScanner.StripSubscript(Name, out subscript);
                result.Append(name);

                result.Append("(");
                ConvertListToStringInternal(result, TypeArguments, prefixGenerator);
                result.Append(")");

                result.Append(subscript);
            }
            else
            {
                result.Append(Name);
            }
        }
    }
}
