// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Contents:  Context interface for value serializers
//
//  Created:   04/28/2005 Microsoft
//

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup 
{
    /// <summary>
    /// Context provided to ValueSerializer that can be used to special case serialization for different users of the 
    /// ValueSerializaer or for modes of serialization.
    /// </summary>
    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public interface IValueSerializerContext : ITypeDescriptorContext
    {

        /// <summary>
        /// Get the value serializer associated with the given type.
        /// </summary>
        /// <param name="type">The type of the value that is to be convert</param>
        /// <returns>A value serializer for capable of serializing the given type</returns>
        ValueSerializer GetValueSerializerFor(Type type);

        /// <summary>
        /// Get a value serializer for the given property descriptor. A property can override the value serializer that 
        /// is to be used to serialize the property by specifing either a ValueSerializerAttribute or a 
        /// TypeConverterAttribute. This method takes these attributes into account when determining the value 
        /// serializer.
        /// </summary>
        /// <param name="descriptor">The property descriptor for whose property value is being converted</param>
        /// <returns>A value serializer capable of serializing the given property</returns>
        ValueSerializer GetValueSerializerFor(PropertyDescriptor descriptor);
    }
}
