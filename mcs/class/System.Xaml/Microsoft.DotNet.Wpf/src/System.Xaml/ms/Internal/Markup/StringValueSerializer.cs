// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Microsoft Windows Client Platform
//
//
//  Contents:  Stub value serializer for strings
//
//  Created:   04/28/2005 Microsoft
//

using System.Windows.Markup;

namespace MS.Internal.Serialization
{
    /// <summary>
    /// Stub string serializer. It exists to remove special caseing strings in a couple cases in the
    /// serialization code. It essentially states that strings are serialized as their value.
    /// </summary>
    internal sealed class StringValueSerializer : ValueSerializer
    {
        public StringValueSerializer() { }

        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            return true;
        }

        public override bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            return true;
        }

        public override object ConvertFromString(string value, IValueSerializerContext context)
        {
            return value;
        }

        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            return (string)value;
        }
    }
}
