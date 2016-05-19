// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: IndexOutOfRangeException
**
**
** Purpose: Exception class for invalid array indices.
**
**
=============================================================================*/

namespace System {
    
    using System;
    using System.Runtime.Serialization;
[System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public sealed class IndexOutOfRangeException : SystemException {
        public IndexOutOfRangeException() 
            : base(Environment.GetResourceString("Arg_IndexOutOfRangeException")) {
            SetErrorCode(__HResults.COR_E_INDEXOUTOFRANGE);
        }
    
        public IndexOutOfRangeException(String message) 
            : base(message) {
            SetErrorCode(__HResults.COR_E_INDEXOUTOFRANGE);
        }
        
        public IndexOutOfRangeException(String message, Exception innerException) 
            : base(message, innerException) {
            SetErrorCode(__HResults.COR_E_INDEXOUTOFRANGE);
        }

        internal IndexOutOfRangeException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

    }

}
