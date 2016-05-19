//------------------------------------------------------------------------------
// <copyright file="HttpHeaderCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Collection of headers with write through to IIS for Set, Add, and Remove
 * 
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Runtime.Serialization;
    using System.Web.Hosting;
    using System.Web.Util;
    
    [Serializable()]
    internal class HttpHeaderCollection : HttpValueCollection {
        private HttpRequest _request;
        private HttpResponse _response;
        private IIS7WorkerRequest _iis7WorkerRequest;
        

        // This constructor creates the header collection for request headers.
        // Try to preallocate the base collection with a size that should be sufficient
        // to store the headers for most requests.
        internal HttpHeaderCollection(HttpWorkerRequest wr, HttpRequest request, int capacity) : base(capacity) {

            // if this is an IIS7WorkerRequest, then the collection will be writeable and we will
            // call into IIS7 to update the header blocks when changes are made.
            _iis7WorkerRequest = wr as IIS7WorkerRequest;

            _request = request;
        }

        // This constructor creates the header collection for response headers.
        // Try to preallocate the base collection with a size that should be sufficient
        // to store the headers for most requests.
        internal HttpHeaderCollection(HttpWorkerRequest wr, HttpResponse response, int capacity) : base(capacity) {

            // if this is an IIS7WorkerRequest, then the collection will be writeable and we will
            // call into IIS7 to update the header blocks when changes are made.
            _iis7WorkerRequest = wr as IIS7WorkerRequest;

            _response = response;
        }

        // This copy constructor is used by the granular request validation feature. Since these collections are immutable
        // once created, it's ok for us to have two collections containing the same data.
        internal HttpHeaderCollection(HttpHeaderCollection col)
            : base(col) {

            _request = col._request;
            _response = col._response;
            _iis7WorkerRequest = col._iis7WorkerRequest;
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            // WOS 127340: Request.Headers and Response.Headers are no longer serializable
            base.GetObjectData(info, context);
            // create an instance of HttpValueCollection since HttpHeaderCollection is tied to the request
            info.SetType(typeof(HttpValueCollection));
        }

        public override void Add(String name, String value) {
            if (_iis7WorkerRequest == null) {
                throw new PlatformNotSupportedException();
            }
            // append to existing value
            SetHeader(name, value, false /*replace*/);
        }

        public override void Clear() {
            throw new NotSupportedException();
        }

        internal void ClearInternal() {
            // clear is only supported for response headers
            if (_request != null) {
                throw new NotSupportedException();
            }
            base.Clear();
        }

        public override void Set(String name, String value) {
            if (_iis7WorkerRequest == null) {
                throw new PlatformNotSupportedException();
            }
            // set new value
            SetHeader(name, value, true /*replace*/);
        }

        internal void SetHeader(String name, String value, bool replace) {
            Debug.Assert(_iis7WorkerRequest != null, "_iis7WorkerRequest != null");

            if (name == null) {
                throw new ArgumentNullException("name"); 
            }
            
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            if (_request != null) {
                _iis7WorkerRequest.SetRequestHeader(name, value, replace);
            }
            else {
                if (_response.HeadersWritten) {
                    throw new HttpException(SR.GetString(SR.Cannot_append_header_after_headers_sent));
                }

                // IIS7 integrated pipeline mode needs to call the header encoding routine explicitly since it
                // doesn't go through HttpResponse.WriteHeaders().
                string encodedName = name;
                string encodedValue = value;
                if (HttpRuntime.EnableHeaderChecking) {
                    HttpEncoder.Current.HeaderNameValueEncode(name, value, out encodedName, out encodedValue);
                }
                
                // set the header encoding to the selected encoding
                _iis7WorkerRequest.SetHeaderEncoding(_response.HeaderEncoding);

                _iis7WorkerRequest.SetResponseHeader(encodedName, encodedValue, replace);

                if (_response.HasCachePolicy && StringUtil.EqualsIgnoreCase("Set-Cookie", name)) {
                    _response.Cache.SetHasSetCookieHeader();
                }
            }
            
            // update managed copy of header
            if (replace) {
                base.Set(name, value);
            }
            else {
                base.Add(name, value);
            }

            if (_request != null) {
                // update managed copy of server variable
                string svValue = replace ? value : base.Get(name);
                HttpServerVarsCollection serverVars = _request.ServerVariables as HttpServerVarsCollection;
                if (serverVars != null) {
                    serverVars.SynchronizeServerVariable("HTTP_" + name.ToUpper(CultureInfo.InvariantCulture).Replace('-', '_'), svValue, ensurePopulated: false);
                }

                // invalidate Params collection
                _request.InvalidateParams();
            }
        }

        // updates managed copy of header with current value from native header block
        internal void SynchronizeHeader(String name, String value) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            
            if (value != null) {
                base.Set(name, value);
            }
            else {
                base.Remove(name);
            }

            if (_request != null) {
                _request.InvalidateParams();
            }
        }

        public override void Remove(String name) {
            if (_iis7WorkerRequest == null) {
                throw new PlatformNotSupportedException();
            }

            if (name == null) {
                throw new ArgumentNullException("name");
            }           

            if (_request != null) {
                // delete by sending null value
                _iis7WorkerRequest.SetRequestHeader(name, null /*value*/, false /*replace*/);
            }
            else {
                _iis7WorkerRequest.SetResponseHeader(name, null /*value*/, false /*replace*/);
            }

            base.Remove(name);
            if (_request != null) {
                // update managed copy of server variable
                HttpServerVarsCollection serverVars = _request.ServerVariables as HttpServerVarsCollection;
                if (serverVars != null) {
                    serverVars.SynchronizeServerVariable("HTTP_" + name.ToUpper(CultureInfo.InvariantCulture).Replace('-', '_'), null, ensurePopulated: false);
                }

                // invalidate Params collection
                _request.InvalidateParams();
            }
        }
    }
}

