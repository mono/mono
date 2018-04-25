//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Transactions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Text;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.Recovery;

    class WhereaboutsReader
    {
        string hostName;
        ProtocolInformationReader protocolInfo;

        // Whereabouts internals
        static Guid GuidWhereaboutsInfo = new Guid("{2adb4462-bd41-11d0-b12e-00c04fc2f3ef}");
        const long STmToTmProtocolSize = 4 + 4;

        enum TmProtocol
        {
            TmProtocolNone = 0,
            TmProtocolTip = 1,
            TmProtocolMsdtcV1 = 2,
            TmProtocolMsdtcV2 = 3, // unicode host names in nameobject blobs etc
            TmProtocolExtended = 4  // other stuff (e.g., WS-AT)
        }

        public WhereaboutsReader(byte[] whereabouts)
        {
            MemoryStream mem = new MemoryStream(whereabouts,
                                                0,
                                                whereabouts.Length,
                                                false,
                                                true); // Enable calls to GetBuffer()
            DeserializeWhereabouts(mem);
        }

        public string HostName
        {
            get { return this.hostName; }
        }

        public ProtocolInformationReader ProtocolInformation
        {
            get { return this.protocolInfo; }
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The calls to SerializationException and SerializationUtils are safe.")]
        void DeserializeWhereabouts(MemoryStream mem)
        {
            // guidSignature
            Guid signature = SerializationUtils.ReadGuid(mem);
            if (signature != GuidWhereaboutsInfo)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SerializationException(SR.GetString(SR.WhereaboutsSignatureMissing)));
            }

            // cTmToTmProtocols
            uint cTmToTmProtocols = SerializationUtils.ReadUInt(mem);

            // Make sure that cTmToTmProtocols is at least plausible
            if (cTmToTmProtocols * STmToTmProtocolSize > mem.Length - mem.Position)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SerializationException(SR.GetString(SR.WhereaboutsImplausibleProtocolCount)));
            }

            // Loop through each protocol
            for (uint i = 0; i < cTmToTmProtocols; i++)
            {
                DeserializeWhereaboutsProtocol(mem);
            }

            // Require a host name
            if (string.IsNullOrEmpty(this.hostName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SerializationException(SR.GetString(SR.WhereaboutsNoHostName)));
            }
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The calls to SerializationUtils are safe.")]
        void DeserializeWhereaboutsProtocol(MemoryStream mem)
        {
            // tmprotDescribed
            TmProtocol tmprotDescribed = (TmProtocol)SerializationUtils.ReadInt(mem);

            // cbTmProtocolData
            uint cbTmProtocolData = SerializationUtils.ReadUInt(mem);

            switch (tmprotDescribed)
            {
                case TmProtocol.TmProtocolMsdtcV2:
                    ReadMsdtcV2Protocol(mem, cbTmProtocolData);
                    break;

                case TmProtocol.TmProtocolExtended:
                    ReadExtendedProtocol(mem, cbTmProtocolData);
                    break;

                default:
                    // We don't care about this protocol
                    SerializationUtils.IncrementPosition(mem, cbTmProtocolData);
                    break;
            }

            // Align the cursor to a 4-byte boundary
            SerializationUtils.AlignPosition(mem, 4);
        }

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The calls to SerializationException and SerializationUtils are safe.")]
        void ReadMsdtcV2Protocol(MemoryStream mem, uint cbTmProtocolData)
        {
            const int MaxComputerName = 15;

            //
            // The host name is encoded in unicode format
            // It is followed by a null-terminating unicode character,
            // plus some padding to align on 4
            //

            // Reject host names of disproportionate size
            if (cbTmProtocolData > (MaxComputerName + 1) * 2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SerializationException(SR.GetString(SR.WhereaboutsImplausibleHostNameByteCount)));
            }

            byte[] chars = SerializationUtils.ReadBytes(mem, (int)cbTmProtocolData);

            // Count the bytes until the first null terminating character
            int cbString = 0;
            while (cbString < cbTmProtocolData - 1 &&
                  (chars[cbString] != 0 || chars[cbString + 1] != 0))
            {
                cbString += 2;
            }

            if (cbString == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SerializationException(SR.GetString(SR.WhereaboutsInvalidHostName)));
            }

            try
            {
                this.hostName = Encoding.Unicode.GetString(chars, 0, cbString);
            }
            catch (ArgumentException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SerializationException(SR.GetString(SR.WhereaboutsInvalidHostName), e));
            }
        }

        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)] // because we use ProtocolInformationReader, which is defined in a non-APTCA assembly; WSATs are not supported in partial trust, so customers should not be broken by this demand
        */
        void ReadExtendedProtocol(MemoryStream mem, uint cbTmProtocolData)
        {
            // Read the WSAT1.0 protoocol identifier
            Guid guid = SerializationUtils.ReadGuid(mem);
            if (guid == PluggableProtocol10.ProtocolGuid || guid == PluggableProtocol11.ProtocolGuid)
            {
                // This is the WS-AT extended whereabouts blob
                this.protocolInfo = new ProtocolInformationReader(mem);
            }
            else
            {
                // Some other gateway protocol... Skip the rest of the data
                SerializationUtils.IncrementPosition(mem, cbTmProtocolData - 16);
            }
        }
    }
}
