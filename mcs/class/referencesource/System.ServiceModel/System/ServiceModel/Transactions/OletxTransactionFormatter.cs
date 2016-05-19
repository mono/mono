//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Transactions
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Transactions;

    class OleTxTransactionFormatter : TransactionFormatter
    {
        static OleTxTransactionHeader emptyTransactionHeader = new OleTxTransactionHeader(null, null);

        public override MessageHeader EmptyTransactionHeader
        {
            get { return emptyTransactionHeader; }
        }

        public override void WriteTransaction(Transaction transaction, Message message)
        {
            byte[] propToken = TransactionInterop.GetTransmitterPropagationToken(transaction);

            // Find or compute extended information for the transaction
            WsatExtendedInformation info;
            if (!WsatExtendedInformationCache.Find(transaction, out info))
            {
                uint timeout = GetTimeoutFromTransaction(transaction);
                info = (timeout != 0) ? new WsatExtendedInformation(null, timeout) : null;
            }

            OleTxTransactionHeader header = new OleTxTransactionHeader(propToken, info);
            message.Headers.Add(header);
        }

        public override TransactionInfo ReadTransaction(Message message)
        {
            OleTxTransactionHeader header = OleTxTransactionHeader.ReadFrom(message);
            if (header == null)
                return null;

            return new OleTxTransactionInfo(header);
        }

        public static uint GetTimeoutFromTransaction(Transaction transaction)
        {
            // For transactions created inside this process, we can ask ITransactionOptions
            IDtcTransaction dtcTransaction = TransactionInterop.GetDtcTransaction(transaction);
            ITransactionOptions transactionOptions = (ITransactionOptions)dtcTransaction;

            XACTOPT options;
            transactionOptions.GetOptions(out options);

            // For transactions not created inside this process, this will return zero
            return options.ulTimeout;
        }

        public static void GetTransactionAttributes(Transaction transaction,
                                                    out uint timeout,
                                                    out IsolationFlags isoFlags,
                                                    out string description)
        {
            IDtcTransaction dtcTransaction = TransactionInterop.GetDtcTransaction(transaction);
            ITransactionOptions transactionOptions = (ITransactionOptions)dtcTransaction;
            ISaneDtcTransaction saneTransaction = (ISaneDtcTransaction)dtcTransaction;

            XACTOPT options;
            transactionOptions.GetOptions(out options);

            // For transactions not created inside this process, this will be zero
            timeout = options.ulTimeout;

            description = options.szDescription;

            XACTTRANSINFO info;
            saneTransaction.GetTransactionInfo(out info);

            isoFlags = info.isoFlags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
        struct XACTOPT
        {
            public uint ulTimeout;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
            public string szDescription;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct XACTTRANSINFO
        {
            public Guid uow;
            public IsolationLevel isoLevel;
            public IsolationFlags isoFlags;
            public uint grfTCSupported;
            public uint grfRMSupported;
            public uint grfTCSupportedRetaining;
            public uint grfRMSupportedRetaining;
        }

        [ComImport,
         Guid("3A6AD9E0-23B9-11cf-AD60-00AA00A74CCD"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ITransactionOptions
        {
            void SetOptions([In] ref XACTOPT pOptions);
            void GetOptions([Out] out XACTOPT pOptions);
        }

        [ComImport,
         GuidAttribute("0fb15084-af41-11ce-bd2b-204c4f4f5020"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface ISaneDtcTransaction
        {
            void Abort(IntPtr reason, int retaining, int async);
            void Commit(int retaining, int commitType, int reserved);
            void GetTransactionInfo(out XACTTRANSINFO transactionInformation);
        }
    }
}
