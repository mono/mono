
// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// X509Utils.cs
//

namespace System.Security.Cryptography.X509Certificates {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Text;

    internal class X509Utils {
        private X509Utils () {}

        internal static bool IsCertRdnCharString (uint dwValueType) {
            return ((dwValueType & CAPI.CERT_RDN_TYPE_MASK) >= CAPI.CERT_RDN_NUMERIC_STRING);
        }

        // this method maps a cert content type returned from CryptQueryObject
        // to a value in the managed X509ContentType enum
        internal static X509ContentType MapContentType (uint contentType) {
            switch (contentType) {
            case CAPI.CERT_QUERY_CONTENT_CERT:
                return X509ContentType.Cert;
            case CAPI.CERT_QUERY_CONTENT_SERIALIZED_STORE:
                return X509ContentType.SerializedStore;
            case CAPI.CERT_QUERY_CONTENT_SERIALIZED_CERT:
                return X509ContentType.SerializedCert;
            case CAPI.CERT_QUERY_CONTENT_PKCS7_SIGNED:
            case CAPI.CERT_QUERY_CONTENT_PKCS7_UNSIGNED:
                return X509ContentType.Pkcs7;
            case CAPI.CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED:
                return X509ContentType.Authenticode;
            case CAPI.CERT_QUERY_CONTENT_PFX:
                return X509ContentType.Pkcs12;
            default:
                return X509ContentType.Unknown;
            }
        }

        // this method maps a X509KeyStorageFlags enum to a combination of crypto API flags
        internal static uint MapKeyStorageFlags (X509KeyStorageFlags keyStorageFlags) {
            uint dwFlags = 0;
            if ((keyStorageFlags & X509KeyStorageFlags.UserKeySet) == X509KeyStorageFlags.UserKeySet)
                dwFlags |= CAPI.CRYPT_USER_KEYSET;
            else if ((keyStorageFlags & X509KeyStorageFlags.MachineKeySet) == X509KeyStorageFlags.MachineKeySet)
                dwFlags |= CAPI.CRYPT_MACHINE_KEYSET;

            if ((keyStorageFlags & X509KeyStorageFlags.Exportable) == X509KeyStorageFlags.Exportable)
                dwFlags |= CAPI.CRYPT_EXPORTABLE;
            if ((keyStorageFlags & X509KeyStorageFlags.UserProtected) == X509KeyStorageFlags.UserProtected)
                dwFlags |= CAPI.CRYPT_USER_PROTECTED;

            return dwFlags;
        }

        // this method maps X509Store OpenFlags to a combination of crypto API flags
        internal static uint MapX509StoreFlags (StoreLocation storeLocation, OpenFlags flags) {
            uint dwFlags = 0;
            uint openMode = ((uint)flags) & 0x3;
            switch (openMode) {
            case (uint) OpenFlags.ReadOnly:
                dwFlags |= CAPI.CERT_STORE_READONLY_FLAG;
                break;
            case (uint) OpenFlags.MaxAllowed:
                dwFlags |= CAPI.CERT_STORE_MAXIMUM_ALLOWED_FLAG;
                break;
            }

            if ((flags & OpenFlags.OpenExistingOnly) == OpenFlags.OpenExistingOnly)
                dwFlags |= CAPI.CERT_STORE_OPEN_EXISTING_FLAG;
            if ((flags & OpenFlags.IncludeArchived) == OpenFlags.IncludeArchived)
                dwFlags |= CAPI.CERT_STORE_ENUM_ARCHIVED_FLAG;

            if (storeLocation == StoreLocation.LocalMachine)
                dwFlags |= CAPI.CERT_SYSTEM_STORE_LOCAL_MACHINE;
            else if (storeLocation == StoreLocation.CurrentUser)
                dwFlags |= CAPI.CERT_SYSTEM_STORE_CURRENT_USER;

            return dwFlags;
        }

        // this method maps an X509NameType to crypto API flags.
        internal static uint MapNameType (X509NameType nameType) {
            uint type = 0;
            switch (nameType) {
            case X509NameType.SimpleName:
                type = CAPI.CERT_NAME_SIMPLE_DISPLAY_TYPE;
                break;
            case X509NameType.EmailName:
                type = CAPI.CERT_NAME_EMAIL_TYPE;
                break;
            case X509NameType.UpnName:
                type = CAPI.CERT_NAME_UPN_TYPE;
                break;
            case X509NameType.DnsName:
            case X509NameType.DnsFromAlternativeName:
                type = CAPI.CERT_NAME_DNS_TYPE;
                break;
            case X509NameType.UrlName:
                type = CAPI.CERT_NAME_URL_TYPE;
                break;
            default:
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidNameType));
            }

