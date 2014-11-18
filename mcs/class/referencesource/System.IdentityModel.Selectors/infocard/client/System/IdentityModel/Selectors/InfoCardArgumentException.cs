//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.InfoCards
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.InfoCards.Diagnostics;

    //
    // Indicates an incorrect argument was passed to the system.
    // 

    internal class InfoCardArgumentException : InfoCardBaseException
    {
        //
        // This the code that this exception translates into.
        //
        const int HRESULT = (int)EventCode.E_ICARD_ARGUMENT;

        public InfoCardArgumentException()
            : base(HRESULT)
        {
        }
        public InfoCardArgumentException(string message)
            : base(HRESULT, message)
        {
        }


        public InfoCardArgumentException(string message, Exception inner)
            : base(HRESULT, message, inner)
        {
        }

        protected InfoCardArgumentException(SerializationInfo si, StreamingContext sc)
            : base(HRESULT, si, sc)
        {
        }


    }
}
