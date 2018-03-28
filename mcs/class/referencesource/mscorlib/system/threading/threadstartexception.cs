// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// <OWNER>Microsoft</OWNER>

namespace System.Threading 
{
    using System;
    using System.Runtime.Serialization;
    using System.Runtime.InteropServices;

    [Serializable]
    public sealed class ThreadStartException : SystemException 
    {
        private ThreadStartException() 
            : base(Environment.GetResourceString("Arg_ThreadStartException")) 
        {
            SetErrorCode(__HResults.COR_E_THREADSTART);
        }

        private ThreadStartException(Exception reason)
            : base(Environment.GetResourceString("Arg_ThreadStartException"), reason)
        {
            SetErrorCode(__HResults.COR_E_THREADSTART);
        }

        //required for serialization
        internal ThreadStartException(SerializationInfo info, StreamingContext context) 
            : base(info, context) 
        {
        }
   
    }
}


