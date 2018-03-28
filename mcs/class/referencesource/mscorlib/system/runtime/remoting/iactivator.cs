// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    IActivator.cs
**
**
** Purpose: Defines the interface provided by activation services
**          
**
**
===========================================================*/
namespace System.Runtime.Remoting.Activation {

    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Collections;
    using System.Security.Permissions;
    
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IActivator
    {
        // return the next activator in the chain
        IActivator NextActivator 
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get; 
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }
        
        // New method for activators.
        [System.Security.SecurityCritical]  // auto-generated_required
        IConstructionReturnMessage Activate(IConstructionCallMessage msg);     

           // Returns the level at which this activator is active ..
           // Should return one of the ActivatorLevels below
        ActivatorLevel Level 
        { 
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
    }

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public enum ActivatorLevel
    {
        Construction = 4,
        Context = 8,
        AppDomain = 12,
        Process = 16,
        Machine = 20
    }

    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IConstructionCallMessage : IMethodCallMessage
    {
        IActivator Activator                   
        { 
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }
        Object[] CallSiteActivationAttributes  
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
        String ActivationTypeName               
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
        Type ActivationType                     
        { 
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
        IList ContextProperties                
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
    }
    
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IConstructionReturnMessage : IMethodReturnMessage
    {
    }
    
}
