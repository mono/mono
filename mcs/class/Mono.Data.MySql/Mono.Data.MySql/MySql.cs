// 
// MySql.cs
//
// Provides the core of C# bindings 
// to the MySQL library libMySQL.dll
//
// Author:
//    Brad Merrill <zbrad@cybercom.net>
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (C)Copyright 2002 Brad Merril
// (C)Copyright 2002 Daniel Morgan
//
// http://www.cybercom.net/~zbrad/DotNet/MySql/
//
// Mono has gotten permission from Brad Merrill to include in 
// the Mono Class Library
// his C# bindings to MySQL under the X11 License
//
// Mono can be found at http://www.go-mono.com/
// The X11/MIT License can be found 
// at http://www.opensource.org/licenses/mit-license.html
//

namespace Mono.Data.MySql {
	using System;
	using System.Security;
	using System.Runtime.InteropServices;

	internal sealed class MySql {
		///<value>protocol version</value>
		public static readonly uint ProtocolVersion = 10;
		///<value>server version</value>
		public static readonly string ServerVersion = "3.23.49";
		///<value>server suffix</value>
		public static readonly string ServerSuffix = "";
		///<value>server version as int</value>
		public static readonly uint VersionId = 32349;
		///<value>server port number</value>
		public static readonly uint Port = 3306;
		///<value>unix named socket</value>
		public static readonly string UnixAddr = "/tmp/mysql.sock";
		///<value>character set</value>
		public static readonly string CharSet = "latin1";

		///<summary>Gets or initializes a `MySql' structure</summary>
		///<returns>An initialized `MYSQL*' handle.  IntPtr.Zero if there was insufficient memory to allocate a new object.</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL", EntryPoint="mysql_init", ExactSpelling=true)]
		public static extern IntPtr Init(IntPtr db);

		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL", EntryPoint="mysql_affected_rows", ExactSpelling=true)]
		public static extern ulong AffectedRows(IntPtr db);
		//my_ulonglong mysql_affected_rows(MYSQL *mysql);

