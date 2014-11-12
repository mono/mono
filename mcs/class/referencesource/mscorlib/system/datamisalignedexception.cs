// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
** Class: DataMisalignedException
**
** Purpose: The exception class for a misaligned access exception
**
=============================================================================*/

namespace System 
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class DataMisalignedException : SystemException 
    {
        public DataMisalignedException() 
            : base(Environment.GetResourceString("Arg_DataMisalignedException")) 
        {
            SetErrorCode(__HResults.COR_E_DATAMISALIGNED);
        }
    
        public DataMisalignedException(String message) 
            : base(message) 
        {
            SetErrorCode(__HResults.COR_E_DATAMISALIGNED);
        }

        public DataMisalignedException(String message, Exception innerException) 
            : base(message, innerException) 
        {
            SetErrorCode(__HResults.COR_E_DATAMISALIGNED);
        }

        internal DataMisalignedException(SerializationInfo info, StreamingContext context) 
            : base (info, context) 
        {
        }
    }

}
