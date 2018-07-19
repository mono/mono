// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: XmlIgnoreMemberAttribute
**
**
** Purpose: Attribute for properties/members that the Xml Serializer should
**          ignore.
**
**
=============================================================================*/

namespace System
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
    internal sealed class XmlIgnoreMemberAttribute : Attribute
    {
    }
}
