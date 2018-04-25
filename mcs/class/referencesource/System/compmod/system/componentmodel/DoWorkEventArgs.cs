//------------------------------------------------------------------------------
// <copyright file="DoWorkEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel 
{
    using System.Security.Permissions;

    [HostProtection(SharedState = true)]
    public class DoWorkEventArgs : CancelEventArgs
    {
        private object result;
        private object argument;

        public DoWorkEventArgs(object argument)
        {
            this.argument = argument;
        }

        [ SRDescription(SR.BackgroundWorker_DoWorkEventArgs_Argument) ]
        public object Argument
        {
            get { return argument; }
        }

        [ SRDescription(SR.BackgroundWorker_DoWorkEventArgs_Result) ]
        public object Result
        {
            get { return result; }
            set { result = value; }
        }
    }
}

