//------------------------------------------------------------------------------
// <copyright file="PartialTrustHelpers.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services
{
    using System.Security;
    using System.Web.Hosting;
    
    internal static class PartialTrustHelpers
    {
        [SecurityCritical]
        private static bool isInPartialTrustOutsideAspNet; // indicates if we are running in partial trust outside the ASP.NET context

        [SecurityCritical]
        private static bool isInPartialTrustOutsideAspNetInitialized = false;

        /// <summary>
        /// Used to guard usage of System.Web types in partial trust outside the ASP.NET context (because they are not secure), 
        /// in which case we shutdown the process.
        /// </summary>
        [SecuritySafeCritical] // Critical because it uses security critical fields. Safe because it doesn't take user input and it doesn't leak security sensitive information.
        internal static void FailIfInPartialTrustOutsideAspNet()
        {
            if (!isInPartialTrustOutsideAspNetInitialized)
            {
                // The HostingEnvironment.IsHosted property is safe to be called in partial trust outside the ASP.NET context.
                isInPartialTrustOutsideAspNet = !(AppDomain.CurrentDomain.IsFullyTrusted || HostingEnvironment.IsHosted);
                isInPartialTrustOutsideAspNetInitialized = true;
            }

            if (isInPartialTrustOutsideAspNet)
            {
                throw new SecurityException(Res.GetString(Res.CannotRunInPartialTrustOutsideAspNet));
            }
        }
    }
}
