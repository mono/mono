// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/***************************************************************************\
*
*
*  Class for Xaml markup extension {Null}
*
*
\***************************************************************************/
using System;
using System.Windows;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup
{
    /// <summary>
    ///  Class for Xaml markup extension for Null.  
    /// </summary>
    [TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    [MarkupExtensionReturnType(typeof(object))]
    public class NullExtension : MarkupExtension 
    {
        /// <summary>
        ///  Default constructor
        /// </summary>
        public NullExtension()
        {
        }
        
        /// <summary>
        ///  Return an object that should be set on the targetObject's targetProperty
        ///  for this markup extension.  In this case it is simply null.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        ///  The object to set on this property.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return null;
        }

    }
}

