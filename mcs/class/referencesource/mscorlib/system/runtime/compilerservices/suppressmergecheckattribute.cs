// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices 
{
    [AttributeUsage(AttributeTargets.Class | 
                    AttributeTargets.Constructor | 
                    AttributeTargets.Method |
                    AttributeTargets.Field |
                    AttributeTargets.Event |
                    AttributeTargets.Property)]

    internal sealed class SuppressMergeCheckAttribute : Attribute
    {
        public SuppressMergeCheckAttribute() 
        {}  
    }
}

