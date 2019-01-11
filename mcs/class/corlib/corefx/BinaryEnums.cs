// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System;

namespace System.Runtime.Serialization.Formatters.Binary
{
    [Serializable]
    [Flags]
    internal enum MessageEnum
    {
        NoArgs = 0x1,
        ArgsInline = 0x2,
        ArgsIsArray = 0x4,
        ArgsInArray = 0x8,
        NoContext = 0x10,
        ContextInline = 0x20,
        ContextInArray = 0x40,
        MethodSignatureInArray = 0x80,
        PropertyInArray = 0x100,
        NoReturnValue = 0x200,
        ReturnValueVoid = 0x400,
        ReturnValueInline = 0x800,
        ReturnValueInArray = 0x1000,
        ExceptionInArray = 0x2000,
        GenericMethod = 0x8000
    }

    [Serializable]
    internal enum SoapAttributeType
    {
        None = 0x0,
        SchemaType = 0x1,
        Embedded = 0x2,
        XmlElement = 0x4,
        XmlAttribute = 0x8
    }
}