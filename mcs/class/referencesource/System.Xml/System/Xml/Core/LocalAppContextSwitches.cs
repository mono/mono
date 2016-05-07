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
        private static int _dontThrowOnInvalidSurrogatePairs;
        public static bool DontThrowOnInvalidSurrogatePairs
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(@"Switch.System.Xml.DontThrowOnInvalidSurrogatePairs", ref _dontThrowOnInvalidSurrogatePairs);
            }
        }

        private static int _ignoreEmptyKeySequences;
        public static bool IgnoreEmptyKeySequences
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return LocalAppContext.GetCachedSwitchValue(@"Switch.System.Xml.IgnoreEmptyKeySequences", ref _ignoreEmptyKeySequences);
            }
        }
    }
}
