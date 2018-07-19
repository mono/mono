// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System.Collections.Generic;

namespace System
{

    // 
    // The AppContext class consists of a collection of APIs that applications can use to reason about the context
    // in which they are running.
    // 
    // One of the APIs that are available to the application (and the framework) are the ones related to the 
    // configuration switches (SetSwitch, TryGetSwitch). Those APIs are used to set and retrieve the value of a switch. 
    // 
    // Inside the framework we use those APIs to provide backward compatibility for our in-place updates. In order for
    // that to work in as many cases as possible we have intentionally coded the APIs to use very low level concepts.
    // For instance, we are directly P/Invoking into the Registry calls and so that we don't initialize the infrastructure
    // for accessing registry. Doing that would allow us to use AppContext switches inside the Registry code.
    // 
    public static class AppContext
    {
        [Flags]
        private enum SwitchValueState
        {
            HasFalseValue = 0x1,
            HasTrueValue = 0x2,
            HasLookedForOverride = 0x4,
            UnknownValue = 0x8 // Has no default and could not find an override
        }
        
        // The Dictionary is intentionally left without specifying the comparer. StringComparer.Ordinal for instance will
        // end up pulling in Globalization since all StringComparers are initialized in the static constructor (including the
        // ones that use CultureInfo).
        // Not specifying a comparer means that the dictionary will end up with a GenericEqualityComparer<string> comparer 
        private static readonly Dictionary<string, SwitchValueState> s_switchMap = new Dictionary<string, SwitchValueState>();
        private static volatile bool s_defaultsInitialized = false;

