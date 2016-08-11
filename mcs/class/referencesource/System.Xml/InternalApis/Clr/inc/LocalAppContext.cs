// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

// There are cases where we have multiple assemblies that are going to import this file and 
// if they are going to also have InternalsVisibleTo between them, there will be a compiler warning
// that the type is found both in the source and in a referenced assembly. The compiler will prefer 
// the version of the type defined in the source
//
// In order to disable the warning for this type we are disabling this warning for this entire file.
#pragma warning disable 436

// NOTE: This file should not be included in mscorlib. This should only be included in FX libraries that need to provide switches
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System
{
    internal static partial class LocalAppContext
    {
        private delegate bool TryGetSwitchDelegate(string switchName, out bool value);

        private static TryGetSwitchDelegate TryGetSwitchFromCentralAppContext;
        private static bool s_canForwardCalls;

        private static Dictionary<string, bool> s_switchMap = new Dictionary<string, bool>();
        private static readonly object s_syncLock = new object();

        private static bool DisableCaching { get; set; }

        static LocalAppContext()
        {
            // Try to setup the callback into the central AppContext
            s_canForwardCalls = SetupDelegate();

            // Populate the default values of the local app context 
            AppContextDefaultValues.PopulateDefaultValues();

            // Cache the value of the switch that help with testing
            DisableCaching = IsSwitchEnabled(@"TestSwitch.LocalAppContext.DisableCaching");
        }

        public static bool IsSwitchEnabled(string switchName)
        {
            if (s_canForwardCalls)
            {
                bool isEnabledCentrally;
                if (TryGetSwitchFromCentralAppContext(switchName, out isEnabledCentrally))
                {
                    // we found the switch, so return whatever value it has
                    return isEnabledCentrally;
                }
                // if we could not get the value from the central authority, try the local storage.
            }

            return IsSwitchEnabledLocal(switchName);
        }

        private static bool IsSwitchEnabledLocal(string switchName)
        {
            // read the value from the set of local defaults
            bool isEnabled, isPresent;
            lock (s_switchMap)
            {
                isPresent = s_switchMap.TryGetValue(switchName, out isEnabled);
            }

            // If the value is in the set of local switches, reutrn the value
            if (isPresent)
            {
                return isEnabled;
            }

            // if we could not find the switch name, we should return 'false'
            // This will preserve the concept of switches been 'off' unless explicitly set to 'on'
            return false;
        }

        private static bool SetupDelegate()
        {
            Type appContextType = typeof(object).Assembly.GetType("System.AppContext");
            if (appContextType == null)
                return false;

            MethodInfo method = appContextType.GetMethod(
                                            "TryGetSwitch",  // the method name
                                            BindingFlags.Static | BindingFlags.Public,  // binding flags
                                            null, // use the default binder
                                            new Type[] { typeof(string), typeof(bool).MakeByRefType() },
                                            null); // parameterModifiers - this is ignored by the default binder 
            if (method == null)
                return false;

            // Create delegate if we found the method.
            TryGetSwitchFromCentralAppContext = (TryGetSwitchDelegate)Delegate.CreateDelegate(typeof(TryGetSwitchDelegate), method);

            return true;
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
            if (LocalAppContext.DisableCaching)
            {
                return LocalAppContext.IsSwitchEnabled(switchName);
            }

            bool isEnabled = LocalAppContext.IsSwitchEnabled(switchName);
            switchValue = isEnabled ? 1 /*true*/ : -1 /*false*/;
            return isEnabled;
        }

        /// <summary>
        /// This method is going to be called from the AppContextDefaultValues class when setting up the 
        /// default values for the switches. !!!! This method is called during the static constructor so it does not
        /// take a lock !!!! If you are planning to use this outside of that, please ensure proper locking.
        /// </summary>
        internal static void DefineSwitchDefault(string switchName, bool initialValue)
        {
            s_switchMap[switchName] = initialValue;
        }
    }
}

#pragma warning restore 436
