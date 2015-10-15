//------------------------------------------------------------------------------
// <copyright file="HttpRequestBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web.Routing;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class HttpRequestBase {
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Matches HttpRequest class")]
        public virtual String[] AcceptTypes {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String ApplicationPath {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId="ID")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        public virtual String AnonymousID {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String AppRelativeCurrentExecutionFilePath {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpBrowserCapabilitiesBase Browser {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual ChannelBinding HttpChannelBinding {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpClientCertificate ClientCertificate {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Encoding ContentEncoding {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual int ContentLength {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String ContentType {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual HttpCookieCollection Cookies {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String CurrentExecutionFilePath {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string CurrentExecutionFilePathExtension {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String FilePath {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpFileCollectionBase Files {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Stream Filter {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual NameValueCollection Form {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String HttpMethod {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Stream InputStream {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsAuthenticated {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsLocal {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsSecureConnection {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpRequest class")]
        public virtual WindowsIdentity LogonUserIdentity {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpRequest class")]
        public virtual NameValueCollection Params {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String Path {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String PathInfo {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String PhysicalApplicationPath {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String PhysicalPath {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings",
            Justification = "Matches HttpRequest class")]
        public virtual String RawUrl {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual ReadEntityBodyMode ReadEntityBodyMode {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual RequestContext RequestContext {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual String RequestType {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual NameValueCollection ServerVariables {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual CancellationToken TimedOutToken {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual ITlsTokenBindingInfo TlsTokenBindingInfo {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int TotalBytes {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual UnvalidatedRequestValuesBase Unvalidated {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Uri Url {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Uri UrlReferrer {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String UserAgent {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Matches HttpRequest class")]
        public virtual String[] UserLanguages {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String UserHostAddress {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String UserHostName {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual NameValueCollection Headers {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual NameValueCollection QueryString {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String this[String key] {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual void Abort() {
            throw new NotImplementedException();
        }

        public virtual byte[] BinaryRead(int count) {
            throw new NotImplementedException();
        }

        public virtual Stream GetBufferedInputStream() {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Bufferless", Justification = "Inline with HttpRequest.GetBufferlessInputStream")]
        public virtual Stream GetBufferlessInputStream() {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Bufferless", Justification = "Inline with HttpRequest.GetBufferlessInputStream")]
        public virtual Stream GetBufferlessInputStream(bool disableMaxRequestLength) {
            throw new NotImplementedException();
        }

        public virtual void InsertEntityBody() {
            throw new NotImplementedException();
        }

        public virtual void InsertEntityBody(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public virtual int[] MapImageCoordinates(String imageFieldName) {
            throw new NotImplementedException();
        }

        public virtual double[] MapRawImageCoordinates(String imageFieldName) {
            throw new NotImplementedException();
        }

        public virtual String MapPath(String virtualPath) {
            throw new NotImplementedException();
        }

        public virtual String MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping) {
            throw new NotImplementedException();
        }

        public virtual void ValidateInput() {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpRequest class")]
        public virtual void SaveAs(String filename, bool includeHeaders) {
            throw new NotImplementedException();
        }
    }
}
