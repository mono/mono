//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Hosting;

    // wrapper class that helps with partial trust analysis 
    // -- HostingEnvironment does a number of Demands and LinkDemands
    // -- this wrapper encapsulates access into "Safe" and "Unsafe" methods that do the appropriate asserts
    // -- it is recommended that ALL HostingEnvironment access go through this class
    // -- "Safe" methods are [SecurityCritical, SecurityTreatAsSafe] or not [SecurityCritical]
    // -- "Unsafe" methods are [SecurityCritical]
    // -- because each method does precisely one access, we use declarative asserts for clarity
    static class HostingEnvironmentWrapper
    {
        public static string ApplicationVirtualPath
        {
            get
            {
                AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
                return HostingEnvironment.ApplicationVirtualPath;
            }
        }

        public static bool IsHosted
        {
            get { return HostingEnvironment.IsHosted; }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        public static VirtualPathProvider VirtualPathProvider
        {
            [SecuritySafeCritical]
            get 
            {
                AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
                return HostingEnvironment.VirtualPathProvider; 
            }
        }
                
        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void DecrementBusyCount()
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            HostingEnvironment.DecrementBusyCount();
        }

        // demands SecurityPermission(ControlPrincipal) -- use Unsafe version to assert
        public static IDisposable Impersonate()
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            return HostingEnvironment.Impersonate();
        }

        // demands SecurityPermission(Unrestricted) -- use Unsafe version to assert
        public static IDisposable Impersonate(IntPtr token)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            return HostingEnvironment.Impersonate(token);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects.",
            Safe = "Does not leak control or mutable/harmful data, no potential for harm.")]
        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void IncrementBusyCount()
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            HostingEnvironment.IncrementBusyCount();
        }

        public static string MapPath(string virtualPath)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            return HostingEnvironment.MapPath(virtualPath);
        }
        
        public static string UnsafeApplicationID
        {
            [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts, Justification = "Users cannot pass arbitrary data to this code.")]
            [Fx.Tag.SecurityNote(Critical = "Asserts SecurityPermission in order to call HostingEnvironment.get_ApplicationID.")]
            [SecurityCritical]
            [AspNetHostingPermission(System.Security.Permissions.SecurityAction.Assert, Level = AspNetHostingPermissionLevel.High)]
            get 
            {
                AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
                return HostingEnvironment.ApplicationID; 
            }
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts, Justification = "Users cannot pass arbitrary data to this code.")]
        [Fx.Tag.SecurityNote(Critical = "Asserts SecurityPermission in order to call HostingEnvironment.UnsafeImpersonate.")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, ControlPrincipal = true)]
        public static IDisposable UnsafeImpersonate()
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            return HostingEnvironment.Impersonate();
        }

        [Fx.Tag.SecurityNote(Critical = "Asserts SecurityPermission in order to call HostingEnvironment.UnsafeImpersonate.")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static IDisposable UnsafeImpersonate(IntPtr token)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            return HostingEnvironment.Impersonate(token);
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.SecureAsserts, Justification = "This is an internal SecurityCritical method and its only caller passes in non-user data. Users cannot pass arbitrary data to this code.")]
        [Fx.Tag.SecurityNote(Critical = "Asserts SecurityPermission in order to call HostingEnvironment.RegisterObject.")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void UnsafeRegisterObject(IRegisteredObject target)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            HostingEnvironment.RegisterObject(target);
        }

        [Fx.Tag.SecurityNote(Critical = "Asserts SecurityPermission in order to call HostingEnvironment.UnregisterObject.")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public static void UnsafeUnregisterObject(IRegisteredObject target)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            HostingEnvironment.UnregisterObject(target);
        }

        [Fx.Tag.SecurityNote(Critical = "Uses SecurityCritical method UnsafeImpersonate in order to check whether the service file exists.",
            Safe = "Does not leak anything, does not let caller influence impersonation.")]
        [SecuritySafeCritical]
        public static bool ServiceFileExists(string normalizedVirtualPath)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            IDisposable unsafeImpersonate = null;
            try
            {
                try
                {
                    try 
                    {
                    }
                    finally
                    {
                        unsafeImpersonate = HostingEnvironmentWrapper.UnsafeImpersonate();
                    }
                    return HostingEnvironment.VirtualPathProvider.FileExists(normalizedVirtualPath);
                }
                finally
                {
                    if (null != unsafeImpersonate)
                    {
                        unsafeImpersonate.Dispose();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses SecurityCritical method UnsafeImpersonate in order to get service file.",
            Safe = "Does not leak anything, does not let caller influence impersonation.")]
        [SecuritySafeCritical]
        public static VirtualFile GetServiceFile(string normalizedVirtualPath)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            IDisposable unsafeImpersonate = null;
            try
            {
                try
                {
                    try 
                    {
                    }
                    finally
                    {
                        unsafeImpersonate = HostingEnvironmentWrapper.UnsafeImpersonate();
                    }
                    return HostingEnvironment.VirtualPathProvider.GetFile(normalizedVirtualPath);
                }
                finally
                {
                    if (null != unsafeImpersonate)
                    {
                        unsafeImpersonate.Dispose();
                    }
                }
            }
            catch
            {
                throw;
            }            
        }     
    }
}
