//-----------------------------------------------------------------------------
// <copyright file="SmtpNtlmAuthenticationModule.cs" company="Microsoft">
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

    // 

#if MAKE_MAILCLIENT_PUBLIC
    internal
#else
    internal
#endif
        class SmtpNtlmAuthenticationModule : ISmtpAuthenticationModule
    {
        Hashtable sessions = new Hashtable();

        internal SmtpNtlmAuthenticationModule()
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
                    NTAuthentication clientContext = this.sessions[sessionCookie] as NTAuthentication;
                    if (clientContext == null)
                    {
                        if(credential == null){
                            return null;
                        }

                        this.sessions[sessionCookie] =
                            clientContext =
                            new NTAuthentication(false, "Ntlm", credential, spn, ContextFlags.Connection, channelBindingToken);

                    }

                    string resp = clientContext.GetOutgoingBlob(challenge);


                    if (!clientContext.IsCompleted)
                    {
                        return new Authorization(resp, false);
                    }
                    else
                    {
                        this.sessions.Remove(sessionCookie);
                        return new Authorization(resp, true);
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
                return "ntlm";
            }
        }

        public void CloseContext(object sessionCookie) {
            // This is a no-op since the context is not
            // kept open by this module beyond auth completion.
        }
        
        #endregion
    }
}
