//---------------------------------------------------------------------
// <copyright file="SqlProviderManifest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;

namespace System.Data.SqlClient
{
    /// <summary>
    /// The Provider Manifest for SQL Server
    /// </summary>
    internal class SqlProviderManifest : DbXmlEnabledProviderManifest
    {
        internal const string TokenSql8 = "2000";
        internal const string TokenSql9 = "2005";
        internal const string TokenSql10 = "2008";

        // '~' is the same escape character that L2S uses
        internal const char LikeEscapeChar = '~';
        internal const string LikeEscapeCharToString = "~";

        #region Private Fields

        // Default to SQL Server 2005 (9.0)
        private SqlVersion _version = SqlVersion.Sql9;

        /// <summary>
        /// maximum size of sql server unicode 
        /// </summary>
        private const int varcharMaxSize = 8000;
        private const int nvarcharMaxSize = 4000;
        private const int binaryMaxSize = 8000;

        private System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> _primitiveTypes = null;
        private System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> _functions = null;

        #endregion

        #region Constructors
               
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manifestToken">A token used to infer the capabilities of the store</param>
        public SqlProviderManifest(string manifestToken)
            : base(SqlProviderManifest.GetProviderManifest())
        {
            // GetSqlVersion will throw ArgumentException if manifestToken is null, empty, or not recognized.
            _version = SqlVersionUtils.GetSqlVersion(manifestToken);
        }


        #endregion

        #region Properties

        internal SqlVersion SqlVersion
        {
            get { return this._version; }
        }

        #endregion

        #region Private Methods
        private static XmlReader GetProviderManifest()
        {
            return DbProviderServices.GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices.ProviderManifest.xml");
        }

        private XmlReader GetStoreSchemaMapping(string mslName)
        {
            return DbProviderServices.GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices." + mslName + ".msl");
        }

        private XmlReader GetStoreSchemaDescription(string ssdlName)
        {
            if (this._version == SqlVersion.Sql8)
            {
                return DbProviderServices.GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices." + ssdlName + "_Sql8.ssdl");
            }

            return DbProviderServices.GetXmlResource("System.Data.Resources.SqlClient.SqlProviderServices." + ssdlName + ".ssdl");
        }
        #endregion 

        #region Internal Methods

