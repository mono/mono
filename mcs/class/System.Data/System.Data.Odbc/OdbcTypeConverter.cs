//
// System.Data.Odbc.OdbcTypeConverter
//
// Author:
//   Sureshkumar T <tsureshkumar@novell.com>
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//
//
// * Type mapping between various odbc driver types.
// For further infomartion between these mapping visit following msdn site
//
//      * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/odbc/htm/
//        odbcc_data_types.asp
//      * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/odbc/htm/
//        odbcconverting_data_from_c_to_sql_data_types.asp
//      * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/odbc/htm/
//        odbcconverting_data_from_sql_to_c_data_types.asp
//      * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/odbc/htm/
//        odbcparameter_data_types.asp
//      * http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/
//        html/frlrfsystemdataodbcodbctypeclasstopic.asp
//
//
// OdbcType             SQL_C_TYPE              SQL_TYPE
// ===================================================================
// BigInt		SQL_C_TYPE.SBIGINT	SQL_TYPE.BIGINT	    
// Binary		SQL_C_TYPE.BINARY	SQL_TYPE.BINARY	    
// Bit			SQL_C_TYPE.BIT		SQL_TYPE.BIT	    
// Char			SQL_C_TYPE.CHAR		SQL_TYPE.CHAR	    
// Date			SQL_C_TYPE.TYPE_DATE	SQL_TYPE.TYPE_DATE  
// DateTime		SQL_C_TYPE.TIMESTAMP	SQL_TYPE.TIMESTAMP  
// Decimal		SQL_C_TYPE.NUMERIC	SQL_TYPE.NUMERIC    
// Double		SQL_C_TYPE.DOUBLE	SQL_TYPE.DOUBLE	    
// Image		SQL_C_TYPE.BINARY	SQL_TYPE.BINARY	    
// Int			SQL_C_TYPE.LONG		SQL_TYPE.INTEGER    
// NChar		SQL_C_TYPE.WCHAR	SQL_TYPE.WCHAR	    
// NText		SQL_C_TYPE.WCHAR	SQL_TYPE.WLONGVARCHAR
// Numeric		SQL_C_TYPE.NUMERIC	SQL_TYPE.NUMERIC    
// NVarChar		SQL_C_TYPE.WCHAR	SQL_TYPE.WVARCHAR   
// Real			SQL_C_TYPE.FLOAT	SQL_TYPE.REAL	    
// SignedBigInt		SQL_C_TYPE.SBIGINT	SQL_TYPE.BIGINT	    
// SmallDateTime	SQL_C_TYPE.TIMESTAMP	SQL_TYPE.TIMESTAMP  
// SmallInt		SQL_C_TYPE.SHORT	SQL_TYPE.SMALLINT   
// Text			SQL_C_TYPE.WCHAR	SQL_TYPE.LONGVARCHAR
// Time			SQL_C_TYPE.TYPE_TIME	SQL_TYPE.TYPE_TIME  
// Timestamp		SQL_C_TYPE.BINARY	SQL_TYPE.BINARY	    
// TinyInt		SQL_C_TYPE.UTINYINT	SQL_TYPE.TINYINT    
// UniqueIdentifier	SQL_C_TYPE.GUID		SQL_TYPE.GUID	    
// VarBinary		SQL_C_TYPE.BINARY	SQL_TYPE.VARBINARY  
// VarChar		SQL_C_TYPE.WCHAR	SQL_TYPE.WVARCHAR   
//====================================================================


using System.Data;
using System.Collections;
using System.Data.Common;

namespace System.Data.Odbc
{ 
        internal sealed class OdbcTypeConverter
        {
                internal struct TypeMap
                {
                        public const short DefaultForOdbcType   = 1;
                        public const short DefaultForSQLCType   = 1<<1;
                        public const short DefaultForSQLType    = 1<<2;
                        public const short DefaultAll           = (DefaultForOdbcType |
                                                                   DefaultForSQLCType |
                                                                   DefaultForSQLType);
                        public OdbcType         OdbcType;
                        public SQL_C_TYPE       SqlCType;
                        public SQL_TYPE         SqlType;
                        public short            BitMask;

