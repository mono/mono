//
// System.Data.OleDb.libgda
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
//

using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace System.Data.OleDb
{
	sealed internal class libgda
	{
		[DllImport("gda-2")]
		public static extern void gda_init (string app_id,
						    string version,
						    int nargs,
						    string[] args);

		[DllImport("gda-2")]
		public static extern IntPtr gda_client_new ();

		[DllImport("gda-2")]
		public static extern IntPtr gda_client_open_connection (IntPtr client,
									string dsn,
									string username,
									string password);

		[DllImport("gda-2")]
		public static extern bool gda_connection_is_open (IntPtr cnc);
		
		[DllImport("gda-2")]
		public static extern bool gda_connection_close (IntPtr cnc);

		[DllImport("gda-2")]
		public static extern string gda_connection_get_database (IntPtr cnc);
	}
}
