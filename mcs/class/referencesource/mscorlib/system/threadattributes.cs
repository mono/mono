// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** File: ThreadAttributes.cs
**
** Author: 
**
** Purpose: For Threads-related custom attributes.
**
** Date: July, 2000
**
=============================================================================*/


namespace System {
    [AttributeUsage (AttributeTargets.Method)]  
[System.Runtime.InteropServices.ComVisible(true)]
#if FEATURE_CORECLR && !FEATURE_NETCORE
    [Obsolete("STAThreadAttribute is not supported in this release. It has been left in so that legacy tools can be used with this release, but it cannot be used in your code.", true)] 
#endif // FEATURE_CORECLR
    public sealed class STAThreadAttribute : Attribute
    {
        public STAThreadAttribute()
        {
        }
    }

    [AttributeUsage (AttributeTargets.Method)]  
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class MTAThreadAttribute : Attribute
    {
        public MTAThreadAttribute()
        {
        }
    }
}
