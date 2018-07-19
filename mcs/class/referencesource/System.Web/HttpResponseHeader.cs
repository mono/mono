//------------------------------------------------------------------------------
// <copyright file="HttpResponseHeader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Single http header representation
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Web.Util;

    /*
     * Response header (either known or unknown)
     */
    [Serializable]
    internal class HttpResponseHeader {

        private String _unknownHeader;
        private int _knownHeaderIndex;
        private String _value;

        internal HttpResponseHeader(int knownHeaderIndex, String value)
            : this(knownHeaderIndex, value, HttpRuntime.EnableHeaderChecking) {
        }

        internal HttpResponseHeader(int knownHeaderIndex, string value, bool enableHeaderChecking) {
            _unknownHeader = null;
            _knownHeaderIndex = knownHeaderIndex;

            // encode header value if
            if (enableHeaderChecking) {
                string encodedName; // unused
                HttpEncoder.Current.HeaderNameValueEncode(Name, value, out encodedName, out _value);
            }
            else {
                _value = value;
            }
        }

        internal HttpResponseHeader(String unknownHeader, String value)
            : this(unknownHeader, value, HttpRuntime.EnableHeaderChecking) {
        }

        internal HttpResponseHeader(string unknownHeader, string value, bool enableHeaderChecking) {
            if (enableHeaderChecking) {
                HttpEncoder.Current.HeaderNameValueEncode(unknownHeader, value, out _unknownHeader, out _value);
                _knownHeaderIndex = HttpWorkerRequest.GetKnownResponseHeaderIndex(_unknownHeader);
            }
            else {
                _unknownHeader = unknownHeader;
                _knownHeaderIndex = HttpWorkerRequest.GetKnownResponseHeaderIndex(_unknownHeader);
                _value = value;
            }
        }

        internal String Name {
            get {
                if (_unknownHeader != null)
                    return _unknownHeader;
                else
                    return HttpWorkerRequest.GetKnownResponseHeaderName(_knownHeaderIndex);
            }
        }

        internal String Value {
            get { return _value; }
        }

        internal void Send(HttpWorkerRequest wr) {
            if (_knownHeaderIndex >= 0)
                wr.SendKnownResponseHeader(_knownHeaderIndex, _value);
            else
                wr.SendUnknownResponseHeader(_unknownHeader, _value);
        }

    }
}
