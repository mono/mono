//------------------------------------------------------------------------------
// <copyright file="HttpCachePolicyBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class HttpCachePolicyBase {
        public virtual HttpCacheVaryByContentEncodings VaryByContentEncodings {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpCacheVaryByHeaders VaryByHeaders {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpCachePolicy class")]
        public virtual HttpCacheVaryByParams VaryByParams {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual void AddValidationCallback(HttpCacheValidateHandler handler, object data) {
            throw new NotImplementedException();
        }

        public virtual void AppendCacheExtension(string extension) {
            throw new NotImplementedException();
        }

        public virtual void SetAllowResponseInBrowserHistory(bool allow) {
            throw new NotImplementedException();
        }

        public virtual void SetCacheability(HttpCacheability cacheability) {
            throw new NotImplementedException();
        }

        public virtual void SetCacheability(HttpCacheability cacheability, string field) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpCachePolicy class")]
        public virtual void SetETag(string etag) {
            throw new NotImplementedException();
        }

        public virtual void SetETagFromFileDependencies() {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date",
            Justification = "Matches HttpCachePolicy class")]
        public virtual void SetExpires(DateTime date) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date",
            Justification = "Matches HttpCachePolicy class")]
        public virtual void SetLastModified(DateTime date) {
            throw new NotImplementedException();
        }

        public virtual void SetLastModifiedFromFileDependencies() {
            throw new NotImplementedException();
        }

        public virtual void SetMaxAge(TimeSpan delta) {
            throw new NotImplementedException();
        }

        public virtual void SetNoServerCaching() {
            throw new NotImplementedException();
        }

        public virtual void SetNoStore() {
            throw new NotImplementedException();
        }

        public virtual void SetNoTransforms() {
            throw new NotImplementedException();
        }

        public virtual void SetOmitVaryStar(bool omit) {
            throw new NotImplementedException();
        }

        public virtual void SetProxyMaxAge(TimeSpan delta) {
            throw new NotImplementedException();
        }

        public virtual void SetRevalidation(HttpCacheRevalidation revalidation) {
            throw new NotImplementedException();
        }

        public virtual void SetSlidingExpiration(bool slide) {
            throw new NotImplementedException();
        }

        public virtual void SetValidUntilExpires(bool validUntilExpires) {
            throw new NotImplementedException();
        }

        public virtual void SetVaryByCustom(string custom) {
            throw new NotImplementedException();
        }
    }
}
