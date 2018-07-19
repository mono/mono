//------------------------------------------------------------------------------
// <copyright file="UserValidatedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.ClientServices.Providers {
    using System;
    using System.Diagnostics.CodeAnalysis;

    public class UserValidatedEventArgs : EventArgs
    {
        public string UserName {
            get {
                return _UserName;
            }
        }
        private string _UserName;

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="username", Justification="consistent with Whidbey")]
        public UserValidatedEventArgs(string username) {
            _UserName = username;
        }
    }
}
