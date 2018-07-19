//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Channels;

    public enum BasicHttpsSecurityMode
    {
        Transport,
        TransportWithMessageCredential
    }

    static class BasicHttpsSecurityModeHelper
    {
        internal static bool IsDefined(BasicHttpsSecurityMode value)
        {
            return value == BasicHttpsSecurityMode.Transport ||
                value == BasicHttpsSecurityMode.TransportWithMessageCredential;
        }

        internal static BasicHttpsSecurityMode ToSecurityMode(UnifiedSecurityMode value)
        {
            switch (value)
            {
                case UnifiedSecurityMode.Transport:
                    return BasicHttpsSecurityMode.Transport;
                case UnifiedSecurityMode.TransportWithMessageCredential:
                    return BasicHttpsSecurityMode.TransportWithMessageCredential;
                default:
                    return (BasicHttpsSecurityMode)value;
            }
        }

        internal static BasicHttpsSecurityMode ToBasicHttpsSecurityMode(BasicHttpSecurityMode mode)
        {
            Fx.Assert(mode == BasicHttpSecurityMode.Transport || mode == BasicHttpSecurityMode.TransportWithMessageCredential, string.Format(CultureInfo.InvariantCulture, "Invalid BasicHttpSecurityMode value: {0}.", mode.ToString()));
            BasicHttpsSecurityMode basicHttpsSecurityMode = (mode == BasicHttpSecurityMode.Transport) ? BasicHttpsSecurityMode.Transport : BasicHttpsSecurityMode.TransportWithMessageCredential;

            return basicHttpsSecurityMode;
        }

        internal static BasicHttpSecurityMode ToBasicHttpSecurityMode(BasicHttpsSecurityMode mode)
        {
            if (!BasicHttpsSecurityModeHelper.IsDefined(mode))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("mode"));
            }

            BasicHttpSecurityMode basicHttpSecurityMode = (mode == BasicHttpsSecurityMode.Transport) ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.TransportWithMessageCredential;

            return basicHttpSecurityMode;
        }
    }
}
