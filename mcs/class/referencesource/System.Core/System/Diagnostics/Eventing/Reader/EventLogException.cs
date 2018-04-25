// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class: EventLogException
**
** Purpose: 
** This public class describes an exception thrown from Event 
** Log related classes.
**
============================================================*/
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Diagnostics.Eventing.Reader {
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class EventLogException : Exception, ISerializable {
        internal static void Throw(int errorCode) {
            switch (errorCode) {
                case 2: 
                case 3:
                case 15007:
                case 15027:
                case 15028:
                case 15002:
                    throw new EventLogNotFoundException(errorCode);

                case 13:
                case 15005:
                    throw new EventLogInvalidDataException(errorCode);

                case 1818: // RPC_S_CALL_CANCELED is converted to ERROR_CANCELLED
                case 1223: 
                    throw new OperationCanceledException();

                case 15037: 
                    throw new EventLogProviderDisabledException(errorCode);

                case 5: 
                    throw new UnauthorizedAccessException();

                case 15011: 
                case 15012:
                    throw new EventLogReadingException(errorCode);
                
                default: throw new EventLogException(errorCode);     
            }
        }

        public EventLogException() { }
        public EventLogException(string message) : base(message) { }
        public EventLogException(string message, Exception innerException) : base(message, innerException) { }
        protected EventLogException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
        protected EventLogException(int errorCode) { this.errorCode = errorCode; }

        // SecurityCritical due to inherited link demand for GetObjectData.
        [System.Security.SecurityCritical,SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new ArgumentNullException("info");
            info.AddValue("errorCode", errorCode);
            base.GetObjectData(info, context);
        }

        public override string Message {
            // marked as SecurityCritical because it uses Win32Exception.
            // marked as TreatAsSafe because it performs Demand.
            [System.Security.SecurityCritical]
            get {
                EventLogPermissionHolder.GetEventLogPermission().Demand();
                Win32Exception win32Exception = new Win32Exception(errorCode);
                return win32Exception.Message;
            }
        }

        private int errorCode;
    }

    /// <summary>
    /// The object requested by the operation is not found. 
    /// </summary>
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class EventLogNotFoundException : EventLogException {
        public EventLogNotFoundException() { }
        public EventLogNotFoundException(string message) : base(message) { }
        public EventLogNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        protected EventLogNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
        internal EventLogNotFoundException(int errorCode) : base(errorCode) { }
    }

    /// <summary>
    /// The state of the reader cursor has become invalid, most likely due to the fact
    /// that the log has been cleared.  User needs to obtain a new reader object if 
    /// they wish to continue navigating result set. 
    /// </summary>
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class EventLogReadingException : EventLogException {
        public EventLogReadingException() { }
        public EventLogReadingException(string message) : base(message) { }
        public EventLogReadingException(string message, Exception innerException) : base(message, innerException) { }
        protected EventLogReadingException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
        internal EventLogReadingException(int errorCode) : base(errorCode) { }
    }

    /// <summary>
    /// Provider has been uninstalled while ProviderMetadata operations are being performed.  
    /// Obtain a new ProviderMetadata object, when provider is reinstalled, to continue navigating
    /// provider's metadata.  
    /// </summary>
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class EventLogProviderDisabledException : EventLogException {
        public EventLogProviderDisabledException() { }
        public EventLogProviderDisabledException(string message) : base(message) { }
        public EventLogProviderDisabledException(string message, Exception innerException) : base(message, innerException) { }
        protected EventLogProviderDisabledException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
        internal EventLogProviderDisabledException(int errorCode) : base(errorCode) { }
    }

    /// <summary>
    /// Data obtained from the eventlog service, for the current operation, is invalid . 
    /// </summary>
    [Serializable]
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public class EventLogInvalidDataException : EventLogException {
        public EventLogInvalidDataException() { }
        public EventLogInvalidDataException(string message) : base(message) { }
        public EventLogInvalidDataException(string message, Exception innerException) : base(message, innerException) { }
        protected EventLogInvalidDataException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
        internal EventLogInvalidDataException(int errorCode) : base(errorCode) { }
    }
   
}

