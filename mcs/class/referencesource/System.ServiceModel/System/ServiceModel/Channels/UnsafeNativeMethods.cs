//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
#define WSARECV
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using Microsoft.Win32.SafeHandles;
    using System.ComponentModel;
    using System.Text;
    using System.Transactions;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Security;
    using System.EnterpriseServices;

    using SafeCloseHandle = System.ServiceModel.Activation.SafeCloseHandle;
    using TOKEN_INFORMATION_CLASS = System.ServiceModel.Activation.ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        public const string KERNEL32 = "kernel32.dll";
        public const string ADVAPI32 = "advapi32.dll";
        public const string BCRYPT = "bcrypt.dll";
        public const string MQRT = "mqrt.dll";
        public const string SECUR32 = "secur32.dll";
        public const string USERENV = "userenv.dll";

#if WSARECV
        public const string WS2_32 = "ws2_32.dll";
#endif

        // 

        public const int ERROR_SUCCESS = 0;
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_INVALID_HANDLE = 6;
        public const int ERROR_NOT_ENOUGH_MEMORY = 8;
        public const int ERROR_OUTOFMEMORY = 14;
        public const int ERROR_SHARING_VIOLATION = 32;
        public const int ERROR_NETNAME_DELETED = 64;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_BROKEN_PIPE = 109;
        public const int ERROR_ALREADY_EXISTS = 183;
        public const int ERROR_PIPE_BUSY = 231;
        public const int ERROR_NO_DATA = 232;
        public const int ERROR_MORE_DATA = 234;
        public const int WAIT_TIMEOUT = 258;
        public const int ERROR_PIPE_CONNECTED = 535;
        public const int ERROR_OPERATION_ABORTED = 995;
        public const int ERROR_IO_PENDING = 997;
        public const int ERROR_SERVICE_ALREADY_RUNNING = 1056;
        public const int ERROR_SERVICE_DISABLED = 1058;
        public const int ERROR_NO_TRACKING_SERVICE = 1172;
        public const int ERROR_ALLOTTED_SPACE_EXCEEDED = 1344;
        public const int ERROR_NO_SYSTEM_RESOURCES = 1450;

        // When querying for the token length
        const int ERROR_INSUFFICIENT_BUFFER = 122;

        public const int STATUS_PENDING = 0x103;

        // socket errors
        public const int WSAACCESS = 10013;
        public const int WSAEMFILE = 10024;
        public const int WSAEMSGSIZE = 10040;
        public const int WSAEADDRINUSE = 10048;
        public const int WSAEADDRNOTAVAIL = 10049;
        public const int WSAENETDOWN = 10050;
        public const int WSAENETUNREACH = 10051;
        public const int WSAENETRESET = 10052;
        public const int WSAECONNABORTED = 10053;
        public const int WSAECONNRESET = 10054;
        public const int WSAENOBUFS = 10055;
        public const int WSAESHUTDOWN = 10058;
        public const int WSAETIMEDOUT = 10060;
        public const int WSAECONNREFUSED = 10061;
        public const int WSAEHOSTDOWN = 10064;
        public const int WSAEHOSTUNREACH = 10065;

        public const int DUPLICATE_CLOSE_SOURCE = 0x00000001;
        public const int DUPLICATE_SAME_ACCESS = 0x00000002;

        public const int FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;

        public const int GENERIC_ALL = 0x10000000;
        public const int GENERIC_READ = unchecked((int)0x80000000);
        public const int GENERIC_WRITE = 0x40000000;
        public const int FILE_CREATE_PIPE_INSTANCE = 0x00000004;
        public const int FILE_WRITE_ATTRIBUTES = 0x00000100;
        public const int FILE_WRITE_DATA = 0x00000002;
        public const int FILE_WRITE_EA = 0x00000010;

        public const int OPEN_EXISTING = 3;

        public const int PIPE_ACCESS_DUPLEX = 3;
        public const int PIPE_UNLIMITED_INSTANCES = 255;
        public const int PIPE_TYPE_BYTE = 0;
        public const int PIPE_TYPE_MESSAGE = 4;
        public const int PIPE_READMODE_BYTE = 0;
        public const int PIPE_READMODE_MESSAGE = 2;

        // VirtualAlloc constants
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_DECOMMIT = 0x4000;
        public const int PAGE_READWRITE = 4;

        public const int FILE_MAP_WRITE = 2;
        public const int FILE_MAP_READ = 4;


        public const int SDDL_REVISION_1 = 1;

        public const int SECURITY_ANONYMOUS = 0x00000000;
        public const int SECURITY_QOS_PRESENT = 0x00100000;
        public const int SECURITY_IDENTIFICATION = 0x00010000;

        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const int FORMAT_MESSAGE_FROM_STRING = 0x00000400;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;

        public const int MQ_RECEIVE_ACCESS = 0x00000001;
        public const int MQ_SEND_ACCESS = 0x00000002;
        public const int MQ_MOVE_ACCESS = 0x00000004;

        public const int MQ_DENY_NONE = 0x00000000;
        public const int MQ_DENY_RECEIVE_SHARE = 0x00000001;

        public const int MQ_ACTION_RECEIVE = 0x00000000;
        public const int MQ_ACTION_PEEK_CURRENT = unchecked((int)0x80000000);
        public const int MQ_ACTION_PEEK_NEXT = unchecked((int)0x80000001);

        public const int MQ_LOOKUP_RECEIVE_CURRENT = unchecked((int)0x40000020);
        public const int MQ_LOOKUP_PEEK_CURRENT = unchecked((int)0x40000010);

        public const int MQ_NO_TRANSACTION = 0;
        public const int MQ_MTS_TRANSACTION = 1;
        public const int MQ_SINGLE_MESSAGE = 3;

        public const int MQ_INFORMATION_PROPERTY = unchecked((int)0x400E0001);
        public const int MQ_INFORMATION_ILLEGAL_PROPERTY = unchecked((int)0x400E0002);
        public const int MQ_INFORMATION_PROPERTY_IGNORED = unchecked((int)0x400E0003);
        public const int MQ_INFORMATION_UNSUPPORTED_PROPERTY = unchecked((int)0x400E0004);
        public const int MQ_INFORMATION_DUPLICATE_PROPERTY = unchecked((int)0x400E0005);
        public const int MQ_INFORMATION_OPERATION_PENDING = unchecked((int)0x400E0006);
        public const int MQ_INFORMATION_FORMATNAME_BUFFER_TOO_SMALL = unchecked((int)0x400E0009);
        public const int MQ_INFORMATION_INTERNAL_USER_CERT_EXIST = unchecked((int)0x400E000A);
        public const int MQ_INFORMATION_OWNER_IGNORED = unchecked((int)0x400E000B);
        public const int MQ_ERROR = unchecked((int)0xC00E0001);
        public const int MQ_ERROR_PROPERTY = unchecked((int)0xC00E0002);
        public const int MQ_ERROR_QUEUE_NOT_FOUND = unchecked((int)0xC00E0003);
        public const int MQ_ERROR_QUEUE_NOT_ACTIVE = unchecked((int)0xC00E0004);
        public const int MQ_ERROR_QUEUE_EXISTS = unchecked((int)0xC00E0005);
        public const int MQ_ERROR_INVALID_PARAMETER = unchecked((int)0xC00E0006);
        public const int MQ_ERROR_INVALID_HANDLE = unchecked((int)0xC00E0007);
        public const int MQ_ERROR_OPERATION_CANCELLED = unchecked((int)0xC00E0008);
        public const int MQ_ERROR_SHARING_VIOLATION = unchecked((int)0xC00E0009);
        public const int MQ_ERROR_SERVICE_NOT_AVAILABLE = unchecked((int)0xC00E000B);
        public const int MQ_ERROR_MACHINE_NOT_FOUND = unchecked((int)0xC00E000D);
        public const int MQ_ERROR_ILLEGAL_SORT = unchecked((int)0xC00E0010);
        public const int MQ_ERROR_ILLEGAL_USER = unchecked((int)0xC00E0011);
        public const int MQ_ERROR_NO_DS = unchecked((int)0xC00E0013);
        public const int MQ_ERROR_ILLEGAL_QUEUE_PATHNAME = unchecked((int)0xC00E0014);
        public const int MQ_ERROR_ILLEGAL_PROPERTY_VALUE = unchecked((int)0xC00E0018);
        public const int MQ_ERROR_ILLEGAL_PROPERTY_VT = unchecked((int)0xC00E0019);
        public const int MQ_ERROR_BUFFER_OVERFLOW = unchecked((int)0xC00E001A);
        public const int MQ_ERROR_IO_TIMEOUT = unchecked((int)0xC00E001B);
        public const int MQ_ERROR_ILLEGAL_CURSOR_ACTION = unchecked((int)0xC00E001C);
        public const int MQ_ERROR_MESSAGE_ALREADY_RECEIVED = unchecked((int)0xC00E001D);
        public const int MQ_ERROR_ILLEGAL_FORMATNAME = unchecked((int)0xC00E001E);
        public const int MQ_ERROR_FORMATNAME_BUFFER_TOO_SMALL = unchecked((int)0xC00E001F);
        public const int MQ_ERROR_UNSUPPORTED_FORMATNAME_OPERATION = unchecked((int)0xC00E0020);
        public const int MQ_ERROR_ILLEGAL_SECURITY_DESCRIPTOR = unchecked((int)0xC00E0021);
        public const int MQ_ERROR_SENDERID_BUFFER_TOO_SMALL = unchecked((int)0xC00E0022);
        public const int MQ_ERROR_SECURITY_DESCRIPTOR_TOO_SMALL = unchecked((int)0xC00E0023);
        public const int MQ_ERROR_CANNOT_IMPERSONATE_CLIENT = unchecked((int)0xC00E0024);
        public const int MQ_ERROR_ACCESS_DENIED = unchecked((int)0xC00E0025);
        public const int MQ_ERROR_PRIVILEGE_NOT_HELD = unchecked((int)0xC00E0026);
        public const int MQ_ERROR_INSUFFICIENT_RESOURCES = unchecked((int)0xC00E0027);
        public const int MQ_ERROR_USER_BUFFER_TOO_SMALL = unchecked((int)0xC00E0028);
        public const int MQ_ERROR_MESSAGE_STORAGE_FAILED = unchecked((int)0xC00E002A);
        public const int MQ_ERROR_SENDER_CERT_BUFFER_TOO_SMALL = unchecked((int)0xC00E002B);
        public const int MQ_ERROR_INVALID_CERTIFICATE = unchecked((int)0xC00E002C);
        public const int MQ_ERROR_CORRUPTED_INTERNAL_CERTIFICATE = unchecked((int)0xC00E002D);
        public const int MQ_ERROR_INTERNAL_USER_CERT_EXIST = unchecked((int)0xC00E002E);
        public const int MQ_ERROR_NO_INTERNAL_USER_CERT = unchecked((int)0xC00E002F);
        public const int MQ_ERROR_CORRUPTED_SECURITY_DATA = unchecked((int)0xC00E0030);
        public const int MQ_ERROR_CORRUPTED_PERSONAL_CERT_STORE = unchecked((int)0xC00E0031);
        public const int MQ_ERROR_COMPUTER_DOES_NOT_SUPPORT_ENCRYPTION = unchecked((int)0xC00E0033);
        public const int MQ_ERROR_BAD_SECURITY_CONTEXT = unchecked((int)0xC00E0035);
        public const int MQ_ERROR_COULD_NOT_GET_USER_SID = unchecked((int)0xC00E0036);
        public const int MQ_ERROR_COULD_NOT_GET_ACCOUNT_INFO = unchecked((int)0xC00E0037);
        public const int MQ_ERROR_ILLEGAL_MQCOLUMNS = unchecked((int)0xC00E0038);
        public const int MQ_ERROR_ILLEGAL_PROPID = unchecked((int)0xC00E0039);
        public const int MQ_ERROR_ILLEGAL_RELATION = unchecked((int)0xC00E003A);
        public const int MQ_ERROR_ILLEGAL_PROPERTY_SIZE = unchecked((int)0xC00E003B);
        public const int MQ_ERROR_ILLEGAL_RESTRICTION_PROPID = unchecked((int)0xC00E003C);
        public const int MQ_ERROR_ILLEGAL_MQQUEUEPROPS = unchecked((int)0xC00E003D);
        public const int MQ_ERROR_PROPERTY_NOTALLOWED = unchecked((int)0xC00E003E);
        public const int MQ_ERROR_INSUFFICIENT_PROPERTIES = unchecked((int)0xC00E003F);
        public const int MQ_ERROR_MACHINE_EXISTS = unchecked((int)0xC00E0040);
        public const int MQ_ERROR_ILLEGAL_MQQMPROPS = unchecked((int)0xC00E0041);
        public const int MQ_ERROR_DS_IS_FULL = unchecked((int)0xC00E0042);
        public const int MQ_ERROR_DS_ERROR = unchecked((int)0xC00E0043);
        public const int MQ_ERROR_INVALID_OWNER = unchecked((int)0xC00E0044);
        public const int MQ_ERROR_UNSUPPORTED_ACCESS_MODE = unchecked((int)0xC00E0045);
        public const int MQ_ERROR_RESULT_BUFFER_TOO_SMALL = unchecked((int)0xC00E0046);
        public const int MQ_ERROR_DELETE_CN_IN_USE = unchecked((int)0xC00E0048);
        public const int MQ_ERROR_NO_RESPONSE_FROM_OBJECT_SERVER = unchecked((int)0xC00E0049);
        public const int MQ_ERROR_OBJECT_SERVER_NOT_AVAILABLE = unchecked((int)0xC00E004A);
        public const int MQ_ERROR_QUEUE_NOT_AVAILABLE = unchecked((int)0xC00E004B);
        public const int MQ_ERROR_DTC_CONNECT = unchecked((int)0xC00E004C);
        public const int MQ_ERROR_TRANSACTION_IMPORT = unchecked((int)0xC00E004E);
        public const int MQ_ERROR_TRANSACTION_USAGE = unchecked((int)0xC00E0050);
        public const int MQ_ERROR_TRANSACTION_SEQUENCE = unchecked((int)0xC00E0051);
        public const int MQ_ERROR_MISSING_CONNECTOR_TYPE = unchecked((int)0xC00E0055);
        public const int MQ_ERROR_STALE_HANDLE = unchecked((int)0xC00E0056);
        public const int MQ_ERROR_TRANSACTION_ENLIST = unchecked((int)0xC00E0058);
        public const int MQ_ERROR_QUEUE_DELETED = unchecked((int)0xC00E005A);
        public const int MQ_ERROR_ILLEGAL_CONTEXT = unchecked((int)0xC00E005B);
        public const int MQ_ERROR_ILLEGAL_SORT_PROPID = unchecked((int)0xC00E005C);
        public const int MQ_ERROR_LABEL_TOO_LONG = unchecked((int)0xC00E005D);
        public const int MQ_ERROR_LABEL_BUFFER_TOO_SMALL = unchecked((int)0xC00E005E);
        public const int MQ_ERROR_MQIS_SERVER_EMPTY = unchecked((int)0xC00E005F);
        public const int MQ_ERROR_MQIS_READONLY_MODE = unchecked((int)0xC00E0060);
        public const int MQ_ERROR_SYMM_KEY_BUFFER_TOO_SMALL = unchecked((int)0xC00E0061);
        public const int MQ_ERROR_SIGNATURE_BUFFER_TOO_SMALL = unchecked((int)0xC00E0062);
        public const int MQ_ERROR_PROV_NAME_BUFFER_TOO_SMALL = unchecked((int)0xC00E0063);
        public const int MQ_ERROR_ILLEGAL_OPERATION = unchecked((int)0xC00E0064);
        public const int MQ_ERROR_WRITE_NOT_ALLOWED = unchecked((int)0xC00E0065);
        public const int MQ_ERROR_WKS_CANT_SERVE_CLIENT = unchecked((int)0xC00E0066);
        public const int MQ_ERROR_DEPEND_WKS_LICENSE_OVERFLOW = unchecked((int)0xC00E0067);
        public const int MQ_ERROR_REMOTE_MACHINE_NOT_AVAILABLE = unchecked((int)0xC00E0069);
        public const int MQ_ERROR_UNSUPPORTED_OPERATION = unchecked((int)0xC00E006A);
        public const int MQ_ERROR_ENCRYPTION_PROVIDER_NOT_SUPPORTED = unchecked((int)0xC00E006B);
        public const int MQ_ERROR_CANNOT_SET_CRYPTO_SEC_DESCR = unchecked((int)0xC00E006C);
        public const int MQ_ERROR_CERTIFICATE_NOT_PROVIDED = unchecked((int)0xC00E006D);
        public const int MQ_ERROR_Q_DNS_PROPERTY_NOT_SUPPORTED = unchecked((int)0xC00E006E);
        public const int MQ_ERROR_CANNOT_CREATE_CERT_STORE = unchecked((int)0xC00E006F);
        public const int MQ_ERROR_CANNOT_OPEN_CERT_STORE = unchecked((int)0xC00E0070);
        public const int MQ_ERROR_ILLEGAL_ENTERPRISE_OPERATION = unchecked((int)0xC00E0071);
        public const int MQ_ERROR_CANNOT_GRANT_ADD_GUID = unchecked((int)0xC00E0072);
        public const int MQ_ERROR_CANNOT_LOAD_MSMQOCM = unchecked((int)0xC00E0073);
        public const int MQ_ERROR_NO_ENTRY_POINT_MSMQOCM = unchecked((int)0xC00E0074);
        public const int MQ_ERROR_NO_MSMQ_SERVERS_ON_DC = unchecked((int)0xC00E0075);
        public const int MQ_ERROR_CANNOT_JOIN_DOMAIN = unchecked((int)0xC00E0076);
        public const int MQ_ERROR_CANNOT_CREATE_ON_GC = unchecked((int)0xC00E0077);
        public const int MQ_ERROR_GUID_NOT_MATCHING = unchecked((int)0xC00E0078);
        public const int MQ_ERROR_PUBLIC_KEY_NOT_FOUND = unchecked((int)0xC00E0079);
        public const int MQ_ERROR_PUBLIC_KEY_DOES_NOT_EXIST = unchecked((int)0xC00E007A);
        public const int MQ_ERROR_ILLEGAL_MQPRIVATEPROPS = unchecked((int)0xC00E007B);
        public const int MQ_ERROR_NO_GC_IN_DOMAIN = unchecked((int)0xC00E007C);
        public const int MQ_ERROR_NO_MSMQ_SERVERS_ON_GC = unchecked((int)0xC00E007D);
        public const int MQ_ERROR_CANNOT_GET_DN = unchecked((int)0xC00E007E);
        public const int MQ_ERROR_CANNOT_HASH_DATA_EX = unchecked((int)0xC00E007F);
        public const int MQ_ERROR_CANNOT_SIGN_DATA_EX = unchecked((int)0xC00E0080);
        public const int MQ_ERROR_CANNOT_CREATE_HASH_EX = unchecked((int)0xC00E0081);
        public const int MQ_ERROR_FAIL_VERIFY_SIGNATURE_EX = unchecked((int)0xC00E0082);
        public const int MQ_ERROR_CANNOT_DELETE_PSC_OBJECTS = unchecked((int)0xC00E0083);
        public const int MQ_ERROR_NO_MQUSER_OU = unchecked((int)0xC00E0084);
        public const int MQ_ERROR_CANNOT_LOAD_MQAD = unchecked((int)0xC00E0085);
        public const int MQ_ERROR_CANNOT_LOAD_MQDSSRV = unchecked((int)0xC00E0086);
        public const int MQ_ERROR_PROPERTIES_CONFLICT = unchecked((int)0xC00E0087);
        public const int MQ_ERROR_MESSAGE_NOT_FOUND = unchecked((int)0xC00E0088);
        public const int MQ_ERROR_CANT_RESOLVE_SITES = unchecked((int)0xC00E0089);
        public const int MQ_ERROR_NOT_SUPPORTED_BY_DEPENDENT_CLIENTS = unchecked((int)0xC00E008A);
        public const int MQ_ERROR_OPERATION_NOT_SUPPORTED_BY_REMOTE_COMPUTER = unchecked((int)0xC00E008B);
        public const int MQ_ERROR_NOT_A_CORRECT_OBJECT_CLASS = unchecked((int)0xC00E008C);
        public const int MQ_ERROR_MULTI_SORT_KEYS = unchecked((int)0xC00E008D);
        public const int MQ_ERROR_GC_NEEDED = unchecked((int)0xC00E008E);
        public const int MQ_ERROR_DS_BIND_ROOT_FOREST = unchecked((int)0xC00E008F);
        public const int MQ_ERROR_DS_LOCAL_USER = unchecked((int)0xC00E0090);
        public const int MQ_ERROR_Q_ADS_PROPERTY_NOT_SUPPORTED = unchecked((int)0xC00E0091);
        public const int MQ_ERROR_BAD_XML_FORMAT = unchecked((int)0xC00E0092);
        public const int MQ_ERROR_UNSUPPORTED_CLASS = unchecked((int)0xC00E0093);
        public const int MQ_ERROR_UNINITIALIZED_OBJECT = unchecked((int)0xC00E0094);
        public const int MQ_ERROR_CANNOT_CREATE_PSC_OBJECTS = unchecked((int)0xC00E0095);
        public const int MQ_ERROR_CANNOT_UPDATE_PSC_OBJECTS = unchecked((int)0xC00E0096);
        public const int MQ_ERROR_MESSAGE_LOCKED_UNDER_TRANSACTION = unchecked((int)0xC00E009C);

        public const int MQMSG_DELIVERY_EXPRESS = 0;
        public const int MQMSG_DELIVERY_RECOVERABLE = 1;

        public const int PROPID_M_MSGID_SIZE = 20;
        public const int PROPID_M_CORRELATIONID_SIZE = 20;

        public const int MQ_MAX_MSG_LABEL_LEN = 250;

        public const int MQMSG_JOURNAL_NONE = 0;
        public const int MQMSG_DEADLETTER = 1;
        public const int MQMSG_JOURNAL = 2;

        public const int MQMSG_ACKNOWLEDGMENT_NONE = 0x00;
        public const int MQMSG_ACKNOWLEDGMENT_POS_ARRIVAL = 0x01;
        public const int MQMSG_ACKNOWLEDGMENT_POS_RECEIVE = 0x02;
        public const int MQMSG_ACKNOWLEDGMENT_NEG_ARRIVAL = 0x04;
        public const int MQMSG_ACKNOWLEDGMENT_NEG_RECEIVE = 0x08;

        public const int MQMSG_CLASS_NORMAL = 0x0;
        public const int MQMSG_CLASS_REPORT = 0x1;

        public const int MQMSG_SENDERID_TYPE_NONE = 0;
        public const int MQMSG_SENDERID_TYPE_SID = 1;

        public const int MQMSG_AUTH_LEVEL_NONE = 0;
        public const int MQMSG_AUTH_LEVEL_ALWAYS = 1;

        public const int MQMSG_PRIV_LEVEL_NONE = 0;
        public const int MQMSG_PRIV_LEVEL_BODY_BASE = 0x01;
        public const int MQMSG_PRIV_LEVEL_BODY_ENHANCED = 0x03;

        public const int MQMSG_TRACE_NONE = 0;
        public const int MQMSG_SEND_ROUTE_TO_REPORT_QUEUE = 1;

        public const int PROPID_M_BASE = 0;
        public const int PROPID_M_CLASS = (PROPID_M_BASE + 1);                  /* VT_UI2           */
        public const int PROPID_M_MSGID = (PROPID_M_BASE + 2);                  /* VT_UI1|VT_VECTOR */
        public const int PROPID_M_CORRELATIONID = (PROPID_M_BASE + 3);          /* VT_UI1|VT_VECTOR */
        public const int PROPID_M_PRIORITY = (PROPID_M_BASE + 4);               /* VT_UI1           */
        public const int PROPID_M_DELIVERY = (PROPID_M_BASE + 5);               /* VT_UI1           */
        public const int PROPID_M_ACKNOWLEDGE = (PROPID_M_BASE + 6);            /* VT_UI1           */
        public const int PROPID_M_JOURNAL = (PROPID_M_BASE + 7);                /* VT_UI1           */
        public const int PROPID_M_APPSPECIFIC = (PROPID_M_BASE + 8);            /* VT_UI4           */
        public const int PROPID_M_BODY = (PROPID_M_BASE + 9);                   /* VT_UI1|VT_VECTOR */
        public const int PROPID_M_BODY_SIZE = (PROPID_M_BASE + 10);             /* VT_UI4           */
        public const int PROPID_M_LABEL = (PROPID_M_BASE + 11);                 /* VT_LPWSTR        */
        public const int PROPID_M_LABEL_LEN = (PROPID_M_BASE + 12);             /* VT_UI4           */
        public const int PROPID_M_TIME_TO_REACH_QUEUE = (PROPID_M_BASE + 13);   /* VT_UI4           */
        public const int PROPID_M_TIME_TO_BE_RECEIVED = (PROPID_M_BASE + 14);   /* VT_UI4           */
        public const int PROPID_M_RESP_QUEUE = (PROPID_M_BASE + 15);            /* VT_LPWSTR        */
        public const int PROPID_M_RESP_QUEUE_LEN = (PROPID_M_BASE + 16);        /* VT_UI4           */
        public const int PROPID_M_ADMIN_QUEUE = (PROPID_M_BASE + 17);           /* VT_LPWSTR        */
        public const int PROPID_M_ADMIN_QUEUE_LEN = (PROPID_M_BASE + 18);       /* VT_UI4           */
        public const int PROPID_M_VERSION = (PROPID_M_BASE + 19);               /* VT_UI4           */
        public const int PROPID_M_SENDERID = (PROPID_M_BASE + 20);              /* VT_UI1|VT_VECTOR */
        public const int PROPID_M_SENDERID_LEN = (PROPID_M_BASE + 21);          /* VT_UI4           */
        public const int PROPID_M_SENDERID_TYPE = (PROPID_M_BASE + 22);         /* VT_UI4           */
        public const int PROPID_M_PRIV_LEVEL = (PROPID_M_BASE + 23);            /* VT_UI4           */
        public const int PROPID_M_AUTH_LEVEL = (PROPID_M_BASE + 24);            /* VT_UI4           */
        public const int PROPID_M_AUTHENTICATED = (PROPID_M_BASE + 25);         /* VT_UI1           */
        public const int PROPID_M_HASH_ALG = (PROPID_M_BASE + 26);              /* VT_UI4           */
        public const int PROPID_M_ENCRYPTION_ALG = (PROPID_M_BASE + 27);        /* VT_UI4           */
        public const int PROPID_M_SENDER_CERT = (PROPID_M_BASE + 28);           /* VT_UI1|VT_VECTOR */
        public const int PROPID_M_SENDER_CERT_LEN = (PROPID_M_BASE + 29);       /* VT_UI4           */
        public const int PROPID_M_SRC_MACHINE_ID = (PROPID_M_BASE + 30);        /* VT_CLSID         */
        public const int PROPID_M_SENTTIME = (PROPID_M_BASE + 31);              /* VT_UI4           */
        public const int PROPID_M_ARRIVEDTIME = (PROPID_M_BASE + 32);           /* VT_UI4           */
        public const int PROPID_M_DEST_QUEUE = (PROPID_M_BASE + 33);            /* VT_LPWSTR        */
        public const int PROPID_M_DEST_QUEUE_LEN = (PROPID_M_BASE + 34);        /* VT_UI4           */
        public const int PROPID_M_EXTENSION = (PROPID_M_BASE + 35);             /* VT_UI1|VT_VECTOR */
        public const int PROPID_M_EXTENSION_LEN = (PROPID_M_BASE + 36);         /* VT_UI4           */
        public const int PROPID_M_SECURITY_CONTEXT = (PROPID_M_BASE + 37);      /* VT_UI4           */
        public const int PROPID_M_CONNECTOR_TYPE = (PROPID_M_BASE + 38);        /* VT_CLSID         */
        public const int PROPID_M_XACT_STATUS_QUEUE = (PROPID_M_BASE + 39);     /* VT_LPWSTR        */
        public const int PROPID_M_XACT_STATUS_QUEUE_LEN = (PROPID_M_BASE + 40); /* VT_UI4           */
        public const int PROPID_M_TRACE = (PROPID_M_BASE + 41);                 /* VT_UI1           */
        public const int PROPID_M_BODY_TYPE = (PROPID_M_BASE + 42);             /* VT_UI4           */
        public const int PROPID_M_DEST_SYMM_KEY = (PROPID_M_BASE + 43);         /* VT_UI1|VT_VECTOR */
        public const int PROPID_M_DEST_SYMM_KEY_LEN = (PROPID_M_BASE + 44);     /* VT_UI4           */
        public const int PROPID_M_SIGNATURE = (PROPID_M_BASE + 45);             /* VT_UI1|VT_VECTOR */
        public const int PROPID_M_SIGNATURE_LEN = (PROPID_M_BASE + 46);         /* VT_UI4           */
        public const int PROPID_M_PROV_TYPE = (PROPID_M_BASE + 47);             /* VT_UI4           */
        public const int PROPID_M_PROV_NAME = (PROPID_M_BASE + 48);             /* VT_LPWSTR        */
        public const int PROPID_M_PROV_NAME_LEN = (PROPID_M_BASE + 49);         /* VT_UI4           */
        public const int PROPID_M_FIRST_IN_XACT = (PROPID_M_BASE + 50);         /* VT_UI1           */
        public const int PROPID_M_LAST_IN_XACT = (PROPID_M_BASE + 51);          /* VT_UI1           */
        public const int PROPID_M_XACTID = (PROPID_M_BASE + 52);                /* VT_UI1|VT_VECTOR */
        public const int PROPID_M_AUTHENTICATED_EX = (PROPID_M_BASE + 53);      /* VT_UI1           */

        public const int PROPID_M_RESP_FORMAT_NAME = (PROPID_M_BASE + 54);      /* VT_LPWSTR        */
        public const int PROPID_M_RESP_FORMAT_NAME_LEN = (PROPID_M_BASE + 55);  /* VT_UI4           */
        public const int PROPID_M_DEST_FORMAT_NAME = (PROPID_M_BASE + 58);      /* VT_LPWSTR        */
        public const int PROPID_M_DEST_FORMAT_NAME_LEN = (PROPID_M_BASE + 59);  /* VT_UI4           */
        public const int PROPID_M_LOOKUPID = (PROPID_M_BASE + 60);              /* VT_UI8           */
        public const int PROPID_M_SOAP_ENVELOPE = (PROPID_M_BASE + 61);         /* VT_LPWSTR        */
        public const int PROPID_M_SOAP_ENVELOPE_LEN = (PROPID_M_BASE + 62);     /* VT_UI4           */
        public const int PROPID_M_COMPOUND_MESSAGE = (PROPID_M_BASE + 63);      /* VT_UI1|VT_VECTOR */
        public const int PROPID_M_COMPOUND_MESSAGE_SIZE = (PROPID_M_BASE + 64); /* VT_UI4           */
        public const int PROPID_M_SOAP_HEADER = (PROPID_M_BASE + 65);           /* VT_LPWSTR        */
        public const int PROPID_M_SOAP_BODY = (PROPID_M_BASE + 66);             /* VT_LPWSTR        */
        public const int PROPID_M_DEADLETTER_QUEUE = (PROPID_M_BASE + 67);      /* VT_LPWSTR        */
        public const int PROPID_M_DEADLETTER_QUEUE_LEN = (PROPID_M_BASE + 68);  /* VT_UI4           */
        public const int PROPID_M_ABORT_COUNT = (PROPID_M_BASE + 69);           /* VT_UI4           */
        public const int PROPID_M_MOVE_COUNT = (PROPID_M_BASE + 70);            /* VT_UI4           */
        public const int PROPID_M_GROUP_ID = (PROPID_M_BASE + 71);              /* VT_LPWSTR        */
        public const int PROPID_M_GROUP_ID_LEN = (PROPID_M_BASE + 72);          /* VT_UI4           */
        public const int PROPID_M_FIRST_IN_GROUP = (PROPID_M_BASE + 73);        /* VT_UI1           */
        public const int PROPID_M_LAST_IN_GROUP = (PROPID_M_BASE + 74);         /* VT_UI1           */
        public const int PROPID_M_LAST_MOVE_TIME = (PROPID_M_BASE + 75);        /* VT_UI4           */

        public const int PROPID_Q_BASE = 100;
        public const int PROPID_Q_INSTANCE = (PROPID_Q_BASE + 1);              /* VT_CLSID         */
        public const int PROPID_Q_TYPE = (PROPID_Q_BASE + 2);                  /* VT_CLSID         */
        public const int PROPID_Q_PATHNAME = (PROPID_Q_BASE + 3);              /* VT_LPWSTR        */
        public const int PROPID_Q_JOURNAL = (PROPID_Q_BASE + 4);               /* VT_UI1           */
        public const int PROPID_Q_QUOTA = (PROPID_Q_BASE + 5);                 /* VT_UI4           */
        public const int PROPID_Q_BASEPRIORITY = (PROPID_Q_BASE + 6);          /* VT_I2            */
        public const int PROPID_Q_JOURNAL_QUOTA = (PROPID_Q_BASE + 7);         /* VT_UI4           */
        public const int PROPID_Q_LABEL = (PROPID_Q_BASE + 8);                 /* VT_LPWSTR        */
        public const int PROPID_Q_CREATE_TIME = (PROPID_Q_BASE + 9);           /* VT_I4            */
        public const int PROPID_Q_MODIFY_TIME = (PROPID_Q_BASE + 10);           /* VT_I4            */
        public const int PROPID_Q_AUTHENTICATE = (PROPID_Q_BASE + 11);          /* VT_UI1           */
        public const int PROPID_Q_PRIV_LEVEL = (PROPID_Q_BASE + 12);            /* VT_UI4           */
        public const int PROPID_Q_TRANSACTION = (PROPID_Q_BASE + 13);           /* VT_UI1           */
        public const int PROPID_Q_PATHNAME_DNS = (PROPID_Q_BASE + 24);          /* VT_LPWSTR        */
        public const int PROPID_Q_MULTICAST_ADDRESS = (PROPID_Q_BASE + 25);     /* VT_LPWSTR        */
        public const int PROPID_Q_ADS_PATH = (PROPID_Q_BASE + 26);              /* VT_LPWSTR        */

        public const int PROPID_PC_BASE = 5800;
        public const int PROPID_PC_VERSION = (PROPID_PC_BASE + 1);              /* VT_UI4           */
        public const int PROPID_PC_DS_ENABLED = (PROPID_PC_BASE + 2);           /* VT_BOOL          */

        public const int PROPID_MGMT_QUEUE_BASE = 0;
        public const int PROPID_MGMT_QUEUE_SUBQUEUE_NAMES = (PROPID_MGMT_QUEUE_BASE + 27); /* VT_LPWSTR|VT_VECTOR */

        public const int MQ_TRANSACTIONAL_NONE = 0;
        public const int MQ_TRANSACTIONAL = 1;

        public const int ALG_CLASS_HASH = (4 << 13);
        public const int ALG_CLASS_DATA_ENCRYPT = (3 << 13);

        public const int ALG_TYPE_ANY = 0;
        public const int ALG_TYPE_STREAM = (4 << 9);
        public const int ALG_TYPE_BLOCK = (3 << 9);

        public const int ALG_SID_MD5 = 3;
        public const int ALG_SID_SHA1 = 4;
        public const int ALG_SID_SHA_256 = 12;
        public const int ALG_SID_SHA_512 = 14;

        public const int ALG_SID_RC4 = 1;
        public const int ALG_SID_AES = 17;

        public const int CALG_MD5 = ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_MD5;
        public const int CALG_SHA1 = ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_SHA1;
        public const int CALG_SHA_256 = ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_SHA_256;
        public const int CALG_SHA_512 = ALG_CLASS_HASH | ALG_TYPE_ANY | ALG_SID_SHA_512;

        public const int CALG_RC4 = ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_STREAM | ALG_SID_RC4;
        public const int CALG_AES = ALG_CLASS_DATA_ENCRYPT | ALG_TYPE_BLOCK | ALG_SID_AES;

        public const int PROV_RSA_AES = 24;
        public const string MS_ENH_RSA_AES_PROV = "Microsoft Enhanced RSA and AES Cryptographic Provider";

        public const ushort VT_NULL = 1;
        public const ushort VT_BOOL = 11;
        public const ushort VT_UI1 = 17;
        public const ushort VT_UI2 = 18;
        public const ushort VT_UI4 = 19;
        public const ushort VT_UI8 = 21;
        public const ushort VT_LPWSTR = 31;
        public const ushort VT_VECTOR = 0x1000;

        public const uint MAX_PATH = 260;

        public const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
        public const uint LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800;

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            internal int nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));
            internal IntPtr lpSecurityDescriptor = IntPtr.Zero;
            internal bool bInheritHandle = false;
        }

        public unsafe delegate void MQReceiveCallback(int error, IntPtr handle, int timeout,
            int action, IntPtr props, NativeOverlapped* nativeOverlapped, IntPtr cursor);

        [DllImport(KERNEL32), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int CloseHandle
        (
            IntPtr handle
        );

        [DllImport(SECUR32), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int SspiFreeAuthIdentity(
               [In] IntPtr ppAuthIdentity
          );

        [DllImport(SECUR32), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern uint SspiExcludePackage(
        [In] IntPtr AuthIdentity,
        [MarshalAs(UnmanagedType.LPWStr)]
        [In] string pszPackageName,
        [Out] out IntPtr ppNewAuthIdentity);

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal unsafe static extern int ConnectNamedPipe
        (
            PipeHandle handle,
            NativeOverlapped* lpOverlapped
        );

        [DllImport(KERNEL32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern PipeHandle CreateFile
        (
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSECURITY_ATTRIBUTES,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport(KERNEL32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern SafeFileMappingHandle CreateFileMapping(
            IntPtr fileHandle,
            SECURITY_ATTRIBUTES securityAttributes,
            int protect,
            int sizeHigh,
            int sizeLow,
            string name
        );

        [DllImport(KERNEL32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.Machine)]
        internal unsafe static extern PipeHandle CreateNamedPipe
        (
            string name,
            int openMode,
            int pipeMode,
            int maxInstances,
            int outBufSize,
            int inBufSize,
            int timeout,
            SECURITY_ATTRIBUTES securityAttributes
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal unsafe static extern int DisconnectNamedPipe
        (
            PipeHandle handle
        );

        [DllImport(KERNEL32, ExactSpelling = true, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            PipeHandle hSourceHandle,
            SafeCloseHandle hTargetProcessHandle,
            out IntPtr lpTargetHandle,
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwOptions
        );

        [DllImport(KERNEL32, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int FormatMessage
        (
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            StringBuilder lpBuffer,
            int nSize,
            IntPtr arguments
        );

        [DllImport(KERNEL32, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int FormatMessage
        (
            int dwFlags,
            SafeLibraryHandle lpSource,
            int dwMessageId,
            int dwLanguageId,
            StringBuilder lpBuffer,
            int nSize,
            IntPtr arguments
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern int GetOverlappedResult
        (
            PipeHandle handle,
            NativeOverlapped* overlapped,
            out int bytesTransferred,
            int wait
        );

        // This p/invoke is for perf-sensitive codepaths which can guarantee a valid handle via custom locking.
        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern int GetOverlappedResult
        (
            IntPtr handle,
            NativeOverlapped* overlapped,
            out int bytesTransferred,
            int wait
        );

        // NOTE: a macro in win32
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        internal unsafe static bool HasOverlappedIoCompleted(
            NativeOverlapped* overlapped)
        {
            return overlapped->InternalLow != (IntPtr)STATUS_PENDING;
        }

        [DllImport(KERNEL32, SetLastError = true, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.Machine)]
        internal static extern SafeFileMappingHandle OpenFileMapping
        (
            int access,
            bool inheritHandle,
            string name
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern SafeViewOfFileHandle MapViewOfFile
        (
            SafeFileMappingHandle handle,
            int dwDesiredAccess,
            int dwFileOffsetHigh,
            int dwFileOffsetLow,
            IntPtr dwNumberOfBytesToMap
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand)]
        public static extern int QueryPerformanceCounter(out long time);

        // This p/invoke is for perf-sensitive codepaths which can guarantee a valid handle via custom locking.
        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        unsafe internal static extern int ReadFile
        (
            IntPtr handle,
            byte* bytes,
            int numBytesToRead,
            IntPtr numBytesRead_mustBeZero,
            NativeOverlapped* overlapped
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int SetNamedPipeHandleState
        (
            PipeHandle handle,
            ref int mode,
            IntPtr collectionCount,
            IntPtr collectionDataTimeout
        );

        // This p/invoke is for perf-sensitive codepaths which can guarantee a valid handle via custom locking.
        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static unsafe extern int WriteFile
        (
            IntPtr handle,
            byte* bytes,
            int numBytesToWrite,
            IntPtr numBytesWritten_mustBeZero,
            NativeOverlapped* lpOverlapped
        );

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static unsafe extern bool GetNamedPipeClientProcessId(PipeHandle handle, out int id);

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static unsafe extern bool GetNamedPipeServerProcessId(PipeHandle handle, out int id);

        [DllImport(KERNEL32, ExactSpelling = true),
        ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int UnmapViewOfFile
        (
            IntPtr lpBaseAddress
        );

        [DllImport(KERNEL32, ExactSpelling = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool SetWaitableTimer(SafeWaitHandle handle, ref long dueTime, int period, IntPtr mustBeZero, IntPtr mustBeZeroAlso, bool resume);

        [DllImport(KERNEL32, BestFitMapping = false, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr mustBeZero, bool manualReset, string timerName);

#if WSARECV
        [DllImport(WS2_32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static unsafe extern int WSARecv
        (
            IntPtr handle, WSABuffer* buffers, int bufferCount, out int bytesTransferred,
            ref int socketFlags,
            NativeOverlapped* nativeOverlapped,
            IntPtr completionRoutine
        );

        [DllImport(WS2_32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static unsafe extern bool WSAGetOverlappedResult(
                                                 IntPtr socketHandle,
                                                 NativeOverlapped* overlapped,
                                                 out int bytesTransferred,
                                                 bool wait,
                                                 out uint flags
                                                 );

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSABuffer
        {
            public int length;
            public IntPtr buffer;
        }
#endif

        internal static string GetComputerName(ComputerNameFormat nameType)
        {
            return System.Runtime.Interop.UnsafeNativeMethods.GetComputerName(nameType);
        }


        [DllImport(USERENV, SetLastError = true)]
        internal static extern int DeriveAppContainerSidFromAppContainerName
        (
            [In, MarshalAs(UnmanagedType.LPWStr)] string appContainerName,
            out IntPtr appContainerSid
        );

        [DllImport(ADVAPI32, SetLastError = true)]
        internal static extern IntPtr FreeSid
        (
            IntPtr pSid
        );

        // If the function succeeds, the return value is ERROR_SUCCESS and 'packageFamilyNameLength' contains the size of the data copied 
        // to 'packageFamilyName' (in WCHARs, including the null-terminator). If the function fails, the return value is a Win32 error code.
        [DllImport(KERNEL32)]
        internal static extern int PackageFamilyNameFromFullName
        (
            [In, MarshalAs(UnmanagedType.LPWStr)] string packageFullName,
            ref uint packageFamilyNameLength,
            [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder packageFamilyName
        );

        [DllImport(KERNEL32, SetLastError = true)]
        internal static extern bool GetAppContainerNamedObjectPath
        (
            IntPtr token,
            IntPtr appContainerSid,
            uint objectPathLength,
            [In, Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder objectPath,
            ref uint returnLength
        );

        [DllImport(KERNEL32, SetLastError = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport(ADVAPI32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool OpenProcessToken
        (
            IntPtr ProcessHandle,
            TokenAccessLevels DesiredAccess,
            out SafeCloseHandle TokenHandle
        );

        // Token marshalled as byte[]
        [DllImport(ADVAPI32, SetLastError = true)]
        static extern unsafe bool GetTokenInformation
        (
            SafeCloseHandle tokenHandle,
            TOKEN_INFORMATION_CLASS tokenInformationClass,
            byte[] tokenInformation,
            uint tokenInformationLength,
            out uint returnLength
        );

        // Token marshalled as uint
        [DllImport(ADVAPI32, SetLastError = true)]
        static extern bool GetTokenInformation
        (
            SafeCloseHandle tokenHandle,
            TOKEN_INFORMATION_CLASS tokenInformationClass,
            out uint tokenInformation,
            uint tokenInformationLength,
            out uint returnLength
        );

        internal static unsafe SecurityIdentifier GetAppContainerSid(SafeCloseHandle tokenHandle)
        {
            // Get length of buffer needed for sid.
            uint returnLength = UnsafeNativeMethods.GetTokenInformationLength(
                                                        tokenHandle,
                                                        TOKEN_INFORMATION_CLASS.TokenAppContainerSid);

            byte[] tokenInformation = new byte[returnLength];
            fixed (byte* pTokenInformation = tokenInformation)
            {
                if (!UnsafeNativeMethods.GetTokenInformation(
                                                tokenHandle,
                                                TOKEN_INFORMATION_CLASS.TokenAppContainerSid,
                                                tokenInformation,
                                                returnLength,
                                                out returnLength))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw FxTrace.Exception.AsError(new Win32Exception(errorCode));
                }

                TokenAppContainerInfo* ptg = (TokenAppContainerInfo*)pTokenInformation;
                return new SecurityIdentifier(ptg->psid);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct TokenAppContainerInfo
        {
            public IntPtr psid;
        }

        static uint GetTokenInformationLength(SafeCloseHandle token, TOKEN_INFORMATION_CLASS tokenInformationClass)
        {
            uint lengthNeeded;
            bool success;
            if (!(success = GetTokenInformation(
                                       token,
                                       tokenInformationClass,
                                       null,
                                       0,
                                       out lengthNeeded)))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != UnsafeNativeMethods.ERROR_INSUFFICIENT_BUFFER)
                {
                    throw FxTrace.Exception.AsError(new Win32Exception(error));
                }
            }

            Fx.Assert(!success, "Retreving the length should always fail.");

            return lengthNeeded;
        }

        internal static int GetSessionId(SafeCloseHandle tokenHandle)
        {
            uint sessionId;
            uint returnLength;

            if (!UnsafeNativeMethods.GetTokenInformation(
                                            tokenHandle,
                                            TOKEN_INFORMATION_CLASS.TokenSessionId,
                                            out sessionId,
                                            sizeof(uint),
                                            out returnLength))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw FxTrace.Exception.AsError(new Win32Exception(errorCode));
            }

            return (int)sessionId;
        }

        internal static bool RunningInAppContainer(SafeCloseHandle tokenHandle)
        {
            uint runningInAppContainer;
            uint returnLength;
            if (!UnsafeNativeMethods.GetTokenInformation(
                                        tokenHandle,
                                        TOKEN_INFORMATION_CLASS.TokenIsAppContainer,
                                        out runningInAppContainer,
                                        sizeof(uint),
                                        out returnLength))
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw FxTrace.Exception.AsError(new Win32Exception(errorCode));
            }

            return runningInAppContainer == 1;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MQMSGPROPS
        {
            public int count;
            public IntPtr ids;
            public IntPtr variants;
            public IntPtr status;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MQPROPVARIANT
        {
            [FieldOffset(0)]
            public ushort vt;
            [FieldOffset(2)]
            public ushort reserved1;
            [FieldOffset(4)]
            public ushort reserved2;
            [FieldOffset(6)]
            public ushort reserved3;
            [FieldOffset(8)]
            public byte byteValue;
            [FieldOffset(8)]
            public short shortValue;
            [FieldOffset(8)]
            public int intValue;
            [FieldOffset(8)]
            public long longValue;
            [FieldOffset(8)]
            public IntPtr intPtr;
            [FieldOffset(8)]
            public CAUI1 byteArrayValue;
            [FieldOffset(8)]
            public CALPWSTR stringArraysValue;

            [StructLayout(LayoutKind.Sequential)]
            public struct CAUI1
            {
                public int size;
                public IntPtr intPtr;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CALPWSTR
            {
                public int count;
                public IntPtr stringArrays;
            }
        }

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.Machine)]
        public static extern int MQOpenQueue(string formatName, int access, int shareMode, out MsmqQueueHandle handle);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int MQBeginTransaction(out ITransaction refTransaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int MQCloseQueue(IntPtr handle);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int MQSendMessage(MsmqQueueHandle handle, IntPtr properties, IntPtr transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int MQSendMessage(MsmqQueueHandle handle, IntPtr properties, IDtcTransaction transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQReceiveMessage(MsmqQueueHandle handle, int timeout, int action, IntPtr properties,
            NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr cursorHandle, IntPtr transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQReceiveMessage(IntPtr handle, int timeout, int action, IntPtr properties,
            NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr cursorHandle, IntPtr transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQReceiveMessage(MsmqQueueHandle handle, int timeout, int action, IntPtr properties,
            NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr cursorHandle, IDtcTransaction transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQReceiveMessage(IntPtr handle, int timeout, int action, IntPtr properties,
            NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr cursorHandle, IDtcTransaction transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQReceiveMessage(MsmqQueueHandle handle, int timeout, int action, IntPtr properties,
            NativeOverlapped* nativeOverlapped, MQReceiveCallback receiveCallback, IntPtr cursorHandle, IntPtr transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQReceiveMessage(IntPtr handle, int timeout, int action, IntPtr properties,
            NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr cursorHandle, ITransaction transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQReceiveMessageByLookupId(MsmqQueueHandle handle, long lookupId, int action,
            IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IDtcTransaction transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQReceiveMessageByLookupId(MsmqQueueHandle handle, long lookupId, int action,
            IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, IntPtr transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQReceiveMessageByLookupId(MsmqQueueHandle handle, long lookupId, int action,
            IntPtr properties, NativeOverlapped* nativeOverlapped, IntPtr receiveCallback, ITransaction transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQGetPrivateComputerInformation(string computerName, IntPtr properties);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQMarkMessageRejected(MsmqQueueHandle handle, long lookupId);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int MQMoveMessage(MsmqQueueHandle sourceQueueHandle,
                                               MsmqQueueHandle destinationQueueHandle,
                                               long lookupId,
                                               IntPtr transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public static extern int MQMoveMessage(MsmqQueueHandle sourceQueueHandle,
                                               MsmqQueueHandle destinationQueueHandle,
                                               long lookupId,
                                               IDtcTransaction transaction);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQGetOverlappedResult(NativeOverlapped* nativeOverlapped);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQGetQueueProperties(string formatName, IntPtr properties);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQPathNameToFormatName(string pathName, StringBuilder formatName, ref int count);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int MQMgmtGetInfo(string computerName, string objectName, IntPtr properties);

        [DllImport(MQRT, CharSet = CharSet.Unicode)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern void MQFreeMemory(IntPtr nativeBuffer);

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        public unsafe static extern int GetHandleInformation(MsmqQueueHandle handle, out int flags);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport(KERNEL32, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport(KERNEL32, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport(KERNEL32, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        [DllImport(KERNEL32, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr GetProcAddress(SafeLibraryHandle hModule, [In, MarshalAs(UnmanagedType.LPStr)]string lpProcName);

        [DllImport(KERNEL32, CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.Process)]
        internal static extern SafeLibraryHandle LoadLibrary(string libFilename);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.Process)]
        internal static extern SafeLibraryHandle LoadLibraryEx(string lpModuleName, IntPtr hFile, uint dwFlags);

        // On Vista and higher, check the value of the machine FIPS policy
        [DllImport(BCRYPT, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern int BCryptGetFipsAlgorithmMode(
            [MarshalAs(UnmanagedType.U1), Out] out bool pfEnabled
            );

#if !FEATURE_CORECLR
        private static IntPtr GetCurrentProcessToken() { return new IntPtr(-4); }

        enum AppPolicyClrCompat
        {
            AppPolicyClrCompat_Others = 0,
            AppPolicyClrCompat_ClassicDesktop = 1,
            AppPolicyClrCompat_Universal = 2,
            AppPolicyClrCompat_PackagedDesktop = 3
        };

        [DllImport(KERNEL32, CharSet = CharSet.None, EntryPoint = "AppPolicyGetClrCompat")]
        [System.Security.SecuritySafeCritical]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern Int32 _AppPolicyGetClrCompat(IntPtr processToken, out AppPolicyClrCompat appPolicyClrCompat);

        // AppModel.h functions (Win8+)
        [DllImport(KERNEL32, CharSet = CharSet.None, EntryPoint = "GetCurrentPackageId")]
        [System.Security.SecuritySafeCritical]
        [return: MarshalAs(UnmanagedType.I4)]
        private static extern Int32 _GetCurrentPackageId(ref Int32 pBufferLength, Byte[] pBuffer);

        [DllImport(KERNEL32, CharSet=System.Runtime.InteropServices.CharSet.Auto, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Machine)]
        private static extern IntPtr GetModuleHandle(string modName);

        // Copied from Win32Native.cs
        // Note - do NOT use this to call methods.  Use P/Invoke, which will
        // do much better things w.r.t. marshaling, pinning memory, security 
        // stuff, better interactions with thread aborts, etc.  This is used
        // solely by DoesWin32MethodExist for avoiding try/catch EntryPointNotFoundException
        // in scenarios where an OS Version check is insufficient
        [DllImport(KERNEL32, CharSet=CharSet.Ansi, BestFitMapping=false, SetLastError=true, ExactSpelling=true)]
        [ResourceExposure(ResourceScope.None)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, String methodName);

        [System.Security.SecurityCritical]  // auto-generated
        private static bool DoesWin32MethodExist(String moduleName, String methodName)
        {
            // GetModuleHandle does not increment the module's ref count, so we don't need to call FreeLibrary.
            IntPtr hModule = GetModuleHandle(moduleName);
            if (hModule == IntPtr.Zero) {
                System.Diagnostics.Debug.Assert(hModule != IntPtr.Zero, "GetModuleHandle failed.  Dll isn't loaded?");
                return false;
            }
            IntPtr functionPointer = GetProcAddress(hModule, methodName);
            return (functionPointer != IntPtr.Zero);       
        }
        
        // On CoreCLR this is not the way to determine if a process is a tailored application (which means APPX).
        // On CoreCLR AppX is determined by a flag past to the host which is exposed by AppDomain.IsAppXProcess in mscorlib.
        // The reason for this if-def is to ensure nobody takes a dependency on this on CoreCLR.        
        [System.Security.SecuritySafeCritical]
        private static bool _IsTailoredApplication()
        {
            Version windows8Version = new Version(6, 2, 0, 0);
            OperatingSystem os = Environment.OSVersion;
            bool osSupportsPackagedProcesses = os.Platform == PlatformID.Win32NT && os.Version >= windows8Version;

            if (osSupportsPackagedProcesses && DoesWin32MethodExist(KERNEL32, "AppPolicyGetClrCompat"))
            {
                // Use AppPolicyGetClrCompat if it is available. Return true if and only if this is a UWA which means if
                // this is packaged desktop app this method will return false. This may cause some confusion however 
                // this is necessary to make the behavior of packaged desktop apps identical to desktop apps.
                AppPolicyClrCompat appPolicyClrCompat;
                return _AppPolicyGetClrCompat(GetCurrentProcessToken(), out appPolicyClrCompat) == ERROR_SUCCESS && 
                    appPolicyClrCompat == AppPolicyClrCompat.AppPolicyClrCompat_Universal;
            }
            else if(osSupportsPackagedProcesses && DoesWin32MethodExist(KERNEL32, "GetCurrentPackageId"))
            {
                Int32 bufLen = 0;
                // Will return ERROR_INSUFFICIENT_BUFFER when running within a packaged application,
                // and will return ERROR_NO_PACKAGE_IDENTITY otherwise.
                return _GetCurrentPackageId(ref bufLen, null) == ERROR_INSUFFICIENT_BUFFER;
            }
            else
            {   // We must be running on a downlevel OS.
                return false;
            }
        }

        /// <summary>
        /// Indicates weather the running application is an immersive (or modern) Windows 8 (or later) application.
        /// </summary>
        internal static Lazy<bool> IsTailoredApplication = new Lazy<bool>(() => _IsTailoredApplication());
#endif //!FEATURE_CORECLR
    }

    [SuppressUnmanagedCodeSecurity]
    class PipeHandle : SafeHandleMinusOneIsInvalid
    {
        internal PipeHandle() : base(true) { }

        // This is unsafe, but is useful for a duplicated handle, which is inherently unsafe already.
        internal PipeHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);
        }

        internal int GetClientPid()
        {
            int pid;
#pragma warning suppress 56523 // Microsoft, Win32Exception ctor calls Marshal.GetLastWin32Error()
            bool success = UnsafeNativeMethods.GetNamedPipeClientProcessId(this, out pid);
            if (!success)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception());
            }
            return pid;
        }

        protected override bool ReleaseHandle()
        {
            return UnsafeNativeMethods.CloseHandle(handle) != 0;
        }
    }

    [SuppressUnmanagedCodeSecurityAttribute()]
    sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeFileMappingHandle()
            : base(true)
        {
        }

        override protected bool ReleaseHandle()
        {
            return UnsafeNativeMethods.CloseHandle(handle) != 0;
        }
    }

    [SuppressUnmanagedCodeSecurityAttribute]
    sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        bool doNotfreeLibraryOnRelease;

        internal SafeLibraryHandle()
            : base(true)
        {
            doNotfreeLibraryOnRelease = false;
        }

        public void DoNotFreeLibraryOnRelease()
        {
            this.doNotfreeLibraryOnRelease = true;
        }

        [DllImport(UnsafeNativeMethods.KERNEL32, CharSet = CharSet.Unicode)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern bool FreeLibrary(IntPtr hModule);

        override protected bool ReleaseHandle()
        {
            if (doNotfreeLibraryOnRelease)
            {
                handle = IntPtr.Zero;
                return true;
            }

            return FreeLibrary(handle);
        }
    }

    [SuppressUnmanagedCodeSecurityAttribute]
    sealed class SafeViewOfFileHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeViewOfFileHandle()
            : base(true)
        {
        }

        override protected bool ReleaseHandle()
        {
            if (UnsafeNativeMethods.UnmapViewOfFile(handle) != 0)
            {
                handle = IntPtr.Zero;
                return true;
            }
            return false;
        }
    }

    [SuppressUnmanagedCodeSecurity]
    sealed class MsmqQueueHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal MsmqQueueHandle() : base(true) { }

        protected override bool ReleaseHandle()
        {
            return UnsafeNativeMethods.MQCloseQueue(handle) >= 0;
        }
    }
}
