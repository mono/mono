//------------------------------------------------------------------------------
// <copyright file="DbCommandDefinition.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Data.Metadata.Edm;

namespace System.Data.Common {

    /// <summary>
    /// A prepared command definition, can be cached and reused to avoid 
    /// repreparing a command.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public class DbCommandDefinition {

        private readonly ICloneable _prototype;

        /// <summary>
        /// Internal factory method to create the default Command Definition object
        /// based on a prototype command. The prototype command is cloned 
        /// before the protected constructor is invoked
        /// </summary>
        /// <param name="prototype">prototype DbCommand</param>
        /// <returns>the DbCommandDefinition</returns>
        internal static DbCommandDefinition CreateCommandDefinition(DbCommand prototype) {
            EntityUtil.CheckArgumentNull(prototype, "prototype");
            ICloneable cloneablePrototype = prototype as ICloneable;
            if (null == cloneablePrototype) {
                throw EntityUtil.CannotCloneStoreProvider();
            }
            DbCommand clonedPrototype = (DbCommand)(cloneablePrototype.Clone());
            return new DbCommandDefinition(clonedPrototype);
        }

        /// <summary>
        /// Protected constructor; the command is assumed to be a prototype
        /// that will be cloned on CreateCommand, and the cloned command will be executed.
        /// </summary>
        protected DbCommandDefinition(DbCommand prototype) {
            EntityUtil.CheckArgumentNull(prototype, "prototype");
            _prototype = prototype as ICloneable;
            if (null == _prototype) {
                throw EntityUtil.CannotCloneStoreProvider();
            }
        }

        /// <summary>
        /// Constructor overload for subclasses to use
        /// </summary>
        protected DbCommandDefinition() {
        }

        /// <summary>
        /// Create a DbCommand object from the definition, that can be executed.
        /// </summary>
        /// <returns></returns>
        public virtual DbCommand CreateCommand() {
            return (DbCommand)(_prototype.Clone());
        }

        internal static void PopulateParameterFromTypeUsage(DbParameter parameter, TypeUsage type, bool isOutParam)
        {
            EntityUtil.CheckArgumentNull(parameter, "parameter");
            EntityUtil.CheckArgumentNull(type, "type");
            
            // parameter.IsNullable - from the NullableConstraintAttribute value
            parameter.IsNullable = TypeSemantics.IsNullable(type);
            
            // parameter.ParameterName - set by the caller;
            // parameter.SourceColumn - not applicable until we have a data adapter;
            // parameter.SourceColumnNullMapping - not applicable until we have a data adapter;
            // parameter.SourceVersion - not applicable until we have a data adapter;
            // parameter.Value - left unset;
            // parameter.DbType - determined by the TypeMapping;
            // parameter.Precision - from the TypeMapping;
            // parameter.Scale - from the TypeMapping;
            // parameter.Size - from the TypeMapping;


            // type.EdmType may not be a primitive type here - e.g. the user specified
            // a complex or entity type when creating an ObjectParameter instance. To keep 
            // the same behavior we had in previous versions we let it through here. We will 
            // throw an exception later when actually invoking the stored procedure where we
            // don't allow parameters that are non-primitive.
            if(Helper.IsPrimitiveType(type.EdmType))
            {
                DbType dbType;
                if (TryGetDbTypeFromPrimitiveType((PrimitiveType)type.EdmType, out dbType))
                {
                    switch (dbType)
                    {
                        case DbType.Binary:
                            PopulateBinaryParameter(parameter, type, dbType, isOutParam);
                            break;
                        case DbType.DateTime:
                        case DbType.Time:
                        case DbType.DateTimeOffset:
                            PopulateDateTimeParameter(parameter, type, dbType);
                            break;
                        case DbType.Decimal:
                            PopulateDecimalParameter(parameter, type, dbType);
                            break;
                        case DbType.String:
                            PopulateStringParameter(parameter, type, isOutParam);
                            break;
                        default:
                            parameter.DbType = dbType;
                            break;
                    }
                }
            }
        }

