// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation
{
    using System.Runtime.Versioning;

    internal static class FrameworkNameExtensions
    {
        public static bool Is45OrHigher(this FrameworkName frameworkName)
        {
            return frameworkName.Version.Major > 4 || (frameworkName.Version.Major == 4 && frameworkName.Version.Minor >= 5);
        }

        public static bool IsLessThan45(this FrameworkName frameworkName)
        {
            return frameworkName.Version.Major < 4 || (frameworkName.Version.Major == 4 && frameworkName.Version.Minor < 5);
        }

        public static bool IsLessThan40(this FrameworkName frameworkName)
        {
            return frameworkName.Version.Major < 4;
        }

        public static bool IsProfileSupported(this FrameworkName frameworkName)
        {
            if (frameworkName.Profile == string.Empty)
            {
                return true;
            }

            if (frameworkName.Profile == FrameworkNameConstants.ClientProfileName)
            {
                return true;
            }

            return false;
        }

        public static bool IsFullProfile(this FrameworkName frameworkName)
        {
            return string.IsNullOrEmpty(frameworkName.Profile);
        }
    }
}
