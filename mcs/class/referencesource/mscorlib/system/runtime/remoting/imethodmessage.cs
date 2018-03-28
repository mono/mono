// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    IMethodMessage.cs
**
**
** Purpose: Defines the message object interface
**
**
===========================================================*/
namespace System.Runtime.Remoting.Messaging {
    using System;
    using System.Reflection;
    using System.Security.Permissions;
    using IList = System.Collections.IList;
    
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IMethodMessage : IMessage
    {
        String Uri                      
        {
             [System.Security.SecurityCritical]  // auto-generated_required
             get;
        }
        String MethodName               
        {
             [System.Security.SecurityCritical]  // auto-generated_required
             get;
        }
        String TypeName     
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
        Object MethodSignature
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get; 
        }
       
        int ArgCount
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
        [System.Security.SecurityCritical]  // auto-generated_required
        String GetArgName(int index);
        [System.Security.SecurityCritical]  // auto-generated_required
        Object GetArg(int argNum);
        Object[] Args
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }

        bool HasVarArgs
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
        LogicalCallContext LogicalCallContext
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }

        // This is never actually put on the wire, it is
        // simply used to cache the method base after it's
        // looked up once.
        MethodBase MethodBase           
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
    }
    
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IMethodCallMessage : IMethodMessage
    {
        int InArgCount
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
        [System.Security.SecurityCritical]  // auto-generated_required
        String GetInArgName(int index);
        [System.Security.SecurityCritical]  // auto-generated_required
        Object GetInArg(int argNum);
        Object[] InArgs
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
    }

[System.Runtime.InteropServices.ComVisible(true)]
    public interface IMethodReturnMessage : IMethodMessage
    {
        int OutArgCount                
        {
            [System.Security.SecurityCritical]  // auto-generated_required
             get;
        }
        [System.Security.SecurityCritical]  // auto-generated_required
        String GetOutArgName(int index);
        [System.Security.SecurityCritical]  // auto-generated_required
        Object GetOutArg(int argNum);
        Object[]  OutArgs         
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
        
        Exception Exception        
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
        Object    ReturnValue 
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
    }

}
