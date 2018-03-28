//------------------------------------------------------------------------------
// <copyright file="HttpResponseInternalWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Web.Caching;

    internal sealed class HttpResponseInternalWrapper : HttpResponseInternalBase {
        private HttpResponse _httpResponse;

        public HttpResponseInternalWrapper(HttpResponse httpResponse) {
            Debug.Assert(httpResponse != null);
            _httpResponse = httpResponse;
        }

        public override HttpCachePolicyBase Cache {
            get {
                return new HttpCachePolicyWrapper(_httpResponse.Cache);
            }
        }

        public override string ContentType {
            get {
                return _httpResponse.ContentType;
            }
            set {
                _httpResponse.ContentType = value;
            }
        }

        public override Stream Filter {
            get {
                return _httpResponse.Filter;
            }
            set {
                _httpResponse.Filter = value;
            }
        }

        public override TextWriter Output {
            get {
                return _httpResponse.Output;
            }
        }

        public override void Clear() {
            _httpResponse.Clear();
        }

        public override void End() {
            _httpResponse.End();
        }

        public override void Write(string s) {
            _httpResponse.Write(s);
        }

        public override bool Buffer {
            get {
                return _httpResponse.Buffer;
            }
            set {
                _httpResponse.Buffer = value;
            }
        }

        public override bool BufferOutput {
            get {
                return _httpResponse.BufferOutput;
            }
            set {
                _httpResponse.BufferOutput = value;
            }
        }

        public override string CacheControl {
            get {
                return _httpResponse.CacheControl;
            }
            set {
                _httpResponse.CacheControl = value;
            }
        }

        public override string Charset {
            get {
                return _httpResponse.Charset;
            }
            set {
                _httpResponse.Charset = value;
            }
        }

        public override Encoding ContentEncoding {
            get {
                return _httpResponse.ContentEncoding;
            }
            set {
                _httpResponse.ContentEncoding = value;
            }
        }

        public override HttpCookieCollection Cookies {
            get {
                return _httpResponse.Cookies;
            }
        }

        public override int Expires {
            get {
                return _httpResponse.Expires;
            }
            set {
                _httpResponse.Expires = value;
            }
        }

        public override DateTime ExpiresAbsolute {
            get {
                return _httpResponse.ExpiresAbsolute;
            }
            set {
                _httpResponse.ExpiresAbsolute = value;
            }
        }

        public override NameValueCollection Headers {
            get {
                return _httpResponse.Headers;
            }
        }

        public override Encoding HeaderEncoding {
            get {
                return _httpResponse.HeaderEncoding;
            }
            set {
                _httpResponse.HeaderEncoding = value;
            }
        }

        public override bool IsClientConnected {
            get {
                return _httpResponse.IsClientConnected;
            }
        }

        public override bool IsRequestBeingRedirected {
            get {
                return _httpResponse.IsRequestBeingRedirected;
            }
        }

        public override string RedirectLocation {
            get {
                return _httpResponse.RedirectLocation;
            }
            set {
                _httpResponse.RedirectLocation = value;
            }
        }

        public override string Status {
            get {
                return _httpResponse.Status;
            }
            set {
                _httpResponse.Status = value;
            }
        }

        public override int StatusCode {
            get {
                return _httpResponse.StatusCode;
            }
            set {
                _httpResponse.StatusCode = value;
            }
        }

        public override string StatusDescription {
            get {
                return _httpResponse.StatusDescription;
            }
            set {
                _httpResponse.StatusDescription = value;
            }
        }

        public override int SubStatusCode {
            get {
                return _httpResponse.SubStatusCode;
            }
            set {
                _httpResponse.SubStatusCode = value;
            }
        }

        public override bool SuppressContent {
            get {
                return _httpResponse.SuppressContent;
            }
            set {
                _httpResponse.SuppressContent = value;
            }
        }

        public override bool TrySkipIisCustomErrors {
            get {
                return _httpResponse.TrySkipIisCustomErrors;
            }
            set {
                _httpResponse.TrySkipIisCustomErrors = value;
            }
        }

        public override void AddCacheItemDependency(string cacheKey) {
            _httpResponse.AddCacheItemDependency(cacheKey);
        }

        public override void AddCacheItemDependencies(ArrayList cacheKeys) {
            _httpResponse.AddCacheItemDependencies(cacheKeys);
        }

        public override void AddCacheItemDependencies(string[] cacheKeys) {
            _httpResponse.AddCacheItemDependencies(cacheKeys);
        }

        public override void AddCacheDependency(params CacheDependency[] dependencies) {
            _httpResponse.AddCacheDependency(dependencies);
        }

        public override void AddFileDependency(string filename) {
            _httpResponse.AddFileDependency(filename);
        }

        public override void AddFileDependencies(ArrayList filenames) {
            _httpResponse.AddFileDependencies(filenames);
        }

        public override void AddFileDependencies(string[] filenames) {
            _httpResponse.AddFileDependencies(filenames);
        }

        public override void AppendCookie(HttpCookie cookie) {
            _httpResponse.AppendCookie(cookie);
        }

        public override void AppendHeader(string name, string value) {
            _httpResponse.AppendHeader(name, value);
        }

        public override void AppendToLog(string param) {
            _httpResponse.AppendToLog(param);
        }

        public override string ApplyAppPathModifier(string virtualPath) {
            return _httpResponse.ApplyAppPathModifier(virtualPath);
        }

        public override void BinaryWrite(byte[] buffer) {
            _httpResponse.BinaryWrite(buffer);
        }

        public override void ClearContent() {
            _httpResponse.ClearContent();
        }

        public override void ClearHeaders() {
            _httpResponse.ClearHeaders();
        }

        public override void DisableKernelCache() {
            _httpResponse.DisableKernelCache();
        }

        public override void Flush() {
            _httpResponse.Flush();
        }

        public override void Pics(string value) {
            _httpResponse.Pics(value);
        }

        public override void Redirect(string url) {
            _httpResponse.Redirect(url);
        }

        public override void Redirect(string url, bool endResponse) {
            _httpResponse.Redirect(url, endResponse);
        }

        public override void SetCookie(HttpCookie cookie) {
            _httpResponse.SetCookie(cookie);
        }

        public override TextWriter SwitchWriter(TextWriter writer) {
            return _httpResponse.SwitchWriter(writer);
        }

        public override void TransmitFile(string filename) {
            _httpResponse.TransmitFile(filename);
        }

        public override void TransmitFile(string filename, long offset, long length) {
            _httpResponse.TransmitFile(filename, offset, length);
        }

        public override void Write(char[] buffer, int index, int count) {
            _httpResponse.Write(buffer, index, count);
        }

        public override void Write(object obj) {
            _httpResponse.Write(obj);
        }

        public override void WriteFile(string filename) {
            _httpResponse.WriteFile(filename);
        }

        public override void WriteFile(string filename, bool readIntoMemory) {
            _httpResponse.WriteFile(filename, readIntoMemory);
        }

        public override void WriteFile(string filename, long offset, long size) {
            _httpResponse.WriteFile(filename, offset, size);
        }

        public override void WriteFile(IntPtr fileHandle, long offset, long size) {
            _httpResponse.WriteFile(fileHandle, offset, size);
        }

        public override void WriteSubstitution(HttpResponseSubstitutionCallback callback) {
            _httpResponse.WriteSubstitution(callback);
        }
    }
}
