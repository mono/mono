//
// PostgresTypes.cs - holding methods to convert 
//                    between PostgreSQL types and .NET types
//
// Author:
//     Daniel Morgan <danmorg@sc.rr.com>
//
// (c)copyright 2002 Daniel Morgan
//

// Note: this might become PostgresType and PostgresTypeCollection
//       also, the PostgresTypes that exist as an inner internal class
//       within PgSqlConnection maybe moved here in the future

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Mono.Data.PostgreSqlClient {

	internal struct PostgresType {
		public int oid;
		public string typname;
		public DbType dbType;
	}

	sealed internal class PostgresHelper {

		// translates the PostgreSQL typname to System.Data.DbType
		public static DbType TypnameToSqlDbType(string typname) {
			DbType sqlType;
			
			// FIXME: use hashtable here?

			switch(typname) {

			case "abstime":
				sqlType = DbType.Int32;
				break;

			case "aclitem":
				sqlType = DbType.String;
				break;

			case "bit":
				sqlType = DbType.String;
				break;

			case "bool":
				sqlType = DbType.Boolean;
				break;

			case "box":
				sqlType = DbType.String;
				break;

			case "bpchar":
				sqlType = DbType.String;
				break;

			case "bytea":
				sqlType = DbType.String;
				break;

			case "char":
				sqlType = DbType.String;
				break;

			case "cidr":
				sqlType = DbType.String;
				break;

			case "circle":
				sqlType = DbType.String;
				break;

			case "date":
				sqlType = DbType.Date;
				break;

			case "float4":
				sqlType = DbType.Single;
				break;

			case "float8":
				sqlType = DbType.Double;
				break;

			case "inet":
				sqlType = DbType.String;
				break;

			case "int2":
				sqlType = DbType.Int16;
				break;

			case "int4":
				sqlType = DbType.Int32;
				break;

			case "int8":
				sqlType = DbType.Int64;
				break;

			case "interval":
				sqlType = DbType.String;
				break;

			case "line":
				sqlType = DbType.String;
				break;

			case "lseg":
				sqlType = DbType.String;
				break;

			case "macaddr":
				sqlType = DbType.String;
				break;

			case "money":
				sqlType = DbType.Decimal;
				break;

			case "name":
				sqlType = DbType.String;
				break;

			case "numeric":
				sqlType = DbType.Decimal;
				break;

			case "oid":
				sqlType = DbType.Int32;
				break;

			case "path":
				sqlType = DbType.String;
				break;

			case "point":
				sqlType = DbType.String;
				break;

			case "polygon":
				sqlType = DbType.String;
				break;

			case "refcursor":
				sqlType = DbType.String;
				break;

			case "reltime":
				sqlType = DbType.String;
				break;

			case "text":
				sqlType = DbType.String;
				break;

			case "time":
				sqlType = DbType.Time;
				break;

			case "timestamp":
				sqlType = DbType.DateTime;
				break;

			case "timestamptz":
				sqlType = DbType.DateTime;
				break;

			case "timetz":
				sqlType = DbType.DateTime;
				break;

			case "tinterval":
				sqlType = DbType.String;
				break;

			case "varbit":
				sqlType = DbType.String;
				break;

			case "varchar":
				sqlType = DbType.String;
				break;

			default:
				sqlType = DbType.String;
				break;
			}
			return sqlType;
		}
		
		// Converts data value from database to .NET System type.
		public static object ConvertDbTypeToSystem (DbType typ, String value) {
			object obj = null;

			// FIXME: more types need 
			//        to be converted 
			//        from PostgreSQL oid type
			//        to .NET System.<type>

			// FIXME: need to handle a NULL for each type
			//       maybe setting obj to System.DBNull.Value ?

			
			if(value == null) {
				//Console.WriteLine("ConvertDbTypeToSystemDbType typ: " +
				//	typ + " value is null");
				return null;
			}
			else if(value.Equals("")) {
				//Console.WriteLine("ConvertDbTypeToSystemDbType typ: " +
				//	typ + " value is string empty");
				return null;
			}
			
			//Console.WriteLine("ConvertDbTypeToSystemDbType typ: " +
			//	typ + " value: " + value);

			// Date, Time, and DateTime 
			// are parsed based on ISO format
			// "YYYY-MM-DD hh:mi:ss.ms"

			switch(typ) {
			case DbType.String:
				obj = String.Copy(value); 
				break;
			case DbType.Boolean:
				obj = value.Equals("t");
				break;
			case DbType.Int16:
				obj = Int16.Parse(value);
				break;
			case DbType.Int32:
				obj = Int32.Parse(value);
				break;
			case DbType.Int64:
				obj = Int64.Parse(value);
				break;
			case DbType.Decimal:
				obj = Decimal.Parse(value);
				break;
			case DbType.Single:
				obj = Single.Parse(value);
				break;
			case DbType.Double:
				obj = Double.Parse(value);
				break;
			case DbType.Date:
				String[] sd = value.Split(new Char[] {'-'});
				obj = new DateTime(
					Int32.Parse(sd[0]), Int32.Parse(sd[1]), Int32.Parse(sd[2]),
					0,0,0);
				break;
			case DbType.Time:
				String[] st = value.Split(new Char[] {':'});
				obj = new DateTime(0001,01,01,
					Int32.Parse(st[0]),Int32.Parse(st[1]),Int32.Parse(st[2]));
				break;
			case DbType.DateTime:
				Int32 YYYY,MM,DD,hh,mi,ss,ms;
				YYYY = Int32.Parse(value.Substring(0,4));
				MM = Int32.Parse(value.Substring(5,2));
				DD = Int32.Parse(value.Substring(8,2));
				hh = Int32.Parse(value.Substring(11,2));
				mi = Int32.Parse(value.Substring(14,2));
				ss = Int32.Parse(value.Substring(17,2));
				ms = Int32.Parse(value.Substring(20,2));
				obj = new DateTime(YYYY,MM,DD,hh,mi,ss,ms);
				break;
			default:
				obj = String.Copy(value);
				break;
			}

			return obj;
		}
		
		// Translates System.Data.DbType to System.Type
		public static Type DbTypeToSystemType (DbType dType) {
			// FIXME: more types need 
			//        to be mapped
			//        from PostgreSQL oid type
			//        to .NET System.<type>

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

		// Find DbType for oid
		// which requires a look up of PostgresTypes
		// DbType <-> typname <-> oid
		public static string OidToTypname (int oid, ArrayList pgTypes) {
			// FIXME: more types need 
			//        to be mapped
			//        from PostgreSQL oid type
			//        to .NET System.<type>
			
			string typname = "text"; // default
			int i;
			for(i = 0; i < pgTypes.Count; i++) {
				PostgresType pt = (PostgresType) pgTypes[i];
				if(pt.oid == oid) {
					typname = pt.typname;
					break; 
				}
			}

			return typname;
		}

		// Convert a .NET System value type (Int32, String, Boolean, etc)
		// to a string that can be included within a SQL statement.
		// This is to methods provides the parameters support
		// for the PostgreSQL .NET Data provider
		public static string ObjectToString(DbType dbtype, object obj) {
			
			// TODO: how do we handle a NULL?
			//if(isNull == true)
			//	return "NULL";

			string s;

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
				sb.Append(".");
				// millisecond
				if(dt.Millisecond < 10)
					sb.Append(zero + dt.Millisecond);
				else
					sb.Append(dt.Millisecond);
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