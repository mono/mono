// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices {

    using System;

    // Custom attribute to indicating a TypeDef is a discardable attribute
[System.Runtime.InteropServices.ComVisible(true)]
    public class DiscardableAttribute : Attribute
    {
        public DiscardableAttribute()
        {
        }
    }
}
