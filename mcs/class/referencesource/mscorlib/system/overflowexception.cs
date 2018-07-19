// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: OverflowException
**
**
** Purpose: Exception class for Arthimatic Overflows.
**
**
=============================================================================*/

namespace System {
 
    
    using System;
    using System.Runtime.Serialization;
[System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public class OverflowException : ArithmeticException {
        public OverflowException() 
            : base(Environment.GetResourceString("Arg_OverflowException")) {
            SetErrorCode(__HResults.COR_E_OVERFLOW);
        }
    
        public OverflowException(String message) 
            : base(message) {
            SetErrorCode(__HResults.COR_E_OVERFLOW);
        }
        
        public OverflowException(String message, Exception innerException) 
            : base(message, innerException) {
            SetErrorCode(__HResults.COR_E_OVERFLOW);
        }

        protected OverflowException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

    }

}
