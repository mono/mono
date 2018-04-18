//------------------------------------------------------------------------------
// <copyright file="SqlConnectionStringBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;
    using System.Diagnostics.CodeAnalysis;

namespace System.Data.SqlClient {

    [DefaultProperty("DataSource")]
    [System.ComponentModel.TypeConverterAttribute(typeof(SqlConnectionStringBuilder.SqlConnectionStringBuilderConverter))]
    public sealed class SqlConnectionStringBuilder : DbConnectionStringBuilder {

        private enum Keywords { // specific ordering for ConnectionString output construction
//            NamedConnection,

            DataSource,
            FailoverPartner,
            AttachDBFilename,
            InitialCatalog,
            IntegratedSecurity,
            PersistSecurityInfo,
            UserID,
            Password,

            Enlist,
            Pooling,
            MinPoolSize,
            MaxPoolSize,
            PoolBlockingPeriod,

            AsynchronousProcessing,
            ConnectionReset,
            MultipleActiveResultSets,
            Replication,

            ConnectTimeout,
            Encrypt,
            TrustServerCertificate,
            LoadBalanceTimeout,
            NetworkLibrary,
            PacketSize,
            TypeSystemVersion,

            Authentication,

            ApplicationName,
            CurrentLanguage,
            WorkstationID,

            UserInstance,

            ContextConnection,

            TransactionBinding,

            ApplicationIntent,

            MultiSubnetFailover,

            TransparentNetworkIPResolution,

            ConnectRetryCount,

            ConnectRetryInterval,

            ColumnEncryptionSetting,

            // keep the count value last
            KeywordsCount
        }

        internal const int KeywordsCount = (int)Keywords.KeywordsCount;

        private static readonly string[] _validKeywords;
        private static readonly Dictionary<string,Keywords> _keywords;

        private ApplicationIntent _applicationIntent = DbConnectionStringDefaults.ApplicationIntent;
        private string _applicationName   = DbConnectionStringDefaults.ApplicationName;
        private string _attachDBFilename  = DbConnectionStringDefaults.AttachDBFilename;
        private string _currentLanguage   = DbConnectionStringDefaults.CurrentLanguage;
        private string _dataSource        = DbConnectionStringDefaults.DataSource;
        private string _failoverPartner   = DbConnectionStringDefaults.FailoverPartner;
        private string _initialCatalog    = DbConnectionStringDefaults.InitialCatalog;
//      private string _namedConnection   = DbConnectionStringDefaults.NamedConnection;
        private string _networkLibrary    = DbConnectionStringDefaults.NetworkLibrary;
        private string _password          = DbConnectionStringDefaults.Password;
        private string _transactionBinding = DbConnectionStringDefaults.TransactionBinding;
        private string _typeSystemVersion = DbConnectionStringDefaults.TypeSystemVersion;
        private string _userID            = DbConnectionStringDefaults.UserID;
        private string _workstationID     = DbConnectionStringDefaults.WorkstationID;

        private int _connectTimeout      = DbConnectionStringDefaults.ConnectTimeout;
        private int _loadBalanceTimeout  = DbConnectionStringDefaults.LoadBalanceTimeout;
        private int _maxPoolSize         = DbConnectionStringDefaults.MaxPoolSize;
        private int _minPoolSize         = DbConnectionStringDefaults.MinPoolSize;
        private int _packetSize          = DbConnectionStringDefaults.PacketSize;
        private int _connectRetryCount   = DbConnectionStringDefaults.ConnectRetryCount;
        private int _connectRetryInterval = DbConnectionStringDefaults.ConnectRetryInterval;

        private bool _asynchronousProcessing		= DbConnectionStringDefaults.AsynchronousProcessing;
        private bool _connectionReset				= DbConnectionStringDefaults.ConnectionReset;
        private bool _contextConnection				= DbConnectionStringDefaults.ContextConnection;
        private bool _encrypt						= DbConnectionStringDefaults.Encrypt;
        private bool _trustServerCertificate		= DbConnectionStringDefaults.TrustServerCertificate;
        private bool _enlist						= DbConnectionStringDefaults.Enlist;
        private bool _integratedSecurity			= DbConnectionStringDefaults.IntegratedSecurity;
        private bool _multipleActiveResultSets		= DbConnectionStringDefaults.MultipleActiveResultSets;
        private bool _multiSubnetFailover			= DbConnectionStringDefaults.MultiSubnetFailover;
        private bool _transparentNetworkIPResolution= DbConnectionStringDefaults.TransparentNetworkIPResolution;
        private bool _persistSecurityInfo			= DbConnectionStringDefaults.PersistSecurityInfo;
        private bool _pooling						= DbConnectionStringDefaults.Pooling;
        private bool _replication					= DbConnectionStringDefaults.Replication;
        private bool _userInstance					= DbConnectionStringDefaults.UserInstance;
        private SqlAuthenticationMethod _authentication     = DbConnectionStringDefaults.Authentication;
        private SqlConnectionColumnEncryptionSetting _columnEncryptionSetting = DbConnectionStringDefaults.ColumnEncryptionSetting;
        private PoolBlockingPeriod _poolBlockingPeriod = DbConnectionStringDefaults.PoolBlockingPeriod;

