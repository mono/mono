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
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Mono.Data.MySql {
	internal enum MySqlEnumFieldTypes { 
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

		public static string GetMySqlTypeName(MySqlEnumFieldTypes mysqlFieldType) {
			
			string typeName;

			switch(mysqlFieldType) {
			case MySqlEnumFieldTypes.FIELD_TYPE_DECIMAL: 
				typeName = "decimal";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_TINY: 
				typeName = "tiny";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_SHORT: 
				typeName = "short";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_LONG: 
				typeName = "long";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_FLOAT: 
				typeName = "float";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_DOUBLE: 
				typeName = "double";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_NULL: 
				typeName = "null";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_TIMESTAMP: 
				typeName = "timestamp";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_LONGLONG: 
				typeName = "longlong";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_INT24: 
				typeName = "int24";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_DATE: 
				typeName = "date";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_TIME: 
				typeName = "time";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_DATETIME: 
				typeName = "datetime";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_YEAR: 
				typeName = "year";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_NEWDATE: 
				typeName = "newdate";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_ENUM: 
				typeName = "enum";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_SET: 
				typeName = "set";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_TINY_BLOB: 
				typeName = "tinyblob";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_MEDIUM_BLOB: 
				typeName = "mediumblob";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_LONG_BLOB: 
				typeName = "longblob";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_BLOB: 
				typeName = "blob";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_VAR_STRING: 
				typeName = "varchar";
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_STRING: 
				typeName = "char";
				break;
			default:
				typeName = "text";
				break;
			}
			return typeName;
		}

		public static DbType MySqlTypeToDbType(MySqlEnumFieldTypes mysqlFieldType) {
			DbType dbType;

			// FIXME: verify these translation are correct

			switch(mysqlFieldType) {
			case MySqlEnumFieldTypes.FIELD_TYPE_DECIMAL: 
				dbType = DbType.Decimal;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_TINY: 
				dbType = DbType.Int16;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_SHORT: 
				dbType = DbType.Int16;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_LONG: 
				dbType = DbType.Int32;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_FLOAT: 
				dbType = DbType.Single;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_DOUBLE: 
				dbType = DbType.Double;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_NULL: 
				dbType = DbType.String;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_TIMESTAMP: 
				dbType = DbType.String;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_LONGLONG: 
				dbType = DbType.Int64;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_INT24: 
				dbType = DbType.Int64;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_DATE: 
				dbType = DbType.Date;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_TIME: 
				dbType = DbType.Time;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_DATETIME: 
				dbType = DbType.DateTime;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_YEAR: 
				dbType = DbType.Int16;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_NEWDATE: 
				dbType = DbType.Date;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_ENUM: 
				dbType = DbType.Int32;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_SET: 
				dbType = DbType.String;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_TINY_BLOB: 
			case MySqlEnumFieldTypes.FIELD_TYPE_MEDIUM_BLOB: 
			case MySqlEnumFieldTypes.FIELD_TYPE_LONG_BLOB: 
			case MySqlEnumFieldTypes.FIELD_TYPE_BLOB: 
				dbType = DbType.Binary;
				break;
			case MySqlEnumFieldTypes.FIELD_TYPE_VAR_STRING: 
			case MySqlEnumFieldTypes.FIELD_TYPE_STRING: 
				dbType = DbType.String;
				break;
			default:
				dbType = DbType.String;
				break;
			}

			return dbType;
		}

		// Translates System.Data.DbType to System.Type
		public static Type DbTypeToSystemType (DbType dType) {

			Type typ = null;

			switch(dType) {
			case DbType.String:
				typ = typeof(String);
				break;
			case DbType.Boolean:
				typ = typeof(Boolean);
				break;
			case DbType.Int16: 
				typ = typeof(Int16);
				break;
			case DbType.Int32:
				typ = typeof(Int32);
				break;
			case DbType.Int64:
				typ = typeof(Int64);
				break;
			case DbType.Decimal:
				typ = typeof(Decimal);
				break;
			case DbType.Single:
				typ = typeof(Single);
				break;
			case DbType.Double:
				typ = typeof(Double);
				break;
			case DbType.Date:
			case DbType.Time:
			case DbType.DateTime:
				typ = typeof(DateTime);
				break;
			default:
				typ = typeof(String);
				break;
			}
			return typ;
		}

		// Converts data value from database to .NET System type.
		public static object ConvertDbTypeToSystem (MySqlEnumFieldTypes mysqlFieldType, 
						DbType typ, String myValue) {

			object obj = null;

			//Console.WriteLine("DEBUG: ConvertDbTypeToSystem: " + myValue);
			
			// FIXME: how do you handle NULL and "" for MySQL correctly?
			if(myValue == null) {
				return DBNull.Value;
			}
			else if(myValue.Equals("")) {
				return DBNull.Value;
			}		

			switch(mysqlFieldType) {
				case MySqlEnumFieldTypes.FIELD_TYPE_TIMESTAMP: 
					if(myValue.Equals("00000000000000"))
						return DBNull.Value;
				break;
			}

			// Date, Time, and DateTime 
			// are parsed based on ISO format
			// "YYYY-MM-DD hh:mi:ss"

			switch(typ) {
			case DbType.String:
				obj = String.Copy(myValue); 
				break;
			case DbType.Boolean:
				obj = myValue.Equals("t");
				break;
			case DbType.Int16:
				obj = Int16.Parse(myValue);
				break;
			case DbType.Int32:
				obj = Int32.Parse(myValue);
				break;
			case DbType.Int64:
				obj = Int64.Parse(myValue);
				break;
			case DbType.Decimal:
				obj = Decimal.Parse(myValue);
				break;
			case DbType.Single:
				obj = Single.Parse(myValue);
				break;
			case DbType.Double:
				obj = Double.Parse(myValue);
				break;
			case DbType.Date:
				String[] sd = myValue.Split(new Char[] {'-'});
				obj = new DateTime(
					Int32.Parse(sd[0]), Int32.Parse(sd[1]), Int32.Parse(sd[2]),
					0,0,0);
				break;
			case DbType.Time:
				String[] st = myValue.Split(new Char[] {':'});
				obj = new DateTime(0001,01,01,
					Int32.Parse(st[0]),Int32.Parse(st[1]),Int32.Parse(st[2]));
				break;
			case DbType.DateTime:
				Int32 YYYY,MM,DD,hh,mi,ss;
				YYYY = Int32.Parse(myValue.Substring(0,4));
				MM = Int32.Parse(myValue.Substring(5,2));
				DD = Int32.Parse(myValue.Substring(8,2));
				hh = Int32.Parse(myValue.Substring(11,2));
				mi = Int32.Parse(myValue.Substring(14,2));
				ss = Int32.Parse(myValue.Substring(17,2));
				obj = new DateTime(YYYY,MM,DD,hh,mi,ss,0);
				break;
			default:
				obj = String.Copy(myValue);
				break;
			}

			return obj;
		}

		// FIXME: handle NULLs correctly in MySQL
		public static string DBNullObjectToString(DbType dbtype) {

			string s = "";

			const string NullString = "''";
			const string Null = "NULL";
		
			switch(dbtype) {
			case DbType.String:
				s = NullString;
				break;
			case DbType.Boolean:
				s = NullString;
				break;
			case DbType.Int16:
				s = Null;
				break;
			case DbType.Int32:
				s = Null;
				break;
			case DbType.Int64:
				s = Null;
				break;
			case DbType.Decimal:
				s = Null;
				break;
			case DbType.Single:
				s = Null;
				break;
			case DbType.Double:
				s = Null;
				break;
			case DbType.Date:
				s = NullString;
				break;
			case DbType.Time:
				s = NullString;
				break;
			case DbType.DateTime:
				s = NullString;
				break;
			default:
				// default to DbType.String
				s = NullString;
				break;
			}

			return s;
		}

		// Convert a .NET System value type (Int32, String, Boolean, etc)
		// to a string that can be included within a SQL statement.
		// This is to methods provides the parameters support
		// for the MySQL .NET Data provider
		public static string ObjectToString(DbType dbtype, object obj) {
			
			string s = "";

			// FIXME: how do we handle NULLs?
			//        code is untested
			if(obj.Equals(DBNull.Value)) {
				return DBNullObjectToString(dbtype);
			}

			// Date, Time, and DateTime are expressed in ISO format
			// which is "YYYY-MM-DD hh:mm:ss.ms";
			DateTime dt;
			StringBuilder sb;

			const string zero = "0";

			switch(dbtype) {
			case DbType.String:
				s = "'" + obj + "'";
				break;
			case DbType.Boolean:
				if((bool)obj == true)
					s = "'t'";
				else
					s = "'f'";
				break;
			case DbType.Int16:
				s = obj.ToString();
				break;
			case DbType.Int32:
				s = obj.ToString();
				break;
			case DbType.Int64:
				s = obj.ToString();
				break;
			case DbType.Decimal:
				s = obj.ToString();
				break;
			case DbType.Single:
				s = obj.ToString();
				break;
			case DbType.Double:
				s = obj.ToString();
				break;
			case DbType.Date:
				dt = (DateTime) obj;
				sb = new StringBuilder();
				sb.Append('\'');
				// year
				if(dt.Year < 10)
					sb.Append("000" + dt.Year);
				else if(dt.Year < 100)
					sb.Append("00" + dt.Year);
				else if(dt.Year < 1000)
					sb.Append("0" + dt.Year);
				else
					sb.Append(dt.Year);
				sb.Append("-");
				// month
				if(dt.Month < 10)
					sb.Append(zero + dt.Month);
				else
					sb.Append(dt.Month);
				sb.Append("-");
				// day
				if(dt.Day < 10)
					sb.Append(zero + dt.Day);
				else
					sb.Append(dt.Day);
				sb.Append('\'');
				s = sb.ToString();
				break;
			case DbType.Time:
				dt = (DateTime) obj;
				sb = new StringBuilder();
				sb.Append('\'');
				// hour
				if(dt.Hour < 10)
					sb.Append(zero + dt.Hour);
				else
					sb.Append(dt.Hour);
				sb.Append(":");
				// minute
				if(dt.Minute < 10)
					sb.Append(zero + dt.Minute);
				else
					sb.Append(dt.Minute);
				sb.Append(":");
				// second
				if(dt.Second < 10)
					sb.Append(zero + dt.Second);
				else
					sb.Append(dt.Second);
				sb.Append('\'');
				s = sb.ToString();
				break;
			case DbType.DateTime:
				dt = (DateTime) obj;
				sb = new StringBuilder();
				sb.Append('\'');
				// year
				if(dt.Year < 10)
					sb.Append("000" + dt.Year);
				else if(dt.Year < 100)
					sb.Append("00" + dt.Year);
				else if(dt.Year < 1000)
					sb.Append("0" + dt.Year);
				else
					sb.Append(dt.Year);
				sb.Append("-");
				// month
				if(dt.Month < 10)
					sb.Append(zero + dt.Month);
				else
					sb.Append(dt.Month);
				sb.Append("-");
				// day
				if(dt.Day < 10)
					sb.Append(zero + dt.Day);
				else
					sb.Append(dt.Day);
				sb.Append(" ");
				// hour
				if(dt.Hour < 10)
					sb.Append(zero + dt.Hour);
				else
					sb.Append(dt.Hour);
				sb.Append(":");
				// minute
				if(dt.Minute < 10)
					sb.Append(zero + dt.Minute);
				else
					sb.Append(dt.Minute);
				sb.Append(":");
				// second
				if(dt.Second < 10)
					sb.Append(zero + dt.Second);
				else
					sb.Append(dt.Second);
				sb.Append('\'');
				s = sb.ToString();
				break;
			default:
				// default to DbType.String
				s = "'" + obj + "'";
				break;
			}
			return s;	
		}
	}
}
