//
// System.Data.SqlClient.PostgresLibrary.cs  
//
// PInvoke methods to libpq
// which is PostgreSQL client library
//
// May also contain enumerations,
// data types, or wrapper methods.
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;

namespace System.Data.SqlClient {

	/* IMPORTANT: DO NOT CHANGE ANY OF THESE ENUMS BELOW */
	
	internal enum ConnStatusType
	{
		CONNECTION_OK,
		CONNECTION_BAD,
		CONNECTION_STARTED,
		CONNECTION_MADE,
		CONNECTION_AWAITING_RESPONSE,
		CONNECTION_AUTH_OK,			 
		CONNECTION_SETENV		
	} 

	internal enum PostgresPollingStatusType
	{
		PGRES_POLLING_FAILED = 0,
		PGRES_POLLING_READING,
		PGRES_POLLING_WRITING,
		PGRES_POLLING_OK,
		PGRES_POLLING_ACTIVE
	}

	internal enum ExecStatusType
	{
		PGRES_EMPTY_QUERY = 0,
		PGRES_COMMAND_OK,			
		PGRES_TUPLES_OK,			
		PGRES_COPY_OUT,				
		PGRES_COPY_IN,				
		PGRES_BAD_RESPONSE,			
		PGRES_NONFATAL_ERROR,
		PGRES_FATAL_ERROR
	}

	internal struct PostgresType {
		public int oid;
		public string typname;
		public DbType dbType;
	}

	sealed internal class PostgresHelper {

