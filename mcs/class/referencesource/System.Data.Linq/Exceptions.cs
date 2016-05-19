using System;
using System.Collections.Generic;
using System.Data.Linq.Provider;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq {
    /// <summary>
    /// DLinq-specific custom exception factory.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Unknown reason.")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Unknown reason.")]
    public class ChangeConflictException : Exception {
        public ChangeConflictException() { }
        public ChangeConflictException(string message) : base(message) { }
        public ChangeConflictException(string message, Exception innerException) : base(message, innerException) { }
    }
    /// <summary>
    /// An attempt was made to add an object to the identity cache with a key that is already in use
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Unknown reason.")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Unknown reason.")]
    public class DuplicateKeyException : InvalidOperationException {
        private object duplicate;
        public DuplicateKeyException(object duplicate) {
            this.duplicate = duplicate;
        }
        public DuplicateKeyException(object duplicate, string message)
            : base(message) {
            this.duplicate = duplicate;
        }
        public DuplicateKeyException(object duplicate, string message, Exception innerException)
            : base(message, innerException) {
            this.duplicate = duplicate;
        }

        /// <summary>
        /// The object whose duplicate key caused the exception.
        /// </summary>
        public object Object {
            get {
                return duplicate;
            }
        }
    }

    /// <summary>
    /// An attempt was made to change an FK but the Entity is Loaded
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Unknown reason.")]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Unknown reason.")]
    public class ForeignKeyReferenceAlreadyHasValueException : InvalidOperationException {
        public ForeignKeyReferenceAlreadyHasValueException() { }
        public ForeignKeyReferenceAlreadyHasValueException(string message) : base(message) { }
        public ForeignKeyReferenceAlreadyHasValueException(string message, Exception innerException) : base(message, innerException) { }
    }
}
