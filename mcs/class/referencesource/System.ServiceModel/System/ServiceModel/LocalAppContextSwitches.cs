// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
namespace System.ServiceModel
{
    using System;
    using System.Runtime.CompilerServices;

    // When adding a quirk, name it such that false is new behavior and true is old behavior.
    // You are opting IN to old behavior. The new behavior is default.
    // For example, we want to enable the functionality to explicitly add a connection close header
    // in 4.6 and above. So we set DisableExplicitConnectionCloseHeader to true if running 4.5.2 or less.
    internal static class LocalAppContextSwitches
    {
        internal const string DisableExplicitConnectionCloseHeaderString = "Switch.System.ServiceModel.DisableExplicitConnectionCloseHeader";
        internal const string AllowUnsignedToHeaderString = "Switch.System.ServiceModel.AllowUnsignedToHeader";
        internal const string DisableCngCertificatesString = "Switch.System.ServiceModel.DisableCngCertificates";
        internal const string DisableUsingServicePointManagerSecurityProtocolsString = "Switch.System.ServiceModel.DisableUsingServicePointManagerSecurityProtocols";
        internal const string UseSha1InPipeConnectionGetHashAlgorithmString = "Switch.System.ServiceModel.UseSha1InPipeConnectionGetHashAlgorithm";
        internal const string DisableAddressHeaderCollectionValidationString = "Switch.System.ServiceModel.DisableAddressHeaderCollectionValidation";
        internal const string UseSha1InMsmqEncryptionAlgorithmString = "Switch.System.ServiceModel.UseSha1InMsmqEncryptionAlgorithm";
        internal const string DontEnableSystemDefaultTlsVersionsString = "Switch.System.ServiceModel.DontEnableSystemDefaultTlsVersions";

        private static int disableExplicitConnectionCloseHeader;
        private static int allowUnsignedToHeader;
        private static int disableCngCertificates;
        private static int disableUsingServicePointManagerSecurityProtocols;
        private static int useSha1InPipeConnectionGetHashAlgorithm;
        private static int disableAddressHeaderCollectionValidation;
        private static int useSha1InMsmqEncryptionAlgorithm;
        private static int dontEnableSystemDefaultTlsVersions;

        public static bool DontEnableSystemDefaultTlsVersions
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DontEnableSystemDefaultTlsVersionsString, ref dontEnableSystemDefaultTlsVersions);
            }
        }

        public static bool UseSha1InMsmqEncryptionAlgorithm
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(UseSha1InMsmqEncryptionAlgorithmString, ref useSha1InMsmqEncryptionAlgorithm);
            }
        }

        public static bool DisableAddressHeaderCollectionValidation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DisableAddressHeaderCollectionValidationString, ref disableAddressHeaderCollectionValidation);
            }
        }

        public static bool UseSha1InPipeConnectionGetHashAlgorithm
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(UseSha1InPipeConnectionGetHashAlgorithmString, ref useSha1InPipeConnectionGetHashAlgorithm);
            }
        }

        public static bool DisableExplicitConnectionCloseHeader
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DisableExplicitConnectionCloseHeaderString, ref disableExplicitConnectionCloseHeader);
            }
        }

        public static bool AllowUnsignedToHeader
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(AllowUnsignedToHeaderString, ref allowUnsignedToHeader);
            }
        }

        public static bool DisableCngCertificates
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DisableCngCertificatesString, ref disableCngCertificates);
            }
        }

        public static bool DisableUsingServicePointManagerSecurityProtocols
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DisableUsingServicePointManagerSecurityProtocolsString, ref disableUsingServicePointManagerSecurityProtocols);
            }
        }

        public static void SetDefaultsLessOrEqual_452()
        {
#pragma warning disable BCL0012            
            // Define the switches that should be true for 4.5.2 or less, false for 4.6+.
            LocalAppContext.DefineSwitchDefault(DisableExplicitConnectionCloseHeaderString, true);
#pragma warning restore BCL0012
        }

        public static void SetDefaultsLessOrEqual_461()
        {
#pragma warning disable BCL0012            
            // Define the switches that should be true for 4.6.1 or less, false for 4.6.2+.
            LocalAppContext.DefineSwitchDefault(DisableCngCertificatesString, true);
#pragma warning restore BCL0012
        }
    }
}
