//------------------------------------------------------------------------------
// <copyright file="SoapException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException"]/*' />
    /// <devdoc>
    ///    <para>SoapException is the only mechanism for raising exceptions when a Web
    ///       Method is called over SOAP. A SoapException can either be generated
    ///       by the .Net Runtime or by a Web Service Method.
    ///       The .Net Runtime can generate an SoapException, if a response to a request
    ///       that is malformed. A Web Service Method can generate a SoapException by simply
    ///       generating an Exception within the Web Service Method, if the client accessed the method
    ///       over SOAP. Any time a Web Service Method throws an exception, that exception
    ///       is caught on the server and wrapped inside a new SoapException.</para>
    /// </devdoc>
    [Serializable]
    public class SoapException : SystemException {
        XmlQualifiedName code = XmlQualifiedName.Empty;
        string actor;
        string role;
        System.Xml.XmlNode detail;
        SoapFaultSubCode subCode;
        string lang;


        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.ServerFaultCode"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly XmlQualifiedName ServerFaultCode = new XmlQualifiedName(Soap.Code.Server, Soap.Namespace);
        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.ClientFaultCode"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly XmlQualifiedName ClientFaultCode = new XmlQualifiedName(Soap.Code.Client, Soap.Namespace);
        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.VersionMismatchFaultCode"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly XmlQualifiedName VersionMismatchFaultCode = new XmlQualifiedName(Soap.Code.VersionMismatch, Soap.Namespace);
        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.MustUnderstandFaultCode"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly XmlQualifiedName MustUnderstandFaultCode = new XmlQualifiedName(Soap.Code.MustUnderstand, Soap.Namespace);
        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.DetailElementName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        // NOTE, [....]: The SOAP 1.1 is unclear on whether the detail element can or should be qualified.
        // Based on consensus about the intent, we will not qualify it.
        public static readonly XmlQualifiedName DetailElementName = new XmlQualifiedName(Soap.Element.FaultDetail, "");

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.IsServerFaultCode"]/*' />
        public static bool IsServerFaultCode(XmlQualifiedName code) {
            return code == ServerFaultCode || code == Soap12FaultCodes.ReceiverFaultCode;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.IsClientFaultCode"]/*' />
        public static bool IsClientFaultCode(XmlQualifiedName code) {
            return code == ClientFaultCode || code == Soap12FaultCodes.SenderFaultCode;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.IsVersionMismatchFaultCode"]/*' />
        public static bool IsVersionMismatchFaultCode(XmlQualifiedName code) {
            return code == VersionMismatchFaultCode || code == Soap12FaultCodes.VersionMismatchFaultCode;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.IsMustUnderstandFaultCode"]/*' />
        public static bool IsMustUnderstandFaultCode(XmlQualifiedName code) {
            return code == MustUnderstandFaultCode || code == Soap12FaultCodes.MustUnderstandFaultCode;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SoapException9"]/*' />
        public SoapException() : base(null) {
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SoapException"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.Protocols.SoapException'/> class, setting <see cref='System.Exception.Message'/> to <paramref name="message"/>, <see cref='System.Web.Services.Protocols.SoapException.Code'/> to
        /// <paramref name="code"/> and <see cref='System.Web.Services.Protocols.SoapException.Actor'/> to <paramref name="actor"/>.</para>
        /// </devdoc>
        public SoapException(string message, XmlQualifiedName code, string actor) : base(message) { 
            this.code = code;
            this.actor = actor;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SoapException1"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.Protocols.SoapException'/> class, setting <see cref='System.Exception.Message'/> to 
        /// <paramref name="message"/>, <see cref='System.Web.Services.Protocols.SoapException.Code'/> to <paramref name="code, 
        ///    "/><see cref='System.Web.Services.Protocols.SoapException.Actor'/> to <paramref name="actor
        ///    "/>and <see cref='System.Exception.InnerException'/> to <paramref name="innerException"/> .</para>
        /// </devdoc>
        public SoapException(string message, XmlQualifiedName code, string actor, Exception innerException) : base(message, innerException) { 
            this.code = code;
            this.actor = actor;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SoapException2"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.Protocols.SoapException'/> class, setting <see cref='System.Exception.Message'/> to
        /// <paramref name="message "/>and<paramref name=" "/>
        /// <see cref='System.Web.Services.Protocols.SoapException.Code'/> 
        /// to <paramref name="code"/>.</para>
        /// </devdoc>
        public SoapException(string message, XmlQualifiedName code) : base(message) { 
            this.code = code;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SoapException3"]/*' />
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.Services.Protocols.SoapException'/> class, setting <see cref='System.Exception.Message'/> to
        /// <paramref name="message"/>, <see cref='System.Web.Services.Protocols.SoapException.Code'/> to <paramref name="code "/>and 
        /// <see cref='System.Exception.InnerException'/> 
        /// to <paramref name="innerException"/>.</para>
        /// </devdoc>
        public SoapException(string message, XmlQualifiedName code, Exception innerException) : base(message, innerException) { 
            this.code = code;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SoapException4"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapException(string message, XmlQualifiedName code, string actor, System.Xml.XmlNode detail) : base(message) {
            this.code = code;
            this.actor = actor;
            this.detail = detail;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SoapException5"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapException(string message, XmlQualifiedName code, string actor, System.Xml.XmlNode detail, Exception innerException) : base(message, innerException) {
            this.code = code;
            this.actor = actor;
            this.detail = detail;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SoapException6"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapException(string message, XmlQualifiedName code, SoapFaultSubCode subCode) : base(message) {
            this.code = code;
            this.subCode = subCode;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SoapException7"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapException(string message, XmlQualifiedName code, string actor, string role, System.Xml.XmlNode detail, SoapFaultSubCode subCode, Exception innerException) : base(message, innerException) {
            this.code = code;
            this.actor = actor;
            this.role = role;
            this.detail = detail;
            this.subCode = subCode;
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SoapException8"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapException(string message, XmlQualifiedName code, string actor, string role, string lang, System.Xml.XmlNode detail, SoapFaultSubCode subCode, Exception innerException) : base(message, innerException) {
            this.code = code;
            this.actor = actor;
            this.role = role;
            this.detail = detail;
            this.lang = lang;
            this.subCode = subCode;
        }

        protected SoapException(SerializationInfo info, StreamingContext context) : base(info, context) {
            IDictionary list = base.Data;
            code = (XmlQualifiedName)list["code"];
            actor = (string)list["actor"];
            role = (string)list["role"];
            
            // Bug: 323493: XmlNode is not serializable, and I don't think we want to really want to create
            // an XmlDocument just to read a XmlNode from string to get the deserialized instance back.
            // detail = (XmlNode)list["detail"];

            subCode = (SoapFaultSubCode)list["subCode"];
            lang = (string)list["lang"];
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.Actor"]/*' />
        /// <devdoc>
        ///    The piece of code that caused the exception.
        ///    Typically, an URL to a Web Service Method.
        /// </devdoc>
        public string Actor {
            get { return actor == null ? string.Empty : actor; }
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.Code"]/*' />
        /// <devdoc>
        ///    <para>The type of error that occurred.</para>
        /// </devdoc>
        public XmlQualifiedName Code {    
            get { return code; }
        }

        // the <soap:detail> element. If null, the <detail> element was not present in the <fault> element.
        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.Detail"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public System.Xml.XmlNode Detail {
            get {
                return detail;
            }
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.Lang"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ComVisible(false)]
        public string Lang {
            get { return lang == null ? string.Empty : lang; }
        }

        // this is semantically the same as Actor so we use the same field but we offer a second property
        // in case the user is thinking in soap 1.2
        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.Node"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ComVisible(false)]
        public string Node {
            get { return actor == null ? string.Empty : actor; }
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.Role"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ComVisible(false)]
        public string Role {
            get { return role == null ? string.Empty : role; }
        }

        /// <include file='doc\SoapException.uex' path='docs/doc[@for="SoapException.SubCode"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ComVisible(false)]
        public SoapFaultSubCode SubCode {
            get {
                return subCode; 
            }
        }

        // helper function that allows us to pass dummy subCodes around but clear them before they get to the user
        internal void ClearSubCode() {
            if (subCode != null)
                subCode = subCode.SubCode;
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            IDictionary list = Data;
            list["code"] = Code;
            list["actor"] = Actor;
            list["role"] = Role;
            
            // Bug: 323493: XmlNode is not serializable, and I don't think we want to really want to create
            // an XmlDocument just to read a XmlNode from string to get the deserialized instance back.
            // list["detail"] = Detail;
            
            list["subCode"] = SubCode;
            list["lang"] = Lang;

            base.GetObjectData(info, context);
        }

        static SoapException CreateSuppressedException(SoapProtocolVersion soapVersion, string message, Exception innerException) {
            return new SoapException(Res.GetString(Res.WebSuppressedExceptionMessage), 
                soapVersion == SoapProtocolVersion.Soap12 
                ? new XmlQualifiedName(Soap12.Code.Receiver, Soap12.Namespace) 
                : new XmlQualifiedName(Soap.Code.Server, Soap.Namespace));
        }

        internal static SoapException Create(SoapProtocolVersion soapVersion, string message, XmlQualifiedName code, 
                                                       string actor, string role, System.Xml.XmlNode detail,
                                                       SoapFaultSubCode subCode, Exception innerException) {
            if (System.Web.Services.Configuration.WebServicesSection.Current.Diagnostics.SuppressReturningExceptions) {
                return CreateSuppressedException(soapVersion, Res.GetString(Res.WebSuppressedExceptionMessage), innerException);
            }
            else {
                return new SoapException(message, code, actor, role, detail, subCode, innerException);
            }
        }
        internal static SoapException Create(SoapProtocolVersion soapVersion, string message, XmlQualifiedName code, Exception innerException) { 
            if (System.Web.Services.Configuration.WebServicesSection.Current.Diagnostics.SuppressReturningExceptions) {
                return CreateSuppressedException(soapVersion, Res.GetString(Res.WebSuppressedExceptionMessage), innerException);
            }
            else {
                return new SoapException(message, code, innerException);
            }
        }
    }
}

