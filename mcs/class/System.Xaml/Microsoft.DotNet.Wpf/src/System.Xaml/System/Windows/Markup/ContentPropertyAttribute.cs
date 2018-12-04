// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Description: Specifies which property of a class should be written as the
//               direct content of the class when written to XAML.
//

using System;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup
{

    /// <summary>
    /// An attribute that specifies which property the direct content of a XAML
    /// element should be associated with.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class ContentPropertyAttribute : Attribute
    {

        /// <summary>
        /// Creates a new content property attriubte that indicates that associated
        /// class does not have a content property attribute. This allows a descendent
        /// remove an ancestors declaration of a content property attribute.
        /// </summary>
        public ContentPropertyAttribute()
        {
        }

        /// <summary>
        /// Creates a new content property attribute that associates the direct content
        /// of XAML content with property of the given name
        /// </summary>
        /// <param name="name">The property to associate to direct XAML content</param>
        public ContentPropertyAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// The name of the property that is associated with direct XAML content
        /// </summary>
        public string Name { 
            get { return _name; } 
        }

        private string _name;
    }
}
