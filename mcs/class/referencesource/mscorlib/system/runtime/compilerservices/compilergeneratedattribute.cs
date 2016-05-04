// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
namespace System.Runtime.CompilerServices {

[Serializable]
[AttributeUsage(AttributeTargets.All, Inherited = true)]
    public sealed class CompilerGeneratedAttribute : Attribute
    {
        public CompilerGeneratedAttribute () {}
    }
}

