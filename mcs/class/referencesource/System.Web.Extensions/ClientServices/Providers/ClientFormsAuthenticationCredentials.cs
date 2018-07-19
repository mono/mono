//------------------------------------------------------------------------------
// <copyright file="ClientFormsAuthenticationCredentials.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.ClientServices.Providers
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public class ClientFormsAuthenticationCredentials
    {

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="username", Justification="consistent with Whidbey")]
        public ClientFormsAuthenticationCredentials(string username, string password, bool rememberMe)
        {
            _UserName = username;
            _Password = password;
            _RememberMe = rememberMe;
        }

        public string UserName { get { return _UserName; } set { _UserName = value; } }
        public string Password { get { return _Password; } set { _Password = value; } }
        public bool RememberMe { get { return _RememberMe; } set { _RememberMe = value; } }

        private string _UserName;
        private string _Password;
        private bool _RememberMe;
    }
}
