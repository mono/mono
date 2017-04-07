// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>Microsoft</OWNER>
/*=============================================================================
**
** Class: SynchronizationLockException
**
**
** Purpose: Wait(), Notify() or NotifyAll() was called from an unsynchronized
**          block of code.
**
**
=============================================================================*/

namespace System.Threading {

    using System;
    using System.Runtime.Serialization;
[System.Runtime.InteropServices.ComVisible(true)]
    [Serializable]
    public class SynchronizationLockException : SystemException {
        public SynchronizationLockException() 
            : base(Environment.GetResourceString("Arg_SynchronizationLockException")) {
            SetErrorCode(__HResults.COR_E_SYNCHRONIZATIONLOCK);
        }
    
        public SynchronizationLockException(String message) 
            : base(message) {
            SetErrorCode(__HResults.COR_E_SYNCHRONIZATIONLOCK);
        }
    
        public SynchronizationLockException(String message, Exception innerException) 
            : base(message, innerException) {
            SetErrorCode(__HResults.COR_E_SYNCHRONIZATIONLOCK);
        }

        protected SynchronizationLockException(SerializationInfo info, StreamingContext context) : base (info, context) {
        }
    }

}


