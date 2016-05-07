// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: StackOverflowException
**
**
** Purpose: The exception class for stack overflow.
**
**
=============================================================================*/

namespace System {
    
    using System;
    using System.Runtime.Serialization;
[System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public sealed class StackOverflowException : SystemException {
        public StackOverflowException() 
            : base(Environment.GetResourceString("Arg_StackOverflowException")) {
            SetErrorCode(__HResults.COR_E_STACKOVERFLOW);
        }
    
        public StackOverflowException(String message) 
            : base(message) {
            SetErrorCode(__HResults.COR_E_STACKOVERFLOW);
        }
        
        public StackOverflowException(String message, Exception innerException) 
            : base(message, innerException) {
            SetErrorCode(__HResults.COR_E_STACKOVERFLOW);
        }

        internal StackOverflowException(SerializationInfo info, StreamingContext context) : base (info, context) {
        }
        
    }
}
