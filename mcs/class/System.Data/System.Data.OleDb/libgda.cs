//
// System.Data.OleDb.libgda
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace System.Data.OleDb
{
	internal enum GdaCommandOptions {
		IgnoreErrors = 1,
		StopOnErrors = 1 << 1,
		BadOption = 1 << 2,
	};

	internal enum GdaCommandType {
		Sql = 0,
		Xml = 1,
		Procedure = 2,
		Table = 3,
		Schema = 4,
		Invalid = 5
	};
	
	sealed internal class libgda
	{
		private static IntPtr m_gdaClient = IntPtr.Zero;
		
		static libgda ()
		{
			gda_init ("System.Data.OleDb", "0.1", 0, null);
		}

		public static IntPtr GdaClient
		{
			get {
				if (m_gdaClient == IntPtr.Zero)
					m_gdaClient = gda_client_new ();

				return m_gdaClient;
			}
		}
		
		[DllImport("gda-2")]
		public static extern void gda_init (string app_id, string version, int nargs, string[] args);

		[DllImport("gda-2")]
		public static extern IntPtr gda_client_new ();

		[DllImport("gda-2")]
		public static extern IntPtr gda_client_open_connection (IntPtr client, string dsn, string username, string password);

		[DllImport("gda-2")]
		public static extern bool gda_connection_is_open (IntPtr cnc);
		
		[DllImport("gda-2")]
		public static extern bool gda_connection_close (IntPtr cnc);

		[DllImport("gda-2")]
		public static extern string gda_connection_get_database (IntPtr cnc);

		[DllImport("gda-2")]
		public static extern string gda_connection_get_dsn (IntPtr cnc);

		[DllImport("gda-2")]
		public static extern string gda_connection_get_cnc_string (IntPtr cnc);

		[DllImport("gda-2")]
		public static extern string gda_connection_get_provider (IntPtr cnc);

		[DllImport("gda-2")]
		public static extern string gda_connection_get_username (IntPtr cnc);

		[DllImport("gda-2")]
		public static extern string gda_connection_get_password (IntPtr cnc);

		[DllImport("gda-2")]
		public static extern IntPtr gda_transaction_new (string name);

		[DllImport("gda-2")]
		public static extern IntPtr gda_transaction_get_name (IntPtr xaction);

		[DllImport("gda-2")]
		public static extern IntPtr gda_transaction_set_name (IntPtr xaction, string name);
	
		[DllImport("gda-2")]
		public static extern bool gda_connection_begin_transaction (IntPtr cnc, IntPtr xaction);

		[DllImport("gda-2")]
		public static extern bool gda_connection_commit_transaction (IntPtr cnc, IntPtr xaction);

		[DllImport("gda-2")]
		public static extern bool gda_connection_rollback_transaction (IntPtr cnc, IntPtr xaction);

		[DllImport("gda-2")]
		public static extern int gda_connection_execute_non_query (IntPtr cnc, IntPtr command, IntPtr parameterList);

		[DllImport("gda-2")]
		public static extern IntPtr gda_connection_execute_single_command (IntPtr cnc, IntPtr command, IntPtr parameterList);

		[DllImport("gda-2")]
		public static extern IntPtr gda_command_new (string text, int type, int options);
	}
}
