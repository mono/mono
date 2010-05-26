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
    using System.Runtime.Serialization;
    using System.Web;

    [Serializable]
    public sealed class HttpAntiForgeryException : HttpException {

        public HttpAntiForgeryException() {
        }

        private HttpAntiForgeryException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public HttpAntiForgeryException(string message)
            : base(message) {
        }

        public HttpAntiForgeryException(string message, Exception innerException)
            : base(message, innerException) {
        }

    }
}
