// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: MarshalDirectiveException
**
** Purpose: This exception is thrown when the marshaller encounters a signature
**          that has an invalid MarshalAs CA for a given argument or is not
**          supported.
**
=============================================================================*/

namespace System.Runtime.InteropServices {

    using System;
    using System.Runtime.Serialization;

[System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public class MarshalDirectiveException : SystemException {
        public MarshalDirectiveException() 
            : base(Environment.GetResourceString("Arg_MarshalDirectiveException")) {
            SetErrorCode(__HResults.COR_E_MARSHALDIRECTIVE);
        }
    
        public MarshalDirectiveException(String message) 
            : base(message) {
            SetErrorCode(__HResults.COR_E_MARSHALDIRECTIVE);
        }
    
        public MarshalDirectiveException(String message, Exception inner) 
            : base(message, inner) {
            SetErrorCode(__HResults.COR_E_MARSHALDIRECTIVE);
        }

        protected MarshalDirectiveException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
