//------------------------------------------------------------------------------
// <copyright file="SoapException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="Soap12FaultCodes"]/*' />
    public sealed class Soap12FaultCodes {

        private Soap12FaultCodes() {
        }

        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="Soap12FaultCodes.ReceiverFaultCode"]/*' />
        public static readonly XmlQualifiedName ReceiverFaultCode = new XmlQualifiedName(Soap12.Code.Receiver, Soap12.Namespace);
        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="Soap12FaultCodes.SenderFaultCode"]/*' />
        public static readonly XmlQualifiedName SenderFaultCode = new XmlQualifiedName(Soap12.Code.Sender, Soap12.Namespace);
        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="Soap12FaultCodes.VersionMismatchFaultCode"]/*' />
        public static readonly XmlQualifiedName VersionMismatchFaultCode = new XmlQualifiedName(Soap12.Code.VersionMismatch, Soap12.Namespace);
        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="Soap12FaultCodes.MustUnderstandFaultCode"]/*' />
        public static readonly XmlQualifiedName MustUnderstandFaultCode = new XmlQualifiedName(Soap12.Code.MustUnderstand, Soap12.Namespace);
        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="Soap12FaultCodes.DataEncodingUnknownFaultCode"]/*' />
        public static readonly XmlQualifiedName DataEncodingUnknownFaultCode = new XmlQualifiedName(Soap12.Code.DataEncodingUnknown, Soap12.Namespace);

        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="Soap12FaultCodes.RpcProcedureNotPresentFaultCode"]/*' />
        public static readonly XmlQualifiedName RpcProcedureNotPresentFaultCode = new XmlQualifiedName(Soap12.Code.RpcProcedureNotPresentSubcode, Soap12.RpcNamespace);
        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="Soap12FaultCodes.RpcBadArgumentsFaultCode"]/*' />
        public static readonly XmlQualifiedName RpcBadArgumentsFaultCode = new XmlQualifiedName(Soap12.Code.RpcBadArgumentsSubcode, Soap12.RpcNamespace);
    
        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="Soap12FaultCodes.EncodingMissingIdFaultCode"]/*' />
        public static readonly XmlQualifiedName EncodingMissingIdFaultCode = new XmlQualifiedName(Soap12.Code.EncodingMissingIDFaultSubcode, Soap12.Encoding);
        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="Soap12FaultCodes.EncodingUntypedValueFaultCode"]/*' />
        public static readonly XmlQualifiedName EncodingUntypedValueFaultCode = new XmlQualifiedName(Soap12.Code.EncodingUntypedValueFaultSubcode, Soap12.Encoding);
    
        internal static readonly XmlQualifiedName UnsupportedMediaTypeFaultCode = new XmlQualifiedName("UnsupportedMediaType", "http://microsoft.com/soap/");
        internal static readonly XmlQualifiedName MethodNotAllowed = new XmlQualifiedName("MethodNotAllowed", "http://microsoft.com/soap/");
    }

    /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="SoapFaultSubCode"]/*' />
    [Serializable]
    public class SoapFaultSubCode {
        XmlQualifiedName code;
        SoapFaultSubCode subCode;

        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="SoapFaultSubCode.SoapFaultSubCode"]/*' />
        public SoapFaultSubCode(XmlQualifiedName code) : this(code, null) {
        }

        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="SoapFaultSubCode.SoapFaultSubCode1"]/*' />
        public SoapFaultSubCode(XmlQualifiedName code, SoapFaultSubCode subCode) {
            this.code = code;
            this.subCode = subCode;
        }

        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="SoapFaultSubCode.Code"]/*' />
        public XmlQualifiedName Code {
            get { return code; }
        }

        /// <include file='doc\SoapFaultCodes.uex' path='docs/doc[@for="SoapFaultSubCode.Subcode"]/*' />
        public SoapFaultSubCode SubCode {
            get { return subCode; }
        }
    }
}





