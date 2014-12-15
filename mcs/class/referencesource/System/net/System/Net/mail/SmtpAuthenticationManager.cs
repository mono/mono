//-----------------------------------------------------------------------------
// <copyright file="SmtpAuthenticationManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mail
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;

internal static class SmtpAuthenticationManager
    {
        static ArrayList modules = new ArrayList();

        static SmtpAuthenticationManager()
        {
            // 

#if !FEATURE_PAL
            Register(new SmtpNegotiateAuthenticationModule());
            Register(new SmtpNtlmAuthenticationModule());
            Register(new SmtpDigestAuthenticationModule());
#endif // !FEATURE_PAL
            Register(new SmtpLoginAuthenticationModule());
        }

        internal static void Register(ISmtpAuthenticationModule module)
        {
            if (module == null)
                throw new ArgumentNullException("module");

            lock (modules)
            {
                modules.Add(module);
            }
        }

        /*
        // Consider removing.
        internal static void Unregister(ISmtpAuthenticationModule module)
        {
            if (module == null)
                throw new ArgumentNullException("module");

            lock (modules)
            {
                modules.Remove(module);
            }
        }
        */

        /*
        // Consider removing.
        internal static void Unregister(string authenticationType)
        {
            if (authenticationType == null)
                throw new ArgumentNullException("authenticationType");

            lock (modules)
            {
                foreach (ISmtpAuthenticationModule module in modules)
                {
                    if (0 == string.Compare(module.AuthenticationType, authenticationType, StringComparison.OrdinalIgnoreCase))
                    {
                        modules.Remove(module);
                    }
                }
            }
        }
        */

        internal static ISmtpAuthenticationModule[] GetModules()
        {
            lock (modules)
            {
                ISmtpAuthenticationModule[] copy = new ISmtpAuthenticationModule[modules.Count];
                modules.CopyTo(0, copy, 0, modules.Count);
                return copy;
            }
        }
    }
}
