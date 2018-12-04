// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//
// Description:
//     This attribute is placed on a class to identify it as the place to set
// the value of the xml:lang attribute from the XML markup file.
//
// Example:
//     [XmlLangProperty("Language")]
//     public class ExampleFrameworkElement
//
//   Means that when the parser sees:
//
//     <ExampleFrameworkElement xml:lang="en-US">
//
//   The parser will set the "Language" property with the value "en-US".
//
//
// 

using System;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup
{
    /// <summary>
    ///     An attribute that specifies which property the xml:lang value should
    /// be directed to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class XmlLangPropertyAttribute : Attribute
    {
        /// <summary>
        ///     Creates a new XmlLangPropertyAttribute with the given string 
        /// as the property name.
        /// </summary>
        public XmlLangPropertyAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        ///     The name of the property that is designated to accept the xml:lang value
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        // The name of the property that is designated to accept the xml:lang value
        private string _name = null;
    }    
}
