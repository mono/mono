// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Windows;
using System.Collections.Generic;

namespace System.Windows.Markup
{
    public interface INameScopeDictionary : INameScope, IDictionary<string, object>
    {
    }
}
