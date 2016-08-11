// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace System
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class AppContextSwitches
    {
        private static int _noAsyncCurrentCulture;
        public static bool NoAsyncCurrentCulture
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetCachedSwitchValue(AppContextDefaultValues.SwitchNoAsyncCurrentCulture, ref _noAsyncCurrentCulture);
            }
        }

        private static int _throwExceptionIfDisposedCancellationTokenSource;
        public static bool ThrowExceptionIfDisposedCancellationTokenSource
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetCachedSwitchValue(AppContextDefaultValues.SwitchThrowExceptionIfDisposedCancellationTokenSource, ref _throwExceptionIfDisposedCancellationTokenSource);
            }
        }

        private static int _preserveEventListnerObjectIdentity;
        public static bool PreserveEventListnerObjectIdentity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetCachedSwitchValue(AppContextDefaultValues.SwitchPreserveEventListnerObjectIdentity, ref _preserveEventListnerObjectIdentity);
            }
        }

        private static int _useLegacyPathHandling;

        /// <summary>
        /// Use legacy path normalization logic and blocking of extended syntax.
        /// </summary>
        public static bool UseLegacyPathHandling
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetCachedSwitchValue(AppContextDefaultValues.SwitchUseLegacyPathHandling, ref _useLegacyPathHandling);
            }
        }

        private static int _blockLongPaths;

        /// <summary>
        /// Throw PathTooLongException for paths greater than MAX_PATH or directories greater than 248 (as per CreateDirectory Win32 limitations)
        /// </summary>
        public static bool BlockLongPaths
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetCachedSwitchValue(AppContextDefaultValues.SwitchBlockLongPaths, ref _blockLongPaths);
            }
        }

        private static int _cloneActor;

        /// <summary>
        /// When copying a ClaimsIdentity.Actor this switch controls whether ClaimsIdentity.Actor should be set as a reference or the result of Actor.Clone()
        /// </summary>
        public static bool SetActorAsReferenceWhenCopyingClaimsIdentity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return GetCachedSwitchValue(AppContextDefaultValues.SwitchSetActorAsReferenceWhenCopyingClaimsIdentity, ref _cloneActor);
            }
        }

        //
        // Implementation details
        //

        private static bool DisableCaching { get; set; }

        static AppContextSwitches()
        {
            bool isEnabled;
            if (AppContext.TryGetSwitch(@"TestSwitch.LocalAppContext.DisableCaching", out isEnabled))
            {
                DisableCaching = isEnabled;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool GetCachedSwitchValue(string switchName, ref int switchValue)
        {
            if (switchValue < 0) return false;
            if (switchValue > 0) return true;

            return GetCachedSwitchValueInternal(switchName, ref switchValue);
        }

        private static bool GetCachedSwitchValueInternal(string switchName, ref int switchValue)
        {
            bool isSwitchEnabled;
            AppContext.TryGetSwitch(switchName, out isSwitchEnabled);

            if (DisableCaching)
            {
                return isSwitchEnabled;
            }

            switchValue = isSwitchEnabled ? 1 /*true*/ : -1 /*false*/;
            return isSwitchEnabled;
        }
    }
}
