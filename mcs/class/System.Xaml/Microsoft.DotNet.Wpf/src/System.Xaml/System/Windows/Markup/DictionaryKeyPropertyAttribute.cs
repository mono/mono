// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

using System.Text;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class DictionaryKeyPropertyAttribute : Attribute
    {
        string _name;

        public DictionaryKeyPropertyAttribute(string name)
        {
            _name = name;
        }

        public string Name { get { return _name; } }
    }
}