        public static string BaseDirectory
        {
#if FEATURE_CORECLR
            [System.Security.SecuritySafeCritical]
#endif
            get
            {
                // The value of APP_CONTEXT_BASE_DIRECTORY key has to be a string and it is not allowed to be any other type. 
                // Otherwise the caller will get invalid cast exception
                return (string) AppDomain.CurrentDomain.GetData("APP_CONTEXT_BASE_DIRECTORY") ?? AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public static string TargetFrameworkName
        {
            get
            {
                // Forward the value that is set on the current domain.
                return AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName;
            }
        }

#if FEATURE_CORECLR
        [System.Security.SecuritySafeCritical]
#endif
        public static object GetData(string name)
        {
            return AppDomain.CurrentDomain.GetData(name);
        }

        #region Switch APIs

        private static void InitializeDefaultSwitchValues()
        {
            // To save a method call into this method, we are first checking
            // the value of s_defaultsInitialized in the caller.
            lock (s_switchMap)
            {
                if (s_defaultsInitialized == false)
                {
                    // populate the AppContext with the default set of values
                    AppContextDefaultValues.PopulateDefaultValues();
                    s_defaultsInitialized = true;
                }
            }
        }        

        /// <summary>
        /// Try to get the value of the switch.
        /// </summary>
        /// <param name="switchName">The name of the switch</param>
        /// <param name="isEnabled">A variable where to place the value of the switch</param>
        /// <returns>A return value of true represents that the switch was set and <paramref name="isEnabled"/> contains the value of the switch</returns>
        public static bool TryGetSwitch(string switchName, out bool isEnabled)
        {
            if (switchName == null)
                throw new ArgumentNullException("switchName");
            if (switchName.Length == 0)
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "switchName");

            if (s_defaultsInitialized == false)
            {
                InitializeDefaultSwitchValues();
            }
#if DEBUG
            BCLDebug.Assert(s_defaultsInitialized == true, "AppContext defaults should have been initialized.");
#endif

            // By default, the switch is not enabled.
            isEnabled = false;

            SwitchValueState switchValue;
            lock (s_switchMap)
            {
                if (s_switchMap.TryGetValue(switchName, out switchValue))
                {
                    // The value is in the dictionary. 
                    // There are 3 cases here:
                    // 1. The value of the switch is 'unknown'. This means that the switch name is not known to the system (either via defaults or checking overrides).
                    //    Example: This is the case when, during a servicing event, a switch is added to System.Xml which ships before mscorlib. The value of the switch
                    //             Will be unknown to mscorlib.dll and we want to prevent checking the overrides every time we check this switch
                    // 2. The switch has a valid value AND we have read the overrides for it
                    //    Example: TryGetSwitch is called for a switch set via SetSwitch
                    // 3. The switch has the default value and we need to check for overrides
                    //    Example: TryGetSwitch is called for the first time for a switch that has a default value 

                    // 1. The value is unknown
                    if (switchValue == SwitchValueState.UnknownValue)
                    {
                        isEnabled = false;
                        return false;
                    }

                    // We get the value of isEnabled from the value that we stored in the dictionary
                    isEnabled = (switchValue & SwitchValueState.HasTrueValue) == SwitchValueState.HasTrueValue; 

                    // 2. The switch has a valid value AND we have checked for overrides
                    if ((switchValue & SwitchValueState.HasLookedForOverride) == SwitchValueState.HasLookedForOverride)
                    {
                        return true;
                    }
                    // 3. The switch has a valid value, but we need to check for overrides.
                    // Regardless of whether or not the switch has an override, we need to update the value to reflect
                    // the fact that we checked for overrides. 
                    bool overrideValue;
                    if (AppContextDefaultValues.TryGetSwitchOverride(switchName, out overrideValue))
                    {
                        // we found an override!
                        isEnabled = overrideValue;
                    }
                    // Update the switch in the dictionary to mark it as 'checked for override'
                    s_switchMap[switchName] = (isEnabled ? SwitchValueState.HasTrueValue : SwitchValueState.HasFalseValue)
                                                | SwitchValueState.HasLookedForOverride;

                    return true;
                }
                else
                {
                    // The value is NOT in the dictionary
                    // In this case we need to see if we have an override defined for the value.
                    // There are 2 cases:
                    // 1. The value has an override specified. In this case we need to add the value to the dictionary 
                    //    and mark it as checked for overrides
                    //    Example: In a servicing event, System.Xml introduces a switch and an override is specified.
                    //             The value is not found in mscorlib (as System.Xml ships independent of mscorlib)
                    // 2. The value does not have an override specified
                    //    In this case, we want to capture the fact that we looked for a value and found nothing by adding 
                    //    an entry in the dictionary with the 'sentinel' value of 'SwitchValueState.UnknownValue'.
                    //    Example: This will prevent us from trying to find overrides for values that we don't have in the dictionary
                    // 1. The value has an override specified.
                    bool overrideValue;
                    if (AppContextDefaultValues.TryGetSwitchOverride(switchName, out overrideValue))
                    {
                        isEnabled = overrideValue;

                        // Update the switch in the dictionary to mark it as 'checked for override'
                        s_switchMap[switchName] = (isEnabled ? SwitchValueState.HasTrueValue : SwitchValueState.HasFalseValue)
                                                    | SwitchValueState.HasLookedForOverride;

                        return true;
                    }
                    // 2. The value does not have an override.
                    s_switchMap[switchName] = SwitchValueState.UnknownValue;
                }
            }
            return false; // we did not find a value for the switch
        }

        /// <summary>
        /// Assign a switch a value
        /// </summary>
        /// <param name="switchName">The name of the switch</param>
        /// <param name="isEnabled">The value to assign</param>
        public static void SetSwitch(string switchName, bool isEnabled)
        {
            if (switchName == null)
                throw new ArgumentNullException("switchName");
            if (switchName.Length == 0)
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyName"), "switchName");

            if (s_defaultsInitialized == false)
            {
                InitializeDefaultSwitchValues();
            }
#if DEBUG
            BCLDebug.Assert(s_defaultsInitialized == true, "AppContext defaults should have been initialized.");
#endif

            SwitchValueState switchValue = (isEnabled ? SwitchValueState.HasTrueValue : SwitchValueState.HasFalseValue)
                                             | SwitchValueState.HasLookedForOverride;
            lock (s_switchMap)
            {
                // Store the new value and the fact that we checked in the dictionary
                s_switchMap[switchName] = switchValue;
            }
        }

        /// <summary>
        /// This method is going to be called from the AppContextDefaultValues class when setting up the 
        /// default values for the switches.
        /// </summary>
        internal static void DefineSwitchDefault(string switchName, bool isEnabled)
        {
#if DEBUG
            BCLDebug.Assert(System.Threading.Monitor.IsEntered(s_switchMap), "Expected the method to be called within a lock");
#endif

            s_switchMap[switchName] = isEnabled ? SwitchValueState.HasTrueValue : SwitchValueState.HasFalseValue;
        }

        /// <summary>
        /// This method is going to be called from the AppContextDefaultValues class when setting up the 
        /// override default values for the switches.
        /// </summary>
        internal static void DefineSwitchOverride(string switchName, bool isEnabled)
        {
#if DEBUG
            BCLDebug.Assert(System.Threading.Monitor.IsEntered(s_switchMap), "Expected the method to be called within a lock");
#endif

            s_switchMap[switchName] = (isEnabled ? SwitchValueState.HasTrueValue : SwitchValueState.HasFalseValue) 
                                        | SwitchValueState.HasLookedForOverride;
        }
        #endregion
    }
}
