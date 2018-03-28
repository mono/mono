//------------------------------------------------------------------------------
// <copyright file="ValidatePasswordEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * ValidatePasswordEventArgs class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Security 
{
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web, Version=2.0.0.0, Culture=Neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public sealed class ValidatePasswordEventArgs : EventArgs 
    {
        private string    _userName;
        private string    _password;
        private bool      _isNewUser;
        private bool      _cancel;
        private Exception _failureInformation;
        
        public ValidatePasswordEventArgs(
            string userName,
            string password,
            bool   isNewUser ) 
        {
            _userName = userName;
            _password = password;
            _isNewUser = isNewUser;
            _cancel = false;
        }

        public string UserName
        {
            get{ return _userName; }
        }

        public string Password
        {
            get{ return _password; }
        }

        public bool IsNewUser
        {
            get{ return _isNewUser; }
        }

        public bool Cancel
        {
            get{ return _cancel; }            
            set{ _cancel = value; }
        }

        public Exception FailureInformation
        {
            get{ return _failureInformation; }
            set{ _failureInformation = value; }
        }
    }
}
