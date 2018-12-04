// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//
// Description:
//     This attribute is placed on a class to identify it as the place to set
// the value of the x:Uid attribute from the XML markup file.
//
// Example:
//     [UidProperty("Uid")]
//     public class ExampleFrameworkElement
//
//   Means that when the parser sees:
//
//     <ExampleFrameworkElement x:Uid="efe1">
//
//   The parser will set the "Uid" property with the value "efe1".
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
    public sealed class UidPropertyAttribute : Attribute
    {
        /// <summary>
        ///     Creates a new UidPropertyAttribute with the given string 
        /// as the property name.
        /// </summary>
        public UidPropertyAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        ///     The name of the property that is designated to accept the x:Uid value
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        // The name of the property that is designated to accept the x:Uid value
        private string _name = null;
    }    
}
