// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Description: Describes what type a markup extension can return.
//
//  Created:     11/17/2005
//

using System;

namespace System.Windows.Markup
{

    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class MarkupExtensionReturnTypeAttribute : Attribute
    {

        /// <summary>
        /// </summary>
        public MarkupExtensionReturnTypeAttribute(Type returnType)
        {
            _returnType = returnType;
        }

        [Obsolete("The expressionType argument is not used by the XAML parser. To specify the expected return type, " +
            "use MarkupExtensionReturnTypeAttribute(Type). To specify custom handling for expression types, use " +
            "XamlSetMarkupExtensionAttribute.")]
        public MarkupExtensionReturnTypeAttribute(Type returnType, Type expressionType)
        {
            _returnType = returnType;
            _expressionType = expressionType;
        }

        /// <summary>
        /// </summary>
        public MarkupExtensionReturnTypeAttribute()
        {
        }

        /// <summary>
        /// </summary>
        public Type ReturnType
        { 
            get { return _returnType; } 
        }

        [Obsolete("This is not used by the XAML parser. Please look at XamlSetMarkupExtensionAttribute.")]
        public Type ExpressionType
        { 
            get { return _expressionType; } 
        }

        private Type _returnType;
        private Type _expressionType;
    }
}
