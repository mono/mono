//------------------------------------------------------------------------------
// <copyright file="MetaDataUtilsSmi.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace Microsoft.SqlServer.Server {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Sql;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Diagnostics;


    // Utilities for manipulating smi-related metadata.
    //
    //  THIS CLASS IS BUILT ON TOP OF THE SMI INTERFACE -- SMI SHOULD NOT DEPEND ON IT!
    //
    //  These are all based off of knowing the clr type of the value
    //  as an ExtendedClrTypeCode enum for rapid access (lookup in static array is best, if possible).
    internal class MetaDataUtilsSmi {

        internal const SqlDbType    InvalidSqlDbType = (SqlDbType) (-1);
        internal const long         InvalidMaxLength = -2;

        // Standard type inference map to get SqlDbType when all you know is the value's type (typecode)
        //  This map's index is off by one (add one to typecode locate correct entry) in order 
        //  support ExtendedSqlDbType.Invalid
        // ONLY ACCESS THIS ARRAY FROM InferSqlDbTypeFromTypeCode!!!
        static readonly SqlDbType[] __extendedTypeCodeToSqlDbTypeMap = {
            InvalidSqlDbType,               // Invalid extended type code
            SqlDbType.Bit,                  // System.Boolean
            SqlDbType.TinyInt,              // System.Byte
            SqlDbType.NVarChar,             // System.Char
            SqlDbType.DateTime,             // System.DateTime
            InvalidSqlDbType,               // System.DBNull doesn't have an inferable SqlDbType
            SqlDbType.Decimal,              // System.Decimal
            SqlDbType.Float,                // System.Double
            InvalidSqlDbType,               // null reference doesn't have an inferable SqlDbType
            SqlDbType.SmallInt,             // System.Int16
            SqlDbType.Int,                  // System.Int32
            SqlDbType.BigInt,               // System.Int64
            InvalidSqlDbType,               // System.SByte doesn't have an inferable SqlDbType
            SqlDbType.Real,                 // System.Single
            SqlDbType.NVarChar,             // System.String
            InvalidSqlDbType,               // System.UInt16 doesn't have an inferable SqlDbType
            InvalidSqlDbType,               // System.UInt32 doesn't have an inferable SqlDbType
            InvalidSqlDbType,               // System.UInt64 doesn't have an inferable SqlDbType
            InvalidSqlDbType,               // System.Object doesn't have an inferable SqlDbType
            SqlDbType.VarBinary,            // System.ByteArray
            SqlDbType.NVarChar,             // System.CharArray
            SqlDbType.UniqueIdentifier,     // System.Guid
            SqlDbType.VarBinary,            // System.Data.SqlTypes.SqlBinary
            SqlDbType.Bit,                  // System.Data.SqlTypes.SqlBoolean
            SqlDbType.TinyInt,              // System.Data.SqlTypes.SqlByte
            SqlDbType.DateTime,             // System.Data.SqlTypes.SqlDateTime
            SqlDbType.Float,                // System.Data.SqlTypes.SqlDouble
            SqlDbType.UniqueIdentifier,     // System.Data.SqlTypes.SqlGuid
            SqlDbType.SmallInt,             // System.Data.SqlTypes.SqlInt16
            SqlDbType.Int,                  // System.Data.SqlTypes.SqlInt32
            SqlDbType.BigInt,               // System.Data.SqlTypes.SqlInt64
            SqlDbType.Money,                // System.Data.SqlTypes.SqlMoney
            SqlDbType.Decimal,              // System.Data.SqlTypes.SqlDecimal
            SqlDbType.Real,                 // System.Data.SqlTypes.SqlSingle
            SqlDbType.NVarChar,             // System.Data.SqlTypes.SqlString
            SqlDbType.NVarChar,             // System.Data.SqlTypes.SqlChars
            SqlDbType.VarBinary,            // System.Data.SqlTypes.SqlBytes
            SqlDbType.Xml,                  // System.Data.SqlTypes.SqlXml
            SqlDbType.Structured,           // System.Data.DataTable
            SqlDbType.Structured,           // System.Collections.IEnumerable, used for TVPs it must return IDataRecord
            SqlDbType.Structured,           // System.Collections.Generic.IEnumerable<Microsoft.SqlServer.Server.SqlDataRecord>
            SqlDbType.Time,                 // System.TimeSpan
            SqlDbType.DateTimeOffset,       // System.DateTimeOffset
        };

        // Hash table to map from clr type object to ExtendedClrTypeCodeMap enum
        //  ONLY ACCESS THIS HASH TABLE FROM DetermineExtendedTypeCode METHOD!!!  (and class ctor for setup)
        static readonly Hashtable __typeToExtendedTypeCodeMap;


        // class ctor
        static MetaDataUtilsSmi() {
            // set up type mapping hash table
            //  keep this initialization list in the same order as ExtendedClrTypeCode for ease in validating!
            Hashtable ht = new Hashtable(42);
            ht.Add( typeof( System.Boolean ),       ExtendedClrTypeCode.Boolean );
            ht.Add( typeof( System.Byte ),          ExtendedClrTypeCode.Byte );
            ht.Add( typeof( System.Char ),          ExtendedClrTypeCode.Char );
            ht.Add( typeof( System.DateTime ),      ExtendedClrTypeCode.DateTime );
            ht.Add( typeof( System.DBNull ),        ExtendedClrTypeCode.DBNull );
            ht.Add( typeof( System.Decimal ),       ExtendedClrTypeCode.Decimal );
            ht.Add( typeof( System.Double ),        ExtendedClrTypeCode.Double );
            // lookup code will have to special-case null-ref anyway, so don't bother adding ExtendedTypeCode.Empty to the table
            ht.Add( typeof( System.Int16 ),         ExtendedClrTypeCode.Int16 );
            ht.Add( typeof( System.Int32 ),         ExtendedClrTypeCode.Int32 );
            ht.Add( typeof( System.Int64 ),         ExtendedClrTypeCode.Int64 );
            ht.Add( typeof( System.SByte ),         ExtendedClrTypeCode.SByte );
            ht.Add( typeof( System.Single ),        ExtendedClrTypeCode.Single );
            ht.Add( typeof( System.String ),        ExtendedClrTypeCode.String );
            ht.Add( typeof( System.UInt16 ),        ExtendedClrTypeCode.UInt16 );
            ht.Add( typeof( System.UInt32 ),        ExtendedClrTypeCode.UInt32 );
            ht.Add( typeof( System.UInt64 ),        ExtendedClrTypeCode.UInt64 );
            ht.Add( typeof( System.Object ),        ExtendedClrTypeCode.Object );
            ht.Add( typeof( System.Byte[] ),        ExtendedClrTypeCode.ByteArray );
            ht.Add( typeof( System.Char[] ),        ExtendedClrTypeCode.CharArray );
            ht.Add( typeof( System.Guid ),          ExtendedClrTypeCode.Guid );
            ht.Add( typeof( SqlBinary ),            ExtendedClrTypeCode.SqlBinary );
            ht.Add( typeof( SqlBoolean ),           ExtendedClrTypeCode.SqlBoolean );
            ht.Add( typeof( SqlByte ),              ExtendedClrTypeCode.SqlByte );
            ht.Add( typeof( SqlDateTime ),          ExtendedClrTypeCode.SqlDateTime );
            ht.Add( typeof( SqlDouble ),            ExtendedClrTypeCode.SqlDouble );
            ht.Add( typeof( SqlGuid ),              ExtendedClrTypeCode.SqlGuid );
            ht.Add( typeof( SqlInt16 ),             ExtendedClrTypeCode.SqlInt16 );
            ht.Add( typeof( SqlInt32 ),             ExtendedClrTypeCode.SqlInt32 );
            ht.Add( typeof( SqlInt64 ),             ExtendedClrTypeCode.SqlInt64 );
            ht.Add( typeof( SqlMoney ),             ExtendedClrTypeCode.SqlMoney );
            ht.Add( typeof( SqlDecimal ),           ExtendedClrTypeCode.SqlDecimal );
            ht.Add( typeof( SqlSingle ),            ExtendedClrTypeCode.SqlSingle );
            ht.Add( typeof( SqlString ),            ExtendedClrTypeCode.SqlString );
            ht.Add( typeof( SqlChars ),             ExtendedClrTypeCode.SqlChars );
            ht.Add( typeof( SqlBytes ),             ExtendedClrTypeCode.SqlBytes );
            ht.Add( typeof( SqlXml ),               ExtendedClrTypeCode.SqlXml );
            ht.Add( typeof( DataTable ),            ExtendedClrTypeCode.DataTable );
            ht.Add( typeof( DbDataReader ),         ExtendedClrTypeCode.DbDataReader );
            ht.Add( typeof( IEnumerable<SqlDataRecord> ),          ExtendedClrTypeCode.IEnumerableOfSqlDataRecord );
            ht.Add( typeof( System.TimeSpan ),      ExtendedClrTypeCode.TimeSpan );
            ht.Add( typeof( System.DateTimeOffset ),               ExtendedClrTypeCode.DateTimeOffset );
            __typeToExtendedTypeCodeMap = ht;
        }


        internal static bool IsCharOrXmlType(SqlDbType type) {
            return  IsUnicodeType(type) ||
                    IsAnsiType(type) ||
                    type == SqlDbType.Xml;
        }

        internal static bool IsUnicodeType(SqlDbType type) {
            return  type == SqlDbType.NChar ||
                    type == SqlDbType.NVarChar ||
                    type == SqlDbType.NText;
        }

        internal static bool IsAnsiType(SqlDbType type) {
            return  type == SqlDbType.Char ||
                    type == SqlDbType.VarChar ||
                    type == SqlDbType.Text;
        }

        internal static bool IsBinaryType(SqlDbType type) {
            return  type == SqlDbType.Binary ||
                    type == SqlDbType.VarBinary ||
                    type == SqlDbType.Image;
        }

        // Does this type use PLP format values?
        internal static bool IsPlpFormat(SmiMetaData metaData) {
            return  metaData.MaxLength == SmiMetaData.UnlimitedMaxLengthIndicator ||
                    metaData.SqlDbType == SqlDbType.Image ||
                    metaData.SqlDbType == SqlDbType.NText ||
                    metaData.SqlDbType == SqlDbType.Text ||
                    metaData.SqlDbType == SqlDbType.Udt;
        }



        // If we know we're only going to use this object to assign to a specific SqlDbType back end object,
        //  we can save some processing time by only checking for the few valid types that can be assigned to the dbType.
        //  This assumes a switch statement over SqlDbType is faster than getting the ClrTypeCode and iterating over a
        //  series of if statements, or using a hash table. 
        // NOTE: the form of these checks is taking advantage of a feature of the JIT compiler that is supposed to
        //      optimize checks of the form '(xxx.GetType() == typeof( YYY ))'.  The JIT team claimed at one point that
        //      this doesn't even instantiate a Type instance, thus was the fastest method for individual comparisions.
        //      Given that there's a known SqlDbType, thus a minimal number of comparisions, it's likely this is faster
        //      than the other approaches considered (both GetType().GetTypeCode() switch and hash table using Type keys
        //      must instantiate a Type object.  The typecode switch also degenerates into a large if-then-else for
        //      all but the primitive clr types.
        internal static ExtendedClrTypeCode DetermineExtendedTypeCodeForUseWithSqlDbType(
                SqlDbType   dbType, 
                bool        isMultiValued, 
                object      value, 
                Type        udtType,
                ulong       smiVersion) {
            ExtendedClrTypeCode extendedCode = ExtendedClrTypeCode.Invalid;

            // fast-track null, which is valid for all types
            if ( null == value ) {
                extendedCode = ExtendedClrTypeCode.Empty;
            }
            else if ( DBNull.Value == value ) {
                extendedCode = ExtendedClrTypeCode.DBNull;
            }
            else {
                switch(dbType)
                    {
                    case SqlDbType.BigInt:
                        if (value.GetType() == typeof(Int64))
                            extendedCode = ExtendedClrTypeCode.Int64;
                        else if (value.GetType() == typeof(SqlInt64))
                            extendedCode = ExtendedClrTypeCode.SqlInt64;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.Int64)
                            extendedCode = ExtendedClrTypeCode.Int64;
                        break;
                    case SqlDbType.Binary:
                    case SqlDbType.VarBinary:
                    case SqlDbType.Image:
                    case SqlDbType.Timestamp:
                        if (value.GetType() == typeof( byte[] ))
                            extendedCode = ExtendedClrTypeCode.ByteArray;
                        else if (value.GetType() == typeof( SqlBinary ))
                            extendedCode = ExtendedClrTypeCode.SqlBinary;
                        else if (value.GetType() == typeof( SqlBytes ))
                            extendedCode = ExtendedClrTypeCode.SqlBytes;
                        else if (value.GetType() == typeof(StreamDataFeed))
                            extendedCode = ExtendedClrTypeCode.Stream;
                        break;
                    case SqlDbType.Bit:
                        if (value.GetType() == typeof( bool ))
                            extendedCode = ExtendedClrTypeCode.Boolean;
                        else if (value.GetType() == typeof( SqlBoolean ))
                            extendedCode = ExtendedClrTypeCode.SqlBoolean;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.Boolean)
                            extendedCode = ExtendedClrTypeCode.Boolean;
                        break;
                    case SqlDbType.Char:
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Text:
                    case SqlDbType.VarChar:
                        if (value.GetType() == typeof( string ))
                            extendedCode = ExtendedClrTypeCode.String;
                        if (value.GetType() == typeof(TextDataFeed))
                            extendedCode = ExtendedClrTypeCode.TextReader;                     
                        else if (value.GetType() == typeof(SqlString))
                            extendedCode = ExtendedClrTypeCode.SqlString;
                        else if (value.GetType() == typeof(char[]))
                            extendedCode = ExtendedClrTypeCode.CharArray;
                        else if (value.GetType() == typeof(SqlChars))
                            extendedCode = ExtendedClrTypeCode.SqlChars;
                        else if (value.GetType() == typeof(char))
                            extendedCode = ExtendedClrTypeCode.Char;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.Char)
                            extendedCode = ExtendedClrTypeCode.Char;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.String)
                            extendedCode = ExtendedClrTypeCode.String;
                        break;
                    case SqlDbType.Date:
                    case SqlDbType.DateTime2:
                        if (smiVersion >= SmiContextFactory.KatmaiVersion) {
                            goto case SqlDbType.DateTime;
                        }
                        break;
                    case SqlDbType.DateTime:
                    case SqlDbType.SmallDateTime:
                        if (value.GetType() == typeof( DateTime ))
                            extendedCode = ExtendedClrTypeCode.DateTime;
                        else if (value.GetType() == typeof( SqlDateTime ))
                            extendedCode = ExtendedClrTypeCode.SqlDateTime;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.DateTime)
                            extendedCode = ExtendedClrTypeCode.DateTime;
                        break;
                    case SqlDbType.Decimal:
                        if (value.GetType() == typeof( Decimal ))
                            extendedCode = ExtendedClrTypeCode.Decimal;
                        else if (value.GetType() == typeof( SqlDecimal ))
                            extendedCode = ExtendedClrTypeCode.SqlDecimal;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.Decimal)
                            extendedCode = ExtendedClrTypeCode.Decimal;
                        break;
                    case SqlDbType.Real:
                        if (value.GetType() == typeof( Single ))
                            extendedCode = ExtendedClrTypeCode.Single;
                        else if (value.GetType() == typeof( SqlSingle ))
                            extendedCode = ExtendedClrTypeCode.SqlSingle;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.Single)
                            extendedCode = ExtendedClrTypeCode.Single;
                        break;
                    case SqlDbType.Int:
                        if (value.GetType() == typeof( Int32 ))
                            extendedCode = ExtendedClrTypeCode.Int32;
                        else if (value.GetType() == typeof( SqlInt32 ))
                            extendedCode = ExtendedClrTypeCode.SqlInt32;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.Int32)
                            extendedCode = ExtendedClrTypeCode.Int32;
                        break;
                    case SqlDbType.Money:
                    case SqlDbType.SmallMoney:
                        if (value.GetType() == typeof( SqlMoney ))
                            extendedCode = ExtendedClrTypeCode.SqlMoney;
                        else if (value.GetType() == typeof( Decimal ))
                            extendedCode = ExtendedClrTypeCode.Decimal;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.Decimal)
                            extendedCode = ExtendedClrTypeCode.Decimal;
                        break;
                    case SqlDbType.Float:
                        if (value.GetType() == typeof( SqlDouble ))
                            extendedCode = ExtendedClrTypeCode.SqlDouble;
                        else if (value.GetType() == typeof( Double ))
                            extendedCode = ExtendedClrTypeCode.Double;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.Double)
                            extendedCode = ExtendedClrTypeCode.Double;
                        break;
                    case SqlDbType.UniqueIdentifier:
                        if (value.GetType() == typeof( SqlGuid ))
                            extendedCode = ExtendedClrTypeCode.SqlGuid;
                        else if (value.GetType() == typeof( Guid ))
                            extendedCode = ExtendedClrTypeCode.Guid;
                        break;
                    case SqlDbType.SmallInt:
                        if (value.GetType() == typeof( Int16 ))
                            extendedCode = ExtendedClrTypeCode.Int16;
                        else if (value.GetType() == typeof( SqlInt16 ))
                            extendedCode = ExtendedClrTypeCode.SqlInt16;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.Int16)
                            extendedCode = ExtendedClrTypeCode.Int16;
                        break;
                    case SqlDbType.TinyInt:
                        if (value.GetType() == typeof( Byte ))
                            extendedCode = ExtendedClrTypeCode.Byte;
                        else if (value.GetType() == typeof( SqlByte ))
                            extendedCode = ExtendedClrTypeCode.SqlByte;
                        else if (Type.GetTypeCode(value.GetType()) == TypeCode.Byte)
                            extendedCode = ExtendedClrTypeCode.Byte;
                        break;
                    case SqlDbType.Variant:
                        // SqlDbType doesn't help us here, call general-purpose function
                        extendedCode = DetermineExtendedTypeCode( value );

                        // Some types aren't allowed for Variants but are for the general-purpos function.  
                        //  Match behavior of other types and return invalid in these cases.
                        if ( ExtendedClrTypeCode.SqlXml == extendedCode ) {
                            extendedCode = ExtendedClrTypeCode.Invalid;
                        }
                        break;
                    case SqlDbType.Udt:
                        // Validate UDT type if caller gave us a type to validate against
                        if ( null == udtType ||
                                value.GetType() == udtType
                            ) {
                            extendedCode = ExtendedClrTypeCode.Object;
                        }
                        else {
                            extendedCode = ExtendedClrTypeCode.Invalid;
                        }
                        break;
                    case SqlDbType.Time:
                        if (value.GetType() == typeof(TimeSpan) && smiVersion >= SmiContextFactory.KatmaiVersion)
                            extendedCode = ExtendedClrTypeCode.TimeSpan;
                        break;
                    case SqlDbType.DateTimeOffset:
                        if (value.GetType() == typeof(DateTimeOffset) && smiVersion >= SmiContextFactory.KatmaiVersion)
                            extendedCode = ExtendedClrTypeCode.DateTimeOffset;
                        break;
                    case SqlDbType.Xml:
                        if (value.GetType() == typeof( SqlXml ))
                            extendedCode = ExtendedClrTypeCode.SqlXml;
                        if (value.GetType() == typeof(XmlDataFeed))
                            extendedCode = ExtendedClrTypeCode.XmlReader;
                        else if (value.GetType() == typeof( System.String ))
                            extendedCode = ExtendedClrTypeCode.String;
                        break;
                    case SqlDbType.Structured:
                        if (isMultiValued) {
                            if (value is DataTable) {
                                extendedCode = ExtendedClrTypeCode.DataTable;
                            }
                            // Order is important, since some of these types are base types of the others.
                            //  Evaluate from most derived to parent types
                            else if (value is IEnumerable<SqlDataRecord>) {
                                extendedCode = ExtendedClrTypeCode.IEnumerableOfSqlDataRecord;
                            }
                            else if (value is DbDataReader) {
                                extendedCode = ExtendedClrTypeCode.DbDataReader;
                            }
                        }
                        break;
                    default:
                        // Leave as invalid
                        break;
                    }
            }

            return extendedCode;

        }

        // Method to map from Type to ExtendedTypeCode
        static internal ExtendedClrTypeCode DetermineExtendedTypeCodeFromType(Type clrType) {
            object result = __typeToExtendedTypeCodeMap[clrType];

            ExtendedClrTypeCode resultCode;
            if ( null == result ) {
                resultCode = ExtendedClrTypeCode.Invalid;
            }
            else {
                resultCode = (ExtendedClrTypeCode) result;
            }

            return resultCode;
        }

         // Returns the ExtendedClrTypeCode that describes the given value
        //   











        static internal ExtendedClrTypeCode DetermineExtendedTypeCode( object value ) {
            ExtendedClrTypeCode resultCode;
            if ( null == value ) {
                resultCode = ExtendedClrTypeCode.Empty;
            }
            else {
                resultCode = DetermineExtendedTypeCodeFromType(value.GetType());
            }

            return resultCode;
        }

        // returns a sqldbtype for the given type code
        static internal SqlDbType InferSqlDbTypeFromTypeCode( ExtendedClrTypeCode typeCode ) {
            Debug.Assert( typeCode >= ExtendedClrTypeCode.Invalid && typeCode <= ExtendedClrTypeCode.Last, "Someone added a typecode without adding support here!" );

            return __extendedTypeCodeToSqlDbTypeMap[ (int) typeCode+1 ];
        }

        // Infer SqlDbType from Type in the general case.  Katmai-only (or later) features that need to 
        //  infer types should use InferSqlDbTypeFromType_Katmai.
        static internal SqlDbType InferSqlDbTypeFromType(Type type) {
            ExtendedClrTypeCode typeCode = DetermineExtendedTypeCodeFromType(type);
            SqlDbType returnType;
            if (ExtendedClrTypeCode.Invalid == typeCode) {
                returnType = InvalidSqlDbType;  // Return invalid type so caller can generate specific error
            }
            else {
                returnType = InferSqlDbTypeFromTypeCode(typeCode);
            }

            return returnType;
        }

        // Inference rules changed for Katmai-or-later-only cases.  Only features that are guaranteed to be 
        //  running against Katmai and don't have backward compat issues should call this code path.
        //      example: TVP's are a new Katmai feature (no back compat issues) so can infer DATETIME2
        //          when mapping System.DateTime from DateTable or DbDataReader.  DATETIME2 is better because
        //          of greater range that can handle all DateTime values.
        static internal SqlDbType InferSqlDbTypeFromType_Katmai(Type type) {
            SqlDbType returnType = InferSqlDbTypeFromType(type);
            if (SqlDbType.DateTime == returnType) {
                returnType = SqlDbType.DateTime2;
            }
            return returnType;
        }

        static internal bool IsValidForSmiVersion(SmiExtendedMetaData md, ulong smiVersion) {
            if (SmiContextFactory.LatestVersion == smiVersion) {
                return true;
            }
            else {
                // Yukon doesn't support Structured nor the new time types
                Debug.Assert(SmiContextFactory.YukonVersion == smiVersion, "Other versions should have been eliminated during link stage");
                return md.SqlDbType != SqlDbType.Structured &&
                        md.SqlDbType != SqlDbType.Date &&
                        md.SqlDbType != SqlDbType.DateTime2 &&
                        md.SqlDbType != SqlDbType.DateTimeOffset &&
                        md.SqlDbType != SqlDbType.Time;
            }
        }

        static internal SqlMetaData SmiExtendedMetaDataToSqlMetaData(SmiExtendedMetaData source) {
            if (SqlDbType.Xml == source.SqlDbType) {
                return new SqlMetaData(source.Name,
                    source.SqlDbType,
                    source.MaxLength,
                    source.Precision,
                    source.Scale,
                    source.LocaleId,
                    source.CompareOptions,
                    source.TypeSpecificNamePart1,
                    source.TypeSpecificNamePart2,
                    source.TypeSpecificNamePart3,
                    true,
                    source.Type);
            }

            return new SqlMetaData(source.Name,
                source.SqlDbType,
                source.MaxLength,
                source.Precision,
                source.Scale,
                source.LocaleId,
                source.CompareOptions,
                source.Type);
        }

        // Convert SqlMetaData instance to an SmiExtendedMetaData instance.

        internal static SmiExtendedMetaData SqlMetaDataToSmiExtendedMetaData( SqlMetaData source ) {
            // now map everything across to the extended metadata object
            string typeSpecificNamePart1 = null;
            string typeSpecificNamePart2 = null;
            string typeSpecificNamePart3 = null;
            
            if (SqlDbType.Xml == source.SqlDbType) {
                typeSpecificNamePart1 = source.XmlSchemaCollectionDatabase;
                typeSpecificNamePart2 = source.XmlSchemaCollectionOwningSchema;
                typeSpecificNamePart3 = source.XmlSchemaCollectionName;
            }
            else if (SqlDbType.Udt == source.SqlDbType) {
                // Split the input name. UdtTypeName is specified as single 3 part name.
                // NOTE: ParseUdtTypeName throws if format is incorrect
                string typeName = source.ServerTypeName;
                if (null != typeName) {
                    String[] names = SqlParameter.ParseTypeName(typeName, true /* is for UdtTypeName */);

                    if (1 == names.Length) {
                        typeSpecificNamePart3 = names[0];
                    }
                    else if (2 == names.Length) {
                        typeSpecificNamePart2 = names[0];
                        typeSpecificNamePart3 = names[1];
                    }
                    else if (3 == names.Length) {
                        typeSpecificNamePart1 = names[0];
                        typeSpecificNamePart2 = names[1];
                        typeSpecificNamePart3 = names[2];
                    }
                    else {
                        throw ADP.ArgumentOutOfRange("typeName");
                    }

                    if ((!ADP.IsEmpty(typeSpecificNamePart1) && TdsEnums.MAX_SERVERNAME < typeSpecificNamePart1.Length)
                        || (!ADP.IsEmpty(typeSpecificNamePart2) && TdsEnums.MAX_SERVERNAME < typeSpecificNamePart2.Length)
                        || (!ADP.IsEmpty(typeSpecificNamePart3) && TdsEnums.MAX_SERVERNAME < typeSpecificNamePart3.Length)) {
                        throw ADP.ArgumentOutOfRange("typeName");
                    }
                }
            }                    

            return new SmiExtendedMetaData( source.SqlDbType,
                                            source.MaxLength,
                                            source.Precision,
                                            source.Scale,
                                            source.LocaleId,
                                            source.CompareOptions,
                                            source.Type,
                                            source.Name,
                                            typeSpecificNamePart1,
                                            typeSpecificNamePart2,
                                            typeSpecificNamePart3 );


        }


        // compare SmiMetaData to SqlMetaData and determine if they are compatible.
        static internal bool IsCompatible(SmiMetaData firstMd, SqlMetaData secondMd) {
            return firstMd.SqlDbType == secondMd.SqlDbType &&
                    firstMd.MaxLength == secondMd.MaxLength &&
                    firstMd.Precision == secondMd.Precision &&
                    firstMd.Scale == secondMd.Scale &&
                    firstMd.CompareOptions == secondMd.CompareOptions &&
                    firstMd.LocaleId == secondMd.LocaleId &&
                    firstMd.Type == secondMd.Type &&
                    firstMd.SqlDbType != SqlDbType.Structured &&  // SqlMetaData doesn't support Structured types
                    !firstMd.IsMultiValued;  // SqlMetaData doesn't have a "multivalued" option
        }

        static internal long AdjustMaxLength(SqlDbType dbType, long maxLength) {
            if (SmiMetaData.UnlimitedMaxLengthIndicator != maxLength) {
                if (maxLength < 0) {
                    maxLength = InvalidMaxLength;
                }

                switch(dbType) {
                    case SqlDbType.Binary:
                        if (maxLength > SmiMetaData.MaxBinaryLength) {
                            maxLength = InvalidMaxLength;
                        }
                        break;
                    case SqlDbType.Char:
                        if (maxLength > SmiMetaData.MaxANSICharacters) {
                            maxLength = InvalidMaxLength;
                        }
                        break;
                    case SqlDbType.NChar:
                        if (maxLength > SmiMetaData.MaxUnicodeCharacters) {
                            maxLength = InvalidMaxLength;
                        }
                        break;
                    case SqlDbType.NVarChar:
                        // Promote to MAX type if it won't fit in a normal type
                        if (SmiMetaData.MaxUnicodeCharacters < maxLength) {
                            maxLength = SmiMetaData.UnlimitedMaxLengthIndicator;
                        }
                        break;
                    case SqlDbType.VarBinary:
                        // Promote to MAX type if it won't fit in a normal type
                        if (SmiMetaData.MaxBinaryLength < maxLength) {
                            maxLength = SmiMetaData.UnlimitedMaxLengthIndicator;
                        }
                        break;
                    case SqlDbType.VarChar:
                        // Promote to MAX type if it won't fit in a normal type
                        if (SmiMetaData.MaxANSICharacters < maxLength) {
                            maxLength = SmiMetaData.UnlimitedMaxLengthIndicator;
                        }
                        break;
                    default:
                        break;
                }
            }

            return maxLength;
        }

        // Extract metadata for a single DataColumn
        static internal SmiExtendedMetaData SmiMetaDataFromDataColumn(DataColumn column, DataTable parent) {
            SqlDbType dbType = InferSqlDbTypeFromType_Katmai(column.DataType);
            if (InvalidSqlDbType == dbType) {
                throw SQL.UnsupportedColumnTypeForSqlProvider(column.ColumnName, column.DataType.Name);
            }

            long maxLength = AdjustMaxLength(dbType, column.MaxLength);
            if (InvalidMaxLength == maxLength) {
                throw SQL.InvalidColumnMaxLength(column.ColumnName, maxLength);
            }

            byte precision;
            byte scale;
            if (column.DataType == typeof(SqlDecimal)) {

                // Must scan all values in column to determine best-fit precision & scale
                Debug.Assert(null != parent);
                scale = 0;
                byte nonFractionalPrecision = 0; // finds largest non-Fractional portion of precision
                foreach (DataRow row in parent.Rows) {
                    object obj = row[column];
                    if (!(obj is DBNull)) {
                        SqlDecimal value = (SqlDecimal) obj;
                        if (!value.IsNull) {
                            byte tempNonFractPrec = checked((byte) (value.Precision - value.Scale));
                            if (tempNonFractPrec > nonFractionalPrecision) {
                                nonFractionalPrecision = tempNonFractPrec;
                            }

                            if (value.Scale > scale) {
                                scale = value.Scale;
                            }
                        }
                    }
                }

                precision = checked((byte)(nonFractionalPrecision + scale));

                if (SqlDecimal.MaxPrecision < precision) {
                    throw SQL.InvalidTableDerivedPrecisionForTvp(column.ColumnName, precision);
                }
                else if (0 == precision) {
                    precision = 1;
                }
            }
            else if (dbType == SqlDbType.DateTime2 || dbType == SqlDbType.DateTimeOffset || dbType == SqlDbType.Time) {
                // Time types care about scale, too.  But have to infer maximums for these.
                precision = 0;
                scale = SmiMetaData.DefaultTime.Scale;
            }
            else if (dbType == SqlDbType.Decimal) {
                // Must scan all values in column to determine best-fit precision & scale
                Debug.Assert(null != parent);
                scale = 0;
                byte nonFractionalPrecision = 0; // finds largest non-Fractional portion of precision
                foreach (DataRow row in parent.Rows) {
                    object obj = row[column];
                    if (!(obj is DBNull)) {
                        SqlDecimal value = (SqlDecimal)(Decimal)obj;
                        byte tempNonFractPrec = checked((byte)(value.Precision - value.Scale));
                        if (tempNonFractPrec > nonFractionalPrecision) {
                            nonFractionalPrecision = tempNonFractPrec;
                        }

                        if (value.Scale > scale) {
                            scale = value.Scale;
                        }
                    }
                }

                precision = checked((byte)(nonFractionalPrecision + scale));

                if (SqlDecimal.MaxPrecision < precision) {
                    throw SQL.InvalidTableDerivedPrecisionForTvp(column.ColumnName, precision);
                }
                else if (0 == precision) {
                    precision = 1;
                }
            }
            else {
                precision = 0;
                scale = 0;
            }

            return new SmiExtendedMetaData(
                                        dbType, 
                                        maxLength, 
                                        precision, 
                                        scale, 
                                        column.Locale.LCID, 
                                        SmiMetaData.DefaultNVarChar.CompareOptions, 
                                        column.DataType, 
                                        false,  // no support for multi-valued columns in a TVP yet
                                        null,   // no support for structured columns yet
                                        null,   // no support for structured columns yet
                                        column.ColumnName, 
                                        null, 
                                        null, 
                                        null);
        }

        // Map SmiMetaData from a schema table.
        //  DEVNOTE: since we're using SchemaTable, we can assume that we aren't directly using a SqlDataReader
        //      so we don't support the Sql-specific stuff, like collation
        static internal SmiExtendedMetaData SmiMetaDataFromSchemaTableRow(DataRow schemaRow) {
            // One way or another, we'll need column name, so put it in a local now to shorten code later.
            string colName = "";
            object temp = schemaRow[SchemaTableColumn.ColumnName];
            if (DBNull.Value != temp) {
                colName = (string)temp;
            }

            // Determine correct SqlDbType.
            temp = schemaRow[SchemaTableColumn.DataType];
            if (DBNull.Value == temp) {
                throw SQL.NullSchemaTableDataTypeNotSupported(colName);
            }
            Type colType = (Type)temp;
            SqlDbType colDbType = InferSqlDbTypeFromType_Katmai(colType);
            if (InvalidSqlDbType == colDbType) {
                // Unknown through standard mapping, use VarBinary for columns that are Object typed, otherwise error
                if (typeof(object) == colType) {
                    colDbType = SqlDbType.VarBinary;
                }
                else {
                    throw SQL.UnsupportedColumnTypeForSqlProvider(colName, colType.ToString());
                }
            }

            // Determine metadata modifier values per type (maxlength, precision, scale, etc)
            long maxLength = 0;
            byte precision = 0;
            byte scale = 0;
            switch (colDbType) {
                case SqlDbType.BigInt:
                case SqlDbType.Bit:
                case SqlDbType.DateTime:
                case SqlDbType.Float:
                case SqlDbType.Image:
                case SqlDbType.Int:
                case SqlDbType.Money:
                case SqlDbType.NText:
                case SqlDbType.Real:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.SmallDateTime:
                case SqlDbType.SmallInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.Text:
                case SqlDbType.Timestamp:
                case SqlDbType.TinyInt:
                case SqlDbType.Variant:
                case SqlDbType.Xml:
                case SqlDbType.Date:
                    // These types require no  metadata modifies
                    break;
                case SqlDbType.Binary:
                case SqlDbType.VarBinary:
                    // These types need a binary max length
                    temp = schemaRow[SchemaTableColumn.ColumnSize];
                    if (DBNull.Value == temp) {
                        // source isn't specifying a size, so assume the worst
                        if (SqlDbType.Binary == colDbType) {
                            maxLength = SmiMetaData.MaxBinaryLength;
                        }
                        else {
                            maxLength = SmiMetaData.UnlimitedMaxLengthIndicator;
                        }
                    }
                    else {
                        // We (should) have a valid maxlength, so use it.
                        maxLength = Convert.ToInt64(temp, null);

                        // Max length must be 0 to MaxBinaryLength or it can be UnlimitedMAX if type is varbinary
                        //   If it's greater than MaxBinaryLength, just promote it to UnlimitedMAX, if possible
                        if (maxLength > SmiMetaData.MaxBinaryLength) {
                            maxLength = SmiMetaData.UnlimitedMaxLengthIndicator;
                        }

                        if ((maxLength < 0 &&
                                (maxLength != SmiMetaData.UnlimitedMaxLengthIndicator ||
                                 SqlDbType.Binary == colDbType))) {
                            throw SQL.InvalidColumnMaxLength(colName, maxLength);
                        }
                    }
                    break;
                case SqlDbType.Char:
                case SqlDbType.VarChar:
                    // These types need an ANSI max length
                    temp = schemaRow[SchemaTableColumn.ColumnSize];
                    if (DBNull.Value == temp) {
                        // source isn't specifying a size, so assume the worst
                        if (SqlDbType.Char == colDbType) {
                            maxLength = SmiMetaData.MaxANSICharacters;
                        }
                        else {
                            maxLength = SmiMetaData.UnlimitedMaxLengthIndicator;
                        }
                    }
                    else {
                        // We (should) have a valid maxlength, so use it.
                        maxLength = Convert.ToInt64(temp, null);

                        // Max length must be 0 to MaxANSICharacters or it can be UnlimitedMAX if type is varbinary
                        //   If it's greater than MaxANSICharacters, just promote it to UnlimitedMAX, if possible
                        if (maxLength > SmiMetaData.MaxANSICharacters) {
                            maxLength = SmiMetaData.UnlimitedMaxLengthIndicator;
                        }

                        if ((maxLength < 0 && 
                                (maxLength != SmiMetaData.UnlimitedMaxLengthIndicator ||
                                 SqlDbType.Char == colDbType))) {
                            throw SQL.InvalidColumnMaxLength(colName, maxLength);
                        }
                    }
                    break;
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                    // These types need a unicode max length
                    temp = schemaRow[SchemaTableColumn.ColumnSize];
                    if (DBNull.Value == temp) {
                        // source isn't specifying a size, so assume the worst
                        if (SqlDbType.NChar == colDbType) {
                            maxLength = SmiMetaData.MaxUnicodeCharacters;
                        }
                        else {
                            maxLength = SmiMetaData.UnlimitedMaxLengthIndicator;
                        }
                    }
                    else {
                        // We (should) have a valid maxlength, so use it.
                        maxLength = Convert.ToInt64(temp, null);

                        // Max length must be 0 to MaxUnicodeCharacters or it can be UnlimitedMAX if type is varbinary
                        //   If it's greater than MaxUnicodeCharacters, just promote it to UnlimitedMAX, if possible
                        if (maxLength > SmiMetaData.MaxUnicodeCharacters) {
                            maxLength = SmiMetaData.UnlimitedMaxLengthIndicator;
                        }

                        if ((maxLength < 0 &&
                                (maxLength != SmiMetaData.UnlimitedMaxLengthIndicator ||
                                 SqlDbType.NChar == colDbType))) {
                            throw SQL.InvalidColumnMaxLength(colName, maxLength);
                        }
                    }
                    break;
                case SqlDbType.Decimal:
                    // Decimal requires precision and scale
                    temp = schemaRow[SchemaTableColumn.NumericPrecision];
                    if (DBNull.Value == temp) {
                        precision = SmiMetaData.DefaultDecimal.Precision;
                    }
                    else {
                        precision = Convert.ToByte(temp, null);
                    }

                    temp = schemaRow[SchemaTableColumn.NumericScale];
                    if (DBNull.Value == temp) {
                        scale = SmiMetaData.DefaultDecimal.Scale;
                    }
                    else {
                        scale = Convert.ToByte(temp, null);
                    }

                    if (precision < SmiMetaData.MinPrecision || 
                            precision > SqlDecimal.MaxPrecision || 
                            scale < SmiMetaData.MinScale || 
                            scale > SqlDecimal.MaxScale ||
                            scale > precision) {
                        throw SQL.InvalidColumnPrecScale();
                    }
                    break;
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                    // requires scale
                    temp = schemaRow[SchemaTableColumn.NumericScale];
                    if (DBNull.Value == temp) {
                        scale = SmiMetaData.DefaultTime.Scale;
                    }
                    else {
                        scale = Convert.ToByte(temp, null);
                    }

                    if (scale > SmiMetaData.MaxTimeScale) {
                        throw SQL.InvalidColumnPrecScale();
                    }
                    else if (scale < 0) {
                        scale = SmiMetaData.DefaultTime.Scale;
                    }
                    break;
                case SqlDbType.Udt:
                case SqlDbType.Structured:
                default:
                    // These types are not supported from SchemaTable
                    throw SQL.UnsupportedColumnTypeForSqlProvider(colName, colType.ToString());
            }

            return new SmiExtendedMetaData(
                                        colDbType, 
                                        maxLength, 
                                        precision, 
                                        scale, 
                                        System.Globalization.CultureInfo.CurrentCulture.LCID, 
                                        SmiMetaData.GetDefaultForType(colDbType).CompareOptions, 
                                        null,   // no support for UDTs from SchemaTable
                                        false,  // no support for multi-valued columns in a TVP yet
                                        null,   // no support for structured columns yet
                                        null,   // no support for structured columns yet
                                        colName, 
                                        null, 
                                        null, 
                                        null);
        }
    }
}
