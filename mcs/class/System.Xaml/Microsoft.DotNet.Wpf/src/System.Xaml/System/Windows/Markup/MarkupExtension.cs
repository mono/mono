// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup
{
    /// <summary>
    ///  Base class for all Xaml markup extensions.  Only subclasses can
    ///  be instantiated.
    /// </summary>
    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class MarkupExtension 
    {
        /// <summary>
        ///  Return an object that should be set on the targetObject's targetProperty
        ///  for this markup extension.  
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        ///  The object to set on this property.
        /// </returns>
        public abstract object ProvideValue(IServiceProvider serviceProvider);
       
    }
}

