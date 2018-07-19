// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>ericeil</OWNER>
/*=============================================================================
**
** Class: ApartmentState
**
**
** Purpose: Enum to represent the different threading models
**
**
=============================================================================*/

namespace System.Threading {

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public enum ApartmentState
    {   
        /*=========================================================================
        ** Constants for thread apartment states.
        =========================================================================*/
        STA = 0,
        MTA = 1,
        Unknown = 2
    }
}
