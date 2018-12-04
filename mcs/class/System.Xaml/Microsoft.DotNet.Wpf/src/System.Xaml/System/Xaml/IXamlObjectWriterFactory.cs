// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.ComponentModel;

namespace System.Xaml
{
    public interface IXamlObjectWriterFactory
    {
        XamlObjectWriterSettings GetParentSettings();

        XamlObjectWriter GetXamlObjectWriter(XamlObjectWriterSettings settings);
    }
}
