// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
using System;
using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices {
[Serializable]
[AttributeUsage(AttributeTargets.Struct, Inherited = true),
     System.Runtime.InteropServices.ComVisible(true)]
    public sealed class NativeCppClassAttribute : Attribute
    {
        public NativeCppClassAttribute () {}
    }
}