        static SqlConnectionStringBuilder() {
            string[] validKeywords = new string[KeywordsCount];
            validKeywords[(int)Keywords.ApplicationIntent]              = DbConnectionStringKeywords.ApplicationIntent;
            validKeywords[(int)Keywords.ApplicationName]                = DbConnectionStringKeywords.ApplicationName;
            validKeywords[(int)Keywords.AsynchronousProcessing]         = DbConnectionStringKeywords.AsynchronousProcessing;
            validKeywords[(int)Keywords.AttachDBFilename]               = DbConnectionStringKeywords.AttachDBFilename;
            validKeywords[(int)Keywords.PoolBlockingPeriod]       = DbConnectionStringKeywords.PoolBlockingPeriod;
            validKeywords[(int)Keywords.ConnectionReset]                = DbConnectionStringKeywords.ConnectionReset;
            validKeywords[(int)Keywords.ContextConnection]              = DbConnectionStringKeywords.ContextConnection;
            validKeywords[(int)Keywords.ConnectTimeout]                 = DbConnectionStringKeywords.ConnectTimeout;
            validKeywords[(int)Keywords.CurrentLanguage]                = DbConnectionStringKeywords.CurrentLanguage;
            validKeywords[(int)Keywords.DataSource]                     = DbConnectionStringKeywords.DataSource;
            validKeywords[(int)Keywords.Encrypt]                        = DbConnectionStringKeywords.Encrypt;
            validKeywords[(int)Keywords.Enlist]                         = DbConnectionStringKeywords.Enlist;
            validKeywords[(int)Keywords.FailoverPartner]                = DbConnectionStringKeywords.FailoverPartner;
            validKeywords[(int)Keywords.InitialCatalog]                 = DbConnectionStringKeywords.InitialCatalog;
            validKeywords[(int)Keywords.IntegratedSecurity]             = DbConnectionStringKeywords.IntegratedSecurity;
            validKeywords[(int)Keywords.LoadBalanceTimeout]             = DbConnectionStringKeywords.LoadBalanceTimeout;
            validKeywords[(int)Keywords.MaxPoolSize]                    = DbConnectionStringKeywords.MaxPoolSize;
            validKeywords[(int)Keywords.MinPoolSize]                    = DbConnectionStringKeywords.MinPoolSize;
            validKeywords[(int)Keywords.MultipleActiveResultSets]       = DbConnectionStringKeywords.MultipleActiveResultSets;
            validKeywords[(int)Keywords.MultiSubnetFailover]            = DbConnectionStringKeywords.MultiSubnetFailover;
            validKeywords[(int)Keywords.TransparentNetworkIPResolution] = DbConnectionStringKeywords.TransparentNetworkIPResolution;
//          validKeywords[(int)Keywords.NamedConnection]                = DbConnectionStringKeywords.NamedConnection;
            validKeywords[(int)Keywords.NetworkLibrary]                 = DbConnectionStringKeywords.NetworkLibrary;
            validKeywords[(int)Keywords.PacketSize]                     = DbConnectionStringKeywords.PacketSize;
            validKeywords[(int)Keywords.Password]                       = DbConnectionStringKeywords.Password;
            validKeywords[(int)Keywords.PersistSecurityInfo]            = DbConnectionStringKeywords.PersistSecurityInfo;
            validKeywords[(int)Keywords.Pooling]                        = DbConnectionStringKeywords.Pooling;
            validKeywords[(int)Keywords.Replication]                    = DbConnectionStringKeywords.Replication;
            validKeywords[(int)Keywords.TransactionBinding]             = DbConnectionStringKeywords.TransactionBinding;
            validKeywords[(int)Keywords.TrustServerCertificate]         = DbConnectionStringKeywords.TrustServerCertificate;
            validKeywords[(int)Keywords.TypeSystemVersion]              = DbConnectionStringKeywords.TypeSystemVersion;
            validKeywords[(int)Keywords.UserID]                         = DbConnectionStringKeywords.UserID;
            validKeywords[(int)Keywords.UserInstance]                   = DbConnectionStringKeywords.UserInstance;
            validKeywords[(int)Keywords.WorkstationID]                  = DbConnectionStringKeywords.WorkstationID;
            validKeywords[(int)Keywords.ConnectRetryCount]              = DbConnectionStringKeywords.ConnectRetryCount;
            validKeywords[(int)Keywords.ConnectRetryInterval]           = DbConnectionStringKeywords.ConnectRetryInterval;
            validKeywords[(int)Keywords.Authentication]                 = DbConnectionStringKeywords.Authentication;
            validKeywords[(int)Keywords.ColumnEncryptionSetting]        = DbConnectionStringKeywords.ColumnEncryptionSetting;
            _validKeywords = validKeywords;

            Dictionary<string, Keywords> hash = new Dictionary<string, Keywords>(KeywordsCount + SqlConnectionString.SynonymCount, StringComparer.OrdinalIgnoreCase);
            hash.Add(DbConnectionStringKeywords.ApplicationIntent,					Keywords.ApplicationIntent);
            hash.Add(DbConnectionStringKeywords.ApplicationName,					Keywords.ApplicationName);
            hash.Add(DbConnectionStringKeywords.AsynchronousProcessing,				Keywords.AsynchronousProcessing);
            hash.Add(DbConnectionStringKeywords.AttachDBFilename,					Keywords.AttachDBFilename);
            hash.Add(DbConnectionStringKeywords.PoolBlockingPeriod,           Keywords.PoolBlockingPeriod);
            hash.Add(DbConnectionStringKeywords.ConnectTimeout,						Keywords.ConnectTimeout);
            hash.Add(DbConnectionStringKeywords.ConnectionReset,					Keywords.ConnectionReset);
            hash.Add(DbConnectionStringKeywords.ContextConnection,					Keywords.ContextConnection);
            hash.Add(DbConnectionStringKeywords.CurrentLanguage,					Keywords.CurrentLanguage);
            hash.Add(DbConnectionStringKeywords.DataSource,							Keywords.DataSource);
            hash.Add(DbConnectionStringKeywords.Encrypt,							Keywords.Encrypt);
            hash.Add(DbConnectionStringKeywords.Enlist,								Keywords.Enlist);
            hash.Add(DbConnectionStringKeywords.FailoverPartner,					Keywords.FailoverPartner);
            hash.Add(DbConnectionStringKeywords.InitialCatalog,						Keywords.InitialCatalog);
            hash.Add(DbConnectionStringKeywords.IntegratedSecurity,					Keywords.IntegratedSecurity);
            hash.Add(DbConnectionStringKeywords.LoadBalanceTimeout,					Keywords.LoadBalanceTimeout);
            hash.Add(DbConnectionStringKeywords.MultipleActiveResultSets,			Keywords.MultipleActiveResultSets);
            hash.Add(DbConnectionStringKeywords.MaxPoolSize,						Keywords.MaxPoolSize);
            hash.Add(DbConnectionStringKeywords.MinPoolSize,						Keywords.MinPoolSize);
            hash.Add(DbConnectionStringKeywords.MultiSubnetFailover,				Keywords.MultiSubnetFailover);
            hash.Add(DbConnectionStringKeywords.TransparentNetworkIPResolution,		Keywords.TransparentNetworkIPResolution);
//          hash.Add(DbConnectionStringKeywords.NamedConnection,					Keywords.NamedConnection);
            hash.Add(DbConnectionStringKeywords.NetworkLibrary,						Keywords.NetworkLibrary);
            hash.Add(DbConnectionStringKeywords.PacketSize,							Keywords.PacketSize);
            hash.Add(DbConnectionStringKeywords.Password,							Keywords.Password);
            hash.Add(DbConnectionStringKeywords.PersistSecurityInfo,				Keywords.PersistSecurityInfo);
            hash.Add(DbConnectionStringKeywords.Pooling,							Keywords.Pooling);
            hash.Add(DbConnectionStringKeywords.Replication,						Keywords.Replication);
            hash.Add(DbConnectionStringKeywords.TransactionBinding,					Keywords.TransactionBinding);
            hash.Add(DbConnectionStringKeywords.TrustServerCertificate,				Keywords.TrustServerCertificate);
            hash.Add(DbConnectionStringKeywords.TypeSystemVersion,					Keywords.TypeSystemVersion);
            hash.Add(DbConnectionStringKeywords.UserID,								Keywords.UserID);
            hash.Add(DbConnectionStringKeywords.UserInstance,						Keywords.UserInstance);
            hash.Add(DbConnectionStringKeywords.WorkstationID,						Keywords.WorkstationID);
            hash.Add(DbConnectionStringKeywords.ConnectRetryCount,					Keywords.ConnectRetryCount);
            hash.Add(DbConnectionStringKeywords.ConnectRetryInterval,				Keywords.ConnectRetryInterval);
            hash.Add(DbConnectionStringKeywords.Authentication,						Keywords.Authentication);
            hash.Add(DbConnectionStringKeywords.ColumnEncryptionSetting,			Keywords.ColumnEncryptionSetting);

            hash.Add(DbConnectionStringSynonyms.APP,								Keywords.ApplicationName);
            hash.Add(DbConnectionStringSynonyms.Async,								Keywords.AsynchronousProcessing);
            hash.Add(DbConnectionStringSynonyms.EXTENDEDPROPERTIES,					Keywords.AttachDBFilename);
            hash.Add(DbConnectionStringSynonyms.INITIALFILENAME,					Keywords.AttachDBFilename);
            hash.Add(DbConnectionStringSynonyms.CONNECTIONTIMEOUT,					Keywords.ConnectTimeout);
            hash.Add(DbConnectionStringSynonyms.TIMEOUT,							Keywords.ConnectTimeout);
            hash.Add(DbConnectionStringSynonyms.LANGUAGE,							Keywords.CurrentLanguage);
            hash.Add(DbConnectionStringSynonyms.ADDR,								Keywords.DataSource);
            hash.Add(DbConnectionStringSynonyms.ADDRESS,							Keywords.DataSource);
            hash.Add(DbConnectionStringSynonyms.NETWORKADDRESS,						Keywords.DataSource);
            hash.Add(DbConnectionStringSynonyms.SERVER,								Keywords.DataSource);
            hash.Add(DbConnectionStringSynonyms.DATABASE,							Keywords.InitialCatalog);
            hash.Add(DbConnectionStringSynonyms.TRUSTEDCONNECTION,					Keywords.IntegratedSecurity);
            hash.Add(DbConnectionStringSynonyms.ConnectionLifetime,					Keywords.LoadBalanceTimeout);
            hash.Add(DbConnectionStringSynonyms.NET,								Keywords.NetworkLibrary);
            hash.Add(DbConnectionStringSynonyms.NETWORK,							Keywords.NetworkLibrary);
            hash.Add(DbConnectionStringSynonyms.Pwd,								Keywords.Password);
            hash.Add(DbConnectionStringSynonyms.PERSISTSECURITYINFO,				Keywords.PersistSecurityInfo);
            hash.Add(DbConnectionStringSynonyms.UID,								Keywords.UserID);
            hash.Add(DbConnectionStringSynonyms.User,								Keywords.UserID);
            hash.Add(DbConnectionStringSynonyms.WSID,								Keywords.WorkstationID);
            Debug.Assert((KeywordsCount + SqlConnectionString.SynonymCount) == hash.Count, "initial expected size is incorrect");
            _keywords = hash;

        }

