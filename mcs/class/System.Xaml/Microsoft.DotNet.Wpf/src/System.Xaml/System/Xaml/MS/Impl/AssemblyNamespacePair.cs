// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using System.Diagnostics;

namespace System.Xaml.MS.Impl
{
    [DebuggerDisplay("{ClrNamespace} {Assembly.FullName}")]
    internal class AssemblyNamespacePair
    {
        WeakReference _assembly;
        String _clrNamespace;

        public AssemblyNamespacePair(Assembly asm, String clrNamespace)
        {
            _assembly = new WeakReference(asm);
            _clrNamespace = clrNamespace;
        }

        public Assembly Assembly
        {
            get { return (Assembly)_assembly.Target; }
        }

        public string ClrNamespace
        {
            get { return _clrNamespace; }
        }
    }
}
