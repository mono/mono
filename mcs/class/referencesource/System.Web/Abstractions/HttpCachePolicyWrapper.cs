//------------------------------------------------------------------------------
// <copyright file="HttpCachePolicyWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpCachePolicyWrapper : HttpCachePolicyBase {
        private HttpCachePolicy _httpCachePolicy;

        public HttpCachePolicyWrapper(HttpCachePolicy httpCachePolicy) {
            if (httpCachePolicy == null) {
                throw new ArgumentNullException("httpCachePolicy");
            }
            _httpCachePolicy = httpCachePolicy;
        }

        public override HttpCacheVaryByContentEncodings VaryByContentEncodings {
            get {
                return _httpCachePolicy.VaryByContentEncodings;
            }
        }

        public override HttpCacheVaryByHeaders VaryByHeaders {
            get {
                return _httpCachePolicy.VaryByHeaders;
            }
        }

        public override HttpCacheVaryByParams VaryByParams {
            get {
                return _httpCachePolicy.VaryByParams;
            }
        }

        public override void AddValidationCallback(HttpCacheValidateHandler handler, object data) {
            _httpCachePolicy.AddValidationCallback(handler, data);
        }

        public override void AppendCacheExtension(string extension) {
            _httpCachePolicy.AppendCacheExtension(extension);
        }

        public override void SetAllowResponseInBrowserHistory(bool allow) {
            _httpCachePolicy.SetAllowResponseInBrowserHistory(allow);
        }

        public override void SetCacheability(HttpCacheability cacheability) {
            _httpCachePolicy.SetCacheability(cacheability);
        }

        public override void SetCacheability(HttpCacheability cacheability, string field) {
            _httpCachePolicy.SetCacheability(cacheability, field);
        }

        public override void SetETag(string etag) {
            _httpCachePolicy.SetETag(etag);
        }

        public override void SetETagFromFileDependencies() {
            _httpCachePolicy.SetETagFromFileDependencies();
        }

        public override void SetExpires(DateTime date) {
            _httpCachePolicy.SetExpires(date);
        }

        public override void SetLastModified(DateTime date) {
            _httpCachePolicy.SetLastModified(date);
        }

        public override void SetLastModifiedFromFileDependencies() {
            _httpCachePolicy.SetLastModifiedFromFileDependencies();
        }

        public override void SetMaxAge(TimeSpan delta) {
            _httpCachePolicy.SetMaxAge(delta);
        }

        public override void SetNoServerCaching() {
            _httpCachePolicy.SetNoServerCaching();
        }

        public override void SetNoStore() {
            _httpCachePolicy.SetNoStore();
        }

        public override void SetNoTransforms() {
            _httpCachePolicy.SetNoTransforms();
        }

        public override void SetOmitVaryStar(bool omit) {
            _httpCachePolicy.SetOmitVaryStar(omit);
        }

        public override void SetProxyMaxAge(TimeSpan delta) {
            _httpCachePolicy.SetProxyMaxAge(delta);
        }

        public override void SetRevalidation(HttpCacheRevalidation revalidation) {
            _httpCachePolicy.SetRevalidation(revalidation);
        }

        public override void SetSlidingExpiration(bool slide) {
            _httpCachePolicy.SetSlidingExpiration(slide);
        }

        public override void SetValidUntilExpires(bool validUntilExpires) {
            _httpCachePolicy.SetValidUntilExpires(validUntilExpires);
        }

        public override void SetVaryByCustom(string custom) {
            _httpCachePolicy.SetVaryByCustom(custom);
        }
    }
}
