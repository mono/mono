//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Threading;
    using ClaimsIdentity = System.Security.Claims.ClaimsIdentity;
    using ClaimsPrincipal = System.Security.Claims.ClaimsPrincipal;
    using EXTENDED_NAME_FORMAT = System.ServiceModel.ComIntegration.EXTENDED_NAME_FORMAT;
    using SafeCloseHandle = System.IdentityModel.SafeCloseHandle;
    using SafeNativeMethods = System.ServiceModel.ComIntegration.SafeNativeMethods;
    using Win32Error = System.ServiceModel.ComIntegration.Win32Error;
    
    internal sealed class SecurityImpersonationBehavior
    {
        PrincipalPermissionMode principalPermissionMode;
        object roleProvider;
        bool impersonateCallerForAllOperations;
        Dictionary<string, string> domainNameMap;
        Random random;
        const int maxDomainNameMapSize = 5;

        static WindowsPrincipal anonymousWindowsPrincipal;
        AuditLevel auditLevel = ServiceSecurityAuditBehavior.defaultMessageAuthenticationAuditLevel;
        AuditLogLocation auditLogLocation = ServiceSecurityAuditBehavior.defaultAuditLogLocation;
        bool suppressAuditFailure = ServiceSecurityAuditBehavior.defaultSuppressAuditFailure;

        SecurityImpersonationBehavior(DispatchRuntime dispatch)
        {
            this.principalPermissionMode = dispatch.PrincipalPermissionMode;
            this.impersonateCallerForAllOperations = dispatch.ImpersonateCallerForAllOperations;
            this.auditLevel = dispatch.MessageAuthenticationAuditLevel;
            this.auditLogLocation = dispatch.SecurityAuditLogLocation;
            this.suppressAuditFailure = dispatch.SuppressAuditFailure;
            if (dispatch.IsRoleProviderSet)
            {
                ApplyRoleProvider(dispatch);
            }
            this.domainNameMap = new Dictionary<string, string>(maxDomainNameMapSize, StringComparer.OrdinalIgnoreCase);
        }

        public static SecurityImpersonationBehavior CreateIfNecessary(DispatchRuntime dispatch)
        {
            if (IsSecurityBehaviorNeeded(dispatch))
            {
                return new SecurityImpersonationBehavior(dispatch);
            }
            else
            {
                return null;
            }
        }

        static WindowsPrincipal AnonymousWindowsPrincipal
        {
            get
            {
                if (anonymousWindowsPrincipal == null)
                    anonymousWindowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetAnonymous());

                return anonymousWindowsPrincipal;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void ApplyRoleProvider(DispatchRuntime dispatch)
        {
            this.roleProvider = dispatch.RoleProvider;
        }

        static bool IsSecurityBehaviorNeeded(DispatchRuntime dispatch)
        {
            if (AspNetEnvironment.Current.RequiresImpersonation)
            {
                return true;
            }

            if (dispatch.PrincipalPermissionMode != PrincipalPermissionMode.None)
            {
                return true;
            }

            // Impersonation behavior is required if 
            // 1) Contract requires it or 
            // 2) Contract allows it and config requires it
            for (int i = 0; i < dispatch.Operations.Count; i++)
            {
                DispatchOperation operation = dispatch.Operations[i];

                if (operation.Impersonation == ImpersonationOption.Required)
                {
                    return true;
                }
                else if (operation.Impersonation == ImpersonationOption.NotAllowed)
                {
                    // a validation rule enforces that config cannot require impersonation in this case
                    return false;
                }
            }
            // contract allows impersonation. Return true if config requires it.
            return dispatch.ImpersonateCallerForAllOperations;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        IPrincipal SetCurrentThreadPrincipal(ServiceSecurityContext securityContext, out bool isThreadPrincipalSet)
        {
            IPrincipal result = null;
            IPrincipal principal = null;

            ClaimsPrincipal claimsPrincipal = OperationContext.Current.ClaimsPrincipal;

            if (this.principalPermissionMode == PrincipalPermissionMode.UseWindowsGroups)
            {
                principal = ( claimsPrincipal is WindowsPrincipal ) ? claimsPrincipal : GetWindowsPrincipal( securityContext );
            }
            else if (this.principalPermissionMode == PrincipalPermissionMode.UseAspNetRoles)
            {
                principal = new RoleProviderPrincipal(this.roleProvider, securityContext);
            }
            else if (this.principalPermissionMode == PrincipalPermissionMode.Custom)
            {
                principal = GetCustomPrincipal(securityContext);
            }
            else if (this.principalPermissionMode == PrincipalPermissionMode.Always)
            {
                principal = claimsPrincipal ?? new ClaimsPrincipal( new ClaimsIdentity() );
            }

            if (principal != null)
            {
                result = Thread.CurrentPrincipal;
                Thread.CurrentPrincipal = principal;
                isThreadPrincipalSet = true;
            }
            else
            {
                isThreadPrincipalSet = false;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static IPrincipal GetCustomPrincipal(ServiceSecurityContext securityContext)
        {
            object customPrincipal;
            if (securityContext.AuthorizationContext.Properties.TryGetValue(SecurityUtils.Principal, out customPrincipal) && customPrincipal is IPrincipal)
                return (IPrincipal)customPrincipal;
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoPrincipalSpecifiedInAuthorizationContext)));
        }        

        internal bool IsSecurityContextImpersonationRequired(ref MessageRpc rpc)
        {
            return ((rpc.Operation.Impersonation == ImpersonationOption.Required)
                || ((rpc.Operation.Impersonation == ImpersonationOption.Allowed) && this.impersonateCallerForAllOperations));
        }

        internal bool IsImpersonationEnabledOnCurrentOperation(ref MessageRpc rpc)
        {
            return this.IsSecurityContextImpersonationRequired(ref rpc) ||
                    AspNetEnvironment.Current.RequiresImpersonation ||
                    this.principalPermissionMode != PrincipalPermissionMode.None;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method StartImpersonation2."
            + "Caller must ensure that this method is called at an appropriate time and that impersonationContext out param is Dispose()'d correctly.")]
        [SecurityCritical]
        public void StartImpersonation(ref MessageRpc rpc, out IDisposable impersonationContext, out IPrincipal originalPrincipal, out bool isThreadPrincipalSet)
        {
            impersonationContext = null;
            originalPrincipal = null;
            isThreadPrincipalSet = false;
            ServiceSecurityContext securityContext;
            bool setThreadPrincipal = this.principalPermissionMode != PrincipalPermissionMode.None;
            bool isSecurityContextImpersonationOn = IsSecurityContextImpersonationRequired(ref rpc);
            if (setThreadPrincipal || isSecurityContextImpersonationOn)
                securityContext = GetAndCacheSecurityContext(ref rpc);
            else
                securityContext = null;

            if (setThreadPrincipal && securityContext != null)
                originalPrincipal = this.SetCurrentThreadPrincipal(securityContext, out isThreadPrincipalSet);

            if (isSecurityContextImpersonationOn || AspNetEnvironment.Current.RequiresImpersonation)
            {
                impersonationContext = StartImpersonation2(ref rpc, securityContext, isSecurityContextImpersonationOn);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method HostedImpersonationContext.Impersonate."
            + "Caller must ensure that this method is called at an appropriate time and that result is Dispose()'d correctly.")]
        [SecurityCritical]
        IDisposable StartImpersonation2(ref MessageRpc rpc, ServiceSecurityContext securityContext, bool isSecurityContextImpersonationOn)
        {
            IDisposable impersonationContext = null;
            try
            {
                if (isSecurityContextImpersonationOn)
                {
                    if (securityContext == null)
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxSecurityContextPropertyMissingFromRequestMessage)), rpc.Request);

                    WindowsIdentity impersonationToken = securityContext.WindowsIdentity;
                    if (impersonationToken.User != null)
                    {
                        impersonationContext = impersonationToken.Impersonate();
                    }
                    else if (securityContext.PrimaryIdentity is WindowsSidIdentity)
                    {
                        WindowsSidIdentity sidIdentity = (WindowsSidIdentity)securityContext.PrimaryIdentity;
                        if (sidIdentity.SecurityIdentifier.IsWellKnown(WellKnownSidType.AnonymousSid))
                        {
                            impersonationContext = new WindowsAnonymousIdentity().Impersonate();
                        }
                        else
                        {
                            string fullyQualifiedDomainName = GetUpnFromDownlevelName(sidIdentity.Name);
                            using (WindowsIdentity windowsIdentity = new WindowsIdentity(fullyQualifiedDomainName, SecurityUtils.AuthTypeKerberos))
                            {
                                impersonationContext = windowsIdentity.Impersonate();
                            }
                        }
                    }
                    else
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityContextDoesNotAllowImpersonation, rpc.Operation.Action)), rpc.Request);
                }
                else if (AspNetEnvironment.Current.RequiresImpersonation)
                {
                    if (rpc.HostingProperty != null)
                    {
                        impersonationContext = rpc.HostingProperty.Impersonate();
                    }
                }

                SecurityTraceRecordHelper.TraceImpersonationSucceeded(rpc.EventTraceActivity, rpc.Operation);

                // update the impersonation succeed audit
                if (AuditLevel.Success == (this.auditLevel & AuditLevel.Success))
                {
                    SecurityAuditHelper.WriteImpersonationSuccessEvent(this.auditLogLocation,
                        this.suppressAuditFailure, rpc.Operation.Name, SecurityUtils.GetIdentityNamesFromContext(securityContext.AuthorizationContext));
                }
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }
                SecurityTraceRecordHelper.TraceImpersonationFailed(rpc.EventTraceActivity, rpc.Operation, ex);

                //
                // Update the impersonation failure audit
                // Copy SecurityAuthorizationBehavior.Audit level to here!!!
                //
                if (AuditLevel.Failure == (this.auditLevel & AuditLevel.Failure))
                {
                    try
                    {
                        string primaryIdentity;
                        if (securityContext != null)
                            primaryIdentity = SecurityUtils.GetIdentityNamesFromContext(securityContext.AuthorizationContext);
                        else
                            primaryIdentity = SecurityUtils.AnonymousIdentity.Name;

                        SecurityAuditHelper.WriteImpersonationFailureEvent(this.auditLogLocation,
                            this.suppressAuditFailure, rpc.Operation.Name, primaryIdentity, ex);
                    }
#pragma warning suppress 56500
                    catch (Exception auditException)
                    {
                        if (Fx.IsFatal(auditException))
                            throw;

                        DiagnosticUtility.TraceHandledException(auditException, TraceEventType.Error);
                    }
                }

                throw;
            }

            return impersonationContext;
        }

        public void StopImpersonation(ref MessageRpc rpc, IDisposable impersonationContext, IPrincipal originalPrincipal, bool isThreadPrincipalSet)
        {
            try
            {
                if (IsSecurityContextImpersonationRequired(ref rpc) || AspNetEnvironment.Current.RequiresImpersonation)
                {
                    if (impersonationContext != null)
                    {
                        impersonationContext.Dispose();
                    }
                }

                if (isThreadPrincipalSet)
                {
                    Thread.CurrentPrincipal = originalPrincipal;
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch
            {
                string message = null;
                try
                {
                    message = SR.GetString(SR.SFxRevertImpersonationFailed0);
                }
                finally
                {
                    DiagnosticUtility.FailFast(message);
                }
            }
        }

        IPrincipal GetWindowsPrincipal(ServiceSecurityContext securityContext)
        {
            WindowsIdentity wid = securityContext.WindowsIdentity;
            if (!wid.IsAnonymous)
                return new WindowsPrincipal(wid);

            WindowsSidIdentity wsid = securityContext.PrimaryIdentity as WindowsSidIdentity;
            if (wsid != null)
                return new WindowsSidPrincipal(wsid, securityContext);

            return AnonymousWindowsPrincipal;
        }

        ServiceSecurityContext GetAndCacheSecurityContext(ref MessageRpc rpc)
        {
            ServiceSecurityContext securityContext = rpc.SecurityContext;

            if (!rpc.HasSecurityContext)
            {
                SecurityMessageProperty securityContextProperty = rpc.Request.Properties.Security;
                if (securityContextProperty == null)
                    securityContext = null; // SecurityContext.Anonymous
                else
                {
                    securityContext = securityContextProperty.ServiceSecurityContext;
                    if (securityContext == null)
                        throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityContextMissing, rpc.Operation.Name)), rpc.Request);
                }

                rpc.SecurityContext = securityContext;
                rpc.HasSecurityContext = true;
            }

            return securityContext;
        }

        string GetUpnFromDownlevelName(string downlevelName)
        {
            if (downlevelName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("downlevelName");
            }
            int delimiterPos = downlevelName.IndexOf('\\');
            if ((delimiterPos < 0) || (delimiterPos == 0) || (delimiterPos == downlevelName.Length - 1))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.DownlevelNameCannotMapToUpn, downlevelName)));
            }
            string shortDomainName = downlevelName.Substring(0, delimiterPos + 1);
            string userName = downlevelName.Substring(delimiterPos + 1);
            string fullDomainName;
            bool found;

            // 1) Read from cache
            lock (this.domainNameMap)
            {
                found = this.domainNameMap.TryGetValue(shortDomainName, out fullDomainName);
            }

            // 2) Not found, do expensive look up
            if (!found)
            {
                uint capacity = 50;
                StringBuilder fullyQualifiedDomainName = new StringBuilder((int)capacity);
                if (!SafeNativeMethods.TranslateName(shortDomainName, EXTENDED_NAME_FORMAT.NameSamCompatible, EXTENDED_NAME_FORMAT.NameCanonical,
                    fullyQualifiedDomainName, out capacity))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    if (errorCode == (int)Win32Error.ERROR_INSUFFICIENT_BUFFER)
                    {
                        fullyQualifiedDomainName = new StringBuilder((int)capacity);
                        if (!SafeNativeMethods.TranslateName(shortDomainName, EXTENDED_NAME_FORMAT.NameSamCompatible, EXTENDED_NAME_FORMAT.NameCanonical,
                            fullyQualifiedDomainName, out capacity))
                        {
                            errorCode = Marshal.GetLastWin32Error();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.DownlevelNameCannotMapToUpn, downlevelName), new Win32Exception(errorCode)));
                        }
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new InvalidOperationException(SR.GetString(SR.DownlevelNameCannotMapToUpn, downlevelName), new Win32Exception(errorCode)));
                    }
                }
                // trim the trailing / from fqdn
                fullyQualifiedDomainName = fullyQualifiedDomainName.Remove(fullyQualifiedDomainName.Length - 1, 1);
                fullDomainName = fullyQualifiedDomainName.ToString();

                // 3) Save in cache (remove a random item if cache is full)
                lock (this.domainNameMap)
                {
                    if (this.domainNameMap.Count >= maxDomainNameMapSize)
                    {
                        if (this.random == null)
                        {
                            this.random = new Random(unchecked((int)DateTime.Now.Ticks));
                        }
                        int victim = this.random.Next() % this.domainNameMap.Count;
                        foreach (string key in this.domainNameMap.Keys)
                        {
                            if (victim <= 0)
                            {
                                this.domainNameMap.Remove(key);
                                break;
                            }
                            --victim;
                        }
                    }
                    this.domainNameMap[shortDomainName] = fullDomainName;
                }
            }
            return userName + "@" + fullDomainName;
        }


        class WindowsSidPrincipal : IPrincipal
        {
            WindowsSidIdentity identity;
            ServiceSecurityContext securityContext;

            public WindowsSidPrincipal(WindowsSidIdentity identity, ServiceSecurityContext securityContext)
            {
                this.identity = identity;
                this.securityContext = securityContext;
            }

            public IIdentity Identity
            {
                get { return this.identity; }
            }

            public bool IsInRole(string role)
            {
                if (role == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("role");

                NTAccount account = new NTAccount(role);
                Claim claim = Claim.CreateWindowsSidClaim((SecurityIdentifier)account.Translate(typeof(SecurityIdentifier)));
                AuthorizationContext authContext = this.securityContext.AuthorizationContext;
                for (int i = 0; i < authContext.ClaimSets.Count; i++)
                {
                    ClaimSet claimSet = authContext.ClaimSets[i];
                    if (claimSet.ContainsClaim(claim))
                        return true;
                }
                return false;
            }
        }

        class WindowsAnonymousIdentity
        {
            public IDisposable Impersonate()
            {
                // PreSharp 
#pragma warning suppress 56523 // The LastWin32Error can be ignored here.
                IntPtr threadHandle = SafeNativeMethods.GetCurrentThread();
                SafeCloseHandle tokenHandle;
                if (!SafeNativeMethods.OpenCurrentThreadToken(threadHandle, TokenAccessLevels.Impersonate, true, out tokenHandle))
                {
                    int error = Marshal.GetLastWin32Error();
                    System.ServiceModel.Diagnostics.Utility.CloseInvalidOutSafeHandle(tokenHandle);
                    if (error == (int)System.ServiceModel.ComIntegration.Win32Error.ERROR_NO_TOKEN)
                    {
                        tokenHandle = new SafeCloseHandle(IntPtr.Zero, false);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                    }
                }

                if (!SafeNativeMethods.ImpersonateAnonymousUserOnCurrentThread(threadHandle))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }

                return new ImpersonationContext(threadHandle, tokenHandle);
            }

            class ImpersonationContext : IDisposable
            {
                IntPtr threadHandle;
                SafeCloseHandle tokenHandle;
                bool disposed = false;

                public ImpersonationContext(IntPtr threadHandle, SafeCloseHandle tokenHandle)
                {
                    this.threadHandle = threadHandle;
                    this.tokenHandle = tokenHandle;
                }

                void Undo()
                {
                    // PreSharp 
#pragma warning suppress 56523 // The LastWin32Error can be ignored here.
                    Fx.Assert(this.threadHandle == SafeNativeMethods.GetCurrentThread(), "");
                    // We are in the Dispose method. If a failure occurs we just have to ignore it.
                    // PreSharp 

                    if (!SafeNativeMethods.SetCurrentThreadToken(IntPtr.Zero, this.tokenHandle))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(SR.GetString(SR.RevertImpersonationFailure,
                            new Win32Exception(error).Message)));
                    }
                    tokenHandle.Close();
                }

                public void Dispose()
                {
                    if (!this.disposed)
                    {
                        Undo();
                    }
                    this.disposed = true;
                }
            }
        }
    }
}

