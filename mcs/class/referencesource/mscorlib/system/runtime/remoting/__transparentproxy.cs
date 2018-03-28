// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    __TransparentProxy.cs
**
**
** Purpose: Defines Transparent proxy
**
**
===========================================================*/
namespace System.Runtime.Remoting.Proxies {
    using System.Runtime.Remoting;
    // Transparent proxy and Real proxy are vital pieces of the
    // remoting data structures. Transparent proxy magically
    // creates a message that represents a call on it and delegates
    // to the Real proxy to do the real remoting work.
    using System;
    internal sealed class __TransparentProxy {
        // Created inside EE
        private __TransparentProxy() {
           throw new NotSupportedException(Environment.GetResourceString(ResId.NotSupported_Constructor));
        }
        
        // Private members called by VM
#pragma warning disable 169
        [System.Security.SecurityCritical] // auto-generated
        private RealProxy _rp;          // Reference to the real proxy
        private Object    _stubData;    // Data used by stubs to decide whether to short circuit calls or not
        private IntPtr _pMT;            // Method table of the class this proxy represents
        private IntPtr _pInterfaceMT;   // Cached interface method table        
        private IntPtr _stub;           // Unmanaged code that decides whether to short circuit calls or not
#pragma warning restore 169

    }

}

