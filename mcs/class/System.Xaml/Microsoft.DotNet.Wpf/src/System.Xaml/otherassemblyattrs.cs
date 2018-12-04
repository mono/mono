// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
// This file specifies various assembly level attributes.
//

using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;

[assembly:DependencyAttribute("mscorlib,", LoadHint.Always)]
[assembly:DependencyAttribute("System,", LoadHint.Always)]
[assembly:DependencyAttribute("System.Xml,", LoadHint.Sometimes)]

[assembly:System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System.Windows.Markup")]
