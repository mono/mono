using System;
using System.Data.OleDb;

namespace Mono.Data.GDA.Test
{
	public class TestGDA
	{
		private IntPtr m_gdaClient = IntPtr.Zero;
		private IntPtr m_gdaConnection = IntPtr.Zero;
		
		static void Main (string[] args)
		{
			TestGDA test = new TestGDA ();
			
			/* initialization */
			libgda.gda_init ("TestGDA#", "0.1", args.Length, args);
			test.m_gdaClient = libgda.gda_client_new ();

			/* open connection */
			test.m_gdaConnection = libgda.gda_client_open_connection (
				test.m_gdaClient,
				"PostgreSQL",
				"", "");
			if (test.m_gdaConnection != IntPtr.Zero) {
				System.Console.Write ("Connection successful!");

				/* close connection */
				libgda.gda_connection_close (test.m_gdaConnection);
			}
		}       
	}
}
