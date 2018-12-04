// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Markup
{
   [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class XamlSetTypeConverterAttribute : Attribute
    {
        public XamlSetTypeConverterAttribute(string xamlSetTypeConverterHandler)
        {
            XamlSetTypeConverterHandler = xamlSetTypeConverterHandler;
        }

        public string XamlSetTypeConverterHandler { get; private set; }
    }
}