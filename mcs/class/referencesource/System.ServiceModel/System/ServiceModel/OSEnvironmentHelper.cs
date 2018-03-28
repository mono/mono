//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Runtime;

    static class OSEnvironmentHelper
    {        
        private static readonly OSVersion currentVersion;
        private static readonly byte currentServicePack;

        static OSEnvironmentHelper()
        {
            currentServicePack = (byte)Environment.OSVersion.Version.MajorRevision;

            int major = Environment.OSVersion.Version.Major;
            int minor = Environment.OSVersion.Version.Minor;

            if ((major < 5) || ((major == 5) && (minor == 0)))
            {
                currentVersion = OSVersion.PreWinXP;
            }
            if ((major == 5) && (minor == 1))
            {
                currentVersion = OSVersion.WinXP;
            }
            else if ((major == 5) && (minor == 2))
            {
                currentVersion = OSVersion.Win2003;
            }
            else if ((major == 6) && (minor == 0))
            {
                currentVersion = OSVersion.WinVista;
            }
            else if ((major == 6) && (minor == 1))
            {
                currentVersion = OSVersion.Win7;
            }
            else if ((major == 6) && (minor == 2))
            {
                currentVersion = OSVersion.Win8;
            }
            else if ((major > 6) ||
                    ((major == 6) && (minor > 2)))
            {
                currentVersion = OSVersion.PostWin8;
            }
            else
            {
                // This should only be possible if major == 5 and minor > 2
                Fx.Assert(false, "Unknown OS");
                currentVersion = OSVersion.Unknown;
            }

        }

        internal static bool IsVistaOrGreater
        {
            get
            {
                return IsAtLeast(OSVersion.WinVista);
            }
        }

        internal static bool IsApplicationTargeting45
        {
            get
            {
#pragma warning disable 0618
                return System.Net.WebSockets.WebSocket.IsApplicationTargeting45();
#pragma warning restore 0618
            }
        }


        internal static int ProcessorCount
        {
            get
            {
                return Environment.ProcessorCount;
            }
        }

        internal static bool IsAtLeast(OSVersion version)
        {
            return IsAtLeast(version, 0);
        }

        static bool IsAtLeast(OSVersion version, byte servicePack)
        {
            Fx.Assert(version != OSVersion.Unknown, "Unknown OS");

            if (servicePack == 0)
            {
                return version <= currentVersion;
            }

            // If a SP value is provided and we have the same OS version, compare SP values
            if (version == currentVersion)
            {
                return servicePack <= currentServicePack;
            }

            return version < currentVersion;
        }
    }
}
