//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime
{
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.CompilerServices;
    using System.Reflection;

    static class PartialTrustHelpers
    {
        [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision")]
        [SecurityCritical]
        static Type aptca;

        [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision")]
        [SecurityCritical]
        static volatile bool checkedForFullTrust;
        [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision")]
        [SecurityCritical]
        static bool inFullTrust;

        internal static bool ShouldFlowSecurityContext
        {
            [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision")]
            [SecurityCritical]
            get
            {
                return SecurityManager.CurrentThreadRequiresSecurityContextCapture();
            }
        }

        [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision")]
        [SecurityCritical]
        internal static bool IsInFullTrust()
        {
            if (!SecurityManager.CurrentThreadRequiresSecurityContextCapture())
            {
                return true;
            }

            try
            {
                DemandForFullTrust();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Captures security context with identity flow suppressed, " +
            "this requires satisfying a LinkDemand for infrastructure.")]
        [SecurityCritical]
        internal static SecurityContext CaptureSecurityContextNoIdentityFlow()
        {
            // capture the security context but never flow windows identity
            if (SecurityContext.IsWindowsIdentityFlowSuppressed())
            {
                return SecurityContext.Capture();
            }
            else
            {
                using (SecurityContext.SuppressFlowWindowsIdentity())
                {
                    return SecurityContext.Capture();
                }
            }
        }
                
        [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision")]
        [SecurityCritical]
        internal static bool IsTypeAptca(Type type)
        {
            Assembly assembly = type.Assembly;
            return IsAssemblyAptca(assembly) || !IsAssemblySigned(assembly);
        }

        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void DemandForFullTrust()
        {
        }

        [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision")]
        [SecurityCritical]
        static bool IsAssemblyAptca(Assembly assembly)
        {
            if (aptca == null)
            {
                aptca = typeof(AllowPartiallyTrustedCallersAttribute);
            }
            return assembly.GetCustomAttributes(aptca, false).Length > 0;
        }

        [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision")]
        [SecurityCritical]
        [FileIOPermission(SecurityAction.Assert, Unrestricted = true)]
        static bool IsAssemblySigned(Assembly assembly)
        {
            byte[] publicKeyToken = assembly.GetName().GetPublicKeyToken();
            return publicKeyToken != null & publicKeyToken.Length > 0;
        }

        [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision")]
        [SecurityCritical]
        internal static bool CheckAppDomainPermissions(PermissionSet permissions)
        {
            return AppDomain.CurrentDomain.IsHomogenous &&
                   permissions.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }

        [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision")]
        [SecurityCritical]
        internal static bool HasEtwPermissions()
        {
            //Currently unrestricted permissions are required to create Etw provider. 
            PermissionSet permissions = new PermissionSet(PermissionState.Unrestricted);
            return CheckAppDomainPermissions(permissions);
        }

        internal static bool AppDomainFullyTrusted
        {
            [Fx.Tag.SecurityNote(Critical = "used in a security-sensitive decision",
                Safe = "Does not leak critical resources")]
            [SecuritySafeCritical]
            get
            {
                if (!checkedForFullTrust)
                {
                    inFullTrust = AppDomain.CurrentDomain.IsFullyTrusted;
                    checkedForFullTrust = true;
                }

                return inFullTrust;
            }
        }
    }
}
