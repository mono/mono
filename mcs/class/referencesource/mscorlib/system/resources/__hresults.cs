// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//=============================================================================
//
// Class: __HResults
// 
// <OWNER>jathaine</OWNER>
//
// Purpose: Define HResult constants returned by the Windows Modern Resource Manager
// and consumed by System.Resources.ResourceManager.
//
//===========================================================================*/
#if FEATURE_APPX
namespace System.Resources {
    using System;
    // Only static data no need to serialize
    internal static class __HResults
    {
        // From WinError.h
        public const int ERROR_MRM_MAP_NOT_FOUND = unchecked((int)0x80073B1F);
    }
}
#endif
