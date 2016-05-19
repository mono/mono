//------------------------------------------------------------------------------
// <copyright file="AsyncCompletedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel
{
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;
    using System.Reflection;

    [HostProtection(SharedState = true)]
    public class AsyncCompletedEventArgs : System.EventArgs
    {
        private readonly Exception error;
        private readonly bool cancelled;
        private readonly object userState;
        
        [Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public AsyncCompletedEventArgs()
        {
             // This method was public in .NET CF, but didn't do anything.  The fact that it was public was to
             // work around a tooling issue on their side.  
        }
        
        public AsyncCompletedEventArgs(Exception error, bool cancelled, object userState)
        {
            this.error = error;
            this.cancelled = cancelled;
            this.userState = userState;
        }
        
        [ SRDescription(SR.Async_AsyncEventArgs_Cancelled) ]
        public bool Cancelled
        {
            get { return cancelled; }
        }
        
        [ SRDescription(SR.Async_AsyncEventArgs_Error) ]
        public Exception Error
        {
            get { return error; }
        }

        [ SRDescription(SR.Async_AsyncEventArgs_UserState) ]
        public object UserState
        {
            get { return userState; }
        }

        // Call from every result 'getter'. Will throw if there's an error or operation was cancelled
        //
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected void RaiseExceptionIfNecessary()
        {
            if (Error != null)
            {
                throw new TargetInvocationException(SR.GetString(SR.Async_ExceptionOccurred), Error);
            }
            else if (Cancelled)
            {
                throw new InvalidOperationException(SR.GetString(SR.Async_OperationCancelled));
            }
            
        }
        
    }
}
