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
#endregion

#region System.Net.WebSockets.HttpListenerAsyncEventArgs
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
#endregion

    }
}
