//------------------------------------------------------------------------------
// <copyright file="WebResourceAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Util;


    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class WebResourceAttribute : Attribute {

        private string _contentType;
        private bool _performSubstitution;
        private string _webResource;
        private string _cdnPath;
        private string _cdnPathSecureConnection;
        private bool _cdnSupportsSecureConnection;

        internal const string _microsoftCdnBasePath = "http://ajax.aspnetcdn.com/ajax/4.6/1/";

        public WebResourceAttribute(string webResource, string contentType) {
            if (String.IsNullOrEmpty(webResource)) {
                throw ExceptionUtil.ParameterNullOrEmpty("webResource");
            }

            if (String.IsNullOrEmpty(contentType)) {
                throw ExceptionUtil.ParameterNullOrEmpty("contentType");
            }

            _contentType = contentType;
            _webResource = webResource;
            _performSubstitution = false;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Cdn", Justification="Stands for Content Delivery Network.")]
        public string CdnPath {
            get {
                return _cdnPath ?? String.Empty;
            }
            set {
                _cdnPath = value;
            }
        }

        public string LoadSuccessExpression {
            get;
            set;
        }

        internal string CdnPathSecureConnection {
            get {
                if (_cdnPathSecureConnection == null) {
                    string cdnPath = CdnPath;
                    if (String.IsNullOrEmpty(cdnPath) || !CdnSupportsSecureConnection || !cdnPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) {
                        cdnPath = String.Empty;
                    }
                    else {
                        // convert http to https
                        cdnPath = "https" + cdnPath.Substring(4);
                    }
                    _cdnPathSecureConnection = cdnPath;
                }
                return _cdnPathSecureConnection;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cdn", Justification = "Stands for Content Delivery Network.")]
        public bool CdnSupportsSecureConnection {
            get {
                return _cdnSupportsSecureConnection;
            }
            set {
                _cdnSupportsSecureConnection = value;
            }
        }

        public string ContentType {
            get {
                return _contentType;
            }
        }


        public bool PerformSubstitution {
            get {
                return _performSubstitution;
            }
            set {
                _performSubstitution = value;
            }
        }


        public string WebResource {
            get {
                return _webResource;
            }
        }
    }
}


