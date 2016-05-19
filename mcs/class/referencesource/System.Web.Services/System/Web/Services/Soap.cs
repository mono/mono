//------------------------------------------------------------------------------
// <copyright file="Soap.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services {
    using System;
    using System.Xml;

    internal class Soap {
        private Soap() { }
        internal const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        internal const string Encoding = "http://schemas.xmlsoap.org/soap/encoding/";
        internal const string Namespace = "http://schemas.xmlsoap.org/soap/envelope/";
        internal const string ConformanceClaim = "http://ws-i.org/schemas/conformanceClaim/";
        internal const string BasicProfile1_1 = "http://ws-i.org/profiles/basic/1.1";

        internal const string Action = "SOAPAction";
        internal const string ArrayType = "Array";
        internal const string Prefix = "soap";
        internal const string ClaimPrefix = "wsi";
        internal const string DimeContentType = "application/dime";
        internal const string SoapContentType = "text/xml";

        internal class Attribute {
            private Attribute() { }
            internal const string MustUnderstand = "mustUnderstand";
            internal const string Actor = "actor";
            internal const string EncodingStyle = "encodingStyle";
            internal const string Lang = "lang";
            internal const string ConformsTo = "conformsTo";
        }

        internal class Element {
            private Element() { }
            internal const string Envelope = "Envelope";
            internal const string Header = "Header";
            internal const string Body = "Body";
            internal const string Fault = "Fault";
            internal const string FaultActor = "faultactor";
            internal const string FaultCode = "faultcode";
            internal const string FaultDetail = "detail";
            internal const string FaultString = "faultstring";
            internal const string StackTrace = "StackTrace";
            internal const string Message = "Message";
            internal const string Claim = "Claim";
        }

        internal class Code {
            private Code() { }
            internal const string Server = "Server";
            internal const string VersionMismatch = "VersionMismatch";
            internal const string MustUnderstand = "MustUnderstand";
            internal const string Client = "Client";
        }
    }

    internal sealed class Soap12 {
        private Soap12() { }
        internal const string Namespace = "http://www.w3.org/2003/05/soap-envelope";
        internal const string Encoding = "http://www.w3.org/2003/05/soap-encoding";
        internal const string RpcNamespace = "http://www.w3.org/2003/05/soap-rpc";
        internal const string Prefix = "soap12";
        internal class Attribute {
            private Attribute() { }
            internal const string UpgradeEnvelopeQname = "qname";
            internal const string Role = "role";
            internal const string Relay = "relay";
        }

        internal sealed class Element {
            private Element() { }
            internal const string Upgrade = "Upgrade";
            internal const string UpgradeEnvelope = "SupportedEnvelope";
            internal const string FaultRole = "Role";
            internal const string FaultReason = "Reason";
            internal const string FaultReasonText = "Text";
            internal const string FaultCode = "Code";
            internal const string FaultNode = "Node";
            internal const string FaultCodeValue = "Value";
            internal const string FaultSubcode = "Subcode";
            internal const string FaultDetail = "Detail";
        }

        internal sealed class Code {
            private Code() { }
            internal const string VersionMismatch = "VersionMismatch";
            internal const string MustUnderstand = "MustUnderstand";
            internal const string DataEncodingUnknown = "DataEncodingUnknown";
            internal const string Sender = "Sender";
            internal const string Receiver = "Receiver";

            // Subcodes
            internal const string RpcProcedureNotPresentSubcode = "ProcedureNotPresent";
            internal const string RpcBadArgumentsSubcode = "BadArguments";
            internal const string EncodingMissingIDFaultSubcode = "MissingID";
            internal const string EncodingUntypedValueFaultSubcode = "UntypedValue";
        }
    }
}

