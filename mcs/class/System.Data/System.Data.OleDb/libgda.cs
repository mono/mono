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

	internal enum GdaValueType {
		Null = 0,
		Bigint = 1,
		Binary = 2,
		Boolean = 3,
		Date = 4,
		Double = 5,
		GeometricPoint = 6,
		Integer = 7,
		List = 8,
		Numeric = 9,
		Single = 10,
		Smallint = 11,
		String = 12,
		Time = 13,
		Timestamp = 14,
		Tinyint = 15,
		Type = 16,
		Unknown = 17
	};

	[StructLayout(LayoutKind.Sequential)]
	internal class GdaList
	{
		public IntPtr data;
		public IntPtr next;
		public IntPtr prev;
	}
	
	sealed internal class libgda
	{
		private static IntPtr gdaClient = IntPtr.Zero;

		public static IntPtr GdaClient
		{
			get {
				if (gdaClient == IntPtr.Zero)
					gdaClient = gda_client_new ();

				return gdaClient;
			}
		}
		
		[DllImport("gda-2")]
		public static extern void gda_init (string app_id, string version, int nargs, string[] args);

		[DllImport("gda-2")]
		public static extern GdaValueType gda_value_get_vtype (IntPtr value);

		[DllImport("gda-2")]
		public static extern bool gda_value_get_boolean (IntPtr value);

		[DllImport("gda-2")]
		public static extern int gda_value_get_smallint (IntPtr value);

		[DllImport("gda-2")]
		public static extern byte gda_value_get_tinyint (IntPtr value);

		[DllImport("gda-2")]
		public static extern string gda_value_stringify (IntPtr value);
		
		[DllImport("gda-2")]
		public static extern IntPtr gda_parameter_list_new ();

		[DllImport("gda-2")]
		public static extern string gda_type_to_string (GdaValueType type);
		
		[DllImport("gda-2")]
		public static extern int gda_data_model_get_n_rows (IntPtr model);

		[DllImport("gda-2")]
		public static extern int gda_data_model_get_n_columns (IntPtr model);

		[DllImport("gda-2")]
		public static extern IntPtr gda_data_model_get_value_at (IntPtr model, int col, int row);
		
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
		public static extern IntPtr gda_connection_execute_command (IntPtr cnc, IntPtr cmd, IntPtr parameterList);
		
		[DllImport("gda-2")]
		public static extern int gda_connection_execute_non_query (IntPtr cnc, IntPtr command, IntPtr parameterList);

		[DllImport("gda-2")]
		public static extern IntPtr gda_connection_execute_single_command (IntPtr cnc, IntPtr command, IntPtr parameterList);

		[DllImport("gda-2")]
		public static extern IntPtr gda_command_new (string text, GdaCommandType type, GdaCommandOptions options);

		[DllImport("gda-2")]
		public static extern void gda_command_set_text (IntPtr cmd, string text);

		[DllImport("gda-2")]
		public static extern void gda_command_set_command_type (IntPtr cmd, GdaCommandType type);
	}
}
