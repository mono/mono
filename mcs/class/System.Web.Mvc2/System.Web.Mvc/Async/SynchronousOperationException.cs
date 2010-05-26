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

namespace System.Web.Mvc.Async {
    using System;
    using System.Runtime.Serialization;

    // This exception type is thrown by the SynchronizationContextUtil helper class since the AspNetSynchronizationContext
    // type swallows exceptions. The inner exception contains the data the user cares about.

    [Serializable]
    public sealed class SynchronousOperationException : HttpException {

        public SynchronousOperationException() {
        }

        private SynchronousOperationException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public SynchronousOperationException(string message)
            : base(message) {
        }

        public SynchronousOperationException(string message, Exception innerException)
            : base(message, innerException) {
        }

    }
}
