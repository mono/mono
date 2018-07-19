// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    AssemblyVersionCompatibility
**
** <EMAIL>Author:  Suzanne Cook</EMAIL>
**
** Purpose: defining the different flavor's assembly version compatibility
**
** Date:    June 4, 1999
**
===========================================================*/
namespace System.Configuration.Assemblies {
    
    using System;
     [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public enum AssemblyVersionCompatibility
    {
        SameMachine         = 1,
        SameProcess         = 2,
        SameDomain          = 3,
    }
}
