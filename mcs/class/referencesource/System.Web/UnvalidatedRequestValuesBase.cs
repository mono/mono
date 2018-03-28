//------------------------------------------------------------------------------
// <copyright file="UnvalidatedRequestValuesBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections.Specialized;

    // Abstract base class for wrapping the request's unvalidated values.

    public abstract class UnvalidatedRequestValuesBase {

        public virtual NameValueCollection Form {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual NameValueCollection QueryString {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual NameValueCollection Headers {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpCookieCollection Cookies {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual HttpFileCollectionBase Files {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string RawUrl {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string Path {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string PathInfo {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string this[string field] {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Uri Url {
            get {
                throw new NotImplementedException();
            }
        }
    }
}
