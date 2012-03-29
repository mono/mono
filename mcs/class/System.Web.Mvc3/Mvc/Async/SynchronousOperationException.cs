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
