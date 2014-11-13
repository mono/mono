//------------------------------------------------------------------------------
// <copyright file="SqlParameter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.IO;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using MSS=Microsoft.SqlServer.Server;

    using Microsoft.SqlServer.Server;
    using System.Threading.Tasks;

    internal abstract class DataFeed  {       
    }

    internal class StreamDataFeed : DataFeed {
        internal Stream _source;
        
        internal StreamDataFeed(Stream source) {
            _source = source;
        }       
    }

    internal class TextDataFeed : DataFeed {
        internal TextReader _source;

        internal TextDataFeed(TextReader source) {
            _source = source;
        }       
    }

    internal class XmlDataFeed : DataFeed {
        internal  XmlReader _source;

        internal XmlDataFeed(XmlReader source) {
            _source = source;
        }
    }

    [
    System.ComponentModel.TypeConverterAttribute(typeof(System.Data.SqlClient.SqlParameter.SqlParameterConverter))
    ]
    public sealed partial class SqlParameter : DbParameter, IDbDataParameter, ICloneable {
        private MetaType              _metaType;

        private SqlCollation          _collation;
        private string                _xmlSchemaCollectionDatabase;
        private string                _xmlSchemaCollectionOwningSchema;
        private string                _xmlSchemaCollectionName;

        private string                _udtTypeName;
        private string                _typeName;
        private Type                  _udtType;
        private Exception             _udtLoadError;

        private string                _parameterName;
        private byte                  _precision;
        private byte                  _scale;
        private bool                  _hasScale; // V1.0 compat, ignore _hasScale

        private MetaType              _internalMetaType;
        private SqlBuffer             _sqlBufferReturnValue;
        private INullable             _valueAsINullable;
        private bool                  _isSqlParameterSqlType;
        private bool                  _isNull = true;
        private bool                  _coercedValueIsSqlType;
        private bool                  _coercedValueIsDataFeed;
        private int                   _actualSize = -1;

        public SqlParameter() : base() {
        }

        [ EditorBrowsableAttribute(EditorBrowsableState.Advanced) ] // MDAC 69508
        public SqlParameter(string parameterName,
                            SqlDbType dbType, int size,
                            ParameterDirection direction, bool isNullable,
                            byte precision, byte scale,
                            string sourceColumn, DataRowVersion sourceVersion,
                            object value) : this() { // V1.0 everything
            this.ParameterName = parameterName;
            this.SqlDbType = dbType;
            this.Size = size;
            this.Direction = direction;
            this.IsNullable = isNullable;
            PrecisionInternal = precision;
            ScaleInternal = scale;
            this.SourceColumn = sourceColumn;
            this.SourceVersion = sourceVersion;
            this.Value = value;
        }
        public SqlParameter(string parameterName,
                               SqlDbType dbType, int size,
                               ParameterDirection direction,
                               byte precision, byte scale,
                               string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping,
                               object value,
                               string xmlSchemaCollectionDatabase, string xmlSchemaCollectionOwningSchema,
                               string xmlSchemaCollectionName
                               ) { // V2.0 everything - round trip all browsable properties + precision/scale
            this.ParameterName = parameterName;
            this.SqlDbType = dbType;
            this.Size = size;
            this.Direction = direction;
            this.PrecisionInternal = precision;
            this.ScaleInternal = scale;
            this.SourceColumn = sourceColumn;
            this.SourceVersion = sourceVersion;
            this.SourceColumnNullMapping = sourceColumnNullMapping;
            this.Value = value;
            this._xmlSchemaCollectionDatabase = xmlSchemaCollectionDatabase;
            this._xmlSchemaCollectionOwningSchema = xmlSchemaCollectionOwningSchema;
            this._xmlSchemaCollectionName = xmlSchemaCollectionName;
        }
        public SqlParameter(string parameterName, SqlDbType dbType) : this() {
            this.ParameterName = parameterName;
            this.SqlDbType = dbType;
        }

        public SqlParameter(string parameterName, object value) : this() {
            Debug.Assert(!(value is SqlDbType), "use SqlParameter(string, SqlDbType)");

            this.ParameterName = parameterName;
            this.Value = value;
        }

        public SqlParameter(string parameterName, SqlDbType dbType, int size) : this() {
            this.ParameterName = parameterName;
            this.SqlDbType = dbType;
            this.Size = size;
        }

        public SqlParameter(string parameterName, SqlDbType dbType, int size, string sourceColumn) : this() {
            this.ParameterName = parameterName;
            this.SqlDbType = dbType;
            this.Size = size;
            this.SourceColumn = sourceColumn;
        }

        //
        // currently the user can't set this value.  it gets set by the returnvalue from tds
        //
        internal SqlCollation Collation {
            get {
                return _collation;
            }
            set {
                _collation = value;
            }
        }

        [
        Browsable(false),
        ]
        public SqlCompareOptions CompareInfo {
            // Bits 21 through 25 represent the CompareInfo
            get {
                SqlCollation collation = _collation;
                if (null != collation) {
                    return collation.SqlCompareOptions;
                }
                return SqlCompareOptions.None;
            }
            set {
                SqlCollation collation = _collation;
                if (null == collation) {
                    _collation = collation = new SqlCollation();
                }
                if ((value & SqlString.x_iValidSqlCompareOptionMask) != value) {
                    throw ADP.ArgumentOutOfRange("CompareInfo");
                }
                collation.SqlCompareOptions = value;
            }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Xml),
        ResDescriptionAttribute(Res.SqlParameter_XmlSchemaCollectionDatabase),
        ]
        public string  XmlSchemaCollectionDatabase {
            get {
                string xmlSchemaCollectionDatabase = _xmlSchemaCollectionDatabase;
                return ((xmlSchemaCollectionDatabase != null) ? xmlSchemaCollectionDatabase : ADP.StrEmpty);
            }
            set {
                _xmlSchemaCollectionDatabase = value;
            }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Xml),
        ResDescriptionAttribute(Res.SqlParameter_XmlSchemaCollectionOwningSchema),
        ]
        public string XmlSchemaCollectionOwningSchema {
            get {
                string xmlSchemaCollectionOwningSchema = _xmlSchemaCollectionOwningSchema;
                return ((xmlSchemaCollectionOwningSchema != null) ? xmlSchemaCollectionOwningSchema : ADP.StrEmpty);
            }
            set {
                _xmlSchemaCollectionOwningSchema = value;
            }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Xml),
        ResDescriptionAttribute(Res.SqlParameter_XmlSchemaCollectionName),
        ]
        public string XmlSchemaCollectionName {
            get {
                string xmlSchemaCollectionName = _xmlSchemaCollectionName;
                return ((xmlSchemaCollectionName != null) ? xmlSchemaCollectionName : ADP.StrEmpty);
            }
            set {
                _xmlSchemaCollectionName = value;
            }
        }

        override public DbType DbType {
            get {
                return GetMetaTypeOnly().DbType;
            }
            set {
                MetaType metatype = _metaType;
                if ((null == metatype) || (metatype.DbType != value) ||
                        // SQLBU 504029: Two special datetime cases for backward compat
                        //  DbType.Date and DbType.Time should always be treated as setting DbType.DateTime instead
                        value == DbType.Date ||
                        value == DbType.Time) {
                    PropertyTypeChanging();
                    _metaType = MetaType.GetMetaTypeFromDbType(value);
                }
            }
        }

        public override void ResetDbType() {
            ResetSqlDbType();
        }

        internal MetaType InternalMetaType {
            get {
                Debug.Assert(null != _internalMetaType, "null InternalMetaType");
                return _internalMetaType;
            }
            set { _internalMetaType = value; }
        }

        [
        Browsable(false),
        ]
        public int LocaleId {
            // Lowest 20 bits represent LocaleId
            get {
                SqlCollation collation = _collation;
                if (null != collation) {
                    return collation.LCID;
                }
                return 0;
            }
            set {
                SqlCollation collation = _collation;
                if (null == collation) {
                    _collation = collation = new SqlCollation();
                }
                if (value != (SqlCollation.MaskLcid & value)) {
                    throw ADP.ArgumentOutOfRange("LocaleId");
                }
                collation.LCID = value;
            }
        }

        private SqlMetaData MetaData {
            get {
                MetaType mt = GetMetaTypeOnly();
 			    long maxlen;

				if (mt.IsFixed) {
					maxlen = (long)mt.FixedLength;
				}
                else if (Size > 0 || Size < 0) {
                   	maxlen = Size;   // Bug Fix: 302768, 302695, 302694, 302693
                }
				else {
					maxlen = MSS.SmiMetaData.GetDefaultForType( mt.SqlDbType ).MaxLength;
				}
                return new SqlMetaData(this.ParameterName, mt.SqlDbType, maxlen, GetActualPrecision(), GetActualScale(), LocaleId, CompareInfo,
                                       XmlSchemaCollectionDatabase, XmlSchemaCollectionOwningSchema, XmlSchemaCollectionName, mt.IsPlp, _udtType);
            }
        }

        internal bool SizeInferred {
            get {
                return 0 == _size;
            }
        }

        internal MSS.SmiParameterMetaData MetaDataForSmi(out ParameterPeekAheadValue peekAhead) {
            peekAhead = null;
            MetaType mt = ValidateTypeLengths( true /* Yukon or newer */ );
            long actualLen = GetActualSize( );
            long maxLen = this.Size; 

            // GetActualSize returns bytes length, but smi expects char length for 
            //  character types, so adjust
            if ( !mt.IsLong ) { 
                if ( SqlDbType.NChar == mt.SqlDbType || SqlDbType.NVarChar == mt.SqlDbType ) {
                    actualLen = actualLen / sizeof( char );
                }

                if ( actualLen > maxLen ) {
                    maxLen = actualLen;
                }
            }

            // Determine maxLength for types that ValidateTypeLengths won't figure out
            if ( 0 == maxLen  ) {
                if ( SqlDbType.Binary == mt.SqlDbType || SqlDbType.VarBinary == mt.SqlDbType ) {
                    maxLen = MSS.SmiMetaData.MaxBinaryLength;
                }
                else if ( SqlDbType.Char == mt.SqlDbType || SqlDbType.VarChar == mt.SqlDbType ) {
                    maxLen = MSS.SmiMetaData.MaxANSICharacters;
                }
                else if ( SqlDbType.NChar == mt.SqlDbType || SqlDbType.NVarChar == mt.SqlDbType ) {
                    maxLen = MSS.SmiMetaData.MaxUnicodeCharacters;
                }
            }
            else if (( maxLen > MSS.SmiMetaData.MaxBinaryLength     && ( SqlDbType.Binary == mt.SqlDbType || SqlDbType.VarBinary == mt.SqlDbType ))
                  || ( maxLen > MSS.SmiMetaData.MaxANSICharacters   && ( SqlDbType.Char   == mt.SqlDbType || SqlDbType.VarChar   == mt.SqlDbType ))
                  || ( maxLen > MSS.SmiMetaData.MaxUnicodeCharacters&& ( SqlDbType.NChar  == mt.SqlDbType || SqlDbType.NVarChar  == mt.SqlDbType )) ) {
                maxLen = -1;
            }


            int localeId = LocaleId;
            if ( 0 == localeId && mt.IsCharType ) {
                object value = GetCoercedValue();
                if ( value is SqlString && !( (SqlString)value ).IsNull ) {
                    localeId = ( (SqlString)value ).LCID;
                }
                else {
                    localeId = System.Globalization.CultureInfo.CurrentCulture.LCID;
                }
            }

            SqlCompareOptions compareOpts = CompareInfo;
            if ( 0 == compareOpts && mt.IsCharType ) {
                object value = GetCoercedValue();
                if ( value is SqlString && !( (SqlString)value ).IsNull ) {
                    compareOpts = ( (SqlString)value ).SqlCompareOptions;
                }
                else {
                    compareOpts = MSS.SmiMetaData.GetDefaultForType( mt.SqlDbType ).CompareOptions;
                }
            }

            string typeSpecificNamePart1 = null;
            string typeSpecificNamePart2 = null;
            string typeSpecificNamePart3 = null;
            
            if (SqlDbType.Xml == mt.SqlDbType) {
                typeSpecificNamePart1 = this.XmlSchemaCollectionDatabase;
                typeSpecificNamePart2 = this.XmlSchemaCollectionOwningSchema;
                typeSpecificNamePart3 = this.XmlSchemaCollectionName;
            }
            else if (SqlDbType.Udt == mt.SqlDbType || (SqlDbType.Structured == mt.SqlDbType && !ADP.IsEmpty(this.TypeName))) {
                // Split the input name. The type name is specified as single 3 part name.
                // NOTE: ParseTypeName throws if format is incorrect
                String[] names;
                if (SqlDbType.Udt == mt.SqlDbType) {
                    names = ParseTypeName(this.UdtTypeName, true /* is UdtTypeName */);
                }
                else {
                    names = ParseTypeName(this.TypeName, false /* not UdtTypeName */);
                }

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
                    throw ADP.ArgumentOutOfRange("names");
                }
                
                if ((!ADP.IsEmpty(typeSpecificNamePart1) && TdsEnums.MAX_SERVERNAME < typeSpecificNamePart1.Length)
                    || (!ADP.IsEmpty(typeSpecificNamePart2) && TdsEnums.MAX_SERVERNAME < typeSpecificNamePart2.Length)
                    || (!ADP.IsEmpty(typeSpecificNamePart3) && TdsEnums.MAX_SERVERNAME < typeSpecificNamePart3.Length)) {
                    throw ADP.ArgumentOutOfRange("names");
                }
            }

            byte precision = GetActualPrecision();
            byte scale = GetActualScale();

            // precision for decimal types may still need adjustment.
            if ( SqlDbType.Decimal == mt.SqlDbType ) {
                if ( 0 == precision ) {
                    precision = TdsEnums.DEFAULT_NUMERIC_PRECISION;
                }
            }

            // Sub-field determination
            List<SmiExtendedMetaData> fields = null;
            MSS.SmiMetaDataPropertyCollection extendedProperties = null;
            if (SqlDbType.Structured == mt.SqlDbType) {
                GetActualFieldsAndProperties(out fields, out extendedProperties, out peekAhead);
            }
            
            return new MSS.SmiParameterMetaData( mt.SqlDbType,
                                            maxLen,
                                            precision,
                                            scale,
                                            localeId,
                                            compareOpts,
                                            null,           // Udt type not used for parameters
                                            SqlDbType.Structured == mt.SqlDbType,
                                            fields,
                                            extendedProperties,
                                            this.ParameterNameFixed,
                                            typeSpecificNamePart1,
                                            typeSpecificNamePart2,
                                            typeSpecificNamePart3,
                                            this.Direction);
        }

        internal bool ParamaterIsSqlType {
            get {
                return _isSqlParameterSqlType;
                }
            set {
                    _isSqlParameterSqlType = value;
                }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.SqlParameter_ParameterName),
        ]
        override public string ParameterName {
            get {
                string parameterName = _parameterName;
                return ((null != parameterName) ? parameterName : ADP.StrEmpty);
            }
            set {
                if (ADP.IsEmpty(value) || (value.Length < TdsEnums.MAX_PARAMETER_NAME_LENGTH)
                    || (('@' == value[0]) && (value.Length <= TdsEnums.MAX_PARAMETER_NAME_LENGTH))) {
                    if (_parameterName != value) {
                        PropertyChanging();
                        _parameterName = value;
                    }
                }
                else {
                    throw SQL.InvalidParameterNameLength(value);
                }
            }
        }

        internal string ParameterNameFixed {
            get {
                string parameterName = ParameterName;
                if ((0 < parameterName.Length) && ('@' != parameterName[0])) {
                    parameterName = "@" + parameterName;
                }
                Debug.Assert(parameterName.Length <= TdsEnums.MAX_PARAMETER_NAME_LENGTH, "parameter name too long");
                return parameterName;
            }
        }

        [DefaultValue((Byte)0)] // MDAC 65862
        [ResCategoryAttribute(Res.DataCategory_Data)]
        [ResDescriptionAttribute(Res.DbDataParameter_Precision)]
        public new Byte Precision {
            get {
                return PrecisionInternal;
            }
            set {
                PrecisionInternal = value;
            }
        }

        internal byte PrecisionInternal {
            get {
                byte precision = _precision;
                SqlDbType dbtype = GetMetaSqlDbTypeOnly();
                if ((0 == precision) && (SqlDbType.Decimal == dbtype)) {
                    precision = ValuePrecision(SqlValue);
                }
                return precision;
            }
            set {
                SqlDbType sqlDbType = SqlDbType;
                if (sqlDbType == SqlDbType.Decimal && value > TdsEnums.MAX_NUMERIC_PRECISION) {
                    throw SQL.PrecisionValueOutOfRange(value);
                }
                if (_precision != value) {
                    PropertyChanging();
                    _precision = value;
                }
            }
        }

        private bool ShouldSerializePrecision() {
            return (0 != _precision);
        }

        [DefaultValue((Byte)0)] // MDAC 65862
        [ResCategoryAttribute(Res.DataCategory_Data)]
        [ResDescriptionAttribute(Res.DbDataParameter_Scale)]
        public new Byte Scale {
            get {
                return ScaleInternal;
            }
            set {
                ScaleInternal = value;
            }
        }
        internal byte ScaleInternal {
            get {
                byte scale = _scale;
                SqlDbType dbtype = GetMetaSqlDbTypeOnly();
                if ((0 == scale) && (SqlDbType.Decimal == dbtype)) {
                    scale = ValueScale(SqlValue);
                }
                return scale;
            }
            set {
                if (_scale != value || !_hasScale) {
                    PropertyChanging();
                    _scale = value;
                    _hasScale = true;
                    _actualSize = -1;   // Invalidate actual size such that it is re-calculated
                }
            }
        }

        private bool ShouldSerializeScale() {
            return (0 != _scale); // V1.0 compat, ignore _hasScale
        }

        [
        RefreshProperties(RefreshProperties.All),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.SqlParameter_SqlDbType),
        System.Data.Common.DbProviderSpecificTypePropertyAttribute(true),
        ]
        public SqlDbType SqlDbType {
            get {
                return GetMetaTypeOnly().SqlDbType;
            }
            set {
                MetaType metatype = _metaType;
                // HACK!!!
                // We didn't want to expose SmallVarBinary on SqlDbType so we 
                // stuck it at the end of SqlDbType in v1.0, except that now 
                // we have new data types after that and it's smack dab in the
                // middle of the valid range.  To prevent folks from setting 
                // this invalid value we have to have this code here until we
                // can take the time to fix it later.
                if ((SqlDbType)TdsEnums.SmallVarBinary == value) {
                    throw SQL.InvalidSqlDbType(value);
                }
                if ((null == metatype) || (metatype.SqlDbType != value)) {
                    PropertyTypeChanging();
                    _metaType = MetaType.GetMetaTypeFromSqlDbType(value, value == SqlDbType.Structured);
                }
            }
        }

        private bool ShouldSerializeSqlDbType() {
            return (null != _metaType);
        }

        public void ResetSqlDbType() {
            if (null != _metaType) {
                PropertyTypeChanging();
                _metaType = null;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public object SqlValue {
            get {
                if (_udtLoadError != null) { // SQL BU DT 329981
                    throw _udtLoadError;
                }

                if (_value != null) {
                    if (_value == DBNull.Value) {
                        return MetaType.GetNullSqlValue(GetMetaTypeOnly().SqlType);
                    }
                    if (_value is INullable) {
                          return _value;
                    }

                    // SQLBU 503165: for Date and DateTime2, return the CLR object directly without converting it to a SqlValue
                    // SQLBU 527900: GetMetaTypeOnly() will convert _value to a string in the case of char or char[], so only check
                    //               the SqlDbType for DateTime. This is the only case when we might return the CLR value directly.
                    if (_value is DateTime) {
                        SqlDbType sqlDbType = GetMetaTypeOnly().SqlDbType;                        
                        if (sqlDbType == SqlDbType.Date || sqlDbType == SqlDbType.DateTime2) {
                            return _value;
                        }
                    }

                    return (MetaType.GetSqlValueFromComVariant(_value));
                }
                else if (_sqlBufferReturnValue != null) {
                    return _sqlBufferReturnValue.SqlValue;
                }
                return null;
            }
            set {
                Value = value;
            }
        }

        [
        Browsable(false),
        EditorBrowsableAttribute(EditorBrowsableState.Advanced)
        ]
        public String UdtTypeName {
            get {
                string typeName = _udtTypeName;
                return ((null != typeName) ? typeName : ADP.StrEmpty);
            }
            set {
                _udtTypeName = value;
            }
        }

        [
        Browsable(false),
        EditorBrowsableAttribute(EditorBrowsableState.Advanced)
        ]
        public String TypeName {
            get {
                string typeName = _typeName;
                return ((null != typeName) ? typeName : ADP.StrEmpty);
            }
            set {
                _typeName = value;
            }
        }

        [
        RefreshProperties(RefreshProperties.All),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DbParameter_Value),
        TypeConverterAttribute(typeof(StringConverter)),
        ]
        override public object Value { // V1.2.3300, XXXParameter V1.0.3300
            get {
                if (_udtLoadError != null) { // SQL BU DT 329981
                    throw _udtLoadError;
                }

                if (_value != null) {
                    return _value;
                }
                else if (_sqlBufferReturnValue != null) {
                    if (ParamaterIsSqlType) {
                        return _sqlBufferReturnValue.SqlValue;
                    }
                    return _sqlBufferReturnValue.Value;
                }
                return null;
            }
            set {
                _value = value;
                _sqlBufferReturnValue = null;
                _coercedValue = null;
                _valueAsINullable = _value as INullable;
                _isSqlParameterSqlType = (_valueAsINullable != null);
                _isNull = ((_value == null) || (_value == DBNull.Value) || ((_isSqlParameterSqlType) && (_valueAsINullable.IsNull)));
                _udtLoadError = null;
                _actualSize = -1;
            }
        }

        internal INullable ValueAsINullable {
            get {
                return _valueAsINullable;
            }
        }

        internal bool IsNull {
            get {
                // NOTE: Udts can change their value any time
                if (_internalMetaType.SqlDbType == Data.SqlDbType.Udt) {
                    _isNull = ((_value == null) || (_value == DBNull.Value) || ((_isSqlParameterSqlType) && (_valueAsINullable.IsNull)));
                }
                return _isNull;
            }
        }

        //
        // always returns data in bytes - except for non-unicode chars, which will be in number of chars
        //
        internal int GetActualSize() {
            MetaType mt = InternalMetaType;
            SqlDbType actualType = mt.SqlDbType;
            // NOTE: Users can change the Udt at any time, so we may need to recalculate
            if ((_actualSize == -1) || (actualType == Data.SqlDbType.Udt)) {
                _actualSize = 0;
                object val = GetCoercedValue();
                bool isSqlVariant = false;

                // 
                if (IsNull && !mt.IsVarTime) {
                    return 0;
                }

                // if this is a backend SQLVariant type, then infer the TDS type from the SQLVariant type
                if (actualType == SqlDbType.Variant) {
                    mt = MetaType.GetMetaTypeFromValue(val, streamAllowed: false);
                    actualType = MetaType.GetSqlDataType(mt.TDSType, 0 /*no user type*/, 0 /*non-nullable type*/).SqlDbType;
                    isSqlVariant = true;
                }

                if (mt.IsFixed) {
                    _actualSize = mt.FixedLength;
                }
                else {
                    // @hack: until we have ForceOffset behavior we have the following semantics:
                    // @hack: if the user supplies a Size through the Size propeprty or constructor,
                    // @hack: we only send a MAX of Size bytes over.  If the actualSize is < Size, then
                    // @hack: we send over actualSize
                    int coercedSize = 0;

                    // get the actual length of the data, in bytes
                    switch (actualType) {
                        case SqlDbType.NChar:
                        case SqlDbType.NVarChar:
                        case SqlDbType.NText:
                        case SqlDbType.Xml:
                            {
                                coercedSize = ((!_isNull) && (!_coercedValueIsDataFeed)) ? (StringSize(val, _coercedValueIsSqlType)) : 0;
                                _actualSize = (ShouldSerializeSize() ? Size : 0);
                                _actualSize = ((ShouldSerializeSize() && (_actualSize <= coercedSize)) ? _actualSize : coercedSize);
                                if (_actualSize == -1)
                                    _actualSize = coercedSize;
                                _actualSize <<= 1;
                            }
                            break;
                        case SqlDbType.Char:
                        case SqlDbType.VarChar:
                        case SqlDbType.Text:
                            {
                                // for these types, ActualSize is the num of chars, not actual bytes - since non-unicode chars are not always uniform size
                                coercedSize = ((!_isNull) && (!_coercedValueIsDataFeed)) ? (StringSize(val, _coercedValueIsSqlType)) : 0;
                                _actualSize = (ShouldSerializeSize() ? Size : 0);
                                _actualSize = ((ShouldSerializeSize() && (_actualSize <= coercedSize)) ? _actualSize : coercedSize);
                                if (_actualSize == -1)
                                    _actualSize = coercedSize;
                            }
                            break;
                        case SqlDbType.Binary:
                        case SqlDbType.VarBinary:
                        case SqlDbType.Image:
                        case SqlDbType.Timestamp:
                            coercedSize = ((!_isNull) && (!_coercedValueIsDataFeed)) ? (BinarySize(val, _coercedValueIsSqlType)) : 0;
                            _actualSize = (ShouldSerializeSize() ? Size : 0);
                            _actualSize = ((ShouldSerializeSize() && (_actualSize <= coercedSize)) ? _actualSize : coercedSize);
                            if (_actualSize == -1)
                                _actualSize = coercedSize;
                            break;
                         case SqlDbType.Udt:
                            //we assume that the object is UDT
                            if (!IsNull) {
                                //call the static function
                                coercedSize = AssemblyCache.GetLength(val);
                            }
                            break;
                        case SqlDbType.Structured:
                            coercedSize = -1;
                            break;
                        case SqlDbType.Time:
                            _actualSize = (isSqlVariant ? 5 : MetaType.GetTimeSizeFromScale(GetActualScale()));
                            break;
                        case SqlDbType.DateTime2:
                            // Date in number of days (3 bytes) + time
                            _actualSize = 3 + (isSqlVariant ? 5 : MetaType.GetTimeSizeFromScale(GetActualScale()));
                            break;
                        case SqlDbType.DateTimeOffset:
                            // Date in days (3 bytes) + offset in minutes (2 bytes) + time
                            _actualSize = 5 + (isSqlVariant ? 5 : MetaType.GetTimeSizeFromScale(GetActualScale()));
                            break;
                        default:
                            Debug.Assert(false, "Unknown variable length type!");
                            break;
                    } // switch

                    // don't even send big values over to the variant
                    if (isSqlVariant && (coercedSize > TdsEnums.TYPE_SIZE_LIMIT))
                        throw SQL.ParameterInvalidVariant(this.ParameterName);
                }
            }

            return _actualSize;
        }

        object ICloneable.Clone() {
            return new SqlParameter(this);
        }

        // Coerced Value is also used in SqlBulkCopy.ConvertValue(object value, _SqlMetaData metadata)
        internal static object CoerceValue(object value, MetaType destinationType, out bool coercedToDataFeed, out bool typeChanged, bool allowStreaming = true) {
            Debug.Assert(!(value is DataFeed), "Value provided should not already be a data feed");
            Debug.Assert(!ADP.IsNull(value), "Value provided should not be null");
            Debug.Assert(null != destinationType, "null destinationType");

            coercedToDataFeed = false;
            typeChanged = false;
            Type currentType = value.GetType();

            if ((typeof(object) != destinationType.ClassType) &&
                    (currentType != destinationType.ClassType) && 
                    ((currentType != destinationType.SqlType) || (SqlDbType.Xml == destinationType.SqlDbType))) {   // Special case for Xml types (since we need to convert SqlXml into a string)
                try {
                    // Assume that the type changed
                    typeChanged = true;
                    if ((typeof(string) == destinationType.ClassType)) {
                        // For Xml data, destination Type is always string
                        if (typeof(SqlXml) == currentType) {
                            value = MetaType.GetStringFromXml((XmlReader)(((SqlXml)value).CreateReader()));
                        }
                        else if (typeof(SqlString) == currentType) {
                            typeChanged = false;   // Do nothing
                        }
                        else if (typeof(XmlReader).IsAssignableFrom(currentType)) {
                            if (allowStreaming) {
                                coercedToDataFeed = true;
                                value = new XmlDataFeed((XmlReader)value);
                            }
                            else {
                                value = MetaType.GetStringFromXml((XmlReader)value);
                            }
                        }
                        else if (typeof(char[]) == currentType) {
                            value = new string((char[])value);
                        }
                        else if (typeof(SqlChars) == currentType) {
                            value = new string(((SqlChars)value).Value);
                        } 
                        else if (value is TextReader && allowStreaming) {
                            coercedToDataFeed = true;
                            value = new TextDataFeed((TextReader)value);
                        }
                        else {
                            value = Convert.ChangeType(value, destinationType.ClassType, (IFormatProvider)null);
                        }
                    }
                    else if ((DbType.Currency == destinationType.DbType) && (typeof(string) == currentType)) {
                        value = Decimal.Parse((string)value, NumberStyles.Currency, (IFormatProvider)null); // WebData 99376
                    }
                    else if ((typeof(SqlBytes) == currentType) && (typeof(byte[]) == destinationType.ClassType)) {
                        typeChanged = false;    // Do nothing
                    }
                    else if ((typeof(string) == currentType) && (SqlDbType.Time == destinationType.SqlDbType)) {
                        value = TimeSpan.Parse((string)value);
                    }
                    else if ((typeof(string) == currentType) && (SqlDbType.DateTimeOffset == destinationType.SqlDbType)) {
                        value = DateTimeOffset.Parse((string)value, (IFormatProvider)null);
                    }
                    else if ((typeof(DateTime) == currentType) && (SqlDbType.DateTimeOffset == destinationType.SqlDbType)) {
                        value = new DateTimeOffset((DateTime)value);
                    }
                    else if (TdsEnums.SQLTABLE == destinationType.TDSType &&
                                (value is DataTable ||
                                value is DbDataReader ||
                                value is System.Collections.Generic.IEnumerable<SqlDataRecord>)) {
                        // no conversion for TVPs.
                        typeChanged = false;
                    }
                    else if (destinationType.ClassType==typeof(byte[]) && value is Stream && allowStreaming) {
                        coercedToDataFeed = true;        
                        value = new StreamDataFeed((Stream)value);
                    }
                    else {
                        value = Convert.ChangeType(value, destinationType.ClassType, (IFormatProvider)null);
                    }
                }
                catch(Exception e) {
                    // 
                    if (!ADP.IsCatchableExceptionType(e)) {
                        throw;
                    }

                    throw ADP.ParameterConversionFailed(value, destinationType.ClassType, e); // WebData 75433
                }
            }

            Debug.Assert(allowStreaming || !coercedToDataFeed, "Streaming is not allowed, but type was coerced into a data feed");
            Debug.Assert(value.GetType() == currentType ^ typeChanged, "Incorrect value for typeChanged");
            return value;
        }

        internal void FixStreamDataForNonPLP() {
            object value = GetCoercedValue();
            AssertCachedPropertiesAreValid();
            if (!_coercedValueIsDataFeed) {
                return;
            }

            _coercedValueIsDataFeed = false;
         
            if (value is TextDataFeed) {
                if (Size > 0) {
                    char[] buffer = new char[Size];
                    int nRead = ((TextDataFeed)value)._source.ReadBlock(buffer, 0, Size);
                    CoercedValue = new string(buffer, 0, nRead);
                }
                else {
                    CoercedValue = ((TextDataFeed)value)._source.ReadToEnd();
                }
                return;
            }

            if (value is StreamDataFeed) {
                if (Size > 0) {
                    byte[] buffer = new byte[Size];
                    int totalRead = 0;
                    Stream sourceStream = ((StreamDataFeed)value)._source;
                    while (totalRead < Size) {
                        int nRead = sourceStream.Read(buffer, totalRead, Size - totalRead);
                        if (nRead == 0) {
                            break;
                        }
                        totalRead += nRead;
                    }
                    if (totalRead < Size) {
                        Array.Resize(ref buffer, totalRead);
                    }
                    CoercedValue = buffer;
                }
                else {
                    MemoryStream ms = new MemoryStream();
                    ((StreamDataFeed)value)._source.CopyTo(ms);
                    CoercedValue = ms.ToArray();
                }
                return;
            }

            if (value is XmlDataFeed) {
                CoercedValue = MetaType.GetStringFromXml(((XmlDataFeed)value)._source);
                return;
            }

            // We should have returned before reaching here
            Debug.Assert(false, "_coercedValueIsDataFeed was true, but the value was not a known DataFeed type");
        }

        private void CloneHelper(SqlParameter destination) {
            CloneHelperCore(destination);
            destination._metaType     = _metaType;
            destination._collation    = _collation;
            destination._xmlSchemaCollectionDatabase = _xmlSchemaCollectionDatabase;
            destination._xmlSchemaCollectionOwningSchema = _xmlSchemaCollectionOwningSchema;
            destination._xmlSchemaCollectionName = _xmlSchemaCollectionName;
            destination._udtTypeName  = _udtTypeName;
            destination._typeName  = _typeName;
            destination._udtLoadError = _udtLoadError;

            destination._parameterName = _parameterName;
            destination._precision = _precision;
            destination._scale = _scale;
            destination._sqlBufferReturnValue = _sqlBufferReturnValue;
            destination._isSqlParameterSqlType = _isSqlParameterSqlType;
            destination._internalMetaType = _internalMetaType;
            destination.CoercedValue     = CoercedValue; // copy cached value reference because of XmlReader problem
            destination._valueAsINullable = _valueAsINullable;
            destination._isNull = _isNull;
            destination._coercedValueIsDataFeed = _coercedValueIsDataFeed;
            destination._coercedValueIsSqlType = _coercedValueIsSqlType;
            destination._actualSize = _actualSize;
        }

        internal byte GetActualPrecision() {
            return ShouldSerializePrecision() ? PrecisionInternal: ValuePrecision(CoercedValue);
        }

        internal byte GetActualScale() {
            if (ShouldSerializeScale()) {
                return ScaleInternal;
            }

            // issue: how could a user specify 0 as the actual scale?
            if (GetMetaTypeOnly().IsVarTime) {
                return TdsEnums.DEFAULT_VARTIME_SCALE;
            }
            return ValueScale(CoercedValue);
        }

        internal int GetParameterSize() {
            return ShouldSerializeSize() ? Size : ValueSize(CoercedValue);
        }

        private void GetActualFieldsAndProperties(out List<MSS.SmiExtendedMetaData> fields, out SmiMetaDataPropertyCollection props, out ParameterPeekAheadValue peekAhead) {
            fields = null;
            props = null;
            peekAhead = null;

            object value = GetCoercedValue();
            if (value is DataTable) {
                DataTable dt = value as DataTable;
                if (dt.Columns.Count <= 0) {
                    throw SQL.NotEnoughColumnsInStructuredType();
                }
                fields = new List<MSS.SmiExtendedMetaData>(dt.Columns.Count);
                bool[] keyCols = new bool[dt.Columns.Count];
                bool hasKey = false;

                // set up primary key as unique key list
                //  do this prior to general metadata loop to favor the primary key
                if (null != dt.PrimaryKey && 0 < dt.PrimaryKey.Length) {
                    foreach(DataColumn col in dt.PrimaryKey) {
                        keyCols[col.Ordinal] = true;
                        hasKey = true;
                    }
                }

                for(int i=0; i<dt.Columns.Count; i++) {
                    fields.Add(MSS.MetaDataUtilsSmi.SmiMetaDataFromDataColumn(dt.Columns[i], dt));

                    // DataColumn uniqueness is only for a single column, so don't add
                    //  more than one.  (keyCols.Count first for assumed minimal perf benefit)
                    if (!hasKey && dt.Columns[i].Unique) {
                        keyCols[i] = true;
                        hasKey = true;
                    }
                }

                // Add unique key property, if any found.
                if (hasKey) {
                    props = new SmiMetaDataPropertyCollection();
                    props[MSS.SmiPropertySelector.UniqueKey] = new MSS.SmiUniqueKeyProperty(new List<bool>(keyCols));
                }
            }
            else if (value is SqlDataReader) {
                fields = new List<MSS.SmiExtendedMetaData>(((SqlDataReader)value).GetInternalSmiMetaData());
                if (fields.Count <= 0) {
                    throw SQL.NotEnoughColumnsInStructuredType();
                }

                bool[] keyCols = new bool[fields.Count];
                bool hasKey = false;
                for(int i=0; i<fields.Count; i++) {
                    MSS.SmiQueryMetaData qmd = fields[i] as MSS.SmiQueryMetaData;
                    if (null != qmd && !qmd.IsKey.IsNull && qmd.IsKey.Value) {
                        keyCols[i] = true;
                        hasKey = true;
                    }
                }

                // Add unique key property, if any found.
                if (hasKey) {
                    props = new SmiMetaDataPropertyCollection();
                    props[MSS.SmiPropertySelector.UniqueKey] = new MSS.SmiUniqueKeyProperty(new List<bool>(keyCols));
                }
            }
            else if (value is IEnumerable<SqlDataRecord>) {
                // must grab the first record of the enumerator to get the metadata
                IEnumerator<MSS.SqlDataRecord> enumerator = ((IEnumerable<MSS.SqlDataRecord>) value).GetEnumerator();
                MSS.SqlDataRecord firstRecord = null;
                try {
                    // no need for fields if there's no rows or no columns -- we'll be sending a null instance anyway.
                    if (enumerator.MoveNext()) {
                        firstRecord = enumerator.Current;
                        int fieldCount = firstRecord.FieldCount;
                        if (0 < fieldCount) {
                            // It's valid!  Grab those fields.
                            bool[] keyCols = new bool[fieldCount];
                            bool[] defaultFields = new bool[fieldCount];
                            bool[] sortOrdinalSpecified = new bool[fieldCount];
                            int maxSortOrdinal = -1;  // largest sort ordinal seen, used to optimize locating holes in the list
                            bool hasKey = false;
                            bool hasDefault = false;
                            int sortCount = 0;
                            SmiOrderProperty.SmiColumnOrder[] sort = new SmiOrderProperty.SmiColumnOrder[fieldCount];
                            fields = new List<MSS.SmiExtendedMetaData>(fieldCount);
                            for (int i = 0; i < fieldCount; i++) {
                                SqlMetaData colMeta = firstRecord.GetSqlMetaData(i);
                                fields.Add(MSS.MetaDataUtilsSmi.SqlMetaDataToSmiExtendedMetaData(colMeta));
                                if (colMeta.IsUniqueKey) {
                                    keyCols[i] = true;
                                    hasKey = true;
                                }

                                if (colMeta.UseServerDefault) {
                                    defaultFields[i] = true;
                                    hasDefault = true;
                                }

                                sort[i].Order = colMeta.SortOrder;
                                if (SortOrder.Unspecified != colMeta.SortOrder) {
                                    // SqlMetaData takes care of checking for negative sort ordinals with specified sort order

                                    // bail early if there's no way sort order could be monotonically increasing
                                    if (fieldCount <= colMeta.SortOrdinal) {
                                        throw SQL.SortOrdinalGreaterThanFieldCount(i, colMeta.SortOrdinal);
                                    }

                                    // Check to make sure we haven't seen this ordinal before
                                    if (sortOrdinalSpecified[colMeta.SortOrdinal]) {
                                        throw SQL.DuplicateSortOrdinal(colMeta.SortOrdinal);
                                    }

                                    sort[i].SortOrdinal = colMeta.SortOrdinal;
                                    sortOrdinalSpecified[colMeta.SortOrdinal] = true;
                                    if (colMeta.SortOrdinal > maxSortOrdinal) {
                                        maxSortOrdinal = colMeta.SortOrdinal;
                                    }
                                    sortCount++;
                                }
                            }

                            if (hasKey) {
                                props = new SmiMetaDataPropertyCollection();
                                props[MSS.SmiPropertySelector.UniqueKey] = new MSS.SmiUniqueKeyProperty(new List<bool>(keyCols));
                            }

                            if (hasDefault) {
                                // May have already created props list in unique key handling
                                if (null == props) {
                                    props = new SmiMetaDataPropertyCollection();
                                }

                                props[MSS.SmiPropertySelector.DefaultFields] = new MSS.SmiDefaultFieldsProperty(new List<bool>(defaultFields));
                            }

                            if (0 < sortCount) {
                                // validate monotonically increasing sort order.
                                //  Since we already checked for duplicates, we just need
                                //  to watch for values outside of the sortCount range.
                                if (maxSortOrdinal >= sortCount) {
                                    // there is at least one hole, find the first one
                                    int i;
                                    for (i = 0; i < sortCount; i++) {
                                        if (!sortOrdinalSpecified[i]) {
                                            break;
                                        }
                                    }
                                    Debug.Assert(i < sortCount, "SqlParameter.GetActualFieldsAndProperties: SortOrdinal hole-finding algorithm failed!");
                                    throw SQL.MissingSortOrdinal(i);
                                }

                                // May have already created props list
                                if (null == props) {
                                    props = new SmiMetaDataPropertyCollection();
                                }

                                props[MSS.SmiPropertySelector.SortOrder] = new MSS.SmiOrderProperty(
                                        new List<SmiOrderProperty.SmiColumnOrder>(sort));
                            }

                            // pack it up so we don't have to rewind to send the first value
                            peekAhead = new ParameterPeekAheadValue();
                            peekAhead.Enumerator = enumerator;
                            peekAhead.FirstRecord = firstRecord;

                            // now that it's all packaged, make sure we don't dispose it.
                            enumerator = null;
                        }
                        else {
                            throw SQL.NotEnoughColumnsInStructuredType();
                        }
                    }
                    else {
                        throw SQL.IEnumerableOfSqlDataRecordHasNoRows();
                    }
                }
                finally {
                    if (enumerator != null) {
                        enumerator.Dispose();
                    }
                }
            }
            else if (value is DbDataReader) {
                DataTable schema = ((DbDataReader)value).GetSchemaTable();
                if (schema.Rows.Count <= 0) {
                    throw SQL.NotEnoughColumnsInStructuredType();
                }

                int fieldCount = schema.Rows.Count;
                fields = new List<MSS.SmiExtendedMetaData>(fieldCount);
                bool[] keyCols = new bool[fieldCount];
                bool hasKey = false;
                int ordinalForIsKey = schema.Columns[SchemaTableColumn.IsKey].Ordinal;
                int ordinalForColumnOrdinal = schema.Columns[SchemaTableColumn.ColumnOrdinal].Ordinal;
                // Extract column metadata
                for(int rowOrdinal=0; rowOrdinal<fieldCount; rowOrdinal++) {
                    DataRow row = schema.Rows[rowOrdinal];
                    SmiExtendedMetaData candidateMd = MSS.MetaDataUtilsSmi.SmiMetaDataFromSchemaTableRow(row);

                    // Determine destination ordinal.  Allow for ordinal not specified by assuming rowOrdinal *is* columnOrdinal
                    //  in that case, but don't worry about mix-and-match of the two techniques
                    int columnOrdinal = rowOrdinal;
                    if (!row.IsNull(ordinalForColumnOrdinal)) {
                       columnOrdinal = (int) row[ordinalForColumnOrdinal];
                    }

                    // After this point, things we are creating (keyCols, fields) should be accessed by columnOrdinal
                    //  while the source should just be accessed via "row".

                    // Watch for out-of-range ordinals
                    if (columnOrdinal >= fieldCount || columnOrdinal < 0) {
                        throw SQL.InvalidSchemaTableOrdinals();
                    }

                    // extend empty space if out-of-order ordinal
                    while (columnOrdinal > fields.Count) {
                        fields.Add(null);
                    }

                    // Now add the candidate to the list
                    if (fields.Count == columnOrdinal) {
                        fields.Add(candidateMd);
                    }
                    else {
                        // Disallow two columns using the same ordinal (even if due to mixing null and non-null columnOrdinals)
                        if (fields[columnOrdinal] != null) {
                            throw SQL.InvalidSchemaTableOrdinals();
                        }

                        // Don't use insert, since it shifts all later columns down a notch
                        fields[columnOrdinal] = candidateMd;
                    }

                    // Propogate key information
                    if (!row.IsNull(ordinalForIsKey) && (bool)row[ordinalForIsKey]) {
                        keyCols[columnOrdinal] = true;
                        hasKey = true;
                    }
                }

#if DEBUG
                // Check for holes
                //  Above loop logic prevents holes since:
                //      1) loop processes fieldcount # of columns
                //      2) no ordinals outside continuous range from 0 to fieldcount - 1 are allowed
                //      3) no duplicate ordinals are allowed
                // But assert no holes to be sure.
                foreach (SmiExtendedMetaData md in fields) {
                    Debug.Assert(null != md, "Shouldn't be able to have holes, since original loop algorithm prevents such.");
                }
#endif

                // Add unique key property, if any defined.
                if (hasKey) {
                    props = new SmiMetaDataPropertyCollection();
                    props[MSS.SmiPropertySelector.UniqueKey] = new MSS.SmiUniqueKeyProperty(new List<bool>(keyCols));
                }
            }
        }

        internal object GetCoercedValue() {
            // NOTE: User can change the Udt at any time
            if ((null == _coercedValue) || (_internalMetaType.SqlDbType == Data.SqlDbType.Udt)) {  // will also be set during parameter Validation
                bool isDataFeed = Value is DataFeed;
                if ((IsNull) || (isDataFeed)) {
                    // No coercion is done for DataFeeds and Nulls
                    _coercedValue = Value;
                    _coercedValueIsSqlType = (_coercedValue==null) ? false: _isSqlParameterSqlType; // set to null for output parameters that keeps _isSqlParameterSqlType
                    _coercedValueIsDataFeed = isDataFeed;
                    _actualSize = IsNull ? 0 : -1;
                }
                else {
                    bool typeChanged;
                    _coercedValue = CoerceValue(Value, _internalMetaType, out _coercedValueIsDataFeed, out typeChanged);
                    _coercedValueIsSqlType = ((_isSqlParameterSqlType) && (!typeChanged));  // Type changed always results in a CLR type
                    _actualSize = -1;
                }
            }
            AssertCachedPropertiesAreValid();
            return _coercedValue;
        }
        
        internal bool CoercedValueIsSqlType {
            get {
                if (null == _coercedValue) {
                    GetCoercedValue();
                }
                AssertCachedPropertiesAreValid();
                return _coercedValueIsSqlType;
            }
        }

        internal bool CoercedValueIsDataFeed {
            get {
                if (null == _coercedValue) {
                    GetCoercedValue();
                }
                AssertCachedPropertiesAreValid();
                return _coercedValueIsDataFeed;
            }
        }

        [Conditional("DEBUG")]
        internal void AssertCachedPropertiesAreValid()
        {
            AssertPropertiesAreValid(_coercedValue, _coercedValueIsSqlType, _coercedValueIsDataFeed, IsNull);
        }

        [Conditional("DEBUG")]
        internal void AssertPropertiesAreValid(object value, bool? isSqlType = null, bool? isDataFeed = null, bool? isNull = null)
        {
            Debug.Assert(!isSqlType.HasValue || (isSqlType.Value == (value is INullable)), "isSqlType is incorrect");
            Debug.Assert(!isDataFeed.HasValue || (isDataFeed.Value == (value is DataFeed)), "isDataFeed is incorrect");
            Debug.Assert(!isNull.HasValue || (isNull.Value == ADP.IsNull(value)), "isNull is incorrect");
        }

        private SqlDbType GetMetaSqlDbTypeOnly() {
            MetaType metaType = _metaType;
            if (null == metaType) { // infer the type from the value
                metaType = MetaType.GetDefaultMetaType();
            }
            return metaType.SqlDbType;
        }

        // This may not be a good thing to do in case someone overloads the parameter type but I
        // don't want to go from SqlDbType -> metaType -> TDSType
        private MetaType GetMetaTypeOnly() {
            if (null != _metaType) {
                return _metaType;
            }
            if (null != _value && DBNull.Value != _value) {
                  // We have a value set by the user then just use that value
                  // char and char[] are not directly supported so we convert those values to string
                  Type valueType = _value.GetType ();
                  if (typeof(char) == valueType) {
                      _value = _value.ToString();
                      valueType = typeof (string);
                  }
                  else if (typeof(char[]) == valueType) {
                     _value = new string((char[])_value);
                     valueType = typeof (string);
                  }
                  return MetaType.GetMetaTypeFromType(valueType);
            }
            else if (null != _sqlBufferReturnValue) {  // value came back from the server
                 Type valueType = _sqlBufferReturnValue.GetTypeFromStorageType (_isSqlParameterSqlType);
                 if (null != valueType) {
                     return MetaType.GetMetaTypeFromType(valueType);
                 }
            }
            return MetaType.GetDefaultMetaType();
        }

        internal void Prepare(SqlCommand cmd) { // MDAC 67063
            if (null == _metaType) {
                throw ADP.PrepareParameterType(cmd);
            }
            else if (!ShouldSerializeSize() && !_metaType.IsFixed) {
                throw ADP.PrepareParameterSize(cmd);
            }
            else if ( (!ShouldSerializePrecision() && !ShouldSerializeScale()) &&  (_metaType.SqlDbType == SqlDbType.Decimal) ) {
                throw ADP.PrepareParameterScale(cmd, SqlDbType.ToString());
            }
        }

        private void PropertyChanging () {
            _internalMetaType = null;
        }

        private void PropertyTypeChanging () {
            PropertyChanging();
            CoercedValue = null;
        }

        internal void SetSqlBuffer (SqlBuffer buff){
            _sqlBufferReturnValue = buff;
            _value = null;
            _coercedValue = null;
            _isNull = _sqlBufferReturnValue.IsNull;
            _coercedValueIsDataFeed = false;
            _coercedValueIsSqlType = false;
            _udtLoadError = null;
            _actualSize = -1;
        }

        internal void SetUdtLoadError(Exception e) { // SQL BU DT 329981
            _udtLoadError = e;
        }

        internal void Validate(int index, bool isCommandProc) {
            MetaType metaType = GetMetaTypeOnly();
            _internalMetaType = metaType;

            // NOTE: (General Criteria): SqlParameter does a Size Validation check and would fail if the size is 0. 
            //                           This condition filters all scenarios where we view a valid size 0.
            if (ADP.IsDirection(this, ParameterDirection.Output) &&
                !ADP.IsDirection(this, ParameterDirection.ReturnValue) && // SQL BU DT 372370
                (!metaType.IsFixed) && 
                !ShouldSerializeSize() && 
                ((null == _value) || Convert.IsDBNull(_value)) && 
                (SqlDbType != SqlDbType.Timestamp) && 
                (SqlDbType != SqlDbType.Udt) &&
                // 

                (SqlDbType != SqlDbType.Xml) &&
                !metaType.IsVarTime) {

                throw ADP.UninitializedParameterSize(index, metaType.ClassType);
            }

            if (metaType.SqlDbType != SqlDbType.Udt && Direction != ParameterDirection.Output) {
                GetCoercedValue();
            }

            //check if the UdtTypeName is specified for Udt params
            if (metaType.SqlDbType == SqlDbType.Udt) {
                if (ADP.IsEmpty (UdtTypeName))
                    throw SQL.MustSetUdtTypeNameForUdtParams ();
            }
            else if (!ADP.IsEmpty (UdtTypeName)) {
                throw SQL.UnexpectedUdtTypeNameForNonUdtParams();
            }

            // Validate structured-type-specific details.
            if (metaType.SqlDbType == SqlDbType.Structured) {
                if (!isCommandProc && ADP.IsEmpty (TypeName))
                    throw SQL.MustSetTypeNameForParam(metaType.TypeName, this.ParameterName);

                if (ParameterDirection.Input != this.Direction) {
                    throw SQL.UnsupportedTVPOutputParameter(this.Direction, this.ParameterName);
                }

                if (DBNull.Value == GetCoercedValue()) {
                    throw SQL.DBNullNotSupportedForTVPValues(this.ParameterName);
                }
            }
            else if (!ADP.IsEmpty (TypeName)) {
                throw SQL.UnexpectedTypeNameForNonStructParams(this.ParameterName);
            }
        }

        // func will change type to that with a 4 byte length if the type has a two
        // byte length and a parameter length > than that expressable in 2 bytes
        internal MetaType ValidateTypeLengths(bool yukonOrNewer) {
            MetaType mt = InternalMetaType;
            // MDAC bug #50839 + #52829 : Since the server will automatically reject any
            // char, varchar, binary, varbinary, nchar, or nvarchar parameter that has a
            // byte sizeInCharacters > 8000 bytes, we promote the parameter to image, text, or ntext.  This
            // allows the user to specify a parameter type using a COM+ datatype and be able to
            // use that parameter against a BLOB column.
            if ((SqlDbType.Udt != mt.SqlDbType) && (false == mt.IsFixed) && (false == mt.IsLong)) { // if type has 2 byte length
                long actualSizeInBytes = this.GetActualSize();
                long sizeInCharacters = this.Size;

                // Bug: VSTFDevDiv #636867
                // Notes:
                // 'actualSizeInBytes' is the size of value passed; 
                // 'sizeInCharacters' is the parameter size;
                // 'actualSizeInBytes' is in bytes; 
                // 'this.Size' is in charaters; 
                // 'sizeInCharacters' is in characters; 
                // 'TdsEnums.TYPE_SIZE_LIMIT' is in bytes;
                // For Non-NCharType and for non-Yukon or greater variables, size should be maintained;
                // Reverting changes from bug VSTFDevDiv # 479739 as it caused an regression;
                // Modifed variable names from 'size' to 'sizeInCharacters', 'actualSize' to 'actualSizeInBytes', and 
                // 'maxSize' to 'maxSizeInBytes'
                // The idea is to
                //  1) revert the regression from bug 479739
                //  2) fix as many scenarios as possible including bug 636867
                //  3) cause no additional regression from 3.5 sp1
                // Keeping these goals in mind - the following are the changes we are making

                long maxSizeInBytes = 0;
                if ((mt.IsNCharType) && (yukonOrNewer))
                    maxSizeInBytes = ((sizeInCharacters * sizeof(char)) > actualSizeInBytes) ? sizeInCharacters * sizeof(char) : actualSizeInBytes;
                else
                {
                    // Notes:
                    // Elevation from (n)(var)char (4001+) to (n)text succeeds without failure only with Yukon and greater.
                    // it fails in sql server 2000
                    maxSizeInBytes = (sizeInCharacters > actualSizeInBytes) ? sizeInCharacters : actualSizeInBytes;
                }

                if ((maxSizeInBytes > TdsEnums.TYPE_SIZE_LIMIT) || (_coercedValueIsDataFeed) || 
                    (sizeInCharacters == -1) || (actualSizeInBytes == -1)) { // is size > size able to be described by 2 bytes
                    if (yukonOrNewer) {
                        // Convert the parameter to its max type
                        mt = MetaType.GetMaxMetaTypeFromMetaType(mt);
                        _metaType = mt;
                        InternalMetaType = mt;
                        if (!mt.IsPlp) {
                            if (mt.SqlDbType == SqlDbType.Xml) {
                                throw ADP.InvalidMetaDataValue();     //Xml should always have IsPartialLength = true
                            }
                            if (mt.SqlDbType == SqlDbType.NVarChar 
                             || mt.SqlDbType == SqlDbType.VarChar 
                             || mt.SqlDbType == SqlDbType.VarBinary) {
                                Size = (int)(SmiMetaData.UnlimitedMaxLengthIndicator);
                            }
                        }
                    }
                    else {
                        switch (mt.SqlDbType) { // widening the SqlDbType is automatic
                        case SqlDbType.Binary:
                        case SqlDbType.VarBinary:
                            mt = MetaType.GetMetaTypeFromSqlDbType (SqlDbType.Image, false);
                            _metaType = mt; // do not use SqlDbType property which calls PropertyTypeChanging resetting coerced value
                            InternalMetaType = mt;
                            break;
                        case SqlDbType.Char:
                        case SqlDbType.VarChar:
                            mt = MetaType.GetMetaTypeFromSqlDbType (SqlDbType.Text, false);
                            _metaType = mt;
                            InternalMetaType = mt;
                            break;
                        case SqlDbType.NChar:
                        case SqlDbType.NVarChar:
                            mt = MetaType.GetMetaTypeFromSqlDbType (SqlDbType.NText, false);
                            _metaType = mt;
                            InternalMetaType = mt;
                            break;
                        default:
                            Debug.Assert(false, "Missed metatype in SqlCommand.BuildParamList()");
                            break;
                        }
                    }
                }
            }
            return mt;
        }

        private byte ValuePrecision(object value) {
            if (value is SqlDecimal) {
                if (((SqlDecimal) value).IsNull) // MDAC #79648
                    return 0;

                return ((SqlDecimal)value).Precision;
            }
            return ValuePrecisionCore(value);
        }

        private byte ValueScale(object value) {
            if (value is SqlDecimal) {
                if (((SqlDecimal) value).IsNull) // MDAC #79648
                    return 0;

                return ((SqlDecimal) value).Scale;
            }
            return ValueScaleCore(value);
        }

        private static int StringSize(object value, bool isSqlType) {
            if (isSqlType) {
                Debug.Assert(!((INullable)value).IsNull, "Should not call StringSize on null values");
                if (value is SqlString) {
                    return ((SqlString)value).Value.Length;
                }
                if (value is SqlChars) {
                    return ((SqlChars)value).Value.Length;
                }
            }
            else {
                string svalue = (value as string);
                if (null != svalue) {
                    return svalue.Length;
                }
                char[] cvalue = (value as char[]);
                if (null != cvalue) {
                    return cvalue.Length;
                }
                if (value is char) {
                    return 1;
                }
            }

            // Didn't match, unknown size
            return 0;
        }

        private static int BinarySize(object value, bool isSqlType) {
            if (isSqlType) {
                Debug.Assert(!((INullable)value).IsNull, "Should not call StringSize on null values");
                if (value is SqlBinary) {
                    return ((SqlBinary)value).Length;
                }
                if (value is SqlBytes) {
                    return ((SqlBytes)value).Value.Length;
                }
            }
            else {
                byte[] bvalue = (value as byte[]);
                if (null != bvalue) {
                    return bvalue.Length;
                }
                if (value is byte) {
                    return 1;
                }
            }

            // Didn't match, unknown size
            return 0;
        }

        private int ValueSize(object value) {
            if (value is SqlString) {
                if (((SqlString) value).IsNull) // MDAC #79648
                    return 0;

                return ((SqlString) value).Value.Length;
            }
            if (value is SqlChars) {
                if (((SqlChars) value).IsNull)
                    return 0;

                return ((SqlChars) value).Value.Length;
            }

            if (value is SqlBinary) {
                if (((SqlBinary) value).IsNull) // MDAC #79648
                    return 0;

                return ((SqlBinary) value).Length;
            }
            if (value is SqlBytes) {
                if (((SqlBytes) value).IsNull)
                    return 0;

                return (int)(((SqlBytes) value).Length);
            }
            if (value is DataFeed)
            {
                // Unknown length
                return 0;                
            }
            return ValueSizeCore(value);
        }

        // 

        // parse an string of the form db.schema.name where any of the three components
        // might have "[" "]" and dots within it.
        // returns:
        //   [0] dbname (or null)
        //   [1] schema (or null)
        //   [2] name
        // NOTE: if perf/space implications of Regex is not a problem, we can get rid
        // of this and use a simple regex to do the parsing
        internal static string[] ParseTypeName(string typeName, bool isUdtTypeName) {
            Debug.Assert(null != typeName, "null typename passed to ParseTypeName");

            try {
                string errorMsg;
                if (isUdtTypeName) {
                    errorMsg = Res.SQL_UDTTypeName;
                }
                else {
                    errorMsg = Res.SQL_TypeName;
                }
                return MultipartIdentifier.ParseMultipartIdentifier(typeName, "[\"", "]\"", '.', 3, true, errorMsg, true);
            }
            catch (ArgumentException) {
                if (isUdtTypeName) {
                    throw SQL.InvalidUdt3PartNameFormat();
                }
                else {
                    throw SQL.InvalidParameterTypeNameFormat();
                }
            }
        }

        sealed internal class SqlParameterConverter : ExpandableObjectConverter {

            // converter classes should have public ctor
            public SqlParameterConverter() {
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
                if ((typeof(System.ComponentModel.Design.Serialization.InstanceDescriptor) == destinationType) && (value is SqlParameter)) {
                    return ConvertToInstanceDescriptor(value as SqlParameter);
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            private System.ComponentModel.Design.Serialization.InstanceDescriptor ConvertToInstanceDescriptor(SqlParameter p) {
                // MDAC 67321 - reducing parameter generated code
                int flags = 0; // if part of the collection - the parametername can't be empty

                if (p.ShouldSerializeSqlDbType()) {
                    flags |= 1;
                }
                if (p.ShouldSerializeSize()) {
                    flags |= 2;
                }
                if (!ADP.IsEmpty(p.SourceColumn)) {
                    flags |= 4;
                }
                if (null != p.Value) {
                    flags |= 8;
                }
                if ((ParameterDirection.Input != p.Direction) || p.IsNullable
                    || p.ShouldSerializePrecision() || p.ShouldSerializeScale()
                    || (DataRowVersion.Current != p.SourceVersion)
                    ) {
                     flags |= 16; // v1.0 everything
                }

                if (p.SourceColumnNullMapping || !ADP.IsEmpty(p.XmlSchemaCollectionDatabase) ||
                    !ADP.IsEmpty(p.XmlSchemaCollectionOwningSchema) || !ADP.IsEmpty(p.XmlSchemaCollectionName)) {
                    flags |= 32; // v2.0 everything
                }

                Type[] ctorParams;
                object[] ctorValues;
                switch(flags) {
                case  0: // ParameterName
                case  1: // SqlDbType
                    ctorParams = new Type[] { typeof(string), typeof(SqlDbType) };
                    ctorValues = new object[] { p.ParameterName, p.SqlDbType };
                    break;
                case  2: // Size
                case  3: // Size, SqlDbType
                    ctorParams = new Type[] { typeof(string), typeof(SqlDbType), typeof(int) };
                    ctorValues = new object[] { p.ParameterName, p.SqlDbType, p.Size };
                    break;
                case  4: // SourceColumn
                case  5: // SourceColumn, SqlDbType
                case  6: // SourceColumn, Size
                case  7: // SourceColumn, Size, SqlDbType
                    ctorParams = new Type[] { typeof(string), typeof(SqlDbType), typeof(int), typeof(string) };
                    ctorValues = new object[] { p.ParameterName, p.SqlDbType, p.Size, p.SourceColumn };
                    break;
                case  8: // Value
                    ctorParams = new Type[] { typeof(string), typeof(object) };
                    ctorValues = new object[] { p.ParameterName, p.Value };
                    break;
                default:
                    if (0 == (32 & flags)) { // v1.0 everything
                        ctorParams = new Type[] {
                                                    typeof(string), typeof(SqlDbType), typeof(int), typeof(ParameterDirection),
                                                    typeof(bool), typeof(byte), typeof(byte),
                                                    typeof(string), typeof(DataRowVersion),
                                                    typeof(object) };
                        ctorValues = new object[] {
                                                      p.ParameterName, p.SqlDbType,  p.Size, p.Direction,
                                                      p.IsNullable, p.PrecisionInternal, p.ScaleInternal,
                                                      p.SourceColumn, p.SourceVersion,
                                                      p.Value };
                    }
                    else { // v2.0 everything - round trip all browsable properties + precision/scale
                        ctorParams = new Type[] {
                                                    typeof(string), typeof(SqlDbType), typeof(int), typeof(ParameterDirection),
                                                    typeof(byte), typeof(byte),
                                                    typeof(string), typeof(DataRowVersion), typeof(bool),
                                                    typeof(object),
                                                    typeof(string), typeof(string),
                                                    typeof(string) };
                        ctorValues = new object[] {
                                                      p.ParameterName, p.SqlDbType,  p.Size, p.Direction,
                                                      p.PrecisionInternal, p.ScaleInternal,
                                                      p.SourceColumn, p.SourceVersion, p.SourceColumnNullMapping,
                                                      p.Value,
                                                      p.XmlSchemaCollectionDatabase, p.XmlSchemaCollectionOwningSchema,
                                                      p.XmlSchemaCollectionName};
                    }
                    break;
                }
                System.Reflection.ConstructorInfo ctor = typeof(SqlParameter).GetConstructor(ctorParams);
                return new System.ComponentModel.Design.Serialization.InstanceDescriptor(ctor, ctorValues);
            }
        }

    }
}
