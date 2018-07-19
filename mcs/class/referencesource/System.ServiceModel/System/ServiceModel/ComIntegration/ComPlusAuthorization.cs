//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using SafeCloseHandle = System.IdentityModel.SafeCloseHandle;
    using SafeHGlobalHandle = System.IdentityModel.SafeHGlobalHandle;

    static class SecurityUtils
    {
        static WindowsIdentity anonymousIdentity;
        static WindowsIdentity processIdentity;
        static object lockObject = new object();

        [Fx.Tag.SecurityNote(Critical = "Uses critical type SafeHGlobalHandle.",
            Safe = "Performs a Demand for full trust.")]
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static SafeHandle GetTokenInformation(SafeCloseHandle token, TOKEN_INFORMATION_CLASS infoClass)
        {
            uint length;
            if (!SafeNativeMethods.GetTokenInformation(token, infoClass, SafeHGlobalHandle.InvalidHandle, 0, out length))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != (int)Win32Error.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, SR.GetString(SR.GetTokenInfoFailed, error)));
                }
            }
            SafeHandle buffer = SafeHGlobalHandle.AllocHGlobal(length);
            try
            {
                if (!SafeNativeMethods.GetTokenInformation(token, infoClass, buffer, length, out length))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, SR.GetString(SR.GetTokenInfoFailed, error)));
                }
            }
            catch
            {
                buffer.Dispose();
                throw;
            }
            return buffer;
        }

        internal static bool IsAtleastImpersonationToken(SafeCloseHandle token)
        {
            using (SafeHandle buffer =
                GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenImpersonationLevel))
            {
                int level = Marshal.ReadInt32(buffer.DangerousGetHandle());
                if (level < (int)SecurityImpersonationLevel.Impersonation)
                    return false;
                else
                    return true;
            }
        }

        internal static bool IsPrimaryToken(SafeCloseHandle token)
        {
            using (SafeHandle buffer =
                GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenType))
            {
                int level = Marshal.ReadInt32(buffer.DangerousGetHandle());
                return (level == (int)TokenType.TokenPrimary);
            }
        }

        internal static LUID GetModifiedIDLUID(SafeCloseHandle token)
        {
            using (SafeHandle buffer =
                GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenStatistics))
            {
                TOKEN_STATISTICS tokenStats = (TOKEN_STATISTICS)
                    Marshal.PtrToStructure(buffer.DangerousGetHandle(), typeof(TOKEN_STATISTICS));
                return tokenStats.ModifiedId;
            }
        }

        public static WindowsIdentity GetAnonymousIdentity()
        {
            SafeCloseHandle tokenHandle = null;
            bool isImpersonating = false;

            lock (lockObject)
            {
                if (anonymousIdentity == null)
                {
                    try
                    {
                        try
                        {
                            if (!SafeNativeMethods.ImpersonateAnonymousUserOnCurrentThread(SafeNativeMethods.GetCurrentThread()))
                            {
                                int error = Marshal.GetLastWin32Error();
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, SR.GetString(SR.ImpersonateAnonymousTokenFailed, error)));
                            }
                            isImpersonating = true;
                            bool revertSuccess;
                            bool isSuccess = SafeNativeMethods.OpenCurrentThreadToken(SafeNativeMethods.GetCurrentThread(), TokenAccessLevels.Query, true, out tokenHandle);
                            if (!isSuccess)
                            {
                                int error = Marshal.GetLastWin32Error();

                                revertSuccess = SafeNativeMethods.RevertToSelf();
                                if (false == revertSuccess)
                                {
                                    error = Marshal.GetLastWin32Error();

                                    //this requires a failfast since failure to revert impersonation compromises security
                                    DiagnosticUtility.FailFast("RevertToSelf() failed with " + error);
                                }
                                isImpersonating = false;

                                Utility.CloseInvalidOutSafeHandle(tokenHandle);
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, SR.GetString(SR.OpenThreadTokenFailed, error)));
                            }

                            revertSuccess = SafeNativeMethods.RevertToSelf();
                            if (false == revertSuccess)
                            {
                                int error = Marshal.GetLastWin32Error();

                                //this requires a failfast since failure to revert impersonation compromises security
                                DiagnosticUtility.FailFast("RevertToSelf() failed with " + error);
                            }
                            isImpersonating = false;

                            using (tokenHandle)
                            {
                                anonymousIdentity = new WindowsIdentity(tokenHandle.DangerousGetHandle());
                            }
                        }
                        finally
                        {
                            if (isImpersonating)
                            {
                                bool revertSuccess = SafeNativeMethods.RevertToSelf();
                                if (false == revertSuccess)
                                {
                                    int error = Marshal.GetLastWin32Error();

                                    //this requires a failfast since failure to revert impersonation compromises security
                                    DiagnosticUtility.FailFast("RevertToSelf() failed with " + error);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Force the finally to run before leaving the method.
                        throw;
                    }
                }
            }
            return anonymousIdentity;
        }

        public static WindowsIdentity GetProcessIdentity()
        {

            SafeCloseHandle tokenHandle = null;
            lock (lockObject)
            {

                try
                {
                    bool isSuccess = SafeNativeMethods.GetCurrentProcessToken(SafeNativeMethods.GetCurrentProcess(), TokenAccessLevels.Query, out tokenHandle);
                    if (!isSuccess)
                    {
                        int error = Marshal.GetLastWin32Error();
                        Utility.CloseInvalidOutSafeHandle(tokenHandle);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, SR.GetString(SR.OpenProcessTokenFailed, error)));
                    }
                    processIdentity = new WindowsIdentity(tokenHandle.DangerousGetHandle());
                }
                finally
                {
                    if (tokenHandle != null)
                        tokenHandle.Dispose();
                }
            }
            return processIdentity;
        }
    }

    internal sealed class ComPlusAuthorization
    {

        string[] serviceRoleMembers = null;
        string[] contractRoleMembers = null;
        string[] operationRoleMembers = null;
        CommonSecurityDescriptor securityDescriptor = null;
        static SecurityIdentifier sidAdministrators = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
        Dictionary<LUID, bool> accessCheckCache = new Dictionary<LUID, bool>();

        public ComPlusAuthorization(string[] serviceRoleMembers, string[] contractRoleMembers, string[] operationRoleMembers)
        {
            this.serviceRoleMembers = serviceRoleMembers;
            this.contractRoleMembers = contractRoleMembers;
            this.operationRoleMembers = operationRoleMembers;
        }

        private void BuildSecurityDescriptor()
        {
            Fx.Assert((null == securityDescriptor), "SecurityDescriptor must be NULL");

            NTAccount name;
            SecurityIdentifier sid;
            CommonAce ace;
            RawAcl acl = new RawAcl(GenericAcl.AclRevision, 1);
            int index = 0;
            if (operationRoleMembers != null)
            {
                foreach (string userName in operationRoleMembers)
                {
                    name = new NTAccount(userName);
                    sid = (SecurityIdentifier)name.Translate(typeof(SecurityIdentifier));
                    ace = new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, (int)ComRights.EXECUTE, sid, false, null);
                    acl.InsertAce(index, ace);
                    index++;
                }
            }
            if (contractRoleMembers != null)
            {
                foreach (string userName in contractRoleMembers)
                {
                    name = new NTAccount(userName);
                    sid = (SecurityIdentifier)name.Translate(typeof(SecurityIdentifier));
                    ace = new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, (int)ComRights.EXECUTE, sid, false, null);
                    acl.InsertAce(index, ace);
                    index++;
                }
            }
            if (serviceRoleMembers != null)
            {
                foreach (string userName in serviceRoleMembers)
                {
                    name = new NTAccount(userName);
                    sid = (SecurityIdentifier)name.Translate(typeof(SecurityIdentifier));
                    ace = new CommonAce(AceFlags.None, AceQualifier.AccessAllowed, (int)ComRights.EXECUTE, sid, false, null);
                    acl.InsertAce(index, ace);
                    index++;
                }
            }
            DiscretionaryAcl dacl = new DiscretionaryAcl(true, false, acl);
            securityDescriptor = new CommonSecurityDescriptor(true, false, ControlFlags.DiscretionaryAclPresent, sidAdministrators, sidAdministrators, null, dacl);

        }

        private bool IsAccessCached(LUID luidModifiedID, out bool isAccessAllowed)
        {
            if (null == accessCheckCache)
            {
                throw Fx.AssertAndThrowFatal("AcessCheckCache must not be NULL");
            }

            bool retValue = false;

            lock (this)
            {
                retValue = accessCheckCache.TryGetValue(luidModifiedID, out isAccessAllowed);
            }

            return retValue;
        }

        private void CacheAccessCheck(LUID luidModifiedID, bool isAccessAllowed)
        {
            if (null == accessCheckCache)
            {
                throw Fx.AssertAndThrowFatal("AcessCheckCache must not be NULL");
            }

            lock (this)
            {
                accessCheckCache[luidModifiedID] = isAccessAllowed;
            }
        }
        private void CheckAccess(WindowsIdentity clientIdentity, out bool IsAccessAllowed)
        {
            if (null == securityDescriptor)
            {
                throw Fx.AssertAndThrowFatal("Security Descriptor must not be NULL");
            }

            IsAccessAllowed = false;
            byte[] BinaryForm = new byte[securityDescriptor.BinaryLength];
            securityDescriptor.GetBinaryForm(BinaryForm, 0);
            SafeCloseHandle ImpersonationToken = null;
            SafeCloseHandle clientIdentityToken = new SafeCloseHandle(clientIdentity.Token, false);
            try
            {
                if (SecurityUtils.IsPrimaryToken(clientIdentityToken))
                {
                    if (!SafeNativeMethods.DuplicateTokenEx(clientIdentityToken,
                                                                        TokenAccessLevels.Query,
                                                                        IntPtr.Zero,
                                                                        SecurityImpersonationLevel.Identification,
                                                                        TokenType.TokenImpersonation,
                                                                        out ImpersonationToken))
                    {
                        int error = Marshal.GetLastWin32Error();
                        Utility.CloseInvalidOutSafeHandle(ImpersonationToken);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, SR.GetString(SR.DuplicateTokenExFailed, error)));
                    }
                }
                GENERIC_MAPPING GenericMapping = new GENERIC_MAPPING();
                PRIVILEGE_SET PrivilegeSet = new PRIVILEGE_SET();
                uint PrivilegeSetLength = (uint)Marshal.SizeOf(PrivilegeSet);
                uint GrantedAccess = 0;
                if (!SafeNativeMethods.AccessCheck(BinaryForm, (ImpersonationToken != null) ? ImpersonationToken : clientIdentityToken,
                    (int)ComRights.EXECUTE, GenericMapping, out PrivilegeSet,
                    ref PrivilegeSetLength, out GrantedAccess, out IsAccessAllowed))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, SR.GetString(SR.AccessCheckFailed, error)));
                }

            }
            finally
            {
                if (ImpersonationToken != null)
                    ImpersonationToken.Dispose();
            }
        }

        public string[] ServiceRoleMembers
        {
            get
            {
                return serviceRoleMembers;
            }
        }
        public string[] ContractRoleMembers
        {
            get
            {
                return contractRoleMembers;
            }
        }
        public string[] OperationRoleMembers
        {
            get
            {
                return operationRoleMembers;
            }
        }
        public CommonSecurityDescriptor SecurityDescriptor
        {
            get
            {
                return securityDescriptor;
            }
        }
        public bool IsAuthorizedForOperation(WindowsIdentity clientIdentity)
        {

            bool IsAccessAllowed = false;

            if (null == clientIdentity)
            {
                throw Fx.AssertAndThrow("NULL Identity");
            }
            if (IntPtr.Zero == clientIdentity.Token)
            {
                throw Fx.AssertAndThrow("Token handle cannot be zero");
            }

            lock (this)
            {
                if (securityDescriptor == null)
                {
                    BuildSecurityDescriptor();
                }
            }

            LUID luidModified = SecurityUtils.GetModifiedIDLUID(new SafeCloseHandle(clientIdentity.Token, false));

            if (IsAccessCached(luidModified, out IsAccessAllowed))
                return IsAccessAllowed;

            CheckAccess(clientIdentity, out IsAccessAllowed);

            CacheAccessCheck(luidModified, IsAccessAllowed);

            return IsAccessAllowed;
        }

    }

    internal sealed class ComPlusServerSecurity : IContextSecurityPerimeter, IServerSecurity, IDisposable
    {
        WindowsIdentity clientIdentity = null;
        IntPtr oldSecurityObject = IntPtr.Zero;
        WindowsImpersonationContext impersonateContext = null;
        bool isImpersonating = false;
        bool shouldUseCallContext = false;

        const uint RPC_C_AUTHN_GSS_NEGOTIATE = 9;
        const uint RPC_C_AUTHN_WINNT = 10;
        const uint RPC_C_AUTHN_GSS_KERBEROS = 16;
        const uint RPC_C_AUTHN_DEFAULT = unchecked((uint)0xFFFFFFFF);

        const uint RPC_C_AUTHZ_NONE = 0;

        const uint RPC_C_AUTHN_LEVEL_DEFAULT = 0;
        const uint RPC_C_AUTHN_LEVEL_NONE = 1;
        const uint RPC_C_AUTHN_LEVEL_CONNECT = 2;
        const uint RPC_C_AUTHN_LEVEL_CALL = 3;
        const uint RPC_C_AUTHN_LEVEL_PKT = 4;
        const uint RPC_C_AUTHN_LEVEL_PKT_INTEGRITY = 5;
        const uint RPC_C_AUTHN_LEVEL_PKT_PRIVACY = 6;

        public ComPlusServerSecurity(WindowsIdentity clientIdentity, bool shouldUseCallContext)
        {
            if (null == clientIdentity)
            {
                throw Fx.AssertAndThrow("NULL Identity");
            }
            if (IntPtr.Zero == clientIdentity.Token)
            {
                throw Fx.AssertAndThrow("Token handle cannot be zero");
            }

            this.shouldUseCallContext = shouldUseCallContext;
            this.clientIdentity = clientIdentity;
            IntPtr secCtx = Marshal.GetIUnknownForObject(this);
            try
            {
                oldSecurityObject = SafeNativeMethods.CoSwitchCallContext(secCtx);
            }
            catch
            {
                Marshal.Release(secCtx);

                throw;
            }
        }
        ~ComPlusServerSecurity()
        {
            Dispose(false);
        }
        public bool GetPerimeterFlag()
        {
            return shouldUseCallContext;
        }

        public void SetPerimeterFlag(bool flag)
        {
            shouldUseCallContext = flag;
        }

        public void QueryBlanket
        (
            IntPtr authnSvc,
            IntPtr authzSvc,
            IntPtr serverPrincipalName,
            IntPtr authnLevel,
            IntPtr impLevel,
            IntPtr clientPrincipalName,
            IntPtr Capabilities
        )
        {
            // Convert to RPC'isms.

            if (authnSvc != IntPtr.Zero)
            {
                uint tempAuthnSvc = RPC_C_AUTHN_DEFAULT;

                // Try to convert the clientIdentity.AuthenticationType to an RPC constant.
                // This is a best case attempt.
                string authenticationType = clientIdentity.AuthenticationType;
                if (authenticationType.ToUpperInvariant() == "NTLM")
                    tempAuthnSvc = RPC_C_AUTHN_WINNT;
                else if (authenticationType.ToUpperInvariant() == "KERBEROS")
                    tempAuthnSvc = RPC_C_AUTHN_GSS_KERBEROS;
                else if (authenticationType.ToUpperInvariant() == "NEGOTIATE")
                    tempAuthnSvc = RPC_C_AUTHN_GSS_NEGOTIATE;

                Marshal.WriteInt32(authnSvc, (int)tempAuthnSvc);
            }

            if (authzSvc != IntPtr.Zero)
            {
                Marshal.WriteInt32(authzSvc, (int)RPC_C_AUTHZ_NONE);
            }

            if (serverPrincipalName != IntPtr.Zero)
            {
                IntPtr str = Marshal.StringToCoTaskMemUni(SecurityUtils.GetProcessIdentity().Name);

                Marshal.WriteIntPtr(serverPrincipalName, str);
            }

            // There is no equivalent for the RPC authn level. It can only be
            // approximated, in the best case. Use default.

            if (authnLevel != IntPtr.Zero)
            {
                Marshal.WriteInt32(authnLevel, (int)RPC_C_AUTHN_LEVEL_DEFAULT);
            }

            if (impLevel != IntPtr.Zero)
            {
                Marshal.WriteInt32(impLevel, 0);
            }

            if (clientPrincipalName != IntPtr.Zero)
            {
                IntPtr str = Marshal.StringToCoTaskMemUni(clientIdentity.Name);

                Marshal.WriteIntPtr(clientPrincipalName, str);
            }

            if (Capabilities != IntPtr.Zero)
            {
                Marshal.WriteInt32(Capabilities, 0);
            }
        }

        public int ImpersonateClient()
        {
            // We want to return known COM hresults here rather than random CLR-Exception mapped HRESULTS.  Also, 
            // we don't want CLR to set the ErrorInfo object.

            int hresult = HR.E_FAIL;
            try
            {
                impersonateContext = WindowsIdentity.Impersonate(clientIdentity.Token);
                isImpersonating = true;
                hresult = HR.S_OK;
            }
            catch (SecurityException)
            {
                // Special case anonymous impersonation failure. 
                // Unmanaged callers to ImpersonateClient expect this hresult.
                hresult = HR.RPC_NT_BINDING_HAS_NO_AUTH;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;
            }
            return hresult;
        }
        public int RevertToSelf()
        {
            // We want to return known COM hresults here rather than random CLR-Exception mapped HRESULTS.  Also, 
            // we don't want CLR to set the ErrorInfo object.

            int hresult = HR.E_FAIL;
            if (isImpersonating)
            {
                try
                {
                    impersonateContext.Undo();
                    isImpersonating = false;
                    hresult = HR.S_OK;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;
                }
            }
            return hresult;
        }
        public bool IsImpersonating()
        {
            return isImpersonating;
        }
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            RevertToSelf();
            IntPtr secCtx = SafeNativeMethods.CoSwitchCallContext(oldSecurityObject);

            if (IntPtr.Zero == secCtx)
            {
                // this has to be a failfast since not having a security context can compromise security
                DiagnosticUtility.FailFast("Security Context was should not be null");
            }

            if (Marshal.GetObjectForIUnknown(secCtx) != this)
            {
                // this has to be a failfast since being in the wrong security context can compromise security
                DiagnosticUtility.FailFast("Security Context was modified from underneath us");
            }
            Marshal.Release(secCtx);
            if (disposing)
            {
                clientIdentity = null;
                if (impersonateContext != null)
                    impersonateContext.Dispose();
            }
        }

    }

}
