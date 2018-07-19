//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;

    public abstract class ExceptionHandler
    {
        static readonly ExceptionHandler alwaysHandle = new AlwaysHandleExceptionHandler();

        static ExceptionHandler transportExceptionHandler = alwaysHandle;

        public static ExceptionHandler AlwaysHandle
        {
            get
            {
                return alwaysHandle;
            }
        }

        public static ExceptionHandler AsynchronousThreadExceptionHandler
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                // 
                HandlerWrapper wrapper = (HandlerWrapper)Fx.AsynchronousThreadExceptionHandler;
                return wrapper == null ? null : wrapper.Handler;
            }

            [Fx.Tag.SecurityNote(Critical = "Calls a LinkDemanded method (Fx setter) and critical method (HandlerWrapper ctor)",
                Safe = "protected with LinkDemand")]
            [SecuritySafeCritical]
            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            set
            {
                Fx.AsynchronousThreadExceptionHandler = value == null ? null : new HandlerWrapper(value);
            }
        }

        public static ExceptionHandler TransportExceptionHandler
        {
            get
            {
                return transportExceptionHandler;
            }

            set
            {
                transportExceptionHandler = value;
            }
        }

        // Returns true if the exception has been handled.  If it returns false or
        // throws a different exception, the original exception will be rethrown.
        public abstract bool HandleException(Exception exception);


        class AlwaysHandleExceptionHandler : ExceptionHandler
        {
            [Fx.Tag.SecurityNote(Miscellaneous = "this function can be called from within a CER, must not call into PT code")]
            public override bool HandleException(Exception exception)
            {
                return true;
            }
        }

        internal static bool HandleTransportExceptionHelper(Exception exception)
        {
            if (exception == null)
            {
                throw Fx.AssertAndThrow("Null exception passed to HandleTransportExceptionHelper.");
            }

            ExceptionHandler handler = TransportExceptionHandler;
            if (handler == null)
            {
                return false;
            }

            try
            {
                if (!handler.HandleException(exception))
                {
                    return false;
                }
            }
            catch (Exception thrownException)
            {
                if (Fx.IsFatal(thrownException))
                {
                    throw;
                }

                DiagnosticUtility.TraceHandledException(thrownException, TraceEventType.Error);
                return false;
            }

            DiagnosticUtility.TraceHandledException(exception, TraceEventType.Error);
            return true;
        }


        class HandlerWrapper : Fx.ExceptionHandler
        {
            [Fx.Tag.SecurityNote(Critical = "Cannot let PT code alter the handler wrapped by this class.")]
            [SecurityCritical]
            readonly ExceptionHandler handler;

            [Fx.Tag.SecurityNote(Critical = "Cannot let PT code alter the handler wrapped by this class.")]
            [SecurityCritical]
            public HandlerWrapper(ExceptionHandler handler)
            {
                Fx.Assert(handler != null, "Cannot wrap a null handler.");
                this.handler = handler;
            }

            public ExceptionHandler Handler
            {
                [Fx.Tag.SecurityNote(Critical = "Access security-critical field.", Safe = "Ok to read field.")]
                [SecuritySafeCritical]
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
                get
                {
                    return this.handler;
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Access security-critical field.", Safe = "Ok to call handler.",
                Miscellaneous = "Called in a CER, must not call into PT code.")]
            [SecuritySafeCritical]
            public override bool HandleException(Exception exception)
            {
                return this.handler.HandleException(exception);
            }
        }
    }
}
