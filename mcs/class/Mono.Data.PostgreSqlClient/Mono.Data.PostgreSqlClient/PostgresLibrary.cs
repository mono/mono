//
// Mono.Data.PostgreSqlClient.PostgresLibrary.cs  
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

// *** uncomment #define to get debug messages, comment for production ***
//#define DEBUG_PostgresLibrary

using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;

namespace Mono.Data.PostgreSqlClient {

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
		public static extern uint PQescapeString (out string to,
                        string from, uint length);
		// size_t PQescapeString(char *to, 
                //      const char *from, size_t length);

		[DllImport("pq")]
		public static extern byte[] PQescapeBytea (byte[] bintext,
                        uint binlen, uint bytealen);
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
