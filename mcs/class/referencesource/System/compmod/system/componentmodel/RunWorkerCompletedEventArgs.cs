//------------------------------------------------------------------------------
// <copyright file="RunWorkerCompletedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel 
{
    using System.Security.Permissions;

    [HostProtection(SharedState = true)]
    public class RunWorkerCompletedEventArgs : AsyncCompletedEventArgs
    {
        private object result;

        public RunWorkerCompletedEventArgs(object result, 
                                           Exception error,
                                           bool cancelled)
            : base(error, cancelled, null)
        {
            this.result = result;
        }
        
        public object Result
        {
            get
            {
                base.RaiseExceptionIfNecessary();
                return result;
            }
        }
        
        // Hide from editor, since never used.
        [ Browsable(false), EditorBrowsable(EditorBrowsableState.Never) ]
        public new object UserState
        {
            get
            {
                return base.UserState;
            }
        }
    }
}