        /// <summary>
        /// Function to detect wildcard characters %, _, [ and ^ and escape them with a preceding ~
        /// This escaping is used when StartsWith, EndsWith and Contains canonical and CLR functions
        /// are translated to their equivalent LIKE expression
        /// NOTE: This code has been copied from LinqToSql
        /// </summary>
        /// <param name="text">Original input as specified by the user</param>
        /// <param name="alwaysEscapeEscapeChar">escape the escape character ~ regardless whether wildcard 
        /// characters were encountered </param>
        /// <param name="usedEscapeChar">true if the escaping was performed, false if no escaping was required</param>
        /// <returns>The escaped string that can be used as pattern in a LIKE expression</returns>
        internal static string EscapeLikeText(string text, bool alwaysEscapeEscapeChar, out bool usedEscapeChar)
        {
            usedEscapeChar = false;
            if (!(text.Contains("%") || text.Contains("_") || text.Contains("[")
                || text.Contains("^") || alwaysEscapeEscapeChar && text.Contains(LikeEscapeCharToString)))
            {
                return text;
            }
            StringBuilder sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (c == '%' || c == '_' || c == '[' || c == '^' || c == LikeEscapeChar)
                {
                    sb.Append(LikeEscapeChar);
                    usedEscapeChar = true;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Providers should override this to return information specific to their provider.  
        /// 
        /// This method should never return null.
        /// </summary>
        /// <param name="informationType">The name of the information to be retrieved.</param>
        /// <returns>An XmlReader at the begining of the information requested.</returns>
        protected override XmlReader GetDbInformation(string informationType)
        {
            if (informationType == DbProviderManifest.StoreSchemaDefinitionVersion3 ||
                informationType == DbProviderManifest.StoreSchemaDefinition)
            {
                return GetStoreSchemaDescription(informationType);
            }

            if (informationType == DbProviderManifest.StoreSchemaMappingVersion3 ||
                informationType == DbProviderManifest.StoreSchemaMapping)
            {
                return GetStoreSchemaMapping(informationType);
            }

            // Use default Conceptual Schema Definition
            if (informationType == DbProviderManifest.ConceptualSchemaDefinitionVersion3 ||
                informationType == DbProviderManifest.ConceptualSchemaDefinition)
            {
                return null;
            }

            throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.ProviderReturnedNullForGetDbInformation(informationType));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public override System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> GetStoreTypes()
        {
            if (this._primitiveTypes == null)
            {
                if (this._version == SqlVersion.Sql10)
                {
                    this._primitiveTypes = base.GetStoreTypes();
                }
                else
                {
                    List<PrimitiveType> primitiveTypes = new List<PrimitiveType>(base.GetStoreTypes());
                    Debug.Assert((this._version == SqlVersion.Sql8) || (this._version == SqlVersion.Sql9), "Found verion other than Sql 8, 9 or 10");
                    //Remove the Katmai types for both Sql8 and Sql9
                    primitiveTypes.RemoveAll(new Predicate<PrimitiveType>(
                                                        delegate(PrimitiveType primitiveType)
                                                        {
                                                            string name = primitiveType.Name.ToLowerInvariant();
                                                            return name.Equals("time", StringComparison.Ordinal) || 
                                                                   name.Equals("date", StringComparison.Ordinal) || 
                                                                   name.Equals("datetime2", StringComparison.Ordinal) || 
                                                                   name.Equals("datetimeoffset", StringComparison.Ordinal) ||
                                                                   name.Equals("geography", StringComparison.Ordinal) || 
                                                                   name.Equals("geometry", StringComparison.Ordinal);
                                                        }
                                                    )
                                        );
                    //Remove the types that won't work in Sql8
                    if (this._version == SqlVersion.Sql8)                    {
                        
                        // SQLBUDT 550667 and 551271: Remove xml and 'max' types for SQL Server 2000
                        primitiveTypes.RemoveAll(new Predicate<PrimitiveType>(
                                                            delegate(PrimitiveType primitiveType)
                                                            {
                                                                string name = primitiveType.Name.ToLowerInvariant();
                                                                return name.Equals("xml", StringComparison.Ordinal) || name.EndsWith("(max)", StringComparison.Ordinal);
                                                            }
                                                        )
                                            );                        
                    }
                    this._primitiveTypes = primitiveTypes.AsReadOnly();
                }
            }

            return this._primitiveTypes;
        }

        public override System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetStoreFunctions()
        {
            if (this._functions == null)
            {
                if (this._version == SqlVersion.Sql10)
                {
                    this._functions = base.GetStoreFunctions();
                }
                else
                {
                    //Remove the functions over katmai types from both Sql 9 and Sql 8.
                    IEnumerable<EdmFunction> functions = base.GetStoreFunctions().Where(f => !IsKatmaiOrNewer(f));
                    if(this._version == SqlVersion.Sql8)
                    {
                        // SQLBUDT 550998: Remove unsupported overloads from Provider Manifest on SQL 8.0
                        functions = functions.Where(f => !IsYukonOrNewer(f));      
                    }
                    this._functions = functions.ToList().AsReadOnly();
                }
            }

            return this._functions;
        }

        private static bool IsKatmaiOrNewer(EdmFunction edmFunction)
        {
            // Spatial types are only supported from Katmai onward; any functions using them must therefore also be Katmai or newer.
            if ((edmFunction.ReturnParameter != null && Helper.IsSpatialType(edmFunction.ReturnParameter.TypeUsage)) ||
                edmFunction.Parameters.Any(p => Helper.IsSpatialType(p.TypeUsage)))
            {
                return true;
            }

            ReadOnlyMetadataCollection<FunctionParameter> funParams = edmFunction.Parameters;
            switch (edmFunction.Name.ToUpperInvariant())
            {
                case "COUNT":
                case "COUNT_BIG":
                case "MAX":
                case "MIN":
                    {
                        string name = ((CollectionType)funParams[0].TypeUsage.EdmType).TypeUsage.EdmType.Name;
                        return ((name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase)) ||
                               (name.Equals("Time", StringComparison.OrdinalIgnoreCase)));

                    }
                case "DAY":
                case "MONTH":
                case "YEAR":
                case "DATALENGTH":
                case "CHECKSUM":
                    {
                        string name = funParams[0].TypeUsage.EdmType.Name;
                        return ((name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase)) ||
                               (name.Equals("Time", StringComparison.OrdinalIgnoreCase)));

                    }
                case "DATEADD":
                case "DATEDIFF":
                    {
                        string param1Name = funParams[1].TypeUsage.EdmType.Name;
                        string param2Name = funParams[2].TypeUsage.EdmType.Name;
                        return ((param1Name.Equals("Time", StringComparison.OrdinalIgnoreCase)) ||
                               (param2Name.Equals("Time", StringComparison.OrdinalIgnoreCase)) ||
                               (param1Name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase)) ||
                               (param2Name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase)));
                    }
                case "DATENAME":
                case "DATEPART":
                    {
                        string name = funParams[1].TypeUsage.EdmType.Name;
                        return ((name.Equals("DateTimeOffset", StringComparison.OrdinalIgnoreCase)) ||
                               (name.Equals("Time", StringComparison.OrdinalIgnoreCase)));
                    }
                case "SYSUTCDATETIME":
                case "SYSDATETIME":
                case "SYSDATETIMEOFFSET":
                    return true;
                default:
                    break;
            }