		// translates the PostgreSQL typname to System.Data.DbType
		public static DbType TypnameToSqlDbType(string typname) {
			DbType sqlType;
			
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
				sqlType = DbType.String;
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
				sqlType = DbType.String;
				break;

			case "timestamp":
				sqlType = DbType.String;
				break;

			case "timestamptz":
				sqlType = DbType.String;
				break;

			case "timetz":
				sqlType = DbType.String;
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

			switch(typ) {
			case DbType.String:
				obj = (object) String.Copy(value); 
				break;
			case DbType.Boolean:
				obj = (object) Boolean.Parse(value);
				break;
			case DbType.Int16:
				obj = (object) Int16.Parse(value);
				break;
			case DbType.Int32:
				obj = (object) Int32.Parse(value);
				break;
			case DbType.Int64:
				obj = (object) Int64.Parse(value);
				break;
			case DbType.Decimal:
				obj = (object) Decimal.Parse(value);
				break;
			case DbType.Single:
				obj = (object) Single.Parse(value);
				break;
			case DbType.Double:
				obj = (object) Double.Parse(value);
				break;
			default:
				obj = (object) String.Copy(value);
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

	}

	sealed internal class PostgresLibrary
	{
		#region PInvoke Functions

		// pinvoke prototypes to PostgreSQL client library
		// pq.dll on windows and libpq.so on linux

		[DllImport("pq")]
		public static extern IntPtr PQconnectStart (string conninfo);
		// PGconn *PQconnectStart(const char *conninfo);

		[DllImport("pq")]
		public static extern PostgresPollingStatusType PQconnectPoll (IntPtr conn);
		// PostgresPollingStatusType PQconnectPoll(PGconn *conn);	

		[DllImport("pq")]
		public static extern IntPtr PQconnectdb (string conninfo);
		// PGconn *PQconnectdb(const char *conninfo);

		[DllImport("pq")]
		public static extern IntPtr PQsetdbLogin (string pghost, 
			string pgport, string pgoptions, 
			string pgtty, string dbName, 
			string login, string pwd);
		// PGconn *PQsetdbLogin(const char *pghost, 
		//		const char *pgport, const char *pgoptions, 
		//		const char *pgtty, const char *dbName, 
		//		const char *login, const char *pwd);

		[DllImport("pq")]
		public static extern void PQfinish (IntPtr conn);
		// void PQfinish(PGconn *conn);

		[DllImport("pq")]
		public static extern IntPtr PQconndefaults ();
		// PQconninfoOption *PQconndefaults(void);

		[DllImport("pq")]
		public static extern void PQconninfoFree (IntPtr connOptions);
		// void PQconninfoFree(PQconninfoOption *connOptions);

		[DllImport("pq")]
		public static extern int PQresetStart (IntPtr conn);
		// int PQresetStart(PGconn *conn);

		[DllImport("pq")]
		public static extern IntPtr PQresetPoll (IntPtr conn);
		// PostgresPollingStatusType PQresetPoll(PGconn *conn);

		[DllImport("pq")]
		public static extern void PQreset (IntPtr conn);
		// void PQreset(PGconn *conn);

		[DllImport("pq")]
		public static extern int PQrequestCancel (IntPtr conn);
		// int PQrequestCancel(PGconn *conn);

		[DllImport("pq")]
		public static extern string PQdb (IntPtr conn);
		// char *PQdb(const PGconn *conn);

		[DllImport("pq")]
		public static extern string PQuser (IntPtr conn);
		// char *PQuser(const PGconn *conn);

		[DllImport("pq")]
		public static extern string PQpass (IntPtr conn);
		// char *PQpass(const PGconn *conn);

		[DllImport("pq")]
		public static extern string PQhost (IntPtr conn);
		// char *PQhost(const PGconn *conn);

		[DllImport("pq")]
		public static extern string PQport (IntPtr conn);
		// char *PQport(const PGconn *conn);

		[DllImport("pq")]
		public static extern string PQtty (IntPtr conn);
		// char *PQtty(const PGconn *conn);

		[DllImport("pq")]
		public static extern string PQoptions (IntPtr conn);
		// char *PQoptions(const PGconn *conn);

		[DllImport("pq")]
		public static extern ConnStatusType PQstatus (IntPtr conn);
		// ConnStatusType PQstatus(const PGconn *conn);

		[DllImport("pq")]
		public static extern string PQerrorMessage (IntPtr conn);
		// char *PQerrorMessage(const PGconn *conn);

		[DllImport("pq")]
		public static extern int PQsocket (IntPtr conn);
		// int PQsocket(const PGconn *conn);

		[DllImport("pq")]
		public static extern int PQbackendPID (IntPtr conn);
		// int PQbackendPID(const PGconn *conn);

		[DllImport("pq")]
		public static extern int PQclientEncoding (IntPtr conn);
		// int PQclientEncoding(const PGconn *conn);

		[DllImport("pq")]
		public static extern int PQsetClientEncoding (IntPtr conn,
			string encoding);
		// int PQsetClientEncoding(PGconn *conn, 
		//		const char *encoding);

		//FIXME: when loading, causes runtime exception
		//[DllImport("pq")]
		//public static extern IntPtr PQgetssl (IntPtr conn);
		// SSL *PQgetssl(PGconn *conn);

		[DllImport("pq")]
		public static extern void PQtrace (IntPtr conn, 
			IntPtr debug_port);
		// void PQtrace(PGconn *conn, 
		//		FILE *debug_port);

		[DllImport("pq")]
		public static extern void PQuntrace (IntPtr conn);
		// void PQuntrace(PGconn *conn);

		[DllImport("pq")]
		public static extern IntPtr PQsetNoticeProcessor (IntPtr conn,
			IntPtr proc, IntPtr arg);
		// PQnoticeProcessor PQsetNoticeProcessor(PGconn *conn, 
		//		PQnoticeProcessor proc, void *arg);

		[DllImport("pq")]
		public static extern int PQescapeString (string to,
                        string from, int length);
		// size_t PQescapeString(char *to, 
                //      const char *from, size_t length);

		[DllImport("pq")]
		public static extern string PQescapeBytea (string bintext,
                        int binlen, IntPtr bytealen);
		// unsigned char *PQescapeBytea(unsigned char *bintext, 
                //      size_t binlen, size_t *bytealen);

		[DllImport("pq")]
		public static extern IntPtr PQexec (IntPtr conn,
                        string query);
		// PGresult *PQexec(PGconn *conn, 
                //      const char *query);

		[DllImport("pq")]
		public static extern IntPtr PQnotifies (IntPtr conn);
		// PGnotify *PQnotifies(PGconn *conn);

		[DllImport("pq")]
		public static extern void PQfreeNotify (IntPtr notify);
		// void PQfreeNotify(PGnotify *notify);

		[DllImport("pq")]
		public static extern int PQsendQuery (IntPtr conn,
                        string query);
		// int PQsendQuery(PGconn *conn, 
                //      const char *query);

		[DllImport("pq")]
		public static extern IntPtr PQgetResult (IntPtr conn);
		// PGresult *PQgetResult(PGconn *conn);

		[DllImport("pq")]
		public static extern int PQisBusy (IntPtr conn);
		// int PQisBusy(PGconn *conn);

		[DllImport("pq")]
		public static extern int PQconsumeInput (IntPtr conn);
		// int PQconsumeInput(PGconn *conn);

		[DllImport("pq")]
		public static extern int PQgetline (IntPtr conn,
                        string str, int length);
		// int PQgetline(PGconn *conn,
                //      char *string, int length);

		[DllImport("pq")]
		public static extern int PQputline (IntPtr conn,
                        string str);
		// int PQputline(PGconn *conn, 
                //      const char *string);

		[DllImport("pq")]
		public static extern int PQgetlineAsync (IntPtr conn,
                        string buffer, int bufsize);
		// int PQgetlineAsync(PGconn *conn, char *buffer,
                //      int bufsize);

		[DllImport("pq")]
		public static extern int PQputnbytes (IntPtr conn,
                        string buffer, int nbytes);
		// int PQputnbytes(PGconn *conn, 
                //const char *buffer, int nbytes);

		[DllImport("pq")]
		public static extern int PQendcopy (IntPtr conn);
		// int PQendcopy(PGconn *conn);

		[DllImport("pq")]
		public static extern int PQsetnonblocking (IntPtr conn,
                        int arg);
		// int PQsetnonblocking(PGconn *conn, int arg);

		[DllImport("pq")]
		public static extern int PQisnonblocking (IntPtr conn);
		// int PQisnonblocking(const PGconn *conn);

		[DllImport("pq")]
		public static extern int PQflush (IntPtr conn);
		// int PQflush(PGconn *conn);

		[DllImport("pq")]
		public static extern IntPtr PQfn (IntPtr conn, int fnid, 
                        IntPtr result_buf, IntPtr result_len, 
                        int result_is_int, IntPtr args,
                        int nargs);
		// PGresult *PQfn(PGconn *conn, int fnid, 
                //      int *result_buf, int *result_len, 
                //      int result_is_int, const PQArgBlock *args,
                //      int nargs);

		[DllImport("pq")]
		public static extern ExecStatusType PQresultStatus (IntPtr res);
		// ExecStatusType PQresultStatus(const PGresult *res);

		[DllImport("pq")]
		public static extern string PQresStatus (ExecStatusType status);
		// char *PQresStatus(ExecStatusType status);

		[DllImport("pq")]
		public static extern string PQresultErrorMessage (IntPtr res);
		// char *PQresultErrorMessage(const PGresult *res);

		[DllImport("pq")]
		public static extern int PQntuples (IntPtr res);
		// int PQntuples(const PGresult *res);

		[DllImport("pq")]
		public static extern int PQnfields (IntPtr res);
		// int PQnfields(const PGresult *res);

		[DllImport("pq")]
		public static extern int PQbinaryTuples (IntPtr res);
		// int PQbinaryTuples(const PGresult *res);

		[DllImport("pq")]
		public static extern string PQfname (IntPtr res,
                        int field_num);
		// char *PQfname(const PGresult *res,
                //      int field_num);

		[DllImport("pq")]
		public static extern int PQfnumber (IntPtr res,
                        string field_name);
		// int PQfnumber(const PGresult *res, 
                //      const char *field_name);

		[DllImport("pq")]
		public static extern int PQftype (IntPtr res,
                        int field_num);
		// Oid PQftype(const PGresult *res,
                //      int field_num);

		[DllImport("pq")]
		public static extern int PQfsize (IntPtr res,
                        int field_num);
		// int PQfsize(const PGresult *res,
                //      int field_num);

		[DllImport("pq")]
		public static extern int PQfmod (IntPtr res, int field_num);
		// int PQfmod(const PGresult *res, int field_num);

		[DllImport("pq")]
		public static extern string PQcmdStatus (IntPtr res);
		// char *PQcmdStatus(PGresult *res);

		[DllImport("pq")]
		public static extern string PQoidStatus (IntPtr res);
		// char *PQoidStatus(const PGresult *res);

		[DllImport("pq")]
		public static extern int PQoidValue (IntPtr res);
		// Oid PQoidValue(const PGresult *res);

		[DllImport("pq")]
		public static extern string PQcmdTuples (IntPtr res);
		// char *PQcmdTuples(PGresult *res);

		[DllImport("pq")]
		public static extern string PQgetvalue (IntPtr res,
                        int tup_num, int field_num);
		// char *PQgetvalue(const PGresult *res,
                //      int tup_num, int field_num);

		[DllImport("pq")]
		public static extern int PQgetlength (IntPtr res,
                        int tup_num, int field_num);
		// int PQgetlength(const PGresult *res,
                //      int tup_num, int field_num);

		[DllImport("pq")]
		public static extern int PQgetisnull (IntPtr res,
                        int tup_num, int field_num);
		// int PQgetisnull(const PGresult *res,
                //      int tup_num, int field_num);

		[DllImport("pq")]
		public static extern void PQclear (IntPtr res);
		// void PQclear(PGresult *res);

		[DllImport("pq")]
		public static extern IntPtr PQmakeEmptyPGresult (IntPtr conn,
                        IntPtr status);
		// PGresult *PQmakeEmptyPGresult(PGconn *conn,
                //      ExecStatusType status);

		[DllImport("pq")]
		public static extern void PQprint (IntPtr fout,
                        IntPtr res, IntPtr ps);
		// void PQprint(FILE *fout,
                //      const PGresult *res, const PQprintOpt *ps);

		[DllImport("pq")]
		public static extern void PQdisplayTuples (IntPtr res,
                        IntPtr fp, int fillAlign, string fieldSep, 
                        int printHeader, int quiet);
		// void PQdisplayTuples(const PGresult *res, 
                //	FILE *fp, int fillAlign, const char *fieldSep, 
                //	int printHeader, int quiet);

		[DllImport("pq")]
		public static extern void PQprintTuples (IntPtr res,
			IntPtr fout, int printAttName, int terseOutput, 
			int width);
		// void PQprintTuples(const PGresult *res,
		//	FILE *fout, int printAttName, int terseOutput, 
		//	int width);						

		[DllImport("pq")]
		public static extern int lo_open (IntPtr conn,
			int lobjId, int mode);
		// int lo_open(PGconn *conn,
		//	Oid lobjId, int mode);

		[DllImport("pq")]
		public static extern int lo_close (IntPtr conn, int fd);
		// int lo_close(PGconn *conn, int fd);

		[DllImport("pq")]
		public static extern int lo_read (IntPtr conn,
			int fd, string buf, int len);
		// int lo_read(PGconn *conn,
		//	int fd, char *buf, size_t len);

		[DllImport("pq")]
		public static extern int lo_write (IntPtr conn,
			int fd, string buf, int len);
		// int lo_write(PGconn *conn,
		//	int fd, char *buf, size_t len);

		[DllImport("pq")]
		public static extern int lo_lseek (IntPtr conn,
			int fd, int offset, int whence);
		// int lo_lseek(PGconn *conn, 
		//	int fd, int offset, int whence);

		[DllImport("pq")]
		public static extern int lo_creat (IntPtr conn,
			int mode);
		// Oid lo_creat(PGconn *conn,
		//	int mode);

		[DllImport("pq")]
		public static extern int lo_tell (IntPtr conn, int fd);
		// int lo_tell(PGconn *conn, int fd);

		[DllImport("pq")]
		public static extern int lo_unlink (IntPtr conn,
			int lobjId);
		// int lo_unlink(PGconn *conn,
		//	Oid lobjId);

		[DllImport("pq")]
		public static extern int lo_import (IntPtr conn,
			string filename);
		// Oid lo_import(PGconn *conn,
		//	const char *filename);

		[DllImport("pq")]
		public static extern int lo_export (IntPtr conn,
			int lobjId, string filename);
		// int lo_export(PGconn *conn,
		//	Oid lobjId, const char *filename);

		[DllImport("pq")]
		public static extern int PQmblen (string s,
			int encoding);
		// int PQmblen(const unsigned char *s,
		//	int encoding);

		[DllImport("pq")]
		public static extern int PQenv2encoding ();
		// int PQenv2encoding(void);

		#endregion
	}
}
