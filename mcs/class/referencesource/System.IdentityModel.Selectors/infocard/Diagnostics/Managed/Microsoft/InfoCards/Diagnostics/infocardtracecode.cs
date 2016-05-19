//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace Microsoft.InfoCards.Diagnostics
{
    //
    // Summary:
    // Used to define the set of trace codes used by the infocard system. The codes
    // are used in a couple of ways:
    //  1) To uniquely identify the traces for use in the event log, etw traces etc.
    //  2) To enable easy resource lookup - the intention is that code.ToString() maps
    //      directly to a resource identifier, allowing usage such as.
    //
    // IDT.TraceCritical( InfoCardTraceCode.ServiceInformation, "service started" );
    // where the infocard.txt resource file contains the line
    // ServiceInformation   The following occured: %s
    //
    internal enum InfoCardTraceCode : int
    {
        //
        // The component offsets in the enum array. Used to partition the eventid space also.
        //
        None = 0,
        General = 5000,
        Store = 10000,
        UIAgent = 20000,
        Engine = 30000,
        Client = 40000,
        Service = 50000,

        //
        // General enums. these all take a single string parameter.
        //
        GeneralInformation = General + 1,

        //
        // Store enums. The names are used doubly for string lookup into the resources.txt file.
        //
        StoreLoading = Store + 1,
        StoreBeginTransaction,
        StoreCommitTransaction,
        StoreRollbackTransaction,
        StoreClosing,
        StoreFailedToOpenStore,
        StoreSignatureNotValid,
        StoreInvalidKey,
        StoreDeleting,

        //
        // UIAgent enums.
        //
        AgentInfoCardSelected = UIAgent + 1,
        AgentPiiDisclosed,

        //
        // Service enums.
        //

        //
        // Client enums.
        //
        ClientInformation = Client + 1,

    }



    internal enum EventCode
    {
        //
        // Audit codes
        //

        AUDIT_CARD_WRITTEN = 0x40050200,
        AUDIT_CARD_DELETE = 0x40050201,
        AUDIT_CARD_IMPORT = 0x40050202,
        AUDIT_STORE_IMPORT = 0x40050203,
        AUDIT_STORE_EXPORT = 0x40050204,
        AUDIT_STORE_DELETE = 0x40050205,

        AUDIT_SERVICE_IDLE_STOP = 0x40050206,

        //
        // Error codes
        //
        E_ICARD_COMMUNICATION = unchecked((int)0xC0050100),
        E_ICARD_DATA_ACCESS = unchecked((int)0xC0050101),
        E_ICARD_EXPORT = unchecked((int)0xC0050102),
        E_ICARD_IDENTITY = unchecked((int)0xC0050103),
        E_ICARD_IMPORT = unchecked((int)0xC0050104),
        E_ICARD_ARGUMENT = unchecked((int)0xC0050105),
        E_ICARD_REQUEST = unchecked((int)0xC0050106),
        E_ICARD_INFORMATIONCARD = unchecked((int)0xC0050107),
        E_ICARD_STOREKEY = unchecked((int)0xC0050108),
        E_ICARD_LOGOVALIDATION = unchecked((int)0xC0050109), //also defined in agent\common.h
        E_ICARD_PASSWORDVALIDATION = unchecked((int)0xC005010A),
        E_ICARD_POLICY = unchecked((int)0xC005010B),
        E_ICARD_PROCESSDIED = unchecked((int)0xC005010C),
        E_ICARD_SERVICEBUSY = unchecked((int)0xC005010D),
        E_ICARD_SERVICE = unchecked((int)0xC005010E),
        E_ICARD_SHUTTINGDOWN = unchecked((int)0xC005010F),
        E_ICARD_TOKENCREATION = unchecked((int)0xC0050110),
        E_ICARD_TRUSTEXCHANGE = unchecked((int)0xC0050111),
        E_ICARD_UNTRUSTED = unchecked((int)0xC0050112),
        E_ICARD_USERCANCELLED = unchecked((int)0xC0050113),
        E_ICARD_STORE_IMPORT = unchecked((int)0xC0050114),
        E_ICARD_FAILEDUISTART = unchecked((int)0xC0050115),
        E_ICARD_UNSUPPORTED = unchecked((int)0xC0050116),
        E_ICARD_MAXSESSIONCOUNT = unchecked((int)0xC0050117),
        E_ICARD_FILE_ACCESS = unchecked((int)0xC0050118),
        E_ICARD_MALFORMED_REQUEST = unchecked((int)0xC0050119),
        E_ICARD_UI_INITIALIZATION = unchecked((int)0xC005011A), // also defined in agent\common.h


        //
        // Trust exchange messages returned from the ip sts.
        //
        E_ICARD_REFRESH_REQUIRED = unchecked((int)0xC0050180),
        E_ICARD_MISSING_APPLIESTO = unchecked((int)0xC0050181),
        E_ICARD_INVALID_PROOF_KEY = unchecked((int)0xC0050182),
        E_ICARD_UNKNOWN_REFERENCE = unchecked((int)0xC0050183),
        E_ICARD_FAILED_REQUIRED_CLAIMS = unchecked((int)0xC0050184),

        //
        // Events that we reuse from other places.
        //
        E_INVALIDARG = unchecked((int)0x80000003),
        E_OUTOFMEMORY = unchecked((int)0x8007000E),
        SCARD_W_CANCELLED_BY_USER = unchecked((int)0x8010006E),
    }
    internal enum InfoCardEventCategory : short
    {
        General = 1
    }
}
