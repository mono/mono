//------------------------------------------------------------------------------
// <copyright file="HttpRawResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Lean representation of response data
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {

    using System.Collections;

    internal class HttpRawResponse {
        private int _statusCode;
        private String _statusDescr;
        private ArrayList _headers;
        private ArrayList _buffers;
        private bool _hasSubstBlocks;

        internal HttpRawResponse(int statusCode, string statusDescription, ArrayList headers, ArrayList buffers, bool hasSubstBlocks) {
            _statusCode = statusCode;
            _statusDescr = statusDescription;
            _headers = headers;
            _buffers = buffers;
            _hasSubstBlocks = hasSubstBlocks;
        }

        internal int StatusCode {
            get { return _statusCode;}
        }

        internal String StatusDescription {
            get { return _statusDescr;}
        }

        // list of HttpResponseHeader objects
        internal ArrayList Headers {
            get { return _headers;}
        }

        // list of IHttpResponseElement objects
        internal ArrayList Buffers {
            get { 
                return _buffers;
            }
        }

        internal bool HasSubstBlocks {
            get { return _hasSubstBlocks;}
        }
    }
}
