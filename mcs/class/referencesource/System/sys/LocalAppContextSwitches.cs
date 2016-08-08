// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System;
using System.Runtime.CompilerServices;

namespace System
{
    internal static class LocalAppContextSwitches
    {

        #region System quirks
        private static int _memberDescriptorEqualsReturnsFalseIfEquivalent;
        internal const string MemberDescriptorEqualsReturnsFalseIfEquivalentName = @"Switch.System.MemberDescriptorEqualsReturnsFalseIfEquivalent";

        public static bool MemberDescriptorEqualsReturnsFalseIfEquivalent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(MemberDescriptorEqualsReturnsFalseIfEquivalentName, ref _memberDescriptorEqualsReturnsFalseIfEquivalent);
            }
        }
        #endregion

        #region System.Net quirks
        private static int _dontEnableSchUseStrongCrypto;
        internal const string DontEnableSchUseStrongCryptoName = @"Switch.System.Net.DontEnableSchUseStrongCrypto";

        public static bool DontEnableSchUseStrongCrypto
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DontEnableSchUseStrongCryptoName, ref _dontEnableSchUseStrongCrypto);
            }
        }

        private static int _allocateOverlappedOnDemand;
        internal const string AllocateOverlappedOnDemandName = @"Switch.System.Net.WebSockets.HttpListenerAsyncEventArgs.AllocateOverlappedOnDemand";

        public static bool AllocateOverlappedOnDemand
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(AllocateOverlappedOnDemandName, ref _allocateOverlappedOnDemand);
            }
        }

        private static int _dontEnableSchSendAuxRecord;
        internal const string DontEnableSchSendAuxRecordName = @"Switch.System.Net.DontEnableSchSendAuxRecord";

        public static bool DontEnableSchSendAuxRecord
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(DontEnableSchSendAuxRecordName, ref _dontEnableSchSendAuxRecord);
            }
        }
        #endregion
    }
}
