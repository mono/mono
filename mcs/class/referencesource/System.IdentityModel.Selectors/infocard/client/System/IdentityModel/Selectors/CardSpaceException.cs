//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.InfoCards.Diagnostics;
    using IDT = Microsoft.InfoCards.Diagnostics.InfoCardTrace;
    using Microsoft.InfoCards;
    internal static class ExceptionHelper
    {
        public static void ThrowIfCardSpaceException(int status)
        {
            switch (status)
            {
                case (int)EventCode.E_ICARD_COMMUNICATION:
                    throw IDT.ThrowHelperError(new CardSpaceException(SR.GetString(SR.ClientAPIInfocardError)));

                case (int)EventCode.E_ICARD_USERCANCELLED:
                    throw IDT.ThrowHelperError(new UserCancellationException(SR.GetString(SR.ClientAPIUserCancellationError)));

                case (int)EventCode.E_ICARD_SERVICE:
                    throw IDT.ThrowHelperError(new ServiceNotStartedException(SR.GetString(SR.ClientAPIServiceNotStartedError)));

                case (int)EventCode.E_ICARD_UNTRUSTED:
                    throw IDT.ThrowHelperError(new UntrustedRecipientException(SR.GetString(SR.ClientAPIUntrustedRecipientError)));

                case (int)EventCode.E_ICARD_TRUSTEXCHANGE:
                    throw IDT.ThrowHelperError(new StsCommunicationException(SR.GetString(SR.ClientStsCommunicationException)));

                case (int)EventCode.E_ICARD_IDENTITY:
                    throw IDT.ThrowHelperError(new IdentityValidationException(SR.GetString(SR.ClientAPIInvalidIdentity)));

                case (int)EventCode.E_ICARD_SERVICEBUSY:
                    throw IDT.ThrowHelperError(new ServiceBusyException(SR.GetString(SR.ClientAPIServiceBusy)));

                case (int)EventCode.E_ICARD_POLICY:
                    throw IDT.ThrowHelperError(new PolicyValidationException(SR.GetString(SR.ClientAPIInvalidPolicy)));

                case (int)EventCode.E_ICARD_UNSUPPORTED:
                    throw IDT.ThrowHelperError(new UnsupportedPolicyOptionsException(SR.GetString(SR.ClientAPIUnsupportedPolicyOptions)));

                case (int)EventCode.E_ICARD_UI_INITIALIZATION:
                    throw IDT.ThrowHelperError(new UIInitializationException(SR.GetString(SR.ClientAPIUIInitializationFailed)));

                case (int)EventCode.E_ICARD_IMPORT:
                    throw IDT.ThrowHelperError(new CardSpaceException(SR.GetString(SR.ClientAPICannotImport)));

                default:
                    //
                    // In current implementation, caller will determine what to do in the default case.
                    //
                    break;
            }
        }
    }
    //
    // Summary
    //  Generic Infocard Exception class used to indicate failures in teh Infocard system
    //
    [Serializable]
    public class CardSpaceException : System.Exception
    {
        public CardSpaceException()
            : base()
        {
        }

        public CardSpaceException(string message)
            : base(message)
        {
        }

        public CardSpaceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CardSpaceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
