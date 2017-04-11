//------------------------------------------------------------------------------
// <copyright file="SqlProviderServices.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Data.Spatial;
    using System.Diagnostics;
    using System.IO;
    using System.Globalization;

    /// <summary>
    /// The DbProviderServices implementation for the SqlClient provider for SQL Server.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class SqlProviderServices : DbProviderServices
    {
        /// <summary>
        /// Private constructor to ensure only Singleton instance is created.
        /// </summary>
        private SqlProviderServices()
        {
        }

        /// <summary>
        /// Singleton object;
        /// </summary>
        internal static readonly SqlProviderServices Instance = new SqlProviderServices();

        /// <summary>
        /// The Singleton instance of the SqlProviderServices type.
        /// </summary>
        public static SqlProviderServices SingletonInstance
        {
            get { return Instance; }
        }

        /// <summary>
        /// Create a Command Definition object, given the connection and command tree
        /// </summary>
        /// <param name="providerManifest">provider manifest that was determined from metadata</param>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>an exectable command definition object</returns>
        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree) {
            Debug.Assert(providerManifest != null, "CreateCommandDefinition passed null provider manifest to CreateDbCommandDefinition?");
            Debug.Assert(commandTree != null, "CreateCommandDefinition did not validate commandTree argument?");
                        
            DbCommand prototype = CreateCommand(providerManifest, commandTree);
            DbCommandDefinition result = this.CreateCommandDefinition(prototype);
            return result;
        }

        /// <summary>
        /// Create a SqlCommand object given a command tree
        /// </summary>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>a command object</returns>
        internal override DbCommand CreateCommand(DbCommandTree commandTree) {
            EntityUtil.CheckArgumentNull(commandTree, "commandTree");
            StoreItemCollection storeMetadata = (StoreItemCollection)commandTree.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
            Debug.Assert(storeMetadata.StoreProviderManifest != null, "StoreItemCollection has null StoreProviderManifest?");

            return this.CreateCommand(storeMetadata.StoreProviderManifest, commandTree);
        }

        /// <summary>
        /// Create a SqlCommand object, given the provider manifest and command tree
        /// </summary>
        /// <param name="providerManifest">provider manifest</param>
        /// <param name="commandTree">command tree for the statement</param>
        /// <returns>a command object</returns>
        private DbCommand CreateCommand(DbProviderManifest providerManifest, DbCommandTree commandTree) {
            EntityUtil.CheckArgumentNull(providerManifest, "providerManifest");
            EntityUtil.CheckArgumentNull(commandTree, "commandTree");

            SqlProviderManifest sqlManifest = (providerManifest as SqlProviderManifest);
            if (sqlManifest == null)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Mapping_Provider_WrongManifestType(typeof(SqlProviderManifest)));
            }

            SqlVersion sqlVersion = sqlManifest.SqlVersion;
            SqlCommand command = new SqlCommand();

            List<SqlParameter> parameters;
            CommandType commandType;
            HashSet<string> paramsToForceNonUnicode;
            command.CommandText = System.Data.SqlClient.SqlGen.SqlGenerator.GenerateSql(commandTree, sqlVersion, out parameters, out commandType, out paramsToForceNonUnicode);
            command.CommandType = commandType;

            // Get the function (if any) implemented by the command tree since this influences our interpretation of parameters
            EdmFunction function = null;
            if (commandTree.CommandTreeKind == DbCommandTreeKind.Function) {
                function = ((DbFunctionCommandTree)commandTree).EdmFunction;
            }
            // Now make sure we populate the command's parameters from the CQT's parameters:
            foreach (KeyValuePair<string, TypeUsage> queryParameter in commandTree.Parameters) {
                SqlParameter parameter;

                // Use the corresponding function parameter TypeUsage where available (currently, the SSDL facets and 
                // type trump user-defined facets and type in the EntityCommand).
                FunctionParameter functionParameter;
                if (null != function && function.Parameters.TryGetValue(queryParameter.Key, false, out functionParameter)) {
                    const bool preventTruncation = false;
                    parameter = CreateSqlParameter(functionParameter.Name, functionParameter.TypeUsage, functionParameter.Mode, DBNull.Value, preventTruncation, sqlVersion);
                }
                else {
                    TypeUsage parameterType;
                    if ( (paramsToForceNonUnicode != null) &&              //Reached when a Function Command Tree is passed an incorrect parameter name by the user.
                         (paramsToForceNonUnicode.Contains(queryParameter.Key)) )
                    {
                        parameterType = queryParameter.Value.ShallowCopy(new FacetValues { Unicode = false });
                    }
                    else
                    {
                        parameterType = queryParameter.Value;
                    }
                    const bool preventTruncation = false;
                    parameter = CreateSqlParameter(queryParameter.Key, parameterType, ParameterMode.In, DBNull.Value, preventTruncation, sqlVersion);
                }
                command.Parameters.Add(parameter);
            }

            // Now add parameters added as part of SQL gen (note: this feature is only safe for DML SQL gen which
            // does not support user parameters, where there is no risk of name collision)
            if (null != parameters && 0 < parameters.Count) {
                if (commandTree.CommandTreeKind != DbCommandTreeKind.Delete &&
                    commandTree.CommandTreeKind != DbCommandTreeKind.Insert &&
                    commandTree.CommandTreeKind != DbCommandTreeKind.Update) {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.SqlGenParametersNotPermitted);
                }
                foreach (SqlParameter parameter in parameters) {
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        protected override void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
        {
            // Ensure a value that can be used with SqlParameter
            value = EnsureSqlParameterValue(value);

            if (TypeSemantics.IsPrimitiveType(parameterType, PrimitiveTypeKind.String) ||
                TypeSemantics.IsPrimitiveType(parameterType, PrimitiveTypeKind.Binary))
            {
                int? size = GetParameterSize(parameterType, ((parameter.Direction & ParameterDirection.Output) == ParameterDirection.Output));
                if(!size.HasValue)
                {
                    // Remember the current Size
                    int previousSize = parameter.Size;

                    // Infer the Size from the value
                    parameter.Size = 0;
                    parameter.Value = value;

                    if (previousSize > -1)
                    {
                        // The 'max' length was chosen as a specific value for the parameter's Size property on Sql8 (4000 or 8000)
                        // because no MaxLength was specified in the TypeUsage and the provider is Sql8. 
                        // If the value's length is less than or equal to this preset size, then the Size value can be retained, 
                        // otherwise this preset size must be removed in favor of the Size inferred from the value itself.
                                                
                        // If the inferred Size is less than the preset 'max' size, restore that preset size
                        if (parameter.Size < previousSize)
                        {
                            parameter.Size = previousSize;
                        }
                    }
                    else
                    {
                        // -1 was chosen as the parameter's size because no MaxLength was specified in the TypeUsage and the 
                        // provider is more recent than Sql8. However, it is more optimal to specify a non-max (-1) value for
                        // the size where possible, since 'max' parameters may prevent, for example, filter pushdown.
                        // (see Dev10#617447 for more details)
                        int suggestedLength = GetNonMaxLength(((SqlParameter)parameter).SqlDbType);
                        if (parameter.Size < suggestedLength)
                        {
                            parameter.Size = suggestedLength;
                        }
                        else if (parameter.Size > suggestedLength)
                        {
                            // The parameter size is greater than the suggested length, so the suggested length cannot be used.
                            // Since the provider is Sql9 or newer, set the size to max (-1) instead of the inferred size for better plan reuse.
                            parameter.Size = -1;
                        }
                    }
                }
                else
                {
                    // Just set the value
                    parameter.Value = value;
                }
            }
            else
            {
                // Not a string or binary parameter - just set the value
                parameter.Value = value;
            }
        }

        protected override string GetDbProviderManifestToken(DbConnection connection) {
            EntityUtil.CheckArgumentNull(connection, "connection");

            SqlConnection sqlConnection = SqlProviderUtilities.GetRequiredSqlConnection(connection);

            if (string.IsNullOrEmpty(sqlConnection.ConnectionString)) {
                throw EntityUtil.Argument(Strings.UnableToDetermineStoreVersion);
            }

            string providerManifestToken = null;
            // Try to get the provider manifest token from the database connection
            // That failing, try using connection to master database (in case the database doesn't exist yet)
            try
            {
                UsingConnection(sqlConnection, conn =>
                {
                    providerManifestToken = SqlVersionUtils.GetVersionHint(SqlVersionUtils.GetSqlVersion(conn));
                });
            }
            catch
            {
                UsingMasterConnection(sqlConnection, conn =>
                {
                    providerManifestToken = SqlVersionUtils.GetVersionHint(SqlVersionUtils.GetSqlVersion(conn));
                });
            }
            return providerManifestToken;
        }

        protected override DbProviderManifest GetDbProviderManifest(string versionHint) {
            if (string.IsNullOrEmpty(versionHint)) {
                throw EntityUtil.Argument(Strings.UnableToDetermineStoreVersion);
            }

            return new SqlProviderManifest(versionHint);
        }

        protected override Spatial.DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string versionHint) 
        {    
            EntityUtil.CheckArgumentNull(fromReader, "fromReader");

            ValidateVersionHint(versionHint);

            SqlDataReader underlyingReader = fromReader as SqlDataReader;
            if (underlyingReader == null)
            {
                throw EntityUtil.ProviderIncompatible(Strings.SqlProvider_NeedSqlDataReader(fromReader.GetType()));
            }


             return new SqlSpatialDataReader(underlyingReader);
        }

        protected override DbSpatialServices DbGetSpatialServices(string versionHint)
        {
            ValidateVersionHint(versionHint);
            return SqlSpatialServices.Instance;
        }

        void ValidateVersionHint(string versionHint)
        {
            if (string.IsNullOrEmpty(versionHint))
            {
                throw EntityUtil.Argument(Strings.UnableToDetermineStoreVersion);
            }

            // GetSqlVersion will throw ArgumentException if manifestToken is null, empty, or not recognized.
            SqlVersion tokenVersion = SqlVersionUtils.GetSqlVersion(versionHint);

            // SQL spatial support is only available for SQL Server 2008 and later
            if (tokenVersion < SqlVersion.Sql10)
            {
                throw EntityUtil.ProviderIncompatible(Strings.SqlProvider_Sql2008RequiredForSpatial);
            }
        }
        
        internal static SqlTypesAssembly GetSqlTypesAssembly()
        {
            SqlTypesAssembly sqlTypes;
            if (!TryGetSqlTypesAssembly(out sqlTypes))
            {
                throw EntityUtil.SqlTypesAssemblyNotFound();
            }
            Debug.Assert(sqlTypes != null);
            return sqlTypes;
        }

        internal static bool SqlTypesAssemblyIsAvailable
        {
            get
            {
                SqlTypesAssembly notUsed;
                return TryGetSqlTypesAssembly(out notUsed);
            }
        }

        private static bool TryGetSqlTypesAssembly(out SqlTypesAssembly sqlTypesAssembly)
        {
            sqlTypesAssembly = SqlTypesAssembly.Latest;
            return sqlTypesAssembly != null;
        }

        /// <summary>
        /// Creates a SqlParameter given a name, type, and direction
        /// </summary>
        internal static SqlParameter CreateSqlParameter(string name, TypeUsage type, ParameterMode mode, object value, bool preventTruncation, SqlVersion version) {
            int? size;
            byte? precision;
            byte? scale;
            string udtTypeName;

            value = EnsureSqlParameterValue(value);

            SqlParameter result = new SqlParameter(name, value);

            // .Direction
            ParameterDirection direction = MetadataHelper.ParameterModeToParameterDirection(mode);
            if (result.Direction != direction) {
                result.Direction = direction;
            }
            
            // .Size, .Precision, .Scale and .SqlDbType
            // output parameters are handled differently (we need to ensure there is space for return
            // values where the user has not given a specific Size/MaxLength)
            bool isOutParam = mode != ParameterMode.In;
            SqlDbType sqlDbType = GetSqlDbType(type, isOutParam, version, out size, out precision, out scale, out udtTypeName);

            if (result.SqlDbType != sqlDbType) {
                result.SqlDbType = sqlDbType;
            }

            if (sqlDbType == SqlDbType.Udt)
            {
                result.UdtTypeName = udtTypeName;
            }

            // Note that we overwrite 'facet' parameters where either the value is different or
            // there is an output parameter. This is because output parameters in SqlClient have their
            // facets clobbered if they are implicitly set (e.g. if the Size was implicitly set
            // by setting the value)
            if (size.HasValue)
            {
                // size.HasValue is always true for Output parameters
                if ((isOutParam || result.Size != size.Value))
                {
                    if (preventTruncation && size.Value != -1)
                    {
                        // To prevent truncation, set the Size of the parameter to the larger of either
                        // the declared length or the actual length for the parameter. This allows SQL
                        // Server to complain if a value is too long while preventing cache misses for
                        // values within the range.
                        result.Size = Math.Max(result.Size, size.Value);
                    }
                    else
                    {
                        result.Size = size.Value;
                    }
                }
            }
            else 
            {
                PrimitiveTypeKind typeKind = MetadataHelper.GetPrimitiveTypeKind(type);
                if (typeKind == PrimitiveTypeKind.String)
                {
                    result.Size = GetDefaultStringMaxLength(version, sqlDbType);
                }
                else if(typeKind == PrimitiveTypeKind.Binary)
                {
                    result.Size = GetDefaultBinaryMaxLength(version);
                }
            }
            if (precision.HasValue && (isOutParam || result.Precision != precision.Value)) {
                result.Precision = precision.Value;
            }
            if (scale.HasValue && (isOutParam || result.Scale != scale.Value)) {
                result.Scale = scale.Value;
            }

            // .IsNullable
            bool isNullable = TypeSemantics.IsNullable(type);
            if (isOutParam || isNullable != result.IsNullable) {
                result.IsNullable = isNullable;
            }

            return result;
        }

        /// <summary>
        /// Validates that the specified value is compatible with SqlParameter and if not, attempts to return an appropriate value that is.
        /// Currently only spatial values (DbGeography/DbGeometry) may not be directly usable with SqlParameter. For these types, an instance
        /// of the corresponding SQL Server CLR spatial UDT will be manufactured based on the spatial data contained in <paramref name="value"/>.
        /// If <paramref name="value"/> is an instance of DbGeography/DbGeometry that was read from SQL Server by this provider, then the wrapped
        /// CLR UDT value is available via the ProviderValue property (see SqlSpatialServices for the full conversion process from instances of 
        /// DbGeography/DbGeometry to instances of the CLR SqlGeography/SqlGeometry UDTs)
        /// </summary>
        internal static object EnsureSqlParameterValue(object value)
        {
            if (value != null &&
                value != DBNull.Value &&
                Type.GetTypeCode(value.GetType()) == TypeCode.Object)
            {
                // If the parameter is being created based on an actual value (typically for constants found in DML expressions) then a DbGeography/DbGeometry
                // value must be replaced by an an appropriate Microsoft.SqlServer.Types.SqlGeography/SqlGeometry instance. Since the DbGeography/DbGeometry
                // value may not have been originally created by this SqlClient provider services implementation, just using the ProviderValue is not sufficient.
                DbGeography geographyValue = value as DbGeography;
                if (geographyValue != null)
                {
                    value = GetSqlTypesAssembly().ConvertToSqlTypesGeography(geographyValue);
                }
                else
                {
                    DbGeometry geometryValue = value as DbGeometry;
                    if (geometryValue != null)
                    {
                        value = GetSqlTypesAssembly().ConvertToSqlTypesGeometry(geometryValue);
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Determines SqlDbType for the given primitive type. Extracts facet
        /// information as well.
        /// </summary>
        private static SqlDbType GetSqlDbType(TypeUsage type, bool isOutParam, SqlVersion version, out int? size, out byte? precision, out byte? scale, out string udtName) {
            // only supported for primitive type
            PrimitiveTypeKind primitiveTypeKind = MetadataHelper.GetPrimitiveTypeKind(type);

            size = default(int?);
            precision = default(byte?);
            scale = default(byte?);
            udtName = default(string);

            // 
            switch (primitiveTypeKind) {
                case PrimitiveTypeKind.Binary:
                    // for output parameters, ensure there is space...
                    size = GetParameterSize(type, isOutParam);
                    return GetBinaryDbType(type);

                case PrimitiveTypeKind.Boolean:
                    return SqlDbType.Bit;

                case PrimitiveTypeKind.Byte:
                    return SqlDbType.TinyInt;

                case PrimitiveTypeKind.Time:
                    if (!SqlVersionUtils.IsPreKatmai(version)) {
                        precision = GetKatmaiDateTimePrecision(type, isOutParam);
                    }
                    return SqlDbType.Time;

                case PrimitiveTypeKind.DateTimeOffset:
                    if (!SqlVersionUtils.IsPreKatmai(version)) {
                        precision = GetKatmaiDateTimePrecision(type, isOutParam);
                    }
                    return SqlDbType.DateTimeOffset;

                case PrimitiveTypeKind.DateTime:
                    //For katmai pick the type with max precision which is datetime2
                    if (!SqlVersionUtils.IsPreKatmai(version)) {
                        precision = GetKatmaiDateTimePrecision(type, isOutParam);
                        return SqlDbType.DateTime2;
                    }
                    else {
                        return SqlDbType.DateTime;
                    }

                case PrimitiveTypeKind.Decimal:
                    precision = GetParameterPrecision(type, null);
                    scale = GetScale(type);
                    return SqlDbType.Decimal;

                case PrimitiveTypeKind.Double:
                    return SqlDbType.Float;

                case PrimitiveTypeKind.Geography:
                    {
                        udtName = "geography";
                        return SqlDbType.Udt;
                    }

                case PrimitiveTypeKind.Geometry:
                    {
                        udtName = "geometry";
                        return SqlDbType.Udt;
                    }

                case PrimitiveTypeKind.Guid:
                    return SqlDbType.UniqueIdentifier;

                case PrimitiveTypeKind.Int16:
                    return SqlDbType.SmallInt;

                case PrimitiveTypeKind.Int32:
                    return SqlDbType.Int;

                case PrimitiveTypeKind.Int64:
                    return SqlDbType.BigInt;

                case PrimitiveTypeKind.SByte:
                    return SqlDbType.SmallInt;

                case PrimitiveTypeKind.Single:
                    return SqlDbType.Real;

                case PrimitiveTypeKind.String:
                    size = GetParameterSize(type, isOutParam);
                    return GetStringDbType(type);

                default:
                    Debug.Fail("unknown PrimitiveTypeKind " + primitiveTypeKind);
                    return SqlDbType.Variant;
            }
        }

        /// <summary>
        /// Determines preferred value for SqlParameter.Size. Returns null
        /// where there is no preference.
        /// </summary>
        private static int? GetParameterSize(TypeUsage type, bool isOutParam) {
            Facet maxLengthFacet;
            if (type.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, false, out maxLengthFacet) &&
                null != maxLengthFacet.Value) {
                if (maxLengthFacet.IsUnbounded) {
                    return -1;
                }
                else {
                    return (int?)maxLengthFacet.Value;
                }
            }
            else if (isOutParam) {
                // if the parameter is a return/out/inout parameter, ensure there 
                // is space for any value
                return -1;
            }
            else {
                // no value
                return default(int?);
            }
        }

        private static int GetNonMaxLength(SqlDbType type)
        {
            int result = -1;
            if (type == SqlDbType.NChar || type == SqlDbType.NVarChar)
            {
                result = 4000;
            }
            else if(type == SqlDbType.Char || type == SqlDbType.VarChar ||
                    type == SqlDbType.Binary || type == SqlDbType.VarBinary)
            {
                result = 8000;
            }
            return result;
        }

        private static int GetDefaultStringMaxLength(SqlVersion version, SqlDbType type)
        {
            int result;
            if (version < SqlVersion.Sql9)
            {
                if (type == SqlDbType.NChar || type == SqlDbType.NVarChar)
                {
                    result = 4000;
                }
                else
                {
                    result = 8000;
                }
            }
            else
            {
                result = -1;
            }
            return result;
        }

        private static int GetDefaultBinaryMaxLength(SqlVersion version)
        {
            int result;
            if (version < SqlVersion.Sql9)
            {
                result = 8000;
            }
            else
            {
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// Returns SqlParameter.Precision where the type facet exists. Otherwise,
        /// returns null or the maximum available precision to avoid truncation (which can occur
        /// for output parameters).
        /// </summary>
        private static byte? GetKatmaiDateTimePrecision(TypeUsage type, bool isOutParam) {
            byte? defaultIfUndefined = isOutParam ? (byte?)7 : (byte?)null;
            return GetParameterPrecision(type, defaultIfUndefined);
        }
        
        /// <summary>
        /// Returns SqlParameter.Precision where the type facet exists. Otherwise,
        /// returns null.
        /// </summary>
        private static byte? GetParameterPrecision(TypeUsage type, byte? defaultIfUndefined) {
            byte precision;
            if (TypeHelpers.TryGetPrecision(type, out precision)) {
                return precision;
            }
            else {
                return defaultIfUndefined;
            }
        }

        /// <summary>
        /// Returns SqlParameter.Scale where the type facet exists. Otherwise,
        /// returns null.
        /// </summary>
        private static byte? GetScale(TypeUsage type) {
            byte scale;
            if (TypeHelpers.TryGetScale(type, out scale)) {
                return scale;
            }
            else {
                return default(byte?);
            }
        }

        /// <summary>
        /// Chooses the appropriate SqlDbType for the given string type.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static SqlDbType GetStringDbType(TypeUsage type) {
            Debug.Assert(type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType &&
                PrimitiveTypeKind.String == ((PrimitiveType)type.EdmType).PrimitiveTypeKind, "only valid for string type");

            SqlDbType dbType;
            if (type.EdmType.Name.ToLowerInvariant() == "xml") {
                dbType = SqlDbType.Xml;
            }
            else {
                // Specific type depends on whether the string is a unicode string and whether it is a fixed length string.
                // By default, assume widest type (unicode) and most common type (variable length)
                bool unicode;
                bool fixedLength;
                if (!TypeHelpers.TryGetIsFixedLength(type, out fixedLength)) {
                    fixedLength = false;
                }

                if (!TypeHelpers.TryGetIsUnicode(type, out unicode)) {
                    unicode = true;
                }

                if (fixedLength) {
                    dbType = (unicode ? SqlDbType.NChar : SqlDbType.Char);
                }
                else {
                    dbType = (unicode ? SqlDbType.NVarChar : SqlDbType.VarChar);
                }
            }
            return dbType;
        }

        /// <summary>
        /// Chooses the appropriate SqlDbType for the given binary type.
        /// </summary>
        private static SqlDbType GetBinaryDbType(TypeUsage type) {
            Debug.Assert(type.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType &&
                PrimitiveTypeKind.Binary == ((PrimitiveType)type.EdmType).PrimitiveTypeKind, "only valid for binary type");

            // Specific type depends on whether the binary value is fixed length. By default, assume variable length.
            bool fixedLength;
            if (!TypeHelpers.TryGetIsFixedLength(type, out fixedLength)) {
                fixedLength = false;
            }

            return fixedLength ? SqlDbType.Binary : SqlDbType.VarBinary;
        }

        protected override string DbCreateDatabaseScript(string providerManifestToken, StoreItemCollection storeItemCollection)
        {
            EntityUtil.CheckArgumentNull(providerManifestToken, "providerManifestToken");
            EntityUtil.CheckArgumentNull(storeItemCollection, "storeItemCollection");
            SqlVersion version = SqlVersionUtils.GetSqlVersion(providerManifestToken);
            return CreateObjectsScript(version, storeItemCollection);
        }

        /// <summary>
        /// Create the database and the database objects.
        /// If initial catalog is not specified, but AttachDBFilename is specified, we generate a random database name based on the AttachDBFilename.
        /// Note: this causes pollution of the db, as when the connection string is later used, the mdf will get attached under a different name. 
        /// However if we try to replicate the name under which it would be attached, the following scenario would fail:
        ///    The file does not exist, but registered with database.
        ///    The user calls:  If (DatabaseExists) DeleteDatabase 
        ///                     CreateDatabase
        /// For further details on the behavior when AttachDBFilename is specified see Dev10# 188936 
        /// </summary>
        protected override void DbCreateDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            EntityUtil.CheckArgumentNull(connection, "connection");
            EntityUtil.CheckArgumentNull(storeItemCollection, "storeItemCollection");

            SqlConnection sqlConnection = SqlProviderUtilities.GetRequiredSqlConnection(connection);
            string databaseName, dataFileName, logFileName;
            GetOrGenerateDatabaseNameAndGetFileNames(sqlConnection, out databaseName, out dataFileName, out logFileName);
            string createDatabaseScript = SqlDdlBuilder.CreateDatabaseScript(databaseName, dataFileName, logFileName);
            SqlVersion sqlVersion = GetSqlVersion(storeItemCollection);

            string createObjectsScript = CreateObjectsScript(sqlVersion, storeItemCollection);

            UsingMasterConnection(sqlConnection, conn =>
            {
                // create database
                CreateCommand(conn, createDatabaseScript, commandTimeout).ExecuteNonQuery();
            });
            
            // Create database already succeeded. If there is a failure from this point on, the user should be informed.
            try
            {
                // Clear connection pool for the database connection since after the 'create database' call, a previously
                // invalid connection may now be valid.
                SqlConnection.ClearPool(sqlConnection);

                UsingConnection(sqlConnection, conn =>
                {
                    // create database objects
                    CreateCommand(conn, createObjectsScript, commandTimeout).ExecuteNonQuery();
                });
            }  
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    // Try to drop the database
                    try
                    {
                        DropDatabase(sqlConnection, commandTimeout, databaseName);
                    }
                    catch (Exception ie)
                    {
                        // The creation of the database succeeded, the creation of the database objects failed, and the dropping of the database failed.
                        if (EntityUtil.IsCatchableExceptionType(ie))
                        {
                            throw new InvalidOperationException(Strings.SqlProvider_IncompleteCreateDatabase, new AggregateException(Strings.SqlProvider_IncompleteCreateDatabaseAggregate, e, ie));
                        }
                        throw;
                    }
                    // The creation of the database succeeded, the creation of the database objects failed, the database was dropped, no reason to wrap the exception
                    throw;
                }
                throw;
            }
        }

        private static SqlVersion GetSqlVersion(StoreItemCollection storeItemCollection)
        {
            SqlProviderManifest sqlManifest = (storeItemCollection.StoreProviderManifest as SqlProviderManifest);
            if (sqlManifest == null)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.Mapping_Provider_WrongManifestType(typeof(SqlProviderManifest)));
            }
            SqlVersion sqlVersion = sqlManifest.SqlVersion;
            return sqlVersion;
        }

        private static void GetOrGenerateDatabaseNameAndGetFileNames(SqlConnection sqlConnection, out string databaseName, out string dataFileName, out string logFileName)
        {
            Debug.Assert(sqlConnection != null);

            var connectionStringBuilder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);
            
            // Get the file names
            string attachDBFile = connectionStringBuilder.AttachDBFilename;
            if (string.IsNullOrEmpty(attachDBFile))
            {
                dataFileName = null;
                logFileName = null;
            }
            else
            {
                //Handle the other cases
                dataFileName = GetMdfFileName(attachDBFile);
                logFileName = GetLdfFileName(dataFileName);
            }
            
            // Get the database name
            if (!string.IsNullOrEmpty(connectionStringBuilder.InitialCatalog))
            {
                databaseName = connectionStringBuilder.InitialCatalog;
            }
            else if (dataFileName != null)
            {
                //generate the database name here
                databaseName = GenerateDatabaseName(dataFileName);
            }
            else
            {
                throw EntityUtil.InvalidOperation(Strings.SqlProvider_DdlGeneration_MissingInitialCatalog);
            }
        }

        /// <summary>
        /// Get the Ldf name given the Mdf full name
        /// </summary>
        private static string GetLdfFileName(string dataFileName)
        {
            string logFileName;
            var directory = new FileInfo(dataFileName).Directory;
            logFileName = Path.Combine(directory.FullName, String.Concat(Path.GetFileNameWithoutExtension(dataFileName), "_log.ldf"));
            return logFileName;
        }

        /// <summary>
        /// Generates database name based on the given mdfFileName.
        /// The logic is replicated from System.Web.DataAccess.SqlConnectionHelper
        /// </summary>
        private static string GenerateDatabaseName(string mdfFileName)
        {
            string toUpperFileName = mdfFileName.ToUpper(CultureInfo.InvariantCulture);
            char [] strippedFileNameChars = Path.GetFileNameWithoutExtension(toUpperFileName).ToCharArray();

            for (int iter = 0; iter < strippedFileNameChars.Length; iter++)
            {
                if (!char.IsLetterOrDigit(strippedFileNameChars[iter]))
                {
                    strippedFileNameChars[iter] = '_';
                }
            }

            string strippedFileName = new string(strippedFileNameChars);
            strippedFileName = strippedFileName.Length > 30 ? strippedFileName.Substring(0, 30) : strippedFileName;

            string databaseName =  databaseName = String.Format(CultureInfo.InvariantCulture, "{0}_{1}", strippedFileName, Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)); 
            return databaseName;
        }

        /// <summary>
        /// Get the full mdf file name given the attachDBFile value from the connection string
        /// </summary>
        /// <param name="attachDBFile"></param>
        /// <returns></returns>
        private static string GetMdfFileName(string attachDBFile)
        {
            Debug.Assert(!string.IsNullOrEmpty(attachDBFile));
           
            //Handle the case when attachDBFilename starts with |DataDirectory|
            string dataFileName = System.Data.EntityClient.DbConnectionOptions.ExpandDataDirectory("AttachDBFilename", attachDBFile);

            //Handle the other cases
            dataFileName = dataFileName ?? attachDBFile;
            return dataFileName;
        }

        /// <summary>
        /// Determines whether the database for the given connection exists.
        /// There are three cases:
        /// 1.  Initial Catalog = X, AttachDBFilename = null:   (SELECT Count(*) FROM sys.databases WHERE [name]= X) > 0
        /// 2.  Initial Catalog = X, AttachDBFilename = F:      if (SELECT Count(*) FROM sys.databases WHERE [name]= X) >  true,
        /// if not, try to open the connection and then return (SELECT Count(*) FROM sys.databases WHERE [name]= X) > 0
        /// 3.  Initial Catalog = null, AttachDBFilename = F:   Try to open the connection. If that succeeds the result is true, otherwise
        /// if the there are no databases corresponding to the given file return false, otherwise throw.
        /// 
        /// Note: We open the connection to cover the scenario when the mdf exists, but is not attached. 
        /// Given that opening the connection would auto-attach it, it would not be appropriate to return false in this case. 
        /// Also note that checking for the existence of the file does not work for a remote server.  (Dev11 #290487)
        /// For further details on the behavior when AttachDBFilename is specified see Dev10# 188936 
        /// </summary>
        protected override bool DbDatabaseExists(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            EntityUtil.CheckArgumentNull(connection, "connection");
            EntityUtil.CheckArgumentNull(storeItemCollection, "storeItemCollection");

            SqlConnection sqlConnection = SqlProviderUtilities.GetRequiredSqlConnection(connection);
            var connectionBuilder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);

            if (string.IsNullOrEmpty(connectionBuilder.InitialCatalog) && string.IsNullOrEmpty(connectionBuilder.AttachDBFilename))
            {
                throw EntityUtil.InvalidOperation(Strings.SqlProvider_DdlGeneration_MissingInitialCatalog);  
            }
            
            if (!string.IsNullOrEmpty(connectionBuilder.InitialCatalog))
            {
                if (CheckDatabaseExists(sqlConnection, commandTimeout, connectionBuilder.InitialCatalog))
                {
                    //Avoid further processing
                    return true;
                }
            }
            
            if (!string.IsNullOrEmpty(connectionBuilder.AttachDBFilename))
            {
                try
                {
                    UsingConnection(sqlConnection, (SqlConnection con) => { });
                    return true;
                }
                catch (SqlException e)
                {
                    if (!string.IsNullOrEmpty(connectionBuilder.InitialCatalog))
                    {
                        return CheckDatabaseExists(sqlConnection, commandTimeout, connectionBuilder.InitialCatalog);
                    }
                    // Initial catalog not specified
                    string fileName = GetMdfFileName(connectionBuilder.AttachDBFilename);                   
                    bool databaseDoesNotExistInSysTables = false;
                    UsingMasterConnection(sqlConnection, conn =>
                    {
                        SqlVersion sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
                        string databaseExistsScript = SqlDdlBuilder.CreateCountDatabasesBasedOnFileNameScript(fileName, useDeprecatedSystemTable: sqlVersion == SqlVersion.Sql8);      
                        int result = (int)CreateCommand(conn, databaseExistsScript, commandTimeout).ExecuteScalar();
                        databaseDoesNotExistInSysTables = (result == 0);
                    });
                    if (databaseDoesNotExistInSysTables)
                    {
                        return false;
                    }
                    throw EntityUtil.InvalidOperation(Strings.SqlProvider_DdlGeneration_CannotTellIfDatabaseExists, e);
                }
            }

            // CheckDatabaseExists returned false and no AttachDBFilename is specified
            return false;
        }

        private static bool CheckDatabaseExists(SqlConnection sqlConnection, int? commandTimeout, string databaseName)
        {
            bool databaseExistsInSysTables = false;
            UsingMasterConnection(sqlConnection, conn =>
            {
                SqlVersion sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
                string databaseExistsScript = SqlDdlBuilder.CreateDatabaseExistsScript(databaseName, useDeprecatedSystemTable: sqlVersion == SqlVersion.Sql8);
                int result = (int)CreateCommand(conn, databaseExistsScript, commandTimeout).ExecuteScalar();
                databaseExistsInSysTables = (result > 0);
            });
            return databaseExistsInSysTables;
        }

        /// <summary>
        /// Delete the database for the given connection.
        /// There are three cases:
        /// 1.  If Initial Catalog is specified (X) drop database X
        /// 2.  Else if AttachDBFilename is specified (F) drop all the databases corresponding to F
        /// if none throw
        /// 3.  If niether the catalog not the file name is specified - throw
        /// 
        /// Note that directly deleting the files does not work for a remote server.  However, even for not attached 
        /// databases the current logic would work assuming the user does: if (DatabaseExists) DeleteDatabase
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="storeItemCollection"></param>
        protected override void DbDeleteDatabase(DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
        {
            EntityUtil.CheckArgumentNull(connection, "connection");
            EntityUtil.CheckArgumentNull(storeItemCollection, "storeItemCollection");
            SqlConnection sqlConnection = SqlProviderUtilities.GetRequiredSqlConnection(connection);

            var connectionBuilder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);
            string initialCatalog = connectionBuilder.InitialCatalog;
            string attachDBFile = connectionBuilder.AttachDBFilename;

            if (!string.IsNullOrEmpty(initialCatalog))
            {          
                DropDatabase(sqlConnection, commandTimeout, initialCatalog);
            }

            // initial catalog not specified
            else if (!string.IsNullOrEmpty(attachDBFile))
            {
                string fullFileName = GetMdfFileName(attachDBFile);

                List<string> databaseNames = new List<string>();
                UsingMasterConnection(sqlConnection, conn =>
                {
                    SqlVersion sqlVersion = SqlVersionUtils.GetSqlVersion(conn);
                    string getDatabaseNamesScript = SqlDdlBuilder.CreateGetDatabaseNamesBasedOnFileNameScript(fullFileName, sqlVersion == SqlVersion.Sql8);
                    var command = CreateCommand(conn, getDatabaseNamesScript, commandTimeout);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            databaseNames.Add(reader.GetString(0));
                        }
                    }
                });
                if (databaseNames.Count > 0)
                {
                    foreach (var databaseName in databaseNames)
                    {
                        DropDatabase(sqlConnection, commandTimeout, databaseName);
                    }
                }
                else
                {
                  throw EntityUtil.InvalidOperation(Strings.SqlProvider_DdlGeneration_CannotDeleteDatabaseNoInitialCatalog);           
                }
            }
            // neither initial catalog nor attachDB file name are specified
            else
            {
                throw EntityUtil.InvalidOperation(Strings.SqlProvider_DdlGeneration_MissingInitialCatalog);
            }
        }

        private static void DropDatabase(SqlConnection sqlConnection, int? commandTimeout, string databaseName)
        {
            // clear the connection pool in case someone's holding on to the database still
            SqlConnection.ClearPool(sqlConnection);

            string dropDatabaseScript = SqlDdlBuilder.DropDatabaseScript(databaseName);
            UsingMasterConnection(sqlConnection, (conn) =>
            {
                CreateCommand(conn, dropDatabaseScript, commandTimeout).ExecuteNonQuery();
            });
        }

        private static string CreateObjectsScript(SqlVersion version, StoreItemCollection storeItemCollection)
        {
            return SqlDdlBuilder.CreateObjectsScript(storeItemCollection, createSchemas: version != SqlVersion.Sql8);
        }

        private static SqlCommand CreateCommand(SqlConnection sqlConnection, string commandText, int? commandTimeout)
        {
            Debug.Assert(sqlConnection != null);
            if (string.IsNullOrEmpty(commandText))
            {
                // SqlCommand will complain if the command text is empty
                commandText = Environment.NewLine;
            }
            var command = new SqlCommand(commandText, sqlConnection);
            if (commandTimeout.HasValue)
            {
                command.CommandTimeout = commandTimeout.Value;
            }
            return command;
        }

        private static void UsingConnection(SqlConnection sqlConnection, Action<SqlConnection> act)
        {
            // remember the connection string so that we can reset it credentials are wiped
            string holdConnectionString = sqlConnection.ConnectionString;
            bool openingConnection = sqlConnection.State == ConnectionState.Closed;
            if (openingConnection)
            {
                sqlConnection.Open();
            }
            try
            {
                act(sqlConnection);
            }
            finally
            {
                if (openingConnection && sqlConnection.State == ConnectionState.Open)
                {
                    // if we opened the connection, we should close it
                    sqlConnection.Close();
                }
                if (sqlConnection.ConnectionString != holdConnectionString)
                {
                    sqlConnection.ConnectionString = holdConnectionString;
                }
            }
        }

        private static void UsingMasterConnection(SqlConnection sqlConnection, Action<SqlConnection> act)
        {
            var connectionBuilder = new SqlConnectionStringBuilder(sqlConnection.ConnectionString)
            {
                InitialCatalog = "master",
                AttachDBFilename = string.Empty, // any AttachDB path specified is not relevant to master
            };

            try
            {
                using (var masterConnection = new SqlConnection(connectionBuilder.ConnectionString))
                {
                    UsingConnection(masterConnection, act);
                }
            }
            catch (SqlException e)
            {
                // if it appears that the credentials have been removed from the connection string, use an alternate explanation
                if (!connectionBuilder.IntegratedSecurity &&
                    (string.IsNullOrEmpty(connectionBuilder.UserID) || string.IsNullOrEmpty(connectionBuilder.Password)))
                {
                    throw new InvalidOperationException(Strings.SqlProvider_CredentialsMissingForMasterConnection, e);
                }
                throw;
            }
        }
 
    }
}