        public SqlConnectionStringBuilder() : this((string)null) {
        }

        public SqlConnectionStringBuilder(string connectionString) : base() {
            if (!ADP.IsEmpty(connectionString)) {
                ConnectionString = connectionString;
            }
        }

        public override object this[string keyword] {
            get {
                Keywords index = GetIndex(keyword);
                return GetAt(index);
            }
            set {
                if (null != value) {
                    Keywords index = GetIndex(keyword);
                    switch(index) {
                    case Keywords.ApplicationIntent:				this.ApplicationIntent = ConvertToApplicationIntent(keyword, value); break;
                    case Keywords.ApplicationName:					ApplicationName = ConvertToString(value); break;
                    case Keywords.AttachDBFilename:					AttachDBFilename = ConvertToString(value); break;
                    case Keywords.CurrentLanguage:					CurrentLanguage = ConvertToString(value); break;
                    case Keywords.DataSource:						DataSource = ConvertToString(value); break;
                    case Keywords.FailoverPartner:					FailoverPartner = ConvertToString(value); break;
                    case Keywords.InitialCatalog:					InitialCatalog = ConvertToString(value); break;
//                  case Keywords.NamedConnection:					NamedConnection = ConvertToString(value); break;
                    case Keywords.NetworkLibrary:					NetworkLibrary = ConvertToString(value); break;
                    case Keywords.Password:							Password = ConvertToString(value); break;
                    case Keywords.UserID:							UserID = ConvertToString(value); break;
                    case Keywords.TransactionBinding:				TransactionBinding = ConvertToString(value); break;
                    case Keywords.TypeSystemVersion:				TypeSystemVersion = ConvertToString(value); break;
                    case Keywords.WorkstationID:					WorkstationID = ConvertToString(value); break;

                    case Keywords.ConnectTimeout:					ConnectTimeout = ConvertToInt32(value); break;
                    case Keywords.LoadBalanceTimeout:				LoadBalanceTimeout = ConvertToInt32(value); break;
                    case Keywords.MaxPoolSize:						MaxPoolSize = ConvertToInt32(value); break;
                    case Keywords.MinPoolSize:						MinPoolSize = ConvertToInt32(value); break;
                    case Keywords.PacketSize:						PacketSize = ConvertToInt32(value); break;

                    case Keywords.IntegratedSecurity:				IntegratedSecurity = ConvertToIntegratedSecurity(value); break;

                    case Keywords.Authentication:					Authentication = ConvertToAuthenticationType(keyword, value); break;
                    case Keywords.ColumnEncryptionSetting:			ColumnEncryptionSetting = ConvertToColumnEncryptionSetting(keyword, value); break;
                    case Keywords.AsynchronousProcessing:			AsynchronousProcessing = ConvertToBoolean(value); break;
                    case Keywords.PoolBlockingPeriod:               PoolBlockingPeriod = ConvertToPoolBlockingPeriod(keyword, value); break;
#pragma warning disable 618 // Obsolete ConnectionReset
                    case Keywords.ConnectionReset:					ConnectionReset = ConvertToBoolean(value); break;
#pragma warning restore 618
                    case Keywords.ContextConnection:				ContextConnection = ConvertToBoolean(value); break;
                    case Keywords.Encrypt:							Encrypt = ConvertToBoolean(value); break;
                    case Keywords.TrustServerCertificate:			TrustServerCertificate = ConvertToBoolean(value); break;
                    case Keywords.Enlist:							Enlist = ConvertToBoolean(value); break;
                    case Keywords.MultipleActiveResultSets:			MultipleActiveResultSets = ConvertToBoolean(value); break;
                    case Keywords.MultiSubnetFailover:				MultiSubnetFailover = ConvertToBoolean(value); break;
                    case Keywords.TransparentNetworkIPResolution:	TransparentNetworkIPResolution = ConvertToBoolean(value); break;
                    case Keywords.PersistSecurityInfo:				PersistSecurityInfo = ConvertToBoolean(value); break;
                    case Keywords.Pooling:							Pooling = ConvertToBoolean(value); break;
                    case Keywords.Replication:						Replication = ConvertToBoolean(value); break;
                    case Keywords.UserInstance:						UserInstance = ConvertToBoolean(value); break;
                    case Keywords.ConnectRetryCount:				ConnectRetryCount = ConvertToInt32(value); break;
                    case Keywords.ConnectRetryInterval:				ConnectRetryInterval = ConvertToInt32(value); break;

                    default:
                        Debug.Assert(false, "unexpected keyword");
                        throw ADP.KeywordNotSupported(keyword);
                    }
                }
                else {
                    Remove(keyword);
                }
            }
        }

