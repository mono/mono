//------------------------------------------------------------------------------
// <copyright file="ProgressChangedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel 
{
    using System.Security.Permissions;

    [HostProtection(SharedState = true)]
    public class ProgressChangedEventArgs : EventArgs
    {
        private readonly int progressPercentage;
        private readonly object userState;

        public ProgressChangedEventArgs(int progressPercentage, object userState)
        {
            this.progressPercentage = progressPercentage;
            this.userState = userState;
        }
        
        [ SRDescription(SR.Async_ProgressChangedEventArgs_ProgressPercentage) ]
        public int ProgressPercentage
        {
            get { return progressPercentage; }
        }

        [ SRDescription(SR.Async_ProgressChangedEventArgs_UserState) ]
        public object UserState
        {
            get { return userState; }
        }

    }
}

