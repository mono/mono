//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.InfoCards
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.InfoCards.Diagnostics;

    //
    // This exception indicates that the user has explicitly chosen to cancel
    // the ongoing request.
    //
    internal class UserCancelledException : InfoCardBaseException
    {
        //
        // This the code that this exception translates into.
        //
        const int HRESULT = (int)EventCode.E_ICARD_USERCANCELLED;

        public UserCancelledException()
            : base(HRESULT)
        {
        }
        public UserCancelledException(string message)
            : base(HRESULT, message)
        {
        }


        public UserCancelledException(string message, Exception inner)
            : base(HRESULT, message, inner)
        {
        }

        protected UserCancelledException(SerializationInfo si, StreamingContext sc)
            : base(HRESULT, si, sc)
        {
        }


    }
}
