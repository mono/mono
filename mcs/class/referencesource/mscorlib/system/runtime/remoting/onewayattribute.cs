// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    OneWayAttribute.cs
**
** Attribute for marking methods as one way
** 
**
**
===========================================================*/
namespace System.Runtime.Remoting.Messaging {
    using System.Runtime.Remoting;
    using System;
     using System.Security.Permissions;
    
    [AttributeUsage(AttributeTargets.Method)]       // bInherited
[System.Runtime.InteropServices.ComVisible(true)]
    public class OneWayAttribute : Attribute
    {
    }

}