                        public TypeMap (OdbcType odbcType, SQL_C_TYPE sqlCType, SQL_TYPE sqlType)
                        {
                                OdbcType        = odbcType;
                                SqlType         = sqlType;
                                SqlCType        = sqlCType;
                                BitMask         = DefaultForOdbcType 
                                        | DefaultForSQLCType
                                        | DefaultForSQLType
                                        ;
                        }

                        public TypeMap (OdbcType odbcType, SQL_C_TYPE sqlCType, SQL_TYPE sqlType, short defaultFlags)
                                : this (odbcType, sqlCType, sqlType)
                        {
                                BitMask = defaultFlags;
                        }
                        
                }


                // FIXME: Write a binary search to make faster
                internal class MapCollection : CollectionBase
                {
                        public TypeMap this [OdbcType odbcType]
                        {
                                get {
                                        foreach (TypeMap map in List){
                                                if (map.OdbcType == odbcType
                                                    && (map.BitMask & TypeMap.DefaultForOdbcType) > 0 )
                                                        return map;
                                        }
                                        throw new ArgumentException (String.Format ("Type mapping for odbc type {0} is not found", 
                                                                                    odbcType.ToString ()
                                                                                    )
                                                                     );
                                }
                                set {
                                        int i = IndexOf (odbcType);
                                        if (i == -1)
                                                Add (value);
                                        List [i] = value;
                                }
                        }

                        public TypeMap this [SQL_C_TYPE sqlCType]
                        {
                                get {
                                        foreach (TypeMap map in List){
                                                if (map.SqlCType == sqlCType
                                                    && (map.BitMask & TypeMap.DefaultForSQLCType) > 0 )
                                                        return map;
                                        }
                                        throw new ArgumentException (String.Format ("Type mapping for odbc type {0} is not found", 
                                                                                    sqlCType.ToString ()
                                                                                    )
                                                                     );
                                }
                                set {
                                        int i = IndexOf (sqlCType);
                                        if (i == -1)
                                                Add (value);
                                        List [i] = value;
                                }
                                
                        }


                        public TypeMap this [SQL_TYPE sqlType]
                        {
                                get {
                                        foreach (TypeMap map in List){
                                                if (map.SqlType == sqlType
                                                    && (map.BitMask & TypeMap.DefaultForSQLType) > 0 )
                                                        return map;
                                        }
                                        throw new ArgumentException (String.Format ("Type mapping for odbc type {0} is not found", 
                                                                                    sqlType.ToString ()
                                                                                    )
                                                                     );
                                }
                                set {
                                        int i = IndexOf (sqlType);
                                        if (i == -1)
                                                Add (value);
                                        List [i] = value;
                                }
                                
                        }

                        public TypeMap this [int index]
                        {
                                get { return (TypeMap) List [index];}
                                set { List [index] = value;}
                        }
                        

                        
                        public int IndexOf (OdbcType odbcType)
                        {
                                for (int i=0; i < List.Count; i++) {
                                        TypeMap map = (TypeMap) List [i];
                                        if (map.OdbcType == odbcType
                                            && (map.BitMask & TypeMap.DefaultForOdbcType) > 0 )
                                                return i;
                                }
                                return -1;
                        }
                        
                        public int IndexOf (SQL_C_TYPE sqlCType)
                        {
                                for (int i=0; i < List.Count; i++) {
                                        TypeMap map = (TypeMap) List [i];
                                        if (map.SqlCType == sqlCType
                                            && (map.BitMask & TypeMap.DefaultForSQLCType) > 0 )
                                                return i;
                                }
                                return -1;
                        }

