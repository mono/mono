//------------------------------------------------------------------------------
// <copyright file="BinaryCompatibility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Runtime.Versioning;

    // This class contains utility methods that mimic the mscorlib internal System.Runtime.Versioning.BinaryCompatibility type.

    internal sealed class BinaryCompatibility {

        // We need to use this AppDomain key instead of AppDomainSetup.TargetFrameworkName since we don't want applications
        // which happened to set TargetFrameworkName and are calling into ASP.NET APIs to suddenly start getting new behaviors.
        internal const string TargetFrameworkKey = "ASPNET_TARGETFRAMEWORK";

        // quick accessor for the current AppDomain's instance
        public static readonly BinaryCompatibility Current = new BinaryCompatibility(AppDomain.CurrentDomain.GetData(TargetFrameworkKey) as FrameworkName);

        public BinaryCompatibility(FrameworkName frameworkName) {
            // parse version from FrameworkName, otherwise use a default value
            Version version = VersionUtil.FrameworkDefault;
            if (frameworkName != null && frameworkName.Identifier == ".NETFramework") {
                version = frameworkName.Version;
            }

            TargetFramework = version;
            TargetsAtLeastFramework45 = (version >= VersionUtil.Framework45);
            TargetsAtLeastFramework451 = (version >= VersionUtil.Framework451);
            TargetsAtLeastFramework452 = (version >= VersionUtil.Framework452);
        }

        public bool TargetsAtLeastFramework45 { get; private set; }
        public bool TargetsAtLeastFramework451 { get; private set; }
        public bool TargetsAtLeastFramework452 { get; private set; }

        public Version TargetFramework { get; private set; }

    }
}
