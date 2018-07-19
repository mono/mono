namespace System.Web.Util {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    // Thrown when the AppVerifier fails an assert.

    // This type is used solely as a support / diagnostics tool and will never appear over the normal course of an application.
    // Specifically, it will never cross an AppDomain boundary, and we don't want to mark it as [Serializable] since that is
    // essentially a public contract, and the AppVerifier feature has no public API surface. We want to retain the ability to
    // change this feature on a whim without risk of breaking anybody.
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Doesn't need to be serializable.")]
    internal sealed class AppVerifierException : Exception {

        private readonly AppVerifierErrorCode _errorCode;

        public AppVerifierException(AppVerifierErrorCode errorCode, string message)
            : base(message) {

            _errorCode = errorCode;
        }

        private AppVerifierException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public AppVerifierErrorCode ErrorCode {
            get {
                return _errorCode;
            }
        }

    }
}
