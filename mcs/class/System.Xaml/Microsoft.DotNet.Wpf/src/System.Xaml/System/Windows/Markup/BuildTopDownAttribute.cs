// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

using System.Text;

namespace System.Windows.Markup
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class UsableDuringInitializationAttribute : Attribute
    {
        bool _usable;

        public UsableDuringInitializationAttribute(bool usable)
        {
            _usable = usable;
        }

        public bool Usable { get { return _usable; } }
    }
}
