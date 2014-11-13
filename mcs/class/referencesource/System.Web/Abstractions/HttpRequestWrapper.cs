//------------------------------------------------------------------------------
// <copyright file="HttpRequestWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web {
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web.Routing;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpRequestWrapper : HttpRequestBase {
        private HttpRequest _httpRequest;

        public HttpRequestWrapper(HttpRequest httpRequest) {
            if (httpRequest == null) {
                throw new ArgumentNullException("httpRequest");
            }
            _httpRequest = httpRequest;
        }

        public override HttpBrowserCapabilitiesBase Browser {
            get {
                return new HttpBrowserCapabilitiesWrapper(_httpRequest.Browser);
            }
        }

        public override NameValueCollection Params {
            get {
                return _httpRequest.Params;
            }
        }

        public override string Path {
            get {
                return _httpRequest.Path;
            }
        }

        public override string FilePath {
            get {
                return _httpRequest.FilePath;
            }
        }

        public override NameValueCollection Headers {
            get {
                return _httpRequest.Headers;
            }
        }

        public override NameValueCollection QueryString {
            get {
                return _httpRequest.QueryString;
            }
        }

        public override string[] AcceptTypes {
            get {
                return _httpRequest.AcceptTypes;
            }
        }

        public override string ApplicationPath {
            get {
                return _httpRequest.ApplicationPath;
            }
        }

        public override string AnonymousID {
            get {
                return _httpRequest.AnonymousID;
            }
        }

        public override string AppRelativeCurrentExecutionFilePath {
            get {
                return _httpRequest.AppRelativeCurrentExecutionFilePath;
            }
        }

        public override ChannelBinding HttpChannelBinding {
            get {
                return _httpRequest.HttpChannelBinding;
            }
        }

        public override HttpClientCertificate ClientCertificate {
            get {
                return _httpRequest.ClientCertificate;
            }
        }

        public override Encoding ContentEncoding {
            get {
                return _httpRequest.ContentEncoding;
            }
            set {
                _httpRequest.ContentEncoding = value;
            }
        }

        public override int ContentLength {
            get {
                return _httpRequest.ContentLength;
            }
        }

        public override string ContentType {
            get {
                return _httpRequest.ContentType;
            }
            set {
                _httpRequest.ContentType = value;
            }
        }

        public override HttpCookieCollection Cookies {
            get {
                return _httpRequest.Cookies;
            }
        }

        public override string CurrentExecutionFilePath {
            get {
                return _httpRequest.CurrentExecutionFilePath;
            }
        }

        public override string CurrentExecutionFilePathExtension {
            get {
                return _httpRequest.CurrentExecutionFilePathExtension;
            }
        }

        public override HttpFileCollectionBase Files {
            get {
                // method returns an empty collection rather than null
                return new HttpFileCollectionWrapper(_httpRequest.Files);
            }
        }

        public override Stream Filter {
            get {
                return _httpRequest.Filter;
            }
            set {
                _httpRequest.Filter = value;
            }
        }

        public override NameValueCollection Form {
            get {
                return _httpRequest.Form;
            }
        }

        public override string HttpMethod {
            get {
                return _httpRequest.HttpMethod;
            }
        }

        public override Stream InputStream {
            get {
                return _httpRequest.InputStream;
            }
        }

        public override bool IsAuthenticated {
            get {
                return _httpRequest.IsAuthenticated;
            }
        }

        public override bool IsLocal {
            get {
                return _httpRequest.IsLocal;
            }
        }

        public override bool IsSecureConnection {
            get {
                return _httpRequest.IsSecureConnection;
            }
        }

        public override WindowsIdentity LogonUserIdentity {
            get {
                return _httpRequest.LogonUserIdentity;
            }
        }

        public override string PathInfo {
            get {
                return _httpRequest.PathInfo;
            }
        }

        public override string PhysicalApplicationPath {
            get {
                return _httpRequest.PhysicalApplicationPath;
            }
        }

        public override string PhysicalPath {
            get {
                return _httpRequest.PhysicalPath;
            }
        }

        public override string RawUrl {
            get {
                return _httpRequest.RawUrl;
            }
        }

        public override ReadEntityBodyMode ReadEntityBodyMode {
            get {
                return _httpRequest.ReadEntityBodyMode;
            }
        }

        public override RequestContext RequestContext {
            get {
                return _httpRequest.RequestContext;
            }
            set {
                _httpRequest.RequestContext = value;
            }
        }

        public override string RequestType {
            get {
                return _httpRequest.RequestType;
            }
            set {
                _httpRequest.RequestType = value;
            }
        }

        public override NameValueCollection ServerVariables {
            get { 
                return _httpRequest.ServerVariables;
            }
        }

        public override CancellationToken TimedOutToken {
            get {
                return _httpRequest.TimedOutToken;
            }
        }

        public override int TotalBytes {
            get {
                return _httpRequest.TotalBytes;
            }
        }

        public override UnvalidatedRequestValuesBase Unvalidated {
            get {
                return new UnvalidatedRequestValuesWrapper(_httpRequest.Unvalidated);
            }
        }

        public override Uri Url {
            get {
                return _httpRequest.Url;
            }
        }

        public override Uri UrlReferrer {
            get {
                return _httpRequest.UrlReferrer;
            }
        }

        public override string UserAgent {
            get {
                return _httpRequest.UserAgent;
            }
        }

        public override string[] UserLanguages {
            get {
                return _httpRequest.UserLanguages;
            }
        }

        public override string UserHostAddress {
            get {
                return _httpRequest.UserHostAddress;
            }
        }

        public override string UserHostName {
            get {
                return _httpRequest.UserHostName;
            }
        }

        public override string this[string key] {
            get {
                return _httpRequest[key];
            }
        }

        public override void Abort() {
            _httpRequest.Abort();
        }

        public override byte[] BinaryRead(int count) {
            return _httpRequest.BinaryRead(count);
        }

        public override Stream GetBufferedInputStream() {
            return _httpRequest.GetBufferedInputStream();
        }

        public override Stream GetBufferlessInputStream() {
            return _httpRequest.GetBufferlessInputStream();
        }

        public override Stream GetBufferlessInputStream(bool disableMaxRequestLength) {
            return _httpRequest.GetBufferlessInputStream(disableMaxRequestLength);
        }

        public override void InsertEntityBody() {
            _httpRequest.InsertEntityBody();
        }

        public override void InsertEntityBody(byte[] buffer, int offset, int count) {
            _httpRequest.InsertEntityBody(buffer, offset, count);
        }

        public override int[] MapImageCoordinates(string imageFieldName) {
            return _httpRequest.MapImageCoordinates(imageFieldName);
        }

        public override double[] MapRawImageCoordinates(string imageFieldName) {
            return _httpRequest.MapRawImageCoordinates(imageFieldName);
        }

        public override string MapPath(string virtualPath) {
            return _httpRequest.MapPath(virtualPath);
        }

        public override string MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping) {
            return _httpRequest.MapPath(virtualPath, baseVirtualDir, allowCrossAppMapping);
        }

        public override void ValidateInput() {
            _httpRequest.ValidateInput();
        }

        public override void SaveAs(string filename, bool includeHeaders) {
            _httpRequest.SaveAs(filename, includeHeaders);
        }
    }
}
