//------------------------------------------------------------------------------
// <copyright file="HttpResponseWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Routing;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpResponseWrapper : HttpResponseBase {
        private HttpResponse _httpResponse;

        public HttpResponseWrapper(HttpResponse httpResponse) {
            if (httpResponse == null) {
                throw new ArgumentNullException("httpResponse");
            }
            _httpResponse = httpResponse;
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

        public override HttpCachePolicyBase Cache {
            get {
                return new HttpCachePolicyWrapper(_httpResponse.Cache);
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

        public override CancellationToken ClientDisconnectedToken {
            get {
                return _httpResponse.ClientDisconnectedToken;
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

        public override string ContentType {
            get {
                return _httpResponse.ContentType;
            }
            set {
                _httpResponse.ContentType = value;
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

        public override Stream Filter {
            get {
                return _httpResponse.Filter;
            }
            set {
                _httpResponse.Filter = value;
            }
        }

        public override NameValueCollection Headers {
            get {
                return _httpResponse.Headers;
            }
        }

        public override bool HeadersWritten {
            get {
                return _httpResponse.HeadersWritten;
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

        public override TextWriter Output {
            get {
                return _httpResponse.Output;
            }
            set {
                _httpResponse.Output = value;
            }
        }

        public override Stream OutputStream {
            get {
                return _httpResponse.OutputStream;
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

        public override bool SupportsAsyncFlush {
            get {
                return _httpResponse.SupportsAsyncFlush;
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

        public override bool SuppressDefaultCacheControlHeader {
            get {
                return _httpResponse.SuppressDefaultCacheControlHeader;
            }
            set {
                _httpResponse.SuppressDefaultCacheControlHeader = value;
            }
        }

        public override bool SuppressFormsAuthenticationRedirect {
            get {
                return _httpResponse.SuppressFormsAuthenticationRedirect;
            }
            set {
                _httpResponse.SuppressFormsAuthenticationRedirect = value;
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

        public override ISubscriptionToken AddOnSendingHeaders(Action<HttpContextBase> callback) {
            return _httpResponse.AddOnSendingHeaders(HttpContextWrapper.WrapCallback(callback));
        }

        public override void AddFileDependencies(ArrayList filenames) {
            _httpResponse.AddFileDependencies(filenames);
        }

        public override void AddFileDependencies(string[] filenames) {
            _httpResponse.AddFileDependencies(filenames);
        }

        public override void AddHeader(string name, string value) {
            _httpResponse.AddHeader(name, value);
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

        public override IAsyncResult BeginFlush(AsyncCallback callback, Object state) {
            return _httpResponse.BeginFlush(callback, state);
        }

        public override void BinaryWrite(byte[] buffer) {
            _httpResponse.BinaryWrite(buffer);
        }

        public override void Clear() {
            _httpResponse.Clear();
        }

        public override void ClearContent() {
            _httpResponse.ClearContent();
        }

        public override void ClearHeaders() {
            _httpResponse.ClearHeaders();
        }

        public override void Close() {
            _httpResponse.Close();
        }

        public override void DisableKernelCache() {
            _httpResponse.DisableKernelCache();
        }

        public override void DisableUserCache() {
            _httpResponse.DisableUserCache();
        }

        public override void End() {
            _httpResponse.End();
        }

        public override void EndFlush(IAsyncResult asyncResult) {
            _httpResponse.EndFlush(asyncResult);
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

        public override void RedirectPermanent(String url) {
            _httpResponse.RedirectPermanent(url);
        }

        public override void RedirectPermanent(String url, bool endResponse) {
            _httpResponse.RedirectPermanent(url, endResponse);
        }

        public override void RedirectToRoute(object routeValues) {
            _httpResponse.RedirectToRoute(routeValues);
        }

        public override void RedirectToRoute(string routeName) {
            _httpResponse.RedirectToRoute(routeName);
        }

        public override void RedirectToRoute(RouteValueDictionary routeValues) {
            _httpResponse.RedirectToRoute(routeValues);
        }

        public override void RedirectToRoute(string routeName, object routeValues) {
            _httpResponse.RedirectToRoute(routeName, routeValues);
        }

        public override void RedirectToRoute(string routeName, RouteValueDictionary routeValues) {
            _httpResponse.RedirectToRoute(routeName, routeValues);
        }

        public override void RedirectToRoutePermanent(object routeValues) {
            _httpResponse.RedirectToRoutePermanent(routeValues);
        }

        public override void RedirectToRoutePermanent(string routeName) {
            _httpResponse.RedirectToRoutePermanent(routeName);
        }

        public override void RedirectToRoutePermanent(RouteValueDictionary routeValues) {
            _httpResponse.RedirectToRoutePermanent(routeValues);
        }

        public override void RedirectToRoutePermanent(string routeName, object routeValues) {
            _httpResponse.RedirectToRoutePermanent(routeName, routeValues);
        }

        public override void RedirectToRoutePermanent(string routeName, RouteValueDictionary routeValues) {
            _httpResponse.RedirectToRoutePermanent(routeName, routeValues);
        }

        public override void RemoveOutputCacheItem(string path) {
            HttpResponse.RemoveOutputCacheItem(path);
        }

        public override void RemoveOutputCacheItem(string path, string providerName) {
            HttpResponse.RemoveOutputCacheItem(path, providerName);
        }

        public override void SetCookie(HttpCookie cookie) {
            _httpResponse.SetCookie(cookie);
        }

        public override void TransmitFile(string filename) {
            _httpResponse.TransmitFile(filename);
        }

        public override void TransmitFile(string filename, long offset, long length) {
            _httpResponse.TransmitFile(filename, offset, length);
        }

        public override void Write(string s) {
            _httpResponse.Write(s);
        }

        public override void Write(char ch) {
            _httpResponse.Write(ch);
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
