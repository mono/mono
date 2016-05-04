//---------------------------------------------------------------------
// <copyright file="DbTypeMap.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace System.Data.Common.Internal
{
    /// <summary>
    /// Provides singleton model TypeUsages for each DbType that can be expressed using a supported EDM type and appropriate facet values.
    /// Used by EntityParameter.GetTypeUsage - if you add additional TypeUsage fields here, review the impact on that method.
    /// </summary>
    internal static class DbTypeMap
    {
        internal static readonly TypeUsage AnsiString = CreateType(PrimitiveTypeKind.String, new FacetValues { Unicode = false, FixedLength = false, MaxLength = (int?)null });
        internal static readonly TypeUsage AnsiStringFixedLength = CreateType(PrimitiveTypeKind.String, new FacetValues { Unicode = false, FixedLength = true, MaxLength = (int?)null });
        internal static readonly TypeUsage String = CreateType(PrimitiveTypeKind.String, new FacetValues { Unicode = true, FixedLength = false, MaxLength = (int?)null });
        internal static readonly TypeUsage StringFixedLength = CreateType(PrimitiveTypeKind.String, new FacetValues { Unicode = true, FixedLength = true, MaxLength = (int?)null });
        // SQLBUDT #514204 - EntityCommand: XML parameter size must be ignored
        /* XML parameters must not have a explicit size */
        internal static readonly TypeUsage Xml = CreateType(PrimitiveTypeKind.String, new FacetValues { Unicode = true, FixedLength = false, MaxLength = (int?)null });
        internal static readonly TypeUsage Binary = CreateType(PrimitiveTypeKind.Binary , new FacetValues { MaxLength = (int?)null });
        internal static readonly TypeUsage Boolean = CreateType(PrimitiveTypeKind.Boolean);
        internal static readonly TypeUsage Byte = CreateType(PrimitiveTypeKind.Byte);
        internal static readonly TypeUsage DateTime = CreateType(PrimitiveTypeKind.DateTime);
        internal static readonly TypeUsage Date = CreateType(PrimitiveTypeKind.DateTime);
        internal static readonly TypeUsage DateTime2 = CreateType(PrimitiveTypeKind.DateTime, new FacetValues { Precision = (byte?)null });
        internal static readonly TypeUsage Time = CreateType(PrimitiveTypeKind.Time, new FacetValues { Precision = (byte?)null });
        internal static readonly TypeUsage DateTimeOffset = CreateType(PrimitiveTypeKind.DateTimeOffset, new FacetValues { Precision = (byte?)null });
        // For decimal and money, in the case of precision == 0, we don't want any facets when picking the type so the
        // default type should be picked    
        internal static readonly TypeUsage Decimal = CreateType(PrimitiveTypeKind.Decimal, new FacetValues { Precision = (byte?)null, Scale = (byte?)null });
        // SQLBU 480928: Need to make currency a separate case once we enable money type
        internal static readonly TypeUsage Currency = CreateType(PrimitiveTypeKind.Decimal, new FacetValues { Precision = (byte?)null, Scale = (byte?)null });
        internal static readonly TypeUsage Double = CreateType(PrimitiveTypeKind.Double);
        internal static readonly TypeUsage Guid = CreateType(PrimitiveTypeKind.Guid);
        internal static readonly TypeUsage Int16 = CreateType(PrimitiveTypeKind.Int16);
        internal static readonly TypeUsage Int32 = CreateType(PrimitiveTypeKind.Int32);
        internal static readonly TypeUsage Int64 = CreateType(PrimitiveTypeKind.Int64);
        internal static readonly TypeUsage Single = CreateType(PrimitiveTypeKind.Single);
        internal static readonly TypeUsage SByte = CreateType(PrimitiveTypeKind.SByte);

        internal static bool TryGetModelTypeUsage(DbType dbType, out TypeUsage modelType)
        {
            switch(dbType)
            {
                case DbType.AnsiString:
                    modelType = DbTypeMap.AnsiString;
                    break;

                case DbType.AnsiStringFixedLength:
                    modelType = DbTypeMap.AnsiStringFixedLength;
                    break;

                case DbType.String:
                    modelType = DbTypeMap.String;
                    break;

                case DbType.StringFixedLength:
                    modelType = DbTypeMap.StringFixedLength;
                    break;

                case DbType.Xml:
                    modelType = DbTypeMap.Xml;
                    break;

                case DbType.Binary:
                    modelType = DbTypeMap.Binary;
                    break;

                case DbType.Boolean:
                    modelType = DbTypeMap.Boolean;
                    break;

                case DbType.Byte:
                    modelType = DbTypeMap.Byte;
                    break;

                case DbType.DateTime:
                    modelType = DbTypeMap.DateTime;
                    break;

                case DbType.Date:
                    modelType = DbTypeMap.Date;
                    break;

                case DbType.DateTime2:
                    modelType = DbTypeMap.DateTime2;
                    break;

                case DbType.Time:
                    modelType = DbTypeMap.Time;
                    break;

                case DbType.DateTimeOffset:
                    modelType = DbTypeMap.DateTimeOffset;
                    break;

                case DbType.Decimal:
                    modelType = DbTypeMap.Decimal;
                    break;

                case DbType.Currency:
                    modelType = DbTypeMap.Currency;
                    break;

                case DbType.Double:
                    modelType = DbTypeMap.Double;
                    break;

                case DbType.Guid:
                    modelType = DbTypeMap.Guid;
                    break;

                case DbType.Int16:
                    modelType = DbTypeMap.Int16;
                    break;

                case DbType.Int32:
                    modelType = DbTypeMap.Int32;
                    break;

                case DbType.Int64:
                    modelType = DbTypeMap.Int64;
                    break;

                case DbType.Single:
                    modelType = DbTypeMap.Single;
                    break;

                case DbType.SByte:
                    modelType = DbTypeMap.SByte;
                    break;

                case DbType.VarNumeric:
                    modelType = null;
                    break;

                default:
                    modelType = null;
                    break;
            }

            return (modelType != null);
        }

        private static TypeUsage CreateType(PrimitiveTypeKind type)
        {
            return CreateType(type, new FacetValues());
        }

        private static TypeUsage CreateType(PrimitiveTypeKind type, FacetValues facets)
        {
            PrimitiveType primitiveType = EdmProviderManifest.Instance.GetPrimitiveType(type);
            TypeUsage typeUsage = TypeUsage.Create(primitiveType, facets);
            return typeUsage;
        }       
    }
}