		///<summary>Connects to a MySql server</summary>
		///<returns><paramref name="db"/> value on success, else returns IntPtr.Zero</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 CharSet=System.Runtime.InteropServices.CharSet.Ansi,
			 EntryPoint="mysql_connect", ExactSpelling=true)]
		public static extern IntPtr Connect(IntPtr db,
			[In] string host, [In] string user, [In] string passwd,
			[In] string dbname,
			uint port, [In] string socketName, uint flags
			);

		///<summary>Selects a database</summary>
		///<returns>Zero for success.  Non-zero if an error occurred.</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 CharSet=System.Runtime.InteropServices.CharSet.Ansi,
			 EntryPoint="mysql_select_db", ExactSpelling=true)]
		public static extern int SelectDb(IntPtr conn, [In] string db);

		///<summary>Closes a server connection</summary>
		//[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL", EntryPoint="mysql_close", ExactSpelling=true)]
		public static extern void Close(IntPtr db);

		///<summary>Executes a SQL query specified as a string</summary>
		///<returns>number of rows changed, -1 if zero</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 CharSet=System.Runtime.InteropServices.CharSet.Ansi,
			 EntryPoint="mysql_query", ExactSpelling=true)]
		public static extern int Query(IntPtr conn, [In] string query);

		///<summary>Retrieves a complete result set to the client</summary>
		///<returns>An IntPtr result structure with the results. IntPtr.Zero if an error occurred.</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 EntryPoint="mysql_store_result", ExactSpelling=true)]
		public static extern IntPtr StoreResult(IntPtr conn);

		///<returns>Returns the number of rows in a result set</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 EntryPoint="mysql_num_rows", ExactSpelling=true)]
		public static extern int NumRows(IntPtr r);

		///<returns>Returns the number of columns in a result set</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 EntryPoint="mysql_num_fields", ExactSpelling=true)]
		public static extern int NumFields(IntPtr r);

		///<returns>Returns an IntPtr to all field structures</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 EntryPoint="mysql_fetch_field", ExactSpelling=true)]
		public static extern IntPtr FetchField(IntPtr r);

		///<summary>Retrieves the next row of a result set</summary>
		///<returns>An IntPtr structure for the next row. IntPtr.Zero if there are no more rows to retrieve or if an error occurred.</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 EntryPoint="mysql_fetch_row", ExactSpelling=true)]
		public static extern IntPtr FetchRow(IntPtr r);

		///<summary>Frees the memory allocated for a result set by <see cref="StoreResult"/></summary>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 EntryPoint="mysql_free_result", ExactSpelling=true)]
		public static extern void FreeResult(IntPtr r);

		///<returns>Returns a string that represents the client library version</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 CharSet=System.Runtime.InteropServices.CharSet.Ansi,
			 EntryPoint="mysql_get_client_info", ExactSpelling=true)]
		public static extern string GetClientInfo();

		///<returns></returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 CharSet=System.Runtime.InteropServices.CharSet.Ansi,
			 EntryPoint="mysql_get_host_info", ExactSpelling=true)]
		public static extern string GetHostInfo(IntPtr db);

		///<returns>A string describing the type of connection in use, including the server host name.</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 CharSet=System.Runtime.InteropServices.CharSet.Ansi,
			 EntryPoint="mysql_get_server_info", ExactSpelling=true)]
		public static extern string GetServerInfo(IntPtr db);

		///<returns>A string describing the server status. null if an error occurred.</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 CharSet=System.Runtime.InteropServices.CharSet.Ansi,
			 EntryPoint="mysql_stat", ExactSpelling=true)]
		public static extern string Stat(IntPtr db);

		///<summary>
		/// Returns a result set describing the current server
		/// threads.  This is the same kind of information as that
		/// reported by `mysqladmin processlist' or a `SHOW PROCESSLIST'
		/// query.  You must free the result set with
		/// <see cref="FreeResult"/>.
		///</summary> 
		///<returns>
		/// A IntPtr result set for success.  IntPtr.Zero if an error
		/// occurred.
		///</returns> 
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 EntryPoint="mysql_list_processes", ExactSpelling=true)]
		public static extern IntPtr ListProcesses(IntPtr db);

		///<summary>
		///<para>
		/// Returns a result set consisting of table names in
		/// the current database that match the simple regular expression
		/// specified by the <paramref name="wild"/> parameter.
		/// <paramref name="wild"/>may contain the wild-card characters
		/// "%" or "_", or may be a null pointer to match all tables.
		///</para>
		///<para>
		/// Calling <see cref="ListTables"/> is similar to executing
		/// the query "SHOW tables [LIKE wild]".
		///</para>
		///<para>
		/// You must free the result set with <see cref="FreeResult"/>.
		///</para>
		///</summary>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 EntryPoint="mysql_list_tables", ExactSpelling=true)]
		public static extern IntPtr ListTables(IntPtr db, [In] string wild);

		///<summary>
		/// For the connection specified by <paramref name="db"/>,
		/// <see cref="Error"/> returns the
		/// error message for the most recently invoked API function
		/// that can succeed or fail.  An empty string ("") is
		/// returned if no error occurred.
		///</summary>
		///<returns>
		/// A string that describes the error.  An empty string if no error occurred.
		///</returns>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 CharSet=System.Runtime.InteropServices.CharSet.Ansi,
			 EntryPoint="mysql_error", ExactSpelling=true)]
		public static extern string Error(IntPtr db);

		///<summary>
		///<para>
		/// This function needs to be called before exiting
		/// to free memory allocated explicitly by
		/// <see cref="ThreadInit"/> or implicitly by
		/// <see cref="Init"/>.
		///</para>
		///<para>
		/// Note that this function is NOT invoked automatically by the client
		/// library!
		///</para>
		///</summary>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 CharSet=System.Runtime.InteropServices.CharSet.Ansi,
			 EntryPoint="my_thread_end", ExactSpelling=true)]
		public static extern void ThreadEnd();

		///<summary>
		///<para>
		/// This function needs to be called for each created thread
		/// to initialize thread specific variables.
		///</para>
		///<para>
		/// This is automatically called by <see cref="Init"/>.
		///</para>
		///</summary>
		[SuppressUnmanagedCodeSecurity]
		[DllImport("libmySQL",
			 CharSet=System.Runtime.InteropServices.CharSet.Ansi,
			 EntryPoint="my_thread_init", ExactSpelling=true)]
		public static extern void ThreadInit();
	}
}
