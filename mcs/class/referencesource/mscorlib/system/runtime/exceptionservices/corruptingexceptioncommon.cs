#if FEATURE_CORRUPTING_EXCEPTIONS
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** File: CorruptingExceptionCommon.cs
**
**
** Purpose: Contains common usage support entities for Corrupting Exceptions
**
** Created: 06/20/2008
** 
** <owner>Microsoft</owner>
** 
=============================================================================*/

namespace System.Runtime.ExceptionServices {
    using System;
    
    // This attribute can be applied to methods to indicate that ProcessCorruptedState
    // Exceptions should be delivered to them.
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class HandleProcessCorruptedStateExceptionsAttribute : Attribute
    {
        public HandleProcessCorruptedStateExceptionsAttribute()
        {
        }
    }
}
#endif // FEATURE_CORRUPTING_EXCEPTIONS
