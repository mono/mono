// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: ConsoleModifiers
**
**
** Purpose: This enumeration represents the keys Alt, Shift, and Control 
**          which modify the meaning of another key when pressed.
**
**
=============================================================================*/

namespace System {
[Serializable]
[Flags]
    public enum ConsoleModifiers
    {
        Alt = 1,
        Shift = 2,
        Control = 4
    }
}
