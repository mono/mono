//------------------------------------------------------------------------------
// <copyright file="TdsParserHelperClasses.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Security;
    using System.Globalization;

    using Microsoft.SqlServer.Server; // for SMI metadata

    internal enum CallbackType {
        Read  = 0,
        Write = 1
    }

    internal enum EncryptionOptions {
        OFF,
        ON,
        NOT_SUP,
        REQ,
        LOGIN
    }

    internal enum PreLoginHandshakeStatus {
        Successful,
        InstanceFailure
    }

    internal enum PreLoginOptions {
        VERSION,
        ENCRYPT,
        INSTANCE,
        THREADID,
        MARS,
        TRACEID,
        FEDAUTHREQUIRED,
        NUMOPT,
        LASTOPT = 255
    }

    internal enum RunBehavior {
        UntilDone         =  1, // 0001 binary
        ReturnImmediately =  2, // 0010 binary
        Clean             =  5, // 0101 binary - Clean AND UntilDone
        Attention         = 13  // 1101 binary - Clean AND UntilDone AND Attention
    }

    internal enum TdsParserState {
        Closed,
        OpenNotLoggedIn,
        OpenLoggedIn,
        Broken,
    }

    /// <summary>
    /// Struct encapsulating the data to be sent to the server as part of Federated Authentication Feature Extension.
    /// </summary>
    internal struct FederatedAuthenticationFeatureExtensionData
    {
        internal TdsEnums.FedAuthLibrary libraryType;
        internal bool fedAuthRequiredPreLoginResponse;
        internal SqlAuthenticationMethod authentication;
        internal byte[] accessToken;
    }

    /// <summary>
    /// <para> Represents a single encrypted value for a CEK. It contains the encrypted CEK,
    ///  the store type, name,the key path and encryption algorithm.</para>
    /// </summary>
    internal struct SqlEncryptionKeyInfo {
        internal byte[] encryptedKey; // the encrypted "column encryption key"
        internal int databaseId;
        internal int cekId;
        internal int cekVersion;
        internal byte[] cekMdVersion;
        internal string keyPath;
        internal string keyStoreName;
        internal string algorithmName;
        internal byte normalizationRuleVersion;
    }

    /// <summary>
    /// <para> Encapsulates one entry in the CipherInfo table sent as part of Colmetadata.
    /// The same CEK is encrypted multiple times with different master keys (for master key
    /// rotation scenario) We need to keep all these around until we can resolve the CEK
    /// using the correct master key.</para>
    /// </summary>
    internal struct SqlTceCipherInfoEntry {

        /// <summary>
        /// List of Column Encryption Key Information.
        /// </summary>
        private readonly List<SqlEncryptionKeyInfo> _columnEncryptionKeyValues;

        /// <summary>
        /// Key Ordinal.
        /// </summary>
        private readonly int _ordinal;

        /// <summary>
        /// Database ID
        /// </summary>
        private int _databaseId;

        /// <summary>
        /// Cek ID
        /// </summary>
        private int _cekId;

        /// <summary>
        /// Cek Version
        /// </summary>
        private int _cekVersion;

        /// <summary>
        /// Cek MD Version
        /// </summary>
        private byte[] _cekMdVersion;

        /// <summary>
        /// Return the ordinal.
        /// </summary>
        internal int Ordinal {
            get {
                return _ordinal;
            }
        }

        /// <summary>
        /// Return the DatabaseID.
        /// </summary>
        internal int DatabaseId {
            get {
                return _databaseId;
            }
        }

        /// <summary>
        /// Return the CEK ID.
        /// </summary>
        internal int CekId {
            get {
                return _cekId;
            }
        }

        /// <summary>
        /// Return the CEK Version.
        /// </summary>
        internal int CekVersion {
            get {
                return _cekVersion;
            }
        }

        /// <summary>
        /// Return the CEK MD Version.
        /// </summary>
        internal byte[] CekMdVersion {
            get {
                return _cekMdVersion;
            }
        }

        /// <summary>
        /// Return the list of Column Encryption Key Values.
        /// </summary>
        internal List<SqlEncryptionKeyInfo> ColumnEncryptionKeyValues {
            get {
                return _columnEncryptionKeyValues;
            }
        }

        /// <summary>
        /// Add an entry to the list of ColumnEncryptionKeyValues.
        /// </summary>
        /// <param name="encryptedKey"></param>
        /// <param name="databaseId"></param>
        /// <param name="cekId"></param>
        /// <param name="cekVersion"></param>
        /// <param name="cekMdVersion"></param>
        /// <param name="keyPath"></param>
        /// <param name="keyStoreName"></param>
        /// <param name="algorithmName"></param>
        internal void Add(byte[] encryptedKey, int databaseId, int cekId, int cekVersion, byte[] cekMdVersion, string keyPath, string keyStoreName, string algorithmName) {

            Debug.Assert(_columnEncryptionKeyValues != null, "_columnEncryptionKeyValues should already be initialized.");

            SqlEncryptionKeyInfo encryptionKey = new SqlEncryptionKeyInfo();
            encryptionKey.encryptedKey = encryptedKey;
            encryptionKey.databaseId = databaseId;
            encryptionKey.cekId = cekId;
            encryptionKey.cekVersion = cekVersion;
            encryptionKey.cekMdVersion = cekMdVersion;
            encryptionKey.keyPath = keyPath;
            encryptionKey.keyStoreName = keyStoreName;
            encryptionKey.algorithmName = algorithmName;
            _columnEncryptionKeyValues.Add(encryptionKey);

            if (0 == _databaseId) {
                _databaseId = databaseId;
                _cekId = cekId;
                _cekVersion = cekVersion;
                _cekMdVersion = cekMdVersion;
            }
            else {
                Debug.Assert(_databaseId == databaseId);
                Debug.Assert(_cekId == cekId);
                Debug.Assert(_cekVersion == cekVersion);
                Debug.Assert (_cekMdVersion != null && cekMdVersion != null && _cekMdVersion.Length == _cekMdVersion.Length);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ordinal"></param>
        internal SqlTceCipherInfoEntry(int ordinal = 0) : this() {
            _ordinal = ordinal;
            _databaseId = 0;
            _cekId = 0;
            _cekVersion = 0;
            _cekMdVersion = null;
            _columnEncryptionKeyValues = new List<SqlEncryptionKeyInfo>();
        }
    }

    /// <summary> 
    /// <para> Represents a table with various CEKs used in a resultset. Each entry corresponds to one (unique) CEK. The CEK
    /// may have been encrypted using multiple master keys (giving us multiple CEK values). All these values form one single
    /// entry in this table.</para>
    ///</summary>
    internal struct SqlTceCipherInfoTable {
        private readonly SqlTceCipherInfoEntry [] keyList;

        internal SqlTceCipherInfoTable (int tabSize) {
            Debug.Assert (0 < tabSize, "Invalid Table Size");
            keyList = new SqlTceCipherInfoEntry[tabSize];
        }

        internal SqlTceCipherInfoEntry this [int index] {
            get {
                Debug.Assert (index < keyList.Length, "Invalid index specified.");
                return keyList[index];
            }
            set {
                Debug.Assert (index < keyList.Length, "Invalid index specified.");
                keyList[index] = value;
            }
        }

        internal int Size {
            get {
                return keyList.Length;
            }
        }
    }

    sealed internal class SqlCollation {
        // First 20 bits of info field represent the lcid, bits 21-25 are compare options
        private const uint IgnoreCase     = 1 << 20; // bit 21 - IgnoreCase
        private const uint IgnoreNonSpace = 1 << 21; // bit 22 - IgnoreNonSpace / IgnoreAccent
        private const uint IgnoreWidth    = 1 << 22; // bit 23 - IgnoreWidth
        private const uint IgnoreKanaType = 1 << 23; // bit 24 - IgnoreKanaType
        private const uint BinarySort     = 1 << 24; // bit 25 - BinarySort

        internal const uint MaskLcid           = 0xfffff;
        private const int LcidVersionBitOffset = 28;
        private const uint MaskLcidVersion     = unchecked((uint)(0xf << LcidVersionBitOffset));
        private const uint MaskCompareOpt = IgnoreCase | IgnoreNonSpace | IgnoreWidth | IgnoreKanaType | BinarySort;

        internal uint info;
        internal byte sortId;

        static int FirstSupportedCollationVersion(int lcid)
        {
            // NOTE: switch-case works ~3 times faster in this case than search with Dictionary
            switch (lcid)
            {
                case 1044: return 2; // Norwegian_100_BIN
                case 1047: return 2; // Romansh_100_BIN
                case 1056: return 2; // Urdu_100_BIN
                case 1065: return 2; // Persian_100_BIN
                case 1068: return 2; // Azeri_Latin_100_BIN
                case 1070: return 2; // Upper_Sorbian_100_BIN
                case 1071: return 1; // ----n_FYROM_90_BIN
                case 1081: return 1; // Indic_General_90_BIN
                case 1082: return 2; // Maltese_100_BIN
                case 1083: return 2; // Sami_Norway_100_BIN
                case 1087: return 1; // Kazakh_90_BIN
                case 1090: return 2; // Turkmen_100_BIN
                case 1091: return 1; // Uzbek_Latin_90_BIN
                case 1092: return 1; // Tatar_90_BIN
                case 1093: return 2; // Bengali_100_BIN
                case 1101: return 2; // Assamese_100_BIN
                case 1105: return 2; // Tibetan_100_BIN
                case 1106: return 2; // Welsh_100_BIN
                case 1107: return 2; // Khmer_100_BIN
                case 1108: return 2; // Lao_100_BIN
                case 1114: return 1; // Syriac_90_BIN
                case 1121: return 2; // Nepali_100_BIN
                case 1122: return 2; // Frisian_100_BIN
                case 1123: return 2; // Pashto_100_BIN
                case 1125: return 1; // Divehi_90_BIN
                case 1133: return 2; // Bashkir_100_BIN
                case 1146: return 2; // Mapudungan_100_BIN
                case 1148: return 2; // Mohawk_100_BIN
                case 1150: return 2; // Breton_100_BIN
                case 1152: return 2; // Uighur_100_BIN
                case 1153: return 2; // Maori_100_BIN
                case 1155: return 2; // Corsican_100_BIN
                case 1157: return 2; // Yakut_100_BIN
                case 1164: return 2; // Dari_100_BIN
                case 2074: return 2; // Serbian_Latin_100_BIN
                case 2092: return 2; // Azeri_Cyrillic_100_BIN
                case 2107: return 2; // Sami_Sweden_Finland_100_BIN
                case 2143: return 2; // Tamazight_100_BIN
                case 3076: return 1; // Chinese_Hong_Kong_Stroke_90_BIN
                case 3098: return 2; // Serbian_Cyrillic_100_BIN
                case 5124: return 2; // Chinese_Traditional_Pinyin_100_BIN
                case 5146: return 2; // Bosnian_Latin_100_BIN
                case 8218: return 2; // Bosnian_Cyrillic_100_BIN

                default: return 0;   // other LCIDs have collation with version 0
            }
        }

        internal int LCID {
            // First 20 bits of info field represent the lcid
            get {
                return unchecked((int)(info & MaskLcid));
            }
            set {
                int lcid = value & (int)MaskLcid;
                Debug.Assert(lcid == value, "invalid set_LCID value");

                // VSTFDEVDIV 479474: some new Katmai LCIDs do not have collation with version = 0
                // since user has no way to specify collation version, we set the first (minimal) supported version for these collations
                int versionBits = FirstSupportedCollationVersion(lcid) << LcidVersionBitOffset;
                Debug.Assert((versionBits & MaskLcidVersion) == versionBits, "invalid version returned by FirstSupportedCollationVersion");

                // combine the current compare options with the new locale ID and its first supported version
                info = (info & MaskCompareOpt) | unchecked((uint)lcid) | unchecked((uint)versionBits);
            }
        }

        internal SqlCompareOptions SqlCompareOptions {
            get {
                SqlCompareOptions options = SqlCompareOptions.None;
                if (0 != (info & IgnoreCase))
                    options |= SqlCompareOptions.IgnoreCase;
                if (0 != (info & IgnoreNonSpace))
                    options |= SqlCompareOptions.IgnoreNonSpace;
                if (0 != (info & IgnoreWidth))
                    options |= SqlCompareOptions.IgnoreWidth;
                if (0 != (info & IgnoreKanaType))
                    options |= SqlCompareOptions.IgnoreKanaType;
                if (0 != (info & BinarySort))
                    options |= SqlCompareOptions.BinarySort;
                return options;
            }
            set {
                Debug.Assert((value & SqlString.x_iValidSqlCompareOptionMask) == value, "invalid set_SqlCompareOptions value");
                uint tmp = 0;
                if (0 != (value & SqlCompareOptions.IgnoreCase))
                    tmp |= IgnoreCase;
                if (0 != (value & SqlCompareOptions.IgnoreNonSpace))
                    tmp |= IgnoreNonSpace;
                if (0 != (value & SqlCompareOptions.IgnoreWidth))
                    tmp |= IgnoreWidth;
                if (0 != (value & SqlCompareOptions.IgnoreKanaType))
                    tmp |= IgnoreKanaType;
                if (0 != (value & SqlCompareOptions.BinarySort))
                    tmp |= BinarySort;
                info = (info & MaskLcid) | tmp;
            }
        }

        internal string TraceString() {
            return String.Format(/*IFormatProvider*/ null, "(LCID={0}, Opts={1})", this.LCID, (int)this.SqlCompareOptions);
        }

        static internal bool AreSame(SqlCollation a, SqlCollation b) {
            if (a == null || b == null) {
                return a == b;
            }
            else {
                return a.info == b.info && a.sortId == b.sortId;
            }

        }
      
    }

    internal class RoutingInfo {
        internal byte Protocol { get; private set; }
        internal UInt16 Port { get; private set; }
        internal string ServerName { get; private set; }

        internal RoutingInfo(byte protocol, UInt16 port, string servername) {
            Protocol = protocol;
            Port = port;
            ServerName = servername;
        }
    }

    sealed internal class SqlEnvChange {
        internal byte         type;
        internal byte         oldLength;
        internal int          newLength; // 7206 TDS changes makes this length an int
        internal int          length;
        internal string       newValue;
        internal string       oldValue;
        internal byte[]       newBinValue;
        internal byte[]       oldBinValue;
        internal long         newLongValue;
        internal long         oldLongValue;
        internal SqlCollation newCollation;
        internal SqlCollation oldCollation;
        internal RoutingInfo  newRoutingInfo;
    }

    sealed internal class SqlLogin {
        internal SqlAuthenticationMethod authentication = SqlAuthenticationMethod.NotSpecified;               // Authentication type
        internal int timeout;                                                       // login timeout
        internal bool   userInstance     = false;                                   // user instance
        internal string hostName         = "";                                      // client machine name
        internal string userName         = "";                                      // user id
        internal string password         = "";                                      // password
        internal string applicationName  = "";                                      // application name
        internal string serverName       = "";                                      // server name
        internal string language         = "";                                      // initial language
        internal string database         = "";                                      // initial database
        internal string attachDBFilename = "";                                      // DB filename to be attached
        internal string newPassword      = "";                                      // new password for reset password
        internal bool   useReplication   = false;                                   // user login for replication
        internal bool   useSSPI          = false;                                   // use integrated security
        internal int    packetSize       = SqlConnectionString.DEFAULT.Packet_Size; // packet size
        internal bool   readOnlyIntent   = false;                                   // read-only intent
        internal SqlCredential credential;                                          // user id and password in SecureString
        internal SecureString newSecurePassword;                                    // new password in SecureString for resetting pasword
    }

    sealed internal class SqlLoginAck {
        internal string programName;
        internal byte   majorVersion;
        internal byte   minorVersion;
        internal short  buildNum;
        internal bool   isVersion8;
        internal UInt32 tdsVersion;
    }

    sealed internal class SqlFedAuthInfo {
        internal string spn;
        internal string stsurl;
        public override string ToString() {
            return String.Format(CultureInfo.InvariantCulture, "STSURL: {0}, SPN: {1}", stsurl ?? String.Empty, spn ?? String.Empty);
        }
    }

    sealed internal class SqlFedAuthToken {
        internal UInt32 dataLen;
        internal byte[] accessToken;
        internal long expirationFileTime;
    }

    sealed internal class _SqlMetaData : SqlMetaDataPriv, ICloneable {

        internal string             column;
        internal string             baseColumn;
        internal MultiPartTableName multiPartTableName;
        internal readonly int       ordinal;
        internal byte               updatability;     // two bit field (0 is read only, 1 is updatable, 2 is updatability unknown)
        internal byte               tableNum;
        internal bool               isDifferentName;
        internal bool               isKey;
        internal bool               isHidden;
        internal bool               isExpression;
        internal bool               isIdentity;
        internal bool               isColumnSet;
        internal byte               op;       // for altrow-columns only
        internal ushort             operand;  // for altrow-columns only

        internal _SqlMetaData(int ordinal) : base() {
            this.ordinal = ordinal;
        }

        internal string serverName {
            get {
                return multiPartTableName.ServerName;
            }
        }
        internal string catalogName {
            get {
                return multiPartTableName.CatalogName;
            }
        }
        internal string schemaName {
            get {
                return multiPartTableName.SchemaName;
            }
        }
        internal string tableName {
            get {
                return multiPartTableName.TableName;
            }
        }

        internal bool IsNewKatmaiDateTimeType {
            get {
                return SqlDbType.Date == type || SqlDbType.Time == type || SqlDbType.DateTime2 == type || SqlDbType.DateTimeOffset == type;
            }
        }

        internal bool IsLargeUdt {
            get {
                return type == SqlDbType.Udt && length == Int32.MaxValue;
            }
        }

        public object Clone() {
            _SqlMetaData result = new _SqlMetaData(ordinal);
            result.CopyFrom(this);
            result.column = column;
            result.baseColumn = baseColumn;
            result.multiPartTableName = multiPartTableName;
            result.updatability = updatability;
            result.tableNum = tableNum;
            result.isDifferentName = isDifferentName;
            result.isKey = isKey;
            result.isHidden = isHidden;
            result.isExpression = isExpression;
            result.isIdentity = isIdentity;
            result.isColumnSet = isColumnSet;
            result.op = op;
            result.operand = operand;
            return result;
        }
    }

    sealed internal class _SqlMetaDataSet : ICloneable {
        internal ushort         id;             // for altrow-columns only
        internal int[]          indexMap;
        internal int            visibleColumns;
        internal DataTable      schemaTable;
        internal readonly SqlTceCipherInfoTable? cekTable; // table of "column encryption keys" used for this metadataset
        internal readonly _SqlMetaData[] metaDataArray;

        internal _SqlMetaDataSet(int count, SqlTceCipherInfoTable? cipherTable) {
            cekTable = cipherTable;
            metaDataArray = new _SqlMetaData[count];
            for(int i = 0; i < metaDataArray.Length; ++i) {
                metaDataArray[i] = new _SqlMetaData(i);
            }
        }

        private _SqlMetaDataSet(_SqlMetaDataSet original) {
            this.id = original.id;
            // although indexMap is not immutable, in practice it is initialized once and then passed around
            this.indexMap = original.indexMap;
            this.visibleColumns = original.visibleColumns;
            this.schemaTable = original.schemaTable;
            if (original.metaDataArray == null) {
                metaDataArray = null;
            }
            else {
                metaDataArray = new _SqlMetaData[original.metaDataArray.Length];
                for (int idx=0; idx<metaDataArray.Length; idx++) {
                    metaDataArray[idx] = (_SqlMetaData)original.metaDataArray[idx].Clone();
                }
            }
        }

        internal int Length {
            get {
                return metaDataArray.Length;
            }
        }

        internal _SqlMetaData this [int index] {
            get {
                return metaDataArray[index];
            }
            set {
                Debug.Assert(null == value, "used only by SqlBulkCopy");
                metaDataArray[index] = value;
            }
        }

        public object Clone() {
            return new _SqlMetaDataSet(this);
        }
    }

    sealed internal class _SqlMetaDataSetCollection : ICloneable {
        private readonly List<_SqlMetaDataSet> altMetaDataSetArray;
        internal _SqlMetaDataSet metaDataSet;

        internal _SqlMetaDataSetCollection () {
            altMetaDataSetArray = new List<_SqlMetaDataSet>();
        }

        internal void SetAltMetaData(_SqlMetaDataSet altMetaDataSet) {
            // VSTFDEVDIV 479675: if altmetadata with same id is found, override it rather than adding a new one
            int newId = altMetaDataSet.id;
            for (int i = 0; i < altMetaDataSetArray.Count; i++) {
                if (altMetaDataSetArray[i].id == newId) {
                    // override the existing metadata with the same id
                    altMetaDataSetArray[i] = altMetaDataSet;
                    return;
                }
            }

            // if we did not find metadata to override, add as new
            altMetaDataSetArray.Add(altMetaDataSet);
        }

        internal _SqlMetaDataSet GetAltMetaData(int id) {
            foreach (_SqlMetaDataSet altMetaDataSet in altMetaDataSetArray) {
                if (altMetaDataSet.id == id) {
                    return altMetaDataSet;
                }
            }
            Debug.Assert (false, "Can't match up altMetaDataSet with given id");
            return null;
        }

        public object Clone()
        {
            _SqlMetaDataSetCollection result = new _SqlMetaDataSetCollection();
            result.metaDataSet = metaDataSet == null ? null : (_SqlMetaDataSet)metaDataSet.Clone();
            foreach (_SqlMetaDataSet set in altMetaDataSetArray) {
                result.altMetaDataSetArray.Add((_SqlMetaDataSet)set.Clone());
            }
            return result;
        }
    }

    /// <summary>
    /// Represents Encryption related information of the cipher data.
    /// </summary>
    internal class SqlCipherMetadata {

        /// <summary>
        /// Cipher Info Entry.
        /// </summary>
        private SqlTceCipherInfoEntry? _sqlTceCipherInfoEntry;

        /// <summary>
        /// Encryption Algorithm Id.
        /// </summary>
        private readonly byte _cipherAlgorithmId;

        /// <summary>
        /// Encryption Algorithm Name.
        /// </summary>
        private readonly string _cipherAlgorithmName;

        /// <summary>
        /// Encryption Type.
        /// </summary>
        private readonly byte _encryptionType;

        /// <summary>
        /// Normalization Rule Version.
        /// </summary>
        private readonly byte _normalizationRuleVersion;
        
        /// <summary>
        /// Encryption Algorithm Handle.
        /// </summary>
        private SqlClientEncryptionAlgorithm _sqlClientEncryptionAlgorithm;

        /// <summary>
        /// Sql Encryption Key Info.
        /// </summary>
        private SqlEncryptionKeyInfo? _sqlEncryptionKeyInfo;

        /// <summary>
        /// Ordinal (into the Cek Table).
        /// </summary>
        private readonly ushort _ordinal;

        /// <summary>
        /// Return the Encryption Info Entry.
        /// </summary>
        internal SqlTceCipherInfoEntry? EncryptionInfo {
            get {
                return _sqlTceCipherInfoEntry;
            }
            set {
                Debug.Assert(!_sqlTceCipherInfoEntry.HasValue, "We can only set the EncryptionInfo once.");
                _sqlTceCipherInfoEntry = value;
            }
        }

        /// <summary>
        /// Return the cipher's encryption algorithm id.
        /// </summary>
        internal byte CipherAlgorithmId {
            get {
                return _cipherAlgorithmId;
            }
        }

        /// <summary>
        /// Return the cipher's encryption algorithm name (could be null).
        /// </summary>
        internal string CipherAlgorithmName {
            get {
                return _cipherAlgorithmName;
            }
        }

        /// <summary>
        /// Return EncryptionType (Deterministic, Randomized, etc.)
        /// </summary>
        internal byte EncryptionType {
            get {
                return _encryptionType;
            }
        }

        /// <summary>
        /// Return normalization rule version.
        /// </summary>
        internal byte NormalizationRuleVersion {
            get {
                return _normalizationRuleVersion;
            }
        }

        /// <summary>
        /// Return the cipher encyrption algorithm handle.
        /// </summary>
        internal SqlClientEncryptionAlgorithm CipherAlgorithm {
            get {
                return _sqlClientEncryptionAlgorithm;
            }
            set {
                Debug.Assert(_sqlClientEncryptionAlgorithm == null, "_sqlClientEncryptionAlgorithm should not be set more than once.");
                 _sqlClientEncryptionAlgorithm = value;
            }
        }

        /// <summary>
        /// Return Encryption Key Info.
        /// </summary>
        internal SqlEncryptionKeyInfo? EncryptionKeyInfo {
            get {
                return _sqlEncryptionKeyInfo;
            }

            set {
                Debug.Assert(!_sqlEncryptionKeyInfo.HasValue, "_sqlEncryptionKeyInfo should not be set more than once.");
                _sqlEncryptionKeyInfo = value;
            }
        }

        /// <summary>
        /// Return Ordinal into Cek Table.
        /// </summary>
        internal ushort CekTableOrdinal {
            get {
                return _ordinal;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sqlTceCipherInfoEntry"></param>
        /// <param name="sqlClientEncryptionAlgorithm"></param>
        /// <param name="cipherAlgorithmId"></param>
        /// <param name="encryptionType"></param>
        /// <param name="normalizationRuleVersion"></param>
        internal SqlCipherMetadata (SqlTceCipherInfoEntry? sqlTceCipherInfoEntry,
                                    ushort ordinal,
                                    byte cipherAlgorithmId,
                                    string cipherAlgorithmName,
                                    byte encryptionType,
                                    byte normalizationRuleVersion) {
            Debug.Assert(!sqlTceCipherInfoEntry.Equals(default(SqlTceCipherInfoEntry)), "sqlTceCipherInfoEntry should not be un-initialized.");

            _sqlTceCipherInfoEntry = sqlTceCipherInfoEntry;
            _ordinal = ordinal;
            _cipherAlgorithmId = cipherAlgorithmId;
            _cipherAlgorithmName = cipherAlgorithmName;
            _encryptionType = encryptionType;
            _normalizationRuleVersion = normalizationRuleVersion;
            _sqlEncryptionKeyInfo = null;
        }

        /// <summary>
        /// Do we have an handle to the cipher encryption algorithm already ?
        /// </summary>
        /// <returns></returns>
        internal bool IsAlgorithmInitialized() {
            return (null != _sqlClientEncryptionAlgorithm) ? true : false;
        }
    }

    internal class SqlMetaDataPriv {
        internal SqlDbType    type;    // SqlDbType enum value
        internal byte         tdsType; // underlying tds type
        internal byte         precision = TdsEnums.UNKNOWN_PRECISION_SCALE; // give default of unknown (-1)
        internal byte         scale     = TdsEnums.UNKNOWN_PRECISION_SCALE; // give default of unknown (-1)
        internal int          length;
        internal SqlCollation collation;
        internal int          codePage;
        internal Encoding     encoding;
        internal bool         isNullable;
        internal bool         isMultiValued = false;

        // UDT specific metadata
        // server metadata info
        // additional temporary UDT meta data
        internal string                 udtDatabaseName;
        internal string                 udtSchemaName;
        internal string                 udtTypeName;
        internal string                 udtAssemblyQualifiedName;
        // on demand
        internal Type                   udtType;

        // Xml specific metadata
        internal string       xmlSchemaCollectionDatabase;
        internal string       xmlSchemaCollectionOwningSchema;
        internal string       xmlSchemaCollectionName;
        internal MetaType     metaType; // cached metaType

        // Structured type-specific metadata
        internal string                 structuredTypeDatabaseName;
        internal string                 structuredTypeSchemaName;
        internal string                 structuredTypeName;
        internal IList<SmiMetaData>     structuredFields;

        internal bool                    isEncrypted; // TCE encrypted?
        internal SqlMetaDataPriv         baseTI;   // for encrypted columns, represents the TYPE_INFO for plaintext value
        internal SqlCipherMetadata       cipherMD; // Cipher related metadata for encrypted columns.
        internal SqlMetaDataPriv() {
        }

        internal virtual void CopyFrom(SqlMetaDataPriv original) {
            this.type = original.type;
            this.tdsType = original.tdsType;
            this.precision = original.precision;
            this.scale = original.scale;
            this.length = original.length;
            this.collation = original.collation;
            this.codePage = original.codePage;
            this.encoding = original.encoding;
            this.isNullable = original.isNullable;
            this.isMultiValued = original.isMultiValued;
            this.udtDatabaseName = original.udtDatabaseName;
            this.udtSchemaName = original.udtSchemaName;
            this.udtTypeName = original.udtTypeName;
            this.udtAssemblyQualifiedName = original.udtAssemblyQualifiedName;
            this.udtType = original.udtType;
            this.xmlSchemaCollectionDatabase = original.xmlSchemaCollectionDatabase;
            this.xmlSchemaCollectionOwningSchema = original.xmlSchemaCollectionOwningSchema;
            this.xmlSchemaCollectionName = original.xmlSchemaCollectionName;
            this.metaType = original.metaType;
            // 


            this.structuredTypeDatabaseName = original.structuredTypeDatabaseName;
            this.structuredTypeSchemaName = original.structuredTypeSchemaName;
            this.structuredTypeName = original.structuredTypeName;
            this.structuredFields = original.structuredFields;
        }

        /// <summary>
        /// Is the algorithm handle for the cipher encryption initialized ?
        /// </summary>
        /// <returns></returns>
        internal bool IsAlgorithmInitialized() {
            if (null != cipherMD) {
                return cipherMD.IsAlgorithmInitialized(); 
            }

            return false;
        }

        /// <summary>
        /// Returns the normalization rule version byte.
        /// </summary>
        /// <returns></returns>
        internal byte NormalizationRuleVersion {
            get {
                if (null != cipherMD){
                    return cipherMD.NormalizationRuleVersion; 
                }

                return 0x00;
            }
        }
    }

    /// <summary>
    /// Class encapsulating additional information when sending encrypted input parameters.
    /// </summary>
    sealed internal class SqlColumnEncryptionInputParameterInfo
    {
        /// <summary>
        /// Metadata of the parameter to write the TYPE_INFO of the unencrypted column data type.
        /// </summary>
        private readonly SmiParameterMetaData _smiParameterMetadata;

        /// <summary>
        /// Column encryption related metadata.
        /// </summary>
        private readonly SqlCipherMetadata _cipherMetadata;

        /// <summary>
        /// Serialized format for a subset of members.
        /// Does not include _smiParameterMetadata's serialization.
        /// </summary>
        private readonly byte[] _serializedWireFormat;

        /// <summary>
        /// Return the SMI Parameter Metadata.
        /// </summary>
        internal SmiParameterMetaData ParameterMetadata {
            get {
                return _smiParameterMetadata;
            }
        }

        /// <summary>
        /// Return the serialized format for some members.
        /// This is pre-calculated and cached since members are immutable.
        /// Does not include _smiParameterMetadata's serialization.
        /// </summary>
        internal byte[] SerializedWireFormat
        {
            get {
                return _serializedWireFormat;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="smiParameterMetadata"></param>
        /// <param name="cipherMetadata"></param>
        internal SqlColumnEncryptionInputParameterInfo(SmiParameterMetaData smiParameterMetadata, SqlCipherMetadata cipherMetadata) {
            Debug.Assert(smiParameterMetadata != null, "smiParameterMetadata should not be null.");
            Debug.Assert(cipherMetadata != null, "cipherMetadata should not be null");
            Debug.Assert(cipherMetadata.EncryptionKeyInfo.HasValue, "cipherMetadata.EncryptionKeyInfo.HasValue should be true.");

            _smiParameterMetadata = smiParameterMetadata;
            _cipherMetadata = cipherMetadata;
            _serializedWireFormat = SerializeToWriteFormat();
        }

        /// <summary>
        /// Serializes some data members to wire format.
        /// </summary>
        private byte[] SerializeToWriteFormat() {
            int totalLength = 0;

            // CipherAlgorithmId.
            totalLength += sizeof(byte);

            // Encryption Type.
            totalLength += sizeof(byte);

            // Database id of the encryption key.
            totalLength += sizeof(int);

            // Id of the encryption key.
            totalLength += sizeof(int);

            // Version of the encryption key.
            totalLength += sizeof(int);

            // Metadata version of the encryption key.
            totalLength += _cipherMetadata.EncryptionKeyInfo.Value.cekMdVersion.Length;

            // Normalization Rule Version.
            totalLength += sizeof(byte);

            byte[] serializedWireFormat = new byte[totalLength];

            // No:of bytes consumed till now. Running variable.
            int consumedBytes = 0;

            // 1 - Write Cipher Algorithm Id.
            serializedWireFormat[consumedBytes++] = _cipherMetadata.CipherAlgorithmId;

            // 2 - Write Encryption Type.
            serializedWireFormat[consumedBytes++] = _cipherMetadata.EncryptionType;

            // 3 - Write the database id of the encryption key.
            SerializeIntIntoBuffer(_cipherMetadata.EncryptionKeyInfo.Value.databaseId, serializedWireFormat, ref consumedBytes);

            // 4 - Write the id of the encryption key.
            SerializeIntIntoBuffer(_cipherMetadata.EncryptionKeyInfo.Value.cekId, serializedWireFormat, ref consumedBytes);

            // 5 - Write the version of the encryption key.
            SerializeIntIntoBuffer(_cipherMetadata.EncryptionKeyInfo.Value.cekVersion, serializedWireFormat, ref consumedBytes);

            // 6 - Write the metadata version of the encryption key.
            Buffer.BlockCopy(_cipherMetadata.EncryptionKeyInfo.Value.cekMdVersion, 0, serializedWireFormat, consumedBytes, _cipherMetadata.EncryptionKeyInfo.Value.cekMdVersion.Length);
            consumedBytes += _cipherMetadata.EncryptionKeyInfo.Value.cekMdVersion.Length;

            // 7 - Write Normalization Rule Version.
            serializedWireFormat[consumedBytes++] = _cipherMetadata.NormalizationRuleVersion;

            return serializedWireFormat;
        }

        /// <summary>
        /// Serializes an int into the provided buffer and offset.
        /// </summary>
        private void SerializeIntIntoBuffer(int value, byte[] buffer, ref int offset) {
            buffer[offset++] = (byte)(value & 0xff);
            buffer[offset++] = (byte)((value >> 8) & 0xff);
            buffer[offset++] = (byte)((value >> 16) & 0xff);
            buffer[offset++] = (byte)((value >> 24) & 0xff);
        }
    }

    sealed internal class _SqlRPC {
        internal string         rpcName;
        internal string         databaseName; // Used for UDTs
        internal ushort         ProcID;       // Used instead of name
        internal ushort         options;
        internal SqlParameter[] parameters;
        internal byte[]         paramoptions;

        internal int?           recordsAffected;
        internal int            cumulativeRecordsAffected;

        internal int            errorsIndexStart;
        internal int            errorsIndexEnd;
        internal SqlErrorCollection errors;
        
        internal int            warningsIndexStart;
        internal int            warningsIndexEnd;
        internal SqlErrorCollection warnings;
        internal bool           needsFetchParameterEncryptionMetadata;
        internal string GetCommandTextOrRpcName() {
            if (TdsEnums.RPC_PROCID_EXECUTESQL == ProcID) {
                // Param 0 is the actual sql executing
                return (string)parameters[0].Value;
            }
            else {
                return rpcName;
            }
        }
    }

    sealed internal class SqlReturnValue : SqlMetaDataPriv {

        internal ushort    parmIndex;      //Yukon or later only
        internal string    parameter;
        internal readonly  SqlBuffer value;

        internal SqlReturnValue() : base() {
            value = new SqlBuffer();
        }
    }

    internal struct MultiPartTableName {
        private string _multipartName;
        private string _serverName;
        private string _catalogName;
        private string _schemaName;
        private string _tableName;

        internal MultiPartTableName(string[] parts) {
            _multipartName = null;
            _serverName = parts[0];
            _catalogName = parts[1];
            _schemaName = parts[2];
            _tableName = parts[3];
        }

        internal MultiPartTableName(string multipartName) {
            _multipartName = multipartName;
            _serverName = null;
            _catalogName = null;
            _schemaName = null;
            _tableName = null;
        }

        internal string ServerName {
            get {
                ParseMultipartName();
                return _serverName;
            }
            set { _serverName = value; }
        }
        internal string CatalogName {
            get {
                ParseMultipartName();
                return _catalogName;
            }
            set { _catalogName = value; }
        }
        internal string SchemaName {
            get {
                ParseMultipartName();
                return _schemaName;
            }
            set { _schemaName = value; }
        }
        internal string TableName {
            get {
                ParseMultipartName();
                return _tableName;
            }
            set { _tableName = value; }
        }

        private void ParseMultipartName() {
            if (null != _multipartName) {
                string[] parts = MultipartIdentifier.ParseMultipartIdentifier(_multipartName, "[\"", "]\"", Res.SQL_TDSParserTableName, false);
                _serverName = parts[0];
                _catalogName = parts[1];
                _schemaName = parts[2];
                _tableName = parts[3];
                _multipartName = null;
            }
        }

        internal static readonly MultiPartTableName Null = new MultiPartTableName(new string[] {null, null, null, null});
    }
}
