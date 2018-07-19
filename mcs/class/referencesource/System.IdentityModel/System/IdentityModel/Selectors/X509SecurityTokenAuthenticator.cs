//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    public class X509SecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        X509CertificateValidator validator;
        bool mapToWindows;
        bool includeWindowsGroups;
        bool cloneHandle;

        public X509SecurityTokenAuthenticator()
            : this(X509CertificateValidator.ChainTrust)
        {
        }

        public X509SecurityTokenAuthenticator(X509CertificateValidator validator)
            : this(validator, false)
        {
        }

        public X509SecurityTokenAuthenticator(X509CertificateValidator validator, bool mapToWindows)
            : this(validator, mapToWindows, WindowsClaimSet.DefaultIncludeWindowsGroups)
        {
        }

        public X509SecurityTokenAuthenticator(X509CertificateValidator validator, bool mapToWindows, bool includeWindowsGroups)
            : this(validator, mapToWindows, includeWindowsGroups, true)
        {
        }

        internal X509SecurityTokenAuthenticator(X509CertificateValidator validator, bool mapToWindows, bool includeWindowsGroups, bool cloneHandle)
        {
            if (validator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("validator");
            }

            this.validator = validator;
            this.mapToWindows = mapToWindows;
            this.includeWindowsGroups = includeWindowsGroups;
            this.cloneHandle = cloneHandle;
        }

        public bool MapCertificateToWindowsAccount
        {
            get { return this.mapToWindows; }
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return token is X509SecurityToken;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            X509SecurityToken x509Token = (X509SecurityToken)token;
            this.validator.Validate(x509Token.Certificate);

            X509CertificateClaimSet x509ClaimSet = new X509CertificateClaimSet(x509Token.Certificate, this.cloneHandle);
            if (!this.mapToWindows)
                return SecurityUtils.CreateAuthorizationPolicies(x509ClaimSet, x509Token.ValidTo);

            WindowsClaimSet windowsClaimSet;
            if (token is X509WindowsSecurityToken)
            {
                windowsClaimSet = new WindowsClaimSet( ( (X509WindowsSecurityToken)token ).WindowsIdentity, SecurityUtils.AuthTypeCertMap, this.includeWindowsGroups, this.cloneHandle );
            }
            else
            {
                // Ensure NT_AUTH chain policy for certificate account mapping
                X509CertificateValidator.NTAuthChainTrust.Validate(x509Token.Certificate);

                WindowsIdentity windowsIdentity = null;
                // for Vista, LsaLogon supporting mapping cert to NTToken
                if (Environment.OSVersion.Version.Major >= SecurityUtils.WindowsVistaMajorNumber)
                {
                    windowsIdentity = KerberosCertificateLogon(x509Token.Certificate);
                }
                else
                {
                    // Downlevel, S4U over PrincipalName SubjectAltNames
                    string name = x509Token.Certificate.GetNameInfo(X509NameType.UpnName, false);
                    if (string.IsNullOrEmpty(name))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.InvalidNtMapping,
                            SecurityUtils.GetCertificateId(x509Token.Certificate))));
                    }

                    using (WindowsIdentity initialWindowsIdentity = new WindowsIdentity(name, SecurityUtils.AuthTypeCertMap))
                    {
                        // This is to make sure that the auth Type is shoved down to the class as the above constructor does not do it.
                        windowsIdentity = new WindowsIdentity(initialWindowsIdentity.Token, SecurityUtils.AuthTypeCertMap);
                    }
                }

                windowsClaimSet = new WindowsClaimSet(windowsIdentity, SecurityUtils.AuthTypeCertMap, this.includeWindowsGroups, false);
            }
            List<ClaimSet> claimSets = new List<ClaimSet>(2);
            claimSets.Add(windowsClaimSet);
            claimSets.Add(x509ClaimSet);

            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            policies.Add(new UnconditionalPolicy(claimSets.AsReadOnly(), x509Token.ValidTo));
            return policies.AsReadOnly();
        }

        // For Vista, LsaLogon supporting mapping cert to NTToken.  W/O SeTcbPrivilege
        // the identify token is returned; otherwise, impersonation.  This is consistent
        // with S4U.  Note: duplicate code partly from CLR's WindowsIdentity Class (S4U).
        [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeHGlobalHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        internal static WindowsIdentity KerberosCertificateLogon(X509Certificate2 certificate)
        {
            int status;
            SafeHGlobalHandle pSourceName = null;
            SafeHGlobalHandle pPackageName = null;
            SafeHGlobalHandle pLogonInfo = null;
            SafeLsaLogonProcessHandle logonHandle = null;
            SafeLsaReturnBufferHandle profileHandle = null;
            SafeCloseHandle tokenHandle = null;
            try
            {
                pSourceName = SafeHGlobalHandle.AllocHGlobal(NativeMethods.LsaSourceName.Length + 1);
                Marshal.Copy(NativeMethods.LsaSourceName, 0, pSourceName.DangerousGetHandle(), NativeMethods.LsaSourceName.Length);
                UNICODE_INTPTR_STRING sourceName = new UNICODE_INTPTR_STRING(NativeMethods.LsaSourceName.Length, NativeMethods.LsaSourceName.Length + 1, pSourceName.DangerousGetHandle());

                Privilege privilege = null;

                RuntimeHelpers.PrepareConstrainedRegions();
                // Try to get an impersonation token.
                try
                {
                    // Try to enable the TCB privilege if possible
                    try
                    {
                        privilege = new Privilege(Privilege.SeTcbPrivilege);
                        privilege.Enable();
                    }
                    catch (PrivilegeNotHeldException ex)
                    {
                        DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);
                    }

                    IntPtr dummy = IntPtr.Zero;
                    status = NativeMethods.LsaRegisterLogonProcess(ref sourceName, out logonHandle, out dummy);
                    if (NativeMethods.ERROR_ACCESS_DENIED == NativeMethods.LsaNtStatusToWinError(status))
                    {
                        // We don't have the Tcb privilege. The best we can hope for is to get an Identification token.
                        status = NativeMethods.LsaConnectUntrusted(out logonHandle);
                    }
                    if (status < 0) // non-negative numbers indicate success
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(NativeMethods.LsaNtStatusToWinError(status)));
                    }
                }
                finally
                {
                    // if reverting privilege fails, fail fast!
                    int revertResult = -1;
                    string message = null;
                    try
                    {
                        revertResult = privilege.Revert();
                        if (revertResult != 0)
                        {
                            message = SR.GetString(SR.RevertingPrivilegeFailed, new Win32Exception(revertResult));
                        }
                    }
                    finally
                    {
                        if (revertResult != 0)
                        {
                            DiagnosticUtility.FailFast(message);
                        }
                    }
                }

                // package name ("Kerberos")
                pPackageName = SafeHGlobalHandle.AllocHGlobal(NativeMethods.LsaKerberosName.Length + 1);
                Marshal.Copy(NativeMethods.LsaKerberosName, 0, pPackageName.DangerousGetHandle(), NativeMethods.LsaKerberosName.Length);
                UNICODE_INTPTR_STRING packageName = new UNICODE_INTPTR_STRING(NativeMethods.LsaKerberosName.Length, NativeMethods.LsaKerberosName.Length + 1, pPackageName.DangerousGetHandle());

                uint packageId = 0;
                status = NativeMethods.LsaLookupAuthenticationPackage(logonHandle, ref packageName, out packageId);
                if (status < 0) // non-negative numbers indicate success
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(NativeMethods.LsaNtStatusToWinError(status)));
                }

                // source context
                TOKEN_SOURCE sourceContext = new TOKEN_SOURCE();
                if (!NativeMethods.AllocateLocallyUniqueId(out sourceContext.SourceIdentifier))
                {
                    int dwErrorCode = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(dwErrorCode));
                }

                // SourceContext
                sourceContext.Name = new char[8];
                sourceContext.Name[0] = 'W'; sourceContext.Name[1] = 'C'; sourceContext.Name[2] = 'F';

                // LogonInfo
                byte[] certRawData = certificate.RawData;
                int logonInfoSize = KERB_CERTIFICATE_S4U_LOGON.Size + certRawData.Length;
                pLogonInfo = SafeHGlobalHandle.AllocHGlobal(logonInfoSize);
                unsafe
                {
                    KERB_CERTIFICATE_S4U_LOGON* pInfo = (KERB_CERTIFICATE_S4U_LOGON*)pLogonInfo.DangerousGetHandle().ToPointer();
                    pInfo->MessageType = KERB_LOGON_SUBMIT_TYPE.KerbCertificateS4ULogon;
                    pInfo->Flags = NativeMethods.KERB_CERTIFICATE_S4U_LOGON_FLAG_CHECK_LOGONHOURS;
                    pInfo->UserPrincipalName = new UNICODE_INTPTR_STRING(0, 0, IntPtr.Zero);
                    pInfo->DomainName = new UNICODE_INTPTR_STRING(0, 0, IntPtr.Zero);
                    pInfo->CertificateLength = (uint)certRawData.Length;
                    pInfo->Certificate = new IntPtr(pLogonInfo.DangerousGetHandle().ToInt64() + KERB_CERTIFICATE_S4U_LOGON.Size);
                    Marshal.Copy(certRawData, 0, pInfo->Certificate, certRawData.Length);
                }

                QUOTA_LIMITS quotas = new QUOTA_LIMITS();
                LUID logonId = new LUID();
                uint profileBufferLength;
                int subStatus = 0;

                // Call LsaLogonUser
                status = NativeMethods.LsaLogonUser(
                    logonHandle,
                    ref sourceName,
                    SecurityLogonType.Network,
                    packageId,
                    pLogonInfo.DangerousGetHandle(),
                    (uint)logonInfoSize,
                    IntPtr.Zero,
                    ref sourceContext,
                    out profileHandle,
                    out profileBufferLength,
                    out logonId,
                    out tokenHandle,
                    out quotas,
                    out subStatus
                    );

                // LsaLogon has restriction (eg. password expired).  SubStatus indicates the reason.
                if ((uint)status == NativeMethods.STATUS_ACCOUNT_RESTRICTION && subStatus < 0)
                {
                    status = subStatus;
                }
                if (status < 0) // non-negative numbers indicate success
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(NativeMethods.LsaNtStatusToWinError(status)));
                }
                if (subStatus < 0) // non-negative numbers indicate success
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(NativeMethods.LsaNtStatusToWinError(subStatus)));
                }

                return new WindowsIdentity(tokenHandle.DangerousGetHandle(), SecurityUtils.AuthTypeCertMap);
            }
            finally
            {
                if (tokenHandle != null)
                {
                    tokenHandle.Close();
                }
                if (pLogonInfo != null)
                {
                    pLogonInfo.Close();
                }
                if (profileHandle != null)
                {
                    profileHandle.Close();
                }
                if (pSourceName != null)
                {
                    pSourceName.Close();
                }
                if (pPackageName != null)
                {
                    pPackageName.Close();
                }
                if (logonHandle != null)
                {
                    logonHandle.Close();
                }
            }
        }
    }
}
