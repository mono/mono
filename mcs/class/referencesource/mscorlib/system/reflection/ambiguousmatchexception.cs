// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// AmbiguousMatchException is thrown when binding to a method results in more
// 
// <OWNER>[....]</OWNER>
//    than one method matching the binding criteria.  This exception is thrown in
//    general when something is Ambiguous.
//
//  
//  
//
namespace System.Reflection {
    using System;
    using SystemException = System.SystemException;
    using System.Runtime.Serialization;
    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class AmbiguousMatchException : SystemException
    {
        
        public AmbiguousMatchException() 
            : base(Environment.GetResourceString("RFLCT.Ambiguous")) {
            SetErrorCode(__HResults.COR_E_AMBIGUOUSMATCH);
        }
    
        public AmbiguousMatchException(String message) : base(message) {
            SetErrorCode(__HResults.COR_E_AMBIGUOUSMATCH);
        }
        
        public AmbiguousMatchException(String message, Exception inner)  : base(message, inner) {
            SetErrorCode(__HResults.COR_E_AMBIGUOUSMATCH);
        }

        internal AmbiguousMatchException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

    }
}
