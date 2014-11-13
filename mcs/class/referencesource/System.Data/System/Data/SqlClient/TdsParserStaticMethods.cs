//------------------------------------------------------------------------------
// <copyright file="TdsParserStaticFunctionality.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    using System;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Runtime.Versioning;

    internal sealed class TdsParserStaticMethods {

        private TdsParserStaticMethods() { /* prevent utility class from being insantiated*/ }
        //
        // Static methods
        //

        // SxS: this method accesses registry to resolve the alias.
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        static internal void AliasRegistryLookup(ref string host, ref string protocol) {
            if (!ADP.IsEmpty(host)) {
                const String folder = "SOFTWARE\\Microsoft\\MSSQLServer\\Client\\ConnectTo";
                // Put a try...catch... around this so we don't abort ANY connection if we can't read the registry.
                string aliasLookup = (string) ADP.LocalMachineRegistryValue(folder, host);
                if (!ADP.IsEmpty(aliasLookup)) {
                    /* Result will be in the form of: "DBNMPNTW,\\blained1\pipe\sql\query". or
                         Result will be in the form of: "DBNETLIB, via:\\blained1\pipe\sql\query".

                        supported formats:
                            tcp	- DBMSSOCN,[server|server\instance][,port]
                            np - DBNMPNTW,[\\server\pipe\sql\query | \\server\pipe\MSSQL$instance\sql\query]
                                  where \sql\query is the pipename and can be replaced with any other pipe name
                            via - [DBMSGNET,server,port | DBNETLIB, via:server, port]
                            sm - DBMSLPCN,server

                        unsupported formats:
                            rpc - DBMSRPCN,server,[parameters] where parameters could be "username,password"
                            bv -  DBMSVINN,service@group@organization
                            appletalk - DBMSADSN,objectname@zone
                            spx - DBMSSPXN,[service | address,port,network]
                    */
                    // We must parse into the two component pieces, then map the first protocol piece to the
                    // appropriate value.
                    int index = aliasLookup.IndexOf(',');

                    // If we found the key, but there was no "," in the string, it is a bad Alias so return.
                    if (-1 != index) {
                        string parsedProtocol = aliasLookup.Substring(0, index).ToLower(CultureInfo.InvariantCulture);

                        // If index+1 >= length, Alias consisted of "FOO," which is a bad alias so return.
                        if (index+1 < aliasLookup.Length) {
                            string parsedAliasName = aliasLookup.Substring(index+1);

                            // Fix bug 298286
                            if ("dbnetlib" == parsedProtocol) {
                                    index = parsedAliasName.IndexOf(':');
                                    if (-1 != index && index + 1 < parsedAliasName.Length) {
                                        parsedProtocol = parsedAliasName.Substring (0, index);
                                        if (SqlConnectionString.ValidProtocal (parsedProtocol)) {
                                            protocol = parsedProtocol;
                                            host = parsedAliasName.Substring(index + 1);
                                        }
                                    }
                                }
                            else {
                                    protocol = (string)SqlConnectionString.NetlibMapping()[parsedProtocol];
                                    if (null != protocol) {
                                        host = parsedAliasName;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Encrypt password to be sent to SQL Server
        // Note: The same logic is used in SNIPacketSetData (SniManagedWrapper) to encrypt passwords stored in SecureString
        //       If this logic changed, SNIPacketSetData needs to be changed as well
        static internal Byte[] EncryptPassword(string password) {
            Byte[] bEnc = new Byte[password.Length << 1];
            int s;
            byte bLo;
            byte bHi;

            for (int i = 0; i < password.Length; i ++) {
                s = (int) password[i];
                bLo = (byte) (s & 0xff);
                bHi = (byte) ((s >> 8) & 0xff);
                bEnc[i<<1] = (Byte) ( (((bLo & 0x0f) << 4) | (bLo >> 4)) ^  0xa5 );
                bEnc[(i<<1)+1] = (Byte) ( (((bHi & 0x0f) << 4) | (bHi >> 4)) ^  0xa5);
            }
            return bEnc;
        }

        [ResourceExposure(ResourceScope.None)] // SxS: we use this method for TDS login only
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        static internal int GetCurrentProcessIdForTdsLoginOnly() {
            return SafeNativeMethods.GetCurrentProcessId();
        }


        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.UnmanagedCode)]
        [ResourceExposure(ResourceScope.None)] // SxS: we use this method for TDS login only
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        static internal Int32 GetCurrentThreadIdForTdsLoginOnly() {
#pragma warning disable 618
            return AppDomain.GetCurrentThreadId(); // don't need this to be support fibres;
#pragma warning restore 618
        }


        [ResourceExposure(ResourceScope.None)] // SxS: we use MAC address for TDS login only
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        static internal byte[] GetNetworkPhysicalAddressForTdsLoginOnly() {
            // NIC address is stored in NetworkAddress key.  However, if NetworkAddressLocal key
            // has a value that is not zero, then we cannot use the NetworkAddress key and must
            // instead generate a random one.  I do not fully understand why, this is simply what
            // the native providers do.  As for generation, I use a random number generator, which
            // means that different processes on the same machine will have different NIC address
            // values on the server.  It is not ideal, but native does not have the same value for
            // different processes either.

            const string key        = "NetworkAddress";
            const string localKey   = "NetworkAddressLocal";
            const string folder     = "SOFTWARE\\Description\\Microsoft\\Rpc\\UuidTemporaryData";

            int result = 0;
            byte[] nicAddress = null;

            object temp = ADP.LocalMachineRegistryValue(folder, localKey);
            if (temp is int) {
                result = (int) temp;
            }

            if (result <= 0) {
                temp = ADP.LocalMachineRegistryValue(folder, key);
                if (temp is byte[]) {
                    nicAddress = (byte[]) temp;
                }
            }

            if (null == nicAddress) {
                nicAddress = new byte[TdsEnums.MAX_NIC_SIZE];
                Random random = new Random();
                random.NextBytes(nicAddress);
            }

            return nicAddress;
        }
        // translates remaining time in stateObj (from user specified timeout) to timout value for SNI
        static internal Int32 GetTimeoutMilliseconds(long timeoutTime) {
            // User provided timeout t | timeout value for SNI | meaning
            // ------------------------+-----------------------+------------------------------
            //      t == long.MaxValue |                    -1 | infinite timeout (no timeout)
            //   t>0 && t<int.MaxValue |                     t |
            //          t>int.MaxValue |          int.MaxValue | must not exceed int.MaxValue

            if (Int64.MaxValue == timeoutTime) {
                return -1;  // infinite timeout
            }

            long msecRemaining = ADP.TimerRemainingMilliseconds(timeoutTime);

            if (msecRemaining < 0) {
                return 0;
            }
            if (msecRemaining > (long)Int32.MaxValue) {
                return Int32.MaxValue;
            }
            return (Int32)msecRemaining;
        }

        static internal long GetTimeoutSeconds(int timeout) {
            return GetTimeout((long)timeout * 1000L);
        }

        static internal long GetTimeout(long timeoutMilliseconds) {
            long result;
            if (timeoutMilliseconds <= 0) {
                result = Int64.MaxValue; // no timeout...
            }
            else {
                try
                {
                    result = checked(ADP.TimerCurrent() + ADP.TimerFromMilliseconds(timeoutMilliseconds));
                }
                catch (OverflowException)
                {
                    // In case of overflow, set to 'infinite' timeout
                    result = Int64.MaxValue;
                }
            }
            return result;
        }

        static internal bool TimeoutHasExpired(long timeoutTime) {
            bool result = false;

            if (0 != timeoutTime && Int64.MaxValue != timeoutTime) {
                result = ADP.TimerHasExpired(timeoutTime);
            }
            return result;
        }

        static internal int NullAwareStringLength(string str) {
            if (str == null) {
                return 0;
            }
            else {
                return str.Length;
            }
        }

        static internal int GetRemainingTimeout(int timeout, long start) {
            if (timeout <= 0) {
                return timeout;
            }
            long remaining = ADP.TimerRemainingSeconds(start + ADP.TimerFromSeconds(timeout));
            if (remaining <= 0) {
                return 1;
            }
            else {
                return checked((int)remaining);
            }
        }

    }
}
