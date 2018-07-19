// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: IFormatProvider
**
**
** Purpose: Notes a class which knows how to return formatting information
**
**
============================================================*/
namespace System {
    
    using System;

    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IFormatProvider
    {
        // Interface does not need to be marked with the serializable attribute
        Object GetFormat(Type formatType);
    }
}
