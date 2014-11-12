// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

// moved to mscorlib.dll in Full 4.5. In SL5, we can't because it ships first
// so we defer that for SL6. For now, we keep the current location (in SL5)
// For Windows Phone 8 move it to mscorlib

#if SILVERLIGHT && !FEATURE_NETCORE
using System;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Indicates that a method is an extension method, or that a class or assembly contains extension methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class ExtensionAttribute : Attribute { }
}
#else
[assembly:System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Runtime.CompilerServices.ExtensionAttribute))]
#endif

