// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics.Contracts;
using Microsoft.Win32.SafeHandles;

namespace System.Security.Cryptography {
    /// <summary>
    ///     Flags providing information about a raw key handle being opened
    /// </summary>
    [Flags]
    public enum CngKeyHandleOpenOptions {
        None = 0x00000000,

        /// <summary>
        ///     The key handle being opened represents an ephemeral key
        /// </summary>
        EphemeralKey = 0x00000001
    }

    /// <summary>
    ///     Managed representation of an NCrypt key
    /// </summary>
    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class CngKey : IDisposable {
        private SafeNCryptKeyHandle m_keyHandle;
        private SafeNCryptProviderHandle m_kspHandle;

        [System.Security.SecurityCritical]
        private CngKey(SafeNCryptProviderHandle kspHandle, SafeNCryptKeyHandle keyHandle) {
            Contract.Requires(keyHandle != null && !keyHandle.IsInvalid && !keyHandle.IsClosed);
            Contract.Requires(kspHandle != null && !kspHandle.IsInvalid && !kspHandle.IsClosed);
            Contract.Ensures(m_keyHandle != null && !m_keyHandle.IsInvalid && !m_keyHandle.IsClosed);
            Contract.Ensures(kspHandle != null && !kspHandle.IsInvalid && !kspHandle.IsClosed);

            m_keyHandle = keyHandle;
            m_kspHandle = kspHandle;
        }

        //
        // Key properties
        //

        /// <summary>
        ///     Algorithm group this key can be used with
        /// </summary>
        public CngAlgorithmGroup AlgorithmGroup {
            [SecuritySafeCritical]
            [Pure]
            get {
                Contract.Assert(m_keyHandle != null);
                string group = NCryptNative.GetPropertyAsString(m_keyHandle,
                                                                NCryptNative.KeyPropertyName.AlgorithmGroup,
                                                                CngPropertyOptions.None);

                if (group == null) {
                    return null;
                }
                else {
                    return new CngAlgorithmGroup(group);
                }
            }
        }

        /// <summary>
        ///     Name of the algorithm this key can be used with
        /// </summary>
        public CngAlgorithm Algorithm {
            [SecuritySafeCritical]
            get {
                Contract.Assert(m_keyHandle != null);
                string algorithm = NCryptNative.GetPropertyAsString(m_keyHandle,
                                                                    NCryptNative.KeyPropertyName.Algorithm,
                                                                    CngPropertyOptions.None);
                return new CngAlgorithm(algorithm);
            }
        }

        /// <summary>
        ///     Export restrictions on the key
        /// </summary>
        public CngExportPolicies ExportPolicy {
            [SecuritySafeCritical]
            get {
                Contract.Assert(m_keyHandle != null);
                int policy = NCryptNative.GetPropertyAsDWord(m_keyHandle,
                                                             NCryptNative.KeyPropertyName.ExportPolicy,
                                                             CngPropertyOptions.None);

                return (CngExportPolicies)policy;
            }
        }

        /// <summary>
        ///     Native handle for the key
        /// </summary>
        public SafeNCryptKeyHandle Handle {
            [System.Security.SecurityCritical]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            get {
                Contract.Assert(m_keyHandle != null);
                return m_keyHandle.Duplicate();
            }
        }

        /// <summary>
        ///     Is this key ephemeral or persisted
        /// </summary>
        /// <remarks>
        ///     Any ephemeral key created by the CLR will have a property 'CLR IsEphemeral' which consists
        ///     of a single byte containing the value 1. We cannot detect ephemeral keys created by other
        ///     APIs and imported via handle.
        /// </remarks>
        public bool IsEphemeral {
            [SecuritySafeCritical]
            [Pure]
            get {
                Contract.Assert(m_keyHandle != null);

                bool foundProperty;
                byte[] ephemeralProperty = NCryptNative.GetProperty(m_keyHandle,
                                                                    NCryptNative.KeyPropertyName.ClrIsEphemeral,
                                                                    CngPropertyOptions.CustomProperty,
                                                                    out foundProperty);

                return foundProperty &&
                       ephemeralProperty != null &&
                       ephemeralProperty.Length == 1 &&
                       ephemeralProperty[0] == 1;
            }

            [System.Security.SecurityCritical]
            private set {
                Contract.Assert(m_keyHandle != null);

                NCryptNative.SetProperty(m_keyHandle,
                                         NCryptNative.KeyPropertyName.ClrIsEphemeral,
                                         new byte[] { value ? (byte)1 : (byte)0 },
                                         CngPropertyOptions.CustomProperty);
            }
        }

        /// <summary>
        ///     Is this a machine key or a user key
        /// </summary>
        public bool IsMachineKey {
            [SecuritySafeCritical]
            get {
                Contract.Assert(m_keyHandle != null);
                int type = NCryptNative.GetPropertyAsDWord(m_keyHandle,
                                                           NCryptNative.KeyPropertyName.KeyType,
                                                           CngPropertyOptions.None);

                return ((CngKeyTypes)type & CngKeyTypes.MachineKey) == CngKeyTypes.MachineKey;
            }
        }

        /// <summary>
        ///     The name of the key, null if it is ephemeral. We can only detect ephemeral keys created by
        ///     the CLR. Other ephemeral keys, such as those imported by handle, will get a CryptographicException
        ///     if they read this property.
        /// </summary>
        public string KeyName {
            [SecuritySafeCritical]
            get {
                Contract.Assert(m_keyHandle != null);

                if (IsEphemeral) {
                    return null;
                }
                else {
                    return NCryptNative.GetPropertyAsString(m_keyHandle,
                                                            NCryptNative.KeyPropertyName.Name,
                                                            CngPropertyOptions.None);
                }
            }
        }

        /// <summary>
        ///     Size, in bits, of the key
        /// </summary>
        public int KeySize {
            [SecuritySafeCritical]
            get {
                Contract.Assert(m_keyHandle != null);
                return NCryptNative.GetPropertyAsDWord(m_keyHandle,
                                                       NCryptNative.KeyPropertyName.Length,
                                                       CngPropertyOptions.None);
            }
        }

        /// <summary>
        ///     Usage restrictions on the key
        /// </summary>
        public CngKeyUsages KeyUsage {
            [SecuritySafeCritical]
            get {
                Contract.Assert(m_keyHandle != null);
                int keyUsage = NCryptNative.GetPropertyAsDWord(m_keyHandle,
                                                               NCryptNative.KeyPropertyName.KeyUsage,
                                                               CngPropertyOptions.None);
                return (CngKeyUsages)keyUsage;
            }
        }

        /// <summary>
        ///     HWND of the window to use as a parent for any UI
        /// </summary>
        public IntPtr ParentWindowHandle {
            [SecuritySafeCritical]
            get {
                Contract.Assert(m_keyHandle != null);
                return NCryptNative.GetPropertyAsIntPtr(m_keyHandle,
                                                        NCryptNative.KeyPropertyName.ParentWindowHandle,
                                                        CngPropertyOptions.None);
            }

            [SecuritySafeCritical]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            set {
                Contract.Assert(m_keyHandle != null);
                NCryptNative.SetProperty(m_keyHandle,
                                         NCryptNative.KeyPropertyName.ParentWindowHandle,
                                         value,
                                         CngPropertyOptions.None);
            }
        }

        /// <summary>
        ///     KSP which holds this key
        /// </summary>
        public CngProvider Provider {
            [SecuritySafeCritical]
            get {
                Contract.Assert(m_kspHandle != null);
                string provider = NCryptNative.GetPropertyAsString(m_kspHandle,
                                                                   NCryptNative.ProviderPropertyName.Name,
                                                                   CngPropertyOptions.None);

                if (provider == null) {
                    return null;
                }
                else {
                    return new CngProvider(provider);
                }
            }
        }

        /// <summary>
        ///     Native handle to the KSP associated with this key
        /// </summary>
        public SafeNCryptProviderHandle ProviderHandle {
            [System.Security.SecurityCritical]
            [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
            get {
                Contract.Assert(m_kspHandle != null);
                return m_kspHandle.Duplicate();
            }
        }

        /// <summary>
        ///     Unique name of the key, null if it is ephemeral. See the comments on the Name property for
        ///     details about names of ephemeral keys.
        /// </summary>
        public string UniqueName {
            [SecuritySafeCritical]
            get {
                Contract.Assert(m_keyHandle != null);

                if (IsEphemeral) {
                    return null;
                }
                else {
                    return NCryptNative.GetPropertyAsString(m_keyHandle,
                                                            NCryptNative.KeyPropertyName.UniqueName,
                                                            CngPropertyOptions.None);
                }
            }
        }

        /// <summary>
        ///     UI strings associated with a key
        /// </summary>
        public CngUIPolicy UIPolicy {
            [SecuritySafeCritical]
            get {
                Contract.Ensures(Contract.Result<CngUIPolicy>() != null);
                Contract.Assert(m_keyHandle != null);

                NCryptNative.NCRYPT_UI_POLICY uiPolicy =
                    NCryptNative.GetPropertyAsStruct<NCryptNative.NCRYPT_UI_POLICY>(m_keyHandle,
                                                                                    NCryptNative.KeyPropertyName.UIPolicy,
                                                                                    CngPropertyOptions.None);

                string useContext = NCryptNative.GetPropertyAsString(m_keyHandle,
                                                                     NCryptNative.KeyPropertyName.UseContext,
                                                                     CngPropertyOptions.None);

                return new CngUIPolicy(uiPolicy.dwFlags,
                                       uiPolicy.pszFriendlyName,
                                       uiPolicy.pszDescription,
                                       useContext,
                                       uiPolicy.pszCreationTitle);
            }
        }

        /// <summary>
        ///     Build a key container permission for the specified access to this key
        /// 
        ///     If the key is a known ephemeral key, return null, since we don't require permission to work with
        ///     those keys.  Otherwise return a permission scoped to the specific key and ksp if we can get those
        ///     values, defaulting back to a full KeyContainerPermission if we cannot.
        /// </summary>
        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Security", "CA2103:ReviewImperativeSecurity", Justification = "The demand is based on a non-mutable value type")]
        internal KeyContainerPermission BuildKeyContainerPermission(KeyContainerPermissionFlags flags) {
            Contract.Ensures(Contract.Result<KeyContainerPermission>() != null || IsEphemeral);
            Contract.Assert(m_keyHandle != null);
            Contract.Assert(m_kspHandle != null);

            KeyContainerPermission permission = null;

            if (!IsEphemeral) {

                // Try to get the name of the key and ksp to demand for this specific instance
                string keyName = null;
                string kspName = null;
                try {
                    keyName = KeyName;
                    kspName = NCryptNative.GetPropertyAsString(m_kspHandle,
                                                               NCryptNative.ProviderPropertyName.Name,
                                                               CngPropertyOptions.None);
                }
                catch (CryptographicException) { /* This may have been an imported ephemeral key */ }

                if (keyName != null) {
                    KeyContainerPermissionAccessEntry access = new KeyContainerPermissionAccessEntry(keyName, flags);
                    access.ProviderName = kspName;
                    
                    permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                    permission.AccessEntries.Add(access);
                }
                else {
                    permission = new KeyContainerPermission(flags);
                }
            }

            return permission;
        }

        //
        // Creation factory methods
        //

        public static CngKey Create(CngAlgorithm algorithm) {
            Contract.Ensures(Contract.Result<CngKey>() != null);
            return Create(algorithm, null);
        }

        public static CngKey Create(CngAlgorithm algorithm, string keyName) {
            Contract.Ensures(Contract.Result<CngKey>() != null);
            return Create(algorithm, keyName, null);
        }

        [SecuritySafeCritical]
        public static CngKey Create(CngAlgorithm algorithm, string keyName, CngKeyCreationParameters creationParameters) {
            Contract.Ensures(Contract.Result<CngKey>() != null);

            if (algorithm == null) {
                throw new ArgumentNullException("algorithm");
            }

            if (creationParameters == null) {
                creationParameters = new CngKeyCreationParameters();
            }

            // Make sure that NCrypt is supported on this platform
            if (!NCryptNative.NCryptSupported) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
            }

            // If we're not creating an ephemeral key, then we need to ensure the user has access to the key name
            if (keyName != null) {
                KeyContainerPermissionAccessEntry access = new KeyContainerPermissionAccessEntry(keyName, KeyContainerPermissionFlags.Create);
                access.ProviderName = creationParameters.Provider.Provider;

                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                permission.AccessEntries.Add(access);
                permission.Demand();
            }

            //
            // Create the native handles representing the new key, setup the creation parameters on it, and
            // finalize it for use.
            //

            SafeNCryptProviderHandle kspHandle = NCryptNative.OpenStorageProvider(creationParameters.Provider.Provider);
            SafeNCryptKeyHandle keyHandle = NCryptNative.CreatePersistedKey(kspHandle,
                                                                            algorithm.Algorithm,
                                                                            keyName,
                                                                            creationParameters.KeyCreationOptions);

            SetKeyProperties(keyHandle, creationParameters);
            NCryptNative.FinalizeKey(keyHandle);
            
            CngKey key = new CngKey(kspHandle, keyHandle);

            // No name translates to an ephemeral key
            if (keyName == null) {
                key.IsEphemeral = true;
            }

            return key;
        }

        /// <summary>
        ///     Delete this key
        /// </summary>
        [SecuritySafeCritical]
        public void Delete() {
            Contract.Assert(m_keyHandle != null);

            // Make sure we have permission to delete this key
            KeyContainerPermission permission = BuildKeyContainerPermission(KeyContainerPermissionFlags.Delete);
            if (permission != null) {
                permission.Demand();
            }

            NCryptNative.DeleteKey(m_keyHandle);

            // Once the key is deleted, the handles are no longer valid so dispose of this instance
            Dispose();
        }

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        public void Dispose() {
            if (m_kspHandle != null) {
                m_kspHandle.Dispose();
            }

            if (m_keyHandle != null) {
                m_keyHandle.Dispose();
            }
        }

        //
        // Check to see if a key already exists
        //

        public static bool Exists(string keyName) {
            return Exists(keyName, CngProvider.MicrosoftSoftwareKeyStorageProvider);
        }

        public static bool Exists(string keyName, CngProvider provider) {
            return Exists(keyName, provider, CngKeyOpenOptions.None);
        }

        [SecuritySafeCritical]
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Reviewed")]
        public static bool Exists(string keyName, CngProvider provider, CngKeyOpenOptions options) {
            if (keyName == null) {
                throw new ArgumentNullException("keyName");
            }
            if (provider == null) {
                throw new ArgumentNullException("provider");
            }

            // Make sure that NCrypt is supported on this platform
            if (!NCryptNative.NCryptSupported) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
            }

            using (SafeNCryptProviderHandle kspHandle = NCryptNative.OpenStorageProvider(provider.Provider)) {

                SafeNCryptKeyHandle keyHandle = null;

                try {
                    NCryptNative.ErrorCode error = NCryptNative.UnsafeNativeMethods.NCryptOpenKey(kspHandle,
                                                                                                  out keyHandle,
                                                                                                  keyName,
                                                                                                  0,
                                                                                                  options);

                    // CNG will return either NTE_NOT_FOUND or NTE_BAD_KEYSET for the case where the key does
                    // not exist, so we need to check for both return codes.
                    bool keyNotFound = error == NCryptNative.ErrorCode.KeyDoesNotExist ||
                                       error == NCryptNative.ErrorCode.NotFound;

                    if (error != NCryptNative.ErrorCode.Success && !keyNotFound) {
                        throw new CryptographicException((int)error);
                    }

                    return error == NCryptNative.ErrorCode.Success;
                }
                finally {
                    if (keyHandle != null) {
                        keyHandle.Dispose();
                    }
                }
            }
        }

