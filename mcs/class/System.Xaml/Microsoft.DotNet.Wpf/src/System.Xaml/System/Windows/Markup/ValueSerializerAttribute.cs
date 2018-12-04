// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Contents:  An attribute that allows associating a ValueSerializer 
//             implementation with either a type or a property (or
//             an attached property by setting it on the static accessor
//             for the attachable property).
//
//  Created:   04/28/2005 Microsoft
//

using System;
using System.Runtime.CompilerServices;

[assembly:TypeForwardedTo(typeof(System.Windows.Markup.ValueSerializerAttribute))]