                        public int IndexOf (SQL_TYPE sqlType)
                        {
                                for (int i=0; i < List.Count; i++) {
                                        TypeMap map = (TypeMap) List [i];
                                        if (map.SqlType == sqlType
                                            && (map.BitMask & TypeMap.DefaultForSQLType) > 0 )
                                                return i;
                                }
                                return -1;
                        }

                        public int Add (TypeMap map)
                        {
                                return List.Add (map);
                        }
                        
                        protected override void OnValidate (object value)
                        {
                                if (value.GetType () != typeof (TypeMap))
                                        throw new ArgumentException ("value is not of type TypeMap");
                        }
                        
                }
                

                private static MapCollection OdbcTypeMap;

                static OdbcTypeConverter ()
                {
                        lock (typeof (OdbcTypeConverter)) {
                                if (OdbcTypeMap == null) {
                                        OdbcTypeMap = new MapCollection ();
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.BigInt,		SQL_C_TYPE.SBIGINT,		SQL_TYPE.BIGINT	    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Binary,		SQL_C_TYPE.BINARY,		SQL_TYPE.BINARY	    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Bit,		SQL_C_TYPE.BIT,		        SQL_TYPE.BIT	    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Char,            SQL_C_TYPE.CHAR,		SQL_TYPE.CHAR	    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Date,		SQL_C_TYPE.TYPE_DATE,		SQL_TYPE.TYPE_DATE  ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.DateTime,	SQL_C_TYPE.TIMESTAMP,		SQL_TYPE.TIMESTAMP  ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Decimal,		SQL_C_TYPE.NUMERIC,		SQL_TYPE.NUMERIC    , TypeMap.DefaultAll & (~TypeMap.DefaultForSQLType)));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Double,		SQL_C_TYPE.DOUBLE,		SQL_TYPE.DOUBLE	    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Image,		SQL_C_TYPE.BINARY,		SQL_TYPE.BINARY	    , TypeMap.DefaultAll & (~TypeMap.DefaultForSQLType)));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Int,		SQL_C_TYPE.LONG,		SQL_TYPE.INTEGER    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.NChar,		SQL_C_TYPE.WCHAR,		SQL_TYPE.WCHAR	    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.NText,		SQL_C_TYPE.WCHAR,		SQL_TYPE.WLONGVARCHAR));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Numeric,		SQL_C_TYPE.NUMERIC,		SQL_TYPE.NUMERIC    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.NVarChar,	SQL_C_TYPE.WCHAR,		SQL_TYPE.WVARCHAR   ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Real,		SQL_C_TYPE.FLOAT,		SQL_TYPE.REAL	    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.SmallDateTime,	SQL_C_TYPE.TIMESTAMP,		SQL_TYPE.TIMESTAMP  , TypeMap.DefaultAll & (~TypeMap.DefaultForSQLType)));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.SmallInt,	SQL_C_TYPE.SHORT,		SQL_TYPE.SMALLINT   ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Text,		SQL_C_TYPE.WCHAR,		SQL_TYPE.LONGVARCHAR));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Time,		SQL_C_TYPE.TYPE_TIME,		SQL_TYPE.TYPE_TIME  ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.Timestamp,	SQL_C_TYPE.BINARY,		SQL_TYPE.BINARY	    , TypeMap.DefaultAll & (~TypeMap.DefaultForSQLType)));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.TinyInt,		SQL_C_TYPE.UTINYINT,		SQL_TYPE.TINYINT    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.UniqueIdentifier,SQL_C_TYPE.GUID,		SQL_TYPE.GUID	    ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.VarBinary,	SQL_C_TYPE.BINARY,		SQL_TYPE.VARBINARY  ));
                                        OdbcTypeMap.Add (new TypeMap (OdbcType.VarChar,		SQL_C_TYPE.WCHAR,		SQL_TYPE.WVARCHAR   , TypeMap.DefaultAll & (~TypeMap.DefaultForSQLType)));
                                }
                                
                        }
                }
                
                public static SQL_C_TYPE ConvertToSqlCType (OdbcType type)
                {
                        return OdbcTypeMap [type].SqlCType;
                }
                
                public static SQL_TYPE ConvertToSqlType (OdbcType type)
                {
                        return OdbcTypeMap [type].SqlType;
                }


                public static OdbcType ConvertToOdbcType (SQL_TYPE sqlType)
                {
                        // Unmapped SQL Types
                        //
                        //#define SQL_FLOAT     6
                        //	could map to SQL_DOUBLE?
                        //#define SQL_INTERVAL	10
                        //	could map to SmallDateTime?
                        return GetTypeMap (sqlType).OdbcType;
                }

                public static TypeMap GetTypeMap (SQL_TYPE sqlType)
                {
                        TypeMap map;
                        try {
                                map  = OdbcTypeMap [sqlType];
                                return map;
                        } catch (ArgumentException) {
                                
                        }

                        // If not in default translation
                        map = new TypeMap ();
                        map.SqlType = sqlType;
                        switch (sqlType) {
			case SQL_TYPE.DATE:                      
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.TYPE_DATE;
                                return map;

			case SQL_TYPE.DECIMAL:                      
                                map.OdbcType = OdbcType.Decimal;
                                map.SqlCType = SQL_C_TYPE.CHAR;
                                return map;

			case SQL_TYPE.INTERVAL_DAY:              
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_DAY;
                                return map;

			case SQL_TYPE.INTERVAL_DAY_TO_HOUR:      
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_DAY_TO_HOUR;
                                return map;

			case SQL_TYPE.INTERVAL_DAY_TO_MINUTE:    
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_DAY_TO_MINUTE;
                                return map;

			case SQL_TYPE.INTERVAL_DAY_TO_SECOND:    
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_DAY_TO_SECOND;
                                return map;

			case SQL_TYPE.INTERVAL_HOUR:             
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_HOUR;
                                return map;

			case SQL_TYPE.INTERVAL_HOUR_TO_MINUTE:   
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_HOUR_TO_MINUTE;
                                return map;

			case SQL_TYPE.INTERVAL_HOUR_TO_SECOND:   
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_HOUR_TO_SECOND;
                                return map;

			case SQL_TYPE.INTERVAL_MINUTE:           
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_MINUTE;
                                return map;

			case SQL_TYPE.INTERVAL_MINUTE_TO_SECOND: 
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_MINUTE_TO_SECOND;
                                return map;

			case SQL_TYPE.INTERVAL_MONTH:            
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_MONTH;
                                return map;

			case SQL_TYPE.INTERVAL_SECOND:           
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_SECOND;
                                return map;

			case SQL_TYPE.INTERVAL_YEAR:             
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_YEAR;
                                return map;

			case SQL_TYPE.INTERVAL_YEAR_TO_MONTH:    
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.INTERVAL_YEAR_TO_MONTH;
                                return map;

                        case SQL_TYPE.LONGVARBINARY:    
                                map.OdbcType = OdbcType.Binary;
                                map.SqlCType = SQL_C_TYPE.BINARY;
                                return map;

			case SQL_TYPE.TIME:                      
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.TIME;
                                return map;

			case SQL_TYPE.TYPE_TIMESTAMP:            
                                map.OdbcType = OdbcType.DateTime;
                                map.SqlCType = SQL_C_TYPE.TIMESTAMP;
                                return map;

			case SQL_TYPE.VARCHAR:                   
                                map.OdbcType = OdbcType.VarChar;
                                map.SqlCType = SQL_C_TYPE.CHAR;
                                return map;

                        default:                        
                                map.OdbcType = OdbcType.NVarChar;
                                map.SqlCType = SQL_C_TYPE.WCHAR;
                                return map;
                        }
                }
                
        }
}
