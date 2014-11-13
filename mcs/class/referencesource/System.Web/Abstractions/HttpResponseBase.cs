//------------------------------------------------------------------------------
// <copyright file="HttpResponseBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Routing;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class HttpResponseBase {
        public virtual bool Buffer {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual bool BufferOutput {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual HttpCachePolicyBase Cache {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string CacheControl {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual String Charset {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual CancellationToken ClientDisconnectedToken {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Encoding ContentEncoding {
            set {
                throw new NotImplementedException();
            }
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string ContentType {
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

        public virtual int Expires {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual DateTime ExpiresAbsolute {
            get {
                throw new NotImplementedException();
            }
            set {
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

        public virtual NameValueCollection Headers {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool HeadersWritten {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Encoding HeaderEncoding {
            set {
                throw new NotImplementedException();
            }
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsClientConnected {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsRequestBeingRedirected {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual TextWriter Output {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual Stream OutputStream {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual String RedirectLocation {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual string Status {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual int StatusCode {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual String StatusDescription {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual int SubStatusCode {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsAsyncFlush {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SuppressContent {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual bool SuppressDefaultCacheControlHeader {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual bool SuppressFormsAuthenticationRedirect {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual bool TrySkipIisCustomErrors {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual void AddCacheItemDependency(string cacheKey) {
            throw new NotImplementedException();
        }

        public virtual void AddCacheItemDependencies(ArrayList cacheKeys) {
            throw new NotImplementedException();
        }

        public virtual void AddCacheItemDependencies(string[] cacheKeys) {
            throw new NotImplementedException();
        }

        public virtual void AddCacheDependency(params CacheDependency[] dependencies) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void AddFileDependency(String filename) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void AddFileDependencies(ArrayList filenames) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void AddFileDependencies(string[] filenames) {
            throw new NotImplementedException();
        }

        public virtual void AddHeader(String name, String value) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = @"The normal event pattern doesn't work between HttpResponse and HttpResponseBase since the signatures differ.")]
        public virtual ISubscriptionToken AddOnSendingHeaders(Action<HttpContextBase> callback) {
            throw new NotImplementedException();
        }

        public virtual void AppendCookie(HttpCookie cookie) {
            throw new NotImplementedException();
        }

        public virtual void AppendHeader(String name, String value) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void AppendToLog(String param) {
            throw new NotImplementedException();
        }

        public virtual string ApplyAppPathModifier(string virtualPath) {
            throw new NotImplementedException();
        }

        public virtual IAsyncResult BeginFlush(AsyncCallback callback, Object state) {
            throw new NotImplementedException();
        }

        public virtual void BinaryWrite(byte[] buffer) {
            throw new NotImplementedException();
        }

        public virtual void Clear() {
            throw new NotImplementedException();
        }

        public virtual void ClearContent() {
            throw new NotImplementedException();
        }

        public virtual void ClearHeaders() {
            throw new NotImplementedException();
        }

        public virtual void Close() {
            throw new NotImplementedException();
        }

        public virtual void DisableKernelCache() {
            throw new NotImplementedException();
        }

        public virtual void DisableUserCache() {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "End",
            Justification = "Matches HttpResponse class")]
        public virtual void End() {
            throw new NotImplementedException();
        }

        public virtual void EndFlush(IAsyncResult asyncResult) {
            throw new NotImplementedException();
        }

        public virtual void Flush() {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void Pics(String value) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Matches HttpResponse class")]
        public virtual void Redirect(String url) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Matches HttpResponse class")]
        public virtual void Redirect(String url, bool endResponse) {
            throw new NotImplementedException();
        }

        public virtual void RedirectToRoute(object routeValues) {
            throw new NotImplementedException();
        }

        public virtual void RedirectToRoute(string routeName) {
            throw new NotImplementedException();
        }

        public virtual void RedirectToRoute(RouteValueDictionary routeValues) {
            throw new NotImplementedException();
        }

        public virtual void RedirectToRoute(string routeName, object routeValues) {
            throw new NotImplementedException();
        }

        public virtual void RedirectToRoute(string routeName, RouteValueDictionary routeValues) {
            throw new NotImplementedException();
        }

        public virtual void RedirectToRoutePermanent(object routeValues) {
            throw new NotImplementedException();
        }

        public virtual void RedirectToRoutePermanent(string routeName) {
            throw new NotImplementedException();
        }

        public virtual void RedirectToRoutePermanent(RouteValueDictionary routeValues) {
            throw new NotImplementedException();
        }

        public virtual void RedirectToRoutePermanent(string routeName, object routeValues) {
            throw new NotImplementedException();
        }

        public virtual void RedirectToRoutePermanent(string routeName, RouteValueDictionary routeValues) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Matches HttpResponse class")]
        public virtual void RedirectPermanent(String url) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings",
            Justification = "Matches HttpResponse class")]
        public virtual void RedirectPermanent(String url, bool endResponse) {
            throw new NotImplementedException();
        }

        public virtual void RemoveOutputCacheItem(string path) {
            throw new NotImplementedException();
        }

        public virtual void RemoveOutputCacheItem(string path, string providerName) {
            throw new NotImplementedException();
        }

        public virtual void SetCookie(HttpCookie cookie) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void TransmitFile(string filename) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void TransmitFile(string filename, long offset, long length) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void Write(char ch) {
            throw new NotImplementedException();
        }

        public virtual void Write(char[] buffer, int index, int count) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "obj",
            Justification = "Matches HttpResponse class")]
        public virtual void Write(Object obj) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void Write(string s) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void WriteFile(String filename) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void WriteFile(String filename, bool readIntoMemory) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void WriteFile(String filename, long offset, long size) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpResponse class")]
        public virtual void WriteFile(IntPtr fileHandle, long offset, long size) {
            throw new NotImplementedException();
        }

        public virtual void WriteSubstitution(HttpResponseSubstitutionCallback callback) {
            throw new NotImplementedException();
        }
    }
}
