namespace System.IO {
    
    using System;
    using System.Runtime.Serialization;

#if !FEATURE_NETCORE
    [Serializable] 
#endif // !FEATURE_NETCORE
    public sealed class InvalidDataException : SystemException
    {
        public InvalidDataException () 
            : base(SR.GetString(SR.GenericInvalidData)) {
        }

        public InvalidDataException (String message) 
            : base(message) {
        }
    
        public InvalidDataException (String message, Exception innerException) 
            : base(message, innerException) {
        }
    
#if !FEATURE_NETCORE
        internal InvalidDataException (SerializationInfo info, StreamingContext context) : base(info, context) {
        }
#endif // !FEATURE_NETCORE

    }
}
