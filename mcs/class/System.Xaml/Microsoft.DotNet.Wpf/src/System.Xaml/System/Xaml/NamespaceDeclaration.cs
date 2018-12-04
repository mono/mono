// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;

namespace System.Xaml
{
    [DebuggerDisplay("Prefix={Prefix} Namespace={Namespace}")]
    public class NamespaceDeclaration
    {
        private string prefix;
        
        private string ns;

        public NamespaceDeclaration(string ns, string prefix)
        {
            this.ns = ns;
            this.prefix = prefix;
        }

        public string Prefix
        {
            get
            {
                return prefix;
            }
        }
        
        public string Namespace
        {
            get
            {
                return ns;
            }
        }
    }
}
