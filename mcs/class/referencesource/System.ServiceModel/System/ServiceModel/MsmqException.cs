//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Text;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.ServiceModel.Channels;

    [Serializable]
    public class MsmqException : ExternalException
    {
        [NonSerialized]
        bool? faultSender = null;
        [NonSerialized]
        bool? faultReceiver = null;
        [NonSerialized]
        Type outerExceptionType = null;

        public MsmqException()
        {
        }

        public MsmqException(string message)
            : base(message)
        {
        }

        public MsmqException(string message, int error)
            : base(message, error)
        {
        }

        public MsmqException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected MsmqException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        internal bool FaultSender
        {
            get
            {
                TuneBehavior();
                return this.faultSender.Value;
            }
        }

        internal bool FaultReceiver
        {
            get
            {
                TuneBehavior();
                return this.faultReceiver.Value;
            }
        }

        void TuneBehavior()
        {
            if (this.faultSender.HasValue && this.faultReceiver.HasValue)
                return;

            switch (this.ErrorCode)
            {
                // configuration erors
                case UnsafeNativeMethods.MQ_ERROR_ACCESS_DENIED:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(AddressAccessDeniedException); break;
                case UnsafeNativeMethods.MQ_ERROR_NO_INTERNAL_USER_CERT:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_QUEUE_DELETED:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(EndpointNotFoundException); break;
                case UnsafeNativeMethods.MQ_ERROR_QUEUE_NOT_FOUND:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(EndpointNotFoundException); break;
                case UnsafeNativeMethods.MQ_ERROR_CERTIFICATE_NOT_PROVIDED:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_INVALID_CERTIFICATE:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_CANNOT_CREATE_CERT_STORE:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_CORRUPTED_PERSONAL_CERT_STORE:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_COULD_NOT_GET_USER_SID:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_ILLEGAL_FORMATNAME:
                    faultSender = false; faultReceiver = false; outerExceptionType = typeof(ArgumentException); break;
                case UnsafeNativeMethods.MQ_ERROR_ILLEGAL_QUEUE_PATHNAME:
                    faultSender = false; faultReceiver = false; outerExceptionType = typeof(ArgumentException); break;
                case UnsafeNativeMethods.MQ_ERROR_UNSUPPORTED_FORMATNAME_OPERATION:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(ArgumentException); break;
                case UnsafeNativeMethods.MQ_ERROR_CANNOT_HASH_DATA_EX:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_CANNOT_SIGN_DATA_EX:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_FAIL_VERIFY_SIGNATURE_EX:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_BAD_SECURITY_CONTEXT:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_PRIVILEGE_NOT_HELD:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_SHARING_VIOLATION:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(AddressAccessDeniedException); break;
                // transient errors
                case UnsafeNativeMethods.MQ_ERROR_DTC_CONNECT:
                    faultSender = false; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_IO_TIMEOUT:
                    faultSender = false; faultReceiver = false; outerExceptionType = typeof(TimeoutException); break;
                case UnsafeNativeMethods.MQ_ERROR_QUEUE_NOT_AVAILABLE:
                    faultSender = false; faultReceiver = true; outerExceptionType = typeof(EndpointNotFoundException); break;
                case UnsafeNativeMethods.MQ_ERROR_REMOTE_MACHINE_NOT_AVAILABLE:
                    faultSender = false; faultReceiver = true; outerExceptionType = typeof(EndpointNotFoundException); break;
                case UnsafeNativeMethods.MQ_ERROR_SERVICE_NOT_AVAILABLE:
                    faultSender = false; faultReceiver = true; outerExceptionType = typeof(EndpointNotFoundException); break;
                case UnsafeNativeMethods.MQ_ERROR_INSUFFICIENT_RESOURCES:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_MESSAGE_STORAGE_FAILED:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_TRANSACTION_ENLIST:
                    faultSender = false; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_TRANSACTION_IMPORT:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_TRANSACTION_USAGE:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(InvalidOperationException); break;
                case UnsafeNativeMethods.MQ_ERROR_STALE_HANDLE:
                    faultSender = false; faultReceiver = false; outerExceptionType = typeof(InvalidOperationException); break;
                // malformed messages
                case UnsafeNativeMethods.MQ_ERROR_ILLEGAL_MQQMPROPS:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                case UnsafeNativeMethods.MQ_ERROR_INSUFFICIENT_PROPERTIES:
                    faultSender = true; faultReceiver = true; outerExceptionType = typeof(CommunicationException); break;
                default:
                    faultSender = true; faultReceiver = true; outerExceptionType = null; break;
            }
        }

        internal Exception Normalized
        {
            get
            {
                TuneBehavior();
                if (null != this.outerExceptionType)
                    return Activator.CreateInstance(this.outerExceptionType, new object[] { this.Message, this }) as Exception;
                else
                    return this;
            }
        }
    }

    static class MsmqError
    {

        [ResourceConsumption(ResourceScope.Process)]
        public static string GetErrorString(int error)
        {
            StringBuilder stringBuilder = new StringBuilder(512);

            bool result = false;
            if ((error & 0x0FFF0000) == 0x000E0000)
            {
                int formatFlags = UnsafeNativeMethods.FORMAT_MESSAGE_IGNORE_INSERTS
                    | UnsafeNativeMethods.FORMAT_MESSAGE_ARGUMENT_ARRAY
                    | UnsafeNativeMethods.FORMAT_MESSAGE_FROM_HMODULE;

                result = (0 != UnsafeNativeMethods.FormatMessage(
                        formatFlags,
                        Msmq.ErrorStrings,
                        error,
                        CultureInfo.CurrentCulture.LCID,
                        stringBuilder,
                        stringBuilder.Capacity,
                        IntPtr.Zero));
            }
            else
            {
                int formatFlags = UnsafeNativeMethods.FORMAT_MESSAGE_IGNORE_INSERTS
                    | UnsafeNativeMethods.FORMAT_MESSAGE_ARGUMENT_ARRAY
                    | UnsafeNativeMethods.FORMAT_MESSAGE_FROM_SYSTEM;

                result = (0 != UnsafeNativeMethods.FormatMessage(
                        formatFlags,
                        IntPtr.Zero,
                        error,
                        CultureInfo.CurrentCulture.LCID,
                        stringBuilder,
                        stringBuilder.Capacity,
                        IntPtr.Zero));
            }

            if (result)
            {
                stringBuilder = stringBuilder.Replace("\n", "");
                stringBuilder = stringBuilder.Replace("\r", "");
                return SR.GetString(
                    SR.MsmqKnownWin32Error,
                    stringBuilder.ToString(),
                    error.ToString(CultureInfo.InvariantCulture),
                    Convert.ToString(error, 16));
            }
            else
            {
                return SR.GetString(
                    SR.MsmqUnknownWin32Error,
                    error.ToString(CultureInfo.InvariantCulture),
                    Convert.ToString(error, 16));
            }
        }
    }
}

