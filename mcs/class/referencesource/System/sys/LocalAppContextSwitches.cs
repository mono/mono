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

    }
}
