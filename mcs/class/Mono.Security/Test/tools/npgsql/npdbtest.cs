using System;
using System.Data;

using Npgsql;

public class NpgsqlSslTest {

	static bool showcs = false;

	public static void Help ()
	{
		Console.WriteLine ("npdbtest [options] [iterations]");
		Console.WriteLine ("where available options are:");
		Console.WriteLine ("\t--protocol=2\tUse v2 protocol (not SSL related)");
		Console.WriteLine ("\t--protocol=3\tUse v3 protocol (not SSL related)");
		Console.WriteLine ("\t--ssl\tUse SSL for communication");
		Console.WriteLine ("\t--nossl\tDo not use SSL for communication");
		Console.WriteLine ("\t--v\tAdd verbosity (can be used multiple times)");
		Console.WriteLine ("\t--echo\tActivate Npgsql logging (debug log leve)");
		Console.WriteLine ("and defaults options are --protocol=3, --ssl and iterations = 1");
	}

	public static bool Connect (int protocol, bool ssl, int verbose)
	{
		string cs = "Server=127.0.0.1;Port=5432;Protocol={0};User Id=npgsql_tests;Password=npgsql_tests;Database=npgsql_tests;ssl={1}";
		cs = String.Format (cs, protocol, ssl);
		if ((verbose > 0) && !showcs) {
			Console.WriteLine ("Connection string: {0}", cs);
			showcs = true;
		}

		NpgsqlConnection m_conn = new NpgsqlConnection (cs);
		m_conn.Open ();
		if (m_conn.State != ConnectionState.Open)
			return false;

		string query = "select * from wordlist";
		NpgsqlCommand cmd = new NpgsqlCommand (query, m_conn);

		if (verbose > 1)
			Console.WriteLine ("> NpgsqlDataReader.ExecuteReader");

		NpgsqlDataReader rdr = cmd.ExecuteReader ();
		int i=0;
		while (rdr.Read ()) {
			i++;
			if (verbose > 2)
				Console.WriteLine (i);
		}
		rdr.Close ();

		if (verbose > 1)
			Console.WriteLine ("< NpgsqlDataReader.ExecuteReader ({0})", i);

		m_conn.Close ();
		return true;
	}

	public static int Main (string[] args)
	{
		int err = 0;
		int iter = 1;
		bool ssl = true;
		int verbose = 0;
		int protocol = 3;

		if (args.Length > 0) {
			foreach (string arg in args) {
				switch (arg.ToLower ()) {
				case "--protocol=2":
					protocol = 2;
					break;
				case "--protocol=3":
					protocol = 3;
					break;
				case "--ssl":
				case "--ssl3":
				case "--tls":
				case "--tls1":
					Console.WriteLine ("You cannot select between SSL/TLS with this tool.");
					ssl = true;
					break;
				case "--nossl":
					ssl = false;
					break;
				case "--v":
					verbose++;
					break;
				case "--echo":
					NpgsqlEventLog.Level = LogLevel.Debug;
					NpgsqlEventLog.EchoMessages = true;
					break;
				case "--help":
					Help ();
					return 0;
				default:
					try {
						iter = Convert.ToInt32 (arg);
					}
					catch {
						Console.WriteLine ("Unknown option '{0}'.", arg);
						Help ();
						return 1;
					}
					break;
				}
			}
		}

		int i = 0;
		try {
			for (i = 1; i <= iter; i++) {
				if (verbose > 0)
					Console.WriteLine ("Connection #{0}...", i);

				if (!Connect (protocol, ssl, verbose)) {
					Console.WriteLine ("Connection failed at iteration #{0}!", i);
					break;
				}
			}
			i--;
			err = 0;
		}
		catch (Exception e) {
			err = i;
			// don't count last iteration as it failed
			Console.WriteLine ("ERROR at iteration #{0}{1}{2}", --i, Environment.NewLine, e);
		}
		finally {
			Console.WriteLine ("{0} {1}connection established and closed.",
				i, (ssl ? "SSL " : String.Empty));
		}
		return err;
	}
}
