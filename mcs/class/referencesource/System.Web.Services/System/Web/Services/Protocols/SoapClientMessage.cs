//------------------------------------------------------------------------------
// <copyright file="SoapClientMessage.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Web.Services;
    using System.Xml.Serialization;
    using System;
    using System.Reflection;
    using System.Collections;
    using System.IO;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    /// <include file='doc\SoapClientMessage.uex' path='docs/doc[@for="SoapClientMessage"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class SoapClientMessage : SoapMessage {
        SoapClientMethod method;
        SoapHttpClientProtocol protocol;
        string url;

        internal SoapExtension[] initializedExtensions;

        internal SoapClientMessage(SoapHttpClientProtocol protocol, SoapClientMethod method, string url) {
            this.method = method;
            this.protocol = protocol;
            this.url = url;
        }

        /*
        internal override bool IsInitialized {
            get { return true; }
        }
        */

        /// <include file='doc\SoapClientMessage.uex' path='docs/doc[@for="SoapClientMessage.OneWay"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool OneWay {
            get { return method.oneWay; }
        }

        /// <include file='doc\SoapClientMessage.uex' path='docs/doc[@for="SoapClientMessage.Client"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapHttpClientProtocol Client {
            get { return protocol; }
        }

        /// <include file='doc\SoapClientMessage.uex' path='docs/doc[@for="SoapClientMessage.MethodInfo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override LogicalMethodInfo MethodInfo {
            get { return method.methodInfo; }
        }
    
        /*
        internal override SoapReflectedExtension[] Extensions {
            get { return method.extensions; }
        }

        internal override object[] ExtensionInitializers {
            get { return method.extensionInitializers; }
        }
        */

        /// <include file='doc\SoapClientMessage.uex' path='docs/doc[@for="SoapClientMessage.Url"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string Url {
            get { return url; }
        }

        /// <include file='doc\SoapClientMessage.uex' path='docs/doc[@for="SoapClientMessage.Action"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string Action {
            get { return method.action; }
        }

        /// <include file='doc\SoapClientMessage.uex' path='docs/doc[@for="SoapClientMessage.SoapVersion"]/*' />
        [ComVisible(false)]
        public override SoapProtocolVersion SoapVersion {
            get { return protocol.SoapVersion == SoapProtocolVersion.Default ? SoapProtocolVersion.Soap11 : protocol.SoapVersion; }
        }

        internal SoapClientMethod Method {
            get { return method; }
        }

        /// <include file='doc\SoapClientMessage.uex' path='docs/doc[@for="SoapClientMessage.EnsureOutStage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void EnsureOutStage() {
            EnsureStage(SoapMessageStage.AfterDeserialize);
        }

        /// <include file='doc\SoapClientMessage.uex' path='docs/doc[@for="SoapClientMessage.EnsureInStage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void EnsureInStage() {
            EnsureStage(SoapMessageStage.BeforeSerialize);
        }
    }
}
