/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.UI;

    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
        Justification = "Unsealed so that subclassed types can set properties in the default constructor.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class OutputCacheAttribute : ActionFilterAttribute {

        private OutputCacheParameters _cacheSettings = new OutputCacheParameters();

        public string CacheProfile {
            get {
                return _cacheSettings.CacheProfile ?? String.Empty;
            }
            set {
                _cacheSettings.CacheProfile = value;
            }
        }

        internal OutputCacheParameters CacheSettings {
            get {
                return _cacheSettings;
            }
        }

        public int Duration {
            get {
                return _cacheSettings.Duration;
            }
            set {
                _cacheSettings.Duration = value;
            }
        }

        public OutputCacheLocation Location {
            get {
                return _cacheSettings.Location;
            }
            set {
                _cacheSettings.Location = value;
            }
        }

        public bool NoStore {
            get {
                return _cacheSettings.NoStore;
            }
            set {
                _cacheSettings.NoStore = value;
            }
        }

        public string SqlDependency {
            get {
                return _cacheSettings.SqlDependency ?? String.Empty;
            }
            set {
                _cacheSettings.SqlDependency = value;
            }
        }

        public string VaryByContentEncoding {
            get {
                return _cacheSettings.VaryByContentEncoding ?? String.Empty;
            }
            set {
                _cacheSettings.VaryByContentEncoding = value;
            }
        }

        public string VaryByCustom {
            get {
                return _cacheSettings.VaryByCustom ?? String.Empty;
            }
            set {
                _cacheSettings.VaryByCustom = value;
            }
        }

        public string VaryByHeader {
            get {
                return _cacheSettings.VaryByHeader ?? String.Empty;
            }
            set {
                _cacheSettings.VaryByHeader = value;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param",
            Justification = "Matches the @ OutputCache page directive.")]
        public string VaryByParam {
            get {
                return _cacheSettings.VaryByParam ?? String.Empty;
            }
            set {
                _cacheSettings.VaryByParam = value;
            }
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext) {
            if (filterContext == null) {
                throw new ArgumentNullException("filterContext");
            }

            // we need to call ProcessRequest() since there's no other way to set the Page.Response intrinsic
            OutputCachedPage page = new OutputCachedPage(_cacheSettings);
            page.ProcessRequest(HttpContext.Current);
        }

        private sealed class OutputCachedPage : Page {
            private OutputCacheParameters _cacheSettings;

            public OutputCachedPage(OutputCacheParameters cacheSettings) {
                // Tracing requires Page IDs to be unique.
                ID = Guid.NewGuid().ToString();
                _cacheSettings = cacheSettings;
            }

            protected override void FrameworkInitialize() {
                // when you put the <%@ OutputCache %> directive on a page, the generated code calls InitOutputCache() from here
                base.FrameworkInitialize();
                InitOutputCache(_cacheSettings);
            }
        }

    }
}
