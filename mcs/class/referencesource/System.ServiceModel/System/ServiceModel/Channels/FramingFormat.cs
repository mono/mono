//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    /*
        Message Framing BNF:

        protocol-stream-a = (singleton-unsized-stream-a | duplex-stream-a | simplex-stream-a | singleton-sized-stream-a)+
        protocol-stream-b = (singleton-unsized-stream-b | duplex-stream-b)+

        singleton-unsized-stream-a = version-record mode-record-type singleton-unsized-mode via-record encoding-record upgrade-request* preamble-end-record-type singleton-message end-record-type
        duplex-stream-a = version-record mode-record-type duplex-mode via-record encoding-record upgrade-request* preamble-end-record-type duplex-message* end-record-type
        simplex-stream-a = version-record mode-record-type simplex-mode via-record encoding-record simplex-message* end-record-type
        singleton-sized-stream-a = version-record mode-record-type singleton-sized-mode via-record encoding-record octets

        singleton-unsized-stream-b = upgrade-response* preamble-response singleton-message? end-record-type
        duplex-stream-b = upgrade-response* preamble-response duplex-message* (fault-message | end-record-type)

        singleton-message = unsized-message
        duplex-message = sized-message
        simplex-message = sized-message
        fault-message = fault-record-type mbint utf8-octets
        sized-message = sized-envelope-record-type mbint octets
        unsized-message = unsized-envelope-record-type (mbint octets)* octet(0x0)

        preamble-response = preamble-ack-record-type | fault-message
     
        upgrade-request = upgrade-request-record-type mbint utf8-octets octets
        upgrade-response = upgrade-response-record-type octets

        version-record = version-record-type major-version-number minor-version-number
        major-version-number = octet(0x1)
        minor-version-number = octet(0x0)

        encoding-record = known-encoding-record | extensible-encoding-record
        known-encoding-record = known-encoding-record-type known-encoding-type
        extensible-encoding-record = extensible-encoding-record-type mbint utf8-octets

        via-record = via-record-type mbint utf8-octets

        singleton-unsized-mode = octet(0x1)
        duplex-mode = octet(0x2)
        simplex-mode = octet(0x3)
        singleton-sized-mode = octet(0x4)

        known-encoding-type = text-encoding | binary-encoding | mtom-encoding
        binary-encoding = binary-sessionless-encoding | binary-session-encoding
        text-encoding = soap11-text-encoding | soap12-text-encoding
        soap11-text-encoding = soap11-utf8-encoding | soap11-utf16-encoding | soap11-unicodeFFFE-encoding
        soap12-text-encoding = soap12-utf8-encoding | soap12-utf16-encoding | soap12-unicodeFFFE-encoding

        soap11-utf8-encoding = octet(0x0)
        soap11-utf16-encoding = octet(0x1)
        soap11-unicodeFFFE-encoding = octet(0x2)
        soap12-utf8-encoding = octet(0x3)
        soap12-utf16-encoding = octet(0x4)
        soap12-unicodeFFFE-encoding = octet(0x5)
        mtom-encoding = octet(0x6)
        binary-sessionless-encoding = octet(0x7)
        binary-session-encoding = octet(0x8)

        version-record-type = octet(0x0)
        mode-record-type = octet(0x1)
        via-record-type = octet(0x2)
        known-encoding-record-type = octet(0x3)
        extensible-encoding-record-type = octet(0x4)
        unsized-envelope-record-type = octet(0x5)
        sized-envelope-record-type = octet(0x6)
        end-record-type = octet(0x7)
        fault-record-type = octet(0x8)
        upgrade-request-record-type = octet(0x9)
        upgrade-response-record-type = octet(0xA)
        preamble-ack-record-type = octet (0xB)
        preamble-end-record-type = octet (0xC)
    */

    enum FramingRecordType
    {
        Version = 0x0,
        Mode = 0x1,
        Via = 0x2,
        KnownEncoding = 0x3,
        ExtensibleEncoding = 0x4,
        UnsizedEnvelope = 0x5,
        SizedEnvelope = 0x6,
        End = 0x7,
        Fault = 0x8,
        UpgradeRequest = 0x9,
        UpgradeResponse = 0xA,
        PreambleAck = 0xB,
        PreambleEnd = 0xC,
    }

    enum FramingMode
    {
        Singleton = 0x1,
        Duplex = 0x2,
        Simplex = 0x3,
        SingletonSized = 0x4,
    }

    static class FramingUpgradeString
    {
        public const string SslOrTls = "application/ssl-tls";
        public const string Negotiate = "application/negotiate";
    }

    enum FramingEncodingType
    {
        Soap11Utf8 = 0x0,
        Soap11Utf16 = 0x1,
        Soap11Utf16FFFE = 0x2,
        Soap12Utf8 = 0x3,
        Soap12Utf16 = 0x4,
        Soap12Utf16FFFE = 0x5,
        MTOM = 0x6,
        Binary = 0x7,
        BinarySession = 0x8,
    }

    static class FramingEncodingString
    {
        public const string Soap11Utf8 = "text/xml; charset=utf-8";
        public const string Soap11Utf16 = "text/xml; charset=utf16";
        public const string Soap11Utf16FFFE = "text/xml; charset=unicodeFFFE";
        public const string Soap12Utf8 = "application/soap+xml; charset=utf-8";
        public const string Soap12Utf16 = "application/soap+xml; charset=utf16";
        public const string Soap12Utf16FFFE = "application/soap+xml; charset=unicodeFFFE";
        public const string MTOM = "multipart/related";
        public const string Binary = "application/soap+msbin1";
        public const string BinarySession = "application/soap+msbinsession1";
        public const string ExtendedBinaryGZip = Binary + "+gzip";
        public const string ExtendedBinarySessionGZip = BinarySession + "+gzip";
        public const string ExtendedBinaryDeflate = Binary + "+deflate";
        public const string ExtendedBinarySessionDeflate = BinarySession + "+deflate";
        public const string NamespaceUri = "http://schemas.microsoft.com/ws/2006/05/framing";
        const string FaultBaseUri = NamespaceUri + "/faults/";
        public const string ContentTypeInvalidFault = FaultBaseUri + "ContentTypeInvalid";
        public const string ContentTypeTooLongFault = FaultBaseUri + "ContentTypeTooLong";
        public const string ConnectionDispatchFailedFault = FaultBaseUri + "ConnectionDispatchFailed";
        public const string EndpointNotFoundFault = FaultBaseUri + "EndpointNotFound";
        public const string EndpointUnavailableFault = FaultBaseUri + "EndpointUnavailable";
        public const string MaxMessageSizeExceededFault = FaultBaseUri + "MaxMessageSizeExceededFault";
        public const string ServerTooBusyFault = FaultBaseUri + "ServerTooBusy";
        public const string ServiceActivationFailedFault = FaultBaseUri + "ServiceActivationFailed";
        public const string UnsupportedModeFault = FaultBaseUri + "UnsupportedMode";
        public const string UnsupportedVersionFault = FaultBaseUri + "UnsupportedVersion";
        public const string UpgradeInvalidFault = FaultBaseUri + "UpgradeInvalid";
        public const string ViaTooLongFault = FaultBaseUri + "ViaTooLong";

        const string ExceptionKey = "FramingEncodingString";
        public static bool TryGetFaultString(Exception exception, out string framingFault)
        {
            framingFault = null;
            if (exception.Data.Contains(FramingEncodingString.ExceptionKey))
            {
                framingFault = exception.Data[FramingEncodingString.ExceptionKey] as string;
                if (framingFault != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static void AddFaultString(Exception exception, string framingFault)
        {
            exception.Data[FramingEncodingString.ExceptionKey] = framingFault;
        }
    }

    static class FramingVersion
    {
        public const int Major = 0x1;
        public const int Minor = 0x0;
    }
}
