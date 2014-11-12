// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    ContextBoundObject.cs
**<EMAIL>
** Author(s):   Tarun Anand    ([....])
**</EMAIL>              
**
** Purpose: Defines the root type for all context bound types
**          
**
===========================================================*/
namespace System {   
    
    using System;
    using System.Security.Permissions;
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
#if FEATURE_REMOTING
    public abstract class ContextBoundObject : MarshalByRefObject {
#else // FEATURE_REMOTING
    public abstract class ContextBoundObject {
#endif // FEATURE_REMOTING
    }
}
