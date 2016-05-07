// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: EditAndContinueHelper
**
**
** Purpose: Helper for EditAndContinue
**
**
=============================================================================*/

namespace System.Diagnostics {
    
    using System;
    
#if !FEATURE_PAL
    [Serializable]
    internal sealed class EditAndContinueHelper 
    {
#pragma warning disable 169
#pragma warning disable 414  // Field is not used from managed.
        private Object _objectReference;
#pragma warning restore 414
#pragma warning restore 169
    }
#endif // !FEATURE_PAL
}
