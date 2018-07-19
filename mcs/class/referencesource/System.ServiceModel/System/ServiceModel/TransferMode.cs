//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ComponentModel;

    /// <summary>
    /// Modes that can be set in a to determine whether a channel 
    /// supports streamed and/or buffered mode.
    /// </summary>
    public enum TransferMode
    {
        Buffered,
        Streamed,
        StreamedRequest,
        StreamedResponse,
    }

    static class TransferModeHelper
    {
        public static bool IsDefined(TransferMode v)
        {
            return ((v == TransferMode.Buffered) || (v == TransferMode.Streamed) ||
                (v == TransferMode.StreamedRequest) || (v == TransferMode.StreamedResponse));
        }

        public static bool IsRequestStreamed(TransferMode v)
        {
            return ((v == TransferMode.StreamedRequest) || (v == TransferMode.Streamed));
        }

        public static bool IsResponseStreamed(TransferMode v)
        {
            return ((v == TransferMode.StreamedResponse) || (v == TransferMode.Streamed));
        }

        public static void Validate(TransferMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(TransferMode)));
            }
        }
    }
}


