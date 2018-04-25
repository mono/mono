// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security;

namespace System
{
    internal static partial class AppContextDefaultValues
    {
        /// <summary>
        /// This method is going to parse the <paramref name="overrides"/> parameter and set the values corresponding to them 
        /// in the AppContext object
        /// </summary>
        [SecuritySafeCritical]
        static partial void PopulateOverrideValuesPartial()
        {
            // Retrieve the value from EE config.
            string overrides = System.Runtime.Versioning.CompatibilitySwitch.GetAppContextOverridesInternalCall();

            // If we have no override values, do nothing.
            if (string.IsNullOrEmpty(overrides))
                return;

            bool encounteredEquals = false, encounteredCharsInKey = false, encounteredCharsInValue = false;
            int previousSemicolonPos = -1, firstEqualsPos = -1;

            // Iterate over the string one character at a time until we reach the end of the string.
            for (int currentPos = 0; currentPos <= overrides.Length; currentPos++)
            {
                // If the current position is either ';' or 'end-of-string' then we potentially have a key=value pair
                if (currentPos == overrides.Length || overrides[currentPos] == ';')
                {
                    // We only have a key=value pair if we encountered an equals, characters in the key and in the value
                    // portion of the pair.
                    if (encounteredEquals && encounteredCharsInKey && encounteredCharsInValue)
                    {
                        // We compute the indexes in the string for key and value
                        int firstCharOfKey = previousSemicolonPos + 1; //+1 because we don't take the ';' char
                        int lenghtOfKey = firstEqualsPos - previousSemicolonPos - 1; //-1 because we don't take the '=' char
                        string name = overrides.Substring(firstCharOfKey, lenghtOfKey);

                        int firstCharOfValue = firstEqualsPos + 1; // +1 because we don't count the '='
                        int lengthOfValue = currentPos - firstEqualsPos - 1; // -1 because we don't count the '='
                        string value = overrides.Substring(firstCharOfValue, lengthOfValue);

                        // apply the value only if it parses as a boolean
                        bool switchValue;
                        if (bool.TryParse(value, out switchValue))
                        {
                            // If multiple switches have the same name, the last value that we find will win.
                            AppContext.DefineSwitchOverride(name, switchValue);
                        }
                    }
                    previousSemicolonPos = currentPos;

                    // We need to reset these flags once we encounter a ';'
                    encounteredCharsInKey = encounteredCharsInValue = encounteredEquals = false;
                }
                else if (overrides[currentPos] == '=')
                {
                    // if the current character is '=' then we should flag it and remember it
                    if (!encounteredEquals)
                    {
                        encounteredEquals = true;
                        firstEqualsPos = currentPos;
                    }
                }
                else
                {
                    // We need to know if the key or value contain any characters (other than ';' and '=');
                    if (encounteredEquals)
                    {
                        encounteredCharsInValue = true;
                    }
                    else
                    {
                        encounteredCharsInKey = true;
                    }
                }
            }
        }

        // Note -- partial methods cannot return a value so we use refs to return information 
        [SecuritySafeCritical]
        static partial void TryGetSwitchOverridePartial(string switchName, ref bool overrideFound, ref bool overrideValue)
        {
            string valueFromConfig = null;
            bool boolFromConfig;
            overrideFound = false;

            // Read the value from the registry if we can (ie. the key exists)
            if (s_errorReadingRegistry != true)
            {
                // try to read it from the registry key and return null if the switch name is not found
                valueFromConfig = GetSwitchValueFromRegistry(switchName);
            }

            // Note: valueFromConfig will be null only if the key is not found.
            // Read the value from the Shim database.
            if (valueFromConfig == null)
            {
                // We are only going to check the Shim Database for an override in this case
                valueFromConfig = System.Runtime.Versioning.CompatibilitySwitch.GetValue(switchName);
            }

            if (valueFromConfig != null && bool.TryParse(valueFromConfig, out boolFromConfig))
            {
                // If we found a valid override value, we need to let the caller know that.
                overrideValue = boolFromConfig;
                overrideFound = true;
            }
        }

        private volatile static bool s_errorReadingRegistry;
        [SecuritySafeCritical]
        private static string GetSwitchValueFromRegistry(string switchName)
        {
            //
            // We are using P/Invokes directly instead of using the RegistryKey class to avoid pulling in the 
            // globalization stack that is required by RegistryKey.
            // 
            const string REG_KEY_APPCONTEXT = @"SOFTWARE\Microsoft\.NETFramework\AppContext";
            try
            {
                using (SafeRegistryHandle hklm = new SafeRegistryHandle((IntPtr)RegistryHive.LocalMachine, true))
                {
                    SafeRegistryHandle hkey = null;

                    if (Win32Native.RegOpenKeyEx(hklm, REG_KEY_APPCONTEXT, 0, Win32Native.KEY_READ, out hkey) == 0)
                    {
                        int size = 6 * sizeof(char); // "false".Length+1 * sizeof (char) as the API expects byte count and not char count.
                        int type = 0;
                        System.Text.StringBuilder keyBuffer = new System.Text.StringBuilder((int)size);
                        if (Win32Native.RegQueryValueEx(hkey, switchName, null, ref type, keyBuffer, ref size) == 0)
                        {
                            return keyBuffer.ToString();
                        }
                    }
                    else
                    {
                        // If we could not open the AppContext key, don't try it again.
                        s_errorReadingRegistry = true;
                    }
                }
            }
            catch 
            {
                // If there was an error, flag it so that we don't try this again.
                s_errorReadingRegistry = true;
            } 
            return null;
        }
    }
}
