//------------------------------------------------------------------------------
// <copyright file="SoapServerMessage.cs" company="Microsoft">
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
    using System.Security.Permissions;
    using System.Runtime.InteropServices;

    /// <include file='doc\SoapServerMessage.uex' path='docs/doc[@for="SoapServerMessage"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class SoapServerMessage : SoapMessage {
        SoapServerProtocol protocol;
        internal SoapExtension[] highPriConfigExtensions;
        internal SoapExtension[] otherExtensions;
        internal SoapExtension[] allExtensions;

        internal SoapServerMessage(SoapServerProtocol protocol) {
            this.protocol = protocol;
        }

        /*
        internal override bool IsInitialized {
            get { return protocol.IsInitialized; }
        }
        */

        /*
        internal override SoapReflectedExtension[] Extensions {
            get { return protocol.ServerMethod.extensions; }
        }

        internal override object[] ExtensionInitializers {
            get { return protocol.ServerMethod.extensionInitializers; }
        }
        */

        /// <include file='doc\SoapServerMessage.uex' path='docs/doc[@for="SoapServerMessage.OneWay"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool OneWay {
            get { return protocol.ServerMethod.oneWay; }
        }

        /// <include file='doc\SoapServerMessage.uex' path='docs/doc[@for="SoapServerMessage.Url"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string Url {
            get { return Uri.EscapeUriString(protocol.Request.Url.ToString()).Replace("#", "%23"); }
        }

        /// <include file='doc\SoapServerMessage.uex' path='docs/doc[@for="SoapServerMessage.Action"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string Action {
            get { return protocol.ServerMethod.action; }
        }

        /// <include file='doc\SoapServerMessage.uex' path='docs/doc[@for="SoapServerMessage.SoapVersion"]/*' />
        [ComVisible(false)]
        public override SoapProtocolVersion SoapVersion {
            get { return protocol.Version; }
        }

        /// <include file='doc\SoapServerMessage.uex' path='docs/doc[@for="SoapServerMessage.Server"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object Server {
            get { EnsureStage(SoapMessageStage.AfterDeserialize | SoapMessageStage.BeforeSerialize); return protocol.Target; }
        }

        /// <include file='doc\SoapServerMessage.uex' path='docs/doc[@for="SoapServerMessage.MethodInfo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override LogicalMethodInfo MethodInfo {
            get { return protocol.MethodInfo; }
        }

        /// <include file='doc\SoapServerMessage.uex' path='docs/doc[@for="SoapServerMessage.EnsureOutStage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void EnsureOutStage() {
            EnsureStage(SoapMessageStage.BeforeSerialize);
        }

        /// <include file='doc\SoapServerMessage.uex' path='docs/doc[@for="SoapServerMessage.EnsureInStage"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void EnsureInStage() {
            EnsureStage(SoapMessageStage.AfterDeserialize);
        }
    }
}
