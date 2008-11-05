#if ENTITIES
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Xml;
using System.Data;

namespace Npgsql
{
    internal class NpgsqlProviderManifest : DbXmlEnabledProviderManifest
	{
        public NpgsqlProviderManifest(string serverVersion)
            : base(CreateXmlReaderForResource("Npgsql.NpgsqlProviderManifest.Manifest.xml"))
        {
        }

        protected override XmlReader GetDbInformation(string informationType)
        {
            XmlReader xmlReader = null;

            if (informationType == StoreSchemaDefinition)
            {
                xmlReader = CreateXmlReaderForResource("Npgsql.NpgsqlSchema.ssdl");
            }
            else if (informationType == StoreSchemaMapping)
            {
                xmlReader = CreateXmlReaderForResource("Npgsql.NpgsqlSchema.msl");
            }

            if (xmlReader == null)
                throw new ArgumentOutOfRangeException("informationType");

            return xmlReader;
        }

        private const string MaxLengthFacet = "MaxLength";
        private const string ScaleFacet = "Scale";
        private const string PrecisionFacet = "Precision";
        private const string FixedLengthFacet = "FixedLength";

        public override TypeUsage GetEdmType(TypeUsage storeType)
        {
            if (storeType == null)
                throw new ArgumentNullException("storeType");

            string storeTypeName = storeType.EdmType.Name;
            PrimitiveType primitiveType = StoreTypeNameToEdmPrimitiveType[storeTypeName];
            // TODO: come up with way to determin if unicode is used
            bool isUnicode = true;
            Facet facet;

            switch (storeTypeName)
            {
                case "bool":
                case "int2":
                case "int4":
                case "int8":
                case "float4":
                case "float8":
                case "uuid":
                    return TypeUsage.CreateDefaultTypeUsage(primitiveType);
                case "numeric":
                    {
                        byte scale;
                        byte precision;
                        if (storeType.Facets.TryGetValue(ScaleFacet, false, out facet) &&
                            !facet.IsUnbounded && facet.Value != null)
                        {
                            scale = (byte)facet.Value;
                            if (storeType.Facets.TryGetValue(PrecisionFacet, false, out facet) &&
                                !facet.IsUnbounded && facet.Value != null)
                            {
                                precision = (byte)facet.Value;
                                return TypeUsage.CreateDecimalTypeUsage(primitiveType, precision, scale);
                            }
                        }
                        return TypeUsage.CreateDecimalTypeUsage(primitiveType);
                    }
                case "bpchar":
                    if (storeType.Facets.TryGetValue(MaxLengthFacet, false, out facet) &&
                        !facet.IsUnbounded && facet.Value != null)
                        return TypeUsage.CreateStringTypeUsage(primitiveType, isUnicode, true, (int)facet.Value);
                    else
                        return TypeUsage.CreateStringTypeUsage(primitiveType, isUnicode, true);
                case "varchar":
                    if (storeType.Facets.TryGetValue(MaxLengthFacet, false, out facet) &&
                        !facet.IsUnbounded && facet.Value != null)
                        return TypeUsage.CreateStringTypeUsage(primitiveType, isUnicode, false, (int)facet.Value);
                    else
                        return TypeUsage.CreateStringTypeUsage(primitiveType, isUnicode, false);
                case "text":
                case "xml":
                    return TypeUsage.CreateStringTypeUsage(primitiveType, isUnicode, false);
                case "timestamp":
                    // TODO: make sure the arguments are correct here
                    if (storeType.Facets.TryGetValue(PrecisionFacet, false, out facet) &&
                        !facet.IsUnbounded && facet.Value != null)
                    {
                        return TypeUsage.CreateDateTimeTypeUsage(primitiveType, (byte)facet.Value);
                    }
                    else
                    {
                        return TypeUsage.CreateDateTimeTypeUsage(primitiveType, null);
                    }
                case "date":
                    return TypeUsage.CreateDateTimeTypeUsage(primitiveType, 0);
                case "bytea":
                    {
                        if (storeType.Facets.TryGetValue(MaxLengthFacet, false, out facet) &&
                            !facet.IsUnbounded && facet.Value != null)
                        {
                            return TypeUsage.CreateBinaryTypeUsage(primitiveType, false, (int)facet.Value);
                        }
                        return TypeUsage.CreateBinaryTypeUsage(primitiveType, false);
                    }
                    //TypeUsage.CreateBinaryTypeUsage
                    //TypeUsage.CreateDateTimeTypeUsage
                    //TypeUsage.CreateDecimalTypeUsage
                    //TypeUsage.CreateStringTypeUsage
            }
            throw new NotSupportedException();
        }

