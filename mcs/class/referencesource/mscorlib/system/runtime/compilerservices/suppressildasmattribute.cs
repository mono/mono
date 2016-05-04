// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
namespace System.Runtime.CompilerServices 
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module)]
    public sealed class SuppressIldasmAttribute : Attribute
    {
        public SuppressIldasmAttribute()
        {
        }
    }
}