            return type;
        }

        // this method maps X509RevocationFlag to crypto API flags.
        internal static uint MapRevocationFlags (X509RevocationMode revocationMode, X509RevocationFlag revocationFlag) {
            uint dwFlags = 0;
            if (revocationMode == X509RevocationMode.NoCheck)
                return dwFlags;

            if (revocationMode == X509RevocationMode.Offline)
                dwFlags |= CAPI.CERT_CHAIN_REVOCATION_CHECK_CACHE_ONLY;

            if (revocationFlag == X509RevocationFlag.EndCertificateOnly)
                dwFlags |= CAPI.CERT_CHAIN_REVOCATION_CHECK_END_CERT;
            else if (revocationFlag == X509RevocationFlag.EntireChain)
                dwFlags |= CAPI.CERT_CHAIN_REVOCATION_CHECK_CHAIN;
            else
                dwFlags |= CAPI.CERT_CHAIN_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT;

            return dwFlags;
        }

        private static readonly char[] hexValues = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
        internal static string EncodeHexString (byte[] sArray) {
            return EncodeHexString(sArray, 0, (uint) sArray.Length);
        }

        internal static string EncodeHexString (byte[] sArray, uint start, uint end) {
            String result = null;
            if (sArray != null) {
                char[] hexOrder = new char[(end - start) * 2];
                uint digit;
                for (uint i = start, j = 0; i < end; i++) {
                    digit = (uint) ((sArray[i] & 0xf0) >> 4);
                    hexOrder[j++] = hexValues[digit];
                    digit = (uint) (sArray[i] & 0x0f);
                    hexOrder[j++] = hexValues[digit];
                }
                result = new String(hexOrder);
            }
            return result;
        }

        internal static string EncodeHexStringFromInt (byte[] sArray, uint start, uint end) {
            String result = null;
            if(sArray != null) {
                char[] hexOrder = new char[(end - start) * 2];
                uint i = end;
                uint digit, j=0;
                while (i-- > start) {
                    digit = (uint) (sArray[i] & 0xf0) >> 4;
                    hexOrder[j++] = hexValues[digit];
                    digit = (uint) (sArray[i] & 0x0f);
                    hexOrder[j++] = hexValues[digit];
                }
                result = new String(hexOrder);
            }
            return result;
        }

        internal static byte HexToByte (char val) {
            if (val <= '9' && val >= '0')
                return (byte) (val - '0');
            else if (val >= 'a' && val <= 'f')
                return (byte) ((val - 'a') + 10);
            else if (val >= 'A' && val <= 'F')
                return (byte) ((val - 'A') + 10);
            else
                return 0xFF;
        }

        internal static uint AlignedLength (uint length) {
            return ((length + (uint) 7) & ((uint) 0xfffffff8));
        }

        internal static String DiscardWhiteSpaces (string inputBuffer) {
            return DiscardWhiteSpaces(inputBuffer, 0, inputBuffer.Length);
        }

        internal static String DiscardWhiteSpaces (string inputBuffer, int inputOffset, int inputCount) {
            int i, iCount = 0;
            for (i=0; i<inputCount; i++)
                if (Char.IsWhiteSpace(inputBuffer[inputOffset + i])) iCount++;
            char[] rgbOut = new char[inputCount - iCount];
            iCount = 0;
            for (i=0; i<inputCount; i++)
                if (!Char.IsWhiteSpace(inputBuffer[inputOffset + i])) {
                    rgbOut[iCount++] = inputBuffer[inputOffset + i];
                }
            return new String(rgbOut);
        }

        internal static byte[] DecodeHexString (string s) {
            string hexString = X509Utils.DiscardWhiteSpaces(s);
            uint cbHex = (uint) hexString.Length / 2;
            byte[] hex = new byte[cbHex];
            int i = 0;
            for (int index = 0; index < cbHex; index++) {
                hex[index] = (byte) ((HexToByte(hexString[i]) << 4) | HexToByte(hexString[i+1]));
                i += 2;
            }
            return hex;
        }

        internal static int GetHexArraySize (byte[] hex) {
            int index = hex.Length;
            while (index-- > 0) {
                if (hex[index] != 0)
                    break;
            }
            return index + 1;
        }

        internal static SafeLocalAllocHandle ByteToPtr (byte[] managed) {
            SafeLocalAllocHandle pb = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(managed.Length));
            Marshal.Copy(managed, 0, pb.DangerousGetHandle(), managed.Length);
            return pb;
        }

        //
        // This method copies an unmanaged structure into the address of a managed structure.
        // This is useful when the structure is returned to us by Crypto API and its size varies 
        // following the platform.
        //

        internal unsafe static void memcpy (IntPtr source, IntPtr dest, uint size) {
            for (uint index = 0; index < size; index++) {
                *(byte*) ((long)dest + index) = Marshal.ReadByte(new IntPtr((long)source + index));
            }
        }

        internal static byte[] PtrToByte (IntPtr unmanaged, uint size) {
            byte[] array = new byte[(int) size];
            Marshal.Copy(unmanaged, array, 0, array.Length);
            return array;
        }

        internal static unsafe bool MemEqual (byte * pbBuf1, uint cbBuf1, byte * pbBuf2, uint cbBuf2) {
            if (cbBuf1 != cbBuf2)
                return false;

            while (cbBuf1-- > 0) {
                if (*pbBuf1++ != *pbBuf2++) {
                    return false;
                }
            }
            return true;
        }

        internal static SafeLocalAllocHandle StringToAnsiPtr (string s) {
            byte[] arr = new byte[s.Length + 1];
            Encoding.ASCII.GetBytes(s, 0, s.Length, arr, 0);
            SafeLocalAllocHandle pb = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(arr.Length));
            Marshal.Copy(arr, 0, pb.DangerousGetHandle(), arr.Length);
            return pb;
        }

        internal static SafeLocalAllocHandle StringToUniPtr (string s) {
            byte[] arr = new byte[2 * (s.Length + 1)];
            Encoding.Unicode.GetBytes(s, 0, s.Length, arr, 0);
            SafeLocalAllocHandle pb = CAPI.LocalAlloc(CAPI.LMEM_FIXED, new IntPtr(arr.Length));
            Marshal.Copy(arr, 0, pb.DangerousGetHandle(), arr.Length);
            return pb;
        }

        // this method create a memory store from a certificate collection
        internal static SafeCertStoreHandle ExportToMemoryStore (X509Certificate2Collection collection) {
            //
            // We need to Assert all StorePermission flags since this is a memory store and we want 
            // semi-trusted code to be able to export certificates to a memory store.
            //

            StorePermission sp = new StorePermission(StorePermissionFlags.AllFlags);
            sp.Assert();

            SafeCertStoreHandle safeCertStoreHandle = SafeCertStoreHandle.InvalidHandle;

            // we always want to use CERT_STORE_ENUM_ARCHIVED_FLAG since we want to preserve the collection in this operation.
            // By default, Archived certificates will not be included.

            safeCertStoreHandle = CAPI.CertOpenStore(new IntPtr(CAPI.CERT_STORE_PROV_MEMORY), 
                                                     CAPI.X509_ASN_ENCODING | CAPI.PKCS_7_ASN_ENCODING,
                                                     IntPtr.Zero,
                                                     CAPI.CERT_STORE_ENUM_ARCHIVED_FLAG | CAPI.CERT_STORE_CREATE_NEW_FLAG, 
                                                     null);

            if (safeCertStoreHandle == null || safeCertStoreHandle.IsInvalid)
                throw new CryptographicException(Marshal.GetLastWin32Error());

            //
            // We use CertAddCertificateLinkToStore to keep a link to the original store, so any property changes get
            // applied to the original store. This has a limit of 99 links per cert context however.
            //

            foreach (X509Certificate2 x509 in collection) {
                if (!CAPI.CertAddCertificateLinkToStore(safeCertStoreHandle,
                                                        x509.CertContext,
                                                        CAPI.CERT_STORE_ADD_ALWAYS,
                                                        SafeCertContextHandle.InvalidHandle))
                    throw new CryptographicException(Marshal.GetLastWin32Error());
            }

            return safeCertStoreHandle;
        }

        internal static uint OidToAlgId (string value) {
            SafeLocalAllocHandle pszOid = StringToAnsiPtr(value);
            CAPI.CRYPT_OID_INFO pOIDInfo = CAPI.CryptFindOIDInfo(CAPI.CRYPT_OID_INFO_OID_KEY, pszOid, 0);
            return pOIDInfo.Algid;
        }

        internal static string FindOidInfo(uint keyType, string keyValue, OidGroup oidGroup) {
            if (keyValue == null)
                throw new ArgumentNullException("keyValue");
            if (keyValue.Length == 0)
                return null;

            SafeLocalAllocHandle pvKey = SafeLocalAllocHandle.InvalidHandle;

            try {
                switch(keyType) {
                case CAPI.CRYPT_OID_INFO_OID_KEY:
                    pvKey = StringToAnsiPtr(keyValue);
                    break;

                case CAPI.CRYPT_OID_INFO_NAME_KEY:
                    pvKey = StringToUniPtr(keyValue);
                    break;

                default:
                    Debug.Assert(false);
                    break;
                }

                CAPI.CRYPT_OID_INFO pOidInfo = CAPI.CryptFindOIDInfo(keyType, pvKey, oidGroup);


                if (keyType == CAPI.CRYPT_OID_INFO_OID_KEY) {
                    return pOidInfo.pwszName;
                }
                else {
                    return pOidInfo.pszOID;
                }
            }
            finally {
                pvKey.Dispose();
            }
        }

        // Try to find OID info within a specific group, and if that doesn't work fall back to all
        // groups for compatibility with previous frameworks
        internal static string FindOidInfoWithFallback(uint key, string value, OidGroup group) {
            string info = FindOidInfo(key, value, group);

            // If we couldn't find it in the requested group, then try again in all groups
            if (info == null && group != OidGroup.All) {
                info = FindOidInfo(key, value, OidGroup.All);
            }

            return info;
        }

        //
        // verify the passed keyValue is valid as per X.208
        //
        // The first number must be 0, 1 or 2.
        // Enforce all characters are digits and dots.
        // Enforce that no dot starts or ends the Oid, and disallow double dots.
        // Enforce there is at least one dot separator.
        //

        internal static void ValidateOidValue (string keyValue) {
            if (keyValue == null)
                throw new ArgumentNullException("keyValue");

            int len = keyValue.Length;
            if (len < 2)
                goto error;

            // should not start with a dot. The first digit must be 0, 1 or 2.
            char c = keyValue[0];
            if (c != '0' && c != '1' && c != '2')
                goto error;
            if (keyValue[1] != '.' || keyValue[len - 1] == '.') // should not end in a dot
                goto error;

            bool hasAtLeastOneDot = false;
            for (int i = 1; i < len; i++) {
                // ensure every character is either a digit or a dot
                if (Char.IsDigit(keyValue[i]))
                    continue;
                if (keyValue[i] != '.' || keyValue[i + 1] == '.') // disallow double dots
                    goto error;
                hasAtLeastOneDot = true;
            }
            if (hasAtLeastOneDot)
                return;

error:
            throw new ArgumentException(SR.GetString(SR.Argument_InvalidOidValue));
        }

        internal static SafeLocalAllocHandle CopyOidsToUnmanagedMemory (OidCollection oids) {
            SafeLocalAllocHandle safeLocalAllocHandle = SafeLocalAllocHandle.InvalidHandle;
            if (oids == null || oids.Count == 0)
                return safeLocalAllocHandle;

            // Copy the oid strings to a local list to prevent a security race condition where
            // the OidCollection or individual oids can be modified by another thread and
            // potentially cause a buffer overflow
            List<string> oidStrs = new List<string>();
            foreach (Oid oid in oids) {
                oidStrs.Add(oid.Value);
            }

            IntPtr pOid = IntPtr.Zero;
            // Needs to be checked to avoid having large sets of oids overflow the sizes and allow
            // a potential buffer overflow
            checked {
                int ptrSize = oidStrs.Count * Marshal.SizeOf(typeof(IntPtr));
                int oidSize = 0;
                foreach (string oidStr in oidStrs) {
                    oidSize += (oidStr.Length + 1);
                }
                safeLocalAllocHandle = CAPI.LocalAlloc(CAPI.LPTR, new IntPtr((uint)ptrSize + (uint)oidSize));
                pOid = new IntPtr((long)safeLocalAllocHandle.DangerousGetHandle() + ptrSize);
            }
            for (int index = 0; index < oidStrs.Count; index++) {
                Marshal.WriteIntPtr(new IntPtr((long) safeLocalAllocHandle.DangerousGetHandle() + index * Marshal.SizeOf(typeof(IntPtr))), pOid);
                byte[] ansiOid = Encoding.ASCII.GetBytes(oidStrs[index]);
                Marshal.Copy(ansiOid, 0, pOid, ansiOid.Length);
                pOid = new IntPtr((long)pOid + oidStrs[index].Length + 1);
            }
            return safeLocalAllocHandle;
        }

        internal static X509Certificate2Collection GetCertificates(SafeCertStoreHandle safeCertStoreHandle) {
            X509Certificate2Collection collection = new X509Certificate2Collection();
            IntPtr pEnumContext = CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, IntPtr.Zero);
            while (pEnumContext != IntPtr.Zero) {
                X509Certificate2 certificate = new X509Certificate2(pEnumContext);
                collection.Add(certificate);
                pEnumContext = CAPI.CertEnumCertificatesInStore(safeCertStoreHandle, pEnumContext);
            }
            return collection;
        }

        //
        // Verifies whether a certificate is valid for the specified policy.
        // S_OK means the certificate is valid for the specified policy.
        // S_FALSE means the certificate is invalid for the specified policy.
        // Anything else is an error.
        //

        internal static unsafe int VerifyCertificate (SafeCertContextHandle pCertContext,
                                                      OidCollection applicationPolicy,
                                                      OidCollection certificatePolicy,
                                                      X509RevocationMode revocationMode,
                                                      X509RevocationFlag revocationFlag,
                                                      DateTime verificationTime,
                                                      TimeSpan timeout,
                                                      X509Certificate2Collection extraStore,
                                                      IntPtr pszPolicy,
                                                      IntPtr pdwErrorStatus) {
            if (pCertContext == null || pCertContext.IsInvalid)
                throw new ArgumentException("pCertContext");

            CAPI.CERT_CHAIN_POLICY_PARA PolicyPara = new CAPI.CERT_CHAIN_POLICY_PARA(Marshal.SizeOf(typeof(CAPI.CERT_CHAIN_POLICY_PARA)));
            CAPI.CERT_CHAIN_POLICY_STATUS PolicyStatus = new CAPI.CERT_CHAIN_POLICY_STATUS(Marshal.SizeOf(typeof(CAPI.CERT_CHAIN_POLICY_STATUS)));

            // Build the chain.
            SafeCertChainHandle pChainContext = SafeCertChainHandle.InvalidHandle;
            int hr = X509Chain.BuildChain(new IntPtr(CAPI.HCCE_CURRENT_USER),
                                          pCertContext, 
                                          extraStore,
                                          applicationPolicy, 
                                          certificatePolicy,
                                          revocationMode,
                                          revocationFlag,
                                          verificationTime,
                                          timeout,
                                          ref pChainContext);
            if (hr != CAPI.S_OK)
                return hr;

            // Verify the chain using the specified policy.
            if (CAPI.CertVerifyCertificateChainPolicy(pszPolicy, pChainContext, ref PolicyPara, ref PolicyStatus)) {
                if (pdwErrorStatus != IntPtr.Zero)
                    *(uint*) pdwErrorStatus = PolicyStatus.dwError;

                if (PolicyStatus.dwError != 0)
                    return CAPI.S_FALSE;
            } else {
                // The API failed.
                return Marshal.GetHRForLastWin32Error();
            }

            return CAPI.S_OK;
        }

        internal static string GetSystemErrorString (int hr) {
            StringBuilder strMessage = new StringBuilder(512);
            uint dwErrorCode = CAPI.FormatMessage (CAPI.FORMAT_MESSAGE_FROM_SYSTEM | CAPI.FORMAT_MESSAGE_IGNORE_INSERTS,
                                                   IntPtr.Zero,
                                                   (uint) hr,
                                                   0,
                                                   strMessage,
                                                   (uint)strMessage.Capacity,
                                                   IntPtr.Zero);
            if (dwErrorCode != 0)
                return strMessage.ToString();
            else 
                return SR.GetString(SR.Unknown_Error);
        }
    }
}