        //
        // Import factory methods
        //

        public static CngKey Import(byte[] keyBlob, CngKeyBlobFormat format) {
            Contract.Ensures(Contract.Result<CngKey>() != null);
            return Import(keyBlob, format, CngProvider.MicrosoftSoftwareKeyStorageProvider);
        }

        [SecuritySafeCritical]
        public static CngKey Import(byte[] keyBlob, CngKeyBlobFormat format, CngProvider provider) {
            Contract.Ensures(Contract.Result<CngKey>() != null);

            if (keyBlob == null) {
                throw new ArgumentNullException("keyBlob"); 
            }
            if (format == null) {
                throw new ArgumentNullException("format");
            }
            if (provider == null) {
                throw new ArgumentNullException("provider");
            }

            // Make sure that NCrypt is supported on this platform
            if (!NCryptNative.NCryptSupported) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
            }

            // If we don't know for sure that the key will be ephemeral, then we need to demand Import
            // permission.  Since we won't know the name of the key until it's too late, we demand a full Import
            // rather than one scoped to the key.
            bool safeKeyImport = format == CngKeyBlobFormat.EccPublicBlob ||
                                 format == CngKeyBlobFormat.GenericPublicBlob;

            if (!safeKeyImport) {
                new KeyContainerPermission(KeyContainerPermissionFlags.Import).Demand();
            }

