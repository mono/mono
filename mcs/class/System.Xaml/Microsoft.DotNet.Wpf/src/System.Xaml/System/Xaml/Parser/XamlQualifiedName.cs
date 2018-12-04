// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Xaml.MS.Impl;
using System.Collections.Generic;

namespace MS.Internal.Xaml.Parser
{
    internal class XamlQualifiedName : XamlName
    {
        public XamlQualifiedName(string prefix, string name)
            : base(prefix, name)
        {
        }

        public override string ScopedName
        {
            get
            {
                return string.IsNullOrEmpty(Prefix) ?
                    Name :
                    Prefix + ":" + Name;
            }
        }

        internal static bool IsNameValid(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }
            if (!XamlName.IsValidNameStartChar(name[0]))
            {
                return false;
            }
            for (int i = 1; i < name.Length; i++)
            {
                if (!XamlName.IsValidQualifiedNameChar(name[i]))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsNameValid_WithPlus(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }
            if (!XamlName.IsValidNameStartChar(name[0]))
            {
                return false;
            }
            for (int i = 1; i < name.Length; i++)
            {
                if (!XamlName.IsValidQualifiedNameCharPlus(name[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Parse(string longName, out string prefix, out string name)
        {
            int start = 0;
            int colonIdx = longName.IndexOf(':');

            prefix = String.Empty;
            name = String.Empty;

            if (colonIdx != -1)
            {
                prefix = longName.Substring(start, colonIdx);

                if (String.IsNullOrEmpty(prefix) || !IsNameValid(prefix))
                {
                    return false;
                }

                start = colonIdx + 1;
            }

            name = (start==0) ? longName : longName.Substring(start);

            // we allow Internal type name (ie. Foo+Bar) on "trival" ie. "non-generic" type names.
            // This is back compat with 3.0.
            // Don't want to allow it in any of the new type name syntax.  (including trival typeArgs)
            if (String.IsNullOrEmpty(name) || !IsNameValid_WithPlus(name))
            {
                return false;
            }

            return true;
        }
    }
}
