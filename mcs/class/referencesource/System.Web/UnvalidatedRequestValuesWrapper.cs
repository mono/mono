//------------------------------------------------------------------------------
// <copyright file="UnvalidatedRequestValuesWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections.Specialized;

    // Concrete class for wrapping the request's unvalidated values.

    public class UnvalidatedRequestValuesWrapper : UnvalidatedRequestValuesBase {

        private readonly UnvalidatedRequestValues _requestValues;

        public UnvalidatedRequestValuesWrapper(UnvalidatedRequestValues requestValues) {
            if (requestValues == null) {
                throw new ArgumentNullException("requestValues");
            }

            _requestValues = requestValues;
        }

        public override NameValueCollection Form {
            get {
                return _requestValues.Form;
            }
        }

        public override NameValueCollection QueryString {
            get {
                return _requestValues.QueryString;
            }
        }

        public override NameValueCollection Headers {
            get {
                return _requestValues.Headers;
            }
        }

        public override HttpCookieCollection Cookies {
            get {
                return _requestValues.Cookies;
            }
        }

        public override HttpFileCollectionBase Files {
            get {
                return new HttpFileCollectionWrapper(_requestValues.Files);
            }
        }

        public override string RawUrl {
            get {
                return _requestValues.RawUrl;
            }
        }

        public override string Path {
            get {
                return _requestValues.Path;
            }
        }

        public override string PathInfo {
            get {
                return _requestValues.PathInfo;
            }
        }

        public override string this[string field] {
            get {
                return _requestValues[field];
            }
        }

        public override Uri Url {
            get {
                return _requestValues.Url;
            }
        }

    }
}
