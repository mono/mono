// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: SafeArrayTypeMismatchException
**
** Purpose: This exception is thrown when the runtime type of an array
**            is different than the safe array sub type specified in the 
**            metadata.
**
=============================================================================*/

namespace System.Runtime.InteropServices {

    using System;
    using System.Runtime.Serialization;

[System.Runtime.InteropServices.ComVisible(true)]
    [Serializable] public class SafeArrayTypeMismatchException : SystemException {
        public SafeArrayTypeMismatchException() 
            : base(Environment.GetResourceString("Arg_SafeArrayTypeMismatchException")) {
            SetErrorCode(__HResults.COR_E_SAFEARRAYTYPEMISMATCH);
        }
    
        public SafeArrayTypeMismatchException(String message) 
            : base(message) {
            SetErrorCode(__HResults.COR_E_SAFEARRAYTYPEMISMATCH);
        }
    
        public SafeArrayTypeMismatchException(String message, Exception inner) 
            : base(message, inner) {
            SetErrorCode(__HResults.COR_E_SAFEARRAYTYPEMISMATCH);
        }

        protected SafeArrayTypeMismatchException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

    }

}