        [DisplayName(DbConnectionStringKeywords.ApplicationIntent)]
        [ResCategoryAttribute(Res.DataCategory_Initialization)]
        [ResDescriptionAttribute(Res.DbConnectionString_ApplicationIntent)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public ApplicationIntent ApplicationIntent {
            get { return _applicationIntent; }
            set {
                if (!DbConnectionStringBuilderUtil.IsValidApplicationIntentValue(value)) {
                    throw ADP.InvalidEnumerationValue(typeof(ApplicationIntent), (int)value);
                }

                SetApplicationIntentValue(value);
                _applicationIntent = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.ApplicationName)]
        [ResCategoryAttribute(Res.DataCategory_Context)]
        [ResDescriptionAttribute(Res.DbConnectionString_ApplicationName)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public string ApplicationName {
            get { return _applicationName; }
            set {
                SetValue(DbConnectionStringKeywords.ApplicationName, value);
                _applicationName = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.AsynchronousProcessing)]
        [ResCategoryAttribute(Res.DataCategory_Initialization)]
        [ResDescriptionAttribute(Res.DbConnectionString_AsynchronousProcessing)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool AsynchronousProcessing {
            get { return _asynchronousProcessing; }
            set {
                SetValue(DbConnectionStringKeywords.AsynchronousProcessing, value);
                _asynchronousProcessing = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.AttachDBFilename)]
        [ResCategoryAttribute(Res.DataCategory_Source)]
        [ResDescriptionAttribute(Res.DbConnectionString_AttachDBFilename)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        // 
        [Editor("System.Windows.Forms.Design.FileNameEditor, " + AssemblyRef.SystemDesign, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing)]
        public string AttachDBFilename {
            get { return _attachDBFilename; }
            set {
                SetValue(DbConnectionStringKeywords.AttachDBFilename, value);
                _attachDBFilename = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.PoolBlockingPeriod)]
        [ResCategoryAttribute(Res.DataCategory_Pooling)]
        [ResDescriptionAttribute(Res.DbConnectionString_PoolBlockingPeriod)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public PoolBlockingPeriod PoolBlockingPeriod
        {
            get { return _poolBlockingPeriod; }
            set
            {
                if (!DbConnectionStringBuilderUtil.IsValidPoolBlockingPeriodValue(value))
                {
                    throw ADP.InvalidEnumerationValue(typeof(PoolBlockingPeriod), (int)value);
                }

                SetPoolBlockingPeriodValue(value);
                _poolBlockingPeriod = value;
            }
        }

        [Browsable(false)]
        [DisplayName(DbConnectionStringKeywords.ConnectionReset)]
        [Obsolete("ConnectionReset has been deprecated.  SqlConnection will ignore the 'connection reset' keyword and always reset the connection")] // SQLPT 41700
        [ResCategoryAttribute(Res.DataCategory_Pooling)]
        [ResDescriptionAttribute(Res.DbConnectionString_ConnectionReset)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool ConnectionReset {
            get { return _connectionReset; }
            set {
                SetValue(DbConnectionStringKeywords.ConnectionReset, value);
                _connectionReset = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.ContextConnection)]
        [ResCategoryAttribute(Res.DataCategory_Source)]
        [ResDescriptionAttribute(Res.DbConnectionString_ContextConnection)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool ContextConnection {
            get { return _contextConnection; }
            set {
                SetValue(DbConnectionStringKeywords.ContextConnection, value);
                _contextConnection = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.ConnectTimeout)]
        [ResCategoryAttribute(Res.DataCategory_Initialization)]
        [ResDescriptionAttribute(Res.DbConnectionString_ConnectTimeout)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public int ConnectTimeout {
            get { return _connectTimeout; }
            set {
                if (value < 0) {
                    throw ADP.InvalidConnectionOptionValue(DbConnectionStringKeywords.ConnectTimeout);
                }
                SetValue(DbConnectionStringKeywords.ConnectTimeout, value);
                _connectTimeout = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.CurrentLanguage)]
        [ResCategoryAttribute(Res.DataCategory_Initialization)]
        [ResDescriptionAttribute(Res.DbConnectionString_CurrentLanguage)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public string CurrentLanguage {
            get { return _currentLanguage; }
            set {
                SetValue(DbConnectionStringKeywords.CurrentLanguage, value);
                _currentLanguage = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.DataSource)]
        [ResCategoryAttribute(Res.DataCategory_Source)]
        [ResDescriptionAttribute(Res.DbConnectionString_DataSource)]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(SqlDataSourceConverter))]
        public string DataSource {
            get { return _dataSource; }
            set {
                SetValue(DbConnectionStringKeywords.DataSource, value);
                _dataSource = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.Encrypt)]
        [ResCategoryAttribute(Res.DataCategory_Security)]
        [ResDescriptionAttribute(Res.DbConnectionString_Encrypt)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool Encrypt {
            get { return _encrypt; }
            set {
                SetValue(DbConnectionStringKeywords.Encrypt, value);
                _encrypt = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.ColumnEncryptionSetting)]
        [ResCategoryAttribute(Res.DataCategory_Security)]
        [ResDescriptionAttribute(Res.TCE_DbConnectionString_ColumnEncryptionSetting)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public SqlConnectionColumnEncryptionSetting ColumnEncryptionSetting {
            get { return _columnEncryptionSetting; }
            set {
                if (!DbConnectionStringBuilderUtil.IsValidColumnEncryptionSetting(value)) {
                    throw ADP.InvalidEnumerationValue(typeof(SqlConnectionColumnEncryptionSetting), (int)value);
                }

                 SetColumnEncryptionSettingValue(value);
                _columnEncryptionSetting = value;
            }
        }
        
        [DisplayName(DbConnectionStringKeywords.TrustServerCertificate)]
        [ResCategoryAttribute(Res.DataCategory_Security)]
        [ResDescriptionAttribute(Res.DbConnectionString_TrustServerCertificate)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool TrustServerCertificate {
            get { return _trustServerCertificate; }
            set {
                SetValue(DbConnectionStringKeywords.TrustServerCertificate, value);
                _trustServerCertificate = value;
            }
        }
        
        [DisplayName(DbConnectionStringKeywords.Enlist)]
        [ResCategoryAttribute(Res.DataCategory_Pooling)]
        [ResDescriptionAttribute(Res.DbConnectionString_Enlist)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool Enlist {
            get { return _enlist; }
            set {
                SetValue(DbConnectionStringKeywords.Enlist, value);
                _enlist = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.FailoverPartner)]
        [ResCategoryAttribute(Res.DataCategory_Source)]
        [ResDescriptionAttribute(Res.DbConnectionString_FailoverPartner)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        [TypeConverter(typeof(SqlDataSourceConverter))]
        public string FailoverPartner {
            get { return _failoverPartner; }
            set {
                SetValue(DbConnectionStringKeywords.FailoverPartner, value);
                _failoverPartner= value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.InitialCatalog)]
        [ResCategoryAttribute(Res.DataCategory_Source)]
        [ResDescriptionAttribute(Res.DbConnectionString_InitialCatalog)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        [TypeConverter(typeof(SqlInitialCatalogConverter))]
        public string InitialCatalog {
            get { return _initialCatalog; }
            set {
                SetValue(DbConnectionStringKeywords.InitialCatalog, value);
                _initialCatalog = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.IntegratedSecurity)]
        [ResCategoryAttribute(Res.DataCategory_Security)]
        [ResDescriptionAttribute(Res.DbConnectionString_IntegratedSecurity)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool IntegratedSecurity {
            get { return _integratedSecurity; }
            set {
                SetValue(DbConnectionStringKeywords.IntegratedSecurity, value);
                _integratedSecurity = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.Authentication)]
        [ResCategoryAttribute(Res.DataCategory_Security)]
        [ResDescriptionAttribute(Res.DbConnectionString_Authentication)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public SqlAuthenticationMethod Authentication {
            get { return _authentication; }
            set {
                if (!DbConnectionStringBuilderUtil.IsValidAuthenticationTypeValue(value)) {
                    throw ADP.InvalidEnumerationValue(typeof(SqlAuthenticationMethod), (int)value);
                }

                SetAuthenticationValue(value);
                _authentication = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.LoadBalanceTimeout)]
        [ResCategoryAttribute(Res.DataCategory_Pooling)]
        [ResDescriptionAttribute(Res.DbConnectionString_LoadBalanceTimeout)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public int LoadBalanceTimeout {
            get { return _loadBalanceTimeout; }
            set {
                if (value < 0) {
                    throw ADP.InvalidConnectionOptionValue(DbConnectionStringKeywords.LoadBalanceTimeout);
                }
                SetValue(DbConnectionStringKeywords.LoadBalanceTimeout, value);
                _loadBalanceTimeout = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.MaxPoolSize)]
        [ResCategoryAttribute(Res.DataCategory_Pooling)]
        [ResDescriptionAttribute(Res.DbConnectionString_MaxPoolSize)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public int MaxPoolSize {
            get { return _maxPoolSize; }
            set {
                if (value < 1) {
                    throw ADP.InvalidConnectionOptionValue(DbConnectionStringKeywords.MaxPoolSize);
                }
                SetValue(DbConnectionStringKeywords.MaxPoolSize, value);
                _maxPoolSize = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.ConnectRetryCount)]
        [ResCategoryAttribute(Res.DataCategory_ConnectionResilency)]
        [ResDescriptionAttribute(Res.DbConnectionString_ConnectRetryCount)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public int ConnectRetryCount {
            get { return _connectRetryCount; }
            set {
                if ((value < 0) || (value>255)) {
                    throw ADP.InvalidConnectionOptionValue(DbConnectionStringKeywords.ConnectRetryCount);
                }
                SetValue(DbConnectionStringKeywords.ConnectRetryCount, value);
                _connectRetryCount = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.ConnectRetryInterval)]
        [ResCategoryAttribute(Res.DataCategory_ConnectionResilency)]
        [ResDescriptionAttribute(Res.DbConnectionString_ConnectRetryInterval)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public int ConnectRetryInterval {
            get { return _connectRetryInterval; }
            set {
                if ((value < 1) || (value > 60)) {
                    throw ADP.InvalidConnectionOptionValue(DbConnectionStringKeywords.ConnectRetryInterval);
                }
                SetValue(DbConnectionStringKeywords.ConnectRetryInterval, value);
                _connectRetryInterval = value;
            }
        }



        [DisplayName(DbConnectionStringKeywords.MinPoolSize)]
        [ResCategoryAttribute(Res.DataCategory_Pooling)]
        [ResDescriptionAttribute(Res.DbConnectionString_MinPoolSize)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public int MinPoolSize {
            get { return _minPoolSize; }
            set {
                if (value < 0) {
                    throw ADP.InvalidConnectionOptionValue(DbConnectionStringKeywords.MinPoolSize);
                }
                SetValue(DbConnectionStringKeywords.MinPoolSize, value);
                _minPoolSize = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.MultipleActiveResultSets)]
        [ResCategoryAttribute(Res.DataCategory_Advanced)]
        [ResDescriptionAttribute(Res.DbConnectionString_MultipleActiveResultSets)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool MultipleActiveResultSets {
            get { return _multipleActiveResultSets; }
            set {
                SetValue(DbConnectionStringKeywords.MultipleActiveResultSets, value);
                _multipleActiveResultSets = value;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Reviewed and Approved by UE")]
        [DisplayName(DbConnectionStringKeywords.MultiSubnetFailover)]
        [ResCategoryAttribute(Res.DataCategory_Source)]
        [ResDescriptionAttribute(Res.DbConnectionString_MultiSubnetFailover)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool MultiSubnetFailover {
            get { return _multiSubnetFailover; }
            set {
                SetValue(DbConnectionStringKeywords.MultiSubnetFailover, value);
                _multiSubnetFailover = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.TransparentNetworkIPResolution)]
        [ResCategoryAttribute(Res.DataCategory_Source)]
        [ResDescriptionAttribute(Res.DbConnectionString_TransparentNetworkIPResolution)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool TransparentNetworkIPResolution
        {
            get { return _transparentNetworkIPResolution; }
            set {
                SetValue(DbConnectionStringKeywords.TransparentNetworkIPResolution, value);
                _transparentNetworkIPResolution = value;
            }
        }
/*
        [DisplayName(DbConnectionStringKeywords.NamedConnection)]
        [ResCategoryAttribute(Res.DataCategory_NamedConnectionString)]
        [ResDescriptionAttribute(Res.DbConnectionString_NamedConnection)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        [TypeConverter(typeof(NamedConnectionStringConverter))]
        public string NamedConnection {
            get { return _namedConnection; }
            set {
                SetValue(DbConnectionStringKeywords.NamedConnection, value);
                _namedConnection = value;
            }
        }
*/
        [DisplayName(DbConnectionStringKeywords.NetworkLibrary)]
        [ResCategoryAttribute(Res.DataCategory_Advanced)]
        [ResDescriptionAttribute(Res.DbConnectionString_NetworkLibrary)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        [TypeConverter(typeof(NetworkLibraryConverter))]
        public string NetworkLibrary {
            get { return _networkLibrary; }
            set {
                if (null != value) {
                    switch(value.Trim().ToLower(CultureInfo.InvariantCulture)) {
                    case SqlConnectionString.NETLIB.AppleTalk:
                        value = SqlConnectionString.NETLIB.AppleTalk;
                        break;
                    case SqlConnectionString.NETLIB.BanyanVines:
                        value = SqlConnectionString.NETLIB.BanyanVines;
                        break;
                    case SqlConnectionString.NETLIB.IPXSPX:
                        value = SqlConnectionString.NETLIB.IPXSPX;
                        break;
                    case SqlConnectionString.NETLIB.Multiprotocol:
                        value = SqlConnectionString.NETLIB.Multiprotocol;
                        break;
                    case SqlConnectionString.NETLIB.NamedPipes:
                        value = SqlConnectionString.NETLIB.NamedPipes;
                        break;
                    case SqlConnectionString.NETLIB.SharedMemory:
                        value = SqlConnectionString.NETLIB.SharedMemory;
                        break;
                    case SqlConnectionString.NETLIB.TCPIP:
                        value = SqlConnectionString.NETLIB.TCPIP;
                        break;
                    case SqlConnectionString.NETLIB.VIA:
                        value = SqlConnectionString.NETLIB.VIA;
                        break;
                    default:
                        throw ADP.InvalidConnectionOptionValue(DbConnectionStringKeywords.NetworkLibrary);
                    }
                }
                SetValue(DbConnectionStringKeywords.NetworkLibrary, value);
                _networkLibrary = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.PacketSize)]
        [ResCategoryAttribute(Res.DataCategory_Advanced)]
        [ResDescriptionAttribute(Res.DbConnectionString_PacketSize)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public int PacketSize {
            get { return _packetSize; }
            set {
                if ((value < TdsEnums.MIN_PACKET_SIZE) || (TdsEnums.MAX_PACKET_SIZE < value)) {
                    throw SQL.InvalidPacketSizeValue();
                }
                SetValue(DbConnectionStringKeywords.PacketSize, value);
                _packetSize = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.Password)]
        [PasswordPropertyTextAttribute(true)]
        [ResCategoryAttribute(Res.DataCategory_Security)]
        [ResDescriptionAttribute(Res.DbConnectionString_Password)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public string Password {
            get { return _password; }
            set {
                SetValue(DbConnectionStringKeywords.Password, value);
                _password = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.PersistSecurityInfo)]
        [ResCategoryAttribute(Res.DataCategory_Security)]
        [ResDescriptionAttribute(Res.DbConnectionString_PersistSecurityInfo)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool PersistSecurityInfo {
            get { return _persistSecurityInfo; }
            set {
                SetValue(DbConnectionStringKeywords.PersistSecurityInfo, value);
                _persistSecurityInfo = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.Pooling)]
        [ResCategoryAttribute(Res.DataCategory_Pooling)]
        [ResDescriptionAttribute(Res.DbConnectionString_Pooling)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool Pooling {
            get { return _pooling; }
            set {
                SetValue(DbConnectionStringKeywords.Pooling, value);
                _pooling = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.Replication)]
        [ResCategoryAttribute(Res.DataCategory_Replication)]
        [ResDescriptionAttribute(Res.DbConnectionString_Replication )]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool Replication {
            get { return _replication; }
            set {
                SetValue(DbConnectionStringKeywords.Replication, value);
                _replication = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.TransactionBinding)]
        [ResCategoryAttribute(Res.DataCategory_Advanced)]
        [ResDescriptionAttribute(Res.DbConnectionString_TransactionBinding)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public string TransactionBinding {
            get { return _transactionBinding; }
            set {
                SetValue(DbConnectionStringKeywords.TransactionBinding, value);
                _transactionBinding = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.TypeSystemVersion)]
        [ResCategoryAttribute(Res.DataCategory_Advanced)]
        [ResDescriptionAttribute(Res.DbConnectionString_TypeSystemVersion)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public string TypeSystemVersion {
            get { return _typeSystemVersion; }
            set {
                SetValue(DbConnectionStringKeywords.TypeSystemVersion, value);
                _typeSystemVersion = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.UserID)]
        [ResCategoryAttribute(Res.DataCategory_Security)]
        [ResDescriptionAttribute(Res.DbConnectionString_UserID)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public string UserID {
            get { return _userID; }
            set {
                SetValue(DbConnectionStringKeywords.UserID, value);
                _userID = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.UserInstance)]
        [ResCategoryAttribute(Res.DataCategory_Source)]
        [ResDescriptionAttribute(Res.DbConnectionString_UserInstance)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public bool UserInstance {
            get { return _userInstance; }
            set {
                SetValue(DbConnectionStringKeywords.UserInstance, value);
                _userInstance = value;
            }
        }

        [DisplayName(DbConnectionStringKeywords.WorkstationID)]
        [ResCategoryAttribute(Res.DataCategory_Context)]
        [ResDescriptionAttribute(Res.DbConnectionString_WorkstationID)]
        [RefreshPropertiesAttribute(RefreshProperties.All)]
        public string WorkstationID {
            get { return _workstationID; }
            set {
                SetValue(DbConnectionStringKeywords.WorkstationID, value);
                _workstationID = value;
            }
        }

        public override bool IsFixedSize {
            get {
                return true;
            }
        }

        public override ICollection Keys {
            get {
                return new System.Data.Common.ReadOnlyCollection<string>(_validKeywords);
            }
        }

        public override ICollection Values {
            get {
                // written this way so if the ordering of Keywords & _validKeywords changes
                // this is one less place to maintain
                object[] values = new object[_validKeywords.Length];
                for(int i = 0; i < values.Length; ++i) {
                    values[i] = GetAt((Keywords)i);
                }
                return new System.Data.Common.ReadOnlyCollection<object>(values);
            }
        }

        public override void Clear() {
            base.Clear();
            for(int i = 0; i < _validKeywords.Length; ++i) {
                Reset((Keywords)i);
            }
        }

        public override bool ContainsKey(string keyword) {
            ADP.CheckArgumentNull(keyword, "keyword");
            return _keywords.ContainsKey(keyword);
        }

        private static bool ConvertToBoolean(object value) {
            return DbConnectionStringBuilderUtil.ConvertToBoolean(value);
        }
        private static int ConvertToInt32(object value) {
            return DbConnectionStringBuilderUtil.ConvertToInt32(value);
        }
        private static bool ConvertToIntegratedSecurity(object value) {
            return DbConnectionStringBuilderUtil.ConvertToIntegratedSecurity(value);
        }
        private static string ConvertToString(object value) {
            return DbConnectionStringBuilderUtil.ConvertToString(value);
        }
        private static ApplicationIntent ConvertToApplicationIntent(string keyword, object value) {
            return DbConnectionStringBuilderUtil.ConvertToApplicationIntent(keyword, value);
        }
        private static SqlAuthenticationMethod ConvertToAuthenticationType(string keyword, object value) {
            return DbConnectionStringBuilderUtil.ConvertToAuthenticationType(keyword, value);
        }
        private static PoolBlockingPeriod ConvertToPoolBlockingPeriod(string keyword, object value)
        {
            return DbConnectionStringBuilderUtil.ConvertToPoolBlockingPeriod(keyword, value);
        }

        /// <summary>
        /// Convert to SqlConnectionColumnEncryptionSetting.
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="value"></param>
        private static SqlConnectionColumnEncryptionSetting ConvertToColumnEncryptionSetting(string keyword, object value) {
            return DbConnectionStringBuilderUtil.ConvertToColumnEncryptionSetting(keyword, value);
        }

        internal override string ConvertValueToString(object value) {
            if (value is SqlAuthenticationMethod) {
                return DbConnectionStringBuilderUtil.AuthenticationTypeToString((SqlAuthenticationMethod)value);
            }
            else {
                return base.ConvertValueToString(value);
            }
        }

        private object GetAt(Keywords index) {
            switch(index) {
            case Keywords.ApplicationIntent:				return this.ApplicationIntent;
            case Keywords.ApplicationName:					return ApplicationName;
            case Keywords.AsynchronousProcessing:			return AsynchronousProcessing;
            case Keywords.AttachDBFilename:					return AttachDBFilename;
            case Keywords.PoolBlockingPeriod:         return PoolBlockingPeriod;
            case Keywords.ConnectTimeout:					return ConnectTimeout;
#pragma warning disable 618 // Obsolete ConnectionReset
            case Keywords.ConnectionReset:					return ConnectionReset;
#pragma warning restore 618
            case Keywords.ContextConnection:				return ContextConnection;
            case Keywords.CurrentLanguage:					return CurrentLanguage;
            case Keywords.DataSource:						return DataSource;
            case Keywords.Encrypt:							return Encrypt;
            case Keywords.Enlist:							return Enlist;
            case Keywords.FailoverPartner:					return FailoverPartner;
            case Keywords.InitialCatalog:					return InitialCatalog;
            case Keywords.IntegratedSecurity:				return IntegratedSecurity;
            case Keywords.LoadBalanceTimeout:				return LoadBalanceTimeout;
            case Keywords.MultipleActiveResultSets:			return MultipleActiveResultSets;
            case Keywords.MaxPoolSize:						return MaxPoolSize;
            case Keywords.MinPoolSize:						return MinPoolSize;
            case Keywords.MultiSubnetFailover:				return MultiSubnetFailover;
            case Keywords.TransparentNetworkIPResolution:	return TransparentNetworkIPResolution;
//          case Keywords.NamedConnection:					return NamedConnection;
            case Keywords.NetworkLibrary:					return NetworkLibrary;
            case Keywords.PacketSize:						return PacketSize;
            case Keywords.Password:							return Password;
            case Keywords.PersistSecurityInfo:				return PersistSecurityInfo;
            case Keywords.Pooling:							return Pooling;
            case Keywords.Replication:						return Replication;
            case Keywords.TransactionBinding:				return TransactionBinding;
            case Keywords.TrustServerCertificate:			return TrustServerCertificate;
            case Keywords.TypeSystemVersion:				return TypeSystemVersion;
            case Keywords.UserID:							return UserID;
            case Keywords.UserInstance:						return UserInstance;
            case Keywords.WorkstationID:					return WorkstationID;
            case Keywords.ConnectRetryCount:				return ConnectRetryCount;
            case Keywords.ConnectRetryInterval:				return ConnectRetryInterval;
            case Keywords.Authentication:					return Authentication;
            case Keywords.ColumnEncryptionSetting:			return ColumnEncryptionSetting;
            default:
                Debug.Assert(false, "unexpected keyword");
                throw ADP.KeywordNotSupported(_validKeywords[(int)index]);
            }
        }

        private Keywords GetIndex(string keyword) {
            ADP.CheckArgumentNull(keyword, "keyword");
            Keywords index;
            if (_keywords.TryGetValue(keyword, out index)) {
                return index;
            }
            throw ADP.KeywordNotSupported(keyword);            
        }

        protected override void GetProperties(Hashtable propertyDescriptors) {
            foreach(PropertyDescriptor reflected in TypeDescriptor.GetProperties(this, true)) {
                bool refreshOnChange = false;
                bool isReadonly = false;
                string displayName = reflected.DisplayName;

                // 'Password' & 'User ID' will be readonly if 'Integrated Security' is true
                if (DbConnectionStringKeywords.IntegratedSecurity == displayName) {
                    refreshOnChange = true;
                    isReadonly = reflected.IsReadOnly;
                }
                else if ((DbConnectionStringKeywords.Password == displayName) ||
                         (DbConnectionStringKeywords.UserID == displayName)) {
                     isReadonly = IntegratedSecurity;
                }
                else {
                    continue;
                }
                Attribute[] attributes = GetAttributesFromCollection(reflected.Attributes);
                DbConnectionStringBuilderDescriptor descriptor = new DbConnectionStringBuilderDescriptor(reflected.Name,
                        reflected.ComponentType, reflected.PropertyType, isReadonly, attributes);
                descriptor.RefreshOnChange = refreshOnChange;
                propertyDescriptors[displayName] = descriptor;
            }
            base.GetProperties(propertyDescriptors);
        }

        public override bool Remove(string keyword) {
            ADP.CheckArgumentNull(keyword, "keyword");
            Keywords index;
            if (_keywords.TryGetValue(keyword, out index)) {
                if (base.Remove(_validKeywords[(int)index])) {
                    Reset(index);
                    return true;
                }
            }
            return false;
        }

        private void Reset(Keywords index) {
            switch(index) {
            case Keywords.ApplicationIntent:
                _applicationIntent = DbConnectionStringDefaults.ApplicationIntent;
                break;
            case Keywords.ApplicationName:
                _applicationName = DbConnectionStringDefaults.ApplicationName;
                break;
            case Keywords.AsynchronousProcessing:
                _asynchronousProcessing = DbConnectionStringDefaults.AsynchronousProcessing;
                break;
            case Keywords.AttachDBFilename:
                _attachDBFilename = DbConnectionStringDefaults.AttachDBFilename;
                break;
            case Keywords.Authentication:
                _authentication = DbConnectionStringDefaults.Authentication;
                break;
            case Keywords.PoolBlockingPeriod:
                _poolBlockingPeriod = DbConnectionStringDefaults.PoolBlockingPeriod;
                break;
              
            case Keywords.ConnectTimeout:
                _connectTimeout = DbConnectionStringDefaults.ConnectTimeout;
                break;
            case Keywords.ConnectionReset:
                _connectionReset = DbConnectionStringDefaults.ConnectionReset;
                break;
            case Keywords.ContextConnection:
                _contextConnection = DbConnectionStringDefaults.ContextConnection;
                break;
            case Keywords.CurrentLanguage:
                _currentLanguage = DbConnectionStringDefaults.CurrentLanguage;
                break;
            case Keywords.DataSource:
                _dataSource = DbConnectionStringDefaults.DataSource;
                break;
            case Keywords.Encrypt:
                _encrypt = DbConnectionStringDefaults.Encrypt;
                break;
            case Keywords.Enlist:
                _enlist = DbConnectionStringDefaults.Enlist;
                break;
            case Keywords.FailoverPartner:
                _failoverPartner = DbConnectionStringDefaults.FailoverPartner;
                break;
            case Keywords.InitialCatalog:
                _initialCatalog = DbConnectionStringDefaults.InitialCatalog;
                break;
            case Keywords.IntegratedSecurity:
                _integratedSecurity = DbConnectionStringDefaults.IntegratedSecurity;
                break;
            case Keywords.LoadBalanceTimeout:
                _loadBalanceTimeout = DbConnectionStringDefaults.LoadBalanceTimeout;
                break;
            case Keywords.MultipleActiveResultSets:
                _multipleActiveResultSets = DbConnectionStringDefaults.MultipleActiveResultSets;
                break;
            case Keywords.MaxPoolSize:
                _maxPoolSize = DbConnectionStringDefaults.MaxPoolSize;
                break;
            case Keywords.MinPoolSize:
                _minPoolSize = DbConnectionStringDefaults.MinPoolSize;
                break;
            case Keywords.MultiSubnetFailover:
                _multiSubnetFailover = DbConnectionStringDefaults.MultiSubnetFailover;
                break;
            case Keywords.TransparentNetworkIPResolution:
                _transparentNetworkIPResolution = DbConnectionStringDefaults.TransparentNetworkIPResolution;
                    break;
//          case Keywords.NamedConnection:
//              _namedConnection = DbConnectionStringDefaults.NamedConnection;
//              break;
            case Keywords.NetworkLibrary:
                _networkLibrary = DbConnectionStringDefaults.NetworkLibrary;
                break;
            case Keywords.PacketSize:
                _packetSize = DbConnectionStringDefaults.PacketSize;
                break;
            case Keywords.Password:
                _password = DbConnectionStringDefaults.Password;
                break;
            case Keywords.PersistSecurityInfo:
                _persistSecurityInfo = DbConnectionStringDefaults.PersistSecurityInfo;
                break;
            case Keywords.Pooling:
                _pooling = DbConnectionStringDefaults.Pooling;
                break;
            case Keywords.ConnectRetryCount:
                _connectRetryCount = DbConnectionStringDefaults.ConnectRetryCount;
                break;
            case Keywords.ConnectRetryInterval:
                _connectRetryInterval = DbConnectionStringDefaults.ConnectRetryInterval;
                break;
            case Keywords.Replication:
                _replication = DbConnectionStringDefaults.Replication;
                break;
            case Keywords.TransactionBinding:
                _transactionBinding = DbConnectionStringDefaults.TransactionBinding;
                break;
            case Keywords.TrustServerCertificate:
                _trustServerCertificate = DbConnectionStringDefaults.TrustServerCertificate;
                break;
            case Keywords.TypeSystemVersion:
                _typeSystemVersion = DbConnectionStringDefaults.TypeSystemVersion;
                break;
            case Keywords.UserID:
                _userID = DbConnectionStringDefaults.UserID;
                break;
            case Keywords.UserInstance:
                _userInstance = DbConnectionStringDefaults.UserInstance;
                break;
            case Keywords.WorkstationID:
                _workstationID = DbConnectionStringDefaults.WorkstationID;
                break;
            case Keywords.ColumnEncryptionSetting:
                _columnEncryptionSetting = DbConnectionStringDefaults.ColumnEncryptionSetting;
                break;
            default:
                Debug.Assert(false, "unexpected keyword");
                throw ADP.KeywordNotSupported(_validKeywords[(int)index]);
            }
        }

        private void SetValue(string keyword, bool value) {
            base[keyword] = value.ToString((System.IFormatProvider)null);
        }
        private void SetValue(string keyword, int value) {
            base[keyword] = value.ToString((System.IFormatProvider)null);
        }
        private void SetValue(string keyword, string value) {
            ADP.CheckArgumentNull(value, keyword);
            base[keyword] = value;
        }
        private void SetApplicationIntentValue(ApplicationIntent value) {
            Debug.Assert(DbConnectionStringBuilderUtil.IsValidApplicationIntentValue(value), "Invalid value for ApplicationIntent");
            base[DbConnectionStringKeywords.ApplicationIntent] = DbConnectionStringBuilderUtil.ApplicationIntentToString(value);
        }
        private void SetPoolBlockingPeriodValue(PoolBlockingPeriod value)
        {
            Debug.Assert(DbConnectionStringBuilderUtil.IsValidPoolBlockingPeriodValue(value), "Invalid value for PoolBlockingPeriod");
            base[DbConnectionStringKeywords.PoolBlockingPeriod] = DbConnectionStringBuilderUtil.PoolBlockingPeriodToString(value);
        }
        private void SetAuthenticationValue(SqlAuthenticationMethod value) {
            Debug.Assert(DbConnectionStringBuilderUtil.IsValidAuthenticationTypeValue(value), "Invalid value for AuthenticationType");
            base[DbConnectionStringKeywords.Authentication] = DbConnectionStringBuilderUtil.AuthenticationTypeToString(value);
        }
        private void SetColumnEncryptionSettingValue(SqlConnectionColumnEncryptionSetting value) {
            Debug.Assert(DbConnectionStringBuilderUtil.IsValidColumnEncryptionSetting(value), "Invalid value for SqlConnectionColumnEncryptionSetting");
            base[DbConnectionStringKeywords.ColumnEncryptionSetting] = DbConnectionStringBuilderUtil.ColumnEncryptionSettingToString(value);
        }

        public override bool ShouldSerialize(string keyword) {
            ADP.CheckArgumentNull(keyword, "keyword");
            Keywords index;
            return _keywords.TryGetValue(keyword, out index) && base.ShouldSerialize(_validKeywords[(int)index]);
        }

        public override bool TryGetValue(string keyword, out object value) {
            Keywords index;
            if (_keywords.TryGetValue(keyword, out index)) {
                value = GetAt(index);
                return true;
            }
            value = null;
            return false;
        }

        private sealed class NetworkLibraryConverter : TypeConverter {
//            private const string AppleTalk     = "Apple Talk (DBMSADSN)";  Invalid protocals
//            private const string BanyanVines   = "Banyan VINES (DBMSVINN)";
//            private const string IPXSPX        = "NWLink IPX/SPX (DBMSSPXN)";
//            private const string Multiprotocol = "Multiprotocol (DBMSRPCN)";
            private const string NamedPipes    = "Named Pipes (DBNMPNTW)";   // valid protocols
            private const string SharedMemory  = "Shared Memory (DBMSLPCN)";
            private const string TCPIP         = "TCP/IP (DBMSSOCN)";
            private const string VIA           = "VIA (DBMSGNET)";

            // these are correctly non-static, property grid will cache an instance
            private StandardValuesCollection _standardValues;

            // converter classes should have public ctor
            public NetworkLibraryConverter() {
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
                // Only know how to convert from a string
                return ((typeof(string) == sourceType) || base.CanConvertFrom(context, sourceType));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
                string svalue = (value as string);
                if (null != svalue) {
                    svalue = svalue.Trim();
                    if (StringComparer.OrdinalIgnoreCase.Equals(svalue, NamedPipes)) {
                        return SqlConnectionString.NETLIB.NamedPipes;
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Equals(svalue, SharedMemory)) {
                        return SqlConnectionString.NETLIB.SharedMemory;
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Equals(svalue, TCPIP)) {
                        return SqlConnectionString.NETLIB.TCPIP;
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Equals(svalue, VIA)) {
                        return SqlConnectionString.NETLIB.VIA;
                    }
                    else {
                        return svalue;
                    }
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
                return ((typeof(string) == destinationType) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
                string svalue = (value as string);
                if ((null != svalue) && (destinationType == typeof(string))) {
                    switch(svalue.Trim().ToLower(CultureInfo.InvariantCulture)) {
                    case SqlConnectionString.NETLIB.NamedPipes:
                        return NamedPipes;
                    case SqlConnectionString.NETLIB.SharedMemory:
                        return SharedMemory;
                    case SqlConnectionString.NETLIB.TCPIP:
                        return TCPIP;
                    case SqlConnectionString.NETLIB.VIA:
                        return VIA;
                    default:
                        return svalue;
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
                return false;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {

                SqlConnectionStringBuilder constr = null;
                if (null != context) {
                    constr = (context.Instance as SqlConnectionStringBuilder);
                }

                StandardValuesCollection standardValues = _standardValues;
                if (null == standardValues) {
                    string[] names = new string[] {
                        NamedPipes,
                        SharedMemory,
                        TCPIP,
                        VIA,
                    };
                    standardValues = new StandardValuesCollection(names);
                    _standardValues = standardValues;
                }
                return standardValues;
            }
        }

        private sealed class SqlDataSourceConverter : StringConverter {

            private StandardValuesCollection _standardValues;

            // converter classes should have public ctor
            public SqlDataSourceConverter() {
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
                return false;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                StandardValuesCollection dataSourceNames = _standardValues;
                if (null == _standardValues) {
                    // Get the sources rowset for the SQLOLEDB enumerator
                    DataTable table = SqlClientFactory.Instance.CreateDataSourceEnumerator().GetDataSources();
                    DataColumn serverName = table.Columns[System.Data.Sql.SqlDataSourceEnumerator.ServerName];
                    DataColumn instanceName = table.Columns[System.Data.Sql.SqlDataSourceEnumerator.InstanceName];
                    DataRowCollection rows = table.Rows;

                    string[] serverNames = new string[rows.Count];
                    for(int i = 0; i < serverNames.Length; ++i) {
                        string server   = rows[i][serverName] as string;
                        string instance = rows[i][instanceName] as string;
                        if ((null == instance) || (0 == instance.Length) || ("MSSQLSERVER" == instance)) {
                            serverNames[i] = server;
                        }
                        else {
                            serverNames[i] = server + @"\" + instance;
                        }
                    }
                    Array.Sort<string>(serverNames);

                    // Create the standard values collection that contains the sources
                    dataSourceNames = new StandardValuesCollection(serverNames);
                    _standardValues = dataSourceNames;
                }
                return dataSourceNames;
            }
        }

        private sealed class SqlInitialCatalogConverter : StringConverter {

            // converter classes should have public ctor
            public SqlInitialCatalogConverter() {
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
                return GetStandardValuesSupportedInternal(context);
            }

            private bool GetStandardValuesSupportedInternal(ITypeDescriptorContext context) {
                // Only say standard values are supported if the connection string has enough
                // information set to instantiate a connection and retrieve a list of databases
                bool flag = false;
                if (null != context) {
                    SqlConnectionStringBuilder constr = (context.Instance as SqlConnectionStringBuilder);
                    if (null != constr) {
                        if ((0 < constr.DataSource.Length) && (constr.IntegratedSecurity || (0 < constr.UserID.Length))) {
                            flag = true;
                        }
                    }
                }
                return flag;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
                // Although theoretically this could be true, some people may want to just type in a name
                return false;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                // There can only be standard values if the connection string is in a state that might
                // be able to instantiate a connection
                if (GetStandardValuesSupportedInternal(context)) {

                    // Create an array list to store the database names
                    List<string> values = new List<string>();

                    try {
                        SqlConnectionStringBuilder constr = (SqlConnectionStringBuilder)context.Instance;

                        // Create a connection
                        using(SqlConnection connection = new SqlConnection()) {

                            // Create a basic connection string from current property values
                            connection.ConnectionString = constr.ConnectionString;

                            // Try to open the connection
                            connection.Open();

                            DataTable databaseTable = connection.GetSchema("DATABASES");

                            foreach (DataRow row in databaseTable.Rows) {
                                string dbName = (string)row["database_name"];
                                values.Add(dbName);
                            }
                        }
                    }
                    catch(SqlException e) {
                        ADP.TraceExceptionWithoutRethrow(e);
                        // silently fail
                    }

                    // Return values as a StandardValuesCollection
                    return new StandardValuesCollection(values);
                }
                return null;
            }
        }

        sealed internal class SqlConnectionStringBuilderConverter : ExpandableObjectConverter {

            // converter classes should have public ctor
            public SqlConnectionStringBuilderConverter() {
            }

            override public bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
                if (typeof(System.ComponentModel.Design.Serialization.InstanceDescriptor) == destinationType) {
                    return true;
                }
                return base.CanConvertTo(context, destinationType);
            }

            override public object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (destinationType == null) {
                    throw ADP.ArgumentNull("destinationType");
                }
                if (typeof(System.ComponentModel.Design.Serialization.InstanceDescriptor) == destinationType) {
                    SqlConnectionStringBuilder obj = (value as SqlConnectionStringBuilder);
                    if (null != obj) {
                        return ConvertToInstanceDescriptor(obj);
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            private System.ComponentModel.Design.Serialization.InstanceDescriptor ConvertToInstanceDescriptor(SqlConnectionStringBuilder options) {
                Type[] ctorParams = new Type[] { typeof(string) };
                object[] ctorValues = new object[] { options.ConnectionString };
                System.Reflection.ConstructorInfo ctor = typeof(SqlConnectionStringBuilder).GetConstructor(ctorParams);
                return new System.ComponentModel.Design.Serialization.InstanceDescriptor(ctor, ctorValues);
            }
        }
    }

}

