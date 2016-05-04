// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    ComponentServices.cs
**
**
** Purpose: Defines the general purpose ComponentServices
**
**
===========================================================*/
namespace System.Runtime.Remoting.Services {   
    using System;
    using System.Reflection;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Proxies;
    using System.Runtime.Remoting.Messaging;
    using System.Text;
    using System.Runtime.Serialization;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;

    //---------------------------------------------------------\\
    //---------------------------------------------------------\\
    //    internal sealed class ComponentServices                   \\
    //---------------------------------------------------------\\
    //---------------------------------------------------------\\
    
    [System.Security.SecurityCritical]  // auto-generated_required
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class EnterpriseServicesHelper 
    {    
        [System.Security.SecurityCritical]  // auto-generated_required
        public static Object WrapIUnknownWithComObject(IntPtr punk)
        {
            return Marshal.InternalWrapIUnknownWithComObject(punk);
        }        

[System.Runtime.InteropServices.ComVisible(true)]
        public static IConstructionReturnMessage CreateConstructionReturnMessage(IConstructionCallMessage ctorMsg, MarshalByRefObject retObj)
        {            
            IConstructionReturnMessage ctorRetMsg = null;
            
            // Create the return message
            ctorRetMsg = new ConstructorReturnMessage(retObj, null, 0, null, ctorMsg);

            // NOTE: WE ALLOW ONLY DEFAULT CTORs on SERVICEDCOMPONENTS

            return ctorRetMsg;
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        public static void SwitchWrappers(RealProxy oldcp, RealProxy newcp)
        {
            Object oldtp = oldcp.GetTransparentProxy();
            Object newtp = newcp.GetTransparentProxy();

            IntPtr oldcontextId = RemotingServices.GetServerContextForProxy(oldtp);
            IntPtr newcontextId = RemotingServices.GetServerContextForProxy(newtp);

            // switch the CCW from oldtp to new tp
            Marshal.InternalSwitchCCW(oldtp, newtp);
        }                                    
    };
}
