//
// MySqlTypes.cs - enums, classes, and structs for handling MySql Types
//
// Assembly: Mono.Data.MySql.dll
// Namespace: Mono.Data.MySql
//
// Author: 
//      Daniel Morgan <danmorg@sc.rr.com>
//
// (c)copyright 2002 Daniel Morgan
//

using System;
using System.Data;

namespace Mono.Data.MySql {
	internal enum enum_field_types { 
		FIELD_TYPE_DECIMAL, 
		FIELD_TYPE_TINY,
		FIELD_TYPE_SHORT,  
		FIELD_TYPE_LONG,
		FIELD_TYPE_FLOAT,  
		FIELD_TYPE_DOUBLE,
		FIELD_TYPE_NULL,   
		FIELD_TYPE_TIMESTAMP,
		FIELD_TYPE_LONGLONG,
		FIELD_TYPE_INT24,
		FIELD_TYPE_DATE,   
		FIELD_TYPE_TIME,
		FIELD_TYPE_DATETIME, 
		FIELD_TYPE_YEAR,
		FIELD_TYPE_NEWDATE,
		FIELD_TYPE_ENUM=247,
		FIELD_TYPE_SET=248,
		FIELD_TYPE_TINY_BLOB=249,
		FIELD_TYPE_MEDIUM_BLOB=250,
		FIELD_TYPE_LONG_BLOB=251,
		FIELD_TYPE_BLOB=252,
		FIELD_TYPE_VAR_STRING=253,
		FIELD_TYPE_STRING=254
	}

	sealed internal class MySqlHelper {
		
		public static DbType MySqlTypeToDbType(enum_field_types mysqlFieldType) {
			DbType dbType;

			// FIXME: verify these translation are correct

			switch(mysqlFieldType) {
			case enum_field_types.FIELD_TYPE_DECIMAL: 
				dbType = DbType.Decimal;
				break;
			case enum_field_types.FIELD_TYPE_TINY: 
				dbType = DbType.Int16;
				break;
			case enum_field_types.FIELD_TYPE_SHORT: 
				dbType = DbType.Int16;
				break;
			case enum_field_types.FIELD_TYPE_LONG: 
				dbType = DbType.Int32;
				break;
			case enum_field_types.FIELD_TYPE_FLOAT: 
				dbType = DbType.Single;
				break;
			case enum_field_types.FIELD_TYPE_DOUBLE: 
				dbType = DbType.Double;
				break;
			case enum_field_types.FIELD_TYPE_NULL: 
				dbType = DbType.String;
				break;
			case enum_field_types.FIELD_TYPE_TIMESTAMP: 
				dbType = DbType.DateTime;
				break;
			case enum_field_types.FIELD_TYPE_LONGLONG: 
				dbType = DbType.Int64;
				break;
			case enum_field_types.FIELD_TYPE_INT24: 
				dbType = DbType.Int64;
				break;
			case enum_field_types.FIELD_TYPE_DATE: 
				dbType = DbType.Date;
				break;
			case enum_field_types.FIELD_TYPE_TIME: 
				dbType = DbType.Time;
				break;
			case enum_field_types.FIELD_TYPE_DATETIME: 
				dbType = DbType.DateTime;
				break;
			case enum_field_types.FIELD_TYPE_YEAR: 
				dbType = DbType.Int16;
				break;
			case enum_field_types.FIELD_TYPE_NEWDATE: 
				dbType = DbType.Date;
				break;
			case enum_field_types.FIELD_TYPE_ENUM: 
				dbType = DbType.Int32;
				break;
			case enum_field_types.FIELD_TYPE_SET: 
				dbType = DbType.String;
				break;
			case enum_field_types.FIELD_TYPE_TINY_BLOB: 
			case enum_field_types.FIELD_TYPE_MEDIUM_BLOB: 
			case enum_field_types.FIELD_TYPE_LONG_BLOB: 
			case enum_field_types.FIELD_TYPE_BLOB: 
				dbType = DbType.Binary;
				break;
			case enum_field_types.FIELD_TYPE_VAR_STRING: 
			case enum_field_types.FIELD_TYPE_STRING: 
				dbType = DbType.String;
				break;
			default:
				dbType = DbType.String;
				break;
			}

			return dbType;
		}
	}
}
