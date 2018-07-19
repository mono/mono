// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//=============================================================================
//
// Class: __HResults
//
// Purpose: Define HResult constants. Every exception has one of these.
//
// Date: 98/08/31 11:57:11 AM
//
//===========================================================================*/
namespace System.Runtime.Remoting {
    using System;
    internal sealed class __HResults
    {
        public const int COR_E_REMOTING = unchecked((int)0x8013150B);
        public const int COR_E_SERVER = unchecked((int)0x8013150E);        
    }
}
