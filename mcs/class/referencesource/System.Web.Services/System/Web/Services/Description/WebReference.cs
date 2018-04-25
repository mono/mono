//------------------------------------------------------------------------------
// <copyright file="WebReference.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System;
    using System.Net;
    using System.Web.Services.Description;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using System.Web.Services.Protocols;
    using System.Text;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Threading;
    using System.CodeDom;
    using System.Web.Services.Discovery;

    /// <include file='doc\WebReference.uex' path='docs/doc[@for="WebReference"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class WebReference {
        CodeNamespace proxyCode;
        DiscoveryClientDocumentCollection documents;
        string appSettingUrlKey;
        string appSettingBaseUrl;
        string protocolName;
        ServiceDescriptionImportWarnings warnings;
        StringCollection validationWarnings;

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="WebReference.WebReference"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebReference(DiscoveryClientDocumentCollection documents, CodeNamespace proxyCode, string protocolName, string appSettingUrlKey, string appSettingBaseUrl) {
            // parameter check
            if (documents == null) {
                throw new ArgumentNullException("documents");
            }
            if (proxyCode == null) {
                // no namespace
                throw new ArgumentNullException("proxyCode");
            }
            if (appSettingBaseUrl != null && appSettingUrlKey == null) {
                throw new ArgumentNullException("appSettingUrlKey");
            }
            this.protocolName = protocolName;
            this.appSettingUrlKey = appSettingUrlKey;
            this.appSettingBaseUrl = appSettingBaseUrl;
            this.documents = documents;
            this.proxyCode = proxyCode;
        }

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="WebReference.WebReference2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebReference(DiscoveryClientDocumentCollection documents, CodeNamespace proxyCode) : this(documents, proxyCode, null, null, null) {
        }

        /// <include file='doc\ServiceDescriptionImporter.uex' path='docs/doc[@for="WebReference.WebReference3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public WebReference(DiscoveryClientDocumentCollection documents, CodeNamespace proxyCode, string appSettingUrlKey, string appSettingBaseUrl) 
            : this(documents, proxyCode, null, appSettingUrlKey, appSettingBaseUrl) {
        }

        /// <include file='doc\WebReference.uex' path='docs/doc[@for="WebReference.AppSettingBaseUrl"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string AppSettingBaseUrl {
            get { return appSettingBaseUrl; }
        }

        /// <include file='doc\WebReference.uex' path='docs/doc[@for="WebReference.AppSettingUrlKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string AppSettingUrlKey {
            get { return appSettingUrlKey; }
        }

        /// <include file='doc\WebReference.uex' path='docs/doc[@for="WebReference.Documents"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DiscoveryClientDocumentCollection Documents {
            get {
                return documents;
            }
        }

        /// <include file='doc\WebReference.uex' path='docs/doc[@for="WebReference.CodeNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeNamespace ProxyCode 
        {
            get { return proxyCode; }
        }

        /// <include file='doc\WebReference.uex' path='docs/doc[@for="WebReference.ValidationWarnings"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public StringCollection ValidationWarnings 
        {
            get 
            {
                if (validationWarnings == null) 
                {
                    validationWarnings = new StringCollection();
                }
                return validationWarnings;
            }
        }

        /// <include file='doc\WebReference.uex' path='docs/doc[@for="WebReference.Warnings"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescriptionImportWarnings Warnings 
        {
            get { return warnings; }
            set { warnings = value; }
        }

        /// <include file='doc\WebReference.uex' path='docs/doc[@for="WebReference.ProtocolName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public String ProtocolName 
        {
            get { return protocolName == null ? string.Empty : protocolName; }
            set { protocolName = value; }
        }
    }
}
