// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:        StringFreezingAttribute.cs
**
**
** Purpose:     Custom attribute to indicate that strings should be frozen
**
**
===========================================================*/

namespace System.Runtime.CompilerServices
{
    
[Serializable]
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class StringFreezingAttribute : Attribute
    {
        public StringFreezingAttribute()
        {
        }
    }
}
