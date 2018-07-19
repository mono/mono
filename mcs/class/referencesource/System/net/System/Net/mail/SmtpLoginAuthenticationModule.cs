//-----------------------------------------------------------------------------
// <copyright file="SmtpLoginAuthenticationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Security.Permissions;
    using System.Security.Authentication.ExtendedProtection;

#if MAKE_MAILCLIENT_PUBLIC
    internal
#else
    internal
#endif
        class SmtpLoginAuthenticationModule : ISmtpAuthenticationModule
    {
        Hashtable sessions = new Hashtable();

        internal SmtpLoginAuthenticationModule()
        {
        }

        #region ISmtpAuthenticationModule Members

        // Security this method will access NetworkCredential properties that demand UnmanagedCode and Environment Permission
        [EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public Authorization Authenticate(string challenge, NetworkCredential credential, object sessionCookie, string spn, ChannelBinding channelBindingToken)
        {
            if(Logging.On)Logging.Enter(Logging.Web, this, "Authenticate", null);
            try {
                lock (this.sessions)
                {
                    NetworkCredential cachedCredential = sessions[sessionCookie] as NetworkCredential;
                    if (cachedCredential == null)
                    {
                        if (credential == null  || credential is SystemNetworkCredential)
                        {
                            return null;
                        }

                        sessions[sessionCookie] = credential;

                        string userName = credential.UserName;
                        string domain = credential.Domain;

                        if (domain!=null && domain.Length > 0) {
                            userName = domain + "\\" + userName;
                        }

                        // 
                        return new Authorization(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(userName)), false);
                    }
                    else
                    {
                        this.sessions.Remove(sessionCookie);

                        // 
                        return new Authorization(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(cachedCredential.Password)), true);
                    }
                }
            } finally {
                if(Logging.On)Logging.Exit(Logging.Web, this, "Authenticate", null);
            }
        }

        public string AuthenticationType
        {
            get
            {
                return "login";
            }
        }

        public void CloseContext(object sessionCookie) {
            // This is a no-op since the context is not
            // kept open by this module beyond auth completion.
        }

        #endregion
    }
}
