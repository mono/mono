// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;

    internal class XamlNamespaceHelper 
    {
        internal const string ClrNamespacePrefix = "clr-namespace:";
        internal const string ClrNamespaceAssemblyField = ";assembly=";

        internal static bool TryParseClrNsUri(string xamlNamespace, out string clrNamespace, out string assembly)
        {
            clrNamespace = null;
            assembly = null;
            if (!xamlNamespace.StartsWith(ClrNamespacePrefix, StringComparison.Ordinal))
            {
                return false;
            }
            int clrNsIndex = ClrNamespacePrefix.Length;
            int assemblyIndex = xamlNamespace.IndexOf(ClrNamespaceAssemblyField, StringComparison.Ordinal);
            if (assemblyIndex < clrNsIndex)
            {
                clrNamespace = xamlNamespace.Substring(clrNsIndex);
                return true;
            }
            clrNamespace = xamlNamespace.Substring(clrNsIndex, assemblyIndex - clrNsIndex);
            assemblyIndex += ClrNamespaceAssemblyField.Length;
            assembly = xamlNamespace.Substring(assemblyIndex);
            return true;
        }
    }
}
