// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
namespace System
{
    using System;
    using System.ServiceModel;

    internal static partial class AppContextDefaultValues
    {
        static partial void PopulateDefaultValuesPartial(string platformIdentifier, string profile, int version)
        {
            // When defining a new switch  you should add it to the last known version.             
            // For instance, if you are adding a switch in .NET 4.6 (the release after 4.5.2) you should defined your switch             
            // like this:             
            //    if (version <= 40502) ...             
            // This ensures that all previous versions of that platform (up-to 4.5.2) will get the old behavior by default             
            // NOTE: When adding a default value for a switch please make sure that the default value is added to ALL of the existing platforms!             
            // NOTE: When adding a new if statement for the version please ensure that ALL previous switches are enabled (ie. don't use else if) 
            switch (platformIdentifier)
            {
                case ".NETCore":
                case ".NETFramework":
                {
                    if (version <= 40502)
                    {
                        LocalAppContextSwitches.SetDefaultsLessOrEqual_452();
                    }

                    if (version <= 40601)
                    {
                        LocalAppContextSwitches.SetDefaultsLessOrEqual_461();
                    }

                    if (version <= 40602)
                    {
                        // Define the switches that should be true for 4.6.2 or less, false for 4.6.3+.
                        LocalAppContext.DefineSwitchDefault(LocalAppContextSwitches.DisableUsingServicePointManagerSecurityProtocolsString, true);
                    }

                    if (version <= 40700)
                    {
                        // Define the switches that should be true for 4.7.0 or less, false for 4.7.1+.
                        LocalAppContext.DefineSwitchDefault(LocalAppContextSwitches.UseSha1InPipeConnectionGetHashAlgorithmString, true);
                        LocalAppContext.DefineSwitchDefault(LocalAppContextSwitches.DisableAddressHeaderCollectionValidationString, true);
                        LocalAppContext.DefineSwitchDefault(LocalAppContextSwitches.UseSha1InMsmqEncryptionAlgorithmString, true);
                        LocalAppContext.DefineSwitchDefault(LocalAppContextSwitches.DontEnableSystemDefaultTlsVersionsString, true);
                     }

                    break;
                }
            }
        }
    }
}