            return false;
        }

        private static bool IsYukonOrNewer(EdmFunction edmFunction)
        {
            ReadOnlyMetadataCollection<FunctionParameter> funParams = edmFunction.Parameters;
            if (funParams == null || funParams.Count == 0)
            {
                return false;
            }

            switch (edmFunction.Name.ToUpperInvariant())
            {
                case "COUNT":
                case "COUNT_BIG":
                    {
                        string name = ((CollectionType)funParams[0].TypeUsage.EdmType).TypeUsage.EdmType.Name;
                        return name.Equals("Guid", StringComparison.OrdinalIgnoreCase);
                    }

                case "CHARINDEX":
                    {
                        foreach (FunctionParameter funParam in funParams)
                        {
                            if (funParam.TypeUsage.EdmType.Name.Equals("Int64", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                    break;

                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// This method takes a type and a set of facets and returns the best mapped equivalent type 
        /// in EDM.
        /// </summary>
        /// <param name="storeType">A TypeUsage encapsulating a store type and a set of facets</param>
        /// <returns>A TypeUsage encapsulating an EDM type and a set of facets</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public override TypeUsage GetEdmType(TypeUsage storeType)
        {
            EntityUtil.CheckArgumentNull<TypeUsage>(storeType, "storeType");

            string storeTypeName = storeType.EdmType.Name.ToLowerInvariant();
            if (!base.StoreTypeNameToEdmPrimitiveType.ContainsKey(storeTypeName))
            {
                throw EntityUtil.Argument(Strings.ProviderDoesNotSupportType(storeTypeName));
            }

            PrimitiveType edmPrimitiveType = base.StoreTypeNameToEdmPrimitiveType[storeTypeName];

            int maxLength = 0;
            bool isUnicode = true;
            bool isFixedLen = false;
            bool isUnbounded = true;

            PrimitiveTypeKind newPrimitiveTypeKind;

            switch (storeTypeName)
            {
                // for some types we just go with simple type usage with no facets
                case "tinyint":
                case "smallint":
                case "bigint":
                case "bit":
                case "uniqueidentifier":
                case "int":
                case "geography":
                case "geometry":
                    return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);

                case "varchar":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isUnicode = false;
                    isFixedLen = false;
                    break;

                case "char":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isUnicode = false;
                    isFixedLen = true;
                    break;

                case "nvarchar":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isUnicode = true;
                    isFixedLen = false;
                    break;

                case "nchar":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isUnicode = true;
                    isFixedLen = true;
                    break;

                case "varchar(max)":
                case "text":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = true;
                    isUnicode = false;
                    isFixedLen = false;
                    break;

                case "nvarchar(max)":
                case "ntext":
                case "xml":
                    newPrimitiveTypeKind = PrimitiveTypeKind.String;
                    isUnbounded = true;
                    isUnicode = true;
                    isFixedLen = false;
                    break;

                case "binary":
                    newPrimitiveTypeKind = PrimitiveTypeKind.Binary;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isFixedLen = true;
                    break;

                case "varbinary":
                    newPrimitiveTypeKind = PrimitiveTypeKind.Binary;
                    isUnbounded = !TypeHelpers.TryGetMaxLength(storeType, out maxLength);
                    isFixedLen = false;
                    break;

                case "varbinary(max)":
                case "image":
                    newPrimitiveTypeKind = PrimitiveTypeKind.Binary;
                    isUnbounded = true;
                    isFixedLen = false;
                    break;

                case "timestamp":
                case "rowversion":
                    return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, true, 8);

                case "float":
                case "real":
                    return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);

                case "decimal":
                case "numeric":
                    {
                        byte precision;
                        byte scale;
                        if (TypeHelpers.TryGetPrecision(storeType, out precision) && TypeHelpers.TryGetScale(storeType, out scale))
                        {
                            return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType, precision, scale);
                        }
                        else
                        {
                            return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType);
                        }
                    }

                case "money":
                    return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType, 19, 4);

                case "smallmoney":
                    return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType, 10, 4);

                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return TypeUsage.CreateDateTimeTypeUsage(edmPrimitiveType, null);
                case "date":
                    return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);
                case "time":
                    return TypeUsage.CreateTimeTypeUsage(edmPrimitiveType, null);
                case "datetimeoffset":
                    return TypeUsage.CreateDateTimeOffsetTypeUsage(edmPrimitiveType, null);

                default:
                    throw EntityUtil.NotSupported(Strings.ProviderDoesNotSupportType(storeTypeName));
            }

            Debug.Assert(newPrimitiveTypeKind == PrimitiveTypeKind.String || newPrimitiveTypeKind == PrimitiveTypeKind.Binary, "at this point only string and binary types should be present");

            switch(newPrimitiveTypeKind)
            {
                case PrimitiveTypeKind.String:
                    if (!isUnbounded)
                    {
                        return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, isUnicode, isFixedLen, maxLength);
                    }
                    else
                    {
                        return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, isUnicode, isFixedLen);
                    }
                case PrimitiveTypeKind.Binary:
                    if (!isUnbounded)
                    {
                        return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, isFixedLen, maxLength);
                    }
                    else
                    {
                        return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, isFixedLen);
                    }
                default:
                    throw EntityUtil.NotSupported(Strings.ProviderDoesNotSupportType(storeTypeName));
            }
        }

        /// <summary>
        /// This method takes a type and a set of facets and returns the best mapped equivalent type 
        /// in SQL Server, taking the store version into consideration.
        /// </summary>
        /// <param name="storeType">A TypeUsage encapsulating an EDM type and a set of facets</param>
        /// <returns>A TypeUsage encapsulating a store type and a set of facets</returns>
        public override TypeUsage GetStoreType(TypeUsage edmType)
        {
            EntityUtil.CheckArgumentNull<TypeUsage>(edmType, "edmType");
            System.Diagnostics.Debug.Assert(edmType.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType);

            PrimitiveType primitiveType = edmType.EdmType as PrimitiveType;
            if (primitiveType == null)
            {
                throw EntityUtil.Argument(Strings.ProviderDoesNotSupportType(edmType.Identity));
            }

            ReadOnlyMetadataCollection<Facet> facets = edmType.Facets;

            switch (primitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Boolean:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["bit"]);

                case PrimitiveTypeKind.Byte:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["tinyint"]);

                case PrimitiveTypeKind.Int16:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["smallint"]);

                case PrimitiveTypeKind.Int32:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["int"]);

                case PrimitiveTypeKind.Int64:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["bigint"]);

                case PrimitiveTypeKind.Geography:
                case PrimitiveTypeKind.GeographyPoint:
                case PrimitiveTypeKind.GeographyLineString:
                case PrimitiveTypeKind.GeographyPolygon:
                case PrimitiveTypeKind.GeographyMultiPoint:
                case PrimitiveTypeKind.GeographyMultiLineString:
                case PrimitiveTypeKind.GeographyMultiPolygon:
                case PrimitiveTypeKind.GeographyCollection:
                    return GetStorePrimitiveTypeIfPostSql9("geography", edmType.Identity, primitiveType.PrimitiveTypeKind);

                case PrimitiveTypeKind.Geometry:
                case PrimitiveTypeKind.GeometryPoint:
                case PrimitiveTypeKind.GeometryLineString:
                case PrimitiveTypeKind.GeometryPolygon:
                case PrimitiveTypeKind.GeometryMultiPoint:
                case PrimitiveTypeKind.GeometryMultiLineString:
                case PrimitiveTypeKind.GeometryMultiPolygon:
                case PrimitiveTypeKind.GeometryCollection:
                    return GetStorePrimitiveTypeIfPostSql9("geometry", edmType.Identity, primitiveType.PrimitiveTypeKind);

                case PrimitiveTypeKind.Guid:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["uniqueidentifier"]);

                case PrimitiveTypeKind.Double:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["float"]);

                case PrimitiveTypeKind.Single:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["real"]);

                case PrimitiveTypeKind.Decimal: // decimal, numeric, smallmoney, money
                    {
                        byte precision;
                        if (!TypeHelpers.TryGetPrecision(edmType, out precision))
                        {
                            precision = 18;
                        }

                        byte scale;
                        if (!TypeHelpers.TryGetScale(edmType, out scale))
                        {
                            scale = 0;
                        }
                        TypeUsage tu = TypeUsage.CreateDecimalTypeUsage(StoreTypeNameToStorePrimitiveType["decimal"], precision, scale);
                        return tu;
                    }

                case PrimitiveTypeKind.Binary: // binary, varbinary, varbinary(max), image, timestamp, rowversion
                    {
                        bool isFixedLength = null != facets[DbProviderManifest.FixedLengthFacetName].Value && (bool)facets[DbProviderManifest.FixedLengthFacetName].Value;                     
                        Facet f = facets[DbProviderManifest.MaxLengthFacetName];
                        bool isMaxLength = Helper.IsUnboundedFacetValue(f) || null == f.Value || (int)f.Value > binaryMaxSize;
                        int maxLength = !isMaxLength ? (int)f.Value : Int32.MinValue;

                        TypeUsage tu;
                        if (isFixedLength)
                        {
                            tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["binary"], true, (isMaxLength ? binaryMaxSize : maxLength));
                        }
                        else
                        {
                            if (isMaxLength)
                            {
                                if (_version != SqlVersion.Sql8)
                                {

                                    tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["varbinary(max)"], false);
                                    Debug.Assert(tu.Facets[DbProviderManifest.MaxLengthFacetName].Description.IsConstant, "varbinary(max) is not constant!");
                                }
                                else
                                {
                                    tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["varbinary"], false, binaryMaxSize);
                                }
                            }
                            else
                            {
                                tu = TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["varbinary"], false, maxLength);
                            }
                        }
                        return tu;
                    }

                case PrimitiveTypeKind.String:
                    // char, nchar, varchar, nvarchar, varchar(max), nvarchar(max), ntext, text, xml
                    {
                        bool isUnicode = null == facets[DbProviderManifest.UnicodeFacetName].Value || (bool)facets[DbProviderManifest.UnicodeFacetName].Value;
                        bool isFixedLength = null != facets[DbProviderManifest.FixedLengthFacetName].Value && (bool)facets[DbProviderManifest.FixedLengthFacetName].Value;
                        Facet f = facets[DbProviderManifest.MaxLengthFacetName];
                        // maxlen is true if facet value is unbounded, the value is bigger than the limited string sizes *or* the facet
                        // value is null. this is needed since functions still have maxlength facet value as null
                        bool isMaxLength = Helper.IsUnboundedFacetValue(f) || null == f.Value || (int)f.Value > (isUnicode ? nvarcharMaxSize : varcharMaxSize);
                        int maxLength = !isMaxLength ? (int)f.Value : Int32.MinValue;
                        
                        TypeUsage tu;

                        if (isUnicode)
                        {
                            if (isFixedLength)
                            {
                                tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["nchar"], true, true, (isMaxLength ? nvarcharMaxSize : maxLength));
                            }
                            else
                            {
                                if (isMaxLength)
                                {
                                    // nvarchar(max) (SQL 9) or ntext (SQL 8)
                                    if (_version != SqlVersion.Sql8)
                                    {
                                        tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["nvarchar(max)"], true, false);
                                        Debug.Assert(tu.Facets[DbProviderManifest.MaxLengthFacetName].Description.IsConstant, "NVarchar(max) is not constant!");
                                    }
                                    else
                                    {   
                                        // if it is unknown, fallback to nvarchar[4000] instead of ntext since it has limited store semantics
                                        tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["nvarchar"], true, false, nvarcharMaxSize);
                                    }
                                }
                                else
                                {
                                    tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["nvarchar"], true, false, maxLength);
                                }
                            }
                        }
                        else    // !isUnicode
                        {
                            if (isFixedLength)
                            {
                                tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["char"], false, true,
                                    (isMaxLength ? varcharMaxSize : maxLength));
                            }
                            else
                            {
                                if (isMaxLength)
                                {
                                    // nvarchar(max) (SQL 9) or ntext (SQL 8)
                                    if (_version != SqlVersion.Sql8)
                                    {
                                        tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["varchar(max)"], false, false);
                                        Debug.Assert(tu.Facets[DbProviderManifest.MaxLengthFacetName].Description.IsConstant, "varchar(max) is not constant!");
                                    }
                                    else
                                    {
                                        // if it is unknown, fallback to varchar[8000] instead of text since it has limited store semantics
                                        tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["varchar"], false, false, varcharMaxSize);
                                    }
                                }
                                else
                                {
                                    tu = TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["varchar"], false, false, maxLength);
                                }
                            }
                        }
                        return tu;
                    }


                case PrimitiveTypeKind.DateTime:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["datetime"]);
                case PrimitiveTypeKind.DateTimeOffset:
                    return GetStorePrimitiveTypeIfPostSql9("datetimeoffset", edmType.Identity, primitiveType.PrimitiveTypeKind); 
                case PrimitiveTypeKind.Time:
                    return GetStorePrimitiveTypeIfPostSql9("time", edmType.Identity, primitiveType.PrimitiveTypeKind);
                 
                default:
                    throw EntityUtil.NotSupported(Strings.NoStoreTypeForEdmType(edmType.Identity, primitiveType.PrimitiveTypeKind));
            }
        }

        private TypeUsage GetStorePrimitiveTypeIfPostSql9(string storeTypeName, string edmTypeIdentity, PrimitiveTypeKind primitiveTypeKind)
        {
            if ((this.SqlVersion != SqlVersion.Sql8) && (this.SqlVersion != SqlVersion.Sql9))
            {
                return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType[storeTypeName]);
            }
            else
            {
                throw EntityUtil.NotSupported(Strings.NoStoreTypeForEdmType(edmTypeIdentity, primitiveTypeKind));
            }
        }

        /// <summary>
        /// Returns true, SqlClient supports escaping strings to be used as arguments to like
        /// The escape character is '~'
        /// </summary>
        /// <param name="escapeCharacter">The character '~'</param>
        /// <returns>True</returns>
        public override bool SupportsEscapingLikeArgument(out char escapeCharacter)
        {
            escapeCharacter = SqlProviderManifest.LikeEscapeChar;
            return true;
        }

        /// <summary>
        /// Escapes the wildcard characters and the escape character in the given argument.
        /// </summary>
        /// <param name="argument"></param>
        /// <returns>Equivalent to the argument, with the wildcard characters and the escape character escaped</returns>
        public override string EscapeLikeArgument(string argument)
        {
            EntityUtil.CheckArgumentNull(argument, "argument");

            bool usedEscapeCharacter;
            return SqlProviderManifest.EscapeLikeText(argument, true, out usedEscapeCharacter);
        }
        #endregion
    }
}
