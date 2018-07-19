//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Administration
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    internal class WbemException : Win32Exception
    {
        internal WbemException(WbemNative.WbemStatus hr)
            : base((int)hr)
        {
        }

        internal WbemException(int hr)
            : base(hr)
        {
        }

        internal WbemException(int hr, string message)
            : base(hr, message)
        {
        }

        internal static void Throw(WbemNative.WbemStatus hr)
        {
            switch (hr)
            {
                case WbemNative.WbemStatus.WBEM_E_NOT_FOUND:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemInstanceNotFoundException());
                case WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemInvalidParameterException());
                case WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemNotSupportedException());
                case WbemNative.WbemStatus.WBEM_E_INVALID_METHOD:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemInvalidMethodException());
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemException(hr));
            }
        }

        internal static void ThrowIfFail(int hr)
        {
            if (hr < 0)
            {
                Throw((WbemNative.WbemStatus)hr);
            }
        }
    }

    internal class WbemInstanceNotFoundException : WbemException
    {
        internal WbemInstanceNotFoundException()
            : base(WbemNative.WbemStatus.WBEM_E_NOT_FOUND)
        {
        }
    }

    internal class WbemInvalidParameterException : WbemException
    {
        internal WbemInvalidParameterException(string name)
            : base((int)WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER, name)
        {
        }

        internal WbemInvalidParameterException()
            : base(WbemNative.WbemStatus.WBEM_E_INVALID_PARAMETER)
        {
        }
    }

    internal class WbemNotSupportedException : WbemException
    {
        internal WbemNotSupportedException()
            : base(WbemNative.WbemStatus.WBEM_E_NOT_SUPPORTED)
        {
        }
    }

    internal class WbemInvalidMethodException : WbemException
    {
        internal WbemInvalidMethodException()
            : base(WbemNative.WbemStatus.WBEM_E_INVALID_METHOD)
        {
        }
    }
}
