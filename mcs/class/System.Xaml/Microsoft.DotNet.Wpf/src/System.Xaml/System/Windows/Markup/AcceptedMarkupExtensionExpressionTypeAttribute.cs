// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace System.Windows.Markup
{
    [Obsolete("This is not used by the XAML parser. Please look at XamlSetMarkupExtensionAttribute.")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=true)]
    public class AcceptedMarkupExtensionExpressionTypeAttribute: Attribute
    {
        public Type Type {get; set; }

        public AcceptedMarkupExtensionExpressionTypeAttribute(Type type)
	{
            Type = type;
        }
    }
}