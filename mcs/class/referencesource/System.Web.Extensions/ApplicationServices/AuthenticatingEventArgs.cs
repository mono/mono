//------------------------------------------------------------------------------
// <copyright file="AuthenticatingEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.ApplicationServices
{
    using System;
    using System.Web;

    public class AuthenticatingEventArgs : EventArgs
    {
        private bool _authenticated;
        public bool Authenticated {
            get { return _authenticated; }
            set { _authenticated = value; }
        }

        private bool _authenticationIsComplete;
        public bool AuthenticationIsComplete {
            get { return _authenticationIsComplete; }
            set { _authenticationIsComplete = value; }
        }

        private string _userName;
        public string UserName {
            get { return _userName; }
        }

        private string _password;
        public string Password {
            get { return _password; }
        }

        private string _customCredential;
        public string CustomCredential {
            get { return _customCredential; }
        }

        internal AuthenticatingEventArgs(string username, string password, string customCredential) {
            _authenticated = false;
            _authenticationIsComplete = false;
            _userName = username;
            _password = password;
            _customCredential = customCredential;
        }

        //hiding default constructor
        private AuthenticatingEventArgs() { }
    }
}
