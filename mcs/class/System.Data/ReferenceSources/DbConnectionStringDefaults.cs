using System.Data.SqlClient;

namespace System.Data.Common {
    internal static class DbConnectionStringDefaults {
        // all
//        internal const string NamedConnection           = "";

        // Odbc
        internal const string Driver                    = "";
        internal const string Dsn                       = "";

        // OleDb
        internal const bool   AdoNetPooler              = false;
        internal const string FileName                  = "";
        internal const int    OleDbServices             = ~(/*DBPROPVAL_OS_AGR_AFTERSESSION*/0x00000008 | /*DBPROPVAL_OS_CLIENTCURSOR*/0x00000004); // -13
        internal const string Provider                  = "";

        // OracleClient
        internal const bool   Unicode                   = false;
        internal const bool   OmitOracleConnectionName  = false;

        // SqlClient
        internal const ApplicationIntent ApplicationIntent = System.Data.SqlClient.ApplicationIntent.ReadWrite;
        internal const string ApplicationName           = ".Net SqlClient Data Provider";
        internal const bool   AsynchronousProcessing    = false;
        internal const string AttachDBFilename          = "";
        internal const int    ConnectTimeout            = 15;
        internal const bool   ConnectionReset           = true;
        internal const bool   ContextConnection         = false;
        internal const string CurrentLanguage           = "";
        internal const string DataSource                = "";
        internal const bool   Encrypt                   = false;
        internal const bool   Enlist                    = true;
        internal const string FailoverPartner           = "";
        internal const string InitialCatalog            = "";
        internal const bool   IntegratedSecurity        = false;
        internal const int    LoadBalanceTimeout        = 0; // default of 0 means don't use
        internal const bool   MultipleActiveResultSets  = false;
        internal const bool   MultiSubnetFailover       = false;
        internal const int    MaxPoolSize               = 100;
        internal const int    MinPoolSize               = 0;
        internal const string NetworkLibrary            = "";
        internal const int    PacketSize                = 8000;
        internal const string Password                  = "";
        internal const bool   PersistSecurityInfo       = false;
        internal const bool   Pooling                   = true;
        internal const bool   TrustServerCertificate    = false;
        internal const string TypeSystemVersion         = "Latest";
        internal const string UserID                    = "";
        internal const bool   UserInstance              = false;
        internal const bool   Replication               = false;
        internal const string WorkstationID             = "";
        internal const string TransactionBinding        = "Implicit Unbind";
        internal const int    ConnectRetryCount         = 1;
        internal const int    ConnectRetryInterval      = 10;
    }
}