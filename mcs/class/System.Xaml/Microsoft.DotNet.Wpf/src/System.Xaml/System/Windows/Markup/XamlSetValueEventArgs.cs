// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Xaml; 

namespace System.Windows.Markup
{   
    public class XamlSetValueEventArgs : EventArgs
    {
        public XamlSetValueEventArgs(XamlMember member, object value)
        {
            Value = value;
            Member = member;
        }

        public XamlMember Member { get; private set; }
        public object Value { get; private set; }

        public bool Handled { get; set; }

        public virtual void CallBase()
        {
        }
    }
}
