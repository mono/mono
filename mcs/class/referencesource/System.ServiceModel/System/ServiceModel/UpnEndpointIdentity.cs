//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel.ComIntegration;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;

    public class UpnEndpointIdentity : EndpointIdentity
    {
        SecurityIdentifier upnSid;
        bool hasUpnSidBeenComputed;
        WindowsIdentity windowsIdentity;

        Object thisLock = new Object();

        public UpnEndpointIdentity(string upnName)
        {
            if (upnName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("upnName");

            base.Initialize(Claim.CreateUpnClaim(upnName));
            this.hasUpnSidBeenComputed = false;
        }

        public UpnEndpointIdentity(Claim identity)
        {
            if (identity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("identity");

            // PreSharp Bug: Parameter 'identity.ResourceType' to this public method must be validated: A null-dereference can occur here.
#pragma warning suppress 56506 // Claim.ResourceType will never return null
            if (!identity.ClaimType.Equals(ClaimTypes.Upn))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.UnrecognizedClaimTypeForIdentity, identity.ClaimType, ClaimTypes.Upn));

            base.Initialize(identity);
        }

        internal UpnEndpointIdentity(WindowsIdentity windowsIdentity)
        {
            if (windowsIdentity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windowsIdentity");

            this.windowsIdentity = windowsIdentity;
            this.upnSid = windowsIdentity.User;
            this.hasUpnSidBeenComputed = true;
        }

        internal override void EnsureIdentityClaim()
        {
            if (this.windowsIdentity != null)
            {
                lock (thisLock)
                {
                    if (this.windowsIdentity != null)
                    {
                        base.Initialize(Claim.CreateUpnClaim(GetUpnFromWindowsIdentity(this.windowsIdentity)));
                        this.windowsIdentity.Dispose();
                        this.windowsIdentity = null;
                    }
                }
            }
        }

        string GetUpnFromWindowsIdentity(WindowsIdentity windowsIdentity)
        {
            string downlevelName = null;
            string upnName = null;

            try
            {
                downlevelName = windowsIdentity.Name;

                if (this.IsMachineJoinedToDomain())
                {
                    upnName = GetUpnFromDownlevelName(downlevelName);
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
            }

            // if the AD cannot be queried for the fully qualified domain name,
            // fall back to the downlevel UPN name
            return upnName ?? downlevelName;
        }

        bool IsMachineJoinedToDomain()
        {
            IntPtr pDomainControllerInfo = IntPtr.Zero;

            try
            {
                int result = SafeNativeMethods.DsGetDcName(null, null, IntPtr.Zero, null, (uint)DSFlags.DS_DIRECTORY_SERVICE_REQUIRED, out pDomainControllerInfo);

                return result != (int)Win32Error.ERROR_NO_SUCH_DOMAIN;
            }
            finally
            {
                if (pDomainControllerInfo != IntPtr.Zero)
                {
                    SafeNativeMethods.NetApiBufferFree(pDomainControllerInfo);
                }
            }
        }

        // Duplicate code from SecurityImpersonationBehavior
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
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new Win32Exception(errorCode));
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new Win32Exception(errorCode));
                }
            }
            // trim the trailing / from fqdn
            fullyQualifiedDomainName = fullyQualifiedDomainName.Remove(fullyQualifiedDomainName.Length - 1, 1);
            fullDomainName = fullyQualifiedDomainName.ToString();

            return userName + "@" + fullDomainName;
        }

        internal override void WriteContentsTo(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");

            writer.WriteElementString(XD.AddressingDictionary.Upn, XD.AddressingDictionary.IdentityExtensionNamespace, (string)this.IdentityClaim.Resource);
        }

        internal SecurityIdentifier GetUpnSid()
        {
            Fx.Assert(ClaimTypes.Upn.Equals(this.IdentityClaim.ClaimType), "");
            if (!hasUpnSidBeenComputed)
            {
                lock (thisLock)
                {
                    string upn = (string)this.IdentityClaim.Resource;
                    if (!hasUpnSidBeenComputed)
                    {
                        try
                        {
                            NTAccount userAccount = new NTAccount(upn);
                            this.upnSid = userAccount.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch (Exception e)
                        {
                            // Always immediately rethrow fatal exceptions.
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            if (e is NullReferenceException)
                            {
                                throw;
                            }

                            SecurityTraceRecordHelper.TraceSpnToSidMappingFailure(upn, e);
                        }
                        finally
                        {
                            hasUpnSidBeenComputed = true;
                        }
                    }
                }
            }
            return this.upnSid;
        }
    }

}
