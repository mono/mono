using System;
using System.Security.Cryptography.X509Certificates;

class Program {

	static int Main (string[] args)
	{
		if (args.Length == 0) {
			Console.WriteLine ("Usage: mono x509build.exe filename");
			return 2;
		}
		string filename = args [0];

		X509Certificate2 cert = new X509Certificate2 (filename);
		// using X509Chain.Create will use the X509Chain defined in machine.config
		X509Chain chain = X509Chain.Create ();
		bool result = chain.Build (cert);
		Console.WriteLine ("Build: {0}", result);
		Console.WriteLine ();

		Console.WriteLine ("ChainStatus:");
		if (chain.ChainStatus.Length > 0) {
			foreach (X509ChainStatus st in chain.ChainStatus) {
				Console.WriteLine ("\t{0}", st.Status);
			}
		} else {
			Console.WriteLine ("\t{0}", X509ChainStatusFlags.NoError);
		}
		Console.WriteLine ();

		int n = 1;
		Console.WriteLine ("ChainElements:");
		foreach (X509ChainElement ce in chain.ChainElements) {
			Console.WriteLine ("{0}. Certificate: {1}", n++, ce.Certificate);
			Console.WriteLine ("\tChainStatus:");
			if (ce.ChainElementStatus.Length > 0) {
				foreach (X509ChainStatus st in ce.ChainElementStatus) {
					Console.WriteLine ("\t\t{0}", st.Status);
				}
			} else {
				Console.WriteLine ("\t\t{0}", X509ChainStatusFlags.NoError);
			}
			Console.WriteLine ();
		}

		return result ? 0 : 1;
	}
}
