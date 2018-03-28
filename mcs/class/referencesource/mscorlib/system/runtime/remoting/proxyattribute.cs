// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    ProxyAttribute.cs
**
**
** Purpose: Defines the attribute that is used on types which
**          need custom proxies.
**
**
===========================================================*/
namespace System.Runtime.Remoting.Proxies {
        
    using System.Reflection;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Contexts;
    using System.Security.Permissions;

    // Attribute for types that need custom proxies
    [System.Security.SecurityCritical]  // auto-generated_required
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ProxyAttribute : Attribute , IContextAttribute
    {
        public ProxyAttribute()
        {
            // Default constructor
        }
        
        // Default implementation of CreateInstance uses our activation services to create an instance
        // of the transparent proxy or an uninitialized marshalbyrefobject and returns it.

        [System.Security.SecurityCritical]  // auto-generated
        public virtual MarshalByRefObject CreateInstance(Type serverType)
        {
            if (serverType == null)
                throw new ArgumentNullException("serverType");

            RuntimeType rt = serverType as RuntimeType;
            if (rt == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));

            if (!serverType.IsContextful)
            {
                throw new RemotingException(                     
                    Environment.GetResourceString(
                        "Remoting_Activation_MBR_ProxyAttribute"));
            }
            if (serverType.IsAbstract)
            {
                throw new RemotingException(
                    Environment.GetResourceString(
                        "Acc_CreateAbst"));
            }
            return CreateInstanceInternal(rt);
        }

        internal MarshalByRefObject CreateInstanceInternal(RuntimeType serverType)
        {
            return ActivationServices.CreateInstance(serverType);
        }

        // Default implementation of CreateProxy creates an instance of our
        // remoting proxy

        [System.Security.SecurityCritical]  // auto-generated
        public virtual RealProxy CreateProxy(ObjRef objRef, 
                                             Type serverType,  
                                             Object serverObject, 
                                             Context serverContext)
        {
            RemotingProxy rp =  new RemotingProxy(serverType);    

            // If this is a serverID, set the native context field in the TP
            if (null != serverContext)
            {
                RealProxy.SetStubData(rp, serverContext.InternalContextID);
            }

            if (objRef != null && objRef.GetServerIdentity().IsAllocated)
            {
                rp.SetSrvInfo(objRef.GetServerIdentity(), objRef.GetDomainID());                
            }
            
            // Set the flag indicating that the fields of the proxy
            // have been initialized
            rp.Initialized = true;
    
            // Sanity check
            Type t = serverType;
            if (!t.IsContextful && 
                !t.IsMarshalByRef && 
                (null != serverContext))
            {
                throw new RemotingException(                     
                    Environment.GetResourceString(
                        "Remoting_Activation_MBR_ProxyAttribute"));
            }

            return rp;
        }

        // implementation of interface IContextAttribute
        [System.Security.SecurityCritical]
        [System.Runtime.InteropServices.ComVisible(true)]
        public bool IsContextOK(Context ctx, IConstructionCallMessage msg)
        {
            // always happy...
            return true;
        }

        [System.Security.SecurityCritical]
        [System.Runtime.InteropServices.ComVisible(true)]
        public void GetPropertiesForNewContext(IConstructionCallMessage msg)
        {
            // chill.. do nothing.
            return;
        }
    }
}
