//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Diagnostics;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Xml;
    using Microsoft.Win32;

    static class SecurityUtils
    {
        public const string Identities = "Identities";
        static int fipsAlgorithmPolicy = -1;
        public const int WindowsVistaMajorNumber = 6;
        static IIdentity anonymousIdentity;

        // these should be kept in [....] with IIS70
        public const string AuthTypeNTLM = "NTLM";
        public const string AuthTypeNegotiate = "Negotiate";
        public const string AuthTypeKerberos = "Kerberos";
        public const string AuthTypeAnonymous = "";
        public const string AuthTypeCertMap = "SSL/PCT"; // mapped from a cert
        public const string AuthTypeBasic = "Basic"; //LogonUser

        internal static IIdentity AnonymousIdentity
        {
            get
            {
                if (anonymousIdentity == null)
                    anonymousIdentity = SecurityUtils.CreateIdentity(String.Empty);
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

        internal static byte[] CloneBuffer(byte[] buffer)
        {
            return CloneBuffer(buffer, 0, buffer.Length);
        }

        internal static byte[] CloneBuffer(byte[] buffer, int offset, int len)
        {
            DiagnosticUtility.DebugAssert(offset >= 0, "Negative offset passed to CloneBuffer.");
            DiagnosticUtility.DebugAssert(len >= 0, "Negative len passed to CloneBuffer.");
            DiagnosticUtility.DebugAssert(buffer.Length - offset >= len, "Invalid parameters to CloneBuffer.");

            byte[] copy = DiagnosticUtility.Utility.AllocateByteArray(len);
            Buffer.BlockCopy(buffer, offset, copy, 0, len);
            return copy;
        }

        internal static ReadOnlyCollection<SecurityKey> CreateSymmetricSecurityKeys( byte[] key )
        {
            List<SecurityKey> temp = new List<SecurityKey>( 1 );
            temp.Add( new InMemorySymmetricSecurityKey( key ) );
            return temp.AsReadOnly();
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

        internal static bool MatchesBuffer(byte[] src, byte[] dst)
        {
            return MatchesBuffer(src, 0, dst, 0);
        }

        internal static bool MatchesBuffer(byte[] src, int srcOffset, byte[] dst, int dstOffset)
        {
            DiagnosticUtility.DebugAssert(dstOffset >= 0, "Negative dstOffset passed to MatchesBuffer.");
            DiagnosticUtility.DebugAssert(srcOffset >= 0, "Negative srcOffset passed to MatchesBuffer.");

            // defensive programming
            if ((dstOffset < 0) || (srcOffset < 0))
                return false;

            if (src == null || srcOffset >= src.Length)
                return false;
            if (dst == null || dstOffset >= dst.Length)
                return false;
            if ((src.Length - srcOffset) != (dst.Length - dstOffset))
                return false;

            for (int i = srcOffset, j = dstOffset; i < src.Length; i++, j++)
            {
                if (src[i] != dst[j])
                    return false;
            }
            return true;
        }

        internal static string GetCertificateId(X509Certificate2 certificate)
        {
            string certificateId = certificate.SubjectName.Name;
            if (string.IsNullOrEmpty(certificateId))
                certificateId = certificate.Thumbprint;
            return certificateId;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls critical method X509Certificate2.Reset.",
            Safe = "Per review from CLR security team, this method does nothing unsafe.")]
        [SecuritySafeCritical]
        internal static void ResetCertificate(X509Certificate2 certificate)
        {
            certificate.Reset();
        }

        internal static bool IsCurrentlyTimeEffective(DateTime effectiveTime, DateTime expirationTime, TimeSpan maxClockSkew)
        {
            DateTime curEffectiveTime = (effectiveTime < DateTime.MinValue.Add(maxClockSkew)) ? effectiveTime : effectiveTime.Subtract(maxClockSkew);
            DateTime curExpirationTime = (expirationTime > DateTime.MaxValue.Subtract(maxClockSkew)) ? expirationTime : expirationTime.Add(maxClockSkew);
            DateTime curTime = DateTime.UtcNow;

            return (curEffectiveTime.ToUniversalTime() <= curTime) && (curTime < curExpirationTime.ToUniversalTime());
        }

        // Federal Information Processing Standards Publications
        // at http://www.itl.nist.gov/fipspubs/geninfo.htm
        internal static bool RequiresFipsCompliance
        {
            [Fx.Tag.SecurityNote(Critical = "Calls an UnsafeNativeMethod and a Critical method (GetFipsAlgorithmPolicyKeyFromRegistry.",
                Safe = "processes the return and just returns a bool, which is safe.")]
            [SecuritySafeCritical]
            get
            {
                if (fipsAlgorithmPolicy == -1)
                {
                    if (Environment.OSVersion.Version.Major >= WindowsVistaMajorNumber)
                    {
                        bool fipsEnabled;
#pragma warning suppress 56523 // we check for the return code of the method instead of calling GetLastWin32Error
                        bool readPolicy = (CAPI.S_OK == CAPI.BCryptGetFipsAlgorithmMode(out fipsEnabled));

                        if (readPolicy && fipsEnabled)
                            fipsAlgorithmPolicy = 1;
                        else
                            fipsAlgorithmPolicy = 0;
                    }
                    else
                    {
                        fipsAlgorithmPolicy = GetFipsAlgorithmPolicyKeyFromRegistry();
                        if (fipsAlgorithmPolicy != 1)
                            fipsAlgorithmPolicy = 0;
                    }
                }
                return fipsAlgorithmPolicy == 1;
            }
        }

        const string fipsPolicyRegistryKey = @"System\CurrentControlSet\Control\Lsa";

        /// <SecurityNote>
        /// Critical - Asserts to get a value from the registry
        /// </SecurityNote>
        [SecurityCritical]
        [RegistryPermission(SecurityAction.Assert, Read = @"HKEY_LOCAL_MACHINE\" + fipsPolicyRegistryKey)]
        static int GetFipsAlgorithmPolicyKeyFromRegistry()
        {
            int fipsAlgorithmPolicy = -1;
            using (RegistryKey fipsAlgorithmPolicyKey = Registry.LocalMachine.OpenSubKey(fipsPolicyRegistryKey, false))
            {
                if (fipsAlgorithmPolicyKey != null)
                {
                    object data = fipsAlgorithmPolicyKey.GetValue("FIPSAlgorithmPolicy");
                    if (data != null)
                        fipsAlgorithmPolicy = (int)data;
                }
            }
            return fipsAlgorithmPolicy;
        }

        class SimpleAuthorizationContext : AuthorizationContext
        {
            SecurityUniqueId id;
            UnconditionalPolicy policy;
            IDictionary<string, object> properties;

            public SimpleAuthorizationContext(IList<IAuthorizationPolicy> authorizationPolicies)
            {
                this.policy = (UnconditionalPolicy)authorizationPolicies[0];
                Dictionary<string, object> properties = new Dictionary<string, object>();
                if (this.policy.PrimaryIdentity != null && this.policy.PrimaryIdentity != SecurityUtils.AnonymousIdentity)
                {
                    List<IIdentity> identities = new List<IIdentity>();
                    identities.Add(this.policy.PrimaryIdentity);
                    properties.Add(SecurityUtils.Identities, identities);
                }
                // Might need to port ReadOnlyDictionary?
                this.properties = properties;
            }

            public override string Id
            {
                get
                {
                    if (this.id == null)
                        this.id = SecurityUniqueId.Create();
                    return this.id.Value;
                }
            }
            public override ReadOnlyCollection<ClaimSet> ClaimSets { get { return this.policy.Issuances; } }
            public override DateTime ExpirationTime { get { return this.policy.ExpirationTime; } }
            public override IDictionary<string, object> Properties { get { return this.properties; } }
        }

        internal static AuthorizationContext CreateDefaultAuthorizationContext(IList<IAuthorizationPolicy> authorizationPolicies)
        {
            AuthorizationContext authorizationContext;
            // This is faster than Policy evaluation.
            if (authorizationPolicies != null && authorizationPolicies.Count == 1 && authorizationPolicies[0] is UnconditionalPolicy)
            {
                authorizationContext = new SimpleAuthorizationContext(authorizationPolicies);
            }
            // degenerate case
            else if (authorizationPolicies == null || authorizationPolicies.Count <= 0)
            {
                return DefaultAuthorizationContext.Empty;
            }
            else
            {
                // there are some policies, run them until they are all done
                DefaultEvaluationContext evaluationContext = new DefaultEvaluationContext();
                object[] policyState = new object[authorizationPolicies.Count];
                object done = new object();

                int oldContextCount;
                do
                {
                    oldContextCount = evaluationContext.Generation;

                    for (int i = 0; i < authorizationPolicies.Count; i++)
                    {
                        if (policyState[i] == done)
                            continue;

                        IAuthorizationPolicy policy = authorizationPolicies[i];
                        if (policy == null)
                        {
                            policyState[i] = done;
                            continue;
                        }

                        if (policy.Evaluate(evaluationContext, ref policyState[i]))
                        {
                            policyState[i] = done;

                            if (DiagnosticUtility.ShouldTraceVerbose)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.AuthorizationPolicyEvaluated,
                                    SR.GetString(SR.AuthorizationPolicyEvaluated, policy.Id));
                            }
                        }
                    }

                } while (oldContextCount < evaluationContext.Generation);

                authorizationContext = new DefaultAuthorizationContext(evaluationContext);
            }

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.AuthorizationContextCreated,
                    SR.GetString(SR.AuthorizationContextCreated, authorizationContext.Id));
            }

            return authorizationContext;
        }

        internal static string ClaimSetToString(ClaimSet claimSet)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ClaimSet [");
            for (int i = 0; i < claimSet.Count; i++)
            {
                Claim claim = claimSet[i];
                if (claim != null)
                {
                    sb.Append("  ");
                    sb.AppendLine(claim.ToString());
                }
            }
            string prefix = "] by ";
            ClaimSet issuer = claimSet;
            do
            {
                // PreSharp Bug: A null-dereference can occur here.
#pragma warning suppress 56506 // issuer was just set to this.
                issuer = issuer.Issuer;
                sb.AppendFormat("{0}{1}", prefix, issuer == claimSet ? "Self" : (issuer.Count <= 0 ? "Unknown" : issuer[0].ToString()));
                prefix = " -> ";
            } while (issuer.Issuer != issuer);
            return sb.ToString();
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new LimitExceededException(SR.GetString(SR.BufferQuotaExceededReadingBase64, maxBufferSize)));
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityMessageSerializationException(SR.GetString(SR.CannotFindMatchingCrypto, encryptionMethod)));
            }
            return unwrappingSecurityKey.DecryptKey(encryptionMethod, wrappedKey);
        }

        public static bool TryCreateX509CertificateFromRawData(byte[] rawData, out X509Certificate2 certificate)
        {
            certificate = (rawData == null || rawData.Length == 0) ? null : new X509Certificate2(rawData);
            return certificate != null && certificate.Handle != IntPtr.Zero;
        }

        internal static byte[] DecodeHexString(string hexString)
        {
            hexString = hexString.Trim();

            bool spaceSkippingMode = false;

            int i = 0;
            int length = hexString.Length;

            if ((length >= 2) &&
                (hexString[0] == '0') &&
                ((hexString[1] == 'x') || (hexString[1] == 'X')))
            {
                length = hexString.Length - 2;
                i = 2;
            }

            if (length < 2)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.InvalidHexString)));

            byte[] sArray;

            if (length >= 3 && hexString[i + 2] == ' ')
            {
                if (length % 3 != 2)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.InvalidHexString)));

                spaceSkippingMode = true;

                // Each hex digit will take three spaces, except the first (hence the plus 1).
                sArray = DiagnosticUtility.Utility.AllocateByteArray(length / 3 + 1);
            }
            else
            {
                if (length % 2 != 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.InvalidHexString)));

                spaceSkippingMode = false;

                // Each hex digit will take two spaces
                sArray = DiagnosticUtility.Utility.AllocateByteArray(length / 2);
            }

            int digit;
            int rawdigit;
            for (int j = 0; i < hexString.Length; i += 2, j++)
            {
                rawdigit = ConvertHexDigit(hexString[i]);
                digit = ConvertHexDigit(hexString[i + 1]);
                sArray[j] = (byte)(digit | (rawdigit << 4));
                if (spaceSkippingMode)
                    i++;
            }
            return (sArray);
        }

        static int ConvertHexDigit(Char val)
        {
            if (val <= '9' && val >= '0')
                return (val - '0');
            else if (val >= 'a' && val <= 'f')
                return ((val - 'a') + 10);
            else if (val >= 'A' && val <= 'F')
                return ((val - 'A') + 10);
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.InvalidHexString)));
        }

        internal static ReadOnlyCollection<IAuthorizationPolicy> CreateAuthorizationPolicies(ClaimSet claimSet)
        {
            return CreateAuthorizationPolicies(claimSet, SecurityUtils.MaxUtcDateTime);
        }

        internal static ReadOnlyCollection<IAuthorizationPolicy> CreateAuthorizationPolicies(ClaimSet claimSet, DateTime expirationTime)
        {
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
            policies.Add(new UnconditionalPolicy(claimSet, expirationTime));
            return policies.AsReadOnly();
        }

        internal static string GenerateId()
        {
            return SecurityUniqueId.Create().Value;
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

        internal static IIdentity CloneIdentityIfNecessary(IIdentity identity)
        {
            if (identity != null)
            {
                WindowsIdentity wid = identity as WindowsIdentity;
                if (wid != null)
                {
                    return CloneWindowsIdentityIfNecessary(wid);
                }
                //X509Identity x509 = identity as X509Identity;
                //if (x509 != null)
                //{
                //    return x509.Clone();
                //}
            }
            return identity;
        }

        /// <SecurityNote>
        /// Critical - calls two critical methods: UnsafeGetWindowsIdentityToken and UnsafeCreateWindowsIdentityFromToken
        /// Safe - "clone" operation is considered safe despite using WindowsIdentity IntPtr token
        ///        must not let IntPtr token leak in or out
        /// </SecurityNote>
        [SecuritySafeCritical]
        internal static WindowsIdentity CloneWindowsIdentityIfNecessary(WindowsIdentity wid)
        {
            return CloneWindowsIdentityIfNecessary(wid, wid.AuthenticationType);
        }

        [SecuritySafeCritical]
        internal static WindowsIdentity CloneWindowsIdentityIfNecessary(WindowsIdentity wid, string authenticationType)
        {

            if (wid != null)
            {
                IntPtr token = UnsafeGetWindowsIdentityToken(wid);
                if (token != IntPtr.Zero)
                {
                    return UnsafeCreateWindowsIdentityFromToken(token, authenticationType);
                }
            }
            return wid;
        }

        /// <SecurityNote>
        /// Critical - elevates in order to return the WindowsIdentity.Token property
        ///            caller must protect return value
        /// </SecurityNote>
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        static IntPtr UnsafeGetWindowsIdentityToken(WindowsIdentity wid)
        {
            return wid.Token;
        }

        /// <SecurityNote>
        /// Critical - elevates in order to construct a WindowsIdentity instance from an IntPtr
        ///            caller must protect parameter return value
        /// </SecurityNote>
        // We pass the authenticationType in as WindowsIdentity will all into a priviledged call in LSA which could fail
        // resulting in a null authenticationType.
        [SecurityCritical]
        [SecurityPermission(SecurityAction.Assert, ControlPrincipal = true, UnmanagedCode = true)]
        static WindowsIdentity UnsafeCreateWindowsIdentityFromToken(IntPtr token, string authenticationType)
        {
            if (authenticationType != null)
            {
                return new WindowsIdentity(token, authenticationType);
            }
            else
            {
                return new WindowsIdentity(token);
            }
        }

        internal static ClaimSet CloneClaimSetIfNecessary(ClaimSet claimSet)
        {
            if (claimSet != null)
            {
                WindowsClaimSet wic = claimSet as WindowsClaimSet;
                if (wic != null)
                {
                    return wic.Clone();
                }
                //X509CertificateClaimSet x509 = claimSet as X509CertificateClaimSet;
                //if (x509 != null)
                //{
                //    return x509.Clone();
                //}
            }
            return claimSet;
        }

        internal static ReadOnlyCollection<ClaimSet> CloneClaimSetsIfNecessary(ReadOnlyCollection<ClaimSet> claimSets)
        {
            if (claimSets != null)
            {
                bool clone = false;
                for (int i = 0; i < claimSets.Count; ++i)
                {
                    if (claimSets[i] is WindowsClaimSet)// || claimSets[i] is X509CertificateClaimSet)
                    {
                        clone = true;
                        break;
                    }
                }
                if (clone)
                {
                    List<ClaimSet> ret = new List<ClaimSet>(claimSets.Count);
                    for (int i = 0; i < claimSets.Count; ++i)
                    {
                        ret.Add(SecurityUtils.CloneClaimSetIfNecessary(claimSets[i]));
                    }
                    return ret.AsReadOnly();
                }
            }
            return claimSets;
        }

        internal static void DisposeClaimSetIfNecessary(ClaimSet claimSet)
        {
            if (claimSet != null)
            {
                SecurityUtils.DisposeIfNecessary(claimSet as WindowsClaimSet);
            }
        }

        internal static void DisposeClaimSetsIfNecessary(ReadOnlyCollection<ClaimSet> claimSets)
        {
            if (claimSets != null)
            {
                for (int i = 0; i < claimSets.Count; ++i)
                {
                    SecurityUtils.DisposeIfNecessary(claimSets[i] as WindowsClaimSet);
                }
            }
        }

        internal static ReadOnlyCollection<IAuthorizationPolicy> CloneAuthorizationPoliciesIfNecessary(ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (authorizationPolicies != null && authorizationPolicies.Count > 0)
            {
                bool clone = false;
                for (int i = 0; i < authorizationPolicies.Count; ++i)
                {
                    UnconditionalPolicy policy = authorizationPolicies[i] as UnconditionalPolicy;
                    if (policy != null && policy.IsDisposable)
                    {
                        clone = true;
                        break;
                    }
                }
                if (clone)
                {
                    List<IAuthorizationPolicy> ret = new List<IAuthorizationPolicy>(authorizationPolicies.Count);
                    for (int i = 0; i < authorizationPolicies.Count; ++i)
                    {
                        UnconditionalPolicy policy = authorizationPolicies[i] as UnconditionalPolicy;
                        if (policy != null)
                        {
                            ret.Add(policy.Clone());
                        }
                        else
                        {
                            ret.Add(authorizationPolicies[i]);
                        }
                    }
                    return ret.AsReadOnly();
                }
            }
            return authorizationPolicies;
        }

        public static void DisposeAuthorizationPoliciesIfNecessary(ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (authorizationPolicies != null && authorizationPolicies.Count > 0)
            {
                for (int i = 0; i < authorizationPolicies.Count; ++i)
                {
                    DisposeIfNecessary(authorizationPolicies[i] as UnconditionalPolicy);
                }
            }
        }

        public static void DisposeIfNecessary(IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
            }
        }
    }

    /// <summary>
    /// Internal helper class to help keep Kerberos and Spnego in [....].
    /// This code is shared by: 
    ///     System\IdentityModel\Tokens\KerberosReceiverSecurityToken.cs
    ///     System\ServiceModel\Security\WindowsSspiNegotiation.cs
    /// Both this code paths require this logic.
    /// </summary>
    internal class ExtendedProtectionPolicyHelper
    {
        //
        // keep the defaults: _protectionScenario and _policyEnforcement, in [....] with: static class System.ServiceModel.Channel.ChannelBindingUtility
        // We can't access those defaults as IdentityModel cannot take a dependency on ServiceModel
        //
        static ExtendedProtectionPolicy disabledPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.Never);

        PolicyEnforcement _policyEnforcement;
        ProtectionScenario _protectionScenario;
        ChannelBinding _channelBinding;
        ServiceNameCollection _serviceNameCollection;
        bool _checkServiceBinding;

        public ExtendedProtectionPolicyHelper(ChannelBinding channelBinding, ExtendedProtectionPolicy extendedProtectionPolicy)
        {
            _protectionScenario = DefaultPolicy.ProtectionScenario;
            _policyEnforcement = DefaultPolicy.PolicyEnforcement;

            _channelBinding = channelBinding;
            _serviceNameCollection = null;
            _checkServiceBinding = true;

            if (extendedProtectionPolicy != null)
            {
                _policyEnforcement = extendedProtectionPolicy.PolicyEnforcement;
                _protectionScenario = extendedProtectionPolicy.ProtectionScenario;
                _serviceNameCollection = extendedProtectionPolicy.CustomServiceNames;
            }

            if (_policyEnforcement == PolicyEnforcement.Never)
            {
                _checkServiceBinding = false;
            }
        }

        public bool ShouldAddChannelBindingToASC()
        {
            return (_channelBinding != null && _policyEnforcement != PolicyEnforcement.Never && _protectionScenario != ProtectionScenario.TrustedProxy);
        }

        public ChannelBinding ChannelBinding
        {
            get { return _channelBinding; }
        }

        public bool ShouldCheckServiceBinding
        {
            get { return _checkServiceBinding; }
        }

        public ServiceNameCollection ServiceNameCollection
        {
            get { return _serviceNameCollection; }
        }

        public ProtectionScenario ProtectionScenario
        {
            get { return _protectionScenario; }
        }

        public PolicyEnforcement PolicyEnforcement
        {
            get { return _policyEnforcement; }
        }

        /// <summary>
        /// ServiceBinding check has the following logic:
        /// 1. Check PolicyEnforcement - never => return true;
        /// 1. Check status returned from SecurityContext which is obtained when querying for the serviceBinding
        /// 2. Check PolicyEnforcement
        ///     a. WhenSupported - valid when OS does not support, null serviceBinding is valid
        ///     b. Always - a non-empty servicebinding must be available
        /// 3. if serviceBinding is non null, check that an expected value is in the ServiceNameCollection - ignoring case
        ///    note that the empty string must be explicitly specified in the serviceNames.
        /// </summary>
        /// <param name="securityContext to ">status Code returned when obtaining serviceBinding from SecurityContext</param>
        /// <returns>If servicebinding is valid</returns>
        public void CheckServiceBinding(SafeDeleteContext securityContext, string defaultServiceBinding)
        {
            if (_policyEnforcement == PolicyEnforcement.Never)
            {
                return;
            }

            string serviceBinding = null;
            int statusCode = SspiWrapper.QuerySpecifiedTarget(securityContext, out serviceBinding);

            if (statusCode != (int)SecurityStatus.OK)
            {
                // only two acceptable non-zero values
                // client OS not patched: stausCode == TargetUnknown
                // service OS not patched: statusCode == Unsupported
                if (statusCode != (int)SecurityStatus.TargetUnknown && statusCode != (int)SecurityStatus.Unsupported)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.InvalidServiceBindingInSspiNegotiationNoServiceBinding)));
                }

                // if policyEnforcement is Always we needed to see a TargetName (SPN)
                if (_policyEnforcement == PolicyEnforcement.Always)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.InvalidServiceBindingInSspiNegotiationNoServiceBinding)));
                }

                // in this case we accept because either the client or service is not patched.
                if (_policyEnforcement == PolicyEnforcement.WhenSupported)
                {
                    return;
                }

                // guard against futures, force failure and fix as necessary
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.InvalidServiceBindingInSspiNegotiationNoServiceBinding)));
            }

            switch (_policyEnforcement)
            {
                case PolicyEnforcement.WhenSupported:
                    // serviceBinding == null => client is not patched
                    if (serviceBinding == null)
                        return;
                    break;

                case PolicyEnforcement.Always:
                    // serviceBinding == null => client is not patched 
                    // serviceBinding == "" => SB was not specified
                    if (string.IsNullOrEmpty(serviceBinding))
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched, string.Empty)));
                    break;
            }

            // iff no values were 'user' set, then check the defaultServiceBinding
            if (_serviceNameCollection == null || _serviceNameCollection.Count < 1)
            {
                if (defaultServiceBinding == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched, string.Empty)));

                if (string.Compare(defaultServiceBinding, serviceBinding, StringComparison.OrdinalIgnoreCase) == 0)
                    return;

                if (string.IsNullOrEmpty(serviceBinding))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched, string.Empty)));
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched, serviceBinding)));
            }

            if (_serviceNameCollection != null)
            {
                if (_serviceNameCollection.Contains(serviceBinding))
                {
                    return;
                }
            }

            if (string.IsNullOrEmpty(serviceBinding))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched, string.Empty)));
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.InvalidServiceBindingInSspiNegotiationServiceBindingNotMatched, serviceBinding)));
        }

        /// <summary>
        /// Keep this in [....] with \System\ServiceModel\Channels\ChannelBindingUtility.cs
        /// </summary>
        public static ExtendedProtectionPolicy DefaultPolicy
        {   //
            //keep the default in [....] with : static class System.ServiceModel.Channels.ChannelBindingUtility
            //we can't use these defaults as IdentityModel cannot take a dependency on ServiceModel
            //

            // Current POR is "Never" respect the above note.

            get { return disabledPolicy; }
        }
    }

    static class EmptyReadOnlyCollection<T>
    {
        public static ReadOnlyCollection<T> Instance = new ReadOnlyCollection<T>(new List<T>());
    }
}
