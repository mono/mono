// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:        ThreadStaticAttribute.cs
**
**
** Purpose:     Custom attribute to indicate that the field should be treated 
**              as a static relative to a thread.
**          
**
**
===========================================================*/
namespace System {
    
    using System;

[Serializable]
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
[System.Runtime.InteropServices.ComVisible(true)]
    public class  ThreadStaticAttribute : Attribute
    {
        public ThreadStaticAttribute()
        {
        }
    }
}
