// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    AssemblyNameProxy
** 
** <OWNER>Microsoft</OWNER>
** <OWNER>Microsoft</OWNER>
**
**
** Purpose: Remotable version the AssemblyName
**
**
===========================================================*/
namespace System.Reflection {
    using System;
    using System.Runtime.Versioning;

    [System.Runtime.InteropServices.ComVisible(true)]
    public class AssemblyNameProxy : MarshalByRefObject 
    {
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public AssemblyName GetAssemblyName(String assemblyFile)
        {
            return AssemblyName.GetAssemblyName(assemblyFile);
        }
    }
}
