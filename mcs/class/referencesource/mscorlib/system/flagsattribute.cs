// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
namespace System {
    
    using System;
    // Custom attribute to indicate that the enum
    // should be treated as a bitfield (or set of flags).
    // An IDE may use this information to provide a richer
    // development experience.
    [Serializable]
    [AttributeUsage(AttributeTargets.Enum, Inherited = false)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class FlagsAttribute : Attribute
    {
        public FlagsAttribute()
        {
        }
    }
}
