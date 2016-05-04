//------------------------------------------------------------------------------
// <copyright file="CreatingCookieEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.ApplicationServices
{
    using System;

    public class CreatingCookieEventArgs : EventArgs {
        private string _userName;
        public string UserName {
            get { return _userName; }
        }

        private string _password;
        public string Password {
            get { return _password; }
        }

        private string _customCredential ;
        public string CustomCredential {
            get { return _customCredential;}
        }

        private bool _isPersistent ;
        public bool IsPersistent {
            get { return _isPersistent;}
        }

        private bool _cookieIsSet;
        public bool CookieIsSet {
            set { _cookieIsSet = value; }
            get { return _cookieIsSet; }
        }

        internal CreatingCookieEventArgs(string username, string password, bool isPersistent, string customCredential ) {
            _cookieIsSet = false;
            _userName = username;
            _password = password;
            _password = password;
            _isPersistent = isPersistent;
            _customCredential = customCredential;
        }

        //hiding default constructor
        private CreatingCookieEventArgs() { }
    }
}
