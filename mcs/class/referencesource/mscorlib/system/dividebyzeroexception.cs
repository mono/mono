// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: DivideByZeroException
**
**
** Purpose: Exception class for bad arithmetic conditions!
**
**
=============================================================================*/

namespace System {
    
    using System;
    using System.Runtime.Serialization;
[System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public class DivideByZeroException : ArithmeticException {
        public DivideByZeroException() 
            : base(Environment.GetResourceString("Arg_DivideByZero")) {
            SetErrorCode(__HResults.COR_E_DIVIDEBYZERO);
        }
    
        public DivideByZeroException(String message) 
            : base(message) {
            SetErrorCode(__HResults.COR_E_DIVIDEBYZERO);
        }
    
        public DivideByZeroException(String message, Exception innerException) 
            : base(message, innerException) {
            SetErrorCode(__HResults.COR_E_DIVIDEBYZERO);
        }

        protected DivideByZeroException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
