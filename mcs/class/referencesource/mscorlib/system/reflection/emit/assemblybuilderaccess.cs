// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// <OWNER>WESU</OWNER>
using System;

// This enumeration defines the access modes for a dynamic assembly.
// EE uses these enum values..look for m_dwDynamicAssemblyAccess in Assembly.hpp

namespace System.Reflection.Emit 
{    
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    [Flags]
    public enum AssemblyBuilderAccess
    {
        Run = 1,
#if !FEATURE_CORECLR // these are unsupported
        Save = 2,
        RunAndSave = Run | Save,
#endif // !FEATURE_CORECLR
#if FEATURE_REFLECTION_ONLY_LOAD
        ReflectionOnly = 6, // 4 | Save,
#endif // FEATURE_REFLECTION_ONLY_LOAD
#if FEATURE_COLLECTIBLE_TYPES
        RunAndCollect = 8 | Run
#endif
    }
}
