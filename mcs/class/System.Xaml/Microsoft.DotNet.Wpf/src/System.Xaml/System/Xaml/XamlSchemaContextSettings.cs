// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace System.Xaml
{
    public class XamlSchemaContextSettings
    {
        public bool SupportMarkupExtensionsWithDuplicateArity { get; set; }
        public bool FullyQualifyAssemblyNamesInClrNamespaces { get; set; }

        public XamlSchemaContextSettings()
        {
        }

        public XamlSchemaContextSettings(XamlSchemaContextSettings settings)
        {
            if (settings != null)
            {
                SupportMarkupExtensionsWithDuplicateArity = settings.SupportMarkupExtensionsWithDuplicateArity;
                FullyQualifyAssemblyNamesInClrNamespaces = settings.FullyQualifyAssemblyNamesInClrNamespaces;
            }
        }
    }
}