            // Import the key into the KSP
            SafeNCryptProviderHandle kspHandle = NCryptNative.OpenStorageProvider(provider.Provider);
            SafeNCryptKeyHandle keyHandle = NCryptNative.ImportKey(kspHandle, keyBlob, format.Format);

            // Prepare the key for use
            CngKey key = new CngKey(kspHandle, keyHandle);
            
            // We can't tell directly if an OpaqueTransport blob imported as an ephemeral key or not
            key.IsEphemeral = format != CngKeyBlobFormat.OpaqueTransportBlob;

            return key;
        }

        /// <summary>
        ///     Export the key out of the KSP
        /// </summary>
        [SecuritySafeCritical]
        public byte[] Export(CngKeyBlobFormat format) {
            Contract.Assert(m_keyHandle != null);

            if (format == null) {
                throw new ArgumentNullException("format");
            }

            KeyContainerPermission permission = BuildKeyContainerPermission(KeyContainerPermissionFlags.Export);
            if (permission != null) {
                permission.Demand();
            }

            return NCryptNative.ExportKey(m_keyHandle, format.Format);
        }

        /// <summary>
        ///     Get the value of an arbitrary property
        /// </summary>
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public CngProperty GetProperty(string name, CngPropertyOptions options) {
            Contract.Assert(m_keyHandle != null);

            if (name == null) {
                throw new ArgumentNullException("name");
            }

            bool foundProperty;
            byte[] value = NCryptNative.GetProperty(m_keyHandle, name, options, out foundProperty);

            if (!foundProperty) {
                throw new CryptographicException((int)NCryptNative.ErrorCode.NotFound);
            }

            return new CngProperty(name, value, options);
        }

        /// <summary>
        ///     Determine if a property exists on the key
        /// </summary>
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public bool HasProperty(string name, CngPropertyOptions options) {
            Contract.Assert(m_keyHandle != null);

            if (name == null) {
                throw new ArgumentNullException("name");
            }

            bool foundProperty;
            NCryptNative.GetProperty(m_keyHandle, name, options, out foundProperty);

            return foundProperty;
        }

        //
        // Open factory methods
        //

        public static CngKey Open(string keyName) {
            Contract.Ensures(Contract.Result<CngKey>() != null);
            return Open(keyName, CngProvider.MicrosoftSoftwareKeyStorageProvider);
        }

        public static CngKey Open(string keyName, CngProvider provider) {
            Contract.Ensures(Contract.Result<CngKey>() != null);
            return Open(keyName, provider, CngKeyOpenOptions.None);
        }

        [SecuritySafeCritical]
        public static CngKey Open(string keyName, CngProvider provider, CngKeyOpenOptions openOptions) {
            Contract.Ensures(Contract.Result<CngKey>() != null);

            if (keyName == null) {
                throw new ArgumentNullException("keyName");
            }
            if (provider == null) {
                throw new ArgumentNullException("provider");
            }

            // Make sure that NCrypt is supported on this platform
            if (!NCryptNative.NCryptSupported) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Cryptography_PlatformNotSupported));
            }

            // Ensure the user has access to the key name
            KeyContainerPermissionAccessEntry access = new KeyContainerPermissionAccessEntry(keyName, KeyContainerPermissionFlags.Open);
            access.ProviderName = provider.Provider;

            KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
            permission.AccessEntries.Add(access);
            permission.Demand();

            // Open the key
            SafeNCryptProviderHandle kspHandle = NCryptNative.OpenStorageProvider(provider.Provider);
            SafeNCryptKeyHandle keyHandle = NCryptNative.OpenKey(kspHandle, keyName, openOptions);

            return new CngKey(kspHandle, keyHandle);
        }

        /// <summary>
        ///     Wrap an existing key handle with a CngKey object
        /// </summary>
        [System.Security.SecurityCritical]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public static CngKey Open(SafeNCryptKeyHandle keyHandle, CngKeyHandleOpenOptions keyHandleOpenOptions) {
            if (keyHandle == null) {
                throw new ArgumentNullException("keyHandle");
            }
            if (keyHandle.IsClosed || keyHandle.IsInvalid) {
                throw new ArgumentException(SR.GetString(SR.Cryptography_OpenInvalidHandle), "keyHandle");
            }

            SafeNCryptKeyHandle keyHandleCopy = keyHandle.Duplicate();

            // Get a handle to the key's KSP
            SafeNCryptProviderHandle kspHandle = new SafeNCryptProviderHandle();
            RuntimeHelpers.PrepareConstrainedRegions();
            try { }
            finally {
                IntPtr rawHandle = NCryptNative.GetPropertyAsIntPtr(keyHandle,
                                                                    NCryptNative.KeyPropertyName.ProviderHandle,
                                                                    CngPropertyOptions.None);
                kspHandle.SetHandleValue (rawHandle);
            }

            // Setup a key object wrapping the handle
            CngKey key = null;
            bool keyFullySetup = false;
            try {
                key = new CngKey(kspHandle, keyHandleCopy);

                bool openingEphemeralKey = (keyHandleOpenOptions & CngKeyHandleOpenOptions.EphemeralKey) == CngKeyHandleOpenOptions.EphemeralKey;

                //
                // If we're wrapping a handle to an ephemeral key, we need to make sure that IsEphemeral is
                // setup to return true.  In the case that the handle is for an ephemeral key that was created
                // by the CLR, then we don't have anything to do as the IsEphemeral CLR property will already
                // be setup.  However, if the key was created outside of the CLR we will need to setup our
                // ephemeral detection property.
                // 
                // This enables consumers of CngKey objects to always be able to rely on the result of
                // calling IsEphemeral, and also allows them to safely access the Name property.
                // 
                // Finally, if we detect that this is an ephemeral key that the CLR created but we were not
                // told that it was an ephemeral key we'll throw an exception.  This prevents us from having
                // to decide who to believe -- the key property or the caller of the API.  Since other code
                // relies on the ephemeral flag being set properly to avoid tripping over bugs in CNG, we
                // need to reject the case that we suspect that the flag is incorrect.
                // 

                if (!key.IsEphemeral && openingEphemeralKey) {
                    key.IsEphemeral = true;
                }
                else if (key.IsEphemeral && !openingEphemeralKey) {
                    throw new ArgumentException(SR.GetString(SR.Cryptography_OpenEphemeralKeyHandleWithoutEphemeralFlag), "keyHandleOpenOptions");
                }

                keyFullySetup = true;
            }
            finally {
                // Make sure that we don't leak the handle the CngKey duplicated
                if (!keyFullySetup && key != null) {
                    key.Dispose();
                }
            }

            return key;
        }

        /// <summary>
        ///     Setup the key properties specified in the key creation parameters
        /// </summary>
        /// <param name="keyHandle"></param>
        /// <param name="creationParameters"></param>
        [System.Security.SecurityCritical]
        private static void SetKeyProperties(SafeNCryptKeyHandle keyHandle,
                                             CngKeyCreationParameters creationParameters) {
            Contract.Requires(keyHandle != null && !keyHandle.IsInvalid && !keyHandle.IsClosed);
            Contract.Requires(creationParameters != null);

            //
            // Setup the well-known properties.
            //

            if (creationParameters.ExportPolicy.HasValue) {
                NCryptNative.SetProperty(keyHandle,
                                         NCryptNative.KeyPropertyName.ExportPolicy,
                                         (int)creationParameters.ExportPolicy.Value,
                                         CngPropertyOptions.Persist);
            }

            if (creationParameters.KeyUsage.HasValue) {
                NCryptNative.SetProperty(keyHandle,
                                         NCryptNative.KeyPropertyName.KeyUsage,
                                         (int)creationParameters.KeyUsage.Value,
                                         CngPropertyOptions.Persist);
            }

            if (creationParameters.ParentWindowHandle != IntPtr.Zero) {
                NCryptNative.SetProperty(keyHandle,
                                         NCryptNative.KeyPropertyName.ParentWindowHandle,
                                         creationParameters.ParentWindowHandle,
                                         CngPropertyOptions.None);
            }

            if (creationParameters.UIPolicy != null) {
                NCryptNative.NCRYPT_UI_POLICY uiPolicy = new NCryptNative.NCRYPT_UI_POLICY();
                uiPolicy.dwVersion = 1;
                uiPolicy.dwFlags = creationParameters.UIPolicy.ProtectionLevel;
                uiPolicy.pszCreationTitle = creationParameters.UIPolicy.CreationTitle;
                uiPolicy.pszFriendlyName = creationParameters.UIPolicy.FriendlyName;
                uiPolicy.pszDescription = creationParameters.UIPolicy.Description;

                NCryptNative.SetProperty(keyHandle,
                                         NCryptNative.KeyPropertyName.UIPolicy,
                                         uiPolicy,
                                         CngPropertyOptions.Persist);

                // The use context is a seperate property from the standard UI context
                if (creationParameters.UIPolicy.UseContext != null) {
                    NCryptNative.SetProperty(keyHandle,
                                             NCryptNative.KeyPropertyName.UseContext,
                                             creationParameters.UIPolicy.UseContext,
                                             CngPropertyOptions.Persist);
                }
            }

            // Iterate over the custom properties, setting those as well.
            foreach (CngProperty property in creationParameters.ParametersNoDemand) {
                NCryptNative.SetProperty(keyHandle, property.Name, property.Value, property.Options);
            }
        }

        /// <summary>
        ///     Set an arbitrary property on the key
        /// </summary>
        [SecuritySafeCritical]
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void SetProperty(CngProperty property) {
            Contract.Assert(m_keyHandle != null);
            NCryptNative.SetProperty(m_keyHandle, property.Name, property.Value, property.Options);
        }
    }
}