        internal static bool TryGetDbTypeFromPrimitiveType(PrimitiveType type, out DbType dbType)
        {
            switch (type.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Binary:
                    dbType = DbType.Binary; 
                    return true;
                case PrimitiveTypeKind.Boolean:
                    dbType = DbType.Boolean; 
                    return true;
                case PrimitiveTypeKind.Byte:
                    dbType = DbType.Byte; 
                    return true;
                case PrimitiveTypeKind.DateTime:
                    dbType = DbType.DateTime; 
                    return true;
                case PrimitiveTypeKind.Time:
                    dbType = DbType.Time; 
                    return true;
                case PrimitiveTypeKind.DateTimeOffset:
                    dbType = DbType.DateTimeOffset; 
                    return true;
                case PrimitiveTypeKind.Decimal:
                    dbType = DbType.Decimal; 
                    return true;
                case PrimitiveTypeKind.Double:
                    dbType = DbType.Double; 
                    return true;
                case PrimitiveTypeKind.Guid:
                    dbType = DbType.Guid; 
                    return true;
                case PrimitiveTypeKind.Single:
                    dbType = DbType.Single; 
                    return true;
                case PrimitiveTypeKind.SByte:
                    dbType = DbType.SByte; 
                    return true;
                case PrimitiveTypeKind.Int16:
                    dbType = DbType.Int16; 
                    return true;
                case PrimitiveTypeKind.Int32:
                    dbType = DbType.Int32; 
                    return true;
                case PrimitiveTypeKind.Int64:
                    dbType = DbType.Int64; 
                    return true;
                case PrimitiveTypeKind.String:
                    dbType = DbType.String; 
                    return true;
                default:
                    dbType = default(DbType);
                    return  false;
            }
        }

        private static void PopulateBinaryParameter(DbParameter parameter, TypeUsage type, DbType dbType, bool isOutParam)
        {
            parameter.DbType = dbType;

            // For each facet, set the facet value only if we have it, note that it's possible to not have
            // it in the case the facet value is null
            SetParameterSize(parameter, type, isOutParam);
        }

        private static void PopulateDecimalParameter (DbParameter parameter, TypeUsage type, DbType dbType)
        {
            parameter.DbType = dbType;
            IDbDataParameter dataParameter = (IDbDataParameter)parameter;

            // For each facet, set the facet value only if we have it, note that it's possible to not have
            // it in the case the facet value is null
            byte precision;
            byte scale;
            if (TypeHelpers.TryGetPrecision(type, out precision))
            {
                dataParameter.Precision = precision;
            }

            if (TypeHelpers.TryGetScale(type, out scale))
            {
                dataParameter.Scale = scale;
            }
        }

        private static void PopulateDateTimeParameter(DbParameter parameter, TypeUsage type, DbType dbType)
        {
            parameter.DbType = dbType;
            IDbDataParameter dataParameter = (IDbDataParameter)parameter;

            // For each facet, set the facet value only if we have it, note that it's possible to not have
            // it in the case the facet value is null
            byte precision;
            if (TypeHelpers.TryGetPrecision(type, out precision))
            {
                dataParameter.Precision = precision;
            }
        }


        private static void PopulateStringParameter(DbParameter parameter, TypeUsage type, bool isOutParam)
        {
            // For each facet, set the facet value only if we have it, note that it's possible to not have
            // it in the case the facet value is null
            bool unicode = true;
            bool fixedLength = false;

            if (!TypeHelpers.TryGetIsFixedLength(type, out fixedLength))
            {
                // If we can't get the fixed length facet value, then default to fixed length = false
                fixedLength = false;
            }

            if (!TypeHelpers.TryGetIsUnicode(type, out unicode))
            {
                // If we can't get the unicode facet value, then default to unicode = true
                unicode = true;
            }

            if (fixedLength)
            {
                parameter.DbType = (unicode ? DbType.StringFixedLength : DbType.AnsiStringFixedLength);
            }
            else
            {
                parameter.DbType = (unicode ? DbType.String : DbType.AnsiString);
            }

            SetParameterSize(parameter, type, isOutParam);
        }

        private static void SetParameterSize(DbParameter parameter, TypeUsage type, bool isOutParam)
        {
            // only set the size if the parameter has a specific size value.
            Facet maxLengthFacet;
            if (type.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, true, out maxLengthFacet) && maxLengthFacet.Value != null)
            {
                // only set size if there is a specific size
                if (!Helper.IsUnboundedFacetValue(maxLengthFacet))
                {
                    parameter.Size = (int)maxLengthFacet.Value;
                }
                else if (isOutParam)
                {
                    // if it is store procedure parameter and it is unbounded set the size to max
                    parameter.Size = Int32.MaxValue;
                }
            }
        }
    }
}
