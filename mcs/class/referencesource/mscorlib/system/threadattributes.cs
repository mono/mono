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
