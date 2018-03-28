//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    static class AspNetPartialTrustHelpers
    {
        [Fx.Tag.SecurityNote(Critical = "Caches the PermissionSet associated with the asp.net trust level."
            + "This will not change over the life of the AppDomain.")]
        [SecurityCritical]
        static SecurityContext aspNetSecurityContext;

        [Fx.Tag.SecurityNote(Critical = "If erroneously set to true, could bypass the PermitOnly.")]
        [SecurityCritical]
        static bool isInitialized;

        [Fx.Tag.SecurityNote(Critical = "Critical field used to prevent usage of System.Web types in partial trust outside the ASP.NET context.")]
        [SecurityCritical]
        private static bool isInPartialTrustOutsideAspNet; // indicates if we are running in partial trust outside the ASP.NET context

        [Fx.Tag.SecurityNote(Critical = "Critical field used to prevent usage of System.Web types in partial trust outside the ASP.NET context.")]
        [SecurityCritical]
        private static bool isInPartialTrustOutsideAspNetInitialized = false;

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - determines if the given PermissionSet is full trust."
            + "We will base subsequent security decisions on this.")]
        static bool IsFullTrust(PermissionSet perms)
        {
            return perms == null || perms.IsUnrestricted();
        }

        internal static bool NeedPartialTrustInvoke
        {
            [SuppressMessage(FxCop.Category.Security, "CA2107:ReviewDenyAndPermitOnlyUsage")]
            [Fx.Tag.SecurityNote(Critical = "Makes a security sensitive decision, updates aspNetSecurityContext and isInitialized.",
                Safe = "Ok to know whether the ASP app is partial trust.")]
            [SecuritySafeCritical]
            get
            {
                if (!isInitialized)
                {
                    FailIfInPartialTrustOutsideAspNet();
                    NamedPermissionSet aspNetPermissionSet = GetHttpRuntimeNamedPermissionSet();
                    if (!IsFullTrust(aspNetPermissionSet))
                    {
                        try
                        {
                            aspNetPermissionSet.PermitOnly();
                            aspNetSecurityContext = System.Runtime.PartialTrustHelpers.CaptureSecurityContextNoIdentityFlow();
                        }
                        finally
                        {
                            CodeAccessPermission.RevertPermitOnly();
                        }
                    }
                    isInitialized = true;
                }
                return aspNetSecurityContext != null;
            }
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts, Justification = "Users cannot pass arbitrary data to this code.")]
        [Fx.Tag.SecurityNote(Critical = "Asserts AspNetHostingPermission.")]
        [SecurityCritical]
        [AspNetHostingPermission(SecurityAction.Assert, Level = AspNetHostingPermissionLevel.Unrestricted)]
        static NamedPermissionSet GetHttpRuntimeNamedPermissionSet()
        {
            return HttpRuntime.GetNamedPermissionSet();
        }

        [Fx.Tag.SecurityNote(Critical = "Touches aspNetSecurityContext.",
            Safe = "Ok to invoke the user's delegate under the PT context.")]
        [SecuritySafeCritical]
        internal static void PartialTrustInvoke(ContextCallback callback, object state)
        {
            if (NeedPartialTrustInvoke)
            {
                SecurityContext.Run(aspNetSecurityContext.CreateCopy(), callback, state);
            }
            else
            {
                callback(state);
            }
        }

        /// <summary>
        /// Used to guard usage of System.Web types in partial trust outside the ASP.NET context (because they are not secure), 
        /// in which case we shutdown the process.
        /// </summary>
        [Fx.Tag.SecurityNote(Critical = "Critical because it uses security critical fields.",
           Safe = "Safe because it doesn't take user input and it doesn't leak security sensitive information.")]
        [SecuritySafeCritical]
        internal static void FailIfInPartialTrustOutsideAspNet()
        {
            if (!isInPartialTrustOutsideAspNetInitialized)
            {
                // The HostingEnvironment.IsHosted property is safe to be called in partial trust outside the ASP.NET context.
                isInPartialTrustOutsideAspNet = !(PartialTrustHelpers.AppDomainFullyTrusted || HostingEnvironment.IsHosted);
                isInPartialTrustOutsideAspNetInitialized = true;
            }

            if (isInPartialTrustOutsideAspNet)
            {
                throw FxTrace.Exception.AsError(new SecurityException(Activation.SR.CannotRunInPartialTrustOutsideAspNet));
            }
        }
    }
}
