// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  Enums
**
**
** Purpose: Enums shared by IO classes
**
**
===========================================================*/

using System;
using System.Text;

namespace System.IO {

#if !FEATURE_CORESYSTEM
    [Serializable]
#endif
    public enum HandleInheritability {
        None = 0,
        Inheritable = 1,
    }

}


