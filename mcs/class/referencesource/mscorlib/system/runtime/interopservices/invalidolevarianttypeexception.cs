// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: InvalidOleVariantTypeException
**
** Purpose: The type of an OLE variant that was passed into the runtime is 
**            invalid.
**
=============================================================================*/

namespace System.Runtime.InteropServices {
    
    using System;
    using System.Runtime.Serialization;

[System.Runtime.InteropServices.ComVisible(true)]
    [Serializable] public class InvalidOleVariantTypeException : SystemException {
        public InvalidOleVariantTypeException() 
            : base(Environment.GetResourceString("Arg_InvalidOleVariantTypeException")) {
            SetErrorCode(__HResults.COR_E_INVALIDOLEVARIANTTYPE);
        }
    
        public InvalidOleVariantTypeException(String message) 
            : base(message) {
            SetErrorCode(__HResults.COR_E_INVALIDOLEVARIANTTYPE);
        }
    
        public InvalidOleVariantTypeException(String message, Exception inner) 
            : base(message, inner) {
            SetErrorCode(__HResults.COR_E_INVALIDOLEVARIANTTYPE);
        }

        protected InvalidOleVariantTypeException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