        public override TypeUsage GetStoreType(TypeUsage edmType)
        {
            if (edmType == null)
                throw new ArgumentNullException("edmType");

            PrimitiveType primitiveType = edmType.EdmType as PrimitiveType;
            if (primitiveType == null)
                throw new ArgumentException("Store does not support specified edm type");

            // TODO: come up with way to determin if unicode is used
            bool isUnicode = true;
            Facet facet;

            switch (primitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Boolean:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["bool"]);
                case PrimitiveTypeKind.Int16:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["int2"]);
                case PrimitiveTypeKind.Int32:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["int4"]);
                case PrimitiveTypeKind.Int64:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["int8"]);
                case PrimitiveTypeKind.Single:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["float4"]);
                case PrimitiveTypeKind.Double:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["float8"]);
                case PrimitiveTypeKind.Decimal:
                    {
                        byte scale;
                        byte precision;
                        if (edmType.Facets.TryGetValue(ScaleFacet, false, out facet) &&
                            !facet.IsUnbounded && facet.Value != null)
                        {
                            scale = (byte)facet.Value;
                            if (edmType.Facets.TryGetValue(PrecisionFacet, false, out facet) &&
                                !facet.IsUnbounded && facet.Value != null)
                            {
                                precision = (byte)facet.Value;
                                return TypeUsage.CreateDecimalTypeUsage(StoreTypeNameToStorePrimitiveType["numeric"], precision, scale);
                            }
                        }
                        return TypeUsage.CreateDecimalTypeUsage(StoreTypeNameToStorePrimitiveType["numeric"]);
                    }
                case PrimitiveTypeKind.String:
                    {
                        // TODO: could get character, character varying, text
                        if (edmType.Facets.TryGetValue(FixedLengthFacet, false, out facet) &&
                            !facet.IsUnbounded && facet.Value != null)
                        {
                            PrimitiveType characterPrimitive = StoreTypeNameToStorePrimitiveType["bpchar"];
                            if (edmType.Facets.TryGetValue(MaxLengthFacet, false, out facet) &&
                                !facet.IsUnbounded && facet.Value != null)
                            {
                                return TypeUsage.CreateStringTypeUsage(characterPrimitive, isUnicode, true, (int)facet.Value);
                            }
                            // this may not work well
                            return TypeUsage.CreateStringTypeUsage(characterPrimitive, isUnicode, true);
                        }
                        if (edmType.Facets.TryGetValue(MaxLengthFacet, false, out facet) &&
                            !facet.IsUnbounded && facet.Value != null)
                        {
                            return TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["varchar"], isUnicode, false, (int)facet.Value);
                        }
                        // assume text since it is not fixed length and has no max length
                        return TypeUsage.CreateStringTypeUsage(StoreTypeNameToStorePrimitiveType["text"], isUnicode, false);
                    }
                case PrimitiveTypeKind.DateTime:
                    if (edmType.Facets.TryGetValue(PrecisionFacet, false, out facet) &&
                        !facet.IsUnbounded && facet.Value != null)
                    {
                        return TypeUsage.CreateDateTimeTypeUsage(StoreTypeNameToStorePrimitiveType["timestamp"], (byte)facet.Value);
                    }
                    else
                    {
                        return TypeUsage.CreateDateTimeTypeUsage(StoreTypeNameToStorePrimitiveType["timestamp"], null);
                    }
                case PrimitiveTypeKind.Binary:
                    {
                        if (edmType.Facets.TryGetValue(MaxLengthFacet, false, out facet) &&
                            !facet.IsUnbounded && facet.Value != null)
                        {
                            return TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["bytea"], false, (int)facet.Value);
                        }
                        return TypeUsage.CreateBinaryTypeUsage(StoreTypeNameToStorePrimitiveType["bytea"], false);
                    }
                case PrimitiveTypeKind.Guid:
                    return TypeUsage.CreateDefaultTypeUsage(StoreTypeNameToStorePrimitiveType["uuid"]);
                // notably missing
                // PrimitiveTypeKind.Byte:
                // PrimitiveTypeKind.SByte:
                // PrimitiveTypeKind.DateTimeOffset:
                // PrimitiveTypeKind.Time:
            }

            throw new NotSupportedException();
        }

        private static XmlReader CreateXmlReaderForResource(string resourceName)
        {
            return XmlReader.Create(System.Reflection.Assembly.GetAssembly(typeof(NpgsqlProviderManifest)).GetManifestResourceStream(resourceName));
        }
    }
}
#endif