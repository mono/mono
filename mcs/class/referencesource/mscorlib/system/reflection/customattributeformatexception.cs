// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// CustomAttributeFormatException is thrown when the binary format of a 
// 
// <OWNER>Microsoft</OWNER>
//    custom attribute is invalid.
//
// <EMAIL>Author: darylo</EMAIL>
// Date: Microsoft 98
//
namespace System.Reflection {
    using System;
    using ApplicationException = System.ApplicationException;
    using System.Runtime.Serialization;
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public class CustomAttributeFormatException  : FormatException {
    
        public CustomAttributeFormatException()
            : base(Environment.GetResourceString("Arg_CustomAttributeFormatException")) {
            SetErrorCode(__HResults.COR_E_CUSTOMATTRIBUTEFORMAT);
        }
    
        public CustomAttributeFormatException(String message) : base(message) {
            SetErrorCode(__HResults.COR_E_CUSTOMATTRIBUTEFORMAT);
        }
        
        public CustomAttributeFormatException(String message, Exception inner) : base(message, inner) {
            SetErrorCode(__HResults.COR_E_CUSTOMATTRIBUTEFORMAT);
        }

        protected CustomAttributeFormatException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

    }
}
