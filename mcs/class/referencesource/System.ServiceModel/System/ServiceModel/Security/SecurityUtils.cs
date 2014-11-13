//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.DirectoryServices.ActiveDirectory;
    using System.Globalization;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Net.Security;
    using System.Runtime;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using Microsoft.Win32;
    using AuthIdentityEx = System.IdentityModel.AuthIdentityEx;
    using CredentialUse = System.IdentityModel.CredentialUse;
    using DictionaryManager = System.IdentityModel.DictionaryManager;
    using SafeFreeCredentials = System.IdentityModel.SafeFreeCredentials;
    using SspiWrapper = System.IdentityModel.SspiWrapper;

    static class StoreLocationHelper
    {
        internal static bool IsDefined(StoreLocation value)
        {
            return (value == StoreLocation.CurrentUser
                || value == StoreLocation.LocalMachine);
        }
    }

    static class ProtectionLevelHelper
    {
        internal static bool IsDefined(ProtectionLevel value)
        {
            return (value == ProtectionLevel.None
                || value == ProtectionLevel.Sign
                || value == ProtectionLevel.EncryptAndSign);
        }

        internal static void Validate(ProtectionLevel value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(ProtectionLevel)));
            }
        }

        internal static bool IsStronger(ProtectionLevel v1, ProtectionLevel v2)
        {
            return ((v1 == ProtectionLevel.EncryptAndSign && v2 != ProtectionLevel.EncryptAndSign)
                    || (v1 == ProtectionLevel.Sign && v2 == ProtectionLevel.None));
        }

        internal static bool IsStrongerOrEqual(ProtectionLevel v1, ProtectionLevel v2)
        {
            return (v1 == ProtectionLevel.EncryptAndSign
                    || (v1 == ProtectionLevel.Sign && v2 != ProtectionLevel.EncryptAndSign));
        }

        internal static ProtectionLevel Max(ProtectionLevel v1, ProtectionLevel v2)
        {
            return IsStronger(v1, v2) ? v1 : v2;
        }

        internal static int GetOrdinal(Nullable<ProtectionLevel> p)
        {
            if (p.HasValue)
            {
                switch ((ProtectionLevel)p)
                {
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("p", (int)p,
                        typeof(ProtectionLevel)));
                    case ProtectionLevel.None:
                        return 2;
                    case ProtectionLevel.Sign:
                        return 3;
                    case ProtectionLevel.EncryptAndSign:
                        return 4;
                }
            }
            else
                return 1;
        }
    }

    static class TokenImpersonationLevelHelper
    {
        internal static bool IsDefined(TokenImpersonationLevel value)
        {
            return (value == TokenImpersonationLevel.None
                || value == TokenImpersonationLevel.Anonymous
                || value == TokenImpersonationLevel.Identification
                || value == TokenImpersonationLevel.Impersonation
                || value == TokenImpersonationLevel.Delegation);
        }

        internal static void Validate(TokenImpersonationLevel value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value,
                    typeof(TokenImpersonationLevel)));
            }
        }

        static TokenImpersonationLevel[] TokenImpersonationLevelOrder = new TokenImpersonationLevel[]
            {
                TokenImpersonationLevel.None,
                TokenImpersonationLevel.Anonymous,
                TokenImpersonationLevel.Identification,
                TokenImpersonationLevel.Impersonation,
                TokenImpersonationLevel.Delegation
            };

        internal static string ToString(TokenImpersonationLevel impersonationLevel)
        {
            if (impersonationLevel == TokenImpersonationLevel.Identification)
            {
                return "identification";
            }
            else if (impersonationLevel == TokenImpersonationLevel.None)
            {
                return "none";
            }
            else if (impersonationLevel == TokenImpersonationLevel.Anonymous)
            {
                return "anonymous";
            }
            else if (impersonationLevel == TokenImpersonationLevel.Impersonation)
            {
                return "impersonation";
            }
            else if (impersonationLevel == TokenImpersonationLevel.Delegation)
            {
                return "delegation";
            }

            Fx.Assert("unknown token impersonation level");
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("impersonationLevel", (int)impersonationLevel,
            typeof(TokenImpersonationLevel)));
        }

        internal static bool IsGreaterOrEqual(TokenImpersonationLevel x, TokenImpersonationLevel y)
        {
            TokenImpersonationLevelHelper.Validate(x);
            TokenImpersonationLevelHelper.Validate(y);

            if (x == y)
                return true;

            int px = 0;
            int py = 0;
            for (int i = 0; i < TokenImpersonationLevelOrder.Length; i++)
            {
                if (x == TokenImpersonationLevelOrder[i])
                    px = i;
                if (y == TokenImpersonationLevelOrder[i])
                    py = i;
            }

            return (px > py);
        }

        internal static int Compare(TokenImpersonationLevel x, TokenImpersonationLevel y)
        {
            int result = 0;

            if (x != y)
            {
                switch (x)
                {
                    case TokenImpersonationLevel.Identification:
                        result = -1;
                        break;
                    case TokenImpersonationLevel.Impersonation:
                        switch (y)
                        {
                            case TokenImpersonationLevel.Identification:
                                result = 1;
                                break;
                            case TokenImpersonationLevel.Delegation:
                                result = -1;
                                break;
                            default:
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("y", (int)y,
                                    typeof(TokenImpersonationLevel)));

                        }
                        break;
                    case TokenImpersonationLevel.Delegation:
                        result = 1;
                        break;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("x", (int)x,
                            typeof(TokenImpersonationLevel)));

                }
            }

            return result;
        }
    }

    internal class ServiceModelDictionaryManager
    {
        static DictionaryManager dictionaryManager;

        public static DictionaryManager Instance
        {
            get
            {
                if (dictionaryManager == null)
                    dictionaryManager = new DictionaryManager(BinaryMessageEncoderFactory.XmlDictionary);

                return dictionaryManager;
            }
        }
    }

    static class SecurityUtils
    {
        public const string Principal = "Principal";
        public const string Identities = "Identities";
        static bool computedDomain;
        static string currentDomain;
        static byte[] combinedHashLabel;
        static IIdentity anonymousIdentity;
        static NetworkCredential dummyNetworkCredential;
        static object dummyNetworkCredentialLock = new object();
        static X509SecurityTokenAuthenticator nonValidatingX509Authenticator;
        static SecurityIdentifier administratorsSid;
        const int WindowsServerMajorNumber = 5;
        const int WindowsServerMinorNumber = 2;
        const int XPMajorNumber = 5;
        const int XPMinorNumber = 1;
        const string ServicePack1 = "Service Pack 1";
        const string ServicePack2 = "Service Pack 2";
        volatile static bool shouldValidateSslCipherStrength;
        volatile static bool isSslValidationRequirementDetermined = false;
        static readonly int MinimumSslCipherStrength = 128;

        // these are kept in [....] with IIS70
        public const string AuthTypeNTLM = "NTLM";
        public const string AuthTypeNegotiate = "Negotiate";
        public const string AuthTypeKerberos = "Kerberos";
        public const string AuthTypeAnonymous = "";
        public const string AuthTypeCertMap = "SSL/PCT"; // mapped from a cert
        public const string AuthTypeBasic = "Basic"; //LogonUser

        public static ChannelBinding GetChannelBindingFromMessage(Message message)
        {
            if (message == null)
            {
                return null;
            }

            ChannelBindingMessageProperty channelBindingMessageProperty = null;
            ChannelBindingMessageProperty.TryGet(message, out channelBindingMessageProperty);
            ChannelBinding channelBinding = null;

            if (channelBindingMessageProperty != null)
            {
                channelBinding = channelBindingMessageProperty.ChannelBinding;
            }

            return channelBinding;
        }

        internal static bool IsOsGreaterThanXP()
        {
            return ((Environment.OSVersion.Version.Major >= SecurityUtils.XPMajorNumber && Environment.OSVersion.Version.Minor > SecurityUtils.XPMinorNumber) ||
                    Environment.OSVersion.Version.Major > SecurityUtils.XPMajorNumber);
        }

        internal static bool IsOSGreaterThanOrEqualToWin7()
        {
            Version windows7Version = new Version(6, 1, 0, 0);
            return (Environment.OSVersion.Version.Major >= windows7Version.Major && Environment.OSVersion.Version.Minor >= windows7Version.Minor);
        }

        internal static bool IsCurrentlyTimeEffective(DateTime effectiveTime, DateTime expirationTime, TimeSpan maxClockSkew)
        {
            DateTime curEffectiveTime = (effectiveTime < DateTime.MinValue.Add(maxClockSkew)) ? effectiveTime : effectiveTime.Subtract(maxClockSkew);
            DateTime curExpirationTime = (expirationTime > DateTime.MaxValue.Subtract(maxClockSkew)) ? expirationTime : expirationTime.Add(maxClockSkew);
            DateTime curTime = DateTime.UtcNow;

            return (curEffectiveTime.ToUniversalTime() <= curTime) && (curTime < curExpirationTime.ToUniversalTime());
        }

        internal static X509SecurityTokenAuthenticator NonValidatingX509Authenticator
        {
            get
            {
                if (nonValidatingX509Authenticator == null)
                {
                    nonValidatingX509Authenticator = new X509SecurityTokenAuthenticator(X509CertificateValidator.None);
                }
                return nonValidatingX509Authenticator;
            }
        }

        public static SecurityIdentifier AdministratorsSid
        {
            get
            {
                if (administratorsSid == null)
                    administratorsSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                return administratorsSid;
            }
        }

        internal static IIdentity AnonymousIdentity
        {
            get
            {
                if (anonymousIdentity == null)
                {
                    anonymousIdentity = SecurityUtils.CreateIdentity(String.Empty);
                }
                return anonymousIdentity;
            }
        }

        public static DateTime MaxUtcDateTime
        {
            get
            {
                // + and -  TimeSpan.TicksPerDay is to compensate the DateTime.ParseExact (to localtime) overflow.
                return new DateTime(DateTime.MaxValue.Ticks - TimeSpan.TicksPerDay, DateTimeKind.Utc);
            }
        }

        public static DateTime MinUtcDateTime
        {
            get
            {
                // + and -  TimeSpan.TicksPerDay is to compensate the DateTime.ParseExact (to localtime) overflow.
                return new DateTime(DateTime.MinValue.Ticks + TimeSpan.TicksPerDay, DateTimeKind.Utc);
            }
        }

        internal static IIdentity CreateIdentity(string name, string authenticationType)
        {
            return new GenericIdentity(name, authenticationType);
        }

        internal static IIdentity CreateIdentity(string name)
        {
            return new GenericIdentity(name);
        }

        internal static EndpointIdentity CreateWindowsIdentity()
        {
            return CreateWindowsIdentity(false);
        }

        internal static EndpointIdentity CreateWindowsIdentity(NetworkCredential serverCredential)
        {
            if (serverCredential != null && !NetworkCredentialHelper.IsDefault(serverCredential))
            {
                string upn;
                if (serverCredential.Domain != null && serverCredential.Domain.Length > 0)
                {
                    upn = serverCredential.UserName + "@" + serverCredential.Domain;
                }
                else
                {
                    upn = serverCredential.UserName;
                }
                return EndpointIdentity.CreateUpnIdentity(upn);
            }
            else
            {
                return SecurityUtils.CreateWindowsIdentity();
            }
        }

        static bool IsSystemAccount(WindowsIdentity self)
        {
            SecurityIdentifier sid = self.User;
            if (sid == null)
            {
                return false;
            }
            // S-1-5-82 is the prefix for the sid that represents the identity that IIS 7.5 Apppool thread runs under.
            return (sid.IsWellKnown(WellKnownSidType.LocalSystemSid)
                    || sid.IsWellKnown(WellKnownSidType.NetworkServiceSid)
                    || sid.IsWellKnown(WellKnownSidType.LocalServiceSid)
                    || self.User.Value.StartsWith("S-1-5-82", StringComparison.OrdinalIgnoreCase));
        }

        internal static EndpointIdentity CreateWindowsIdentity(bool spnOnly)
        {
            EndpointIdentity identity = null;
            using (WindowsIdentity self = WindowsIdentity.GetCurrent())
            {
                bool isSystemAccount = IsSystemAccount(self);
                if (spnOnly || isSystemAccount)
                {
                    identity = EndpointIdentity.CreateSpnIdentity(String.Format(CultureInfo.InvariantCulture, "host/{0}", DnsCache.MachineName));
                }
                else
                {
                    // Save windowsIdentity for delay lookup
                    identity = new UpnEndpointIdentity(CloneWindowsIdentityIfNecessary(self));
                }
            }

            return identity;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls two critical methods: UnsafeGetWindowsIdentityToken and UnsafeCreateWindowsIdentityFromToken.",
            Safe = "'Clone' operation is considered safe despite using WindowsIdentity IntPtr token. Must not let IntPtr token leak in or out.")]
        [SecuritySafeCritical]
        internal static WindowsIdentity CloneWindowsIdentityIfNecessary(WindowsIdentity wid)
        {
            return SecurityUtils.CloneWindowsIdentityIfNecessary(wid, null);
        }

        [Fx.Tag.SecurityNote(Critical = "Calls two critical methods: UnsafeGetWindowsIdentityToken and UnsafeCreateWindowsIdentityFromToken.",
            Safe = "'Clone' operation is considered safe despite using WindowsIdentity IntPtr token. Must not let IntPtr token leak in or out.")]
        [SecuritySafeCritical]
        internal static WindowsIdentity CloneWindowsIdentityIfNecessary(WindowsIdentity wid, string authType)
        {
            if (wid != null)
            {
                IntPtr token = UnsafeGetWindowsIdentityToken(wid);
                if (token != IntPtr.Zero)
                {
                    return UnsafeCreateWindowsIdentityFromToken(token, authType);
                }
            }
            return wid;
        }

        [Fx.Tag.SecurityNote(Critical = "Elevates in order to return the WindowsIdentity.Token property, caller must protect return value.")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        static IntPtr UnsafeGetWindowsIdentityToken(WindowsIdentity wid)
        {
            return wid.Token;
        }

        [Fx.Tag.SecurityNote(Critical = "Elevates in order to return the SecurityIdentifier of the current user as a string, caller must protect return value.")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.ControlPrincipal)]
        static string UnsafeGetCurrentUserSidAsString()
        {
            using (WindowsIdentity self = WindowsIdentity.GetCurrent())
            {
                return self.User.Value;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Elevates in order to return the WindowsIdentity.Token property, caller must protect return value.")]
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, ControlPrincipal = true, UnmanagedCode = true)]
        static WindowsIdentity UnsafeCreateWindowsIdentityFromToken(IntPtr token, string authType)
        {
            if (authType != null)
                return new WindowsIdentity(token, authType);
            else
                return new WindowsIdentity(token);
        }

        internal static bool AllowsImpersonation(WindowsIdentity windowsIdentity, TokenImpersonationLevel impersonationLevel)
        {
            if (windowsIdentity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windowsIdentity");

            TokenImpersonationLevelHelper.Validate(impersonationLevel);

            if (impersonationLevel == TokenImpersonationLevel.Identification)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("impersonationLevel"));

            bool result = true;

            switch (windowsIdentity.ImpersonationLevel)
            {
                case TokenImpersonationLevel.None:
                case TokenImpersonationLevel.Anonymous:
                case TokenImpersonationLevel.Identification:
                    result = false; break;
                case TokenImpersonationLevel.Impersonation:
                    if (impersonationLevel == TokenImpersonationLevel.Delegation)
                        result = false;
                    break;
                case TokenImpersonationLevel.Delegation:
                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }

        internal static byte[] CombinedHashLabel
        {
            get
            {
                if (combinedHashLabel == null)
                    combinedHashLabel = Encoding.UTF8.GetBytes(TrustApr2004Strings.CombinedHashLabel);
                return combinedHashLabel;
            }
        }

        internal static T GetSecurityKey<T>(SecurityToken token)
            where T : SecurityKey
        {
            T result = null;
            if (token.SecurityKeys != null)
            {
                for (int i = 0; i < token.SecurityKeys.Count; ++i)
                {
                    T temp = (token.SecurityKeys[i] as T);
                    if (temp != null)
                    {
                        if (result != null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.MultipleMatchingCryptosFound, typeof(T).ToString())));
                        }
                        else
                        {
                            result = temp;
                        }
                    }
                }
            }
            return result;
        }

        internal static bool HasSymmetricSecurityKey(SecurityToken token)
        {
            return GetSecurityKey<SymmetricSecurityKey>(token) != null;
        }

        internal static void EnsureExpectedSymmetricMatch(SecurityToken t1, SecurityToken t2, Message message)
        {
            // nulls are not mismatches
            if (t1 == null || t2 == null || ReferenceEquals(t1, t2))
            {
                return;
            }
            // check for interop flexibility
            SymmetricSecurityKey c1 = SecurityUtils.GetSecurityKey<SymmetricSecurityKey>(t1);
            SymmetricSecurityKey c2 = SecurityUtils.GetSecurityKey<SymmetricSecurityKey>(t2);
            if (c1 == null || c2 == null || !CryptoHelper.IsEqual(c1.GetSymmetricKey(), c2.GetSymmetricKey()))
            {
                throw System.ServiceModel.Diagnostics.TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenNotExpectedInSecurityHeader, t2)), message);
            }
        }

        internal static SymmetricAlgorithm GetSymmetricAlgorithm(string algorithm, SecurityToken token)
        {
            SymmetricSecurityKey securityKey = SecurityUtils.GetSecurityKey<SymmetricSecurityKey>(token);
            if (securityKey != null && securityKey.IsSupportedAlgorithm(algorithm))
            {
                return securityKey.GetSymmetricAlgorithm(algorithm);
            }
            else
            {
                return null;
            }
        }

        internal static KeyedHashAlgorithm GetKeyedHashAlgorithm(string algorithm, SecurityToken token)
        {
            SymmetricSecurityKey securityKey = SecurityUtils.GetSecurityKey<SymmetricSecurityKey>(token);
            if (securityKey != null && securityKey.IsSupportedAlgorithm(algorithm))
            {
                return securityKey.GetKeyedHashAlgorithm(algorithm);
            }
            else
            {
                return null;
            }
        }

        internal static ReadOnlyCollection<SecurityKey> CreateSymmetricSecurityKeys(byte[] key)
        {
            List<SecurityKey> temp = new List<SecurityKey>(1);
            temp.Add(new InMemorySymmetricSecurityKey(key));
            return temp.AsReadOnly();
        }

        internal static byte[] DecryptKey(SecurityToken unwrappingToken, string encryptionMethod, byte[] wrappedKey, out SecurityKey unwrappingSecurityKey)
        {
            unwrappingSecurityKey = null;
            if (unwrappingToken.SecurityKeys != null)
            {
                for (int i = 0; i < unwrappingToken.SecurityKeys.Count; ++i)
                {
                    if (unwrappingToken.SecurityKeys[i].IsSupportedAlgorithm(encryptionMethod))
                    {
                        unwrappingSecurityKey = unwrappingToken.SecurityKeys[i];
                        break;
                    }
                }
            }
            if (unwrappingSecurityKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.CannotFindMatchingCrypto, encryptionMethod)));
            }
            return unwrappingSecurityKey.DecryptKey(encryptionMethod, wrappedKey);
        }

        internal static byte[] EncryptKey(SecurityToken wrappingToken, string encryptionMethod, byte[] keyToWrap)
        {
            SecurityKey wrappingSecurityKey = null;
            if (wrappingToken.SecurityKeys != null)
            {
                for (int i = 0; i < wrappingToken.SecurityKeys.Count; ++i)
                {
                    if (wrappingToken.SecurityKeys[i].IsSupportedAlgorithm(encryptionMethod))
                    {
                        wrappingSecurityKey = wrappingToken.SecurityKeys[i];
                        break;
                    }
                }
            }
            if (wrappingSecurityKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.CannotFindMatchingCrypto, encryptionMethod));
            }
            return wrappingSecurityKey.EncryptKey(encryptionMethod, keyToWrap);
        }

        internal static byte[] ReadContentAsBase64(XmlDictionaryReader reader, long maxBufferSize)
        {
            if (reader == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

            // Code cloned from System.Xml.XmlDictionaryReder.
            byte[][] buffers = new byte[32][];
            byte[] buffer;
            // Its best to read in buffers that are a multiple of 3 so we don't break base64 boundaries when converting text
            int count = 384;
            int bufferCount = 0;
            int totalRead = 0;
            while (true)
            {
                buffer = new byte[count];
                buffers[bufferCount++] = buffer;
                int read = 0;
                while (read < buffer.Length)
                {
                    int actual = reader.ReadContentAsBase64(buffer, read, buffer.Length - read);
                    if (actual == 0)
                        break;
                    read += actual;
                }
                if (totalRead > maxBufferSize - read)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QuotaExceededException(SR.GetString(SR.BufferQuotaExceededReadingBase64, maxBufferSize)));
                totalRead += read;
                if (read < buffer.Length)
                    break;
                count = count * 2;
            }
            buffer = new byte[totalRead];
            int offset = 0;
            for (int i = 0; i < bufferCount - 1; i++)
            {
                Buffer.BlockCopy(buffers[i], 0, buffer, offset, buffers[i].Length);
                offset += buffers[i].Length;
            }
            Buffer.BlockCopy(buffers[bufferCount - 1], 0, buffer, offset, totalRead - offset);
            return buffer;
        }

        internal static byte[] GenerateDerivedKey(SecurityToken tokenToDerive, string derivationAlgorithm, byte[] label, byte[] nonce,
            int keySize, int offset)
        {
            SymmetricSecurityKey symmetricSecurityKey = SecurityUtils.GetSecurityKey<SymmetricSecurityKey>(tokenToDerive);
            if (symmetricSecurityKey == null || !symmetricSecurityKey.IsSupportedAlgorithm(derivationAlgorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.CannotFindMatchingCrypto, derivationAlgorithm)));
            }
            return symmetricSecurityKey.GenerateDerivedKey(derivationAlgorithm, label, nonce, keySize, offset);
        }

        internal static string GetSpnFromIdentity(EndpointIdentity identity, EndpointAddress target)
        {
            bool foundSpn = false;
            string spn = null;
            if (identity != null)
            {
                if (ClaimTypes.Spn.Equals(identity.IdentityClaim.ClaimType))
                {
                    spn = (string)identity.IdentityClaim.Resource;
                    foundSpn = true;
                }
                else if (ClaimTypes.Upn.Equals(identity.IdentityClaim.ClaimType))
                {
                    spn = (string)identity.IdentityClaim.Resource;
                    foundSpn = true;
                }
                else if (ClaimTypes.Dns.Equals(identity.IdentityClaim.ClaimType))
                {
                    spn = String.Format(CultureInfo.InvariantCulture, "host/{0}", (string)identity.IdentityClaim.Resource);
                    foundSpn = true;
                }
            }
            if (!foundSpn)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.CannotDetermineSPNBasedOnAddress, target)));
            }
            return spn;
        }

        internal static string GetSpnFromTarget(EndpointAddress target)
        {
            if (target == null)
            {
                throw Fx.AssertAndThrow("target should not be null - expecting an EndpointAddress");
            }

            return string.Format(CultureInfo.InvariantCulture, "host/{0}", target.Uri.DnsSafeHost);
        }

        internal static bool IsSupportedAlgorithm(string algorithm, SecurityToken token)
        {
            if (token.SecurityKeys == null)
            {
                return false;
            }
            for (int i = 0; i < token.SecurityKeys.Count; ++i)
            {
                if (token.SecurityKeys[i].IsSupportedAlgorithm(algorithm))
                {
                    return true;
                }
            }
            return false;
        }

        internal static Claim GetPrimaryIdentityClaim(ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            return GetPrimaryIdentityClaim(AuthorizationContext.CreateDefaultAuthorizationContext(authorizationPolicies));
        }

        internal static Claim GetPrimaryIdentityClaim(AuthorizationContext authContext)
        {
            if (authContext != null)
            {
                for (int i = 0; i < authContext.ClaimSets.Count; ++i)
                {
                    ClaimSet claimSet = authContext.ClaimSets[i];
                    foreach (Claim claim in claimSet.FindClaims(null, Rights.Identity))
                    {
                        return claim;
                    }
                }
            }
            return null;
        }

        internal static int GetServiceAddressAndViaHash(EndpointAddress sr)
        {
            if (sr == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("sr");
            }
            return sr.GetHashCode();
        }

        internal static string GenerateId()
        {
            return SecurityUniqueId.Create().Value;
        }

        internal static string GenerateIdWithPrefix(string prefix)
        {
            return SecurityUniqueId.Create(prefix).Value;
        }

        internal static UniqueId GenerateUniqueId()
        {
            return new UniqueId();
        }

        internal static string GetPrimaryDomain()
        {
            using (WindowsIdentity wid = WindowsIdentity.GetCurrent())
            {
                return GetPrimaryDomain(IsSystemAccount(wid));
            }
        }

        internal static string GetPrimaryDomain(bool isSystemAccount)
        {
            if (computedDomain == false)
            {
                try
                {
                    if (isSystemAccount)
                    {
                        currentDomain = Domain.GetComputerDomain().Name;
                    }
                    else
                    {
                        currentDomain = Domain.GetCurrentDomain().Name;
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
                finally
                {
                    computedDomain = true;
                }
            }
            return currentDomain;
        }

        internal static void EnsureCertificateCanDoKeyExchange(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            bool canDoKeyExchange = false;
            Exception innerException = null;
            if (certificate.HasPrivateKey)
            {
                try
                {
                    canDoKeyExchange = CanKeyDoKeyExchange(certificate);
                }
                // exceptions can be due to ACLs on the key etc
                catch (System.Security.SecurityException e)
                {
                    innerException = e;
                }
                catch (CryptographicException e)
                {
                    innerException = e;
                }
            }
            if (!canDoKeyExchange)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.SslCertMayNotDoKeyExchange, certificate.SubjectName.Name), innerException));
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls critical method GetKeyContainerInfo.",
            Safe = "Info is not leaked.")]
        [SecuritySafeCritical]
        static bool CanKeyDoKeyExchange(X509Certificate2 certificate)
        {
            CspKeyContainerInfo info = GetKeyContainerInfo(certificate);
            return info != null && info.KeyNumber == KeyNumber.Exchange;
        }

        [Fx.Tag.SecurityNote(Critical = "Elevates to call properties: X509Certificate2.PrivateKey and CspKeyContainerInfo. Caller must protect the return value.")]
        [SecurityCritical]
        [KeyContainerPermission(SecurityAction.Assert, Flags = KeyContainerPermissionFlags.Open)]
        static CspKeyContainerInfo GetKeyContainerInfo(X509Certificate2 certificate)
        {
            RSACryptoServiceProvider rsa = certificate.PrivateKey as RSACryptoServiceProvider;
            if (rsa != null)
            {
                return rsa.CspKeyContainerInfo;
            }

            return null;
        }

        internal static string GetCertificateId(X509Certificate2 certificate)
        {
            StringBuilder str = new StringBuilder(256);
            AppendCertificateIdentityName(str, certificate);
            return str.ToString();
        }

        internal static ReadOnlyCollection<IAuthorizationPolicy> CreatePrincipalNameAuthorizationPolicies(string principalName)
        {
            if (principalName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("principalName");

            Claim identityClaim;
            Claim primaryPrincipal;
            if (principalName.Contains("@") || principalName.Contains(@"\"))
            {
                identityClaim = new Claim(ClaimTypes.Upn, principalName, Rights.Identity);
                primaryPrincipal = Claim.CreateUpnClaim(principalName);
            }
            else
            {
                identityClaim = new Claim(ClaimTypes.Spn, principalName, Rights.Identity);
                primaryPrincipal = Claim.CreateSpnClaim(principalName);
            }

            List<Claim> claims = new List<Claim>(2);
            claims.Add(identityClaim);
            claims.Add(primaryPrincipal);

            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            policies.Add(new UnconditionalPolicy(SecurityUtils.CreateIdentity(principalName), new DefaultClaimSet(ClaimSet.Anonymous, claims)));
            return policies.AsReadOnly();
        }

        internal static string GetIdentityNamesFromPolicies(IList<IAuthorizationPolicy> authPolicies)
        {
            return GetIdentityNamesFromContext(AuthorizationContext.CreateDefaultAuthorizationContext(authPolicies));
        }

        internal static string GetIdentityNamesFromContext(AuthorizationContext authContext)
        {
            if (authContext == null)
                return String.Empty;

            StringBuilder str = new StringBuilder(256);
            for (int i = 0; i < authContext.ClaimSets.Count; ++i)
            {
                ClaimSet claimSet = authContext.ClaimSets[i];

                // Windows
                WindowsClaimSet windows = claimSet as WindowsClaimSet;
                if (windows != null)
                {
                    if (str.Length > 0)
                        str.Append(", ");

                    AppendIdentityName(str, windows.WindowsIdentity);
                }
                else
                {
                    // X509
                    X509CertificateClaimSet x509 = claimSet as X509CertificateClaimSet;
                    if (x509 != null)
                    {
                        if (str.Length > 0)
                            str.Append(", ");

                        AppendCertificateIdentityName(str, x509.X509Certificate);
                    }
                }
            }

            if (str.Length <= 0)
            {
                List<IIdentity> identities = null;
                object obj;
                if (authContext.Properties.TryGetValue(SecurityUtils.Identities, out obj))
                {
                    identities = obj as List<IIdentity>;
                }
                if (identities != null)
                {
                    for (int i = 0; i < identities.Count; ++i)
                    {
                        IIdentity identity = identities[i];
                        if (identity != null)
                        {
                            if (str.Length > 0)
                                str.Append(", ");

                            AppendIdentityName(str, identity);
                        }
                    }
                }
            }
            return str.Length <= 0 ? String.Empty : str.ToString();
        }

        internal static void AppendCertificateIdentityName(StringBuilder str, X509Certificate2 certificate)
        {
            string value = certificate.SubjectName.Name;
            if (String.IsNullOrEmpty(value))
            {
                value = certificate.GetNameInfo(X509NameType.DnsName, false);
                if (String.IsNullOrEmpty(value))
                {
                    value = certificate.GetNameInfo(X509NameType.SimpleName, false);
                    if (String.IsNullOrEmpty(value))
                    {
                        value = certificate.GetNameInfo(X509NameType.EmailName, false);
                        if (String.IsNullOrEmpty(value))
                        {
                            value = certificate.GetNameInfo(X509NameType.UpnName, false);
                        }
                    }
                }
            }
            // Same format as X509Identity
            str.Append(String.IsNullOrEmpty(value) ? "<x509>" : value);
            str.Append("; ");
            str.Append(certificate.Thumbprint);
        }

        internal static void AppendIdentityName(StringBuilder str, IIdentity identity)
        {
            string name = null;
            try
            {
                name = identity.Name;
            }
#pragma warning suppress 56500
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                // suppress exception, this is just info.
            }

            str.Append(String.IsNullOrEmpty(name) ? "<null>" : name);

            WindowsIdentity windows = identity as WindowsIdentity;
            if (windows != null)
            {
                if (windows.User != null)
                {
                    str.Append("; ");
                    str.Append(windows.User.ToString());
                }
            }
            else
            {
                WindowsSidIdentity sid = identity as WindowsSidIdentity;
                if (sid != null)
                {
                    str.Append("; ");
                    str.Append(sid.SecurityIdentifier.ToString());
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls critical methods UnsafeGetDomain, UnsafeGetUserName, UnsafeGetPassword and UnsafeGetCurrentUserSidAsString.")]
        [SecurityCritical]
        internal static string AppendWindowsAuthenticationInfo(string inputString, NetworkCredential credential,
            AuthenticationLevel authenticationLevel, TokenImpersonationLevel impersonationLevel)
        {
            const string delimiter = "\0"; // nonprintable characters are invalid for SSPI Domain/UserName/Password

            if (IsDefaultNetworkCredential(credential))
            {
                string sid = UnsafeGetCurrentUserSidAsString();
                return string.Concat(inputString, delimiter,
                    sid, delimiter,
                    AuthenticationLevelHelper.ToString(authenticationLevel), delimiter,
                    TokenImpersonationLevelHelper.ToString(impersonationLevel));
            }
            else
            {
                return string.Concat(inputString, delimiter,
                    NetworkCredentialHelper.UnsafeGetDomain(credential), delimiter,
                    NetworkCredentialHelper.UnsafeGetUsername(credential), delimiter,
                    NetworkCredentialHelper.UnsafeGetPassword(credential), delimiter,
                    AuthenticationLevelHelper.ToString(authenticationLevel), delimiter,
                    TokenImpersonationLevelHelper.ToString(impersonationLevel));
            }
        }

        internal static string GetIdentityName(IIdentity identity)
        {
            StringBuilder str = new StringBuilder(256);
            AppendIdentityName(str, identity);
            return str.ToString();
        }

        /// <SecurityNote>
        /// Critical - Calls an UnsafeNativeMethod and a Critical method (GetFipsAlgorithmPolicyKeyFromRegistry)
        /// Safe - processes the return and just returns a bool, which is safe
        /// </SecurityNote>
        internal static bool IsChannelBindingDisabled
        {
            [SecuritySafeCritical]
            get
            {
                return ((GetSuppressChannelBindingValue() & 0x1) != 0);
            }
        }

        const string suppressChannelBindingRegistryKey = @"System\CurrentControlSet\Control\Lsa";

        /// <SecurityNote>
        /// Critical - Asserts to get a value from the registry
        /// </SecurityNote>
        [SecurityCritical]
        [RegistryPermission(SecurityAction.Assert, Read = @"HKEY_LOCAL_MACHINE\" + suppressChannelBindingRegistryKey)]
        internal static int GetSuppressChannelBindingValue()
        {
            int channelBindingPolicyKeyValue = 0;

            try
            {
                using (RegistryKey channelBindingPolicyKey = Registry.LocalMachine.OpenSubKey(suppressChannelBindingRegistryKey, false))
                {
                    if (channelBindingPolicyKey != null)
                    {
                        object data = channelBindingPolicyKey.GetValue("SuppressChannelBindingInfo");
                        if (data != null)
                            channelBindingPolicyKeyValue = (int)data;
                    }
                }
            }
#pragma warning suppress 56500
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;
            }

            return channelBindingPolicyKeyValue;
        }

        internal static bool IsSecurityBindingSuitableForChannelBinding(TransportSecurityBindingElement securityBindingElement)
        {
            if (securityBindingElement == null)
            {
                return false;
            }

            // channel binding of OperationSupportingTokenParameters, OptionalEndpointSupportingTokenParameters, or OptionalOperationSupportingTokenParameters
            // is not supported in Win7
            if (AreSecurityTokenParametersSuitableForChannelBinding(securityBindingElement.EndpointSupportingTokenParameters.Endorsing))
            {
                return true;
            }

            if (AreSecurityTokenParametersSuitableForChannelBinding(securityBindingElement.EndpointSupportingTokenParameters.Signed))
            {
                return true;
            }

            if (AreSecurityTokenParametersSuitableForChannelBinding(securityBindingElement.EndpointSupportingTokenParameters.SignedEncrypted))
            {
                return true;
            }

            if (AreSecurityTokenParametersSuitableForChannelBinding(securityBindingElement.EndpointSupportingTokenParameters.SignedEndorsing))
            {
                return true;
            }

            return false;
        }

        internal static bool AreSecurityTokenParametersSuitableForChannelBinding(Collection<SecurityTokenParameters> tokenParameters)
        {
            if (tokenParameters == null)
            {
                return false;
            }

            foreach (SecurityTokenParameters stp in tokenParameters)
            {
                if (stp is SspiSecurityTokenParameters || stp is KerberosSecurityTokenParameters)
                {
                    return true;
                }

                SecureConversationSecurityTokenParameters scstp = stp as SecureConversationSecurityTokenParameters;
                if (scstp != null)
                {
                    return IsSecurityBindingSuitableForChannelBinding(scstp.BootstrapSecurityBindingElement as TransportSecurityBindingElement);
                }
            }

            return false;
        }

        internal static void ThrowIfNegotiationFault(Message message, EndpointAddress target)
        {
            if (message.IsFault)
            {
                MessageFault fault = MessageFault.CreateFault(message, TransportDefaults.MaxSecurityFaultSize);
                Exception faultException = new FaultException(fault, message.Headers.Action);
                if (fault.Code != null && fault.Code.IsReceiverFault && fault.Code.SubCode != null)
                {
                    FaultCode subCode = fault.Code.SubCode;
                    if (subCode.Name == DotNetSecurityStrings.SecurityServerTooBusyFault && subCode.Namespace == DotNetSecurityStrings.Namespace)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ServerTooBusyException(SR.GetString(SR.SecurityServerTooBusy, target), faultException));
                    }
                    else if (subCode.Name == AddressingStrings.EndpointUnavailable && subCode.Namespace == message.Version.Addressing.Namespace)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(SR.GetString(SR.SecurityEndpointNotFound, target), faultException));
                    }
                }
                throw TraceUtility.ThrowHelperError(faultException, message);
            }
        }

        internal static bool IsSecurityFault(MessageFault fault, SecurityStandardsManager standardsManager)
        {
            if (fault.Code.IsSenderFault)
            {
                FaultCode subCode = fault.Code.SubCode;
                if (subCode != null)
                {
                    return (subCode.Namespace == standardsManager.SecurityVersion.HeaderNamespace.Value
                        || subCode.Namespace == standardsManager.SecureConversationDriver.Namespace.Value
                        || subCode.Namespace == standardsManager.TrustDriver.Namespace.Value
                        || subCode.Namespace == DotNetSecurityStrings.Namespace);
                }
            }
            return false;
        }

        internal static Exception CreateSecurityFaultException(Message unverifiedMessage)
        {
            MessageFault fault = MessageFault.CreateFault(unverifiedMessage, TransportDefaults.MaxSecurityFaultSize);
            return CreateSecurityFaultException(fault);
        }

        internal static Exception CreateSecurityFaultException(MessageFault fault)
        {
            FaultException faultException = FaultException.CreateFault(fault, typeof(string), typeof(object));
            return new MessageSecurityException(SR.GetString(SR.UnsecuredMessageFaultReceived), faultException);
        }

        internal static MessageFault CreateSecurityContextNotFoundFault(SecurityStandardsManager standardsManager, string action)
        {
            SecureConversationDriver scDriver = standardsManager.SecureConversationDriver;
            FaultCode subCode = new FaultCode(scDriver.BadContextTokenFaultCode.Value, scDriver.Namespace.Value);
            FaultReason reason;
            if (action != null)
            {
                reason = new FaultReason(SR.GetString(SR.BadContextTokenOrActionFaultReason, action), CultureInfo.CurrentCulture);
            }
            else
            {
                reason = new FaultReason(SR.GetString(SR.BadContextTokenFaultReason), CultureInfo.CurrentCulture);
            }
            FaultCode senderCode = FaultCode.CreateSenderFaultCode(subCode);
            return MessageFault.CreateFault(senderCode, reason);
        }

        internal static MessageFault CreateSecurityMessageFault(Exception e, SecurityStandardsManager standardsManager)
        {
            bool isSecurityError = false;
            bool isTokenValidationError = false;
            bool isGenericTokenError = false;
            FaultException faultException = null;
            while (e != null)
            {
                if (e is SecurityTokenValidationException)
                {
                    if (e is SecurityContextTokenValidationException)
                    {
                        return CreateSecurityContextNotFoundFault(SecurityStandardsManager.DefaultInstance, null);
                    }
                    isSecurityError = true;
                    isTokenValidationError = true;
                    break;
                }
                else if (e is SecurityTokenException)
                {
                    isSecurityError = true;
                    isGenericTokenError = true;
                    break;
                }
                else if (e is MessageSecurityException)
                {
                    MessageSecurityException ms = (MessageSecurityException)e;
                    if (ms.Fault != null)
                    {
                        return ms.Fault;
                    }
                    isSecurityError = true;
                }
                else if (e is FaultException)
                {
                    faultException = (FaultException)e;
                    break;
                }
                e = e.InnerException;
            }
            if (!isSecurityError && faultException == null)
            {
                return null;
            }
            FaultCode subCode;
            FaultReason reason;
            SecurityVersion wss = standardsManager.SecurityVersion;
            if (isTokenValidationError)
            {
                subCode = new FaultCode(wss.FailedAuthenticationFaultCode.Value, wss.HeaderNamespace.Value);
                reason = new FaultReason(SR.GetString(SR.FailedAuthenticationFaultReason), CultureInfo.CurrentCulture);
            }
            else if (isGenericTokenError)
            {
                subCode = new FaultCode(wss.InvalidSecurityTokenFaultCode.Value, wss.HeaderNamespace.Value);
                reason = new FaultReason(SR.GetString(SR.InvalidSecurityTokenFaultReason), CultureInfo.CurrentCulture);
            }
            else if (faultException != null)
            {
                // Only support Code and Reason.  No detail or action customization.
                return MessageFault.CreateFault(faultException.Code, faultException.Reason);
            }
            else
            {
                subCode = new FaultCode(wss.InvalidSecurityFaultCode.Value, wss.HeaderNamespace.Value);
                reason = new FaultReason(SR.GetString(SR.InvalidSecurityFaultReason), CultureInfo.CurrentCulture);
            }
            FaultCode senderCode = FaultCode.CreateSenderFaultCode(subCode);
            return MessageFault.CreateFault(senderCode, reason);
        }

        internal static bool IsCompositeDuplexBinding(BindingContext context)
        {
            return ((context.Binding.Elements.Find<CompositeDuplexBindingElement>() != null)
                    || (context.Binding.Elements.Find<InternalDuplexBindingElement>() != null));
        }

        // The method checks TransportToken, ProtectionToken and all SupportingTokens to find a
        // UserNameSecurityToken. If found, it sets the password of the UserNameSecurityToken to null. 
        // Custom UserNameSecurityToken are skipped. 
        internal static void ErasePasswordInUsernameTokenIfPresent(SecurityMessageProperty messageProperty)
        {
            if (messageProperty == null)
            {
                // Nothing to fix.
                return;
            }

            if (messageProperty.TransportToken != null)
            {
                UserNameSecurityToken token = messageProperty.TransportToken.SecurityToken as UserNameSecurityToken;
                if ((token != null) && !messageProperty.TransportToken.SecurityToken.GetType().IsSubclassOf(typeof(UserNameSecurityToken)))
                {
                    messageProperty.TransportToken = new SecurityTokenSpecification(new UserNameSecurityToken(token.UserName, null, token.Id), messageProperty.TransportToken.SecurityTokenPolicies);
                }
            }

            if (messageProperty.ProtectionToken != null)
            {
                UserNameSecurityToken token = messageProperty.ProtectionToken.SecurityToken as UserNameSecurityToken;
                if ((token != null) && !messageProperty.ProtectionToken.SecurityToken.GetType().IsSubclassOf(typeof(UserNameSecurityToken)))
                {
                    messageProperty.ProtectionToken = new SecurityTokenSpecification(new UserNameSecurityToken(token.UserName, null, token.Id), messageProperty.ProtectionToken.SecurityTokenPolicies);
                }
            }

            if (messageProperty.HasIncomingSupportingTokens)
            {
                for (int i = 0; i < messageProperty.IncomingSupportingTokens.Count; ++i)
                {
                    SupportingTokenSpecification supportingTokenSpecification = messageProperty.IncomingSupportingTokens[i];
                    UserNameSecurityToken token = supportingTokenSpecification.SecurityToken as UserNameSecurityToken;
                    if ((token != null) && !supportingTokenSpecification.SecurityToken.GetType().IsSubclassOf(typeof(UserNameSecurityToken)))
                    {
                        messageProperty.IncomingSupportingTokens[i] = new SupportingTokenSpecification(new UserNameSecurityToken(token.UserName, null, token.Id), supportingTokenSpecification.SecurityTokenPolicies, supportingTokenSpecification.SecurityTokenAttachmentMode, supportingTokenSpecification.SecurityTokenParameters);
                    }
                }
            }
        }

        // work-around to Windows SE Bug 141614
        [Fx.Tag.SecurityNote(Critical = "Uses unsafe critical method UnsafeGetPassword to access the credential password without a Demand.",
            Safe = "Only uses the password to construct a cloned NetworkCredential instance, does not leak password value.")]
        [SecuritySafeCritical]
        internal static void FixNetworkCredential(ref NetworkCredential credential)
        {
            if (credential == null)
            {
                return;
            }
            string username = NetworkCredentialHelper.UnsafeGetUsername(credential);
            string domain = NetworkCredentialHelper.UnsafeGetDomain(credential);
            if (!string.IsNullOrEmpty(username) && string.IsNullOrEmpty(domain))
            {
                // do the splitting only if there is exactly 1 \ or exactly 1 @
                string[] partsWithSlashDelimiter = username.Split('\\');
                string[] partsWithAtDelimiter = username.Split('@');
                if (partsWithSlashDelimiter.Length == 2 && partsWithAtDelimiter.Length == 1)
                {
                    if (!string.IsNullOrEmpty(partsWithSlashDelimiter[0]) && !string.IsNullOrEmpty(partsWithSlashDelimiter[1]))
                    {
                        credential = new NetworkCredential(partsWithSlashDelimiter[1], NetworkCredentialHelper.UnsafeGetPassword(credential), partsWithSlashDelimiter[0]);
                    }
                }
                else if (partsWithSlashDelimiter.Length == 1 && partsWithAtDelimiter.Length == 2)
                {
                    if (!string.IsNullOrEmpty(partsWithAtDelimiter[0]) && !string.IsNullOrEmpty(partsWithAtDelimiter[1]))
                    {
                        credential = new NetworkCredential(partsWithAtDelimiter[0], NetworkCredentialHelper.UnsafeGetPassword(credential), partsWithAtDelimiter[1]);
                    }
                }
            }
        }

        // WORKAROUND, [....], VSWhidbey 561276: The first NetworkCredential must be created in a lock.
        internal static void PrepareNetworkCredential()
        {
            if (dummyNetworkCredential == null)
            {
                PrepareNetworkCredentialWorker();
            }
        }

        // Since this takes a lock, it probably won't be inlined, but the typical case will be.
        static void PrepareNetworkCredentialWorker()
        {
            lock (dummyNetworkCredentialLock)
            {
                dummyNetworkCredential = new NetworkCredential("dummy", "dummy");
            }
        }

        // This is the workaround, Since store.Certificates returns a full collection
        // of certs in store.  These are holding native resources.
        internal static void ResetAllCertificates(X509Certificate2Collection certificates)
        {
            if (certificates != null)
            {
                for (int i = 0; i < certificates.Count; ++i)
                {
                    ResetCertificate(certificates[i]);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls critical method X509Certificate2.Reset.",
            Safe = "Per review from CLR security team, this method does nothing unsafe.")]
        [SecuritySafeCritical]
        internal static void ResetCertificate(X509Certificate2 certificate)
        {
            certificate.Reset();
        }

        internal static bool IsDefaultNetworkCredential(NetworkCredential credential)
        {
            return NetworkCredentialHelper.IsDefault(credential);
        }

        internal static void OpenTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout)
        {
            OpenCommunicationObject(tokenProvider as ICommunicationObject, timeout);
        }

        internal static IAsyncResult BeginOpenTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            return new OpenCommunicationObjectAsyncResult(tokenProvider, timeout, callback, state);
        }

        internal static void EndOpenTokenProviderIfRequired(IAsyncResult result)
        {
            OpenCommunicationObjectAsyncResult.End(result);
        }

        internal static IAsyncResult BeginCloseTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            return new CloseCommunicationObjectAsyncResult(tokenProvider, timeout, callback, state);
        }

        internal static void EndCloseTokenProviderIfRequired(IAsyncResult result)
        {
            CloseCommunicationObjectAsyncResult.End(result);
        }

        internal static void CloseTokenProviderIfRequired(SecurityTokenProvider tokenProvider, TimeSpan timeout)
        {
            CloseCommunicationObject(tokenProvider, false, timeout);
        }

        internal static void CloseTokenProviderIfRequired(SecurityTokenProvider tokenProvider, bool aborted, TimeSpan timeout)
        {
            CloseCommunicationObject(tokenProvider, aborted, timeout);
        }

        internal static void AbortTokenProviderIfRequired(SecurityTokenProvider tokenProvider)
        {
            CloseCommunicationObject(tokenProvider, true, TimeSpan.Zero);
        }

        internal static void OpenTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout)
        {
            OpenCommunicationObject(tokenAuthenticator as ICommunicationObject, timeout);
        }

        internal static void CloseTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout)
        {
            CloseTokenAuthenticatorIfRequired(tokenAuthenticator, false, timeout);
        }

        internal static void CloseTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, bool aborted, TimeSpan timeout)
        {
            CloseCommunicationObject(tokenAuthenticator, aborted, timeout);
        }

        internal static IAsyncResult BeginOpenTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            return new OpenCommunicationObjectAsyncResult(tokenAuthenticator, timeout, callback, state);
        }

        internal static void EndOpenTokenAuthenticatorIfRequired(IAsyncResult result)
        {
            OpenCommunicationObjectAsyncResult.End(result);
        }

        internal static IAsyncResult BeginCloseTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            return new CloseCommunicationObjectAsyncResult(tokenAuthenticator, timeout, callback, state);
        }

        internal static void EndCloseTokenAuthenticatorIfRequired(IAsyncResult result)
        {
            CloseCommunicationObjectAsyncResult.End(result);
        }

        internal static void AbortTokenAuthenticatorIfRequired(SecurityTokenAuthenticator tokenAuthenticator)
        {
            CloseCommunicationObject(tokenAuthenticator, true, TimeSpan.Zero);
        }

        static void OpenCommunicationObject(ICommunicationObject obj, TimeSpan timeout)
        {
            if (obj != null)
                obj.Open(timeout);
        }

        static void CloseCommunicationObject(Object obj, bool aborted, TimeSpan timeout)
        {
            if (obj != null)
            {
                ICommunicationObject co = obj as ICommunicationObject;
                if (co != null)
                {
                    if (aborted)
                    {
                        try
                        {
                            co.Abort();
                        }
                        catch (CommunicationException e)
                        {
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        }
                    }
                    else
                    {
                        co.Close(timeout);
                    }
                }
                else if (obj is IDisposable)
                {
                    ((IDisposable)obj).Dispose();
                }
            }
        }

        class OpenCommunicationObjectAsyncResult : AsyncResult
        {
            ICommunicationObject communicationObject;
            static AsyncCallback onOpen;

            public OpenCommunicationObjectAsyncResult(object obj, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.communicationObject = obj as ICommunicationObject;

                bool completeSelf = false;
                if (this.communicationObject == null)
                {
                    completeSelf = true;
                }
                else
                {
                    if (onOpen == null)
                    {
                        onOpen = Fx.ThunkCallback(new AsyncCallback(OnOpen));
                    }

                    IAsyncResult result = this.communicationObject.BeginOpen(timeout, onOpen, this);
                    if (result.CompletedSynchronously)
                    {
                        this.communicationObject.EndOpen(result);
                        completeSelf = true;
                    }
                }

                if (completeSelf)
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenCommunicationObjectAsyncResult>(result);
            }

            static void OnOpen(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                OpenCommunicationObjectAsyncResult thisPtr =
                    (OpenCommunicationObjectAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    thisPtr.communicationObject.EndOpen(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }
                thisPtr.Complete(false, completionException);
            }
        }

        class CloseCommunicationObjectAsyncResult : AsyncResult
        {
            ICommunicationObject communicationObject;
            static AsyncCallback onClose;

            public CloseCommunicationObjectAsyncResult(object obj, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.communicationObject = obj as ICommunicationObject;

                bool completeSelf = false;
                if (this.communicationObject == null)
                {
                    IDisposable disposable = obj as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                    completeSelf = true;
                }
                else
                {
                    if (onClose == null)
                    {
                        onClose = Fx.ThunkCallback(new AsyncCallback(OnClose));
                    }

                    IAsyncResult result = this.communicationObject.BeginClose(timeout, onClose, this);
                    if (result.CompletedSynchronously)
                    {
                        this.communicationObject.EndClose(result);
                        completeSelf = true;
                    }
                }

                if (completeSelf)
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseCommunicationObjectAsyncResult>(result);
            }

            static void OnClose(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                CloseCommunicationObjectAsyncResult thisPtr =
                    (CloseCommunicationObjectAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    thisPtr.communicationObject.EndClose(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }
                thisPtr.Complete(false, completionException);
            }
        }

        internal static void MatchRstWithEndpointFilter(Message rst, IMessageFilterTable<EndpointAddress> endpointFilterTable, Uri listenUri)
        {
            if (endpointFilterTable == null)
            {
                return;
            }
            Collection<EndpointAddress> result = new Collection<EndpointAddress>();
            if (!endpointFilterTable.GetMatchingValues(rst, result))
            {
                throw TraceUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.RequestSecurityTokenDoesNotMatchEndpointFilters, listenUri)), rst);
            }
        }

        // match the RST with the endpoint filters in case there is at least 1 asymmetric signature in the message
        internal static bool ShouldMatchRstWithEndpointFilter(SecurityBindingElement sbe)
        {
            foreach (SecurityTokenParameters parameters in new SecurityTokenParametersEnumerable(sbe, true))
            {
                if (parameters.HasAsymmetricKey)
                {
                    return true;
                }
            }
            return false;
        }

        internal static SecurityStandardsManager CreateSecurityStandardsManager(MessageSecurityVersion securityVersion, SecurityTokenManager tokenManager)
        {
            SecurityTokenSerializer tokenSerializer = tokenManager.CreateSecurityTokenSerializer(securityVersion.SecurityTokenVersion);
            return new SecurityStandardsManager(securityVersion, tokenSerializer);
        }

        internal static SecurityStandardsManager CreateSecurityStandardsManager(SecurityTokenRequirement requirement, SecurityTokenManager tokenManager)
        {
            MessageSecurityTokenVersion securityVersion = (MessageSecurityTokenVersion)requirement.GetProperty<MessageSecurityTokenVersion>(ServiceModelSecurityTokenRequirement.MessageSecurityVersionProperty);
            if (securityVersion == MessageSecurityTokenVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10)
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity10WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10, tokenManager);
            else if (securityVersion == MessageSecurityTokenVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005)
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11, tokenManager);
            else if (securityVersion == MessageSecurityTokenVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005BasicSecurityProfile10)
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11BasicSecurityProfile10, tokenManager);
            else if (securityVersion == MessageSecurityTokenVersion.WSSecurity10WSTrust13WSSecureConversation13BasicSecurityProfile10)
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity10WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10, tokenManager);
            else if (securityVersion == MessageSecurityTokenVersion.WSSecurity11WSTrust13WSSecureConversation13)
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12, tokenManager);
            else if (securityVersion == MessageSecurityTokenVersion.WSSecurity11WSTrust13WSSecureConversation13BasicSecurityProfile10)
                return CreateSecurityStandardsManager(MessageSecurityVersion.WSSecurity11WSTrust13WSSecureConversation13WSSecurityPolicy12BasicSecurityProfile10, tokenManager);
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        internal static SecurityStandardsManager CreateSecurityStandardsManager(MessageSecurityVersion securityVersion, SecurityTokenSerializer securityTokenSerializer)
        {
            if (securityVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("securityVersion"));
            }
            if (securityTokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("securityTokenSerializer");
            }
            return new SecurityStandardsManager(securityVersion, securityTokenSerializer);
        }

        static bool TryCreateIdentity(ClaimSet claimSet, string claimType, out EndpointIdentity identity)
        {
            identity = null;
            foreach (Claim claim in claimSet.FindClaims(claimType, null))
            {
                identity = EndpointIdentity.CreateIdentity(claim);
                return true;
            }
            return false;
        }

        internal static EndpointIdentity GetServiceCertificateIdentity(X509Certificate2 certificate)
        {
            using (X509CertificateClaimSet claimSet = new X509CertificateClaimSet(certificate))
            {
                EndpointIdentity identity;
                if (!TryCreateIdentity(claimSet, ClaimTypes.Dns, out identity))
                {
                    TryCreateIdentity(claimSet, ClaimTypes.Rsa, out identity);
                }
                return identity;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses unsafe critical method UnsafeGetPassword to access the credential password without a Demand.",
            Safe = "Only uses the password to construct a new NetworkCredential which will then protect access, password does not leak from this method.")]
        [SecuritySafeCritical]
        internal static NetworkCredential GetNetworkCredentialsCopy(NetworkCredential networkCredential)
        {
            NetworkCredential result;
            if (networkCredential != null && !NetworkCredentialHelper.IsDefault(networkCredential))
            {
                result = new NetworkCredential(NetworkCredentialHelper.UnsafeGetUsername(networkCredential), NetworkCredentialHelper.UnsafeGetPassword(networkCredential), NetworkCredentialHelper.UnsafeGetDomain(networkCredential));
            }
            else
            {
                result = networkCredential;
            }
            return result;
        }

        internal static NetworkCredential GetNetworkCredentialOrDefault(NetworkCredential credential)
        {
            // because of VSW 564452, we dont use CredentialCache.DefaultNetworkCredentials in our OM. Instead we
            // use an empty NetworkCredential to denote the default credentials
            if (NetworkCredentialHelper.IsNullOrEmpty(credential))
            {
                // FYI: this will fail with SecurityException in PT due to Demand for EnvironmentPermission.
                // Typically a PT app should not have access to DefaultNetworkCredentials. If there is a valid reason,
                // see UnsafeGetDefaultNetworkCredentials.
                return CredentialCache.DefaultNetworkCredentials;
            }
            else
            {
                return credential;
            }
        }

        static class NetworkCredentialHelper
        {
            [Fx.Tag.SecurityNote(Critical = "Uses unsafe critical methods UnsafeGetUsername, UnsafeGetPassword, and UnsafeGetDomain to access the credential details without a Demand.",
                Safe = "Only uses the protected values to test for null/empty.  Does not leak.")]
            [SecuritySafeCritical]
            static internal bool IsNullOrEmpty(NetworkCredential credential)
            {
                return credential == null ||
                        (
                            String.IsNullOrEmpty(UnsafeGetUsername(credential)) &&
                            String.IsNullOrEmpty(UnsafeGetDomain(credential)) &&
                            String.IsNullOrEmpty(UnsafeGetPassword(credential))
                        );
            }

            [Fx.Tag.SecurityNote(Critical = "Uses unsafe critical method UnsafeGetDefaultNetworkCredentials to access the default network credentials without a Demand.",
                Safe = "Only uses the default credentials to test for equality and uses the system credential's .Equals, not the caller's.")]
            [SecuritySafeCritical]
            static internal bool IsDefault(NetworkCredential credential)
            {
                return UnsafeGetDefaultNetworkCredentials().Equals(credential);
            }

            [Fx.Tag.SecurityNote(Critical = "Asserts SecurityPermission(UnmanagedCode) in order to get the NetworkCredential password."
                + "This is used for example to test for empty/null or to construct a cloned NetworkCredential."
                + "Callers absolutely must not leak the return value.")]
            [SecurityCritical]
            [EnvironmentPermission(SecurityAction.Assert, Read = "USERNAME")]
            static internal string UnsafeGetUsername(NetworkCredential credential)
            {
                return credential.UserName;
            }

            [Fx.Tag.SecurityNote(Critical = "Asserts SecurityPermission(UnmanagedCode) in order to get the NetworkCredential password."
                + "This is used for example to test for empty/null or to construct a cloned NetworkCredential."
                + "Callers absolutely must not leak the return value.")]
            [SecurityCritical]
            [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
            static internal string UnsafeGetPassword(NetworkCredential credential)
            {
                return credential.Password;
            }

            [Fx.Tag.SecurityNote(Critical = "Asserts SecurityPermission(UnmanagedCode) in order to get the NetworkCredential password."
                + "This is used for example to test for empty/null or to construct a cloned NetworkCredential."
                + "Callers absolutely must not leak the return value.")]
            [SecurityCritical]
            [EnvironmentPermission(SecurityAction.Assert, Read = "USERDOMAIN")]
            static internal string UnsafeGetDomain(NetworkCredential credential)
            {
                return credential.Domain;
            }

            [Fx.Tag.SecurityNote(Critical = "Asserts EnvironmentPermission(Read='USERNAME') in order to get the DefaultNetworkCredentials in PT."
                + "This is used for example to test for instance equality with a specific NetworkCredential."
                + "Callers absolutely must not leak the return value.")]
            [SecurityCritical]
            [EnvironmentPermission(SecurityAction.Assert, Read = "USERNAME")]
            static NetworkCredential UnsafeGetDefaultNetworkCredentials()
            {
                return CredentialCache.DefaultNetworkCredentials;
            }
        }

        internal static SafeFreeCredentials GetCredentialsHandle(string package, NetworkCredential credential, bool isServer, params string[] additionalPackages)
        {
            SafeFreeCredentials credentialsHandle;
            CredentialUse credentialUse = isServer ? CredentialUse.Inbound : CredentialUse.Outbound;
            if (credential == null || NetworkCredentialHelper.IsDefault(credential))
            {
                AuthIdentityEx authIdentity = new AuthIdentityEx(null, null, null, additionalPackages);
                credentialsHandle = SspiWrapper.AcquireCredentialsHandle(package, credentialUse, ref authIdentity);
            }
            else
            {
                SecurityUtils.FixNetworkCredential(ref credential);

                // we're not using DefaultCredentials, we need a
                // AuthIdentity struct to contain credentials
                AuthIdentityEx authIdentity = new AuthIdentityEx(credential.UserName, credential.Password, credential.Domain);
                credentialsHandle = SspiWrapper.AcquireCredentialsHandle(package, credentialUse, ref authIdentity);
            }
            return credentialsHandle;
        }

        internal static SafeFreeCredentials GetCredentialsHandle(Binding binding, KeyedByTypeCollection<IEndpointBehavior> behaviors)
        {
            ClientCredentials clientCredentials = (behaviors == null) ? null : behaviors.Find<ClientCredentials>();
            return GetCredentialsHandle(binding, clientCredentials);
        }

        internal static SafeFreeCredentials GetCredentialsHandle(Binding binding, ClientCredentials clientCredentials)
        {
            SecurityBindingElement sbe = (binding == null) ? null : binding.CreateBindingElements().Find<SecurityBindingElement>();
            return GetCredentialsHandle(sbe, clientCredentials);
        }

        internal static SafeFreeCredentials GetCredentialsHandle(SecurityBindingElement sbe, BindingContext context)
        {
            ClientCredentials clientCredentials = (context == null) ? null : context.BindingParameters.Find<ClientCredentials>();
            return GetCredentialsHandle(sbe, clientCredentials);
        }

        internal static SafeFreeCredentials GetCredentialsHandle(SecurityBindingElement sbe, ClientCredentials clientCredentials)
        {
            if (sbe == null)
            {
                return null;
            }

            bool isSspi = false;
            bool isKerberos = false;
            foreach (SecurityTokenParameters stp in new SecurityTokenParametersEnumerable(sbe, true))
            {
                if (stp is SecureConversationSecurityTokenParameters)
                {
                    SafeFreeCredentials result = GetCredentialsHandle(((SecureConversationSecurityTokenParameters)stp).BootstrapSecurityBindingElement, clientCredentials);
                    if (result != null)
                    {
                        return result;
                    }
                    continue;
                }
                else if (stp is IssuedSecurityTokenParameters)
                {
                    SafeFreeCredentials result = GetCredentialsHandle(((IssuedSecurityTokenParameters)stp).IssuerBinding, clientCredentials);
                    if (result != null)
                    {
                        return result;
                    }
                    continue;
                }
                else if (stp is SspiSecurityTokenParameters)
                {
                    isSspi = true;
                    break;
                }
                else if (stp is KerberosSecurityTokenParameters)
                {
                    isKerberos = true;
                    break;
                }
            }
            if (!isSspi && !isKerberos)
            {
                return null;
            }

            NetworkCredential credential = null;
            if (clientCredentials != null)
            {
                credential = SecurityUtils.GetNetworkCredentialOrDefault(clientCredentials.Windows.ClientCredential);
            }

            if (isKerberos)
            {
                return SecurityUtils.GetCredentialsHandle("Kerberos", credential, false);
            }
            // if OS is less that Vista cannot use !NTLM, Windows SE 142400

// To disable AllowNtlm warning.
#pragma warning disable 618

            else if (clientCredentials != null && !clientCredentials.Windows.AllowNtlm)
            {
                if (SecurityUtils.IsOsGreaterThanXP())
                {
                    return SecurityUtils.GetCredentialsHandle("Negotiate", credential, false, "!NTLM");
                }
                else
                {
                    return SecurityUtils.GetCredentialsHandle("Kerberos", credential, false);
                }
            }

#pragma warning restore 618

            return SecurityUtils.GetCredentialsHandle("Negotiate", credential, false);
        }

        internal static byte[] CloneBuffer(byte[] buffer)
        {
            byte[] copy = DiagnosticUtility.Utility.AllocateByteArray(buffer.Length);
            Buffer.BlockCopy(buffer, 0, copy, 0, buffer.Length);
            return copy;
        }

        internal static X509Certificate2 GetCertificateFromStore(StoreName storeName, StoreLocation storeLocation,
            X509FindType findType, object findValue, EndpointAddress target)
        {
            X509Certificate2 certificate = GetCertificateFromStoreCore(storeName, storeLocation, findType, findValue, target, true);
            if (certificate == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotFindCert, storeName, storeLocation, findType, findValue)));

            return certificate;
        }

        internal static bool TryGetCertificateFromStore(StoreName storeName, StoreLocation storeLocation,
            X509FindType findType, object findValue, EndpointAddress target, out X509Certificate2 certificate)
        {
            certificate = GetCertificateFromStoreCore(storeName, storeLocation, findType, findValue, target, false);
            return (certificate != null);
        }

        static X509Certificate2 GetCertificateFromStoreCore(StoreName storeName, StoreLocation storeLocation,
            X509FindType findType, object findValue, EndpointAddress target, bool throwIfMultipleOrNoMatch)
        {
            if (findValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("findValue");
            }
            X509CertificateStore store = new X509CertificateStore(storeName, storeLocation);
            X509Certificate2Collection certs = null;
            try
            {
                store.Open(OpenFlags.ReadOnly);
                certs = store.Find(findType, findValue, false);
                if (certs.Count == 1)
                {
                    return new X509Certificate2(certs[0]);
                }
                if (throwIfMultipleOrNoMatch)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateCertificateLoadException(
                        storeName, storeLocation, findType, findValue, target, certs.Count));
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                SecurityUtils.ResetAllCertificates(certs);
                store.Close();
            }
        }

        static Exception CreateCertificateLoadException(StoreName storeName, StoreLocation storeLocation,
            X509FindType findType, object findValue, EndpointAddress target, int certCount)
        {
            if (certCount == 0)
            {
                if (target == null)
                {
                    return new InvalidOperationException(SR.GetString(SR.CannotFindCert, storeName, storeLocation, findType, findValue));
                }
                else
                {
                    return new InvalidOperationException(SR.GetString(SR.CannotFindCertForTarget, storeName, storeLocation, findType, findValue, target));
                }
            }
            else
            {
                if (target == null)
                {
                    return new InvalidOperationException(SR.GetString(SR.FoundMultipleCerts, storeName, storeLocation, findType, findValue));
                }
                else
                {
                    return new InvalidOperationException(SR.GetString(SR.FoundMultipleCertsForTarget, storeName, storeLocation, findType, findValue, target));
                }
            }
        }

        public static SecurityBindingElement GetIssuerSecurityBindingElement(ServiceModelSecurityTokenRequirement requirement)
        {
            SecurityBindingElement bindingElement = requirement.SecureConversationSecurityBindingElement;
            if (bindingElement != null)
            {
                return bindingElement;
            }

            Binding binding = requirement.IssuerBinding;
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.IssuerBindingNotPresentInTokenRequirement, requirement));
            }
            BindingElementCollection bindingElements = binding.CreateBindingElements();
            return bindingElements.Find<SecurityBindingElement>();
        }

        public static int GetMaxNegotiationBufferSize(BindingContext bindingContext)
        {
            TransportBindingElement transport = bindingContext.RemainingBindingElements.Find<TransportBindingElement>();
            Fx.Assert(transport != null, "TransportBindingElement is null!");
            int maxNegoMessageSize;
            if (transport is ConnectionOrientedTransportBindingElement)
            {
                maxNegoMessageSize = ((ConnectionOrientedTransportBindingElement)transport).MaxBufferSize;
            }
            else if (transport is HttpTransportBindingElement)
            {
                maxNegoMessageSize = ((HttpTransportBindingElement)transport).MaxBufferSize;
            }
            else
            {
                maxNegoMessageSize = TransportDefaults.MaxBufferSize;
            }
            return maxNegoMessageSize;
        }

        public static bool TryCreateKeyFromIntrinsicKeyClause(SecurityKeyIdentifierClause keyIdentifierClause, SecurityTokenResolver resolver, out SecurityKey key)
        {
            key = null;
            if (keyIdentifierClause.CanCreateKey)
            {
                key = keyIdentifierClause.CreateKey();
                return true;
            }
            if (keyIdentifierClause is EncryptedKeyIdentifierClause)
            {
                EncryptedKeyIdentifierClause keyClause = (EncryptedKeyIdentifierClause)keyIdentifierClause;
                // PreSharp Bug: Parameter 'keyClause' to this public method must be validated: A null-dereference can occur here.
#pragma warning suppress 56506 // keyClause will not be null due to the if condition above.
                for (int i = 0; i < keyClause.EncryptingKeyIdentifier.Count; i++)
                {
                    SecurityKey unwrappingSecurityKey = null;
                    if (resolver.TryResolveSecurityKey(keyClause.EncryptingKeyIdentifier[i], out unwrappingSecurityKey))
                    {
                        byte[] wrappedKey = keyClause.GetEncryptedKey();
                        string wrappingAlgorithm = keyClause.EncryptionMethod;
                        byte[] unwrappedKey = unwrappingSecurityKey.DecryptKey(wrappingAlgorithm, wrappedKey);
                        key = new InMemorySymmetricSecurityKey(unwrappedKey, false);
                        return true;
                    }
                }
            }
            return false;
        }

        public static WrappedKeySecurityToken CreateTokenFromEncryptedKeyClause(EncryptedKeyIdentifierClause keyClause, SecurityToken unwrappingToken)
        {
            SecurityKeyIdentifier wrappingTokenReference = keyClause.EncryptingKeyIdentifier;
            byte[] wrappedKey = keyClause.GetEncryptedKey();
            SecurityKey unwrappingSecurityKey = unwrappingToken.SecurityKeys[0];
            string wrappingAlgorithm = keyClause.EncryptionMethod;
            byte[] unwrappedKey = unwrappingSecurityKey.DecryptKey(wrappingAlgorithm, wrappedKey);
            return new WrappedKeySecurityToken(SecurityUtils.GenerateId(), unwrappedKey, wrappingAlgorithm,
                unwrappingToken, wrappingTokenReference, wrappedKey, unwrappingSecurityKey
                    );
        }

        public static void ValidateAnonymityConstraint(WindowsIdentity identity, bool allowUnauthenticatedCallers)
        {
            if (!allowUnauthenticatedCallers && identity.User.IsWellKnown(WellKnownSidType.AnonymousSid))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(
                    new SecurityTokenValidationException(SR.GetString(SR.AnonymousLogonsAreNotAllowed)));
            }
        }

        static bool ComputeSslCipherStrengthRequirementFlag()
        {
            // validate only for  XP versions < XP SP3 and windows server versions < Win2K3 SP2
            if ((Environment.OSVersion.Version.Major > WindowsServerMajorNumber)
                || (Environment.OSVersion.Version.Major == WindowsServerMajorNumber && Environment.OSVersion.Version.Minor > WindowsServerMinorNumber))
            {
                return false;
            }
            // version <= Win2K3
            if (Environment.OSVersion.Version.Major == XPMajorNumber && Environment.OSVersion.Version.Minor == XPMinorNumber)
            {
                if ((Environment.OSVersion.ServicePack == string.Empty) || String.Equals(Environment.OSVersion.ServicePack, ServicePack1, StringComparison.OrdinalIgnoreCase) || String.Equals(Environment.OSVersion.ServicePack, ServicePack2, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    // the OS is XP SP3 or higher
                    return false;
                }
            }
            else if (Environment.OSVersion.Version.Major == WindowsServerMajorNumber && Environment.OSVersion.Version.Minor == WindowsServerMinorNumber)
            {
                if (Environment.OSVersion.ServicePack == string.Empty || String.Equals(Environment.OSVersion.ServicePack, ServicePack1, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    // the OS is Win2K3 SP2 or higher
                    return false;
                }
            }
            else
            {
                // this is <= XP. We should never get here but if we do validate SSL strength
                return true;
            }
        }

        public static bool ShouldValidateSslCipherStrength()
        {
            if (!isSslValidationRequirementDetermined)
            {
                shouldValidateSslCipherStrength = ComputeSslCipherStrengthRequirementFlag();
                Thread.MemoryBarrier();
                isSslValidationRequirementDetermined = true;
            }
            return shouldValidateSslCipherStrength;
        }

        public static void ValidateSslCipherStrength(int keySizeInBits)
        {
            if (ShouldValidateSslCipherStrength() && keySizeInBits < MinimumSslCipherStrength)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.SslCipherKeyTooSmall, keySizeInBits, MinimumSslCipherStrength)));
            }
        }

        public static bool TryCreateX509CertificateFromRawData(byte[] rawData, out X509Certificate2 certificate)
        {
            certificate = (rawData == null || rawData.Length == 0) ? null : new X509Certificate2(rawData);
            return certificate != null && certificate.Handle != IntPtr.Zero;
        }

        internal static string GetKeyDerivationAlgorithm(SecureConversationVersion version)
        {
            string derivationAlgorithm = null;
            if (version == SecureConversationVersion.WSSecureConversationFeb2005)
            {
                derivationAlgorithm = SecurityAlgorithms.Psha1KeyDerivation;
            }
            else if (version == SecureConversationVersion.WSSecureConversation13)
            {
                derivationAlgorithm = SecurityAlgorithms.Psha1KeyDerivationDec2005;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            return derivationAlgorithm;
        }

    }

    struct SecurityUniqueId
    {
        static long nextId = 0;
        static string commonPrefix = "uuid-" + Guid.NewGuid().ToString() + "-";

        long id;
        string prefix;
        string val;

        SecurityUniqueId(string prefix, long id)
        {
            this.id = id;
            this.prefix = prefix;
            this.val = null;
        }

        public static SecurityUniqueId Create()
        {
            return SecurityUniqueId.Create(commonPrefix);
        }

        public static SecurityUniqueId Create(string prefix)
        {
            return new SecurityUniqueId(prefix, Interlocked.Increment(ref nextId));
        }

        public string Value
        {
            get
            {
                if (this.val == null)
                    this.val = this.prefix + this.id.ToString(CultureInfo.InvariantCulture);

                return this.val;
            }
        }
    }

    static class EmptyReadOnlyCollection<T>
    {
        public static ReadOnlyCollection<T> Instance = new ReadOnlyCollection<T>(new List<T>());
    }

    class OperationWithTimeoutAsyncResult : TraceAsyncResult
    {
        static readonly Action<object> scheduledCallback = new Action<object>(OnScheduled);
        TimeoutHelper timeoutHelper;
        OperationWithTimeoutCallback operationWithTimeout;

        public OperationWithTimeoutAsyncResult(OperationWithTimeoutCallback operationWithTimeout, TimeSpan timeout, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.operationWithTimeout = operationWithTimeout;
            this.timeoutHelper = new TimeoutHelper(timeout);
            ActionItem.Schedule(scheduledCallback, this);
        }

        static void OnScheduled(object state)
        {
            OperationWithTimeoutAsyncResult thisResult = (OperationWithTimeoutAsyncResult)state;
            Exception completionException = null;
            try
            {
                using (thisResult.CallbackActivity == null ? null : ServiceModelActivity.BoundOperation(thisResult.CallbackActivity))
                {
                    thisResult.operationWithTimeout(thisResult.timeoutHelper.RemainingTime());
                }
            }
#pragma warning suppress 56500 // covered by FxCOP
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                completionException = e;
            }
            thisResult.Complete(false, completionException);
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<OperationWithTimeoutAsyncResult>(result);
        }
    }
}
