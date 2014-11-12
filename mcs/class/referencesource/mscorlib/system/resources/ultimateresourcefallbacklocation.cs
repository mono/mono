// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Enum:  UltimateResourceFallbackLocation
** 
** <OWNER>[....]</OWNER>
**
** <EMAIL>Author: Brian Grunkemeyer ([....])</EMAIL>
**
** Purpose: Tells the ResourceManager where to find the
**          ultimate fallback resources for your assembly.
**
** Date:  August 21, 2003
**
===========================================================*/

using System;

namespace System.Resources {

[Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public enum UltimateResourceFallbackLocation
    {
        MainAssembly,
        Satellite
    }
}
